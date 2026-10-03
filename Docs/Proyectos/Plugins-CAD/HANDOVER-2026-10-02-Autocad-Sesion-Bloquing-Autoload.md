# Handover — AutoCAD sesion, menu de proyectos, bloquing local y autoload

Documento para **retomar** el plugin AutoCAD 2026 + intranet Desing. Fecha de corte: **2026-10-02**.

Handovers previos (aun validos para muros y contrato de insercion; este fichero **actualiza** sesion, home, biblioteca y arranque):

- [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md)
- [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

Conversacion de referencia: [Sesion plugin ATK-60](c403cd73-821f-4fe0-a7ea-5badfad9da9e).

---

## 0. START HERE (agente)

1. Leer este fichero entero antes de tocar codigo.
2. Desarrollo: **Desing en IIS Express** (`TANDEM_LOCAL` → `https://localhost:44384/`). Si no corre Develop, el plugin cuelga o sale en blanco.
3. Compilar plugin: `dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug`. AutoCAD bloquea `bin\Debug`; cada build deja `bin\DebugN`. **NETLOAD esa carpeta.**
4. Desarrolladores **siguen usando NETLOAD** para probar un Debug nuevo. Tras **conectar una vez**, AutoCAD debe autoload al dia siguiente (ver seccion 8).
5. Cadenas UI intranet: generador `Desing/Scripts/ResourceGenerators/_gen_common_resources.js` + `node` (si no esta en PATH, el de Cursor: `...\cursor\resources\app\resources\helpers\node.exe`). No editar `.resx` / `Designer.cs` a mano salvo que el generador no se pueda ejecutar; entonces paridad `es`/`en`.
6. No hacer commit/push salvo que el usuario lo pida.
7. Ultimo slot compilado en esta sesion: **Debug79** (`...\bin\Debug79\AutocadPlugin.dll`).

---

## 1. Que se hizo el 2026-10-02 (resumen)

| Area | Que quedo |
|------|-----------|
| Sesion CAD | App-level, no por DWG. Cookie Identity en WebView2 + `cad-session.json`. |
| Handshake vs home | `PluginCadAuth` = login rapido. `PluginReady` = menu obras/ofertas/disenos. **No** postear `session-ready` desde PluginReady. |
| Home en blanco | El 540x700 era `PluginCadAuth` vacio. `ShowHome` siempre navega a PluginReady. Listados por `PluginHomeData`. |
| Biblioteca local | DWG **solo** en `%LocalAppData%\AtDesing\Content\Data\Block\{3D,3DRef,Xr}`. Insert/convert **no** leen el repo. |
| Primera instalacion | Sync + copiar DWG a AppData. Splash con pasos (`Copiando i/total`). Logo TDesing, no AT. |
| Actualizar | Boton en la **barra de modo** (antes del home), no en bloquing. |
| Bloquing | Catalogo local (`bloquing.json`). Busqueda/insert sin SQL/HTTP. |
| Convertir | Popup bajo (solo cabecera al seleccionar). |
| Paletas | Arrastrables (barrita superior); recuerdan sitio en `palette-layout.json`. Bloquing plegado = chip ~46 px. |
| Intro | Repite el **ultimo articulo** insertado (`_last` en `BlockInsertCommand`). |
| UnAtdesing | Cierra paletas, quita autoload/menu, borra WebView2 y `AtDesing` (confirma `SI`). |
| Barras negras | Overlay blanco + reloj + «Cargando…» hasta que WebView2 pinta. |
| Diagnostico red | Pie del menu general: Wi-Fi y servidor con texto (Buena/Aceptable/Regular/No es buena) + 4 puntitos. |
| Autoload | **Desactivado (2026-10-03).** Hay que NETLOAD el ultimo DebugN. `PluginAutoload.Disable()` quita el registro. |

---

## 2. Como se prueba (flujo de usuario)

1. Arrancar **Develop** (IIS Express, puerto 44384).
2. AutoCAD: `NETLOAD` del ultimo `bin\DebugN\AutocadPlugin.dll` (o abrir CAD si ya esta autoload).
3. `TANDEM` o pestana **Tandem 2026**.
4. Primera vez: splash (logo TDesing) → login Desing → validar equipo → si no hay biblioteca, copiar DWG a AppData → paletas modo/herramientas.
5. Primer boton de la barra izquierda = menu general (obras / ofertas / disenos). Debe **conectar al servidor**. Pie: calidad Wi-Fi y servidor.
6. Bloquing: buscar/insertar desde disco local. **Actualizar** en la barra de modo refresca AppData.
7. Convertir bloques: popup bajo; seleccionar e Intro; elegir 3D / 3DRef / Xr.
8. Tras insertar, **Intro** en el dibujo = mismo articulo otra vez.
9. Paletas: arrastrar por la barrita de arriba; al reabrir mantienen sitio.
10. `UnAtdesing` + `SI` = desinstalacion. Cerrar AutoCAD. Para volver: NETLOAD + conectar.

---

## 3. Arquitectura de sesion y home

```
TANDEM / pestana
  → PaletteHost.Show()
  → WebView2 perfil: %LocalAppData%\AtDesing\WebView2
  → GET DesignToolsAutocad/PluginSession  (deviceId, maquina, version)
       → login Account/Login si no hay cookie
       → GET PluginCadAuth   [Authorize, sin EDMX]
            post { action: "session-ready" }
            location.replace(PluginReady)   // prefetch
       → MarkAuthenticated()
            si !HasLocalLibrary() → RunLibraryUpdate (splash)
            si no → FinishConnectUi()
  → FinishConnectUi: oculta sesion, ShowToolPalettes, PluginAutoload.Install()

Menu general (boton show-home):
  → _waitingHome = true
  → splash «Cargando obras, ofertas y disenos…»
  → Navigate PluginReady?t=ticks   (siempre; no reutilizar PluginCadAuth)
  → GET PluginReady  [Authorize]  ficha vacia, rapida
  → JS GET PluginHomeData         listados + combos
  → post { action: "home-ready" }  solo entonces RevealSessionPage
```

**Regla critica:** `session-ready` solo en `PluginCadAuth`. Si PluginReady lo envia y ya hay sesion, `FinishConnectUi` **escondia** el home (ventana blanca). Ahora PluginReady envia `home-ready` / `page-ready`; el plugin solo revela si `_waitingHome`.

`PluginCadAuth.cshtml` esta vacio a proposito (solo handshake). No mostrarlo como menu.

---

## 4. Ficheros clave (plugin)

| Fichero | Rol |
|---------|-----|
| `TandemAutocadPlugin/AutocadPlugin/PaletteHost.cs` | Paletas, mensajes WebView2, ShowHome, sync, autoload al conectar |
| `UI/Views/PaletteWindow.xaml(.cs)` | WebView2, splash, overlay «Cargando…», drag, UserDataDir |
| `UI/Views/TandemMiniPopup.xaml(.cs)` | Convertir / progreso instalacion; `SizeToContent=Height`; drag cabecera |
| `PluginSessionStore.cs` | `%LocalAppData%\AtDesing\Content\Data\db\cad-session.json` (14 dias) |
| `PluginAutoload.cs` | Copia a `AtDesing\Plugin`, registro HKCU Applications, TRUSTEDPATHS |
| `PluginDevReset.cs` | Wipe WebView2 + AtDesing + eta/theme |
| `Atk60LibrarySync.cs` | Manifiesto + copia a `Content\Data\Block`; `HasLocalLibrary` = hay `.dwg` |
| `Atk60DwgResolver.cs` | **Solo cache AppData.** No leer `Desing\Content\...\AtkSystem60` al insertar |
| `BlockInsertCommand.cs` | INSERT; `_pending` / `_last`; Intro repite |
| `BlockConvertCommand.cs` | Cambiar 3D/3DRef/Xr; popup compacto |
| `PaletteLayoutStore.cs` | `palette-layout.json` (left/top por id: session, mode, tools, blocks, convert) |
| `NetworkLinkInfo.cs` | Velocidad NIC (Wi-Fi/cable) → post `link-speed` |
| `MenuManager.cs` | `IExtensionApplication`, ribbon, `RemoveTandemTab` |
| `Commands.cs` | `TANDEM_*`, `UnAtdesing` |
| `copy-next-debug-slot.ps1` | Tras build: `bin\DebugN` |

---

## 5. Ficheros clave (intranet Desing)

| Fichero | Rol |
|---------|-----|
| `Desing/Controllers/DesignToolsAutocadController.cs` | PluginSession, PluginCadAuth, PluginReady, PluginHomeData, PluginPing, PluginBlocks, PluginBlockLibrary, PluginLibraryFile, create obra/oferta/diseno |
| `Views/DesignToolsAutocad/PluginReady.cshtml` | Menu general + pie de calidad de red |
| `Views/DesignToolsAutocad/PluginCadAuth.cshtml` | Handshake + redirect a PluginReady |
| `Views/DesignToolsAutocad/PluginBlocks.cshtml` | Bloquing; catalogo por `postMessage` catalog; sin fetch SQL |
| `Views/Shared/_LayoutAutocadPalette.cshtml` | Layout paletas; `plantilla-theme` |
| `Views/Shared/_TdesingBootSplash.cshtml` | Splash HTML; `page-ready` al dismiss |
| `Views/Desing_2/_Desing2ModeToolBar.cshtml` | Boton **Actualizar** biblioteca **antes** de show-home |
| `Scripts/ResourceGenerators/_gen_common_resources.js` | Claves `PluginCad_*` (incl. Speed*) |

Endpoints nuevos/relevantes:

- `GET PluginPing` — `dbMs` / `serverMs` (anonimo, sin cache).
- `GET PluginHomeData` — JSON listados (Authorize).
- `GET PluginLibraryFile?folder=&file=` — **stream** DWG/JSON (IIS no sirve `.dwg` estatico; por eso AppData estaba vacio).
- `GET PluginBlockLibrary` — manifiesto; URLs apuntan a `PluginLibraryFile`.

---

## 6. Biblioteca local (obligatorio para usuarios finales)

Un usuario **no** tiene `C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60`.

Cache oficial:

```
%LocalAppData%\AtDesing\Content\Data\Block\3D\
%LocalAppData%\AtDesing\Content\Data\Block\3DRef\
%LocalAppData%\AtDesing\Content\Data\Block\Xr\
%LocalAppData%\AtDesing\Content\Data\Block\Snaps\     (JSON nudos)
%LocalAppData%\AtDesing\Content\Data\db\bloquing.json
```

- Primera instalacion / **Actualizar**: `Atk60LibrarySync` descarga (o copia desde el repo **solo como semilla** si el HTTP falla en el PC de desarrollo) **hacia** esas carpetas.
- `HasLocalLibrary()` = hay al menos un `.dwg` en 3D/3DRef/Xr. Un `bloquing.json` con `"ready": false` **no** cuenta (bug que saltaba el sync).
- `Atk60DwgResolver.ResolveDwg` no debe devolver rutas del repo. Si falta el fichero: `EnsureCached` → AppData.

En el PC de desarrollo, si AppData `3D` esta vacio, el insert **rapido** era un fallback al repo. Eso ya no vale.

---

## 7. Mensajes WebView2 (no romper)

Plugin ← pagina:

| action | Efecto |
|--------|--------|
| `session-ready` | `MarkAuthenticated` **solo si** aun no hay sesion. Si ya autenticado, ignorar. |
| `home-ready` / `page-ready` | Revelar home **solo si** `_waitingHome`. |
| `show-home` / `hide-home` | Toggle menu general. |
| `show-blocks` / collapse / expand / hide | Bloquing. |
| `insert-block` | `BlockInsertCommand.QueueFromPalette` + comando. |
| `open-design` | Muros en CAD. |
| `plantilla-theme` / `company-logo` | Color/logo splash. |
| `request-link-speed` | Plugin responde `link-speed` { kind, mbps }. |

`ShowHome` no debe hacer `RevealSessionPage` si la URL actual es PluginCadAuth o si PluginReady aun no ha pintado.

---

## 8. Autoload (sin NETLOAD al dia siguiente)

Tras `FinishConnectUi()`:

1. Copia `*.dll` (+ WebView2Loader) a `%LocalAppData%\AtDesing\Plugin\`.
2. HKCU `UserRegistryProductRootKey\Applications\AtDesing`: `LOADCTRLS=2`, `LOADER=...\AutocadPlugin.dll`, `MANAGED=1`.
3. Anade esa carpeta a `TRUSTEDPATHS`.

`UnAtdesing`: `PluginAutoload.Uninstall()` (registro, TRUSTEDPATHS, quita ribbon) **antes** de borrar `AtDesing`. Luego wipe WebView2 + producto. Cerrar AutoCAD.

Desarrollador: NETLOAD `DebugN` nuevo + **conectar** actualiza la copia en `AtDesing\Plugin`.

---

## 9. UI / UX que no revertir

- Splash nativo (`TandemMiniPopup`) durante connect/install: decir **que** pasa (Validando, Copiando i/n, Cargando obras…).
- Logo **TDesing** hasta que llegue `plantilla-theme` (`PluginSplashBrand.ForceDefaultUntilPlantilla`).
- Convertir: altura al contenido; no 320 px vacios.
- Toolbars: fondo **blanco** + «Cargando…» hasta `NavigationCompleted`. Nunca dejar el HWND negro de WebView2 a la vista (`ParkWeb` hasta listo).
- Pie home: calidad en palabras + 4 puntitos (no solo ms). Tooltip con numeros. Un tercio mas grande (~13 px).
- Paletas moviles; bloquing plegado no debe ser una tira de 680 px.

---

## 10. i18n

Modulo `Common` (generador `_gen_common_resources.js`):

- Home: `PluginCad_LoadingHome`, `PluginCad_HomeLoadFailed`, `PluginCad_NoJobsides`, …
- Red: `PluginCad_SpeedMeasuring`, `PluginCad_SpeedWifiTitle`, `PluginCad_SpeedGood` / `Ok` / `Poor` / `Bad`, etc.

Tras cambiar el generador: ejecutarlo. Paridad `es` / `en`.

---

## 11. Anti-patrones (hoy se rompio por esto)

- Navegar o revelar el home estando en `PluginCadAuth` (pagina en blanco).
- `session-ready` desde PluginReady con sesion ya puesta → oculta el menu.
- `HasLocalLibrary` = «existe bloquing.json» sin DWG.
- Insertar leyendo `C:\00_Tandem2026\Desing\Content\...` (el cliente no lo tiene).
- Servir `.dwg` como estatico IIS (404); usar `PluginLibraryFile`.
- `PositionOverAcad` en cada Show/collapse (pisa la posicion guardada).
- Mostrar WebView2 de toolbars antes de `EnsureCoreWebView2` (negro ~40 s).
- Borrar `AtDesing` en UnAtdesing **sin** quitar el registro de autoload.

---

## 12. Pendiente / no de esta sesion

- Personal / derechos, correo SendGrid de instalacion, Web Deploy colgado: fuera de alcance.
- BricsCAD / Revit: no.
- Velocidad de descarga de la biblioteca (MaxParallel 4, HTTP por fichero): se puede mejorar.
- El README del plugin aun decia que INSERT no existia; la insercion ATK-60 **si** esta (3D / 3DRef / Xr, nudos, Intro = repetir).
- **Encofrado muros rectos (2026-10-02, tarde):** unica logica en `Atk60WallsRepository.SolveFromIdsJson`. Desing_2 `GetWallsAtk-60` y CAD `PluginEncofrarAtk60` solo llaman a eso. Desing pinta STL/GLB; AutoCAD inserta DWG (`TANDEM_ENCOFRAR`). No duplicar pack/pose/recorte 450 mm en JS ni en el plugin.

---

## 13. Checklist si un agente retoma manana

- [ ] Develop (44384) en marcha antes de culpar al plugin.
- [ ] NETLOAD el `DebugN` mas reciente o, si ya hubo primera conexion, abrir AutoCAD y ver si autoload.
- [ ] AppData `Block\3D` tiene `.dwg` tras primera instalacion o Actualizar.
- [ ] Menu general lista obras y el pie muestra puntitos (no un recuadro blanco).
- [ ] UnAtdesing + cerrar CAD deja de cargar el plugin al dia siguiente.
- [ ] No commitear `.vs`, `bin\`, `obj\`, GLB/STL de `Desing\bin`, ni slots DebugN salvo que el usuario lo pida.
