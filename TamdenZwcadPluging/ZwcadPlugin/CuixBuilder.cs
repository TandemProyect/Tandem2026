using System.IO;
using System.Text;

namespace ZwcadPlugin
{
    /// <summary>
    /// Genera Tandem2026.cui — XML plano en el formato real de ZWCAD 2026.
    /// MenuGroups.Load() acepta este XML directamente.
    /// Incluye Ribbon + PopMenu.
    /// </summary>
    public static class CuixBuilder
    {
        public static void Build(string rutaSalida)
        {
            string dir = Path.GetDirectoryName(rutaSalida);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(rutaSalida, Contenido(), new UTF8Encoding(false));
        }

        private static string Contenido() => @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<CustSection xml:lang=""en-US"">
  <MenuGroup DisplayName=""Tandem 2026"" Name=""TANDEM2026"">
    <MacroGroup Name=""TD-Main"">
      <MenuMacro UID=""td_panel"">
        <Macro>
          <Name>Panel Principal</Name>
          <Command>^c^cMVCCONEXION</Command>
          <HelpString>Abre el panel principal de Tandem 2026</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_detect"">
        <Macro>
          <Name>Detectar Muros</Name>
          <Command>^c^cDETECTARMUROS</Command>
          <HelpString>Lee geometria 2D y construye el modelo topologico</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_gen3d"">
        <Macro>
          <Name>Generar 3D</Name>
          <Command>^c^cGENERAR3D</Command>
          <HelpString>Genera los solidos 3D a partir del modelo topologico</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_regen"">
        <Macro>
          <Name>Regenerar 3D</Name>
          <Command>^c^cREGENERAR3D</Command>
          <HelpString>Borra y regenera los solidos 3D</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_cfg"">
        <Macro>
          <Name>Configurar Encofrado</Name>
          <Command>^c^cCONFIGENCOFRADO</Command>
          <HelpString>Selecciona el sistema de encofrado y sus parametros</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_leer"">
        <Macro>
          <Name>Leer Diseno</Name>
          <Command>^c^cLEERDISENOMVC</Command>
          <HelpString>Lee un diseno desde el servidor MVC</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_crear"">
        <Macro>
          <Name>Crear Diseno</Name>
          <Command>^c^cCREARDISENOMVC</Command>
          <HelpString>Crea un diseno nuevo y lo guarda en el servidor MVC</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_save"">
        <Macro>
          <Name>Guardar Diseno</Name>
          <Command>^c^cGUARDARDISENOMVC</Command>
          <HelpString>Guarda el diseno actual en el servidor MVC</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_imagen"">
        <Macro>
          <Name>Analizar Imagen</Name>
          <Command>^c^cTANDEM_ANALIZAR_IMAGEN</Command>
          <HelpString>Analiza una foto de plano dibujado a mano y detecta esquinas L via GPT-4o</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_ayuda"">
        <Macro>
          <Name>Ayuda</Name>
          <Command>^c^cTANDEM</Command>
          <HelpString>Muestra los comandos disponibles de Tandem 2026</HelpString>
        </Macro>
      </MenuMacro>
      <MenuMacro UID=""td_seleccionar"">
        <Macro>
          <Name>Seleccionar Lineas</Name>
          <Command>^c^cTANDEM_SELECCIONAR_LINEAS</Command>
          <HelpString>Permite seleccionar lineas y polilineas en el dibujo</HelpString>
          <LargeImage>img\SelectLines.png</LargeImage>
          <SmallImage>img\SelectLines.png</SmallImage>
        </Macro>
      </MenuMacro>
    </MacroGroup>
    <MenuRoot>
      <PopMenuRoot>
        <PopMenu UID=""td_popmenu"">
          <Name>Tandem 2026</Name>
          <PopMenuItem UID=""td_pmi_panel"">
            <MenuItem><MacroRef MenuMacroID=""td_panel""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem IsSeparator=""true"" UID=""td_sep1""/>
          <PopMenuItem UID=""td_pmi_detect"">
            <MenuItem><MacroRef MenuMacroID=""td_detect""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem UID=""td_pmi_gen3d"">
            <MenuItem><MacroRef MenuMacroID=""td_gen3d""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem UID=""td_pmi_regen"">
            <MenuItem><MacroRef MenuMacroID=""td_regen""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem IsSeparator=""true"" UID=""td_sep2""/>
          <PopMenuItem UID=""td_pmi_cfg"">
            <MenuItem><MacroRef MenuMacroID=""td_cfg""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem IsSeparator=""true"" UID=""td_sep3""/>
          <PopMenuItem UID=""td_pmi_leer"">
            <MenuItem><MacroRef MenuMacroID=""td_leer""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem UID=""td_pmi_crear"">
            <MenuItem><MacroRef MenuMacroID=""td_crear""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem UID=""td_pmi_save"">
            <MenuItem><MacroRef MenuMacroID=""td_save""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem IsSeparator=""true"" UID=""td_sep4""/>
          <PopMenuItem UID=""td_pmi_seleccionar"">
            <MenuItem><MacroRef MenuMacroID=""td_seleccionar""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem UID=""td_pmi_imagen"">
            <MenuItem><MacroRef MenuMacroID=""td_imagen""/></MenuItem>
          </PopMenuItem>
          <PopMenuItem IsSeparator=""true"" UID=""td_sep5""/>
          <PopMenuItem UID=""td_pmi_ayuda"">
            <MenuItem><MacroRef MenuMacroID=""td_ayuda""/></MenuItem>
          </PopMenuItem>
        </PopMenu>
      </PopMenuRoot>
      <RibbonRoot>
        <RibbonTabSourceCollection>
          <RibbonTabSource Text=""Tandem 2026"" UID=""td_ribbon_tab"">
            <Name>Tandem</Name>
            <RibbonPanelSourceReference PanelId=""td_ribbon_panel1""/>
            <RibbonPanelSourceReference PanelId=""td_ribbon_panel2""/>
            <RibbonPanelSourceReference PanelId=""td_ribbon_panel3""/>
            <RibbonPanelSourceReference PanelId=""td_ribbon_panel4""/>
          </RibbonTabSource>
        </RibbonTabSourceCollection>
        <RibbonPanelSourceCollection>
          <RibbonPanelSource Text=""Principal"" UID=""td_ribbon_panel1"">
            <Name>Principal</Name>
            <RibbonRow>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_panel"" Text=""Panel""/>
            </RibbonRow>
          </RibbonPanelSource>
          <RibbonPanelSource Text=""Modelo"" UID=""td_ribbon_panel2"">
            <Name>Modelo 3D</Name>
            <RibbonRow>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_detect"" Text=""Detectar""/>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_gen3d"" Text=""Generar 3D""/>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_regen"" Text=""Regenerar""/>
            </RibbonRow>
          </RibbonPanelSource>
          <RibbonPanelSource Text=""Datos"" UID=""td_ribbon_panel3"">
            <Name>Datos MVC</Name>
            <RibbonRow>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_leer"" Text=""Leer""/>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_crear"" Text=""Crear""/>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_save"" Text=""Guardar""/>
            </RibbonRow>
          </RibbonPanelSource>
          <RibbonPanelSource Text=""Seleccion"" UID=""td_ribbon_panel4"">
            <Name>Herramientas</Name>
            <RibbonRow>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_seleccionar"" Text=""Seleccionar""/>
              <RibbonCommandButton ButtonStyle=""LargeWithText"" MenuMacroID=""td_imagen"" Text=""Analizar Img""/>
            </RibbonRow>
          </RibbonPanelSource>
        </RibbonPanelSourceCollection>
        <RibbonTabSelectors/>
      </RibbonRoot>
      <MouseButtonRoot/>
      <DoubleClickRoot/>
      <DigitizerButtonRoot/>
      <TabletMenuRoot/>
      <QuadRoot><QuadTabs/></QuadRoot>
    </MenuRoot>
  </MenuGroup>
</CustSection>";
    }
}