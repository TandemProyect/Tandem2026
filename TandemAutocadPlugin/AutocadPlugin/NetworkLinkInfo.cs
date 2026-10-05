using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;

namespace AutocadPlugin
{
    /// <summary>
    /// Velocidad de enlace de la NIC activa (Wi‑Fi o cable) y ping al MVC.
    /// </summary>
    internal static class NetworkLinkInfo
    {
        public static int LastServerMs = -2;

        public static void Read(out string kind, out int mbps)
        {
            kind = "";
            mbps = 0;
            try
            {
                NetworkInterface best = null;
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni == null || ni.OperationalStatus != OperationalStatus.Up)
                        continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback
                        || ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;
                    if (best == null)
                    {
                        best = ni;
                        continue;
                    }
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                        && best.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                    {
                        best = ni;
                        continue;
                    }
                    if (ni.NetworkInterfaceType == best.NetworkInterfaceType && ni.Speed > best.Speed)
                        best = ni;
                }
                if (best == null)
                    return;
                kind = best.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? "wifi" : "cable";
                if (best.Speed > 0 && best.Speed < long.MaxValue / 2)
                    mbps = (int)Math.Max(1, best.Speed / 1000000L);
            }
            catch
            {
            }
        }

        public static string FormatWifiLine()
        {
            string kind;
            int mbps;
            Read(out kind, out mbps);
            var name = string.Equals(kind, "cable", StringComparison.OrdinalIgnoreCase) ? "Cable" : "Wi‑Fi";
            if (mbps <= 0)
                return name + " · Sin datos";
            return name + " · " + mbps + " Mb/s · " + QualityFromMbps(mbps);
        }

        public static string FormatServerLine()
        {
            if (LastServerMs == -2)
                return "Servidor · Midiendo…";
            if (LastServerMs < 0)
                return "Servidor · Sin respuesta";
            return "Servidor · " + LastServerMs + " ms · " + QualityFromMs(LastServerMs);
        }

        public static string QualityFromMbps(int mbps)
        {
            if (mbps >= 100) return "Buena";
            if (mbps >= 50) return "Aceptable";
            if (mbps >= 20) return "Regular";
            if (mbps > 0) return "Mala";
            return "Sin datos";
        }

        public static string QualityFromMs(int ms)
        {
            if (ms < 0) return "Sin respuesta";
            if (ms < 80) return "Buena";
            if (ms < 200) return "Aceptable";
            if (ms < 500) return "Regular";
            return "Mala";
        }

        public static void RefreshServerPing()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                using (var handler = PluginHttp.CreateHandler())
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(4);
                    var url = MvcServerSettings.CurrentUrl().TrimEnd('/') + "/DesignToolsAutocad/PluginPing";
                    var json = client.GetStringAsync(url).GetAwaiter().GetResult();
                    sw.Stop();
                    var ms = (int)sw.ElapsedMilliseconds;
                    try
                    {
                        var obj = JObject.Parse(json ?? "{}");
                        var db = obj["dbMs"] ?? obj["DbMs"];
                        if (db != null && db.Type != JTokenType.Null)
                            ms = Math.Max(0, db.Value<int>());
                    }
                    catch
                    {
                    }
                    LastServerMs = ms;
                }
            }
            catch
            {
                LastServerMs = -1;
            }
        }
    }
}
