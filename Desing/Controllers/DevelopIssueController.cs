using Desing.Helpers;
using Desing.Resources;
using Desing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    /// <summary>
    /// Develop › US y Bugs (<c>TSql_DevelopIssue</c> + <c>TSql_DevelopIssueDetail</c>).
    /// Acceso: función <c>DevelopIssue</c> (rol Develop) vía filtro global por controlador.
    /// </summary>
    [Authorize]
    public class DevelopIssueController : BaseController
    {
        private const string CaptureFolder = "~/Files/DevelopIssue";
        private const int MaxCaptureBytes = 10 * 1024 * 1024;
        private static readonly string[] CaptureExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
        private const string WallExtension = ".tnwall";
        private const int MaxWallBytes = 20 * 1024 * 1024;
        private const string WallHeader = "TNWALL1";

        public ActionResult Index()
        {
            ViewBag.TableMissing = !DevelopIssueRepository.TablesExist(db);
            return View(DevelopIssueRepository.ListIssues(db));
        }

        public ActionResult Details(long? id)
        {
            var issue = Load(id);
            if (issue == null) return HttpNotFound();

            ViewBag.IssueDetails = DevelopIssueRepository.ListDetails(db, issue.IdObject);
            ViewBag.Assessments = DevelopIssueRepository.ListAssessments(db, issue.IdObject);
            ViewBag.Audit = IntranetAuditHelper.BuildDisplay(
                db, issue.LinkMadeBy, issue.LinModifiedBy, issue.AddDateMade, issue.AddLastDateChange, issue.Ntimeschanged);
            return View(issue);
        }

        public ActionResult Create(string type)
        {
            if (!DevelopIssueRepository.TablesExist(db))
            {
                TempData["DevelopIssueError"] = DevelopIssue.Msg_NoTable;
                return RedirectToAction("Index");
            }

            var model = new DevelopIssueRow
            {
                TextType = DevelopIssueCodes.Types.Contains(type) ? type : "US",
                TextUrgency = "MEDIUM",
                TextState = "OPEN",
                DateIssue = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DevelopIssueRow model, string observation, IEnumerable<HttpPostedFileBase> captures, IEnumerable<HttpPostedFileBase> walls)
        {
            ValidateIssue(model);
            var files = ValidCaptures(captures);
            var wallFiles = ValidWalls(walls);
            var ajax = Request.IsAjaxRequest();
            if (ajax && ModelState.IsValid && !DevelopIssueRepository.TablesExist(db))
                ModelState.AddModelError("", DevelopIssue.Msg_NoTable);
            if (!ModelState.IsValid)
            {
                if (ajax)
                    return Json(new { ok = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)).Distinct()) });
                ViewBag.Observation = observation;
                return View(model);
            }

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            var id = DevelopIssueRepository.InsertIssue(db, model, userId);
            SaveDetails(id, model.TextLabel, observation, files, wallFiles, userId);

            var created = string.Format(DevelopIssue.Toast_Created, model.TextLabel.Trim());
            if (ajax) return Json(new { ok = true, id, message = created, url = Url.Action("Details", new { id }) });
            TempData["DevelopIssueOk"] = created;
            return RedirectToAction("Details", new { id });
        }

        public ActionResult Edit(long? id)
        {
            var issue = Load(id);
            if (issue == null) return HttpNotFound();
            return View(issue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DevelopIssueRow model)
        {
            var persisted = Load(model?.IdObject);
            if (persisted == null) return HttpNotFound();

            ValidateIssue(model);
            if (!ModelState.IsValid) return View(model);

            DevelopIssueRepository.UpdateIssue(db, model, IntranetAuditHelper.ResolveCurrentUserId(User));
            TempData["DevelopIssueOk"] = string.Format(DevelopIssue.Toast_Saved, model.TextLabel.Trim());
            return RedirectToAction("Details", new { id = model.IdObject });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDetail(long id, string observation, IEnumerable<HttpPostedFileBase> captures, IEnumerable<HttpPostedFileBase> walls)
        {
            var issue = Load(id);
            if (issue == null) return Request.IsAjaxRequest() ? (ActionResult)Json(new { ok = false, message = DevelopIssue.Err_NotFound }) : HttpNotFound();

            var files = ValidCaptures(captures);
            var wallFiles = ValidWalls(walls);
            if (string.IsNullOrWhiteSpace(observation) && files.Count == 0 && wallFiles.Count == 0 && ModelState.IsValid)
            {
                ModelState.AddModelError("observation", DevelopIssue.Err_DetailEmpty);
            }

            var ajax = Request.IsAjaxRequest();
            if (!ModelState.IsValid)
            {
                var error = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                if (ajax) return Json(new { ok = false, message = error });
                TempData["DevelopIssueError"] = error;
                return RedirectToAction("Details", new { id });
            }

            SaveDetails(id, issue.TextLabel, observation, files, wallFiles, IntranetAuditHelper.ResolveCurrentUserId(User));
            if (ajax) return Json(new { ok = true, message = DevelopIssue.Toast_DetailAdded });
            TempData["DevelopIssueOk"] = DevelopIssue.Toast_DetailAdded;
            return RedirectToAction("Details", new { id });
        }

        /// <summary>Guarda la captura editada como fichero nuevo; el original se conserva en disco.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveDetailImage(long id, long detailId, HttpPostedFileBase image)
        {
            var files = ValidCaptures(new[] { image });
            if (files.Count == 0 || !ModelState.IsValid)
            {
                var error = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { ok = false, message = string.IsNullOrEmpty(error) ? DevelopIssue.Err_ImageFormat : error });
            }

            var path = SaveCapture(id, image);
            var n = DevelopIssueRepository.UpdateDetailImage(db, id, detailId, path, IntranetAuditHelper.ResolveCurrentUserId(User));
            return Json(new { ok = n > 0, message = n > 0 ? DevelopIssue.Toast_ImageSaved : DevelopIssue.Err_NotFound, url = Url.Content("~" + path) });
        }

        /// <summary>Descarga el .tnwall adjunto con su nombre original (IIS no sirve esa extensión como estático).</summary>
        public ActionResult DownloadFile(long id, long detailId)
        {
            return Load(id) == null ? HttpNotFound() : WallFile(id, detailId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteDetail(long id, long detailId)
        {
            var n = DevelopIssueRepository.SoftDeleteDetail(db, id, detailId, IntranetAuditHelper.ResolveCurrentUserId(User));
            return Json(new { ok = n > 0, message = n > 0 ? DevelopIssue.Toast_Deleted : DevelopIssue.Err_NotFound });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(long id)
        {
            var n = DevelopIssueRepository.SoftDeleteIssue(db, id, IntranetAuditHelper.ResolveCurrentUserId(User));
            return Json(new { ok = n > 0, message = n > 0 ? DevelopIssue.Toast_Deleted : DevelopIssue.Err_NotFound });
        }

        /// <summary>Valoración orientativa con la IA de la intranet (título, observaciones y capturas).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Assess(long id)
        {
            var issue = Load(id);
            if (issue == null) return Json(new { ok = false, message = DevelopIssue.Err_NotFound });
            if (!DevelopIssueRepository.AssessmentTableExists(db)) return Json(new { ok = false, message = DevelopIssue.Msg_NoTable });
            if (!DevelopIssueAssessmentService.IsConfigured) return Json(new { ok = false, message = DevelopIssue.Assess_NoKey });

            var userId = IntranetAuditHelper.ResolveCurrentUserId(User);
            var language = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            var details = DevelopIssueRepository.ListDetails(db, id);
            try
            {
                var result = await DevelopIssueAssessmentService.AssessAsync(issue, details, Server.MapPath, language);
                DevelopIssueRepository.InsertAssessment(db, result, issue.TextLabel, userId);
                return Json(new { ok = true, message = DevelopIssue.Assess_Done });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DevelopIssue.Assess] " + ex);
                return Json(new { ok = false, message = DevelopIssue.Assess_Error });
            }
        }

        // ----- Agente de programación: acceso por token (Docs/areas/intranet/us-bugs-agente.md) -----

        /// <summary>US y bugs sin cerrar, con su última valoración.</summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AgentList()
        {
            if (!AgentAllowed()) return HttpNotFound();
            var data = DevelopIssueRepository.ListIssues(db)
                .Where(i => i.TextState != "CLOSED")
                .Select(i => AgentIssueJson(i, DevelopIssueRepository.ListAssessments(db, i.IdObject).FirstOrDefault()));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>Ficha completa: datos, observaciones, URL de las capturas y valoraciones.</summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AgentIssue(long id)
        {
            if (!AgentAllowed()) return HttpNotFound();
            var issue = Load(id);
            if (issue == null) return HttpNotFound();

            var root = Request.Url.GetLeftPart(UriPartial.Authority);
            return Json(new
            {
                issue = AgentIssueJson(issue, null),
                details = DevelopIssueRepository.ListDetails(db, id).Select(d => new
                {
                    id = d.IdObject,
                    date = d.AddDateMade.ToString("s"),
                    by = d.TextMadeBy,
                    observation = d.TextObservation,
                    imageUrl = string.IsNullOrWhiteSpace(d.TextImagePath) ? null : root + Url.Content("~" + d.TextImagePath),
                    fileName = string.IsNullOrWhiteSpace(d.TextFilePath) ? null : d.TextLabel,
                    fileUrl = string.IsNullOrWhiteSpace(d.TextFilePath) ? null : root + Url.Action("AgentFile", new { id, detailId = d.IdObject })
                }),
                assessments = DevelopIssueRepository.ListAssessments(db, id).Select(AgentAssessmentJson)
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>Descarga de un .tnwall adjunto para el agente.</summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AgentFile(long id, long detailId)
        {
            if (!AgentAllowed() || Load(id) == null) return HttpNotFound();
            return WallFile(id, detailId);
        }

        /// <summary>El agente anota su valoración, una observación y/o cambia el estado. Nunca borra.</summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AgentReport(long id, string size, decimal? hours, string area, string analysis, string observation, string state)
        {
            if (!AgentAllowed()) return HttpNotFound();
            var issue = Load(id);
            if (issue == null) return HttpNotFound();

            size = (size ?? "").Trim().ToUpperInvariant();
            state = (state ?? "").Trim().ToUpperInvariant();
            if (size.Length > 0 && !DevelopIssueCodes.Sizes.Contains(size)) return Json(new { ok = false, message = "size: SMALL | MEDIUM | LARGE" });
            if (state.Length > 0 && !DevelopIssueCodes.States.Contains(state)) return Json(new { ok = false, message = "state: OPEN | IN_PROGRESS | CLOSED" });
            if (size.Length > 0 && !DevelopIssueRepository.AssessmentTableExists(db)) return Json(new { ok = false, message = DevelopIssue.Msg_NoTable });

            const string agentUser = "agent";
            if (size.Length > 0)
            {
                DevelopIssueRepository.InsertAssessment(db, new DevelopIssueAssessmentRow
                {
                    LinkDevelopIssue = id, TextSource = "AGENT", TextSize = size, NumberHours = hours,
                    TextArea = area, TextAnalysis = analysis, TextModel = "Devin"
                }, issue.TextLabel, agentUser);
            }
            if (!string.IsNullOrWhiteSpace(observation))
                DevelopIssueRepository.InsertDetail(db, id, issue.TextLabel, observation, null, null, agentUser);
            if (state.Length > 0 && state != issue.TextState)
                DevelopIssueRepository.UpdateState(db, id, state, agentUser);

            return Json(new { ok = true });
        }

        /// <summary>Token en cabecera <c>X-Develop-Agent-Token</c>; sin <c>DEVELOP_AGENT_TOKEN</c> configurado el acceso no existe.</summary>
        private bool AgentAllowed()
        {
            var expected = Environment.GetEnvironmentVariable("DEVELOP_AGENT_TOKEN");
            if (string.IsNullOrWhiteSpace(expected)) expected = System.Configuration.ConfigurationManager.AppSettings["DEVELOP_AGENT_TOKEN"];
            var given = Request.Headers["X-Develop-Agent-Token"];
            if (string.IsNullOrWhiteSpace(expected) || expected.Trim().Length < 32 || string.IsNullOrEmpty(given)) return false;

            expected = expected.Trim();
            var diff = expected.Length ^ given.Length;
            for (var i = 0; i < expected.Length && i < given.Length; i++) diff |= expected[i] ^ given[i];
            return diff == 0;
        }

        private static object AgentIssueJson(DevelopIssueRow i, DevelopIssueAssessmentRow last)
        {
            return new
            {
                id = i.IdObject, name = i.TextLabel, type = i.TextType, urgency = i.TextUrgency, state = i.TextState,
                date = i.DateIssue.ToString("yyyy-MM-dd"),
                lastAssessment = last == null ? null : AgentAssessmentJson(last)
            };
        }

        private static object AgentAssessmentJson(DevelopIssueAssessmentRow a)
        {
            return new
            {
                source = a.TextSource, size = a.TextSize, hours = a.NumberHours, area = a.TextArea,
                analysis = a.TextAnalysis, date = a.AddDateMade.ToString("s")
            };
        }

        private DevelopIssueRow Load(long? id)
        {
            if (!id.HasValue || !DevelopIssueRepository.TablesExist(db)) return null;
            return DevelopIssueRepository.GetIssue(db, id.Value);
        }

        private void ValidateIssue(DevelopIssueRow model)
        {
            if (!DevelopIssueCodes.Types.Contains(model.TextType)) ModelState.AddModelError("TextType", DevelopIssue.Err_InvalidValue);
            if (!DevelopIssueCodes.Urgencies.Contains(model.TextUrgency)) ModelState.AddModelError("TextUrgency", DevelopIssue.Err_InvalidValue);
            if (!DevelopIssueCodes.States.Contains(model.TextState)) ModelState.AddModelError("TextState", DevelopIssue.Err_InvalidValue);
            if (model.DateIssue == default(DateTime)) model.DateIssue = DateTime.Today;
        }

        private List<HttpPostedFileBase> ValidCaptures(IEnumerable<HttpPostedFileBase> captures)
        {
            var list = (captures ?? Enumerable.Empty<HttpPostedFileBase>())
                .Where(f => f != null && f.ContentLength > 0).ToList();
            foreach (var f in list)
            {
                var ext = (Path.GetExtension(f.FileName) ?? "").ToLowerInvariant();
                if (!CaptureExtensions.Contains(ext)) ModelState.AddModelError("captures", DevelopIssue.Err_ImageFormat);
                else if (f.ContentLength > MaxCaptureBytes) ModelState.AddModelError("captures", DevelopIssue.Err_ImageSize);
            }
            return list;
        }

        /// <summary>Una fila de detalle por captura; la observación va en la primera (o sola si no hay capturas).</summary>
        private ActionResult WallFile(long issueId, long detailId)
        {
            var detail = DevelopIssueRepository.ListDetails(db, issueId)
                .FirstOrDefault(d => d.IdObject == detailId && !string.IsNullOrWhiteSpace(d.TextFilePath));
            if (detail == null) return HttpNotFound();
            var physical = Server.MapPath("~" + detail.TextFilePath);
            if (!System.IO.File.Exists(physical)) return HttpNotFound();
            var name = string.IsNullOrWhiteSpace(detail.TextLabel) ? Path.GetFileName(physical) : detail.TextLabel;
            return File(physical, "application/octet-stream", name);
        }

        /// <summary>Archivos de muros de Desing_2 (.tnwall): extensión, tamaño y cabecera <c>TNWALL1</c>.</summary>
        private List<HttpPostedFileBase> ValidWalls(IEnumerable<HttpPostedFileBase> walls)
        {
            var list = (walls ?? Enumerable.Empty<HttpPostedFileBase>())
                .Where(f => f != null && f.ContentLength > 0).ToList();
            foreach (var f in list)
            {
                var ext = (Path.GetExtension(f.FileName) ?? "").ToLowerInvariant();
                if (ext != WallExtension || !HasWallHeader(f)) ModelState.AddModelError("walls", DevelopIssue.Err_WallFormat);
                else if (f.ContentLength > MaxWallBytes) ModelState.AddModelError("walls", DevelopIssue.Err_WallSize);
            }
            return list;
        }

        private static bool HasWallHeader(HttpPostedFileBase f)
        {
            var head = new byte[WallHeader.Length];
            var read = f.InputStream.Read(head, 0, head.Length);
            f.InputStream.Position = 0;
            return read == head.Length && System.Text.Encoding.ASCII.GetString(head) == WallHeader;
        }

        private void SaveDetails(long issueId, string label, string observation, List<HttpPostedFileBase> files, List<HttpPostedFileBase> walls, string userId)
        {
            var total = files.Count + walls.Count;
            if (total == 0)
            {
                if (!string.IsNullOrWhiteSpace(observation))
                    DevelopIssueRepository.InsertDetail(db, issueId, label, observation, null, null, userId);
                return;
            }

            for (var i = 0; i < files.Count; i++)
            {
                var f = files[i];
                var fileLabel = Path.GetFileName(f.FileName);
                DevelopIssueRepository.InsertDetail(
                    db, issueId, string.IsNullOrWhiteSpace(fileLabel) ? label : fileLabel,
                    i == 0 ? observation : null, SaveCapture(issueId, f), null, userId);
            }

            for (var i = 0; i < walls.Count; i++)
            {
                var f = walls[i];
                var fileLabel = Path.GetFileName(f.FileName);
                DevelopIssueRepository.InsertDetail(
                    db, issueId, string.IsNullOrWhiteSpace(fileLabel) ? label : fileLabel,
                    files.Count == 0 && i == 0 ? observation : null, null, SaveCapture(issueId, f), userId);
            }
        }

        /// <summary>Guarda el fichero en <c>~/Files/DevelopIssue/yyyy-MM</c> y devuelve su ruta relativa a la raíz del sitio.</summary>
        private string SaveCapture(long issueId, HttpPostedFileBase f)
        {
            var monthFolder = CaptureFolder + "/" + DateTime.Now.ToString("yyyy-MM");
            var physical = Server.MapPath(monthFolder);
            Directory.CreateDirectory(physical);
            var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            var name = "issue-" + issueId + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
            f.SaveAs(Path.Combine(physical, name));
            return monthFolder.TrimStart('~') + "/" + name;
        }
    }
}
