using DAL;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Desing.Helpers
{
    /// <summary>
    /// Icono de recuento junto a «Abrir diseño», solo si el diseño tiene piezas guardadas.
    /// </summary>
    public static class DesignMaterialCountUi
    {
        public static HashSet<long> IdsWithSavedCount(ConexionData db, IEnumerable<long> designIds)
        {
            var ids = (designIds ?? Enumerable.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0 || db == null)
            {
                return new HashSet<long>();
            }

            var found = db.TSql_DesignWallArticle.AsNoTracking()
                .Where(a => !a.Is_Delete && a.Is_Active && ids.Contains(a.LinkDesign_V2))
                .Select(a => a.LinkDesign_V2)
                .Distinct()
                .ToList();
            return new HashSet<long>(found);
        }

        public static string Button(long designId, string designName, string tip, bool fontAwesome)
        {
            if (designId <= 0)
            {
                return "";
            }

            var name = HttpUtility.HtmlAttributeEncode(designName ?? "");
            var title = HttpUtility.HtmlAttributeEncode(tip ?? "");
            var icon = fontAwesome
                ? "<span class=\"fas fa-list\" aria-hidden=\"true\"></span>"
                : "<i class=\"icon-base ri ri-file-list-3-line\" aria-hidden=\"true\"></i>";
            var cls = fontAwesome
                ? "btn btn-info btn-xs js-design-material-count"
                : "btn btn-sm btn-icon btn-text-secondary rounded-pill js-design-material-count";
            return "<button type=\"button\" class=\"" + cls + "\" data-design-id=\"" + designId.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "\" data-design-name=\"" + name + "\" title=\"" + title + "\" aria-label=\"" + title + "\">" + icon + "</button>";
        }
    }
}
