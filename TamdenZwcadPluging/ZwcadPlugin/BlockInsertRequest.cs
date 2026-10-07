using System;
using Newtonsoft.Json.Linq;

namespace ZwcadPlugin
{
    internal sealed class BlockInsertRequest
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public string View { get; set; }
        public int RotationDeg { get; set; }
        public string Caption { get; set; }
        public string DwgUrl { get; set; }
        public string DwgUrl3D { get; set; }
        public string DwgUrl3DRef { get; set; }
        public string DwgUrlXr { get; set; }

        public string UrlForView(string view)
        {
            var v = (view ?? "").Trim();
            if (string.Equals(v, "3d", StringComparison.OrdinalIgnoreCase))
                return FirstNonEmpty(DwgUrl3D, DwgUrl);
            if (string.Equals(v, "xr", StringComparison.OrdinalIgnoreCase))
                return DwgUrlXr;
            return FirstNonEmpty(DwgUrl3DRef, DwgUrl);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return "";
            foreach (var s in values)
            {
                if (!string.IsNullOrWhiteSpace(s))
                    return s.Trim();
            }
            return "";
        }

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
                Caption = ((string)obj["caption"] ?? "").Trim(),
                DwgUrl = ((string)obj["dwg"] ?? (string)obj["DwgUrl"] ?? "").Trim(),
                DwgUrl3D = ((string)obj["dwg3d"] ?? (string)obj["DwgUrl3D"] ?? "").Trim(),
                DwgUrl3DRef = ((string)obj["dwg3dref"] ?? (string)obj["DwgUrl3DRef"] ?? "").Trim(),
                DwgUrlXr = ((string)obj["dwgXr"] ?? (string)obj["DwgUrlXr"] ?? "").Trim()
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
                    if (h == 120 && w == 90) return "12904215";
                    if (h == 120 && w == 60) return "12604213";
                    if (h == 120 && w == 45) return "12454212";
                    if (h == 120 && w == 30) return "12304211";
                    if (h == 270 && w == 75) return "27104219";
                    if (h == 240 && w == 75) return "24104224";
                    if (h == 120 && w == 75) return "12104120";
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

