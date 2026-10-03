# AutocadPlugin — Plugin Tandem 2026 para AutoCAD

Class Library (`net8.0-windows`) para AutoCAD 2026. Paletas MVC, muro 2D y generar 3D.

**Handover (leer primero el de 2026-10-03):**

- NETLOAD vs ATDESING, conexión, siguiente US: [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md)
- Sesión, home, AppData, insert/convert: [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md)
- Muros / paletas base: [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Bloquing (contexto sept-29): [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Contrato geométrico inserción ATK-60 (el código **ya existe**): [`Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`](../../Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

## Compilar

Abrir `Design.sln` y compilar AutocadPlugin. Requisito: AutoCAD 2026 en `C:\Program Files\Autodesk\AutoCAD 2026\` (o `AUTOCAD_API_ROOT`).

```
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

Si AutoCAD deja la DLL bloqueada, usa otra carpeta: `-p:OutputPath=bin\Debug9\` y NETLOAD esa.

## Cargar en AutoCAD

```
NETLOAD → TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll
```

Pestaña **Tandem 2026** o comando `TANDEM`. Por defecto habla con **https://localhost:44384/** (`TANDEM_LOCAL`). Producción: `TANDEM_PRODUCCION` (tdesing.net). Override: `TANDEM_MVC_BASE_URL`.

Si no hay sesión, aparece el **login de Desing**. El **autoload está desactivado**: hay que `NETLOAD` el último `bin\DebugN`. `NETLOAD` / `TANDEM` **no** instalan la biblioteca. `ATDESING` crea `%LocalAppData%\AtDesing` y copia los bloques (una vez). `UnAtdesing` quita registro y estado local.

Tras conectar: paletas de modo/herramientas. El **primer botón** de la barra izquierda abre el menú de obras/ofertas/diseños (`PluginReady` + `PluginHomeData`). **Actualizar** biblioteca está en esa barra, **antes** del home. Bloquing: botón de modo o `INSERTARBLOQUE` / `TANDEM_INSERTBLOQUE`.

Abre paletas (sin título, compactas, arrastrables; recuerdan sitio):

- `DesignToolsAutocad/PaletteMode` — modo + Atk-60 + Actualizar + home
- `DesignToolsAutocad/PaletteTools` — herramientas CAD de Desing_2
- `DesignToolsAutocad/PluginBlocks` — biblioteca local (inserta 3D / 3DRef / Xr)

Cada `dotnet build` copia a `bin\DebugN` si AutoCAD bloquea `bin\Debug`. **NETLOAD esa carpeta.** El autoload (copia en AppData + registro) está apagado de momento.

DWG de usuario: **solo** `%LocalAppData%\AtDesing\Content\Data\Block\{3D,3DRef,Xr}`. El repo no existe en el PC del cliente.

No se usa el submenú CUI clásico (Panel / Detectar / …).

## Comandos

| Comando | Uso |
|---------|-----|
| `TANDEM` | Mostrar/ocultar paletas MVC (conecta; no copia bloques) |
| `ATDESING` | Instala la biblioteca local (`%LocalAppData%\AtDesing` + DWG). Única vía de instalación |
| `TANDEM_MURO2D` | Dibujar muro 2D (eje + caras 300 mm) |
| `TANDEM_MURO3D` / `GENERAR3D` | Sólidos 3D vía `LCornerDetector` |
| `TANDEM_INSERTBLOQUE` / `INSERTARBLOQUE` | Insertar bloque ATK-60 desde la paleta; **Intro** repite el último |
| `TANDEM_CAMBIARBLOQUES` | Convertir 3D / 3DRef / Xr (popup compacto) |
| `TANDEM_ENCOFRAR` / `ENCOFRAR` | Muros rectos: misma lógica que Desing (`SolveFromIdsJson`); inserta DWG |
| `TANDEM_SALVAR` / `SALVAR` | Guarda muros en `TSql_DesignWall` (mismo `ReplaceWalls` que Desing) |
| `TANDEM_PROBAR_CONEXION` | Ping al MVC |
| `TANDEM_LOCAL` | Destino IIS Express (`localhost:44384`) y recarga paletas |
| `TANDEM_PRODUCCION` | Destino tdesing.net y recarga paletas |
| `TANDEM_SERVIDOR` | Muestra URL activa |
| `TANDEM_CARGAR_MENU` | Recarga ribbon |
| `UnAtdesing` | Desinstala: quita autoload/ribbon, borra WebView2 y `%LocalAppData%\AtDesing` (confirma `SI`). Cerrar AutoCAD. Script: `reset-cad-dev-local.ps1` |

Otros stubs ZWCAD (`DETECTARMUROS`, …) siguen sin lógica.

## Documentación

[`Docs/Proyectos/Plugins-CAD/`](../../Docs/Proyectos/Plugins-CAD/)
