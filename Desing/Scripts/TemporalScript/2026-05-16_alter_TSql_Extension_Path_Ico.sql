/* =============================================================================
   dbo.TSql_Extension — columna Path_Ico (icono opcional por extensión)
   NVARCHAR(500) NULL; ruta virtual relativa a la aplicación (~ o raíz www)
   Misma convención que Path_Ico en dbo.TSql_Client_V2.

   IMPORTANTE — EF EDMX: tras ejecutar, en Visual Studio abrir Model.edmx →
   "Update Model from Database" y refrescar dbo.TSql_Extension. El diseñador
   mapeará Path_Ico sobre la entidad; hasta entonces esta solución puede usar
   SQL directo vía ExtensionPathIcoQueries si no queréis todavía refrescar.

   Opcional tras el refresh: revisar partial TSql_Extension.NotMapped.cs
   si queréis usar sólo Path_Ico mapeada por EF sin IcoPath auxiliar para formularios.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Extension', N'Path_Ico') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Extension
        ADD Path_Ico NVARCHAR(500) NULL;
    PRINT N'OK — Añadida columna Path_Ico (opcional).';
END
ELSE IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
    PRINT N'OK — Path_Ico ya existe.';
ELSE
    PRINT N'Aviso: ejecute antes 2026-05-16_create_TSql_Extension.sql.';
GO
