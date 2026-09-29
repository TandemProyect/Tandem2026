using Desing.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Desing.Services
{
    /// <summary>
    /// Envío de correo por API SendGrid (HTTPS). No usa SMTP de SmarterASP.
    /// Clave: appSetting SENDGRID_APIKEY (Web.GoogleMaps.config o panel del host).
    /// </summary>
    public static class TandemMailClient
    {
        public static async Task SendHtmlAsync(
            string to,
            string subject,
            string html,
            IList<Documents> documents = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Destinatario vacío.", nameof(to));

            var client = SendGridConfig.CreateClient();
            var msg = new SendGridMessage
            {
                From = SendGridConfig.FromAddress(),
                Subject = subject ?? "",
                HtmlContent = string.IsNullOrEmpty(html) ? " " : html
            };
            msg.AddTo(to.Trim());
            msg.SetClickTracking(false, false);

            if (documents != null)
            {
                foreach (var item in documents)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Path) || !File.Exists(item.Path))
                        continue;
                    var bytes = File.ReadAllBytes(item.Path);
                    msg.AddAttachment(
                        item.Filename ?? Path.GetFileName(item.Path),
                        Convert.ToBase64String(bytes),
                        item.Type,
                        string.IsNullOrWhiteSpace(item.Disposition) ? "attachment" : item.Disposition,
                        item.ContentId);
                }
            }

            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            var code = (int)response.StatusCode;
            if (code >= 400)
            {
                var body = response.Body != null ? await response.Body.ReadAsStringAsync().ConfigureAwait(false) : "";
                throw new InvalidOperationException("SendGrid HTTP " + code + ". " + (body ?? "").Trim());
            }
        }

        public static void SendHtml(string to, string subject, string html)
        {
            SendHtmlAsync(to, subject, html).GetAwaiter().GetResult();
        }
    }

    public static class SendGridConfig
    {
        public static SendGridClient CreateClient()
        {
            var key = (ConfigurationManager.AppSettings["SENDGRID_APIKEY"] ?? "").Trim();
            if (key.Length == 0 || key.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Falta SENDGRID_APIKEY. Añádela en Web.GoogleMaps.config (local) o en el panel de SmarterASP (producción).");
            }
            return new SendGridClient(key);
        }

        public static EmailAddress FromAddress()
        {
            var from = (ConfigurationManager.AppSettings["SENDGRID_From"] ?? "admin@atenko.net").Trim();
            var name = (ConfigurationManager.AppSettings["SENDGRID_FromName"] ?? "T Desing.net").Trim();
            return new EmailAddress(from, name);
        }
    }
}
