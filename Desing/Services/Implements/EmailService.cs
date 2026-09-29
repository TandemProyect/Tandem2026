using Desing.Models;
using Desing.Services;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Desing.Controllers
{
    /// <summary>Plantillas HTML + envío vía SendGrid (<see cref="TandemMailClient"/>).</summary>
    public class EmailService
    {
        public string GetHtml(string basePathTemplate, object data)
        {
            if (!File.Exists(basePathTemplate))
                throw new Exception("La plantilla no se ha encontrado en el folder de plantillas: " + basePathTemplate);
            var templateText = File.ReadAllText(basePathTemplate);
            var template = Handlebars.Compile(templateText);
            return template(data);
        }

        public Task SendNotification(List<Documents> documents, string destination, string subject, string content)
        {
            return TandemMailClient.SendHtmlAsync(destination, subject, content, documents);
        }
    }
}
