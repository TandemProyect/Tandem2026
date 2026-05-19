/* =============================================================================
   Semilla: textos de la página Traducciones UI (módulo UiTranslation).

   Inserta filas en dbo.TSql_UiTranslation solo para el idioma por defecto
   (TSql_Language.Is_Default = 1). Otros idiomas: Excel import o pantalla futura.

   Requisitos: TSql_Language + TSql_UiTranslation ya creadas; AspNetUsers con al menos un Id.

   Idempotente por (TextResourceKey, LinkLanguage) con Is_Delete = 0.
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
BEGIN
    PRINT N'Aviso: no hay idioma por defecto activo — ejecute primero el script de TSql_Language.';
END
ELSE IF @User IS NULL
BEGIN
    PRINT N'Aviso: AspNetUsers vacío — no se puede cumplir LinkMadeBy; semilla omitida.';
END
ELSE IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
    PRINT N'Aviso: falta dbo.TSql_UiTranslation — ejecute 2026-05-17_create_TSql_UiTranslation.sql.';
ELSE
BEGIN
    DECLARE @Seed TABLE
    (
        TextResourceKey NVARCHAR(256) NOT NULL,
        TextValue       NVARCHAR(MAX) NOT NULL
    );

    INSERT INTO @Seed (TextResourceKey, TextValue)
    VALUES
        (N'UiTranslation.PageTitle', N'Traducciones UI'),
        (N'UiTranslation.BreadcrumbHome', N'Inicio'),
        (N'UiTranslation.BreadcrumbConfiguration', N'Configuración'),
        (N'UiTranslation.ExportHeading', N'Exportar'),
        (N'UiTranslation.ExportDescription', N'Descarga un .xlsx con TextResourceKey, TextModule, texto del idioma por defecto y una columna por cada idioma activo (TextCode).'),
        (N'UiTranslation.ExportButton', N'Descargar Excel'),
        (N'UiTranslation.ImportHeading', N'Importar'),
        (N'UiTranslation.ImportDescription', N'Sube un .xlsx con la columna TextResourceKey, opcionalmente TextModule e Is_Active, y columnas por idioma (mismo encabezado que el TextCode en BD).'),
        (N'UiTranslation.ImportButton', N'Importar'),
        (N'UiTranslation.ErrorReportButton', N'Descargar informe de errores (.xlsx)');

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
    SELECT S.TextResourceKey,
           N'UiTranslation',
           @Lang,
           S.TextValue,
           0,
           1,
           @User,
           NULL,
           GETDATE(),
           NULL,
           0
      FROM @Seed S
     WHERE NOT EXISTS (
               SELECT 1
                 FROM dbo.TSql_UiTranslation T
                WHERE T.TextResourceKey = S.TextResourceKey
                  AND T.LinkLanguage = @Lang
                  AND T.Is_Delete = 0
           );

    PRINT N'OK — semilla UiTranslation (filas nuevas según corresponda).';
END
GO
