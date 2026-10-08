# Resumen agentes — septiembre y octubre 2026

Documento de orientacion para continuar el trabajo hecho por varios agentes durante septiembre y octubre de 2026. No sustituye a los handovers tecnicos; sirve como mapa rapido de que se hizo, donde quedo documentado y que reglas no se deben romper.

Fuentes revisadas:

- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md`
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md`
- Historial local de sesiones Copilot del repo `Tandem2026`, especialmente `5fd86cc9-9547-4659-bddf-0e4590ba3ee5`, `813a819a-e496-4852-90cb-a9be70726de0`, `8af6d1f4-1cda-4cd6-96b5-63fa102e6bc3` y `1659f1ab-bb28-464b-b29d-a84a54d38576`.

## Regla principal

La logica de negocio, geometria, DTOs, catalogos, conexion MVC y reglas ATK-60 debe vivir en `Desing` o en una capa comun C#. Los plugins CAD solo adaptan su host: menus, paletas, seleccion de entidades, conversion de unidades y creacion de objetos nativos.

No duplicar en cada plugin:

- Deteccion de muros/esquinas: `DesignToolsAutocad/ProcesarLineasZwcad` -> `LCornerDetector`.
- Encofrado automatico ATK-60: `Atk60WallsRepository.SolveFromIdsJson` y `DesignToolsAutocad/PluginEncofrarAtk60`.
- Formularios, menus y herramientas: vistas MVC `PluginSession`, `PluginReady`, `PaletteMode`, `PaletteTools`, `PluginBlocks`.
- Catalogos de articulos y biblioteca local: manifiesto/descarga desde MVC, cache en AppData.

## Septiembre 2026

### Base comun CAD: paletas MVC y muros

Se alineo el objetivo de los plugins AutoCAD, BricsCAD y Revit con `Desing_2`:

- Pestaña o comando `TANDEM` abre sesion Desing con WebView2.
- Login MVC si no hay cookie Identity.
- Paleta `PaletteMode`: lineas, muro 2D, muro 3D, encofrar, sistema ATK-60.
- Paleta `PaletteTools`: herramientas de dibujo/edicion.
- `Muro 2D`: eje + dos caras en capas o estilos propios del host.
- `Generar 3D`: recoge caras, manda a `ProcesarLineasZwcad`, extruye polilineas `ModelDesing`.

Referencia: `HANDOVER-2026-09-Plugins-Tandem-CAD.md`.

### Bloquing y visor de articulos

Se construyo la base de la biblioteca de bloques desde MVC:

- `PluginBlocks` muestra catalogo, busqueda y visor STL/Three.js.
- La paleta se abre desde la barra de modo con `show-blocks`.
- Mensajes WebView2 se normalizan con `action` y se procesan en `PaletteHost`.
- Se definio UX de plegado/expandido/cierre para bloquing.

Estado historico: el formulario quedo listo antes del INSERT final. Luego fue superado por la implementacion de octubre, donde el catalogo local vive en `%LocalAppData%\AtDesing`.

Referencia: `HANDOVER-2026-09-29-Autocad-Bloquing.md`.

### Insercion ATK-60 y contratos geometricos

Se establecio el contrato para insertar articulos ATK-60:

- Articulos por codigo y vista (`3D`, `3DRef`, `Xr`).
- Insercion nativa en CAD desde biblioteca local.
- `Intro` repite el ultimo articulo.
- No consultar SQL desde el plugin.
- El plugin debe trabajar con la cache local y endpoints MVC.

Referencia: `HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`.

### Otros trabajos de septiembre detectados en sesiones

- Se arreglo un problema de arranque de Visual Studio relacionado con configuracion local `.vs` y proyectos de biblioteca.
- Se corrigieron DTOs de encofrado que no tenian propiedades como `BaseRotX`.
- Se inicio `TandemIAHelp`: flujo para generar ayuda/narracion a partir de funciones del visor, empezando por el boton de dibujar muro en planta (`wall.create.plan`).
- Se agrego en el visor una lectura de longitud del muro al doble click, ademas de altura.

Estos trabajos aparecen en sesiones locales de septiembre y en memoria del repo. Si se retoman, buscar tambien en `IA/TandemIAHelp` y `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`.

## Octubre 2026

### Sesion CAD, home, biblioteca local y ATDESING

Se estabilizo el flujo de conexion AutoCAD:

```text
NETLOAD ultimo DebugN
  -> MenuManager.Initialize
  -> si hay cad-session reciente, reanuda sin instalar
  -> si no, TANDEM conecta y ATDESING instala biblioteca

TANDEM / pestana
  -> PaletteHost.Show()
  -> PluginSession / login / PluginCadAuth
  -> FinishConnectUi()
  -> ShowToolPalettes()

ATDESING
  -> EnsureFolders()
  -> RunLibraryUpdate()
  -> copia DWG/JSON a %LocalAppData%\AtDesing
```

Decisiones importantes:

- `NETLOAD` y `TANDEM` no instalan biblioteca.
- Solo `ATDESING` crea/copia la biblioteca local.
- Autoload quedo desactivado el 2026-10-03.
- `PluginPing` mide servidor con `dbMs`, no con listados pesados.
- El diagnostico de conexion no debe filtrar origen SQL, catalogo ni connection strings.

Referencias:

- `HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md`
- `HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md`

### Encofrado manual ATK-60

Se avanzo el insertado uno a uno de paneles ATK-60 en AutoCAD y su persistencia:

- Bloquing permite elegir panel y arrastrarlo por el muro.
- Muro con panel manual pasa a `Is_Special = 1`.
- El encofrado automatico omite muros especiales.
- Al guardar, se persisten articulos en `TSql_DesignWallArticle`.
- XData del articulo guarda codigo, vista, rol, rotacion, wall id y punto logico `AT:x,y,z`.
- `AT:` es la coordenada que debe guardarse; no usar `BlockReference.Position` porque queda desplazado por giros DWG.
- Al abrir, se restauran articulos manuales.

Reglas de snap documentadas:

- El camino estable es inferior izquierdo (`V_BL` / `V_TL`).
- Inferior derecho (`V_BR` / `V_TR`) sigue como bug conocido.
- No arreglar `V_BR` tocando el camino de `V_BL`.
- El giro de 180 grados del DWG existe porque el DWG mira al reves que el STL; no quitarlo sin validar huella.

Builds relevantes:

- `Debug116`: inferior izquierdo restaurado; inferior derecho sigue mal.
- `Debug119`: simetrico a 90 grados en la misma estacion.
- `Debug121`: guarda giro visual del bloque (`EncodePoseZ`).
- `Debug122`: build recomendada para reabrir dibujos con articulos manuales.

Referencias:

- `HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md`
- `HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md`

### Visor Desing_2 para articulos manuales

Se ajusto `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js` para pintar articulos manuales:

- Funciones principales: `maStlDesing2RenderManualWallArticles`, `maStlDesing2ReflectManualArticleOutward`, `maStlDesing2FaceOutwardNormal`.
- Panel con pose codificada `5000 + yaw` no se refleja como los demas.
- Los demas paneles se reflejan solo en espesor, no a lo largo del muro.
- No usar STL tumbado `*T.stl` para resolver la pose: se usa STL de pie y se aplica rotacion.
- El ultimo cambio de reflejo solo por espesor quedo pendiente de confirmacion visual con Ctrl+F5.

### FreeCAD

Se inicio el plugin FreeCAD con la misma regla de arquitectura compartida:

- Proyecto C# `FreeCadPluging` agregado a `Design.sln`.
- Workbench Python `Tandem2026` instalado en `%APPDATA%\FreeCAD\v1-1\Mod\Tandem2026`.
- Python queda como cargador/adaptador de FreeCAD; no debe contener negocio ni geometria comun.
- Host C# `FreeCadPluging.exe` gestiona servidor local/produccion, ping, WebView2 y llamadas MVC.
- Build copia Workbench y host a AppData.
- `WebView2Loader.dll` debe copiarse junto al host.

Conversion de objetos FreeCAD:

- Se intento DWG/STEP/SAT, pero AutoCAD/BricsCAD no fueron ruta fiable por export/licencias.
- ZWCAD se tomo como ruta CAD disponible.
- FreeCAD 1.1.4 tiene `ifcopenshell`; IFC fue la via viable.
- Se convirtieron 15 IFC ATK60 a `.FCStd` bajo `Desing/Content/DesignTools/FreeCAD/AtkSystem60/3D`.
- Los `.FCStd` generados reabren, aunque FreeCAD muestra avisos de reparacion de malla.

Flujo FreeCAD implementado/probado:

- Crear `Wall2D` demo.
- Generar `Wall3D` demo.
- Insertar piezas `.FCStd` por codigo.
- Primer puente real de encofrado: construir `IdsJson` como ZWCAD y llamar a `DesignToolsAutocad/PluginEncofrarAtk60` con `formwork-solve`.
- Si `Desing` local no esta levantado, `localhost:44384` da conexion denegada; no es error del plugin.

Arranque MVC en FreeCAD:

- Se agrego `connect-ui` / `prepare` / `atdesing` en el host C#.
- Se agregaron ventanas WPF/WebView2 para cargar `PluginSession`, `PaletteMode` y `PaletteTools`.
- Se agrego puente C# -> FreeCAD mediante peticiones JSON en `%APPDATA%\Tandem\FreecadPlugin\requests`.
- El Workbench `Tandem 2026` debe comportarse como ZWCAD/AutoCAD: al activarse abre el entorno MVC y las herramientas reales vienen de MVC.
- Los menus antiguos FreeCAD (`Servidor local`, `Servidor produccion`, `Mostrar servidor`, `Muro 2D`, `Generar 3D`, `Encofrar`) se empezaron a retirar de la UI; deben quedar detras como comandos internos o puente de desarrollo, no como interfaz principal.

Archivos clave FreeCAD:

- `FreeCadPluging/FreeCadPluging.csproj`
- `FreeCadPluging/Program.cs`
- `FreeCadPluging/FreeCadPaletteHost.cs`
- `FreeCadPluging/FreeCadPaletteWindow.cs`
- `FreeCadPluging/FreeCadCommandBridge.cs`
- `FreeCadPluging/FreeCadPluginEnvironment.cs`
- `FreeCadPluging/Workbench/Tandem2026/InitGui.py`
- `FreeCadPluging/Workbench/Tandem2026/tandem_commands.py`
- `FreeCadPluging/Workbench/Tandem2026/tandem_command_bridge.py`
- `FreeCadPluging/Workbench/Tandem2026/tandem_freecad_tools.py`
- `FreeCadPluging/Tools/convert_atk60_ifc_to_fcstd.py`

## Estado actual por host

| Host | Estado resumido | Retomar por |
|------|-----------------|-------------|
| AutoCAD | Flujo mas avanzado: paletas, sesion, ATDESING, bloquing, insert manual, guardado, restauracion. | `HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md` |
| ZWCAD | Referencia funcional para comandos CAD y llamadas MVC; usar solo ZWCAD como CAD disponible para exportaciones. | `TamdenZwcadPluging/ZwcadPlugin` |
| BricsCAD | Copia funcional de AutoCAD en paletas/muros, pero no es la ruta actual por licencia. | `TandemBricscadPlugin/BricscadPlugin` |
| Revit | Add-in e instalador per-user, muro 2D/3D via API Revit. | `TandemRevitPlugin/README.md` |
| FreeCAD | Base creada; Workbench + host C# + paletas MVC iniciales + objetos FCStd. Aun no esta al nivel AutoCAD/ZWCAD. | `FreeCadPluging/README.md` y este resumen |

## Validaciones usadas durante el mes

Comandos habituales:

```powershell
dotnet build .\FreeCadPluging\FreeCadPluging.csproj -c Debug
dotnet build .\TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
& 'C:\Program Files\FreeCAD 1.1\bin\python.exe' -m py_compile .\FreeCadPluging\Workbench\Tandem2026\InitGui.py .\FreeCadPluging\Workbench\Tandem2026\tandem_commands.py
& "$env:APPDATA\FreeCAD\v1-1\Mod\Tandem2026\Host\FreeCadPluging.exe" show-server
& "$env:APPDATA\FreeCAD\v1-1\Mod\Tandem2026\Host\FreeCadPluging.exe" ping
```

FreeCAD:

- FreeCAD instalado: `C:\Program Files\FreeCAD 1.1`.
- Ejecutable CLI: `C:\Program Files\FreeCAD 1.1\bin\FreeCADCmd.exe`.
- Workbench instalado: `%APPDATA%\FreeCAD\v1-1\Mod\Tandem2026`.
- Si el endpoint local falla con conexion denegada, arrancar `Desing` en IIS Express o usar produccion.

AutoCAD:

- Cerrar AutoCAD si una DLL `DebugN` ya esta cargada; NETLOAD no sustituye una DLL en memoria.
- No reactivar autoload sin pedirlo.
- Para demo de insert manual usar inferior izquierdo.

## Pendientes principales

1. Confirmar visualmente en Desing_2 el reflejo final de paneles manuales con Ctrl+F5.
2. Arreglar `V_BR` / `V_TR` solo despues de la demo y sin tocar el camino estable `V_BL`.
3. Terminar equivalencia FreeCAD con AutoCAD/ZWCAD: abrir `PluginSession` correcto, paletas MVC sin error de vista, y mapear acciones `show-home`, `open-design`, `show-blocks`, `insert-block`, `save-walls` a adaptadores FreeCAD.
4. Extraer capa comun C# real (`TandemCad.Core` o equivalente) cuando una funcionalidad empiece a duplicarse entre plugins.
5. Consolidar assets FreeCAD: decidir si los IFC fuente se versionan, si solo se versionan `.FCStd`, y como se actualizan desde ZWCAD.

## Errores que no hay que repetir

- Crear logica geometrica distinta en cada plugin.
- Hacer que `NETLOAD` o `TANDEM` instalen/copien biblioteca completa.
- Usar rutas del repo como biblioteca final del usuario; la cache de usuario es AppData.
- Mostrar `PluginCadAuth` como home; es solo handshake.
- Enviar `session-ready` desde `PluginReady`; debe usarse `home-ready` / `page-ready`.
- Medir calidad del servidor con listados pesados en lugar de `PluginPing`.
- Guardar `BlockReference.Position` en vez de `AT:` para articulos manuales.
- Quitar el giro de 180 grados del DWG sin validar huella.
- Tocar `V_BL` para arreglar el bug del inferior derecho.
- En FreeCAD, poner negocio en Python. Python solo registra Workbench y muta documento FreeCAD.