using DAL;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web.Mvc;
namespace Desing.Controllers
{
    public class BaseController : Controller
    {
        private ConexionData _db;

        protected ConexionData db
        {
            get
            {
                if (_db == null)
                {
                    _db = new ConexionData();
                }
                return _db;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Disponibilizar avatar y userName en todas las vistas (navbar Materio).
            try
            {
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var idUser = User.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(idUser))
                    {
                        var employee = db.TSql_Employee.FirstOrDefault(n => n.LinAspNetUsert == idUser);
                        if (employee != null)
                        {
                            ViewBag.avatar = employee.AttPhotoMenu;
                            ViewBag.userName = (employee.AttName + " " + employee.AttSurname).Trim();
                        }
                    }
                }
            }
            catch
            {
                // Si falla la consulta, simplemente no se establecen los ViewBag.
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}