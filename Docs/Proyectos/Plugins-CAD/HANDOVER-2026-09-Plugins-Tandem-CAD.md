# Handover — Plugins Tandem CAD (semana 2026-09-22 → 2026-09-28)

Documento para **continuar** AutoCAD, BricsCAD y Revit. No es el plugin ZWCAD ni el add-in de empresa en `No_Publicar/Revit_2026`.

**Estado:** muro 2D + generar 3D usable; **sesión Desing + abrir diseño (muros) en AutoCAD**; uniones al terminar el 2D y encofrado, pendientes. BricsCAD/Revit aún no tienen el login.

**Biblioteca de bloques (2026-09-29):** formulario listo en AutoCAD; inserción pendiente. Ver [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md).

---

## 1. Qué se hizo

Objetivo: el mismo flujo de **Desing_2** (intranet) en CAD:

1. Pestaña **Tandem 2026** abre la **sesión Desing** (login si hace falta) y las paletas MVC.
2. **Muro 2D:** eje + dos caras (espesor 300 mm), inglete entre tramos.
3. **Generar muros 3D:** llama al detector **compartido** y extruye perímetros `ModelDesing`.
4. Mientras genera, sale el popup de Desing_2: *«Generando modelo de muros…»* y se cierra al terminar.
5. **Elegir un diseño** (mismas `TSql_Design_V2` / `TSql_DesignWall` que la intranet) dibuja los muros 2D.

CAD tocados:

| Host | Carpeta | Cómo se carga |
|------|---------|----------------|
| AutoCAD 2026 | `TandemAutocadPlugin/AutocadPlugin/` | `NETLOAD` de la DLL |
| BricsCAD V26 | `TandemBricscadPlugin/BricscadPlugin/` | `NETLOAD` de la DLL |
| Revit 2026 | `TandemRevitPlugin/` | `.addin` en `%AppData%` o MSI WiX |

Los tres están en `Design.sln`.

---

## 2. Regla de oro (no romper)

**La geometría de muros y esquinas L no se duplica en cada plugin.**

Flujo único:

```
caras 2D (mm)
  → POST DesignToolsAutocad/ProcesarLineasZwcad  [AllowAnonymous]
  → LCornerDetector.DetectarEsquinasL
  → PolilineasADibujar (capa ModelDesing, AlturaExtrusion)
  → el plugin solo extruye / crea sólidos
```

Ficheros MVC:

- `Desing/Services/LCornerDetector.cs`
- `Desing/Controllers/DesignToolsAutocadController.cs` → `ProcesarLineasZwcad`
- Paletas: `PaletteMode` / `PaletteTools` (layout `_LayoutAutocadPalette.cshtml`)
- Barra de modo: `Desing/Views/Desing_2/_Desing2ModeToolBar.cshtml` (compartida con el visor)

Bugs o funciones nuevas de **muros/esquinas** → detector o este endpoint. El plugin solo: dibujar 2D, convertir unidades, HTTP, extruir.

Desing_2, en el visor, a veces **salta** perímetros de 6+ vértices para mallas ATK-60. En CAD **sí se extruyen las esquinas L** (todos los `ModelDesing` con altura).

---

## 3. Paletas MVC

La pestaña **Tandem 2026** no abre un submenú CUI (Panel / Detectar / …). Abre dos ventanas WebView2:

| Paleta | URL | Contenido |
|--------|-----|-----------|
| Modo | `{base}/DesignToolsAutocad/PaletteMode` | líneas / muro 2D / muro 3D / Encofrar + Atk-60 |
| Herramientas | `{base}/DesignToolsAutocad/PaletteTools` | PLINE, copiar, mover, recortar, alargar, estirar… |

Comportamiento:

- Sin título de ventana; compactas; **se mueven, no se redimensionan**.
- Mostrar/ocultar **solo** con la pestaña Tandem 2026 (o comando `TANDEM`).
- Clics → `chrome.webview.postMessage` → el plugin lanza el comando.
- Comandos nativos de AutoCAD/BricsCAD en **inglés** con prefijo internacional: `._MOVE`, `._EXTEND`, `._STRETCH`, …
- En Revit no hay `._MOVE`: se usa `PostableCommand` (Move, Copy, Offset, Delete, ModelLine).

**Requisito:** Desing en IIS Express para develop. Destino por defecto `https://localhost:44384/`.

### Local vs producción (AutoCAD)

No hay doble conexión simultánea: un `baseUrl` para paletas y API.

| Destino | URL | Comando |
|---------|-----|---------|
| Develop | `https://localhost:44384/` | `TANDEM_LOCAL` (default) |
| Servidor | `https://tdesing.net/` | `TANDEM_PRODUCCION` |

Consultar destino: `TANDEM_SERVIDOR` o `HOLA`. Override: env `TANDEM_MVC_BASE_URL` (gana sobre el fichero). Persistencia: `%AppData%\Tandem\AutocadPlugin\mvc-target.txt` (sobrevive NETLOAD). Al cambiar: cierra paletas y relanza sesión (`ReconnectToCurrentServer`). Detalle: [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md) §14.

WebView2: `WebView2Loader.dll` se copia a la raíz del output (`NETLOAD` / add-in no resuelven `runtimes\win-x64\native`). Perfil cookies: `%TEMP%\TandemAutocadWebView2`.

### Sesión Desing y diseños (AutoCAD, 2026-09-28)

La pestaña Tandem abre primero `DesignToolsAutocad/PluginReady` (`[Authorize]`).

- Si no hay cookie Identity → la página de **login de Desing** (`/Account/Login`). Una vez por sesión de AutoCAD (el perfil WebView2 está en `%TEMP%\TandemAutocadWebView2`).
- Tras entrar, paleta de **diseños** (`TSql_Design_V2` + oferta, igual que el dashboard). Al elegir uno se llama `Desing_2/GetDesignWalls` (mismas filas `TSql_DesignWall`) y AutoCAD dibuja eje/caras en `TANDEM_MURO_EJE` / `TANDEM_MURO_CARA`.
- No hay conexión SQL desde el plugin: todo pasa por MVC, como la intranet.
- Pendiente: el mismo flujo en BricsCAD y Revit.

---

## 4. Muro 2D

Comando: `TANDEM_MURO2D` (AutoCAD/BricsCAD). En Revit: botón **Muro 2D** / paleta (hace falta **vista de planta**).

- Cadena de puntos; Enter/Esc termina; en AutoCAD, **Cerrar** cierra el recinto si hay ≥ 2 tramos.
- Espesor Desing_2: **300 mm** (0,30 m si el DWG está en metros).
- Capas / estilos:
  - `TANDEM_MURO_EJE` — eje
  - `TANDEM_MURO_CARA` — las dos caras (esto es lo que come el 3D)
- Inglete en cambios de dirección.
- AutoCAD: XData app `TANDEM`.

**Pendiente (pedido explícito):** al finalizar el 2D, **arreglar uniones/conexiones**. Aún no está. El 2D se deja editable con STRETCH/EXTEND a propósito.

---

## 5. Generar muros 3D

Comandos AutoCAD/BricsCAD: `TANDEM_MURO3D`, `GENERAR3D`, `REGENERAR3D`. Paleta: icono edificio (`wall-3d`).

1. Recoge todas las entidades de `TANDEM_MURO_CARA` (no pide selección).
2. Convierte a **mm** y POST al detector (`AlturaMuroMm` = 2700).
3. Extruye cada polilínea `ModelDesing` con `AlturaExtrusion > 0`.
4. Capa / nombre `TANDEM_MURO_3D` (sustituye sólidos previos).
5. Popup *«Generando modelo de muros…»* (misma tarjeta que Desing_2).

Implementación 3D:

- AutoCAD / BricsCAD: `Region` + `Solid3d.Extrude`
- Revit: `DirectShape` + `GeometryCreationUtilities.CreateExtrusionGeometry` (pies internos; 1 ft = 304,8 mm)

El JSON del POST puede no bindear en MVC 5: `ProcesarLineasZwcad` hace fallback `ReadJsonBody`. Sesión opcional (plugin sin cookie). `MaxJsonLength` alto.

---

## 6. AutoCAD 2026

**Código:** `TandemAutocadPlugin/AutocadPlugin/` (`net8.0-windows`)  
**API:** `C:\Program Files\Autodesk\AutoCAD 2026\` o `AUTOCAD_API_ROOT`

```text
dotnet build TandemAutocadPlugin\AutocadPlugin\AutocadPlugin.csproj -c Debug
NETLOAD → TandemAutocadPlugin\AutocadPlugin\bin\Debug\AutocadPlugin.dll
```

Si AutoCAD **bloquea** la DLL, compilar a otra carpeta (`-p:OutputPath=bin\Debug9\`) y NETLOAD esa. En la semana se usaron Debug2…Debug9.

Pestaña ribbon (AdWindows): al activarla se abren/cierran paletas y se restaura la pestaña anterior. **No se carga el CUI** clásico (evita el submenú Panel/Detectar). `CuixBuilder` / `MNU/Tandem2026.cui` son residuales.

Comandos útiles: `TANDEM`, `TANDEM_MURO2D`, `TANDEM_MURO3D` / `GENERAR3D`, `TANDEM_PROBAR_CONEXION`, `TANDEM_CARGAR_MENU`.

`Commands.cs` aún tiene **stubs** ZWCAD (`INSERTARBLOQUE`, `DETECTARMUROS`, `CONFIGENCOFRADO`, leer/crear diseño, analizar imagen). En BricsCAD se eliminaron. No hace falta portarlos para el flujo actual.

---

## 7. BricsCAD V26

**Código:** `TandemBricscadPlugin/BricscadPlugin/`  
**API:** `C:\Program Files\Bricsys\BricsCAD V26 en_US\` o `BRICSCAD_API_ROOT`  
Referencias: `BrxMgd.dll`, `TD_Mgd.dll`. Namespaces `Bricscad.*` / `Teigha.*`. Ribbon: `Bricscad.Windows.ComponentManager` + `RibbonServices.CreateRibbonPaletteSet()` si el ribbon aún no existe.

```text
dotnet build TandemBricscadPlugin\BricscadPlugin\BricscadPlugin.csproj -c Debug
NETLOAD → TandemBricscadPlugin\BricscadPlugin\bin\Debug\BricscadPlugin.dll
```

Copia funcional de AutoCAD **sin** CUI, CuixBuilder ni stubs. Mismos comandos y paletas.

---

## 8. Revit 2026

**Código:** `TandemRevitPlugin/` (plugin + instalador). Proyecto `Tandem.Revit` en `Design.sln`.

Se miró **solo la arquitectura de instalación** de `No_Publicar/Revit_2026` (add-in per-user + WiX). **No se copió código** de ese add-in (protegido).

### Carga en desarrollo

`dotnet build TandemRevitPlugin\Revit\Tandem.Revit.csproj -c Debug` copia a:

- `%AppData%\Autodesk\Revit\Addins\2026\Tandem.addin`
- `%AppData%\Autodesk\Revit\Addins\2026\Tandem.Revit.Addin\`
- `TandemRevitPlugin\bundle\` (entrada del MSI)

Cerrar y abrir Revit. Pestaña **Tandem 2026**: Menús / Muro 2D / Generar 3D.

Muro 2D: estilos de línea `TANDEM_MURO_EJE` / `TANDEM_MURO_CARA` (subcategorías de líneas), no capas DWG. Paletas y HTTP van por `ExternalEvent` (el WebView2 no está en contexto de API).

### Instalador (WiX v4, per-user, sin admin)

```text
dotnet build TandemRevitPlugin\Installer\Installer.wixproj -c Release
```

MSI: `TandemRevitPlugin\Installer\bin\x64\Release\Tandem.Revit.v2026.Installer.msi`

Registro: `HKCU\SOFTWARE\Tandem\Tandem.Plugin.Revit`  
AddInId: `7A3C1E5B-9D24-4F81-B6A2-E18C4D7F2A90`  
Guía corta: `TandemRevitPlugin/README.md`

---

## 9. Unidades

El detector habla **milímetros**.

| Host | Unidades internas | Conversión |
|------|-------------------|------------|
| AutoCAD / BricsCAD | `INSUNITS` (4=mm, 5=cm, 6=m; default tratado como m) | `CadUnits` |
| Revit | pies | × 304,8 |

---

## 10. Pendiente (continuar por aquí)

Orden sugerido:

1. **Probar login + abrir diseño en AutoCAD** (Desing arrancado, pestaña Tandem 2026).
2. Portar la misma sesión a **BricsCAD** y **Revit** (mismo MVC `PluginReady`).
3. **Uniones al terminar muro 2D** (el usuario lo dejó para después; AutoCAD STRETCH/EXTEND es el motivo de dibujar líneas nativas).
4. **Biblioteca de bloques AutoCAD:** formulario listo. **Inserción:** contrato 3D/3DRef en [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md); no implementar hasta cerrar debate.
5. **Encofrar / ATK-60** sobre muros: el botón existe en la paleta de modo; no hay lógica de generación. Reutilizar el backend de Desing_2. El visor de bloques ya usa el par STL `Atk60Element`.
6. Quitar stubs restantes de `AutocadPlugin/Commands.cs` (ya limpios en BricsCAD) o portar de verdad lo que haga falta. `INSERTARBLOQUE` ahora abre el formulario.
7. Uniones / limpieza de caras **después** de estirar en CAD, luego regenerar 3D.
8. Altura de muro configurable (ZWCAD tiene formulario 2,70 m; CAD ahora fija 2700 mm).
9. Instaladores AutoCAD/BricsCAD (hoy solo Revit tiene MSI). Bundle/NETLOAD documentado basta para desarrollo.

---

## 11. Cómo seguir en un plugin sin tocar los otros

Plantilla de cambio:

1. ¿Es geometría de muro/esquina? → `LCornerDetector` + `ProcesarLineasZwcad`.
2. ¿Es UI de botones Desing_2? → parciales `_Desing2ModeToolBar` / toolbar superior + paletas MVC.
3. ¿Es dibujar en el host? → `Wall2dCommand` / `Wall3dCommand` / `PaletteHost` **de ese** plugin.
4. No copiar el detector al `.dll` de AutoCAD/BricsCAD/Revit.

Probar: Desing up → dibujar 2D → Generar 3D → debe aparecer el popup y sólidos en `TANDEM_MURO_3D`.

---

## 12. Índice de código

| Pieza | Ruta |
|-------|------|
| Detector | `Desing/Services/LCornerDetector.cs` |
| API | `Desing/Controllers/DesignToolsAutocadController.cs` |
| Paletas MVC | `PaletteMode.cshtml`, `PaletteTools.cshtml`, `PluginReady.cshtml`, `PluginBlocks.cshtml` |
| Biblioteca bloques | [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md) |
| Sesión plugin | `Account/Login` (ReturnUrl) → `DesignToolsAutocad/PluginReady` → `Desing_2/GetDesignWalls` |
| Import muros AutoCAD | `TandemAutocadPlugin/AutocadPlugin/WallImportCommand.cs` |
| Layout paleta | `Desing/Views/Shared/_LayoutAutocadPalette.cshtml` |
| Toolbar modo | `Desing/Views/Desing_2/_Desing2ModeToolBar.cshtml` |
| AutoCAD | `TandemAutocadPlugin/AutocadPlugin/` |
| BricsCAD | `TandemBricscadPlugin/BricscadPlugin/` |
| Revit | `TandemRevitPlugin/Revit/` + `TandemRevitPlugin/Installer/` |
| README Revit (instalación) | `TandemRevitPlugin/README.md` |
