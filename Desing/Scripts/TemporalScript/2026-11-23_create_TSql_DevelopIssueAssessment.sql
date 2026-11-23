/* =============================================================================
   dbo.TSql_DevelopIssueAssessment — Valoraciones de una US o un bug (TSql_DevelopIssue):
   tamaño, horas estimadas, análisis y zona afectada. Una fila por valoración
   (TextSource = AI: IA de la intranet; AGENT: agente de programación).
   Requiere 2026-11-23_create_TSql_DevelopIssue.sql. Idempotente: develop y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; -- índices filtrados (sqlcmd lo trae en OFF)
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DevelopIssueAssessment'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DevelopIssueAssessment]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DevelopIssueAssessment PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [LinkDevelopIssue]    BIGINT          NOT NULL,
        [TextSource]          NVARCHAR(20)    NOT NULL,   -- AI | AGENT
        [TextSize]            NVARCHAR(20)    NOT NULL,   -- SMALL | MEDIUM | LARGE
        [NumberHours]         DECIMAL(7,2)    NULL,
        [TextArea]            NVARCHAR(500)   NULL,
        [TextAnalysis]        NVARCHAR(MAX)   NULL,
        [TextModel]           NVARCHAR(100)   NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueAssessment_Is_Delete      DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueAssessment_Is_Active      DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueAssessment_AddDateMade    DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DevelopIssueAssessment_Ntimeschanged  DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DevelopIssueAssessment_DevelopIssue')
    ALTER TABLE [dbo].[TSql_DevelopIssueAssessment]
        ADD CONSTRAINT FK_TSql_DevelopIssueAssessment_DevelopIssue
        FOREIGN KEY ([LinkDevelopIssue]) REFERENCES [dbo].[TSql_DevelopIssue]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DevelopIssueAssessment_Issue'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelopIssueAssessment')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DevelopIssueAssessment_Issue
        ON [dbo].[TSql_DevelopIssueAssessment] ([LinkDevelopIssue], [IdObject])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DevelopIssueAssessment creada (o ya existía).';
GO
