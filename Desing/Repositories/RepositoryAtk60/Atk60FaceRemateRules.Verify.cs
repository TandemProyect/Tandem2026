using System;

namespace Desing.Repositories.RepositoryAtk60
{
    // No entra en Design.csproj. csc /main:Desing.Repositories.RepositoryAtk60.Atk60FaceRemateRulesVerify
    internal static class Atk60FaceRemateRulesVerify
    {
        public static int Main()
        {
            var failed = 0;
            double lead;
            double trail;
            double along;

            // L al final (0,30/0,30): tramo 5050, remate 100 junto a la L en las dos caras.
            var ok = Atk60FaceRemateRules.CornerRemate(
                Atk60FaceRemateRules.CornerAtEnd, 100d, 5050d, out lead, out trail, out along);
            failed += Check("L al final aplica", ok ? 1d : 0d, 1d);
            failed += Check("L al final: along = run - remate", along, 4950d);
            failed += Check("L al final: trail", trail, 100d);
            failed += Check("L al final: módulos desde 0", Atk60FaceRemateRules.ModuleCursorMm(lead, trail, 4950d, 5050d), 0d);

            // L al inicio: remate en 0, módulos corridos.
            Atk60FaceRemateRules.CornerRemate(
                Atk60FaceRemateRules.CornerAtStart, 60d, 2910d, out lead, out trail, out along);
            failed += Check("L al inicio: along 0", along, 0d);
            failed += Check("L al inicio: módulos desde el remate", Atk60FaceRemateRules.ModuleCursorMm(lead, trail, 2850d, 2910d), 60d);

            // L en los dos extremos (U, rectángulo): al inicio, como sale hoy el rectángulo.
            Atk60FaceRemateRules.CornerRemate(
                Atk60FaceRemateRules.CornerAtStart | Atk60FaceRemateRules.CornerAtEnd,
                100d, 2800d, out lead, out trail, out along);
            failed += Check("dos L: al inicio", along, 0d);
            failed += Check("dos L: sin trail", trail, 0d);

            // Bordes: sin L, remate fuera de tope, remate 0, tramo más corto que el remate.
            failed += Check("sin L no aplica",
                Atk60FaceRemateRules.CornerRemate(0, 100d, 5050d, out lead, out trail, out along) ? 1d : 0d, 0d);
            failed += Check("remate > 0,15 no aplica",
                Atk60FaceRemateRules.CornerRemate(Atk60FaceRemateRules.CornerAtEnd, 210d, 2910d, out lead, out trail, out along) ? 1d : 0d, 0d);
            failed += Check("remate 0 no aplica",
                Atk60FaceRemateRules.CornerRemate(Atk60FaceRemateRules.CornerAtEnd, 0d, 2910d, out lead, out trail, out along) ? 1d : 0d, 0d);
            failed += Check("tramo <= remate no aplica",
                Atk60FaceRemateRules.CornerRemate(Atk60FaceRemateRules.CornerAtEnd, 100d, 100d, out lead, out trail, out along) ? 1d : 0d, 0d);

            failed += Check("caras independientes no se fusionan",
                Atk60FaceRemateRules.IndependentFaces(true, true) ? 1d : 0d, 1d);
            failed += Check("una sola cara no bloquea",
                Atk60FaceRemateRules.IndependentFaces(true, false) ? 1d : 0d, 0d);

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