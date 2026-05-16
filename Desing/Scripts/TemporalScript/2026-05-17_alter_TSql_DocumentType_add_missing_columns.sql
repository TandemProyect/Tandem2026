/* =============================================================================
   dbo.TSql_DocumentType — añade columnas ausentes en BD legada (EF / scripts repo).

   Definición canónica de negocio y tipos:
     - 2026-05-16_create_TSql_DocumentType.sql

   El tamaño máximo por fichero está en dbo.TSql_Extension (NumberMaxFileSizeBytes).

   Auditoría AddLastDateChange: tras 2026-05-16_align_TSql_audit_columns_to_rule.sql
   debe ser DATETIME NULL sin DEFAULT (solo se rellena en UPDATE).

   Idempotente: COL_LENGTH(...) por columna.
   Preferencia: solo ALTER ADD (sin DROP).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_DocumentType no existe; ejecute antes el script de creación.';
END
ELSE
BEGIN
    /* ----- AddLastDateChange (alineado con script de auditoría: NULL, sin DEFAULT) ----- */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'AddLastDateChange') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD AddLastDateChange DATETIME NULL;
        PRINT N'OK — Añadida columna AddLastDateChange (DATETIME NULL, sin DEFAULT).';
    END
    ELSE
        PRINT N'OK — AddLastDateChange ya existe.';

    /* ----- Negocio ----- */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'TextCode') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD TextCode NVARCHAR(50) NULL;
        PRINT N'OK — Añadida columna TextCode.';
    END
    ELSE
        PRINT N'OK — TextCode ya existe.';

    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'TextDescription') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD TextDescription NVARCHAR(500) NULL;
        PRINT N'OK — Añadida columna TextDescription.';
    END
    ELSE
        PRINT N'OK — TextDescription ya existe.';
END
GO

PRINT N'OK — dbo.TSql_DocumentType: revisión AddLastDateChange / TextCode / TextDescription completada.';
GO
