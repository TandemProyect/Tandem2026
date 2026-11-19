using Desing.Helpers;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public partial class Desing_2Controller
    {
        [HttpGet]
        public JsonResult FormworkPanelColors(string system)
        {
            var colors = FormworkPanelColorRepository.ListForViewer(db, system);
            return Json(new { Exito = true, Colors = colors }, JsonRequestBehavior.AllowGet);
        }
    }
}
