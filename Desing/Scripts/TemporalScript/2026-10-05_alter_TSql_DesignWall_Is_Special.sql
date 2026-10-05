/* =============================================================================
 dbo.TSql_DesignWall — Is_Special
 Muro con encofrado manual (bloquing). Si es 1, no se lanza la automatización
 ATK-60 sobre ese segmento. Convención Is_* (el usuario lo llamó IsSpecial).
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_DesignWall', N'Is_Special') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD [Is_Special] BIT NOT NULL
        CONSTRAINT DF_TSql_DesignWall_Is_Special DEFAULT (0);
END
GO

PRINT N'OK — dbo.TSql_DesignWall.Is_Special (o ya existía).';
GO
