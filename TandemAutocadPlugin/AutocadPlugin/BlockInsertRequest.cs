using System;
using Newtonsoft.Json.Linq;

namespace AutocadPlugin
{
    internal sealed class BlockInsertRequest
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public string View { get; set; }
        public int RotationDeg { get; set; }
        public string Caption { get; set; }

        public bool Is3dRef
        {
            get
            {
                return string.Equals(View, "3dref", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static BlockInsertRequest FromJson(JObject obj)
        {
            if (obj == null)
                return null;
            var req = new BlockInsertRequest
            {
                Id = obj["id"] != null ? (long)obj["id"] : 0L,
                Code = ((string)obj["code"] ?? "").Trim(),
                CodeName = ((string)obj["codeName"] ?? (string)obj["CodeName"] ?? "").Trim(),
                View = ((string)obj["view"] ?? "3dref").Trim(),
                Caption = ((string)obj["caption"] ?? "").Trim()
            };
            int rot;
            if (int.TryParse((string)obj["rotation"] ?? (string)obj["rotationDeg"] ?? "0", out rot))
                req.RotationDeg = rot;
            else if (obj["rotation"] != null)
                req.RotationDeg = (int)obj["rotation"];
            if (string.IsNullOrWhiteSpace(req.View))
                req.View = "3dref";
            if (string.IsNullOrWhiteSpace(req.CodeName))
                req.CodeName = Atk60CodeNames.FromArticleCode(req.Code);
            return req;
        }
    }

    internal static class Atk60CodeNames
    {
        public static string FromArticleCode(string atenko)
        {
            var digits = OnlyDigits(atenko);
            if (digits.Length == 10 && digits.StartsWith("3120", StringComparison.Ordinal))
            {
                int h;
                int w;
                if (int.TryParse(digits.Substring(4, 3), out h) && int.TryParse(digits.Substring(7, 3), out w))
                {
                    if (h == 270 && w == 90) return "27904209";
                    if (h == 270 && w == 60) return "27604207";
                    if (h == 270 && w == 45) return "27454206";
                    if (h == 270 && w == 30) return "27304205";
                    if (h == 240 && w == 90) return "24904240";
                    if (h == 240 && w == 60) return "24604242";
                    if (h == 240 && w == 45) return "24454243";
                    if (h == 240 && w == 30) return "24304244";
                }
            }
            return "";
        }

        private static string OnlyDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            var chars = new char[value.Length];
            var n = 0;
            foreach (var c in value)
            {
                if (c >= '0' && c <= '9')
                    chars[n++] = c;
            }
            return new string(chars, 0, n);
        }
    }
}
