using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZwcadPlugin
{
    /// <summary>
    /// Limpieza de desarrollador: deja el PC como si AtDesing no se hubiera usado.
    /// No desinstala ZWCAD ni la DLL del plugin.
    /// </summary>
    internal static class PluginDevReset
    {
        public static IList<string> TargetPaths()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pluginRoaming = Path.Combine(roaming, "Tandem", "AutocadPlugin");
            return new List<string>
            {
                Path.Combine(local, Atk60LibrarySync.ProductFolder, "WebView2"),
                Path.Combine(local, Atk60LibrarySync.ProductFolder),
                Path.Combine(Path.GetTempPath(), "TandemAutocadWebView2"),
                Path.Combine(pluginRoaming, "connect-eta-local.txt"),
                Path.Combine(pluginRoaming, "connect-eta-prod.txt"),
                Path.Combine(pluginRoaming, "plantilla-theme.txt")
            };
        }

        public static string WipeAll()
        {
            var log = new StringBuilder();
            var failed = 0;
            foreach (var path in TargetPaths())
            {
                string error;
                if (TryDelete(path, out error))
                {
                    if (error == "missing")
                        log.AppendLine("  · (ya no estaba) " + path);
                    else
                        log.AppendLine("  · borrado " + path);
                }
                else
                {
                    failed++;
                    log.AppendLine("  · BLOQUEADO " + path + " — " + error);
                }
            }

            if (failed > 0)
            {
                log.AppendLine("Cierra ZWCAD y ejecuta reset-cad-dev-local.ps1 para terminar.");
            }
            return log.ToString();
        }

        private static bool TryDelete(string path, out string error)
        {
            error = null;
            try
            {
                if (Directory.Exists(path))
                {
                    DeleteTree(path);
                    if (Directory.Exists(path))
                    {
                        error = "WebView2 sigue en uso. Cierra ZWCAD y vuelve a UnAtdesing.";
                        return false;
                    }
                    return true;
                }
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                    return true;
                }
                error = "missing";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void DeleteTree(string dir)
        {
            foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {
                }
            }
            try { Directory.Delete(dir, true); }
            catch { }
        }
    }
}

