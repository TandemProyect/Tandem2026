/* =============================================================================
 dbo.TSql_DesignWallArticle — Artículos insertados a mano en un muro especial.
 Al cargar el diseño se vuelven a insertar. Padre: TSql_DesignWall.IdObject.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesignWallArticle'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesignWallArticle]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesignWallArticle PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,
        [LinkDesignWall] BIGINT NOT NULL,
        [LinkMasterArticle] BIGINT NULL,
        [TextCode] NVARCHAR(50) NULL,
        [TextView] NVARCHAR(20) NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_TextView DEFAULT (N'3D'),
        [NumberSequence] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberSequence DEFAULT (0),
        [NumberInsertX] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberInsertX DEFAULT (0),
        [NumberInsertY] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberInsertY DEFAULT (0),
        [NumberInsertZ] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberInsertZ DEFAULT (0),
        [NumberXRotation] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberXRotation DEFAULT (0),
        [NumberYRotation] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberYRotation DEFAULT (0),
        [NumberZRotation] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_NumberZRotation DEFAULT (0),
        [TextHandleCad] NVARCHAR(50) NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesignWallArticle_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesignWallArticle_TSql_DesignWall'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
   AND OBJECT_ID(N'dbo.TSql_DesignWall', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWallArticle] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesignWallArticle_TSql_DesignWall
        FOREIGN KEY ([LinkDesignWall])
        REFERENCES [dbo].[TSql_DesignWall] ([IdObject]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesignWallArticle_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWallArticle] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesignWallArticle_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesignWallArticle_Tsql_Master_Articles'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
   AND OBJECT_ID(N'dbo.Tsql_Master_Articles', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWallArticle] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesignWallArticle_Tsql_Master_Articles
        FOREIGN KEY ([LinkMasterArticle])
        REFERENCES [dbo].[Tsql_Master_Articles] ([IdObject]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWallArticle_LinkDesignWall_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWallArticle_LinkDesignWall_Active
    ON [dbo].[TSql_DesignWallArticle] ([LinkDesignWall], [NumberSequence])
    INCLUDE ([TextCode], [TextView], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWallArticle_LinkDesign_V2_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWallArticle')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWallArticle_LinkDesign_V2_Active
    ON [dbo].[TSql_DesignWallArticle] ([LinkDesign_V2])
    INCLUDE ([LinkDesignWall], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesignWallArticle creada (o ya existía).';
GO
