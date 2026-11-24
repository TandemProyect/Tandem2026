using Desing.Repositories.Repository_Desing_2.Atk60;
using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Desing.Repositories.RepositoryAtk60
{
    public sealed partial class Atk60WallsRepository
    {
        /// <summary>
        /// Cada cara tiene su remate. No se suman en la esquina: el taco de
        /// cada cara se queda en el extremo que toca la L.
        /// </summary>
        private static Dictionary<string, FaceRemateSides> JoinCornerFaceRemates(List<Desing2FormworkWallDto> walls)
        {
            var result = new Dictionary<string, FaceRemateSides>(StringComparer.OrdinalIgnoreCase);
            var slots = new List<CornerFaceSlot>();
            var wallByKey = new Dictionary<string, Desing2FormworkWallDto>(StringComparer.OrdinalIgnoreCase);
            if (walls == null)
            {
                return result;
            }

            for (var i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall == null || PillarFormwork.IsPillar(wall) || wall.Attributes == null)
                {
                    continue;
                }

                if (ExactWidthEndTape.SuppressesRemate(wall))
                {
                    continue;
                }

                var key = WallFormworkKey(wall);
                var faces = CornerFacesToken(wall.Attributes) as JArray;
                if (string.IsNullOrWhiteSpace(key) || faces == null)
                {
                    continue;
                }

                wallByKey[key] = wall;
                for (var fi = 0; fi < faces.Count; fi++)
                {
                    var item = faces[fi] as JObject;
                    if (item == null)
                    {
                        continue;
                    }

                    var length = TokenDouble(item, "lengthMm");
                    if (!length.HasValue)
                    {
                        continue;
                    }

                    var lengthMm = length.Value;
                    var x1 = TokenDouble(item, "x1") ?? 0d;
                    var z1 = TokenDouble(item, "z1") ?? 0d;
                    var x2 = TokenDouble(item, "x2") ?? 0d;
                    var z2 = TokenDouble(item, "z2") ?? 0d;
                    if (Math.Abs(lengthMm) <= 50d)
                    {
                        lengthMm *= 1000d;
                        x1 *= 1000d;
                        z1 *= 1000d;
                        x2 *= 1000d;
                        z2 *= 1000d;
                    }
                    else
                    {
                        var maxAbs = Math.Max(
                            Math.Max(Math.Abs(x1), Math.Abs(z1)),
                            Math.Max(Math.Abs(x2), Math.Abs(z2)));
                        if (maxAbs <= 50d && lengthMm > 50d)
                        {
                            x1 *= 1000d;
                            z1 *= 1000d;
                            x2 *= 1000d;
                            z2 *= 1000d;
                        }
                    }

                    if (lengthMm <= 1d)
                    {
                        continue;
                    }

                    int n270;
                    int remateMm;
                    PackAtk60LengthMm((int)Math.Round(lengthMm), out n270, out remateMm);
                    remateMm = Atk60FaceRemateRules.CapMm(remateMm);
                    if (remateMm <= 1)
                    {
                        continue;
                    }

                    var side = TokenDouble(item, "side");
                    slots.Add(new CornerFaceSlot
                    {
                        WallKey = key,
                        Side = side.HasValue && side.Value < 0d ? -1 : 1,
                        X1 = x1,
                        Z1 = z1,
                        X2 = x2,
                        Z2 = z2,
                        LengthMm = lengthMm,
                        RemateMm = remateMm,
                        IsInterior = true
                    });
                }
            }

            if (slots.Count == 0)
            {
                return result;
            }

            MarkCornerMeetings(slots);
            MarkInteriorFaces(slots);
            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.MeetAt1 && !slot.MeetAt2)
                {
                    continue;
                }

                Desing2FormworkWallDto wall;
                if (!wallByKey.TryGetValue(slot.WallKey, out wall) || wall == null)
                {
                    continue;
                }

                FaceRemateSides sides;
                if (!result.TryGetValue(slot.WallKey, out sides))
                {
                    sides = new FaceRemateSides();
                    result[slot.WallKey] = sides;
                }

                var alongMm = RemateStartAlongMm(ResolveWallGeom(wall), slot);
                if (slot.Side < 0)
                {
                    sides.HasNeg = true;
                    sides.NegMm = slot.RemateMm;
                    sides.NegAlongMm = alongMm;
                    sides.NegInterior = slot.IsInterior;
                }
                else
                {
                    sides.HasPos = true;
                    sides.PosMm = slot.RemateMm;
                    sides.PosAlongMm = alongMm;
                    sides.PosInterior = slot.IsInterior;
                }
            }

            return result;
        }

        private static void PaintFaceRemate(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double axisRemateMm,
            double startAlongMm,
            FaceRemateSides sides)
        {
            if (sides == null)
            {
                if (axisRemateMm > 1d)
                {
                    outElements.AddRange(Modulo270PanelElementGenerator.BuildRemate(wall, anchor, axisRemateMm, startAlongMm));
                }

                return;
            }

            var runMm = ExactWidthEndTape.RunMm(anchor, wall);
            var primaryIsPos = anchor == null || anchor.FaceSign >= 0d;
            if (sides.HasPos)
            {
                PaintOneFaceRemate(
                    outElements, wall, anchor, sides.PosMm, sides.PosAlongMm, runMm, primaryIsPos, sides.PosInterior);
            }

            if (sides.HasNeg)
            {
                PaintOneFaceRemate(
                    outElements, wall, anchor, sides.NegMm, sides.NegAlongMm, runMm, !primaryIsPos, sides.NegInterior);
            }
        }

        private static void PaintOneFaceRemate(
            List<Atk60ElementPaintItem> outElements,
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double remateMm,
            double alongMm,
            double runMm,
            bool showPrimary,
            bool interior)
        {
            remateMm = Atk60FaceRemateRules.CapMm((int)Math.Round(remateMm));

            if (remateMm <= 1d)
            {
                return;
            }

            if (runMm > 1d && remateMm > runMm)
            {
                remateMm = runMm;
            }

            alongMm = Atk60FaceRemateRules.PaintAlongMm(interior, alongMm, remateMm, runMm);
            if (!interior)
            {
                if (runMm > 1d && alongMm + remateMm > runMm + 1d)
                {
                    alongMm = runMm - remateMm;
                }

                if (alongMm < 0d)
                {
                    alongMm = 0d;
                }
            }

            outElements.AddRange(EndFiller.Build(wall, anchor, remateMm, alongMm, showPrimary, !showPrimary));
        }

        private static bool ChunkAbutsFaceRemate(FaceRemateSides sides, double chunkEndMm)
        {
            if (sides == null)
            {
                return false;
            }

            const double tolMm = 2d;
            if (sides.HasPos && sides.PosMm > 1d && Math.Abs(sides.PosAlongMm - chunkEndMm) <= tolMm)
            {
                return true;
            }

            if (sides.HasNeg && sides.NegMm > 1d && Math.Abs(sides.NegAlongMm - chunkEndMm) <= tolMm)
            {
                return true;
            }

            return false;
        }

        private static double FaceRemateLeadMm(FaceRemateSides sides, double wallRunMm)
        {
            if (sides == null)
            {
                return 0d;
            }

            var lead = 0d;
            if (sides.HasPos
                && !sides.PosInterior
                && Atk60FaceRemateRules.AtStart(sides.PosAlongMm, sides.PosMm, wallRunMm))
            {
                lead = Math.Max(lead, sides.PosMm);
            }

            if (sides.HasNeg
                && !sides.NegInterior
                && Atk60FaceRemateRules.AtStart(sides.NegAlongMm, sides.NegMm, wallRunMm))
            {
                lead = Math.Max(lead, sides.NegMm);
            }

            return lead;
        }

        private static double FaceRemateTrailMm(FaceRemateSides sides, double wallRunMm)
        {
            if (sides == null)
            {
                return 0d;
            }

            var trail = 0d;
            if (sides.HasPos
                && !sides.PosInterior
                && Atk60FaceRemateRules.AtEnd(sides.PosAlongMm, sides.PosMm, wallRunMm))
            {
                trail = Math.Max(trail, sides.PosMm);
            }

            if (sides.HasNeg
                && !sides.NegInterior
                && Atk60FaceRemateRules.AtEnd(sides.NegAlongMm, sides.NegMm, wallRunMm))
            {
                trail = Math.Max(trail, sides.NegMm);
            }

            return trail;
        }

        private static double RemateStartAlongMm(Atk60ResolvedWallGeom geom, CornerFaceSlot slot)
        {
            if (geom == null || slot == null
                || !geom.StartX.HasValue || !geom.StartZ.HasValue
                || !geom.EndX.HasValue || !geom.EndZ.HasValue)
            {
                return 0d;
            }

            var sx = geom.StartX.Value;
            var sz = geom.StartZ.Value;
            var dx = geom.EndX.Value - sx;
            var dz = geom.EndZ.Value - sz;
            var len = Math.Sqrt((dx * dx) + (dz * dz));
            if (len < 1d)
            {
                return 0d;
            }

            var ux = dx / len;
            var uz = dz / len;
            var along1 = ((slot.X1 - sx) * ux) + ((slot.Z1 - sz) * uz);
            var along2 = ((slot.X2 - sx) * ux) + ((slot.Z2 - sz) * uz);
            return Atk60FaceRemateRules.StartAlongMm(
                len, along1, along2, slot.MeetAt1, slot.MeetAt2, slot.RemateMm);
        }

        private static string WallFormworkKey(Desing2FormworkWallDto wall)
        {
            if (wall == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(wall.WallId))
            {
                return wall.WallId;
            }

            return !string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId;
        }

        private static JToken CornerFacesToken(AttributesList attrs)
        {
            if (attrs == null || attrs.Extra == null)
            {
                return null;
            }

            foreach (var pair in attrs.Extra)
            {
                if (string.Equals(pair.Key, "CornerFaces", StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        private static double? TokenDouble(JObject item, string name)
        {
            if (item == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var token = item.GetValue(name, StringComparison.OrdinalIgnoreCase);
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            double value;
            if (double.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return null;
        }

        private static void MarkInteriorFaces(List<CornerFaceSlot> slots)
        {
            if (slots == null || slots.Count == 0)
            {
                return;
            }

            for (var i = 0; i < slots.Count; i++)
            {
                var key = slots[i].WallKey;
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                var minLen = slots[i].LengthMm;
                var maxLen = minLen;
                var count = 0;
                for (var j = 0; j < slots.Count; j++)
                {
                    if (!string.Equals(slots[j].WallKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    count++;
                    if (slots[j].LengthMm < minLen)
                    {
                        minLen = slots[j].LengthMm;
                    }

                    if (slots[j].LengthMm > maxLen)
                    {
                        maxLen = slots[j].LengthMm;
                    }
                }

                if (count < 2 || maxLen - minLen <= 20d)
                {
                    continue;
                }

                for (var j = 0; j < slots.Count; j++)
                {
                    if (!string.Equals(slots[j].WallKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    slots[j].IsInterior = slots[j].LengthMm <= minLen + 20d;
                }
            }
        }

        private static void MarkCornerMeetings(List<CornerFaceSlot> slots)
        {
            for (var a = 0; a < slots.Count; a++)
            {
                for (var b = a + 1; b < slots.Count; b++)
                {
                    if (string.Equals(slots[a].WallKey, slots[b].WallKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    MarkIfFacesMeet(slots[a], slots[b]);
                }
            }
        }

        private static void MarkIfFacesMeet(CornerFaceSlot a, CornerFaceSlot b)
        {
            if (CornerPointNear(a.X1, a.Z1, b.X1, b.Z1))
            {
                a.MeetAt1 = true;
                b.MeetAt1 = true;
            }

            if (CornerPointNear(a.X1, a.Z1, b.X2, b.Z2))
            {
                a.MeetAt1 = true;
                b.MeetAt2 = true;
            }

            if (CornerPointNear(a.X2, a.Z2, b.X1, b.Z1))
            {
                a.MeetAt2 = true;
                b.MeetAt1 = true;
            }

            if (CornerPointNear(a.X2, a.Z2, b.X2, b.Z2))
            {
                a.MeetAt2 = true;
                b.MeetAt2 = true;
            }
        }

        private static bool CornerPointNear(double ax, double az, double bx, double bz)
        {
            var dx = ax - bx;
            var dz = az - bz;
            return (dx * dx) + (dz * dz) <= 40d * 40d;
        }

        private sealed class FaceRemateSides
        {
            public bool HasPos;
            public bool HasNeg;
            public double PosMm;
            public double NegMm;
            public double PosAlongMm;
            public double NegAlongMm;
            public bool PosInterior;
            public bool NegInterior;
        }

        private sealed class CornerFaceSlot
        {
            public string WallKey;
            public int Side;
            public double X1;
            public double Z1;
            public double X2;
            public double Z2;
            public double LengthMm;
            public int RemateMm;
            public bool MeetAt1;
            public bool MeetAt2;
            public bool IsInterior;
        }
    }
}
