/* =============================================================================
 dbo.TSql_XrDevice — Dispositivos XR (Meta Quest / tablet) para Enviar a XR
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_XrDevice'
)
BEGIN
    CREATE TABLE [dbo].[TSql_XrDevice]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_XrDevice PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        /* Quest | Tablet */
        [TextDeviceType] NVARCHAR(50) NOT NULL,
        /* Código que introduce la app Unity para identificarse */
        [TextPairingCode] NVARCHAR(50) NOT NULL,
        [TextNotes] NVARCHAR(500) NULL,
        [Is_Paired] BIT NOT NULL
            CONSTRAINT DF_TSql_XrDevice_Is_Paired DEFAULT (0),
        [DateLastSeen] DATETIME NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_XrDevice_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_XrDevice_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_XrDevice_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_XrDevice_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_XrDevice_TextPairingCode'
      AND object_id = OBJECT_ID(N'dbo.TSql_XrDevice')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_XrDevice_TextPairingCode
    ON [dbo].[TSql_XrDevice] ([TextPairingCode])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_XrDevice_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_XrDevice')
)
    CREATE NONCLUSTERED INDEX IX_TSql_XrDevice_TextLabel_Active
    ON [dbo].[TSql_XrDevice] ([TextLabel])
    INCLUDE ([Is_Active], [TextDeviceType])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_XrDevice creada (o ya existía).';
GO
