using DAL;
using Microsoft.AspNet.Identity;
using Desing.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace Desing.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// Nombre de la cookie persistente que guarda el ID de la plantilla
        /// del ultimo usuario que se logueo en el navegador. Se usa para
        /// pintar el Login con el color/logo correcto antes de autenticar.
        /// </summary>
        public const string PlantillaCookieName = "tandem_plantilla";

        private ConexionData _db;

        protected ConexionData db
        {
            get
            {
                if (_db == null)
                {
                    _db = new ConexionData();
                }
                return _db;
            }
        }

        /// <summary>Contexto EF para helpers de vista (p. ej. textos <see cref="DAL.TSql_UiTranslation"/>).</summary>
        public ConexionData ConexionData => db;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Plantilla por defecto del sitio (color + logo + favicon) - fallback si no hay usuario.
            ViewBag.PlantillaColor = "#349d7d";
            ViewBag.PlantillaLogo = "/Content/images/Login/at.png";
            ViewBag.PlantillaFavicon = "/assets/client/images/Default/Ico/at.ico";
            ViewBag.PlantillaBrandText = "T Desing.net";
            ViewBag.PlantillaBrandTextColor = "";
            ViewBag.PlantillaBrandAccentColor = "#f29100";

            // Disponibilizar avatar, userName y plantilla en todas las vistas (navbar Materio).
            try
            {
                TSql_Plantilla plantilla = null;

                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var idUser = User.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(idUser))
                    {
                        var employee = db.TSql_Employee.FirstOrDefault(n => n.LinAspNetUsert == idUser);
                        if (employee != null)
                        {
                            ViewBag.avatar = employee.AttPhotoMenu;
                            ViewBag.userName = (employee.AttName + " " + employee.AttSurname).Trim();
                            var company = db.TSql_Company.FirstOrDefault(c =>
                                c.SysObjectID == employee.LinCompany && !c.BitIsDeleted);
                            if (company != null)
                            {
                                if (company.LinPlantilla.HasValue)
                                {
                                    plantilla = db.TSql_Plantilla.FirstOrDefault(p =>
                                        p.SysObjectID == company.LinPlantilla.Value && !p.AttIsDeleted);
                                }

                                if (company.LinkLanguage.HasValue)
                                {
                                    var langRow = db.TSql_language.AsNoTracking().FirstOrDefault(l =>
                                        l.IdObject == company.LinkLanguage.Value && !l.Is_Delete && l.Is_Active);
                                    if (langRow != null && !string.IsNullOrWhiteSpace(langRow.TextCode))
                                    {
                                        var code = langRow.TextCode.Trim();
                                        HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageId] = langRow.IdObject;
                                        HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageCode] = code;
                                        HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageLocked] = true;
                                        LanguageUiHelper.WriteLanguageCookies(Response, code);
                                        LanguageUiHelper.ApplyCultureExplicit(code);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Anonimo: leer plantilla preferida desde cookie (ultimo login en este navegador).
                    long? cookiePlantillaId = ReadPlantillaCookie();
                    if (cookiePlantillaId.HasValue)
                    {
                        plantilla = db.TSql_Plantilla
                                      .FirstOrDefault(p => p.SysObjectID == cookiePlantillaId.Value && !p.AttIsDeleted);
                    }
                }

                // Fallback: plantilla marcada como por defecto.
                if (plantilla == null)
                {
                    plantilla = db.TSql_Plantilla
                                  .FirstOrDefault(p => p.AttIsDefault && !p.AttIsDeleted);
                }

                if (plantilla != null)
                {
                    if (!string.IsNullOrWhiteSpace(plantilla.AttColor))
                        ViewBag.PlantillaColor = plantilla.AttColor;
                    if (!string.IsNullOrWhiteSpace(plantilla.AttLogo))
                        ViewBag.PlantillaLogo = plantilla.AttLogo;
                    if (!string.IsNullOrWhiteSpace(plantilla.AttFavicon))
                        ViewBag.PlantillaFavicon = plantilla.AttFavicon;
                    ViewBag.PlantillaBrandText = string.IsNullOrWhiteSpace(plantilla.AttBrandText)
                        ? "T Desing.net"
                        : plantilla.AttBrandText.Trim();
                    ViewBag.PlantillaBrandTextColor = plantilla.AttBrandTextColor != null
                        ? plantilla.AttBrandTextColor.Trim()
                        : "";
                    ViewBag.PlantillaBrandAccentColor = string.IsNullOrWhiteSpace(plantilla.AttBrandAccentColor)
                        ? "#f29100"
                        : plantilla.AttBrandAccentColor.Trim();
                }
            }
            catch
            {
                // Si falla la consulta, simplemente no se establecen los ViewBag.
            }

            ViewBag.TandemUiCultureCode = LanguageUiHelper.ReadResolvedUiCultureCode(Request);
            ViewBag.TandemLanguageIdObject = LanguageUiHelper.TryResolveLanguageId(db, Request);
            ViewBag.TandemCompanyLanguageLocked =
                HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageLocked] as bool? == true;
        }

        /// <summary>
        /// Lee el ID de plantilla guardado en la cookie persistente, si existe.
        /// </summary>
        protected long? ReadPlantillaCookie()
        {
            try
            {
                var cookie = Request != null ? Request.Cookies[PlantillaCookieName] : null;
                long id;
                if (cookie != null && long.TryParse(cookie.Value, out id) && id > 0)
                    return id;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Guarda en cookie persistente (1 año) el ID de plantilla del usuario que acaba de loguearse.
        /// Si plantillaId es null, se intenta usar la plantilla por defecto.
        /// </summary>
        protected void WritePlantillaCookie(long? plantillaId)
        {
            try
            {
                if (!plantillaId.HasValue)
                {
                    plantillaId = db.TSql_Plantilla
                                    .Where(p => p.AttIsDefault && !p.AttIsDeleted)
                                    .Select(p => (long?)p.SysObjectID)
                                    .FirstOrDefault();
                }
                if (!plantillaId.HasValue) return;

                var cookie = new HttpCookie(PlantillaCookieName, plantillaId.Value.ToString())
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Path = "/"
                };
                Response.Cookies.Set(cookie);
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}