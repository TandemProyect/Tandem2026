using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Oferta al cliente: textos generales con logo y documento editable por sistema.
    /// </summary>
    [Authorize]
    public class ClientOfferController : BaseController
    {
        private static readonly string[] AllowedLogoExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".svg" };
        private const long MaxLogoBytes = 2 * 1024 * 1024;
        private static readonly Regex ContactStateCodeRegex = new Regex("^[A-Z0-9_-]+$", RegexOptions.Compiled);

        public ActionResult Index()
        {
            ViewBag.TableMissing = !ClientOfferRepository.TablesExist(db);
            return View();
        }

        public ActionResult Texts()
        {
            ViewBag.TableMissing = !ClientOfferRepository.TablesExist(db);
            return View();
        }

        public ActionResult Create(long? designId, long? systemId)
        {
            if (!ClientOfferRepository.TablesExist(db))
            {
                return RedirectToAction("Index");
            }

            var row = new ClientOfferRow { Is_Active = true, TextStatus = "BORRADOR" };
            if (designId.HasValue && designId.Value > 0)
            {
                row.LinkDesign = designId;
            }

            if (systemId.HasValue && systemId.Value > 0)
            {
                row.LinkSystem = systemId.Value;
            }

            var companyId = ClientOfferRepository.UserCompanyId(db, User.Identity.GetUserId());
            if (companyId.HasValue)
            {
                row.LinkCompany = companyId.Value;
            }

            if (row.LinkCompany > 0)
            {
                var ctx = ClientOfferRepository.LoadContext(db, row);
                if (string.IsNullOrWhiteSpace(row.TextClientAddress))
                {
                    row.TextClientAddress = ctx.DireccionCliente;
                }
            }

            FillLookups(row.LinkCompany, row.LinkClientOfferBrand, row.LinkClient_V2);
            return View(row);
        }

        public ActionResult OpenFromDesign(long designId, long? systemId, string systemLabel)
        {
            if (!systemId.HasValue || systemId.Value <= 0)
            {
                var found = ClientOfferRepository.FindSystemIdByLabel(db, systemLabel);
                if (found.HasValue && found.Value > 0)
                {
                    systemId = found;
                }
            }

            return RedirectToAction("Create", new { designId = designId, systemId = systemId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClientOfferRow model, HttpPostedFileBase logoFile)
        {
            if (!ClientOfferRepository.TablesExist(db))
            {
                return RedirectToAction("Index");
            }

            string logoError;
            var savedLogo = TrySaveLogo(logoFile, out logoError);
            if (logoError != null)
            {
                ModelState.AddModelError("TextLogo", logoError);
            }

            if (model.LinkSystem <= 0)
            {
                ModelState.AddModelError("LinkSystem", ClientOffer.Err_SystemRequired);
            }

            if (model.LinkCompany <= 0)
            {
                ModelState.AddModelError("LinkCompany", ClientOffer.Err_CompanyRequired);
            }

            var ctx = model.LinkCompany > 0
                ? ClientOfferRepository.LoadContext(db, model)
                : new ClientOfferContext();
            if (string.IsNullOrWhiteSpace(model.TextClientAddress))
            {
                model.TextClientAddress = ctx.DireccionCliente;
            }
            ApplyContact(model);

            var brand = ResolveBrand(model);
            if (!model.LinkClientOfferBrand.HasValue || model.LinkClientOfferBrand.Value <= 0)
            {
                ModelState.AddModelError("LinkClientOfferBrand", ClientOffer.Err_TemplateRequired);
            }

            var tokens = ClientOfferRepository.Tokens(ctx, model.TextSubject, model.TextRecuento);
            tokens["NombreContacto"] = model.TextContactName ?? "";
            tokens["EmailContacto"] = model.TextContactEmail ?? "";
            tokens["Material"] = model.TextMaterial ?? "";
            if (string.IsNullOrWhiteSpace(model.TextSubject))
            {
                model.TextSubject = ClientOfferMerge.Apply(
                    brand == null || string.IsNullOrWhiteSpace(brand.TextSubjectTemplate)
                        ? ClientOfferMerge.DefaultSubject
                        : brand.TextSubjectTemplate,
                    tokens);
                tokens["Asunto"] = model.TextSubject ?? "";
            }

            if (string.IsNullOrWhiteSpace(model.TextSubject))
            {
                ModelState.AddModelError("TextSubject", ClientOffer.Err_SubjectRequired);
            }

            if (!ModelState.IsValid)
            {
                FillLookups(model.LinkCompany, model.LinkClientOfferBrand, model.LinkClient_V2);
                return View(model);
            }

            if (!string.IsNullOrEmpty(savedLogo))
            {
                model.TextLogo = savedLogo;
            }
            else if (brand != null && !string.IsNullOrEmpty(brand.TextLogo))
            {
                model.TextLogo = brand.TextLogo;
            }
            else
            {
                model.TextLogo = ctx.Logo;
            }

            if (brand != null)
            {
                model.LinkClientOfferBrand = brand.IdObject;
                if (!string.IsNullOrWhiteSpace(brand.TextCompanyName))
                {
                    tokens["Empresa"] = brand.TextCompanyName;
                }
            }

            var header = ClientOfferMerge.Apply(
                brand == null || string.IsNullOrWhiteSpace(brand.TextHeaderTemplate)
                    ? ClientOfferMerge.DefaultHeader
                    : brand.TextHeaderTemplate,
                tokens);
            var body = ClientOfferMerge.Apply(
                brand == null || string.IsNullOrWhiteSpace(brand.TextBodyTemplate)
                    ? ClientOfferMerge.DefaultBody
                    : brand.TextBodyTemplate,
                tokens);

            try
            {
                var id = ClientOfferRepository.InsertDocument(db, model, header, body, User.Identity.GetUserId());
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                FillLookups(model.LinkCompany, model.LinkClientOfferBrand, model.LinkClient_V2);
                return View(model);
            }
        }

        public ActionResult Edit(long id)
        {
            var row = ClientOfferRepository.GetDocument(db, id);
            if (row == null)
            {
                return HttpNotFound();
            }

            FillLookups(row.LinkCompany, row.LinkClientOfferBrand, row.LinkClient_V2);
            return View(row);
        }

        [HttpPost]
        public JsonResult ListAssistantMessages(long offerId)
        {
            var data = ClientOfferRepository.ListAssistantMessages(db, offerId).Select(m => new
            {
                role = (m.TextRole ?? "").ToLowerInvariant(),
                text = m.TextMessage ?? "",
                date = m.AddDateMade.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
            return Json(new { IsOk = true, Data = data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AskAssistant(long offerId, string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Json(new { IsOk = false, Message = ClientOffer.Msg_AssistantEmpty });
            }

            var offer = ClientOfferRepository.GetDocument(db, offerId);
            if (offer == null)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Msg_AssistantError });
            }

            var context = ClientOfferRepository.LoadContext(db, offer);
            var tokens = ClientOfferRepository.Tokens(context, offer.TextSubject, offer.TextRecuento);
            var req = new ClientOfferAssistantRequest
            {
                Prompt = prompt,
                Oferta = offer.TextSubject,
                Contexto = context == null ? string.Empty : (context.Empresa + " " + context.Cliente + " " + context.Obra + " " + context.Sistema),
                Tokens = tokens
            };

            var response = ClientOfferAssistantService.Suggest(req);
            var by = User.Identity.GetUserId();
            ClientOfferRepository.InsertAssistantMessage(db, offerId, "user", prompt.Trim(), null, by);
            ClientOfferRepository.InsertAssistantMessage(db, offerId, "assistant", response.Body ?? "", JsonConvert.SerializeObject(response), by);

            return Json(new
            {
                IsOk = true,
                Response = new
                {
                    answer = response.Body ?? "",
                    subject = response.Subject ?? "",
                    header = response.Header ?? "",
                    body = response.Body ?? ""
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClientOfferRow model, HttpPostedFileBase logoFile, string rebuild)
        {
            var current = ClientOfferRepository.GetDocument(db, model.IdObject);
            if (current == null)
            {
                return HttpNotFound();
            }

            string logoError;
            var savedLogo = TrySaveLogo(logoFile, out logoError);
            if (logoError != null)
            {
                ModelState.AddModelError("TextLogo", logoError);
            }

            if (model.LinkSystem <= 0)
            {
                ModelState.AddModelError("LinkSystem", ClientOffer.Err_SystemRequired);
            }

            if (model.LinkCompany <= 0)
            {
                ModelState.AddModelError("LinkCompany", ClientOffer.Err_CompanyRequired);
            }

            if (string.IsNullOrWhiteSpace(model.TextSubject))
            {
                ModelState.AddModelError("TextSubject", ClientOffer.Err_SubjectRequired);
            }

            ApplyContact(model);

            if (!model.LinkClientOfferBrand.HasValue || model.LinkClientOfferBrand.Value <= 0)
            {
                ModelState.AddModelError("LinkClientOfferBrand", ClientOffer.Err_TemplateRequired);
            }

            var selectedBrand = ResolveBrand(model);
            if (selectedBrand != null)
            {
                model.LinkClientOfferBrand = selectedBrand.IdObject;
            }

            if (!string.IsNullOrEmpty(savedLogo))
            {
                model.TextLogo = savedLogo;
            }
            else
            {
                model.TextLogo = current.TextLogo;
            }

            if (!string.IsNullOrEmpty(rebuild))
            {
                var ctx = ClientOfferRepository.LoadContext(db, model);
                var brand = selectedBrand;
                var tokens = ClientOfferRepository.Tokens(ctx, model.TextSubject, model.TextRecuento);
                tokens["NombreContacto"] = model.TextContactName ?? "";
                tokens["EmailContacto"] = model.TextContactEmail ?? "";
                tokens["Material"] = model.TextMaterial ?? "";
                if (brand != null && !string.IsNullOrWhiteSpace(brand.TextCompanyName))
                {
                    tokens["Empresa"] = brand.TextCompanyName;
                }

                model.LinkClientOfferBrand = brand == null ? (long?)null : brand.IdObject;
                model.HeaderBody = ClientOfferMerge.Apply(
                    brand == null || string.IsNullOrWhiteSpace(brand.TextHeaderTemplate)
                        ? ClientOfferMerge.DefaultHeader
                        : brand.TextHeaderTemplate,
                    tokens);
                model.TextBody = ClientOfferMerge.Apply(
                    brand == null || string.IsNullOrWhiteSpace(brand.TextBodyTemplate)
                        ? ClientOfferMerge.DefaultBody
                        : brand.TextBodyTemplate,
                    tokens);
                if (string.IsNullOrEmpty(savedLogo) && brand != null && !string.IsNullOrEmpty(brand.TextLogo))
                {
                    model.TextLogo = brand.TextLogo;
                }
            }
            else
            {
                if (!model.LinkClientOfferBrand.HasValue || model.LinkClientOfferBrand.Value <= 0)
                {
                    model.LinkClientOfferBrand = current.LinkClientOfferBrand;
                }
            }

            if (!ModelState.IsValid)
            {
                FillLookups(model.LinkCompany, model.LinkClientOfferBrand, model.LinkClient_V2);
                return View(model);
            }

            ClientOfferRepository.UpdateDocument(db, model, User.Identity.GetUserId());
            return RedirectToAction("Edit", new { id = model.IdObject });
        }

        public ActionResult Print(long id)
        {
            var row = ClientOfferRepository.GetDocument(db, id);
            if (row == null)
            {
                return HttpNotFound();
            }

            return View(row);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GeneratePdf(long id)
        {
            var row = ClientOfferRepository.GetDocument(db, id);
            if (row == null)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Msg_AssistantError });
            }

            var folder = Server.MapPath("~/Files/ClientOfferPdf/");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = "offer_" + id + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".pdf";
            var physical = Path.Combine(folder, fileName);
            ClientOfferPdfWriter.Write(physical, row);
            var relative = "/Files/ClientOfferPdf/" + fileName;
            ClientOfferRepository.UpdateDocumentPdfPath(db, id, relative, User.Identity.GetUserId());
            return Json(new { IsOk = true, Url = relative });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteCapture(long offerId, long captureId)
        {
            if (offerId <= 0 || captureId <= 0)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Msg_AssistantError });
            }

            ClientOfferRepository.SoftDeleteCapture(db, offerId, captureId, User.Identity.GetUserId());
            return Json(new { IsOk = true });
        }

        [HttpPost]
        public JsonResult SaveCaptureFromViewer()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }

                string rawJson;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    rawJson = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    return Json(new { IsOk = false, Message = "JSON vacío." });
                }

                var body = JObject.Parse(rawJson);
                var offerId = body.Value<long?>("ClientOfferId") ?? 0;
                var designId = body.Value<long?>("DesignId");
                var pngBase64 = body.Value<string>("PngBase64") ?? "";
                if (offerId <= 0)
                {
                    return Json(new { IsOk = false, Message = "Oferta no indicada." });
                }

                var offer = ClientOfferRepository.GetDocument(db, offerId);
                if (offer == null)
                {
                    return Json(new { IsOk = false, Message = "Oferta no encontrada." });
                }

                if (designId.HasValue && designId.Value > 0 && offer.LinkDesign.HasValue && offer.LinkDesign.Value > 0 && offer.LinkDesign.Value != designId.Value)
                {
                    return Json(new { IsOk = false, Message = "La captura no corresponde al diseño de la oferta." });
                }

                byte[] png;
                try
                {
                    png = DecodeOfferCapturePng(pngBase64);
                }
                catch (Exception ex)
                {
                    return Json(new { IsOk = false, Message = ex.Message });
                }

                var folder = DateTime.Now.ToString("yyyy-MM");
                var fileName = "offer_capture_" + offerId + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + ".png";
                var virtualPath = "/Files/ClientOfferCapture/" + folder + "/" + fileName;
                var dir = Server.MapPath("~/Files/ClientOfferCapture/" + folder);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var physical = Path.Combine(dir, fileName);
                System.IO.File.WriteAllBytes(physical, png);
                var captureId = ClientOfferRepository.InsertCapture(db, offerId, virtualPath, null, User.Identity.GetUserId());
                return Json(new { IsOk = true, CaptureId = captureId, ImagePath = virtualPath, ImageUrl = Url.Content("~" + virtualPath) });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { IsOk = false, Message = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { IsOk = false, Message = ex.Message });
            }
        }

        public ActionResult CreateText()
        {
            var row = new ClientOfferBrandRow
            {
                Is_Active = true,
                TextSubjectTemplate = ClientOfferMerge.DefaultSubject,
                TextHeaderTemplate = ClientOfferMerge.DefaultHeader,
                TextBodyTemplate = ClientOfferMerge.DefaultBody
            };
            var companyId = ClientOfferRepository.UserCompanyId(db, User.Identity.GetUserId());
            if (companyId.HasValue)
            {
                row.LinkCompany = companyId.Value;
            }

            FillLookups(row.LinkCompany);
            return View(row);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateText(ClientOfferBrandRow model, HttpPostedFileBase logoFile)
        {
            return SaveText(model, logoFile, isCreate: true);
        }

        public ActionResult EditText(long id)
        {
            var row = ClientOfferRepository.GetBrand(db, id);
            if (row == null)
            {
                return HttpNotFound();
            }

            FillLookups(row.LinkCompany);
            return View(row);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditText(ClientOfferBrandRow model, HttpPostedFileBase logoFile)
        {
            return SaveText(model, logoFile, isCreate: false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(long id)
        {
            ClientOfferRepository.SoftDeleteDocument(db, id, User.Identity.GetUserId());
            return Json(new { IsOk = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteText(long id)
        {
            ClientOfferRepository.SoftDeleteBrand(db, id, User.Identity.GetUserId());
            return Json(new { IsOk = true });
        }

        [HttpPost]
        public JsonResult ListDocuments([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            return ListRows(requestModel, ClientOfferRepository.ListDocuments(db).Select(p => new ListItem
            {
                IdObject = p.IdObject,
                TextLabel = p.TextSubject ?? p.TextLabel,
                Extra = p.SystemLabel,
                Third = p.ClientLabel,
                Is_Active = p.Is_Active
            }).AsQueryable(), "Edit");
        }

        [HttpPost]
        public JsonResult ListTexts([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            return ListRows(requestModel, ClientOfferRepository.ListBrands(db).Select(p => new ListItem
            {
                IdObject = p.IdObject,
                TextLabel = p.TextLabel,
                Extra = string.IsNullOrEmpty(p.SystemLabel) ? ClientOffer.Lbl_AllSystems : p.SystemLabel,
                Third = p.CompanyLabel,
                Is_Active = p.Is_Active
            }).AsQueryable(), "EditText");
        }

        [HttpPost]
        public JsonResult ListContactStates([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var query = ClientOfferRepository.ListContactStateTypes(db).Select(p => new ContactStateListItem
            {
                IdObject = p.IdObject,
                TextCode = p.TextCode ?? "",
                TextLabel = p.TextLabel ?? "",
                Is_Active = p.Is_Active
            }).AsQueryable();

            var totalCount = query.Count();
            if (!string.IsNullOrEmpty(requestModel.Search.Value))
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p =>
                    p.TextCode.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                    || p.TextLabel.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var filteredCount = query.Count();
            var orderBy = requestModel.Columns.GetSortedColumns().Any(c => c.Data == "TextLabel")
                ? "TextLabel asc"
                : "TextCode asc";
            query = query.OrderBy(orderBy).ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

            var ttDelete = HttpUtility.HtmlAttributeEncode(ClientOffer.List_DeleteTooltip);
            var data = query.ToList().Select(p => new
            {
                p.IdObject,
                TextCode = HttpUtility.HtmlEncode(p.TextCode),
                TextLabel = HttpUtility.HtmlEncode(p.TextLabel),
                activeBadge = p.Is_Active
                    ? "<span class=\"badge bg-label-success\">" + HttpUtility.HtmlEncode(ClientOffer.Lbl_Active) + "</span>"
                    : "<span class=\"badge bg-label-secondary\">" + HttpUtility.HtmlEncode(ClientOffer.Lbl_Inactive) + "</span>",
                rowActions = "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"DeleteClientOfferContactState(" + p.IdObject + ");return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash\"></span></a>"
            }).ToList();

            return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateContactState(string textCode, string textLabel, bool isActive = true)
        {
            var code = (textCode ?? "").Trim().ToUpperInvariant();
            var label = (textLabel ?? "").Trim();

            if (code.Length == 0 || !ContactStateCodeRegex.IsMatch(code))
            {
                return Json(new { IsOk = false, Message = ClientOffer.Err_ContactStateCodeInvalid });
            }

            if (label.Length == 0)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Err_ContactStateLabelRequired });
            }

            var existing = ClientOfferRepository.GetContactStateTypeByCode(db, code);
            if (existing != null)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Err_ContactStateDuplicate });
            }

            try
            {
                ClientOfferRepository.InsertContactStateType(db, code, label, isActive, User.Identity.GetUserId());
                return Json(new { IsOk = true });
            }
            catch (Exception ex)
            {
                var sql = ex as SqlException ?? ex.InnerException as SqlException;
                return Json(new
                {
                    IsOk = false,
                    Message = sql != null && (sql.Number == 2601 || sql.Number == 2627)
                        ? ClientOffer.Err_ContactStateDuplicate
                        : ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteContactState(long id)
        {
            var row = ClientOfferRepository.ListContactStateTypes(db).FirstOrDefault(p => p.IdObject == id);
            if (row == null)
            {
                return Json(new { IsOk = false, Message = ClientOffer.Err_ContactStateNotFound });
            }

            var code = (row.TextCode ?? "").Trim().ToUpperInvariant();
            if (code == "ALTA" || code == "BAJA" || code == "ENFERMO")
            {
                return Json(new { IsOk = false, Message = ClientOffer.Err_ContactStateProtected });
            }

            ClientOfferRepository.SoftDeleteContactStateType(db, id, User.Identity.GetUserId());
            return Json(new { IsOk = true });
        }

        private ActionResult SaveText(ClientOfferBrandRow model, HttpPostedFileBase logoFile, bool isCreate)
        {
            string logoError;
            var savedLogo = TrySaveLogo(logoFile, out logoError);
            if (logoError != null)
            {
                ModelState.AddModelError("TextLogo", logoError);
            }

            if (model.LinkCompany <= 0)
            {
                ModelState.AddModelError("LinkCompany", ClientOffer.Err_CompanyRequired);
            }

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", ClientOffer.Err_LabelRequired);
            }

            if (!isCreate)
            {
                var current = ClientOfferRepository.GetBrand(db, model.IdObject);
                if (current == null)
                {
                    return HttpNotFound();
                }

                if (string.IsNullOrEmpty(savedLogo))
                {
                    model.TextLogo = current.TextLogo;
                }
            }

            if (!string.IsNullOrEmpty(savedLogo))
            {
                model.TextLogo = savedLogo;
            }

            if (!ModelState.IsValid)
            {
                FillLookups(model.LinkCompany);
                return View(isCreate ? "CreateText" : "EditText", model);
            }

            try
            {
                if (isCreate)
                {
                    ClientOfferRepository.InsertBrand(db, model, User.Identity.GetUserId());
                }
                else
                {
                    ClientOfferRepository.UpdateBrand(db, model, User.Identity.GetUserId());
                }

                return RedirectToAction("Texts");
            }
            catch (Exception ex)
            {
                var sql = ex as SqlException ?? ex.InnerException as SqlException;
                ModelState.AddModelError("", sql != null && (sql.Number == 2601 || sql.Number == 2627)
                    ? ClientOffer.Err_BrandDuplicate
                    : ex.Message);
                FillLookups(model.LinkCompany);
                return View(isCreate ? "CreateText" : "EditText", model);
            }
        }

        private JsonResult ListRows(IDataTablesRequest requestModel, IQueryable<ListItem> query, string editAction)
        {
            var totalCount = query.Count();
            if (!string.IsNullOrEmpty(requestModel.Search.Value))
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p =>
                    (p.TextLabel ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Extra ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                    || (p.Third ?? "").IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var filteredCount = query.Count();
            var orderByString = string.Empty;
            foreach (var column in requestModel.Columns.GetSortedColumns())
            {
                string orderColumn;
                switch (column.Data)
                {
                    case "Extra": orderColumn = "Extra"; break;
                    case "Third": orderColumn = "Third"; break;
                    case "activeBadge": orderColumn = "Is_Active"; break;
                    default: orderColumn = "TextLabel"; break;
                }

                orderByString += orderByString != string.Empty ? "," : "";
                orderByString += orderColumn + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
            }

            query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "TextLabel asc" : orderByString);
            query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
            var ttEdit = HttpUtility.HtmlAttributeEncode(ClientOffer.List_EditTooltip);
            var ttDelete = HttpUtility.HtmlAttributeEncode(ClientOffer.List_DeleteTooltip);
            var active = HttpUtility.HtmlEncode(ClientOffer.Lbl_Active);
            var inactive = HttpUtility.HtmlEncode(ClientOffer.Lbl_Inactive);
            var deleteFn = editAction == "EditText" ? "DeleteClientOfferText" : "DeleteClientOffer";

            var data = query.ToList().Select(p => new
            {
                p.IdObject,
                TextLabel = "<a href=\"" + Url.Action(editAction, new { id = p.IdObject }) + "\">" + HttpUtility.HtmlEncode(p.TextLabel ?? "") + "</a>",
                TextLabelPlain = p.TextLabel ?? "",
                Extra = HttpUtility.HtmlEncode(p.Extra ?? ""),
                Third = HttpUtility.HtmlEncode(p.Third ?? ""),
                activeBadge = p.Is_Active
                    ? "<span class=\"badge bg-label-success\">" + active + "</span>"
                    : "<span class=\"badge bg-label-secondary\">" + inactive + "</span>",
                rowActions = "<div class=\"d-inline-flex align-items-center gap-2\">" +
                             "<a title=\"" + ttEdit + "\" href=\"" + Url.Action(editAction, new { id = p.IdObject }) +
                             "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\"></span></a>" +
                             "<a title=\"" + ttDelete + "\" href=\"#\" onclick=\"" + deleteFn + "(" + p.IdObject +
                             ");return false;\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash\"></span></a></div>"
            }).ToList();

            return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
        }

        private ClientOfferBrandRow ResolveBrand(ClientOfferRow row)
        {
            if (row == null || row.LinkCompany <= 0 || row.LinkSystem <= 0)
            {
                return null;
            }

            if (row.LinkClientOfferBrand.HasValue && row.LinkClientOfferBrand.Value > 0)
            {
                var selected = ClientOfferRepository.GetBrand(db, row.LinkClientOfferBrand.Value);
                if (selected == null || !selected.Is_Active || selected.LinkCompany != row.LinkCompany)
                {
                    ModelState.AddModelError("LinkClientOfferBrand", ClientOffer.Err_TemplateInvalid);
                    return null;
                }

                if (selected.LinkSystem.HasValue && selected.LinkSystem.Value > 0 && selected.LinkSystem.Value != row.LinkSystem)
                {
                    ModelState.AddModelError("LinkClientOfferBrand", ClientOffer.Err_TemplateInvalid);
                    return null;
                }

                return selected;
            }

            return null;
        }

        private void FillLookups(long companyId, long? selectedBrandId = null, long? clientId = null)
        {
            ViewBag.Companies = ClientOfferRepository.ListCompanies(db);
            ViewBag.Systems = ClientOfferRepository.ListSystems(db);
            ViewBag.Plantillas = ClientOfferRepository.ListPlantillas(db);
            ViewBag.OfferTemplates = ClientOfferRepository.ListSelectableBrands(db, companyId, selectedBrandId);
            ViewBag.OfferContacts = ClientOfferRepository.ListOfferContacts(db, companyId, clientId);
            ViewBag.Designs = ClientOfferRepository.ListDesigns(db);
            ViewBag.Offers = ClientOfferRepository.ListOffers(db);
            ViewBag.Jobsides = ClientOfferRepository.ListJobsides(db);
            ViewBag.Clients = ClientOfferRepository.ListClients(db);
            ViewBag.CompanyId = companyId;
        }

        private void ApplyContact(ClientOfferRow model)
        {
            if (model == null)
            {
                return;
            }

            if ((!model.LinkClientOfferContact.HasValue || model.LinkClientOfferContact.Value <= 0)
                && model.LinkCompany > 0
                && model.LinkClient_V2.HasValue
                && model.LinkClient_V2.Value > 0)
            {
                var byDefault = ClientOfferRepository.GetDefaultOfferContact(db, model.LinkCompany, model.LinkClient_V2.Value);
                if (byDefault != null)
                {
                    model.LinkClientOfferContact = byDefault.IdObject;
                }
            }

            if (model.LinkClientOfferContact.HasValue && model.LinkClientOfferContact.Value > 0)
            {
                var contact = ClientOfferRepository.GetOfferContact(db, model.LinkClientOfferContact.Value);
                if (contact != null
                    && contact.Is_Active
                    && contact.LinkCompany == model.LinkCompany
                    && model.LinkClient_V2.HasValue
                    && contact.LinkClient_V2 == model.LinkClient_V2
                    && !string.Equals(contact.StateCode, "BAJA", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(contact.StateCode, "ENFERMO", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(model.TextContactName))
                    {
                        model.TextContactName = contact.TextLabel;
                    }

                    if (string.IsNullOrWhiteSpace(model.TextContactEmail))
                    {
                        model.TextContactEmail = contact.TextEmail;
                    }
                }
                else
                {
                    model.LinkClientOfferContact = null;
                }
            }

            if (string.IsNullOrWhiteSpace(model.TextContactEmail))
            {
                model.TextContactEmail = "contacto@pendiente.local";
            }
        }

        private string TrySaveLogo(HttpPostedFileBase logoFile, out string error)
        {
            error = null;
            if (logoFile == null || logoFile.ContentLength <= 0)
            {
                return null;
            }

            var extension = (Path.GetExtension(logoFile.FileName) ?? "").ToLowerInvariant();
            if (Array.IndexOf(AllowedLogoExtensions, extension) < 0)
            {
                error = ClientOffer.Err_LogoFormat;
                return null;
            }

            if (logoFile.ContentLength > MaxLogoBytes)
            {
                error = ClientOffer.Err_LogoSize;
                return null;
            }

            var folder = Server.MapPath("~/Files/ClientOffer/");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = "logo_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" +
                           Guid.NewGuid().ToString("N").Substring(0, 6) + extension;
            logoFile.SaveAs(Path.Combine(folder, fileName));
            return "/Files/ClientOffer/" + fileName;
        }

        private static byte[] DecodeOfferCapturePng(string raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                throw new InvalidOperationException("La captura está vacía.");
            }

            const string prefix = "data:image/png;base64,";
            if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Formato de captura no válido.");
            }

            var b64 = value.Substring(prefix.Length).Trim();
            if (b64.Length == 0)
            {
                throw new InvalidOperationException("La captura está vacía.");
            }

            return Convert.FromBase64String(b64);
        }

        private class ListItem
        {
            public long IdObject { get; set; }
            public string TextLabel { get; set; }
            public string Extra { get; set; }
            public string Third { get; set; }
            public bool Is_Active { get; set; }
        }

        private class ContactStateListItem
        {
            public long IdObject { get; set; }
            public string TextCode { get; set; }
            public string TextLabel { get; set; }
            public bool Is_Active { get; set; }
        }
    }
}
