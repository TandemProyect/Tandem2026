using DAL;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Desing.Helpers;
using Desing.Models;
using Desing.Repositories.RepositoryAtk60;
using Desing.Repositories.RepositoryCommun;
using Desing.Resources;
using Desing.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;

namespace Desing.Controllers
{
    public class DesignToolsAutocadController : BaseController
    {
        /// <summary>
        /// Paleta izquierda: modo de dibujo + sistema Atk-60 (mismos botones que Desing_2).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PaletteMode()
        {
            return View();
        }

        /// <summary>
        /// Paleta central: herramientas CAD de Desing_2 (polilínea, muro, copiar, borrar…).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PaletteTools()
        {
            return View();
        }

        /// <summary>
        /// Arranque de sesión CAD: si el equipo está en TSql_PluginDeviceAuth y activo,
        /// inicia sesión con el usuario ligado. El alta del equipo es en Personal, no aquí.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PluginSession(string deviceId, string machineName, string usuarioWindows, string pluginVersion)
        {
            var snap = new PluginCadDeviceHelper.Snapshot
            {
                DeviceId = (deviceId ?? "").Trim(),
                MachineName = machineName,
                UsuarioWindows = usuarioWindows,
                PluginVersion = pluginVersion
            };
            PluginCadDeviceHelper.WriteCookie(Response, Request, snap);

            var pluginReadyUrl = Url.Action("PluginCadAuth", "DesignToolsAutocad");

            // No abrir ConexionData aquí: el primer uso del EDMX tarda minutos y deja
            // el splash de AutoCAD en "Comprobando autorización…". Si ya hay cookie
            // Identity en WebView2, entrar; si no, login inmediato.
            TryWarmEdmxInBackground();
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("PluginCadAuth");

            return RedirectToPluginLogin(pluginReadyUrl, blocked: false);
        }

        /// <summary>
        /// Instalación del plugin: solo con token de correo. No está en el menú.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult InstallPlugin(string t)
        {
            string userId;
            long employeeId;
            string error;
            ViewBag.Token = t;
            ViewBag.MachineName = "";
            if (!PluginInstallLink.TryRead(t, out employeeId, out userId, out error))
            {
                ViewBag.State = error == "expired" ? "expired" : "invalid";
                return View();
            }

            var employee = db.TSql_Employee.AsNoTracking()
                .FirstOrDefault(e => e.SysObjectID == employeeId && e.LinAspNetUsert == userId && !e.AttIsDeleted);
            var device = PluginCadDeviceHelper.FindAuthorizedForUser(db, userId);
            if (employee == null || device == null || string.IsNullOrWhiteSpace(device.MachineName))
            {
                ViewBag.State = "nodevice";
                return View();
            }

            ViewBag.State = "ready";
            ViewBag.EmployeeName = ((employee.AttName ?? "") + " " + (employee.AttSurname ?? "")).Trim();
            ViewBag.MachineName = device.MachineName.Trim();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult InstallPluginClaim(string t, string machineName)
        {
            string userId;
            long employeeId;
            string error;
            if (!PluginInstallLink.TryRead(t, out employeeId, out userId, out error))
                return Json(new { ok = false, message = error == "expired" ? Common.PluginInstall_Expired : Common.PluginInstall_Invalid });

            var device = PluginCadDeviceHelper.FindAuthorizedForUser(db, userId);
            if (device == null)
                return Json(new { ok = false, message = Common.PluginInstall_NoDevice });
            if (!PluginCadDeviceHelper.MachineMatches(device, machineName))
                return Json(new { ok = false, message = Common.PluginInstall_DeviceMismatch });

            return Json(new
            {
                ok = true,
                packageUrl = Url.Action("InstallPluginPackage", "DesignToolsAutocad", new { t, machineName = machineName.Trim() })
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult InstallPluginPackage(string t, string machineName)
        {
            string userId;
            long employeeId;
            string error;
            if (!PluginInstallLink.TryRead(t, out employeeId, out userId, out error))
                return new HttpStatusCodeResult(403);
            var device = PluginCadDeviceHelper.FindAuthorizedForUser(db, userId);
            if (device == null || !PluginCadDeviceHelper.MachineMatches(device, machineName))
                return new HttpStatusCodeResult(403);

            var bundleRoot = Server.MapPath("~/Content/DesignTools/CadPlugin/AtDesing.bundle");
            if (string.IsNullOrWhiteSpace(bundleRoot) || !Directory.Exists(bundleRoot))
                return new HttpStatusCodeResult(404);

            var tmp = Path.Combine(Path.GetTempPath(), "AtDesing-" + Guid.NewGuid().ToString("N") + ".zip");
            if (System.IO.File.Exists(tmp))
                System.IO.File.Delete(tmp);
            ZipFile.CreateFromDirectory(bundleRoot, tmp);
            var bytes = System.IO.File.ReadAllBytes(tmp);
            try { System.IO.File.Delete(tmp); } catch { }
            return File(bytes, "application/zip", "AtDesing.bundle.zip");
        }

        private ActionResult RedirectToPluginLogin(string pluginReadyUrl, bool blocked = false, bool inactiveUser = false)
        {
            if (blocked || inactiveUser)
                PluginCadDeviceHelper.ClearLogoCookie(Response);

            var url = "~/Account/Login?returnUrl=" + Uri.EscapeDataString(pluginReadyUrl ?? "");
            if (blocked)
                url += "&pluginDeviceBlocked=1";
            if (inactiveUser)
                url += "&pluginUserInactive=1";
            return Redirect(url);
        }

        /// <summary>
        /// Handshake CAD rápido: sin EDMX ni listados. El menú general sigue en PluginReady.
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult PluginCadAuth()
        {
            TryWarmEdmxInBackground();
            return View();
        }

        private static int _edmxWarmStarted;

        private static void TryWarmEdmxInBackground()
        {
            if (System.Threading.Interlocked.Exchange(ref _edmxWarmStarted, 1) == 1)
                return;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using (var warmup = new ConexionData())
                    {
                        if (warmup.Database.Connection.State != ConnectionState.Open)
                            warmup.Database.Connection.Open();
                        warmup.Database.Connection.Close();
                    }
                }
                catch
                {
                    System.Threading.Interlocked.Exchange(ref _edmxWarmStarted, 0);
                }
            });
        }

        private void PersistPluginCompanyLogo(string userId)
        {
            var logo = PluginCadDeviceHelper.ResolveCompanyLogoAbsoluteUrl(db, userId, Request, Url);
            PluginCadDeviceHelper.WriteLogoCookie(Response, Request, logo);
            ViewBag.CompanyLogoUrl = logo;
        }

        /// <summary>
        /// Paleta de sesión del plugin CAD: exige login (cookie Identity).
        /// Menú general (obras / ofertas / diseños) — el diseño se abre en el plugin CAD.
        /// La ficha se pinta al momento; los listados llegan por PluginHomeData.
        /// </summary>
        [HttpGet]
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PluginReady()
        {
            var userId = User.Identity.GetUserId();
            if (!PluginCadDeviceHelper.IsUserAllowed(db, userId))
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToPluginLogin(Url.Action("PluginReady", "DesignToolsAutocad"), inactiveUser: true);
            }

            var deviceSnap = PluginCadDeviceHelper.TryReadCookie(Request);
            var deviceRow = PluginCadDeviceHelper.Find(db, deviceSnap != null ? deviceSnap.DeviceId : null);
            if (!PluginCadDeviceHelper.IsCadDeveloper(db, userId)
                && (PluginCadDeviceHelper.IsBlocked(deviceRow)
                    || !PluginCadDeviceHelper.AllowsPluginOnThisPc(db, userId, deviceSnap)))
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToPluginLogin(Url.Action("PluginReady", "DesignToolsAutocad"), blocked: true);
            }

            PersistPluginCompanyLogo(userId);
            return View(new PluginCadHomeVm
            {
                UserName = User.Identity.Name,
                Jobsides = new List<PluginCadJobsideRow>(),
                Offers = new List<PluginCadOfferRow>(),
                Designs = new List<PluginCadDesignRow>(),
                Clients = new List<PluginCadLookupItem>(),
                Branches = new List<PluginCadLookupItem>(),
                OfferStates = new List<PluginCadLookupItem>()
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PluginPing()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long dbMs = -1;
            try
            {
                db.Database.SqlQuery<int>("SELECT CAST(1 AS INT)").FirstOrDefault();
                dbMs = sw.ElapsedMilliseconds;
            }
            catch
            {
                dbMs = -1;
            }
            return Json(new
            {
                ok = dbMs >= 0,
                dbMs,
                serverMs = sw.ElapsedMilliseconds
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Listados vivos de obras / ofertas / diseños para el menú general CAD.
        /// </summary>
        [HttpGet]
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PluginHomeData()
        {
            if (!EnsurePluginCadUser())
                return new HttpUnauthorizedResult();

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var vm = LoadPluginHomeLists();
                var json = Json(new
                {
                    ok = true,
                    listMs = sw.ElapsedMilliseconds,
                    userName = User.Identity.Name ?? "",
                    jobsides = vm.Jobsides.Select(j => new { id = j.Id, code = j.Code ?? "", label = j.Label ?? "" }).ToList(),
                    offers = vm.Offers.Select(o => new { id = o.Id, jobsideId = o.JobsideId, number = o.Number ?? "", label = o.Label ?? "" }).ToList(),
                    designs = vm.Designs.Select(d => new { id = d.Id, label = d.Label ?? "", offerNumber = d.OfferNumber ?? "" }).ToList(),
                    clients = vm.Clients.Select(c => new { id = c.Id, label = c.Label ?? "" }).ToList(),
                    branches = vm.Branches.Select(b => new { id = b.Id, label = b.Label ?? "" }).ToList(),
                    offerStates = vm.OfferStates.Select(s => new { id = s.Id, label = s.Label ?? "" }).ToList()
                }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch
            {
                return Json(new { ok = false, message = Common.PluginCad_HomeLoadFailed }, JsonRequestBehavior.AllowGet);
            }
        }

        private PluginCadHomeVm LoadPluginHomeLists()
        {
            return new PluginCadHomeVm
            {
                UserName = User.Identity.Name,
                Jobsides = db.TSql_Jobside.AsNoTracking()
                    .Where(j => !j.Is_Delete)
                    .OrderByDescending(j => j.AddLastDateChange ?? j.AddDateMade)
                    .ThenByDescending(j => j.IdObject)
                    .Take(80)
                    .Select(j => new PluginCadJobsideRow
                    {
                        Id = j.IdObject,
                        Code = j.AddNJobside,
                        Label = j.TextLabel
                    })
                    .ToList(),
                Offers = db.TSql_Offers.AsNoTracking()
                    .Where(o => !o.Is_Delete)
                    .OrderByDescending(o => o.Ntimeschanged)
                    .ThenByDescending(o => o.AddDateMade)
                    .ThenByDescending(o => o.IdObject)
                    .Take(80)
                    .Select(o => new PluginCadOfferRow
                    {
                        Id = o.IdObject,
                        JobsideId = o.LinkJobside,
                        Number = o.AddOfferNumber,
                        Label = o.TextLabel
                    })
                    .ToList(),
                Designs = (from d in db.TSql_Design_V2.AsNoTracking()
                           join o in db.TSql_Offers.AsNoTracking() on d.LinkOffers equals o.IdObject
                           where !d.AttIsDeleted && !o.Is_Delete
                           orderby d.AttChange descending, d.SysObjectID descending
                           select new PluginCadDesignRow
                           {
                               Id = d.SysObjectID,
                               Label = d.AttLabel,
                               OfferNumber = o.AddOfferNumber
                           })
                    .Take(80)
                    .ToList(),
                Clients = db.TSql_Client_V2.AsNoTracking()
                    .Where(c => !c.Is_Delete && c.Is_Active)
                    .OrderBy(c => c.TextLabel)
                    .Take(80)
                    .Select(c => new PluginCadLookupItem { Id = c.IdObject, Label = c.TextLabel })
                    .ToList(),
                Branches = db.TSql_Branch.AsNoTracking()
                    .OrderBy(b => b.AttLabel)
                    .Select(b => new PluginCadLookupItem { Id = b.SysObjectID, Label = b.AttLabel })
                    .ToList(),
                OfferStates = db.TSql_OfferState.AsNoTracking()
                    .Where(s => !s.Is_Delete && s.Is_Active)
                    .OrderBy(s => s.TextLabel)
                    .Select(s => new PluginCadLookupItem { Id = s.IdObject, Label = s.TextLabel })
                    .ToList()
            };
        }

        /// <summary>
        /// Paleta CAD: biblioteca de bloques (maestro de artículos). Solo formulario; no inserta.
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult PluginBlocks()
        {
            if (!EnsurePluginCadUser())
                return RedirectToPluginLogin(Url.Action("PluginBlocks", "DesignToolsAutocad"), inactiveUser: true);

            PersistPluginCompanyLogo(User.Identity.GetUserId());
            var logo = ViewBag.CompanyLogoUrl as string;
            if (string.IsNullOrWhiteSpace(logo))
                logo = Url.Content("~" + PluginCadDeviceHelper.DefaultLogoVirtualPath);

            return View(new PluginCadBlocksVm
            {
                LogoUrl = logo,
                Items = new List<PluginCadBlockRow>()
            });
        }

        [HttpGet]
        [Authorize]
        public ActionResult PluginSearchBlocks(string q)
        {
            if (!EnsurePluginCadUser())
                return new HttpUnauthorizedResult();

            try
            {
                var json = Json(QueryPluginBlocks(q), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch
            {
                return Json(new List<PluginCadBlockRow>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Paleta CAD / convert: URL del DWG 3D, 3DRef o Xr según Tsql_Master_Articles.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PluginBlockDwg(string codeName, string view)
        {
            var code = (codeName ?? "").Trim();
            var kind = (view ?? "3dref").Trim();
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { ok = false, url = (string)null }, JsonRequestBehavior.AllowGet);

            var article = FindMasterArticleForPlugin(code);
            if (article == null)
                return Json(new { ok = false, url = (string)null, codeName = code }, JsonRequestBehavior.AllowGet);

            string virtualPath;
            if (string.Equals(kind, "xr", StringComparison.OrdinalIgnoreCase))
                virtualPath = article.LinkBlockDwgXr;
            else if (string.Equals(kind, "3d", StringComparison.OrdinalIgnoreCase))
                virtualPath = FirstNonEmpty(article.LinkBlockDwg3D, article.LinkBlockDwgPlant3D);
            else
                virtualPath = FirstNonEmpty(article.LinkBlockDwg3DRef, article.LinkBlockDwgPlant3D);

            var url = ToPublicUrl(virtualPath);
            return Json(new
            {
                ok = !string.IsNullOrWhiteSpace(url),
                url,
                codeName = FirstNonEmpty(article.TextCode, code),
                view = kind
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Manifiesto de DWG 3D / 3DRef / Xr + snaps para caché local del plugin.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PluginBlockLibrary()
        {
            var files = new List<PluginCadLibraryFile>();
            var catalog = new List<PluginCadBlockRow>();
            var articles = db.Tsql_Master_Articles.AsNoTracking()
                .Where(a => a.AddIsActive)
                .Where(a =>
                    (a.LinkBlockDwg3D != null && a.LinkBlockDwg3D != "")
                    || (a.LinkBlockDwg3DRef != null && a.LinkBlockDwg3DRef != "")
                    || (a.LinkBlockDwgXr != null && a.LinkBlockDwgXr != "")
                    || (a.LinkBlockDwgPlant3D != null && a.LinkBlockDwgPlant3D != ""))
                .ToList();
            var icoById = LoadMasterArticleIcoMap(articles.Select(a => a.IdObject));

            foreach (var article in articles)
            {
                string codeName;
                PluginCadAtk60PanelStlHelper.TryGetCodeName(article, out codeName);
                codeName = FirstNonEmpty(codeName, article.TextCode, article.TextBlockNumber);
                if (string.IsNullOrWhiteSpace(codeName))
                    continue;

                AddLibraryFile(files, article, codeName, "3d", "3D",
                    FirstNonEmpty(article.LinkBlockDwg3D, article.LinkBlockDwgPlant3D),
                    codeName + ".dwg");
                AddLibraryFile(files, article, codeName, "3dref", "3DRef",
                    FirstNonEmpty(article.LinkBlockDwg3DRef, article.LinkBlockDwgPlant3D),
                    codeName + "R.dwg");
                AddLibraryFile(files, article, codeName, "xr", "Xr",
                    article.LinkBlockDwgXr,
                    codeName + "X.dwg");

                var snapVirtual = "~/Content/DesignTools/DWG/AtkSystem60/Snaps/" + codeName + ".json";
                AddLibraryFile(files, article, codeName, "snap", "Snaps", snapVirtual, codeName + ".json");
                catalog.Add(MapPluginBlock(article, icoById));
            }

            return Json(new PluginCadLibraryManifest
            {
                Ok = true,
                GeneratedUtc = DateTime.UtcNow.ToString("o"),
                Files = files,
                Catalog = catalog
            }, JsonRequestBehavior.AllowGet);
        }

        private void AddLibraryFile(
            List<PluginCadLibraryFile> files,
            DAL.Tsql_Master_Articles article,
            string codeName,
            string view,
            string folder,
            string virtualPath,
            string defaultFile)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
                return;

            long size;
            long ticks;
            var hasFile = TryFileStamp(virtualPath, out size, out ticks);
            if (!hasFile && string.Equals(view, "snap", StringComparison.OrdinalIgnoreCase))
                return;
            if (!hasFile && string.IsNullOrWhiteSpace(ToPublicUrl(virtualPath)))
                return;

            var fileName = defaultFile;
            var version = hasFile
                ? size.ToString(CultureInfo.InvariantCulture) + "|" + ticks.ToString(CultureInfo.InvariantCulture)
                : article.AddLastDateChange.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture)
                    + "|" + article.Ntimeschanged.ToString(CultureInfo.InvariantCulture);

            var url = Url.Action("PluginLibraryFile", "DesignToolsAutocad", new { folder, file = fileName });
            if (string.IsNullOrWhiteSpace(url))
                return;

            files.Add(new PluginCadLibraryFile
            {
                Code = codeName,
                View = view,
                Folder = folder,
                File = fileName,
                Url = url,
                Version = version + "|" + url,
                Size = size
            });
        }

        /// <summary>
        /// Descarga un DWG/JSON de la biblioteca ATK-60. El plugin lo copia a AppData.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PluginLibraryFile(string folder, string file)
        {
            var dir = (folder ?? "").Trim();
            var name = Path.GetFileName((file ?? "").Trim());
            if (string.IsNullOrWhiteSpace(name)
                || !(string.Equals(dir, "3D", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dir, "3DRef", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dir, "Xr", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dir, "Snaps", StringComparison.OrdinalIgnoreCase)))
                return HttpNotFound();

            string physical;
            try
            {
                physical = Server.MapPath("~/Content/DesignTools/DWG/AtkSystem60/" + dir + "/" + name);
            }
            catch
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(physical) || !System.IO.File.Exists(physical))
                return HttpNotFound();

            var ext = (Path.GetExtension(name) ?? "").ToLowerInvariant();
            var mime = ext == ".json" ? "application/json" : "application/octet-stream";
            return File(physical, mime, name);
        }

        private bool TryFileStamp(string virtualPath, out long size, out long ticks)
        {
            size = 0;
            ticks = 0;
            var appRel = ToAppRelativePath(virtualPath);
            if (string.IsNullOrWhiteSpace(appRel))
                return false;
            try
            {
                var physical = Server.MapPath(appRel);
                if (string.IsNullOrWhiteSpace(physical) || !System.IO.File.Exists(physical))
                    return false;
                var info = new FileInfo(physical);
                size = info.Length;
                ticks = info.LastWriteTimeUtc.Ticks;
                return info.Length >= 64;
            }
            catch
            {
                return false;
            }
        }

        private string AbsoluteContentUrl(string url)
        {
            var t = (url ?? "").Trim();
            if (t.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return t;
            var root = Request.Url == null
                ? ""
                : Request.Url.GetLeftPart(UriPartial.Authority);
            return root + (t.StartsWith("/") ? t : "/" + t.TrimStart('~', '/'));
        }

        private DAL.Tsql_Master_Articles FindMasterArticleForPlugin(string code)
        {
            var rows = db.Tsql_Master_Articles.AsNoTracking()
                .Where(a => a.AddIsActive)
                .Where(a =>
                    a.TextCode == code
                    || a.TextBlockNumber == code
                    || a.AddAtenkoCode == code)
                .Take(8)
                .ToList();
            if (rows.Count == 1)
                return rows[0];
            if (rows.Count > 1)
                return rows.FirstOrDefault(a => a.TextCode == code) ?? rows[0];

            var mapped = Atk60CodeNameFromAtenko(code);
            if (string.IsNullOrWhiteSpace(mapped))
                return null;
            return db.Tsql_Master_Articles.AsNoTracking()
                .FirstOrDefault(a => a.AddIsActive && (a.TextCode == mapped || a.TextBlockNumber == mapped));
        }

        private static string Atk60CodeNameFromAtenko(string atenko)
        {
            if (string.IsNullOrWhiteSpace(atenko))
                return null;
            var digits = new string(atenko.Where(char.IsDigit).ToArray());
            if (digits.Length != 10 || !digits.StartsWith("3120", StringComparison.Ordinal))
                return null;
            int h;
            int w;
            if (!int.TryParse(digits.Substring(4, 3), out h) || !int.TryParse(digits.Substring(7, 3), out w))
                return null;
            if (h == 270 && w == 90) return "27904209";
            if (h == 270 && w == 60) return "27604207";
            if (h == 270 && w == 45) return "27454206";
            if (h == 270 && w == 30) return "27304205";
            if (h == 240 && w == 90) return "24904240";
            if (h == 240 && w == 60) return "24604242";
            if (h == 240 && w == 45) return "24454243";
            if (h == 240 && w == 30) return "24304244";
            if (h == 120 && w == 90) return "12904215";
            if (h == 120 && w == 60) return "12604213";
            if (h == 120 && w == 45) return "12454212";
            if (h == 120 && w == 30) return "12304211";
            if (h == 270 && w == 75) return "27104219";
            if (h == 240 && w == 75) return "24104224";
            if (h == 120 && w == 75) return "12104120";
            return null;
        }

        private bool EnsurePluginCadUser()
        {
            var userId = User.Identity.GetUserId();
            if (!PluginCadDeviceHelper.IsUserAllowed(db, userId))
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return false;
            }

            var deviceSnap = PluginCadDeviceHelper.TryReadCookie(Request);
            var deviceRow = PluginCadDeviceHelper.Find(db, deviceSnap != null ? deviceSnap.DeviceId : null);
            if (!PluginCadDeviceHelper.IsCadDeveloper(db, userId)
                && (PluginCadDeviceHelper.IsBlocked(deviceRow)
                    || !PluginCadDeviceHelper.AllowsPluginOnThisPc(db, userId, deviceSnap)))
            {
                HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return false;
            }

            return true;
        }

        private List<PluginCadBlockRow> QueryPluginBlocks(string q)
        {
            var term = (q ?? "").Trim();
            var query = db.Tsql_Master_Articles.AsNoTracking().Where(a => a.AddIsActive);
            if (term.Length > 0)
            {
                query = query.Where(a =>
                    (a.AddAtenkoCode != null && a.AddAtenkoCode.Contains(term))
                    || (a.TextCode != null && a.TextCode.Contains(term))
                    || (a.TextLabel != null && a.TextLabel.Contains(term))
                    || (a.TextStlNumber != null && a.TextStlNumber.Contains(term))
                    || (a.TextBlockNumber != null && a.TextBlockNumber.Contains(term))
                    || (a.LinkBlockDwgPlantStl != null && a.LinkBlockDwgPlantStl.Contains(term))
                    || (a.LinkBlockDwg3D != null && a.LinkBlockDwg3D.Contains(term))
                    || (a.LinkBlockDwg3DRef != null && a.LinkBlockDwg3DRef.Contains(term))
                    || (a.LinkBlockDwgXr != null && a.LinkBlockDwgXr.Contains(term)));
            }

            var rows = query
                .OrderBy(a => a.TextLabel)
                .ThenBy(a => a.AddAtenkoCode)
                .Take(80)
                .ToList();

            if (term.Length >= 4 && rows.Count == 0)
            {
                var panels = db.Tsql_Master_Articles.AsNoTracking()
                    .Where(a => a.AddIsActive && a.AddAtenkoCode != null && a.AddAtenkoCode.StartsWith("3120"))
                    .Take(200)
                    .ToList();
                rows = panels
                    .Where(a => ArticleMatchesCodeName(a, term))
                    .OrderBy(a => a.TextLabel)
                    .ThenBy(a => a.AddAtenkoCode)
                    .Take(80)
                    .ToList();
            }

            var icoById = LoadMasterArticleIcoMap(rows.Select(a => a.IdObject));
            return rows.Select(a => MapPluginBlock(a, icoById)).ToList();
        }

        private Dictionary<long, string> LoadMasterArticleIcoMap(IEnumerable<long> ids)
        {
            var idList = ids != null ? ids.Distinct().ToList() : new List<long>();
            if (idList.Count == 0)
                return new Dictionary<long, string>();
            try
            {
                var inList = string.Join(",", idList.Select(id => id.ToString(CultureInfo.InvariantCulture)));
                return db.Database.SqlQuery<PluginCadArticleIcoRow>(
                        "SELECT IdObject, ImgIco FROM dbo.Tsql_Master_Articles WHERE IdObject IN (" + inList + ") AND ImgIco IS NOT NULL AND LTRIM(RTRIM(ImgIco)) <> ''")
                    .ToDictionary(x => x.IdObject, x => x.ImgIco);
            }
            catch
            {
                return new Dictionary<long, string>();
            }
        }

        private PluginCadBlockRow MapPluginBlock(DAL.Tsql_Master_Articles a, Dictionary<long, string> icoById)
        {
            var code = FirstNonEmpty(a.AddAtenkoCode, a.TextCode);
            var size = FormatBlockSize(a.NumberHigh, a.NumberWidth);
            var captionParts = new[] { code, a.TextLabel };
            if (!string.IsNullOrWhiteSpace(size) && (a.TextLabel == null || a.TextLabel.IndexOf(size, StringComparison.OrdinalIgnoreCase) < 0))
                captionParts = new[] { code, a.TextLabel, size };
            var caption = string.Join(" ", captionParts.Where(s => !string.IsNullOrWhiteSpace(s)));
            string storedIco = null;
            if (icoById != null)
                icoById.TryGetValue(a.IdObject, out storedIco);
            string stlUrl;
            string stlPhenolicUrl;
            ResolveStlPair(a, out stlUrl, out stlPhenolicUrl);
            string codeName;
            PluginCadAtk60PanelStlHelper.TryGetCodeName(a, out codeName);
            if (string.IsNullOrWhiteSpace(codeName))
                codeName = FirstNonEmpty(a.TextCode, a.TextBlockNumber);
            var dwg3d = ToPublicUrl(FirstNonEmpty(a.LinkBlockDwg3D, a.LinkBlockDwgPlant3D));
            var dwg3dRef = ToPublicUrl(FirstNonEmpty(a.LinkBlockDwg3DRef, a.LinkBlockDwgPlant3D));
            var dwgXr = ToPublicUrl(a.LinkBlockDwgXr);
            return new PluginCadBlockRow
            {
                Id = a.IdObject,
                Code = code,
                CodeName = codeName,
                Label = a.TextLabel,
                Caption = caption,
                IcoUrl = ResolveIcoUrl(a, storedIco),
                StlUrl = stlUrl,
                StlPhenolicUrl = stlPhenolicUrl,
                DwgUrl = FirstNonEmpty(dwg3d, dwg3dRef, dwgXr),
                DwgUrl3D = dwg3d,
                DwgUrl3DRef = dwg3dRef,
                DwgUrlXr = dwgXr
            };
        }

        private static bool ArticleMatchesCodeName(DAL.Tsql_Master_Articles a, string term)
        {
            string frame;
            string phenolic;
            if (!PluginCadAtk60PanelStlHelper.TryResolve(a, out frame, out phenolic))
                return false;
            return (!string.IsNullOrWhiteSpace(frame) && frame.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                || (!string.IsNullOrWhiteSpace(phenolic) && phenolic.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private string ResolveIcoUrl(DAL.Tsql_Master_Articles a, string storedIco)
        {
            var fromDb = ToPublicUrl(storedIco);
            if (!string.IsNullOrWhiteSpace(fromDb))
                return fromDb;

            var code = FirstNonEmpty(a.AddAtenkoCode, a.TextCode);
            if (!string.IsNullOrWhiteSpace(code))
                return ToPublicUrl("~/Files/MaterialIco/" + code.Trim() + ".png");

            var label = (a.TextLabel ?? "").ToLowerInvariant();
            if (label.Contains("atk") || label.Contains("panel"))
                return ToPublicUrl("~/Files/MaterialIco/panel.png");

            return ToPublicUrl("~/Files/MaterialIco/SinArticulo.png");
        }

        private void ResolveStlPair(DAL.Tsql_Master_Articles a, out string stlUrl, out string stlPhenolicUrl)
        {
            stlUrl = null;
            stlPhenolicUrl = null;
            string catalogFrame;
            string catalogPhenolic;
            if (PluginCadAtk60PanelStlHelper.TryResolve(a, out catalogFrame, out catalogPhenolic))
            {
                stlUrl = ToPublicUrl(catalogFrame);
                stlPhenolicUrl = ToPublicUrl(catalogPhenolic);
                if (!string.IsNullOrWhiteSpace(stlUrl))
                    return;
            }

            stlUrl = ResolveStlUrl(a);
            stlPhenolicUrl = ResolvePhenolicSiblingUrl(stlUrl);
        }

        private string ResolveStlUrl(DAL.Tsql_Master_Articles a)
        {
            var candidates = new[]
            {
                a.LinkBlockDwgPlantStl,
                a.LinkBlockDwgVerticalElevationStl,
                a.LinkBlockDwgHorizontalElevationStl
            };
            foreach (var p in candidates)
            {
                var url = ToPublicUrl(p);
                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }

            var dwg = FirstNonEmpty(a.LinkBlockDwg3D, a.LinkBlockDwgPlant3D);
            if (!string.IsNullOrWhiteSpace(dwg))
            {
                var sibling = Path.ChangeExtension(dwg.Trim().Replace('\\', '/'), ".stl");
                var siblingUrl = ToPublicUrl(sibling);
                if (!string.IsNullOrWhiteSpace(siblingUrl))
                    return siblingUrl;
            }

            return ToPublicUrl(FirstNonEmpty(candidates));
        }

        private string ResolvePhenolicSiblingUrl(string publicStlUrl)
        {
            if (string.IsNullOrWhiteSpace(publicStlUrl))
                return null;

            var path = publicStlUrl.Replace('\\', '/');
            var q = path.IndexOf('?');
            if (q >= 0)
                path = path.Substring(0, q);
            if (!path.EndsWith(".stl", StringComparison.OrdinalIgnoreCase))
                return null;

            string siblingVirtual = null;
            if (path.EndsWith("P.stl", StringComparison.OrdinalIgnoreCase))
            {
                siblingVirtual = ToAppRelativePath(path.Substring(0, path.Length - 5) + "P2.stl");
            }
            else
            {
                siblingVirtual = ToAppRelativePath(path.Substring(0, path.Length - 4) + "_F.stl");
            }

            return ToPublicUrl(siblingVirtual);
        }

        private bool VirtualFileExists(string virtualPath)
        {
            var appRel = ToAppRelativePath(virtualPath);
            if (string.IsNullOrWhiteSpace(appRel))
                return false;
            try
            {
                var physical = Server.MapPath(appRel);
                return !string.IsNullOrWhiteSpace(physical) && System.IO.File.Exists(physical);
            }
            catch
            {
                return false;
            }
        }

        private static string ToAppRelativePath(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
                return null;
            var t = virtualPath.Trim().Replace('\\', '/');
            while (t.StartsWith("../", StringComparison.Ordinal))
                t = t.Substring(3);
            while (t.StartsWith("./", StringComparison.Ordinal))
                t = t.Substring(2);
            if (t.StartsWith("http", StringComparison.OrdinalIgnoreCase) || t.StartsWith("//", StringComparison.Ordinal))
                return null;
            if (t.StartsWith("~/"))
                return t;
            if (t.StartsWith("/"))
                return "~" + t;
            return "~/" + t.TrimStart('/');
        }

        private static string FormatBlockSize(double? high, double? width)
        {
            if (!high.HasValue && !width.HasValue)
                return "";
            var ci = CultureInfo.GetCultureInfo("es-ES");
            if (high.HasValue && width.HasValue)
                return high.Value.ToString("0.00", ci) + " x " + width.Value.ToString("0.00", ci);
            return (high ?? width).Value.ToString("0.00", ci);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return "";
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return "";
        }

        private string ToPublicUrl(string virtualPath)
        {
            var appRel = ToAppRelativePath(virtualPath);
            if (string.IsNullOrWhiteSpace(appRel))
            {
                if (string.IsNullOrWhiteSpace(virtualPath))
                    return null;
                var raw = virtualPath.Trim();
                if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("//", StringComparison.Ordinal))
                    return raw;
                return null;
            }
            return Url.Content(appRel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PluginCreateJobside(string textLabel, long? linkClientV2, long linBranch)
        {
            var name = (textLabel ?? "").Trim();
            if (string.IsNullOrEmpty(name))
                return Json(new { ok = false, message = Jobside.Val_NameRequired });

            if (linBranch <= 0 || !db.TSql_Branch.Any(b => b.SysObjectID == linBranch))
                return Json(new { ok = false, message = Jobside.Val_BranchRequired });

            if (linkClientV2.HasValue && linkClientV2.Value > 0)
            {
                if (!db.TSql_Client_V2.Any(c => !c.Is_Delete && c.IdObject == linkClientV2.Value))
                    return Json(new { ok = false, message = Jobside.Val_ClientInvalid });
            }
            else
            {
                linkClientV2 = null;
            }

            bool duplicate = db.TSql_Jobside.Any(x => !x.Is_Delete
                && x.LinkClient_V2 == linkClientV2
                && x.TextLabel == name);
            if (duplicate)
                return Json(new { ok = false, message = Jobside.Val_DuplicateNameCreate });

            var model = new TSql_Jobside
            {
                TextLabel = name,
                LinkClient_V2 = linkClientV2,
                LinBranch = linBranch,
                TextContractRef = string.Empty,
                BitBillSameAsLoc = true,
                AddNJobside = null
            };
            IntranetAuditHelper.SetAuditOnCreate(model, User);

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    db.TSql_Jobside.Add(model);
                    db.SaveChanges();
                    model.AddNJobside = JobsideCodeHelper.BuildAddNJobside(
                        model.IdObject,
                        JobsideCodeHelper.GetSpainLocalNow());
                    db.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return Json(new { ok = false, message = ex.Message });
                }
            }

            return Json(new
            {
                ok = true,
                message = string.Format(Jobside.ToastMessage_JobsideCreated, model.TextLabel),
                item = new { id = model.IdObject, code = model.AddNJobside, label = model.TextLabel }
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PluginCreateOffer(long jobsideId, string textLabel, long linkOfferState)
        {
            var name = (textLabel ?? "").Trim();
            if (string.IsNullOrEmpty(name))
                return Json(new { ok = false, message = Jobside.Offers_Val_NameRequired });

            if (linkOfferState <= 0 ||
                !db.TSql_OfferState.Any(s => s.IdObject == linkOfferState && !s.Is_Delete && s.Is_Active))
                return Json(new { ok = false, message = Jobside.Offers_Val_StateInvalid });

            var jobsideRow = db.TSql_Jobside
                .Include("TSql_Branch")
                .Include("TSql_Branch.TSql_Company")
                .FirstOrDefault(j => j.IdObject == jobsideId && !j.Is_Delete);
            if (jobsideRow == null)
                return Json(new { ok = false, message = Jobside.Err_JobsideNotFound });

            if (!jobsideRow.LinkClient_V2.HasValue)
                return Json(new { ok = false, message = Jobside.Offers_Val_JobsideNeedsClient });

            if (jobsideRow.TSql_Branch == null)
                return Json(new { ok = false, message = Jobside.Offers_Val_BranchOrCompanyMissing });

            if (string.IsNullOrWhiteSpace(jobsideRow.AddNJobside))
                return Json(new { ok = false, message = Jobside.Offers_Val_JobsideCodePending });

            var resolvedClientId = jobsideRow.LinkClient_V2.Value;
            var coLetter = jobsideRow.TSql_Branch.TSql_Company != null
                ? jobsideRow.TSql_Branch.TSql_Company.AddLetter
                : null;
            var brLetter = jobsideRow.TSql_Branch.AddLetter;

            TSql_Offers entity;
            using (var tran = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var addOfferNumber = OfferNumberHelper.AllocateNextOfferNumber(
                        db, coLetter, brLetter, jobsideRow.AddNJobside);
                    entity = new TSql_Offers
                    {
                        AddOfferNumber = addOfferNumber,
                        TextLabel = name,
                        LinkJobside = jobsideId,
                        LinkClient_V2 = resolvedClientId,
                        LinkOfferState = linkOfferState
                    };
                    IntranetAuditHelper.SetAuditOnCreate(entity, User);
                    entity.Is_Active = true;
                    db.TSql_Offers.Add(entity);
                    db.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return Json(new { ok = false, message = ex.Message });
                }
            }

            return Json(new
            {
                ok = true,
                message = Jobside.Offers_SaveSuccess,
                item = new
                {
                    id = entity.IdObject,
                    jobsideId = entity.LinkJobside,
                    number = entity.AddOfferNumber,
                    label = entity.TextLabel
                }
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult PluginCreateDesign(long offerId, string attLabel)
        {
            var uid = IntranetAuditHelper.ResolveCurrentUserId(User);
            var offer = db.TSql_Offers.AsNoTracking()
                .FirstOrDefault(o => o.IdObject == offerId && !o.Is_Delete);
            if (offer == null)
                return Json(new { ok = false, message = Jobside.Offers_Val_NotFound });

            var label = (attLabel ?? "").Trim();
            if (string.IsNullOrEmpty(label))
                return Json(new { ok = false, message = Jobside.OfferWorkspace_Designs_Val_LabelRequired });
            if (label.Length > 500)
                label = label.Substring(0, 500);

            var now = DateTime.Now;
            var entity = new TSql_Design_V2
            {
                AttLabel = label,
                AttDescription = null,
                AttCenterX = 0d,
                AttCenterY = 0d,
                AttCreated = now,
                AttChange = now,
                AttIsDeleted = false,
                AttThumbnail = null,
                AttActiveCameraType = 0L,
                LinCreatedBy = uid,
                LinModifiedBy = uid,
                SysUpdateNumber = 0L,
                ItIsShared = false,
                ItIsSharedMyGrup = false,
                IsRenderAt60 = false,
                LinkOffers = offerId
            };

            db.TSql_Design_V2.Add(entity);
            db.SaveChanges();

            return Json(new
            {
                ok = true,
                message = Jobside.OfferWorkspace_Designs_SaveSuccess,
                item = new
                {
                    id = entity.SysObjectID,
                    label = entity.AttLabel,
                    offerNumber = offer.AddOfferNumber
                }
            });
        }

        public ActionResult _SaveDwgFiles(string IdDesign, string NameDesign, IEnumerable<ImportBlock> ListMaterialExport)
        {
            try
            {
                string path = Server.MapPath("~/LibraryBlock/");

                if (Directory.Exists(path))
                {

                }
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
                var Name = NameDesign + "_" + Guid.NewGuid().ToString("N");
                var file = path + Name + ".dxf";
                DxfDocument doc = new DxfDocument();
                doc.Save(file);
                DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file);
                // netDxf is only compatible with AutoCad2000 and higher DXF versions
                if (dxfVersion < DxfVersion.AutoCad2000)
                {
                    return null;
                }
                DxfDocument loaded = DxfDocument.Load(file);
                foreach (var iten in ListMaterialExport)
                {
                    SendATK_Element(iten, doc);
                }
                doc.Save(file);
                //var j = BeginDownload(filed);
                var FileToDonload = "~/LibraryBlock/" + Name + ".dxf";
                var NameFile = Name + ".dxf";
                var fileContents = System.IO.File.ReadAllText(Server.MapPath(FileToDonload));
                return Json(new { data = true, ListMaterialExport, IsOk = true, fileContents, NameFile });
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public ActionResult DownloadFile(string filename)
        {
            WebClient webClient = new WebClient();
            byte[] myDataBuffer = webClient.DownloadData(filename);
            // Display the downloaded data.
            string download = Encoding.ASCII.GetString(myDataBuffer);
            Uri uri = new Uri(@"c:\atenco\myfile.dxf");
            webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " + "Windows NT 5.2; .NET CLR 1.0.3705;)");
            String newFile = filename;
            webClient.DownloadFileAsync(uri, @newFile);
            return null;
        }


        private void SendATK_Element(ImportBlock iten, DxfDocument doc)
        {
            string ATK_Panel = "";
            Block block = null;
            if (iten.NameBlock == "ATK_Braket")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Braket.dxf");
                block = new Block("ATK_Braket");
            }
            if (iten.NameBlock == "ATK_Panel27x30")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x30.dxf");
                block = new Block("ATK_Panel27x30");
            }
            if (iten.NameBlock == "ATK_Panel27x90")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x90.dxf");
                block = new Block("ATK_Panel27x90");
            }
            if (iten.NameBlock == "ATK_Panel27x75R")
            {
                ATK_Panel = Server.MapPath("~/LibraryBlock/Atk_60/ATK_Panel27x75R.dxf");
                block = new Block("ATK_Panel27x75R");
            }
            if (iten.NameBlock == "ATK_Union10443020")
            {
                return;

            }

            DxfDocument _AllEntityBlock = DxfDocument.Load(ATK_Panel);
            foreach (netDxf.Entities.Face3D Face3D in _AllEntityBlock.Entities.Faces3D) { netDxf.Entities.Face3D copy = (netDxf.Entities.Face3D)Face3D.Clone(); block.Entities.Add(copy); }
            foreach (netDxf.Entities.Polyline2D Polyline2D in _AllEntityBlock.Entities.Polylines2D) { netDxf.Entities.Polyline2D copyPolyline2D = (netDxf.Entities.Polyline2D)Polyline2D.Clone(); block.Entities.Add(copyPolyline2D); }
            foreach (netDxf.Entities.Circle Circle in _AllEntityBlock.Entities.Circles) { netDxf.Entities.Circle copyCircle = (netDxf.Entities.Circle)Circle.Clone(); block.Entities.Add(copyCircle); }
            foreach (netDxf.Entities.Point Point in _AllEntityBlock.Entities.Points)
            {
                netDxf.Entities.Point copyPoint = (netDxf.Entities.Point)Point.Clone();
                block.Entities.Add(copyPoint);
            }
            iten.z = (Convert.ToDouble(iten.z) * -1).ToString();
            switch (iten.Rotate_X)
            {
                case "270":
                    //iten.x = (Convert.ToDouble(iten.x) * -1).ToString();
                    iten.Rotate_X = "90";
                    break;
                case "90":
                    //iten.x = (Convert.ToDouble(iten.x) * -1).ToString();
                    iten.Rotate_X = "270";
                    break;
                case "0":
                    iten.Rotate_X = "0";
                    break;
                case "180":
                    iten.Rotate_X = "180";
                    break;
            }
            Insert insert = new Insert(block, new Vector3(Convert.ToDouble(iten.x) / 100, Convert.ToDouble(iten.z) / 100, (Convert.ToDouble(iten.y) / 100)));

            iten.Rotate_Z = (Convert.ToDouble(iten.Rotate_Z) * -1).ToString();
            insert.Rotation = Convert.ToDouble(iten.Rotate_X);
            insert.Layer = new Layer("ATK_Panel");
            //insert.Layer.Color.Index = 4;
            doc.Entities.Add(insert);
        }

        /// <summary>
        /// Encofrado ATK-60 para el plugin CAD. Llama a la misma SolveFromIdsJson
        /// que Desing_2/GetWallsAtk-60. Aquí no hay otra lógica de paneles.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ActionName("PluginEncofrarAtk60")]
        public JsonResult PluginEncofrarAtk60(Desing2WallIdsRequest idsRequest)
        {
            try
            {
                var jsonRaw = idsRequest != null ? idsRequest.IdsJson : null;
                var solved = new Atk60WallsRepository(new FormworkJsonCommonRepository())
                    .SolveFromIdsJson(jsonRaw);
                var walls = solved.Walls;
                var modulos = solved.Modulos;
                var elementsForThreeJs = solved.ElementsForThreeJs;
                var result = Json(new
                {
                    Exito = true,
                    System = solved.System,
                    WallsCount = walls != null ? walls.Count : 0,
                    ModulosCount = modulos != null ? modulos.Count : 0,
                    ElementsForThreeJsCount = elementsForThreeJs != null && elementsForThreeJs.Elements != null
                        ? elementsForThreeJs.Elements.Count
                        : 0,
                    ElementsForThreeJs = elementsForThreeJs,
                    Walls = walls
                });
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                return Json(new { Exito = false, Mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Procesa líneas/caras de muro (Desing_2, ZWCAD, AutoCAD, BricsCAD y Revit).
        /// La geometría de muros y esquinas sale solo de LCornerDetector.
        /// </summary>
        /// <param name="seleccion">Datos de las líneas y polilíneas seleccionadas</param>
        /// <returns>Respuesta JSON con el resultado del procesamiento</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ProcesarLineasZwcad(SeleccionLineasDTO seleccion)
        {
            try
            {
                // 🔴 LOG CRÍTICO: Verificar que el endpoint se está ejecutando
                string logInicio = $"🔴🔴🔴 ENDPOINT LLAMADO: ProcesarLineasZwcad - {DateTime.Now:HH:mm:ss} 🔴🔴🔴";
                System.Diagnostics.Debug.WriteLine(logInicio);
                System.Console.WriteLine(logInicio);

                if (seleccion == null || seleccion.Lineas == null || seleccion.Lineas.Count == 0)
                    seleccion = ReadJsonBody<SeleccionLineasDTO>() ?? seleccion;

                // Validar datos recibidos
                if (seleccion == null || seleccion.Lineas == null || seleccion.Lineas.Count == 0)
                {
                    string logError = "❌ ERROR: No se recibieron líneas para procesar";
                    System.Diagnostics.Debug.WriteLine(logError);
                    System.Console.WriteLine(logError);

                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "No se recibieron líneas para procesar",
                        Datos = null
                    });
                }

                // Log de información recibida
                System.Diagnostics.Debug.WriteLine($"=== Procesando líneas desde ZWCAD ===");
                System.Diagnostics.Debug.WriteLine($"Total líneas: {seleccion.TotalLineas}");
                System.Diagnostics.Debug.WriteLine($"Total polilíneas: {seleccion.TotalPolilineas}");
                System.Diagnostics.Debug.WriteLine($"Usuario: {seleccion.Usuario}");
                System.Diagnostics.Debug.WriteLine($"Fecha: {seleccion.FechaSeleccion}");

                // Procesar las líneas (estadísticas básicas)
                var estadisticas = new
                {
                    TotalProcesadas = seleccion.Lineas.Count,
                    Lineas = seleccion.Lineas.Where(l => l.Tipo == "Line").Count(),
                    Polilineas = seleccion.Lineas.Where(l => l.Tipo == "Polyline").Count(),
                    LongitudTotal = seleccion.Lineas.Sum(l => l.Longitud),
                    Layers = seleccion.Lineas.Select(l => l.Layer).Distinct().ToList(),
                    FechaProcesamiento = DateTime.Now
                };

                // Normalizar geometría de entrada para evitar residuos decimales
                // que generan conexiones/paneles espurios en el detector.
                var lineasNormalizadas = NormalizarLineasEntrada(seleccion.Lineas);

                // ⭐ DETECCIÓN DE ESQUINAS TIPO L ⭐
                // US-697 — la altura del muro proviene del formulario del cliente (default 2700)
                var detector = new LCornerDetector();
                var deteccionEsquinas = detector.DetectarEsquinasL(lineasNormalizadas, seleccion.AlturaMuroMm);

                System.Diagnostics.Debug.WriteLine($"=== Detección de Esquinas L ===");
                System.Diagnostics.Debug.WriteLine($"Esquinas detectadas: {deteccionEsquinas.TotalEsquinasDetectadas}");
                System.Diagnostics.Debug.WriteLine($"Puntos a dibujar: {deteccionEsquinas.PuntosADibujar.Count}");

                // Agregar información detallada de cada esquina al log
                for (int i = 0; i < deteccionEsquinas.Esquinas.Count; i++)
                {
                    var esquina = deteccionEsquinas.Esquinas[i];
                    System.Diagnostics.Debug.WriteLine($"  Esquina {i + 1}: Vértice ({esquina.Vertice.X:F2}, {esquina.Vertice.Y:F2}) - Ángulo: {esquina.Angulo:F2}° - Líneas: [{esquina.IndiceLinea1}, {esquina.IndiceLinea2}]");
                }

                try
                {
                    Session["UltimaSeleccionLineas"] = seleccion;
                    Session["ResultadoProcesamiento"] = estadisticas;
                    Session["EsquinasDetectadas"] = deteccionEsquinas;
                }
                catch
                {
                    // Plugin AutoCAD/ZWCAD: POST sin cookie de sesión.
                }

                System.Diagnostics.Debug.WriteLine($"Procesamiento completado: {estadisticas.TotalProcesadas} geometrías");

                var ok = Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = true,
                    Mensaje = $"Se procesaron {estadisticas.TotalProcesadas} geometrías ({estadisticas.Lineas} líneas, {estadisticas.Polilineas} polilíneas). {deteccionEsquinas.Mensaje}",
                    Datos = deteccionEsquinas
                });
                ok.MaxJsonLength = int.MaxValue;
                return ok;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesando líneas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = false,
                    Mensaje = $"Error al procesar líneas: {ex.Message}",
                    Datos = null
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> DetectarEsquinasImagen()
        {
            try
            {
                if (Request.Files.Count == 0)
                    return Json(new ApiResponse<DeteccionEsquinasLDTO> { Exito = false, Mensaje = "No se recibió ninguna imagen" });

                var file = Request.Files[0];
                var imagenBytes = new byte[file.ContentLength];
                file.InputStream.Read(imagenBytes, 0, file.ContentLength);

                var imageService = new ImageAnalysisService();
                var (lineasSimples, espesorMuro, alturaMuro) = await imageService.AnalizarImagenAsync(imagenBytes, file.ContentType);

                System.Diagnostics.Debug.WriteLine($"[ImageAnalysis] {lineasSimples.Count} líneas extraídas, espesor: {espesorMuro}, altura: {alturaMuro}");

                if (lineasSimples == null || lineasSimples.Count == 0)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "No se detectaron líneas de muro en la imagen. Asegúrate de que los muros estén dibujados con trazo claro y sin demasiado ruido.",
                        Datos = null
                    });
                }

                if (espesorMuro == null)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "Falta la cota de espesor. Añade E=0,30 en la línea de cota del espesor (entre las dos líneas del muro) y vuelve a intentarlo.",
                        Datos = null
                    });
                }

                // Modo boceto: preparación suave (cotas/vertices ya vienen ordenados; no fusionar ni filtrar).
                var lineasEje = SketchWallBuilder.PrepararLineasEjeBoceto(lineasSimples);
                if (lineasEje.Count == 0)
                {
                    return Json(new ApiResponse<DeteccionEsquinasLDTO>
                    {
                        Exito = false,
                        Mensaje = "Tras filtrar cotas y ruido no quedaron tramos de muro. Revisa que el boceto tenga solo líneas gruesas de perímetro.",
                        Datos = null
                    });
                }

                // Fase 1: solo perímetro exterior en planta (espesor/altura en pasos posteriores).
                var resultado = SketchWallBuilder.ConstruirBocetoSoloEje(lineasEje);
                resultado.LineasEje = lineasEje;

                SketchWallBuilder.ObtenerBoundsPublico(lineasEje, out _, out _, out double maxX, out double maxY);
                resultado.Mensaje =
                    $"Imagen analizada ({file.FileName}): {lineasEje.Count} tramos, " +
                    $"{maxX / 1000.0:0.##}×{maxY / 1000.0:0.##} m. " +
                    $"E={espesorMuro.Value * 1000:0} mm. " +
                    resultado.Mensaje;

                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = true,
                    Mensaje = resultado.Mensaje,
                    Datos = resultado
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = false,
                    Mensaje = $"Error al analizar imagen: {ex.Message}",
                    Datos = null
                });
            }
        }

        /// <summary>
        /// Geocodifica una dirección vía Nominatim (OpenStreetMap) para el modal de importación de edificios.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> BuscarDireccionOsm(string q)
        {
            try
            {
                var service = new OsmBuildingImportService();
                var datos = await service.SearchAddressAsync(q).ConfigureAwait(false);
                return Json(new ApiResponse<List<OsmGeocodeResultDTO>>
                {
                    Exito = true,
                    Mensaje = datos.Count + " resultado(s)",
                    Datos = datos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<OsmGeocodeResultDTO>>
                {
                    Exito = false,
                    Mensaje = "Error al buscar dirección: " + ex.Message,
                    Datos = null
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lista huellas de edificios (Catastro/OSM) en el bounding box visible del mapa.
        /// Body JSON obligatorio (el model binder MVC a menudo deja el DTO en ceros).
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> BuscarEdificiosOsm()
        {
            try
            {
                var request = ReadJsonBody<OsmBuildingsBboxRequestDTO>();
                if (request == null ||
                    (Math.Abs(request.South) < 1e-12 &&
                     Math.Abs(request.North) < 1e-12 &&
                     Math.Abs(request.West) < 1e-12 &&
                     Math.Abs(request.East) < 1e-12))
                {
                    return Json(new ApiResponse<List<OsmBuildingFootprintDTO>>
                    {
                        Exito = false,
                        Mensaje = "Solicitud inválida: bbox vacío o no recibido.",
                        Datos = null
                    });
                }

                if (!(request.South < request.North && request.West < request.East))
                {
                    return Json(new ApiResponse<List<OsmBuildingFootprintDTO>>
                    {
                        Exito = false,
                        Mensaje = "Solicitud inválida: bbox incorrecto.",
                        Datos = null
                    });
                }

                var service = new OsmBuildingImportService();
                var datos = await service.FetchBuildingsInBboxAsync(
                    request.South, request.West, request.North, request.East).ConfigureAwait(false);

                var ok = Json(new ApiResponse<List<OsmBuildingFootprintDTO>>
                {
                    Exito = true,
                    Mensaje = datos.Count + " edificio(s)",
                    Datos = datos
                });
                ok.MaxJsonLength = int.MaxValue;
                return ok;
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<OsmBuildingFootprintDTO>>
                {
                    Exito = false,
                    Mensaje = "Error al cargar edificios: " + ex.Message,
                    Datos = null
                });
            }
        }

        /// <summary>
        /// Convierte la huella OSM/Catastro seleccionada a líneas de planta (mm).
        /// </summary>
        [HttpPost]
        [Authorize]
        public ActionResult ImportarEdificioOsm()
        {
            try
            {
                var request = ReadJsonBody<OsmBuildingImportRequestDTO>();
                var service = new OsmBuildingImportService();
                var resultado = service.BuildSketchFromFootprint(request);
                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = true,
                    Mensaje = resultado.Mensaje,
                    Datos = resultado
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<DeteccionEsquinasLDTO>
                {
                    Exito = false,
                    Mensaje = "Error al importar edificio: " + ex.Message,
                    Datos = null
                });
            }
        }

        /// <summary>
        /// Referencia catastral + dirección por coordenadas WGS84 (Catastro Consulta_RCCOOR).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ConsultarCatastroPorCoordenadas(double lat, double lng)
        {
            try
            {
                var service = new OsmBuildingImportService();
                var datos = await service.LookupCatastroByCoordsAsync(lat, lng).ConfigureAwait(false);
                return Json(new ApiResponse<CatastroRcLookupResultDTO>
                {
                    Exito = true,
                    Mensaje = datos.CadastralRef,
                    Datos = datos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<CatastroRcLookupResultDTO>
                {
                    Exito = false,
                    Mensaje = "Catastro: " + ex.Message,
                    Datos = null
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fallback cuando el model binder de MVC no rellena el DTO desde application/json.
        /// </summary>
        private T ReadJsonBody<T>() where T : class
        {
            try
            {
                if (Request == null || Request.InputStream == null)
                    return null;
                if (Request.InputStream.CanSeek)
                    Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8, true, 1024, true))
                {
                    var body = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(body))
                        return null;
                    return JsonConvert.DeserializeObject<T>(body);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Valida si un equipo puede ejecutar el plugin según la tabla de autorización de dispositivos.
        /// Espera tabla dbo.TSql_PluginDeviceAuth con columna DeviceId y (opcionalmente) LinAspNetUsert.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ValidarEquipoPlugin(PluginAuthRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = false,
                    Mensaje = "Solicitud inválida: DeviceId requerido.",
                    Datos = new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "SolicitudInvalida",
                        Motivo = "DeviceId requerido",
                        DeviceId = request?.DeviceId
                    }
                });
            }

            try
            {
                var result = ValidarEquipoEnSql(request);
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = true,
                    Mensaje = result.Permitido ? "Equipo autorizado." : "Equipo no autorizado.",
                    Datos = result
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<PluginAuthResultDTO>
                {
                    Exito = false,
                    Mensaje = $"Error validando equipo: {ex.Message}",
                    Datos = new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "Error",
                        Motivo = ex.Message,
                        DeviceId = request.DeviceId
                    }
                });
            }
        }

        private PluginAuthResultDTO ValidarEquipoEnSql(PluginAuthRequestDTO request)
        {
            var cnn = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cnn))
                throw new InvalidOperationException("ConnectionString IdentityConnection no configurada.");

            using (var cn = new SqlConnection(cnn))
            {
                cn.Open();

                if (!ExisteTabla(cn, "dbo", "TSql_PluginDeviceAuth"))
                {
                    return new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "TablaNoExiste",
                        Motivo = "No existe dbo.TSql_PluginDeviceAuth. Ejecuta primero el script SQL.",
                        DeviceId = request.DeviceId
                    };
                }

                var columnas = ObtenerColumnas(cn, "dbo", "TSql_PluginDeviceAuth");
                if (!columnas.Contains("DeviceId"))
                {
                    return new PluginAuthResultDTO
                    {
                        Permitido = false,
                        Estado = "EsquemaInvalido",
                        Motivo = "La tabla de autorización no tiene columna DeviceId.",
                        DeviceId = request.DeviceId
                    };
                }

                string userColumn = null;
                foreach (var c in new[] { "LinAspNetUsert", "AspNetUserId", "UserId" })
                {
                    if (columnas.Contains(c))
                    {
                        userColumn = c;
                        break;
                    }
                }

                var sql = "SELECT TOP 1 * FROM dbo.TSql_PluginDeviceAuth WHERE DeviceId = @DeviceId";
                if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                    sql += $" AND ({userColumn} = @AspNetUserId OR {userColumn} IS NULL OR {userColumn} = '')";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", request.DeviceId);
                    if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                        cmd.Parameters.AddWithValue("@AspNetUserId", request.AspNetUserId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            return new PluginAuthResultDTO
                            {
                                Permitido = false,
                                Estado = "NoRegistrado",
                                Motivo = "Equipo no registrado/autorizado.",
                                DeviceId = request.DeviceId
                            };
                        }

                        bool permitido = true;
                        string estado = "Activo";
                        string motivo = "OK";

                        if (TieneColumna(rd, "Allowed") && rd["Allowed"] != DBNull.Value)
                            permitido = Convert.ToBoolean(rd["Allowed"]);
                        else if (TieneColumna(rd, "IsActive") && rd["IsActive"] != DBNull.Value)
                            permitido = Convert.ToBoolean(rd["IsActive"]);

                        if (TieneColumna(rd, "IsRevoked") && rd["IsRevoked"] != DBNull.Value && Convert.ToBoolean(rd["IsRevoked"]))
                        {
                            permitido = false;
                            estado = "Revocado";
                            motivo = "Equipo revocado por administración.";
                        }

                        if (TieneColumna(rd, "AttIsDeleted") && rd["AttIsDeleted"] != DBNull.Value && Convert.ToBoolean(rd["AttIsDeleted"]))
                        {
                            permitido = false;
                            estado = "Eliminado";
                            motivo = "Registro de equipo marcado como eliminado.";
                        }

                        if (TieneColumna(rd, "Estado") && rd["Estado"] != DBNull.Value)
                        {
                            var estadoRaw = rd["Estado"].ToString();
                            if (!string.IsNullOrWhiteSpace(estadoRaw))
                            {
                                estado = estadoRaw;
                                var s = estadoRaw.Trim().ToLowerInvariant();
                                if (s == "2" || s == "3" || s.Contains("revoc") || s.Contains("bloq") || s.Contains("inactiv"))
                                {
                                    permitido = false;
                                    motivo = "Estado de equipo bloqueado/revocado.";
                                }
                            }
                        }

                        if (!permitido && motivo == "OK")
                            motivo = "Equipo deshabilitado por política.";

                        rd.Close();
                        ActualizarHeartbeat(cn, request, columnas, userColumn);

                        return new PluginAuthResultDTO
                        {
                            Permitido = permitido,
                            Estado = estado,
                            Motivo = motivo,
                            DeviceId = request.DeviceId
                        };
                    }
                }
            }
        }

        private static void ActualizarHeartbeat(SqlConnection cn, PluginAuthRequestDTO request, HashSet<string> columnas, string userColumn)
        {
            var setParts = new List<string>();
            if (columnas.Contains("LastCheckUtc")) setParts.Add("LastCheckUtc = @NowUtc");
            if (columnas.Contains("AttLastModification")) setParts.Add("AttLastModification = @NowUtc");
            if (columnas.Contains("MachineName")) setParts.Add("MachineName = @MachineName");
            if (columnas.Contains("UsuarioWindows")) setParts.Add("UsuarioWindows = @UsuarioWindows");
            if (columnas.Contains("PluginVersion")) setParts.Add("PluginVersion = @PluginVersion");
            if (setParts.Count == 0) return;

            var sql = $"UPDATE dbo.TSql_PluginDeviceAuth SET {string.Join(", ", setParts)} WHERE DeviceId = @DeviceId";
            if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                sql += $" AND ({userColumn} = @AspNetUserId OR {userColumn} IS NULL OR {userColumn} = '')";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@NowUtc", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@DeviceId", request.DeviceId);
                cmd.Parameters.AddWithValue("@MachineName", (object)(request.MachineName ?? string.Empty));
                cmd.Parameters.AddWithValue("@UsuarioWindows", (object)(request.UsuarioWindows ?? string.Empty));
                cmd.Parameters.AddWithValue("@PluginVersion", (object)(request.PluginVersion ?? string.Empty));
                if (!string.IsNullOrWhiteSpace(userColumn) && !string.IsNullOrWhiteSpace(request.AspNetUserId))
                    cmd.Parameters.AddWithValue("@AspNetUserId", request.AspNetUserId);
                cmd.ExecuteNonQuery();
            }
        }

        private static bool ExisteTabla(SqlConnection cn, string schema, string table)
        {
            const string sql = @"
SELECT COUNT(1)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = @Schema
  AND TABLE_NAME = @Table
  AND TABLE_TYPE = 'BASE TABLE'";

            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Schema", schema);
                cmd.Parameters.AddWithValue("@Table", table);
                var n = Convert.ToInt32(cmd.ExecuteScalar());
                return n > 0;
            }
        }

        private static HashSet<string> ObtenerColumnas(SqlConnection cn, string schema, string table)
        {
            const string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @Schema
  AND TABLE_NAME = @Table";

            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Schema", schema);
                cmd.Parameters.AddWithValue("@Table", table);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        cols.Add(rd.GetString(0));
                }
            }
            return cols;
        }

        private static bool TieneColumna(SqlDataReader rd, string columnName)
        {
            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (string.Equals(rd.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private sealed class OffsetLineWork
        {
            public int SourceIndex { get; set; }
            public LineaDTO CenterLine { get; set; }
            public LineaDTO ExteriorLine { get; set; }
            public LineaDTO InteriorLine { get; set; }
        }

        /// <summary>
        /// Usa las líneas detectadas como cara EXTERIOR (cota de referencia)
        /// y genera la cara INTERIOR desplazada E hacia el centro del conjunto.
        /// </summary>
        private static List<LineaDTO> ExpandirLineasCentroACaras(List<LineaDTO> lineasCentro, double espesorMuroMetros)
        {
            var resultado = new List<LineaDTO>();
            if (lineasCentro == null || lineasCentro.Count == 0)
                return resultado;

            // E total hacia adentro desde la cara exterior.
            double espesorMm = espesorMuroMetros * 1000.0;
            var trabajos = new List<OffsetLineWork>();
            var centroide = CalcularCentroide(lineasCentro);

            for (int index = 0; index < lineasCentro.Count; index++)
            {
                var linea = lineasCentro[index];
                double dx = linea.FinX - linea.InicioX;
                double dy = linea.FinY - linea.InicioY;
                double longitud = Math.Sqrt((dx * dx) + (dy * dy));
                if (longitud < 0.001)
                    continue;

                // Normal perpendicular (izquierda del vector dirección)
                double nx = -dy / longitud;
                double ny = dx / longitud;

                // Elegimos la normal que apunta hacia el interior (más cerca del centroide).
                double mx = (linea.InicioX + linea.FinX) / 2.0;
                double my = (linea.InicioY + linea.FinY) / 2.0;
                double c1x = mx + (nx * espesorMm);
                double c1y = my + (ny * espesorMm);
                double c2x = mx - (nx * espesorMm);
                double c2y = my - (ny * espesorMm);
                double d1 = Distancia2d(c1x, c1y, centroide.X, centroide.Y);
                double d2 = Distancia2d(c2x, c2y, centroide.X, centroide.Y);

                double ox = d1 <= d2 ? nx * espesorMm : -nx * espesorMm;
                double oy = d1 <= d2 ? ny * espesorMm : -ny * espesorMm;

                trabajos.Add(new OffsetLineWork
                {
                    SourceIndex = index,
                    CenterLine = linea,
                    ExteriorLine = new LineaDTO
                    {
                        Tipo = "Line",
                        InicioX = linea.InicioX,
                        InicioY = linea.InicioY,
                        InicioZ = linea.InicioZ,
                        FinX = linea.FinX,
                        FinY = linea.FinY,
                        FinZ = linea.FinZ,
                        Layer = "ObjetoDB2d",
                        Color = "8",
                        Longitud = longitud,
                        Vertices = null
                    },
                    InteriorLine = new LineaDTO
                    {
                        Tipo = "Line",
                        InicioX = linea.InicioX + ox,
                        InicioY = linea.InicioY + oy,
                        InicioZ = linea.InicioZ,
                        FinX = linea.FinX + ox,
                        FinY = linea.FinY + oy,
                        FinZ = linea.FinZ,
                        Layer = "ObjetoDB2d",
                        Color = "8",
                        Longitud = longitud,
                        Vertices = null
                    }
                });
            }

            AjustarEncuentrosInteriores(trabajos, lineasCentro.Count);

            foreach (var t in trabajos)
            {
                SnapLinea(t.ExteriorLine);
                SnapLinea(t.InteriorLine);
                t.ExteriorLine.Longitud = Distancia2d(t.ExteriorLine.InicioX, t.ExteriorLine.InicioY, t.ExteriorLine.FinX, t.ExteriorLine.FinY);
                t.InteriorLine.Longitud = Distancia2d(t.InteriorLine.InicioX, t.InteriorLine.InicioY, t.InteriorLine.FinX, t.InteriorLine.FinY);
                resultado.Add(t.ExteriorLine);
                resultado.Add(t.InteriorLine);
            }

            return resultado;
        }

        private static void AjustarEncuentrosInteriores(List<OffsetLineWork> trabajos, int totalLineasCentro)
        {
            if (trabajos == null || trabajos.Count == 0 || totalLineasCentro < 2)
                return;

            const double TOLERANCIA_ENCUENTRO_MM = 200.0;
            var map = new Dictionary<string, OffsetLineWork>();
            foreach (var t in trabajos)
                map[$"{t.SourceIndex}"] = t;

            for (int i = 0; i < totalLineasCentro; i++)
            {
                for (int j = i + 1; j < totalLineasCentro; j++)
                {
                    var ci = map.ContainsKey($"{i}") ? map[$"{i}"].CenterLine : null;
                    var cj = map.ContainsKey($"{j}") ? map[$"{j}"].CenterLine : null;
                    if (ci == null || cj == null)
                        continue;

                    if (!TryGetSharedEndpoint(ci, cj, TOLERANCIA_ENCUENTRO_MM, out bool iEsInicio, out bool jEsInicio))
                        continue;

                    if (!map.TryGetValue($"{i}", out var wi) || !map.TryGetValue($"{j}", out var wj))
                        continue;

                    if (!TryIntersectInfiniteLines(wi.InteriorLine, wj.InteriorLine, out double ix, out double iy))
                        continue;

                    AplicarPuntoExtremo(wi.InteriorLine, iEsInicio, ix, iy);
                    AplicarPuntoExtremo(wj.InteriorLine, jEsInicio, ix, iy);
                }
            }
        }

        private static PuntoDTO CalcularCentroide(List<LineaDTO> lineas)
        {
            double sx = 0;
            double sy = 0;
            int n = 0;

            foreach (var l in lineas)
            {
                sx += l.InicioX + l.FinX;
                sy += l.InicioY + l.FinY;
                n += 2;
            }

            if (n == 0)
                return new PuntoDTO { X = 0, Y = 0, Z = 0 };

            return new PuntoDTO { X = sx / n, Y = sy / n, Z = 0 };
        }

        private static bool TryGetSharedEndpoint(LineaDTO a, LineaDTO b, double tol, out bool aEsInicio, out bool bEsInicio)
        {
            aEsInicio = true;
            bEsInicio = true;
            double best = double.MaxValue;
            bool ok = false;

            double d;

            d = Distancia2d(a.InicioX, a.InicioY, b.InicioX, b.InicioY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = true;
                bEsInicio = true;
                ok = true;
            }

            d = Distancia2d(a.InicioX, a.InicioY, b.FinX, b.FinY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = true;
                bEsInicio = false;
                ok = true;
            }

            d = Distancia2d(a.FinX, a.FinY, b.InicioX, b.InicioY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = false;
                bEsInicio = true;
                ok = true;
            }

            d = Distancia2d(a.FinX, a.FinY, b.FinX, b.FinY);
            if (d <= tol && d < best)
            {
                best = d;
                aEsInicio = false;
                bEsInicio = false;
                ok = true;
            }

            return ok;
        }

        private static bool TryIntersectInfiniteLines(LineaDTO a, LineaDTO b, out double ix, out double iy)
        {
            ix = 0;
            iy = 0;

            double x1 = a.InicioX, y1 = a.InicioY, x2 = a.FinX, y2 = a.FinY;
            double x3 = b.InicioX, y3 = b.InicioY, x4 = b.FinX, y4 = b.FinY;

            double den = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            if (Math.Abs(den) < 1e-9)
                return false;

            ix = (((x1 * y2) - (y1 * x2)) * (x3 - x4) - (x1 - x2) * ((x3 * y4) - (y3 * x4))) / den;
            iy = (((x1 * y2) - (y1 * x2)) * (y3 - y4) - (y1 - y2) * ((x3 * y4) - (y3 * x4))) / den;
            return true;
        }

        private static void AplicarPuntoExtremo(LineaDTO l, bool extremoInicio, double x, double y)
        {
            if (extremoInicio)
            {
                l.InicioX = x;
                l.InicioY = y;
            }
            else
            {
                l.FinX = x;
                l.FinY = y;
            }
        }

        private static double Distancia2d(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        /// <summary>
        /// Normaliza líneas/polilíneas de entrada con snap a milímetro entero.
        /// Se aplica antes del detector para estabilizar encuentros entre extremos.
        /// </summary>
        private static List<LineaDTO> NormalizarLineasEntrada(List<LineaDTO> lineas)
        {
            var resultado = new List<LineaDTO>();
            if (lineas == null || lineas.Count == 0)
                return resultado;

            foreach (var origen in lineas)
            {
                if (origen == null)
                    continue;

                var copia = new LineaDTO
                {
                    Tipo = origen.Tipo,
                    InicioX = Snap(origen.InicioX),
                    InicioY = Snap(origen.InicioY),
                    InicioZ = origen.InicioZ,
                    FinX = Snap(origen.FinX),
                    FinY = Snap(origen.FinY),
                    FinZ = origen.FinZ,
                    Layer = origen.Layer,
                    Color = origen.Color,
                    Vertices = null
                };

                if (origen.Vertices != null && origen.Vertices.Count > 0)
                {
                    copia.Vertices = origen.Vertices
                        .Select(v => new PuntoDTO
                        {
                            X = Snap(v.X),
                            Y = Snap(v.Y),
                            Z = v.Z,
                            TipoPunto = v.TipoPunto,
                            ColorIndex = v.ColorIndex,
                            Forma = v.Forma,
                            Tamano = v.Tamano
                        })
                        .ToList();
                }

                if (copia.Vertices != null && copia.Vertices.Count >= 2)
                {
                    var inicio = copia.Vertices[0];
                    var fin = copia.Vertices[copia.Vertices.Count - 1];
                    copia.InicioX = inicio.X;
                    copia.InicioY = inicio.Y;
                    copia.InicioZ = inicio.Z;
                    copia.FinX = fin.X;
                    copia.FinY = fin.Y;
                    copia.FinZ = fin.Z;
                }

                copia.Longitud = Distancia2d(copia.InicioX, copia.InicioY, copia.FinX, copia.FinY);
                resultado.Add(copia);
            }

            return resultado;
        }

        /// <summary>
        /// Redondea coordenadas para evitar residuos numéricos del cálculo geométrico.
        /// </summary>
        private static void SnapLinea(LineaDTO linea)
        {
            if (linea == null) return;
            linea.InicioX = Snap(linea.InicioX);
            linea.InicioY = Snap(linea.InicioY);
            linea.FinX = Snap(linea.FinX);
            linea.FinY = Snap(linea.FinY);
        }

        private static double Snap(double value)
        {
            // Snap a milímetro entero para eliminar residuos acumulados de intersecciones.
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Agrega al resultado las líneas detectadas en imagen como polilíneas 2D
        /// para que el cliente ZWCAD las dibuje y el usuario visualice exactamente
        /// qué geometría interpretó el analizador.
        /// </summary>
        private static void AgregarLineasDetectadasDesdeImagen(
            DeteccionEsquinasLDTO resultado,
            List<LineaDTO> lineasSimples)
        {
            if (resultado == null || lineasSimples == null || lineasSimples.Count == 0)
                return;

            // Si el flujo común ya generó polilíneas de salida, evitamos duplicar geometría.
            if (resultado.PolilineasADibujar != null && resultado.PolilineasADibujar.Count > 0)
                return;

            if (resultado.PolilineasADibujar == null)
                resultado.PolilineasADibujar = new List<PolilineaDTO>();

            foreach (var linea in lineasSimples)
            {
                var vertices = new List<PuntoDTO>();

                if (linea.Vertices != null && linea.Vertices.Count >= 2)
                {
                    foreach (var v in linea.Vertices)
                    {
                        vertices.Add(new PuntoDTO
                        {
                            X = v.X,
                            Y = v.Y,
                            Z = v.Z
                        });
                    }
                }
                else
                {
                    vertices.Add(new PuntoDTO
                    {
                        X = linea.InicioX,
                        Y = linea.InicioY,
                        Z = linea.InicioZ
                    });
                    vertices.Add(new PuntoDTO
                    {
                        X = linea.FinX,
                        Y = linea.FinY,
                        Z = linea.FinZ
                    });
                }

                if (vertices.Count < 2)
                    continue;

                resultado.PolilineasADibujar.Add(new PolilineaDTO
                {
                    Vertices = vertices,
                    Cerrada = false,
                    Capa = "ObjetoDB2d",
                    ColorIndex = 8,
                    AlturaExtrusion = 0
                });
            }
        }
    }
}