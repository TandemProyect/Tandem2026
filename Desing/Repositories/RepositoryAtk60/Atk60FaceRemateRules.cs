using System;

namespace Desing.Repositories.RepositoryAtk60
{
    /// <summary>
    /// Remate por cara: tope 0,15 m, taco en el extremo que toca la L, sin sumar caras.
    /// </summary>
    public static class Atk60FaceRemateRules
    {
        public const int MaxMm = 150;

        public static int CapMm(int remateMm)
        {
            if (remateMm > MaxMm)
            {
                return MaxMm;
            }

            if (remateMm < 0)
            {
                return 0;
            }

            return remateMm;
        }

        /// <summary>
        /// Origen del tablón sobre el eje: el extremo de la cara que se encuentra
        /// con la otra, hacia el tramo libre. No va en medio.
        /// </summary>
        public static double StartAlongMm(
            double axisLenMm,
            double along1,
            double along2,
            bool meetAt1,
            bool meetAt2,
            double remateMm)
        {
            double cornerAlong;
            double freeAlong;
            if (meetAt1 && !meetAt2)
            {
                cornerAlong = along1;
                freeAlong = along2;
            }
            else if (meetAt2 && !meetAt1)
            {
                cornerAlong = along2;
                freeAlong = along1;
            }
            else if (along1 >= along2)
            {
                cornerAlong = along1;
                freeAlong = along2;
            }
            else
            {
                cornerAlong = along2;
                freeAlong = along1;
            }

            var start = cornerAlong >= freeAlong - 1d
                ? cornerAlong - remateMm
                : cornerAlong;
            if (start < 0d)
            {
                start = 0d;
            }

            if (axisLenMm > 1d && start + remateMm > axisLenMm + 1d)
            {
                start = axisLenMm - remateMm;
                if (start < 0d)
                {
                    start = 0d;
                }
            }

            return start;
        }

        public static bool IndependentFaces(bool hasPos, bool hasNeg)
        {
            return hasPos && hasNeg;
        }

        public static bool AtStart(double alongMm, double remateMm, double wallRunMm)
        {
            if (remateMm <= 1d)
            {
                return false;
            }

            if (alongMm < 50d)
            {
                return true;
            }

            if (wallRunMm > 1d && alongMm + remateMm >= wallRunMm - 50d)
            {
                return false;
            }

            return wallRunMm > 1d && alongMm <= wallRunMm * 0.5d;
        }

        public static bool AtEnd(double alongMm, double remateMm, double wallRunMm)
        {
            if (remateMm <= 1d || wallRunMm <= 1d)
            {
                return false;
            }

            return !AtStart(alongMm, remateMm, wallRunMm)
                && alongMm + remateMm >= wallRunMm - 50d;
        }

        /// <summary>
        /// Interior: el taco va en el hueco de la L (antes de 0 o pasado el run),
        /// no encima del panel de esquina en pts[3]. Exterior: en el tramo.
        /// </summary>
        public static double PaintAlongMm(
            bool interior,
            double alongMm,
            double remateMm,
            double runMm)
        {
            if (remateMm <= 1d)
            {
                return alongMm;
            }

            var atStart = AtStart(alongMm, remateMm, runMm);
            var atEnd = AtEnd(alongMm, remateMm, runMm);
            if (interior)
            {
                if (atStart)
                {
                    return -remateMm;
                }

                if (atEnd)
                {
                    return runMm;
                }

                return alongMm;
            }

            if (atStart)
            {
                return 0d;
            }

            if (atEnd)
            {
                return runMm - remateMm;
            }

            return alongMm;
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
