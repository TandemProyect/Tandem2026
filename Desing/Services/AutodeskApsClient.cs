using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Desing.Services
{
    /// <summary>
    /// Cliente mínimo Autodesk Platform Services: token 2-legged, OSS, traducción Model Derivative para visor DWG.
    /// Documentación: https://aps.autodesk.com/
    /// </summary>
    public static class AutodeskApsClient
    {
        private const string AuthUrl = "https://developer.api.autodesk.com/authentication/v2/token";
        private const string OssBase = "https://developer.api.autodesk.com/oss/v2";
        private const string MdBase = "https://developer.api.autodesk.com/modelderivative/v2";

        private static readonly HttpClient Http = CreateClient();

        private static HttpClient CreateClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            return new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
        }

        public static bool IsConfigured
        {
            get
            {
                var id = ConfigurationManager.AppSettings["AutodeskAps:ClientId"];
                var sec = ConfigurationManager.AppSettings["AutodeskAps:ClientSecret"];
                return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(sec);
            }
        }

        private static string ClientId => (ConfigurationManager.AppSettings["AutodeskAps:ClientId"] ?? "").Trim();
        private static string ClientSecret => (ConfigurationManager.AppSettings["AutodeskAps:ClientSecret"] ?? "").Trim();

        private static string BucketKey
        {
            get
            {
                var b = (ConfigurationManager.AppSettings["AutodeskAps:BucketKey"] ?? "tandem-master-articles-dwg").Trim().ToLowerInvariant();
                foreach (var c in b)
                {
                    if (!(char.IsLetterOrDigit(c) || c == '-'))
                    {
                        return "tandem-master-articles-dwg";
                    }
                }
                if (b.Length < 3 || b.Length > 63)
                {
                    return "tandem-master-articles-dwg";
                }
                return b;
            }
        }

        public static async Task<(string accessToken, int expiresIn)> GetTwoLeggedTokenAsync(CancellationToken cancellationToken)
        {
            var body = new StringContent(
                "grant_type=client_credentials&client_id=" + Uri.EscapeDataString(ClientId)
                + "&client_secret=" + Uri.EscapeDataString(ClientSecret)
                + "&scope=" + Uri.EscapeDataString("data:read data:write data:create bucket:create bucket:read"),
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            using (var resp = await Http.PostAsync(AuthUrl, body, cancellationToken).ConfigureAwait(false))
            {
                var txt = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("APS OAuth: " + (int)resp.StatusCode + " " + txt);
                }
                var jo = JObject.Parse(txt);
                return (jo.Value<string>("access_token"), jo.Value<int?>("expires_in") ?? 3600);
            }
        }

        private static string ToSafeBase64Urn(string objectUrn)
        {
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(objectUrn));
            return raw.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static async Task EnsureBucketExistsAsync(string token, CancellationToken cancellationToken)
        {
            var key = BucketKey;
            var url = OssBase + "/buckets/" + Uri.EscapeDataString(key) + "/details";
            using (var req = new HttpRequestMessage(HttpMethod.Get, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        return;
                    }
                    if (resp.StatusCode != HttpStatusCode.NotFound)
                    {
                        var t = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new InvalidOperationException("APS OSS bucket check: " + (int)resp.StatusCode + " " + t);
                    }
                }
            }

            var createUrl = OssBase + "/buckets";
            var payload = new JObject
            {
                ["bucketKey"] = key,
                ["policyKey"] = "transient"
            };
            using (var req = new HttpRequestMessage(HttpMethod.Post, createUrl))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                {
                    var t = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode || resp.StatusCode == (HttpStatusCode)409)
                    {
                        return;
                    }
                    throw new InvalidOperationException("APS OSS create bucket: " + (int)resp.StatusCode + " " + t);
                }
            }
        }

        private static async Task UploadObjectAsync(string token, string objectName, string physicalPath, CancellationToken cancellationToken)
        {
            var key = BucketKey;
            var url = OssBase + "/buckets/" + Uri.EscapeDataString(key) + "/objects/" + Uri.EscapeDataString(objectName);
            using (var fs = File.OpenRead(physicalPath))
            {
                using (var req = new HttpRequestMessage(HttpMethod.Put, url))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    req.Content = new StreamContent(fs);
                    req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                    {
                        var t = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode)
                        {
                            throw new InvalidOperationException("APS OSS upload: " + (int)resp.StatusCode + " " + t);
                        }
                    }
                }
            }
        }

        private static async Task StartTranslationJobAsync(string token, string objectUrn, CancellationToken cancellationToken)
        {
            var safe = ToSafeBase64Urn(objectUrn);
            var url = MdBase + "/designdata/job";
            var job = new JObject
            {
                ["input"] = new JObject { ["urn"] = safe },
                ["output"] = new JObject
                {
                    ["formats"] = new JArray(
                        new JObject
                        {
                            ["type"] = "svf",
                            ["views"] = new JArray("2d", "3d")
                        })
                }
            };
            using (var req = new HttpRequestMessage(HttpMethod.Post, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent(job.ToString(Formatting.None), Encoding.UTF8, "application/json");
                using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                {
                    var t = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException("APS Model Derivative job: " + (int)resp.StatusCode + " " + t);
                    }
                }
            }
        }

        private static async Task WaitForManifestSuccessAsync(string token, string safeUrn, CancellationToken cancellationToken)
        {
            var url = MdBase + "/designdata/" + Uri.EscapeDataString(safeUrn) + "/manifest";
            for (var i = 0; i < 90; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                    {
                        var txt = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.Created)
                        {
                            await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                        if (!resp.IsSuccessStatusCode)
                        {
                            throw new InvalidOperationException("APS manifest: " + (int)resp.StatusCode + " " + txt);
                        }
                        var jo = JObject.Parse(txt);
                        var status = jo.Value<string>("status");
                        var progress = jo.Value<string>("progress");
                        if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
                        {
                            var reason = jo["reason"]?.ToString() ?? txt;
                            throw new InvalidOperationException("APS traducción fallida: " + reason);
                        }
                        if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(progress, "complete", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                    }
                }
                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            }
            throw new TimeoutException("Tiempo de espera agotado esperando la traducción APS del DWG.");
        }

        /// <summary>true = traducción lista; false = fallida; null = aún no lista o sin manifiesto.</summary>
        private static async Task<bool?> TryPeekManifestOutcomeAsync(string token, string safeUrn, CancellationToken cancellationToken)
        {
            var url = MdBase + "/designdata/" + Uri.EscapeDataString(safeUrn) + "/manifest";
            using (var req = new HttpRequestMessage(HttpMethod.Get, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var resp = await Http.SendAsync(req, cancellationToken).ConfigureAwait(false))
                {
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    if (resp.StatusCode == HttpStatusCode.Accepted || resp.StatusCode == HttpStatusCode.Created)
                    {
                        return null;
                    }
                    var txt = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    var jo = JObject.Parse(txt);
                    var status = jo.Value<string>("status");
                    var progress = jo.Value<string>("progress");
                    if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(progress, "complete", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Sube (si hace falta), traduce y devuelve el URN seguro (sin prefijo urn:) para Autodesk.Viewing.Document.load('urn:' + urn).
        /// </summary>
        public static async Task<string> GetOrCreateViewerUrnAsync(string physicalPath, CancellationToken cancellationToken)
        {
            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException("No existe el archivo DWG.", physicalPath);
            }
            if (!string.Equals(Path.GetExtension(physicalPath), ".dwg", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo archivos .dwg para APS.");
            }

            string hashKey;
            using (var sha = SHA256.Create())
            {
                using (var fs = File.OpenRead(physicalPath))
                {
                    var hash = sha.ComputeHash(fs);
                    hashKey = BitConverter.ToString(hash, 0, 16).Replace("-", "").ToLowerInvariant();
                }
            }

            var cacheKey = "ApsDwgViewerUrn_" + hashKey;
            var cached = HttpRuntime.Cache.Get(cacheKey) as string;
            if (!string.IsNullOrEmpty(cached))
            {
                return cached;
            }

            var token = (await GetTwoLeggedTokenAsync(cancellationToken).ConfigureAwait(false)).accessToken;
            await EnsureBucketExistsAsync(token, cancellationToken).ConfigureAwait(false);

            var objectName = "dwg/" + hashKey + ".dwg";
            var objectUrn = "urn:adsk.objects:os.object:" + BucketKey + "/" + objectName;
            var safeUrn = ToSafeBase64Urn(objectUrn);

            var peek = await TryPeekManifestOutcomeAsync(token, safeUrn, cancellationToken).ConfigureAwait(false);
            if (peek == true)
            {
                HttpRuntime.Cache.Insert(cacheKey, safeUrn, null, DateTime.UtcNow.AddHours(12), Cache.NoSlidingExpiration);
                return safeUrn;
            }
            if (peek == false)
            {
                throw new InvalidOperationException("La traducción APS de este DWG falló. Vuelva a subir el archivo o revise el modelo en Autodesk.");
            }

            await UploadObjectAsync(token, objectName, physicalPath, cancellationToken).ConfigureAwait(false);
            await StartTranslationJobAsync(token, objectUrn, cancellationToken).ConfigureAwait(false);
            await WaitForManifestSuccessAsync(token, safeUrn, cancellationToken).ConfigureAwait(false);

            HttpRuntime.Cache.Insert(cacheKey, safeUrn, null, DateTime.UtcNow.AddHours(12), Cache.NoSlidingExpiration);
            return safeUrn;
        }
    }
}
