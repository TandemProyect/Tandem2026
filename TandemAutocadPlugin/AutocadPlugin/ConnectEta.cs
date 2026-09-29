using System;
using System.Globalization;
using System.IO;

namespace AutocadPlugin
{
    /// <summary>
    /// Estimación de conexión paleta ↔ MVC, mismo patrón que el overlay de encofrar:
    /// cuenta atrás "Quedan unos Xs" y, si se pasa, pasa a arranque en frío (hasta 2 min).
    /// </summary>
    internal static class ConnectEta
    {
        public const int WarmMsLocal = 18000;
        public const int WarmMsProd = 12000;
        public const int ColdMsLocal = 120000;
        public const int ColdMsProd = 25000;

        public static int WarmMs()
        {
            return MvcServerSettings.IsProduction() ? WarmMsProd : WarmMsLocal;
        }

        public static int ColdMs()
        {
            return MvcServerSettings.IsProduction() ? ColdMsProd : ColdMsLocal;
        }

        public static int FallbackMs()
        {
            return ColdMs() + 15000;
        }

        public static int PredictedStartMs()
        {
            var last = ReadLastMs();
            if (last >= 3000)
                return last;
            return WarmMs();
        }

        public static void Record(int elapsedMs)
        {
            if (elapsedMs < 800 || elapsedMs > 180000)
                return;
            try
            {
                File.WriteAllText(EtaPath(), elapsedMs.ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
            }
        }

        public static string Format(int remainingMs, bool coldHang)
        {
            if (coldHang)
                return "Sigue conectando… el primero tras arrancar puede tardar hasta 2 min";
            if (remainingMs < 1400)
                return "Casi listo…";
            var sec = Math.Max(1, (int)Math.Ceiling(remainingMs / 1000.0));
            if (sec < 55)
                return "Quedan unos " + sec + " s";
            var min = Math.Max(1, (int)Math.Round(sec / 60.0));
            if (min <= 1)
                return "Queda 1 min";
            return "Quedan unos " + min + " min";
        }

        private static int ReadLastMs()
        {
            try
            {
                var path = EtaPath();
                if (!File.Exists(path))
                    return 0;
                var raw = (File.ReadAllText(path) ?? "").Trim();
                int ms;
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out ms))
                    return ms;
            }
            catch
            {
            }
            return 0;
        }

        private static string EtaPath()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tandem",
                "AutocadPlugin");
            Directory.CreateDirectory(dir);
            var tag = MvcServerSettings.IsProduction() ? "prod" : "local";
            return Path.Combine(dir, "connect-eta-" + tag + ".txt");
        }
    }
}
