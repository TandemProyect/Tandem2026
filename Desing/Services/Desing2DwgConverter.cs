using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Desing.Services
{
    /// <summary>
    /// Desing_2 «Importar DWG»: convierte DWG → DXF ASCII (ACAD2018) con ODA File Converter.
    /// Ruta en <c>Desing2:OdaFileConverterExe</c>; vacía ⇒ se busca en <c>%ProgramFiles%\ODA\ODAFileConverter*\ODAFileConverter.exe</c>.
    /// Si no hay ODA pero sí <c>MasterArticles:DwgToDxfExe</c> (p. ej. LibreDWG dwg2dxf), se usa ese conversor.
    /// Descarga oficial (gratuita): https://www.opendesign.com/guestfiles/oda_file_converter
    /// </summary>
    public static class Desing2DwgConverter
    {
        public const string OdaDownloadUrl = "https://www.opendesign.com/guestfiles/oda_file_converter";

        /// <summary>Ruta del ODAFileConverter.exe configurado o detectado; null si no hay.</summary>
        public static string ResolveOdaExe()
        {
            var configured = (ConfigurationManager.AppSettings["Desing2:OdaFileConverterExe"] ?? "").Trim().Trim('"');
            if (!string.IsNullOrEmpty(configured))
            {
                return File.Exists(configured) ? configured : null;
            }
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

        private static int TimeoutMs()
        {
            int timeout;
            if (!int.TryParse(ConfigurationManager.AppSettings["Desing2:OdaFileConverterTimeoutMs"] ?? "", out timeout)) timeout = 120000;
            return Math.Max(5000, timeout);
        }

        /// <summary>
        /// Convierte <paramref name="dwgPath"/> a DXF dentro de <paramref name="workDir"/>.
        /// <paramref name="errorCode"/>: oda_missing | convert_timeout | convert_failed | convert_no_output.
        /// </summary>
        public static bool TryConvert(string dwgPath, string workDir, out string dxfPath, out string errorCode, out string errorDetail)
        {
            dxfPath = null;
            errorCode = null;
            errorDetail = null;
            var oda = ResolveOdaExe();
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
                    if (err != null) errorDetail = Truncate(File.ReadAllText(err));
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
                    return false;
                }
                dxfPath = outFile;
                return true;
            }

            errorCode = "oda_missing";
            errorDetail = OdaDownloadUrl;
            return false;
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
                WorkingDirectory = workDir,
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
                        errorDetail = "process_start_failed";
                        return false;
                    }
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    if (!p.WaitForExit(TimeoutMs()))
                    {
                        try { p.Kill(); } catch { /* ya terminó */ }
                        errorCode = "convert_timeout";
                        return false;
                    }
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        errorCode = "convert_failed";
                        lock (output) errorDetail = Truncate("exit " + p.ExitCode + " " + output);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorCode = "convert_failed";
                errorDetail = Truncate(ex.Message);
                return false;
            }
        }

        private static string Truncate(string s)
        {
            s = (s ?? "").Trim();
            return s.Length > 1000 ? s.Substring(0, 1000) + "…" : s;
        }
    }
}
