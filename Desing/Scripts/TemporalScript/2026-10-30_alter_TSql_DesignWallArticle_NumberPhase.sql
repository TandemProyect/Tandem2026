/* =============================================================================
 dbo.TSql_DesignWallArticle.NumberPhase — fase que colocó la pieza.
 NULL = recuento sin fase. El encofrado de una fase guarda aquí su material,
 incluido el extra del solape.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.TSql_DesignWallArticle', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_DesignWallArticle', N'NumberPhase') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWallArticle]
        ADD [NumberPhase] BIGINT NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_DesignWallArticle', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_DesignWallArticle', N'NumberPhase') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_TSql_DesignWallArticle_Phase'
          AND object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
   )
BEGIN
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWallArticle_Phase
        ON [dbo].[TSql_DesignWallArticle] ([LinkDesign_V2], [NumberPhase])
        WHERE [Is_Delete] = 0;
END
GO

PRINT N'OK — dbo.TSql_DesignWallArticle.NumberPhase lista (o ya existía).';
GO
