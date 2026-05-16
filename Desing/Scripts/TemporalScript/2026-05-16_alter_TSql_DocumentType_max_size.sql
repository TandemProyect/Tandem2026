/* =============================================================================
   OBSOLETO — El tamaño máximo ya no vive en TSql_DocumentType.

   No ejecutar en bases nuevas. Para quitar la columna si ya se añadió antes:
     Desing/Scripts/TemporalScript/2026-05-17_alter_TSql_DocumentType_drop_NumberMaxFileSizeBytes.sql

   --- texto histórico (solo referencia) ---
   dbo.TSql_DocumentType — columna NumberMaxFileSizeBytes (tamaño máximo por tipo)
   Idempotente. Ejecutar si la tabla ya existía sin esta columna.
   Valor por defecto: 10485760 bytes (10 MB).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_DocumentType', N'NumberMaxFileSizeBytes') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_DocumentType
        ADD NumberMaxFileSizeBytes BIGINT NOT NULL
            CONSTRAINT DF_TSql_DocumentType_NumberMaxFileSizeBytes DEFAULT (10485760);
    PRINT N'OK — Añadida columna NumberMaxFileSizeBytes (default 10 MB).';
END
ELSE IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL
    PRINT N'OK — NumberMaxFileSizeBytes ya existe.';
ELSE
    PRINT N'Aviso: ejecute antes 2026-05-16_create_TSql_DocumentType.sql';
GO
