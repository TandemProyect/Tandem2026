using System;

namespace Desing.Repositories.RepositoryAtk60
{
    // No entra en Design.csproj. csc /main:Desing.Repositories.RepositoryAtk60.Atk60AdjustableCornerHardwareVerify
    internal static class Atk60AdjustableCornerHardwareVerify
    {
        public static int Main()
        {
            var failed = 0;
            double ox;
            double oz;
            Atk60AdjustableCornerHardware.ExteriorNormal(false, 0d, 1d, out ox, out oz);
            failed += Check("exterior primaria +Z", oz, 1d);
            failed += Check("exterior primaria X nula", ox, 0d);
            Atk60AdjustableCornerHardware.ExteriorNormal(true, 0d, 1d, out ox, out oz);
            failed += Check("exterior opuesta si primaria al hueco", oz, -1d);

            // Dibujo de Juan: esquina 0,30; punta a cruce = 450 − espesor.
            failed += Check("muro 0,30 → 2º agujero", Atk60AdjustableCornerHardware.HoleNumber(150d), 2d);
            failed += Check("muro 0,25 → 3º agujero", Atk60AdjustableCornerHardware.HoleNumber(200d), 3d);
            failed += Check("muro 0,20 → 4º agujero", Atk60AdjustableCornerHardware.HoleNumber(250d), 4d);
            failed += Check("0,30 agujero a 100 de la punta", Atk60AdjustableCornerHardware.HoleFromTipMm(150d), 100d);
            failed += Check("borde: punta pegada al cruce → 1º", Atk60AdjustableCornerHardware.HoleNumber(20d), 1d);
            failed += Check("borde: 0,22 cae al agujero más cercano (3º)", Atk60AdjustableCornerHardware.HoleNumber(230d), 3d);
            failed += Check("gancho cara 20 (100 hacia el muro)", Atk60AdjustableCornerHardware.HookFaceMm, 20d);
            failed += Check("gancho X como estaba (80)", Atk60AdjustableCornerHardware.HookFromHoleTowardTipMm, 80d);

            // Wall1: cruce (6250,−500), U=+X, atEnd. Placa 50 mm hacia la punta.
            double x;
            double z;
            Atk60AdjustableCornerHardware.WorldMm(
                6250d, -500d, 0d, 1d, 1d, 0d,
                Atk60AdjustableCornerHardware.FrameFaceMm, 150d - 100d,
                out x, out z);
            failed += Check("placa 0,30 X = cruce + 50", x, 6300d);
            failed += Check("placa Z cara U", z, -380d);
            Atk60AdjustableCornerHardware.ExteriorNormal(true, 1d, 0d, out ox, out oz);
            Atk60AdjustableCornerHardware.WorldMm(
                0d, 0d, ox, oz, 0d, 1d,
                Atk60AdjustableCornerHardware.HookFaceMm, -30d,
                out x, out z);
            failed += Check("borde primaria al hueco X", x, -20d);
            failed += Check("borde primaria al hueco Z", z, -30d);

            failed += Check(
                "yaw normal roja +Z 180 planta",
                Atk60AdjustableCornerHardware.HookYawRad(0d, 1d),
                Math.PI * 0.5d);
            failed += Check(
                "yaw opuesta -Z es π (180º)",
                DeltaPi(
                    Atk60AdjustableCornerHardware.HookYawRad(0d, 1d),
                    Atk60AdjustableCornerHardware.HookYawRad(0d, -1d)),
                Math.PI);

            if (failed > 0)
            {
                Console.Error.WriteLine("FAIL " + failed);
                return 1;
            }

            Console.WriteLine("OK Atk60AdjustableCornerHardware");
            return 0;
        }

        private static double DeltaPi(double a, double b)
        {
            var d = Math.Abs(a - b);
            if (d > Math.PI)
            {
                d = (Math.PI * 2d) - d;
            }

            return d;
        }

        private static int CheckOutsideConcrete(string name, double alongNormal, double faceMm, double outward)
        {
            var side = (alongNormal - faceMm) * outward;
            if (side >= -0.01d)
            {
                Console.WriteLine("OK " + name);
                return 0;
            }

            Console.Error.WriteLine("FAIL " + name + " alongNormal=" + alongNormal + " face=" + faceMm);
            return 1;
        }

        private static int Check(string name, double actual, double expected)
        {
            if (Math.Abs(actual - expected) <= 0.01d)
            {
                Console.WriteLine("OK " + name);
                return 0;
            }

            Console.Error.WriteLine("FAIL " + name + " actual=" + actual + " expected=" + expected);
            return 1;
        }
    }
}
