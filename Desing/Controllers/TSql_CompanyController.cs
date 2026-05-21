using DataTables.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Desing.Controllers
{
    public class TSql_CompanyController : BaseController
    {
        private const string CompanyGooglePlacesLocBindFields =
            "Loc_Place_Id,Loc_Formatted_Address,Loc_Lat,Loc_Lng,Loc_Street_Number,Loc_Route,Loc_Subpremise," +
            "Loc_Locality,Loc_Admin_Area_1,Loc_Admin_Area_2,Loc_Postal_Code,Loc_Country_Code,Loc_Country_Name,Loc_Address_Components_Json";

        private const string CompanyCreateBindInclude =
            "AddLeter,TextLabel,TextDescription,TextLogo,TextAddress_1,TextAddress_2,TextPostal_Code,TextTown_1,TextTown_2,LinkCountry,LinPlantilla,LinkLanguage," +
            CompanyGooglePlacesLocBindFields;

        private const string CompanyEditBindInclude =
            "SysObjectID,AddLeter,TextLabel,TextDescription,TextLogo,TextAddress_1,TextAddress_2,TextPostal_Code,TextTown_1,TextTown_2,LinkCountry,LinPlantilla,LinkLanguage," +
            CompanyGooglePlacesLocBindFields;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(long id)
        {
            var company = db.TSql_Company
                .AsNoTracking()
                .FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        /// <summary>Sedes de una empresa para DataTables (ficha empresa).</summary>
        [OutputCache(Duration = 1)]
        [HttpPost]
        public JsonResult ListCompanyBranches(long id, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                if (id <= 0 || !db.TSql_Company.Any(c => c.SysObjectID == id))
                {
                    return Json(DataTablesMvcJson.Create(requestModel.Draw, new object[0], 0, 0), JsonRequestBehavior.AllowGet);
                }

                IQueryable<CompanyBranchDataTablesItem> query = db.TSql_Branch
                    .Where(br => br.LinCompany == id)
                    .Select(br => new CompanyBranchDataTablesItem
                    {
                        SysObjectID = br.SysObjectID,
                        AttLabel = br.AttLabel,
                        AttDescription = br.AttDescription,
                        AddLetter = br.AddLetter,
                        Attcolor = br.AttColor
                    });

                var totalCount = query.Count();

                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p =>
                        (p.AttLabel ?? "").Contains(value) ||
                        (p.AttDescription ?? "").Contains(value) ||
                        (p.AddLetter ?? "").Contains(value));
                }

                var filteredCount = query.Count();

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    string orderColumn;
                    switch (column.Data)
                    {
                        case "LetterHtml":
                            orderColumn = "AddLetter";
                            break;
                        case "AttDescription":
                            orderColumn = "AttDescription";
                            break;
                        case "AttLabel":
                        case "AttLabelPlain":
                        default:
                            orderColumn = "AttLabel";
                            break;
                    }

                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += orderColumn +
                        (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "AttLabel asc" : orderByString);
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var rows = query.ToList();

                var ttName = HttpUtility.HtmlAttributeEncode(Branch.Branch_Details_Field_Name);
                var ttDel = HttpUtility.HtmlAttributeEncode(Branch.Branch_RowTooltip_Delete);

                var data = rows.Select(p =>
                {
                    var labelPlain = p.AttLabel ?? "";
                    var descPlain = p.AttDescription ?? "";
                    var letterRaw = (p.AddLetter ?? "").Trim();
                    var letterHtml = BuildBranchLetterBadgeHtml(letterRaw, p.Attcolor);
                    var attLabelHtml =
                        "<a title=\"" + ttName + "\" href=\"" +
                        Url.Content("~/TSql_Branch/Edit/" + p.SysObjectID) + "\">" +
                        HttpUtility.HtmlEncode(labelPlain) + "</a>";
                    var deleteBtn =
                        "<button type=\"button\" title=\"" + ttDel +
                        "\" class=\"btn btn-outline-danger btn-xs\" data-branch-delete=\"" + p.SysObjectID +
                        "\"><span class=\"icon-base ri ri-delete-bin-line\" aria-hidden=\"true\"></span></button>";
                    var rowActions = deleteBtn;

                    return new
                    {
                        SysObjectID = p.SysObjectID,
                        AttLabelPlain = labelPlain,
                        AttLabel = attLabelHtml,
                        LetterHtml = letterHtml,
                        AttDescription = HttpUtility.HtmlEncode(descPlain),
                        AttDescriptionPlain = descPlain,
                        rowActions
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private static string BuildBranchLetterBadgeHtml(string letterTrimmed, string attcolor)
        {
            if (string.IsNullOrEmpty(letterTrimmed))
                return "";

            var style = BranchColorHelper.BadgeInlineStyle(attcolor);
            if (!string.IsNullOrEmpty(style))
            {
                return "<span class=\"badge\" style=\"" + HttpUtility.HtmlAttributeEncode(style) + "\">" +
                       HttpUtility.HtmlEncode(letterTrimmed) + "</span>";
            }

            return "<span class=\"badge bg-label-secondary\">" + HttpUtility.HtmlEncode(letterTrimmed) + "</span>";
        }

        public ActionResult Create()
        {
            PopulateCountries();
            PopulatePlantillas(null);
            PopulateUiLanguages(null);
            ViewBag.BranchesPanel = BuildBranchesPanel(null);
            return View(new DAL.TSql_Company());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = CompanyCreateBindInclude)] DAL.TSql_Company model)
        {
            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Company.Val_CompanyNameRequired);
            }

            if (db.TSql_Company.Any(x => !x.BitIsDeleted && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Company.Val_DuplicateNameCreate);
            }

            if (!model.LinkLanguage.HasValue)
            {
                ModelState.AddModelError("LinkLanguage", Company.Val_UiLanguageRequired);
            }

            if (!ModelState.IsValid)
            {
                PopulateCountries(model.LinkCountry);
                PopulatePlantillas(model.LinPlantilla);
                PopulateUiLanguages(model.LinkLanguage);
                ViewBag.BranchesPanel = BuildBranchesPanel(null);
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            model.LinkMadeBy = userId;
            model.AddChangeBy = userId;
            model.AddDateMade = DateTime.UtcNow;
            model.AddLastDateChange = DateTime.UtcNow;
            model.Ntimeschanged = 1;
            model.BitIsDeleted = false;
            db.TSql_Company.Add(model);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Company.ToastTitle_CreateCompany;
            TempData["ToastMessage"] = Company.ToastMessage_CompanySaved;
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return HttpNotFound();
            }
            PopulateCountries(company.LinkCountry);
            PopulatePlantillas(company.LinPlantilla);
            PopulateUiLanguages(company.LinkLanguage);
            ViewBag.BranchesPanel = BuildBranchesPanel(id);
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = CompanyEditBindInclude)] DAL.TSql_Company model)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == model.SysObjectID);
            if (company == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Company.Val_CompanyNameRequired);
            }

            if (db.TSql_Company.Any(x => x.SysObjectID != model.SysObjectID && !x.BitIsDeleted && x.TextLabel == model.TextLabel))
            {
                ModelState.AddModelError("TextLabel", Company.Val_DuplicateNameEdit);
            }

            if (!model.LinkLanguage.HasValue)
            {
                ModelState.AddModelError("LinkLanguage", Company.Val_UiLanguageRequired);
            }

            if (!ModelState.IsValid)
            {
                PopulateCountries(model.LinkCountry);
                PopulatePlantillas(model.LinPlantilla);
                PopulateUiLanguages(model.LinkLanguage);
                ViewBag.BranchesPanel = BuildBranchesPanel(model.SysObjectID);
                return View(model);
            }

            company.AddLeter = model.AddLeter;
            company.TextLabel = model.TextLabel;
            company.TextDescription = model.TextDescription;
            company.TextLogo = model.TextLogo;
            company.TextAddress_1 = model.TextAddress_1;
            company.TextAddress_2 = model.TextAddress_2;
            company.TextPostal_Code = model.TextPostal_Code;
            company.TextTown_1 = model.TextTown_1;
            company.TextTown_2 = model.TextTown_2;
            company.LinkCountry = model.LinkCountry;
            company.LinPlantilla = model.LinPlantilla;
            company.LinkLanguage = model.LinkLanguage;
            CopyCompanyGoogleLocFields(company, model);
            company.AddChangeBy = User.Identity.GetUserId();
            company.AddLastDateChange = DateTime.UtcNow;
            company.Ntimeschanged = company.Ntimeschanged + 1;
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Company.ToastTitle_EditCompany;
            TempData["ToastMessage"] = Company.ToastMessage_CompanyUpdated;
            return RedirectToAction("Index");
        }

        [OutputCache(Duration = 1)]
        public JsonResult ListTSql_Company([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                IQueryable<CompanyModel> query = from company in db.TSql_Company
                                                 join country in db.TSql_Countrys on company.LinkCountry equals country.IdObject into countryGroup
                                                 from country in countryGroup.DefaultIfEmpty()
                                                 select new CompanyModel
                                                 {
                                                     SysObjectID = company.SysObjectID,
                                                     AddLeter = company.AddLeter,
                                                     TextLabel = company.TextLabel,
                                                     TextLogo = company.TextLogo,
                                                     TextDescription = company.TextDescription,
                                                     TextAddress_1 = company.TextAddress_1,
                                                     TextAddress_2 = company.TextAddress_2,
                                                     TextPostal_Code = company.TextPostal_Code,
                                                     TextTown_1 = company.TextTown_1,
                                                     Country = country.TextLabel,
                                                     TextFlag = country.TextFlag,
                                                     BitIsDeleted = company.BitIsDeleted
                                                 };

                var totalCount = query.Count();

                // Apply filters
                if (requestModel.Search.Value != String.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => (p.AddLeter ?? "").Contains(value) ||
                                             (p.TextLabel ?? "").Contains(value) ||
                                             (p.TextDescription ?? "").Contains(value) ||
                                             (p.TextAddress_1 ?? "").Contains(value) ||
                                             (p.TextAddress_2 ?? "").Contains(value) ||
                                             (p.TextPostal_Code.HasValue && p.TextPostal_Code.Value.ToString().Contains(value)) ||
                                             (p.Country ?? "").Contains(value)

                    );
                }

                var filteredCount = query.Count();

                // Sort
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = String.Empty;
                string orderColumn = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "AddLeter":
                            orderColumn = "AddLeter";
                            break;
                        case "TextLabel":
                            orderColumn = "TextLabel";
                            break;
                        case "TextAddress_1":
                            orderColumn = "TextAddress_1";
                            break;
                        case "TextAddress_2":
                            orderColumn = "TextAddress_2";
                            break;
                        case "TextPostal_Code":
                            orderColumn = "TextPostal_Code";
                            break;
                        case "country":
                            orderColumn = "Country";
                            break;
                        default:
                            orderColumn = "TextLabel";
                            break;
                    }
                    orderByString += orderByString != String.Empty ? "," : "";
                    orderByString += (column.Data == "TextLabel" ? "TextLabel" : orderColumn) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                }
                query = query.OrderBy(orderByString == String.Empty ? "TextLabel asc" : orderByString);
                // Paging
                query = query.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);

                var data = query.ToList().Select(p =>
                {
                    var ttOpen = HttpUtility.HtmlAttributeEncode(Company.List_LinkOpenTooltip);
                    var ttEdit = HttpUtility.HtmlAttributeEncode(Company.List_LinkEditTooltip);
                    var ttDelete = HttpUtility.HtmlAttributeEncode(Company.List_LinkDeleteTooltip);
                    var ttToggle = HttpUtility.HtmlAttributeEncode(Company.List_LinkToggleTooltip);
                    var editBtn =
                        "<a title=\"" + ttEdit + "\" href=\"" +
                        Url.Content("~/TSql_Company/Edit/" + p.SysObjectID) +
                        "\" class=\"btn btn-warning btn-xs\"><span class=\"fas fa-edit\" aria-hidden=\"true\"></span></a>";
                    var deleteBtn = HasDependencies(p.SysObjectID)
                        ? ""
                        : "<a title=\"" + ttDelete + "\" onclick=\"DeleteCompany('" + p.SysObjectID +
                          "')\" class=\"btn btn-danger btn-xs\"><span class=\"fas fa-trash-alt\" aria-hidden=\"true\"></span></a>";
                    var toggleBtn =
                        "<a title=\"" + ttToggle + "\" onclick=\"ToggleCompany('" + p.SysObjectID +
                        "')\" class=\"btn btn-info btn-xs\"><span class=\"fas fa-sync\" aria-hidden=\"true\"></span></a>";
                    var rowActions = toggleBtn + "&nbsp;" + editBtn +
                        (string.IsNullOrEmpty(deleteBtn) ? "" : "&nbsp;" + deleteBtn);

                    return new
                    {
                        SysObjectID = p.SysObjectID,
                        TextLabelPlain = p.TextLabel ?? "",
                        TextLabel =
                            "<a title=\"" + ttOpen + "\" href=\"" +
                            Url.Content("~/TSql_Company/Details/" + p.SysObjectID) + "\">" +
                            HttpUtility.HtmlEncode(p.TextLabel ?? "") +
                            "</a>",
                        AddLeter = p.AddLeter,
                        TextLogo = p.TextLogo,
                        TextDescription = p.TextDescription,
                        TextAddress_1 = p.TextAddress_1,
                        TextAddress_2 = p.TextAddress_2,
                        TextPostal_Code = p.TextPostal_Code,
                        TextTown_1 = p.TextTown_1,
                        TextFlag = p.Country ?? "",
                        Country = p.Country ?? "",
                        BitIsDeleted = p.BitIsDeleted,
                        rowActions
                    };
                }).ToList();
                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult DeleteCompany(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return Json(new { IsOk = false, Message = Company.Err_CompanyNotFound });
            }

            if (HasDependencies(id))
            {
                return Json(new { IsOk = false, Message = Company.Err_CannotDeleteRelated });
            }

            db.TSql_Company.Remove(company);
            db.SaveChanges();
            return Json(new { IsOk = true, Message = Company.Msg_CompanyDeleted });
        }

        [HttpPost]
        public JsonResult ToggleCompany(long id)
        {
            var company = db.TSql_Company.FirstOrDefault(x => x.SysObjectID == id);
            if (company == null)
            {
                return Json(new { IsOk = false, Message = Company.Err_CompanyNotFound });
            }

            company.BitIsDeleted = !company.BitIsDeleted;
            company.AddChangeBy = User.Identity.GetUserId();
            company.AddLastDateChange = DateTime.UtcNow;
            company.Ntimeschanged = company.Ntimeschanged + 1;
            db.SaveChanges();

            return Json(new
            {
                IsOk = true,
                Message = company.BitIsDeleted ? Company.Msg_CompanyPaused : Company.Msg_CompanyResumed
            });
        }

        [HttpGet]
        public ActionResult BranchPanelList(long companyId)
        {
            var panel = BuildBranchesPanel(companyId);
            return PartialView("_CompanyBranchesPanelList", panel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateCompanyBranch(
            long companyId,
            string attLabel,
            string attDescription,
            string addLetter,
            string attcolor,
            string Loc_Place_Id,
            string Loc_Formatted_Address,
            decimal? Loc_Lat,
            decimal? Loc_Lng,
            string Loc_Street_Number,
            string Loc_Route,
            string Loc_Subpremise,
            string Loc_Locality,
            string Loc_Admin_Area_1,
            string Loc_Admin_Area_2,
            string Loc_Postal_Code,
            string Loc_Country_Code,
            string Loc_Country_Name,
            string Loc_Address_Components_Json)
        {
            if (companyId <= 0)
                return Json(new { IsOk = false, Message = Branch.Branch_Err_InvalidCompany });

            if (!db.TSql_Company.Any(c => c.SysObjectID == companyId))
                return Json(new { IsOk = false, Message = Company.Err_CompanyNotFound });

            if (string.IsNullOrWhiteSpace(attLabel))
                return Json(new { IsOk = false, Message = Branch.Branch_Err_NameRequired });

            if (!BranchColorHelper.TryNormalizeAttcolor(attcolor, out var normColor))
                return Json(new { IsOk = false, Message = Branch.Branch_Val_AttcolorHex });

            var userId = User.Identity.GetUserId();
            var now = DateTime.UtcNow;
            var branch = new DAL.TSql_Branch
            {
                AttLabel = attLabel.Trim(),
                AttDescription = string.IsNullOrWhiteSpace(attDescription) ? null : attDescription.Trim(),
                LinCompany = companyId,
                AddLetter = NormalizeBranchAddLetter(addLetter),
                AttColor = normColor,
                LinCreatedBy = userId,
                LinModifiedBy = userId,
                AttCreated = now,
                AttLastModification = now,
                SysUpdateNumber = 1
            };
            ApplyBranchGoogleLoc(
                branch,
                Loc_Place_Id,
                Loc_Formatted_Address,
                Loc_Lat,
                Loc_Lng,
                Loc_Street_Number,
                Loc_Route,
                Loc_Subpremise,
                Loc_Locality,
                Loc_Admin_Area_1,
                Loc_Admin_Area_2,
                Loc_Postal_Code,
                Loc_Country_Code,
                Loc_Country_Name,
                Loc_Address_Components_Json);
            db.TSql_Branch.Add(branch);
            db.SaveChanges();

            return Json(new { IsOk = true, Message = Branch.Branch_Msg_Created });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCompanyBranch(
            long sysObjectId,
            long companyId,
            string attLabel,
            string attDescription,
            string addLetter,
            string attcolor,
            string Loc_Place_Id,
            string Loc_Formatted_Address,
            decimal? Loc_Lat,
            decimal? Loc_Lng,
            string Loc_Street_Number,
            string Loc_Route,
            string Loc_Subpremise,
            string Loc_Locality,
            string Loc_Admin_Area_1,
            string Loc_Admin_Area_2,
            string Loc_Postal_Code,
            string Loc_Country_Code,
            string Loc_Country_Name,
            string Loc_Address_Components_Json)
        {
            if (companyId <= 0 || sysObjectId <= 0)
                return Json(new { IsOk = false, Message = Branch.Branch_Err_InvalidData });

            var branch = db.TSql_Branch.FirstOrDefault(b => b.SysObjectID == sysObjectId && b.LinCompany == companyId);
            if (branch == null)
                return Json(new { IsOk = false, Message = Branch.Branch_Err_NotFound });

            if (string.IsNullOrWhiteSpace(attLabel))
                return Json(new { IsOk = false, Message = Branch.Branch_Err_NameRequired });

            if (!BranchColorHelper.TryNormalizeAttcolor(attcolor, out var normColor))
                return Json(new { IsOk = false, Message = Branch.Branch_Val_AttcolorHex });

            branch.AttLabel = attLabel.Trim();
            branch.AttDescription = string.IsNullOrWhiteSpace(attDescription) ? null : attDescription.Trim();
            branch.AddLetter = NormalizeBranchAddLetter(addLetter);
            branch.AttColor = normColor;
            ApplyBranchGoogleLoc(
                branch,
                Loc_Place_Id,
                Loc_Formatted_Address,
                Loc_Lat,
                Loc_Lng,
                Loc_Street_Number,
                Loc_Route,
                Loc_Subpremise,
                Loc_Locality,
                Loc_Admin_Area_1,
                Loc_Admin_Area_2,
                Loc_Postal_Code,
                Loc_Country_Code,
                Loc_Country_Name,
                Loc_Address_Components_Json);
            branch.LinModifiedBy = User.Identity.GetUserId();
            branch.AttLastModification = DateTime.UtcNow;
            branch.SysUpdateNumber = branch.SysUpdateNumber + 1;

            db.SaveChanges();

            return Json(new { IsOk = true, Message = Branch.Branch_Msg_Updated });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteCompanyBranch(long sysObjectId, long companyId)
        {
            if (companyId <= 0 || sysObjectId <= 0)
                return Json(new { IsOk = false, Message = Branch.Branch_Err_InvalidData });

            var branch = db.TSql_Branch.FirstOrDefault(b => b.SysObjectID == sysObjectId && b.LinCompany == companyId);
            if (branch == null)
                return Json(new { IsOk = false, Message = Branch.Branch_Err_NotFound });

            db.TSql_Branch.Remove(branch);
            db.SaveChanges();

            return Json(new { IsOk = true, Message = Branch.Branch_Msg_Deleted });
        }

        private CompanyBranchesPanelModel BuildBranchesPanel(long? companyId)
        {
            var model = new CompanyBranchesPanelModel { CompanyId = companyId };
            if (!companyId.HasValue || companyId.Value <= 0)
                return model;

            var id = companyId.Value;

            model.BranchRows = (from br in db.TSql_Branch
                                where br.LinCompany == id
                                orderby br.AttLabel
                                select new CompanyBranchListRow
                                {
                                    SysObjectID = br.SysObjectID,
                                    AttLabel = br.AttLabel,
                                    AttDescription = br.AttDescription,
                                    AddLetter = br.AddLetter,
                                    Attcolor = br.AttColor,
                                    Loc_Place_Id = br.Loc_Place_Id,
                                    Loc_Formatted_Address = br.Loc_Formatted_Address,
                                    Loc_Lat = br.Loc_Lat,
                                    Loc_Lng = br.Loc_Lng,
                                    Loc_Street_Number = br.Loc_Street_Number,
                                    Loc_Route = br.Loc_Route,
                                    Loc_Subpremise = br.Loc_Subpremise,
                                    Loc_Locality = br.Loc_Locality,
                                    Loc_Admin_Area_1 = br.Loc_Admin_Area_1,
                                    Loc_Admin_Area_2 = br.Loc_Admin_Area_2,
                                    Loc_Postal_Code = br.Loc_Postal_Code,
                                    Loc_Country_Code = br.Loc_Country_Code,
                                    Loc_Country_Name = br.Loc_Country_Name,
                                    Loc_Address_Components_Json = br.Loc_Address_Components_Json
                                }).ToList();

            foreach (var row in model.BranchRows)
            {
                if (!string.IsNullOrEmpty(row.AddLetter))
                    row.AddLetter = row.AddLetter.Trim();
                if (row.AttDescription != null)
                    row.AttDescription = row.AttDescription.TrimEnd();
                row.LocJsonDom = SerializeBranchLocForModal(row);
            }

            return model;
        }

        private static string SerializeBranchLocForModal(CompanyBranchListRow r)
        {
            var o = new
            {
                pi = r.Loc_Place_Id,
                fa = r.Loc_Formatted_Address,
                lat = r.Loc_Lat,
                lng = r.Loc_Lng,
                sn = r.Loc_Street_Number,
                rt = r.Loc_Route,
                sp = r.Loc_Subpremise,
                loc = r.Loc_Locality,
                a1 = r.Loc_Admin_Area_1,
                a2 = r.Loc_Admin_Area_2,
                pc = r.Loc_Postal_Code,
                cc = r.Loc_Country_Code,
                cn = r.Loc_Country_Name,
                cj = r.Loc_Address_Components_Json
            };
            return HttpUtility.HtmlAttributeEncode(JsonConvert.SerializeObject(o));
        }

        private static string NullIfWs(string s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static void ApplyBranchGoogleLoc(
            DAL.TSql_Branch branch,
            string locPlaceId,
            string locFormattedAddress,
            decimal? locLat,
            decimal? locLng,
            string locStreetNumber,
            string locRoute,
            string locSubpremise,
            string locLocality,
            string locAdminArea1,
            string locAdminArea2,
            string locPostalCode,
            string locCountryCode,
            string locCountryName,
            string locAddressComponentsJson)
        {
            branch.Loc_Place_Id = NullIfWs(locPlaceId);
            branch.Loc_Formatted_Address = NullIfWs(locFormattedAddress);
            branch.Loc_Lat = locLat;
            branch.Loc_Lng = locLng;
            branch.Loc_Street_Number = NullIfWs(locStreetNumber);
            branch.Loc_Route = NullIfWs(locRoute);
            branch.Loc_Subpremise = NullIfWs(locSubpremise);
            branch.Loc_Locality = NullIfWs(locLocality);
            branch.Loc_Admin_Area_1 = NullIfWs(locAdminArea1);
            branch.Loc_Admin_Area_2 = NullIfWs(locAdminArea2);
            branch.Loc_Postal_Code = NullIfWs(locPostalCode);
            branch.Loc_Country_Code = NullIfWs(locCountryCode);
            branch.Loc_Country_Name = NullIfWs(locCountryName);
            branch.Loc_Address_Components_Json = NullIfWs(locAddressComponentsJson);
        }

        private static string NormalizeBranchAddLetter(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            var t = raw.Trim();
            return t.Length <= 2 ? t : t.Substring(0, 2);
        }

        private static void CopyCompanyGoogleLocFields(DAL.TSql_Company entity, DAL.TSql_Company model)
        {
            entity.Loc_Place_Id = model.Loc_Place_Id;
            entity.Loc_Formatted_Address = model.Loc_Formatted_Address;
            entity.Loc_Lat = model.Loc_Lat;
            entity.Loc_Lng = model.Loc_Lng;
            entity.Loc_Street_Number = model.Loc_Street_Number;
            entity.Loc_Route = model.Loc_Route;
            entity.Loc_Subpremise = model.Loc_Subpremise;
            entity.Loc_Locality = model.Loc_Locality;
            entity.Loc_Admin_Area_1 = model.Loc_Admin_Area_1;
            entity.Loc_Admin_Area_2 = model.Loc_Admin_Area_2;
            entity.Loc_Postal_Code = model.Loc_Postal_Code;
            entity.Loc_Country_Code = model.Loc_Country_Code;
            entity.Loc_Country_Name = model.Loc_Country_Name;
            entity.Loc_Address_Components_Json = model.Loc_Address_Components_Json;
        }

        private bool HasDependencies(long companyId)
        {
            var hasEmployees = db.TSql_Employee.Any(x => x.LinCompany == companyId && !x.AttIsDeleted);
            var hasBranches = db.TSql_Branch.Any(x => x.LinCompany == companyId);
            return hasEmployees || hasBranches;
        }

        private void PopulateCountries(long? selected = null)
        {
            ViewBag.LinkCountry = new SelectList(
                db.TSql_Countrys.OrderBy(x => x.TextLabel).ToList(),
                "IdObject",
                "TextLabel",
                selected
            );
        }

        /// <summary>Idiomas UI activos para <see cref="TSql_Company.LinkLanguage"/> (bandera vía país del idioma).</summary>
        private void PopulateUiLanguages(long? selectedLinkLanguage)
        {
            var langs = db.TSql_language
                .AsNoTracking()
                .Include(l => l.TSql_Countrys)
                .Where(l => !l.Is_Delete && l.Is_Active)
                .OrderByDescending(l => l.Is_Default)
                .ThenBy(l => l.TextLabel)
                .ToList()
                .Select(l => new CompanyUiLanguageOption
                {
                    IdObject = l.IdObject,
                    TextLabel = l.TextLabel,
                    TextCode = l.TextCode,
                    FlagVirtualPath = LanguageUiHelper.NormalizeCountryFlagVirtualPath(
                        l.TSql_Countrys != null ? l.TSql_Countrys.TextFlag : null)
                })
                .ToList();

            ViewBag.UiLanguageOptions = langs;
            ViewBag.LinkLanguageSelected = selectedLinkLanguage;
        }

        /// <summary>
        /// Lista de plantillas para el desplegable de empresa (incluye opcion vacia = usar plantilla global por defecto).
        /// </summary>
        private void PopulatePlantillas(long? selected = null)
        {
            var list = new System.Collections.Generic.List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = Company.Plantilla_InheritGlobalDefault }
            };
            foreach (var item in PlantillaController.GetSelectList(db, selected))
            {
                item.Selected = false;
                list.Add(item);
            }
            if (!selected.HasValue)
                list[0].Selected = true;
            else
            {
                list[0].Selected = false;
                var match = list.FirstOrDefault(x => x.Value == selected.Value.ToString());
                if (match != null) match.Selected = true;
            }
            ViewBag.LinPlantilla = list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}