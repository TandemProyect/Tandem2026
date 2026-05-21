/* =============================================================================
   dbo.TSql_Client_V2 — columnas dirección Google Places (misma estructura Loc_*
   que dbo.TSql_Company / bloque Loc en dbo.TSql_Jobside).

   Tras ejecutar en SQL Server: en Visual Studio, abrir DAL/Model.edmx →
   «Update Model from Database» → marcar dbo.TSql_Client_V2 → Finish.
   Si ya se sincronizó el modelo con este script vía edmx manual en el repo,
   compruebe que las propiedades coinciden con la base de datos.

   Idempotente: COL_LENGTH(...) por columna.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_Client_V2 no existe; no se aplican cambios.';
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Place_Id') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Place_Id NVARCHAR(255) NULL;
        PRINT N'OK — Añadida columna Loc_Place_Id.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Formatted_Address') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Formatted_Address NVARCHAR(1000) NULL;
        PRINT N'OK — Añadida columna Loc_Formatted_Address.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Lat') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Lat DECIMAL(9, 6) NULL;
        PRINT N'OK — Añadida columna Loc_Lat.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Lng') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Lng DECIMAL(9, 6) NULL;
        PRINT N'OK — Añadida columna Loc_Lng.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Street_Number') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Street_Number NVARCHAR(50) NULL;
        PRINT N'OK — Añadida columna Loc_Street_Number.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Route') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Route NVARCHAR(250) NULL;
        PRINT N'OK — Añadida columna Loc_Route.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Subpremise') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Subpremise NVARCHAR(100) NULL;
        PRINT N'OK — Añadida columna Loc_Subpremise.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Locality') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Locality NVARCHAR(250) NULL;
        PRINT N'OK — Añadida columna Loc_Locality.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Admin_Area_1') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Admin_Area_1 NVARCHAR(250) NULL;
        PRINT N'OK — Añadida columna Loc_Admin_Area_1.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Admin_Area_2') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Admin_Area_2 NVARCHAR(250) NULL;
        PRINT N'OK — Añadida columna Loc_Admin_Area_2.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Postal_Code') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Postal_Code NVARCHAR(20) NULL;
        PRINT N'OK — Añadida columna Loc_Postal_Code.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Country_Code') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Country_Code NVARCHAR(10) NULL;
        PRINT N'OK — Añadida columna Loc_Country_Code.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Country_Name') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Country_Name NVARCHAR(100) NULL;
        PRINT N'OK — Añadida columna Loc_Country_Name.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Loc_Address_Components_Json') IS NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ADD Loc_Address_Components_Json NVARCHAR(MAX) NULL;
        PRINT N'OK — Añadida columna Loc_Address_Components_Json.';
    END
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Client_V2_Loc_Place_Id'
      AND object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Client_V2_Loc_Place_Id
        ON dbo.TSql_Client_V2 (Loc_Place_Id)
        WHERE Loc_Place_Id IS NOT NULL;
GO

PRINT N'OK — dbo.TSql_Client_V2: columnas dirección Google Places (Loc_*) revisadas.';
GO
