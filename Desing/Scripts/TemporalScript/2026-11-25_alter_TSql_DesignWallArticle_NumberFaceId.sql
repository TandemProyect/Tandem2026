/* =============================================================================
   dbo.TSql_DesignWallArticle — cara del muro (F-n del visor) de cada pieza.
   US «Generar plano de montaje». NULL = aún sin asignar: el icono «Plano de
   montaje» del visor la rellena (Desing_2/GenerateAssemblyViews).
   Idempotente: develop y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.TSql_DesignWallArticle', N'NumberFaceId') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWallArticle] ADD [NumberFaceId] INT NULL;
GO

PRINT N'OK — dbo.TSql_DesignWallArticle.NumberFaceId añadida (o ya existía).';
GO
