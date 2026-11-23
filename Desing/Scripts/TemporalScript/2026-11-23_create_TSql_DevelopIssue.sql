/* =============================================================================
   dbo.TSql_DevelopIssue — Registro de US y Bugs (Develop › US y Bugs)
   TextType:    US | BUG
   TextUrgency: LOW (Bajo) | MEDIUM (Medio) | URGENT (Urgente)
   TextState:   OPEN (Abierto) | IN_PROGRESS (En proceso) | CLOSED (Cerrado)
   DateIssue:   fecha de creación de la US o el bug.
   Idempotente: develop (TdesingDevelop) y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; -- índices filtrados (sqlcmd lo trae en OFF)
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DevelopIssue'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DevelopIssue]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DevelopIssue PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,
        [TextType]            NVARCHAR(10)    NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_TextType          DEFAULT (N'US'),
        [TextUrgency]         NVARCHAR(20)    NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_TextUrgency       DEFAULT (N'MEDIUM'),
        [TextState]           NVARCHAR(20)    NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_TextState         DEFAULT (N'OPEN'),
        [DateIssue]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_DateIssue         DEFAULT (GETDATE()),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DevelopIssue_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_TSql_DevelopIssue_TextType')
    ALTER TABLE [dbo].[TSql_DevelopIssue] ADD CONSTRAINT CK_TSql_DevelopIssue_TextType
        CHECK ([TextType] IN (N'US', N'BUG'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_TSql_DevelopIssue_TextUrgency')
    ALTER TABLE [dbo].[TSql_DevelopIssue] ADD CONSTRAINT CK_TSql_DevelopIssue_TextUrgency
        CHECK ([TextUrgency] IN (N'LOW', N'MEDIUM', N'URGENT'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_TSql_DevelopIssue_TextState')
    ALTER TABLE [dbo].[TSql_DevelopIssue] ADD CONSTRAINT CK_TSql_DevelopIssue_TextState
        CHECK ([TextState] IN (N'OPEN', N'IN_PROGRESS', N'CLOSED'));
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DevelopIssue_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelopIssue')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DevelopIssue_TextLabel_Active
        ON [dbo].[TSql_DevelopIssue] ([TextLabel])
        INCLUDE ([Is_Active])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DevelopIssue_State_Date'
      AND object_id = OBJECT_ID(N'dbo.TSql_DevelopIssue')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DevelopIssue_State_Date
        ON [dbo].[TSql_DevelopIssue] ([TextState], [DateIssue])
        INCLUDE ([TextType], [TextUrgency], [TextLabel])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DevelopIssue creada (o ya existía).';
GO
