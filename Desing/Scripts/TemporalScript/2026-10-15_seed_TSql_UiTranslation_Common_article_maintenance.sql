/* =============================================================================
   Semilla: aviso de mantenimiento del recuento (TextModule = Common).
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
    PRINT N'Aviso: sin idioma, usuario o TSql_UiTranslation — semilla omitida.';
ELSE
BEGIN
    DECLARE @Seed TABLE
    (
        TextResourceKey NVARCHAR(256) NOT NULL,
        TextValue       NVARCHAR(MAX) NOT NULL
    );

    INSERT INTO @Seed (TextResourceKey, TextValue)
    VALUES
        (N'Common.ArticleMaintenanceTitle', N'Un momento. Mantenimiento'),
        (N'Common.ArticleMaintenanceSeconds', N'Tardará unos {0} segundos.'),
        (N'Common.ArticleMaintenanceCalculating', N'Calculando el tiempo…');

    INSERT INTO dbo.TSql_UiTranslation
    (
        TextResourceKey, TextModule, LinkLanguage, TextValue,
        Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy,
        AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT S.TextResourceKey, N'Common', @Lang, S.TextValue,
           0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Seed S
     WHERE NOT EXISTS (
               SELECT 1
                 FROM dbo.TSql_UiTranslation T
                WHERE T.TextResourceKey = S.TextResourceKey
                  AND T.LinkLanguage = @Lang
                  AND T.Is_Delete = 0
           );

    PRINT N'OK — semilla Common aviso de mantenimiento.';
END
GO
