using System.Web.Mvc;

namespace Desing
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Seguridad: toda la app requiere sesion autenticada por defecto.
            // Las acciones publicas (Login, webhook, etc.) deben usar [AllowAnonymous].
            filters.Add(new AuthorizeAttribute());
        }
    }
}
