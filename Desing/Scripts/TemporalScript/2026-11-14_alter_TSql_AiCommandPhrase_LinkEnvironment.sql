/* =============================================================================
   Frases ligadas a un entorno de conversación (pt107u): columna LinkEnvironment (BIGINT NULL → TSql_AiEnvironment)
   en dbo.TSql_AiCommandPhrase y dbo.TSql_AiCommandUserPhrase. NULL = la frase vale en todos los entornos
   (las 989 de la primera semilla quedan NULL).
   La unicidad no cambia: una frase keyword/example sigue sin poder activar dos comandos del mismo contexto e
   idioma, sea del entorno que sea (así una frase no cambia de significado al cambiar de entorno).
   Requiere antes: create TSql_AiCommandPhrase, TSql_AiCommandUserPhrase y TSql_AiEnvironment.
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_AiCommandPhrase', N'LinkEnvironment') IS NULL
    ALTER TABLE dbo.TSql_AiCommandPhrase ADD LinkEnvironment BIGINT NULL;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandPhrase_TSql_AiEnvironment' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase'))
    ALTER TABLE dbo.TSql_AiCommandPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandPhrase_TSql_AiEnvironment
        FOREIGN KEY (LinkEnvironment) REFERENCES dbo.TSql_AiEnvironment (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandPhrase_Environment' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandPhrase_Environment
        ON dbo.TSql_AiCommandPhrase (LinkEnvironment, TextContext, TextLanguageCode)
        WHERE Is_Delete = 0 AND LinkEnvironment IS NOT NULL;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.TSql_AiCommandUserPhrase', N'LinkEnvironment') IS NULL
    ALTER TABLE dbo.TSql_AiCommandUserPhrase ADD LinkEnvironment BIGINT NULL;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_TSql_AiEnvironment' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_TSql_AiEnvironment
        FOREIGN KEY (LinkEnvironment) REFERENCES dbo.TSql_AiEnvironment (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandUserPhrase_Environment' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandUserPhrase_Environment
        ON dbo.TSql_AiCommandUserPhrase (LinkEnvironment, TextContext, TextLanguageCode)
        WHERE Is_Delete = 0 AND LinkEnvironment IS NOT NULL;
GO

PRINT N'OK — LinkEnvironment en TSql_AiCommandPhrase y TSql_AiCommandUserPhrase (o ya existía).';
GO
