using System;
using System.IO;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Microsoft.Win32;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    /// <summary>
    /// Deja el plugin cargado al arrancar AutoCAD (sin NETLOAD).
    /// UnAtdesing quita el registro, la pestaña y la copia local.
    /// </summary>
    internal static class PluginAutoload
    {
        public const string AppName = "AtDesing";

        public static string InstallDir()
        {
            return Path.Combine(Atk60LibrarySync.ProductRoot(), "Plugin");
        }

        public static string InstalledDll()
        {
            return Path.Combine(InstallDir(), "AutocadPlugin.dll");
        }

        public static string Install()
        {
            var log = new StringBuilder();
            try
            {
                var srcDir = Path.GetDirectoryName(typeof(PluginAutoload).Assembly.Location);
                var destDir = InstallDir();
                if (string.IsNullOrWhiteSpace(srcDir) || !Directory.Exists(srcDir))
                    return "No se encontró la carpeta del plugin.";

                Directory.CreateDirectory(destDir);
                if (!PathsEqual(srcDir, destDir))
                    CopyPluginFiles(srcDir, destDir, log);

                var dll = InstalledDll();
                if (!File.Exists(dll))
                    dll = Path.Combine(srcDir, "AutocadPlugin.dll");
                if (!File.Exists(dll))
                    return "No está AutocadPlugin.dll para registrar.";

                WriteRegistry(dll);
                TrustFolder(destDir);
                File.WriteAllText(Path.Combine(destDir, "installed.txt"), DateTime.UtcNow.ToString("o"));
                log.AppendLine("AutoCAD cargará TDesing al arrancar (sin NETLOAD).");
            }
            catch (Exception ex)
            {
                log.AppendLine("No se pudo dejar el plugin instalado: " + ex.Message);
            }
            return log.ToString().Trim();
        }

        /// <summary>
        /// Quita el arranque automático. No toca la pestaña ni AppData.
        /// Mientras tanto se carga con NETLOAD del último DebugN.
        /// </summary>
        public static void Disable()
        {
            try { DeleteRegistry(); } catch { }
            try { UntrustFolder(InstallDir()); } catch { }
        }

        public static void Uninstall()
        {
            Disable();
            try { MenuManager.RemoveTandemTab(); } catch { }
            try
            {
                var dest = InstallDir();
                if (Directory.Exists(dest) && !PathsEqual(dest, Path.GetDirectoryName(typeof(PluginAutoload).Assembly.Location)))
                {
                    foreach (var file in Directory.GetFiles(dest, "*", SearchOption.AllDirectories))
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
                }
            }
            catch
            {
            }
        }

        private static void CopyPluginFiles(string srcDir, string destDir, StringBuilder log)
        {
            foreach (var src in Directory.GetFiles(srcDir, "*.dll"))
            {
                var name = Path.GetFileName(src);
                var dest = Path.Combine(destDir, name);
                try
                {
                    File.Copy(src, dest, overwrite: true);
                }
                catch (IOException)
                {
                    log.AppendLine("En uso: " + name);
                }
            }
            var loader = Path.Combine(srcDir, "WebView2Loader.dll");
            if (File.Exists(loader))
            {
                try { File.Copy(loader, Path.Combine(destDir, "WebView2Loader.dll"), overwrite: true); }
                catch { }
            }
        }

        private static void WriteRegistry(string dllPath)
        {
            var product = HostApplicationServices.Current.UserRegistryProductRootKey;
            if (string.IsNullOrWhiteSpace(product))
                throw new InvalidOperationException("No hay clave de producto AutoCAD.");

            using (var key = Registry.CurrentUser.CreateSubKey(product + @"\Applications\" + AppName))
            {
                if (key == null)
                    throw new InvalidOperationException("No se pudo crear Applications\\" + AppName);
                key.SetValue("DESCRIPTION", "TDesing / AtDesing", RegistryValueKind.String);
                key.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
                key.SetValue("LOADER", dllPath, RegistryValueKind.String);
                key.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            }

            using (var cmds = Registry.CurrentUser.CreateSubKey(product + @"\Applications\" + AppName + @"\Commands"))
            {
                if (cmds == null) return;
                cmds.SetValue("TANDEM", "TANDEM", RegistryValueKind.String);
                cmds.SetValue("MVCCONEXION", "MVCCONEXION", RegistryValueKind.String);
                cmds.SetValue("INSERTARBLOQUE", "INSERTARBLOQUE", RegistryValueKind.String);
                cmds.SetValue("UnAtdesing", "UnAtdesing", RegistryValueKind.String);
            }
        }

        private static void DeleteRegistry()
        {
            var product = HostApplicationServices.Current.UserRegistryProductRootKey;
            if (string.IsNullOrWhiteSpace(product))
                return;
            using (var apps = Registry.CurrentUser.OpenSubKey(product + @"\Applications", writable: true))
            {
                try { apps?.DeleteSubKeyTree(AppName, throwOnMissingSubKey: false); }
                catch
                {
                    try { apps?.DeleteSubKeyTree(AppName); } catch { }
                }
            }
        }

        private static void TrustFolder(string folder)
        {
            try
            {
                var raw = Convert.ToString(AcadApp.GetSystemVariable("TRUSTEDPATHS")) ?? "";
                if (ContainsPath(raw, folder))
                    return;
                var next = string.IsNullOrWhiteSpace(raw) ? folder : (raw.TrimEnd(';') + ";" + folder);
                AcadApp.SetSystemVariable("TRUSTEDPATHS", next);
            }
            catch
            {
            }
        }

        private static void UntrustFolder(string folder)
        {
            try
            {
                var raw = Convert.ToString(AcadApp.GetSystemVariable("TRUSTEDPATHS")) ?? "";
                if (string.IsNullOrWhiteSpace(raw) || !ContainsPath(raw, folder))
                    return;
                var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var keep = new StringBuilder();
                foreach (var part in parts)
                {
                    if (PathsEqual(part.Trim(), folder))
                        continue;
                    if (keep.Length > 0) keep.Append(';');
                    keep.Append(part.Trim());
                }
                AcadApp.SetSystemVariable("TRUSTEDPATHS", keep.ToString());
            }
            catch
            {
            }
        }

        private static bool ContainsPath(string list, string folder)
        {
            if (string.IsNullOrWhiteSpace(list) || string.IsNullOrWhiteSpace(folder))
                return false;
            foreach (var part in list.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (PathsEqual(part.Trim(), folder))
                    return true;
            }
            return false;
        }

        private static bool PathsEqual(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                return false;
            try
            {
                return string.Equals(
                    Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
