/* =============================================================================
 dbo.TSql_Employee — Is_CadDeveloper: programadores CAD (NETLOAD, cualquier PC).
 Tabla legada (SysObjectID). No se tocan las 9 columnas de auditoría nuevas.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.TSql_Employee', N'Is_CadDeveloper') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Employee]
        ADD [Is_CadDeveloper] BIT NOT NULL
        CONSTRAINT DF_TSql_Employee_Is_CadDeveloper DEFAULT (0);
END
GO

PRINT N'OK — dbo.TSql_Employee.Is_CadDeveloper (o ya existía).';
GO
