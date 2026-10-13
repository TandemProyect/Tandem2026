using DAL;
using DataTables.Mvc;
using Desing.Helpers;
using Desing.Resources;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
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
                        join c in db.TSql_Client_V2.AsNoTracking().Where(c => !c.Is_Delete)
                            on o.LinkClient_V2 equals c.IdObject into cg
                        from c in cg.DefaultIfEmpty()
                        where !d.AttIsDeleted && !o.Is_Delete
                        select new { d, o, ClientName = c == null ? "" : (c.TextLabel ?? "") };

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(x =>
                        (x.d.AttLabel ?? "").Contains(value) ||
                        (x.o.AddOfferNumber ?? "").Contains(value) ||
                        (x.ClientName ?? "").Contains(value));
                }

                var filteredCount = q.Count();

                var page = q.Select(x => new
                {
                    x.d.SysObjectID,
                    AttLabel = x.d.AttLabel ?? "",
                    AttThumbnail = x.d.AttThumbnail,
                    OfferId = x.o.IdObject,
                    JobsideId = x.o.LinkJobside,
                    AddOfferNumber = x.o.AddOfferNumber ?? "",
                    ClientName = x.ClientName ?? "",
                    x.d.AttChange,
                    x.d.AttCreated
                });

                var orderBy = "";
                if (requestModel.Columns != null)
                {
                    foreach (var column in requestModel.Columns.GetSortedColumns())
                    {
                        string orderColumn;
                        switch ((column.Data ?? "").ToLowerInvariant())
                        {
                            case "addoffernumber":
                                orderColumn = "AddOfferNumber";
                                break;
                            case "clientname":
                                orderColumn = "ClientName";
                                break;
                            case "attlabel":
                                orderColumn = "AttLabel";
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
                page = page.OrderBy(string.IsNullOrEmpty(orderBy)
                    ? "AttChange desc, AttCreated desc, SysObjectID desc"
                    : orderBy);

                page = page.ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

                var tt = HttpUtility.HtmlAttributeEncode(Jobside.OfferWorkspace_Designs_Tooltip_Open3DViewer);
                var ttNew = HttpUtility.HtmlAttributeEncode(Jobside.OfferWorkspace_Designs_Btn_NewTooltip);
                var data = rows.Select(p =>
                {
                    var stlPath = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(p.AttThumbnail);
                    var url = OfferDesignWorkspaceHelper.BuildViewerUrl(Url, p.OfferId, p.SysObjectID, stlPath);
                    var encUrl = HttpUtility.HtmlAttributeEncode(url);
                    var btn =
                        "<a href=\"" + encUrl + "\" class=\"btn btn-sm btn-icon btn-text-secondary rounded-pill\" title=\"" + tt +
                        "\"><i class=\"icon-base ri ri-eye-line\" aria-hidden=\"true\"></i></a>";
                    var newLabel = HttpUtility.HtmlEncode(Jobside.OfferWorkspace_Designs_Btn_New);
                    var newDesign =
                        "<button type=\"button\" class=\"btn btn-primary dash-new-design text-nowrap\" data-offer-id=\"" +
                        p.OfferId + "\" data-jobside-id=\"" + p.JobsideId + "\" data-offer-number=\"" +
                        HttpUtility.HtmlAttributeEncode(p.AddOfferNumber) + "\" title=\"" + ttNew +
                        "\" aria-label=\"" + ttNew + "\">" + newLabel + "</button>";
                    return new
                    {
                        AddOfferNumber =
                            "<span class=\"d-inline-flex align-items-center\">" +
                            HttpUtility.HtmlEncode(p.AddOfferNumber) + newDesign + "</span>",
                        ClientName = HttpUtility.HtmlEncode(p.ClientName),
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

        /// <summary>
        /// Pestaña Clientes del panel de diseños: un cliente y, si la tiene, su oferta.
        /// </summary>
        [HttpPost]
        public JsonResult ListDashboardClientDesigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var q = from c in db.TSql_Client_V2.AsNoTracking()
                        where !c.Is_Delete
                        join o in db.TSql_Offers.AsNoTracking().Where(o => !o.Is_Delete)
                            on c.IdObject equals o.LinkClient_V2 into og
                        from o in og.DefaultIfEmpty()
                        select new
                        {
                            ClientId = c.IdObject,
                            ClientName = c.TextLabel ?? "",
                            OfferId = o == null ? 0L : o.IdObject,
                            JobsideId = o == null ? 0L : o.LinkJobside,
                            OfferNumber = o == null ? "" : (o.AddOfferNumber ?? "")
                        };

                var totalCount = q.Count();

                if (requestModel.Search != null && !string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    q = q.Where(x =>
                        (x.ClientName ?? "").Contains(value) ||
                        (x.OfferNumber ?? "").Contains(value));
                }

                var filteredCount = q.Count();

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
                            case "offernumber":
                                orderColumn = "OfferNumber";
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
                    ? "ClientName asc, OfferNumber asc, ClientId asc"
                    : orderBy)
                    .ApplyDataTablesPaging(requestModel.Start, requestModel.Length);
                var rows = page.ToList();

                var newLabel = HttpUtility.HtmlEncode(Jobside.OfferWorkspace_Designs_Btn_New);
                var ttNew = HttpUtility.HtmlAttributeEncode(Jobside.DashDesigns_RowTooltipClient);
                var data = rows.Select(p =>
                {
                    var newDesign =
                        "<button type=\"button\" class=\"btn btn-primary dash-new-client-design text-nowrap\" data-client-id=\"" +
                        p.ClientId + "\" data-offer-id=\"" + p.OfferId + "\" data-jobside-id=\"" + p.JobsideId +
                        "\" data-client-name=\"" + HttpUtility.HtmlAttributeEncode(p.ClientName) +
                        "\" data-offer-number=\"" + HttpUtility.HtmlAttributeEncode(p.OfferNumber) +
                        "\" title=\"" + ttNew + "\" aria-label=\"" + ttNew + "\">" + newLabel + "</button>";
                    return new
                    {
                        ClientName =
                            "<span class=\"d-inline-flex align-items-center\">" +
                            HttpUtility.HtmlEncode(p.ClientName) + newDesign + "</span>",
                        OfferNumber = HttpUtility.HtmlEncode(p.OfferNumber)
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
