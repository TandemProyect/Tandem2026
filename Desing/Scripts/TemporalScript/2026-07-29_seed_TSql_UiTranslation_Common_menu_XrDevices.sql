/* =============================================================================
   Semilla: menú Configuración → Dispositivos XR (TextModule = Common).
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

IF @Lang IS NULL
    PRINT N'Aviso: sin idioma por defecto — ejecute create_TSql_Language.';
ELSE IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
    PRINT N'Aviso: falta TSql_UiTranslation.';
ELSE
BEGIN
    IF NOT EXISTS (
            SELECT 1
              FROM dbo.TSql_UiTranslation T
             WHERE T.TextResourceKey = N'Menu.XrDevices'
               AND T.LinkLanguage = @Lang
               AND T.Is_Delete = 0
        )
    BEGIN
        INSERT INTO dbo.TSql_UiTranslation
        (
            TextResourceKey,
            TextModule,
            LinkLanguage,
            TextValue,
            Is_Delete,
            Is_Active,
            LinkMadeBy,
            LinModifiedBy,
            AddDateMade,
            AddLastDateChange,
            Ntimeschanged
        )
        VALUES
        (
            N'Menu.XrDevices',
            N'Common',
            @Lang,
            N'Dispositivos XR',
            0,
            1,
            @User,
            NULL,
            GETDATE(),
            NULL,
            0
        );

        PRINT N'OK — insertado Menu.XrDevices (es, idioma por defecto).';
    END
    ELSE
        PRINT N'OK — Menu.XrDevices ya existía; sin cambios.';
END
GO
