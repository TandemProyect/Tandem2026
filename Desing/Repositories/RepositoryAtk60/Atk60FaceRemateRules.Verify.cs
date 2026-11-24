using System;

namespace Desing.Repositories.RepositoryAtk60
{
    // No entra en Design.csproj. csc /main:Desing.Repositories.RepositoryAtk60.Atk60FaceRemateRulesVerify
    internal static class Atk60FaceRemateRulesVerify
    {
        public static int Main()
        {
            var failed = 0;
            failed += Check(
                "esquina al final 3,36 remate 60",
                Atk60FaceRemateRules.StartAlongMm(3360d, 0d, 3360d, false, true, 60d),
                3300d);
            failed += Check(
                "esquina al inicio",
                Atk60FaceRemateRules.StartAlongMm(3360d, 0d, 3360d, true, false, 60d),
                0d);
            failed += Check(
                "sin esquina no desplaza a medio (fallback extremo mayor)",
                Atk60FaceRemateRules.StartAlongMm(3360d, 0d, 3360d, false, false, 60d),
                3300d);
            failed += Check(
                "tope 0,15",
                Atk60FaceRemateRules.CapMm(210),
                150d);
            failed += Check(
                "caras independientes no se fusionan",
                Atk60FaceRemateRules.IndependentFaces(true, true) ? 1d : 0d,
                1d);
            failed += Check(
                "una sola cara no bloquea",
                Atk60FaceRemateRules.IndependentFaces(true, false) ? 1d : 0d,
                0d);
            failed += Check(
                "lead al inicio",
                Atk60FaceRemateRules.ModuleCursorMm(130d, 0d, 2780d, 2910d),
                130d);
            failed += Check(
                "trail al final no solapa",
                Atk60FaceRemateRules.ModuleCursorMm(0d, 130d, 2850d, 2910d),
                -70d);
            failed += Check(
                "AtEnd 2780/130 en 2910",
                Atk60FaceRemateRules.AtEnd(2780d, 130d, 2910d) ? 1d : 0d,
                1d);
            failed += Check(
                "AtStart 0/130",
                Atk60FaceRemateRules.AtStart(0d, 130d, 2910d) ? 1d : 0d,
                1d);
            failed += Check(
                "interior inicio no va en pts[3]",
                Atk60FaceRemateRules.PaintAlongMm(true, 0d, 130d, 2910d),
                -130d);
            failed += Check(
                "interior final en hueco L",
                Atk60FaceRemateRules.PaintAlongMm(true, 2780d, 130d, 2910d),
                2910d);
            failed += Check(
                "exterior inicio en tramo",
                Atk60FaceRemateRules.PaintAlongMm(false, 0d, 130d, 2910d),
                0d);
            failed += Check(
                "exterior final no solapa modulo",
                Atk60FaceRemateRules.PaintAlongMm(false, 2780d, 130d, 2910d),
                2780d);
            failed += Check(
                "3,29 interior no en rincón",
                Atk60FaceRemateRules.PaintAlongMm(true, 0d, 140d, 2640d) < -1d ? 1d : 0d,
                1d);

            if (failed > 0)
            {
                Console.Error.WriteLine("FAIL " + failed);
                return 1;
            }

            Console.WriteLine("OK Atk60FaceRemateRules");
            return 0;
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
