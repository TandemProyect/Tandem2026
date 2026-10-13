/* =============================================================================
 dbo.TSql_ClientDesign — Diseño adjuntado a un cliente.
 El diseño sigue perteneciendo a su oferta (TSql_Design_V2.LinkOffers).
 Esta tabla es el adjunto: un cliente puede tener varios diseños y un diseño
 puede adjuntarse a varios clientes. La pareja activa no se repite.
 Padre cliente: dbo.TSql_Client_V2.IdObject
 Padre diseño:  dbo.TSql_Design_V2.SysObjectID
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_ClientDesign'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientDesign]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientDesign PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkClient_V2] BIGINT NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_ClientDesign_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_ClientDesign_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_ClientDesign_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_ClientDesign_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_ClientDesign_TSql_Client_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_ClientDesign')
)
   AND OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_ClientDesign] WITH CHECK
    ADD CONSTRAINT FK_TSql_ClientDesign_TSql_Client_V2
        FOREIGN KEY ([LinkClient_V2])
        REFERENCES [dbo].[TSql_Client_V2] ([IdObject]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_ClientDesign_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_ClientDesign')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_ClientDesign] WITH CHECK
    ADD CONSTRAINT FK_TSql_ClientDesign_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_ClientDesign_Client_Design_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientDesign')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientDesign_Client_Design_Active
    ON [dbo].[TSql_ClientDesign] ([LinkClient_V2], [LinkDesign_V2])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientDesign_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientDesign')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientDesign_TextLabel_Active
    ON [dbo].[TSql_ClientDesign] ([TextLabel])
    INCLUDE ([Is_Active])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientDesign_LinkDesign_V2_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientDesign')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientDesign_LinkDesign_V2_Active
    ON [dbo].[TSql_ClientDesign] ([LinkDesign_V2])
    INCLUDE ([LinkClient_V2], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_ClientDesign creada (o ya existía).';
GO
