/* Dev/manual: ejecutar contra la BD después de pull si aparece SqlException Invalid column TextContractRef / TextJobsideNotes / AddNJobside.
   =============================================================================
   dbo.TSql_Jobside — AddNJobside (nullable hasta post-SaveChanges), TextContractRef
   opcional en UI (NULL en BD), TextJobsideNotes opcional. Idempotente.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
BEGIN
    /* ----- AddNJobside: ADD si falta + NULL hasta asignación en app ----- */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddNJobside') IS NULL
        ALTER TABLE dbo.TSql_Jobside ADD AddNJobside NVARCHAR(50) NULL;
    ELSE IF EXISTS (
            SELECT 1
            FROM sys.columns c
            WHERE c.object_id = OBJECT_ID(N'dbo.TSql_Jobside')
              AND c.name = N'AddNJobside'
              AND c.is_nullable = 0
        )
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN AddNJobside NVARCHAR(50) NULL;

    /* ----- TextContractRef: opcional en formulario ----- */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'TextContractRef') IS NULL
        ALTER TABLE dbo.TSql_Jobside ADD TextContractRef NVARCHAR(500) NULL;
    ELSE IF EXISTS (
            SELECT 1
            FROM sys.columns c
            WHERE c.object_id = OBJECT_ID(N'dbo.TSql_Jobside')
              AND c.name = N'TextContractRef'
              AND c.is_nullable = 0
        )
            ALTER TABLE dbo.TSql_Jobside ALTER COLUMN TextContractRef NVARCHAR(500) NULL;

    /* ----- TextJobsideNotes ----- */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'TextJobsideNotes') IS NULL
        ALTER TABLE dbo.TSql_Jobside ADD TextJobsideNotes NVARCHAR(500) NULL;
END
GO

PRINT N'OK — dbo.TSql_Jobside: TextContractRef, TextJobsideNotes y AddNJobside alineados (NULL permitido donde aplica).';
GO
