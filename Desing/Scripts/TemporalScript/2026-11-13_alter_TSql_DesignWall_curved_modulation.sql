/* =============================================================================
 dbo.TSql_DesignWall — Muro curvo: modulación (Desing_2, US 13/11/2026).
 NumberCurveModulation = ancho del panel de encofrado del muro curvo, en mm:
   300, 450, 600, 750, 900, 1050 o 1200 (0,30 … 1,20 m). Por defecto en la app: 900.
 NULL en muros rectos (y en muros curvos guardados antes de este script; la app usa 900).
 Complementa 2026-11-13_alter_TSql_DesignWall_curved_wall.sql (ejecutar antes).
 Solo ADD de una columna NULL: no borra, no renombra, no toca datos. Idempotente.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NULL
BEGIN
    RAISERROR(N'No existe dbo.TSql_DesignWall. Ejecuta antes el script de creación.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveModulation') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveModulation] INT NULL;
GO

PRINT N'OK — dbo.TSql_DesignWall columna NumberCurveModulation.';
GO
