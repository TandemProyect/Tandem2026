/* =============================================================================
   dbo.TSql_DesignWorker — modelo elegido (Worker / Worker2)
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.TSql_DesignWorker', N'TextCode') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWorker]
        ADD [TextCode] NVARCHAR(50) NULL;
END
GO

PRINT N'OK — dbo.TSql_DesignWorker.TextCode lista (o ya existía).';
GO
