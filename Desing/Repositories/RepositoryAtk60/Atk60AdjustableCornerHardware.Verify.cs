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

            double tx;
            double tz;
            Atk60AdjustableCornerHardware.ThroughIntoProfile(true, 1d, 0d, out tx, out tz);
            failed += Check("along al final hacia el 0,75", tx, -1d);
            Atk60AdjustableCornerHardware.ThroughIntoProfile(false, 1d, 0d, out tx, out tz);
            failed += Check("along al inicio hacia el 0,75", tx, 1d);

            failed += Check(
                "placa 60 mm mitad marco",
                Atk60AdjustableCornerHardware.HoleAlongMm,
                Atk60AdjustableCornerHardware.FrameFaceMm * 0.5d);
            failed += Check("gancho along -20 mm", Atk60AdjustableCornerHardware.HookAlongMm, -20d);
            failed += Check("gancho misma cara que placa", Atk60AdjustableCornerHardware.HookFaceMm, 120d);

            double x;
            double z;
            Atk60AdjustableCornerHardware.WorldMm(
                6250d, -500d, 0d, 1d, -1d, 0d,
                Atk60AdjustableCornerHardware.HookFaceMm,
                Atk60AdjustableCornerHardware.HookAlongMm,
                out x, out z);
            failed += Check("gancho X -20 mm along", x, 6270d);
            failed += Check("gancho Z cara U", z, -380d);
            failed += CheckOutsideConcrete("gancho no entra al muro", z, -500d, 1d);
            double plateX;
            double plateZ;
            Atk60AdjustableCornerHardware.WorldMm(
                6250d, -500d, 0d, 1d, -1d, 0d,
                Atk60AdjustableCornerHardware.FrameFaceMm,
                Atk60AdjustableCornerHardware.HoleAlongMm,
                out plateX, out plateZ);
            failed += Check("placa X 60 desde interseccion", plateX, 6190d);
            failed += Check("placa Z cara U", plateZ, -380d);
            failed += Check("gancho -20 mm along vs placa", x - plateX, 80d);

            Atk60AdjustableCornerHardware.WorldMm(
                400d, -500d, 0d, 1d, 1d, 0d,
                Atk60AdjustableCornerHardware.HookFaceMm,
                Atk60AdjustableCornerHardware.HookAlongMm,
                out x, out z);
            failed += Check("gancho inicio muro X", x, 380d);
            failed += Check("gancho inicio muro Z cara", z, -380d);
            Atk60AdjustableCornerHardware.ExteriorNormal(true, 1d, 0d, out ox, out oz);
            Atk60AdjustableCornerHardware.WorldMm(
                0d, 0d, ox, oz, 0d, 1d,
                Atk60AdjustableCornerHardware.HookFaceMm,
                Atk60AdjustableCornerHardware.HookAlongMm,
                out x, out z);
            failed += Check("borde primaria al hueco X", x, -120d);
            failed += Check("borde primaria al hueco Z", z, -20d);

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
