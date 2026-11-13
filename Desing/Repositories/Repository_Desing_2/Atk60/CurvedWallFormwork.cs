using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Encofrado del muro curvo (WallRole "curved" / _TypeMesh "MuroCurvo") con paneles planos ATK-60.
    ///
    /// Regla (boceto del usuario, 13/11/2026; remate interior mínimo 0,05 el mismo día):
    ///   N = el mayor número de sectores, como mucho floor(porción · 2π · R_int / modulación), con el
    ///       remate interior 2·r·sin(θ/2) − b de al menos 50 mm; el exterior queda el que salga
    ///   ángulo por sector θ = porción · 360° / N       (Ø 10 m, e 0,30, 0,90 → N 31, θ 11,61°)
    /// En cada sector, un panel de la modulación en cada cara, enfrentados y centrados en la bisectriz:
    ///   cara exterior tangente al arco exterior (polígono circunscrito, lado 2·R·tan(θ/2));
    ///   cara interior en la cuerda del arco interior (polígono inscrito, lado 2·r·sin(θ/2)).
    /// Así el hormigón nunca baja del espesor del muro. Cada sector se arma como un tramo recto
    /// de un panel (Atk60ModuleAssembly): panel, grapas y tirantes que atraviesan el muro del panel
    /// exterior a su panel interior, en el agujero a 43,7 mm del canto y a las
    /// alturas del muro recto, en los dos cantos de cada panel (tirante a cada lado del remate, NEVI 3.2.3). En cada junta entre sectores, remate de madera
    /// (EndFiller, tope 0,15) con su grapa regulable en las dos caras:
    ///   remate exterior = 2·R·tan(θ/2) − b, remate interior = 2·r·sin(θ/2) − b.
    /// Porción menor que 1/1: los paneles de los extremos van a ras del final del arco en la cara
    /// interior y el extremo se cierra con el tape del muro recto (ExactWidthEndTape).
    /// Sin grapas de junta vertical: cada sector es un solo panel. Si θ pasa de 90° no se encofra.
    /// Alturas de más de 2,70 (13/11/2026): con modulación estrecha los paneles se apilan de pie como en el
    /// recto (3,00 → 2,70 + 1,20 = 3,90; 4,00 → 2,70 + 2,40). Cada nivel lleva sus tirantes en los dos cantos
    /// (a sus alturas, aunque pasen del alto del muro: 3,00 → 0,55 / 2,15 / 3,05 / 3,55), la junta horizontal
    /// entre niveles se une con grapa fija horizontal 10004220 en las dos caras (a 200 mm de cada canto) y
    /// con el rigidizador vertical 1850162 de siempre; el remate sube hasta el borde alto de los paneles con
    /// sus grapas regulables a las alturas del remate (3,90 → 0,45 / 1,35 / 2,25 / 2,90 / 3,40).
    /// Ménsulas y puntales (13/11/2026): mismos atributos y piezas que el muro recto (`_CHeckBracket*`,
    /// `_CHeckProp*`, `_PropSpacingMm`). Cantidad = la del recto sobre el largo desarrollado de la cara
    /// exterior (N · lado exterior); se reparten por sectores, como mucho uno por sector, perpendiculares al
    /// panel (radiales): ménsula en el centro del panel, puntal en el alojamiento a 50 mm del canto de inicio.
    /// Cara interior: si el pie no cabe hacia el centro (anillo pequeño), no se pone y el formulario avisa.
    /// </summary>
    internal static class CurvedWallFormwork
    {
        internal const double DefaultModulationMm = 900d;
        private static readonly double[] Modulations = { 300d, 450d, 600d, 750d, 900d, 1050d, 1200d };
        private const string ConnectedMark = "curved";
        /// <summary>Remate interior mínimo de cada junta (mm).</summary>
        internal const double MinInnerRemateMm = 50d;
        /// <summary>Origen de ménsula y puntal: 120 mm por fuera de la cara del muro (panel).</summary>
        private const double SupportFaceMm = 120d;
        /// <summary>Vuelo de la ménsula 4120000042 desde su origen (STL: 1,0 m).</summary>
        internal const double BracketReachMm = 1000d;
        /// <summary>Hueco mínimo entre pies (o ménsulas) vecinos de la cara interior.</summary>
        internal const double MinFootGapMm = 400d;

        /// <summary>Vuelo del pie del puntal desde su origen: h270 1,6 m (STL); más altos, 0,6 · altura.</summary>
        internal static double PropReachMm(double wallHeightMm)
        {
            return Math.Max(1600d, 0.6d * wallHeightMm);
        }

        /// <summary>Sectores con ménsula y con puntal, y si caben en la cara interior.</summary>
        internal sealed class Supports
        {
            public List<int> BracketSectors;
            public List<int> PropSectors;
            public bool BracketsInsideFit;
            public bool PropsInsideFit;
        }

        internal static Supports ResolveSupports(Desing2FormworkWallDto wall, Layout lay)
        {
            var developedMm = lay.Count * lay.OuterSideMm;
            var attrs = wall != null ? wall.Attributes : null;
            var brackets = SpreadSectors(WorkingBracket.StationCount(developedMm), lay.Count);
            var props = SpreadSectors(
                (int)Math.Ceiling((developedMm / PushPullProp.MaxSpacingMm(attrs)) - 1e-9), lay.Count);
            return new Supports
            {
                BracketSectors = brackets,
                PropSectors = props,
                BracketsInsideFit = InsideFits(lay, BracketReachMm, brackets),
                PropsInsideFit = InsideFits(lay, PropReachMm(lay.HeightMm), props),
            };
        }

        /// <summary><paramref name="count"/> sectores de <paramref name="n"/>, repartidos a paso constante (centro de cada tramo).</summary>
        internal static List<int> SpreadSectors(int count, int n)
        {
            var found = new List<int>();
            if (count > n)
            {
                count = n;
            }

            for (var k = 0; k < count; k++)
            {
                found.Add((int)Math.Floor((k + 0.5d) * n / count));
            }

            return found;
        }

        /// <summary>
        /// Pie hacia el centro: radio del pie = r·cos(θ/2) − 120 − vuelo &gt; 0 y, entre vecinos, cuerda de al menos 400 mm.
        /// </summary>
        private static bool InsideFits(Layout lay, double reachMm, List<int> sectors)
        {
            var footR = (lay.InnerRadiusMm * Math.Cos(lay.SectorRad * 0.5d)) - SupportFaceMm - reachMm;
            if (footR <= 0d)
            {
                return false;
            }

            if (sectors.Count < 2)
            {
                return true;
            }

            var minGap = int.MaxValue;
            for (var i = 1; i < sectors.Count; i++)
            {
                minGap = Math.Min(minGap, sectors[i] - sectors[i - 1]);
            }

            if (lay.Full)
            {
                minGap = Math.Min(minGap, sectors[0] + lay.Count - sectors[sectors.Count - 1]);
            }

            return 2d * footR * Math.Sin(minGap * lay.SectorRad * 0.5d) >= MinFootGapMm;
        }

        internal static bool IsCurved(Desing2FormworkWallDto wall)
        {
            if (wall == null)
            {
                return false;
            }

            if (string.Equals(wall.WallRole, "curved", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var type = wall.Attributes != null ? wall.Attributes._TypeMesh : null;
            return string.Equals(type, "MuroCurvo", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Reparto del muro curvo. Null si los datos no sirven o no cabe ni un panel.</summary>
        internal sealed class Layout
        {
            public double CenterX;
            public double CenterY;
            public double CenterZ;
            public double OuterRadiusMm;
            public double InnerRadiusMm;
            public double ThicknessMm;
            public double HeightMm;
            public double ModulationMm;
            public double Fraction;
            public double StartRad;
            public int Count;
            public double SectorRad;
            /// <summary>Lado del polígono exterior (tangente) e interior (cuerda).</summary>
            public double OuterSideMm;
            public double InnerSideMm;
            /// <summary>Remate de cada junta entre sectores.</summary>
            public double OuterRemateMm;
            public double InnerRemateMm;
            /// <summary>Distancia entre las dos caras de un sector (espesor + flecha).</summary>
            public double SectorThicknessMm;
            public bool Full;
        }

        internal static Layout Resolve(Desing2FormworkWallDto wall)
        {
            var attrs = wall != null ? wall.Attributes : null;
            if (attrs == null)
            {
                return null;
            }

            var x1 = attrs.ExtraValueAsDouble("InicioX");
            var z1 = attrs.ExtraValueAsDouble("InicioY");
            var x2 = attrs.ExtraValueAsDouble("FinX");
            var z2 = attrs.ExtraValueAsDouble("FinY");
            var cx = attrs.ExtraValueAsDouble("_CurveCenterX");
            var cz = attrs.ExtraValueAsDouble("_CurveCenterZ");
            var cy = attrs.ExtraValueAsDouble("_CurveCenterY") ?? attrs.ExtraValueAsDouble("InicioZ") ?? 0d;
            var r = attrs.ExtraValueAsDouble("_CurveOuterRadiusMm");
            var startDeg = attrs.ExtraValueAsDouble("_CurveStartAngleDeg");
            var hasEnds = x1.HasValue && z1.HasValue && x2.HasValue && z2.HasValue;
            if (!cx.HasValue || !cz.HasValue)
            {
                if (!hasEnds) return null;
                cx = (x1.Value + x2.Value) * 0.5d;
                cz = (z1.Value + z2.Value) * 0.5d;
            }

            if (!r.HasValue || r.Value <= 1d)
            {
                if (!hasEnds) return null;
                r = Math.Sqrt(Math.Pow(x2.Value - x1.Value, 2) + Math.Pow(z2.Value - z1.Value, 2)) * 0.5d;
            }

            if (!startDeg.HasValue)
            {
                startDeg = hasEnds ? Math.Atan2(z2.Value - z1.Value, x2.Value - x1.Value) * 180d / Math.PI : 0d;
            }

            var num = attrs.ExtraValueAsDouble("_CurveFractionNum") ?? 1d;
            var den = attrs.ExtraValueAsDouble("_CurveFractionDen") ?? 1d;
            if (num < 1d || den < 1d || num > den)
            {
                num = 1d;
                den = 1d;
            }

            var modulation = attrs.ExtraValueAsDouble("_CurveModulationMm") ?? DefaultModulationMm;
            if (Array.IndexOf(Modulations, Math.Round(modulation)) < 0)
            {
                modulation = DefaultModulationMm;
            }

            var thickness = Atk60FaceInsertion.ResolveWallThicknessMm(attrs);
            var rOut = r.Value;
            var rIn = rOut - thickness;
            if (rIn <= 1d)
            {
                return null;
            }

            return Compute(cx.Value, cy, cz.Value, rOut, thickness,
                Atk60FaceInsertion.ResolveWallHeightMm(attrs), modulation, num / den, startDeg.Value * Math.PI / 180d);
        }

        internal static Layout Compute(
            double cx, double cy, double cz, double rOut, double thickness, double height,
            double modulation, double fraction, double startRad)
        {
            var rIn = rOut - thickness;
            var full = fraction >= 1d - 1e-9;
            var sweep = Math.PI * 2d * Math.Min(1d, fraction);
            var n = (int)Math.Floor((sweep * rIn / modulation) + 1e-9);
            // Menos sectores hasta que el remate interior llegue al mínimo.
            while (n > 1 && (2d * rIn * Math.Sin(sweep / n * 0.5d)) - modulation < MinInnerRemateMm - 1e-6)
            {
                n--;
            }

            if (n < 1 || rIn <= 1d)
            {
                return null;
            }

            var theta = sweep / n;
            if (theta > (Math.PI * 0.5d) + 1e-9)
            {
                // Más de 90° por panel: el polígono ya no se parece al círculo (radio muy pequeño).
                return null;
            }

            var outerSide = 2d * rOut * Math.Tan(theta * 0.5d);
            var innerSide = 2d * rIn * Math.Sin(theta * 0.5d);
            return new Layout
            {
                CenterX = cx,
                CenterY = cy,
                CenterZ = cz,
                OuterRadiusMm = rOut,
                InnerRadiusMm = rIn,
                ThicknessMm = thickness,
                HeightMm = height,
                ModulationMm = modulation,
                Fraction = Math.Min(1d, fraction),
                StartRad = startRad,
                Count = n,
                SectorRad = theta,
                OuterSideMm = outerSide,
                InnerSideMm = innerSide,
                OuterRemateMm = outerSide - modulation,
                InnerRemateMm = innerSide - modulation,
                SectorThicknessMm = rOut - (rIn * Math.Cos(theta * 0.5d)),
                Full = full,
            };
        }

        internal static List<Atk60ElementPaintItem> Place(Desing2FormworkWallDto wall)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            var lay = Resolve(wall);
            if (lay == null)
            {
                return outElements;
            }

            var b = lay.ModulationMm;
            var halfIn = lay.InnerSideMm * 0.5d;
            var supports = ResolveSupports(wall, lay);
            var formworkTopMm = FormworkTopMm(wall, lay);
            for (var i = 0; i < lay.Count; i++)
            {
                var first = !lay.Full && i == 0;
                var last = !lay.Full && i == lay.Count - 1;
                var mid = lay.StartRad + ((i + 0.5d) * lay.SectorRad);

                // t crece en sentido antihorario (con el ángulo). Panel en [p, p + b].
                var p = -b * 0.5d;
                if (first && !last) p = -halfIn;
                else if (last && !first) p = halfIn - b;

                var sectorWall = SectorWall(wall, lay.SectorThicknessMm, b, first, last);
                // El tape se elige con el espesor real del muro (0,30, 0,45…), no con el del sector.
                var tapeWall = SectorWall(wall, lay.ThicknessMm, b, first, last);
                var anchor = SectorAnchor(wall, lay, mid, lay.OuterRadiusMm, p + b);
                // Panel, grapas y tirantes del panel exterior a su panel interior, como en un tramo
                // recto de un solo panel. Cada junta lleva remate de madera sin rigidizadores, así que
                // va tirante a los dos lados del remate (manual NEVI 3.2.3: "tirantes a ambos lados de
                // la compensación"): agujero de los dos cantos del panel, a 43,7 mm. En el extremo libre
                // del arco manda su tape, como en el muro recto (SkipsPanelEndTie solo actúa ahí).
                // Sector sin extremo libre: la regla de muro < 0,30 (tablón de cierre) no aplica a las juntas.
                outElements.AddRange(Atk60ModuleAssembly.Build(
                    sectorWall, anchor, 1, b, 0d, 0, 1,
                    false, !first && !last,
                    ExactWidthEndTape.EndUsesPanelTies(tapeWall, true),
                    ExactWidthEndTape.EndUsesPanelTies(tapeWall, false),
                    0d, b,
                    ExactWidthEndTape.SkipsPanelEndTie(tapeWall, true),
                    ExactWidthEndTape.SkipsPanelEndTie(tapeWall, false),
                    true));

                // Ménsula y puntal del muro recto, perpendiculares al panel del sector (radiales).
                if (supports.BracketSectors.Contains(i))
                {
                    WorkingBracket.PlaceAt(outElements, sectorWall, anchor, b * 0.5d, b, supports.BracketsInsideFit);
                }

                if (supports.PropSectors.Contains(i))
                {
                    PushPullProp.PlaceAt(outElements, sectorWall, anchor, PushPullProp.PanelHousingMm, supports.PropsInsideFit);
                }

                if (first || last)
                {
                    // Extremo del arco: tape del muro recto. Su "inicio" es along 0 (lado de mayor ángulo).
                    outElements.AddRange(ExactWidthEndTape.Place(tapeWall, anchor, 0d, true, 0d));
                }

                if (lay.Full || i < lay.Count - 1)
                {
                    PlaceJointRemates(outElements, wall, lay, lay.StartRad + ((i + 1) * lay.SectorRad), i, lay.Count, formworkTopMm);
                }
            }

            return outElements;
        }

        /// <summary>
        /// Borde alto de los paneles del sector. Con módulo estrecho (0,30–0,90) los paneles se apilan de pie
        /// (2,70 / 2,40 / 1,20) y el de arriba asoma: 3,00 → 2,70 + 1,20 = 3,90 (como el ejemplo NEVI 3.1.2).
        /// </summary>
        internal static double FormworkTopMm(Desing2FormworkWallDto wall, Layout lay)
        {
            var attrs = wall != null ? wall.Attributes : null;
            var layout = Modulo270HeightPanelCatalog.ResolveForModule(
                lay.HeightMm, (int)Math.Round(lay.ModulationMm),
                Atk60FaceInsertion.UseAdjustable750(attrs),
                Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckTypeFormworkMode : null));
            var top = lay.HeightMm;
            if (layout != null && layout.Pieces != null)
            {
                foreach (var piece in layout.Pieces)
                {
                    top = Math.Max(top, piece.UpOffsetMm + piece.PieceHeightMm);
                }
            }

            return top;
        }

        /// <summary>
        /// Remate de la junta entre dos sectores, centrado en el vértice de cada cara
        /// (exterior a R / cos(θ/2), interior a r), con grapa regulable en sus dos cantos.
        /// Alto: el de los paneles (<paramref name="formworkTopMm"/>), no el del muro, para cerrar también
        /// la junta del panel apilado; grapas regulables a las alturas de ese alto.
        /// </summary>
        private static void PlaceJointRemates(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Layout lay,
            double rayRad,
            int index,
            int count,
            double formworkTopMm)
        {
            var outerVertex = lay.OuterRadiusMm / Math.Cos(lay.SectorRad * 0.5d);
            var jointWall = SectorWall(wall, outerVertex - lay.InnerRadiusMm, lay.ModulationMm, false, false);
            jointWall.Attributes._DataHeight = formworkTopMm / 1000d;
            var anchor = SectorAnchor(wall, lay, rayRad, outerVertex, 0d);
            var outer = OneJointGap(lay, index, count, true);
            var inner = OneJointGap(lay, index, count, false);
            if (outer > 1d)
            {
                outElements.AddRange(EndFiller.Build(jointWall, anchor, outer, -outer * 0.5d, true, false));
            }

            if (inner > 1d)
            {
                outElements.AddRange(EndFiller.Build(jointWall, anchor, inner, -inner * 0.5d, false, true));
            }

        }

        /// <summary>Hueco de la junta: lado del polígono − panel; en porciones, el panel del extremo va a ras.</summary>
        internal static double OneJointGap(Layout lay, int index, int count, bool outer)
        {
            var side = outer ? lay.OuterSideMm : lay.InnerSideMm;
            var gap = side - lay.ModulationMm;
            if (lay.Full || count < 2)
            {
                return gap;
            }

            // Panel del extremo corrido (L_int − b)/2 hacia el final del arco: ese medio hueco pasa a la junta.
            var shift = (lay.InnerSideMm - lay.ModulationMm) * 0.5d;
            if (index == 0) gap += shift;
            if (index + 1 == count - 1) gap += shift;
            return gap;
        }

        /// <summary>
        /// Punto de inserción del sector: en la cara exterior (a <paramref name="faceDistMm"/> del centro),
        /// a <paramref name="tMm"/> sobre la tangente. El eje del tramo va en sentido horario (along = −t),
        /// así la normal (−uz, ux) apunta hacia fuera con FaceSign +1.
        /// </summary>
        private static Atk60WallPaintAnchor SectorAnchor(
            Desing2FormworkWallDto wall, Layout lay, double rayRad, double faceDistMm, double tMm)
        {
            var mx = Math.Cos(rayRad);
            var mz = Math.Sin(rayRad);
            var tx = -mz;
            var tz = mx;
            var ux = -tx;
            var uz = -tz;
            return new Atk60WallPaintAnchor
            {
                IdWall = IdOf(wall),
                X = lay.CenterX + (mx * faceDistMm) + (tx * tMm),
                Y = lay.CenterY,
                Z = lay.CenterZ + (mz * faceDistMm) + (tz * tMm),
                RotX = 0d,
                RotY = Math.Atan2(uz, ux),
                RotZ = 0d,
                NormalX = mx,
                NormalZ = mz,
                FaceSign = 1d,
            };
        }

        /// <summary>Tramo recto equivalente: mismo muro con espesor del sector y largo de un panel.</summary>
        private static Desing2FormworkWallDto SectorWall(
            Desing2FormworkWallDto wall, double thicknessMm, double lengthMm, bool startFree, bool endFree)
        {
            var copy = JsonConvert.DeserializeObject<Desing2FormworkWallDto>(JsonConvert.SerializeObject(wall));
            if (copy.Attributes == null)
            {
                copy.Attributes = new AttributesList();
            }

            copy.WallId = IdOf(wall);
            copy.Attributes._DataWith = thicknessMm / 1000d;
            copy.Attributes._Datalong = lengthMm / 1000d;
            // along 0 = lado de mayor ángulo. El primer sector (inicio del arco) tiene libre el final del tramo.
            copy.Attributes._Idconnection_1 = endFree ? null : ConnectedMark;
            copy.Attributes._Idconnection_2 = startFree ? null : ConnectedMark;
            if (copy.Attributes.Extra != null)
            {
                copy.Attributes.Extra.Remove("_Idconnection_1");
                copy.Attributes.Extra.Remove("_Idconnection_2");
            }

            return copy;
        }

        private static string IdOf(Desing2FormworkWallDto wall)
        {
            return !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
        }
    }
}
