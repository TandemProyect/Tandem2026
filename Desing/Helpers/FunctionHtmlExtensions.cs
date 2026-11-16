using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>Comprobación de funciones desde las vistas (menú, botones, comandos).</summary>
    public static class FunctionHtmlExtensions
    {
        public static bool CanUse(this HtmlHelper html, string code)
        {
            return FunctionAccessService.Can(CurrentUserId(html), code);
        }

        /// <summary>Clave del controlador de una línea de menú (o la función explícita si se indica).</summary>
        public static bool CanUseMenu(this HtmlHelper html, string controller, string functionCode = null)
        {
            var userId = CurrentUserId(html);
            if (!string.IsNullOrWhiteSpace(functionCode) && !FunctionAccessService.Can(userId, functionCode))
            {
                return false;
            }

            return FunctionAccessService.Can(userId, controller)
                && FunctionAccessService.Can(userId, controller + ".Index");
        }

        /// <summary><c>window.TandemDeniedFunctions</c> para <c>Scripts/Intranet/tandem-functions.js</c>.</summary>
        public static MvcHtmlString DeniedFunctionsJson(this HtmlHelper html)
        {
            var user = html.ViewContext.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return MvcHtmlString.Create("[]");
            }

            var json = JsonConvert.SerializeObject(FunctionAccessService.GetDeniedCodes(CurrentUserId(html)))
                .Replace("</", "<\\/");
            return MvcHtmlString.Create(json);
        }

        private static string CurrentUserId(HtmlHelper html)
        {
            var user = html.ViewContext.HttpContext.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated
                ? user.Identity.GetUserId()
                : null;
        }
    }
}
