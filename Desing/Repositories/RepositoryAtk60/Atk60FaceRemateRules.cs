using System;

namespace Desing.Repositories.RepositoryAtk60
{
    /// <summary>
    /// Remate del tramo junto a la L. Las dos caras comparten módulos, así que el
    /// remate del tramo es el mismo en las dos y va enfrentado, pegado a la esquina.
    /// La cara interior lleva además el tablón de la propia L (LCornerFormwork): no se suman.
    /// </summary>
    public static class Atk60FaceRemateRules
    {
        public const int MaxMm = 150;
        public const int CornerAtStart = 1;
        public const int CornerAtEnd = 2;

        /// <summary>
        /// Dónde va el remate del tramo si algún extremo acaba en L. Con L en los dos
        /// extremos va al inicio. Sin L, o con un remate fuera de tope, no aplica.
        /// </summary>
        public static bool CornerRemate(
            int cornerEnds,
            double remateMm,
            double runMm,
            out double leadMm,
            out double trailMm,
            out double alongMm)
        {
            leadMm = 0d;
            trailMm = 0d;
            alongMm = 0d;
            if (remateMm <= 1d || remateMm > MaxMm || runMm <= remateMm)
            {
                return false;
            }

            if ((cornerEnds & CornerAtStart) != 0)
            {
                leadMm = remateMm;
                return true;
            }

            if ((cornerEnds & CornerAtEnd) != 0)
            {
                trailMm = remateMm;
                alongMm = runMm - remateMm;
                return true;
            }

            return false;
        }

        public static bool IndependentFaces(bool hasPos, bool hasNeg)
        {
            return hasPos && hasNeg;
        }

        /// <summary>
        /// Origen de los módulos: el taco de la L no comparte along con el panel.
        /// Si la esquina está al final, el bloque se corre para dejar el trail.
        /// </summary>
        public static double ModuleCursorMm(
            double leadMm,
            double trailMm,
            double moduleSpanMm,
            double wallRunMm)
        {
            var cursor = leadMm;
            if (trailMm > 1d && wallRunMm > 1d)
            {
                var overflow = cursor + moduleSpanMm - (wallRunMm - trailMm);
                if (overflow > 1d)
                {
                    cursor -= overflow;
                }
            }

            return cursor;
        }
    }
}
