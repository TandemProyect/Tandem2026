/* =============================================================================
   Script 2 — dbo.TSql_Jobside (instalación nueva)
   Requiere: Script 1 (TSql_Client_V2) ejecutado antes.
   Incluye LinkClient_V2 → TSql_Client_V2.IdObject. NO incluye Link_Client.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_Jobside
    (
        /* ----- FIJAS (todas las tablas intranet) ----- */
        IdObject            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Jobside PRIMARY KEY CLUSTERED,

        TextLabel           NVARCHAR(500)   NOT NULL,

        Is_Delete           BIT             NOT NULL
            CONSTRAINT DF_TSql_Jobside_Is_Delete DEFAULT (0),
        Is_Active           BIT             NOT NULL
            CONSTRAINT DF_TSql_Jobside_Is_Active DEFAULT (1),

        LinkMadeBy          NVARCHAR(128)   NOT NULL,
        LinModifiedBy       NVARCHAR(128)   NOT NULL,
        AddDateMade         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Jobside_AddDateMade DEFAULT (GETDATE()),
        AddChangeBy         NVARCHAR(128)   NOT NULL,
        AddLastDateChange   DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Jobside_AddLastDateChange DEFAULT (GETDATE()),
        Ntimeschanged       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_Jobside_Ntimeschanged DEFAULT (0),

        /* ----- Cliente (obra → cliente V2) ----- */
        LinkClient_V2       BIGINT          NULL,

        /* ----- “Facturación = misma que local” ----- */
        BitBillSameAsLoc    BIT             NOT NULL
            CONSTRAINT DF_TSql_Jobside_BitBillSameAsLoc DEFAULT (0),

        /* ----- Dirección LOCAL (Google Places + mapa) ----- */
        Loc_Place_Id                    NVARCHAR(255)   NULL,
        Loc_Formatted_Address           NVARCHAR(1000)  NULL,
        Loc_Lat                         DECIMAL(9, 6)   NULL,
        Loc_Lng                         DECIMAL(9, 6)   NULL,
        Loc_Street_Number               NVARCHAR(50)    NULL,
        Loc_Route                       NVARCHAR(250)   NULL,
        Loc_Subpremise                  NVARCHAR(100)   NULL,
        Loc_Locality                    NVARCHAR(250)   NULL,
        Loc_Admin_Area_1                NVARCHAR(250)   NULL,
        Loc_Admin_Area_2                NVARCHAR(250)   NULL,
        Loc_Postal_Code                 NVARCHAR(20)    NULL,
        Loc_Country_Code                NVARCHAR(10)    NULL,
        Loc_Country_Name                NVARCHAR(100)   NULL,
        Loc_Address_Components_Json     NVARCHAR(MAX)   NULL,

        /* ----- Dirección FACTURACIÓN (misma estructura) ----- */
        Bill_Place_Id                   NVARCHAR(255)   NULL,
        Bill_Formatted_Address          NVARCHAR(1000)  NULL,
        Bill_Lat                        DECIMAL(9, 6)   NULL,
        Bill_Lng                        DECIMAL(9, 6)   NULL,
        Bill_Street_Number              NVARCHAR(50)    NULL,
        Bill_Route                      NVARCHAR(250)   NULL,
        Bill_Subpremise                 NVARCHAR(100)   NULL,
        Bill_Locality                   NVARCHAR(250)   NULL,
        Bill_Admin_Area_1               NVARCHAR(250)   NULL,
        Bill_Admin_Area_2               NVARCHAR(250)   NULL,
        Bill_Postal_Code                NVARCHAR(20)    NULL,
        Bill_Country_Code               NVARCHAR(10)    NULL,
        Bill_Country_Name               NVARCHAR(100)   NULL,
        Bill_Address_Components_Json    NVARCHAR(MAX)   NULL,

        CONSTRAINT FK_TSql_Jobside_TSql_Client_V2
            FOREIGN KEY (LinkClient_V2)
            REFERENCES dbo.TSql_Client_V2 (IdObject)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Jobside_LinkClient_V2'
      AND object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Jobside_LinkClient_V2
        ON dbo.TSql_Jobside (LinkClient_V2)
        WHERE LinkClient_V2 IS NOT NULL AND Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Jobside_Loc_Place_Id'
      AND object_id = OBJECT_ID(N'dbo.TSql_Jobside')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Jobside_Loc_Place_Id
        ON dbo.TSql_Jobside (Loc_Place_Id)
        WHERE Loc_Place_Id IS NOT NULL;
GO

PRINT N'OK — dbo.TSql_Jobside creada con LinkClient_V2.';
GO
