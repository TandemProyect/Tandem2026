using System.Net.Mail;
using System.Web.Mvc;
namespace SendMail.Controllers
{
    public class SendMailerController : Controller
    {
        //      
        // GET: /SendMailer/       
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ViewResult Index(SendMail.Models.MailModel _objModelMail)
        {
            if (ModelState.IsValid)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(_objModelMail.To);
                mail.From = new MailAddress(_objModelMail.From);
                mail.Subject = _objModelMail.Subject;
                string Body = _objModelMail.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "mail5005.smarterasp.net";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("admin@atenko.net", "010101Aa@");
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return View("Index", _objModelMail);
            }
            else
            {
                return View();
            }
        }
    }
}