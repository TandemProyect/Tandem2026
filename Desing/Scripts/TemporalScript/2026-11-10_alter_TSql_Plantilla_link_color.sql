/* =============================================================================
   dbo.TSql_Plantilla — AttLinkColor (color de la letra de los links y los iconos)

   Tabla legada (auditoría AttX y SysObjectID, no el patrón TSql_ nuevo): solo se
   añade la columna de negocio; no se agregan las 9 columnas estándar.

   Tras ejecutar: el EDMX (DAL/Model.edmx) debe incluir AttLinkColor para que EF
   lo mapee (Update Model from Database o mapping manual en CSDL SSDL MSL).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Plantilla', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Plantilla', N'AttLinkColor') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Plantilla ADD AttLinkColor NVARCHAR(20) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Plantilla', N'U') IS NOT NULL
BEGIN
    SELECT COL_LENGTH(N'dbo.TSql_Plantilla', N'AttLinkColor') AS Len_AttLinkColor;
END
GO

PRINT N'OK — dbo.TSql_Plantilla: AttLinkColor (color de links e iconos).';
GO
