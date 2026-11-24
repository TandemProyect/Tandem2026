using System;

namespace Desing.Repositories.RepositoryAtk60
{
    /// <summary>
    /// Herraje de esquina regulable (gancho 1920811 + placa 10443020)
    /// en la cara exterior del run regulado (roja si Universal_X):
    /// Placa en la cara exterior del U (120 mm). Gancho −20 mm along.
    /// Cara 120 mm: bajar a 40 mm sacaba el gancho; se mete al revés.
    /// Rotación no se toca. No pilares.
    /// </summary>
    public static class Atk60AdjustableCornerHardware
    {
        /// <summary>Cara de trabajo del marco (120 mm desde el fenólico). Placa.</summary>
        public const double FrameFaceMm = 120d;
        /// <summary>0: misma cara que la placa. Reducir faceGap sacaba el gancho.</summary>
        public const double HookInwardMm = 0d;
        public const double HookFaceMm = FrameFaceMm - HookInwardMm;
        /// <summary>Mitad del ancho del marco, desde la intersección de caras (placa).</summary>
        public const double HoleAlongMm = 60d;
        /// <summary>Gancho −20 mm along: el +20 desplazaba más afuera del agujero.</summary>
        public const double HookAlongMm = -20d;

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

        /// <summary>
        /// Del canto exterior hacia el cuerpo del 0,75 (agujero del perfil).
        /// </summary>
        public static void ThroughIntoProfile(bool atEnd, double ux, double uz, out double x, out double z)
        {
            if (atEnd)
            {
                x = -ux;
                z = -uz;
                return;
            }

            x = ux;
            z = uz;
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
