using System;
using System.Globalization;

namespace Desing.Helpers
{
    /// <summary>
    /// Generación del código interno de obra <c>AddNJobside</c> (sin entrada del usuario).
    /// </summary>
    public static class JobsideCodeHelper
    {
        /// <summary>
        /// Instante local usado para el prefijo mes/año del código.
        /// Por defecto: zona <strong>Europa/Madrid</strong> (Windows: <c>Romance Standard Time</c>).
        /// Si la zona no está disponible, se usa <see cref="DateTime.Now"/> del servidor
        /// (comportamiento solicitado como alternativa).
        /// </summary>
        public static DateTime GetSpainLocalNow()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.Now;
            }
            catch (InvalidTimeZoneException)
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Formato fijo acordado para <c>AddNJobside</c> (no confundir con DDMM+Id):
        /// <c>MM</c> (2) + <c>yy</c> año en 2 cifras + <c>IdObject</c> rellenado con ceros a la izquierda.
        /// Anchura mínima del bloque numérico: 5; si <c>IdObject</c> supera 99999 se usan las cifras necesarias.
        /// Ejemplo: <c>IdObject=5</c>, mes 01, año 2002 → <c>010200005</c> (MM=01, yy=02, Id=00005).
        /// </summary>
        public static string BuildAddNJobside(long idObject, DateTime spainLocalWhenSaved)
        {
            var mm = spainLocalWhenSaved.ToString("MM", CultureInfo.InvariantCulture);
            var yy = spainLocalWhenSaved.ToString("yy", CultureInfo.InvariantCulture);
            var idPart = idObject.ToString(CultureInfo.InvariantCulture);
            const int minLen = 5;
            if (idPart.Length < minLen)
            {
                idPart = idPart.PadLeft(minLen, '0');
            }

            return mm + yy + idPart;
        }
    }
}
