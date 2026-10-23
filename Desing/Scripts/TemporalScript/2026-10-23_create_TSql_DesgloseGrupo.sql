/* =============================================================================
 dbo.TSql_DesgloseGrupo — Nombre de cada grupo de un diseño.
 Una fila por diseño + número de grupo. TextLabel es el nombre del grupo.
 El mismo número activo no se repite dentro de un diseño.

 Padre: dbo.TSql_Design_V2.SysObjectID
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesgloseGrupo'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesgloseGrupo]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesgloseGrupo PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,
        [NumberGroup] BIGINT NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesgloseGrupo_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesgloseGrupo_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesgloseGrupo_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesgloseGrupo_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesgloseGrupo_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesgloseGrupo] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesgloseGrupo_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_DesgloseGrupo_Design_NumberGroup'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_DesgloseGrupo_Design_NumberGroup
    ON [dbo].[TSql_DesgloseGrupo] ([LinkDesign_V2], [NumberGroup])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesgloseGrupo_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesgloseGrupo_TextLabel_Active
    ON [dbo].[TSql_DesgloseGrupo] ([TextLabel])
    INCLUDE ([Is_Active])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesgloseGrupo_LinkDesign_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesgloseGrupo_LinkDesign_Active
    ON [dbo].[TSql_DesgloseGrupo] ([LinkDesign_V2])
    INCLUDE ([NumberGroup], [TextLabel], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesgloseGrupo creada (o ya existía).';
GO
