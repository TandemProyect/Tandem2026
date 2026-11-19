# Base de datos: registros que van a develop y a producción

Ficha completa para el agente de BD. No hace falta leer más para crear tablas o copiar registros entre develop y producción.

## 1. Bases

| Entorno | Servidor | Base | Acceso |
|---------|----------|------|--------|
| **Develop local** (donde trabaja Juan; la usa la web en localhost) | `NUCBOXG5\SQLEXPRESS` | `TdesingDevelop` | Windows, `-E` |
| Producción remota | `SQL5113.site4now.net` | `db_a197cd_desingproducction` | usuario SQL, en el `Web.config` de Juan (comentado) |
| Develop remota | `SQL5085.site4now.net` | `db_a197cd_desingdevelop` | la contraseña de `DAL/App.config` ya no vale (16/11/2026) |

- En local solo hay `TdesingDevelop` (17/11/2026: renombrada desde `db_a197cd_desingproducction`; las demás bases locales se borraron).
- «Las dos bases» = `TdesingDevelop` (local) y producción remota. Lo que va a producción se aplica con un script, no con consultas entre bases.
- Los servidores remotos se tocan **solo si Juan lo pide en ese momento**. Nunca escribas contraseñas en documentos, scripts ni commits.
- Desde Git Bash, `-S .\SQLEXPRESS` falla: usa `-S 'NUCBOXG5\SQLEXPRESS'`.

```bash
sqlcmd -S 'NUCBOXG5\SQLEXPRESS' -E -d <base> -I -f 65001 -b -i "<ruta absoluta>.sql"
```

## 2. Qué va a cada base

| Tipo | Develop | Producción |
|------|---------|------------|
| **Esquema** (tablas, columnas, índices, FK) | sí | sí, siempre el mismo script |
| **Usuarios del sistema `*@tediseng.net`** (`AspNetUsers` + `TSql_Employee` + `TSql_UserRole`) | sí | sí |
| **Derechos:** `TSql_Functions`, `TSql_Roles_V2`, `TSql_RoleFunction` | sí | sí |
| **Catálogos:** `TSql_UiTranslation`, `TSql_Language`, `TSql_Countrys`, `TSql_OfferState`, `TSql_Extension`, `TSql_DocumentType`, `TSql_Plantilla`, `TSql_AiCommand*`, `TSql_AiEnvironment*`, `TSql_FormworkPanelColor` | sí | sí |
| **Empresas** con usuarios del sistema (`TSql_Company`) | sí | sí |
| **Negocio y pruebas:** diseños, ofertas, obras, clientes, artículos de prueba, usuarios de prueba | sí | **no** |
| **Oferta al cliente** (`TSql_ClientOffer`, `TSql_ClientOfferBrand`, `TSql_ClientOfferSection`, `TSql_ClientOfferAiMessage`, `TSql_ClientOfferContact`, `TSql_ClientOfferContactStateType`, `TSql_ClientOfferContactState`, `TSql_ClientOfferCapture`): el esquema irá a producción cuando Juan lo pida; las filas son negocio y no se copian | sí | esquema pendiente |

Si una tabla no está en la lista, pregunta a Juan y añádela aquí.

## 3. Reglas para copiar

1. **No copies Ids de identidad** (`IdObject`, `SysObjectID`): son distintos en cada base. Cruza por **clave natural**:
   - Funciones y roles: `TextCode`.
   - Usuarios: `UserName`.
   - Empresas: `TextLabel`.
   - Traducciones: la clave de texto de cada fila.
2. Las FK se resuelven en la base de destino buscando por la clave natural, por ejemplo `LinkRoles_V2 = (SELECT IdObject FROM dbo.TSql_Roles_V2 WHERE TextCode = N'…' AND Is_Delete = 0)`.
3. **Excepción, `AspNetUsers.Id`:** es un GUID en texto. Cópialo igual, junto con `PasswordHash` y `SecurityStamp`, para que el usuario tenga el mismo Id y la misma contraseña en las dos bases.
4. Todo va en un script idempotente (`IF NOT EXISTS` por clave natural), sin `DELETE`. Para quitar, usa borrado lógico (`Is_Delete = 1`, o `AttIsDeleted` / `BitIsDeleted` en tablas antiguas).
5. Orden: `TdesingDevelop` → (si Juan lo pide) producción remota. Ejecuta el script dos veces: la segunda no debe añadir nada.
6. Cambio de esquema: copia `.bak` (`BACKUP DATABASE … TO DISK … WITH COPY_ONLY`) de las dos bases antes. Tablas nuevas: `.cursor/rules/sql-tsql-table-conventions.mdc`. Después, Juan hace «Update Model from Database» en `DAL/Model.edmx`.
7. No hagas commit ni push. Deja los scripts en `Desing/Scripts/TemporalScript/` sin confirmar.

## 4. Nombre y forma del script

`Desing/Scripts/TemporalScript/AAAA-MM-DD_sync_<tema>.sql`, por ejemplo `2026-11-17_sync_usuarios_tediseng.sql`.

```sql
SET NOCOUNT ON; SET XACT_ABORT ON;
DECLARE @By NVARCHAR(128) = (SELECT Id FROM dbo.AspNetUsers WHERE UserName = N'juan.godoy@vscad.com');
-- Un bloque IF NOT EXISTS (… clave natural …) INSERT … por fila o por tabla.
PRINT N'OK — <tema> sincronizado (o ya existía).';
```

## 5. Receta: usuario del sistema `@tediseng.net`

El usuario se da de alta en develop con la web: **Empleados › Nuevo**. Después:

1. **Ver qué falta en producción:** lanza en cada base la lista y compara por `UserName`:
```sql
SELECT UserName FROM dbo.AspNetUsers WHERE UserName LIKE N'%@tediseng.net' ORDER BY UserName;
```
2. **`AspNetUsers`:** todas las columnas, con el mismo `Id`.
3. **`TSql_Employee`:** se enlaza con el usuario por `LinAspNetUsert`. No copies `SysObjectID`.
   - `LinCompany`: el `SysObjectID` de la empresa con el mismo `TextLabel` en destino. Si no existe, crea antes la empresa.
   - `LinCreatedBy` / `LinModifiedBy`: `@By`.
   - `Linlanguage`: el idioma con el mismo código en destino.
   - Resto (`AttName`, `AttSurname`, `LinBusiness`, `AttIsDeleted`, `AttPassAspNetUsert`, …): tal cual.
4. **`TSql_UserRole`:** un rol por fila, por `TextCode`, con `LinkAspNetUsers = Id`.
5. **Comprobar** en las dos bases:
```sql
SELECT u.UserName, e.AttName, c.TextLabel AS Empresa, r.TextCode AS Rol
  FROM dbo.AspNetUsers u
  LEFT JOIN dbo.TSql_Employee e ON e.LinAspNetUsert = u.Id AND e.AttIsDeleted = 0
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = e.LinCompany
  LEFT JOIN dbo.TSql_UserRole ur ON ur.LinkAspNetUsers = u.Id AND ur.Is_Delete = 0
  LEFT JOIN dbo.TSql_Roles_V2 r ON r.IdObject = ur.LinkRoles_V2
 WHERE u.UserName LIKE N'%@tediseng.net';
```

Otras tablas que apuntan a `AspNetUsers` (auditoría `LinkMadeBy`, `LinModifiedBy`, …) no se copian: en destino usan `@By`.

## 6. Receta: catálogo o derechos nuevos

1. Comparar por clave natural, lanzando la misma consulta en cada base, por ejemplo:
```sql
SELECT TextCode FROM dbo.TSql_Functions WHERE Is_Delete = 0 ORDER BY TextCode;
```
2. Script de §4 con un `IF NOT EXISTS … INSERT` por fila. Para las tablas de relación (`TSql_RoleFunction`, `TSql_UserRole`), resuelve los dos Ids por `TextCode` / `UserName`. Modelo: `2026-11-16_seed_TSql_Functions_IA.sql`.

## 7. Al terminar

- Cuéntale a Juan qué filas has añadido y en qué base. Sin listas largas.
- Si has decidido una tabla nueva para §2, añádela a la tabla. No hagas crecer esta ficha con el historial: va a `historial/AAAA-MM-DD-<tema>.md`.
