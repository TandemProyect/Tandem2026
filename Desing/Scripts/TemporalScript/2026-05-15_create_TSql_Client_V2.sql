/* =============================================================================
   Script 1 — dbo.TSql_Client_V2
   Tabla principal de clientes (intranet).
   Columnas fijas alineadas con dbo.TSql_Client (captura SSMS).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_Client_V2
    (
        /* ----- FIJAS (todas las tablas intranet) ----- */
        IdObject            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Client_V2 PRIMARY KEY CLUSTERED,

        TextLabel           NVARCHAR(500)   NOT NULL,

        Is_Delete           BIT             NOT NULL
            CONSTRAINT DF_TSql_Client_V2_Is_Delete DEFAULT (0),
        Is_Active           BIT             NOT NULL
            CONSTRAINT DF_TSql_Client_V2_Is_Active DEFAULT (1),

        LinkMadeBy          NVARCHAR(128)   NOT NULL,
        LinModifiedBy       NVARCHAR(128)   NOT NULL,
        AddDateMade         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Client_V2_AddDateMade DEFAULT (GETDATE()),
        AddChangeBy         NVARCHAR(128)   NOT NULL,
        AddLastDateChange   DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Client_V2_AddLastDateChange DEFAULT (GETDATE()),
        Ntimeschanged       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_Client_V2_Ntimeschanged DEFAULT (0),

        /* ----- Negocio (heredado de TSql_Client) ----- */
        LinkMethodOfPayment BIGINT          NULL,
        Path_Ico            NVARCHAR(500)   NOT NULL,
        Path_Logo           NVARCHAR(500)   NOT NULL,

        /* ----- Opcionales (ampliar más adelante) ----- */
        TextCode            NVARCHAR(128)   NULL,   /* código interno cliente */
        TextTaxId           NVARCHAR(50)    NULL,   /* CIF / NIF */
        TextEmail           NVARCHAR(256)   NULL,
        TextPhone           NVARCHAR(50)    NULL
    );
END
GO

/* Listados activos por nombre */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Client_V2_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Client_V2_TextLabel_Active
        ON dbo.TSql_Client_V2 (TextLabel)
        INCLUDE (Is_Active, Path_Ico, Path_Logo)
        WHERE Is_Delete = 0;
GO

/* Búsqueda por código (si se usa) */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Client_V2_TextCode'
      AND object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Client_V2_TextCode
        ON dbo.TSql_Client_V2 (TextCode)
        WHERE TextCode IS NOT NULL AND Is_Delete = 0;
GO

/* FK opcional a método de pago — descomentar cuando exista la tabla destino
ALTER TABLE dbo.TSql_Client_V2
    ADD CONSTRAINT FK_TSql_Client_V2_MethodOfPayment
        FOREIGN KEY (LinkMethodOfPayment) REFERENCES dbo.TSql_MethodOfPayment (IdObject);
GO
*/

PRINT N'OK — dbo.TSql_Client_V2 creada (o ya existía).';
GO
