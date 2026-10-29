/* =============================================================================
 dbo.TSql_DesingFasesDetail — fases de encofrado de un diseño.
 Una fila por muro: NumberPhase es la fase, TextWallId es el id del muro
 en el visor. LinkDesign_V2 apunta a dbo.TSql_Design_V2.SysObjectID.
 Si esa tabla no está en la base, la tabla se crea igual y la clave
 foránea se añade cuando el padre exista.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesingFasesDetail'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesingFasesDetail]
    (
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesingFasesDetail PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,
        [NumberPhase] BIGINT NOT NULL,
        [TextWallId] NVARCHAR(128) NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesingFasesDetail_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesingFasesDetail_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesingFasesDetail_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesingFasesDetail_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesingFasesDetail_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesingFasesDetail')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesingFasesDetail] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesingFasesDetail_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_DesingFasesDetail_Design_Wall'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesingFasesDetail')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_DesingFasesDetail_Design_Wall
    ON [dbo].[TSql_DesingFasesDetail] ([LinkDesign_V2], [TextWallId])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesingFasesDetail_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesingFasesDetail')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesingFasesDetail_TextLabel_Active
    ON [dbo].[TSql_DesingFasesDetail] ([TextLabel])
    INCLUDE ([Is_Active])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesingFasesDetail_Design_Phase'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesingFasesDetail')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesingFasesDetail_Design_Phase
    ON [dbo].[TSql_DesingFasesDetail] ([LinkDesign_V2], [NumberPhase])
    INCLUDE ([TextWallId], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesingFasesDetail creada (o ya existía).';
GO
