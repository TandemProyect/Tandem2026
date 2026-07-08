using DAL;
using Microsoft.AspNet.Identity;
using Desing.Helpers;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Caching;
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
        private const string PlantillaCacheKeyPrefix = "TandemPlantilla_";
        private const string PlantillaDefaultCacheKey = "TandemPlantilla_Default";
        private const string LanguageByIdCacheKeyPrefix = "TandemLanguage_ById_";
        private const string UserChromeCacheKeyPrefix = "TandemUserChrome_";
        private static readonly TimeSpan SharedLookupCacheDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan UserChromeCacheDuration = TimeSpan.FromMinutes(5);

        private ConexionData _db;

        private sealed class PlantillaViewData
        {
            public long Id { get; set; }
            public string Color { get; set; }
            public string Logo { get; set; }
            public string Favicon { get; set; }
            public string BrandText { get; set; }
            public string BrandTextColor { get; set; }
            public string BrandAccentColor { get; set; }
        }

        private sealed class LanguageViewData
        {
            public long Id { get; set; }
            public string TextCode { get; set; }
        }

        private sealed class UserChromeViewData
        {
            public string Avatar { get; set; }
            public string UserName { get; set; }
            public long? PlantillaId { get; set; }
            public long? LanguageId { get; set; }
        }

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
            var swAction = Stopwatch.StartNew();
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
                PlantillaViewData plantilla = null;

                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var idUser = User.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(idUser))
                    {
                        var userChrome = GetCachedUserChromeByAspNetUserId(idUser);
                        if (userChrome != null)
                        {
                            ViewBag.avatar = userChrome.Avatar;
                            ViewBag.userName = userChrome.UserName;
                            if (userChrome.PlantillaId.HasValue)
                            {
                                plantilla = GetCachedPlantillaById(userChrome.PlantillaId.Value);
                            }

                            if (userChrome.LanguageId.HasValue)
                            {
                                var langRow = GetCachedLanguageById(userChrome.LanguageId.Value);
                                if (langRow != null && !string.IsNullOrWhiteSpace(langRow.TextCode))
                                {
                                    var code = langRow.TextCode.Trim();
                                    HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageId] = langRow.Id;
                                    HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageCode] = code;
                                    HttpContext.Items[LanguageUiHelper.ItemKeyCompanyLanguageLocked] = true;
                                    LanguageUiHelper.WriteLanguageCookies(Response, code);
                                    LanguageUiHelper.ApplyCultureExplicit(code);
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
                        plantilla = GetCachedPlantillaById(cookiePlantillaId.Value);
                    }
                }

                // Fallback: plantilla marcada como por defecto.
                if (plantilla == null)
                {
                    plantilla = GetCachedDefaultPlantilla();
                }

                if (plantilla != null)
                {
                    if (!string.IsNullOrWhiteSpace(plantilla.Color))
                        ViewBag.PlantillaColor = plantilla.Color;
                    if (!string.IsNullOrWhiteSpace(plantilla.Logo))
                        ViewBag.PlantillaLogo = plantilla.Logo;
                    if (!string.IsNullOrWhiteSpace(plantilla.Favicon))
                        ViewBag.PlantillaFavicon = plantilla.Favicon;
                    ViewBag.PlantillaBrandText = string.IsNullOrWhiteSpace(plantilla.BrandText)
                        ? "T Desing.net"
                        : plantilla.BrandText.Trim();
                    ViewBag.PlantillaBrandTextColor = plantilla.BrandTextColor != null
                        ? plantilla.BrandTextColor.Trim()
                        : "";
                    ViewBag.PlantillaBrandAccentColor = string.IsNullOrWhiteSpace(plantilla.BrandAccentColor)
                        ? "#f29100"
                        : plantilla.BrandAccentColor.Trim();
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
            ViewBag.TandemReleaseNumber = ReleaseVersionHelper.CurrentReleaseNumber;
            swAction.Stop();
            TraceStartupTiming(
                "BaseController.OnActionExecuting " +
                (Request != null ? Request.RawUrl : ""),
                swAction.ElapsedMilliseconds);
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

        private PlantillaViewData GetCachedPlantillaById(long plantillaId)
        {
            if (plantillaId <= 0) return null;
            var key = PlantillaCacheKeyPrefix + plantillaId;
            var cached = HttpRuntime.Cache[key] as PlantillaViewData;
            if (cached != null) return cached;

            var plantilla = db.TSql_Plantilla
                .AsNoTracking()
                .Where(p => p.SysObjectID == plantillaId && !p.AttIsDeleted)
                .Select(p => new PlantillaViewData
                {
                    Id = p.SysObjectID,
                    Color = p.AttColor,
                    Logo = p.AttLogo,
                    Favicon = p.AttFavicon,
                    BrandText = p.AttBrandText,
                    BrandTextColor = p.AttBrandTextColor,
                    BrandAccentColor = p.AttBrandAccentColor
                })
                .FirstOrDefault();

            if (plantilla != null)
            {
                HttpRuntime.Cache.Insert(key, plantilla, null, DateTime.UtcNow.Add(SharedLookupCacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return plantilla;
        }

        private PlantillaViewData GetCachedDefaultPlantilla()
        {
            var cached = HttpRuntime.Cache[PlantillaDefaultCacheKey] as PlantillaViewData;
            if (cached != null) return cached;

            var plantilla = db.TSql_Plantilla
                .AsNoTracking()
                .Where(p => p.AttIsDefault && !p.AttIsDeleted)
                .Select(p => new PlantillaViewData
                {
                    Id = p.SysObjectID,
                    Color = p.AttColor,
                    Logo = p.AttLogo,
                    Favicon = p.AttFavicon,
                    BrandText = p.AttBrandText,
                    BrandTextColor = p.AttBrandTextColor,
                    BrandAccentColor = p.AttBrandAccentColor
                })
                .FirstOrDefault();

            if (plantilla != null)
            {
                HttpRuntime.Cache.Insert(PlantillaDefaultCacheKey, plantilla, null, DateTime.UtcNow.Add(SharedLookupCacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return plantilla;
        }

        private LanguageViewData GetCachedLanguageById(long languageId)
        {
            if (languageId <= 0) return null;
            var key = LanguageByIdCacheKeyPrefix + languageId;
            var cached = HttpRuntime.Cache[key] as LanguageViewData;
            if (cached != null) return cached;

            var lang = db.TSql_language
                .AsNoTracking()
                .Where(l => l.IdObject == languageId && !l.Is_Delete && l.Is_Active)
                .Select(l => new LanguageViewData
                {
                    Id = l.IdObject,
                    TextCode = l.TextCode
                })
                .FirstOrDefault();

            if (lang != null)
            {
                HttpRuntime.Cache.Insert(key, lang, null, DateTime.UtcNow.Add(SharedLookupCacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
            }
            return lang;
        }

        private UserChromeViewData GetCachedUserChromeByAspNetUserId(string aspNetUserId)
        {
            if (string.IsNullOrWhiteSpace(aspNetUserId)) return null;
            var key = UserChromeCacheKeyPrefix + aspNetUserId;
            var cached = HttpRuntime.Cache[key] as UserChromeViewData;
            if (cached != null) return cached;

            var employee = db.TSql_Employee
                .AsNoTracking()
                .Where(n => n.LinAspNetUsert == aspNetUserId)
                .Select(n => new
                {
                    n.AttPhotoMenu,
                    n.AttName,
                    n.AttSurname,
                    n.LinCompany
                })
                .FirstOrDefault();

            if (employee == null) return null;

            var userChrome = new UserChromeViewData
            {
                Avatar = employee.AttPhotoMenu,
                UserName = ((employee.AttName ?? "") + " " + (employee.AttSurname ?? "")).Trim()
            };

            var company = db.TSql_Company
                .AsNoTracking()
                .Where(c => c.SysObjectID == employee.LinCompany && !c.BitIsDeleted)
                .Select(c => new
                {
                    c.LinPlantilla,
                    c.LinkLanguage
                })
                .FirstOrDefault();

            if (company != null)
            {
                userChrome.PlantillaId = company.LinPlantilla;
                userChrome.LanguageId = company.LinkLanguage;
            }

            HttpRuntime.Cache.Insert(key, userChrome, null, DateTime.UtcNow.Add(UserChromeCacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
            return userChrome;
        }

        private static void TraceStartupTiming(string label, long elapsedMs)
        {
            if (!string.Equals(ConfigurationManager.AppSettings["TandemStartupTiming"], "true", StringComparison.OrdinalIgnoreCase))
                return;

            Debug.WriteLine("[TandemStartupTiming] " + label + " = " + elapsedMs + " ms");
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
                    var plantilla = GetCachedDefaultPlantilla();
                    plantillaId = plantilla != null ? (long?)plantilla.Id : null;
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