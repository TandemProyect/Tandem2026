using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Desing.Services
{
    /// <summary>Opciones del análisis DXF para «Importar DWG» de Desing_2.</summary>
    public sealed class Desing2DxfImportOptions
    {
        /// <summary>Si viene informado, sustituye a las unidades detectadas (metros por unidad de dibujo).</summary>
        public double? ToMetersOverride { get; set; }
        public int MaxSegments { get; set; } = 250000;
        public int MaxWalls { get; set; } = 5000;
        public double MinWallThicknessM { get; set; } = 0.05;
        public double MaxWallThicknessM { get; set; } = 0.5;
        public double MinWallLengthM { get; set; } = 0.3;
        public int MaxBlockDepth { get; set; } = 16;
    }

    public sealed class Desing2DxfUnitsInfo
    {
        /// <summary>Valor $INSUNITS (0 = sin unidades).</summary>
        public int InsUnits { get; set; }
        /// <summary>Código corto: mm, cm, dm, m, km, in, ft, yd, mi…</summary>
        public string Code { get; set; }
        public double ToMeters { get; set; }
        /// <summary>insunits | insunits-fixed ($INSUNITS incoherente con el tamaño: se usa la estimación) | lunits | guess | override</summary>
        public string Source { get; set; }
        /// <summary>high | medium | low</summary>
        public string Confidence { get; set; }
        /// <summary>$MEASUREMENT: 0 imperial, 1 métrico, -1 sin dato.</summary>
        public int Measurement { get; set; }
        /// <summary>$LUNITS (formato de unidades lineales), -1 sin dato.</summary>
        public int LUnits { get; set; }
        /// <summary>Unidad sugerida por tamaño si la declarada parece incoherente (o la estimada si no hay unidades).</summary>
        public string SuggestedCode { get; set; }
    }

    public sealed class Desing2DxfExtents
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
    }

    public sealed class Desing2DxfLayerInfo
    {
        public string Name { get; set; }
        public int Segments { get; set; }
        public bool Frozen { get; set; }
        public bool Off { get; set; }
        public bool WallLike { get; set; }
        public int Walls { get; set; }
    }

    /// <summary>Muro candidato: eje en unidades del dibujo (XY) y espesor.</summary>
    public sealed class Desing2DxfWallCandidate
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        /// <summary>Espesor en unidades del dibujo.</summary>
        public double Thickness { get; set; }
        public double ThicknessM { get; set; }
        public double LengthM { get; set; }
        public double? HeightM { get; set; }
        public int Layer { get; set; }
        /// <summary>pair-lines | polyline-rect | hatch-rect | solid-rect</summary>
        public string Source { get; set; }
    }

    public sealed class Desing2DxfImportResult
    {
        public bool Ok { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string AcadVersion { get; set; }
        public Desing2DxfUnitsInfo Units { get; set; }
        /// <summary>Extensión real de la geometría importable (unidades del dibujo).</summary>
        public Desing2DxfExtents Extents { get; set; }
        /// <summary>$EXTMIN/$EXTMAX de cabecera (puede estar desactualizado).</summary>
        public Desing2DxfExtents HeaderExtents { get; set; }
        public Dictionary<string, int> Stats { get; set; }
        /// <summary>Entidades no importadas por tipo (ARC, CIRCLE, TEXT…).</summary>
        public Dictionary<string, int> Ignored { get; set; }
        public List<Desing2DxfLayerInfo> Layers { get; set; }
        /// <summary>[x1, y1, x2, y2, capa, grupoPolilínea(-1), flags(1 = usado por un muro)] en unidades del dibujo.</summary>
        public List<double[]> Segments { get; set; }
        public List<Desing2DxfWallCandidate> Walls { get; set; }
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// Lector DXF ASCII mínimo para Desing_2: LINE, LWPOLYLINE, POLYLINE (arcos por bulge aproximados),
    /// INSERT recursivo (MINSERT, escala negativa, giro, OCS), capas congeladas/apagadas, unidades y detección de muros.
    /// Propio (sin netDxf) para controlar MINSERT / OCS / capas y tolerar entidades desconocidas.
    /// </summary>
    public static class Desing2DxfImportParser
    {
        private struct Pair
        {
            public int Code;
            public string Value;
        }

        private sealed class Ent
        {
            public string Type;
            public List<Pair> P = new List<Pair>();
            public List<Ent> Children;
        }

        private sealed class BlockDef
        {
            public string Name;
            public double BaseX, BaseY, BaseZ;
            public List<Ent> Entities = new List<Ent>();
        }

        private sealed class LayerState
        {
            public string Name;
            public bool Frozen;
            public bool Off;
            public int Index = -1;
        }

        /// <summary>Afín 3D (3x4).</summary>
        private struct Mat
        {
            public double A, B, C, D, E, F, G, H, I, Tx, Ty, Tz;

            public static Mat Identity
            {
                get { return new Mat { A = 1, E = 1, I = 1 }; }
            }

            public void Apply(double x, double y, double z, out double ox, out double oy, out double oz)
            {
                ox = A * x + B * y + C * z + Tx;
                oy = D * x + E * y + F * z + Ty;
                oz = G * x + H * y + I * z + Tz;
            }

            /// <summary>this * m (aplica m primero).</summary>
            public Mat Mul(Mat m)
            {
                return new Mat
                {
                    A = A * m.A + B * m.D + C * m.G,
                    B = A * m.B + B * m.E + C * m.H,
                    C = A * m.C + B * m.F + C * m.I,
                    D = D * m.A + E * m.D + F * m.G,
                    E = D * m.B + E * m.E + F * m.H,
                    F = D * m.C + E * m.F + F * m.I,
                    G = G * m.A + H * m.D + I * m.G,
                    H = G * m.B + H * m.E + I * m.H,
                    I = G * m.C + H * m.F + I * m.I,
                    Tx = A * m.Tx + B * m.Ty + C * m.Tz + Tx,
                    Ty = D * m.Tx + E * m.Ty + F * m.Tz + Ty,
                    Tz = G * m.Tx + H * m.Ty + I * m.Tz + Tz,
                };
            }

            public static Mat Translate(double x, double y, double z)
            {
                var m = Identity;
                m.Tx = x;
                m.Ty = y;
                m.Tz = z;
                return m;
            }

            public static Mat Scale(double sx, double sy, double sz)
            {
                return new Mat { A = sx, E = sy, I = sz };
            }

            public static Mat RotZ(double rad)
            {
                var c = Math.Cos(rad);
                var s = Math.Sin(rad);
                return new Mat { A = c, B = -s, D = s, E = c, I = 1 };
            }

            /// <summary>OCS → WCS (algoritmo del eje arbitrario de AutoCAD).</summary>
            public static Mat Ocs(double nx, double ny, double nz)
            {
                var len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (len < 1e-12) return Identity;
                nx /= len;
                ny /= len;
                nz /= len;
                if (Math.Abs(nx) < 1e-12 && Math.Abs(ny) < 1e-12 && nz > 0) return Identity;
                double ax, ay, az;
                if (Math.Abs(nx) < 1.0 / 64.0 && Math.Abs(ny) < 1.0 / 64.0)
                {
                    ax = nz;
                    ay = 0;
                    az = -nx;
                }
                else
                {
                    ax = -ny;
                    ay = nx;
                    az = 0;
                }
                var al = Math.Sqrt(ax * ax + ay * ay + az * az);
                ax /= al;
                ay /= al;
                az /= al;
                var bx = ny * az - nz * ay;
                var by = nz * ax - nx * az;
                var bz = nx * ay - ny * ax;
                return new Mat { A = ax, B = bx, C = nx, D = ay, E = by, F = ny, G = az, H = bz, I = nz };
            }
        }

        private sealed class Seg
        {
            public double X1, Y1, X2, Y2;
            public int Layer;
            public int Group = -1;
            public int Flags;
            public bool FromRect;
        }

        private sealed class RectPoly
        {
            public double[] Xs;
            public double[] Ys;
            public int Layer;
            public string Source;
            public List<Seg> OwnSegs;
        }

        private sealed class Ctx
        {
            public Desing2DxfImportOptions Options;
            public Dictionary<string, LayerState> Layers = new Dictionary<string, LayerState>(StringComparer.OrdinalIgnoreCase);
            public List<LayerState> UsedLayers = new List<LayerState>();
            public Dictionary<string, BlockDef> Blocks = new Dictionary<string, BlockDef>(StringComparer.OrdinalIgnoreCase);
            public List<Seg> Segs = new List<Seg>();
            /// <summary>Polígonos de 4 vértices candidatos a muro (WCS, unidades del dibujo).</summary>
            public List<RectPoly> Rects = new List<RectPoly>();
            public Dictionary<string, int> Stats = new Dictionary<string, int>();
            public Dictionary<string, int> Ignored = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public int NextGroup;
            public bool Truncated;
            public HashSet<string> MissingBlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Regex WallLayerRegex = new Regex(
            @"(MUR|WALL|TABIQ|PARED|CERRAM|FACHAD|FABRICA|FÁBRICA|LADRILL|HORMIG|PARTI|TRASDOS)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static Desing2DxfImportResult ParseFile(string dxfPath, Desing2DxfImportOptions options)
        {
            return ParseBytes(File.ReadAllBytes(dxfPath), options);
        }

        public static Desing2DxfImportResult ParseBytes(byte[] bytes, Desing2DxfImportOptions options)
        {
            if (options == null) options = new Desing2DxfImportOptions();
            if (bytes == null || bytes.Length == 0) return Fail("dxf_empty", "El archivo DXF está vacío.");
            var binSig = Encoding.ASCII.GetBytes("AutoCAD Binary DXF");
            if (bytes.Length >= binSig.Length && bytes.Take(binSig.Length).SequenceEqual(binSig))
            {
                return Fail("dxf_binary", "DXF binario no soportado: guárdelo como DXF ASCII.");
            }

            string text;
            try
            {
                text = new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                text = Encoding.GetEncoding(28591).GetString(bytes);
            }
            if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1);

            List<Pair> pairs;
            try
            {
                pairs = ReadPairs(text);
            }
            catch (FormatException)
            {
                return Fail("dxf_parse", "El archivo no es un DXF ASCII válido.");
            }
            text = null;

            var ctx = new Ctx { Options = options };
            var header = new Dictionary<string, List<Pair>>(StringComparer.OrdinalIgnoreCase);
            var entities = new List<Ent>();
            var sawSection = false;
            var i = 0;
            while (i < pairs.Count)
            {
                if (pairs[i].Code == 0 && pairs[i].Value == "SECTION" && i + 1 < pairs.Count && pairs[i + 1].Code == 2)
                {
                    sawSection = true;
                    var name = pairs[i + 1].Value;
                    i += 2;
                    if (name == "HEADER") ReadHeader(pairs, ref i, header);
                    else if (name == "TABLES") ReadTables(pairs, ref i, ctx);
                    else if (name == "BLOCKS") ReadBlocks(pairs, ref i, ctx);
                    else if (name == "ENTITIES") entities = ReadEntityList(pairs, ref i);
                    else SkipToEndSec(pairs, ref i);
                    continue;
                }
                i++;
            }
            if (!sawSection) return Fail("dxf_parse", "El archivo no es un DXF ASCII válido.");

            var result = new Desing2DxfImportResult
            {
                Ok = true,
                Warnings = new List<string>(),
                AcadVersion = HeaderString(header, "$ACADVER"),
                HeaderExtents = HeaderExtents(header),
            };

            foreach (var e in entities)
            {
                if (GetInt(e, 67, 0) == 1) continue; // espacio papel
                ProcessEntity(ctx, e, Mat.Identity, null, 0, false);
            }

            if (ctx.Truncated) result.Warnings.Add("segments_truncated");
            if (ctx.MissingBlocks.Count > 0) result.Warnings.Add("missing_blocks");

            result.Extents = ComputeExtents(ctx.Segs, ctx.Rects, 0.0, 1.0);
            var robust = ComputeExtents(ctx.Segs, ctx.Rects, 0.01, 0.99);
            result.Units = ResolveUnits(header, robust, options);
            var toM = result.Units.ToMeters;

            var walls = new List<Desing2DxfWallCandidate>();
            DetectRectWalls(ctx, toM, walls);
            DetectPairWalls(ctx, toM, walls);
            walls = DedupeWalls(walls);
            JoinWallCorners(walls);
            if (walls.Count > options.MaxWalls)
            {
                walls = walls.OrderByDescending(w => Dist(w.X1, w.Y1, w.X2, w.Y2)).Take(options.MaxWalls).ToList();
                result.Warnings.Add("walls_truncated");
            }
            foreach (var w in walls)
            {
                w.LengthM = Math.Round(Dist(w.X1, w.Y1, w.X2, w.Y2) * toM, 4);
                w.ThicknessM = Math.Round(w.Thickness * toM, 4);
                w.X1 = R(w.X1);
                w.Y1 = R(w.Y1);
                w.X2 = R(w.X2);
                w.Y2 = R(w.Y2);
                w.Thickness = R(w.Thickness);
            }

            // Unidades estimadas + espesores de muro típicos ⇒ algo más de confianza.
            if (result.Units.Source == "guess" && walls.Count >= 3)
            {
                var med = walls.Select(w => w.ThicknessM).OrderBy(t => t).ElementAt(walls.Count / 2);
                if (med >= 0.08 && med <= 0.4) result.Units.Confidence = "medium";
            }

            result.Walls = walls;
            result.Layers = ctx.UsedLayers.Select(l => new Desing2DxfLayerInfo
            {
                Name = l.Name,
                Frozen = l.Frozen,
                Off = l.Off,
                WallLike = WallLayerRegex.IsMatch(l.Name ?? ""),
            }).ToList();
            foreach (var s in ctx.Segs)
            {
                if (s.Layer >= 0 && s.Layer < result.Layers.Count) result.Layers[s.Layer].Segments++;
            }
            foreach (var w in walls)
            {
                if (w.Layer >= 0 && w.Layer < result.Layers.Count) result.Layers[w.Layer].Walls++;
            }
            result.Segments = ctx.Segs.Select(s => new[] { R(s.X1), R(s.Y1), R(s.X2), R(s.Y2), s.Layer, s.Group, s.Flags }).ToList();
            ctx.Stats["segments"] = ctx.Segs.Count;
            ctx.Stats["walls"] = walls.Count;
            ctx.Stats["wallLayers"] = result.Layers.Count(l => l.WallLike);
            result.Stats = ctx.Stats;
            result.Ignored = ctx.Ignored;
            if (ctx.Segs.Count == 0 && walls.Count == 0) result.Warnings.Add("no_geometry");
            return result;
        }

        private static Desing2DxfImportResult Fail(string code, string message)
        {
            return new Desing2DxfImportResult { Ok = false, ErrorCode = code, Message = message };
        }

        private static double R(double v)
        {
            return Math.Round(v, 6);
        }

        private static void Bump(Dictionary<string, int> d, string key)
        {
            int cur;
            d.TryGetValue(key, out cur);
            d[key] = cur + 1;
        }

        // ───────────── lectura de pares código/valor ─────────────

        private static List<Pair> ReadPairs(string text)
        {
            var list = new List<Pair>(Math.Max(1024, text.Length / 12));
            var pos = 0;
            var len = text.Length;
            while (pos < len)
            {
                var codeLine = NextLine(text, ref pos);
                if (codeLine == null) break;
                var trimmed = codeLine.Trim();
                if (trimmed.Length == 0 && pos >= len) break;
                var val = NextLine(text, ref pos);
                if (val == null) break;
                int code;
                if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out code))
                {
                    throw new FormatException("dxf_bad_group_code");
                }
                list.Add(new Pair { Code = code, Value = val.Trim() });
            }
            return list;
        }

        private static string NextLine(string text, ref int pos)
        {
            if (pos >= text.Length) return null;
            var start = pos;
            var nl = text.IndexOf('\n', pos);
            string line;
            if (nl < 0)
            {
                line = text.Substring(start);
                pos = text.Length;
            }
            else
            {
                line = text.Substring(start, nl - start);
                pos = nl + 1;
            }
            if (line.Length > 0 && line[line.Length - 1] == '\r') line = line.Substring(0, line.Length - 1);
            return line;
        }

        private static void SkipToEndSec(List<Pair> pairs, ref int i)
        {
            while (i < pairs.Count && !(pairs[i].Code == 0 && pairs[i].Value == "ENDSEC")) i++;
            i++;
        }

        private static void ReadHeader(List<Pair> pairs, ref int i, Dictionary<string, List<Pair>> header)
        {
            List<Pair> cur = null;
            while (i < pairs.Count)
            {
                var p = pairs[i];
                if (p.Code == 0 && p.Value == "ENDSEC")
                {
                    i++;
                    return;
                }
                if (p.Code == 9)
                {
                    cur = new List<Pair>();
                    header[p.Value] = cur;
                }
                else if (cur != null)
                {
                    cur.Add(p);
                }
                i++;
            }
        }

        private static void ReadTables(List<Pair> pairs, ref int i, Ctx ctx)
        {
            while (i < pairs.Count)
            {
                var p = pairs[i];
                if (p.Code == 0 && p.Value == "ENDSEC")
                {
                    i++;
                    return;
                }
                if (p.Code == 0 && p.Value == "LAYER")
                {
                    i++;
                    string name = null;
                    var flags = 0;
                    var color = 7;
                    while (i < pairs.Count && pairs[i].Code != 0)
                    {
                        var q = pairs[i];
                        if (q.Code == 2) name = q.Value;
                        else if (q.Code == 70) flags = (int)D(q.Value);
                        else if (q.Code == 62) color = (int)D(q.Value);
                        i++;
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        ctx.Layers[name] = new LayerState { Name = name, Frozen = (flags & 1) != 0, Off = color < 0 };
                    }
                    continue;
                }
                i++;
            }
        }

        private static void ReadBlocks(List<Pair> pairs, ref int i, Ctx ctx)
        {
            while (i < pairs.Count)
            {
                var p = pairs[i];
                if (p.Code == 0 && p.Value == "ENDSEC")
                {
                    i++;
                    return;
                }
                if (p.Code == 0 && p.Value == "BLOCK")
                {
                    i++;
                    var def = new BlockDef();
                    while (i < pairs.Count && pairs[i].Code != 0)
                    {
                        var q = pairs[i];
                        if (q.Code == 2) def.Name = q.Value;
                        else if (q.Code == 10) def.BaseX = D(q.Value);
                        else if (q.Code == 20) def.BaseY = D(q.Value);
                        else if (q.Code == 30) def.BaseZ = D(q.Value);
                        i++;
                    }
                    def.Entities = ReadEntityList(pairs, ref i);
                    if (!string.IsNullOrEmpty(def.Name)) ctx.Blocks[def.Name] = def;
                    continue;
                }
                i++;
            }
        }

        /// <summary>Lee entidades hasta ENDSEC o ENDBLK (consume el marcador).</summary>
        private static List<Ent> ReadEntityList(List<Pair> pairs, ref int i)
        {
            var list = new List<Ent>();
            Ent open = null; // POLYLINE o INSERT con atributos: recoge VERTEX/ATTRIB hasta SEQEND
            while (i < pairs.Count)
            {
                var p = pairs[i];
                if (p.Code != 0)
                {
                    i++;
                    continue;
                }
                if (p.Value == "ENDSEC")
                {
                    i++;
                    return list;
                }
                if (p.Value == "ENDBLK")
                {
                    i++;
                    while (i < pairs.Count && pairs[i].Code != 0) i++;
                    return list;
                }
                var e = ReadEntity(pairs, ref i);
                if (e.Type == "SEQEND")
                {
                    open = null;
                    continue;
                }
                if (open != null && (e.Type == "VERTEX" || e.Type == "ATTRIB"))
                {
                    if (open.Children == null) open.Children = new List<Ent>();
                    open.Children.Add(e);
                    continue;
                }
                open = null;
                if (e.Type == "POLYLINE" || (e.Type == "INSERT" && GetInt(e, 66, 0) == 1)) open = e;
                list.Add(e);
            }
            return list;
        }

        private static Ent ReadEntity(List<Pair> pairs, ref int i)
        {
            var e = new Ent { Type = pairs[i].Value };
            i++;
            while (i < pairs.Count && pairs[i].Code != 0)
            {
                e.P.Add(pairs[i]);
                i++;
            }
            return e;
        }

        // ───────────── utilidades de valores ─────────────

        private static double D(string s)
        {
            double v;
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) && !double.IsNaN(v) && !double.IsInfinity(v) ? v : 0;
        }

        private static double GetD(Ent e, int code, double def)
        {
            for (var k = 0; k < e.P.Count; k++)
            {
                if (e.P[k].Code == code) return D(e.P[k].Value);
            }
            return def;
        }

        private static int GetInt(Ent e, int code, int def)
        {
            for (var k = 0; k < e.P.Count; k++)
            {
                if (e.P[k].Code == code) return (int)D(e.P[k].Value);
            }
            return def;
        }

        private static string GetS(Ent e, int code)
        {
            for (var k = 0; k < e.P.Count; k++)
            {
                if (e.P[k].Code == code) return e.P[k].Value;
            }
            return null;
        }

        private static Mat ExtrusionOf(Ent e)
        {
            return Mat.Ocs(GetD(e, 210, 0), GetD(e, 220, 0), GetD(e, 230, 1));
        }

        private static string HeaderString(Dictionary<string, List<Pair>> header, string name)
        {
            List<Pair> v;
            return header.TryGetValue(name, out v) && v.Count > 0 ? v[0].Value : null;
        }

        private static int HeaderInt(Dictionary<string, List<Pair>> header, string name, int def)
        {
            var s = HeaderString(header, name);
            return s == null ? def : (int)D(s);
        }

        private static Desing2DxfExtents HeaderExtents(Dictionary<string, List<Pair>> header)
        {
            List<Pair> mn, mx;
            if (!header.TryGetValue("$EXTMIN", out mn) || !header.TryGetValue("$EXTMAX", out mx)) return null;
            Func<List<Pair>, int, double> val = (l, c) =>
            {
                foreach (var p in l)
                {
                    if (p.Code == c) return D(p.Value);
                }
                return 0;
            };
            var ext = new Desing2DxfExtents { MinX = val(mn, 10), MinY = val(mn, 20), MaxX = val(mx, 10), MaxY = val(mx, 20) };
            if (ext.MinX > ext.MaxX || Math.Abs(ext.MinX) > 1e19) return null;
            return ext;
        }

        // ───────────── capas ─────────────

        private static LayerState Layer(Ctx ctx, string name)
        {
            if (string.IsNullOrEmpty(name)) name = "0";
            LayerState st;
            if (!ctx.Layers.TryGetValue(name, out st))
            {
                st = new LayerState { Name = name };
                ctx.Layers[name] = st;
            }
            return st;
        }

        private static int LayerIndex(Ctx ctx, LayerState st)
        {
            if (st.Index < 0)
            {
                st.Index = ctx.UsedLayers.Count;
                ctx.UsedLayers.Add(st);
            }
            return st.Index;
        }

        /// <summary>Capa efectiva: dentro de un bloque, lo dibujado en la capa «0» hereda la capa del INSERT.</summary>
        private static string EffectiveLayer(Ent e, string inherit)
        {
            var name = GetS(e, 8);
            if (string.IsNullOrEmpty(name)) name = "0";
            if (inherit != null && name == "0") return inherit;
            return name;
        }

        // ───────────── entidades ─────────────

        private static void ProcessEntity(Ctx ctx, Ent e, Mat m, string inheritLayer, int depth, bool inBlock)
        {
            if (GetInt(e, 60, 0) == 1) return; // invisible
            var layerName = EffectiveLayer(e, inheritLayer);
            var layer = Layer(ctx, layerName);
            var hidden = layer.Frozen || layer.Off || string.Equals(layerName, "Defpoints", StringComparison.OrdinalIgnoreCase);

            switch (e.Type)
            {
                case "LINE":
                {
                    if (hidden)
                    {
                        Bump(ctx.Stats, "skippedHiddenLayer");
                        return;
                    }
                    double x1, y1, z1, x2, y2, z2;
                    m.Apply(GetD(e, 10, 0), GetD(e, 20, 0), GetD(e, 30, 0), out x1, out y1, out z1);
                    m.Apply(GetD(e, 11, 0), GetD(e, 21, 0), GetD(e, 31, 0), out x2, out y2, out z2);
                    if (AddSeg(ctx, x1, y1, x2, y2, LayerIndex(ctx, layer), -1, null)) Bump(ctx.Stats, inBlock ? "linesInBlocks" : "lines");
                    return;
                }
                case "LWPOLYLINE":
                {
                    if (hidden)
                    {
                        Bump(ctx.Stats, "skippedHiddenLayer");
                        return;
                    }
                    var xs = new List<double>();
                    var ys = new List<double>();
                    var bulges = new List<double>();
                    foreach (var p in e.P)
                    {
                        if (p.Code == 10)
                        {
                            xs.Add(D(p.Value));
                            ys.Add(0);
                            bulges.Add(0);
                        }
                        else if (p.Code == 20 && ys.Count > 0) ys[ys.Count - 1] = D(p.Value);
                        else if (p.Code == 42 && bulges.Count > 0) bulges[bulges.Count - 1] = D(p.Value);
                    }
                    var closed = (GetInt(e, 70, 0) & 1) != 0;
                    AddPolyline(ctx, xs, ys, bulges, GetD(e, 38, 0), closed, m.Mul(ExtrusionOf(e)), LayerIndex(ctx, layer), inBlock);
                    return;
                }
                case "POLYLINE":
                {
                    var flags = GetInt(e, 70, 0);
                    if ((flags & (16 | 64)) != 0)
                    {
                        Bump(ctx.Ignored, "POLYLINE_MESH");
                        return;
                    }
                    if (hidden)
                    {
                        Bump(ctx.Stats, "skippedHiddenLayer");
                        return;
                    }
                    var is3d = (flags & 8) != 0;
                    var xs = new List<double>();
                    var ys = new List<double>();
                    var zs = new List<double>();
                    var bulges = new List<double>();
                    if (e.Children != null)
                    {
                        foreach (var v in e.Children)
                        {
                            if (v.Type != "VERTEX") continue;
                            var vf = GetInt(v, 70, 0);
                            if ((vf & 16) != 0) continue; // marco de spline
                            xs.Add(GetD(v, 10, 0));
                            ys.Add(GetD(v, 20, 0));
                            zs.Add(GetD(v, 30, 0));
                            bulges.Add(is3d ? 0 : GetD(v, 42, 0));
                        }
                    }
                    var closed = (flags & 1) != 0;
                    if (is3d)
                    {
                        // 3D: vértices en WCS y sin bulge.
                        var px = new List<double>();
                        var py = new List<double>();
                        for (var k = 0; k < xs.Count; k++)
                        {
                            double ox, oy, oz;
                            m.Apply(xs[k], ys[k], zs[k], out ox, out oy, out oz);
                            px.Add(ox);
                            py.Add(oy);
                        }
                        AddPolyline(ctx, px, py, new List<double>(new double[px.Count]), 0, closed, Mat.Identity, LayerIndex(ctx, layer), inBlock);
                    }
                    else
                    {
                        AddPolyline(ctx, xs, ys, bulges, GetD(e, 30, 0), closed, m.Mul(ExtrusionOf(e)), LayerIndex(ctx, layer), inBlock);
                    }
                    return;
                }
                case "INSERT":
                {
                    if (layer.Frozen)
                    {
                        Bump(ctx.Stats, "skippedHiddenLayer");
                        return;
                    }
                    if (depth >= ctx.Options.MaxBlockDepth)
                    {
                        Bump(ctx.Stats, "blocksTooDeep");
                        return;
                    }
                    var name = GetS(e, 2);
                    BlockDef def;
                    if (string.IsNullOrEmpty(name) || !ctx.Blocks.TryGetValue(name, out def))
                    {
                        if (!string.IsNullOrEmpty(name)) ctx.MissingBlocks.Add(name);
                        return;
                    }
                    var sx = GetD(e, 41, 1);
                    var sy = GetD(e, 42, 1);
                    var sz = GetD(e, 43, 1);
                    if (sx == 0) sx = 1;
                    if (sy == 0) sy = 1;
                    if (sz == 0) sz = 1;
                    var rot = GetD(e, 50, 0) * Math.PI / 180.0;
                    var cols = Math.Max(1, GetInt(e, 70, 1));
                    var rows = Math.Max(1, GetInt(e, 71, 1));
                    var colSp = GetD(e, 44, 0);
                    var rowSp = GetD(e, 45, 0);
                    cols = Math.Min(cols, 200);
                    rows = Math.Min(rows, 200);
                    var basePart = Mat.Scale(sx, sy, sz).Mul(Mat.Translate(-def.BaseX, -def.BaseY, -def.BaseZ));
                    var head = m.Mul(ExtrusionOf(e)).Mul(Mat.Translate(GetD(e, 10, 0), GetD(e, 20, 0), GetD(e, 30, 0))).Mul(Mat.RotZ(rot));
                    for (var c = 0; c < cols; c++)
                    {
                        for (var r = 0; r < rows; r++)
                        {
                            var mi = head.Mul(Mat.Translate(c * colSp, r * rowSp, 0)).Mul(basePart);
                            Bump(ctx.Stats, "blocksExploded");
                            foreach (var child in def.Entities)
                            {
                                ProcessEntity(ctx, child, mi, layerName, depth + 1, true);
                            }
                        }
                    }
                    if (cols * rows > 1) Bump(ctx.Stats, "minserts");
                    return;
                }
                case "HATCH":
                {
                    Bump(ctx.Stats, "hatches");
                    if (hidden) return;
                    var mm = m.Mul(ExtrusionOf(e));
                    var elev = GetD(e, 30, 0);
                    foreach (var loop in ReadHatchLoops(e))
                    {
                        AddRectCandidate(ctx, loop.Item1, loop.Item2, elev, mm, LayerIndex(ctx, layer), "hatch-rect", null);
                    }
                    return;
                }
                case "SOLID":
                case "TRACE":
                {
                    Bump(ctx.Stats, "solids2d");
                    if (hidden) return;
                    var xs = new List<double> { GetD(e, 10, 0), GetD(e, 11, 0), GetD(e, 13, 0), GetD(e, 12, 0) };
                    var ys = new List<double> { GetD(e, 20, 0), GetD(e, 21, 0), GetD(e, 23, 0), GetD(e, 22, 0) };
                    AddRectCandidate(ctx, xs, ys, GetD(e, 30, 0), m.Mul(ExtrusionOf(e)), LayerIndex(ctx, layer), "solid-rect", null);
                    return;
                }
                case "3DSOLID":
                case "BODY":
                    Bump(ctx.Stats, "solids3d");
                    return;
                case "REGION":
                    Bump(ctx.Stats, "regions");
                    return;
                case "ACAD_PROXY_ENTITY":
                    Bump(ctx.Stats, "proxies");
                    return;
                default:
                    if (e.Type.StartsWith("AEC_", StringComparison.OrdinalIgnoreCase))
                    {
                        Bump(ctx.Stats, e.Type.IndexOf("WALL", StringComparison.OrdinalIgnoreCase) >= 0 ? "aecWalls" : "aecObjects");
                        return;
                    }
                    if (e.Type != "VERTEX" && e.Type != "ATTRIB" && e.Type != "ATTDEF") Bump(ctx.Ignored, e.Type);
                    return;
            }
        }

        private static bool AddSeg(Ctx ctx, double x1, double y1, double x2, double y2, int layer, int group, List<Seg> collect)
        {
            if (double.IsNaN(x1) || double.IsNaN(y1) || double.IsNaN(x2) || double.IsNaN(y2)) return false;
            var dx = x2 - x1;
            var dy = y2 - y1;
            var eps = 1e-9 * Math.Max(1.0, Math.Max(Math.Abs(x1), Math.Abs(y1)));
            if (dx * dx + dy * dy <= eps * eps) return false;
            if (ctx.Segs.Count >= ctx.Options.MaxSegments)
            {
                ctx.Truncated = true;
                return false;
            }
            var s = new Seg { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Layer = layer, Group = group };
            ctx.Segs.Add(s);
            if (collect != null) collect.Add(s);
            return true;
        }

        private static void AddPolyline(Ctx ctx, List<double> xs, List<double> ys, List<double> bulges, double elev, bool closed, Mat m, int layer, bool inBlock)
        {
            var n = xs.Count;
            if (n < 2) return;
            var ptsX = new List<double> { xs[0] };
            var ptsY = new List<double> { ys[0] };
            var hasBulge = false;
            var segCount = closed ? n : n - 1;
            for (var k = 0; k < segCount; k++)
            {
                var k2 = (k + 1) % n;
                var b = bulges.Count > k ? bulges[k] : 0;
                if (Math.Abs(b) > 1e-9)
                {
                    hasBulge = true;
                    AppendBulgeArc(xs[k], ys[k], xs[k2], ys[k2], b, ptsX, ptsY);
                }
                ptsX.Add(xs[k2]);
                ptsY.Add(ys[k2]);
            }
            var group = ctx.NextGroup++;
            var own = new List<Seg>();
            double px = 0, py = 0;
            for (var k = 0; k < ptsX.Count; k++)
            {
                double ox, oy, oz;
                m.Apply(ptsX[k], ptsY[k], elev, out ox, out oy, out oz);
                if (k > 0) AddSeg(ctx, px, py, ox, oy, layer, group, own);
                px = ox;
                py = oy;
            }
            if (own.Count == 0) return;
            Bump(ctx.Stats, inBlock ? "polylinesInBlocks" : "polylines");
            if (closed && !hasBulge) AddRectCandidate(ctx, xs, ys, elev, m, layer, "polyline-rect", own);
        }

        /// <summary>Arco por bulge (plano local) aproximado cada ≤10°; añade solo los puntos intermedios.</summary>
        private static void AppendBulgeArc(double x1, double y1, double x2, double y2, double bulge, List<double> outX, List<double> outY)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var c = Math.Sqrt(dx * dx + dy * dy);
            if (c < 1e-12) return;
            var theta = 4.0 * Math.Atan(bulge);
            var dist = (c / 2.0) * (1.0 - bulge * bulge) / (2.0 * bulge);
            var cx = (x1 + x2) / 2.0 - dy / c * dist;
            var cy = (y1 + y2) / 2.0 + dx / c * dist;
            var r = Math.Sqrt((x1 - cx) * (x1 - cx) + (y1 - cy) * (y1 - cy));
            var a0 = Math.Atan2(y1 - cy, x1 - cx);
            var steps = (int)Math.Ceiling(Math.Abs(theta) / (Math.PI / 18.0));
            steps = Math.Max(2, Math.Min(64, steps));
            for (var k = 1; k < steps; k++)
            {
                var a = a0 + theta * k / steps;
                outX.Add(cx + r * Math.Cos(a));
                outY.Add(cy + r * Math.Sin(a));
            }
        }

        private static void AddRectCandidate(Ctx ctx, List<double> xs, List<double> ys, double elev, Mat m, int layer, string source, List<Seg> own)
        {
            if (xs.Count < 4 || xs.Count > 5) return;
            var px = new List<double>();
            var py = new List<double>();
            for (var k = 0; k < xs.Count; k++)
            {
                double ox, oy, oz;
                m.Apply(xs[k], ys[k], elev, out ox, out oy, out oz);
                if (px.Count > 0 && Math.Abs(ox - px[px.Count - 1]) + Math.Abs(oy - py[py.Count - 1]) < 1e-9) continue;
                px.Add(ox);
                py.Add(oy);
            }
            if (px.Count == 5 && Math.Abs(px[4] - px[0]) + Math.Abs(py[4] - py[0]) < 1e-9)
            {
                px.RemoveAt(4);
                py.RemoveAt(4);
            }
            if (px.Count != 4) return;
            ctx.Rects.Add(new RectPoly { Xs = px.ToArray(), Ys = py.ToArray(), Layer = layer, Source = source, OwnSegs = own });
        }

        /// <summary>Bucles de contorno de un HATCH formados por polilínea sin arcos o aristas rectas (OCS).</summary>
        private static List<Tuple<List<double>, List<double>>> ReadHatchLoops(Ent e)
        {
            var loops = new List<Tuple<List<double>, List<double>>>();
            var p = e.P;
            var k = p.FindIndex(q => q.Code == 91);
            if (k < 0) return loops;
            var nLoops = (int)D(p[k].Value);
            k++;
            Func<int, bool> at = code => k < p.Count && p[k].Code == code;
            for (var li = 0; li < nLoops; li++)
            {
                if (!at(92)) return loops;
                var flags = (int)D(p[k].Value);
                k++;
                var xs = new List<double>();
                var ys = new List<double>();
                var ok = true;
                if ((flags & 2) != 0)
                {
                    if (!at(72)) return loops;
                    k++;
                    if (!at(73)) return loops;
                    k++;
                    if (!at(93)) return loops;
                    var nv = (int)D(p[k].Value);
                    k++;
                    for (var v = 0; v < nv; v++)
                    {
                        if (!at(10)) return loops;
                        xs.Add(D(p[k].Value));
                        k++;
                        if (!at(20)) return loops;
                        ys.Add(D(p[k].Value));
                        k++;
                        if (at(42))
                        {
                            if (Math.Abs(D(p[k].Value)) > 1e-9) ok = false;
                            k++;
                        }
                    }
                }
                else
                {
                    if (!at(93)) return loops;
                    var ne = (int)D(p[k].Value);
                    k++;
                    for (var ei = 0; ei < ne; ei++)
                    {
                        if (!at(72)) return loops;
                        var type = (int)D(p[k].Value);
                        k++;
                        if (type == 1)
                        {
                            var codes = new[] { 10, 20, 11, 21 };
                            var vals = new double[4];
                            for (var ci = 0; ci < 4; ci++)
                            {
                                if (!at(codes[ci])) return loops;
                                vals[ci] = D(p[k].Value);
                                k++;
                            }
                            xs.Add(vals[0]);
                            ys.Add(vals[1]);
                        }
                        else
                        {
                            ok = false;
                            // Arco / elipse / spline: saltar hasta la siguiente arista o el final del bucle.
                            while (k < p.Count && p[k].Code != 72 && p[k].Code != 97 && p[k].Code != 92) k++;
                        }
                    }
                }
                if (at(97))
                {
                    var ns = (int)D(p[k].Value);
                    k++;
                    for (var si = 0; si < ns && at(330); si++) k++;
                }
                if (ok) loops.Add(Tuple.Create(xs, ys));
            }
            return loops;
        }

        // ───────────── extensión y unidades ─────────────

        private static Desing2DxfExtents ComputeExtents(List<Seg> segs, List<RectPoly> rects, double lo, double hi)
        {
            var xs = new List<double>(segs.Count * 2 + rects.Count * 4);
            var ys = new List<double>(xs.Capacity);
            foreach (var s in segs)
            {
                xs.Add(s.X1);
                xs.Add(s.X2);
                ys.Add(s.Y1);
                ys.Add(s.Y2);
            }
            foreach (var r in rects)
            {
                xs.AddRange(r.Xs);
                ys.AddRange(r.Ys);
            }
            if (xs.Count == 0) return null;
            if (lo <= 0 && hi >= 1)
            {
                return new Desing2DxfExtents { MinX = R(xs.Min()), MinY = R(ys.Min()), MaxX = R(xs.Max()), MaxY = R(ys.Max()) };
            }
            xs.Sort();
            ys.Sort();
            Func<List<double>, double, double> q = (l, f) => l[Math.Max(0, Math.Min(l.Count - 1, (int)Math.Round(f * (l.Count - 1))))];
            return new Desing2DxfExtents { MinX = q(xs, lo), MinY = q(ys, lo), MaxX = q(xs, hi), MaxY = q(ys, hi) };
        }

        private static readonly Dictionary<int, KeyValuePair<string, double>> InsUnitsTable = new Dictionary<int, KeyValuePair<string, double>>
        {
            { 1, new KeyValuePair<string, double>("in", 0.0254) },
            { 2, new KeyValuePair<string, double>("ft", 0.3048) },
            { 3, new KeyValuePair<string, double>("mi", 1609.344) },
            { 4, new KeyValuePair<string, double>("mm", 0.001) },
            { 5, new KeyValuePair<string, double>("cm", 0.01) },
            { 6, new KeyValuePair<string, double>("m", 1.0) },
            { 7, new KeyValuePair<string, double>("km", 1000.0) },
            { 8, new KeyValuePair<string, double>("uin", 2.54e-8) },
            { 9, new KeyValuePair<string, double>("mil", 2.54e-5) },
            { 10, new KeyValuePair<string, double>("yd", 0.9144) },
            { 11, new KeyValuePair<string, double>("A", 1e-10) },
            { 12, new KeyValuePair<string, double>("nm", 1e-9) },
            { 13, new KeyValuePair<string, double>("um", 1e-6) },
            { 14, new KeyValuePair<string, double>("dm", 0.1) },
            { 15, new KeyValuePair<string, double>("dam", 10.0) },
            { 16, new KeyValuePair<string, double>("hm", 100.0) },
            { 17, new KeyValuePair<string, double>("Gm", 1e9) },
            { 21, new KeyValuePair<string, double>("ft-us", 1200.0 / 3937.0) },
            { 22, new KeyValuePair<string, double>("in-us", 100.0 / 3937.0) },
            { 23, new KeyValuePair<string, double>("yd-us", 3600.0 / 3937.0) },
        };

        /// <summary>Unidades que la interfaz deja elegir (y las que se prueban al estimar).</summary>
        public static readonly Dictionary<string, double> SelectableUnits = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "mm", 0.001 }, { "cm", 0.01 }, { "dm", 0.1 }, { "m", 1.0 }, { "in", 0.0254 }, { "ft", 0.3048 },
        };

        private static Desing2DxfUnitsInfo ResolveUnits(Dictionary<string, List<Pair>> header, Desing2DxfExtents robust, Desing2DxfImportOptions options)
        {
            var info = new Desing2DxfUnitsInfo
            {
                InsUnits = HeaderInt(header, "$INSUNITS", 0),
                Measurement = HeaderInt(header, "$MEASUREMENT", -1),
                LUnits = HeaderInt(header, "$LUNITS", -1),
            };
            var sizeRaw = robust == null ? 0 : Math.Max(robust.MaxX - robust.MinX, robust.MaxY - robust.MinY);
            var guess = GuessUnit(sizeRaw, info.Measurement);

            if (options.ToMetersOverride.HasValue && options.ToMetersOverride.Value > 0)
            {
                info.ToMeters = options.ToMetersOverride.Value;
                info.Code = CodeForFactor(info.ToMeters);
                info.Source = "override";
                info.Confidence = "high";
                info.SuggestedCode = guess;
                return info;
            }

            KeyValuePair<string, double> known;
            if (InsUnitsTable.TryGetValue(info.InsUnits, out known))
            {
                info.Code = known.Key;
                info.ToMeters = known.Value;
                info.Source = "insunits";
                var sizeM = sizeRaw * known.Value;
                info.Confidence = sizeRaw <= 0 || (sizeM >= 0.5 && sizeM <= 5000) ? "high" : "medium";
                if (info.Confidence != "high") info.SuggestedCode = guess;
                // Tamaño absurdo para un plano (< 20 cm o > 50 km): el $INSUNITS está mal puesto ⇒ usar la estimación.
                if (sizeRaw > 0 && (sizeM < 0.2 || sizeM > 50000) && guess != known.Key)
                {
                    info.SuggestedCode = known.Key;
                    info.Code = guess;
                    info.ToMeters = SelectableUnits[guess];
                    info.Source = "insunits-fixed";
                    info.Confidence = "low";
                }
                return info;
            }

            // Sin unidades: LUNITS arquitectónico / ingeniería ⇒ pulgadas.
            if (info.LUnits == 3 || info.LUnits == 4)
            {
                info.Code = "in";
                info.ToMeters = 0.0254;
                info.Source = "lunits";
                info.Confidence = "medium";
                info.SuggestedCode = guess;
                return info;
            }

            info.Code = guess;
            info.ToMeters = SelectableUnits[guess];
            info.Source = "guess";
            info.Confidence = "low";
            info.SuggestedCode = guess;
            return info;
        }

        private static string CodeForFactor(double f)
        {
            foreach (var kv in SelectableUnits)
            {
                if (Math.Abs(kv.Value - f) <= f * 1e-6) return kv.Key;
            }
            foreach (var kv in InsUnitsTable.Values)
            {
                if (Math.Abs(kv.Value - f) <= f * 1e-6) return kv.Key;
            }
            return "custom";
        }

        /// <summary>Unidad que deja el dibujo con tamaño típico de edificio (~25 m); prefiere métrico salvo $MEASUREMENT = 0.</summary>
        private static string GuessUnit(double sizeRaw, int measurement)
        {
            if (!(sizeRaw > 0)) return measurement == 0 ? "in" : "mm";
            var best = "mm";
            var bestScore = double.MaxValue;
            foreach (var kv in SelectableUnits)
            {
                var imperial = kv.Key == "in" || kv.Key == "ft";
                var score = Math.Abs(Math.Log10(sizeRaw * kv.Value / 25.0));
                if (measurement == 0 ? !imperial : imperial) score += 0.35;
                if (kv.Key == "dm") score += 0.5;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = kv.Key;
                }
            }
            return best;
        }

        // ───────────── detección de muros ─────────────

        /// <summary>Rectángulos cerrados (polilínea, sombreado, SOLID) con lado corto de espesor de muro ⇒ eje + espesor.</summary>
        private static void DetectRectWalls(Ctx ctx, double toM, List<Desing2DxfWallCandidate> walls)
        {
            var o = ctx.Options;
            foreach (var r in ctx.Rects)
            {
                var ok = true;
                for (var k = 0; k < 4 && ok; k++)
                {
                    var b = (k + 1) % 4;
                    var c = (k + 2) % 4;
                    var ux = r.Xs[b] - r.Xs[k];
                    var uy = r.Ys[b] - r.Ys[k];
                    var vx = r.Xs[c] - r.Xs[b];
                    var vy = r.Ys[c] - r.Ys[b];
                    var lu = Math.Sqrt(ux * ux + uy * uy);
                    var lv = Math.Sqrt(vx * vx + vy * vy);
                    if (lu < 1e-12 || lv < 1e-12 || Math.Abs((ux * vx + uy * vy) / (lu * lv)) > 0.035) ok = false;
                }
                if (!ok) continue;
                var s0 = Dist(r.Xs[0], r.Ys[0], r.Xs[1], r.Ys[1]);
                var s1 = Dist(r.Xs[1], r.Ys[1], r.Xs[2], r.Ys[2]);
                var shortSide = Math.Min(s0, s1);
                var tM = shortSide * toM;
                var lM = Math.Max(s0, s1) * toM;
                if (tM < o.MinWallThicknessM || tM > o.MaxWallThicknessM) continue;
                if (lM < Math.Max(o.MinWallLengthM, 1.5 * tM)) continue;
                var w = new Desing2DxfWallCandidate { Thickness = shortSide, Layer = r.Layer, Source = r.Source };
                if (s0 <= s1)
                {
                    w.X1 = (r.Xs[0] + r.Xs[1]) / 2;
                    w.Y1 = (r.Ys[0] + r.Ys[1]) / 2;
                    w.X2 = (r.Xs[2] + r.Xs[3]) / 2;
                    w.Y2 = (r.Ys[2] + r.Ys[3]) / 2;
                }
                else
                {
                    w.X1 = (r.Xs[1] + r.Xs[2]) / 2;
                    w.Y1 = (r.Ys[1] + r.Ys[2]) / 2;
                    w.X2 = (r.Xs[3] + r.Xs[0]) / 2;
                    w.Y2 = (r.Ys[3] + r.Ys[0]) / 2;
                }
                walls.Add(w);
                if (r.OwnSegs == null) continue;
                foreach (var s in r.OwnSegs)
                {
                    s.FromRect = true;
                    s.Flags |= 1;
                }
            }
        }

        private sealed class PairItem
        {
            public Seg S;
            public int Key;
            public double Off;
        }

        private sealed class PairCand
        {
            public Seg A, B;
            public double Dist, O0, O1, Off, Ref;
        }

        /// <summary>Pares de líneas paralelas a distancia de espesor de muro ⇒ eje (tramo solapado) + espesor.</summary>
        private static void DetectPairWalls(Ctx ctx, double toM, List<Desing2DxfWallCandidate> walls)
        {
            var o = ctx.Options;
            var angTol = Math.PI / 180.0; // cubos de 1°
            var nKeys = (int)Math.Round(Math.PI / angTol);
            var buckets = new Dictionary<int, List<PairItem>>();
            var minLen = o.MinWallLengthM / toM;
            foreach (var s in ctx.Segs)
            {
                if (s.FromRect) continue;
                if (Dist(s.X1, s.Y1, s.X2, s.Y2) < minLen) continue;
                var a = Math.Atan2(s.Y2 - s.Y1, s.X2 - s.X1);
                if (a < 0) a += Math.PI;
                if (a >= Math.PI) a -= Math.PI;
                var key = (int)Math.Round(a / angTol) % nKeys;
                List<PairItem> l;
                if (!buckets.TryGetValue(key, out l))
                {
                    l = new List<PairItem>();
                    buckets[key] = l;
                }
                l.Add(new PairItem { S = s, Key = key });
            }

            var minT = o.MinWallThicknessM / toM;
            var maxT = o.MaxWallThicknessM / toM;
            var sinTol = Math.Sin(angTol * 1.01);
            var cands = new List<PairCand>();
            foreach (var kv in buckets)
            {
                var key = kv.Key;
                var refAng = key * angTol;
                var cs = Math.Cos(refAng);
                var sn = Math.Sin(refAng);
                var pool = new List<PairItem>(kv.Value);
                List<PairItem> next;
                if (buckets.TryGetValue((key + 1) % nKeys, out next) && next != kv.Value) pool.AddRange(next);
                foreach (var it in pool) it.Off = -sn * (it.S.X1 + it.S.X2) / 2 + cs * (it.S.Y1 + it.S.Y2) / 2;
                pool.Sort((x, y) => x.Off.CompareTo(y.Off));
                for (var ia = 0; ia < pool.Count; ia++)
                {
                    var A = pool[ia];
                    for (var ib = ia + 1; ib < pool.Count; ib++)
                    {
                        var B = pool[ib];
                        if (B.Off - A.Off > maxT * 1.05) break;
                        if (A.Key != key && B.Key != key) continue; // ese par lo trata su propio cubo
                        var ua = Unit(A.S);
                        var ub = Unit(B.S);
                        if (Math.Abs(ua[0] * ub[1] - ua[1] * ub[0]) > sinTol) continue;
                        var mbx = (B.S.X1 + B.S.X2) / 2;
                        var mby = (B.S.Y1 + B.S.Y2) / 2;
                        var dist = Math.Abs((mbx - A.S.X1) * ua[1] - (mby - A.S.Y1) * ua[0]);
                        if (dist < minT || dist > maxT) continue;
                        var a0 = cs * A.S.X1 + sn * A.S.Y1;
                        var a1 = cs * A.S.X2 + sn * A.S.Y2;
                        var b0 = cs * B.S.X1 + sn * B.S.Y1;
                        var b1 = cs * B.S.X2 + sn * B.S.Y2;
                        var o0 = Math.Max(Math.Min(a0, a1), Math.Min(b0, b1));
                        var o1 = Math.Min(Math.Max(a0, a1), Math.Max(b0, b1));
                        var ov = o1 - o0;
                        var shorter = Math.Min(Math.Abs(a1 - a0), Math.Abs(b1 - b0));
                        if (ov < minLen || ov < 0.5 * shorter || ov < dist) continue;
                        cands.Add(new PairCand { A = A.S, B = B.S, Dist = dist, O0 = o0, O1 = o1, Off = (A.Off + B.Off) / 2, Ref = refAng });
                    }
                }
            }

            // Codicioso: primero los pares más cercanos (las dos caras de un mismo muro); un tramo de línea no se reutiliza.
            var used = new Dictionary<Seg, List<double[]>>();
            Func<Seg, double, double, bool> free = (s, t0, t1) =>
            {
                List<double[]> l;
                if (!used.TryGetValue(s, out l)) return true;
                foreach (var iv in l)
                {
                    if (Math.Min(iv[1], t1) - Math.Max(iv[0], t0) > 0.1 * (t1 - t0)) return false;
                }
                return true;
            };
            Action<Seg, double, double> take = (s, t0, t1) =>
            {
                List<double[]> l;
                if (!used.TryGetValue(s, out l))
                {
                    l = new List<double[]>();
                    used[s] = l;
                }
                l.Add(new[] { t0, t1 });
            };
            foreach (var c in cands.OrderBy(c => c.Dist).ThenByDescending(c => c.O1 - c.O0))
            {
                if (!free(c.A, c.O0, c.O1) || !free(c.B, c.O0, c.O1)) continue;
                take(c.A, c.O0, c.O1);
                take(c.B, c.O0, c.O1);
                c.A.Flags |= 1;
                c.B.Flags |= 1;
                var cs = Math.Cos(c.Ref);
                var sn = Math.Sin(c.Ref);
                walls.Add(new Desing2DxfWallCandidate
                {
                    X1 = c.O0 * cs - c.Off * sn,
                    Y1 = c.O0 * sn + c.Off * cs,
                    X2 = c.O1 * cs - c.Off * sn,
                    Y2 = c.O1 * sn + c.Off * cs,
                    Thickness = c.Dist,
                    Layer = c.A.Layer,
                    Source = "pair-lines",
                });
            }
        }

        private static double[] Unit(Seg s)
        {
            var dx = s.X2 - s.X1;
            var dy = s.Y2 - s.Y1;
            var l = Math.Sqrt(dx * dx + dy * dy);
            return new[] { dx / l, dy / l };
        }

        private static double Dist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        /// <summary>Quita muros casi coincidentes (p. ej. sombreado + doble línea del mismo muro).</summary>
        private static List<Desing2DxfWallCandidate> DedupeWalls(List<Desing2DxfWallCandidate> walls)
        {
            var keep = new List<Desing2DxfWallCandidate>();
            foreach (var w in walls)
            {
                var len = Dist(w.X1, w.Y1, w.X2, w.Y2);
                if (len < 1e-12) continue;
                var ux = (w.X2 - w.X1) / len;
                var uy = (w.Y2 - w.Y1) / len;
                var dup = false;
                foreach (var k in keep)
                {
                    var kl = Dist(k.X1, k.Y1, k.X2, k.Y2);
                    var kx = (k.X2 - k.X1) / kl;
                    var ky = (k.Y2 - k.Y1) / kl;
                    if (Math.Abs(ux * ky - uy * kx) > 0.035) continue;
                    var mx = (w.X1 + w.X2) / 2;
                    var my = (w.Y1 + w.Y2) / 2;
                    if (Math.Abs((mx - k.X1) * ky - (my - k.Y1) * kx) > 0.5 * Math.Min(w.Thickness, k.Thickness)) continue;
                    var t0 = (w.X1 - k.X1) * kx + (w.Y1 - k.Y1) * ky;
                    var t1 = (w.X2 - k.X1) * kx + (w.Y2 - k.Y1) * ky;
                    var ov = Math.Min(Math.Max(t0, t1), kl) - Math.Max(Math.Min(t0, t1), 0);
                    if (ov > 0.5 * Math.Min(len, kl))
                    {
                        dup = true;
                        break;
                    }
                }
                if (!dup) keep.Add(w);
            }
            return keep;
        }

        /// <summary>Alarga o recorta cada extremo hasta el eje del muro vecino (esquinas en L y uniones en T).</summary>
        private static void JoinWallCorners(List<Desing2DxfWallCandidate> walls)
        {
            if (walls.Count > 4000) return;
            for (var wi = 0; wi < walls.Count; wi++)
            {
                for (var end = 0; end < 2; end++)
                {
                    var A = walls[wi];
                    var ex = end == 0 ? A.X1 : A.X2;
                    var ey = end == 0 ? A.Y1 : A.Y2;
                    var ox = end == 0 ? A.X2 : A.X1;
                    var oy = end == 0 ? A.Y2 : A.Y1;
                    var aLen = Dist(ex, ey, ox, oy);
                    if (aLen < 1e-12) continue;
                    var ax = (ex - ox) / aLen;
                    var ay = (ey - oy) / aLen;
                    var bestD = double.MaxValue;
                    double nx = 0, ny = 0;
                    var found = false;
                    for (var wj = 0; wj < walls.Count; wj++)
                    {
                        if (wj == wi) continue;
                        var B = walls[wj];
                        var bLen = Dist(B.X1, B.Y1, B.X2, B.Y2);
                        if (bLen < 1e-12) continue;
                        var ux = (B.X2 - B.X1) / bLen;
                        var uy = (B.Y2 - B.Y1) / bLen;
                        var den = ax * uy - ay * ux;
                        if (Math.Abs(den) < 0.17) continue; // casi paralelos (< 10°)
                        // E + s·a = B1 + t·u
                        var wx = B.X1 - ex;
                        var wy = B.Y1 - ey;
                        var s = (wx * uy - wy * ux) / den;
                        var t = (wx * ay - wy * ax) / den;
                        var tol = Math.Max(A.Thickness, B.Thickness) * 0.75 + 1e-9;
                        if (Math.Abs(s) > tol || -s > aLen * 0.5) continue;
                        if (t < -tol || t > bLen + tol) continue;
                        if (Math.Abs(s) < bestD)
                        {
                            bestD = Math.Abs(s);
                            nx = ex + s * ax;
                            ny = ey + s * ay;
                            found = true;
                        }
                    }
                    if (!found) continue;
                    if (end == 0)
                    {
                        A.X1 = nx;
                        A.Y1 = ny;
                    }
                    else
                    {
                        A.X2 = nx;
                        A.Y2 = ny;
                    }
                }
            }
        }
    }
}
