using System;
using System.IO;

namespace FreeCadPluging
{
    internal static class FreeCadCommandBridge
    {
        public static void Queue(string action, string rawJson)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            FreeCadPluginEnvironment.EnsureFolders();
            var safeAction = Sanitize(action);
            var file = Path.Combine(
                FreeCadPluginEnvironment.RequestDir(),
                DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff") + "-" + safeAction + ".json");
            File.WriteAllText(file, rawJson ?? "{}");
        }

        private static string Sanitize(string value)
        {
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-' && chars[i] != '_')
                    chars[i] = '_';
            }
            return new string(chars);
        }
    }
}