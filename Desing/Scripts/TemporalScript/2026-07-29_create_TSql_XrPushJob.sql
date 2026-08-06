/* =============================================================================
 dbo.TSql_XrPushJob — Cola de envíos de diseño a un dispositivo XR
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_XrPushJob'
)
BEGIN
    CREATE TABLE [dbo].[TSql_XrPushJob]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_XrPushJob PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkXrDevice] BIGINT NOT NULL,
        [LinkDesign] BIGINT NOT NULL,
        [LinkOffer] BIGINT NULL,
        /* Pending | Delivered | Cancelled | Failed */
        [TextStatus] NVARCHAR(50) NOT NULL
            CONSTRAINT DF_TSql_XrPushJob_TextStatus DEFAULT (N'Pending'),
        [DateDelivered] DATETIME NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_XrPushJob_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_XrPushJob_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_XrPushJob_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_XrPushJob_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_XrPushJob_LinkXrDevice'
)
BEGIN
    IF OBJECT_ID(N'dbo.TSql_XrDevice', N'U') IS NOT NULL
    BEGIN
        ALTER TABLE [dbo].[TSql_XrPushJob] WITH CHECK
        ADD CONSTRAINT FK_TSql_XrPushJob_LinkXrDevice
            FOREIGN KEY ([LinkXrDevice]) REFERENCES [dbo].[TSql_XrDevice] ([IdObject]);
    END
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_XrPushJob_Device_Pending'
      AND object_id = OBJECT_ID(N'dbo.TSql_XrPushJob')
)
    CREATE NONCLUSTERED INDEX IX_TSql_XrPushJob_Device_Pending
    ON [dbo].[TSql_XrPushJob] ([LinkXrDevice], [TextStatus], [AddDateMade])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_XrPushJob creada (o ya existía).';
GO
