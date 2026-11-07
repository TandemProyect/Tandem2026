using System;
using System.Text.Json;

namespace FreeCadPluging
{
    internal static class FreeCadPaletteHost
    {
        private static FreeCadPaletteWindow _session;
        private static FreeCadPaletteWindow _mode;
        private static FreeCadPaletteWindow _tools;
        private static bool _authenticated;
        private static string _readyUrl;
        private static IntPtr _ownerHandle;

        public static void Show(IntPtr ownerHandle)
        {
            FreeCadPluginEnvironment.EnsureFolders();
            _ownerHandle = ownerHandle;
            var baseUrl = Program.CurrentUrl();
            _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";
            EnsureSession(baseUrl + "DesignToolsAutocad/PluginSession?" + SessionQuery());
        }

        public static void ReopenUi()
        {
            var baseUrl = Program.CurrentUrl();
            if (string.IsNullOrWhiteSpace(_readyUrl))
                _readyUrl = baseUrl + "DesignToolsAutocad/PluginReady";

            if (_session == null)
                EnsureSession(baseUrl + "DesignToolsAutocad/PluginSession?" + SessionQuery());

            if (_authenticated)
            {
                ShowToolPalettes();
                _session?.Hide();
                return;
            }

            _session?.Show();
            if (!string.IsNullOrWhiteSpace(_readyUrl))
                _session?.Navigate(_readyUrl + "?t=" + DateTime.UtcNow.Ticks);
        }

        private static string SessionQuery()
        {
            return "deviceId=" + Uri.EscapeDataString(DeviceId())
                + "&machineName=" + Uri.EscapeDataString(Environment.MachineName ?? "")
                + "&usuarioWindows=" + Uri.EscapeDataString(Environment.UserName ?? "")
                + "&pluginVersion=" + Uri.EscapeDataString("FreeCAD-0.1");
        }

        private static string DeviceId()
        {
            return Environment.MachineName + ":" + Environment.UserName + ":FREECAD";
        }

        private static void EnsureSession(string url)
        {
            if (_session != null)
            {
                _session.Show();
                _session.Activate();
                return;
            }

            _session = CreateWindow("Tandem 2026", url, 540, 700);
            _session.Navigated += OnSessionNavigated;
            _session.Closed += (_, __) => _session = null;
            _session.Show();
        }

        private static FreeCadPaletteWindow CreateWindow(string title, string url, double width, double height)
        {
            var win = new FreeCadPaletteWindow(title, url, width, height);
            win.SetOwnerHandle(_ownerHandle);
            win.MessageReceived += OnPaletteMessage;
            return win;
        }

        private static void OnSessionNavigated(Uri uri)
        {
            if (uri == null)
                return;
            var path = uri.AbsolutePath ?? "";
            if (path.IndexOf("/DesignToolsAutocad/PluginCadAuth", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MarkAuthenticated();
                return;
            }
            if (path.IndexOf("/DesignToolsAutocad/PluginReady", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MarkAuthenticated();
            }
        }

        private static void MarkAuthenticated()
        {
            if (_authenticated)
            {
                // Si ya estaba autenticado, igual forzamos visibilidad de paletas.
                ShowToolPalettes();
                _session?.Hide();
                return;
            }
            _authenticated = true;
            ShowToolPalettes();
            // La vista PluginCadAuth es solo handshake (pantalla blanca intencional).
            // La ocultamos al autenticar para no tapar el trabajo CAD.
            _session?.Hide();
        }

        private static void ShowToolPalettes()
        {
            var baseUrl = Program.CurrentUrl();
            EnsurePalette(ref _mode, "mode", "Tandem Modo", baseUrl + "DesignToolsAutocad/PaletteMode", 492, 86);
            EnsurePalette(ref _tools, "tools", "Tandem Herramientas", baseUrl + "DesignToolsAutocad/PaletteTools", 508, 86);
        }

        private static void EnsurePalette(ref FreeCadPaletteWindow window, string id, string title, string url, double width, double height)
        {
            if (window != null)
            {
                window.Show();
                window.Activate();
                return;
            }
            var created = CreateWindow(title, url, width, height);
            created.Closed += (_, __) =>
            {
                if (string.Equals(id, "mode", StringComparison.OrdinalIgnoreCase) && ReferenceEquals(_mode, created))
                    _mode = null;
                if (string.Equals(id, "tools", StringComparison.OrdinalIgnoreCase) && ReferenceEquals(_tools, created))
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
                    _session?.Navigate(_readyUrl + "?t=" + DateTime.UtcNow.Ticks);
                return;
            }

            if (string.Equals(action, "session-ready", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "page-ready", StringComparison.OrdinalIgnoreCase))
            {
                MarkAuthenticated();
                return;
            }

            if (string.Equals(action, "hide-home", StringComparison.OrdinalIgnoreCase))
            {
                _session?.Hide();
                return;
            }

            FreeCadCommandBridge.Queue(MapAction(action), json);
        }

        internal static string ParseActionFromPaletteJson(string json)
        {
            return ReadAction(json);
        }

        private static string MapAction(string action)
        {
            if (string.Equals(action, "wall-2d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "muro-2d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "TANDEM_MURO2D", StringComparison.OrdinalIgnoreCase))
                return "wall-2d";
            if (string.Equals(action, "wall-3d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "muro-3d", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "TANDEM_MURO3D", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "GENERAR3D", StringComparison.OrdinalIgnoreCase))
                return "wall-3d";
            if (string.Equals(action, "formwork", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "encofrar", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "TANDEM_ENCOFRAR", StringComparison.OrdinalIgnoreCase))
                return "formwork";
            return action;
        }

        private static string NormalizePaletteJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
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

                    // Prioriza tool sobre mode para cubrir payloads como:
                    // { "tool": "wall-2d", "mode": null }
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
    }
}