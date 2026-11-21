# Derechos: modelo, claves y dónde se comprueban

## 1. Tablas (SQL directo, aún no están en `Model.edmx`)

```text
AspNetUsers ─< TSql_UserRole >─ TSql_Roles_V2 ─< TSql_RoleFunction >─ TSql_Functions
             (LinkAspNetUsers,   (TextCode,        (LinkRoles_V2,       (TextCode = clave,
              LinkRoles_V2)           Is_System,        LinkFunctions)       TextModule = grupo)
                                  Is_AllFunctions)
```

- Todas con las 9 columnas de auditoría; se filtra `Is_Delete = 0` y `Is_Active = 1`.
- `Is_System = 1`: el rol no se puede borrar. `Is_AllFunctions = 1`: concede todas las funciones.
- `TSql_Roles`, `TSql_RollFunctions`, `TSql_RollGrupDesing` y `TSql_RolesFuntionUser` son antiguas: no se leen.
- Si las tablas no existen (`FunctionAccessService.TablesExist`), todo queda libre.

## 2. Cómo se decide (`FunctionAccessService.Can(userId, clave)`)

1. Clave vacía → sí.
2. La clave no es una función activa → sí (libre).
3. Usuario sin identificar → no.
4. El usuario tiene algún rol activo con `Is_AllFunctions` o con esa función → sí. Si no, no.

Caché: funciones protegidas 1 min y acceso por usuario 5 min. `SecurityRepository` llama a `FunctionAccessService.InvalidateAll()` en cada cambio. Un `UPDATE` hecho a mano en la BD tarda hasta 5 min en notarse, o hasta reiniciar IIS.

## 3. Claves

| Dónde | Clave | Ejemplo |
|-------|-------|---------|
| Controlador entero (y su línea de menú) | `Controlador` | `TSql_Company`, `Employee` |
| Una acción | `Controlador.Accion` | `Employee.Create_Employee` |
| Varias acciones = un comando | `[RequireFunction("…")]` | `IA` en `Desing_2/Ai*` |
| Botón o elemento HTML | `data-function="…"` | `data-function="IA"` |
| Herramienta de Desing_2 | `Tool.<data-ma-stl-tool>` | `Tool.polyline` |
| Modo, sistema o menú de plugin | `Mode.*`, `System.*`, `Plugin.*` | `Mode.formwork`, `System.Atk-60` |

- El servidor comprueba **todas** las claves de la acción: `Controlador`, `Controlador.Accion` y cada `[RequireFunction]` (en la clase o en el método). Basta con que falle una.
- `Account` y `UiLanguage` no se protegen nunca.
- `FunctionCatalog.Entries` reúne por reflexión todas las claves MVC y `[RequireFunction]`, y añade las JS de Desing_2. Son las sugerencias del campo «Comando» en Funciones.

## 4. Dónde se aplica

| Capa | Archivo | Qué hace |
|------|---------|----------|
| Servidor | `FunctionAuthorizeFilter` (global en `App_Start/FilterConfig.cs`) | 403. En GET normal, vista `Views/Shared/AccessDenied.cshtml`. En AJAX, POST o `Accept: json`, `{ IsOk:false, Message, Function }`. |
| Menú | `_SidebarMaterio.cshtml`, `_SidebarMenuItemGroup.cshtml` | `Html.CanUseMenu("Controlador")`; las cabeceras sin hijos visibles se ocultan. |
| Vista Razor | cualquiera | `@if (Html.CanUse("clave")) { … }` |
| Cliente | `_TandemFunctions.cshtml` → `window.TandemDeniedFunctions`; `tandem-functions.js` | Oculta los `[data-function]` denegados (y `data-ma-stl-tool` → `Tool.*`) y bloquea click y submit. API: `TandemFunctions.can(c)`, `.canAll([..])`, `.denied`, `.apply()`. |

El cliente se carga en `_LayoutMaterio`, `_LayoutMaterioBlank`, `_LayoutAutocadPalette`, `_LayoutZwcadEmbed` y `DesignTools/Design.cshtml`. En otra página, `window.TandemFunctions` no existe: usa `!window.TandemFunctions || TandemFunctions.can(c)` y confía en el servidor.

## 5. Pantallas

- **Configuración › Roles y funciones › Funciones** (`FunctionController`, clave `Function`): alta, edición, activar/desactivar y en qué roles está.
- **… › Roles** (`RoleController`, clave `Role`): nombre, código y funciones agrupadas por `TextModule`. Aquí no se asignan usuarios.
- **Empleados › escudo › Asignar roles** (`Employee/AssignRoles/{userId}`, `[RequireFunction("Role")]`): tabla de asignados a la izquierda y de disponibles a la derecha. Se arrastra o se usa la flecha, y `AddUserRole` / `RemoveUserRole` lo guardan al momento. `RemoveUserRole` deshace el cambio si el usuario se quita a sí mismo el acceso a `Role`.

## 6. Estado (18/11/2026)

| Función | Activa | Protege |
|---------|--------|---------|
| `Role` | sí | Roles y Asignar roles |
| `Function` | sí | Funciones |
| `IA` | sí | Botón IA de `_Desing2ModeToolBar.cshtml`, pestaña «Asistente IA» de `wall-edit-form.js` y del muro curvo (`master-article-details-stl-viewer.js`), endpoints `Desing_2/AiCommands`, `SaveAiUserSettings`, `SaveAiUnknownPhrase`, `AiUnknownPhrases`, `ResolveAiUnknownPhrase` |
| `IA.TraduccionSimultanea` | sí | Botón «Traducción simultánea» del chat IA (`wall-ai-assistant.js`) |
| `DevelopAgent` | sí | Menú Develop › Agentes y `DevelopAgentController` |
| `TSql_Company` | no | Empresas (se activa en Funciones) |
| `Employee` | no | Empleados |

| Rol | Funciones |
|-----|-----------|
| `SYSADMIN` Administrador de sistema | todas (`Is_AllFunctions`, `Is_System`) |
| `ADMINISTRATIVE` Administrativo | `Employee`, `IA`, `IA.TraduccionSimultanea` |
| `SECTION_HEAD` Jefe de Sección, `DESIGNER` Proyectista | `IA`, `IA.TraduccionSimultanea` |
| `AI_ASSISTANT` Asistente IA | `IA`, `IA.TraduccionSimultanea` |
| `DEVELOP` Develop | todas (`Is_AllFunctions`); también `DevelopAgent` y `DevelopAi` |

`juan.godoy@vscad.com` tiene los seis roles en develop local y en producción.

Scripts en `Desing/Scripts/TemporalScript/`, todos idempotentes y en este orden: `2026-11-16_create_TSql_Roles_V2`, `_create_TSql_Functions`, `_create_TSql_RoleFunction`, `_create_TSql_UserRole`, `_seed_TSql_Roles_V2_Functions`, `_seed_TSql_UiTranslation_Common_menu_Security`, `_seed_TSql_Functions_IA`, `2026-11-17_seed_TSql_Functions_DevelopAgent`, `2026-11-18_seed_TSql_Functions_DevelopAi`, `2026-11-18_sync_rights_and_defaults`, `2026-11-20_seed_TSql_Functions_IA_TraduccionSimultanea`. Ejecutados en producción y en el SQL Express local; develop no es accesible desde fuera.

**Pendiente:**
- «Update Model from Database» en `DAL/Model.edmx` con las 4 tablas.
- Derechos por empresa o ámbito de datos.
- Dar claves a los comandos de Desing_2 que aún no las usan.
