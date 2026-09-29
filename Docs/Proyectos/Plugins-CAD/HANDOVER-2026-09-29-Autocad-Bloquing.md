# Handover — AutoCAD biblioteca de bloques (bloquing)

Documento para **retomar** el formulario de bloques del plugin AutoCAD 2026. Fecha de corte: **2026-09-29**.

**Siguiente fase:** inserción ATK-60 (solo 3D / 3DRef en la primera release). Contrato: [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md). No implementar INSERT hasta que el usuario cierre el debate.

Handover previo (muros 2D/3D, sesión, paletas modo/herramientas):  
[HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)

---

## 0. START HERE (agente)

1. Leer este fichero entero antes de tocar código.
2. **No** duplicar geometría de muros (sigue valiendo el handover de septiembre).
3. **No** implementar INSERT hasta el contrato de [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md) (primera release: 3D + 3DRef, sin XREF).
4. Artículo de referencia para probar el visor: **Atk-60 2,70 × 0,90**, código **3120270090**.
5. Desarrollo: Desing en IIS Express + AutoCAD con **`TANDEM_LOCAL`**. Si la paleta carga tdesing.net, los cambios MVC locales no se ven. Interruptor local/producción: **§14** de este fichero.

Criterio de “formulario listo” (ya cumplido):

- Lista del maestro de artículos, búsqueda, miniatura `ImgIco`, visor 2 STL (estructura + fenólico), cubo TOP/FRONT + 2D/3D, herramientas de Design_2 (salvo color), plegado `_` a tira 32 px, cierre `×`.
- Clic en **Insertar** solo avisa en la línea de comandos; **no inserta**.

---

## 1. Estado

| Pieza | Estado |
|-------|--------|
| Abrir biblioteca desde paleta de modo | Hecho (`data-plugin-menu="show-blocks"`) |
| Formulario MVC + AJAX listado | Hecho |
| Visor STL 2 mallas + cubo + clip/zoom/ejes/fondo | Hecho |
| Plegado `_` / cierre `×` en el plugin WPF | Hecho (hace falta DLL nueva + **cerrar AutoCAD**) |
| Selectores de color de malla | **Quitados** (2026-09-29). Colores fijos encofrar. |
| Insertar bloque en el DWG | **Pendiente**. `insert-block` imprime un mensaje. |
| BricsCAD / Revit biblioteca | No. Solo AutoCAD. |

---

## 2. Cómo se abre

1. AutoCAD pestaña **Tandem 2026** (o `TANDEM`) → sesión Desing + paletas modo/herramientas.
2. En la barra de modo, botón biblioteca (`#ma-stl-plugin-blocks`) envía `{ "action": "show-blocks" }`.
3. `PaletteHost.ShowBlocks()` abre `DesignToolsAutocad/PluginBlocks` en una ventana WebView2 propia (`_blocks`).
4. Comando `INSERTARBLOQUE` también llama a `ShowBlocks()` (ya no es un stub vacío).

Requiere sesión Identity en el perfil WebView2 (`%TEMP%\TandemAutocadWebView2`) y usuario/dispositivo plugin no bloqueados (`EnsurePluginCadUser`).

---

## 3. Arquitectura

```
Paleta modo (MVC)
  → postMessage { action: "show-blocks" }
  → PaletteHost.ShowBlocks()
  → PaletteWindow WebView2 → GET /DesignToolsAutocad/PluginBlocks  [Authorize]
       splash hold → AJAX GET /DesignToolsAutocad/PluginSearchBlocks?q=
       maestro Tsql_Master_Articles
       STL: PluginCadAtk60PanelStlHelper → Atk60Element (encofrar)
            fallback: columnas STL del maestro + hermano _F / P2
  → visor Three.js (plugin-cad-block-preview.js)
  → Insertar → postMessage { action: "insert-block", id, dwg, caption }
  → plugin: mensaje; aún no INSERT
```

Todo el dato de artículos pasa por MVC. El `.dll` de AutoCAD **no** consulta SQL.

---

## 4. Mensajes WebView2 → plugin

Layout: `Desing/Views/Shared/_LayoutAutocadPalette.cshtml`  
Bloques también tiene `post()` propio. Se envía el **objeto** JS (no `JSON.stringify`), con fallback a string.

C# (`PaletteHost.OnPaletteMessage`):

1. Recorta basura alrededor de `{…}` (`NormalizePaletteJson`; el `ç` de `hide-blocks"}ç` no debe tumbar el parseo).
2. `TryHandleBlocksChrome` busca el token en el texto crudo (`hide-blocks`, `collapse-blocks`, `expand-blocks`).
3. `TryHandleSessionMessage` con `JObject`.
4. Si no hay `action` conocida, `MapToAcadCommand` (tool/mode).
5. Si tampoco: `WriteMessage("[Tandem paleta] " + json)` → **síntoma de DLL vieja**.

| `action` | Efecto |
|----------|--------|
| `show-blocks` | Abre / muestra biblioteca |
| `hide-blocks` | `Hide()` de `_blocks` (no `Close`; se puede reabrir) |
| `collapse-blocks` | Ventana **32 × 680**, chrome blanco (tira Design_2) |
| `expand-blocks` | Ventana **320 × 680**. Ignorado ~900 ms tras un collapse (evita reabrir al clic) |
| `insert-block` | Solo mensaje; payload `id`, `dwg`, `caption` |
| `session-ready` / `page-ready` / `open-design` / … | Sesión y muros (handover anterior) |

Si ves `[Tandem paleta] {"action":"collapse-blocks"}` en bucle con `expand-blocks`, AutoCAD **sigue con el plugin anterior en memoria**. NETLOAD no sustituye un assembly ya cargado.

---

## 5. Plegado `_` y cierre `×` (UX acordada)

Referencia visual: tira vertical blanca de Design_2 (`--d2-hsp-collapsed: 32px` en `Desing/assets/materio/css/site.css`).

- **`_`** (HTML `#plugin-blocks-collapse`): clase `is-collapsed` + `collapse-blocks`. El plugin deja una tira **32 px de ancho × 680 de alto**, alineada a la izquierda (`BlocksLeft = 80`). El contenido se oculta; queda el chip (logo pequeño arriba).
- El panel **no** se reabre mientras el ratón sigue encima. Solo tras `mouseleave` del `documentElement` (`hoverArmed`) y volver a entrar en el chip.
- **`×`**: `data-plugin-menu="hide-blocks"` → oculta la ventana. Reabrir con el botón de la barra de modo o `INSERTARBLOQUE` (vuelve expandida).
- Clic en el botón Tandem de bloquing con la ventana ya expandida: **toggle hide**. Si está colapsada (ancho &lt; 320): **expandir**.

Chrome WPF colapsado: `PaletteWindow.SetCollapsedChrome` (margen WebHost 0, fondo blanco, `CornerRadius` 16). `SetSize` admite `MinWidth` 16 (no bloquear a 120).

---

## 6. Visor STL

Fichero: `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js` (ES module, sin bundler).

- **Dos mallas** como encofrar: estructura `0xefb608` + fenólico `0x1a1816`. Artículo sin par: un STL `GENERIC_HEX`.
- CAD Z-up → Three Y-up: `mesh.rotation.x = -π/2` (igual que Design_2). El FRONT del cubo queda como TOP de Design_2.
- Cubo CSS `matrix3d` TOP/FRONT/… + botones 2D (orto) / 3D (iso).
- Herramientas que **sí** quedan: ampliar, zoom ±, encajar, fondo oscuro, clip (sliders X/Y), ejes.
- Herramientas **quitadas**: inputs `type=color` de estructura/fenólico. No reponerlos salvo petición.

Artículo de prueba: código Atenko `3120270090` → `Atk60Element.GetElement("Panel90270")` y `"Panel90270F"` → p.ej. `27904209.stl` + `27904209_F.stl`.

---

## 7. Datos MVC (maestro)

| Pieza | Ruta |
|-------|------|
| GET paleta (lista vacía a propósito) | `DesignToolsAutocadController.PluginBlocks` |
| GET JSON búsqueda | `PluginSearchBlocks` → `QueryPluginBlocks` (máx. 80, `AddIsActive`) |
| Par STL ATK-60 | `Desing/Helpers/PluginCadAtk60PanelStlHelper.cs` |
| Catálogo encofrar | `Atk60Element.GetElement` (internal, mismo ensamblado Desing) |
| Fallback STL | `LinkBlockDwgPlantStl` / alzado / hermano `_F.stl` o `P2.stl` si el maestro es `P.stl` |
| Miniatura | `ImgIco` vía SQL (`LoadMasterArticleIcoMap`); si falta la columna EF, SQL directo + catch. Fallback `Files/MaterialIco/{code}.png`, `panel.png`, `SinArticulo.png` |
| Logo título | `~/Content/images/ico/Logo.ico` **solo** junto a “Biblioteca de bloques”, no como thumb de fila |
| DTO | `PluginCadBlockRow` / `PluginCadBlocksVm` en `Desing/Models/PluginCadDesignRow.cs` |
| Vista | `Desing/Views/DesignToolsAutocad/PluginBlocks.cshtml` |
| Layout | `_LayoutAutocadPalette.cshtml` |
| Splash lento | `ViewBag.BootSplashHold = true` + `_TdesingBootSplash`; dismiss al terminar el AJAX |

Código panel vertical Atenko: `3120` + alto cm (3) + ancho cm (3). `3120270090` = 2,70 × 0,90 → clave `Panel90270`.

Zebra de filas: pares `rgba(0,0,0,0.05)` (gris ~5 %). Filas ~30 % más bajas que el primer prototipo; thumb 34 px.

---

## 8. i18n

Claves `PluginCad_Blocks*` en `Desing/Scripts/ResourceGenerators/_gen_common_resources.js` (paridad `es` / `en`). Regenerar con:

```text
cd Desing
node Scripts/ResourceGenerators/_gen_common_resources.js
```

`PluginCad_BlocksColorFrame` / `PluginCad_BlocksColorPhenolic` **eliminadas** al quitar los color pickers. `PluginCad_BlocksInsertSoon` sigue usándose mientras Insertar no inserte.

---

## 9. Plugin C# (ficheros tocados)

| Fichero | Rol |
|---------|-----|
| `PaletteHost.cs` | `_blocks`, tamaños 320×680 / 32×680, mensajes, debounce expand |
| `UI/Views/PaletteWindow.xaml` | `RootChrome`; splash y WebView2 |
| `UI/Views/PaletteWindow.xaml.cs` | `SetSize`, `SetCollapsedChrome`, splash hug (~320×248, `SizeToContent.Height`), `SetSize` ignorado mientras splash; WebMessage string **o** `WebMessageAsJson` |
| `MenuManager.cs` | Ribbon; al cargar imprime **ruta de la DLL** |
| `Commands.cs` | `INSERTARBLOQUE` → `ShowBlocks`; `HOLA` imprime DLL; `TANDEM_LOCAL` / `TANDEM_PRODUCCION` |
| `MvcServerSettings.cs` | Default **local** `https://localhost:44384/`; persistido en `%AppData%\Tandem\AutocadPlugin\mvc-target.txt` |
| `copy-next-debug-slot.ps1` | Tras cada build, copia a `bin\DebugN` porque AutoCAD bloquea `bin\Debug` |

Constantes paleta bloques (`PaletteHost`):

```text
BlocksWidth = 320
BlocksHeight = 680
BlocksCollapsedWidth = 32
BlocksLeft = 80
BlocksTop = 158
```

Splash de **sesión** (login Tandem 2026): márgenes enormes se debían a `Height` de ventana, no al padding de la tarjeta. `ApplySplashWindowSize` + no aplicar `SetSize` de la página mientras el splash está visible.

---

## 10. Compilar y cargar (trampa DebugN)

```text
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
```

Salida: `bin\Debug\AutocadPlugin.dll` + copia `bin\DebugN\`.

**AutoCAD no descarga un plugin ya NETLOADeado.** Para coger handlers nuevos (`collapse-blocks`, etc.):

1. Cerrar AutoCAD **por completo**.
2. NETLOAD de la carpeta **nueva** que imprima el build (`NETLOAD -> ...\DebugN\AutocadPlugin.dll`).
3. En consola debe salir `DLL: ...\DebugN\AutocadPlugin.dll` al inicializar (o comando `HOLA`).

Si la línea de comandos sigue mostrando `[Tandem paleta] {"action":"..."}`, **no** es un bug HTML: es la DLL vieja.

Slots `Debug2`…`Debug25` se borraron el 2026-09-29; se dejó el último de entonces (`Debug26`). El siguiente compile creará `bin\Debug` y, si está bloqueado, `Debug27`.

WebView2Loader se copia a la raíz del output (NETLOAD no resuelve `runtimes\win-x64\native`).

---

## 11. Decisiones que no hay que deshacer sin el usuario

- Formulario **antes** que insertar.
- Dos STL del catálogo **encofrar** (`Atk60Element`), no el `P.stl`/`P2` del maestro salvo fallback.
- `Logo.ico` no es thumb de fila.
- Rotación STL `-90°` en X como Design_2.
- Plegado = tira **alta y estrecha** (32×680), no chip 56×40.
- Colores de malla **no** editables en la paleta.
- Mensajes de paleta desconocidos se imprimen; las acciones de chrome de bloques **nunca** deben acabar en `[Tandem paleta]`.

---

## 12. Siguiente fase — Inserción

Contrato (debate, no código): [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md).

Hoy `insert-block` llega con `id` / `dwg` del maestro / `caption` y solo hace `WriteMessage`. Eso **no** es la fuente ATK-60. Falta **vista** (3DRef default) + **CodeName** (`27904209`).

**No implementar** hasta que el usuario cierre selector, picado y acoplamiento.

---

## 13. Índice rápido

| Qué | Dónde |
|-----|--------|
| Host paletas AutoCAD | `TandemAutocadPlugin/AutocadPlugin/PaletteHost.cs` |
| Ventana WPF | `UI/Views/PaletteWindow.xaml(.cs)` |
| Vista biblioteca | `Desing/Views/DesignToolsAutocad/PluginBlocks.cshtml` |
| Visor | `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js` |
| API + STL | `Desing/Controllers/DesignToolsAutocadController.cs` (`PluginBlocks`, `PluginSearchBlocks`, `ResolveStlPair`) |
| ATK-60 STL | `Desing/Helpers/PluginCadAtk60PanelStlHelper.cs` |
| Abrir UI | `Desing/Views/Desing_2/_Desing2ModeToolBar.cshtml` (`show-blocks`) |
| i18n | `_gen_common_resources.js` → `Desing/Resources/Common*.resx` |
| Local vs producción | `MvcServerSettings.cs`, comandos `TANDEM_LOCAL` / `TANDEM_PRODUCCION` / `TANDEM_SERVIDOR` (§14) |
| Muros (no tocar salvo bug) | handover 2026-09 CAD |

---

## 14. Conexión MVC: develop vs servidor

El plugin **no abre dos servidores a la vez**. Un solo `baseUrl` alimenta paletas WebView2 y HTTP (`MVCApiService`). Código: `MvcServerSettings.CurrentUrl()` vía `PluginExceptionHelper.ResolveBaseUrlFromEnv()`.

| Destino | URL | Comando |
|---------|-----|---------|
| Develop (IIS Express) | `https://localhost:44384/` | `TANDEM_LOCAL` (**default** si no hay archivo ni env) |
| Servidor intranet | `https://tdesing.net/` | `TANDEM_PRODUCCION` |

`TANDEM_SERVIDOR` y `HOLA` **solo muestran** el destino actual. `www.tdesing.net` se recorta a `tdesing.net`.

**Prioridad:** `TANDEM_MVC_BASE_URL` (env, gana siempre) → `%AppData%\Tandem\AutocadPlugin\mvc-target.txt` (`local` / `production` / URL) → si no hay fichero, **local**. El AppData **sobrevive a NETLOAD** de DebugN: una DLL nueva puede seguir yendo a producción.

Al cambiar de destino: `PaletteHost.ReconnectToCurrentServer()` cierra todas las paletas y reabre sesión contra el URL nuevo (login Identity de **ese** origen). No hace falta NETLOAD para cambiar de servidor; sí hace falta Desing arrancado si el destino es local.

WebView2: perfil único `%TEMP%\TandemAutocadWebView2`. Cookies de localhost y tdesing.net **no se mezclan** (orígenes distintos). En `/Account/Login` se borran cookies de **ese** sitio. El `deviceId` es del equipo (`PluginDeviceId` → `TSql_PluginDeviceAuth`); cada MVC decide si el device está autorizado.

Síntoma: UI de bloques “de producción” o lista que no refleja el código local → ejecutar `TANDEM_SERVIDOR`; si sale tdesing.net, `TANDEM_LOCAL`.

Este interruptor es de **AutoCAD**. BricsCAD/Revit no copian el fichero AppData salvo que su código lo tenga.
