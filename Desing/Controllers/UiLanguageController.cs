using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Desing.Helpers;
using Desing.Models;

namespace Desing.Controllers
{
    /// <summary>
    /// Selector de idioma UI (cookie tandem_lang + legacy tandem_ui_culture).
    /// </summary>
    [AllowAnonymous]
    public class UiLanguageController : BaseController
    {
        [ChildActionOnly]
        public ActionResult NavbarSwitcher()
        {
            var items = LanguageUiHelper.TryGetActiveLanguages(db) ?? new List<LanguageNavItem>();

            long? lockedId;
            string lockedCode;
            if (User.Identity.IsAuthenticated &&
                LanguageUiHelper.TryGetLockedCompanyUiLanguage(db, User, out lockedId, out lockedCode) &&
                lockedId.HasValue)
            {
                var row = db.TSql_language.Include(x => x.TSql_Countrys)
                    .FirstOrDefault(l => l.IdObject == lockedId.Value && !l.Is_Delete && l.Is_Active);
                if (row != null)
                {
                    ViewBag.TandemLanguageSwitcherLocked = true;
                    ViewBag.TandemLanguageCurrent = new LanguageNavItem
                    {
                        IdObject = row.IdObject,
                        TextCode = row.TextCode,
                        TextLabel = row.TextLabel,
                        TextFlagRaw = row.TSql_Countrys != null ? row.TSql_Countrys.TextFlag : null,
                        IsDefault = row.Is_Default
                    };
                    return PartialView("_NavbarLanguageSwitcher", items);
                }
            }

            ViewBag.TandemLanguageSwitcherLocked = false;

            if (items.Count == 0)
                return new EmptyResult();

            var currentCode = LanguageUiHelper.ReadResolvedUiCultureCode(Request).Trim();
            LanguageNavItem current = items.FirstOrDefault(i =>
                string.Equals((i.TextCode ?? "").Trim(), currentCode, System.StringComparison.OrdinalIgnoreCase))
                ?? items.FirstOrDefault(i => i.IsDefault)
                ?? items[0];

            ViewBag.TandemLanguageCurrent = current;

            return PartialView("_NavbarLanguageSwitcher", items);
        }

        /// <summary>
        /// Establece idioma UI (cookies) y redirige. GET para enlaces simples.
        /// </summary>
        public ActionResult SetCulture(string c, long? id, string returnUrl)
        {
            long? lockedId;
            string lockedCode;
            if (LanguageUiHelper.TryGetLockedCompanyUiLanguage(db, User, out lockedId, out lockedCode))
            {
                LanguageUiHelper.WriteLanguageCookies(Response, lockedCode);
                LanguageUiHelper.ApplyCultureExplicit(lockedCode);
                if (!Url.IsLocalUrl(returnUrl))
                    returnUrl = Url.Action("Index", "Home");
                return Redirect(returnUrl);
            }

            var code = ResolveActiveLanguageCode(c, id);
            LanguageUiHelper.WriteLanguageCookies(Response, code);

            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.Action("Index", "Home");

            return Redirect(returnUrl);
        }

        /// <summary>
        /// POST con anti-forgery (formulario navbar).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetCulturePost(string c, long? id, string returnUrl)
        {
            return SetCulture(c, id, returnUrl);
        }

        private string ResolveActiveLanguageCode(string textCode, long? idObject)
        {
            if (idObject.HasValue && idObject.Value > 0)
            {
                var row = db.TSql_language.FirstOrDefault(l =>
                    l.IdObject == idObject.Value && !l.Is_Delete && l.Is_Active);
                if (row != null && !string.IsNullOrWhiteSpace(row.TextCode))
                    return row.TextCode.Trim();
            }

            if (!string.IsNullOrWhiteSpace(textCode))
            {
                var t = textCode.Trim();
                if (db.TSql_language.Any(l =>
                        l.TextCode == t && !l.Is_Delete && l.Is_Active))
                    return t;
            }

            var fallback = LanguageUiHelper.TryGetDefaultLanguageTextCode(db);
            return string.IsNullOrWhiteSpace(fallback) ? "es" : fallback.Trim();
        }
    }
}
