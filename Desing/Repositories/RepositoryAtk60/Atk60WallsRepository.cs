using Desing.Models;
using Desing.Repositories.RepositoryAtk60.ModulosATK60;
using Desing.Repositories.RepositoryCommun;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Desing.Repositories.RepositoryAtk60
{
    public sealed class Atk60WallsRepository
    {
        private readonly FormworkJsonCommonRepository _common;
        private readonly List<IModuloAtk60ElementBuilder> _moduleBuilders;

        private static readonly int[] Atk60FillModuleMm = { 2400, 1200, 900, 750, 600, 450, 300 };
        private const int Atk60BaseModuleMm = 2700;
        private const int Atk60MaxRemateMm = 149;

        public Atk60WallsRepository(FormworkJsonCommonRepository common)
        {
            _common = common;
            _moduleBuilders = ModuloAtk60ElementBuilderCatalog.CreateDefault();
        }

        public Desing2FormworkRequest BuildPayloadFromIdsJson(string idsJson)
        {
            var walls = _common.ParseAndNormalizeWalls(idsJson);

            return new Desing2FormworkRequest
            {
                System = "Atk-60",
                Walls = walls ?? new List<Desing2FormworkWallDto>(),
                List = walls ?? new List<Desing2FormworkWallDto>(),
            };
        }

        // Base funcional documentada en:
        // Docs/Proyectos/Desing/ATK60-Logica-Modulos-Wall.md
        public List<ModulosAtk60Wall> GetWallsForCadSystems(List<Desing2FormworkWallDto> walls)
        {
            var modulos = new List<ModulosAtk60Wall>();
            if (walls == null || walls.Count == 0)
            {
                return modulos;
            }

            // Normas ATK60 para CUALQUIER longitud de muro:
            // 1) Catalogo: 2,70  2,40  1,20  0,90  0,75  0,60  0,45  0,30.
            // 2) Remate de madera estrictamente < 0,15 m (max 149 mm).
            // 3) Se combinan esos modulos hasta que el resto cumpla (2); no se acepta
            //    un greedy que deje 0,15..0,29 (p.ej. 1,20 sobre 1,45 deja 0,25).
            // 4) Si hace falta, se cambia un 2,70 por modulacion menor.
            // 5) Si hay remate, va SIEMPRE el penultimo; el muro acaba en un modulo.
            const int epsMm = 1;

            foreach (var item in walls)
            {
                if (item == null)
                {
                    continue;
                }

                var idWall = !string.IsNullOrWhiteSpace(item.WallId)
                    ? item.WallId
                    : (!string.IsNullOrWhiteSpace(item.Id) ? item.Id : item.LineId);

                var longM = item.Attributes != null && item.Attributes._Datalong.HasValue
                    ? Math.Max(0, item.Attributes._Datalong.Value)
                    : 0;

                var module = new ModulosAtk60Wall
                {
                    IdWall = idWall
                };

                var longMm = (int)Math.Round(longM * 1000d);
                if (longMm <= epsMm)
                {
                    modulos.Add(module);
                    continue;
                }

                int n270;
                int remateMm;
                var fillCounts = PackAtk60LengthMm(longMm, out n270, out remateMm);
                AddModuleCount(module, 2.70, n270);
                for (var i = 0; i < Atk60FillModuleMm.Length; i++)
                {
                    AddModuleCount(module, Atk60FillModuleMm[i] / 1000.0, fillCounts[i]);
                }

                module.M_Remate = Math.Round(remateMm / 1000.0, 3);
                modulos.Add(module);
            }

            return modulos;
        }

        public Atk60ThreeJsPaintPayload BuildThreeJsPaintPayload(
            List<Desing2FormworkWallDto> walls,
            List<ModulosAtk60Wall> modulos)
        {
            var payload = new Atk60ThreeJsPaintPayload();
            if (walls == null || walls.Count == 0)
            {
                return payload;
            }

            var resolvedById = new Dictionary<string, Atk60ResolvedWallGeom>(StringComparer.OrdinalIgnoreCase);
            var centroidByGroup = new Dictionary<string, Atk60CentroidXz>(StringComparer.OrdinalIgnoreCase);
            var centroid = ComputeWallsCentroidXz(walls, resolvedById, centroidByGroup);

            foreach (var wall in walls)
            {
                if (wall == null)
                {
                    continue;
                }

                var attrs = wall.Attributes;
                var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);

                Atk60ResolvedWallGeom geom;
                if (!resolvedById.TryGetValue(idWall ?? string.Empty, out geom) || geom == null)
                {
                    geom = ResolveWallGeom(wall);
                }

                var fallbackX = attrs != null
                    ? (attrs._XCoordinate ?? attrs.ExtraValueAsDouble("_XCoordinate"))
                    : null;
                var fallbackY = attrs != null
                    ? (attrs._YCoordinate ?? attrs.ExtraValueAsDouble("_YCoordinate"))
                    : null;
                var fallbackZ = attrs != null
                    ? (attrs._ZCoordinate ?? attrs.ExtraValueAsDouble("_ZCoordinate"))
                    : null;

                var startX = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioX") : null, fallbackX);
                // Convencion visor Desing_2: InicioZ -> eje Y (vertical), InicioY -> eje Z (planta).
                var startY = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioZ") : null, fallbackY);
                var startZ = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioY") : null, fallbackZ);

                var endX = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("FinX") : null, null);
                var endZ = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("FinY") : null, null);

                if (geom != null)
                {
                    if (geom.StartX.HasValue) startX = geom.StartX;
                    if (geom.StartY.HasValue) startY = geom.StartY;
                    if (geom.StartZ.HasValue) startZ = geom.StartZ;
                    if (geom.EndX.HasValue) endX = geom.EndX;
                    if (geom.EndZ.HasValue) endZ = geom.EndZ;
                }

                var endY = geom != null ? geom.EndY : null;

                var sx = startX.HasValue ? startX.Value : ResolveDouble(fallbackX);
                var sz = startZ.HasValue ? startZ.Value : ResolveDouble(fallbackZ);
                var sy = startY.HasValue ? startY.Value : ResolveDouble(fallbackY);
                var ex = endX.HasValue ? endX.Value : sx;
                var ez = endZ.HasValue ? endZ.Value : sz;

                var hasSegment = startX.HasValue && startZ.HasValue && endX.HasValue && endZ.HasValue;
                var segDx = ex - sx;
                var segDz = ez - sz;
                var segLen = Math.Sqrt(segDx * segDx + segDz * segDz);
                if (!hasSegment || segLen < 1e-6)
                {
                    var centerXFallback = ResolveDouble(fallbackX);
                    var centerZFallback = ResolveDouble(fallbackZ);
                    var yawAttr = ResolveWallYawRad(attrs);
                    var lengthMmFallback = ResolveWallLengthMm(attrs, startX, startZ, endX, endZ);

                    if (yawAttr.HasValue && lengthMmFallback > 1e-3)
                    {
                        var uxFallback = Math.Cos(yawAttr.Value);
                        var uzFallback = Math.Sin(yawAttr.Value);
                        var half = lengthMmFallback * 0.5;

                        sx = centerXFallback - uxFallback * half;
                        sz = centerZFallback - uzFallback * half;
                        ex = centerXFallback + uxFallback * half;
                        ez = centerZFallback + uzFallback * half;

                        hasSegment = true;
                        segDx = ex - sx;
                        segDz = ez - sz;
                        segLen = Math.Sqrt(segDx * segDx + segDz * segDz);
                    }
                }

                CanonicalizeClockwiseWallAxis(ref sx, ref sz, ref ex, ref ez);
                segDx = ex - sx;
                segDz = ez - sz;
                segLen = Math.Sqrt(segDx * segDx + segDz * segDz);

                var dx = segLen > 1e-6 ? segDx : 1d;
                var dz = segLen > 1e-6 ? segDz : 0d;
                var len = segLen > 1e-6 ? segLen : 1d;

                var ux = dx / len;
                var uz = dz / len;
                var nx = -uz;
                var nz = ux;

                if (geom == null)
                {
                    geom = new Atk60ResolvedWallGeom();
                }
                geom.StartX = sx;
                geom.StartZ = sz;
                geom.EndX = ex;
                geom.EndZ = ez;

                var widthMm = ResolveWallWidthMm(attrs);
                var centroidForFace = centroid;
                var groupId = !string.IsNullOrWhiteSpace(wall.WallGroupId) ? wall.WallGroupId : null;
                Atk60CentroidXz groupCentroid;
                if (!string.IsNullOrWhiteSpace(groupId) && centroidByGroup.TryGetValue(groupId, out groupCentroid) && groupCentroid != null)
                {
                    centroidForFace = groupCentroid;
                }

                var faceSign = ResolveFaceSign(attrs, centroidForFace, geom);
                var thicknessHalfGlobal = Math.Max(widthMm, 1d) * 0.5;

                var hasExplicitStart = startX.HasValue && startZ.HasValue;
                var insertX = sx;
                var insertZ = sz;

                if (hasExplicitStart)
                {
                    // Inicio/p1 ahora viene en eje del muro (3D): mover +/- E/2 a cara exterior.
                    insertX = sx + (nx * faceSign * thicknessHalfGlobal);
                    insertZ = sz + (nz * faceSign * thicknessHalfGlobal);
                }
                else
                {
                    // Fallback solo cuando no llega Inicio/p1:
                    // centro + angulo -> mover L/2 en eje, luego E/2 en normal exterior.
                    var centerX = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("CenterX") : null, fallbackX);
                    var centerY = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("CenterZ") : null, fallbackY);
                    var centerZ = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("CenterY") : null, fallbackZ);
                    if (!centerX.HasValue) centerX = (sx + ex) * 0.5;
                    if (!centerY.HasValue) centerY = sy;
                    if (!centerZ.HasValue) centerZ = (sz + ez) * 0.5;

                    var lengthMm = ResolveWallLengthMm(attrs, startX, startZ, endX, endZ);
                    if (lengthMm <= 1e-6)
                    {
                        lengthMm = len;
                    }

                    var axisHalf = Math.Max(lengthMm, 0d) * 0.5;
                    var thicknessHalf = thicknessHalfGlobal;

                    var baseX = centerX.Value - (ux * axisHalf);
                    var baseZ = centerZ.Value - (uz * axisHalf);

                    insertX = baseX + (nx * faceSign * thicknessHalf);
                    insertZ = baseZ + (nz * faceSign * thicknessHalf);
                }
                var yawRad = Math.Atan2(dz, dx);
                var baseY = (startY.HasValue && endY.HasValue)
                    ? Math.Min(startY.Value, endY.Value)
                    : sy;

                var anchor = new Atk60WallPaintAnchor
                {
                    IdWall = idWall,
                    X = insertX,
                    Y = baseY,
                    Z = insertZ,
                    RotX = ResolveDouble(attrs != null ? attrs._XRotation : null),
                    RotY = yawRad,
                    RotZ = ResolveDouble(attrs != null ? attrs._ZRotation : null),
                    NormalX = nx * faceSign,
                    NormalZ = nz * faceSign,
                    FaceSign = faceSign,
                    Debug = new Atk60WallPaintAnchorDebug
                    {
                        StartX = sx,
                        StartZ = sz,
                        EndX = ex,
                        EndZ = ez,
                        // Base previo a offset normal (centro - L/2)
                        InsertX = insertX,
                        InsertZ = insertZ,
                        YawRad = yawRad,
                        FaceSign = faceSign,
                        WidthMm = widthMm,
                    },
                };

                payload.Walls.Add(anchor);
            }
            payload.Elements = BuildPanel270ElementsForThreeJs(walls, modulos, payload.Walls);

            return payload;
        }

        private List<Atk60ElementPaintItem> BuildPanel270ElementsForThreeJs(
            List<Desing2FormworkWallDto> walls,
            List<ModulosAtk60Wall> modulos,
            List<Atk60WallPaintAnchor> anchors)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (walls == null || walls.Count == 0 || modulos == null || modulos.Count == 0 || anchors == null || anchors.Count == 0)
            {
                return outElements;
            }

            var wallById = walls
                .Where(w => w != null)
                .Select(w => new
                {
                    Key = !string.IsNullOrWhiteSpace(w.WallId)
                        ? w.WallId
                        : (!string.IsNullOrWhiteSpace(w.Id) ? w.Id : w.LineId),
                    Wall = w
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Wall, StringComparer.OrdinalIgnoreCase);

            var anchorById = anchors
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.IdWall))
                .GroupBy(a => a.IdWall, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var moduloWall in modulos)
            {
                if (moduloWall == null || string.IsNullOrWhiteSpace(moduloWall.IdWall))
                {
                    continue;
                }

                Atk60WallPaintAnchor anchor;
                if (!anchorById.TryGetValue(moduloWall.IdWall, out anchor) || anchor == null)
                {
                    continue;
                }

                Desing2FormworkWallDto wall;
                if (!wallById.TryGetValue(moduloWall.IdWall, out wall) || wall == null)
                {
                    continue;
                }

                var totalModules = 0;
                for (var bi = 0; bi < _moduleBuilders.Count; bi++)
                {
                    totalModules += (int)_moduleBuilders[bi].GetCount(moduloWall);
                }

                var remateMm = Math.Round(moduloWall.M_Remate * 1000d, 3);
                var chunks = BuildAtk60AlongChunks(moduloWall);
                var cursorMm = 0d;
                var indexOffset = 0;
                for (var ci = 0; ci < chunks.Count; ci++)
                {
                    var chunk = chunks[ci];
                    if (chunk.Builder == null)
                    {
                        outElements.AddRange(
                            Modulo270PanelElementGenerator.BuildRemate(wall, anchor, remateMm, cursorMm));
                        cursorMm += remateMm;
                        continue;
                    }

                    outElements.AddRange(
                        Modulo270PanelElementGenerator.Build(
                            wall,
                            anchor,
                            chunk.Count,
                            chunk.Builder.ModuleLengthMm,
                            cursorMm,
                            indexOffset,
                            totalModules > 0 ? totalModules : chunk.Count));

                    cursorMm += chunk.Count * (double)chunk.Builder.ModuleLengthMm;
                    indexOffset += chunk.Count;
                }
            }

            return outElements;
        }

        private static Atk60ResolvedWallGeom ResolveWallGeom(Desing2FormworkWallDto wall)
        {
            if (wall == null)
            {
                return null;
            }

            var attrs = wall.Attributes;
            var p1 = ResolvePointFromExtra(attrs, "p1")
                ?? ResolvePointFromWallDtoExtra(wall, "P1")
                ?? ResolvePointFromWallDtoExtra(wall, "p1");
            var p2 = ResolvePointFromExtra(attrs, "p2")
                ?? ResolvePointFromWallDtoExtra(wall, "P2")
                ?? ResolvePointFromWallDtoExtra(wall, "p2");

            var startX = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioX") : null, null);
            var startY = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioZ") : null, null);
            var startZ = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("InicioY") : null, null);
            var endX = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("FinX") : null, null);
            var endY = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("FinZ") : null, null);
            var endZ = ResolveSceneMm(attrs != null ? attrs.ExtraValueAsDouble("FinY") : null, null);

            if (p1 != null)
            {
                startX = p1.X;
                startY = p1.Y;
                startZ = p1.Z;
            }
            if (p2 != null)
            {
                endX = p2.X;
                endY = p2.Y;
                endZ = p2.Z;
            }

            if (startX.HasValue && startZ.HasValue && endX.HasValue && endZ.HasValue)
            {
                var sxCanon = startX.Value;
                var szCanon = startZ.Value;
                var exCanon = endX.Value;
                var ezCanon = endZ.Value;
                CanonicalizeClockwiseWallAxis(ref sxCanon, ref szCanon, ref exCanon, ref ezCanon);
                startX = sxCanon;
                startZ = szCanon;
                endX = exCanon;
                endZ = ezCanon;
            }

            var anchorX = startX;
            var anchorY = startY;
            var anchorZ = startZ;

            // Esquina inferior-izquierda del tramo en XZ (misma heuristica usada en JS).
            if (p1 != null && p2 != null)
            {
                var pickP1 = false;
                if (Math.Abs(p1.Z.Value - p2.Z.Value) > 1e-6)
                {
                    pickP1 = p1.Z.Value < p2.Z.Value;
                }
                else if (Math.Abs(p1.X.Value - p2.X.Value) > 1e-6)
                {
                    pickP1 = p1.X.Value < p2.X.Value;
                }

                var a = pickP1 ? p1 : p2;
                anchorX = a.X;
                anchorY = a.Y;
                anchorZ = a.Z;
            }

            return new Atk60ResolvedWallGeom
            {
                StartX = startX,
                StartY = startY,
                StartZ = startZ,
                EndX = endX,
                EndY = endY,
                EndZ = endZ,
                AnchorX = anchorX,
                AnchorY = anchorY,
                AnchorZ = anchorZ,
            };
        }

        private static Atk60CentroidXz ComputeWallsCentroidXz(
            List<Desing2FormworkWallDto> walls,
            Dictionary<string, Atk60ResolvedWallGeom> outResolved,
            Dictionary<string, Atk60CentroidXz> outGroupCentroids)
        {
            if (walls == null || walls.Count == 0)
            {
                return null;
            }

            var sx = 0d;
            var sz = 0d;
            var count = 0;
            var groupAccum = new Dictionary<string, Atk60CentroidAccum>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                if (wall == null)
                {
                    continue;
                }

                var idWall = !string.IsNullOrWhiteSpace(wall.WallId)
                    ? wall.WallId
                    : (!string.IsNullOrWhiteSpace(wall.Id) ? wall.Id : wall.LineId);
                var geom = ResolveWallGeom(wall);

                if (!string.IsNullOrWhiteSpace(idWall))
                {
                    outResolved[idWall] = geom;
                }

                if (geom == null || !geom.StartX.HasValue || !geom.StartZ.HasValue || !geom.EndX.HasValue || !geom.EndZ.HasValue)
                {
                    continue;
                }

                var cx = (geom.StartX.Value + geom.EndX.Value) * 0.5;
                var cz = (geom.StartZ.Value + geom.EndZ.Value) * 0.5;

                sx += cx;
                sz += cz;
                count++;

                var groupId = !string.IsNullOrWhiteSpace(wall.WallGroupId) ? wall.WallGroupId : null;
                if (!string.IsNullOrWhiteSpace(groupId))
                {
                    Atk60CentroidAccum accum;
                    if (!groupAccum.TryGetValue(groupId, out accum) || accum == null)
                    {
                        accum = new Atk60CentroidAccum();
                        groupAccum[groupId] = accum;
                    }

                    accum.Sx += cx;
                    accum.Sz += cz;
                    accum.Count++;
                }
            }

            if (outGroupCentroids != null)
            {
                foreach (var kv in groupAccum)
                {
                    if (kv.Value == null || kv.Value.Count <= 0)
                    {
                        continue;
                    }

                    outGroupCentroids[kv.Key] = new Atk60CentroidXz
                    {
                        X = kv.Value.Sx / kv.Value.Count,
                        Z = kv.Value.Sz / kv.Value.Count,
                    };
                }
            }

            if (count <= 0)
            {
                return null;
            }

            return new Atk60CentroidXz
            {
                X = sx / count,
                Z = sz / count,
            };
        }

        private static Atk60Point ResolvePointFromExtra(AttributesList attrs, string key)
        {
            if (attrs == null || attrs.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!attrs.Extra.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                var match = attrs.Extra.FirstOrDefault(kv =>
                    string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
                token = match.Value;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            var obj = token as JObject;
            if (obj == null)
            {
                return null;
            }

            var x = ResolveSceneMm(GetTokenNumber(obj, "xMm") ?? GetTokenNumber(obj, "x"), null);
            var y = ResolveSceneMm(GetTokenNumber(obj, "yMm") ?? GetTokenNumber(obj, "y"), null);
            var z = ResolveSceneMm(GetTokenNumber(obj, "zMm") ?? GetTokenNumber(obj, "z"), null);

            if (!x.HasValue || !y.HasValue || !z.HasValue)
            {
                return null;
            }

            return new Atk60Point
            {
                X = x,
                Y = y,
                Z = z,
            };
        }

        private static Atk60Point ResolvePointFromWallDtoExtra(Desing2FormworkWallDto wall, string key)
        {
            if (wall == null || wall.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!wall.Extra.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                var match = wall.Extra.FirstOrDefault(kv =>
                    string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
                token = match.Value;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            var obj = token as JObject;
            if (obj == null)
            {
                return null;
            }

            var x = ResolveSceneMm(GetTokenNumber(obj, "xMm") ?? GetTokenNumber(obj, "x"), null);
            var y = ResolveSceneMm(GetTokenNumber(obj, "yMm") ?? GetTokenNumber(obj, "y"), null);
            var z = ResolveSceneMm(GetTokenNumber(obj, "zMm") ?? GetTokenNumber(obj, "z"), null);

            if (!x.HasValue || !y.HasValue || !z.HasValue)
            {
                return null;
            }

            return new Atk60Point
            {
                X = x,
                Y = y,
                Z = z,
            };
        }

        private static double? GetTokenNumber(JObject obj, string key)
        {
            if (obj == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!obj.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                var match = obj.Properties().FirstOrDefault(p =>
                    string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
                token = match != null ? match.Value : null;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<double>();
            }

            double v;
            var s = token.ToString();
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.GetCultureInfo("es-ES"), out v))
            {
                return v;
            }
            s = s.Replace(',', '.');
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }

            return null;
        }

        public List<Atk60ElementPaintItem> CreateElementsByModulo(
            List<ModulosAtk60Wall> modulos,
            List<Atk60WallPaintAnchor> wallAnchors,
            string importPath)
        {
            var outElements = new List<Atk60ElementPaintItem>();
            if (modulos == null || modulos.Count == 0 || wallAnchors == null || wallAnchors.Count == 0)
            {
                return outElements;
            }

            var anchorsByWall = wallAnchors
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.IdWall))
                .GroupBy(x => x.IdWall, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var moduloWall in modulos)
            {
                if (moduloWall == null || string.IsNullOrWhiteSpace(moduloWall.IdWall))
                {
                    continue;
                }

                Atk60WallPaintAnchor anchor;
                if (!anchorsByWall.TryGetValue(moduloWall.IdWall, out anchor) || anchor == null)
                {
                    continue;
                }

                var yawRad = NormalizeYawToRad(anchor.RotY);
                var ux = Math.Cos(yawRad);
                var uz = Math.Sin(yawRad);
                var cursorMm = 0d;
                var remateMm = Math.Round(moduloWall.M_Remate * 1000d, 3);
                var chunks = BuildAtk60AlongChunks(moduloWall);

                for (var ci = 0; ci < chunks.Count; ci++)
                {
                    var chunk = chunks[ci];
                    if (chunk.Builder == null)
                    {
                        var remateX = anchor.X + (ux * cursorMm);
                        var remateZ = anchor.Z + (uz * cursorMm);
                        outElements.Add(new Atk60ElementPaintItem
                        {
                            IdWall = moduloWall.IdWall,
                            ElementCode = "REMATE_WOOD",
                            ImportPath = string.Empty,
                            Color = "wood-remate",
                            X = remateX,
                            Y = anchor.Y,
                            Z = remateZ,
                            RotX = anchor.RotX,
                            RotY = anchor.RotY,
                            RotZ = anchor.RotZ,
                        });
                        cursorMm += remateMm;
                        continue;
                    }

                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var posX = anchor.X + (ux * cursorMm);
                        var posZ = anchor.Z + (uz * cursorMm);

                        outElements.Add(new Atk60ElementPaintItem
                        {
                            IdWall = moduloWall.IdWall,
                            ElementCode = chunk.Builder.ModuleCode + "_FRAME",
                            ImportPath = importPath,
                            Color = "frame-yellow",
                            X = posX,
                            Y = anchor.Y,
                            Z = posZ,
                            RotX = anchor.RotX,
                            RotY = anchor.RotY,
                            RotZ = anchor.RotZ,
                        });

                        outElements.Add(new Atk60ElementPaintItem
                        {
                            IdWall = moduloWall.IdWall,
                            ElementCode = chunk.Builder.ModuleCode + "_PHENOLIC",
                            ImportPath = importPath,
                            Color = "phenolic-dark",
                            X = posX,
                            Y = anchor.Y,
                            Z = posZ,
                            RotX = anchor.RotX,
                            RotY = anchor.RotY,
                            RotZ = anchor.RotZ,
                        });

                        cursorMm += chunk.Builder.ModuleLengthMm;
                    }
                }
            }

            return outElements;
        }

        private sealed class Atk60AlongChunk
        {
            public IModuloAtk60ElementBuilder Builder;
            public int Count;
        }

        /// <summary>
        /// Orden a lo largo del muro: modulos, remate (si hay) y el modulo de cierre.
        /// El remate nunca es el ultimo.
        /// </summary>
        private List<Atk60AlongChunk> BuildAtk60AlongChunks(ModulosAtk60Wall moduloWall)
        {
            var chunks = new List<Atk60AlongChunk>();
            if (moduloWall == null)
            {
                return chunks;
            }

            var remateMm = Math.Round(moduloWall.M_Remate * 1000d, 3);
            var hasRemate = remateMm > 1d && remateMm < 150d;
            var closer = hasRemate ? ResolveAtk60ClosingBuilder(moduloWall) : null;

            for (var bi = 0; bi < _moduleBuilders.Count; bi++)
            {
                var builder = _moduleBuilders[bi];
                var count = (int)builder.GetCount(moduloWall);
                if (closer != null && builder.ModuleLengthMm == closer.ModuleLengthMm)
                {
                    count--;
                }

                if (count > 0)
                {
                    chunks.Add(new Atk60AlongChunk
                    {
                        Builder = builder,
                        Count = count
                    });
                }
            }

            if (hasRemate)
            {
                chunks.Add(new Atk60AlongChunk
                {
                    Builder = null,
                    Count = 1
                });
            }

            if (closer != null)
            {
                chunks.Add(new Atk60AlongChunk
                {
                    Builder = closer,
                    Count = 1
                });
            }

            return chunks;
        }

        private IModuloAtk60ElementBuilder ResolveAtk60ClosingBuilder(ModulosAtk60Wall moduloWall)
        {
            IModuloAtk60ElementBuilder found450 = null;
            IModuloAtk60ElementBuilder found300 = null;
            IModuloAtk60ElementBuilder lastAny = null;
            for (var bi = 0; bi < _moduleBuilders.Count; bi++)
            {
                var builder = _moduleBuilders[bi];
                if (builder.GetCount(moduloWall) <= 0)
                {
                    continue;
                }

                lastAny = builder;
                if (builder.ModuleLengthMm == 450)
                {
                    found450 = builder;
                }
                else if (builder.ModuleLengthMm == 300)
                {
                    found300 = builder;
                }
            }

            return found450 ?? found300 ?? lastAny;
        }

        /// <summary>
        /// Despiece longitudinal para cualquier L: maximos 2,70 y relleno
        /// 2,40..0,30 de modo que el remate quede siempre en 0..149 mm.
        /// </summary>
        private static int[] PackAtk60LengthMm(int longMm, out int n270, out int remateMm)
        {
            n270 = longMm / Atk60BaseModuleMm;
            var remainingMm = longMm - (n270 * Atk60BaseModuleMm);
            var fillCounts = new int[Atk60FillModuleMm.Length];

            while (!TryFillAtk60RemainderMm(remainingMm, fillCounts, 0))
            {
                if (n270 <= 0)
                {
                    for (var i = 0; i < fillCounts.Length; i++)
                    {
                        fillCounts[i] = 0;
                    }

                    remateMm = remainingMm;
                    return fillCounts;
                }

                n270--;
                remainingMm += Atk60BaseModuleMm;
            }

            remateMm = remainingMm;
            for (var i = 0; i < fillCounts.Length; i++)
            {
                remateMm -= fillCounts[i] * Atk60FillModuleMm[i];
            }

            if (remateMm < 0)
            {
                remateMm = 0;
            }

            return fillCounts;
        }

        private static bool TryFillAtk60RemainderMm(int remainingMm, int[] counts, int index)
        {
            if (remainingMm <= Atk60MaxRemateMm)
            {
                for (var i = index; i < counts.Length; i++)
                {
                    counts[i] = 0;
                }

                return true;
            }

            if (index >= Atk60FillModuleMm.Length)
            {
                return false;
            }

            var sizeMm = Atk60FillModuleMm[index];
            var maxN = remainingMm / sizeMm;
            for (var n = maxN; n >= 0; n--)
            {
                counts[index] = n;
                if (TryFillAtk60RemainderMm(remainingMm - (n * sizeMm), counts, index + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddModuleCount(ModulosAtk60Wall module, double moduleValueM, long count)
        {
            if (module == null || count <= 0 || moduleValueM <= 0)
            {
                return;
            }

            var mm = (int)Math.Round(moduleValueM * 1000);
            switch (mm)
            {
                case 2700: module.M_270 += count; break;
                case 2550: module.M_255 += count; break;
                case 2400: module.M_240 += count; break;
                case 2250: module.M_225 += count; break;
                case 2100: module.M_210 += count; break;
                case 1950: module.M_195 += count; break;
                case 1800: module.M_180 += count; break;
                case 1650: module.M_165 += count; break;
                case 1500: module.M_150 += count; break;
                case 1350: module.M_135 += count; break;
                case 1200: module.M_120 += count; break;
                case 1050: module.M_105 += count; break;
                case 900: module.M_090 += count; break;
                case 750: module.M_075 += count; break;
                case 600: module.M_060 += count; break;
                case 450: module.M_045 += count; break;
                case 300: module.M_0430 += count; break;
            }
        }

        private static double ResolveDouble(double? value)
        {
            return value.HasValue ? value.Value : 0d;
        }

        private static double? ResolveSceneMm(double? preferred, double? fallback)
        {
            var v = preferred ?? fallback;
            if (!v.HasValue)
            {
                return null;
            }

            var abs = Math.Abs(v.Value);
            return abs <= 50d ? v.Value * 1000d : v.Value;
        }

        private static double ResolveWallWidthMm(AttributesList attrs)
        {
            if (attrs == null)
            {
                return 300d;
            }

            var withRaw = attrs._DataWith ?? attrs.ExtraValueAsDouble("_DataWith");
            var fromDataWith = ResolveSceneMm(withRaw, null);
            if (fromDataWith.HasValue && fromDataWith.Value > 1)
            {
                return Math.Abs(fromDataWith.Value);
            }

            var fromThickness = ResolveSceneMm(attrs.ExtraValueAsDouble("ThicknessMm"), null);
            if (fromThickness.HasValue && fromThickness.Value > 1)
            {
                return Math.Abs(fromThickness.Value);
            }

            return 300d;
        }

        private static double ResolveWallLengthMm(
            AttributesList attrs,
            double? startX,
            double? startZ,
            double? endX,
            double? endZ)
        {
            if (startX.HasValue && startZ.HasValue && endX.HasValue && endZ.HasValue)
            {
                var dx = endX.Value - startX.Value;
                var dz = endZ.Value - startZ.Value;
                var l = Math.Sqrt(dx * dx + dz * dz);
                if (l > 1e-6)
                {
                    return l;
                }
            }

            if (attrs == null)
            {
                return 0d;
            }

            var fromDataLong = ResolveSceneMm(attrs._Datalong ?? attrs.ExtraValueAsDouble("_Datalong"), null);
            if (fromDataLong.HasValue && fromDataLong.Value > 1)
            {
                return Math.Abs(fromDataLong.Value);
            }

            var fromLongitud = ResolveSceneMm(attrs.ExtraValueAsDouble("Longitud"), null);
            if (fromLongitud.HasValue && fromLongitud.Value > 1)
            {
                return Math.Abs(fromLongitud.Value);
            }

            var fromLength = ResolveSceneMm(attrs.ExtraValueAsDouble("lengthMm"), null);
            if (fromLength.HasValue && fromLength.Value > 1)
            {
                return Math.Abs(fromLength.Value);
            }

            return 0d;
        }

        private static double? ResolveWallYawRad(AttributesList attrs)
        {
            if (attrs == null)
            {
                return null;
            }

            var raw = attrs._YrRtation
                ?? attrs.ExtraValueAsDouble("_YrRtation")
                ?? attrs.ExtraValueAsDouble("_YRotation")
                ?? attrs.ExtraValueAsDouble("Yaw")
                ?? attrs.ExtraValueAsDouble("YawRad")
                ?? attrs.ExtraValueAsDouble("yawRad")
                ?? attrs.ExtraValueAsDouble("rotationY");

            if (!raw.HasValue)
            {
                return null;
            }

            return NormalizeYawToRad(raw.Value);
        }

        private static double ResolveFaceSign(AttributesList attrs, Atk60CentroidXz centroid, Atk60ResolvedWallGeom geom)
        {
            if (attrs != null)
            {
                var side = attrs.ExtraValueAsDouble("numberWallFaceSideSign");
                if (side.HasValue && Math.Abs(side.Value) > 1e-6)
                {
                    return side.Value > 0 ? 1d : -1d;
                }
            }

            if (centroid != null && geom != null &&
                geom.StartX.HasValue && geom.StartZ.HasValue && geom.EndX.HasValue && geom.EndZ.HasValue &&
                true)
            {
                var dx = geom.EndX.Value - geom.StartX.Value;
                var dz = geom.EndZ.Value - geom.StartZ.Value;
                var len = Math.Sqrt(dx * dx + dz * dz);
                if (len > 1e-6)
                {
                    var nx = -dz / len;
                    var nz = dx / len;
                    var cx = (geom.StartX.Value + geom.EndX.Value) * 0.5;
                    var cz = (geom.StartZ.Value + geom.EndZ.Value) * 0.5;
                    var vx = cx - centroid.X;
                    var vz = cz - centroid.Z;
                    var dot = vx * nx + vz * nz;
                    return dot >= 0 ? 1d : -1d;
                }
            }

            return 1d;
        }

        /// <summary>
        /// p1→p2 hacia +X; si el muro es vertical, hacia +Z.
        /// Evita yaw 180° cuando el usuario pulsó los puntos en sentido contrario.
        /// </summary>
        private static void CanonicalizeClockwiseWallAxis(
            ref double sx,
            ref double sz,
            ref double ex,
            ref double ez)
        {
            var dx = ex - sx;
            var dz = ez - sz;
            if (dx < -1e-9 || (Math.Abs(dx) <= 1e-9 && dz < -1e-9))
            {
                var tx = sx;
                var tz = sz;
                sx = ex;
                sz = ez;
                ex = tx;
                ez = tz;
            }
        }

        private static double NormalizeYawToRad(double yaw)
        {
            var abs = Math.Abs(yaw);
            return abs > (Math.PI * 2 + 1e-6)
                ? (yaw * Math.PI / 180d)
                : yaw;
        }

        private sealed class Atk60Point
        {
            public double? X { get; set; }
            public double? Y { get; set; }
            public double? Z { get; set; }
        }

        private sealed class Atk60ResolvedWallGeom
        {
            public double? StartX { get; set; }
            public double? StartY { get; set; }
            public double? StartZ { get; set; }
            public double? EndX { get; set; }
            public double? EndY { get; set; }
            public double? EndZ { get; set; }
            public double? AnchorX { get; set; }
            public double? AnchorY { get; set; }
            public double? AnchorZ { get; set; }
        }

        private sealed class Atk60CentroidXz
        {
            public double X { get; set; }
            public double Z { get; set; }
        }

        private sealed class Atk60CentroidAccum
        {
            public double Sx { get; set; }
            public double Sz { get; set; }
            public int Count { get; set; }
        }

    }

    internal static class Atk60AttributesExtensions
    {
        public static string ExtraValueAsString(this AttributesList attrs, string key)
        {
            if (attrs == null || attrs.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!attrs.Extra.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                var match = attrs.Extra.FirstOrDefault(kv =>
                    string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
                token = match.Value;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            var value = token.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static double? ExtraValueAsDouble(this AttributesList attrs, string key)
        {
            if (attrs == null || attrs.Extra == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            JToken token;
            if (!attrs.Extra.TryGetValue(key, out token) || token == null || token.Type == JTokenType.Null)
            {
                // Fallback case-insensitive porque los nombres pueden variar por serializer/casing.
                var match = attrs.Extra.FirstOrDefault(kv =>
                    string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
                token = match.Value;
                if (token == null || token.Type == JTokenType.Null)
                {
                    return null;
                }
            }

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<double>();
            }

            var raw = token.ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            double v;
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }

            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.GetCultureInfo("es-ES"), out v))
            {
                return v;
            }

            // Ultimo fallback: normalizar coma a punto para payloads mixtos.
            var normalized = raw.Replace(',', '.');
            if (double.TryParse(normalized, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out v))
            {
                return v;
            }

            return null;
        }
    }
}
