# Documentación Intranet — T Desing.net

Guías mantenidas en el repositorio para **desarrolladores** y **agentes Cursor**. Léelas **antes** de crear o modificar pantallas CRUD de Intranet.

## Índice

| Documento | Contenido |
|-----------|-----------|
| [UI-Formularios-y-Estilo.md](./UI-Formularios-y-Estilo.md) | Estructura MVC, tablas DataTables, formularios, plantilla de color, checkboxes, acciones, convenciones de campos |
| [intranet-scroll-layout.md](./intranet-scroll-layout.md) | Scroll único en `.tandem-layout-main-scroll`: cadena flex, `:has(...)` para DT / Jobside / Desing_2 |
| [Google-Places-Direcciones.md](./Google-Places-Direcciones.md) | Bloque reutilizable de dirección, `TandemAddressPlaces`, Web.config, Google Cloud, Jobside |
| [../../docs/agente-ui-materio-datatables.md](../../docs/agente-ui-materio-datatables.md) | Layout Materio, cookies `dt-user`, DataTables globales, menú colapsado |
| [../../Scripts/ThreejsDesing/README.md](../../Scripts/ThreejsDesing/README.md) | Visor STL Three.js (Master Articles) — **no** duplicar aquí |

## Regla Cursor para agentes

- **Archivo:** `.cursor/rules/intranet-ui-forms.mdc`
- Se aplica al trabajar en formularios y CRUD de Intranet bajo `Desing/`.

## Cómo administrar esta documentación

1. **Ubicación canónica:** `Desing/Docs/Intranet/` (este índice y los `.md` enlazados).
2. **Al cambiar convenciones de UI o Google Places**, actualizar el `.md` correspondiente en el mismo PR que el código.
3. **Al añadir un módulo CRUD de referencia**, mencionarlo en `UI-Formularios-y-Estilo.md` (sección «Referencias»).
4. **Agentes:** leer este README → el doc específico → la regla `.cursor/rules/intranet-ui-forms.mdc` → si la pantalla usa tablas Materio, también `docs/agente-ui-materio-datatables.md`.

## Módulos de referencia actuales

- **Clientes V2:** `ClientV2Controller`, `Views/ClientV2/*` — CRUD sin dirección Google.
- **Obras (Jobside):** `JobsideController`, `Views/Jobside/*` — CRUD con doble dirección (Loc/Bill) y Google Places.

## Configuración (intranet)

Menú lateral: sección **CONFIGURACIÓN** en `_SidebarMaterio.cshtml`.

| Módulo | Ruta | Controlador | Tabla SQL |
|--------|------|-------------|-----------|
| Tipos de documento | `/DocumentType/Index` | `DocumentTypeController` | `dbo.TSql_DocumentType` |

Scripts SQL (SSMS, en orden):

1. `Desing/Scripts/TemporalScript/2026-05-16_create_TSql_DocumentType.sql` — tabla + FK auditoría → `AspNetUsers`
2. Si la base tenía la columna antigua `NumberMaxFileSizeBytes` en `TSql_DocumentType`: `2026-05-17_alter_TSql_DocumentType_drop_NumberMaxFileSizeBytes.sql`

Patrón CRUD: igual que ClientV2 (sin Google Places). **Obligatorio:** `TextLabel`. Opcionales: `TextCode`, `TextDescription`. El **tamaño máximo por fichero** se define en el catálogo **Extensiones** (`dbo.TSql_Extension.NumberMaxFileSizeBytes`). Details incluye parcial `_IntranetAuditSection` (enlaces a empleado/usuario Windows).

---

*Última revisión: mayo 2026 — alineado con ClientV2, Jobside, DocumentType y bloque `_GooglePlacesAddressBlock`.*
