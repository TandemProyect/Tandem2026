/* =============================================================================
   dbo.TSql_DocumentType — LinModifiedBy nullable (INSERT sin modificación previa)

   Convención .cursor/rules/sql-tsql-table-conventions.mdc:
   LinModifiedBy NULL en INSERT; se rellena solo en UPDATE.

   Si la tabla se creó con script antiguo (LinModifiedBy NOT NULL), Entity Framework
   falla al crear: "Cannot insert the value NULL into column 'LinModifiedBy'".

   Idempotente.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_DocumentType no existe.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_DocumentType', N'LinModifiedBy') IS NULL
BEGIN
    PRINT N'Aviso: columna LinModifiedBy no existe — ejecutar antes los CREATE/ALTER base.';
END
ELSE IF EXISTS (
    SELECT 1
      FROM sys.columns c
      JOIN sys.tables  t ON c.object_id = t.object_id
     WHERE t.name = N'TSql_DocumentType'
       AND c.name = N'LinModifiedBy'
       AND c.is_nullable = 0
)
BEGIN
    UPDATE dbo.TSql_DocumentType
       SET LinModifiedBy = NULL
     WHERE LinModifiedBy IS NOT NULL
       AND (
                LTRIM(RTRIM(LinModifiedBy)) = N''
             OR (Ntimeschanged = 0 AND LinModifiedBy = LinkMadeBy)
           );

    ALTER TABLE dbo.TSql_DocumentType
        ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
    PRINT N'OK — dbo.TSql_DocumentType.LinModifiedBy -> NVARCHAR(128) NULL.';
END
ELSE
BEGIN
    PRINT N'OK — dbo.TSql_DocumentType.LinModifiedBy ya era NULL.';
END
GO
