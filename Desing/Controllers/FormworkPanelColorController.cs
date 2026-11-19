using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System.Globalization;
using System.Web.Mvc;

namespace Desing.Controllers
{
    [Authorize]
    public class FormworkPanelColorController : BaseController
    {
        public ActionResult Index()
        {
            return View(FormworkPanelColorRepository.List(db));
        }

        public ActionResult Create()
        {
            FillSystems(0);
            return View(new FormworkPanelColorEditModel
            {
                TextColor = "#000000",
                Is_Active = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormworkPanelColorEditModel model)
        {
            var error = FormworkPanelColorRepository.Save(
                db, 0, model.LinkSystem, model.NewSystemLabel, model.WidthMeters, model.TextColor, model.Is_Active, UserId());
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                FillSystems(model.LinkSystem);
                return View(model);
            }

            TempData["ToastMessage"] = "Color creado.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var row = FormworkPanelColorRepository.Get(db, id);
            if (row == null)
            {
                return HttpNotFound();
            }

            FillSystems(row.LinkSystem);
            return View(new FormworkPanelColorEditModel
            {
                IdObject = row.IdObject,
                LinkSystem = row.LinkSystem,
                WidthMeters = (row.NumberWidthMm / 1000m).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")),
                TextColor = row.TextColor,
                Is_Active = row.Is_Active
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormworkPanelColorEditModel model)
        {
            var error = FormworkPanelColorRepository.Save(
                db, model.IdObject, model.LinkSystem, model.NewSystemLabel, model.WidthMeters, model.TextColor, model.Is_Active, UserId());
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                FillSystems(model.LinkSystem);
                return View(model);
            }

            TempData["ToastMessage"] = "Color actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id)
        {
            FormworkPanelColorRepository.SoftDelete(db, id, UserId());
            TempData["ToastMessage"] = "Color desactivado (borrado lógico).";
            return RedirectToAction("Index");
        }

        private void FillSystems(long selected)
        {
            ViewBag.Systems = new SelectList(FormworkPanelColorRepository.ListSystems(db), "IdObject", "TextLabel", selected);
        }

        private string UserId()
        {
            return User.Identity.GetUserId();
        }
    }
}
