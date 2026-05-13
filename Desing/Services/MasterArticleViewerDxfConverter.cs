using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Desing.Services
{
    /// <summary>
    /// Genera un DXF auxiliar <c>{dwg}.viewer.dxf</c> junto al DWG para el visor three-dxf (sin ACadSharp).
    /// Opcional: ejecutable tipo LibreDWG <c>dwg2dxf</c> vía <c>MasterArticles:DwgToDxfExe</c> y argumentos en <c>MasterArticles:DwgToDxfArgumentsFormat</c>.
    /// </summary>
    public static class MasterArticleViewerDxfConverter
    {
        public const string ViewerDxfSuffix = ".viewer.dxf";

        /// <summary>DXF gemelo para vista previa: mismo directorio y nombre base que el .dwg, extensión .dxf (p. ej. 27104209.dwg → 27104209.dxf).</summary>
        public static string GetSiblingPreviewDxfPhysicalPath(string dwgPhysicalPath)
        {
            return Path.ChangeExtension(dwgPhysicalPath, ".dxf");
        }

        public static string GetViewerDxfPath(string dwgPhysicalPath)
        {
            return dwgPhysicalPath + ViewerDxfSuffix;
        }

        /// <summary>True si ya existe DXF auxiliar no más antiguo que el DWG.</summary>
        public static bool IsViewerDxfFresh(string dwgPhysicalPath)
        {
            var dxf = GetViewerDxfPath(dwgPhysicalPath);
            if (!File.Exists(dxf) || !File.Exists(dwgPhysicalPath))
            {
                return false;
            }

            return File.GetLastWriteTimeUtc(dxf) >= File.GetLastWriteTimeUtc(dwgPhysicalPath);
        }

        /// <summary>True si <c>MasterArticles:DwgToDxfExe</c> apunta a un archivo existente (conversión automática posible).</summary>
        public static bool IsConverterConfigured()
        {
            var exe = (ConfigurationManager.AppSettings["MasterArticles:DwgToDxfExe"] ?? "").Trim();
            return !string.IsNullOrWhiteSpace(exe) && File.Exists(exe);
        }

        /// <summary>Resuelve la ruta física del .dwg: rutas planas <c>~/Files/MasterArticles/blocks/archivo.dwg</c> o legado <c>~/Files/MasterArticles/blocks/{id}/archivo.dwg</c> (este último exige que <paramref name="articleId"/> coincida con la carpeta).</summary>
        public static bool TryMapAppRelativeDwgToPhysical(HttpServerUtilityBase server, long articleId, string virtualPath, out string physicalPath, out string error)
        {
            physicalPath = null;
            error = null;
            if (server == null)
            {
                error = "server_null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                error = "empty_path";
                return false;
            }
            var vp = virtualPath.Trim();
            var appRel = vp.StartsWith("~/", StringComparison.Ordinal) ? vp : "~/" + vp.TrimStart('/');
            if (!appRel.StartsWith("~/Files/MasterArticles/blocks/", StringComparison.OrdinalIgnoreCase))
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var tail = appRel.Substring("~/Files/MasterArticles/blocks/".Length).TrimStart('/').Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(tail) || tail.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var blocksRoot = Path.GetFullPath(server.MapPath("~/Files/MasterArticles/blocks"));
            string full;
            try
            {
                full = Path.GetFullPath(server.MapPath(appRel));
            }
            catch
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            if (!full.StartsWith(blocksRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(full, blocksRoot, StringComparison.OrdinalIgnoreCase))
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var slashIdx = tail.IndexOf('/');
            if (slashIdx >= 0)
            {
                var firstSeg = tail.Substring(0, slashIdx);
                if (!long.TryParse(firstSeg, out var folderArticleId) || folderArticleId != articleId)
                {
                    error = "El adjunto no corresponde a este artículo.";
                    return false;
                }
            }
            if (!full.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                error = "Archivo DWG no encontrado en el servidor.";
                return false;
            }
            physicalPath = full;
            return true;
        }

        /// <summary>Resuelve la ruta física del .stl bajo <c>~/Files/MasterArticles/blocks/</c> (mismas reglas de carpeta que el .dwg).</summary>
        public static bool TryMapAppRelativeStlToPhysical(HttpServerUtilityBase server, long articleId, string virtualPath, out string physicalPath, out string error)
        {
            physicalPath = null;
            error = null;
            if (server == null)
            {
                error = "server_null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                error = "empty_path";
                return false;
            }
            var vp = virtualPath.Trim();
            var appRel = vp.StartsWith("~/", StringComparison.Ordinal) ? vp : "~/" + vp.TrimStart('/');
            if (!appRel.StartsWith("~/Files/MasterArticles/blocks/", StringComparison.OrdinalIgnoreCase))
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var tail = appRel.Substring("~/Files/MasterArticles/blocks/".Length).TrimStart('/').Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(tail) || tail.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var blocksRoot = Path.GetFullPath(server.MapPath("~/Files/MasterArticles/blocks"));
            string full;
            try
            {
                full = Path.GetFullPath(server.MapPath(appRel));
            }
            catch
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            if (!full.StartsWith(blocksRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(full, blocksRoot, StringComparison.OrdinalIgnoreCase))
            {
                error = "Ruta de adjunto no permitida.";
                return false;
            }
            var slashIdx = tail.IndexOf('/');
            if (slashIdx >= 0)
            {
                var firstSeg = tail.Substring(0, slashIdx);
                if (!long.TryParse(firstSeg, out var folderArticleId) || folderArticleId != articleId)
                {
                    error = "El adjunto no corresponde a este artículo.";
                    return false;
                }
            }
            if (!full.EndsWith(".stl", StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                error = "Archivo STL no encontrado en el servidor.";
                return false;
            }
            physicalPath = full;
            return true;
        }

        /// <summary>Texto legible para respuestas HTTP / UI (los códigos internos los usamos solo en logs).</summary>
        public static string HumanizeConversionError(string technicalError)
        {
            if (string.IsNullOrWhiteSpace(technicalError))
            {
                return "No se pudo generar el DXF de vista previa.";
            }
            var t = technicalError.Trim();
            if (t.StartsWith("converter_exe_not_configured", StringComparison.OrdinalIgnoreCase))
            {
                return "No hay conversor DWG→DXF configurado. En Web.config defina la ruta absoluta del ejecutable en MasterArticles:DwgToDxfExe, o bien coloque junto al .dwg el archivo «nombreDelDwg.dwg.viewer.dxf» (exportado desde CAD).";
            }
            if (string.Equals(t, "dwg_missing", StringComparison.OrdinalIgnoreCase))
            {
                return "No se encontró el archivo DWG en el servidor.";
            }
            if (string.Equals(t, "converter_timeout", StringComparison.OrdinalIgnoreCase))
            {
                return "La conversión tardó demasiado (tiempo agotado). Puede subir un .viewer.dxf manual o aumentar MasterArticles:DwgToDxfTimeoutMs en Web.config.";
            }
            if (string.Equals(t, "converter_no_output", StringComparison.OrdinalIgnoreCase))
            {
                return "El conversor terminó pero no generó el DXF. Revise MasterArticles:DwgToDxfArgumentsFormat y que el ejecutable escriba en la ruta temporal «{out}».";
            }
            if (string.Equals(t, "process_start_failed", StringComparison.OrdinalIgnoreCase))
            {
                return "No se pudo iniciar el conversor. Compruebe la ruta en MasterArticles:DwgToDxfExe y los permisos del sitio (identidad del application pool).";
            }
            if (t.StartsWith("converter_exit_", StringComparison.OrdinalIgnoreCase))
            {
                return "El conversor devolvió error. Detalle: " + t;
            }
            return t.Length > 4000 ? t.Substring(0, 4000) + "…" : t;
        }

        /// <summary>Ejecuta conversión si hace falta. <paramref name="error"/> texto corto para diagnóstico.</summary>
        public static bool TryEnsureViewerDxf(string dwgPhysicalPath, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(dwgPhysicalPath) || !File.Exists(dwgPhysicalPath))
            {
                error = "dwg_missing";
                return false;
            }

            if (IsViewerDxfFresh(dwgPhysicalPath))
            {
                return true;
            }

            var exe = (ConfigurationManager.AppSettings["MasterArticles:DwgToDxfExe"] ?? "").Trim();
            if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
            {
                error = "converter_exe_not_configured";
                return false;
            }

            var outDxf = GetViewerDxfPath(dwgPhysicalPath);
            var fmt = (ConfigurationManager.AppSettings["MasterArticles:DwgToDxfArgumentsFormat"] ?? "-o \"{out}\" \"{in}\"").Trim();
            var tmp = outDxf + ".tmp_" + Guid.NewGuid().ToString("N");
            try
            {
                if (File.Exists(tmp))
                {
                    File.Delete(tmp);
                }

                var argsTmp = fmt
                    .Replace("{in}", dwgPhysicalPath)
                    .Replace("{out}", tmp);

                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = argsTmp,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.GetDirectoryName(dwgPhysicalPath) ?? Environment.CurrentDirectory
                };

                var timeout = 120000;
                int.TryParse(ConfigurationManager.AppSettings["MasterArticles:DwgToDxfTimeoutMs"] ?? "120000", out timeout);
                if (timeout < 5000)
                {
                    timeout = 5000;
                }

                using (var p = Process.Start(psi))
                {
                    if (p == null)
                    {
                        error = "process_start_failed";
                        return false;
                    }

                    // Hay que consumir stdout y stderr; si no, el buffer se llena y el proceso puede bloquearse (p. ej. LibreDWG).
                    if (!p.WaitForExit(timeout))
                    {
                        try { p.Kill(); } catch { /* ignore */ }
                        error = "converter_timeout";
                        return false;
                    }

                    var stderr = p.StandardError.ReadToEnd();
                    var stdout = p.StandardOutput.ReadToEnd();
                    if (p.ExitCode != 0)
                    {
                        error = "converter_exit_" + p.ExitCode;
                        if (!string.IsNullOrWhiteSpace(stderr))
                        {
                            error += " | stderr: " + stderr.Trim();
                        }
                        if (!string.IsNullOrWhiteSpace(stdout))
                        {
                            error += " | stdout: " + stdout.Trim();
                        }
                        if (error.Length > 1500)
                        {
                            error = error.Substring(0, 1500) + "…";
                        }
                        return false;
                    }
                }

                if (!File.Exists(tmp))
                {
                    error = "converter_no_output";
                    return false;
                }

                File.Copy(tmp, outDxf, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(tmp))
                    {
                        File.Delete(tmp);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
