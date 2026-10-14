using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Desing.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            Session["SessionListMaterial"] = new string[] { };
            TempData.Clear();
            return View();
        }

        [HttpPost]
        public JsonResult ListDashboardJobs([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var rows = db.TSql_Jobside.AsNoTracking()
                    .Where(j => !j.Is_Delete && j.LinkMadeBy == userId)
                    .OrderByDescending(j => j.AddDateMade)
                    .ThenByDescending(j => j.IdObject)
                    .Take(5)
                    .Select(j => new
                    {
                        j.IdObject,
                        AddNJobside = j.AddNJobside ?? "",
                        TextLabel = j.TextLabel ?? ""
                    })
                    .ToList();
                var filteredCount = rows.Count;

                var tt = HttpUtility.HtmlAttributeEncode(Jobside.List_LinkWorkspaceTooltip);
                var data = rows.Select(p =>
                {
                    var url = Url.Action("Details", "Jobside", new { id = p.IdObject });
                    var encUrl = HttpUtility.HtmlAttributeEncode(url);
                    var btn =
                        "<a href=\"" + encUrl + "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + tt +
                        "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>";
                    return new
                    {
                        AddNJobside = HttpUtility.HtmlEncode(p.AddNJobside),
                        TextLabel = HttpUtility.HtmlEncode(p.TextLabel),
                        actions = btn
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, filteredCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ListDashboardOffers([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var rows = (from o in db.TSql_Offers.AsNoTracking()
                            join s in db.TSql_OfferState.AsNoTracking() on o.LinkOfferState equals s.IdObject into sg
                            from s in sg.DefaultIfEmpty()
                            where !o.Is_Delete && o.LinkMadeBy == userId
                            orderby o.AddDateMade descending, o.IdObject descending
                            select new
                            {
                                o.IdObject,
                                AddOfferNumber = o.AddOfferNumber ?? "",
                                TextLabel = o.TextLabel ?? "",
                                StateColor = s == null ? null : s.AddColor
                            })
                    .Take(5)
                    .ToList();
                var filteredCount = rows.Count;

                var tt = HttpUtility.HtmlAttributeEncode(Jobside.Offers_DetailsTooltip);
                var data = rows.Select(p =>
                {
                    var url = Url.Action("OfferDetails", "Jobside", new { id = p.IdObject });
                    var encUrl = HttpUtility.HtmlAttributeEncode(url);
                    var btn =
                        "<a href=\"" + encUrl + "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + tt +
                        "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>";
                    return new
                    {
                        AddOfferNumber = OfferDisplayHelper.BuildOfferNumberCellHtml(p.AddOfferNumber, p.StateColor),
                        TextLabel = HttpUtility.HtmlEncode(p.TextLabel),
                        actions = btn
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, filteredCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /// <summary>
        /// Pestaña Ofertas: una fila por oferta.
        /// Número, descripción de la oferta, nº de obra, descripción de la obra,
        /// nº de diseños y un + para crear.
        /// El árbol (mes y año, y debajo los diseños) va en la propiedad tree.
        /// </summary>
        [HttpPost]
        public JsonResult ListDashboardDesigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var offers = db.TSql_Offers.AsNoTracking().Where(o => !o.Is_Delete);
                var totalCount = offers.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    offers = offers.Where(o =>
                        (o.AddOfferNumber ?? "").Contains(value) ||
                        (o.AddDescription ?? "").Contains(value) ||
                        (o.TextLabel ?? "").Contains(value) ||
                        db.TSql_Jobside.Any(j =>
                            !j.Is_Delete &&
                            j.IdObject == o.LinkJobside &&
                            ((j.AddNJobside ?? "").Contains(value) ||
                             (j.TextLabel ?? "").Contains(value) ||
                             (j.TextJobsideNotes ?? "").Contains(value))) ||
                        db.TSql_Client_V2.Any(c =>
                            !c.Is_Delete &&
                            c.IdObject == o.LinkClient_V2 &&
                            (c.TextLabel ?? "").Contains(value)) ||
                        db.TSql_Design_V2.Any(d =>
                            !d.AttIsDeleted &&
                            d.LinkOffers == o.IdObject &&
                            (d.AttLabel ?? "").Contains(value)));
                }

                var filteredCount = offers.Count();

                var q = from o in offers
                        join j in db.TSql_Jobside.AsNoTracking().Where(j => !j.Is_Delete)
                            on o.LinkJobside equals j.IdObject into jg
                        from j in jg.DefaultIfEmpty()
                        join s in db.TSql_OfferState.AsNoTracking() on o.LinkOfferState equals s.IdObject into sg
                        from s in sg.DefaultIfEmpty()
                        select new
                        {
                            OfferId = o.IdObject,
                            JobsideId = o.LinkJobside,
                            OfferNumber = o.AddOfferNumber ?? "",
                            OfferStateColor = s == null ? null : s.AddColor,
                            OfferName = o.TextLabel ?? "",
                            Description = (o.AddDescription != null && o.AddDescription != "")
                                ? o.AddDescription
                                : (o.TextLabel ?? ""),
                            JobNumber = j == null ? "" : (j.AddNJobside ?? ""),
                            JobDescription = j == null
                                ? ""
                                : ((j.TextJobsideNotes != null && j.TextJobsideNotes != "")
                                    ? j.TextJobsideNotes
                                    : (j.TextLabel ?? "")),
                            DesignCount = db.TSql_Design_V2.Count(d => !d.AttIsDeleted && d.LinkOffers == o.IdObject),
                            LastMove = db.TSql_Design_V2
                                .Where(d => !d.AttIsDeleted && d.LinkOffers == o.IdObject)
                                .Max(d => (DateTime?)d.AttChange)
                        };

                var orderBy = "";
                if (requestModel.Columns != null)
                {
                    foreach (var column in requestModel.Columns.GetSortedColumns())
                    {
                        string orderColumn;
                        switch ((column.Data ?? "").ToLowerInvariant())
                        {
                            case "offernumber":
                                orderColumn = "OfferNumber";
                                break;
                            case "description":
                                orderColumn = "Description";
                                break;
                            case "jobnumber":
                                orderColumn = "JobNumber";
                                break;
                            case "jobdescription":
                                orderColumn = "JobDescription";
                                break;
                            case "designcount":
                                orderColumn = "DesignCount";
                                break;
                            default:
                                continue;
                        }
                        if (orderBy.Length > 0)
                        {
                            orderBy += ", ";
                        }
                        orderBy += orderColumn +
                            (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    }
                }

                var page = q.OrderBy(string.IsNullOrEmpty(orderBy)
                    ? "LastMove desc, OfferId desc"
                    : orderBy)
                    .ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

                var offerIds = rows.Where(p => p.DesignCount > 0).Select(p => p.OfferId).ToList();
                var designsByOffer = offerIds.Count == 0
                    ? new Dictionary<long, List<DashOfferTreeDesign>>()
                    : (from d in db.TSql_Design_V2.AsNoTracking()
                       join o in db.TSql_Offers.AsNoTracking() on d.LinkOffers equals o.IdObject
                       join c in db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete)
                           on o.LinkClient_V2 equals c.IdObject into cg
                       from c in cg.DefaultIfEmpty()
                       where !d.AttIsDeleted && offerIds.Contains(d.LinkOffers)
                       select new DashOfferTreeDesign
                       {
                           OfferId = d.LinkOffers,
                           DesignId = d.SysObjectID,
                           AttLabel = d.AttLabel ?? "",
                           ClientName = c == null ? "" : (c.TextLabel ?? ""),
                           AttCreated = d.AttCreated,
                           AttThumbnail = d.AttThumbnail
                       })
                        .ToList()
                        .GroupBy(d => d.OfferId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                var ttNewFmt = Jobside.DashDesigns_Tip_NewOfferFmt;
                var expandTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tree_Expand);
                var collapseTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tree_Collapse);
                var data = rows.Select(p =>
                {
                    var offerName = !string.IsNullOrWhiteSpace(p.OfferName)
                        ? p.OfferName
                        : (!string.IsNullOrWhiteSpace(p.Description) ? p.Description : p.OfferNumber);
                    var ttNew = HttpUtility.HtmlAttributeEncode(ttNewFmt.Replace("{0}", offerName));
                    var newDesign =
                        "<button type=\"button\" class=\"btn btn-sm btn-icon dash-new-design\" data-offer-id=\"" +
                        p.OfferId + "\" data-jobside-id=\"" + p.JobsideId + "\" data-offer-number=\"" +
                        HttpUtility.HtmlAttributeEncode(p.OfferNumber) + "\" title=\"" + ttNew +
                        "\" aria-label=\"" + ttNew + "\"><span class=\"dash-tree-sign dash-new-design-sign\" aria-hidden=\"true\">+</span></button>";
                    List<DashOfferTreeDesign> designs;
                    designsByOffer.TryGetValue(p.OfferId, out designs);
                    var toggle = "";
                    if (p.DesignCount > 0)
                    {
                        toggle =
                            "<button type=\"button\" class=\"btn btn-sm btn-icon dash-tree-toggle dash-client-tree-root\" aria-expanded=\"false\" title=\"" +
                            expandTip + "\" aria-label=\"" + expandTip + "\" data-expand=\"" + expandTip +
                            "\" data-collapse=\"" + collapseTip + "\"><span class=\"dash-tree-sign\" aria-hidden=\"true\">+</span></button>";
                    }
                    return new
                    {
                        treeToggle = toggle,
                        OfferNumber = OfferDisplayHelper.BuildOfferNumberCellHtml(p.OfferNumber, p.OfferStateColor),
                        Description = HttpUtility.HtmlEncode(p.Description),
                        JobNumber = HttpUtility.HtmlEncode(p.JobNumber),
                        JobDescription = HttpUtility.HtmlEncode(p.JobDescription),
                        DesignCount = p.DesignCount,
                        actions = newDesign,
                        tree = BuildOfferDesignTree(designs, expandTip, collapseTip)
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /// <summary>
        /// Pestaña Diseño: una fila por diseño. La vista de fotos pide view=photos
        /// y devuelve la tarjeta con la imagen y el nombre de la oferta.
        /// </summary>
        [HttpPost]
        public JsonResult ListDashboardAllDesigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var liveOffers = db.TSql_Offers.AsNoTracking().Where(o => !o.Is_Delete);
                var designs = db.TSql_Design_V2.AsNoTracking().Where(d => !d.AttIsDeleted);
                var totalCount = designs.Count(d => liveOffers.Any(o => o.IdObject == d.LinkOffers));

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    designs = designs.Where(d =>
                        (d.AttLabel ?? "").Contains(value) ||
                        (d.AttDescription ?? "").Contains(value) ||
                        db.AspNetUsers.Any(u =>
                            u.Id == d.LinCreatedBy &&
                            (u.UserName ?? "").Contains(value)) ||
                        db.TSql_Employee.Any(e =>
                            !e.AttIsDeleted &&
                            e.LinAspNetUsert == d.LinCreatedBy &&
                            ((e.AttName ?? "").Contains(value) ||
                             (e.AttSurname ?? "").Contains(value))) ||
                        liveOffers.Any(o =>
                            o.IdObject == d.LinkOffers &&
                            ((o.AddOfferNumber ?? "").Contains(value) ||
                             (o.TextLabel ?? "").Contains(value) ||
                             (o.AddDescription ?? "").Contains(value) ||
                             db.TSql_Client_V2.Any(c =>
                                 !c.Is_Delete &&
                                 c.IdObject == o.LinkClient_V2 &&
                                 (c.TextLabel ?? "").Contains(value)) ||
                             db.TSql_Jobside.Any(j =>
                                 !j.Is_Delete &&
                                 j.IdObject == o.LinkJobside &&
                                 ((j.AddNJobside ?? "").Contains(value) ||
                                  (j.TextLabel ?? "").Contains(value) ||
                                  (j.TextJobsideNotes ?? "").Contains(value))))));
                }

                var q = from d in designs
                        join o in liveOffers on d.LinkOffers equals o.IdObject
                        join s in db.TSql_OfferState.AsNoTracking() on o.LinkOfferState equals s.IdObject into sg
                        from s in sg.DefaultIfEmpty()
                        join c in db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete)
                            on o.LinkClient_V2 equals c.IdObject into cg
                        from c in cg.DefaultIfEmpty()
                        join j in db.TSql_Jobside.AsNoTracking().Where(j => !j.Is_Delete)
                            on o.LinkJobside equals j.IdObject into jg
                        from j in jg.DefaultIfEmpty()
                        select new
                        {
                            DesignId = d.SysObjectID,
                            DesignName = d.AttLabel ?? "",
                            Created = d.AttCreated,
                            Changed = d.AttChange,
                            CreatedById = d.LinCreatedBy ?? "",
                            Thumbnail = d.AttThumbnail,
                            OfferId = o.IdObject,
                            OfferNumber = o.AddOfferNumber ?? "",
                            OfferStateColor = s == null ? null : s.AddColor,
                            OfferName = o.TextLabel ?? "",
                            Description = (o.AddDescription != null && o.AddDescription != "")
                                ? o.AddDescription
                                : (o.TextLabel ?? ""),
                            ClientName = c == null ? "" : (c.TextLabel ?? ""),
                            JobNumber = j == null ? "" : (j.AddNJobside ?? ""),
                            JobDescription = j == null
                                ? ""
                                : ((j.TextJobsideNotes != null && j.TextJobsideNotes != "")
                                    ? j.TextJobsideNotes
                                    : (j.TextLabel ?? ""))
                        };

                var filteredCount = q.Count();
                var orderBy = "";
                if (requestModel.Columns != null)
                {
                    foreach (var column in requestModel.Columns.GetSortedColumns())
                    {
                        string orderColumn;
                        switch ((column.Data ?? "").ToLowerInvariant())
                        {
                            case "designname":
                                orderColumn = "DesignName";
                                break;
                            case "offernumber":
                                orderColumn = "OfferNumber";
                                break;
                            case "description":
                                orderColumn = "Description";
                                break;
                            case "clientname":
                                orderColumn = "ClientName";
                                break;
                            case "jobnumber":
                                orderColumn = "JobNumber";
                                break;
                            case "jobdescription":
                                orderColumn = "JobDescription";
                                break;
                            case "created":
                                orderColumn = "Created";
                                break;
                            default:
                                continue;
                        }
                        if (orderBy.Length > 0)
                        {
                            orderBy += ", ";
                        }
                        orderBy += orderColumn +
                            (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    }
                }

                var rows = q.OrderBy(string.IsNullOrEmpty(orderBy)
                    ? "Created desc, DesignId desc"
                    : orderBy)
                    .ApplyDataTablesPaging(requestModel.Start, requestModel.Length)
                    .ToList();

                var wantPhotos = string.Equals(Request["view"], "photos", StringComparison.OrdinalIgnoreCase);
                var photos = LoadDesignPhotos(rows.Select(p => p.DesignId).ToList());
                var creators = wantPhotos
                    ? ResolveDesignCreatorNames(rows.Select(p => p.CreatedById))
                    : new Dictionary<string, string>(StringComparer.Ordinal);
                var openTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tip_OpenDesign);
                var data = rows.Select(p =>
                {
                    var offerName = !string.IsNullOrWhiteSpace(p.OfferName) ? p.OfferName : p.Description;
                    var url = OfferDesignWorkspaceHelper.BuildViewerUrl(Url, p.OfferId, p.DesignId, p.Thumbnail);
                    var eye =
                        "<a href=\"" + HttpUtility.HtmlAttributeEncode(url) +
                        "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + openTip +
                        "\" aria-label=\"" + openTip +
                        "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>";
                    string photo = null;
                    photos.TryGetValue(p.DesignId, out photo);
                    string creator;
                    if (!creators.TryGetValue(p.CreatedById ?? "", out creator) || string.IsNullOrWhiteSpace(creator))
                    {
                        creator = Jobside.Details_NoValue;
                    }
                    return new
                    {
                        photo = BuildDesignRowPhoto(photo, p.DesignName, url),
                        DesignName = HttpUtility.HtmlEncode(p.DesignName),
                        OfferNumber = OfferDisplayHelper.BuildOfferNumberCellHtml(p.OfferNumber, p.OfferStateColor),
                        Description = HttpUtility.HtmlEncode(p.Description),
                        ClientName = HttpUtility.HtmlEncode(p.ClientName),
                        JobNumber = HttpUtility.HtmlEncode(p.JobNumber),
                        JobDescription = HttpUtility.HtmlEncode(p.JobDescription),
                        Created = p.Created.ToString("dd/MM/yyyy HH:mm"),
                        actions = eye,
                        card = wantPhotos
                            ? BuildDesignPhotoCard(new DashDesignCardInput
                            {
                                DesignId = p.DesignId,
                                PhotoRaw = photo,
                                DesignName = p.DesignName,
                                OfferName = offerName,
                                OfferNumberHtml = OfferDisplayHelper.BuildOfferNumberCellHtml(p.OfferNumber, p.OfferStateColor),
                                ClientName = p.ClientName,
                                JobNumber = p.JobNumber,
                                JobDescription = p.JobDescription,
                                CreatedBy = creator,
                                Created = p.Created,
                                Changed = p.Changed,
                                ViewerUrl = url
                            })
                            : ""
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private Dictionary<long, string> LoadDesignPhotos(List<long> ids)
        {
            var map = new Dictionary<long, string>();
            if (ids == null || ids.Count == 0)
            {
                return map;
            }

            try
            {
                var list = string.Join(",", ids);
                var rows = db.Database.SqlQuery<DashDesignPhotoRow>(
                    "SELECT SysObjectID AS DesignId, AttPhoto AS AttPhoto FROM dbo.TSql_Design_V2 WHERE SysObjectID IN (" + list + ")").ToList();
                foreach (var row in rows)
                {
                    map[row.DesignId] = row.AttPhoto;
                }
            }
            catch
            {
                return map;
            }

            return map;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RenameDashboardDesign(long designId, string name)
        {
            var label = (name ?? "").Trim();
            if (label.Length == 0)
            {
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_Val_LabelRequired });
            }
            if (label.Length > 500)
            {
                label = label.Substring(0, 500);
            }

            var entity = db.TSql_Design_V2.FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
            if (entity == null)
            {
                return Json(new { ok = false, message = Jobside.DashDesigns_Card_RenameFailed });
            }

            entity.AttLabel = label;
            entity.AttChange = DateTime.Now;
            entity.LinModifiedBy = IntranetAuditHelper.ResolveCurrentUserId(User);
            entity.SysUpdateNumber += 1;
            db.SaveChanges();
            return Json(new { ok = true, message = Jobside.DashDesigns_Card_RenameSuccess, name = label });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteDashboardDesign(long designId)
        {
            var entity = db.TSql_Design_V2.FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
            if (entity == null)
            {
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_DeleteFailed });
            }

            entity.AttIsDeleted = true;
            entity.AttChange = DateTime.Now;
            entity.LinModifiedBy = IntranetAuditHelper.ResolveCurrentUserId(User);
            entity.SysUpdateNumber += 1;
            db.SaveChanges();
            return Json(new { ok = true, message = Jobside.OfferWorkspace_Designs_DeleteSuccess });
        }

        private Dictionary<string, string> ResolveDesignCreatorNames(IEnumerable<string> userIds)
        {
            var ids = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (ids.Count == 0)
            {
                return map;
            }

            var employees = db.TSql_Employee.AsNoTracking()
                .Where(e => !e.AttIsDeleted && ids.Contains(e.LinAspNetUsert))
                .Select(e => new { e.LinAspNetUsert, e.AttName, e.AttSurname })
                .ToList();
            foreach (var emp in employees)
            {
                var label = ((emp.AttName ?? "") + " " + (emp.AttSurname ?? "")).Trim();
                if (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(emp.LinAspNetUsert) && !map.ContainsKey(emp.LinAspNetUsert))
                {
                    map[emp.LinAspNetUsert] = label;
                }
            }

            var missing = ids.Where(id => !map.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                var users = db.AspNetUsers.AsNoTracking()
                    .Where(u => missing.Contains(u.Id))
                    .Select(u => new { u.Id, u.UserName })
                    .ToList();
                foreach (var user in users)
                {
                    if (!string.IsNullOrWhiteSpace(user.UserName) && !map.ContainsKey(user.Id))
                    {
                        map[user.Id] = user.UserName;
                    }
                }
            }

            return map;
        }

        private string BuildDesignRowPhoto(string photoRaw, string designName, string viewerUrl)
        {
            var path = (photoRaw ?? "").Trim().Replace('\\', '/');
            var photoUrl = path.StartsWith("~/Content/images/desing/", StringComparison.OrdinalIgnoreCase)
                ? Url.Content(path)
                : "";
            var href = HttpUtility.HtmlAttributeEncode(viewerUrl);
            var alt = HttpUtility.HtmlAttributeEncode(designName ?? "");
            var inner = string.IsNullOrEmpty(photoUrl)
                ? "<span class=\"dash-design-row-photo-empty\" aria-hidden=\"true\"></span>"
                : "<img src=\"" + HttpUtility.HtmlAttributeEncode(photoUrl) + "\" alt=\"" + alt + "\" width=\"100\" height=\"76\" />";
            return "<a class=\"dash-design-row-photo\" href=\"" + href + "\">" + inner + "</a>";
        }

        private string BuildDesignPhotoCard(DashDesignCardInput card)
        {
            var name = card.DesignName ?? "";
            var path = (card.PhotoRaw ?? "").Trim().Replace('\\', '/');
            var photoUrl = path.StartsWith("~/Content/images/desing/", StringComparison.OrdinalIgnoreCase)
                ? Url.Content(path)
                : "";
            var href = HttpUtility.HtmlAttributeEncode(card.ViewerUrl);
            var encName = HttpUtility.HtmlEncode(name);
            var encNameAttr = HttpUtility.HtmlAttributeEncode(name);
            var picture = string.IsNullOrEmpty(photoUrl)
                ? "<span class=\"dash-design-photo-empty\" aria-hidden=\"true\"><i class=\"icon-base ri ri-image-line\"></i></span>"
                : "<img src=\"" + HttpUtility.HtmlAttributeEncode(photoUrl) + "\" alt=\"" + encNameAttr + "\" />";
            var tipOpen = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Col_Open);
            var tipEdit = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Card_EditName);
            var tipDelete = HttpUtility.HtmlAttributeEncode(Jobside.OfferWorkspace_Designs_DeleteTooltip);
            var jobText = card.JobNumber ?? "";
            if (!string.IsNullOrWhiteSpace(card.JobDescription) &&
                !string.Equals(card.JobDescription, jobText, StringComparison.Ordinal))
            {
                jobText = string.IsNullOrWhiteSpace(jobText)
                    ? card.JobDescription
                    : jobText + " · " + card.JobDescription;
            }

            var sb = new StringBuilder();
            sb.Append("<article class=\"dash-design-card\" data-design-id=\"" + card.DesignId + "\" data-name=\"" + encNameAttr + "\">");
            sb.Append("<a class=\"dash-design-card-photo\" href=\"" + href + "\">" + picture + "</a>");
            sb.Append("<div class=\"dash-design-card-body\">");
            sb.Append("<h3 class=\"dash-design-card-title\">" + encName + "</h3>");
            sb.Append("<input type=\"text\" class=\"form-control form-control-sm dash-design-card-name-input d-none\" maxlength=\"500\" value=\"" + encNameAttr + "\" />");
            sb.Append("<div class=\"dash-design-card-offer\">" + (card.OfferNumberHtml ?? "") +
                      "<span>" + HttpUtility.HtmlEncode(card.OfferName ?? "") + "</span></div>");
            sb.Append("<dl class=\"dash-design-card-meta\">");
            AppendCardMeta(sb, Jobside.Offers_Col_Client, card.ClientName);
            AppendCardMeta(sb, Jobside.DashDesigns_Col_JobNumber, jobText);
            AppendCardMeta(sb, Jobside.DashDesigns_Card_CreatedBy, card.CreatedBy, "dash-design-card-by");
            AppendCardMeta(sb, Jobside.DashDesigns_Card_Modified, card.Changed.ToString("dd/MM/yyyy HH:mm"));
            sb.Append("</dl>");
            sb.Append("<div class=\"dash-design-card-actions\">");
            sb.Append("<a class=\"btn btn-sm btn-primary dash-design-card-open\" href=\"" + href + "\" title=\"" + tipOpen + "\">" +
                      "<i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i><span>" +
                      HttpUtility.HtmlEncode(Jobside.DashDesigns_Col_Open) + "</span></a>");
            sb.Append("<button type=\"button\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill dash-design-card-edit\" title=\"" +
                      tipEdit + "\" aria-label=\"" + tipEdit + "\"><i class=\"icon-base ri ri-pencil-line\" aria-hidden=\"true\"></i></button>");
            sb.Append("<button type=\"button\" class=\"btn btn-sm btn-icon btn-text-danger rounded-pill dash-design-card-delete\" title=\"" +
                      tipDelete + "\" aria-label=\"" + tipDelete + "\"><i class=\"icon-base ri ri-delete-bin-line\" aria-hidden=\"true\"></i></button>");
            sb.Append("</div></div></article>");
            return sb.ToString();
        }

        private static void AppendCardMeta(StringBuilder sb, string label, string value, string valueClass = null)
        {
            var text = string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
            var cls = string.IsNullOrEmpty(valueClass) ? "" : " class=\"" + valueClass + "\"";
            sb.Append("<div><dt>" + HttpUtility.HtmlEncode(label) + "</dt><dd" + cls + ">" + HttpUtility.HtmlEncode(text) + "</dd></div>");
        }

        private string BuildOfferDesignTree(List<DashOfferTreeDesign> designs, string expandTip, string collapseTip)
        {
            if (designs == null || designs.Count == 0)
            {
                return "";
            }

            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            if (culture == null || string.IsNullOrEmpty(culture.Name))
            {
                culture = new CultureInfo("es-ES");
            }

            var sb = new StringBuilder();
            sb.Append("<div class=\"dash-client-tree\">");
            var groups = designs
                .GroupBy(d => new { d.AttCreated.Year, d.AttCreated.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month);
            foreach (var g in groups)
            {
                var label = culture.TextInfo.ToTitleCase(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", culture));
                sb.Append("<div class=\"dash-client-tree-node\">");
                sb.Append("<div class=\"dash-client-tree-monthline\">");
                sb.Append(
                    "<button type=\"button\" class=\"btn btn-sm btn-icon dash-tree-toggle dash-client-tree-month\" aria-expanded=\"false\" title=\"" +
                    expandTip + "\" aria-label=\"" + expandTip + "\" data-expand=\"" + expandTip +
                    "\" data-collapse=\"" + collapseTip + "\"><span class=\"dash-tree-sign\" aria-hidden=\"true\">+</span></button>");
                sb.Append("<span class=\"dash-client-tree-label\">" + HttpUtility.HtmlEncode(label) + "</span>");
                sb.Append("<span class=\"dash-client-tree-count\">" + g.Count() + "</span>");
                sb.Append("</div>");
                sb.Append("<div class=\"dash-client-tree-designs d-none\">");
                AppendTreeHead(sb, Jobside.DashDesigns_Col_DesignName, Jobside.Offers_Col_Client, Jobside.DashDesigns_Col_Created);
                sb.Append("<div class=\"dash-client-tree-leaves\">");
                foreach (var d in g.OrderByDescending(x => x.AttCreated).ThenBy(x => x.AttLabel))
                {
                    AppendTreeDesignRow(sb, d.OfferId, d.DesignId, d.AttThumbnail, d.AttLabel, d.ClientName, d.AttCreated);
                }
                sb.Append("</div></div></div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Pestaña Clientes: una fila por cliente, con el número de diseños.
        /// El árbol (mes y año, y debajo los diseños) va en la propiedad tree.
        /// </summary>
        [HttpPost]
        public JsonResult ListDashboardClientDesigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var clients = db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete);
                var totalCount = clients.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    clients = clients.Where(c =>
                        (c.TextLabel ?? "").Contains(value) ||
                        db.TSql_Offers.Any(o =>
                            !o.Is_Delete &&
                            o.LinkClient_V2 == c.IdObject &&
                            (((o.AddOfferNumber ?? "").Contains(value)) ||
                             db.TSql_Design_V2.Any(d =>
                                 !d.AttIsDeleted &&
                                 d.LinkOffers == o.IdObject &&
                                 (d.AttLabel ?? "").Contains(value)))));
                }

                var filteredCount = clients.Count();

                var q = clients.Select(c => new
                {
                    ClientId = c.IdObject,
                    ClientName = c.TextLabel ?? "",
                    DesignCount = db.TSql_Design_V2.Count(d =>
                        !d.AttIsDeleted &&
                        db.TSql_Offers.Any(o =>
                            !o.Is_Delete &&
                            o.IdObject == d.LinkOffers &&
                            o.LinkClient_V2 == c.IdObject)),
                    LastMove = db.TSql_Design_V2
                        .Where(d => !d.AttIsDeleted && db.TSql_Offers.Any(o =>
                            !o.Is_Delete &&
                            o.IdObject == d.LinkOffers &&
                            o.LinkClient_V2 == c.IdObject))
                        .Max(d => (DateTime?)d.AttChange)
                });

                var orderBy = "";
                if (requestModel.Columns != null)
                {
                    foreach (var column in requestModel.Columns.GetSortedColumns())
                    {
                        string orderColumn;
                        switch ((column.Data ?? "").ToLowerInvariant())
                        {
                            case "clientname":
                                orderColumn = "ClientName";
                                break;
                            case "designcount":
                                orderColumn = "DesignCount";
                                break;
                            default:
                                continue;
                        }
                        if (orderBy.Length > 0)
                        {
                            orderBy += ", ";
                        }
                        orderBy += orderColumn +
                            (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    }
                }

                var page = q.OrderBy(string.IsNullOrEmpty(orderBy)
                    ? "LastMove desc, ClientId desc"
                    : orderBy)
                    .ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

                var clientIds = rows.Where(p => p.DesignCount > 0).Select(p => p.ClientId).ToList();
                var designsByClient = clientIds.Count == 0
                    ? new Dictionary<long, List<DashClientTreeDesign>>()
                    : (from d in db.TSql_Design_V2.AsNoTracking()
                       join o in db.TSql_Offers.AsNoTracking() on d.LinkOffers equals o.IdObject
                       join s in db.TSql_OfferState.AsNoTracking() on o.LinkOfferState equals s.IdObject into sg
                       from s in sg.DefaultIfEmpty()
                       where !d.AttIsDeleted && !o.Is_Delete && clientIds.Contains(o.LinkClient_V2)
                       select new DashClientTreeDesign
                       {
                           ClientId = o.LinkClient_V2,
                           OfferId = o.IdObject,
                           OfferNumber = o.AddOfferNumber ?? "",
                           OfferStateColor = s == null ? null : s.AddColor,
                           DesignId = d.SysObjectID,
                           AttLabel = d.AttLabel ?? "",
                           AttCreated = d.AttCreated,
                           AttThumbnail = d.AttThumbnail
                       })
                        .ToList()
                        .GroupBy(d => d.ClientId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                var ttNewFmt = Jobside.DashDesigns_Tip_NewClientFmt;
                var expandTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tree_Expand);
                var collapseTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tree_Collapse);
                var data = rows.Select(p =>
                {
                    var ttNew = HttpUtility.HtmlAttributeEncode(ttNewFmt.Replace("{0}", p.ClientName));
                    var newDesign =
                        "<button type=\"button\" class=\"btn btn-sm btn-icon dash-new-design dash-new-client-design\" data-client-id=\"" +
                        p.ClientId + "\" data-client-name=\"" + HttpUtility.HtmlAttributeEncode(p.ClientName) +
                        "\" title=\"" + ttNew + "\" aria-label=\"" + ttNew + "\"><span class=\"dash-tree-sign dash-new-design-sign\" aria-hidden=\"true\">+</span></button>";
                    List<DashClientTreeDesign> designs;
                    designsByClient.TryGetValue(p.ClientId, out designs);
                    var toggle = "";
                    if (p.DesignCount > 0)
                    {
                        toggle =
                            "<button type=\"button\" class=\"btn btn-sm btn-icon dash-tree-toggle dash-client-tree-root\" aria-expanded=\"false\" title=\"" +
                            expandTip + "\" aria-label=\"" + expandTip + "\" data-expand=\"" + expandTip +
                            "\" data-collapse=\"" + collapseTip + "\"><span class=\"dash-tree-sign\" aria-hidden=\"true\">+</span></button>";
                    }
                    return new
                    {
                        treeToggle = toggle,
                        ClientName = HttpUtility.HtmlEncode(p.ClientName),
                        DesignCount = p.DesignCount,
                        actions = newDesign,
                        tree = BuildClientDesignTree(designs, expandTip, collapseTip)
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private static void AppendTreeHead(StringBuilder sb, string col1, string col2, string col3)
        {
            sb.Append("<div class=\"dash-client-tree-head\">");
            sb.Append("<span>" + HttpUtility.HtmlEncode(col1) + "</span>");
            sb.Append("<span>" + HttpUtility.HtmlEncode(col2) + "</span>");
            sb.Append("<span>" + HttpUtility.HtmlEncode(col3) + "</span>");
            sb.Append("<span class=\"dash-client-tree-eye\">" + HttpUtility.HtmlEncode(Jobside.DashDesigns_Col_Open) + "</span>");
            sb.Append("</div>");
        }

        private void AppendTreeDesignRow(
            StringBuilder sb,
            long offerId,
            long designId,
            string thumbnail,
            string label,
            string middle,
            DateTime created,
            bool middleIsHtml = false)
        {
            var stlPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(thumbnail);
            var url = OfferDesignWorkspaceHelper.BuildViewerUrl(Url, offerId, designId, stlPath);
            var encTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tip_OpenDesign);
            var middleHtml = middleIsHtml ? (middle ?? "") : HttpUtility.HtmlEncode(middle);
            sb.Append("<div class=\"dash-client-tree-design\">");
            sb.Append("<span class=\"dash-client-tree-name\">" + HttpUtility.HtmlEncode(label) + "</span>");
            sb.Append("<span class=\"dash-client-tree-offer\">" + middleHtml + "</span>");
            sb.Append("<span class=\"dash-client-tree-date\">" + created.ToString("dd/MM/yyyy HH:mm") + "</span>");
            sb.Append(
                "<a href=\"" + HttpUtility.HtmlAttributeEncode(url) +
                "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill dash-client-tree-eye\" title=\"" + encTip +
                "\" aria-label=\"" + encTip +
                "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>");
            sb.Append("</div>");
        }

        private string BuildClientDesignTree(List<DashClientTreeDesign> designs, string expandTip, string collapseTip)
        {
            if (designs == null || designs.Count == 0)
            {
                return "";
            }

            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            if (culture == null || string.IsNullOrEmpty(culture.Name))
            {
                culture = new CultureInfo("es-ES");
            }

            var sb = new StringBuilder();
            sb.Append("<div class=\"dash-client-tree\">");
            var groups = designs
                .GroupBy(d => new { d.AttCreated.Year, d.AttCreated.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month);
            foreach (var g in groups)
            {
                var label = culture.TextInfo.ToTitleCase(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", culture));
                sb.Append("<div class=\"dash-client-tree-node\">");
                sb.Append("<div class=\"dash-client-tree-monthline\">");
                sb.Append(
                    "<button type=\"button\" class=\"btn btn-sm btn-icon dash-tree-toggle dash-client-tree-month\" aria-expanded=\"false\" title=\"" +
                    expandTip + "\" aria-label=\"" + expandTip + "\" data-expand=\"" + expandTip +
                    "\" data-collapse=\"" + collapseTip + "\"><span class=\"dash-tree-sign\" aria-hidden=\"true\">+</span></button>");
                sb.Append("<span class=\"dash-client-tree-label\">" + HttpUtility.HtmlEncode(label) + "</span>");
                sb.Append("<span class=\"dash-client-tree-count\">" + g.Count() + "</span>");
                sb.Append("</div>");
                sb.Append("<div class=\"dash-client-tree-designs d-none\">");
                AppendTreeHead(sb, Jobside.DashDesigns_Col_DesignName, Jobside.Offers_Col_Number, Jobside.DashDesigns_Col_Created);
                sb.Append("<div class=\"dash-client-tree-leaves\">");
                foreach (var d in g.OrderByDescending(x => x.AttCreated).ThenBy(x => x.AttLabel))
                {
                    AppendTreeDesignRow(
                        sb, d.OfferId, d.DesignId, d.AttThumbnail, d.AttLabel,
                        OfferDisplayHelper.BuildOfferNumberCellHtml(d.OfferNumber, d.OfferStateColor),
                        d.AttCreated, true);
                }
                sb.Append("</div></div></div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Ofertas para el alta de diseño desde la pestaña Ofertas.
        /// </summary>
        [HttpGet]
        public JsonResult DashboardOffersForDesign()
        {
            var rows = (from o in db.TSql_Offers.AsNoTracking()
                        where !o.Is_Delete
                        join c in db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete)
                            on o.LinkClient_V2 equals c.IdObject into cg
                        from c in cg.DefaultIfEmpty()
                        orderby o.AddOfferNumber
                        select new
                        {
                            id = o.IdObject,
                            jobsideId = o.LinkJobside,
                            number = o.AddOfferNumber ?? "",
                            client = c == null ? "" : (c.TextLabel ?? "")
                        }).ToList();

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Clientes para el alta de diseño desde la pestaña Clientes.
        /// </summary>
        [HttpGet]
        public JsonResult DashboardClientsForDesign()
        {
            var rows = db.TSql_Client_V2.AsNoTracking()
                .Where(c => !c.Is_Delete)
                .OrderBy(c => c.TextLabel)
                .Select(c => new
                {
                    id = c.IdObject,
                    name = c.TextLabel ?? ""
                })
                .ToList();

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Crea un diseño nuevo con el nombre indicado y lo adjunta solo al cliente.
        /// La oferta no se pide: si la fila ya la trae, se usa esa; si no, la última del cliente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveDashboardClientDesign(long clientId, long? offerId, string attLabel)
        {
            var client = db.TSql_Client_V2.AsNoTracking()
                .FirstOrDefault(c => c.IdObject == clientId && !c.Is_Delete);
            if (client == null)
            {
                return Json(new { ok = false, message = Jobside.DashDesigns_Val_ClientNotFound });
            }

            TSql_Offers offer;
            if (offerId.HasValue && offerId.Value > 0)
            {
                var picked = offerId.Value;
                offer = db.TSql_Offers.AsNoTracking()
                    .FirstOrDefault(o => o.IdObject == picked && !o.Is_Delete && o.LinkClient_V2 == clientId);
            }
            else
            {
                offer = db.TSql_Offers.AsNoTracking()
                    .Where(o => !o.Is_Delete && o.LinkClient_V2 == clientId)
                    .OrderByDescending(o => o.IdObject)
                    .FirstOrDefault();
            }

            if (offer == null)
            {
                return Json(new { ok = false, message = Jobside.DashDesigns_Val_NoOffer });
            }

            var label = (attLabel ?? "").Trim();
            if (string.IsNullOrEmpty(label))
            {
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_Val_LabelRequired });
            }
            if (label.Length > 500)
            {
                label = label.Substring(0, 500);
            }

            var uid = IntranetAuditHelper.ResolveCurrentUserId(User) ?? "system";
            if (uid.Length > 128)
            {
                uid = uid.Substring(0, 128);
            }

            var now = DateTime.Now;
            var entity = new TSql_Design_V2
            {
                AttLabel = label,
                AttDescription = null,
                AttCenterX = 0d,
                AttCenterY = 0d,
                AttCreated = now,
                AttChange = now,
                AttIsDeleted = false,
                AttThumbnail = null,
                AttActiveCameraType = 0L,
                LinCreatedBy = uid,
                LinModifiedBy = uid,
                SysUpdateNumber = 0L,
                ItIsShared = false,
                ItIsSharedMyGrup = false,
                IsRenderAt60 = false,
                LinkOffers = offer.IdObject
            };

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.TSql_Design_V2.Add(entity);
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand(
                        @"INSERT INTO dbo.TSql_DesgloseOfertaCliente
                            (TextLabel, LinkClient_V2, LinkOffers, LinkDesign_V2,
                             Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy,
                             AddDateMade, AddLastDateChange, Ntimeschanged)
                          VALUES (@p0, @p1, @p2, @p3, 0, 1, @p4, NULL, @p5, NULL, 0)",
                        label,
                        clientId,
                        offer.IdObject,
                        entity.SysObjectID,
                        uid,
                        now);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_SaveFailed });
                }
            }

            return Json(new
            {
                ok = true,
                message = Jobside.DashDesigns_SaveSuccessFmt,
                designId = entity.SysObjectID
            });
        }

        /// <summary>
        /// Obras activas para el alta de oferta desde el panel de inicio.
        /// </summary>
        [HttpGet]
        public JsonResult DashboardJobsForOffer()
        {
            var rows = db.TSql_Jobside.AsNoTracking()
                .Where(j => !j.Is_Delete)
                .OrderBy(j => j.AddNJobside)
                .ThenBy(j => j.TextLabel)
                .Select(j => new
                {
                    id = j.IdObject,
                    code = j.AddNJobside ?? "",
                    name = j.TextLabel ?? ""
                })
                .ToList();

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListDashboardClients([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var rows = db.TSql_Client_V2.AsNoTracking()
                    .Where(c => !c.Is_Delete && c.LinkMadeBy == userId)
                    .OrderByDescending(c => c.AddDateMade)
                    .ThenByDescending(c => c.IdObject)
                    .Take(5)
                    .Select(c => new
                    {
                        c.IdObject,
                        TextCode = c.TextCode ?? "",
                        TextLabel = c.TextLabel ?? ""
                    })
                    .ToList();
                var filteredCount = rows.Count;

                var tt = HttpUtility.HtmlAttributeEncode(ClientV2.List_LinkOpenTooltip);
                var data = rows.Select(p =>
                {
                    var url = Url.Action("Details", "ClientV2", new { id = p.IdObject });
                    var encUrl = HttpUtility.HtmlAttributeEncode(url);
                    var btn =
                        "<a href=\"" + encUrl + "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + tt +
                        "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>";
                    return new
                    {
                        TextCode = HttpUtility.HtmlEncode(p.TextCode),
                        TextLabel = HttpUtility.HtmlEncode(p.TextLabel),
                        actions = btn
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, filteredCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private sealed class DashDesignCardInput
        {
            public long DesignId { get; set; }
            public string PhotoRaw { get; set; }
            public string DesignName { get; set; }
            public string OfferName { get; set; }
            public string OfferNumberHtml { get; set; }
            public string ClientName { get; set; }
            public string JobNumber { get; set; }
            public string JobDescription { get; set; }
            public string CreatedBy { get; set; }
            public DateTime Created { get; set; }
            public DateTime Changed { get; set; }
            public string ViewerUrl { get; set; }
        }

        private sealed class DashDesignPhotoRow
        {
            public long DesignId { get; set; }
            public string AttPhoto { get; set; }
        }

        private sealed class DashOfferTreeDesign
        {
            public long OfferId { get; set; }
            public long DesignId { get; set; }
            public string AttLabel { get; set; }
            public string ClientName { get; set; }
            public DateTime AttCreated { get; set; }
            public string AttThumbnail { get; set; }
        }

        private sealed class DashClientTreeDesign
        {
            public long ClientId { get; set; }
            public long OfferId { get; set; }
            public string OfferNumber { get; set; }
            public string OfferStateColor { get; set; }
            public long DesignId { get; set; }
            public string AttLabel { get; set; }
            public DateTime AttCreated { get; set; }
            public string AttThumbnail { get; set; }
        }
    }
}
