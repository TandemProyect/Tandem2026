/* =============================================================================
   Añade campos de PDF persistido a dbo.TSql_ClientOffer (si faltan).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.TSql_ClientOffer', N'TextPdfPath') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD TextPdfPath NVARCHAR(500) NULL;
GO

IF COL_LENGTH(N'dbo.TSql_ClientOffer', N'AddPdfDate') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD AddPdfDate DATETIME NULL;
GO

PRINT N'OK — campos PDF de TSql_ClientOffer creados (o ya existían).';
GO
