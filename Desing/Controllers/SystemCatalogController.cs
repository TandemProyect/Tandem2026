using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace Desing.Controllers
{
    [Authorize]
    public class SystemCatalogController : BaseController
    {
        public ActionResult Index()
        {
            return View(SystemCatalogRepository.List(db));
        }

        public ActionResult Create()
        {
            return View(new SystemCatalogEditModel
            {
                TextPanelColor = "#EFDF34",
                AddIsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SystemCatalogEditModel model)
        {
            var error = SystemCatalogRepository.Save(
                db, 0, model.TextLabel, model.TextDescription, model.TextPanelColor, model.AddIsActive, UserId());
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["ToastMessage"] = "Sistema creado.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(long id)
        {
            var row = SystemCatalogRepository.Get(db, id);
            if (row == null)
            {
                return HttpNotFound();
            }

            return View(ToModel(row));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SystemCatalogEditModel model)
        {
            var error = SystemCatalogRepository.Save(
                db, model.IdObject, model.TextLabel, DescriptionToStore(model), model.TextPanelColor, model.AddIsActive, UserId());
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["ToastMessage"] = "Sistema actualizado.";
            return RedirectToAction("Index");
        }

        private static SystemCatalogEditModel ToModel(SystemCatalogRow row)
        {
            var color = FormworkPanelColorRepository.NormalizeColor(row.TextPanelColor);
            return new SystemCatalogEditModel
            {
                IdObject = row.IdObject,
                TextLabel = row.TextLabel,
                TextDescription = SystemCatalogRepository.ToPlainText(row.TextDescription, 0),
                TextPanelColor = color ?? "#EFDF34",
                AddIsActive = row.AddIsActive
            };
        }

        private string DescriptionToStore(SystemCatalogEditModel model)
        {
            var posted = (model.TextDescription ?? string.Empty).Trim();
            if (model.IdObject <= 0)
            {
                return posted;
            }

            var existing = SystemCatalogRepository.Get(db, model.IdObject);
            if (existing == null)
            {
                return posted;
            }

            var plain = SystemCatalogRepository.ToPlainText(existing.TextDescription, 0);
            if (string.Equals(posted, plain, System.StringComparison.Ordinal)
                || string.Equals(posted, existing.TextDescription ?? string.Empty, System.StringComparison.Ordinal))
            {
                return existing.TextDescription;
            }

            return posted;
        }

        private string UserId()
        {
            return User.Identity.GetUserId();
        }
    }
}
