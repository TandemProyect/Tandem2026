/* =============================================================================
 dbo.TSql_DesignWorker — Trabajadores insertados en un diseño Desing_2
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesignWorker'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesignWorker]
    (
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesignWorker PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,
        [NumberXMm] FLOAT NOT NULL,
        [NumberYMm] FLOAT NOT NULL,
        [NumberZMm] FLOAT NOT NULL,
        [NumberRotY] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWorker_NumberRotY DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWorker_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWorker_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesignWorker_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesignWorker_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWorker_Design_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWorker')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWorker_Design_Active
        ON [dbo].[TSql_DesignWorker] ([LinkDesign_V2])
        INCLUDE ([NumberXMm], [NumberZMm], [NumberRotY])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesignWorker creada (o ya existía).';
GO
