using DAL;
using System;
using System.Globalization;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Genera <c>TSql_Offers.AddOfferNumber</c> con formato
    /// <c>{Company.AddLetter}-{Branch.AddLetter}-{Jobside.AddNJobside}-{NN}</c>,
    /// donde <c>NN</c> es el siguiente entero (01, 02, …) para el mismo triple
    /// letra empresa / le delegación / código obra entre ofertas no borradas.
    /// </summary>
    public static class OfferNumberHelper
    {
        /// <summary>Segmento sin espacios extremos; nunca null.</summary>
        public static string NormalizeSegment(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        /// <summary>Prefijo acabado en guión, antes del sufijo numérico.</summary>
        public static string BuildPrefix(string companyLetter, string branchLetter, string jobsideCode)
        {
            var c = NormalizeSegment(companyLetter);
            var b = NormalizeSegment(branchLetter);
            var j = NormalizeSegment(jobsideCode);
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-", c, b, j);
        }

        /// <summary>
        /// Siguiente número de oferta (dentro de una transacción con aislamiento
        /// elevado recomendado para reducir colisiones concurrentes).
        /// </summary>
        public static string AllocateNextOfferNumber(
            ConexionData db,
            string companyLetter,
            string branchLetter,
            string jobsideCode)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var c = NormalizeSegment(companyLetter);
            var b = NormalizeSegment(branchLetter);
            var j = NormalizeSegment(jobsideCode);
            var prefix = BuildPrefix(c, b, j);

            var candidates = (from o in db.TSql_Offers
                              where !o.Is_Delete
                              join js in db.TSql_Jobside on o.LinkJobside equals js.IdObject
                              where !js.Is_Delete
                              join br in db.TSql_Branch on js.LinBranch equals br.SysObjectID
                              join co in db.TSql_Company on br.LinCompany equals co.SysObjectID
                              /* EF no traduce NormalizeSegment; string.Trim() en propiedad sí se traduce (LTRIM/RTRIM). */
                              where (co.AddLetter ?? "").Trim() == c
                                    && (br.AddLetter ?? "").Trim() == b
                                    && (js.AddNJobside ?? "").Trim() == j
                              select o.AddOfferNumber).ToList();

            var max = 0;
            foreach (var raw in candidates)
            {
                var s = NormalizeSegment(raw);
                if (s.Length < prefix.Length || !s.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var tail = s.Substring(prefix.Length);
                if (tail.Length == 0)
                {
                    continue;
                }

                int n;
                if (int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out n)
                    && n > max)
                {
                    max = n;
                }
            }

            var next = max + 1;
            if (next < 1)
            {
                next = 1;
            }

            /* Dos cifras con cero a la izquierda; si se supera 99, el formato crece (100, …). */
            var suffix = next.ToString("00", CultureInfo.InvariantCulture);
            return prefix + suffix;
        }
    }
}
