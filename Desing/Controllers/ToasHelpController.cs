using Desing.Models;
using System.Web.Mvc;
namespace Desing.Controllers
{
    public class ToasHelpController : BaseController
    {
        public ActionResult DataToaster(ToasType Type, string ToasConcept, string ToasMessage)
        {
            ToasterModel Model = new ToasterModel
            {
                ToasConcept = ToasConcept,
                ToasMessage = ToasMessage,
                Type = Type,
            };
            return PartialView("_DataToaster", Model);
        }
    }
}