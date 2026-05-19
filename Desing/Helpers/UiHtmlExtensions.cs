using System.Web;
using System.Web.Mvc;
using Desing.Controllers;

namespace Desing.Helpers
{
    /// <summary>
    /// Etiquetas desde <see cref="DAL.TSql_UiTranslation"/> con fallback al idioma por defecto y al literal.
    /// Convención de claves: <c>Modulo.NombreElemento</c> (ej. <c>UiTranslation.ExportHeading</c>), <see cref="DAL.TSql_UiTranslation.TextModule"/> opcional.
    /// </summary>
    public static class UiHtmlExtensions
    {
        public static MvcHtmlString Ui(this HtmlHelper html, string resourceKey, string fallbackSpanish, string module = "UiTranslation")
        {
            var c = html.ViewContext.Controller as BaseController;
            if (c == null || html.ViewContext.HttpContext == null)
                return MvcHtmlString.Create(HttpUtility.HtmlEncode(fallbackSpanish ?? string.Empty));

            var text = LanguageUiHelper.GetUiStringWithFallback(
                c.ConexionData,
                html.ViewContext.HttpContext.Request,
                resourceKey,
                fallbackSpanish,
                module);

            return MvcHtmlString.Create(HttpUtility.HtmlEncode(text ?? string.Empty));
        }
    }
}
