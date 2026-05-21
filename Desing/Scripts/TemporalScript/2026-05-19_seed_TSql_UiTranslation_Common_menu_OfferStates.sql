/* =============================================================================
   Semilla: entrada de menú Configuración → Estados de oferta (TextModule = Common).

   Idioma: solo filas para TSql_Language.Is_Default = 1 (resto vía Excel).

   Ejecutar manualmente contra la BD intranet tras desplegar, si la fila no existe.
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
             WHERE T.TextResourceKey = N'Menu.OfferStates'
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
            N'Menu.OfferStates',
            N'Common',
            @Lang,
            N'Estados de oferta',
            0,
            1,
            @User,
            NULL,
            GETDATE(),
            NULL,
            0
        );

        PRINT N'OK — insertado Menu.OfferStates (es, idioma por defecto).';
    END
    ELSE
        PRINT N'OK — Menu.OfferStates ya existía; sin cambios.';
END
GO
