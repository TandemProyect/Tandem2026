using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(AutocadPlugin.MenuManager))]
[assembly: CommandClass(typeof(AutocadPlugin.MenuManager))]

namespace AutocadPlugin
{
    /// <summary>
    /// Pestaña ribbon Tandem 2026: al activarla abre las paletas MVC.
    /// No carga CUI ni menú clásico (evitar submenu Panel / Detectar / …).
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
                AutocadPlugin.UI.Views.PaletteWindow.PrepareNativeLoader();
                UnloadLegacyMenus();

                if (ComponentManager.Ribbon != null)
                    BuildRibbon();
                else
                    ComponentManager.ItemInitialized += OnRibbonReady;

                AcadApp.Idle += OnIdle;
                WriteMessage("\nTandem 2026 cargado (" + MvcServerSettings.CurrentLabel() + "). Pestaña 'Tandem 2026' o TANDEM_LOCAL / TANDEM_PRODUCCION.\n");
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
                ComponentManager.ItemInitialized -= OnRibbonReady;
                AcadApp.Idle -= OnIdle;
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
                UnloadLegacyMenus();
                BuildRibbon();
                WriteMessage("\nTandem 2026: pestaña lista. Púlsala para mostrar u ocultar las paletas.\n");
            }
            catch (System.Exception ex)
            {
                WriteMessage("\nTandem 2026: no se pudo recargar — " + ex.Message + "\n");
            }
        }

        private static void OnRibbonReady(object sender, RibbonItemEventArgs e)
        {
            if (ComponentManager.Ribbon == null) return;
            ComponentManager.ItemInitialized -= OnRibbonReady;
            BuildRibbon();
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

        private static void UnloadLegacyMenus()
        {
            try
            {
                dynamic acadApp = AcadApp.AcadApplication;
                if (acadApp == null) return;
                dynamic groups = acadApp.MenuGroups;

                try
                {
                    dynamic tandemGroup = groups.Item("TANDEM2026");
                    if (tandemGroup != null)
                        tandemGroup.Unload();
                }
                catch
                {
                }

                try
                {
                    dynamic menus = groups.Item(0).Menus;
                    dynamic popup = menus.Item(MenuCaption);
                    try { popup.RemoveFromMenuBar(); } catch { }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        private static void WriteMessage(string text)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            doc?.Editor.WriteMessage(text);
        }
    }
}
