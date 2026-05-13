/*
  Un solo lote SIN GO (paneles SQL restrictivos).

  IMPORTANTE: los UPDATE no pueden ir en el mismo lote que el ALTER que crea la
  columna: SQL Server valida nombres al compilar el lote y da Msg 207. Por eso
  los UPDATE van en EXEC (segunda fase de compilacion).

  Ejecutar en la base del Initial Catalog de ConexionData (Web.config publicado).
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandText') IS NULL
    ALTER TABLE dbo.TSql_Plantilla ADD
        AttBrandText NVARCHAR(120) NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttBrandText DEFAULT (N'T Desing.net');

IF COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandTextColor') IS NULL
    ALTER TABLE dbo.TSql_Plantilla ADD AttBrandTextColor NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandAccentColor') IS NULL
    ALTER TABLE dbo.TSql_Plantilla ADD
        AttBrandAccentColor NVARCHAR(20) NOT NULL
            CONSTRAINT DF_TSql_Plantilla_AttBrandAccentColor DEFAULT (N'#f29100');

/* Misma logica que arriba, pero compilado despues de existir las columnas */
EXEC(N'UPDATE dbo.TSql_Plantilla SET AttBrandText = N''T Desing.net''
WHERE AttBrandText IS NULL OR LTRIM(RTRIM(AttBrandText)) = N''''');

EXEC(N'UPDATE dbo.TSql_Plantilla SET AttBrandAccentColor = N''#f29100''
WHERE AttBrandAccentColor IS NULL OR LTRIM(RTRIM(AttBrandAccentColor)) = N''''');

SELECT DB_NAME() AS BaseDeDatos,
       COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandText') AS Len_AttBrandText,
       COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandTextColor') AS Len_AttBrandTextColor,
       COL_LENGTH('dbo.TSql_Plantilla', 'AttBrandAccentColor') AS Len_AttBrandAccentColor;
