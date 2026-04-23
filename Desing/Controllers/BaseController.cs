using DAL;
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