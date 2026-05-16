/* =============================================================================
   dbo.TSql_Extension — columna NumberMaxFileSizeBytes (tamaño máximo por extensión)
   Idempotente. Ejecutar si la tabla ya existía sin esta columna.
   Valor por defecto: 10485760 bytes (10 MB).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Extension', N'NumberMaxFileSizeBytes') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Extension
        ADD NumberMaxFileSizeBytes BIGINT NOT NULL
            CONSTRAINT DF_TSql_Extension_NumberMaxFileSizeBytes DEFAULT (10485760);
    PRINT N'OK — Añadida columna NumberMaxFileSizeBytes (default 10 MB).';
END
ELSE IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
    PRINT N'OK — NumberMaxFileSizeBytes ya existe.';
ELSE
    PRINT N'Aviso: ejecute antes 2026-05-16_create_TSql_Extension.sql';
GO
