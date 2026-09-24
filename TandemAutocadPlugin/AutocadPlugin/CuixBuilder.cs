using System.IO;
using System.Text;

namespace AutocadPlugin
{
    /// <summary>
    /// CUI vacío de respaldo. El plugin ya no carga este menú:
    /// Tandem 2026 abre paletas MVC, no un submenu de comandos.
    /// </summary>
    public static class CuixBuilder
    {
        public static void Build(string rutaSalida)
        {
            string dir = Path.GetDirectoryName(rutaSalida);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(rutaSalida, Contenido(), new UTF8Encoding(false));
        }

        private static string Contenido() => @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<CustSection xml:lang=""en-US"">
  <MenuGroup DisplayName=""Tandem 2026"" Name=""TANDEM2026"">
    <MacroGroup Name=""TD-Main"">
      <MenuMacro UID=""td_menus"">
        <Macro>
          <Name>Tandem 2026</Name>
          <Command>^c^cTANDEM</Command>
          <HelpString>Abre las paletas MVC de Tandem 2026</HelpString>
        </Macro>
      </MenuMacro>
    </MacroGroup>
    <MenuRoot>
      <PopMenuRoot/>
      <RibbonRoot>
        <RibbonTabSourceCollection/>
        <RibbonPanelSourceCollection/>
        <RibbonTabSelectors/>
      </RibbonRoot>
    </MenuRoot>
  </MenuGroup>
</CustSection>";
    }
}
