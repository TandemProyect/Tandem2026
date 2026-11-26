/* =============================================================================
   dbo.TSql_DevelopIssueDetail.TextFilePath — adjunto que no es imagen (.tnwall,
   muros exportados del visor Desing_2). Ruta virtual en /Files/DevelopIssue/;
   el nombre original va en TextLabel. TextImagePath sigue siendo solo capturas.
   Requiere 2026-11-23_create_TSql_DevelopIssueDetail.sql. Idempotente: develop y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.TSql_DevelopIssueDetail', N'TextFilePath') IS NULL
    ALTER TABLE [dbo].[TSql_DevelopIssueDetail] ADD [TextFilePath] NVARCHAR(1000) NULL;
GO

PRINT N'OK — dbo.TSql_DevelopIssueDetail.TextFilePath añadida (o ya existía).';
GO
