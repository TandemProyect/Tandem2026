using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Pilar de cuatro paneles universales de 0,75 m (NEVI 3.8).
    /// Misma pieza que el Desing antiguo: marco y fenólico 27104219 / 24104224 / 12104120.
    /// En cada esquina, gancho de cierre 1920811 y tuerca placa campana 10443020,
    /// por los orificios laterales. El sobrante del panel cuelga del lado del orificio 1.
    /// </summary>
    internal static class PillarFormwork
    {
        private const double MinSideMm = 100d;
        private const double MaxSideMm = 650d;
        private const double DefaultSideMm = 300d;
        private const double PanelCm = 75d;
        /// <summary>Cara exterior de la costilla del 27104219.</summary>
        private const double PanelOuterFaceMm = 119.128d;
        /// <summary>
        /// Agujero de cabezal puntal (NEVI 2.9): el de la costilla más
        /// alejado del perfil exterior. En el 27104219 está a 428 mm del
        /// origen del panel.
        /// </summary>
        private const double RibHoleFromPanelStartMm = 428d;
        private const string StlFolder = "/Content/DesignTools/Stl/ATK60/";
        private const string HookCode = "1920811";
        private const string NutCode = "10443020";
        private const double HookRotX = Math.PI * 1.5;
        private const double PanelRotX = -Math.PI * 0.5;

        internal static bool IsPillar(Desing2FormworkWallDto wall)
        {
            if (wall == null)
            {
                return false;
            }

            if (string.Equals(wall.WallRole, "pillar", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var type = wall.Attributes != null ? wall.Attributes._TypeMesh : null;
            return string.Equals(type, "Pilar", StringComparison.OrdinalIgnoreCase);
        }

        internal static List<Atk60ElementPaintItem> Place(Desing2FormworkWallDto wall)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (!TryFrame(wall, out var frame))
            {
                return outElements;
            }

            var bands = Bands(frame.HeightMm);
            for (var b = 0; b < bands.Count; b++)
            {
                AddBand(outElements, frame, bands[b]);
            }

            var attrs = wall.Attributes;
            var props = attrs == null || !attrs._CHeckPropInside.HasValue || attrs._CHeckPropInside.Value;
            if (props)
            {
                AddProps(outElements, frame, FormworkTopMm(bands));
            }

            return outElements;
        }

        private static void AddBand(List<Atk60ElementPaintItem> outElements, Frame frame, Band band)
        {
            var longCm = frame.LongMm / 10d;
            var widthCm = frame.WidthMm / 10d;
            var upCm = band.UpMm / 10d;

            AddPanel(outElements, frame, band, longCm, -(widthCm - PanelCm), upCm, PanelRotX, -Math.PI * 1.5);
            AddPanel(outElements, frame, band, longCm - PanelCm, 0d, upCm, PanelRotX, 0d);
            AddPanel(outElements, frame, band, 0d, -PanelCm, upCm, PanelRotX, Math.PI * 1.5);
            AddPanel(outElements, frame, band, PanelCm, -widthCm, upCm, PanelRotX, Math.PI);

            var heights = HookHeightsCm(band.HeightMm);
            AddCorner(outElements, frame, upCm, heights, longCm + 1d, 6d, longCm + 12d, 15d, Math.PI, Math.PI * 0.5, Math.PI * 0.5);
            AddCorner(outElements, frame, upCm, heights, -4d, 1d, -12d, 12d, Math.PI * 0.5, Math.PI * 1.5, 0d);
            AddCorner(outElements, frame, upCm, heights, -2d, -widthCm - 5d, -12d, -widthCm - 14d, 0d, Math.PI * 0.5, Math.PI * 1.5);
            AddCorner(outElements, frame, upCm, heights, longCm + 5d, -widthCm - 1d, longCm + 14d, -widthCm - 12d, Math.PI * 1.5, Math.PI * 0.5, 0d);
        }

        private static void AddPanel(
            List<Atk60ElementPaintItem> outElements,
            Frame frame,
            Band band,
            double oldXcm,
            double oldYcm,
            double upCm,
            double rotX,
            double rotZ)
        {
            AddPiece(outElements, frame, band.Code, StlFolder + band.Code + ".stl", "frame-yellow", oldXcm, oldYcm, upCm, rotX, rotZ, band);
            AddPiece(outElements, frame, band.Code, StlFolder + band.Code + "_F.stl", "phenolic", oldXcm, oldYcm, upCm, rotX, rotZ, band);
        }

        private static void AddCorner(
            List<Atk60ElementPaintItem> outElements,
            Frame frame,
            double upCm,
            double[] heightsCm,
            double hookXcm,
            double hookYcm,
            double nutXcm,
            double nutYcm,
            double hookRotZ,
            double nutRotX,
            double nutRotZ)
        {
            for (var i = 0; i < heightsCm.Length; i++)
            {
                var zCm = upCm + heightsCm[i];
                AddPiece(outElements, frame, HookCode, StlFolder + HookCode + ".stl", "hook", hookXcm, hookYcm, zCm, HookRotX, hookRotZ, null);
                AddPiece(outElements, frame, NutCode, StlFolder + NutCode + ".stl", "nut", nutXcm, nutYcm, zCm, nutRotX, nutRotZ, null);
            }
        }

        private static void AddPiece(
            List<Atk60ElementPaintItem> outElements,
            Frame frame,
            string code,
            string path,
            string color,
            double oldXcm,
            double oldYcm,
            double heightCm,
            double rotX,
            double rotZ,
            Band band)
        {
            double x;
            double z;
            ToWorld(frame, oldXcm, oldYcm, out x, out z);
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = frame.IdWall,
                ElementType = "Panel",
                ElementCode = code,
                Orientation = "pillar",
                ImportPath = path,
                Color = color,
                X = x,
                Y = frame.BaseY + (heightCm * 10d),
                Z = z,
                RotX = rotX,
                RotY = frame.Yaw,
                RotZ = rotZ,
                UseStrictPose = true,
                PieceWidthMm = 750d,
                PieceHeightMm = band != null ? band.HeightMm : 0d,
                WallHeightMm = frame.HeightMm,
                WallLengthMm = frame.LongMm,
                WallThicknessMm = frame.WidthMm,
            });
        }

        private static void AddProps(List<Atk60ElementPaintItem> outElements, Frame frame, double formworkMm)
        {
            // El puntal sigue la altura del encofrado (suma de paneles), no la
            // del hormigón. 4,00 m son 2,70 + 2,40, así que el puntal es el de 5,10 m.
            double x;
            double z;
            var fromStartCm = (RibHoleFromPanelStartMm - PushPullProp.ProfilePinLeadMm(formworkMm)) / 10d;
            var faceCm = PanelOuterFaceMm / 10d;
            var longCm = frame.LongMm / 10d;
            ToWorld(frame, longCm - PanelCm + fromStartCm, faceCm, out x, out z);
            PushPullProp.PlaceSingle(
                outElements, frame.IdWall, x, frame.BaseY, z,
                Math.Atan2(frame.Yx, frame.Yz), frame.Yx, frame.Yz, formworkMm);
            ToWorld(frame, -faceCm, -PanelCm + fromStartCm, out x, out z);
            PushPullProp.PlaceSingle(
                outElements, frame.IdWall, x, frame.BaseY, z,
                Math.Atan2(-frame.Ax, -frame.Az), -frame.Ax, -frame.Az, formworkMm);
        }

        private static double FormworkTopMm(List<Band> bands)
        {
            if (bands == null || bands.Count == 0)
            {
                return 0d;
            }

            var last = bands[bands.Count - 1];
            return last.UpMm + last.HeightMm;
        }

        private static void ToWorld(Frame frame, double oldXcm, double oldYcm, out double x, out double z)
        {
            var alongMm = oldXcm * 10d;
            var sideMm = oldYcm * 10d;
            x = frame.CornerX + (frame.Ax * alongMm) + (frame.Yx * sideMm);
            z = frame.CornerZ + (frame.Az * alongMm) + (frame.Yz * sideMm);
        }

        private static double[] HookHeightsCm(double heightMm)
        {
            if (heightMm <= 1200d) return new[] { 35d, 115d };
            if (heightMm <= 2400d) return new[] { 32d, 135d, 205d };
            return new[] { 55d, 135d, 215d };
        }

        private static List<Band> Bands(double heightMm)
        {
            var bands = new List<Band>();
            var full = (int)((heightMm + 249d) / 2700d);
            var used = 0d;
            for (var i = 0; i < full; i++)
            {
                bands.Add(BandOf(used, 2700d));
                used += 2700d;
            }

            var rest = heightMm - used;
            if (rest > 1d)
            {
                if (rest <= 1200d) bands.Add(BandOf(used, 1200d));
                else if (rest <= 2400d) bands.Add(BandOf(used, 2400d));
                else bands.Add(BandOf(used, 2700d));
            }

            return bands;
        }

        private static Band BandOf(double upMm, double heightMm)
        {
            var code = "27104219";
            if (heightMm <= 1200d) code = "12104120";
            else if (heightMm <= 2400d) code = "24104224";
            return new Band { UpMm = upMm, HeightMm = heightMm, Code = code };
        }

        private static bool TryFrame(Desing2FormworkWallDto wall, out Frame frame)
        {
            frame = null;
            var attrs = wall != null ? wall.Attributes : null;
            if (attrs == null)
            {
                return false;
            }

            var x1 = ExtraMm(attrs, "InicioX");
            var z1 = ExtraMm(attrs, "InicioY");
            var y1 = ExtraMm(attrs, "InicioZ");
            var x2 = ExtraMm(attrs, "FinX");
            var z2 = ExtraMm(attrs, "FinY");
            if (!x1.HasValue || !z1.HasValue || !x2.HasValue || !z2.HasValue)
            {
                return false;
            }

            var dx = x2.Value - x1.Value;
            var dz = z2.Value - z1.Value;
            var longMm = Math.Sqrt((dx * dx) + (dz * dz));
            if (longMm < 1d)
            {
                return false;
            }

            longMm = ClampSide(longMm);
            var widthMm = ClampSide(MetersToMm(attrs._DataWith));
            var heightMm = attrs._DataHeight.HasValue ? MetersToMm(attrs._DataHeight) : 2700d;
            if (heightMm < 300d) heightMm = 2700d;

            var ax = dx / Math.Sqrt((dx * dx) + (dz * dz));
            var az = dz / Math.Sqrt((dx * dx) + (dz * dz));
            var yx = -az;
            var yz = ax;
            var cx = (x1.Value + x2.Value) * 0.5;
            var cz = (z1.Value + z2.Value) * 0.5;
            var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                ? wall.WallId
                : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

            frame = new Frame
            {
                IdWall = idWall,
                BaseY = y1 ?? 0d,
                HeightMm = heightMm,
                WidthMm = widthMm,
                LongMm = longMm,
                Ax = ax,
                Az = az,
                Yx = yx,
                Yz = yz,
                Yaw = Math.Atan2(az, ax),
                CornerX = cx - (ax * longMm * 0.5) + (yx * widthMm * 0.5),
                CornerZ = cz - (az * longMm * 0.5) + (yz * widthMm * 0.5),
            };
            return true;
        }

        private static double ClampSide(double mm)
        {
            if (mm < MinSideMm) return MinSideMm;
            if (mm > MaxSideMm) return MaxSideMm;
            return mm;
        }

        private static double? ExtraMm(AttributesList attrs, string key)
        {
            if (attrs == null || attrs.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token = null;
            foreach (var pair in attrs.Extra)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    token = pair.Value;
                    break;
                }
            }

            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<double>();
            }

            double value;
            if (double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return null;
        }

        private static double MetersToMm(double? meters)
        {
            if (!meters.HasValue || meters.Value <= 0d)
            {
                return DefaultSideMm;
            }

            return meters.Value < 20d ? meters.Value * 1000d : meters.Value;
        }

        private sealed class Frame
        {
            public string IdWall;
            public double BaseY;
            public double HeightMm;
            public double WidthMm;
            public double LongMm;
            public double Ax;
            public double Az;
            public double Yx;
            public double Yz;
            public double Yaw;
            public double CornerX;
            public double CornerZ;
        }

        private sealed class Band
        {
            public double UpMm;
            public double HeightMm;
            public string Code;
        }
    }
}
