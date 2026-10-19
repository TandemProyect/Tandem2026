/* =============================================================================
   dbo.TSql_DesignWorker — escala del trabajador para caber en la ménsula
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.TSql_DesignWorker', N'NumberScale') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWorker]
        ADD [NumberScale] FLOAT NULL;
END
GO

PRINT N'OK — dbo.TSql_DesignWorker.NumberScale lista (o ya existía).';
GO
