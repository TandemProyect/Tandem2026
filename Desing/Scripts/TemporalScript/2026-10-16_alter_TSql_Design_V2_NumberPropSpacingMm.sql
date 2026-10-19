/* =============================================================================
   dbo.TSql_Design_V2 — distancia máxima entre puntales, por diseño (mm).
   NULL = 2,40 m. No es tabla nueva: la entidad ya existe (Att* / Sys*).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.TSql_Design_V2', N'NumberPropSpacingMm') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Design_V2]
        ADD [NumberPropSpacingMm] FLOAT NULL;
END
GO

PRINT N'OK — dbo.TSql_Design_V2.NumberPropSpacingMm (o ya existía).';
GO
