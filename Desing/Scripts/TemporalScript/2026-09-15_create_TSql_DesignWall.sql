/* =============================================================================
 dbo.TSql_DesignWall — Muros del visor Desing_2 (un registro por segmento).
 Padre: dbo.TSql_Design_V2.SysObjectID (cabecera de diseño V2; no usa IdObject).
 Atributos alineados con el visor (MA_STL_WALL_LINE_ATTR_KEYS) y con
 TSql_DesignDetails (solución de encofrado). Nuevos atributos: ALTER + IF NOT EXISTS.
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesignWall'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesignWall]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesignWall PRIMARY KEY CLUSTERED,

        /* ----- Identidad / padre ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,
        [TextClientWallId] NVARCHAR(128) NULL,
        [NumberLineId] BIGINT NULL,
        [NumberWallGroupId] BIGINT NULL,
        [NumberPolylineGroupId] BIGINT NULL,
        [TextWallRole] NVARCHAR(50) NULL,
        [NumberLinkOffsetFromLineId] BIGINT NULL,
        [NumberOffsetMm] FLOAT NULL,
        [NumberWallFaceSideSign] FLOAT NULL,
        [TextSystem] NVARCHAR(50) NULL,

        /* ----- Geometria escena (mm, plano XZ) ----- */
        [NumberP1X] FLOAT NOT NULL,
        [NumberP1Y] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_NumberP1Y DEFAULT (0),
        [NumberP1Z] FLOAT NOT NULL,
        [NumberP2X] FLOAT NOT NULL,
        [NumberP2Y] FLOAT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_NumberP2Y DEFAULT (0),
        [NumberP2Z] FLOAT NOT NULL,
        [NumberDrawP1X] FLOAT NULL,
        [NumberDrawP1Y] FLOAT NULL,
        [NumberDrawP1Z] FLOAT NULL,
        [NumberDrawP2X] FLOAT NULL,
        [NumberDrawP2Y] FLOAT NULL,
        [NumberDrawP2Z] FLOAT NULL,

        /* ----- Atributos visor (AttributesList) ----- */
        [TextTypeMesh] NVARCHAR(50) NULL,
        [NumberLong] FLOAT NULL,
        [NumberWidth] FLOAT NULL,
        [NumberHeight] FLOAT NULL,
        [NumberXRotation] FLOAT NULL,
        [NumberYRotation] FLOAT NULL,
        [NumberZRotation] FLOAT NULL,
        [NumberXCoordinate] FLOAT NULL,
        [NumberYCoordinate] FLOAT NULL,
        [NumberZCoordinate] FLOAT NULL,
        [Is_Formwork] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Formwork DEFAULT (1),
        [Is_UniversalPanel] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_UniversalPanel DEFAULT (1),
        [TextTape_1] NVARCHAR(50) NULL,
        [TextTape_2] NVARCHAR(50) NULL,
        [TextIdConnection_1] NVARCHAR(128) NULL,
        [TextIdConnection_2] NVARCHAR(128) NULL,
        [Is_CheckBracketInside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckBracketInside DEFAULT (1),
        [Is_CheckBracketOutside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckBracketOutside DEFAULT (1),
        [Is_CheckRijiInside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckRijiInside DEFAULT (1),
        [Is_CheckRijiOutside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckRijiOutside DEFAULT (1),
        [Is_CheckPropInside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckPropInside DEFAULT (1),
        [Is_CheckPropOutside] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckPropOutside DEFAULT (1),
        [Is_CheckPropInsideInf] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckPropInsideInf DEFAULT (1),
        [Is_CheckPropOutsideInf] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckPropOutsideInf DEFAULT (1),

        /* ----- Solucion de encofrado (crecera con ALTER) ----- */
        [TextTape_0] NVARCHAR(50) NULL,
        [TextTape_90] NVARCHAR(50) NULL,
        [TextTape_180] NVARCHAR(50) NULL,
        [TextTape_270] NVARCHAR(50) NULL,
        [TextTypeWall] NVARCHAR(50) NULL,
        [TextTypeWallLeft] NVARCHAR(50) NULL,
        [TextTypeWallRight] NVARCHAR(50) NULL,
        [TextTypeWall_0] NVARCHAR(50) NULL,
        [TextTypeWall_90] NVARCHAR(50) NULL,
        [TextTypeWall_180] NVARCHAR(50) NULL,
        [TextTypeWall_270] NVARCHAR(50) NULL,
        [TextSubLong_0] NVARCHAR(50) NULL,
        [TextSubLong_90] NVARCHAR(50) NULL,
        [TextSubLong_180] NVARCHAR(50) NULL,
        [TextSubLong_270] NVARCHAR(50) NULL,
        [TextIdWall_0] NVARCHAR(128) NULL,
        [TextIdWall_90] NVARCHAR(128) NULL,
        [TextIdWall_180] NVARCHAR(128) NULL,
        [TextIdWall_270] NVARCHAR(128) NULL,
        [TextGroup] NVARCHAR(128) NULL,
        [Is_PilarType] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_PilarType DEFAULT (0),
        [Is_FormworkMode] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_FormworkMode DEFAULT (0),
        [Is_Panel060] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Panel060 DEFAULT (0),
        [Is_CheckDimWall] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_CheckDimWall DEFAULT (0),
        [Is_Check750R] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Check750R DEFAULT (0),
        [Is_SolutionCornerXUniversalPanelCorner] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_SolutionCornerX DEFAULT (0),
        [Is_SolutionCornerYUniversalPanelCorner] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_SolutionCornerY DEFAULT (0),
        [NumberRemate_0] INT NULL,
        [NumberRemate_90] INT NULL,
        [NumberRemate_180] INT NULL,
        [NumberRemate_270] INT NULL,
        [NumberRematePositionX] FLOAT NULL,
        [NumberRematePositionXMirror] FLOAT NULL,
        [NumberRematePosition90Y] FLOAT NULL,
        [NumberRematePosition90YMirror] FLOAT NULL,
        [NumberRematePosition180X] FLOAT NULL,
        [NumberRematePosition180XMirror] FLOAT NULL,
        [NumberRematePosition270Y] FLOAT NULL,
        [NumberRematePosition270YMirror] FLOAT NULL,
        [NumberLongLeft] BIGINT NULL,
        [NumberLongRight] BIGINT NULL,
        [NumberInitialWall] BIGINT NULL,
        [NumberEndWall] BIGINT NULL,
        [NumberIdCornerDown] BIGINT NULL,
        [NumberIdCornerLeft] BIGINT NULL,
        [NumberScaleEsqY] BIGINT NULL,

        /* ----- Auditoria obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesignWall_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesignWall_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesignWall_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesignWall] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesignWall_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWall_LinkDesign_V2_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWall_LinkDesign_V2_Active
    ON [dbo].[TSql_DesignWall] ([LinkDesign_V2], [TextClientWallId])
    INCLUDE ([Is_Active], [TextWallRole], [NumberLineId])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesignWall_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesignWall')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesignWall_TextLabel_Active
    ON [dbo].[TSql_DesignWall] ([TextLabel])
    INCLUDE ([Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesignWall creada (o ya existía).';
GO
