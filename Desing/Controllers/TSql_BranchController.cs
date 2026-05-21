using DAL;
using Desing.Helpers;
using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Ficha completa de sede (<see cref="TSql_Branch"/>): dirección Google Loc_* + color.
    /// Alta rápida y panel siguen en <see cref="TSql_CompanyController"/>.
    /// </summary>
    [Authorize]
    public class TSql_BranchController : BaseController
    {
        private const string BranchGooglePlacesLocBindFields =
            "Loc_Place_Id,Loc_Formatted_Address,Loc_Lat,Loc_Lng,Loc_Street_Number,Loc_Route,Loc_Subpremise," +
            "Loc_Locality,Loc_Admin_Area_1,Loc_Admin_Area_2,Loc_Postal_Code,Loc_Country_Code,Loc_Country_Name,Loc_Address_Components_Json";

        private const string BranchEditBindInclude =
            "SysObjectID,AttLabel,AttDescription,AddLetter,Attcolor," + BranchGooglePlacesLocBindFields;

        private const string BranchCreateBindInclude =
            "AttLabel,AttDescription,AddLetter,Attcolor,LinCompany," + BranchGooglePlacesLocBindFields;

        public ActionResult Create(long companyId)
        {
            if (companyId <= 0)
                return HttpNotFound();

            var company = db.TSql_Company.AsNoTracking().FirstOrDefault(c => c.SysObjectID == companyId);
            if (company == null)
                return HttpNotFound();

            ViewBag.CompanyName = company.TextLabel ?? "";
            ViewBag.CompanyDetailsUrl = Url.Action("Details", "TSql_Company", new { id = companyId });
            var branch = new TSql_Branch { LinCompany = companyId };
            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = BranchCreateBindInclude)] TSql_Branch model)
        {
            ViewBag.CompanyDetailsUrl = Url.Action("Details", "TSql_Company", new { id = model.LinCompany });

            var company = db.TSql_Company.AsNoTracking().FirstOrDefault(c => c.SysObjectID == model.LinCompany);
            ViewBag.CompanyName = company?.TextLabel ?? "";

            if (model.LinCompany <= 0 || company == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(model.AttLabel))
                ModelState.AddModelError("AttLabel", Branch.Branch_Err_NameRequired);

            if (!BranchColorHelper.TryNormalizeAttcolor(model.AttColor, out var normCreateColor))
                ModelState.AddModelError("Attcolor", Branch.Branch_Val_AttcolorHex);

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.Identity.GetUserId();
            var now = DateTime.UtcNow;
            var entity = new TSql_Branch
            {
                AttLabel = model.AttLabel.Trim(),
                AttDescription = string.IsNullOrWhiteSpace(model.AttDescription) ? null : model.AttDescription.Trim(),
                AddLetter = NormalizeBranchAddLetter(model.AddLetter),
                AttColor = normCreateColor,
                LinCompany = model.LinCompany,
                LinCreatedBy = userId,
                LinModifiedBy = userId,
                AttCreated = now,
                AttLastModification = now,
                SysUpdateNumber = 1
            };
            CopyBranchGoogleLocFields(entity, model);
            db.TSql_Branch.Add(entity);
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Branch.BranchPanel_CreateSectionTitle;
            TempData["ToastMessage"] = Branch.Branch_Msg_Created;
            return RedirectToAction("Details", "TSql_Company", new { id = model.LinCompany });
        }

        public ActionResult Details(long id)
        {
            var branch = db.TSql_Branch
                .AsNoTracking()
                .Include(b => b.TSql_Company)
                .FirstOrDefault(b => b.SysObjectID == id);
            if (branch == null)
                return HttpNotFound();
            return View(branch);
        }

        public ActionResult Edit(long id)
        {
            var branch = db.TSql_Branch
                .Include(b => b.TSql_Company)
                .FirstOrDefault(b => b.SysObjectID == id);
            if (branch == null)
                return HttpNotFound();
            ViewBag.CompanyEditUrl = Url.Action("Details", "TSql_Company", new { id = branch.LinCompany });
            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = BranchEditBindInclude)] TSql_Branch model)
        {
            var entity = db.TSql_Branch.FirstOrDefault(b => b.SysObjectID == model.SysObjectID);
            if (entity == null)
                return HttpNotFound();

            ViewBag.CompanyEditUrl = Url.Action("Details", "TSql_Company", new { id = entity.LinCompany });

            if (string.IsNullOrWhiteSpace(model.AttLabel))
                ModelState.AddModelError("AttLabel", Branch.Branch_Err_NameRequired);

            if (!BranchColorHelper.TryNormalizeAttcolor(model.AttColor, out var normColor))
                ModelState.AddModelError("Attcolor", Branch.Branch_Val_AttcolorHex);

            if (!ModelState.IsValid)
            {
                entity.AttLabel = model.AttLabel;
                entity.AttDescription = model.AttDescription;
                entity.AddLetter = model.AddLetter;
                 entity.AttColor = model.AttColor;
                CopyBranchGoogleLocFields(entity, model);
                return View(entity);
            }

            entity.AttLabel = model.AttLabel.Trim();
            entity.AttDescription = string.IsNullOrWhiteSpace(model.AttDescription) ? null : model.AttDescription.Trim();
            entity.AddLetter = NormalizeBranchAddLetter(model.AddLetter);
             entity.AttColor = normColor;
            CopyBranchGoogleLocFields(entity, model);
            entity.LinModifiedBy = User.Identity.GetUserId();
            entity.AttLastModification = DateTime.UtcNow;
            entity.SysUpdateNumber = entity.SysUpdateNumber + 1;
            db.SaveChanges();

            TempData["ToastType"] = "Act";
            TempData["ToastTitle"] = Branch.Branch_ToastTitle_Saved;
            TempData["ToastMessage"] = Branch.Branch_ToastMessage_Saved;
            return RedirectToAction("Details", "TSql_Company", new { id = entity.LinCompany });
        }

        private static void CopyBranchGoogleLocFields(TSql_Branch entity, TSql_Branch model)
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

        private static string NormalizeBranchAddLetter(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            var t = raw.Trim();
            return t.Length <= 2 ? t : t.Substring(0, 2);
        }
    }
}
