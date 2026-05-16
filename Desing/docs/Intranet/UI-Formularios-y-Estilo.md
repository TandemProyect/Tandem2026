# Formularios Intranet y estándares de estilo

Guía para desarrolladores y agentes Cursor al crear o modificar pantallas CRUD en **Intranet** (`Desing/`, layout Materio).

**Relacionado:** [README](./README.md) · [Google-Places-Direcciones.md](./Google-Places-Direcciones.md) · [agente-ui-materio-datatables.md](../../docs/agente-ui-materio-datatables.md) · [ThreejsDesing/README.md](../../Scripts/ThreejsDesing/README.md) (visor 3D, fuera de Intranet CRUD).

---

## 1. Arquitectura MVC de un módulo Intranet

Patrón usado en **ClientV2**, **Jobside** y **DocumentType** (referencia para nuevos módulos):

| Pieza | Ubicación | Responsabilidad |
|-------|-----------|-----------------|
| Entidad EF | `DAL/TSql_*.cs` + EDMX | Tabla SQL, propiedades |
| Controlador | `Desing/Controllers/*Controller.cs` | Index, Details, Create, Edit, List* (JSON DataTables), Delete* (soft) |
| Vistas | `Desing/Views/<Controller>/` | `Index`, `Create`, `Edit`, `Details`, `_<Modulo>FormFields.cshtml` |
| Menú | `Views/Shared/_SidebarMaterio.cshtml` | Enlace con `activeClass("NombreControlador", "")` |
| Proyecto | `Design.csproj` | `<Compile>` del controlador/modelos; `<Content>` de vistas y scripts |

### Acciones estándar del controlador

- **Index:** vista vacía + DataTable en `@section scripts`.
- **Details(long id):** lectura; filtrar `!Is_Delete`.
- **Create GET:** modelo por defecto (`Is_Active = true`, etc.).
- **Create POST:** `[ValidateAntiForgeryToken]`, `[Bind(Include = ...)]`, validación, auditoría en insert, `TempData` toast, redirect a Index.
- **Edit GET/POST:** igual con `IdObject` en Bind del POST.
- **List*:** `JsonResult` + `DataTablesBinder`, proyección a DTO/anónimo, HTML en columnas de acciones.
- **Delete*:** `Is_Delete = true` (no borrado físico), actualizar auditoría.

### Layout

Las vistas Intranet usan el layout Materio (vía `_ViewStart` o equivalente del área). El color de marca llega por `ViewBag.PlantillaColor` y el parcial **`_PlantillaStyles.cshtml`** (incluido en `_LayoutMaterio.cshtml`).

---

## 2. Campos de entidad y auditoría

Convención habitual en tablas `TSql_*` de Intranet:

| Campo | Uso |
|-------|-----|
| `IdObject` | PK `long`; en listados DataTables: columna **oculta** (`visible: false`) |
| `TextLabel` | Nombre visible principal; enlace a Details en la columna de listado |
| `Is_Active` | Booleano; badge «Activo» / «Inactivo» en listado |
| `Is_Delete` | Soft delete; **nunca** mostrar en formulario; filtrar `!Is_Delete` en consultas |
| `LinkMadeBy`, `LinModifiedBy`, `AddChangeBy` | Usuario (`GetUserId()`) |
| `AddDateMade`, `AddLastDateChange` | Fechas |
| `Ntimeschanged` | Contador de ediciones |

En **Create POST**, asignar auditoría de alta y `Is_Delete = false`. En **Edit** y **Delete**, usar un método tipo `ApplyAuditOnEdit`.

En **Details**, mostrar bloque «Seguridad y auditoría» con `@Html.Partial("_IntranetAuditSection", auditModel)` (`IntranetAuditHelper.BuildDisplay`). Los usuarios con ficha en `TSql_Employee` enlazan a `Employee/Edit_Employee`.

---

## 3. Vista Index — DataTables

Estructura de página:

```html
<div class="d-flex align-items-center justify-content-between mb-4">
    <h4 class="py-3 mb-0">
        <span class="text-muted fw-light"><a href="@Url.Action("Index", "Home")">Inicio</a> / Intranet /</span> Título módulo
    </h4>
    <a class="btn btn-primary" href="@Url.Action("Create")">
        <i class="icon-base ri ri-add-line me-1"></i> Nuevo …
    </a>
</div>
```

Tarjeta con tabla `table table-hover`, `id` único (`ListClientV2`, `ListJobside`, …).

### Columnas fijas recomendadas

Orden típico (ajustar columnas de negocio al inicio):

1. Campos de negocio (`TextLabel`, relaciones, etc.)
2. `activeBadge` — HTML generado en servidor
3. `buttonEdit` (+ opcional `buttonDelete`) — `orderable: false`, `searchable: false`
4. **`IdObject`** — `visible: false` (ordenación/filtro interno)

Ejemplo (Jobside):

```javascript
columns: [
    { data: 'TextLabel' },
    { data: 'ClientName' },
    { data: 'Loc_Formatted_Address' },
    { data: 'activeBadge', orderable: false },
    { data: 'buttonEdit', orderable: false, searchable: false },
    { data: 'IdObject', visible: false }
]
```

Opciones habituales: `serverSide: true`, `processing: true`, `ajax` POST a `List*`. Para tablas con ColReorder y estado por usuario, ver [agente-ui-materio-datatables.md](../../docs/agente-ui-materio-datatables.md).

### Listados DataTables (exportación estándar)

Todos los **Index** con DataTables deben usar el mismo menú que **Empleados** (`Employee/Index`):

| Sección del menú | Contenido |
|------------------|-----------|
| **Registros** | Mostrar N filas (`pageLength`: 10 / 25 / 50 / Todas) |
| **Exportar** | Imprimir, Copiar, PDF, CSV, Excel |
| **Columnas visibles** | Selector `colvis` |

Scripts y estilos ya están en **`_LayoutMaterio.cshtml`** (Buttons, JSZip, pdfmake, `datatables-tandem.css`). Helper compartido: **`Scripts/Intranet/intranet-datatables-list.js`** (expone `TandemDataTablesList`).

**Snippet para un Index nuevo** (sustituir `ListMiModulo` y la URL `List*`):

```javascript
$(document).ready(function () {
    $('#ListMiModulo').DataTable(TandemDataTablesList.applyListDefaults({
        serverSide: true,
        processing: true,
        ajax: { url: '@Url.Action("ListMiModulo")', type: 'POST' },
        columns: [
            { data: 'TextLabel', name: 'TextLabel' },
            { data: 'activeBadge', name: 'Is_Active', orderable: false },
            { data: 'buttonEdit', orderable: false, searchable: false },
            { data: 'IdObject', visible: false }
        ]
    }));
});
```

Opciones extra del módulo (p. ej. `colReorder`, `scrollX`) se pasan en el mismo objeto; `applyListDefaults` añade `dom`, `lengthMenu`, `buttons` y `stateSave`.

Icono del menú: por defecto Font Awesome (`fa-bars`, como Empleados). Para iconos Materio (p. ej. Artículos): `buttonsOptions: { icon: 'materio' }` dentro de `applyListDefaults` — ver implementación en `intranet-datatables-list.js`.

Etiquetas del desplegable (mismo criterio que Empleados): `Registros`, `Exportar`, `Columnas visibles`. Los botones de exportación usan los textos i18n de DataTables (p. ej. «Imprimir», «Copiar» según idioma del plugin).

**Módulos Intranet con exportación estándar:** ClientV2, Jobside, DocumentType. Referencia histórica con configuración inline: Employee, Plantilla, TSql_Company, MasterArticles.

---

## 4. Vistas Create / Edit — formulario

### Contenedor

```html
<div class="card mb-4">
    <div class="card-body">
        @using (Html.BeginForm("Create", "MiModulo", FormMethod.Post, new { @class = "row g-3" }))
        {
            @Html.AntiForgeryToken()
            @Html.Partial("_MiModuloFormFields", Model)
            <div class="col-12 d-flex justify-content-end gap-2 tandem-form-actions mt-2">
                <a href="@Url.Action("Index")" class="btn btn-outline-secondary">Cancelar</a>
                <button type="submit" class="btn btn-primary">Guardar</button>
            </div>
        }
    </div>
</div>
```

- Clase del formulario: **`row g-3`** (grid Bootstrap).
- Pie de acciones: **`tandem-form-actions`** + `justify-content-end` (también forzado globalmente en `_PlantillaStyles.cshtml`).

### Cabecera de página

Misma línea que Index: breadcrumb `Inicio / Intranet / … / Crear|Editar`.

---

## 5. Estándares de campos (`_*FormFields.cshtml`)

### Etiquetas — color plantilla

`_PlantillaStyles.cshtml` define:

```css
.form-label,
.col-form-label,
label.control-label {
    color: var(--tandem-primary);
    font-weight: 500;
}
```

Usar siempre **`class="form-label"`** en `<label>` (no depender del gris por defecto de Materio).

### Controles

| Tipo | Clases |
|------|--------|
| Texto | `form-control` + `maxlength` acorde a BD |
| Select FK | `form-select` + `DropDownListFor` con `ViewBag` poblado en el controlador |
| Validación | `@Html.ValidationMessageFor(..., "", new { @class = "text-danger" })` |

### Prohibido: texto de ayuda gris bajo campos

**No** añadir:

- `<div class="form-text text-muted">…</div>`
- `<small class="text-muted">…</small>` bajo inputs
- `help-block` / hints decorativos

La UI debe quedar limpia: etiqueta + control + solo mensaje de validación en rojo si aplica. Excepciones: **alertas** de configuración (p. ej. Google Maps sin clave) o **avisos de error** del bloque de dirección (`alert-warning`), no ayuda por campo.

### Checkboxes (`Is_Active`, flags)

Patrón **ClientV2** / **Jobside**:

```html
<div class="col-md-3">
    <label class="form-label">Activo</label>
    <div class="form-check mt-2">
        @Html.CheckBoxFor(m => m.Is_Active, new { @class = "form-check-input" })
        <label class="form-check-label" for="Is_Active">Cliente activo</label>
    </div>
</div>
```

- Primera fila: `form-label` con el título del grupo (ej. «Activo»).
- Segunda fila: `form-check` con `mt-2` para alinear el switch/check con los inputs de texto.
- `form-check-label` describe el significado del check.

### Grid de columnas

- Campos anchos (nombre): `col-md-8` o `col-12`.
- FK / códigos: `col-md-4`, `col-md-3`, etc.
- Bloques de dirección Google: `col-12` (ver doc Google Places).

---

## 6. Menú lateral

En `Views/Shared/_SidebarMaterio.cshtml`.

**Enlace simple:** sustituir controlador, icono Remix Icon (`ri ri-*`) y texto.

**Submenú expandible:** no usar clase Materio `menu-toggle` (error `Menu._getItem` en hijos). Usar `Scripts/Intranet/sidebar-menu.js`, parcial `_SidebarMenuItemGroup.cshtml` y `SidebarMenuGroupModel`: atributos `data-menu-parent` / `data-menu-toggle`, clase `open` en servidor si la ruta pertenece al grupo. Flecha en `_PlantillaStyles` (`[data-menu-toggle]::after`). Ejemplo en `_SidebarMaterio` (Configuración → Tipos de documento). Registrar vista/JS en `Design.csproj`.

---

## 7. `[Bind]` y campos de dirección

En entidades con prefijos `Loc_` / `Bill_`, el `Bind` del POST debe listar **todos** los campos que envía el formulario (ver `JobsideBindFields` en `JobsideController.cs`).

Si hay checkbox «misma dirección facturación», en POST cuando está marcado copiar Loc → Bill en servidor (`CopyLocToBill`) **antes** de validar/guardar.

---

## 8. Checklist — nuevo CRUD Intranet

1. [ ] Entidad/DAL + script SQL temporal si aplica
2. [ ] Controlador: Index, Details, Create, Edit, List*, Delete* (soft), auditoría
3. [ ] Vistas + `_ModuloFormFields.cshtml`
4. [ ] Entrada en `_SidebarMaterio.cshtml` (enlace simple o `_SidebarMenuItemGroup` si va bajo Configuración u otro grupo)
5. [ ] `Design.csproj`: controlador, vistas, scripts/CSS nuevos
6. [ ] Index DataTables: `IdObject` oculto, `activeBadge`, botones acción
7. [ ] Formulario: `row g-3`, `form-label`, `tandem-form-actions`, sin `form-text text-muted`
8. [ ] Checkboxes con patrón `form-check mt-2`
9. [ ] Si hay dirección: leer [Google-Places-Direcciones.md](./Google-Places-Direcciones.md)
10. [ ] Si hay tabla con ColReorder compleja: leer [agente-ui-materio-datatables.md](../../docs/agente-ui-materio-datatables.md)

---

## 9. Referencias en código

| Módulo | Formulario | Listado |
|--------|------------|---------|
| Clientes V2 | `Views/ClientV2/_ClientV2FormFields.cshtml` | `Views/ClientV2/Index.cshtml` |
| Obras | `Views/Jobside/_JobsideFormFields.cshtml` | `Views/Jobside/Index.cshtml` |

Estilos globales formulario/plantilla: `Views/Shared/_PlantillaStyles.cshtml`.

---

*Mantener este documento alineado con `_PlantillaStyles` y las vistas de referencia.*
