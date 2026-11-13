/* =============================================================================
 dbo.TSql_DesignWall — Muro curvo (Desing_2, US 13/11/2026).
 Un muro curvo es UNA fila con TextWallRole = 'curved' y TextTypeMesh = 'MuroCurvo'.
 Se reutilizan: NumberWidth = espesor (m), NumberHeight = altura (m),
 NumberP1X..Z y NumberP2X..Z = extremos del diámetro exterior en la dirección de inicio
 (se siguen rellenando: son NOT NULL y sirven para emparejar filas).
 Columnas nuevas (todas NULL, sin DEFAULT; en muros rectos quedan a NULL):
   NumberCurveCenterX/Y/Z     centro del arco (mm escena, plano XZ; Y = cota base)
   NumberCurveOuterRadius     radio exterior (mm)
   NumberCurveFractionNum     numerador de la porción del círculo (1/33 -> 1)
   NumberCurveFractionDen     denominador (1/33 -> 33). 1/1 = círculo completo
   NumberCurveStartAngle      ángulo de inicio del arco (grados, de +X hacia +Z)
 Barrido = 360 · Num / Den. Fracción entera num/den para que 1/33 sea exacto.
 Solo ADD de columnas NULL: no borra, no renombra, no toca datos. Idempotente.
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

IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterX') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveCenterX] FLOAT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterY') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveCenterY] FLOAT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveCenterZ') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveCenterZ] FLOAT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveOuterRadius') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveOuterRadius] FLOAT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveFractionNum') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveFractionNum] INT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveFractionDen') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveFractionDen] INT NULL;
GO
IF COL_LENGTH(N'dbo.TSql_DesignWall', N'NumberCurveStartAngle') IS NULL
    ALTER TABLE [dbo].[TSql_DesignWall] ADD [NumberCurveStartAngle] FLOAT NULL;
GO

PRINT N'OK — dbo.TSql_DesignWall columnas de muro curvo.';
GO
