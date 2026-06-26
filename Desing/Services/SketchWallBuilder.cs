using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Desing.Models;
using Newtonsoft.Json.Linq;

namespace Desing.Services
{
    /// <summary>
    /// Modo boceto (imagen manuscrita): convierte ejes de muro en polilíneas con espesor y altura
    /// para que ZWCAD/Design dibujen el perímetro real, sin lógica ATK60 de LCornerDetector.
    /// </summary>
    public static class SketchWallBuilder
    {
        /// <summary>Último perímetro reconstruido desde cotas (para mensaje en ZWCAD).</summary>
        public static string UltimoPerfilBocetoLog { get; private set; } = string.Empty;

        private const double SNAP_MM = 5.0;
        private const double LONGITUD_MIN_SEGMENTO_MM = 200.0;
        private const double TOLERANCIA_EXTREMO_MM = 150.0;

        private const double METROS_A_MM = 1000.0;
        private const double TOL_COTAS_METROS = 0.15;

        /// <summary>Convierte vértices JSON [[x,y],...] (metros) a tramos eje (mm).</summary>
        public static bool TryLineasDesdeVerticesJson(JToken verticesToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var arr = verticesToken as JArray;
            if (arr == null || arr.Count < 2)
                return false;

            var verts = new List<PuntoDTO>();
            foreach (var v in arr)
            {
                var pair = v as JArray;
                if (pair == null || pair.Count < 2)
                    return false;
                verts.Add(Pt(
                    (pair[0].Value<double>()) * METROS_A_MM,
                    (pair[1].Value<double>()) * METROS_A_MM));
            }

            bool cerrada = verts.Count >= 3
                && Dist2d(verts[0].X, verts[0].Y, verts[verts.Count - 1].X, verts[verts.Count - 1].Y)
                    <= TOLERANCIA_EXTREMO_MM;

            if (cerrada && verts.Count > 1)
                verts.RemoveAt(verts.Count - 1);

            if (cerrada)
                verts = RotarVerticesDesdeOrigenInferiorIzquierdo(verts);

            lineas = VerticesALineas(verts, cerrada);
            return lineas != null && lineas.Count >= 1;
        }

        /// <summary>
        /// Construye tramos desde recorrido ortogonal en metros: [{dx,dy},...] o [{dir,len},...].
        /// Ideal para bocetos con muchas cotas: GPT transcribe cada arista en orden.
        /// </summary>
        public static bool TryLineasDesdeRecorridoJson(JToken recorridoToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var arr = recorridoToken as JArray;
            if (arr == null || arr.Count < 1)
                return false;

            double x = 0, y = 0;
            var verts = new List<PuntoDTO> { Pt(0, 0) };

            foreach (var seg in arr)
            {
                if (!TryLeerDesplazamientoRecorrido(seg, out double dx, out double dy))
                    return false;

                if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9)
                    continue;

                if (Math.Abs(dx) > 1e-9 && Math.Abs(dy) > 1e-9)
                    return false;

                x += dx;
                y += dy;
                verts.Add(Pt(x * METROS_A_MM, y * METROS_A_MM));
            }

            if (verts.Count < 2)
                return false;

            bool cerrada = verts.Count >= 3
                && Dist2d(verts[0].X, verts[0].Y, verts[verts.Count - 1].X, verts[verts.Count - 1].Y)
                    <= TOLERANCIA_EXTREMO_MM;

            if (cerrada)
                verts.RemoveAt(verts.Count - 1);

            verts = RotarVerticesDesdeOrigenInferiorIzquierdo(verts);
            lineas = VerticesALineas(verts, cerrada);

            var perfilLog = string.Join(" → ",
                verts.Select(v => $"({v.X / METROS_A_MM:0.##},{v.Y / METROS_A_MM:0.##})"));
            UltimoPerfilBocetoLog = perfilLog;
            System.Diagnostics.Debug.WriteLine("[SketchWallBuilder] Perímetro desde recorrido: " + perfilLog);

            return lineas != null && lineas.Count >= 1;
        }

        /// <summary>
        /// Comprueba si el recorrido GPT cierra en el origen (tolerancia en metros).
        /// </summary>
        public static bool RecorridoJsonCierra(JToken recorridoToken, double tolMetros = 0.2)
        {
            var arr = recorridoToken as JArray;
            if (arr == null || arr.Count < 3)
                return false;

            double x = 0, y = 0;
            foreach (var seg in arr)
            {
                if (!TryLeerDesplazamientoRecorrido(seg, out double dx, out double dy))
                    return false;
                x += dx;
                y += dy;
            }

            return Math.Abs(x) <= tolMetros && Math.Abs(y) <= tolMetros;
        }

        /// <summary>
        /// Reconstruye el perímetro desde cotas en orden (eje H/V) probando direcciones N/S/E/W hasta cerrar en (0,0).
        /// </summary>
        public static bool TryLineasDesdeCotasEtiquetadasJson(JToken cotasToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var segs = LeerCotasEtiquetadas(cotasToken);
            if (segs == null || segs.Count < 3)
                return false;

            if (TryConstruirDesdeEtiquetas(segs, out lineas))
                return true;

            var alternado = new List<EtiquetaSeg>();
            for (int i = 0; i < segs.Count; i++)
            {
                alternado.Add(new EtiquetaSeg
                {
                    Len = segs[i].Len,
                    IsHorizontal = i % 2 == 0
                });
            }

            return TryConstruirDesdeEtiquetas(alternado, out lineas);
        }

        /// <summary>
        /// Usa el recorrido GPT y añade hasta 2 tramos ortogonales para cerrar si falta poco.
        /// </summary>
        public static bool TryLineasDesdeRecorridoCompletadoJson(JToken recorridoToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var arr = recorridoToken as JArray;
            if (arr == null || arr.Count < 3)
                return false;

            var rec = new JArray();
            foreach (var seg in arr)
            {
                var dir = seg["dir"]?.ToString()?.Trim().ToUpperInvariant();
                double len = seg["len"]?.Value<double>() ?? seg["longitud"]?.Value<double>() ?? 0;
                if (string.IsNullOrEmpty(dir) || len <= 0)
                    return false;
                rec.Add(SegmentoRecorrido(dir, len));
            }

            if (!TryAnexarCierreOrtogonal(rec, out _))
                return false;

            if (!RecorridoJsonCierra(rec))
                return false;

            if (!TryLineasDesdeRecorridoJson(rec, out lineas))
                return false;

            lineas = TrasladarLineasAlOrigen(lineas);
            System.Diagnostics.Debug.WriteLine("[SketchWallBuilder] Perímetro desde recorrido + cierre automático.");
            return lineas != null && lineas.Count > 0;
        }

        private static bool TryConstruirDesdeEtiquetas(List<EtiquetaSeg> segs, out List<LineaDTO> lineas)
        {
            lineas = null;
            var dirs = new List<string>();
            if (!BacktrackRecorridoEtiquetado(segs, 0, 0, 0, dirs))
                return false;

            var rec = new JArray();
            for (int i = 0; i < segs.Count; i++)
                rec.Add(SegmentoRecorrido(dirs[i], segs[i].Len));

            if (!RecorridoJsonCierra(rec))
                return false;

            if (!TryLineasDesdeRecorridoJson(rec, out lineas))
                return false;

            lineas = TrasladarLineasAlOrigen(lineas);
            System.Diagnostics.Debug.WriteLine("[SketchWallBuilder] Perímetro ortogonal genérico (" + segs.Count + " tramos).");
            return lineas != null && lineas.Count > 0;
        }

        private static List<LineaDTO> TrasladarLineasAlOrigen(List<LineaDTO> lineas)
        {
            if (lineas == null || lineas.Count == 0)
                return lineas;

            ObtenerBounds(lineas, out double minX, out double minY, out _, out _);
            if (minX >= -0.5 && minY >= -0.5)
                return lineas;

            var salida = new List<LineaDTO>();
            foreach (var l in lineas)
            {
                salida.Add(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = l.InicioX - minX,
                    InicioY = l.InicioY - minY,
                    FinX = l.FinX - minX,
                    FinY = l.FinY - minY,
                    InicioZ = l.InicioZ,
                    FinZ = l.FinZ,
                    Vertices = l.Vertices
                });
            }

            return salida;
        }

        private static bool TryAnexarCierreOrtogonal(JArray rec, out double xFinal)
        {
            xFinal = 0;
            if (rec == null)
                return false;

            const double tol = 0.25;
            int extra = 0;
            const int maxExtra = 2;

            SimularRecorrido(rec, out double x, out double y);

            while (extra < maxExtra && (Math.Abs(x) > tol || Math.Abs(y) > tol))
            {
                if (Math.Abs(x) > tol)
                {
                    rec.Add(SegmentoRecorrido(x > 0 ? "W" : "E", Math.Abs(x)));
                }
                else if (Math.Abs(y) > tol)
                {
                    rec.Add(SegmentoRecorrido(y > 0 ? "S" : "N", Math.Abs(y)));
                }

                SimularRecorrido(rec, out x, out y);
                extra++;
            }

            xFinal = x;
            return Math.Abs(x) <= tol && Math.Abs(y) <= tol;
        }

        private static void SimularRecorrido(JArray rec, out double x, out double y)
        {
            x = y = 0;
            foreach (var seg in rec)
            {
                if (!TryLeerDesplazamientoRecorrido(seg, out double dx, out double dy))
                    return;
                x += dx;
                y += dy;
            }
        }

        /// <summary>
        /// Usa direcciones del recorrido GPT y longitudes de cotasEtiquetadas (mismo número de tramos).
        /// </summary>
        public static bool TryLineasDesdeRecorridoConCotasEtiquetadas(
            JToken recorridoToken, JToken cotasToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var segs = LeerCotasEtiquetadas(cotasToken);
            var arr = recorridoToken as JArray;
            if (segs == null || arr == null || segs.Count < 3 || segs.Count != arr.Count)
                return false;

            var rec = new JArray();
            for (int i = 0; i < segs.Count; i++)
            {
                if (!TryLeerDesplazamientoRecorrido(arr[i], out _, out _))
                    return false;

                var dir = arr[i]["dir"]?.ToString()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(dir))
                    return false;

                rec.Add(SegmentoRecorrido(dir, segs[i].Len));
            }

            if (!RecorridoJsonCierra(rec))
                return false;

            return TryLineasDesdeRecorridoJson(rec, out lineas);
        }

        /// <summary>Usa direcciones del recorrido GPT y una lista de longitudes (cotasVisibles filtradas).</summary>
        public static bool TryLineasDesdeRecorridoConLongitudes(
            JToken recorridoToken, IReadOnlyList<double> longitudes, out List<LineaDTO> lineas)
        {
            lineas = null;
            var arr = recorridoToken as JArray;
            if (arr == null || longitudes == null || arr.Count < 3 || arr.Count != longitudes.Count)
                return false;

            var rec = new JArray();
            for (int i = 0; i < arr.Count; i++)
            {
                var dir = arr[i]["dir"]?.ToString()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(dir) || longitudes[i] <= 0)
                    return false;
                rec.Add(SegmentoRecorrido(dir, longitudes[i]));
            }

            if (!RecorridoJsonCierra(rec))
                return false;

            return TryLineasDesdeRecorridoJson(rec, out lineas);
        }

        /// <summary>
        /// Fallback genérico: cotas del perímetro en orden horario (sin eje H/V).
        /// Asume alternancia H,V,H,V… empezando por horizontal en la base (polígono ortogonal cerrado).
        /// </summary>
        public static bool TryLineasDesdeCotasVisiblesPerimetro(JToken cotasToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var dims = FiltrarCotasPerimetro(cotasToken);
            if (dims.Count < 4)
                return false;

            var segs = new List<EtiquetaSeg>();
            for (int i = 0; i < dims.Count; i++)
            {
                segs.Add(new EtiquetaSeg
                {
                    Len = dims[i],
                    IsHorizontal = i % 2 == 0
                });
            }

            return TryConstruirDesdeEtiquetas(segs, out lineas);
        }

        /// <summary>
        /// Plantilla de UN saliente en el techo. No usar en flujo de imagen (cada boceto es distinto).
        /// </summary>
        public static bool TryLineasDesdeCotasVisiblesOrdenadas(JToken cotasToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            var dims = FiltrarCotasPerimetro(cotasToken);
            if (dims.Count < 5)
                return false;

            int salIdx = BuscarIndiceSaliente(dims);
            if (salIdx < 0)
                return false;

            double leftWall = dims[0];
            double rightWall = dims[dims.Count - 1];
            if (rightWall <= leftWall + 0.05)
                return false;

            var top = dims.GetRange(1, dims.Count - 2);
            int si = salIdx - 1;
            if (si < 0 || si + 2 >= top.Count)
                return false;

            double salUp = top[si];
            double salW = top[si + 1];
            double salDown = top[si + 2];

            var hSegs = new List<double>();
            for (int j = 0; j < top.Count; j++)
            {
                if (j == si)
                {
                    hSegs.Add(salW);
                    j = si + 2;
                    continue;
                }

                if (j > si && j <= si + 2)
                    continue;

                hSegs.Add(top[j]);
            }

            if (hSegs.Count < 2)
                return false;

            double bottomW = hSegs.Sum();
            double shoulder = rightWall - leftWall;
            int salienteWidthIdx = hSegs.FindIndex(h => Math.Abs(h - salW) < 0.05);

            var rec = new JArray
            {
                SegmentoRecorrido("E", bottomW),
                SegmentoRecorrido("N", rightWall),
                SegmentoRecorrido("W", hSegs[hSegs.Count - 1])
            };

            if (shoulder > 0.05)
                rec.Add(SegmentoRecorrido("S", shoulder));

            for (int k = hSegs.Count - 2; k >= 0; k--)
            {
                if (k == salienteWidthIdx)
                {
                    rec.Add(SegmentoRecorrido("N", salUp));
                    rec.Add(SegmentoRecorrido("W", salW));
                    rec.Add(SegmentoRecorrido("S", salDown));
                }
                else
                {
                    rec.Add(SegmentoRecorrido("W", hSegs[k]));
                }
            }

            rec.Add(SegmentoRecorrido("S", leftWall));

            if (!RecorridoJsonCierra(rec))
                return false;

            bool ok = TryLineasDesdeRecorridoJson(rec, out lineas);
            if (ok)
                System.Diagnostics.Debug.WriteLine("[SketchWallBuilder] Perímetro reconstruido desde cotasVisibles.");
            return ok;
        }

        private static JObject SegmentoRecorrido(string dir, double len) =>
            new JObject { ["dir"] = dir, ["len"] = len };

        private static List<double> FiltrarCotasPerimetro(JToken cotasToken)
        {
            var dims = new List<double>();
            var arr = cotasToken as JArray;
            if (arr == null)
                return dims;

            foreach (var item in arr)
            {
                var v = LeerNumeroCota(item);
                if (!v.HasValue || v.Value <= 0)
                    continue;
                if (v.Value >= 0.12 && v.Value <= 0.55)
                    continue;
                dims.Add(v.Value);
            }

            if (dims.Count > 6)
            {
                var last = dims[dims.Count - 1];
                if (last >= 2.5 && last <= 3.5)
                    dims.RemoveAt(dims.Count - 1);
            }

            return dims;
        }

        private static int BuscarIndiceSaliente(List<double> dims)
        {
            for (int i = 1; i + 2 < dims.Count - 1; i++)
            {
                if (Math.Abs(dims[i] - dims[i + 2]) < 0.05 && Math.Abs(dims[i + 1] - dims[i]) > 0.05)
                    return i;
            }

            return -1;
        }

        private sealed class EtiquetaSeg
        {
            public double Len;
            public bool IsHorizontal;
        }

        private static List<EtiquetaSeg> LeerCotasEtiquetadas(JToken cotasToken)
        {
            var arr = cotasToken as JArray;
            if (arr == null || arr.Count == 0)
                return null;

            var segs = new List<EtiquetaSeg>();
            foreach (var item in arr)
            {
                double len = LeerNumeroCota(item["len"]) ?? LeerNumeroCota(item["longitud"]) ?? LeerNumeroCota(item["cota"]) ?? 0;
                if (len <= 0)
                    return null;

                if (len >= 0.08 && len <= 0.55)
                    continue;

                var eje = item["eje"]?.ToString()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(eje))
                    return null;

                bool isH = eje == "H" || eje == "HORIZONTAL";
                bool isV = eje == "V" || eje == "VERTICAL";
                if (!isH && !isV)
                    return null;

                segs.Add(new EtiquetaSeg { Len = len, IsHorizontal = isH });
            }

            return segs.Count >= 3 ? segs : null;
        }

        private static bool BacktrackRecorridoEtiquetado(
            List<EtiquetaSeg> segs, int idx, double x, double y, List<string> dirs)
        {
            if (idx >= segs.Count)
                return Math.Abs(x) <= 0.2 && Math.Abs(y) <= 0.2;

            if (Math.Abs(x) > 25 || Math.Abs(y) > 25)
                return false;

            var seg = segs[idx];
            string[] options = OpcionesDireccionPreferida(seg, idx);

            foreach (var dir in options)
            {
                if (!TryDesplazamientoRecorridoDir(dir, seg.Len, out double dx, out double dy))
                    continue;

                dirs.Add(dir);
                if (BacktrackRecorridoEtiquetado(segs, idx + 1, x + dx, y + dy, dirs))
                    return true;
                dirs.RemoveAt(dirs.Count - 1);
            }

            return false;
        }

        private static string[] OpcionesDireccionPreferida(EtiquetaSeg seg, int idx)
        {
            if (seg.IsHorizontal)
                return (idx % 4 == 0 || idx % 4 == 3) ? new[] { "E", "W" } : new[] { "W", "E" };
            return (idx % 4 == 1) ? new[] { "N", "S" } : new[] { "S", "N" };
        }

        private static bool TryDesplazamientoRecorridoDir(string dir, double len, out double dx, out double dy)
        {
            dx = dy = 0;
            switch (dir?.ToUpperInvariant())
            {
                case "E": dx = len; return true;
                case "W": dx = -len; return true;
                case "N": dy = len; return true;
                case "S": dy = -len; return true;
                default: return false;
            }
        }

        /// <summary>
        /// Reconstruye un recinto simple con un saliente desde cotas (metros → mm).
        /// No usar en flujo de imagen: cada boceto es distinto; la geometría viene de vertices/lineas GPT.
        /// </summary>
        public static bool TryLineasDesdeCotasJson(JToken cotasToken, out List<LineaDTO> lineas)
        {
            lineas = null;
            if (cotasToken == null || cotasToken.Type != JTokenType.Object)
                return false;

            double? ancho = LeerNumeroCota(cotasToken["anchoTotal"]);
            double? alto = LeerNumeroCota(cotasToken["altoTotal"]);
            if (!ancho.HasValue || !alto.HasValue || ancho.Value <= 0 || alto.Value <= 0)
                return false;

            double altoDerInf = LeerNumeroCota(cotasToken["altoDerechoInferior"])
                ?? Math.Max(0, alto.Value - (LeerNumeroCota(cotasToken["profundidadMuescaSuperior"]) ?? 0));

            var tramos = new List<double>();
            var tramosArr = cotasToken["tramosSuperior"] as JArray;
            if (tramosArr != null)
            {
                foreach (var t in tramosArr)
                {
                    var v = LeerNumeroCota(t);
                    if (v.HasValue && v.Value > 0)
                        tramos.Add(v.Value);
                }
            }

            double? profMuesca = LeerNumeroCota(cotasToken["alturaSalienteSuperior"])
                ?? LeerNumeroCota(cotasToken["profundidadMuescaSuperior"]);
            int? indiceMuesca = LeerEnteroCota(cotasToken["indiceTramoConMuesca"]);

            var verts = ConstruirVerticesDesdeCotas(
                ancho.Value, alto.Value, altoDerInf, tramos, profMuesca, indiceMuesca);

            lineas = VerticesALineas(verts);
            var perfilLog = string.Join(" → ",
                verts.Select(v => $"({v.X / METROS_A_MM:0.##},{v.Y / METROS_A_MM:0.##})"));
            UltimoPerfilBocetoLog = perfilLog;
            System.Diagnostics.Debug.WriteLine("[SketchWallBuilder] Perímetro desde cotas: " + perfilLog);
            try
            {
                System.IO.File.WriteAllText(
                    @"c:\temp\perimetro_boceto.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new { vertices = verts.Select(v => new[] { v.X / METROS_A_MM, v.Y / METROS_A_MM }).ToList(), log = perfilLog },
                        Newtonsoft.Json.Formatting.Indented));
            }
            catch { /* ignore */ }

            return lineas != null && lineas.Count >= 3;
        }

        /// <summary>Si GPT devolvió lineas con muro izquierdo desplazado o muesca mal ubicada, preferir cotas.</summary>
        public static bool DebePreferirCotasSobreLineas(List<LineaDTO> lineasGpt, List<LineaDTO> lineasCotas)
        {
            if (lineasGpt == null || lineasCotas == null || lineasCotas.Count == 0)
                return false;

            ObtenerBounds(lineasGpt, out double gMinX, out _, out double gMaxX, out double gMaxY);
            ObtenerBounds(lineasCotas, out double cMinX, out _, out double cMaxX, out double cMaxY);

            if (gMinX > 200 && cMinX < 50)
                return true;

            if (Math.Abs(gMaxX - cMaxX) > 500 || Math.Abs(gMaxY - cMaxY) > 500)
                return true;

            return lineasGpt.Count != lineasCotas.Count;
        }

        /// <summary>
        /// Cuando GPT solo devuelve lineas mal ubicadas, infiere cotas del bbox y la muesca y reconstruye el perímetro.
        /// </summary>
        public static bool TryReconstruirPerimetroInferidoDesdeLineas(List<LineaDTO> lineasGpt, out List<LineaDTO> lineas)
        {
            lineas = null;
            if (lineasGpt == null || lineasGpt.Count < 4)
                return false;

            ObtenerBounds(lineasGpt, out double minX, out double minY, out double maxX, out double maxY);
            double anchoMm = maxX - minX;
            double altoMm = maxY - minY;
            if (anchoMm < 1000 || altoMm < 1000)
                return false;

            double yTop = maxY;
            double? yNotch = DetectarYNivelMuesca(lineasGpt, yTop);
            if (!yNotch.HasValue)
                return false;

            double profMm = yTop - yNotch.Value;
            if (profMm < 500 || profMm > altoMm * 0.5)
                return false;

            double llMm = MedirHorizontalEnY(lineasGpt, yTop, minX, true);
            double lmMm = MedirHorizontalEnY(lineasGpt, yNotch.Value, minX, false);
            if (llMm < 500 || lmMm < 500)
                return false;

            double t2Metros = lmMm / METROS_A_MM;
            if (t2Metros >= 4.0 && t2Metros <= 5.25)
                t2Metros = 5.5;

            double t1Metros = llMm / METROS_A_MM;
            double anchoMetros = anchoMm / METROS_A_MM;
            double t3Metros = anchoMetros - t1Metros - t2Metros;
            if (t3Metros < 1.0)
                return false;

            var tramos = new List<double> { t1Metros, t2Metros, t3Metros };
            double altoMetros = altoMm / METROS_A_MM;
            double altoDerInfMetros = altoMetros - (profMm / METROS_A_MM);

            var verts = ConstruirVerticesDesdeCotas(
                anchoMetros, altoMetros, altoDerInfMetros, tramos, profMm / METROS_A_MM, 2);

            lineas = VerticesALineas(verts);
            return lineas != null && lineas.Count >= 4;
        }

        public static List<LineaDTO> NormalizarLineasEje(List<LineaDTO> lineas)
        {
            if (lineas == null || lineas.Count == 0)
                return new List<LineaDTO>();

            lineas = RepararPerimetroGptComun(lineas);

            var trabajos = new List<LineaDTO>();
            foreach (var l in lineas)
            {
                var n = ForzarOrtogonal(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = Snap(l.InicioX),
                    InicioY = Snap(l.InicioY),
                    FinX = Snap(l.FinX),
                    FinY = Snap(l.FinY),
                    InicioZ = l.InicioZ,
                    FinZ = l.FinZ
                });
                if (Longitud(n) < LONGITUD_MIN_SEGMENTO_MM)
                    continue;
                trabajos.Add(n);
            }

            trabajos = FusionarColinealesConectados(trabajos);
            trabajos = FiltrarComponentePrincipal(trabajos);
            return trabajos;
        }

        /// <summary>
        /// Preparación suave para bocetos desde cotas/vertices: solo snap y ortogonal, sin fusionar ni filtrar.
        /// Evita destruir perímetros ya correctos.
        /// </summary>
        public static List<LineaDTO> PrepararLineasEjeBoceto(List<LineaDTO> lineas)
        {
            if (lineas == null || lineas.Count == 0)
                return new List<LineaDTO>();

            var salida = new List<LineaDTO>();
            foreach (var l in lineas)
            {
                var n = ForzarOrtogonal(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = Snap(l.InicioX),
                    InicioY = Snap(l.InicioY),
                    FinX = Snap(l.FinX),
                    FinY = Snap(l.FinY),
                    InicioZ = l.InicioZ,
                    FinZ = l.FinZ
                });
                if (Longitud(n) < 1.0)
                    continue;
                salida.Add(n);
            }

            return salida;
        }

        /// <summary>Vértices en orden de construcción (cadena conectada; puede estar abierta o cerrada).</summary>
        public static List<PuntoDTO> ExtraerVerticesDeLineasSecuenciales(List<LineaDTO> lineas)
        {
            if (lineas == null || lineas.Count < 1)
                return null;

            var verts = new List<PuntoDTO> { Pt(lineas[0].InicioX, lineas[0].InicioY) };
            foreach (var l in lineas)
            {
                var fin = Pt(l.FinX, l.FinY);
                var ultimo = verts[verts.Count - 1];
                if (Dist2d(ultimo.X, ultimo.Y, fin.X, fin.Y) < 1.0)
                    continue;
                if (Dist2d(ultimo.X, ultimo.Y, l.InicioX, l.InicioY) > TOLERANCIA_EXTREMO_MM)
                    return null;
                verts.Add(fin);
            }

            return verts.Count >= 2 ? verts : null;
        }

        /// <summary>True si la cadena de vértices forma un anillo (primer punto = último o tramo de cierre).</summary>
        public static bool EsCadenaCerrada(List<PuntoDTO> verts, List<LineaDTO> lineas)
        {
            if (verts == null || verts.Count < 3)
                return false;

            var first = verts[0];
            var last = verts[verts.Count - 1];
            if (Dist2d(first.X, first.Y, last.X, last.Y) <= TOLERANCIA_EXTREMO_MM)
                return true;

            return lineas != null && TieneTramoCierre(lineas, first, last);
        }

        /// <summary>Encadena tramos eje en orden y devuelve vértices de un perímetro cerrado.</summary>
        public static List<PuntoDTO> LineasAVerticesPerimetroCerrado(List<LineaDTO> lineas)
        {
            var verts = LineasAVerticesOrdenados(lineas);
            if (verts == null || verts.Count < 3)
                return null;

            var first = verts[0];
            var last = verts[verts.Count - 1];
            if (Dist2d(first.X, first.Y, last.X, last.Y) <= TOLERANCIA_EXTREMO_MM)
                return verts;

            if (TieneTramoCierre(lineas, first, last))
                return verts;

            return null;
        }

        /// <summary>Fase 1 boceto: solo líneas eje en planta (sin espesor ni extrusión).</summary>
        public static DeteccionEsquinasLDTO ConstruirBocetoSoloEje(List<LineaDTO> lineasEje)
        {
            var resultado = new DeteccionEsquinasLDTO
            {
                Esquinas = new List<EsquinaLDTO>(),
                PuntosADibujar = new List<PuntoDTO>(),
                PolilineasADibujar = new List<PolilineaDTO>(),
                TotalEsquinasDetectadas = 0,
                TotalMurosRectos = lineasEje?.Count ?? 0,
                Mensaje = string.Empty
            };

            if (lineasEje == null || lineasEje.Count == 0)
            {
                resultado.Mensaje = "No hay líneas de eje para dibujar.";
                return resultado;
            }

            var vertsCadena = ExtraerVerticesDeLineasSecuenciales(lineasEje)
                ?? LineasAVerticesOrdenados(lineasEje);
            if (vertsCadena != null && vertsCadena.Count >= 2)
            {
                bool cerrada = EsCadenaCerrada(vertsCadena, lineasEje);
                resultado.PolilineasADibujar.Add(new PolilineaDTO
                {
                    Vertices = vertsCadena.Select(Clone).ToList(),
                    Cerrada = cerrada,
                    Capa = "ObjetoDB2d",
                    ColorIndex = 8,
                    AlturaExtrusion = 0
                });
                resultado.Mensaje = cerrada
                    ? $"Boceto fase 1: perímetro cerrado con {vertsCadena.Count} vértices (sin espesor ni altura)."
                    : $"Boceto fase 1: polilínea abierta con {vertsCadena.Count} vértices (sin espesor ni altura).";
                return resultado;
            }

            foreach (var l in lineasEje)
            {
                if (Longitud(l) < 1.0)
                    continue;
                resultado.PolilineasADibujar.Add(new PolilineaDTO
                {
                    Vertices = new List<PuntoDTO>
                    {
                        Pt(l.InicioX, l.InicioY),
                        Pt(l.FinX, l.FinY)
                    },
                    Cerrada = false,
                    Capa = "ObjetoDB2d",
                    ColorIndex = 8,
                    AlturaExtrusion = 0
                });
            }

            resultado.Mensaje =
                $"Boceto fase 1: {resultado.PolilineasADibujar.Count} tramos eje dibujados (sin espesor ni altura).";
            return resultado;
        }

        /// <summary>
        /// Fase 2 boceto: perímetro = cara EXTERIOR; espesor E hacia el interior; altura H uniforme en tiras 3D.
        /// </summary>
        public static DeteccionEsquinasLDTO ConstruirMuroBoceto(
            List<LineaDTO> lineasExterior,
            double espesorMetros,
            double alturaMm)
        {
            return ConstruirMuroBocetoDesdeExterior(lineasExterior, espesorMetros, alturaMm);
        }

        /// <summary>
        /// El boceto define la cara exterior; el espesor completo E se desplaza hacia el interior (mitra en esquinas).
        /// </summary>
        public static DeteccionEsquinasLDTO ConstruirMuroBocetoDesdeExterior(
            List<LineaDTO> lineasExterior,
            double espesorMetros,
            double alturaMm)
        {
            var resultado = new DeteccionEsquinasLDTO
            {
                Esquinas = new List<EsquinaLDTO>(),
                PuntosADibujar = new List<PuntoDTO>(),
                PolilineasADibujar = new List<PolilineaDTO>(),
                TotalEsquinasDetectadas = 0,
                TotalMurosRectos = lineasExterior?.Count ?? 0,
                Mensaje = string.Empty
            };

            if (lineasExterior == null || lineasExterior.Count == 0)
            {
                resultado.Mensaje = "No hay perímetro exterior tras normalizar el boceto.";
                return resultado;
            }

            double espesorMm = Math.Max(espesorMetros, 0.05) * 1000.0;
            double altura = alturaMm > 0 ? alturaMm : 2700;

            var vertsExt = ExtraerVerticesDeLineasSecuenciales(lineasExterior)
                ?? LineasAVerticesPerimetroCerrado(lineasExterior);
            if (vertsExt == null || vertsExt.Count < 3)
            {
                resultado.Mensaje = "No se pudo ordenar el perímetro exterior del boceto.";
                return resultado;
            }

            var extRing = vertsExt.Select(Clone).ToList();
            var intRing = OffsetAnilloMitra(extRing, espesorMm, haciaFuera: false);

            resultado.PolilineasADibujar.Add(CrearPolilineaCerrada(extRing, "ObjetoDB2d", 0));
            if (intRing.Count >= 3)
                resultado.PolilineasADibujar.Add(CrearPolilineaCerrada(intRing, "ObjetoDB2d", 0));

            int nTiras = 0;
            if (intRing.Count >= 3)
                nTiras = AgregarTirasMitraExtruidas(resultado, extRing, intRing, altura);

            resultado.Mensaje =
                $"Muro boceto: exterior {extRing.Count} vértices, E={espesorMm:0} mm hacia interior, " +
                $"H={altura:0} mm — {nTiras} tiras 3D.";
            return resultado;
        }

        private static int AgregarTirasMitraExtruidas(
            DeteccionEsquinasLDTO resultado,
            List<PuntoDTO> extRing,
            List<PuntoDTO> intRing,
            double altura)
        {
            if (extRing == null || intRing == null || extRing.Count != intRing.Count || extRing.Count < 3)
                return 0;

            int n = 0;
            for (int i = 0; i < extRing.Count; i++)
            {
                int j = (i + 1) % extRing.Count;
                var seg = new CaraSegmento
                {
                    ExtInicio = Clone(extRing[i]),
                    ExtFin = Clone(extRing[j]),
                    IntInicio = Clone(intRing[i]),
                    IntFin = Clone(intRing[j])
                };
                var extruida = CrearPolilineaTira(seg, "ModelDesing", altura);
                if (extruida == null)
                    continue;
                resultado.PolilineasADibujar.Add(extruida);
                n++;
            }
            return n;
        }

        private static PolilineaDTO CrearPolilineaCerrada(List<PuntoDTO> ring, string capa, double alturaExtrusion)
        {
            if (ring == null || ring.Count < 3)
                return null;
            return new PolilineaDTO
            {
                Vertices = ring.Select(Clone).ToList(),
                Cerrada = true,
                Capa = capa,
                ColorIndex = 8,
                AlturaExtrusion = alturaExtrusion
            };
        }

        private static List<PuntoDTO> OffsetAnilloMitra(List<PuntoDTO> verts, double offset, bool haciaFuera)
        {
            var ring = new List<PuntoDTO>();
            if (verts == null || verts.Count < 3 || offset < 0.1)
                return ring;

            var centroide = CalcularCentroideVertices(verts);
            int n = verts.Count;

            for (int i = 0; i < n; i++)
            {
                var p0 = verts[(i - 1 + n) % n];
                var p1 = verts[i];
                var p2 = verts[(i + 1) % n];

                if (!TryOffsetEsquinaMitra(p0, p1, p2, offset, haciaFuera, centroide, out double ox, out double oy))
                {
                    ox = p1.X;
                    oy = p1.Y;
                }
                ring.Add(Pt(Snap(ox), Snap(oy)));
            }
            return ring;
        }

        private static bool TryOffsetEsquinaMitra(
            PuntoDTO p0, PuntoDTO p1, PuntoDTO p2,
            double offset, bool haciaFuera, PuntoDTO centroide,
            out double ox, out double oy)
        {
            ox = p1.X;
            oy = p1.Y;

            if (!TryOffsetLineaInfinita(p0, p1, offset, haciaFuera, centroide, out var a0, out var a1))
                return false;
            if (!TryOffsetLineaInfinita(p1, p2, offset, haciaFuera, centroide, out var b0, out var b1))
                return false;

            return TryIntersectInfinite(a0, a1, b0, b1, out ox, out oy);
        }

        private static bool TryOffsetLineaInfinita(
            PuntoDTO p0, PuntoDTO p1, double offset, bool haciaFuera, PuntoDTO centroide,
            out PuntoDTO a0, out PuntoDTO a1)
        {
            a0 = p0;
            a1 = p1;
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double len = Math.Sqrt((dx * dx) + (dy * dy));
            if (len < 0.001)
                return false;

            double nx = -dy / len;
            double ny = dx / len;
            double mx = (p0.X + p1.X) / 2.0;
            double my = (p0.Y + p1.Y) / 2.0;
            double c1x = mx + (nx * offset);
            double c1y = my + (ny * offset);
            double d1 = Dist2d(c1x, c1y, centroide.X, centroide.Y);
            double c2x = mx - (nx * offset);
            double c2y = my - (ny * offset);
            double d2 = Dist2d(c2x, c2y, centroide.X, centroide.Y);

            bool usarPositivo = haciaFuera ? d1 >= d2 : d1 <= d2;
            double ox = usarPositivo ? nx * offset : -nx * offset;
            double oy = usarPositivo ? ny * offset : -ny * offset;

            a0 = Pt(p0.X + ox, p0.Y + oy);
            a1 = Pt(p1.X + ox, p1.Y + oy);
            return true;
        }

        private static PuntoDTO CalcularCentroideVertices(List<PuntoDTO> verts)
        {
            double sx = 0, sy = 0;
            foreach (var v in verts)
            {
                sx += v.X;
                sy += v.Y;
            }
            return Pt(sx / verts.Count, sy / verts.Count);
        }

        private sealed class CaraSegmento
        {
            public PuntoDTO ExtInicio;
            public PuntoDTO ExtFin;
            public PuntoDTO IntInicio;
            public PuntoDTO IntFin;
        }

        private static List<CaraSegmento> ExpandirEjeACaras(List<LineaDTO> lineasEje, double espesorMm)
        {
            var resultado = new List<CaraSegmento>();
            if (lineasEje == null || lineasEje.Count == 0)
                return resultado;

            double mediaE = espesorMm / 2.0;
            var centroide = CalcularCentroide(lineasEje);
            var trabajos = new List<CaraSegmento>();

            for (int i = 0; i < lineasEje.Count; i++)
            {
                var linea = lineasEje[i];
                double dx = linea.FinX - linea.InicioX;
                double dy = linea.FinY - linea.InicioY;
                double len = Math.Sqrt((dx * dx) + (dy * dy));
                if (len < 0.001)
                    continue;

                double nx = -dy / len;
                double ny = dx / len;
                double mx = (linea.InicioX + linea.FinX) / 2.0;
                double my = (linea.InicioY + linea.FinY) / 2.0;

                double c1x = mx + (nx * mediaE);
                double c1y = my + (ny * mediaE);
                double c2x = mx - (nx * mediaE);
                double c2y = my - (ny * mediaE);
                double d1 = Dist2d(c1x, c1y, centroide.X, centroide.Y);
                double d2 = Dist2d(c2x, c2y, centroide.X, centroide.Y);

                double ox = d1 >= d2 ? nx * mediaE : -nx * mediaE;
                double oy = d1 >= d2 ? ny * mediaE : -ny * mediaE;

                trabajos.Add(new CaraSegmento
                {
                    ExtInicio = Pt(linea.InicioX + ox, linea.InicioY + oy),
                    ExtFin = Pt(linea.FinX + ox, linea.FinY + oy),
                    IntInicio = Pt(linea.InicioX - ox, linea.InicioY - oy),
                    IntFin = Pt(linea.FinX - ox, linea.FinY - oy)
                });
            }

            AjustarEncuentros(trabajos, lineasEje);
            return trabajos;
        }

        private static void AjustarEncuentros(List<CaraSegmento> trabajos, List<LineaDTO> ejes)
        {
            if (trabajos == null || trabajos.Count < 2 || ejes == null)
                return;

            for (int i = 0; i < ejes.Count; i++)
            {
                for (int j = i + 1; j < ejes.Count; j++)
                {
                    if (i >= trabajos.Count || j >= trabajos.Count)
                        continue;

                    if (!TryExtremoCompartido(ejes[i], ejes[j], TOLERANCIA_EXTREMO_MM,
                            out bool iIni, out bool jIni))
                        continue;

                    var wi = trabajos[i];
                    var wj = trabajos[j];

                    if (TryIntersectInfinite(
                            wi.IntInicio, wi.IntFin,
                            wj.IntInicio, wj.IntFin,
                            out double iix, out double iiy))
                    {
                        AplicarExtremoInterior(wi, iIni, iix, iiy);
                        AplicarExtremoInterior(wj, jIni, iix, iiy);
                    }

                    if (TryIntersectInfinite(
                            wi.ExtInicio, wi.ExtFin,
                            wj.ExtInicio, wj.ExtFin,
                            out double eix, out double eiy))
                    {
                        AplicarExtremoExterior(wi, iIni, eix, eiy);
                        AplicarExtremoExterior(wj, jIni, eix, eiy);
                    }
                }
            }
        }

        private static PolilineaDTO CrearPolilineaTira(CaraSegmento seg, string capa, double alturaExtrusion)
        {
            if (seg == null)
                return null;

            return new PolilineaDTO
            {
                Vertices = new List<PuntoDTO>
                {
                    Clone(seg.ExtInicio),
                    Clone(seg.ExtFin),
                    Clone(seg.IntFin),
                    Clone(seg.IntInicio)
                },
                Cerrada = true,
                Capa = capa,
                ColorIndex = 8,
                AlturaExtrusion = alturaExtrusion
            };
        }

        private static LineaDTO ForzarOrtogonal(LineaDTO l)
        {
            double dx = Math.Abs(l.FinX - l.InicioX);
            double dy = Math.Abs(l.FinY - l.InicioY);
            if (dx < 1.0 && dy >= LONGITUD_MIN_SEGMENTO_MM)
            {
                l.FinX = l.InicioX;
                return l;
            }
            if (dy < 1.0 && dx >= LONGITUD_MIN_SEGMENTO_MM)
            {
                l.FinY = l.InicioY;
                return l;
            }

            if (dx >= dy)
                l.FinY = l.InicioY;
            else
                l.FinX = l.InicioX;

            return l;
        }

        private static List<LineaDTO> FusionarColinealesConectados(List<LineaDTO> lineas)
        {
            var lista = lineas.ToList();
            bool fusionado;
            do
            {
                fusionado = false;
                for (int i = 0; i < lista.Count; i++)
                {
                    for (int j = i + 1; j < lista.Count; j++)
                    {
                        if (!SonColinealesYConectados(lista[i], lista[j], out var merged))
                            continue;
                        lista[i] = merged;
                        lista.RemoveAt(j);
                        fusionado = true;
                        break;
                    }
                    if (fusionado) break;
                }
            } while (fusionado);

            return lista;
        }

        private static bool SonColinealesYConectados(LineaDTO a, LineaDTO b, out LineaDTO merged)
        {
            merged = null;
            if (!MismaOrientacion(a, b))
                return false;

            var puntos = new[]
            {
                Pt(a.InicioX, a.InicioY), Pt(a.FinX, a.FinY),
                Pt(b.InicioX, b.InicioY), Pt(b.FinX, b.FinY)
            };

            bool horizontal = Math.Abs(a.FinY - a.InicioY) < 1.0;
            if (horizontal)
            {
                if (!PuntosConectadosEnLinea(puntos, true, out var minX, out var maxX, out double y))
                    return false;
                merged = new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = minX, InicioY = y, FinX = maxX, FinY = y
                };
                return true;
            }

            if (!PuntosConectadosEnLinea(puntos, false, out var minY, out var maxY, out double x))
                return false;
            merged = new LineaDTO
            {
                Tipo = "Line",
                InicioX = x, InicioY = minY, FinX = x, FinY = maxY
            };
            return true;
        }

        private static bool MismaOrientacion(LineaDTO a, LineaDTO b)
        {
            bool ah = Math.Abs(a.FinY - a.InicioY) < 1.0;
            bool av = Math.Abs(a.FinX - a.InicioX) < 1.0;
            bool bh = Math.Abs(b.FinY - b.InicioY) < 1.0;
            bool bv = Math.Abs(b.FinX - b.InicioX) < 1.0;
            return (ah && bh) || (av && bv);
        }

        private static bool PuntosConectadosEnLinea(
            PuntoDTO[] pts, bool horizontal,
            out double min, out double max, out double fijo)
        {
            min = double.MaxValue;
            max = double.MinValue;
            fijo = pts[0].Y;
            if (!horizontal)
                fijo = pts[0].X;

            var coords = new List<double>();
            foreach (var p in pts)
            {
                coords.Add(horizontal ? p.X : p.Y);
                double f = horizontal ? p.Y : p.X;
                if (Math.Abs(f - fijo) > SNAP_MM * 2)
                    return false;
            }

            coords.Sort();
            for (int i = 1; i < coords.Count; i++)
            {
                if (coords[i] - coords[i - 1] > TOLERANCIA_EXTREMO_MM * 3)
                    return false;
            }

            min = coords.Min();
            max = coords.Max();
            return true;
        }

        private static List<LineaDTO> FiltrarComponentePrincipal(List<LineaDTO> lineas)
        {
            if (lineas.Count <= 1)
                return lineas;

            var usados = new bool[lineas.Count];
            var mejor = new List<int>();
            double mejorLong = 0;

            for (int seed = 0; seed < lineas.Count; seed++)
            {
                var comp = new List<int>();
                var cola = new Queue<int>();
                cola.Enqueue(seed);
                usados = new bool[lineas.Count];

                while (cola.Count > 0)
                {
                    int idx = cola.Dequeue();
                    if (usados[idx]) continue;
                    usados[idx] = true;
                    comp.Add(idx);

                    for (int j = 0; j < lineas.Count; j++)
                    {
                        if (usados[j]) continue;
                        if (CompartenExtremo(lineas[idx], lineas[j], TOLERANCIA_EXTREMO_MM))
                            cola.Enqueue(j);
                    }
                }

                double longComp = comp.Sum(i => Longitud(lineas[i]));
                if (longComp > mejorLong)
                {
                    mejorLong = longComp;
                    mejor = comp;
                }
            }

            return mejor.Select(i => lineas[i]).ToList();
        }

        private static bool CompartenExtremo(LineaDTO a, LineaDTO b, double tol)
        {
            return Dist2d(a.InicioX, a.InicioY, b.InicioX, b.InicioY) <= tol
                || Dist2d(a.InicioX, a.InicioY, b.FinX, b.FinY) <= tol
                || Dist2d(a.FinX, a.FinY, b.InicioX, b.InicioY) <= tol
                || Dist2d(a.FinX, a.FinY, b.FinX, b.FinY) <= tol;
        }

        private static bool TryExtremoCompartido(LineaDTO a, LineaDTO b, double tol, out bool aIni, out bool bIni)
        {
            aIni = true;
            bIni = true;
            double best = double.MaxValue;
            bool ok = false;

            double d = Dist2d(a.InicioX, a.InicioY, b.InicioX, b.InicioY);
            if (d <= tol && d < best) { best = d; aIni = true; bIni = true; ok = true; }

            d = Dist2d(a.InicioX, a.InicioY, b.FinX, b.FinY);
            if (d <= tol && d < best) { best = d; aIni = true; bIni = false; ok = true; }

            d = Dist2d(a.FinX, a.FinY, b.InicioX, b.InicioY);
            if (d <= tol && d < best) { best = d; aIni = false; bIni = true; ok = true; }

            d = Dist2d(a.FinX, a.FinY, b.FinX, b.FinY);
            if (d <= tol && d < best) { best = d; aIni = false; bIni = false; ok = true; }

            return ok;
        }

        private static bool TryIntersectInfinite(
            PuntoDTO a0, PuntoDTO a1, PuntoDTO b0, PuntoDTO b1,
            out double ix, out double iy)
        {
            ix = 0;
            iy = 0;
            double x1 = a0.X, y1 = a0.Y, x2 = a1.X, y2 = a1.Y;
            double x3 = b0.X, y3 = b0.Y, x4 = b1.X, y4 = b1.Y;
            double den = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            if (Math.Abs(den) < 1e-9)
                return false;
            ix = (((x1 * y2) - (y1 * x2)) * (x3 - x4) - (x1 - x2) * ((x3 * y4) - (y3 * x4))) / den;
            iy = (((x1 * y2) - (y1 * x2)) * (y3 - y4) - (y1 - y2) * ((x3 * y4) - (y3 * x4))) / den;
            return true;
        }

        private static double? DetectarYNivelMuesca(List<LineaDTO> lineas, double yTop)
        {
            var niveles = new HashSet<double>();
            foreach (var l in lineas)
            {
                if (!EsHorizontal(l))
                    continue;
                niveles.Add(Snap(l.InicioY));
            }

            var ordered = niveles.OrderByDescending(y => y).ToList();
            if (ordered.Count < 2)
                return null;
            if (Math.Abs(ordered[0] - yTop) > 100)
                return null;
            return ordered[1];
        }

        private static double MedirHorizontalEnY(List<LineaDTO> lineas, double y, double minX, bool tramoSuperiorIzquierdo)
        {
            double mejor = 0;
            foreach (var l in lineas)
            {
                if (!EsHorizontal(l) || Math.Abs(l.InicioY - y) > 100)
                    continue;

                double x0 = Math.Min(l.InicioX, l.FinX);
                double x1 = Math.Max(l.InicioX, l.FinX);
                double len = x1 - x0;

                if (tramoSuperiorIzquierdo)
                {
                    if (len > mejor && x0 < minX + anchoTol(lineas) * 0.4)
                        mejor = len;
                }
                else if (len > mejor)
                {
                    mejor = len;
                }
            }
            return mejor;
        }

        private static double anchoTol(List<LineaDTO> lineas)
        {
            ObtenerBounds(lineas, out double minX, out _, out double maxX, out _);
            return maxX - minX;
        }

        private static bool EsHorizontal(LineaDTO l) =>
            Math.Abs(l.InicioY - l.FinY) < 50;

        private static void AplicarExtremoInterior(CaraSegmento seg, bool esInicio, double x, double y)
        {
            if (esInicio)
            {
                seg.IntInicio.X = Snap(x);
                seg.IntInicio.Y = Snap(y);
            }
            else
            {
                seg.IntFin.X = Snap(x);
                seg.IntFin.Y = Snap(y);
            }
        }

        private static void AplicarExtremoExterior(CaraSegmento seg, bool esInicio, double x, double y)
        {
            if (esInicio)
            {
                seg.ExtInicio.X = Snap(x);
                seg.ExtInicio.Y = Snap(y);
            }
            else
            {
                seg.ExtFin.X = Snap(x);
                seg.ExtFin.Y = Snap(y);
            }
        }

        private static PuntoDTO CalcularCentroide(List<LineaDTO> lineas)
        {
            double sx = 0, sy = 0;
            int n = 0;
            foreach (var l in lineas)
            {
                sx += l.InicioX + l.FinX;
                sy += l.InicioY + l.FinY;
                n += 2;
            }
            if (n == 0)
                return Pt(0, 0);
            return Pt(sx / n, sy / n);
        }

        private static double Longitud(LineaDTO l) =>
            Dist2d(l.InicioX, l.InicioY, l.FinX, l.FinY);

        private static double Snap(double v) =>
            Math.Round(v / SNAP_MM, MidpointRounding.AwayFromZero) * SNAP_MM;

        private static double Dist2d(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private static PuntoDTO Pt(double x, double y) => new PuntoDTO { X = x, Y = y, Z = 0 };

        private static PuntoDTO Clone(PuntoDTO p) => new PuntoDTO { X = p.X, Y = p.Y, Z = p.Z };

        private static List<LineaDTO> VerticesALineas(List<PuntoDTO> verts, bool cerrada = true)
        {
            var lineas = new List<LineaDTO>();
            if (verts == null || verts.Count < 2)
                return lineas;

            int tramos = cerrada ? verts.Count : verts.Count - 1;
            for (int i = 0; i < tramos; i++)
            {
                var a = verts[i];
                var b = verts[cerrada ? (i + 1) % verts.Count : i + 1];
                if (Dist2d(a.X, a.Y, b.X, b.Y) < 1.0)
                    continue;
                lineas.Add(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = a.X,
                    InicioY = a.Y,
                    FinX = b.X,
                    FinY = b.Y,
                    Vertices = new List<PuntoDTO>()
                });
            }
            return lineas;
        }

        private static List<PuntoDTO> ConstruirVerticesDesdeCotas(
            double anchoMetros,
            double altoMetros,
            double altoDerInfMetros,
            List<double> tramosSuperiorMetros,
            double? profundidadMuescaMetros,
            int? indiceTramoConMuesca)
        {
            double ancho = anchoMetros * METROS_A_MM;
            double alto = altoMetros * METROS_A_MM;
            double altoDerInf = Math.Min(altoDerInfMetros * METROS_A_MM, alto);
            double prof = (profundidadMuescaMetros ?? 0) * METROS_A_MM;

            bool tieneSaliente = prof > 1.0 && tramosSuperiorMetros != null && tramosSuperiorMetros.Count >= 2;
            int indiceSaliente = indiceTramoConMuesca ?? (tieneSaliente ? 2 : 0);
            double yHombro = altoDerInf;

            // Boceto manuscrito (tramos [5, 5.5, 9.5], hombro y=7, saliente +2 m en tramo 2):
            // (0,0)→(20,0)→(20,7)→(10.5,7)→(10.5,9)→(5,9)→(5,7)→(0,7)→(0,0)
            // altoTotal=9 es la altura MÁXIMA (saliente); el hombro izquierdo queda en y=7, sin (0,9).
            var verts = new List<PuntoDTO>
            {
                Pt(0, 0),
                Pt(ancho, 0),
                Pt(ancho, altoDerInf)
            };

            if (!tieneSaliente && altoDerInf < alto - 1.0)
                verts.Add(Pt(ancho, alto));

            if (tramosSuperiorMetros == null || tramosSuperiorMetros.Count == 0)
            {
                if (!UltimoVerticeEs(verts, 0, alto))
                    verts.Add(Pt(0, alto));
                return verts;
            }

            double sumaTramos = tramosSuperiorMetros.Sum() * METROS_A_MM;
            if (Math.Abs(sumaTramos - ancho) > ancho * 0.05)
            {
                double escala = ancho / Math.Max(sumaTramos, 1.0);
                for (int j = 0; j < tramosSuperiorMetros.Count; j++)
                    tramosSuperiorMetros[j] *= escala / METROS_A_MM;
            }

            double xTop = ancho;
            for (int i = tramosSuperiorMetros.Count; i >= 1; i--)
            {
                double tramo = tramosSuperiorMetros[i - 1] * METROS_A_MM;
                double xNext = xTop - tramo;

                if (tieneSaliente && i == indiceSaliente)
                {
                    if (!UltimoVerticeEs(verts, xTop, yHombro))
                        verts.Add(Pt(xTop, yHombro));
                    verts.Add(Pt(xTop, alto));
                    verts.Add(Pt(xNext, alto));
                    verts.Add(Pt(xNext, yHombro));
                }
                else if (tieneSaliente)
                {
                    verts.Add(Pt(xNext, yHombro));
                }
                else
                {
                    verts.Add(Pt(xNext, alto));
                }

                xTop = xNext;
            }

            if (!tieneSaliente && !UltimoVerticeEs(verts, 0, alto))
                verts.Add(Pt(0, alto));

            return verts;
        }

        /// <summary>
        /// Corrige el error típico de GPT: muro izquierdo en x=5 con cola inferior (5,0)-(0,0).
        /// </summary>
        private static List<LineaDTO> RepararPerimetroGptComun(List<LineaDTO> lineas)
        {
            var verts = LineasAVerticesOrdenados(lineas);
            if (verts == null || verts.Count < 4)
                return lineas;

            ObtenerBoundsVertices(verts, out double minX, out double minY, out double maxX, out double maxY);

            double? xMuroIzq = DetectarMuroIzquierdoDesplazado(verts, minY, maxY);
            if (xMuroIzq.HasValue && xMuroIzq.Value > 50)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    if (Math.Abs(verts[i].X - xMuroIzq.Value) < 50)
                        verts[i].X = 0;
                }

                verts = verts
                    .Where(v => !(Math.Abs(v.X) < 50 && Math.Abs(v.Y - minY) < 50 && Dist2d(v.X, v.Y, xMuroIzq.Value, minY) < 50))
                    .ToList();

                verts = SimplificarVerticesColineales(verts);
            }

            return VerticesALineas(verts);
        }

        private static double? DetectarMuroIzquierdoDesplazado(List<PuntoDTO> verts, double minY, double maxY)
        {
            double altura = maxY - minY;
            if (altura < 500)
                return null;

            foreach (var v in verts)
            {
                if (v.X < 50)
                    continue;
                bool tieneParEnY = verts.Any(u => Math.Abs(u.X - v.X) < 50
                    && Math.Abs(u.Y - minY) < 150);
                bool tieneTope = verts.Any(u => Math.Abs(u.X - v.X) < 50
                    && Math.Abs(u.Y - maxY) < 150);
                if (tieneParEnY && tieneTope && verts.Any(u => Math.Abs(u.X) < 50 && Math.Abs(u.Y - minY) < 150))
                    return v.X;
            }
            return null;
        }

        private static List<PuntoDTO> LineasAVerticesOrdenados(List<LineaDTO> lineas)
        {
            if (lineas == null || lineas.Count == 0)
                return null;

            var restantes = lineas.ToList();
            var verts = new List<PuntoDTO>();

            var first = restantes[0];
            verts.Add(Pt(first.InicioX, first.InicioY));
            verts.Add(Pt(first.FinX, first.FinY));
            restantes.RemoveAt(0);

            var actual = verts[verts.Count - 1];
            while (restantes.Count > 0)
            {
                int idx = -1;
                bool invertir = false;
                for (int i = 0; i < restantes.Count; i++)
                {
                    var l = restantes[i];
                    if (Dist2d(actual.X, actual.Y, l.InicioX, l.InicioY) < TOLERANCIA_EXTREMO_MM)
                    {
                        idx = i;
                        invertir = false;
                        break;
                    }
                    if (Dist2d(actual.X, actual.Y, l.FinX, l.FinY) < TOLERANCIA_EXTREMO_MM)
                    {
                        idx = i;
                        invertir = true;
                        break;
                    }
                }

                if (idx < 0)
                    break;

                var seg = restantes[idx];
                restantes.RemoveAt(idx);
                actual = invertir
                    ? Pt(seg.InicioX, seg.InicioY)
                    : Pt(seg.FinX, seg.FinY);
                verts.Add(actual);
            }

            if (verts.Count > 1
                && Dist2d(verts[0].X, verts[0].Y, verts[verts.Count - 1].X, verts[verts.Count - 1].Y) < TOLERANCIA_EXTREMO_MM)
            {
                verts.RemoveAt(verts.Count - 1);
            }

            return verts;
        }

        private static List<PuntoDTO> SimplificarVerticesColineales(List<PuntoDTO> verts)
        {
            if (verts == null || verts.Count < 3)
                return verts;

            var salida = new List<PuntoDTO> { verts[0] };
            for (int i = 1; i < verts.Count; i++)
            {
                var prev = salida[salida.Count - 1];
                var cur = verts[i];
                var next = verts[(i + 1) % verts.Count];

                bool colinealH = Math.Abs(prev.Y - cur.Y) < 50 && Math.Abs(cur.Y - next.Y) < 50;
                bool colinealV = Math.Abs(prev.X - cur.X) < 50 && Math.Abs(cur.X - next.X) < 50;
                if (!colinealH && !colinealV)
                    salida.Add(cur);
            }
            return salida;
        }

        public static void ObtenerBoundsPublico(List<LineaDTO> lineas, out double minX, out double minY, out double maxX, out double maxY)
            => ObtenerBounds(lineas, out minX, out minY, out maxX, out maxY);

        /// <summary>Valida perímetro reconstruido antes de dibujar (evita aceptar JSON alucinado de GPT).</summary>
        public static bool ValidarPerimetroBoceto(List<LineaDTO> lineas, out string motivo)
        {
            motivo = null;
            if (lineas == null || lineas.Count < 4)
            {
                motivo = "Menos de 4 tramos";
                return false;
            }

            if (lineas.Count > 28)
            {
                motivo = "Demasiados tramos (" + lineas.Count + ")";
                return false;
            }

            ObtenerBounds(lineas, out double minX, out double minY, out double maxX, out double maxY);
            if (minX < -150 || minY < -150)
            {
                motivo = "Coordenadas negativas en el perímetro";
                return false;
            }

            double anchoM = (maxX - minX) / METROS_A_MM;
            double altoM = (maxY - minY) / METROS_A_MM;
            if (anchoM < 1.5 || altoM < 1.5 || anchoM > 60 || altoM > 60)
            {
                motivo = $"Tamaño improbable ({anchoM:0.#}×{altoM:0.#} m)";
                return false;
            }

            for (int i = 0; i < lineas.Count - 1; i++)
            {
                var a = lineas[i];
                var b = lineas[i + 1];
                if (Dist2d(a.FinX, a.FinY, b.InicioX, b.InicioY) > TOLERANCIA_EXTREMO_MM)
                {
                    motivo = "Tramos desconectados";
                    return false;
                }
            }

            var primero = lineas[0];
            var ultimo = lineas[lineas.Count - 1];
            bool cerrada = Dist2d(primero.InicioX, primero.InicioY, ultimo.FinX, ultimo.FinY) <= TOLERANCIA_EXTREMO_MM;
            if (!cerrada)
            {
                motivo = "Perímetro no cerrado";
                return false;
            }

            return true;
        }

        /// <summary>Cuenta de cotas del perímetro en cotasEtiquetadas vs cotasVisibles (tolerancia ±1).</summary>
        public static bool CotasPerimetroCoherentes(JToken cotasEtiquetadas, JToken cotasVisibles)
        {
            int nEt = (cotasEtiquetadas as JArray)?.Count ?? 0;
            if (nEt == 0)
                return true;

            var vis = new List<double>();
            if (cotasVisibles is JArray arr)
            {
                foreach (var item in arr)
                {
                    var v = LeerNumeroCota(item);
                    if (!v.HasValue || v.Value <= 0)
                        continue;
                    if (v.Value >= 0.12 && v.Value <= 0.55)
                        continue;
                    vis.Add(v.Value);
                }
            }

            if (vis.Count == 0)
                return true;

            return Math.Abs(vis.Count - nEt) <= 1;
        }

        private static void ObtenerBounds(List<LineaDTO> lineas, out double minX, out double minY, out double maxX, out double maxY)
        {
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;
            foreach (var l in lineas)
            {
                minX = Math.Min(minX, Math.Min(l.InicioX, l.FinX));
                minY = Math.Min(minY, Math.Min(l.InicioY, l.FinY));
                maxX = Math.Max(maxX, Math.Max(l.InicioX, l.FinX));
                maxY = Math.Max(maxY, Math.Max(l.InicioY, l.FinY));
            }
        }

        private static void ObtenerBoundsVertices(List<PuntoDTO> verts, out double minX, out double minY, out double maxX, out double maxY)
        {
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;
            foreach (var v in verts)
            {
                minX = Math.Min(minX, v.X);
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
                maxY = Math.Max(maxY, v.Y);
            }
        }

        private static bool TryLeerDesplazamientoRecorrido(JToken seg, out double dx, out double dy)
        {
            dx = dy = 0;
            if (seg == null)
                return false;

            var dir = seg["dir"]?.ToString()?.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(dir))
            {
                double len = LeerNumeroCota(seg["len"]) ?? LeerNumeroCota(seg["longitud"]) ?? 0;
                if (len <= 0)
                    return false;

                switch (dir)
                {
                    case "E":
                    case "ESTE":
                    case "D":
                    case "DERECHA":
                        dx = len;
                        return true;
                    case "W":
                    case "O":
                    case "OESTE":
                    case "IZQUIERDA":
                        dx = -len;
                        return true;
                    case "N":
                    case "NORTE":
                    case "ARRIBA":
                        dy = len;
                        return true;
                    case "S":
                    case "SUR":
                    case "ABAJO":
                        dy = -len;
                        return true;
                    default:
                        return false;
                }
            }

            if (seg["dx"] != null || seg["dy"] != null)
            {
                dx = seg["dx"]?.Value<double>() ?? 0;
                dy = seg["dy"]?.Value<double>() ?? 0;
                return true;
            }

            var tipo = seg["tipo"]?.ToString()?.Trim().ToUpperInvariant();
            double cota = LeerNumeroCota(seg["cota"]) ?? LeerNumeroCota(seg["longitud"]) ?? 0;
            if (cota <= 0)
                return false;

            var sentido = seg["sentido"]?.ToString()?.Trim().ToUpperInvariant();
            if (tipo == "H" || tipo == "HORIZONTAL")
            {
                bool izq = sentido == "W" || sentido == "O" || sentido == "OESTE" || sentido == "IZQUIERDA";
                dx = izq ? -cota : cota;
                return true;
            }

            if (tipo == "V" || tipo == "VERTICAL")
            {
                bool abajo = sentido == "S" || sentido == "SUR" || sentido == "ABAJO";
                dy = abajo ? -cota : cota;
                return true;
            }

            return false;
        }

        private static double? LeerNumeroCota(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                return token.Value<double>();

            var text = token.ToString();
            var match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+(?:[.,]\d+)?");
            if (!match.Success)
                return null;
            var norm = match.Value.Replace(',', '.');
            return double.TryParse(norm, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : (double?)null;
        }

        private static int? LeerEnteroCota(JToken token)
        {
            var n = LeerNumeroCota(token);
            return n.HasValue ? (int)Math.Round(n.Value) : (int?)null;
        }

        private static bool UltimoVerticeEs(List<PuntoDTO> verts, double x, double y)
        {
            if (verts == null || verts.Count == 0)
                return false;
            var u = verts[verts.Count - 1];
            return Math.Abs(u.X - x) < 50 && Math.Abs(u.Y - y) < 50;
        }

        private static bool TieneTramoCierre(List<LineaDTO> lineas, PuntoDTO first, PuntoDTO last)
        {
            if (lineas == null)
                return false;

            foreach (var l in lineas)
            {
                if ((Dist2d(l.InicioX, l.InicioY, last.X, last.Y) <= TOLERANCIA_EXTREMO_MM
                        && Dist2d(l.FinX, l.FinY, first.X, first.Y) <= TOLERANCIA_EXTREMO_MM)
                    || (Dist2d(l.FinX, l.FinY, last.X, last.Y) <= TOLERANCIA_EXTREMO_MM
                        && Dist2d(l.InicioX, l.InicioY, first.X, first.Y) <= TOLERANCIA_EXTREMO_MM))
                    return true;
            }

            return false;
        }

        private static List<PuntoDTO> RotarVerticesDesdeOrigenInferiorIzquierdo(List<PuntoDTO> verts)
        {
            if (verts == null || verts.Count < 3)
                return verts;

            int idx = 0;
            double minY = verts.Min(v => v.Y);
            for (int i = 0; i < verts.Count; i++)
            {
                if (Math.Abs(verts[i].Y - minY) > 50)
                    continue;
                if (idx == 0 || verts[i].X < verts[idx].X)
                    idx = i;
            }

            var rotados = new List<PuntoDTO>();
            for (int i = 0; i < verts.Count; i++)
                rotados.Add(Clone(verts[(idx + i) % verts.Count]));
            return rotados;
        }

        /// <summary>Detecta muesca (bajada vertical desde el techo hacia el interior).</summary>
        private static bool EsSalienteHaciaAdentro(List<PuntoDTO> verts)
        {
            if (verts == null || verts.Count < 4)
                return false;

            double maxY = verts.Max(v => v.Y);
            for (int i = 0; i < verts.Count; i++)
            {
                var prev = verts[(i - 1 + verts.Count) % verts.Count];
                var a = verts[i];
                var b = verts[(i + 1) % verts.Count];
                bool vertical = Math.Abs(a.X - b.X) < 100;
                bool bajaDesdeTecho = vertical && b.Y < a.Y - 500 && a.Y > maxY - 200;
                if (!bajaDesdeTecho)
                    continue;

                // Muesca: baja tras llegar por el techo desde la derecha, ej. (20,9)→(10.5,9)→(10.5,7).
                // Saliente correcto baja en el lado oeste del saliente, ej. (5,9)→(5,7), sin techo previo a la derecha.
                bool veniaPorTechoDesdeDerecha =
                    Math.Abs(prev.Y - a.Y) < 100 && prev.X > a.X + 500;
                if (veniaPorTechoDesdeDerecha)
                    return true;
            }

            return false;
        }

        /// <summary>Rechaza vértices que empiezan subiendo el muro izquierdo en lugar del borde inferior.</summary>
        private static bool ValidarVerticesPerimetroBoceto(List<PuntoDTO> verts)
        {
            if (verts == null || verts.Count < 4)
                return false;

            var a = verts[0];
            var b = verts[1];
            bool bordeInferior = Math.Abs(a.Y - b.Y) < 100 && b.X > a.X + 500;
            if (bordeInferior)
                return true;

            bool muroIzqPrimero = Math.Abs(a.X - b.X) < 100 && b.Y > a.Y + 500;
            return !muroIzqPrimero;
        }
    }
}
