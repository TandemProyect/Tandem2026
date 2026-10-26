/* =============================================================================
 dbo.TSql_DesignWall — Is_CheckDimWall
 La columna ya existe. El formulario «Modificar muro» la escribe al salvar.
 Un muro nuevo se acota: el default pasa de 0 a 1.
 El relleno a 1 es de una sola vez (la columna no se había escrito desde el visor).
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

IF COL_LENGTH(N'dbo.TSql_DesignWall', N'Is_CheckDimWall') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD [Is_CheckDimWall] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckDimWall DEFAULT (1);
END
GO

DECLARE @dfDim SYSNAME;
SELECT @dfDim = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
  AND c.name = N'Is_CheckDimWall';

IF @dfDim IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
      AND dc.name = N'DF_TSql_DesignWall_Is_CheckDimWall'
      AND dc.definition = N'((1))'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TSql_DesignWall] DROP CONSTRAINT [' + @dfDim + N']');
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD CONSTRAINT DF_TSql_DesignWall_Is_CheckDimWall DEFAULT (1) FOR [Is_CheckDimWall];
END
GO

/* Una sola vez: el visor aún no había escrito Is_CheckDimWall (quedó en 0). */
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (
    SELECT 1
    FROM sys.extended_properties
    WHERE name = N'Desing2_DimWallBackfill_20261026'
      AND major_id = OBJECT_ID(N'dbo.TSql_DesignWall')
      AND minor_id = 0
)
BEGIN
    UPDATE [dbo].[TSql_DesignWall]
    SET [Is_CheckDimWall] = 1
    WHERE [Is_Delete] = 0;

    EXEC sys.sp_addextendedproperty
        @name = N'Desing2_DimWallBackfill_20261026',
        @value = N'Is_CheckDimWall = 1 en muros activos. El visor aún no escribía la columna.',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'TSql_DesignWall';
END
GO

PRINT N'OK — dbo.TSql_DesignWall Is_CheckDimWall por defecto acotado.';
GO
