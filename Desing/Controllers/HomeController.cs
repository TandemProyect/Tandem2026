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
                var rows = db.TSql_Offers.AsNoTracking()
                    .Where(o => !o.Is_Delete && o.LinkMadeBy == userId)
                    .OrderByDescending(o => o.AddDateMade)
                    .ThenByDescending(o => o.IdObject)
                    .Take(5)
                    .Select(o => new
                    {
                        o.IdObject,
                        AddOfferNumber = o.AddOfferNumber ?? "",
                        TextLabel = o.TextLabel ?? ""
                    })
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
                        AddOfferNumber = HttpUtility.HtmlEncode(p.AddOfferNumber),
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
                        select new
                        {
                            OfferId = o.IdObject,
                            JobsideId = o.LinkJobside,
                            OfferNumber = o.AddOfferNumber ?? "",
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
                        OfferNumber = HttpUtility.HtmlEncode(p.OfferNumber),
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
                       where !d.AttIsDeleted && !o.Is_Delete && clientIds.Contains(o.LinkClient_V2)
                       select new DashClientTreeDesign
                       {
                           ClientId = o.LinkClient_V2,
                           OfferId = o.IdObject,
                           OfferNumber = o.AddOfferNumber ?? "",
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
            DateTime created)
        {
            var stlPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(thumbnail);
            var url = OfferDesignWorkspaceHelper.BuildViewerUrl(Url, offerId, designId, stlPath);
            var encTip = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_Tip_OpenDesign);
            sb.Append("<div class=\"dash-client-tree-design\">");
            sb.Append("<span class=\"dash-client-tree-name\">" + HttpUtility.HtmlEncode(label) + "</span>");
            sb.Append("<span class=\"dash-client-tree-offer\">" + HttpUtility.HtmlEncode(middle) + "</span>");
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
                    AppendTreeDesignRow(sb, d.OfferId, d.DesignId, d.AttThumbnail, d.AttLabel, d.OfferNumber, d.AttCreated);
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
            public long DesignId { get; set; }
            public string AttLabel { get; set; }
            public DateTime AttCreated { get; set; }
            public string AttThumbnail { get; set; }
        }
    }
}
