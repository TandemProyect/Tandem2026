using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Paginación server-side compatible con DataTables: cuando el usuario elige
    /// «Todas las filas», el cliente envía <c>length: -1</c>. No usar <c>Take(-1)</c>
    /// en LINQ/EF (no significa «todas» y puede fallar o comportarse mal).
    /// </summary>
    public static class DataTablesQueryExtensions
    {
        public static IQueryable<T> ApplyDataTablesPaging<T>(this IQueryable<T> source, int start, int length)
        {
            if (length < 0)
            {
                return start <= 0 ? source : source.Skip(start);
            }

            if (start < 0)
            {
                start = 0;
            }

            if (length == 0)
            {
                return source.Skip(start).Take(0);
            }

            return source.Skip(start).Take(length);
        }
    }
}
