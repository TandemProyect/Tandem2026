using SendGrid;
using System;

namespace Desing.Controllers
{
    [Obsolete("Usar Desing.Services.SendGridConfig.")]
    public class SendGridConfig
    {
        public static SendGridClient Instance()
        {
            return Services.SendGridConfig.CreateClient();
        }
    }
}
