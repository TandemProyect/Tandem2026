/* =============================================================================
   dbo.TSql_Branch — Color de acento (Attcolor) + campos dirección Google (Loc_*)
   alineados con dbo.TSql_Company.

   Tabla legada (auditoría distinta al patrón TSql_* nuevo): solo se añaden
   columnas de negocio; no se agregan las 9 columnas estándar IdObject/TextLabel/...

   Tras ejecutar: actualizar EDMX (Update Model from Database) o validar el
   mapping manual en DAL/Model.edmx si ya está sincronizado con este script.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Attcolor') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Attcolor NVARCHAR(16) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Place_Id') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Place_Id NVARCHAR(255) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Formatted_Address') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Formatted_Address NVARCHAR(1000) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Lat') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Lat DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Lng') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Lng DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Street_Number') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Street_Number NVARCHAR(50) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Route') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Route NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Subpremise') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Subpremise NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Locality') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Locality NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Admin_Area_1') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Admin_Area_1 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Admin_Area_2') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Admin_Area_2 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Postal_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Postal_Code NVARCHAR(20) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Country_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Country_Code NVARCHAR(10) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Country_Name') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Country_Name NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Branch', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Branch', N'Loc_Address_Components_Json') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Branch ADD Loc_Address_Components_Json NVARCHAR(MAX) NULL;
END
GO

PRINT N'OK — dbo.TSql_Branch: Attcolor + Loc_* (Google Places, como TSql_Company).';
GO
