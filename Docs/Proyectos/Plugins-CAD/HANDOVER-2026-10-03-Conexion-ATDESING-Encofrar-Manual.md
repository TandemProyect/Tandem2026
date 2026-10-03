# Handover — Conexión Desing/CAD, ATDESING y siguiente US

Documento para **retomar** el 2026-10-03. Este fichero **actualiza** NETLOAD, biblioteca, diagnóstico de conexión y el estado Git. El del 2 de octubre sigue valiendo para sesión, home, bloquing e inserción.

Handovers previos (leer si tocas esas áreas):

- [HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md](./HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md) — sesión, home, paletas, insert/convert
- [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md) — contrato geométrico ATK-60
- Encofrado automático de muros: `Atk60WallsRepository.SolveFromIdsJson` (Desing y CAD). No duplicar.

Conversación de referencia: [Sesion plugin ATK-60](c403cd73-821f-4fe0-a7ea-5badfad9da9e).

---

## 0. START HERE (agente)

1. Leer este fichero entero antes de tocar código.
2. Rama de trabajo: **`develo`** (no se llama `develop`). `master` está al mismo commit.
3. Corte Git: **`c0883c4`** — `conexion en desing y cad`. Ya está en `origin/develo` y `origin/master`.
4. La rama `cursor/atk60-articles-3d-3dref-xr` es el mismo commit. Se puede borrar (local y remota).
5. Desarrollo: Desing en IIS Express (`TANDEM_LOCAL` → `https://localhost:44384/`). Plugin: `NETLOAD` del último `bin\DebugN`.
6. **NETLOAD / TANDEM no instalan la biblioteca.** Solo el comando **`ATDESING`** copia DWG a AppData.
7. `Web.config` de Desing está otra vez en **site4now** (remoto). El bloque `.\SQLEXPRESS` queda comentado para pruebas locales.
8. No hacer commit/push salvo que el usuario lo pida.
9. Siguiente trabajo: US **Encofrar manualmente** (aún no existe en Azure; PAT de los scripts en 401). Texto listo en la sección 10.
10. Antes de un code review del equipo: leer [CODE-REVIEW-PREP-2026-10-03.md](../../General/CODE-REVIEW-PREP-2026-10-03.md). Contraseñas ya no van en el JSON de Personal ni en Session al editar. TLS estricto en tdesing.net.

---

## 1. Estado Git (2026-10-03)

| Ref | Commit | Nota |
|-----|--------|------|
| `develo` / `origin/develo` | `c0883c4` | Rama de desarrollo |
| `master` / `origin/master` | `c0883c4` | Fast-forward desde la feature |
| `cursor/atk60-articles-3d-3dref-xr` | `c0883c4` | Misma punta; se puede borrar |

Working tree limpio en el merge. No hay commits pendientes de push.

La feature se fusionó por **fast-forward** (era ancestro de `develo` y `master`). No hubo merge commit.

---

## 2. Qué se hizo el 2026-10-03 (resumen)

| Área | Qué quedó | No revertir |
|------|-----------|-------------|
| NETLOAD | Carga el plugin y, si hay `cad-session.json` reciente, reanuda sesión. **No** crea carpetas ni copia bloques. | No llamar `EnsureFolders` / `CacheDir` / `SyncNow` en `Initialize`, `Show` ni `MarkAuthenticated`. |
| `ATDESING` | Única vía de instalación: `%LocalAppData%\AtDesing` + DWG 3D / 3DRef / Xr. | No reinstalar en cada NETLOAD (tardaba ~2 min). |
| Autoload | Sigue **apagado**. `PluginAutoload.Disable()` al iniciar y al conectar. | No reactivar registro sin pedirlo. |
| Pie plugin (home) | Calidad **Servidor** = `GET PluginPing` → `dbMs` (`SELECT 1`). Re-ping al cargar home y cada 8 s. | No usar `PluginHomeData.listMs` (varias consultas; en local/remoto se iba a ≥500 ms y salía «No es buena»). |
| Test Desing_2 | Botón Wi‑Fi en la tira derecha (junto a MAPA). Panel de 3 bloques: **Tu equipo / Wi‑Fi / Servidor**. | No volver a pintar origen SQL ni catálogo (confidencial). |
| Calidad | Umbrales en ms del ping SQL: &lt;80 Buena, &lt;200 Aceptable, &lt;500 Regular, ≥500 No es buena. Wi‑Fi por Mb/s de la NIC del servidor, no por `navigator.connection.downlink` de Chrome. | No mezclar velocidad del navegador con el servidor. |
| Publicación | Probado en **tdesing.net**: cable 1000 Mb/s, servidor Buena, tiempos ~0,00–0,34 s. Usuario: OK. | `Web.config` publicado = site4now. |

---

## 3. Flujo AutoCAD (hoy)

```
NETLOAD último DebugN
  → MenuManager.Initialize
       PluginAutoload.Disable()
       si PluginSessionStore.HasRecent() → reanuda TANDEM sin instalar
       si no → «TANDEM para conectar. ATDESING para instalar (una vez).»

TANDEM / pestaña
  → PaletteHost.Show()  (login / handshake / home)
  → MarkAuthenticated() → FinishConnectUi()
       NO sync de biblioteca
       si !HasLocalLibrary() → mensaje: escribe ATDESING

ATDESING
  → PaletteHost.InstallLibrary()
       EnsureFolders() + RunLibraryUpdate(..., connectWhenDone: true)
       splash «Copiando i/n» si hace falta
```

`HasLocalLibrary()` = existe `bloquing.json` **o** hay algún `.dwg` en `Block\{3D,3DRef,Xr}`.

`LoadIndexIfPresent()` lee el índice si está; **no** crea carpetas. `CacheDir()` / `IndexPath()` sí crean directorios: solo desde `EnsureFolders()` → solo `ATDESING`.

Si alguien llama `CacheDir()` en el arranque, se crean carpetas vacías y el sync puede dispararse otra vez (el bug de los ~2 minutos).

---

## 4. Diagnóstico de conexión

Hay **dos** superficies. No las mezclar.

### 4.1 Pie del menú CAD (`PluginReady`)

- Fichero: `Desing/Views/DesignToolsAutocad/PluginReady.cshtml`
- Endpoint: `GET DesignToolsAutocad/PluginPing` (anónimo, sin caché)
- Cuerpo: `{ ok, dbMs, serverMs }` — un `SELECT 1` por EF
- El pie muestra Wi‑Fi/cable (NIC del plugin vía `link-speed`) y **Servidor · hostname**
- `lastServer` = `dbMs` del ping, no el tiempo de listar obras

`Global.asax` hace un warmup `SELECT 1` en background para no pagar el primer EF en frío en el pie.

### 4.2 Panel Desing_2 (más completo; el que debe usar un usuario de intranet)

- Botón: tira derecha del visor STL (`_Desing2StlViewerWorkspace.cshtml`), icono Wi‑Fi
- JS: `Desing/Scripts/Desing2/desing2-connection-test.js` (incluido en `Viewer.cshtml`)
- CSS: `site.css` clases `desing2-conn-test*`
- Endpoint: `GET DesignToolsAutocad/ConnectionDiagnostics` **[Authorize]**
- El JSON de MVC puede llegar en camelCase o PascalCase: el JS usa `pick(obj, camel, pascal)`

Bloques del panel (textos en `Common` / `_gen_common_resources.js`, prefijo `ConnectionTest_*`):

| Bloque | Qué muestra | De dónde |
|--------|-------------|----------|
| Tu equipo | Dónde está abierto TDesing, nombre del PC, usuario | `host` / `appUrl` (localhost → «Este ordenador»; tdesing.net → «Internet (TDesing)»), `machineName`, `userName` |
| Wi‑Fi | Calidad + tipo + Mb/s | `net.kind` / `net.mbps` del **servidor** (NIC). Fallback Chrome solo si no hay NIC |
| Servidor | Calidad + dónde están los datos + tiempos | `data.ms` (EF `SELECT 1`), `identity.ms` (ADO Identity), `list.ms` (count obras), `appMs`. Tiempos en **segundos** (`0,17 s`) |

El JSON **no** debe incluir `DataSource`, catálogo, `@@SERVERNAME` ni cadenas de conexión. Internamente `ReadSafeSqlTarget` aún lee source/catalog para calcular `isLocal` / `auth`; no los serializa.

Calidad servidor (ms del ping datos):

| ms | Texto |
|----|--------|
| &lt; 80 | Buena |
| &lt; 200 | Aceptable |
| &lt; 500 | Regular |
| ≥ 500 o fallo | No es buena |

Calidad Wi‑Fi (Mb/s NIC): ≥100 Buena, ≥50 Aceptable, ≥20 Regular, &gt;0 No es buena.

**Por qué el usuario vio «Wi‑Fi Buena» y «Servidor No es buena» con SQL “local”:**

1. `Web.config` seguía en **site4now**, no en `.\SQLEXPRESS`. El plugin `TANDEM_LOCAL` habla con IIS local; IIS puede seguir apuntando a SQL remoto.
2. El pie usaba `listMs` de `PluginHomeData` (varias queries), no `dbMs`.
3. Instancia `NUCBOXG5\SQLEXPRESS` + SQL Browser parado: el primer ping por nombre de host era lento. Por eso el bloque local comentado usa `.\SQLEXPRESS`.

170 ms remotos = **Aceptable**, no un fallo.

---

## 5. Web.config (Desing)

`Desing/Web.config` → `connectionStrings`:

- **Activo:** `SQL5113.site4now.net` / `db_a197cd_desingproducction` (`IdentityConnection` + `ConexionData`).
- **Comentado:** `.\SQLEXPRESS` misma base, Integrated Security.

Para probar local: comentar el bloque site4now y descomentar SQLEXPRESS. Reciclar IIS Express. Volver a site4now antes de publicar.

No commitear contraseñas nuevas ni documentarlas aquí.

---

## 6. Ficheros tocados en esta tanda

### Plugin AutoCAD

| Fichero | Cambio |
|---------|--------|
| `TandemAutocadPlugin/AutocadPlugin/Commands.cs` | Comando `ATDESING` → `PaletteHost.InstallLibrary()` |
| `TandemAutocadPlugin/AutocadPlugin/PaletteHost.cs` | `InstallLibrary`; `MarkAuthenticated` ya no instala; aviso si no hay biblioteca |
| `TandemAutocadPlugin/AutocadPlugin/Atk60LibrarySync.cs` | `LoadIndexIfPresent` / `HasLocalLibrary` no crean carpetas |
| `TandemAutocadPlugin/AutocadPlugin/MenuManager.cs` | Resume sesión en NETLOAD; mensaje ATDESING |
| `TandemAutocadPlugin/AutocadPlugin/PluginAutoload.cs` | Sigue Disable; ATDESING en registro si se reactiva autoload |
| `TandemAutocadPlugin/AutocadPlugin/README.md` | NETLOAD ≠ instalar |

### Intranet Desing

| Fichero | Cambio |
|---------|--------|
| `Desing/Controllers/DesignToolsAutocadController.cs` | `PluginPing`; `ConnectionDiagnostics` + helpers (sin filtrar secretos al JSON) |
| `Desing/Views/DesignToolsAutocad/PluginReady.cshtml` | Pie: ping-only, re-ping 8 s |
| `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml` | Botón + panel 3 bloques |
| `Desing/Scripts/Desing2/desing2-connection-test.js` | Pintado amigable; NIC &gt; downlink Chrome |
| `Desing/Views/Desing_2/Viewer.cshtml` | Include del JS |
| `Desing/assets/materio/css/site.css` | Estilos `.desing2-conn-test*` |
| `Desing/Global.asax.cs` | Warmup `SELECT 1` |
| `Desing/Web.config` | site4now activo; SQLEXPRESS comentado |
| `Desing/Scripts/ResourceGenerators/_gen_common_resources.js` | Claves `ConnectionTest_*` (es/en) |
| `Desing/Resources/Common.resx`, `Common.en.resx`, `Common.Designer.cs` | Paridad; `node` a veces no está en PATH → se rellenó a mano |

i18n: si se tocan cadenas, preferir el generador. Node del PATH puede faltar; el de Cursor sirve.

---

## 7. Cómo se prueba (regresión de esta tanda)

1. Develop (44384) + `Web.config` site4now.
2. AutoCAD: `NETLOAD` DebugN. **No** debe copiar bloques ni tardar minutos.
3. Si ya hubo `ATDESING` antes: bloquing abre catálogo local. Si no: consola pide `ATDESING`.
4. `ATDESING` una vez: splash de copia; luego conectar.
5. Segundo `NETLOAD`: conectar sin reinstalar.
6. Home CAD: pie Servidor = Buena/Aceptable con `dbMs`, no «No es buena» por listar obras.
7. Intranet `/Desing_2/Viewer`: botón Wi‑Fi de la tira derecha → 3 bloques, sin origen/catálogo SQL.
8. Producción tdesing.net: mismo panel; en la oficina (cable 1 Gb) debe salir Buena.

---

## 8. Anti-patrones (esta tanda)

- Instalar biblioteca en NETLOAD / `Show` / `MarkAuthenticated`.
- Crear `Block\3D` vacías y luego decidir que hay que sincronizar.
- Medir «servidor» con el listado de obras o con `navigator.connection.downlink`.
- Devolver Data Source / Initial Catalog / contraseñas en JSON de diagnóstico.
- Confundir IIS local (`TANDEM_LOCAL`) con SQL local (`Web.config`).
- Usar la rama `develop` (no existe). Es **`develo`**.
- Crear la US en Azure con `Scripts/Create-US-Fast.ps1` / `US.ps1` hasta renovar el PAT (hoy **401**).

---

## 9. Pendiente de producto (no de esta tanda)

- Autoload al día siguiente: sigue desactivado a propósito.
- BricsCAD / Revit: no.
- Velocidad de descarga de la biblioteca (MaxParallel 4).
- PAT Azure DevOps caducado: no se pudo crear la US desde script.

---

## 10. Siguiente US — Encofrar manualmente (aún no en Azure)

El usuario va a crearla en el board. Texto acordado:

**Título:** Encofrar manualmente  
**Puntos:** 8  
**Área:** tandem2026  

**Descripción (pegar en Azure):**

Colocar paneles ATK-60 **a mano** (pick de punto o inserción), no solo el encofrado automático de muros. Misma biblioteca y misma lógica de piezas que el encofrado automático: variantes **3D / 3DRef / Xr**. Superficies: Desing_2 (STL/GLB) y AutoCAD (DWG, `TANDEM_ENCOFRAR` hoy es solo muros automáticos). Reutilizar `Atk60WallsRepository` / catálogo AppData; no duplicar pack, pose ni recorte 450 mm en JS ni en el plugin.

**Criterio de éxito (propuesta para el agente que la implemente):**

- El usuario elige un artículo del bloquing (o equivalente en Desing) y lo coloca en un punto.
- El resultado usa el mismo DWG/STL que el automático para ese código.
- No reinstalar biblioteca ni tocar el test de conexión salvo que se rompa.
- Trabajar en `develo`. Vincular commits `AB#<id>` cuando exista la US.

**Punto de entrada de código (automático, no borrar):**

- Servidor: `Atk60WallsRepository.SolveFromIdsJson`
- Desing_2: `GetWallsAtk-60` pinta STL/GLB
- AutoCAD: comando `TANDEM_ENCOFRAR` / `ENCOFRAR` → `PluginEncofrarAtk60` inserta DWG
- Inserción suelta ya existe: `BlockInsertCommand` + paleta bloquing (`TANDEM_INSERTBLOQUE`). La US manual es **encofrar** (pack/pose/reglas), no solo INSERT de un bloque suelto. Confirmar con el usuario si «manual» = insert libre o un modo que aplica reglas ATK-60 sobre un punto.

**Azure:** `https://dev.azure.com/VSCAD/tandem2026`  
Scripts: `Scripts/Create-US-Fast.ps1` y `Scripts/US.ps1` — PAT hardcodeado **401**. Renovar PAT o alta manual. Guía: `Docs/Scripts/CREATE-US-FAST.md`.

---

## 11. Checklist si un agente retoma

- [ ] Estás en `develo` en `c0883c4` o un hijo, al día con `origin/develo`.
- [ ] Has leído la sección 3 (NETLOAD ≠ ATDESING) y la 4 (dos diagnósticos).
- [ ] No tocas `Web.config` en un commit salvo que el usuario pida cambiar SQL.
- [ ] La US «Encofrar manualmente» está en Azure (o el usuario te da el ID) antes de implementar.
- [ ] Confirmas con el usuario el alcance: insert libre vs encofrado con reglas en un punto.
- [ ] Develop 44384 encendido antes de culpar al plugin.
- [ ] No commitear `.vs`, `bin\`, `obj\`, slots `DebugN`, ni secretos.

---

## 12. Code review / recortes (2026-10-03 tarde)

Aplicado en working tree (sin commit todavía):

| Cambio | Por qué |
|--------|---------|
| `ViewBag.SkipIntranetDataTables` en Viewer + `_LayoutMaterio` | El visor no tiene grids; se ahorran ~2,6 MB (DataTables + pdfmake + JSZip) |
| `ShouldSkipChromeDb` incluye PluginPing / HomeData / Diagnostics / library | El ping cada 8 s ya no abre plantilla/idioma/empleado |
| PluginReady: no ping si la pestaña está oculta; `clearInterval` en `pagehide` | Menos SQL en paletas de fondo |
| MapLibre solo al abrir el modal de edificios | No se baja unpkg en cada Viewer |
| `Atk60LibrarySync` cachea `bloquing.json` por mtime; descarga a stream | Menos parse e I/O RAM en insert |
| `Atk60DwgResolver` usa `BlockRoot()` (no crea carpetas) y quita HttpClient muerto | Evita el bug de carpetas vacías |
| `ShouldSkipChromeDb` = todo `DesignToolsAutocad` + APIs `Desing_2` + JSON | Conectar a menudo no abre plantilla/empleado |
| `PluginPing` = ADO `SELECT 1`, sin EDMX | El pie CAD no instancia EF cada 8–20 s |
| Logo plugin desde cookie; `IsUserAllowed` / CAD-dev 45 s | Reabrir home no consulta Personal cada vez |
| `MVCApiService` HttpClient estático (keep-alive) | TANDEM / Encofrar / Salvar reutilizan TCP |
| Ping adaptativo: 20 s si Buena/Aceptable, 8 s si no; sin cultura en `/PluginPing` | Menos ruido SQL cuando la red ya es buena |

**No hecho (hace falta decisión):** rotar secretos de `Web.config`; quitar `AttPassAspNetUsert` del JSON de empleados y `Session["passVscad"]`; partir `DesignToolsController` (~2500 líneas); `TandemCad.Core` para Brics/Revit; un WebView2 Environment compartido.
