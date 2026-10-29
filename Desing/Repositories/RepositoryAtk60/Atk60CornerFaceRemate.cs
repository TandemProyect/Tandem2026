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
        /// Cada cara tiene su remate. En la esquina se suma con el de la cara
        /// del muro que llega al mismo punto, y ese taco se pinta una sola vez.
        /// </summary>
        private static Dictionary<string, FaceRemateSides> JoinCornerFaceRemates(List<Desing2FormworkWallDto> walls)
        {
            var result = new Dictionary<string, FaceRemateSides>(StringComparer.OrdinalIgnoreCase);
            var slots = new List<CornerFaceSlot>();
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

                for (var fi = 0; fi < faces.Count; fi++)
                {
                    var item = faces[fi] as JObject;
                    if (item == null)
                    {
                        continue;
                    }

                    var length = TokenDouble(item, "lengthMm");
                    if (!length.HasValue || length.Value <= 1d)
                    {
                        continue;
                    }

                    int n270;
                    int remateMm;
                    PackAtk60LengthMm((int)Math.Round(length.Value), out n270, out remateMm);
                    var side = TokenDouble(item, "side");
                    slots.Add(new CornerFaceSlot
                    {
                        WallKey = key,
                        Side = side.HasValue && side.Value < 0d ? -1 : 1,
                        X1 = TokenDouble(item, "x1") ?? 0d,
                        Z1 = TokenDouble(item, "z1") ?? 0d,
                        X2 = TokenDouble(item, "x2") ?? 0d,
                        Z2 = TokenDouble(item, "z2") ?? 0d,
                        RemateMm = remateMm,
                        Parent = slots.Count
                    });
                }
            }

            if (slots.Count == 0)
            {
                return result;
            }

            for (var a = 0; a < slots.Count; a++)
            {
                for (var b = a + 1; b < slots.Count; b++)
                {
                    if (string.Equals(slots[a].WallKey, slots[b].WallKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!CornerFacesMeet(slots[a], slots[b]))
                    {
                        continue;
                    }

                    UnionCornerFace(slots, a, b);
                }
            }

            var groups = new Dictionary<int, List<int>>();
            for (var i = 0; i < slots.Count; i++)
            {
                var root = FindCornerFace(slots, i);
                List<int> members;
                if (!groups.TryGetValue(root, out members))
                {
                    members = new List<int>();
                    groups[root] = members;
                }

                members.Add(i);
            }

            foreach (var members in groups.Values)
            {
                var owner = members[0];
                var sum = 0;
                for (var gi = 0; gi < members.Count; gi++)
                {
                    var idx = members[gi];
                    sum += slots[idx].RemateMm;
                    if (CompareCornerFaceOwner(slots[idx], slots[owner]) < 0)
                    {
                        owner = idx;
                    }
                }

                for (var gi = 0; gi < members.Count; gi++)
                {
                    var idx = members[gi];
                    var slot = slots[idx];
                    FaceRemateSides sides;
                    if (!result.TryGetValue(slot.WallKey, out sides))
                    {
                        sides = new FaceRemateSides();
                        result[slot.WallKey] = sides;
                    }

                    var mm = idx == owner ? sum : 0d;
                    if (slot.Side < 0)
                    {
                        sides.HasNeg = true;
                        sides.NegMm = mm;
                    }
                    else
                    {
                        sides.HasPos = true;
                        sides.PosMm = mm;
                    }
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

            // El hueco del eje está en las dos caras. El tablón y las grapas regulables van en las dos.
            if (axisRemateMm > 1d)
            {
                outElements.AddRange(EndFiller.Build(wall, anchor, axisRemateMm, startAlongMm));
                return;
            }

            var anchorSign = anchor.FaceSign >= 0d ? 1 : -1;
            var primaryMm = CornerFaceMm(sides, anchorSign > 0, axisRemateMm);
            var oppositeMm = CornerFaceMm(sides, anchorSign < 0, axisRemateMm);
            if (primaryMm > 150d)
            {
                primaryMm = 150d;
            }

            if (oppositeMm > 150d)
            {
                oppositeMm = 150d;
            }

            if (primaryMm > 1d && oppositeMm > 1d && Math.Abs(primaryMm - oppositeMm) < 0.5d)
            {
                outElements.AddRange(EndFiller.Build(wall, anchor, primaryMm, startAlongMm));
                return;
            }

            if (primaryMm > 1d && oppositeMm > 1d)
            {
                outElements.AddRange(EndFiller.Build(wall, anchor, primaryMm, startAlongMm, true, false));
                outElements.AddRange(EndFiller.Build(wall, anchor, oppositeMm, startAlongMm, false, true));
                return;
            }

            if (primaryMm > 1d)
            {
                outElements.AddRange(EndFiller.Build(wall, anchor, primaryMm, startAlongMm));
            }
            else if (oppositeMm > 1d)
            {
                outElements.AddRange(EndFiller.Build(wall, anchor, oppositeMm, startAlongMm));
            }
        }

        private static double CornerFaceMm(FaceRemateSides sides, bool positiveSide, double axisRemateMm)
        {
            if (positiveSide)
            {
                return sides.HasPos ? sides.PosMm : axisRemateMm;
            }

            return sides.HasNeg ? sides.NegMm : axisRemateMm;
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

        private static bool CornerFacesMeet(CornerFaceSlot a, CornerFaceSlot b)
        {
            return CornerPointNear(a.X1, a.Z1, b.X1, b.Z1)
                || CornerPointNear(a.X1, a.Z1, b.X2, b.Z2)
                || CornerPointNear(a.X2, a.Z2, b.X1, b.Z1)
                || CornerPointNear(a.X2, a.Z2, b.X2, b.Z2);
        }

        private static bool CornerPointNear(double ax, double az, double bx, double bz)
        {
            var dx = ax - bx;
            var dz = az - bz;
            return (dx * dx) + (dz * dz) <= 40d * 40d;
        }

        private static int FindCornerFace(List<CornerFaceSlot> slots, int index)
        {
            var parent = slots[index].Parent;
            if (parent != index)
            {
                slots[index].Parent = FindCornerFace(slots, parent);
            }

            return slots[index].Parent;
        }

        private static void UnionCornerFace(List<CornerFaceSlot> slots, int a, int b)
        {
            var rootA = FindCornerFace(slots, a);
            var rootB = FindCornerFace(slots, b);
            if (rootA != rootB)
            {
                slots[rootB].Parent = rootA;
            }
        }

        private static int CompareCornerFaceOwner(CornerFaceSlot a, CornerFaceSlot b)
        {
            var wall = string.Compare(a.WallKey, b.WallKey, StringComparison.OrdinalIgnoreCase);
            if (wall != 0)
            {
                return wall;
            }

            return a.Side.CompareTo(b.Side);
        }

        private sealed class FaceRemateSides
        {
            public bool HasPos;
            public bool HasNeg;
            public double PosMm;
            public double NegMm;
        }

        private sealed class CornerFaceSlot
        {
            public string WallKey;
            public int Side;
            public double X1;
            public double Z1;
            public double X2;
            public double Z2;
            public int RemateMm;
            public int Parent;
        }
    }
}
