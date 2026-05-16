/* =============================================================================
   dbo.TSql_DocumentType — eliminar NumberMaxFileSizeBytes

   El tamaño máximo por fichero se gestiona en dbo.TSql_Extension
   (NumberMaxFileSizeBytes por extensión).

   Idempotente: DROP CONSTRAINT por nombre si existe, luego DROP COLUMN.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_DocumentType no existe.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_DocumentType', N'NumberMaxFileSizeBytes') IS NOT NULL
BEGIN
    DECLARE @dc NVARCHAR(256);
    SELECT @dc = dc.name
      FROM sys.default_constraints dc
      JOIN sys.columns c ON c.default_object_id = dc.object_id
     WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
       AND c.name = N'NumberMaxFileSizeBytes';

    IF @dc IS NOT NULL
        EXEC(N'ALTER TABLE dbo.TSql_DocumentType DROP CONSTRAINT [' + @dc + N'];');

    ALTER TABLE dbo.TSql_DocumentType DROP COLUMN NumberMaxFileSizeBytes;
    PRINT N'OK — Eliminada columna NumberMaxFileSizeBytes de dbo.TSql_DocumentType.';
END
ELSE
BEGIN
    PRINT N'OK — NumberMaxFileSizeBytes ya no existe en dbo.TSql_DocumentType.';
END
GO
