using Desing.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Desing.Services
{
    /// <summary>
    /// Importación de huellas de edificio desde OpenStreetMap (Nominatim + Overpass),
    /// proyectadas a mm de planta CAD para Desing_2 (mismo contrato que boceto imagen).
    /// </summary>
    public class OsmBuildingImportService
    {
        private const string UserAgent = "TandemDesingIntranet/1.0 (OsmBuildingImport; +https://trdesing.net)";
        private const string NominatimSearchUrl = "https://nominatim.openstreetmap.org/search";
        private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

        private static readonly HttpClient Http = CreateClient();

        static OsmBuildingImportService()
        {
            // .NET Framework: forzar TLS 1.2 hacia Nominatim / Overpass.
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch
            {
                /* ignore */
            }
        }

        private static HttpClient CreateClient()
        {
            var c = new HttpClient();
            c.Timeout = TimeSpan.FromSeconds(45);
            c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
            c.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            return c;
        }

        public async Task<List<OsmGeocodeResultDTO>> SearchAddressAsync(string query, int limit = 6)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<OsmGeocodeResultDTO>();

            limit = Math.Max(1, Math.Min(limit, 10));
            var url =
                NominatimSearchUrl +
                "?format=jsonv2&addressdetails=0&limit=" + limit.ToString(CultureInfo.InvariantCulture) +
                "&q=" + HttpUtility.UrlEncode(query.Trim());

            var json = await Http.GetStringAsync(url).ConfigureAwait(false);
            var arr = JArray.Parse(json);
            var list = new List<OsmGeocodeResultDTO>();
            foreach (var token in arr)
            {
                if (!(token is JObject o)) continue;
                if (!TryParseDouble(o["lat"], out var lat) || !TryParseDouble(o["lon"], out var lng))
                    continue;

                var dto = new OsmGeocodeResultDTO
                {
                    DisplayName = (string)o["display_name"] ?? query.Trim(),
                    Lat = lat,
                    Lng = lng
                };

                var bb = o["boundingbox"] as JArray;
                if (bb != null && bb.Count >= 4 &&
                    TryParseDouble(bb[0], out var south) &&
                    TryParseDouble(bb[1], out var north) &&
                    TryParseDouble(bb[2], out var west) &&
                    TryParseDouble(bb[3], out var east))
                {
                    dto.South = south;
                    dto.North = north;
                    dto.West = west;
                    dto.East = east;
                }

                list.Add(dto);
            }

            return list;
        }

        public async Task<List<OsmBuildingFootprintDTO>> FetchBuildingsInBboxAsync(
            double south, double west, double north, double east)
        {
            NormalizeBbox(ref south, ref west, ref north, ref east);
            var areaDeg2 = Math.Abs(north - south) * Math.Abs(east - west);
            // Evitar consultas enormes a Overpass (~ciudad completa).
            if (areaDeg2 > 0.04) // ~ ~20 km × 20 km en lat media
                throw new InvalidOperationException("Amplíe el zoom: el área del mapa es demasiado grande para cargar edificios.");

            var query = new StringBuilder();
            query.Append("[out:json][timeout:25];");
            query.AppendFormat(
                CultureInfo.InvariantCulture,
                "(way[\"building\"]({0},{1},{2},{3}););out body;>;out skel qt;",
                south, west, north, east);

            using (var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("data", query.ToString())
            }))
            using (var resp = await Http.PostAsync(OverpassUrl, content).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ParseOverpassBuildings(json);
            }
        }

        public DeteccionEsquinasLDTO BuildSketchFromFootprint(OsmBuildingImportRequestDTO request)
        {
            if (request == null || request.Ring == null || request.Ring.Count < 3)
                throw new ArgumentException("La huella del edificio no es válida (mínimo 3 vértices).");

            var ring = NormalizeClosedRing(request.Ring);
            if (ring.Count < 3)
                throw new ArgumentException("La huella del edificio no tiene suficientes vértices distintos.");

            var lineas = FootprintToLineasMm(ring);
            if (lineas.Count == 0)
                throw new InvalidOperationException("No se pudieron generar tramos de muro desde la huella.");

            var resultado = SketchWallBuilder.ConstruirBocetoSoloEje(lineas);
            resultado.LineasEje = lineas;

            SketchWallBuilder.ObtenerBoundsPublico(lineas, out _, out _, out double maxX, out double maxY);
            var label = string.IsNullOrWhiteSpace(request.TextLabel)
                ? (request.OsmId.HasValue ? ("OSM " + request.OsmId.Value) : "edificio")
                : request.TextLabel.Trim();

            resultado.Mensaje =
                "Edificio importado (" + label + "): " + lineas.Count + " tramos, " +
                (maxX / 1000.0).ToString("0.##", CultureInfo.InvariantCulture) + "×" +
                (maxY / 1000.0).ToString("0.##", CultureInfo.InvariantCulture) + " m. " +
                resultado.Mensaje;

            return resultado;
        }

        internal static List<OsmBuildingFootprintDTO> ParseOverpassBuildings(string json)
        {
            var root = JObject.Parse(json);
            var elements = root["elements"] as JArray;
            var result = new List<OsmBuildingFootprintDTO>();
            if (elements == null) return result;

            var nodes = new Dictionary<long, OsmLatLngDTO>();
            foreach (var el in elements.OfType<JObject>())
            {
                var type = (string)el["type"];
                if (type != "node") continue;
                if (!TryParseLong(el["id"], out var id)) continue;
                if (!TryParseDouble(el["lat"], out var lat) || !TryParseDouble(el["lon"], out var lng))
                    continue;
                nodes[id] = new OsmLatLngDTO { Lat = lat, Lng = lng };
            }

            foreach (var el in elements.OfType<JObject>())
            {
                var type = (string)el["type"];
                if (type != "way") continue;
                var tags = el["tags"] as JObject;
                if (tags == null || tags["building"] == null) continue;
                if (!TryParseLong(el["id"], out var wayId)) continue;

                var nds = el["nodes"] as JArray;
                if (nds == null || nds.Count < 3) continue;

                var ring = new List<OsmLatLngDTO>();
                foreach (var nd in nds)
                {
                    if (!TryParseLong(nd, out var nid)) continue;
                    if (!nodes.TryGetValue(nid, out var pt)) continue;
                    ring.Add(new OsmLatLngDTO { Lat = pt.Lat, Lng = pt.Lng });
                }

                ring = NormalizeClosedRing(ring);
                if (ring.Count < 3) continue;

                var name = (string)tags["name"];
                var building = (string)tags["building"];
                result.Add(new OsmBuildingFootprintDTO
                {
                    OsmId = wayId,
                    TextLabel = string.IsNullOrWhiteSpace(name)
                        ? ("Edificio OSM " + wayId.ToString(CultureInfo.InvariantCulture))
                        : name.Trim(),
                    BuildingType = string.IsNullOrWhiteSpace(building) ? "yes" : building.Trim(),
                    Ring = ring
                });
            }

            return result;
        }

        internal static List<LineaDTO> FootprintToLineasMm(IReadOnlyList<OsmLatLngDTO> ring)
        {
            var closed = NormalizeClosedRing(ring);
            if (closed.Count < 3) return new List<LineaDTO>();

            double originLat = closed.Min(p => p.Lat);
            double originLng = closed.Min(p => p.Lng);
            double midLat = closed.Average(p => p.Lat);
            ProjectLatLngToMm(originLat, originLng, midLat, out _, out _, out var mPerDegLat, out var mPerDegLng);

            var pts = new List<PuntoDTO>(closed.Count);
            foreach (var p in closed)
            {
                var xMm = (p.Lng - originLng) * mPerDegLng * 1000.0;
                var yMm = (p.Lat - originLat) * mPerDegLat * 1000.0;
                pts.Add(new PuntoDTO { X = xMm, Y = yMm, Z = 0 });
            }

            var lineas = new List<LineaDTO>();
            for (int i = 0; i < pts.Count; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % pts.Count];
                var dx = b.X - a.X;
                var dy = b.Y - a.Y;
                var len = Math.Sqrt(dx * dx + dy * dy);
                if (len < 1.0) continue;
                lineas.Add(new LineaDTO
                {
                    Tipo = "Line",
                    InicioX = a.X,
                    InicioY = a.Y,
                    InicioZ = 0,
                    FinX = b.X,
                    FinY = b.Y,
                    FinZ = 0,
                    Longitud = len,
                    Layer = "OSM_Building"
                });
            }

            return lineas;
        }

        private static List<OsmLatLngDTO> NormalizeClosedRing(IReadOnlyList<OsmLatLngDTO> ring)
        {
            var list = new List<OsmLatLngDTO>();
            if (ring == null) return list;

            const double tol = 1e-10;
            foreach (var p in ring)
            {
                if (p == null) continue;
                if (list.Count > 0)
                {
                    var last = list[list.Count - 1];
                    if (Math.Abs(last.Lat - p.Lat) < tol && Math.Abs(last.Lng - p.Lng) < tol)
                        continue;
                }
                list.Add(new OsmLatLngDTO { Lat = p.Lat, Lng = p.Lng });
            }

            if (list.Count >= 2)
            {
                var first = list[0];
                var last = list[list.Count - 1];
                if (Math.Abs(first.Lat - last.Lat) < tol && Math.Abs(first.Lng - last.Lng) < tol)
                    list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private static void ProjectLatLngToMm(
            double lat, double lng, double refLat,
            out double xMm, out double yMm,
            out double mPerDegLat, out double mPerDegLng)
        {
            var latRad = refLat * Math.PI / 180.0;
            mPerDegLat = 111132.92 - 559.82 * Math.Cos(2 * latRad) + 1.175 * Math.Cos(4 * latRad);
            mPerDegLng = 111412.84 * Math.Cos(latRad) - 93.5 * Math.Cos(3 * latRad);
            xMm = lng * mPerDegLng * 1000.0;
            yMm = lat * mPerDegLat * 1000.0;
        }

        private static void NormalizeBbox(ref double south, ref double west, ref double north, ref double east)
        {
            if (south > north)
            {
                var t = south;
                south = north;
                north = t;
            }
            if (west > east)
            {
                var t = west;
                west = east;
                east = t;
            }

            south = Clamp(south, -85.0, 85.0);
            north = Clamp(north, -85.0, 85.0);
            west = Clamp(west, -180.0, 180.0);
            east = Clamp(east, -180.0, 180.0);
        }

        private static double Clamp(double v, double min, double max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static bool TryParseDouble(JToken token, out double value)
        {
            value = 0;
            if (token == null || token.Type == JTokenType.Null) return false;
            return double.TryParse(
                token.Type == JTokenType.String ? (string)token : token.ToString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryParseLong(JToken token, out long value)
        {
            value = 0;
            if (token == null || token.Type == JTokenType.Null) return false;
            return long.TryParse(
                token.Type == JTokenType.String ? (string)token : token.ToString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value);
        }
    }
}
