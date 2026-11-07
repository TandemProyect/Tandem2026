using System;
using System.Text.Json;

namespace OpenCadStudioPluging
{
    internal static class OpenCadPaletteHost
    {
        private static OpenCadPaletteWindow _session;
        private static OpenCadPaletteWindow _mode;
        private static OpenCadPaletteWindow _tools;
        private static bool _authenticated;
        private static string _readyUrl;

        public static void Show()
        {
            OpenCadStudioPluginEnvironment.EnsureFolders();
            var baseUrl = MvcServerSettings.CurrentUrl();
            _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";
            var sessionUrl = baseUrl + "DesignToolsAutocad/PluginSession?" + SessionQuery();

            if (_authenticated || OpenCadSessionStore.HasRecentForCurrentBoot())
            {
                _authenticated = true;
                EnsureSession(sessionUrl);
                ShowToolPalettes();
                _session?.Hide();
                if (!string.IsNullOrWhiteSpace(_readyUrl))
                    _session?.Navigate(CacheBust(_readyUrl));
                return;
            }

            EnsureSession(sessionUrl);
            _session?.Show();
        }

        public static void ReopenUi()
        {
            Show();
        }

        public static void CloseAll()
        {
            Close(ref _session);
            Close(ref _mode);
            Close(ref _tools);
            _authenticated = false;
        }

        private static string SessionQuery()
        {
            return "deviceId=" + Uri.EscapeDataString(DeviceId())
                + "&machineName=" + Uri.EscapeDataString(Environment.MachineName ?? "")
                + "&usuarioWindows=" + Uri.EscapeDataString(Environment.UserName ?? "")
                + "&pluginVersion=" + Uri.EscapeDataString("OpenCADStudio-0.1");
        }

        private static string DeviceId()
        {
            return Environment.MachineName + ":" + Environment.UserName + ":OPENCAD";
        }

        private static void EnsureSession(string url)
        {
            if (_session != null)
            {
                _session.EnsureVisible();
                return;
            }

            _session = CreateWindow("Tandem 2026", url, 540, 700, false);
            _session.Navigated += OnSessionNavigated;
            _session.Closed += (_, __) => _session = null;
            _session.Show();
        }

        private static OpenCadPaletteWindow CreateWindow(string title, string url, double width, double height, bool keepOnTop)
        {
            var win = new OpenCadPaletteWindow(title, url, width, height, keepOnTop);
            win.MessageReceived += OnPaletteMessage;
            return win;
        }

        private static void OnSessionNavigated(Uri uri)
        {
            if (uri == null)
                return;

            var path = uri.AbsolutePath ?? "";
            if (path.IndexOf("/Account/Login", StringComparison.OrdinalIgnoreCase) >= 0
                || string.Equals(path, "/", StringComparison.Ordinal))
            {
                _authenticated = false;
                OpenCadSessionStore.Clear();
                Close(ref _mode);
                Close(ref _tools);
                _session?.Show();
                return;
            }

            if (path.IndexOf("/DesignToolsAutocad/PluginCadAuth", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MarkAuthenticated();
            }
        }

        private static void MarkAuthenticated()
        {
            _authenticated = true;
            OpenCadSessionStore.RememberCurrentBoot();
            ShowToolPalettes();
            _session?.Hide();
        }

        private static void ShowToolPalettes()
        {
            var baseUrl = MvcServerSettings.CurrentUrl();
            EnsurePalette(ref _mode, "Tandem Modo", baseUrl + "DesignToolsAutocad/PaletteMode", 492, 86);
            EnsurePalette(ref _tools, "Tandem Herramientas", baseUrl + "DesignToolsAutocad/PaletteTools", 508, 86);
        }

        private static void EnsurePalette(ref OpenCadPaletteWindow window, string title, string url, double width, double height)
        {
            if (window != null)
            {
                window.EnsureVisible();
                return;
            }

            var created = CreateWindow(title, url, width, height, true);
            created.Closed += (_, __) =>
            {
                if (ReferenceEquals(_mode, created))
                    _mode = null;
                if (ReferenceEquals(_tools, created))
                    _tools = null;
            };
            window = created;
            window.Show();
        }

        private static void OnPaletteMessage(string raw)
        {
            var json = NormalizePaletteJson(raw);
            var action = ParseActionFromPaletteJson(json);
            if (string.IsNullOrWhiteSpace(action))
                return;

            if (string.Equals(action, "show-home", StringComparison.OrdinalIgnoreCase))
            {
                _session?.Show();
                if (!string.IsNullOrWhiteSpace(_readyUrl))
                    _session?.Navigate(CacheBust(_readyUrl));
                return;
            }

            if (string.Equals(action, "hide-home", StringComparison.OrdinalIgnoreCase))
            {
                _session?.Hide();
                return;
            }

            if (string.Equals(action, "session-ready", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "page-ready", StringComparison.OrdinalIgnoreCase))
            {
                MarkAuthenticated();
                return;
            }

            var bridgeArgs = BridgeActionMap.MapToBridgeArguments(action);
            if (bridgeArgs == null)
                return;

            try
            {
                BridgeCommandRunner.Run(bridgeArgs);
            }
            catch (Exception ex)
            {
                var detail = "Error ejecutando accion de paleta '" + action + "': " + ex.Message;
                Console.Error.WriteLine(detail);
                try
                {
                    System.Windows.MessageBox.Show(
                        detail,
                        "Tandem 2026 OpenCADStudio",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }
                catch
                {
                }
            }
        }

        internal static string ParseActionFromPaletteJson(string json)
        {
            return ReadAction(json);
        }

        private static string CacheBust(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;
            var sep = url.IndexOf('?') >= 0 ? "&" : "?";
            return url + sep + "t=" + DateTime.UtcNow.Ticks;
        }

        private static string NormalizePaletteJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return raw;
            var text = raw.Trim();
            if (text.Length >= 2 && text[0] == '"')
            {
                try
                {
                    var unquoted = JsonSerializer.Deserialize<string>(text);
                    if (!string.IsNullOrWhiteSpace(unquoted))
                        text = unquoted.Trim();
                }
                catch
                {
                }
            }

            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                return text.Substring(start, end - start + 1);
            return text;
        }

        private static string ReadAction(string json)
        {
            try
            {
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    var action = ReadString(root, "action");
                    if (!string.IsNullOrWhiteSpace(action))
                        return action;

                    var command = ReadString(root, "command");
                    if (!string.IsNullOrWhiteSpace(command))
                        return command;

                    var tool = ReadString(root, "tool");
                    if (!string.IsNullOrWhiteSpace(tool))
                        return tool;

                    var mode = ReadString(root, "mode");
                    if (!string.IsNullOrWhiteSpace(mode))
                        return mode;

                    var system = ReadString(root, "system");
                    if (!string.IsNullOrWhiteSpace(system))
                        return system;
                }
            }
            catch
            {
            }

            return null;
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value))
                return null;
            if (value.ValueKind == JsonValueKind.String)
                return value.GetString();
            return null;
        }

        private static void Close(ref OpenCadPaletteWindow window)
        {
            if (window == null)
                return;
            try
            {
                window.Close();
            }
            finally
            {
                window = null;
            }
        }
    }
}
