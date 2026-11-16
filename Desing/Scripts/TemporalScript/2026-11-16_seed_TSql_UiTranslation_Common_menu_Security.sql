/* =============================================================================
   Semilla: menú «Seguridad» (Roles / Funciones) (TextModule = Common).
   Idioma: solo TSql_Language.Is_Default = 1.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @Lang BIGINT =
(
    SELECT TOP (1) l.IdObject
      FROM dbo.TSql_Language l
     WHERE l.Is_Default = 1 AND l.Is_Delete = 0 AND l.Is_Active = 1
     ORDER BY l.IdObject
);

DECLARE @User NVARCHAR(128) =
(
    SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id
);

IF @Lang IS NULL OR @User IS NULL OR OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
BEGIN
    PRINT N'Aviso: semilla Menu.Security omitida.';
END
ELSE
BEGIN
    DECLARE @Keys TABLE (TextResourceKey NVARCHAR(200), TextValue NVARCHAR(500));
    INSERT INTO @Keys VALUES
        (N'Menu.Security',  N'Seguridad'),
        (N'Menu.Roles',     N'Roles'),
        (N'Menu.Functions', N'Funciones');

    INSERT INTO dbo.TSql_UiTranslation
    (
        TextResourceKey, TextModule, LinkLanguage, TextValue,
        Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy,
        AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT k.TextResourceKey, N'Common', @Lang, k.TextValue,
           0, 1, @User, NULL,
           GETDATE(), NULL, 0
      FROM @Keys k
     WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TSql_UiTranslation T
         WHERE T.TextResourceKey = k.TextResourceKey
           AND T.LinkLanguage = @Lang
           AND T.Is_Delete = 0
     );
END
GO

PRINT N'OK — menú Seguridad sembrado (o ya existía).';
GO
