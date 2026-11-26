using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Desing.Repositories.RepositoryDesing2
{
    // No entra en Design.csproj.
    // csc /main:Desing.Repositories.RepositoryDesing2.DesignWallFaceBuilderVerify DesignWallFaceBuilder.cs DesignWallFaceBuilder.Verify.cs
    // Con "print" saca las caras en el mismo formato que buildDesignWallFaces (JS) para compararlas.
    internal static class DesignWallFaceBuilderVerify
    {
        private static DesignWallFaceAxis A(long id, double x1, double z1, double x2, double z2, double t)
        {
            return new DesignWallFaceAxis
            {
                Id = id,
                WallDbId = id,
                WallId = id.ToString(CultureInfo.InvariantCulture),
                P1X = x1,
                P1Z = z1,
                P2X = x2,
                P2Z = z2,
                ThicknessMm = t,
                HeightMm = 2700d
            };
        }

        private static List<KeyValuePair<string, List<DesignWallFaceAxis>>> Cases()
        {
            return new List<KeyValuePair<string, List<DesignWallFaceAxis>>>
            {
                Case("rect40020", A(1, 400, -650, 6100, -650, 300), A(4, 6100, -4350, 6100, -650, 300), A(7, 400, -4350, 6100, -4350, 300), A(10, 400, -4350, 400, -650, 300)),
                Case("l30009", A(1, 250, -15250, 2250, -15250, 220), A(4, 2250, -17250, 2250, -15250, 300)),
                Case("single", A(1, 0, 0, 3000, 0, 250)),
                Case("oblique", A(1, 0, 0, 3000, 2000, 300)),
                Case("t", A(1, 0, 0, 6000, 0, 300), A(2, 3000, 0, 3000, -4000, 200)),
                Case("twoGroups", A(5, 10000, 0, 13000, 0, 300), A(1, 0, 0, 3000, 0, 300), A(2, 3000, 0, 3000, 3000, 300)),
            };
        }

        private static KeyValuePair<string, List<DesignWallFaceAxis>> Case(string name, params DesignWallFaceAxis[] axes)
        {
            return new KeyValuePair<string, List<DesignWallFaceAxis>>(name, axes.ToList());
        }

        public static int Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "print")
            {
                foreach (var c in Cases())
                {
                    foreach (var f in DesignWallFaceBuilder.Build(c.Value))
                    {
                        Console.WriteLine(string.Join(" ", new[]
                        {
                            c.Key, f.Id.ToString(CultureInfo.InvariantCulture), f.IsInterior ? "interior" : "exterior", f.WallId, f.Orientation,
                            F(f.P1X), F(f.P1Z), F(f.P2X), F(f.P2Z), F(f.LengthMm)
                        }));
                    }
                }

                return 0;
            }

            var failed = 0;
            var rect = DesignWallFaceBuilder.Build(Cases()[0].Value);
            failed += Check("rect: 8 caras", rect.Count, 8);
            failed += Check("rect: F-1 exterior muro 7 (abajo)", rect[0].WallId == "7" && !rect[0].IsInterior ? 1 : 0, 1);
            failed += Check("rect: F-5 interior muro 7", rect[4].WallId == "7" && rect[4].IsInterior ? 1 : 0, 1);
            failed += Check("rect: exterior alargada 6000", rect[0].LengthMm, 6000);
            failed += Check("rect: interior recortada 5400", rect[4].LengthMm, 5400);
            failed += Check("rect: exterior F-1 mira a −Z", rect[0].NormalZ, -1);

            var frameExt = DesignWallFaceFrame.From(rect[0]);
            failed += Check("marco: muro horizontal 0°", frameExt.WallAngleDeg, 0);
            failed += Check("marco: exterior del muro de arriba (CAD) mira a 90°", frameExt.NormalDeg, 90);
            failed += Check("marco: desde fuera, P1 a la izquierda (X mayor)", frameExt.P1X, 6250);
            var frameInt = DesignWallFaceFrame.From(rect[4]);
            failed += Check("marco: interior mira a 270°", frameInt.NormalDeg, 270);
            failed += Check("marco: desde dentro, P1 a la izquierda (X menor)", frameInt.P1X, 550);
            double lx, depth;
            frameInt.Local(550 + 1000, -4200 + 30, out lx, out depth);
            failed += Check("local: 1000 desde P1", lx, 1000);
            failed += Check("local: 30 delante de la cara", depth, 30);
            failed += Check("marco: muro vertical 90°", DesignWallFaceFrame.From(rect[1]).WallAngleDeg, 90);

            // Pieza sobre la cara (F-1 exterior del muro 7: z = −4500, mira a −Z, P1 en X = 6250).
            double d;
            failed += Check("sobre la cara: centro", frameExt.Holds(3250, -4500, 300, out d) ? 1 : 0, 1);
            failed += Check("sobre la cara: puntal a 1 m", frameExt.Holds(3250, -5500, 300, out d) ? 1 : 0, 1);
            failed += Check("sobre la cara: tirante en el eje", frameExt.Holds(3250, -4350, 300, out d) ? 1 : 0, 1);
            failed += Check("borde: 5 m por delante no es de esta cara", frameExt.Holds(3250, -9500, 300, out d) ? 1 : 0, 0);
            failed += Check("borde: detrás del muro no", frameExt.Holds(3250, -4100, 300, out d) ? 1 : 0, 0);
            failed += Check("borde: fuera del largo de la cara no", frameExt.Holds(7000, -4500, 300, out d) ? 1 : 0, 0);

            var oblique = DesignWallFaceBuilder.Build(Cases()[3].Value);
            failed += Check("borde: oblicuo (baja en CAD) 146.31°", DesignWallFaceFrame.From(oblique[0]).WallAngleDeg, 146.31);
            failed += Check("borde: sentido contrario mismo ángulo", DesignWallFaceFrame.AxisAngleDeg(-3000, -2000), 146.31);
            failed += Check("borde: sube a la derecha en CAD 33.69°", DesignWallFaceFrame.AxisAngleDeg(3000, -2000), 33.69);
            failed += Check("borde: 180° es 0°", DesignWallFaceFrame.AxisAngleDeg(-1000, 0), 0);
            failed += Check("borde: −90° es 90°", DesignWallFaceFrame.AxisAngleDeg(0, -1000), 90);

            // Asignación de piezas al muro 7 (z = −4350; exterior en −4500, interior en −4200).
            bool through;
            var ext7 = rect[0];
            var int7 = rect[4];
            failed += Check("pieza a −4500 → exterior", DesignWallFaceAssigner.Pick(ext7, int7, 3000, -4500, out through).Id, 1);
            failed += Check("pieza a −4200 → interior", DesignWallFaceAssigner.Pick(ext7, int7, 3000, -4200, out through).Id, 5);
            failed += Check("pieza a −4200 no atraviesa", through ? 1 : 0, 0);
            failed += Check("borde: tirante en el eje → exterior", DesignWallFaceAssigner.Pick(ext7, int7, 3000, -4350, out through).Id, 1);
            failed += Check("borde: tirante atraviesa", through ? 1 : 0, 1);
            failed += Check("borde: eje sin interior → exterior, no atraviesa", DesignWallFaceAssigner.Pick(ext7, null, 3000, -4350, out through).Id, 1);
            failed += Check("borde: sin caras → null", DesignWallFaceAssigner.Pick(null, null, 0, 0, out through) == null ? 1 : 0, 1);

            failed += Check("borde: sin ejes → sin caras", DesignWallFaceBuilder.Build(new List<DesignWallFaceAxis>()).Count, 0);
            failed += Check("borde: eje de longitud 0 se ignora", DesignWallFaceBuilder.Build(new List<DesignWallFaceAxis> { A(1, 5, 5, 5, 5, 300) }).Count, 0);
            failed += Check("borde: grosor 0 → 300", DesignWallFaceBuilder.Build(new List<DesignWallFaceAxis> { A(1, 0, 0, 1000, 0, 0) })[0].P1Z, 150);

            Console.WriteLine(failed == 0 ? "OK DesignWallFaceBuilder" : "FALLOS: " + failed);
            return failed == 0 ? 0 : 1;
        }

        private static string F(double v)
        {
            return v.ToString("F3", CultureInfo.InvariantCulture);
        }

        private static int Check(string name, double actual, double expected)
        {
            if (Math.Abs(actual - expected) <= 1e-6)
            {
                return 0;
            }

            Console.WriteLine("FALLA " + name + ": " + actual.ToString(CultureInfo.InvariantCulture) + " != " + expected.ToString(CultureInfo.InvariantCulture));
            return 1;
        }
    }
}
