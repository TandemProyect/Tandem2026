using ClosedXML.Excel;
using DAL;
using Desing.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;

namespace Desing.Controllers
{
    [Authorize]
    public class UiTranslationController : BaseController
    {
        private static readonly string[] ReservedImportHeaders =
        {
            "idobject", "textresourcekey", "textmodule", "textdefault", "is_active"
        };

        /// <summary>
        /// Modulos cuyas claves viven en .resx (clase estatica con propiedades string).
        /// El Export añade una fila por cada propiedad publica string de cada tipo, con
        /// <c>TextModule</c> = el nombre indicado. El Import escribe los valores en
        /// <c>TSql_UiTranslation</c> y el helper <see cref="DbBackedResourceManager"/>
        /// hace que el valor de BD prevalezca sobre el .resx en tiempo de ejecucion.
        /// </summary>
        private static readonly IList<Tuple<string, Type>> ResxBackedModules = new List<Tuple<string, Type>>
        {
            Tuple.Create("Company", typeof(Desing.Resources.Company)),
            Tuple.Create("Employee", typeof(Desing.Resources.Employee)),
            Tuple.Create("Plantilla", typeof(Desing.Resources.Plantilla)),
            Tuple.Create("ClientV2", typeof(Desing.Resources.ClientV2)),
            Tuple.Create("MasterArticles", typeof(Desing.Resources.MasterArticles)),
            Tuple.Create("Branch", typeof(Desing.Resources.Branch)),
            Tuple.Create("Jobside", typeof(Desing.Resources.Jobside)),
            Tuple.Create("DocumentType", typeof(Desing.Resources.DocumentType)),
            Tuple.Create("Extension", typeof(Desing.Resources.Extension)),
            Tuple.Create("Language", typeof(Desing.Resources.Language)),
            Tuple.Create("Country", typeof(Desing.Resources.Country)),
            Tuple.Create("OfferState", typeof(Desing.Resources.OfferState)),
            Tuple.Create("Common", typeof(Desing.Resources.Common))
        };

        public ActionResult Index()
        {
            ViewBag.Title = LanguageUiHelper.GetUiStringWithFallback(
                db,
                Request,
                "UiTranslation.PageTitle",
                "Traducciones UI",
                "UiTranslation");

            ViewBag.HasImportErrorReport =
                Session["UiTranslationErrorXlsx"] is byte[] bx && bx.Length > 0;
            return View();
        }

        /// <summary>
        /// Excel (.xlsx) con claves en filas y una columna por idioma activo (TextCode).
        /// </summary>
        [HttpGet]
        public ActionResult Export()
        {
            var langs = db.TSql_language
                .Where(l => !l.Is_Delete && l.Is_Active)
                .OrderByDescending(l => l.Is_Default)
                .ThenBy(l => l.TextLabel)
                .ToList();

            if (langs.Count == 0)
                return new HttpStatusCodeResult(400, "No hay idiomas activos.");

            var defaultLang = langs.FirstOrDefault(l => l.Is_Default) ?? langs.First();
            var langIds = new HashSet<long>(langs.Select(l => l.IdObject));

            var all = db.TSql_UiTranslation.AsNoTracking()
                .Where(t => !t.Is_Delete && langIds.Contains(t.LinkLanguage))
                .ToList();

            var groups = all
                .GroupBy(t => new { t.TextResourceKey, t.TextModule })
                .OrderBy(g => g.Key.TextModule ?? "")
                .ThenBy(g => g.Key.TextResourceKey)
                .ToList();

            var resxByModule = ResxBackedModules.ToDictionary(
                m => m.Item1,
                m => new ResourceManager("Desing.Resources." + m.Item1, m.Item2.Assembly),
                StringComparer.Ordinal);

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Translations");
                var r = 1;
                var c = 1;
                ws.Cell(r, c++).Value = "IdObject";
                ws.Cell(r, c++).Value = "TextResourceKey";
                ws.Cell(r, c++).Value = "TextModule";
                ws.Cell(r, c++).Value = "TextDefault";
                foreach (var lang in langs)
                    ws.Cell(r, c++).Value = lang.TextCode;
                ws.Cell(r, c++).Value = "Is_Active";

                foreach (var g in groups)
                {
                    r++;
                    c = 1;
                    var byLang = g.ToDictionary(x => x.LinkLanguage, x => x);
                    TSql_UiTranslation defRow;
                    byLang.TryGetValue(defaultLang.IdObject, out defRow);

                    ResourceManager resxFallback = null;
                    if (!string.IsNullOrEmpty(g.Key.TextModule))
                        resxByModule.TryGetValue(g.Key.TextModule, out resxFallback);

                    ws.Cell(r, c++).Value = defRow != null ? defRow.IdObject : (long?)null;
                    ws.Cell(r, c++).Value = g.Key.TextResourceKey;
                    ws.Cell(r, c++).Value = g.Key.TextModule ?? "";

                    var defText = defRow != null && !string.IsNullOrEmpty(defRow.TextValue)
                        ? defRow.TextValue
                        : SafeResxGet(resxFallback, g.Key.TextResourceKey, defaultLang.TextCode) ?? "";
                    ws.Cell(r, c++).Value = defText;

                    foreach (var lang in langs)
                    {
                        TSql_UiTranslation tr;
                        var txt = byLang.TryGetValue(lang.IdObject, out tr) && tr != null
                            ? tr.TextValue
                            : null;
                        if (string.IsNullOrEmpty(txt) && resxFallback != null)
                            txt = SafeResxGet(resxFallback, g.Key.TextResourceKey, lang.TextCode);
                        ws.Cell(r, c++).Value = txt ?? "";
                    }
                    ws.Cell(r, c++).Value = defRow != null && defRow.Is_Active;
                }

                /* === Modulos .resx (Company, Employee, ...): añadir filas faltantes ==== */
                var written = new HashSet<string>(
                    groups.Select(g => ((g.Key.TextModule ?? "") + "||" + g.Key.TextResourceKey)),
                    StringComparer.Ordinal);

                foreach (var mod in ResxBackedModules)
                {
                    var moduleName = mod.Item1;
                    var resxRm = new ResourceManager(
                        "Desing.Resources." + moduleName, mod.Item2.Assembly);

                    var props = mod.Item2.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                          .Where(p => p.PropertyType == typeof(string))
                                          .OrderBy(p => p.Name)
                                          .ToList();

                    foreach (var p in props)
                    {
                        if (written.Contains(moduleName + "||" + p.Name))
                            continue;

                        r++;
                        c = 1;
                        ws.Cell(r, c++).Value = (long?)null;
                        ws.Cell(r, c++).Value = p.Name;
                        ws.Cell(r, c++).Value = moduleName;

                        string defaultText = SafeResxGet(resxRm, p.Name, defaultLang.TextCode);
                        ws.Cell(r, c++).Value = defaultText ?? "";

                        foreach (var lang in langs)
                        {
                            ws.Cell(r, c++).Value = SafeResxGet(resxRm, p.Name, lang.TextCode) ?? "";
                        }
                        ws.Cell(r, c++).Value = true;
                        written.Add(moduleName + "||" + p.Name);
                    }
                }

                ws.SheetView.FreezeRows(1);
                ws.Row(1).Style.Font.Bold = true;
                ws.Columns().AdjustToContents(1, Math.Min(r, 200));

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    var fileName = $"UiTranslations_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                    return File(ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }

        /// <summary>
        /// Importa .xlsx: upsert por (TextResourceKey, LinkLanguage) con auditoría estándar.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase upload)
        {
            if (upload == null || upload.ContentLength == 0)
            {
                TempData["UiTranslationMsg"] = "Selecciona un archivo .xlsx.";
                TempData["UiTranslationMsgType"] = "danger";
                return RedirectToAction("Index");
            }

            if (upload.ContentLength > 20 * 1024 * 1024)
            {
                TempData["UiTranslationMsg"] = "El archivo supera el límite de 20 MB.";
                TempData["UiTranslationMsgType"] = "danger";
                return RedirectToAction("Index");
            }

            var langs = db.TSql_language
                .Where(l => !l.Is_Delete && l.Is_Active)
                .ToList();

            var langByCode = langs
                .GroupBy(l => (l.TextCode ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var errors = new List<UiImportError>();

            int rowsCreated = 0, rowsUpdated = 0, rowsSkippedEmpty = 0;
            var unknownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var stream = upload.InputStream)
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    if (ws == null)
                    {
                        TempData["UiTranslationMsg"] = "El libro no tiene hojas.";
                        TempData["UiTranslationMsgType"] = "danger";
                        return RedirectToAction("Index");
                    }

                    var lastRow = ws.LastRowUsed();
                    var lastCol = ws.LastColumnUsed();
                    if (lastRow == null || lastCol == null)
                    {
                        TempData["UiTranslationMsg"] = "Hoja vacía.";
                        TempData["UiTranslationMsgType"] = "warning";
                        return RedirectToAction("Index");
                    }

                    var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (var col = 1; col <= lastCol.ColumnNumber(); col++)
                    {
                        var h = CellText(ws.Row(1).Cell(col));
                        if (string.IsNullOrEmpty(h))
                            continue;
                        if (!headerMap.ContainsKey(h))
                            headerMap[h] = col;
                    }

                    int colResource;
                    int colModule = 0;
                    int colIsActive = 0;
                    if (!TryGetRequiredColumn(headerMap, "TextResourceKey", out colResource))
                    {
                        TempData["UiTranslationMsg"] = "Falta la columna obligatoria TextResourceKey.";
                        TempData["UiTranslationMsgType"] = "danger";
                        return RedirectToAction("Index");
                    }

                    foreach (var kv in headerMap)
                    {
                        var hn = NormHeader(kv.Key);
                        if (hn == "textmodule")
                            colModule = kv.Value;
                        else if (hn == "is_active")
                            colIsActive = kv.Value;
                    }

                    var langCols = new List<Tuple<TSql_language, int>>();
                    foreach (var kv in headerMap)
                    {
                        var norm = kv.Key.Trim();
                        var keyNorm = NormHeader(norm);
                        if (ReservedImportHeaders.Contains(keyNorm))
                            continue;
                        TSql_language langRow;
                        if (langByCode.TryGetValue(norm, out langRow))
                            langCols.Add(Tuple.Create(langRow, kv.Value));
                        else
                            unknownHeaders.Add(norm);
                    }

                    if (langCols.Count == 0)
                    {
                        TempData["UiTranslationMsg"] =
                            "No hay columnas de idioma reconocibles (TextCode de TSql_Language activo).";
                        TempData["UiTranslationMsgType"] = "danger";
                        return RedirectToAction("Index");
                    }

                    for (var rowIdx = 2; rowIdx <= lastRow.RowNumber(); rowIdx++)
                    {
                        var row = ws.Row(rowIdx);
                        var key = colResource > 0 ? CellText(row.Cell(colResource)) : "";
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            rowsSkippedEmpty++;
                            continue;
                        }

                        key = key.Trim();
                        string module = null;
                        if (colModule > 0)
                        {
                            var m = CellText(row.Cell(colModule));
                            module = string.IsNullOrWhiteSpace(m) ? null : m.Trim();
                        }

                        bool rowActive = true;
                        if (colIsActive > 0)
                            rowActive = ParseBool(CellText(row.Cell(colIsActive)), defaultValue: true);

                        var anyCell = false;
                        var rowHadError = false;
                        foreach (var lc in langCols)
                        {
                            var lang = lc.Item1;
                            var col = lc.Item2;
                            var val = CellText(row.Cell(col));
                            if (string.IsNullOrWhiteSpace(val))
                                continue;
                            anyCell = true;

                            try
                            {
                                UpsertTranslationNoSave(key, module, lang.IdObject, val, rowActive,
                                    ref rowsCreated, ref rowsUpdated);
                            }
                            catch (Exception ex)
                            {
                                rowHadError = true;
                                errors.Add(new UiImportError
                                {
                                    Row = rowIdx,
                                    TextResourceKey = key,
                                    TextCode = lang.TextCode,
                                    Message = ex.Message
                                });
                            }
                        }

                        if (!anyCell)
                            rowsSkippedEmpty++;
                        else if (!rowHadError)
                        {
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                errors.Add(new UiImportError
                                {
                                    Row = rowIdx,
                                    TextResourceKey = key,
                                    TextCode = "",
                                    Message = "SaveChanges: " + ex.Message
                                });
                                ClearChangesForContext();
                            }
                        }
                        else
                            ClearChangesForContext();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["UiTranslationMsg"] = "Error al leer el Excel: " + ex.Message;
                TempData["UiTranslationMsgType"] = "danger";
                return RedirectToAction("Index");
            }

            TempData["UiTranslationMsg"] =
                $"Importación terminada. Nuevas filas: {rowsCreated}, actualizadas: {rowsUpdated}, filas sin texto de idioma: {rowsSkippedEmpty}.";
            if (unknownHeaders.Count > 0)
                TempData["UiTranslationMsg"] +=
                    " Columnas no reconocidas como idioma: " + string.Join(", ", unknownHeaders.OrderBy(x => x)) + ".";
            if (errors.Count > 0)
                TempData["UiTranslationMsg"] += $" Errores por fila: {errors.Count} (descarga el informe).";
            TempData["UiTranslationMsgType"] = errors.Count > 0 ? "warning" : "success";

            if (errors.Count > 0)
                Session["UiTranslationErrorXlsx"] = BuildErrorWorkbook(errors);
            else
                Session.Remove("UiTranslationErrorXlsx");

            /* Tras el import, refrescar la cache de DbBackedResourceManager para
               que las claves de Company.* (y futuros modulos) usen YA los valores
               nuevos sin reiniciar la aplicacion. */
            DbBackedResourceManager.Invalidate();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DownloadLastErrorReport()
        {
            var bytes = Session["UiTranslationErrorXlsx"] as byte[];
            if (bytes == null || bytes.Length == 0)
                return RedirectToAction("Index");

            Session.Remove("UiTranslationErrorXlsx");
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"UiTranslations_import_errors_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }

        private void UpsertTranslationNoSave(string resourceKey, string module, long linkLanguage, string textValue,
            bool isActive, ref int created, ref int updated)
        {
            var existing = db.TSql_UiTranslation.FirstOrDefault(t =>
                t.TextResourceKey == resourceKey
                && t.LinkLanguage == linkLanguage
                && t.TextModule == module);

            if (existing != null)
            {
                if (existing.Is_Delete)
                    existing.Is_Delete = false;
                existing.TextModule = module;
                existing.TextValue = textValue;
                existing.Is_Active = isActive;
                IntranetAuditHelper.SetAuditOnUpdate(existing, User);
                updated++;
            }
            else
            {
                var n = new TSql_UiTranslation
                {
                    TextResourceKey = resourceKey,
                    TextModule = module,
                    LinkLanguage = linkLanguage,
                    TextValue = textValue,
                    Is_Active = isActive,
                    Is_Delete = false
                };
                IntranetAuditHelper.SetAuditOnCreate(n, User);
                db.TSql_UiTranslation.Add(n);
                created++;
            }
        }

        private void ClearChangesForContext()
        {
            foreach (var e in db.ChangeTracker.Entries().ToList())
                e.State = EntityState.Detached;
        }

        private static byte[] BuildErrorWorkbook(List<UiImportError> errors)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Errors");
                ws.Cell(1, 1).Value = "Row";
                ws.Cell(1, 2).Value = "TextResourceKey";
                ws.Cell(1, 3).Value = "TextCode";
                ws.Cell(1, 4).Value = "Message";
                ws.Row(1).Style.Font.Bold = true;
                var r = 2;
                foreach (var e in errors)
                {
                    ws.Cell(r, 1).Value = e.Row;
                    ws.Cell(r, 2).Value = e.TextResourceKey;
                    ws.Cell(r, 3).Value = e.TextCode;
                    ws.Cell(r, 4).Value = e.Message;
                    r++;
                }
                ws.Columns().AdjustToContents(1, Math.Min(r - 1, 500));
                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        private static bool TryGetRequiredColumn(Dictionary<string, int> headerMap, string name, out int col)
        {
            col = 0;
            foreach (var kv in headerMap)
            {
                if (string.Equals(NormHeader(kv.Key), NormHeader(name), StringComparison.OrdinalIgnoreCase))
                {
                    col = kv.Value;
                    return true;
                }
            }
            return false;
        }

        private static string NormHeader(string s)
        {
            return (s ?? "").Trim().Replace(" ", "").ToLowerInvariant();
        }

        private static string CellText(IXLCell cell)
        {
            if (cell == null)
                return "";
            try
            {
                if (cell.IsEmpty())
                    return "";
                return Convert.ToString(cell.Value).Trim();
            }
            catch
            {
                return "";
            }
        }

        private static bool ParseBool(string raw, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return defaultValue;
            raw = raw.Trim().ToLowerInvariant();
            if (raw == "1" || raw == "true" || raw == "yes" || raw == "sí" || raw == "si")
                return true;
            if (raw == "0" || raw == "false" || raw == "no")
                return false;
            return defaultValue;
        }

        private class UiImportError
        {
            public int Row { get; set; }
            public string TextResourceKey { get; set; }
            public string TextCode { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Lee el valor crudo del <c>.resx</c> (sin pasar por el override de BD) para el
        /// <paramref name="textCode"/> indicado. Si el <c>.resx</c> de esa cultura no
        /// existe, cae al neutral. Devuelve <c>null</c> si la clave no existe.
        /// </summary>
        private static string SafeResxGet(ResourceManager rm, string keyName, string textCode)
        {
            if (rm == null || string.IsNullOrWhiteSpace(keyName))
                return null;

            try
            {
                CultureInfo culture;
                try
                {
                    culture = string.IsNullOrWhiteSpace(textCode)
                        ? CultureInfo.InvariantCulture
                        : CultureInfo.GetCultureInfo(textCode.Trim());
                }
                catch (CultureNotFoundException)
                {
                    culture = CultureInfo.InvariantCulture;
                }

                return rm.GetString(keyName, culture);
            }
            catch
            {
                return null;
            }
        }
    }
}
