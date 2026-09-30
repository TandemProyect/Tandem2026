using Desing.Repositories.Atk60;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Desing.Helpers
{
    /// <summary>
    /// Resuelve el par STL del encofrado ATK-60: estructura + fenólico
    /// (<see cref="Atk60Element.GetElement"/>, p.ej. Panel90270 / Panel90270F).
    /// </summary>
    public static class PluginCadAtk60PanelStlHelper
    {
        public static bool TryResolve(DAL.Tsql_Master_Articles article, out string frameVirtualPath, out string phenolicVirtualPath)
        {
            frameVirtualPath = null;
            phenolicVirtualPath = null;
            if (article == null)
                return false;

            int heightCm;
            int widthCm;
            if (!TryResolveSizeCm(article, out heightCm, out widthCm))
                return false;

            var key = "Panel" + widthCm.ToString(CultureInfo.InvariantCulture) + heightCm.ToString(CultureInfo.InvariantCulture);
            var frame = Atk60Element.GetElement(key);
            var phenolic = Atk60Element.GetElement(key + "F");
            if (string.IsNullOrWhiteSpace(frame) || string.IsNullOrWhiteSpace(phenolic))
                return false;

            frameVirtualPath = frame;
            phenolicVirtualPath = phenolic;
            return true;
        }

        public static bool TryGetCodeName(DAL.Tsql_Master_Articles article, out string codeName)
        {
            codeName = null;
            string frame;
            string phenolic;
            if (!TryResolve(article, out frame, out phenolic) || string.IsNullOrWhiteSpace(frame))
                return false;
            var file = Path.GetFileNameWithoutExtension(frame.Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(file))
                return false;
            codeName = file.Trim();
            return true;
        }

        private static bool TryResolveSizeCm(DAL.Tsql_Master_Articles article, out int heightCm, out int widthCm)
        {
            heightCm = 0;
            widthCm = 0;

            var code = FirstNonEmpty(article.AddAtenkoCode, article.TextCode);
            int codeH;
            int codeW;
            if (TryParseAtenkoPanelCode(code, out codeH, out codeW))
            {
                heightCm = codeH;
                widthCm = codeW;
                return true;
            }

            if (TryFromMeters(article.NumberHigh, article.NumberWidth, out heightCm, out widthCm))
                return true;

            return TryParseLabelSize(article.TextLabel, out heightCm, out widthCm);
        }

        /// <summary>
        /// Código Atenko de panel vertical: 3120 + alto cm (3) + ancho cm (3).
        /// Ej. 3120270090 = 2,70 × 0,90 → Panel90270.
        /// </summary>
        private static bool TryParseAtenkoPanelCode(string code, out int heightCm, out int widthCm)
        {
            heightCm = 0;
            widthCm = 0;
            if (string.IsNullOrWhiteSpace(code))
                return false;
            var digits = Regex.Replace(code.Trim(), @"\D", "");
            if (digits.Length != 10 || !digits.StartsWith("3120", StringComparison.Ordinal))
                return false;
            if (!int.TryParse(digits.Substring(4, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out heightCm))
                return false;
            if (!int.TryParse(digits.Substring(7, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out widthCm))
                return false;
            return heightCm >= 120 && heightCm <= 270 && widthCm >= 30 && widthCm <= 90;
        }

        private static bool TryFromMeters(double? high, double? width, out int heightCm, out int widthCm)
        {
            heightCm = ToCm(high);
            widthCm = ToCm(width);
            return heightCm >= 120 && heightCm <= 270 && widthCm >= 30 && widthCm <= 90;
        }

        private static bool TryParseLabelSize(string label, out int heightCm, out int widthCm)
        {
            heightCm = 0;
            widthCm = 0;
            if (string.IsNullOrWhiteSpace(label))
                return false;
            var m = Regex.Match(label, @"(\d+[.,]\d+)\s*[x×]\s*(\d+[.,]\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
                return false;
            double h;
            double w;
            if (!double.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out h))
                return false;
            if (!double.TryParse(m.Groups[2].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                return false;
            return TryFromMeters(h, w, out heightCm, out widthCm);
        }

        private static int ToCm(double? value)
        {
            if (!value.HasValue || value.Value <= 0)
                return 0;
            var n = value.Value;
            if (n > 200)
                return (int)Math.Round(n / 10d);
            if (n > 20)
                return (int)Math.Round(n);
            return (int)Math.Round(n * 100d);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return null;
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return null;
        }
    }
}
