# Seguridad: roles y funciones

## Modelo

| Tabla | Qué guarda |
|-------|-----------|
| `TSql_Roles_V2` | Roles. `Is_System` = no se borra; `Is_AllFunctions` = concede todas las funciones. |
| `TSql_Functions` | Funciones. `TextCode` = clave del comando que protege; `TextModule` = grupo en pantalla. |
| `TSql_RoleFunction` | Funciones de cada rol (N:N). |
| `TSql_UserRole` | Roles de cada usuario (N:N, `LinkAspNetUsers`). Un usuario puede tener varios roles. |

Las tablas antiguas `TSql_Roles`, `TSql_RollFunctions`, `TSql_RollGrupDesing` y `TSql_RolesFuntionUser` quedan obsoletas: no se leen ni se borran.

Scripts (`Desing/Scripts/TemporalScript/`, idempotentes, en este orden):

1. `2026-11-16_create_TSql_Roles_V2.sql`
2. `2026-11-16_create_TSql_Functions.sql`
3. `2026-11-16_create_TSql_RoleFunction.sql`
4. `2026-11-16_create_TSql_UserRole.sql`
5. `2026-11-16_seed_TSql_Roles_V2_Functions.sql` — roles base, funciones `Role` y `Function` activas, `TSql_Company` y `Employee` inactivas, `juan.godoy@vscad.com` como «Administrador de sistema».
6. `2026-11-16_seed_TSql_UiTranslation_Common_menu_Security.sql` — textos del menú «Roles y funciones».

Mientras no se ejecuten, la aplicación funciona como antes (todo libre).

## Regla

- Un comando está **protegido** solo si existe una función **activa** con su `TextCode`.
- Si no existe (o está inactiva), **cualquier usuario** autenticado lo puede usar.
- Si está protegido, solo lo usan los usuarios con algún rol activo que tenga esa función, o un rol con `Is_AllFunctions`.

## Claves de comando

| Dónde | Clave | Ejemplo |
|-------|-------|---------|
| Controlador entero (y su línea de menú) | `Controlador` | `TSql_Company`, `Employee` |
| Acción concreta | `Controlador.Accion` | `Employee.Create_Employee` |
| Clave extra en código | `[RequireFunction("…")]` | varias acciones = un comando |
| Botón / elemento HTML | `data-function="…"` | `data-function="Desing2.Export"` |
| Herramienta Desing_2 | `Tool.<data-ma-stl-tool>` | `Tool.polyline`, `Tool.trim` |
| Modo / sistema / menú plugin | `Mode.*`, `System.*`, `Plugin.*` | `Mode.formwork`, `System.Atk-60` |

`Account` y `UiLanguage` nunca se protegen (entrar, salir, idioma).

## Dónde se aplica

- **Servidor:** `Helpers/FunctionAuthorizeFilter.cs` (filtro global en `FilterConfig`). Devuelve 403: vista `Views/Shared/AccessDenied.cshtml`, o JSON `{ IsOk:false, Message }` en AJAX/POST.
- **Menú:** `_SidebarMaterio.cshtml` y `_SidebarMenuItemGroup.cshtml` con `Html.CanUseMenu("Controlador")`. Las cabeceras se ocultan si no queda ninguna línea.
- **Vistas:** `Html.CanUse("clave")`.
- **JS:** `_TandemFunctions.cshtml` (en los layouts) pinta `window.TandemDeniedFunctions`; `Scripts/Intranet/tandem-functions.js` oculta y bloquea los elementos con esas claves. En código: `TandemFunctions.can('Tool.polyline')`.
- **Caché:** `Helpers/FunctionAccessService.cs` (1 min funciones protegidas, 5 min por usuario). Cualquier cambio desde `SecurityRepository` la invalida.

## Pantallas

- **Configuración › Roles y funciones › Roles** (`RoleController`): nombre, código, funciones (agrupadas por módulo) y usuarios.
- **Configuración › Roles y funciones › Funciones** (`FunctionController`): el campo «Comando» sugiere todas las claves conocidas (`Helpers/FunctionCatalog.cs`, por reflexión + comandos JS de Desing_2).
- **Empleados › ficha:** casillas de roles, solo para quien pueda usar `Role` (nadie se da roles a sí mismo).

## Al añadir un comando nuevo

No hay que hacer nada para que funcione: queda libre. Para poder protegerlo:

- Acción MVC → ya tiene clave `Controlador.Accion`.
- Botón JS → poner `data-function="Modulo.Comando"` (o usar `data-ma-stl-tool`).
- Atajo de teclado u otra ruta JS → `if (!TandemFunctions.can('Modulo.Comando')) return;`.

## Pendiente

- `DAL/Model.edmx`: «Update Model from Database» con las 4 tablas nuevas (hoy se accede por SQL directo).
- Filtrar datos por empresa (clientes, obras, diseños…) es otra fase.
