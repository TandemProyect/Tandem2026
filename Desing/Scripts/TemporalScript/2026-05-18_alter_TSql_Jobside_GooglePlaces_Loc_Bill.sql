/* =============================================================================
   dbo.TSql_Jobside — BitBillSameAsLoc + direcciones Google (Loc_* + Bill_*)

   Para instalaciones donde la tabla ya existía y solo se ejecutaron scripts de
   migración (p. ej. LinkClient_V2, auditoría) sin recrear la tabla: el EDMX y
   la app esperan las mismas columnas que en 2026-05-15_create_TSql_Jobside.sql.

   Idempotente (COL_LENGTH). No borra datos.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'BitBillSameAsLoc') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD BitBillSameAsLoc BIT NOT NULL
        CONSTRAINT DF_TSql_Jobside_BitBillSameAsLoc DEFAULT (0);
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Place_Id') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Place_Id NVARCHAR(255) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Formatted_Address') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Formatted_Address NVARCHAR(1000) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Lat') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Lat DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Lng') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Lng DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Street_Number') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Street_Number NVARCHAR(50) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Route') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Route NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Subpremise') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Subpremise NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Locality') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Locality NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Admin_Area_1') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Admin_Area_1 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Admin_Area_2') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Admin_Area_2 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Postal_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Postal_Code NVARCHAR(20) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Country_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Country_Code NVARCHAR(10) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Country_Name') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Country_Name NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Address_Components_Json') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Loc_Address_Components_Json NVARCHAR(MAX) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Place_Id') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Place_Id NVARCHAR(255) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Formatted_Address') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Formatted_Address NVARCHAR(1000) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Lat') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Lat DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Lng') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Lng DECIMAL(9, 6) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Street_Number') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Street_Number NVARCHAR(50) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Route') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Route NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Subpremise') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Subpremise NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Locality') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Locality NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Admin_Area_1') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Admin_Area_1 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Admin_Area_2') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Admin_Area_2 NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Postal_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Postal_Code NVARCHAR(20) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Country_Code') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Country_Code NVARCHAR(10) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Country_Name') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Country_Name NVARCHAR(100) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Bill_Address_Components_Json') IS NULL
BEGIN
    ALTER TABLE dbo.TSql_Jobside ADD Bill_Address_Components_Json NVARCHAR(MAX) NULL;
END
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_TSql_Jobside_Loc_Place_Id'
          AND object_id = OBJECT_ID(N'dbo.TSql_Jobside')
    )
   AND COL_LENGTH(N'dbo.TSql_Jobside', N'Loc_Place_Id') IS NOT NULL
BEGIN
    CREATE NONCLUSTERED INDEX IX_TSql_Jobside_Loc_Place_Id
        ON dbo.TSql_Jobside (Loc_Place_Id)
        WHERE Loc_Place_Id IS NOT NULL;
END
GO

PRINT N'OK — dbo.TSql_Jobside: BitBillSameAsLoc + Loc_* + Bill_* (Google Places).';
GO
