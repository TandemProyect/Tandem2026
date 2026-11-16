using Desing.Resources;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>
    /// Clave de función adicional para una acción o controlador (p. ej. varias acciones
    /// que forman un mismo comando: <c>[RequireFunction("Employee.Create")]</c>).
    /// Las claves automáticas <c>Controlador</c> y <c>Controlador.Accion</c> se comprueban siempre.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequireFunctionAttribute : Attribute
    {
        public RequireFunctionAttribute(string code)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }

    /// <summary>
    /// Filtro global (ver <c>FilterConfig</c>): niega con 403 cualquier acción cuyo
    /// controlador, acción o <see cref="RequireFunctionAttribute"/> tenga una función
    /// activa que el usuario no tiene en ninguno de sus roles.
    /// </summary>
    public sealed class FunctionAuthorizeFilter : IAuthorizationFilter
    {
        /// <summary>Controladores que nunca se protegen (entrar / salir / idioma).</summary>
        private static readonly HashSet<string> ExemptControllers =
            new HashSet<string>(new[] { "Account", "UiLanguage" }, StringComparer.OrdinalIgnoreCase);

        public static IList<string> CodesFor(ActionDescriptor action)
        {
            var controller = action.ControllerDescriptor.ControllerName;
            var codes = new List<string> { controller, controller + "." + action.ActionName };
            codes.AddRange(action.ControllerDescriptor
                .GetCustomAttributes(typeof(RequireFunctionAttribute), true)
                .OfType<RequireFunctionAttribute>()
                .Select(a => a.Code));
            codes.AddRange(action
                .GetCustomAttributes(typeof(RequireFunctionAttribute), true)
                .OfType<RequireFunctionAttribute>()
                .Select(a => a.Code));
            return codes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null || filterContext.IsChildAction || filterContext.Result != null)
            {
                return;
            }

            var user = filterContext.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return;
            }

            var action = filterContext.ActionDescriptor;
            if (ExemptControllers.Contains(action.ControllerDescriptor.ControllerName))
            {
                return;
            }

            var userId = user.Identity.GetUserId();
            var denied = CodesFor(action).FirstOrDefault(c => !FunctionAccessService.Can(userId, c));
            if (denied == null)
            {
                return;
            }

            var http = filterContext.HttpContext;
            http.Response.StatusCode = 403;
            http.Response.TrySkipIisCustomErrors = true;

            var accept = http.Request.AcceptTypes ?? new string[0];
            var wantsJson = http.Request.IsAjaxRequest()
                || accept.Any(a => a != null && a.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
                || !string.Equals(http.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase);

            if (wantsJson)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { IsOk = false, Message = Security.AccessDenied_Message, Function = denied },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }

            var view = new ViewResult { ViewName = "~/Views/Shared/AccessDenied.cshtml" };
            view.ViewBag.DeniedFunction = denied;
            filterContext.Result = view;
        }
    }
}
