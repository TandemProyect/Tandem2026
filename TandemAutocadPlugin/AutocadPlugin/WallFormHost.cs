using System;
using System.Globalization;
using System.Windows;
using AutocadPlugin.UI.Views;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutocadPlugin
{
    internal sealed class WallFormResult
    {
        public double LengthM { get; set; }
        public double WidthM { get; set; }
        public double HeightM { get; set; }
        public bool HeightAll { get; set; }
        public bool ConfigAll { get; set; }
        public WallEditAttrs Attrs { get; set; }
    }

    /// <summary>
    /// Abre el formulario de Desing (WallForm) dentro del plugin.
    /// </summary>
    internal static class WallFormHost
    {
        public static WallFormResult Show(double lengthM, double widthM, WallEditAttrs attrs, string heading)
        {
            attrs = attrs ?? new WallEditAttrs();
            WallFormResult result = null;
            var win = new Window
            {
                Title = "Modificar muro",
                Width = 740,
                Height = 680,
                MinWidth = 640,
                MinHeight = 480,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize,
                Content = new WebView2()
            };
            var web = (WebView2)win.Content;

            win.Loaded += async (_, __) =>
            {
                try
                {
                    var env = await CoreWebView2Environment.CreateAsync(null, PaletteWindow.UserDataDir());
                    await web.EnsureCoreWebView2Async(env);
                    web.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    web.CoreWebView2.ServerCertificateErrorDetected += (s, args) =>
                    {
                        args.Action = MvcServerSettings.IsProduction()
                            ? CoreWebView2ServerCertificateErrorAction.Cancel
                            : CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                    };
                    web.CoreWebView2.WebMessageReceived += (s, args) =>
                    {
                        var text = ReadMessage(args);
                        if (string.IsNullOrWhiteSpace(text))
                            return;
                        JObject msg;
                        try { msg = JObject.Parse(text); }
                        catch { return; }
                        var action = (string)msg["action"] ?? "";
                        if (action == "wall-form-ready")
                        {
                            var payload = BuildPayload(lengthM, widthM, attrs, heading);
                            web.CoreWebView2.PostWebMessageAsString(payload);
                            return;
                        }
                        if (action == "wall-form-cancel")
                        {
                            win.Dispatcher.BeginInvoke(new Action(() => win.DialogResult = false));
                            return;
                        }
                        if (action != "wall-form-accept")
                            return;
                        result = Map(msg["values"] as JObject);
                        win.Dispatcher.BeginInvoke(new Action(() => win.DialogResult = result != null));
                    };
                    var url = MvcServerSettings.CurrentUrl().TrimEnd('/') + "/DesignToolsAutocad/WallForm";
                    web.CoreWebView2.NavigationCompleted += (s, args) =>
                    {
                        if (args != null && args.IsSuccess)
                            return;
                        win.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(
                                "No se pudo abrir el formulario de Desing.\nArranca Desing y vuelve a hacer doble clic en el muro.",
                                "Tandem 2026",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            win.Close();
                        }));
                    };
                    web.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "No se pudo abrir el formulario de Desing.\nArranca Desing en IIS Express y entra en la paleta del plugin.\n\n" + ex.Message,
                        "Tandem 2026",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    win.Dispatcher.Invoke(new Action(() => win.Close()));
                }
            };

            var ok = AcadApp.ShowModalWindow(win);
            if (ok != true)
                return null;
            return result;
        }

        private static string ReadMessage(CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                var text = args.TryGetWebMessageAsString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
            catch { }
            try { return args.WebMessageAsJson; }
            catch { return null; }
        }

        private static string BuildPayload(double lengthM, double widthM, WallEditAttrs attrs, string heading)
        {
            return JsonConvert.SerializeObject(new
            {
                action = "wall-form-values",
                lengthM,
                widthM,
                heightM = attrs.HeightM,
                heightAll = true,
                configAll = false,
                bracketIn = attrs.BracketIn,
                bracketOut = attrs.BracketOut,
                stiffIn = attrs.StiffIn,
                stiffOut = attrs.StiffOut,
                panel750 = attrs.Panel750,
                verticalPanels = attrs.VerticalPanels,
                propIn = attrs.PropIn,
                propOut = attrs.PropOut,
                propInInf = attrs.PropInInf,
                propOutInf = attrs.PropOutInf,
                tapeIni = attrs.TapeIni ?? "",
                tapeEnd = attrs.TapeEnd ?? "",
                solapeIni = attrs.SolapeIni,
                solapeEnd = attrs.SolapeEnd,
                heading = heading ?? ""
            });
        }

        private static WallFormResult Map(JObject values)
        {
            if (values == null)
                return null;
            double lengthM;
            double widthM;
            double heightM;
            if (!TryNum(values["lengthM"], out lengthM) || lengthM <= 0)
                return null;
            if (!TryNum(values["widthM"], out widthM) || widthM <= 0)
                return null;
            if (!TryNum(values["heightM"], out heightM) || heightM <= 0)
                return null;
            var attrs = new WallEditAttrs
            {
                HeightM = heightM,
                BracketIn = Flag(values["bracketIn"]),
                BracketOut = Flag(values["bracketOut"]),
                StiffIn = Flag(values["stiffIn"]),
                StiffOut = Flag(values["stiffOut"]),
                Panel750 = Flag(values["panel750"]),
                VerticalPanels = Flag(values["verticalPanels"]),
                PropIn = Flag(values["propIn"]),
                PropOut = Flag(values["propOut"]),
                PropInInf = Flag(values["propInInf"]),
                PropOutInf = Flag(values["propOutInf"]),
                TapeIni = (string)values["tapeIni"] ?? "",
                TapeEnd = (string)values["tapeEnd"] ?? "",
                SolapeIni = Flag(values["solapeIni"]),
                SolapeEnd = Flag(values["solapeEnd"])
            };
            return new WallFormResult
            {
                LengthM = lengthM,
                WidthM = widthM,
                HeightM = heightM,
                HeightAll = Flag(values["heightAll"]),
                ConfigAll = Flag(values["configAll"]),
                Attrs = attrs
            };
        }

        private static bool Flag(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return false;
            if (token.Type == JTokenType.Boolean)
                return (bool)token;
            bool parsed;
            return bool.TryParse(token.ToString(), out parsed) && parsed;
        }

        private static bool TryNum(JToken token, out double value)
        {
            value = 0;
            if (token == null)
                return false;
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                value = (double)token;
                return true;
            }
            return double.TryParse(
                token.ToString().Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }
    }
}
