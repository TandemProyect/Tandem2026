using Desing.Services;
using System.Web.Mvc;

namespace SendMail.Controllers
{
    public class SendMailerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Index(SendMail.Models.MailModel _objModelMail)
        {
            if (!ModelState.IsValid)
                return View();

            TandemMailClient.SendHtml(_objModelMail.To, _objModelMail.Subject, _objModelMail.Body);
            return View("Index", _objModelMail);
        }
    }
}
