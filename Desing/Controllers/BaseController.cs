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
        public const string PlantillaColorCookieName = "tandem_plantilla_color";
        public const string PlantillaTextColorCookieName = "tandem_plantilla_text";
        public const string MaterioDefaultColor = "#7367F0";
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
            var pluginCadChrome = IsPluginCadRequest();
            ViewBag.PlantillaColor = pluginCadChrome ? MaterioDefaultColor : "#349d7d";
            ViewBag.PlantillaLogo = pluginCadChrome
                ? "/Content/images/tDesing/t-desing-net.png"
                : "/Content/images/Login/at.png";
            ViewBag.PlantillaFavicon = "/assets/client/images/Default/Ico/at.ico";
            ViewBag.PlantillaBrandText = "T Desing.net";
            ViewBag.PlantillaBrandTextColor = "";
            ViewBag.PlantillaBrandAccentColor = "#f29100";
            ApplyPlantillaCookiesWithoutDb();
            if (pluginCadChrome)
                ViewBag.SkipRemoteFonts = true;

            // Login y resto de Account no deben abrir ConexionData: el primer uso del EDMX
            // puede tardar varios minutos y deja la pantalla en blanco.
            var skipChromeDb = ShouldSkipChromeDb();

            try
            {
                PlantillaViewData plantilla = null;

                if (skipChromeDb)
                {
                    plantilla = TryPlantillaFromCacheOnly();
                }
                else if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
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
                if (plantilla == null && !skipChromeDb)
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
            if (!skipChromeDb)
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

        private bool ShouldSkipChromeDb()
        {
            var controller = RouteData != null ? RouteData.Values["controller"] as string : null;
            var action = RouteData != null ? RouteData.Values["action"] as string : null;
            if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase))
                return true;
            // Plugin CAD y APIs Desing_2 se llaman a menudo: no abrir plantilla/empleado/idioma.
            if (string.Equals(controller, "DesignToolsAutocad", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(controller, "Desing_2", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(action, "Viewer", StringComparison.OrdinalIgnoreCase))
                return true;
            try
            {
                if (Request != null && Request.IsAjaxRequest())
                    return true;
                var accept = Request != null ? Request.Headers["Accept"] : null;
                if (!string.IsNullOrEmpty(accept)
                    && accept.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch
            {
            }
            return false;
        }

        private PlantillaViewData TryPlantillaFromCacheOnly()
        {
            try
            {
                var cookieId = ReadPlantillaCookie();
                if (cookieId.HasValue)
                {
                    var cached = HttpRuntime.Cache[PlantillaCacheKeyPrefix + cookieId.Value] as PlantillaViewData;
                    if (cached != null)
                        return cached;
                }
                return HttpRuntime.Cache[PlantillaDefaultCacheKey] as PlantillaViewData;
            }
            catch
            {
                return null;
            }
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
                PlantillaViewData row = null;
                if (!plantillaId.HasValue)
                {
                    row = GetCachedDefaultPlantilla();
                    plantillaId = row != null ? (long?)row.Id : null;
                }
                if (!plantillaId.HasValue) return;
                if (row == null)
                    row = GetCachedPlantillaById(plantillaId.Value);

                var cookie = new HttpCookie(PlantillaCookieName, plantillaId.Value.ToString())
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Path = "/"
                };
                Response.Cookies.Set(cookie);
                WritePlantillaColorCookies(
                    row != null ? row.Color : null,
                    row != null ? row.BrandTextColor : null);
            }
            catch { }
        }

        protected void WritePlantillaChromeFromUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;
            try
            {
                var chrome = GetCachedUserChromeByAspNetUserId(userId);
                WritePlantillaCookie(chrome != null ? chrome.PlantillaId : null);
            }
            catch
            {
            }
        }

        private void WritePlantillaColorCookies(string color, string textColor)
        {
            WritePlainCookie(PlantillaColorCookieName, NormalizeHexCookie(color, MaterioDefaultColor));
            var text = NormalizeHexCookie(textColor, "");
            if (!string.IsNullOrWhiteSpace(text))
                WritePlainCookie(PlantillaTextColorCookieName, text);
        }

        private void ApplyPlantillaCookiesWithoutDb()
        {
            var color = ReadCookieValue(PlantillaColorCookieName);
            var text = ReadCookieValue(PlantillaTextColorCookieName);
            if (LooksLikeHex(color))
                ViewBag.PlantillaColor = color;
            if (LooksLikeHex(text))
                ViewBag.PlantillaBrandTextColor = text;
        }

        private bool IsPluginCadRequest()
        {
            try
            {
                var ru = Request != null
                    ? (Request["returnUrl"] ?? Request["ReturnUrl"] ?? "")
                    : "";
                if (ru.IndexOf("/DesignToolsAutocad/", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                var path = Request != null && Request.Path != null ? Request.Path : "";
                return path.IndexOf("/DesignToolsAutocad/", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private string ReadCookieValue(string name)
        {
            try
            {
                var cookie = Request != null ? Request.Cookies[name] : null;
                return cookie == null ? null : (cookie.Value ?? "").Trim();
            }
            catch
            {
                return null;
            }
        }

        private void WritePlainCookie(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value) || Response == null)
                return;
            var cookie = new HttpCookie(name, value.Trim())
            {
                Expires = DateTime.UtcNow.AddYears(1),
                HttpOnly = true,
                Path = "/"
            };
            Response.Cookies.Set(cookie);
        }

        private static string NormalizeHexCookie(string raw, string fallback)
        {
            var t = (raw ?? "").Trim();
            if (t.Length == 0)
                return fallback;
            if (t[0] != '#')
                t = "#" + t;
            return LooksLikeHex(t) ? t : fallback;
        }

        private static bool LooksLikeHex(string value)
        {
            var t = (value ?? "").Trim();
            if (t.Length != 4 && t.Length != 7)
                return false;
            if (t[0] != '#')
                return false;
            for (var i = 1; i < t.Length; i++)
            {
                var c = t[i];
                var hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex)
                    return false;
            }
            return true;
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