using System.Web.Mvc;

namespace Desing.Helpers
{
    /// <summary>
    /// Enlaces coherentes desde el espacio de oferta al visor <see cref="Desing.Controllers.Desing_2Controller.Viewer"/>.
    /// </summary>
    public static class OfferDesignWorkspaceHelper
    {
        /// <summary>
        /// Con STL de confianza (~/Files/… o ~/Content/DesignTools/…, extensión .stl) se pasa <paramref name="stlVirtualPath"/> y <c>autoLoad=1</c> para cargar al abrir.
        /// Sin STL — visor vacío al iniciar (solo contexto oferta/diseño).
        /// </summary>
        public static string BuildViewerUrl(
            UrlHelper url,
            long offerId,
            long designId,
            string stlVirtualPath)
        {
            var trusted = ApplicationStlUrlHelper.TryGetTrustedStlVirtualPath(stlVirtualPath);
            if (trusted != null)
            {
                return url.Action(
                    "Viewer",
                    "Desing_2",
                    new { stlUrl = trusted, offerId, designId, autoLoad = 1 });
            }

            return url.Action(
                "Viewer",
                "Desing_2",
                new { offerId, designId });
        }
    }
}
