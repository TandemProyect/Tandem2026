using DAL;

using Desing.Helpers;

using Desing.Models;
using Desing.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;



namespace Desing.Controllers

{

    /// <summary>

    /// Espacio de diseño «v2» (visor Three.js / STL). La edición de diseño creado desde la oferta puede redirigir aquí con el STL correspondiente.

    /// </summary>

    [Authorize]

    public class Desing_2Controller : BaseController

    {

        /// <summary>

        /// Visor STL (misma pieza DOM y script que Artículos maestros).

        /// Query opcional: <paramref name="stlUrl"/> (virtual ~/…), <paramref name="offerId"/>, <paramref name="designId"/>.

        /// Sin STL la escena arranca vacía; con <paramref name="autoLoad"/> = 1 y STL válido se carga al iniciar (enlaces desde la oferta).

        /// </summary>

        public ActionResult Viewer(string stlUrl, long? offerId, long? designId, int? autoLoad)

        {

            string resolvedContentUrl = null;

            if (!string.IsNullOrWhiteSpace(stlUrl))

            {

                var virt = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(stlUrl);

                if (virt == null)

                {

                    return HttpNotFound();

                }



                resolvedContentUrl = Url.Content(virt);

            }



            var auto = autoLoad == 1 && !string.IsNullOrEmpty(resolvedContentUrl);



            var statusFooter = auto

                ? Jobside.OfferWorkspace_Designs_ViewerAutoLoadPending

                : Jobside.OfferWorkspace_Designs_ViewerCanvasEmpty;



            var model = new Desing2ViewerPageModel

            {

                TextColor1Hex = "#efdf34",

                TextColor2Hex = "#a18c8c",

                InitialStlUrl = resolvedContentUrl,

                InitialStlLabel = offerId.HasValue

                    ? "Diseño — oferta " + offerId.Value.ToString(CultureInfo.InvariantCulture)

                    : "Planta 3D",

                AutoLoadInitialStl = auto,

                InitialStatusFooter = statusFooter,

                OfferId = offerId,

                DesignId = designId,

                BrandLogoUrl = ResolvePlantillaLogoUrl(Url, ViewBag.PlantillaLogo as string)

            };



            FillOfferDesignContext(model, offerId, designId);



            ViewBag.BodyHtmlClass = "desing2-stl-fullpage";



            return View(model);

        }



        /// <summary>
        /// Guarda el estado topológico de muros/polilíneas que prepara el visor Desing_2.
        /// </summary>
        [HttpPost]
        public JsonResult SaveWallConnections()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }
                string rawJson;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    return Json(new { Exito = false, Mensaje = "JSON vacío." });
                }

                var parsed = JToken.Parse(rawJson);
                var dir = Server.MapPath("~/IA/Communication");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var targetPath = Path.Combine(dir, "WallConnections.json");
                var formatted = JsonConvert.SerializeObject(parsed, Formatting.Indented);
                System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));

                return Json(new { Exito = true, Path = targetPath });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { Exito = false, Mensaje = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Guarda un diagnóstico ampliado de muros en una ruta local estable para análisis con agentes.
        /// </summary>
        [HttpPost]
        public JsonResult SaveWallDiagnostics()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }

                string rawJson;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    return Json(new { Exito = false, Mensaje = "JSON vacío." });
                }

                var parsed = JToken.Parse(rawJson);
                var dir = @"C:\temp";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var targetPath = Path.Combine(dir, "WallDiagnostics.json");
                var formatted = JsonConvert.SerializeObject(parsed, Formatting.Indented);
                System.IO.File.WriteAllText(targetPath, formatted, new UTF8Encoding(false));

                return Json(new { Exito = true, Path = targetPath });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { Exito = false, Mensaje = "JSON inválido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Primer endpoint de encofrado por sistema (ATK-60).
        /// Recibe lista de muros con atributos dinámicos para preparar la lógica de materiales.
        /// </summary>
        [HttpPost]
        [ActionName("GetWallsAtk-60")]
        public JsonResult GetWallsAtk60()
        {
            try
            {
                if (Request.InputStream.CanSeek)
                {
                    Request.InputStream.Position = 0;
                }

                var rawJson = string.Empty;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    rawJson = reader.ReadToEnd();
                }

                Desing2FormworkRequest payload;
                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    payload = new Desing2FormworkRequest
                    {
                        System = "Atk-60",
                        Walls = new List<Desing2FormworkWallDto>()
                    };
                }
                else
                {
                    var parsed = JToken.Parse(rawJson);
                    payload = parsed.ToObject<Desing2FormworkRequest>() ?? new Desing2FormworkRequest();
                    payload.System = string.IsNullOrWhiteSpace(payload.System) ? "Atk-60" : payload.System.Trim();
                    payload.Walls = payload.Walls ?? new List<Desing2FormworkWallDto>();
                }

                var wallCount = payload.Walls.Count;
                var attributeCount = payload.Walls.Sum(w => w.Attributes != null ? w.Attributes.Count : 0);

                return Json(new
                {
                    Exito = true,
                    Sistema = payload.System,
                    MurosRecibidos = wallCount,
                    AtributosRecibidos = attributeCount,
                    Mensaje = "Encofrar ATK-60: endpoint listo (fase 1)."
                });
            }
            catch (JsonReaderException ex)
            {
                return Json(new { Exito = false, Mensaje = "JSON invalido: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }



        private static string ResolvePlantillaLogoUrl(UrlHelper url, string plantillaLogoRaw)

        {

            var plantillaLogo = string.IsNullOrWhiteSpace(plantillaLogoRaw)

                ? "/Content/images/Login/at.png"

                : plantillaLogoRaw.Trim();

            if (plantillaLogo.StartsWith("http", StringComparison.OrdinalIgnoreCase)

                || plantillaLogo.StartsWith("//", StringComparison.Ordinal))

            {

                return plantillaLogo;

            }



            return url.Content("~" + (plantillaLogo.StartsWith("/") ? plantillaLogo : "/" + plantillaLogo));

        }



        /// <summary>

        /// Rellena <see cref="Desing2ViewerPageModel.ContextSubtitleLine"/> con obra / oferta / diseño si hay ids válidos en BD.

        /// </summary>

        private void FillOfferDesignContext(Desing2ViewerPageModel model, long? offerId, long? designId)

        {

            string jobsideCode = null;

            string offerNumber = null;

            string offerName = null;

            string designPart = null;
            if (offerId.HasValue)

            {
                var offer = db.TSql_Offers.AsNoTracking().FirstOrDefault(o => o.IdObject == offerId.Value && !o.Is_Delete);

                if (offer != null)

                {

                    offerNumber = offer.AddOfferNumber;

                    offerName = offer.TextLabel;

                    var js = db.TSql_Jobside.AsNoTracking().FirstOrDefault(j => j.IdObject == offer.LinkJobside && !j.Is_Delete);

                    if (js != null)

                    {

                        jobsideCode = js.AddNJobside;

                    }

                }

            }



            if (designId.HasValue)

            {

                if (offerId.HasValue)

                {

                    var d = db.TSql_Design_V2.AsNoTracking().FirstOrDefault(x =>

                        x.SysObjectID == designId.Value && x.LinkOffers == offerId.Value && !x.AttIsDeleted);

                    if (d != null && !string.IsNullOrWhiteSpace(d.AttLabel))

                    {

                        designPart = d.AttLabel.Trim() + " (#" +

                                     designId.Value.ToString(CultureInfo.InvariantCulture) + ")";

                    }

                }



                if (designPart == null)

                {

                    designPart = "#" + designId.Value.ToString(CultureInfo.InvariantCulture);

                }

            }



            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(jobsideCode))

            {

                parts.Add(jobsideCode.Trim());

            }



            if (!string.IsNullOrWhiteSpace(offerNumber))

            {

                parts.Add(offerNumber.Trim());

            }



            if (!string.IsNullOrWhiteSpace(offerName))

            {

                parts.Add(offerName.Trim());

            }



            if (!string.IsNullOrWhiteSpace(designPart))

            {

                parts.Add(designPart.Trim());

            }



            model.ContextSubtitleLine = parts.Count > 0 ? string.Join(" — ", parts) : null;

        }

    }

    public sealed class Desing2FormworkRequest
    {
        public string System { get; set; }
        public List<Desing2FormworkWallDto> Walls { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }

    public sealed class Desing2FormworkWallDto
    {
        public string Id { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Attributes { get; set; }
    }

}

