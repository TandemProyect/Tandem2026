/* =============================================================================
   dbo.TSql_Design_V2 — puntos de referencia geométricos de esquinas.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NULL
BEGIN
    PRINT N'— dbo.TSql_Design_V2 no está en esta base. AttCornerReferencePointsJson no se añade.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_Design_V2', N'AttCornerReferencePointsJson') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_Design_V2]
        ADD [AttCornerReferencePointsJson] NVARCHAR(MAX) NULL;
    PRINT N'OK — dbo.TSql_Design_V2.AttCornerReferencePointsJson creada.';
END
ELSE
BEGIN
    PRINT N'OK — dbo.TSql_Design_V2.AttCornerReferencePointsJson ya existía.';
END
GO
