/* =============================================================================
 dbo.TSql_DesignWall — checks de configuración del formulario Modificar muro.
 Is_Check750R        = paneles de 0,75 m regulables.
 Is_FormworkMode     = paneles verticales.
 Los dos nacen marcados. El script es idempotente.
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

IF COL_LENGTH(N'dbo.TSql_DesignWall', N'Is_Check750R') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD [Is_Check750R] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Check750R DEFAULT (1);
END
GO

IF COL_LENGTH(N'dbo.TSql_DesignWall', N'Is_FormworkMode') IS NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD [Is_FormworkMode] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_FormworkMode DEFAULT (1);
END
GO

/* El default antiguo era 0. El formulario los trae marcados. */
DECLARE @df750 SYSNAME;
SELECT @df750 = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
  AND c.name = N'Is_Check750R';

IF @df750 IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
      AND dc.name = N'DF_TSql_DesignWall_Is_Check750R'
      AND dc.definition = N'((1))'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TSql_DesignWall] DROP CONSTRAINT [' + @df750 + N']');
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD CONSTRAINT DF_TSql_DesignWall_Is_Check750R DEFAULT (1) FOR [Is_Check750R];
END
GO

DECLARE @dfMode SYSNAME;
SELECT @dfMode = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
    ON c.object_id = dc.parent_object_id
   AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
  AND c.name = N'Is_FormworkMode';

IF @dfMode IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
      AND dc.name = N'DF_TSql_DesignWall_Is_FormworkMode'
      AND dc.definition = N'((1))'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TSql_DesignWall] DROP CONSTRAINT [' + @dfMode + N']');
    ALTER TABLE [dbo].[TSql_DesignWall]
        ADD CONSTRAINT DF_TSql_DesignWall_Is_FormworkMode DEFAULT (1) FOR [Is_FormworkMode];
END
GO

/* Una sola vez: el visor aún no había escrito estas dos columnas. */
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (
    SELECT 1
    FROM sys.extended_properties
    WHERE name = N'Desing2_ConfigChecksBackfill_20261012'
      AND major_id = OBJECT_ID(N'dbo.TSql_DesignWall')
      AND minor_id = 0
)
BEGIN
    UPDATE [dbo].[TSql_DesignWall]
    SET [Is_Check750R] = [Is_UniversalPanel],
        [Is_FormworkMode] = 1;

    EXEC sys.sp_addextendedproperty
        @name = N'Desing2_ConfigChecksBackfill_20261012',
        @value = N'Is_Check750R copiado de Is_UniversalPanel. Is_FormworkMode = 1.',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'TSql_DesignWall';
END
GO

PRINT N'OK — dbo.TSql_DesignWall checks de configuración.';
GO
