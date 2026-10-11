using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Ménsula de trabajo y, solo en el primer muro, la madera de protección.
    /// El origen del STL queda en la cara del panel: coordenada del muro, su rotación
    /// y 120 mm hacia el exterior.
    /// </summary>
    internal static class WorkingBracket
    {
        private const double PanelFaceMm = 120d;
        private const double BayMm = 2400d;
        private const double BracketFromBayStartMm = 600d;
        private const double BracketSecondInBayMm = 1800d;
        private const double BracketBelowTopMm = 300d;
        /// <summary>Paso al agujero vecino del bastidor. El mismo que usa el rigidizador.</summary>
        private const double HoleStepMm = 300d;
        private const double StiffenerClearMm = 80d;

        private const string BracketPath = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
        private const string BracketCode = "4120000042";
        private const string DeckPath = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
        private const string DeckCode = "Tablon24x15x5";
        private const string GuardPath = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5-2.stl";
        private const string GuardCode = "Tablon24x15x5-2";

        /// <summary>Ancho del tablón de piso (eje local que, tras la rotación de unión, entra hacia el muro).</summary>
        private const double DeckWidthMm = 150d;

        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double formedLengthMm,
            bool protectionWood,
            IList<double> stiffenerAlongMm)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (wall == null || anchor == null || formedLengthMm < BracketFromBayStartMm)
            {
                return outElements;
            }

            var attrs = wall.Attributes;
            var yawRad = NormalizeYawToRad(anchor.RotY);
            var ux = Math.Cos(yawRad);
            var uz = Math.Sin(yawRad);
            var faceSign = anchor.FaceSign >= 0d ? 1d : -1d;
            var wallHeightMm = ResolveWallHeightMm(attrs);
            var wallThicknessMm = ResolveWallThicknessMm(attrs);
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
            var upMm = wallHeightMm - BracketBelowTopMm;

            foreach (var alongMm in BracketStations(formedLengthMm, stiffenerAlongMm))
            {
                AddBothFaces(
                    outElements, anchor, idWall, BracketPath, BracketCode, "blue",
                    alongMm, upMm, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, formedLengthMm, PanelFaceMm);
            }

            if (protectionWood && formedLengthMm >= BayMm)
            {
                AddProtectionWood(
                    outElements, anchor, idWall,
                    yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, formedLengthMm);
            }

            return outElements;
        }

        /// <summary>
        /// Dos ménsulas por tramo de 2,40 m (a 600 y a 1 800).
        /// El resto, si llega a 1,20 m, lleva dos; si llega a 0,60 m, una.
        /// Si ese puesto es el del rigidizador, la ménsula pasa al agujero siguiente.
        /// </summary>
        private static List<double> BracketStations(double lengthMm, IList<double> stiffenerAlongMm)
        {
            var raw = new List<double>();
            var full = (int)Math.Floor(lengthMm / BayMm);
            for (var i = 0; i < full; i++)
            {
                var origin = i * BayMm;
                raw.Add(origin + BracketFromBayStartMm);
                raw.Add(origin + BracketSecondInBayMm);
            }

            var rest = lengthMm - (full * BayMm);
            var tail = full * BayMm;
            if (rest >= 1200d)
            {
                raw.Add(tail + (rest / 3d));
                raw.Add(tail + (2d * rest / 3d));
            }
            else if (rest >= BracketFromBayStartMm)
            {
                raw.Add(tail + (rest * 0.5d));
            }

            var placed = new List<double>(raw.Count);
            for (var i = 0; i < raw.Count; i++)
            {
                placed.Add(ClearOfStiffener(raw[i], lengthMm, stiffenerAlongMm, placed));
            }

            return placed;
        }

        /// <summary>
        /// Solo se mueve la ménsula que cae sobre un rigidizador.
        /// El agujero de al lado está a 300 mm. Si ese también está ocupado, el otro lado.
        /// </summary>
        private static double ClearOfStiffener(
            double alongMm,
            double lengthMm,
            IList<double> stiffenerAlongMm,
            IList<double> alreadyPlaced)
        {
            if (!Hits(alongMm, stiffenerAlongMm) && !Hits(alongMm, alreadyPlaced))
            {
                return alongMm;
            }

            var steps = new[] { HoleStepMm, -HoleStepMm, HoleStepMm * 2d, -HoleStepMm * 2d };
            for (var i = 0; i < steps.Length; i++)
            {
                var candidate = alongMm + steps[i];
                if (candidate < StiffenerClearMm || candidate > lengthMm - StiffenerClearMm)
                {
                    continue;
                }

                if (Hits(candidate, stiffenerAlongMm) || Hits(candidate, alreadyPlaced))
                {
                    continue;
                }

                return candidate;
            }

            return alongMm;
        }

        private static bool Hits(double alongMm, IList<double> stations)
        {
            if (stations == null)
            {
                return false;
            }

            for (var i = 0; i < stations.Count; i++)
            {
                if (Math.Abs(alongMm - stations[i]) < StiffenerClearMm)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Una sola vez, en la cara por la que se empieza el muro.
        /// Cinco tablones de piso y tres de protección, sobre el primer tramo de 2,40 m.
        /// El origen del tablón de piso está en el canto exterior: la malla crece hacia el muro.
        /// </summary>
        private static void AddProtectionWood(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            var alongMm = BracketFromBayStartMm;
            var deckUpMm = wallHeightMm - 250d;
            var deckNearEdgeMm = new[] { 280d, 460d, 640d, 820d, 1000d };
            for (var i = 0; i < deckNearEdgeMm.Length; i++)
            {
                AddPrimaryFace(
                    outElements, anchor, idWall, DeckPath, DeckCode, "wood",
                    alongMm, deckUpMm, yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    deckNearEdgeMm[i] + DeckWidthMm);
            }

            var guardOutMm = 1090d;
            var guardHeights = new[]
            {
                wallHeightMm - 240d,
                wallHeightMm + 260d,
                wallHeightMm + 970d,
            };
            for (var i = 0; i < guardHeights.Length; i++)
            {
                AddPrimaryFace(
                    outElements, anchor, idWall, GuardPath, GuardCode, "wood",
                    alongMm, guardHeights[i], yawRad, ux, uz, faceSign,
                    wallHeightMm, wallThicknessMm, wallLengthMm,
                    guardOutMm);
            }
        }

        private static void AddBothFaces(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double alongMm,
            double upMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double outMm)
        {
            AddPrimaryFace(
                outElements, anchor, idWall, path, code, color,
                alongMm, upMm, yawRad, ux, uz, faceSign,
                wallHeightMm, wallThicknessMm, wallLengthMm, outMm);

            var jointX = anchor.X + (ux * alongMm);
            var jointZ = anchor.Z + (uz * alongMm);
            var oppositeX = jointX - (anchor.NormalX * wallThicknessMm);
            var oppositeZ = jointZ - (anchor.NormalZ * wallThicknessMm);
            // La cara de atrás no usa el espejo de la grapa: ese giro
            // deja la ménsula boca abajo. Misma postura vertical, normal hacia fuera.
            AddFace(
                outElements, anchor, idWall, path, code, color,
                oppositeX - (anchor.NormalX * outMm),
                anchor.Y + upMm,
                oppositeZ - (anchor.NormalZ * outMm),
                -yawRad + (faceSign < 0d ? Math.PI : 0d) + Math.PI,
                -anchor.NormalX,
                -anchor.NormalZ,
                -faceSign,
                false,
                alongMm,
                upMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm);
        }

        private static void AddPrimaryFace(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double alongMm,
            double upMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            double outMm)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointZ = anchor.Z + (uz * alongMm);
            AddFace(
                outElements, anchor, idWall, path, code, color,
                jointX + (anchor.NormalX * outMm),
                anchor.Y + upMm,
                jointZ + (anchor.NormalZ * outMm),
                -yawRad + (faceSign < 0d ? Math.PI : 0d),
                anchor.NormalX,
                anchor.NormalZ,
                faceSign,
                false,
                alongMm,
                upMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm);
        }

        private static void AddFace(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double x,
            double y,
            double z,
            double rotY,
            double normalX,
            double normalZ,
            double faceSign,
            bool mirrored,
            double alongMm,
            double upMm,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Union",
                ElementCode = code,
                Orientation = "Vertical",
                ImportPath = path,
                Color = color,
                X = x,
                Y = y,
                Z = z,
                RotX = 0,
                RotY = rotY,
                RotZ = 0,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = faceSign,
                IsMirrored = mirrored,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = wallThicknessMm,
                WallLengthMm = wallLengthMm,
                LocalAlongMm = alongMm,
                LocalUpMm = upMm,
                UseStrictPose = true,
            });
        }

        private static double ResolveWallHeightMm(AttributesList attrs)
        {
            var h = ToSceneMm(attrs != null ? (attrs._DataHeight ?? attrs.ExtraValueAsDouble("_DataHeight")) : null);
            return h > 1d ? h : 2700d;
        }

        private static double ResolveWallThicknessMm(AttributesList attrs)
        {
            var t = ToSceneMm(attrs != null ? (attrs._DataWith ?? attrs.ExtraValueAsDouble("_DataWith")) : null);
            return t > 1d ? Math.Abs(t) : 300d;
        }

        private static double ToSceneMm(double? value)
        {
            if (!value.HasValue)
            {
                return 0d;
            }

            var abs = Math.Abs(value.Value);
            return abs <= 50d ? value.Value * 1000d : value.Value;
        }

        private static double NormalizeYawToRad(double yaw)
        {
            var abs = Math.Abs(yaw);
            return abs > (Math.PI * 2 + 1e-6)
                ? (yaw * Math.PI / 180d)
                : yaw;
        }
    }
}
