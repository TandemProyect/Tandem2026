/* =============================================================================
   dbo.TSql_DevelodAgentDocument — Documentos para agentes (menú Develop › Agentes).
   Cada fila: nombre del documento (TextLabel), clave estable (TextCode), comando que hay que
   darle al agente para que lo lea (TextCommand), ruta en el repo (TextPath),
   explicación (TextDescription) y orden en la lista (NumberOrder).
   Solo lectura en la web. Los datos se mantienen con 2026-11-17_seed_TSql_DevelodAgentDocument.sql.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DevelodAgentDocument'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DevelodAgentDocument]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DevelodAgentDocument PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        /* Clave estable para scripts y sincronización entre bases («area.base-de-datos»). */
        [TextCode]            NVARCHAR(100)   NOT NULL,
        [TextCommand]         NVARCHAR(1000)  NOT NULL,
        [TextPath]            NVARCHAR(300)   NULL,
        [TextDescription]     NVARCHAR(MAX)   NULL,
        [NumberOrder]         INT             NOT NULL
            CONSTRAINT DF_TSql_DevelodAgentDocument_NumberOrder   DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelodAgentDocument_Is_Delete     DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelodAgentDocument_Is_Active     DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DevelodAgentDocument_AddDateMade   DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DevelodAgentDocument_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_DevelodAgentDocument_TextCode_Live'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelodAgentDocument')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_DevelodAgentDocument_TextCode_Live
        ON [dbo].[TSql_DevelodAgentDocument] ([TextCode])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DevelodAgentDocument_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelodAgentDocument')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DevelodAgentDocument_TextLabel_Active
        ON [dbo].[TSql_DevelodAgentDocument] ([TextLabel])
        INCLUDE ([Is_Active])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DevelodAgentDocument creada (o ya existía).';
GO
