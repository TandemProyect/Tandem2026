using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
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

            FillLookups(row.LinkCompany);
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

            var brand = model.LinkCompany > 0 && model.LinkSystem > 0
                ? ClientOfferRepository.FindBrand(db, model.LinkCompany, model.LinkSystem)
                : null;
            var tokens = ClientOfferRepository.Tokens(ctx, model.TextSubject, model.TextRecuento);
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
                FillLookups(model.LinkCompany);
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
                FillLookups(model.LinkCompany);
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

            FillLookups(row.LinkCompany);
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
                var brand = ClientOfferRepository.FindBrand(db, model.LinkCompany, model.LinkSystem);
                var tokens = ClientOfferRepository.Tokens(ctx, model.TextSubject, model.TextRecuento);
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
                model.LinkClientOfferBrand = current.LinkClientOfferBrand;
            }

            if (!ModelState.IsValid)
            {
                FillLookups(model.LinkCompany);
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

        private void FillLookups(long companyId)
        {
            ViewBag.Companies = ClientOfferRepository.ListCompanies(db);
            ViewBag.Systems = ClientOfferRepository.ListSystems(db);
            ViewBag.Plantillas = ClientOfferRepository.ListPlantillas(db);
            ViewBag.Designs = ClientOfferRepository.ListDesigns(db);
            ViewBag.Offers = ClientOfferRepository.ListOffers(db);
            ViewBag.Jobsides = ClientOfferRepository.ListJobsides(db);
            ViewBag.Clients = ClientOfferRepository.ListClients(db);
            ViewBag.CompanyId = companyId;
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

        private class ListItem
        {
            public long IdObject { get; set; }
            public string TextLabel { get; set; }
            public string Extra { get; set; }
            public string Third { get; set; }
            public bool Is_Active { get; set; }
        }
    }
}
