/* =============================================================================
   dbo.TSql_UiTranslation — Cadenas de UI multilenguaje (origen SQL; futura pantalla + Excel)

   TextResourceKey: clave estable (ej. Common.Save, Company.Branch.New).
   TextModule: agrupa por módulo / hoja Excel futura (opcional).
   LinkLanguage → PK de dbo.TSql_Language (IdObject en tablas nuevas; SysObjectID u otra en legado).
   Unicidad lógica: (TextResourceKey, LinkLanguage) con Is_Delete = 0.

   Plantilla Excel sugerida (columnas): TextModule | TextResourceKey | TextCode | TextValue
     TextCode debe coincidir con TSql_Language.TextCode (ej. es, en).

   Tras ejecutar: ejecutar antes 2026-05-17_create_TSql_Language.sql; luego Update Model from Database.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_UiTranslation
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_UiTranslation PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        TextResourceKey NVARCHAR(256) NOT NULL,
        TextModule      NVARCHAR(100) NULL,
        LinkLanguage    BIGINT NOT NULL,
        TextValue       NVARCHAR(MAX) NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_UiTranslation_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_UiTranslation_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_UiTranslation_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_UiTranslation_Ntimeschanged DEFAULT (0)
    );
END
GO

/* FK a idioma: la tabla legada puede usar SysObjectID (u otra PK), no siempre IdObject */
IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
         FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_TSql_UiTranslation_TSql_Language'
          AND fk.parent_object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
   )
BEGIN
    DECLARE @LangPkCol SYSNAME;

    SELECT @LangPkCol = c.name
      FROM sys.key_constraints kc
      INNER JOIN sys.index_columns ic
              ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
      INNER JOIN sys.columns c
              ON c.object_id = ic.object_id AND c.column_id = ic.column_id
     WHERE kc.parent_object_id = OBJECT_ID(N'dbo.TSql_Language')
       AND kc.type = N'PK'
       AND ic.key_ordinal = 1;

    IF @LangPkCol IS NOT NULL
    BEGIN
        DECLARE @FkLang NVARCHAR(MAX) =
              N'ALTER TABLE dbo.TSql_UiTranslation WITH CHECK ADD CONSTRAINT FK_TSql_UiTranslation_TSql_Language '
            + N'FOREIGN KEY (LinkLanguage) REFERENCES dbo.TSql_Language (' + QUOTENAME(@LangPkCol) + N');';

        BEGIN TRY
            EXEC sp_executesql @FkLang;
        END TRY
        BEGIN CATCH
            PRINT N'Aviso: FK_TSql_UiTranslation_TSql_Language no creada — ' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
        PRINT N'Aviso: no se encontró PK en dbo.TSql_Language — FK LinkLanguage omitida.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_UiTranslation_Key_Language_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_UiTranslation_Key_Language_Active
        ON dbo.TSql_UiTranslation (TextResourceKey, LinkLanguage)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_UiTranslation_Lookup'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
    CREATE NONCLUSTERED INDEX IX_TSql_UiTranslation_Lookup
        ON dbo.TSql_UiTranslation (TextResourceKey, LinkLanguage)
        INCLUDE (TextValue, TextModule)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_UiTranslation_Module_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
    CREATE NONCLUSTERED INDEX IX_TSql_UiTranslation_Module_Active
        ON dbo.TSql_UiTranslation (TextModule, TextResourceKey)
        WHERE Is_Delete = 0 AND TextModule IS NOT NULL;
GO

/* FK auditoría → AspNetUsers */
IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.AspNetUsers)
BEGIN
    DECLARE @FallbackUserId NVARCHAR(128);
    SELECT TOP (1) @FallbackUserId = Id FROM dbo.AspNetUsers ORDER BY Id;

    UPDATE T
       SET LinkMadeBy = COALESCE(NULLIF(LTRIM(RTRIM(T.LinkMadeBy)), N''), @FallbackUserId),
           AddDateMade = COALESCE(T.AddDateMade, GETDATE()),
           Ntimeschanged = COALESCE(T.Ntimeschanged, 0)
      FROM dbo.TSql_UiTranslation T
     WHERE T.LinkMadeBy IS NULL OR T.AddDateMade IS NULL OR T.Ntimeschanged IS NULL;

    UPDATE T SET LinkMadeBy = @FallbackUserId
      FROM dbo.TSql_UiTranslation T
     WHERE T.LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_UiTranslation_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_UiTranslation'))
        ALTER TABLE dbo.TSql_UiTranslation ADD CONSTRAINT FK_TSql_UiTranslation_AspNetUsers
            FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_UiTranslation_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_UiTranslation'))
        ALTER TABLE dbo.TSql_UiTranslation ADD CONSTRAINT FK_TSql_UiTranslation_AspNetUsers1
            FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
END
ELSE IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NOT NULL
    PRINT N'Aviso: FK auditoría TSql_UiTranslation omitidas — dbo.AspNetUsers vacía o inexistente.';
GO

PRINT N'OK — dbo.TSql_UiTranslation creada (o ya existía).';
GO
