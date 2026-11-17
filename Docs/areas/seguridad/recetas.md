# Derechos: recetas

Cada receta es completa: no hace falta leer más. El modelo está en [`derechos.md`](derechos.md).

## A. Proteger una pantalla o controlador entero

1. Código: nada. La clave `NombreControlador` ya existe.
2. Menú: que la línea esté dentro de `@if (Html.CanUseMenu("NombreControlador"))` o en un `SidebarMenuGroupModel`, que ya filtra.
3. Activar: crea la función con `TextCode = NombreControlador` en **Funciones** o con una semilla (receta F) y añádela a un rol.

## B. Proteger una sola acción

La clave `Controlador.Accion` ya existe: créala como función (receta F). Si la acción tiene `[ActionName("X")]`, la clave es `Controlador.X`.

## C. Un comando que abarca varias acciones o controladores

```csharp
// Helpers/FunctionCatalog.cs
public const string MiCode = "Modulo.Comando";

// En cada acción (o en la clase)
[RequireFunction(FunctionCatalog.MiCode)]
public ActionResult Accion(...) { ... }
```

`FunctionCatalog` lo descubre por reflexión. Si el comando vive solo en JS, añade la entrada a mano como `AiCode` (`list.Add(new Entry { Code = …, Module = JsModule })`). Ejemplo real: `IA` en `Desing_2Controller.AiCommands.cs` y `.AiUnknownPhrases.cs`.

## D. Botón en una vista Razor

```cshtml
@if (Html.CanUse(Desing.Helpers.FunctionCatalog.MiCode))
{
    <button data-function="Modulo.Comando" ...>...</button>
}
```

Haz las dos cosas: `CanUse` no pinta el botón y `data-function` lo oculta y bloquea si la página se cachea. Ejemplo: botón IA en `Views/Desing_2/_Desing2ModeToolBar.cshtml`.

## E. Botón, pestaña o atajo creado en JS

```javascript
const allowed = !window.TandemFunctions || window.TandemFunctions.can('Modulo.Comando');
if (allowed) {
    btn.setAttribute('data-function', 'Modulo.Comando');
    parent.appendChild(btn);
}
// Atajo de teclado u otra ruta sin botón:
if (window.TandemFunctions && !window.TandemFunctions.can('Modulo.Comando')) return;
```

- No inicialices paneles que el usuario no puede usar.
- Si hay pestañas, redirige a la pestaña por defecto cuando la pedida no está permitida (ver `showTab` en `wall-edit-form.js`).
- El endpoint que use el botón debe llevar `[RequireFunction]` (receta C).
- Después: `node --check` y sube `?v=` donde se importe el archivo.

## F. Crear la función (y el rol) en la BD

Copia `Desing/Scripts/TemporalScript/2026-11-16_seed_TSql_Functions_IA.sql` como `AAAA-MM-DD_seed_TSql_Functions_<Clave>.sql`. Tiene cuatro bloques `IF NOT EXISTS`, cada uno opcional:

1. `TSql_Functions` (`TextLabel`, `TextCode`, `TextModule`, `TextDescription`, `Is_Active`).
2. `TSql_Roles_V2`, si hace falta un rol nuevo.
3. `TSql_RoleFunction`.
4. `TSql_UserRole`, si se pide asignar a alguien.

Ejecútalo dos veces en una BD de prueba para comprobar que no duplica. Después ejecútalo en local, develop y producción según `AGENTS.md`:

```powershell
sqlcmd -S .\SQLEXPRESS -E -d db_a197cd_desingproducction -I -f 65001 -b -i "<ruta absoluta>.sql"
```

**Ojo:** activar una función deja fuera a todos los que no tengan un rol con ella, salvo `SYSADMIN`. Antes de activarla, pregunta a qué roles se da.

## G. Asignar o quitar roles a un usuario

- Desde la UI: **Empleados › escudo › Asignar roles**.
- Desde código: `SecurityRepository.AddUserRole(db, userId, roleId, byUserId)` y `RemoveUserRole(...)`. `RemoveUserRole` hace borrado lógico.
- No crees otros sitios donde asignar roles.

## H. Texto visible nuevo (mensajes, etiquetas)

Añádelo en `Desing/Scripts/ResourceGenerators/_gen_security_resources.js` (es y en) y ejecútalo con node. Regenera `Security.resx`, `Security.en.resx` y `Security.Designer.cs`. Las líneas de menú van además en `TSql_UiTranslation` con un script de semilla.

## I. Probar

1. Usuario con un rol que **no** tiene la función: no ve el menú ni el botón, la URL directa da la página «Acceso denegado» (403) y el AJAX da `{ IsOk:false, Function:"…" }`.
2. Añade la función a su rol: espera menos de 5 min o haz el cambio en la pantalla, que invalida la caché. Ahora lo ve.
3. Desactiva la función en **Funciones**: vuelve a estar libre para todos.

Comprobación SQL:

```sql
SELECT u.UserName, r.TextCode AS Rol, f.TextCode AS Funcion, f.Is_Active
  FROM dbo.TSql_UserRole ur
  JOIN dbo.AspNetUsers u ON u.Id = ur.LinkAspNetUsers
  JOIN dbo.TSql_Roles_V2 r ON r.IdObject = ur.LinkRoles_V2 AND r.Is_Delete = 0
  LEFT JOIN dbo.TSql_RoleFunction rf ON rf.LinkRoles_V2 = r.IdObject AND rf.Is_Delete = 0
  LEFT JOIN dbo.TSql_Functions f ON f.IdObject = rf.LinkFunctions AND f.Is_Delete = 0
 WHERE ur.Is_Delete = 0 AND u.UserName = N'<correo>';
```

## Al terminar

Actualiza la tabla «Estado» de [`derechos.md`](derechos.md) si has creado o activado funciones o roles. No hagas crecer las fichas sin necesidad.
