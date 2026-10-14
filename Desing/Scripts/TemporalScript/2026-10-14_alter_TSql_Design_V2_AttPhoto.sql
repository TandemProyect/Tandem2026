/* =============================================================================
   dbo.TSql_Design_V2 — ruta de la foto del visor (AttPhoto)
   La imagen vive en Content/images/desing/yyyy-MM/<nombre del diseño>.png
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.TSql_Design_V2', N'AttPhoto') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Design_V2]
        ADD [AttPhoto] NVARCHAR(500) NULL;
END
GO

PRINT N'OK — dbo.TSql_Design_V2.AttPhoto lista (o ya existía).';
GO
