using System;

namespace Desing.Repositories.RepositoryAtk60
{
    /// <summary>
    /// Herraje de esquina regulable (gancho 1920811 + placa 10443020).
    /// La unión va en el agujero del 0,75 más cercano a la mitad del marco
    /// del otro panel: cruce de caras exteriores + 60 mm hacia la punta.
    /// Agujeros cada 50 mm desde la punta (1º a 50). Con esquina 0,30:
    /// muro 0,30 → 2º, 0,25 → 3º, 0,20 → 4º. Rotación no se toca. No pilares.
    /// </summary>
    public static class Atk60AdjustableCornerHardware
    {
        /// <summary>Cara de trabajo del marco (120 mm desde el fenólico).</summary>
        public const double FrameFaceMm = 120d;
        /// <summary>
        /// Gancho 100 mm hacia el muro respecto a la placa (pilar 90 + 10 visor).
        /// Visor 25/11: 170 lo sacó; 70 no asomaba; 30 faltaban 10 mm.
        /// </summary>
        public const double HookFaceMm = FrameFaceMm - 100d;
        /// <summary>Mitad del marco 0,12 del otro panel, desde el cruce de caras.</summary>
        public const double FrameHalfMm = 60d;
        /// <summary>Paso de agujeros del perfil; el 1º está a un paso de la punta.</summary>
        public const double HolePitchMm = 50d;
        /// <summary>Gancho respecto a la placa, hacia la punta del 0,75.</summary>
        public const double HookFromHoleTowardTipMm = 80d;

        /// <summary>
        /// Distancia desde la punta del 0,75 al agujero de la unión.
        /// <paramref name="tipToCrossingMm"/>: de la punta al cruce de caras exteriores.
        /// </summary>
        public static double HoleFromTipMm(double tipToCrossingMm)
        {
            var n = Math.Round((tipToCrossingMm - FrameHalfMm) / HolePitchMm, MidpointRounding.AwayFromZero);
            if (n < 1d)
            {
                n = 1d;
            }

            return n * HolePitchMm;
        }

        /// <summary>Número de agujero (1 = el de la punta).</summary>
        public static int HoleNumber(double tipToCrossingMm)
        {
            return (int)Math.Round(HoleFromTipMm(tipToCrossingMm) / HolePitchMm);
        }

        /// <summary>
        /// Normal de la cara exterior de la L (hacia el trabajador).
        /// Si la primaria mira al hueco, la exterior es la opuesta.
        /// </summary>
        public static void ExteriorNormal(bool inside, double nx, double nz, out double ox, out double oz)
        {
            if (inside)
            {
                ox = -nx;
                oz = -nz;
                return;
            }

            ox = nx;
            oz = nz;
        }

        public static void WorldMm(
            double cornerX,
            double cornerZ,
            double faceOutX,
            double faceOutZ,
            double alongX,
            double alongZ,
            double faceGapMm,
            double alongDeltaMm,
            out double x,
            out double z)
        {
            x = cornerX
                + (faceOutX * faceGapMm)
                + (alongX * alongDeltaMm);
            z = cornerZ
                + (faceOutZ * faceGapMm)
                + (alongZ * alongDeltaMm);
        }

        /// <summary>
        /// Yaw mundo (eje Y vertical de la cara) tras Rx(270) mundo.
        /// +90° de la cara y +180° en planta. +Z → π/2.
        /// </summary>
        public static double HookYawRad(double axisX, double axisZ)
        {
            double yaw;
            if ((axisX * axisX) + (axisZ * axisZ) < 1e-12)
            {
                yaw = Math.PI;
            }
            else
            {
                yaw = Math.Atan2(-axisX, -axisZ);
            }

            yaw += Math.PI * 1.5d;
            if (yaw < 0d)
            {
                yaw += Math.PI * 2d;
            }
            else if (yaw >= Math.PI * 2d)
            {
                yaw -= Math.PI * 2d;
            }

            return yaw;
        }
    }
}
