using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                var q = db.TSql_Jobside.AsNoTracking().Where(j => !j.Is_Delete);

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(j =>
                        (j.AddNJobside ?? "").Contains(value) ||
                        (j.TextLabel ?? "").Contains(value));
                }

                var filteredCount = q.Count();

                q = q.OrderByDescending(j => j.AddLastDateChange ?? j.AddDateMade)
                    .ThenByDescending(j => j.IdObject);

                var page = q.Select(j => new
                {
                    j.IdObject,
                    AddNJobside = j.AddNJobside ?? "",
                    TextLabel = j.TextLabel ?? ""
                });

                page = page.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

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

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
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
                var q = db.TSql_Offers.AsNoTracking().Where(o => !o.Is_Delete);

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(o =>
                        (o.AddOfferNumber ?? "").Contains(value) ||
                        (o.TextLabel ?? "").Contains(value));
                }

                var filteredCount = q.Count();

                /* Sin AddLastDateChange en entidad: último movimiento ≈ Ntimeschanged + AddDateMade */
                q = q.OrderByDescending(o => o.Ntimeschanged)
                    .ThenByDescending(o => o.AddDateMade)
                    .ThenByDescending(o => o.IdObject);

                var page = q.Select(o => new
                {
                    o.IdObject,
                    AddOfferNumber = o.AddOfferNumber ?? "",
                    TextLabel = o.TextLabel ?? ""
                });

                page = page.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

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

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ListDashboardDesigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var q = from d in db.TSql_Design_V2.AsNoTracking()
                        join o in db.TSql_Offers.AsNoTracking() on d.LinkOffers equals o.IdObject
                        where !d.AttIsDeleted && !o.Is_Delete
                        select new { d, o };

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(x =>
                        (x.d.AttLabel ?? "").Contains(value) ||
                        (x.o.AddOfferNumber ?? "").Contains(value));
                }

                var filteredCount = q.Count();

                q = q.OrderByDescending(x => x.d.AttChange)
                    .ThenByDescending(x => x.d.AttCreated)
                    .ThenByDescending(x => x.d.SysObjectID);

                var page = q.Select(x => new
                {
                    x.d.SysObjectID,
                    AttLabel = x.d.AttLabel ?? "",
                    AttThumbnail = x.d.AttThumbnail,
                    OfferId = x.o.IdObject,
                    AddOfferNumber = x.o.AddOfferNumber ?? ""
                });

                page = page.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

                var tt = HttpUtility.HtmlAttributeEncode(Jobside.OfferWorkspace_Designs_Tooltip_Open3DViewer);
                var data = rows.Select(p =>
                {
                    var stlPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(p.AttThumbnail);
                    var url = OfferDesignWorkspaceHelper.BuildViewerUrl(Url, p.OfferId, p.SysObjectID, stlPath);
                    var encUrl = HttpUtility.HtmlAttributeEncode(url);
                    var btn =
                        "<a href=\"" + encUrl + "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + tt +
                        "\"><i class=\"icon-base ri ri-shape-3-line\" aria-hidden=\"true\"></i></a>";
                    return new
                    {
                        AddOfferNumber = HttpUtility.HtmlEncode(p.AddOfferNumber),
                        AttLabel = HttpUtility.HtmlEncode(p.AttLabel),
                        actions = btn
                    };
                }).ToList();

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ListDashboardClients([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var q = db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete);

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(c =>
                        (c.TextCode ?? "").Contains(value) ||
                        (c.TextLabel ?? "").Contains(value));
                }

                var filteredCount = q.Count();

                q = q.OrderByDescending(c => c.AddLastDateChange ?? c.AddDateMade)
                    .ThenByDescending(c => c.IdObject);

                var page = q.Select(c => new
                {
                    c.IdObject,
                    TextCode = c.TextCode ?? "",
                    TextLabel = c.TextLabel ?? ""
                });

                page = page.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

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

                return Json(DataTablesMvcJson.Create(requestModel.Draw, data, filteredCount, totalCount));
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
    }
}
