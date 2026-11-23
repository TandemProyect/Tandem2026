/* =============================================================================
   dbo.TSql_DevelopIssueDetail — Detalles de una US o un bug (TSql_DevelopIssue):
   observaciones y capturas de pantalla (ruta virtual en /Files/DevelopIssue/).
   Requiere 2026-11-23_create_TSql_DevelopIssue.sql. Idempotente: develop y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; -- índices filtrados (sqlcmd lo trae en OFF)
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DevelopIssueDetail'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DevelopIssueDetail]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DevelopIssueDetail PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [LinkDevelopIssue]    BIGINT          NOT NULL,
        [NumberOrder]         INT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueDetail_NumberOrder    DEFAULT (1),
        [TextObservation]     NVARCHAR(MAX)   NULL,
        [TextImagePath]       NVARCHAR(1000)  NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueDetail_Is_Delete      DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueDetail_Is_Active      DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueDetail_AddDateMade    DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueDetail_Ntimeschanged  DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DevelopIssueDetail_DevelopIssue')
    ALTER TABLE [dbo].[TSql_DevelopIssueDetail]
        ADD CONSTRAINT FK_TSql_DevelopIssueDetail_DevelopIssue
        FOREIGN KEY ([LinkDevelopIssue]) REFERENCES [dbo].[TSql_DevelopIssue]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DevelopIssueDetail_Issue_Order'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelopIssueDetail')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DevelopIssueDetail_Issue_Order
        ON [dbo].[TSql_DevelopIssueDetail] ([LinkDevelopIssue], [NumberOrder], [IdObject])
        INCLUDE ([TextImagePath], [TextLabel])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DevelopIssueDetail creada (o ya existía).';
GO
