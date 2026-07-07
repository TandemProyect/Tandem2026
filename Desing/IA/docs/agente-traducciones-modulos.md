# Traducciones por módulo: patrón híbrido (mayo 2026)

Guía para agentes y desarrolladores que continúen la internacionalización sin romper el modelo. Ámbito: `Desing/` (ASP.NET MVC 5). Listados DataTables: [`agente-ui-materio-datatables.md`](agente-ui-materio-datatables.md).

**Alcance explícito:** la intranet y catálogos `TSql_*` descritos aquí. **Fuera de alcance de esta migración:** **DesignTools** (STL, herramientas de diseño) pasará a **otro proyecto/solución**; no añadir ni mover aquí recursos ni flujos i18n de DesignTools salvo decisión explícita del equipo.

---

## 1. Patrón híbrido (resumen)

| Capa | Rol |
|------|-----|
| **`.resx` embebidos** | Semilla y **fallback**: `Resources/<Módulo>.resx` + `<Módulo>.en.resx` + `<Módulo>.Designer.cs` (no editar a mano; ver generador). |
| **`DbBackedResourceManager`** | `ResourceManager` que **consulta primero** `TSql_UiTranslation` (por `TextModule` + idioma) y, si no hay fila, usa el `.resx`. Permite ajustar textos en caliente sin recompilar. |
| **`TSql_UiTranslation` + `/UiTranslation`** | Administración e import/export **Excel** (`Export` / `Import`) que upsertea filas con auditoría estándar. Tras import exitoso se llama **`DbBackedResourceManager.Invalidate()`** para limpiar la caché en memoria. |
| **Menú y textos 100 % BD** | Claves resueltas con `@Html.Ui(...)` / `LanguageUiHelper.GetUiStringWithFallback` sobre `TSql_UiTranslation`; seeds en `Scripts/TemporalScript/` (p. ej. `2026-05-17_seed_TSql_UiTranslation_*.sql`). |

**Traducción operativa:** el traductor puede **(A)** editar el Excel exportado y reimportar, **(B)** ajustar filas en BD, o **(C)** cambiar `.resx` vía `_gen_*_resources.js` y desplegar; si además existe fila en BD para esa clave+módulo+idioma, **prevalece la BD**.

---

## 2. Módulos `.resx` registrados (`ResxBackedModules`)

La lista está en `UiTranslationController`: controla qué ensamblados se mezclan en el Excel (filas faltantes) y el `ResourceManager` usado como fallback por `TextModule`.

Orden en código (verificar con búsqueda de `ResxBackedModules` si cambia):

| `TextModule` | Clase `Desing.Resources.*` |
|--------------|----------------------------|
| `Company` | `Company` |
| `Employee` | `Employee` |
| `Plantilla` | `Plantilla` |
| `ClientV2` | `ClientV2` |
| `MasterArticles` | `MasterArticles` |
| `Branch` | `Branch` |
| `Jobside` | `Jobside` |
| `DocumentType` | `DocumentType` |
| `Extension` | `Extension` |
| `Language` | `Language` |
| `Country` | `Country` |
| `Common` | `Common` |

**Nuevo módulo resx-backed:** además del generador y `.resx`, añadir **una tupla** `Tuple.Create("NombreModulo", typeof(Desing.Resources.NombreModulo))` a esta lista para que export/import y fallback sigan siendo coherentes.

---

## 3. Unicidad en BD: clave + idioma + módulo

El índice único antiguo `UX_TSql_UiTranslation_Key_Language_Active` era solo `(TextResourceKey, LinkLanguage)`. Eso choca cuando la misma clave existe en varios módulos (p. ej. `Btn_Save` en `ClientV2`, `MasterArticles` y `Common`).

**Estado deseado:** índice único filtrado `UX_TSql_UiTranslation_Key_Language_Module_Active` sobre `(TextResourceKey, LinkLanguage, TextModuleNorm)` con `TextModuleNorm` persistido = `ISNULL(TextModule, N'')` y `WHERE Is_Delete = 0`.

Script: `Scripts/TemporalScript/2026-05-17_alter_TSql_UiTranslation_unique_with_module.sql` (incluye notas de idempotencia y reemplazo del índice viejo). El controlador ya hace match por `(TextResourceKey, LinkLanguage, TextModule)` en upsert.

---

## 4. Menú, `@Html.Ui` y módulo `Common`

- **`@Html.Ui("Clave.Subclave", "Texto español por defecto", "Modulo")`** (`UiHtmlExtensions`) resuelve cadenas solo desde **BD** (`LanguageUiHelper` + `TSql_UiTranslation`). **No** mezclar esas claves en el `.resx` del CRUD salvo que sea el mismo módulo y esté decidido así.
- Ítems de **menú lateral / navegación** suelen vivir como seeds `Common` (u otro módulo UI), no como entradas duplicadas en cada `.resx` de negocio.
- Para botones y rótulos **compartidos** entre pantallas, preferir **`Common.*`** en `.resx` + registro en `ResxBackedModules` (ya incluido) y **una sola clave** reutilizable; evitar copiar la misma clave en muchos módulos.

---

## 5. Cultura UI (obligatorio conocerlo antes de tocar vistas)

| Pieza | Rol |
|-------|-----|
| `Global.asax.cs` → `Application_BeginRequest` | `LanguageUiHelper.ApplyCultureEarly`. |
| Cookies `tandem_lang` / `tandem_ui_culture` | Selección de idioma (incl. token `i:IdObject` hacia `TSql_language`). |
| **`BaseController.OnActionExecuting`** | Si la empresa del usuario tiene **`LinkLanguage`**, fija `HttpContext.Items` (`TandemCompanyLanguageId`, `TandemCompanyLanguageCode`, `TandemCompanyLanguageLocked`), **puede sobrescribir cookies** y aplicar cultura explícita — el selector manual queda **bloqueado** en ese caso. |
| `ViewBag` | `TandemUiCultureCode`, `TandemLanguageIdObject`, `TandemCompanyLanguageLocked` para navbar / partials. |
| `UiLanguageController` | Cambio manual respetando bloqueo por empresa. |

**Reglas:** no asumir español fijo en vistas migradas; nuevos idiomas `.resx` requieren `.<cultura>.resx` alineado con `TSql_language.TextCode`.

---

## 6. Generador `Scripts/ResourceGenerators/_gen_<módulo>_resources.js`

1. Duplicar un script existente en `Scripts/ResourceGenerators/` → `_gen_<modulo>_resources.js`; ajustar nombre de clase/archivos `.resx`.
2. Mantener **paridad `es` / `en`** en cada clave nueva.
3. Desde `Desing/`: `node Scripts/ResourceGenerators/_gen_<modulo>_resources.js`.
4. En `Design.csproj`: `EmbeddedResource` para ambos `.resx` y `Compile` del `Designer.cs` con `DependentUpon` / `AutoGen` como en `Company`.

**No editar a mano** `.resx` ni `Designer.cs` salvo emergencia; el generador es la fuente de verdad.

Convención de nombres de claves (igual que en plantillas existentes): prefijos `Index_`, `Page_`, `Col_`, `Lbl_`, `Btn_`, `Val_`, `Toast*`, `Msg_`, `Err_`, `Js_`, `Dt_`, `State_`, etc.

---

## 7. Vistas y controladores (recordatorio breve)

```cshtml
@using Desing.Resources
```

- Texto: `@NombreModulo.ClavePropiedad`.
- **JavaScript:** objeto serializado con `JsonConvert.SerializeObject` o atributos `data-i18n` en partials; scripts en `Scripts/Intranet/` sin literales de UI.

En **controlador**: validaciones `Val_*`, toasts, JSON AJAX — mismas clases `Desing.Resources.*`. Para listados DataTables, columnas, export y cookies `dt-user`, seguir [`agente-ui-materio-datatables.md`](agente-ui-materio-datatables.md).

---

## 8. Cuando creas o modificas tablas y formularios

Cada cambio de esquema o pantalla debe **cerrar el circuito i18n** (resx, BD opcional, menú si aplica). Usa esta lista como checklist.

### 8.1 Nueva tabla en SQL / EF (`TSql_*`)

- [ ] Decidir si los textos **visibles al usuario** salen del **catálogo en BD** (`TextLabel`, descripciones) o de **cadenas de UI** (etiquetas de formulario, mensajes). No mezclar responsabilidades: la UI del CRUD sigue en `.resx` (y opcionalmente en `TSql_UiTranslation`).
- [ ] Tras tocar modelo: regenerar/actualizar EDMX según proceso del equipo (`DAL/Model.edmx`).
- [ ] Si el nuevo CRUD expone pantallas: planificar módulo `.resx` (nuevo o existente) y claves para todos los estados (Index, Create, Edit, Details, errores AJAX).

### 8.2 Nuevo módulo MVC o pantalla grande

- [ ] Inventariar textos: HTML, placeholders, DataTables, alerts, JS, JSON, `ModelState`.
- [ ] Crear **`Scripts/ResourceGenerators/_gen_<modulo>_resources.js`**, ejecutar `node`, verificar entradas en **`Design.csproj`** (`EmbeddedResource` + `Compile` del Designer).
- [ ] Registrar **`Tuple` en `UiTranslationController.ResxBackedModules`** (nombre de módulo = `TextModule`).
- [ ] Migrar **controlador, vistas, ViewModels, JS** (`data-i18n` / serialización); eliminar cadenas hardcodeadas en código ya migrado.
- [ ] Donde aplique el estándar del proyecto, usar **`IntranetAuditHelper`** en create/update/delete lógico (`SetAuditOnCreate`, etc.).
- [ ] **Index / listados:** patrón DataTables del doc Materio (`applyListDefaults`, columnas, estado, export).
- [ ] Tras añadir claves: informar que el traductor puede usar **Excel en `/UiTranslation`** **o** **regenerar `.resx`** y desplegar; si hace falta valor sólo en BD, importar filas con **`TextModule`** correcto.
- [ ] **Evitar claves duplicadas entre módulos**; botones genéricos → **`Common.*`**.
- [ ] Tras **import masivo** o cambio manual amplio en `TSql_UiTranslation`, si algo no se refleja al instante: **`DbBackedResourceManager.Invalidate()`** (ya se invoca al final de import en `UiTranslationController`; si se escribe en BD por otro canal, considerar invalidar o reciclar app pool según operación).

### 8.3 Cambios puntuales en pantallas ya migradas

- [ ] **Index:** cabeceras `<th>`, serialización para `applyListDefaults`, `State_*` / `Dt_*` en `columns.render`, tooltips fila servidor si aplica.
- [ ] **Create / Edit:** `Lbl_*`, `Btn_*`, `Help_*`, `Val_*` en controlador para `ModelState`.
- [ ] **Details:** textos para valores vacíos (`Details_NoValue` o equivalente del módulo).
- [ ] **Parciales + JS externo:** mismo `@using` de recursos; `data-i18n` si el script está en `Scripts/Intranet/`.
- [ ] **Pruebas:** cookie idioma `en` (u otro activo) y escenario empresa con **`LinkLanguage`** bloqueado.

---

## 9. Qué NO hacer

- ❌ Cadenas en español fijo en vistas/controladores **ya migrados**.
- ❌ Editar `.resx` / `Designer.cs` sin generador.
- ❌ Clave solo en un idioma del par `es`/`en`.
- ❌ Duplicar claves de menú BD en `.resx` sin criterio (preferir una sola fuente).
- ❌ Ignorar `LinkLanguage` de empresa al probar selectores de idioma.
- ❌ Depender del índice único antiguo sin `TextModule` en entornos donde coexisten varios módulos resx-backed.

---

## 10. Módulos pendientes típicos

- **Manage / administración** u otros CRUD aún sin `_gen_*` ni tupla en `ResxBackedModules`.
- **Document / Offers** (u homónimos en solución): revisar si existen vistas legacy con literales.
- **Vistas legacy** fuera del layout Materio o sin DataTables unificado.
- **DesignTools:** quedará en **proyecto nuevo**; no incluir en esta guía salvo decisión de arquitectura; cualquier i18n allí se definirá en ese repo.

---

## 11. Referencias cruzadas

| Tema | Ubicación |
|------|-----------|
| DataTables, export, `dt-user`, ColReorder | [`agente-ui-materio-datatables.md`](agente-ui-materio-datatables.md) |
| Resolución BD + fallback | `Helpers/DbBackedResourceManager.cs`, `Helpers/LanguageUiHelper.cs`, `Helpers/UiHtmlExtensions.cs` |
| Import/export Excel | `Controllers/UiTranslationController.cs` |
| Auditoría estándar en controladores | `Helpers/IntranetAuditHelper.cs` |
| Seeds menú / UI | `Scripts/TemporalScript/2026-05-17_seed_TSql_UiTranslation_*.sql` |
| Ejemplo plantilla empresa | `Views/TSql_Company/*`, `Controllers/TSql_CompanyController.cs`, `Scripts/ResourceGenerators/_gen_company_resources.js` |

---

*Última revisión: mayo 2026 — patrón híbrido `.resx` + `DbBackedResourceManager` + `TSql_UiTranslation` / Excel; `ResxBackedModules` y unicidad por módulo documentados; DesignTools fuera de alcance.*
