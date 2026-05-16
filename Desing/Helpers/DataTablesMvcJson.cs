using System.Collections;

namespace Desing.Helpers
{
    /// <summary>
    /// Respuesta JSON para DataTables 1.10+ server-side (clave camelCase exacta:
    /// draw, recordsTotal, recordsFiltered, data). Garantiza que JavaScriptSerializer
    /// emita esos nombres; un shape distinto hace que el cliente deje <c>recordsTotal</c>
    /// como undefined, <c>parseInt</c> devuelve NaN y el texto "filtrado de _MAX_"
    /// muestra NaN cuando el listado está vacío.
    /// </summary>
    public static class DataTablesMvcJson
    {
        public static object Create(int draw, IEnumerable data, int recordsFiltered, int recordsTotal)
        {
            return new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            };
        }
    }
}
