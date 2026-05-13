/*
  Anade columnas de marca a dbo.TSql_Plantilla (requerido por Entity Framework).

  Debes ejecutarlo en la MISMA base de datos que "Initial Catalog" / "Database"
  de la cadena ConexionData en el Web.config de la app publicada.

  Archivo en el proyecto: Desing/App_Data/Sql/TSql_Plantilla_add_brand_text_and_colors.sql
*/

SET NOCOUNT ON;

/* Comprueba base y columnas (debe ser la base de la aplicacion) */
SELECT DB_NAME() AS BaseDeDatosActual;
SELECT OBJECT_ID('dbo.TSql_Plantilla', 'U') AS IdTabla_TSql_Plantilla;
SELECT
    COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandText')           AS Len_AttBrandText,
    COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandTextColor')      AS Len_AttBrandTextColor,
    COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandAccentColor')    AS Len_AttBrandAccentColor;
GO

IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NULL
BEGIN
    RAISERROR('No existe dbo.TSql_Plantilla en esta base. Revisa Initial Catalog en Web.config.', 16, 1);
END
ELSE IF COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandText') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Plantilla ADD
        AttBrandText NVARCHAR(120) NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttBrandText DEFAULT (N'T Desing.net');
END
GO

IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandTextColor') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Plantilla ADD AttBrandTextColor NVARCHAR(20) NULL;
END
GO

IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandAccentColor') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Plantilla ADD
        AttBrandAccentColor NVARCHAR(20) NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttBrandAccentColor DEFAULT (N'#f29100');
END
GO

IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.TSql_Plantilla
    SET AttBrandText = N'T Desing.net'
    WHERE AttBrandText IS NULL OR LTRIM(RTRIM(AttBrandText)) = N'';

    UPDATE dbo.TSql_Plantilla
    SET AttBrandAccentColor = N'#f29100'
    WHERE AttBrandAccentColor IS NULL OR LTRIM(RTRIM(AttBrandAccentColor)) = N'';
END
GO

IF OBJECT_ID('dbo.TSql_Plantilla', 'U') IS NOT NULL
    SELECT
        COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandText')        AS Len_AttBrandText,
        COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandTextColor')   AS Len_AttBrandTextColor,
        COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandAccentColor') AS Len_AttBrandAccentColor;
/* Las tres Len_* deben ser > 0. Reinicia la app y prueba Editar plantilla. */
