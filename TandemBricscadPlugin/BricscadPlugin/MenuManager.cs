using System;
using Bricscad.ApplicationServices;
using Bricscad.Ribbon;
using Bricscad.Windows;
using Teigha.Runtime;
using CadApp = Bricscad.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(BricscadPlugin.MenuManager))]
[assembly: CommandClass(typeof(BricscadPlugin.MenuManager))]

namespace BricscadPlugin
{
    /// <summary>
    /// Pestaña ribbon Tandem 2026: al activarla abre las paletas MVC (igual que AutoCAD).
    /// </summary>
    public class MenuManager : IExtensionApplication
    {
        private const string RibbonTabId = "TANDEM2026_TAB";
        private const string MenuCaption = "Tandem 2026";
        private static RibbonTab _tandemTab;
        private static string _lastOtherTabId;
        private static bool _handlingTab;

        public void Initialize()
        {
            try
            {
                BricscadPlugin.UI.Views.PaletteWindow.PrepareNativeLoader();

                if (RibbonServices.RibbonPaletteSet == null)
                    RibbonServices.CreateRibbonPaletteSet();

                if (ComponentManager.Ribbon != null)
                    BuildRibbon();

                CadApp.Idle += OnIdle;
                WriteMessage("\nTandem 2026 cargado. Pulsa la pestaña 'Tandem 2026' para abrir los menús MVC.\n");
            }
            catch (System.Exception ex)
            {
                WriteMessage("\nTandem 2026: error al inicializar — " + ex.Message + "\n");
            }
        }

        public void Terminate()
        {
            try
            {
                CadApp.Idle -= OnIdle;
                PaletteHost.CloseAll();
            }
            catch
            {
            }
        }

        [CommandMethod("TANDEM")]
        public void AbrirPaletasMvc()
        {
            PaletteHost.Toggle();
        }

        [CommandMethod("TANDEM_CARGAR_MENU")]
        public void RecargarMenu()
        {
            try
            {
                if (RibbonServices.RibbonPaletteSet == null)
                    RibbonServices.CreateRibbonPaletteSet();
                BuildRibbon();
                WriteMessage("\nTandem 2026: pestaña lista. Púlsala para mostrar u ocultar las paletas.\n");
            }
            catch (System.Exception ex)
            {
                WriteMessage("\nTandem 2026: no se pudo recargar — " + ex.Message + "\n");
            }
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            if (_handlingTab || _tandemTab == null) return;

            if (!_tandemTab.IsActive)
            {
                RememberActiveTab();
                return;
            }

            _handlingTab = true;
            try
            {
                PaletteHost.Toggle();
                RestorePreviousRibbonTab();
            }
            finally
            {
                _handlingTab = false;
            }
        }

        private static void RememberActiveTab()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;
            foreach (RibbonTab tab in ribbon.Tabs)
            {
                if (tab != null && tab.IsActive && tab.Id != RibbonTabId)
                {
                    _lastOtherTabId = tab.Id;
                    return;
                }
            }
        }

        private static void RestorePreviousRibbonTab()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            if (!string.IsNullOrEmpty(_lastOtherTabId))
            {
                foreach (RibbonTab tab in ribbon.Tabs)
                {
                    if (tab != null && tab.Id == _lastOtherTabId)
                    {
                        tab.IsActive = true;
                        return;
                    }
                }
            }

            foreach (RibbonTab tab in ribbon.Tabs)
            {
                if (tab != null && tab.Id != RibbonTabId)
                {
                    tab.IsActive = true;
                    return;
                }
            }

            _tandemTab.IsActive = false;
        }

        private static void BuildRibbon()
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null) return;

            for (int i = ribbon.Tabs.Count - 1; i >= 0; i--)
            {
                RibbonTab tab = ribbon.Tabs[i];
                if (tab == null) continue;
                if (tab.Id == RibbonTabId ||
                    string.Equals(tab.Title, MenuCaption, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(tab.Title, "Tandem_2026", StringComparison.OrdinalIgnoreCase))
                {
                    ribbon.Tabs.Remove(tab);
                }
            }

            _tandemTab = new RibbonTab
            {
                Title = MenuCaption,
                Id = RibbonTabId,
                Name = "Tandem"
            };
            ribbon.Tabs.Add(_tandemTab);
        }

        private static void WriteMessage(string text)
        {
            Document doc = CadApp.DocumentManager.MdiActiveDocument;
            doc?.Editor.WriteMessage(text);
        }
    }
}
