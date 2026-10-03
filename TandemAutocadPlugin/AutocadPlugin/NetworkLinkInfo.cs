using System;
using System.Net.NetworkInformation;

namespace AutocadPlugin
{
    /// <summary>
    /// Velocidad de enlace de la NIC activa (Wi‑Fi o cable), no el throughput real.
    /// </summary>
    internal static class NetworkLinkInfo
    {
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
    }
}
