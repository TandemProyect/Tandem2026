# UI Tandem (Materio): plantillas, DataTables y cookies

Documentación para continuar el trabajo (otro agente o desarrollador).  
Rama de referencia al redactar esto: `feature/plantilla-favicon`.  
Ámbito principal: `Desing/` (ASP.NET MVC 5, layout Materio, DataTables 1.13 CDN).

---

## 1. Layout y plantilla de usuario

### Archivos clave

| Archivo | Rol |
|--------|-----|
| `Views/Shared/_LayoutMaterio.cshtml` | Layout principal: assets Materio, DataTables (CSS/JS CDN), `meta name="dt-user"`, cookie del menú colapsado, scripts de pie. |
| `Views/Shared/_SidebarMaterio.cshtml` | Menú lateral, logo `ViewBag.PlantillaLogo`, ítems y activo por controlador. |
| `Views/Shared/_NavbarMaterio.cshtml` | Barra superior: toggle menú, badge entorno, avatar/dropdown. |
| `Views/Shared/_PlantillaStyles.cshtml` | **Plantilla de color y marca**: `ViewBag.PlantillaColor` → `--tandem-primary`, Bootstrap primary, enlaces; tipografía marca; estilos DataTables/Materio que dependen del color plantilla. |
| `Views/Shared/_FooterMaterio.cshtml` | Pie de página. |
| `assets/materio/css/site.css` | Overrides Tandem: separador pie (línea color plantilla), fondo menú lateral `--tandem-chrome-bg`, borde inferior navbar gris (como separadores `.menu-header`), DataTables integración, popups, etc. |

### ViewBag / plantilla

En controladores o filtros se suele rellenar (nombres orientativos): `PlantillaColor`, `PlantillaLogo`, `PlantillaFavicon`, `avatar`, `userName`, etc. `_PlantillaStyles` lee al menos `PlantillaColor` con fallback `#349d7d`.

---

## 2. Identidad de usuario para cookies (`dt-user`)

En `_LayoutMaterio.cshtml` ( `<head>` ):

```html
<meta name="dt-user" content="...(User.Identity.Name o 'anon')..." />
```

**Obligatorio** para:

1. **DataTables** (`Scripts/datatables-state.js`): la clave de cookie de estado es `dt_<usuario>_<idTabla>` (misma lógica `getUser()` + `cookieKey(tableId)`).
2. **Menú lateral colapsado** (script inline en `_LayoutMaterio`): cookie `tandem_menu_collapsed_<mismo_valor_dt-user>`.

Así cada usuario tiene su propio estado de tablas y su preferencia de menú; sin login todo queda bajo `anon`.

---

## 3. DataTables: estado, ColReorder y estilos

### `Scripts/datatables-state.js`

- **Defaults globales**: `stateSave`, `stateDuration` largo, `language` español (estilo oración), `colReorder` con objeto `{ bEnable, iFixedColumnsRight: 1 }` (fija la última columna por defecto).
- **Cookies**: `stateSaveCallback` / `stateLoadCallback` guardan JSON en `dt_<usuario>_<tableId>`, `path=/`, `SameSite=Lax`, ~1 año.
- **Parche ColReorder**: el plugin oficial hace `$.extend({}, init, defaults)` y el default `true` podía machacar `iFixedColumnsRight` del `init`. Se sustituye el `preInit.dt.colReorder` para fusionar **defaults → init** (el init gana) y luego `new ColReorder(...)`.
- **Resize de columnas**: handles en cabeceras, anchos en el mismo objeto de estado (`colWidths`). API pública `window.dtResetState(tableId)` borra la cookie de esa tabla para el usuario actual.

### Tablas con columna **Acciones** + ID oculto

Donde la penúltima columna es acciones y la última es `SysObjectID` u oculta, en el `DataTable({...})` conviene:

```js
"colReorder": { "iFixedColumnsRight": 2 }
```

Ejemplos ya ajustados: `Employee/Index`, `DesignTools/Index`, `Country/Index`, `MasterArticles/Index`, `TSql_Company/Index`.

### Lista intranet: columna única «Acciones»

- **Una columna**: Editar, Eliminar y acciones similares van en **una sola** columna DataTables (`rowActions` en el JSON del servidor o HTML equivalente en `columns.render`). No partir cada icono en columnas distintas.
- **Iconos agrupados con aire**: envolver los enlaces/botones en `d-inline-flex align-items-center gap-2` (o `gap-1` si el listado debe ir más compacto). **No usar `btn-group`** cuando los controles queden visualmente «pegados» sin espacio entre ellos (feedback recurrente): flex + `gap-*` mantiene el bloque cohesionado pero legible.
- **Alineación**: opcional `text-end` en el `<th>` «Acciones» y la misma clase en la celda (`className` de la columna) para alinear el bloque a la derecha.

**Referencia de implementación:** `LanguageController.ListLanguages` → propiedad `rowActions` en cada fila + `Views/Language/Index.cshtml` (columna `{ data: 'rowActions', … }`, cabecera `Acciones` con `text-end`).

```html
<div class="d-inline-flex align-items-center gap-2">
  <!-- enlaces/iconos Editar, Eliminar, … -->
</div>
```

### `fnRowCallback` con `$('td:eq(N)')`

Si ColReorder mueve columnas, **los índices DOM dejan de coincidir** con la columna lógica → iconos/textos en celdas equivocadas. Solución correcta: **`columns.render`** por columna (como en `Employee/Index` y `DesignTools/Index` para acciones/avatar). Mientras queden `td:eq`, no fiarse solo de `iFixedColumnsRight` en todas las pantallas.

### CSS DataTables

| Archivo | Rol |
|--------|-----|
| `Scripts/datatables-tandem.css` | Color texto `#4c4c4c`, tamaño base, cabeceras `th` primera letra mayúscula (sin forzar `tbody`), paginación/botones/responsive. Enlazado en `_LayoutMaterio` tras `datatables-resize.css`. |
| `Scripts/datatables-resize.css` | Handles de ancho en `thead th`. |

Vistas parciales que cargan DataTables **sin** layout completo enlazan también `datatables-tandem.css` y, si necesitan defaults de idioma/estado, `datatables-state.js` (p. ej. listas de materiales).

### Textos UI tablas

- Títulos de menú de botones unificados a español (`Registros`, `Exportar`, `Columnas visibles`, filas `10 filas` / `Todas`, etc.) en las vistas principales.
- Bloques `language: { ... }` duplicados eliminados donde el default de `datatables-state.js` basta.

---

## 4. Menú lateral: colapso en escritorio

- Solo **viewport ≥ 1200px** (el hamburger del navbar hace `preventDefault` y alterna clase en `<html>`).
- Clase Materio: `layout-menu-collapsed` en `document.documentElement`.
- **Persistencia**: cookie `tandem_menu_collapsed_<dt-user>`, 365 días, `path=/`, `SameSite=Lax`.
- **Migración una vez**: cookie global antigua `tandem_menu_collapsed` → cookie por usuario y se borra la global; si no, `localStorage` legacy `tandem-menu-collapsed` → cookie por usuario y se limpia `localStorage`.

---

## 5. Cromático / separadores (site.css + plantilla)

- **Pie**: `border-top` 1px color `var(--tandem-primary)` en `.layout-content-navbar .content-footer.footer`.
- **Navbar**: línea inferior 1px `rgba(var(--bs-base-color-rgb), 0.2)` como los separadores del menú.
- **Sidebar**: fondo `--tandem-chrome-bg` (color-mix body + heading) solo en `#layout-menu...`; la barra superior **no** lleva ese fondo (solo la línea inferior).

---

## 6. Checklist para un agente nuevo

1. Leer `_LayoutMaterio` para orden de scripts y `meta dt-user`.
2. Cualquier tabla nueva con ColReorder: o **`columns.render`** en columnas con HTML, o **`iFixedColumnsRight`** acorde al número de columnas fijas a la derecha.
3. No reintroducir `$.extend({}, init, defaults)` en ColReorder sin el parche de `datatables-state.js`.
4. Cookies de UI: prefijos `dt_` (tablas) y `tandem_menu_collapsed_` (menú); siempre coherente con `dt-user`.
5. No commitear de forma rutinaria bajo `Desing/No_Publicar_*` salvo petición explícita.

---

## 7. Archivos tocados recientemente (referencia rápida)

- Layout / parciales: `_LayoutMaterio`, `_SidebarMaterio`, `_NavbarMaterio`, `_PlantillaStyles`, `_FooterMaterio` (según estado git).
- Vistas DataTables: `Employee`, `DesignTools/Index`, `Country`, `MasterArticles`, `TSql_Company`, `Plantilla`, `HelpDesing/_MenuHelp`, `DesignTools/_MaterialList*`.
- JS/CSS: `Scripts/datatables-state.js`, `datatables-tandem.css`, `datatables-resize.css`, `assets/materio/css/site.css`.

---

*Última actualización del documento: alineada con los cambios de UI, DataTables y cookies descritos arriba.*
