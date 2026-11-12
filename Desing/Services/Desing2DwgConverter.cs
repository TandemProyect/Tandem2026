using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Desing.Services
{
    /// <summary>
    /// Desing_2 «Importar DWG»: convierte DWG → DXF ASCII (ACAD2018) con ODA File Converter.
    /// Ruta en <c>Desing2:OdaFileConverterExe</c>: absoluta, <c>~/App_Data/Tools/ODA/ODAFileConverter.exe</c> o relativa al sitio;
    /// vacía ⇒ se busca en <c>%ProgramFiles%\ODA\ODAFileConverter*\ODAFileConverter.exe</c>.
    /// Si no hay ODA pero sí <c>MasterArticles:DwgToDxfExe</c> (p. ej. LibreDWG dwg2dxf), se usa ese conversor.
    /// Descarga oficial (gratuita): https://www.opendesign.com/guestfiles/oda_file_converter
    /// </summary>
    public static class Desing2DwgConverter
    {
        public const string OdaDownloadUrl = "https://www.opendesign.com/guestfiles/oda_file_converter";

        private const string TempFolderName = "Desing2DwgImport";

        /// <summary>Ruta física del sitio (o de la app en pruebas fuera de IIS).</summary>
        private static string AppRoot()
        {
            var root = HostingEnvironment.ApplicationPhysicalPath;
            return string.IsNullOrEmpty(root) ? AppDomain.CurrentDomain.BaseDirectory : root;
        }

        /// <summary>Convierte <c>~/…</c> o una ruta relativa al sitio en ruta física; las absolutas se devuelven tal cual.</summary>
        public static string MapSitePath(string path)
        {
            path = (path ?? "").Trim().Trim('"');
            if (path.Length == 0) return null;
            if (path.StartsWith("~/", StringComparison.Ordinal) || path.StartsWith("~\\", StringComparison.Ordinal))
            {
                var rel = path.Substring(2).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                return Path.GetFullPath(Path.Combine(AppRoot(), rel));
            }
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(AppRoot(), path));
        }

        /// <summary>Ruta del ODAFileConverter.exe configurado o detectado; null si no hay.</summary>
        public static string ResolveOdaExe()
        {
            string tried;
            return ResolveOdaExe(out tried);
        }

        /// <summary>Igual que <see cref="ResolveOdaExe()"/>; <paramref name="tried"/> indica qué ruta se probó (para diagnóstico).</summary>
        public static string ResolveOdaExe(out string tried)
        {
            var configured = (ConfigurationManager.AppSettings["Desing2:OdaFileConverterExe"] ?? "").Trim().Trim('"');
            if (!string.IsNullOrEmpty(configured))
            {
                string mapped;
                try
                {
                    mapped = MapSitePath(configured);
                }
                catch (Exception ex)
                {
                    tried = configured + " (" + ex.Message + ")";
                    return null;
                }
                tried = mapped;
                return File.Exists(mapped) ? mapped : null;
            }
            tried = @"%ProgramFiles%\ODA\ODAFileConverter*\ODAFileConverter.exe";
            var roots = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"C:\Program Files",
            };
            foreach (var root in roots.Where(r => !string.IsNullOrEmpty(r)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var oda = Path.Combine(root, "ODA");
                    if (!Directory.Exists(oda)) continue;
                    var exe = Directory.GetDirectories(oda, "ODAFileConverter*")
                        .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase)
                        .Select(d => Path.Combine(d, "ODAFileConverter.exe"))
                        .FirstOrDefault(File.Exists);
                    if (exe != null) return exe;
                }
                catch (Exception)
                {
                    // sin permisos de lectura: seguir con la siguiente raíz
                }
            }
            return null;
        }

        /// <summary>
        /// Carpeta de trabajo nueva: <c>%TEMP%\Desing2DwgImport\{guid}</c>; si %TEMP% no admite escritura
        /// (hosting compartido), <c>~/App_Data/Temp/Desing2DwgImport/{guid}</c>. El llamante la borra al terminar.
        /// </summary>
        public static string CreateWorkDir()
        {
            var name = Guid.NewGuid().ToString("N");
            try
            {
                var dir = Path.Combine(Path.GetTempPath(), TempFolderName, name);
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, ".probe"), "ok");
                File.Delete(Path.Combine(dir, ".probe"));
                return dir;
            }
            catch (Exception)
            {
                var dir = Path.Combine(AppRoot(), "App_Data", "Temp", TempFolderName, name);
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static int TimeoutMs()
        {
            int timeout;
            if (!int.TryParse(ConfigurationManager.AppSettings["Desing2:OdaFileConverterTimeoutMs"] ?? "", out timeout)) timeout = 120000;
            return Math.Max(5000, timeout);
        }

        /// <summary>
        /// Convierte <paramref name="dwgPath"/> a DXF dentro de <paramref name="workDir"/>.
        /// <paramref name="errorCode"/>: oda_missing | convert_timeout | convert_failed | convert_no_output;
        /// <paramref name="errorDetail"/> lleva exe, código de salida, pista y salida del conversor para diagnosticar en el hosting.
        /// </summary>
        public static bool TryConvert(string dwgPath, string workDir, out string dxfPath, out string errorCode, out string errorDetail)
        {
            dxfPath = null;
            errorCode = null;
            errorDetail = null;
            string tried;
            var oda = ResolveOdaExe(out tried);
            if (oda != null)
            {
                var inDir = Path.Combine(workDir, "in");
                var outDir = Path.Combine(workDir, "out");
                Directory.CreateDirectory(inDir);
                Directory.CreateDirectory(outDir);
                var inFile = Path.Combine(inDir, "plano.dwg");
                if (!string.Equals(Path.GetFullPath(dwgPath), Path.GetFullPath(inFile), StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(dwgPath, inFile, true);
                }
                // ODAFileConverter "carpeta entrada" "carpeta salida" versión tipo recursivo auditoría [filtro]
                var args = "\"" + inDir + "\" \"" + outDir + "\" ACAD2018 DXF 0 1 \"*.DWG\"";
                if (!RunProcess(oda, args, workDir, out errorCode, out errorDetail)) return false;
                var produced = Directory.GetFiles(outDir, "*.dxf").FirstOrDefault();
                if (produced == null)
                {
                    errorCode = "convert_no_output";
                    var err = Directory.GetFiles(outDir, "*.err").FirstOrDefault();
                    errorDetail = Truncate("exe=" + oda + "; sin .dxf de salida" + (err != null ? "; " + File.ReadAllText(err) : "") + (string.IsNullOrEmpty(errorDetail) ? "" : "; " + errorDetail));
                    return false;
                }
                dxfPath = produced;
                return true;
            }

            var fallbackExe = (ConfigurationManager.AppSettings["MasterArticles:DwgToDxfExe"] ?? "").Trim();
            if (!string.IsNullOrEmpty(fallbackExe) && File.Exists(fallbackExe))
            {
                var outFile = Path.Combine(workDir, "plano.dxf");
                var fmt = (ConfigurationManager.AppSettings["MasterArticles:DwgToDxfArgumentsFormat"] ?? "-o \"{out}\" \"{in}\"").Trim();
                var args = fmt.Replace("{in}", dwgPath).Replace("{out}", outFile);
                if (!RunProcess(fallbackExe, args, workDir, out errorCode, out errorDetail)) return false;
                if (!File.Exists(outFile))
                {
                    errorCode = "convert_no_output";
                    errorDetail = Truncate("exe=" + fallbackExe + "; sin .dxf de salida; " + errorDetail);
                    return false;
                }
                dxfPath = outFile;
                return true;
            }

            errorCode = "oda_missing";
            errorDetail = "no existe " + tried + " — " + OdaDownloadUrl;
            return false;
        }

        /// <summary>Pista legible para códigos NTSTATUS típicos al lanzar un .exe nativo en el servidor.</summary>
        private static string ExitCodeHint(int exitCode)
        {
            switch (unchecked((uint)exitCode))
            {
                case 0xC0000135: return "falta una DLL junto al exe (VC++ 2015-2022 x64: vcruntime140_1 / msvcp140_1 / msvcp140_2, o Qt6)";
                case 0xC000007B: return "DLL de arquitectura incorrecta (mezcla 32/64 bits)";
                case 0xC0000142: return "no se pudo iniciar una DLL (proceso sin escritorio / desktop heap del grupo de aplicaciones)";
                case 0xC0000022: return "acceso denegado al ejecutar";
                case 0xC0000005: return "violación de acceso dentro del conversor";
                case 0xC0000409: return "el conversor abortó (stack buffer overrun / excepción no controlada)";
                default: return null;
            }
        }

        private static bool RunProcess(string exe, string args, string workDir, out string errorCode, out string errorDetail)
        {
            errorCode = null;
            errorDetail = null;
            var output = new StringBuilder();
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.GetDirectoryName(exe) ?? workDir,
            };
            try
            {
                using (var p = new Process { StartInfo = psi })
                {
                    // Leer stdout/stderr en segundo plano para que el conversor no se bloquee con el búfer lleno.
                    p.OutputDataReceived += (s, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
                    p.ErrorDataReceived += (s, e) => { if (e.Data != null) lock (output) output.AppendLine(e.Data); };
                    if (!p.Start())
                    {
                        errorCode = "convert_failed";
                        errorDetail = "exe=" + exe + "; el proceso no arrancó";
                        return false;
                    }
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    var timeout = TimeoutMs();
                    if (!p.WaitForExit(timeout))
                    {
                        try { p.Kill(); } catch { /* ya terminó */ }
                        errorCode = "convert_timeout";
                        lock (output) errorDetail = Truncate("exe=" + exe + "; timeout " + timeout + " ms; " + output);
                        return false;
                    }
                    p.WaitForExit();
                    string text;
                    lock (output) text = output.ToString().Trim();
                    if (p.ExitCode != 0)
                    {
                        errorCode = "convert_failed";
                        var hint = ExitCodeHint(p.ExitCode);
                        errorDetail = Truncate("exe=" + exe + "; exit=0x" + p.ExitCode.ToString("X8")
                            + (hint != null ? " (" + hint + ")" : "")
                            + (text.Length > 0 ? "; " + text : ""));
                        return false;
                    }
                    // Salida con código 0: se devuelve por si no aparece el .dxf (convert_no_output).
                    errorDetail = text.Length > 0 ? Truncate(text) : null;
                }
                return true;
            }
            catch (Win32Exception ex)
            {
                errorCode = "convert_failed";
                var hint = ex.NativeErrorCode == 5
                    ? "acceso denegado: el hosting no permite ejecutar el .exe o la cuenta del sitio no tiene permiso de ejecución"
                    : ex.NativeErrorCode == 2 ? "no se encuentra el .exe" : null;
                errorDetail = Truncate("exe=" + exe + "; Win32 " + ex.NativeErrorCode + ": " + ex.Message + (hint != null ? " (" + hint + ")" : ""));
                return false;
            }
            catch (Exception ex)
            {
                errorCode = "convert_failed";
                errorDetail = Truncate("exe=" + exe + "; " + ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }

        private static string Truncate(string s)
        {
            s = (s ?? "").Trim();
            return s.Length > 1500 ? s.Substring(0, 1500) + "…" : s;
        }
    }
}
