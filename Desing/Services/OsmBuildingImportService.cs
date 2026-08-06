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
using System.Xml;

namespace Desing.Services
{
    /// <summary>
    /// Importación de huellas de edificio: Catastro INSPIRE (España) + OSM Nominatim/Overpass,
    /// proyectadas a mm de planta CAD para Desing_2.
    /// </summary>
    public class OsmBuildingImportService
    {
        private const string UserAgent = "TandemDesingIntranet/1.0 (OsmBuildingImport; +https://trdesing.net)";
        private const string NominatimSearchUrl = "https://nominatim.openstreetmap.org/search";
        private const string CatastroWfsBuUrl = "https://ovc.catastro.meh.es/INSPIRE/wfsBU.aspx";
        private const string CatastroRcCoorJsonUrl =
            "https://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCoordenadas.svc/json/Consulta_RCCOOR";

        private static readonly string[] OverpassUrls =
        {
            "https://overpass-api.de/api/interpreter",
            "https://overpass.kumi.systems/api/interpreter",
            "https://overpass.openstreetmap.fr/api/interpreter"
        };

        private static readonly HttpClient Http = CreateClient();

        static OsmBuildingImportService()
        {
            // .NET Framework: forzar TLS 1.2 hacia Nominatim / Overpass / Catastro.
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
            c.Timeout = TimeSpan.FromSeconds(60);
            c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
            c.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, application/xml, text/xml, */*");
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
            // Evitar consultas enormes: recortar al centro (~2 km) en vez de fallar en silencio.
            const double maxAreaDeg2 = 0.0008; // ~1–2 km según latitud
            var areaDeg2 = Math.Abs(north - south) * Math.Abs(east - west);
            if (areaDeg2 > maxAreaDeg2)
            {
                var midLat = (south + north) * 0.5;
                var midLng = (west + east) * 0.5;
                const double half = 0.012; // ~1.3 km
                south = midLat - half;
                north = midLat + half;
                west = midLng - half;
                east = midLng + half;
                NormalizeBbox(ref south, ref west, ref north, ref east);
            }

            Exception lastError = null;

            // 1) Catastro INSPIRE (España): más fiable que Overpass y trae referencia catastral.
            if (LooksLikeSpainBbox(south, west, north, east))
            {
                try
                {
                    var catastro = await FetchBuildingsFromCatastroWfsAsync(south, west, north, east)
                        .ConfigureAwait(false);
                    if (catastro != null && catastro.Count > 0)
                        return catastro;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            // 2) Overpass OSM (varios mirrors).
            try
            {
                var osm = await FetchBuildingsFromOverpassAsync(south, west, north, east).ConfigureAwait(false);
                if (osm != null && osm.Count > 0)
                    return osm;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            if (lastError != null)
                throw new InvalidOperationException(
                    "No se pudieron cargar edificios (Catastro/OSM): " + lastError.Message,
                    lastError);

            return new List<OsmBuildingFootprintDTO>();
        }

        /// <summary>
        /// Consulta referencia catastral + dirección por coordenadas WGS84 (parcela bajo el punto).
        /// </summary>
        public async Task<CatastroRcLookupResultDTO> LookupCatastroByCoordsAsync(double lat, double lng)
        {
            var url =
                CatastroRcCoorJsonUrl +
                "?SRS=" + Uri.EscapeDataString("EPSG:4326") +
                "&CoorX=" + lng.ToString(CultureInfo.InvariantCulture) +
                "&CoorY=" + lat.ToString(CultureInfo.InvariantCulture);

            var json = await Http.GetStringAsync(url).ConfigureAwait(false);
            return ParseCatastroRcCoorJson(json, lat, lng);
        }

        private async Task<List<OsmBuildingFootprintDTO>> FetchBuildingsFromCatastroWfsAsync(
            double south, double west, double north, double east)
        {
            // WFS bbox: minLat,minLon,maxLat,maxLon,EPSG:4326 — count acotado.
            var bbox = string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},EPSG:4326",
                south, west, north, east);
            var url =
                CatastroWfsBuUrl +
                "?service=WFS&version=2.0.0&request=GetFeature" +
                "&typenames=" + Uri.EscapeDataString("bu:Building") +
                "&srsName=" + Uri.EscapeDataString("EPSG:4326") +
                "&bbox=" + Uri.EscapeDataString(bbox) +
                "&count=120";

            using (var resp = await Http.GetAsync(url).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                var bytes = await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                // Catastro suele declarar ISO-8859-1; fallback Latin1 si no hay charset.
                var charset = resp.Content.Headers.ContentType != null
                    ? resp.Content.Headers.ContentType.CharSet
                    : null;
                Encoding enc;
                try
                {
                    enc = !string.IsNullOrWhiteSpace(charset)
                        ? Encoding.GetEncoding(charset)
                        : Encoding.GetEncoding("ISO-8859-1");
                }
                catch
                {
                    enc = Encoding.GetEncoding("ISO-8859-1");
                }

                var xml = enc.GetString(bytes);
                if (xml.IndexOf("ExceptionReport", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    xml.IndexOf("<Exception", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new InvalidOperationException("Catastro WFS devolvió error en el área solicitada.");
                }

                return ParseCatastroBuildingGml(xml);
            }
        }

        private async Task<List<OsmBuildingFootprintDTO>> FetchBuildingsFromOverpassAsync(
            double south, double west, double north, double east)
        {
            var query = new StringBuilder();
            query.Append("[out:json][timeout:25];");
            query.AppendFormat(
                CultureInfo.InvariantCulture,
                "(way[\"building\"]({0},{1},{2},{3}););out body;>;out skel qt;",
                south, west, north, east);
            var data = query.ToString();

            Exception last = null;
            foreach (var endpoint in OverpassUrls)
            {
                try
                {
                    using (var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("data", data)
                    }))
                    using (var resp = await Http.PostAsync(endpoint, content).ConfigureAwait(false))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            last = new InvalidOperationException(
                                endpoint + " HTTP " + (int)resp.StatusCode);
                            continue;
                        }

                        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var list = ParseOverpassBuildings(json);
                        if (list.Count > 0)
                            return list;
                        last = new InvalidOperationException(endpoint + " sin edificios en el área.");
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }

            if (last != null) throw last;
            return new List<OsmBuildingFootprintDTO>();
        }

        internal static List<OsmBuildingFootprintDTO> ParseCatastroBuildingGml(string xml)
        {
            var result = new List<OsmBuildingFootprintDTO>();
            if (string.IsNullOrWhiteSpace(xml)) return result;

            var doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.LoadXml(xml);

            foreach (XmlNode node in doc.GetElementsByTagName("*"))
            {
                if (node == null || node.LocalName != "Building") continue;

                var localId = FirstChildLocalValue(node, "localId")
                    ?? FirstChildLocalValue(node, "reference");
                if (string.IsNullOrWhiteSpace(localId))
                {
                    var gmlId = node.Attributes != null
                        ? (node.Attributes.GetNamedItem("gml:id") ?? node.Attributes.GetNamedItem("id"))
                        : null;
                    if (gmlId != null && !string.IsNullOrWhiteSpace(gmlId.Value))
                    {
                        localId = gmlId.Value;
                        const string prefix = "ES.SDGC.BU.";
                        if (localId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            localId = localId.Substring(prefix.Length);
                    }
                }

                if (string.IsNullOrWhiteSpace(localId)) continue;

                var ring = ParseFirstPosListRing(node);
                ring = NormalizeClosedRing(ring);
                if (ring.Count < 3) continue;

                int? levels = null;
                double? heightM = null;
                var floorsRaw = FirstChildLocalValue(node, "numberOfFloorsAboveGround");
                if (TryParseDouble(floorsRaw, out var floors) && floors > 0)
                {
                    levels = (int)Math.Round(floors);
                    heightM = Math.Max(levels.Value * 3.0, 4.0);
                }

                result.Add(new OsmBuildingFootprintDTO
                {
                    OsmId = StableIdFromText(localId),
                    TextLabel = localId.Trim(),
                    BuildingType = "catastro",
                    Ring = ring,
                    HeightM = heightM,
                    Levels = levels,
                    CadastralRef = localId.Trim(),
                    Source = "Catastro"
                });
            }

            return result;
        }

        internal static CatastroRcLookupResultDTO ParseCatastroRcCoorJson(string json, double lat, double lng)
        {
            var root = JObject.Parse(json ?? "{}");
            var resultNode = root["Consulta_RCCOORResult"] as JObject ?? root;
            var control = resultNode["control"] as JObject;
            if (control != null && control["cuerr"] != null &&
                TryParseLong(control["cuerr"], out var errCount) && errCount > 0)
            {
                var des = resultNode["lerr"]?.First?["des"]?.ToString();
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(des) ? "Catastro sin referencia en ese punto." : des.Trim());
            }

            var coord = resultNode["coordenadas_result"]?["coord"]?.First as JObject
                ?? resultNode["coordenadas"]?["coord"]?.First as JObject;
            if (coord == null)
                throw new InvalidOperationException("Catastro no devolvió parcela para esas coordenadas.");

            var pc = coord["pc"] as JObject;
            var pc1 = (string)(pc?["pc1"] ?? coord["pc1"]);
            var pc2 = (string)(pc?["pc2"] ?? coord["pc2"]);
            var rc = ((pc1 ?? string.Empty) + (pc2 ?? string.Empty)).Trim();
            if (rc.Length == 0)
                throw new InvalidOperationException("Catastro devolvió parcela sin referencia.");

            var address = ((string)coord["ldt"] ?? (string)coord["dir"] ?? string.Empty).Trim();
            return new CatastroRcLookupResultDTO
            {
                CadastralRef = rc,
                Address = address,
                Lat = lat,
                Lng = lng
            };
        }

        private static List<OsmLatLngDTO> ParseFirstPosListRing(XmlNode buildingNode)
        {
            var ring = new List<OsmLatLngDTO>();
            if (buildingNode == null) return ring;
            var nodes = buildingNode.SelectNodes(".//*[local-name()='posList']");
            if (nodes == null) return ring;
            foreach (XmlNode n in nodes)
            {
                if (n == null || string.IsNullOrWhiteSpace(n.InnerText)) continue;
                var parts = n.InnerText.Trim().Split(
                    new[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6) continue;
                for (int i = 0; i + 1 < parts.Length; i += 2)
                {
                    if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
                        continue;
                    if (!double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                        continue;
                    // INSPIRE Catastro EPSG:4326 en posList: lat lon
                    double lat = a;
                    double lng = b;
                    if (Math.Abs(a) > 90 && Math.Abs(b) <= 90)
                    {
                        lng = a;
                        lat = b;
                    }
                    ring.Add(new OsmLatLngDTO { Lat = lat, Lng = lng });
                }
                if (ring.Count >= 3) break;
                ring.Clear();
            }
            return ring;
        }

        private static string FirstChildLocalValue(XmlNode root, string localName)
        {
            if (root == null || string.IsNullOrEmpty(localName)) return null;
            var nodes = root.SelectNodes(".//*[local-name()='" + localName + "']");
            if (nodes == null) return null;
            foreach (XmlNode n in nodes)
            {
                if (n == null) continue;
                var t = (n.InnerText ?? string.Empty).Trim();
                if (t.Length > 0) return t;
            }
            return null;
        }

        private static long StableIdFromText(string text)
        {
            unchecked
            {
                long h = 1469598103934665603L; // FNV-ish
                foreach (var ch in text ?? string.Empty)
                {
                    h ^= ch;
                    h *= 1099511628211L;
                }
                if (h == 0) h = 1;
                // Evitar colisión visual con OSM way ids positivos habituales: usar negativo.
                return h > 0 ? -h : h;
            }
        }

        private static bool LooksLikeSpainBbox(double south, double west, double north, double east)
        {
            var midLat = (south + north) * 0.5;
            var midLng = (west + east) * 0.5;
            return midLat >= 27.0 && midLat <= 44.5 && midLng >= -19.0 && midLng <= 5.5;
        }

        private static bool TryParseDouble(string raw, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return double.TryParse(
                raw.Trim().Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
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
                int? levels = null;
                double? heightM = null;
                if (TryParseDouble(tags["building:levels"], out var levelsRaw) && levelsRaw > 0)
                    levels = (int)Math.Round(levelsRaw);
                if (TryParseHeightMeters(tags["height"], out var hParsed) && hParsed > 0.5)
                    heightM = hParsed;
                else if (levels.HasValue)
                    heightM = Math.Max(levels.Value * 3.0, 4.0);

                result.Add(new OsmBuildingFootprintDTO
                {
                    OsmId = wayId,
                    TextLabel = string.IsNullOrWhiteSpace(name)
                        ? ("Edificio OSM " + wayId.ToString(CultureInfo.InvariantCulture))
                        : name.Trim(),
                    BuildingType = string.IsNullOrWhiteSpace(building) ? "yes" : building.Trim(),
                    Ring = ring,
                    HeightM = heightM,
                    Levels = levels,
                    Source = "Osm"
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

        /// <summary>
        /// OSM height puede ser "12", "12 m", "12.5m". Devuelve metros.
        /// </summary>
        private static bool TryParseHeightMeters(JToken token, out double meters)
        {
            meters = 0;
            if (token == null || token.Type == JTokenType.Null) return false;
            var raw = (token.Type == JTokenType.String ? (string)token : token.ToString()) ?? string.Empty;
            raw = raw.Trim();
            if (raw.Length == 0) return false;
            raw = raw.Replace(',', '.');
            // Quitar sufijo "m" / "m." habitual
            if (raw.EndsWith("m", StringComparison.OrdinalIgnoreCase))
                raw = raw.Substring(0, raw.Length - 1).Trim();
            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out meters);
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
