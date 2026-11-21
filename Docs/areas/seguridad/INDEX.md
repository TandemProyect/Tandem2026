# Área: Derechos (roles y funciones)

**Cubre:** quién puede ver y usar cada menú, pantalla, acción MVC, botón y comando JS. Usuario → varios roles → varias funciones.

**No cubre:** filtrar datos por empresa u obra (fase pendiente), login y sesión (`Account`), autorización de equipos de los plugins (→ `cad-comun`).

## Regla en una línea

Un comando está protegido solo si hay una función **activa** con su clave en `TSql_Functions`. Sin función, lo usa cualquier usuario autenticado.

## Código (no abras más que esto)

- `Desing/Helpers/FunctionAuthorizeFilter.cs`: filtro global y `[RequireFunction]`.
- `Desing/Helpers/FunctionAccessService.cs`: `Can`, `GetDeniedCodes` y caché.
- `Desing/Helpers/FunctionHtmlExtensions.cs`: `Html.CanUse`, `Html.CanUseMenu`.
- `Desing/Helpers/FunctionCatalog.cs`: claves conocidas y constantes (`AiCode`).
- `Desing/Helpers/SecurityRepository.cs`: única vía para escribir roles y funciones.
- `Desing/Scripts/Intranet/tandem-functions.js` + `Views/Shared/_TandemFunctions.cshtml`: ocultar y bloquear en cliente.
- Pantallas: `RoleController`, `FunctionController`, `EmployeeController.AssignRoles` y sus vistas.

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Entender el modelo, las claves y dónde se comprueba | [`derechos.md`](derechos.md) |
| Proteger algo nuevo, crear una función o rol, asignar, probar | [`recetas.md`](recetas.md) |
| Saber qué funciones y roles existen hoy y qué falta | [`derechos.md`](derechos.md) § Estado |

## Reglas del área

- No compruebes roles a mano (`User.IsInRole`, listas de correos, flags en `TSql_Employee`). Usa una clave de función.
- Protege siempre en el **servidor**. Ocultar en la vista o en JS es solo para que no se vea.
- Escribe en las tablas de roles solo con `SecurityRepository` (invalida la caché) o con un script de semilla idempotente.
- Borrado lógico (`Is_Delete = 1`), nunca `DELETE`.
- Los roles se asignan solo desde **Empleados › Asignar roles**, no desde el formulario del rol.
- Las funciones de un rol se asignan solo desde **Roles › Asignar funciones** (dos tablas, arrastrar), no con checkboxes en editar.
- Textos visibles nuevos: `Desing/Scripts/ResourceGenerators/_gen_security_resources.js` (es + en) y regenera `Security.resx`.

## Código común

El filtro global se aplica a todos los controladores: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar el filtro, `FunctionAccessService` o `tandem-functions.js`.
