using Microsoft.AspNet.Identity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web.Mvc;
namespace Desing.Controllers
{
    public class MenuController : BaseController
    {
        public ActionResult Index()
        {
            var IdUser = User.Identity.GetUserId();
            var Employee = db.TSql_Employee.Where(n => n.LinAspNetUsert == IdUser).FirstOrDefault();
            var avatar = "";
            var userName = "";
            if (Employee != null)
            {
                avatar = Employee.AttPhotoMenu;
                userName = (Employee.AttName + " " + Employee.AttSurname)/*.Substring(0, 10)*/;
            }
            ViewBag.avatar = avatar;
            ViewBag.userName = userName;

            return PartialView("_MenuAdmin");
        }
    }

}