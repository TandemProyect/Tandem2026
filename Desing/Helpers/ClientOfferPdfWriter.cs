using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Desing.Helpers
{
    /// <summary>
    /// Generador PDF básico sin dependencias externas (texto simple en una página).
    /// </summary>
    public static class ClientOfferPdfWriter
    {
        public static void Write(string fullPath, ClientOfferRow offer)
        {
            var lines = BuildLines(offer);
            var content = BuildContent(lines);
            var bytes = BuildPdf(content);
            File.WriteAllBytes(fullPath, bytes);
        }

        private static List<string> BuildLines(ClientOfferRow offer)
        {
            var lines = new List<string>
            {
                (offer == null ? string.Empty : offer.TextSubject) ?? "Oferta cliente",
                string.Empty
            };

            var logos = (offer == null ? null : offer.TemplateLogos) ?? new List<string>();
            if (logos.Count == 0 && !string.IsNullOrWhiteSpace(offer == null ? null : offer.TextLogo))
            {
                logos = new List<string> { offer.TextLogo };
            }
            if (logos.Count > 0)
            {
                lines.Add("Logos:");
                foreach (var logoPath in logos.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    lines.Add("- " + logoPath);
                }
                lines.Add(string.Empty);
            }

            AddBlock(lines, offer == null ? null : offer.HeaderBody);
            lines.Add(string.Empty);
            if (!string.IsNullOrWhiteSpace(offer == null ? null : offer.TextMaterial))
            {
                lines.Add("Materiales");
                AddBlock(lines, offer.TextMaterial);
                lines.Add(string.Empty);
            }

            if (offer != null && offer.Captures != null && offer.Captures.Count > 0)
            {
                lines.Add("Planos adjuntos (capturas)");
                foreach (var capture in offer.Captures)
                {
                    var path = (capture == null ? null : capture.TextImagePath) ?? "";
                    lines.Add("- " + path);
                }
                lines.Add(string.Empty);
            }

            AddBlock(lines, offer == null ? null : offer.TextBody);
            return lines;
        }

        private static void AddBlock(List<string> lines, string block)
        {
            var parts = (block ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split(new[] { '\n' }, StringSplitOptions.None);
            foreach (var raw in parts)
            {
                var value = (raw ?? string.Empty).Trim();
                if (value.Length == 0)
                {
                    lines.Add(string.Empty);
                    continue;
                }

                const int max = 96;
                if (value.Length <= max)
                {
                    lines.Add(value);
                    continue;
                }

                var remaining = value;
                while (remaining.Length > max)
                {
                    lines.Add(remaining.Substring(0, max));
                    remaining = remaining.Substring(max);
                }

                if (remaining.Length > 0)
                {
                    lines.Add(remaining);
                }
            }
        }

        private static string BuildContent(List<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append("BT\n/F1 11 Tf\n50 790 Td\n");
            var yStep = 14;
            var y = 790;
            foreach (var line in lines.Take(47))
            {
                var escaped = EscapePdfText(line);
                if (y != 790)
                {
                    sb.AppendFormat("50 {0} Td\n", yStep * -1);
                }

                sb.AppendFormat("({0}) Tj\n", escaped);
                y -= yStep;
                if (y < 80)
                {
                    break;
                }
            }

            sb.Append("ET");
            return sb.ToString();
        }

        private static byte[] BuildPdf(string contentStream)
        {
            var enc = Encoding.GetEncoding(1252);
            var objs = new List<byte[]>
            {
                enc.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
                enc.GetBytes("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
                enc.GetBytes("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n"),
                enc.GetBytes("4 0 obj\n<< /Length " + enc.GetByteCount(contentStream) + " >>\nstream\n" + contentStream + "\nendstream\nendobj\n"),
                enc.GetBytes("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n")
            };

            using (var ms = new MemoryStream())
            {
                var header = enc.GetBytes("%PDF-1.4\n");
                ms.Write(header, 0, header.Length);
                var offsets = new List<int> { 0 };
                for (var i = 0; i < objs.Count; i++)
                {
                    offsets.Add((int)ms.Position);
                    ms.Write(objs[i], 0, objs[i].Length);
                }

                var xrefPos = (int)ms.Position;
                var xref = new StringBuilder();
                xref.Append("xref\n0 ").Append(objs.Count + 1).Append("\n");
                xref.Append("0000000000 65535 f \n");
                for (var i = 1; i < offsets.Count; i++)
                {
                    xref.Append(offsets[i].ToString("0000000000")).Append(" 00000 n \n");
                }

                var trailer = "trailer\n<< /Size " + (objs.Count + 1) + " /Root 1 0 R >>\nstartxref\n" + xrefPos + "\n%%EOF";
                var tail = enc.GetBytes(xref + trailer);
                ms.Write(tail, 0, tail.Length);
                return ms.ToArray();
            }
        }

        private static string EscapePdfText(string value)
        {
            var text = (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
            return text;
        }
    }
}
