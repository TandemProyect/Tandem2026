/* =============================================================================
   Función «DevelopAi» (menú Develop › IA y controlador DevelopAi),
   asignada al rol «Develop» (DEVELOP) y texto de menú Menu.DevelopAi.
   Requiere las tablas de roles y funciones (2026-11-16_*). Idempotente.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
    COALESCE((SELECT TOP (1) u.Id FROM dbo.AspNetUsers u WHERE u.UserName = N'juan.godoy@vscad.com'),
             (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id), N'system');

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode = N'DEVELOP')
    INSERT INTO dbo.TSql_Roles_V2
        (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Develop', N'DEVELOP', N'Desarrollo: menú Develop (documentos para agentes y catálogo IA).', 0, 0,
         0, 1, @User, NULL, GETDATE(), NULL, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND TextCode = N'DevelopAi')
    INSERT INTO dbo.TSql_Functions
        (TextLabel, TextCode, TextModule, TextDescription,
         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES
        (N'Develop › IA', N'DevelopAi', N'Develop',
         N'Menú Develop › IA y catálogo de funciones, órdenes y significado (TSql_AiCommand).',
         0, 1, @User, NULL, GETDATE(), NULL, 0);

INSERT INTO dbo.TSql_RoleFunction
    (TextLabel, LinkRoles_V2, LinkFunctions,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
       0, 1, @User, NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_Roles_V2 r
  JOIN dbo.TSql_Functions f ON f.TextCode = N'DevelopAi' AND f.Is_Delete = 0
 WHERE r.TextCode = N'DEVELOP' AND r.Is_Delete = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_RoleFunction x
                    WHERE x.Is_Delete = 0 AND x.LinkRoles_V2 = r.IdObject AND x.LinkFunctions = f.IdObject);

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

IF @Lang IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.TSql_UiTranslation T
                    WHERE T.TextResourceKey = N'Menu.DevelopAi' AND T.LinkLanguage = @Lang AND T.Is_Delete = 0)
        INSERT INTO dbo.TSql_UiTranslation
            (TextResourceKey, TextModule, LinkLanguage, TextValue,
             Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
        VALUES
            (N'Menu.DevelopAi', N'Common', @Lang, N'IA',
             0, 1, @User, NULL, GETDATE(), NULL, 0);
END
GO

PRINT N'OK — función DevelopAi asignada al rol Develop y menú IA sembrados (o ya existían).';
GO
