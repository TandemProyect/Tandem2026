/* =============================================================================
   Semilla: apartado del menú «Últimos diseños» (TextModule = Common).
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
    PRINT N'Aviso: semilla Menu.LastDesigns omitida.';
ELSE IF NOT EXISTS (
    SELECT 1
      FROM dbo.TSql_UiTranslation T
     WHERE T.TextResourceKey = N'Menu.LastDesigns'
       AND T.LinkLanguage = @Lang
       AND T.Is_Delete = 0
)
BEGIN
    INSERT INTO dbo.TSql_UiTranslation
    (
        TextResourceKey, TextModule, LinkLanguage, TextValue,
        Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy,
        AddDateMade, AddLastDateChange, Ntimeschanged
    )
    VALUES
    (
        N'Menu.LastDesigns', N'Common', @Lang, N'Últimos diseños',
        0, 1, @User, NULL,
        GETDATE(), NULL, 0
    );
END

IF @Lang IS NOT NULL AND @User IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.TSql_UiTranslation T
     WHERE T.TextResourceKey = N'Menu.LastDesignsDoubleClick'
       AND T.LinkLanguage = @Lang AND T.Is_Delete = 0
)
    INSERT INTO dbo.TSql_UiTranslation
    (TextResourceKey, TextModule, LinkLanguage, TextValue, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES (N'Menu.LastDesignsDoubleClick', N'Common', @Lang, N'Doble click para abrir', 0, 1, @User, NULL, GETDATE(), NULL, 0);

IF @Lang IS NOT NULL AND @User IS NOT NULL AND OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.TSql_UiTranslation T
     WHERE T.TextResourceKey = N'Menu.OpeningDesign'
       AND T.LinkLanguage = @Lang AND T.Is_Delete = 0
)
    INSERT INTO dbo.TSql_UiTranslation
    (TextResourceKey, TextModule, LinkLanguage, TextValue, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES (N'Menu.OpeningDesign', N'Common', @Lang, N'Abriendo diseño', 0, 1, @User, NULL, GETDATE(), NULL, 0);
GO

PRINT N'OK — Menu.LastDesigns (o ya existía).';
GO
