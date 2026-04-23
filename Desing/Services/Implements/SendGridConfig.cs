using SendGrid;
using System.Configuration;
namespace Desing.Controllers
{
    public class SendGridConfig
    {
        private static SendGridClient client = null;
        private SendGridConfig() { }
        public static SendGridClient Instance()
        {
            if (client == null)
                client = new SendGridClient(ConfigurationManager.AppSettings["SENDGRID_APIKEY"].ToString());
            return client;
        }
    }
}