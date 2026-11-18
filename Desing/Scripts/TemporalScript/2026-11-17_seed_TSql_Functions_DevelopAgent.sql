/* =============================================================================
   Función «DevelopAgent» (menú Develop › Agentes y controlador DevelopAgent),
   rol «Develop» (DEVELOP; reutiliza el «Develop» sin código si ya existe) con esa función, asignado a juan.godoy@vscad.com,
   y textos del menú (Menu.Develop, Menu.DevelopAgents; TextModule = Common, idioma por defecto).
   Requiere las tablas de roles y funciones (2026-11-16_*). Idempotente.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'DevelopAgent')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Develop › Agentes', N'DevelopAgent', N'Develop',
         N'Menú Develop › Agentes y lista de documentos para agentes (TSql_DevelodAgentDocument).',
         0, 1, @User, NULL, GETDATE(), NULL, 0);

/* Si ya existe un rol «Develop» creado a mano sin código, se reutiliza y se le pone DEVELOP. */
IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode = N'DEVELOP')
    WITH r AS (
        SELECT TOP (1) * FROM dbo.TSql_Roles_V2
         WHERE Is_Delete = 0 AND NULLIF(LTRIM(RTRIM(TextCode)), N'') IS NULL AND TextLabel = N'Develop'
         ORDER BY IdObject
    )
    UPDATE r SET TextCode = N'DEVELOP', LinModifiedBy = @User, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1;

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode = N'DEVELOP')
    INSERT INTO dbo.TSql_Roles_V2
        (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Develop', N'DEVELOP', N'Desarrollo: menú Develop (documentos para agentes).', 0, 0,
         0, 1, @User, NULL, GETDATE(), NULL, 0);

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_Roles_V2 r
  JOIN dbo.TSql_Functions f ON f.TextCode = N'DevelopAgent' AND f.Is_Delete = 0
 WHERE r.TextCode = N'DEVELOP' AND r.Is_Delete = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

INSERT INTO dbo.TSql_UserRole
    (TextLabel, LinkAspNetUsers, LinkRoles_V2,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(u.UserName + N' · ' + r.TextLabel, 500), u.Id, r.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.AspNetUsers u
  JOIN dbo.TSql_Roles_V2 r ON r.TextCode = N'DEVELOP' AND r.Is_Delete = 0
 WHERE u.UserName = N'juan.godoy@vscad.com'
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_UserRole x
                    WHERE x.Is_Delete = 0 AND x.LinkAspNetUsers = u.Id AND x.LinkRoles_V2 = r.IdObject);

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

IF @Lang IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
BEGIN
    DECLARE @Keys TABLE (TextResourceKey NVARCHAR(200), TextValue NVARCHAR(500));
    INSERT INTO @Keys VALUES
        (N'Menu.Develop',       N'Develop'),
        (N'Menu.DevelopAgents', N'Agentes');

    INSERT INTO dbo.TSql_UiTranslation
        (TextResourceKey, TextModule, LinkLanguage, TextValue,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    SELECT k.TextResourceKey, N'Common', @Lang, k.TextValue,
           0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Keys k
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_UiTranslation T
                        WHERE T.TextResourceKey = k.TextResourceKey AND T.LinkLanguage = @Lang AND T.Is_Delete = 0);
END
GO

PRINT N'OK — función DevelopAgent, rol «Develop» y menú Develop sembrados (o ya existían).';
GO
