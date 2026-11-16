/* =============================================================================
   Semilla de seguridad: roles, funciones base y primer administrador.
   Requiere: 2026-11-16_create_TSql_Roles_V2 / _Functions / _RoleFunction / _UserRole.
   Idempotente (NOT EXISTS por clave).

   Funciones que se crean:
     - Role      (Gestión de roles)      → ACTIVA: solo «Administrador de sistema».
     - Function  (Gestión de funciones)  → ACTIVA: solo «Administrador de sistema».
     - TSql_Company (Gestión de empresas) → INACTIVA (catálogo listo; activarla la protege).
     - Employee  (Gestión de usuarios)    → INACTIVA (catálogo listo; ya asignada a «Administrativo»).
   Una función inactiva no protege nada: el comando sigue libre para todos.

   Las tablas antiguas (TSql_Roles, TSql_RollFunctions, TSql_RollGrupDesing,
   TSql_RolesFuntionUser) quedan obsoletas; no se borran ni se leen.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Admin NVARCHAR(128) =
    (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com');
DECLARE @User NVARCHAR(128) =
    COALESCE(@Admin, (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

/* ----- Roles ----- */
DECLARE @Roles TABLE (TextCode NVARCHAR(100), TextLabel NVARCHAR(500), TextDescription NVARCHAR(2000), Is_System BIT, Is_AllFunctions BIT);
INSERT INTO @Roles VALUES
    (N'SYSADMIN',       N'Administrador de sistema', N'Todas las funciones del sistema. No se puede borrar.', 1, 1),
    (N'ADMINISTRATIVE', N'Administrativo',           N'Gestión administrativa (alta de usuarios, etc.).',     0, 0),
    (N'SECTION_HEAD',   N'Jefe de Sección',          N'Derechos de jefe de sección.',                         0, 0),
    (N'DESIGNER',       N'Proyectista',              N'Derechos de proyectista.',                             0, 0);

INSERT INTO dbo.TSql_Roles_V2
    (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT r.TextLabel, r.TextCode, r.TextDescription, r.Is_System, r.Is_AllFunctions,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM @Roles r
 WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 x
                    WHERE x.Is_Delete = 0 AND (x.TextCode = r.TextCode OR x.TextLabel = r.TextLabel));

/* ----- Funciones ----- */
DECLARE @Functions TABLE (TextCode NVARCHAR(200), TextLabel NVARCHAR(500), TextModule NVARCHAR(100), TextDescription NVARCHAR(2000), Is_Active BIT);
INSERT INTO @Functions VALUES
    (N'Role',         N'Gestión de roles',     N'Configuración',  N'Menú «Roles» y todas sus pantallas.',                    1),
    (N'Function',     N'Gestión de funciones', N'Configuración',  N'Menú «Funciones» y todas sus pantallas.',                1),
    (N'TSql_Company', N'Gestión de empresas',  N'Administración', N'Menú «Empresas» y todas sus pantallas.',                 0),
    (N'Employee',     N'Gestión de usuarios',  N'Administración', N'Menú «Empleados»: alta, edición y baja de usuarios.',    0);

INSERT INTO dbo.TSql_Functions
    (TextLabel, TextCode, TextModule, TextDescription,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT f.TextLabel, f.TextCode, f.TextModule, f.TextDescription,
       0, f.Is_Active, @User, NULL, GETDATE(), NULL, 0
  FROM @Functions f
 WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions x WHERE x.Is_Delete = 0 AND x.TextCode = f.TextCode);

/* ----- Funciones por rol ----- */
DECLARE @RoleFunctions TABLE (RoleCode NVARCHAR(100), FunctionCode NVARCHAR(200));
INSERT INTO @RoleFunctions VALUES
    (N'ADMINISTRATIVE', N'Employee');

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM @RoleFunctions rf
  JOIN dbo.TSql_Roles_V2 r ON r.TextCode = rf.RoleCode AND r.Is_Delete = 0
  JOIN dbo.TSql_Functions f ON f.TextCode = rf.FunctionCode AND f.Is_Delete = 0
 WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

/* ----- Primer administrador ----- */
IF @Admin IS NOT NULL
    INSERT INTO dbo.TSql_UserRole
        (TextLabel, LinkAspNetUsers, LinkRoles_V2,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    SELECT LEFT(u.UserName + N' · ' + r.TextLabel, 500), u.Id, r.IdObject,
           0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM dbo.AspNetUsers u
      JOIN dbo.TSql_Roles_V2 r ON r.TextCode = N'SYSADMIN' AND r.Is_Delete = 0
     WHERE u.Id = @Admin
       AND NOT EXISTS (SELECT 1 FROM dbo.TSql_UserRole x
                        WHERE x.Is_Delete = 0 AND x.LinkAspNetUsers = u.Id AND x.LinkRoles_V2 = r.IdObject);
ELSE
    PRINT N'Aviso: no existe juan.godoy@vscad.com; asignar «Administrador de sistema» a mano.';
GO

PRINT N'OK — semilla de roles y funciones aplicada (o ya existía).';
GO
