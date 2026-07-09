using DAL;
using Desing.Helpers;
using Desing.Models.TandemXr;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// API para la app Unity Tandem XR (Quest 3S + tablet Android).
    /// Cliente pasivo: recibe manifest y monta STL en Unity.
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

                var manifest = new TandemXrDesignManifestDto
                {
                    DesignId = designId,
                    OfferId = offerId ?? design.LinkOffers,
                    TextLabel = design.AttLabel ?? ("Diseño " + designId),
                    ServerBaseUrl = baseUrl,
                    ThumbnailStlUrl = thumbUrl != null ? new Uri(new Uri(baseUrl), thumbUrl).AbsoluteUri : null,
                    Message = "v0 — miniatura; lista completa de instancias pendiente de conectar con generador ATK60."
                };

                return Json(new { exito = true, manifest }, JsonRequestBehavior.AllowGet);
        }
    }
}
