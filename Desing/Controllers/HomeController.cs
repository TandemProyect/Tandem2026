using System.Web.Mvc;


namespace Desing.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            Session["SessionListMaterial"] = new string[] { };
            TempData.Clear();
            return View();
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}