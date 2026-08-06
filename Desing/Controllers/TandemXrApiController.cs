using DAL;
using Desing.Helpers;
using Desing.Models.TandemXr;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// API para la app Unity Tandem XR (Quest 3S + tablet Android).
    /// Cliente pasivo: recibe manifest / jobs de envío y monta STL en Unity.
    /// </summary>
    public class TandemXrApiController : BaseController
    {
        /// <summary>
        /// Manifest de diseño para XR. v0: metadatos + STL miniatura; instancias completas en fases posteriores.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Manifest(long designId, long? offerId = null)
        {
            if (designId <= 0)
            {
                return Json(new { exito = false, mensaje = "designId inválido." }, JsonRequestBehavior.AllowGet);
            }

            var design = db.TSql_Design_V2
                    .AsNoTracking()
                    .FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);

            if (design == null)
            {
                return Json(new { exito = false, mensaje = "Diseño no encontrado." }, JsonRequestBehavior.AllowGet);
            }

            if (offerId.HasValue && design.LinkOffers != offerId.Value)
            {
                return Json(new { exito = false, mensaje = "El diseño no pertenece a la oferta indicada." }, JsonRequestBehavior.AllowGet);
            }

            var virt = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(design.AttThumbnail);
            var thumbUrl = virt != null ? Url.Content(virt) : null;
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);

            // camelCase: Unity JsonUtility no mapea PascalCase (TextLabel ≠ textLabel).
            var thumbAbsolute = thumbUrl != null
                ? new Uri(new Uri(baseUrl), thumbUrl).AbsoluteUri
                : null;

            return Json(new
            {
                exito = true,
                manifest = new
                {
                    designId = designId,
                    offerId = offerId ?? design.LinkOffers,
                    textLabel = design.AttLabel ?? ("Diseño " + designId),
                    serverBaseUrl = baseUrl,
                    thumbnailStlUrl = thumbAbsolute,
                    message = "v0 — miniatura; lista completa de instancias pendiente de conectar con generador ATK60."
                }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Lista dispositivos activos para el modal «Enviar a XR» del diseñador.
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult ListDevices()
        {
            if (!XrDeviceQueries.TableExists(db.Database))
            {
                return Json(new { exito = false, mensaje = "Faltan tablas XR. Ejecute los scripts create_TSql_XrDevice / XrPushJob." },
                    JsonRequestBehavior.AllowGet);
            }

            var devices = XrDeviceQueries.ListSelectable(db.Database)
                .Select(d => new
                {
                    id = d.IdObject,
                    textLabel = d.TextLabel,
                    textDeviceType = d.TextDeviceType,
                    isPaired = d.Is_Paired,
                    textPairingCode = d.TextPairingCode
                })
                .ToList();

            return Json(new { exito = true, devices }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Encola el diseño actual hacia un dispositivo XR concreto.
        /// </summary>
        [HttpPost]
        [Authorize]
        public ActionResult SendToDevice(long designId, long deviceId)
        {
            if (designId <= 0 || deviceId <= 0)
            {
                return Json(new { exito = false, mensaje = "Parámetros inválidos." });
            }

            if (!XrDeviceQueries.TableExists(db.Database) || !XrPushJobQueries.TableExists(db.Database))
            {
                return Json(new { exito = false, mensaje = "Faltan tablas XR en BD." });
            }

            var design = db.TSql_Design_V2
                .AsNoTracking()
                .FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
            if (design == null)
            {
                return Json(new { exito = false, mensaje = "Diseño no encontrado." });
            }

            var device = XrDeviceQueries.GetById(db.Database, deviceId);
            if (device == null || !device.Is_Active)
            {
                return Json(new { exito = false, mensaje = "Dispositivo no encontrado o inactivo." });
            }

            var label = "Diseño " + (design.AttLabel ?? designId.ToString()) + " → " + device.TextLabel;
            if (label.Length > 500) label = label.Substring(0, 500);

            var jobId = XrPushJobQueries.Insert(db.Database, new XrPushJobEntity
            {
                TextLabel = label,
                LinkXrDevice = deviceId,
                LinkDesign = designId,
                LinkOffer = design.LinkOffers,
                TextStatus = XrPushJobStatus.Pending,
                LinkMadeBy = User.Identity.GetUserId() ?? "system",
                AddDateMade = DateTime.Now
            });

            return Json(new
            {
                exito = true,
                mensaje = "Enviado a «" + device.TextLabel + "». El dispositivo lo cargará al consultar pendientes.",
                jobId,
                deviceLabel = device.TextLabel
            });
        }

        /// <summary>
        /// Unity: consulta el envío pendiente más antiguo para este código de emparejamiento.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Pending(string pairingCode)
        {
            if (string.IsNullOrWhiteSpace(pairingCode))
            {
                return Json(new { exito = false, mensaje = "pairingCode obligatorio." }, JsonRequestBehavior.AllowGet);
            }

            if (!XrDeviceQueries.TableExists(db.Database) || !XrPushJobQueries.TableExists(db.Database))
            {
                return Json(new { exito = false, mensaje = "Tablas XR no creadas." }, JsonRequestBehavior.AllowGet);
            }

            var device = XrDeviceQueries.GetByPairingCode(db.Database, pairingCode);
            if (device == null)
            {
                return Json(new { exito = false, mensaje = "Código de emparejamiento no reconocido." }, JsonRequestBehavior.AllowGet);
            }

            XrDeviceQueries.TouchSeen(db.Database, device.IdObject, "xr-device");

            var job = XrPushJobQueries.GetOldestPendingForDevice(db.Database, device.IdObject);
            if (job == null)
            {
                return Json(new { exito = true, hayPendiente = false, deviceLabel = device.TextLabel }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                exito = true,
                hayPendiente = true,
                deviceLabel = device.TextLabel,
                job = new
                {
                    jobId = job.IdObject,
                    designId = job.LinkDesign,
                    offerId = job.LinkOffer,
                    textLabel = job.TextLabel,
                    addDateMade = job.AddDateMade
                }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Unity: confirma que el envío se ha cargado (pasa a Delivered).
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AckPending(string pairingCode, long jobId)
        {
            if (string.IsNullOrWhiteSpace(pairingCode) || jobId <= 0)
            {
                return Json(new { exito = false, mensaje = "Parámetros inválidos." });
            }

            if (!XrDeviceQueries.TableExists(db.Database) || !XrPushJobQueries.TableExists(db.Database))
            {
                return Json(new { exito = false, mensaje = "Tablas XR no creadas." });
            }

            var device = XrDeviceQueries.GetByPairingCode(db.Database, pairingCode);
            if (device == null)
            {
                return Json(new { exito = false, mensaje = "Código no reconocido." });
            }

            var job = XrPushJobQueries.GetById(db.Database, jobId);
            if (job == null || job.LinkXrDevice != device.IdObject)
            {
                return Json(new { exito = false, mensaje = "Envío no encontrado para este dispositivo." });
            }

            if (!string.Equals(job.TextStatus, XrPushJobStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { exito = true, mensaje = "Ya estaba entregado." });
            }

            XrPushJobQueries.MarkDelivered(db.Database, jobId, "xr-device");
            XrDeviceQueries.TouchSeen(db.Database, device.IdObject, "xr-device");

            return Json(new { exito = true, mensaje = "Envío marcado como entregado." });
        }
    }
}
