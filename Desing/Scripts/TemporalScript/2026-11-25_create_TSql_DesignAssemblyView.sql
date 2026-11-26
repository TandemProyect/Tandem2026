/* =============================================================================
   US «Generar plano de montaje» — vistas por cara de cada muro.

   dbo.TSql_DesignAssemblyView        Una fila por cara (interior / exterior) de
                                      cada muro del diseño. NumberFaceId = F-n del
                                      visor (Develop › ID Faces).
   dbo.TSql_DesignAssemblyViewElement Piezas de esa cara (paneles, uniones, …)
                                      con posición y rotación para renderizar.

   Al regenerar, las vistas anteriores del diseño pasan a Is_Delete = 1.
   LinkDesignWallArticle no lleva FK: la purga de tandas antiguas de
   TSql_DesignWallArticle borra filas y la FK la bloquearía.
   Idempotente: develop y producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; -- índices filtrados (sqlcmd lo trae en OFF)
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesignAssemblyView'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesignAssemblyView]
    (
        /* ----- Clave primaria ----- */
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesignAssemblyView PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [LinkDesign_V2]         BIGINT          NOT NULL,
        [LinkDesignWall]        BIGINT          NOT NULL,   -- eje del muro (TextWallRole = 'axis')
        [NumberFaceId]          INT             NOT NULL,   -- F-n
        [TextFaceName]          NVARCHAR(100)   NOT NULL,   -- «F-3 Exterior»
        [TextFaceSide]          NVARCHAR(20)    NOT NULL,   -- Exterior | Interior
        [NumberWallAngleDeg]    FLOAT           NOT NULL,   -- eje en planta, [0, 180)
        [NumberFaceNormalDeg]   FLOAT           NOT NULL,   -- hacia dónde mira la cara, [0, 360)
        [NumberP1X]             FLOAT           NOT NULL,   -- cara en mm de escena, P1 a la izquierda
        [NumberP1Z]             FLOAT           NOT NULL,   -- mirándola de frente
        [NumberP2X]             FLOAT           NOT NULL,
        [NumberP2Z]             FLOAT           NOT NULL,
        [NumberBaseY]           FLOAT           NOT NULL,
        [NumberLengthMm]        FLOAT           NOT NULL,
        [NumberHeightMm]        FLOAT           NOT NULL,
        [NumberElementCount]    INT             NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyView_NumberElementCount DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyView_Is_Delete         DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyView_Is_Active         DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyView_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyView_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DesignAssemblyView_TSql_Design_V2')
    ALTER TABLE [dbo].[TSql_DesignAssemblyView]
        ADD CONSTRAINT FK_TSql_DesignAssemblyView_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2]) REFERENCES [dbo].[TSql_Design_V2]([SysObjectID]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DesignAssemblyView_TSql_DesignWall')
    ALTER TABLE [dbo].[TSql_DesignAssemblyView]
        ADD CONSTRAINT FK_TSql_DesignAssemblyView_TSql_DesignWall
        FOREIGN KEY ([LinkDesignWall]) REFERENCES [dbo].[TSql_DesignWall]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignAssemblyView_Design_Face'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignAssemblyView')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignAssemblyView_Design_Face
        ON [dbo].[TSql_DesignAssemblyView] ([LinkDesign_V2], [NumberFaceId])
        INCLUDE ([TextFaceName], [NumberWallAngleDeg])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesignAssemblyViewElement'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesignAssemblyViewElement]
    (
        /* ----- Clave primaria ----- */
        [IdObject]                  BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesignAssemblyViewElement PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]                 NVARCHAR(500)   NOT NULL,
        [LinkDesignAssemblyView]    BIGINT          NOT NULL,
        [LinkDesignWallArticle]     BIGINT          NULL,       -- sin FK (purga de tandas)
        [LinkMasterArticle]         BIGINT          NULL,
        [TextCode]                  NVARCHAR(50)    NULL,
        [NumberSequence]            BIGINT          NOT NULL,
        [NumberInsertX]             FLOAT           NOT NULL,   -- escena, mm (Y arriba)
        [NumberInsertY]             FLOAT           NOT NULL,
        [NumberInsertZ]             FLOAT           NOT NULL,
        [NumberXRotation]           FLOAT           NOT NULL,   -- grados, como TSql_DesignWallArticle
        [NumberYRotation]           FLOAT           NOT NULL,
        [NumberZRotation]           FLOAT           NOT NULL,
        [NumberLocalX]              FLOAT           NOT NULL,   -- a lo largo de la cara desde P1
        [NumberLocalY]              FLOAT           NOT NULL,   -- altura desde NumberBaseY
        [NumberLocalDepth]          FLOAT           NOT NULL,   -- delante (+) o detrás (−) de la cara
        [Is_Through]                BIT             NOT NULL    -- atraviesa el muro (tirante): sale en las dos caras
            CONSTRAINT DF_TSql_DesignAssemblyViewElement_Is_Through      DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]                 BIT             NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyViewElement_Is_Delete       DEFAULT (0),
        [Is_Active]                 BIT             NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyViewElement_Is_Active       DEFAULT (1),
        [LinkMadeBy]                NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]             NVARCHAR(128)   NULL,
        [AddDateMade]               DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyViewElement_AddDateMade     DEFAULT (GETDATE()),
        [AddLastDateChange]         DATETIME        NULL,
        [Ntimeschanged]             BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DesignAssemblyViewElement_Ntimeschanged   DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DesignAssemblyViewElement_View')
    ALTER TABLE [dbo].[TSql_DesignAssemblyViewElement]
        ADD CONSTRAINT FK_TSql_DesignAssemblyViewElement_View
        FOREIGN KEY ([LinkDesignAssemblyView]) REFERENCES [dbo].[TSql_DesignAssemblyView]([IdObject]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DesignAssemblyViewElement_Tsql_Master_Articles')
    ALTER TABLE [dbo].[TSql_DesignAssemblyViewElement]
        ADD CONSTRAINT FK_TSql_DesignAssemblyViewElement_Tsql_Master_Articles
        FOREIGN KEY ([LinkMasterArticle]) REFERENCES [dbo].[Tsql_Master_Articles]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignAssemblyViewElement_View'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignAssemblyViewElement')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignAssemblyViewElement_View
        ON [dbo].[TSql_DesignAssemblyViewElement] ([LinkDesignAssemblyView], [NumberSequence])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesignAssemblyView y dbo.TSql_DesignAssemblyViewElement creadas (o ya existían).';
GO
