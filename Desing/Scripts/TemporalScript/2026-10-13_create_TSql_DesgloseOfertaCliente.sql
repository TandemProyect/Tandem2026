/* =============================================================================
 dbo.TSql_DesgloseOfertaCliente — Línea de desglose: diseño insertado
 en un cliente, dentro de una oferta.
 El diseño sigue naciendo en TSql_Design_V2 (LinkOffers obligatorio).
 Esta tabla guarda el desglose: cliente + oferta + diseño.
 La misma terna activa no se repite.

 Sustituye a dbo.TSql_ClientDesign, creada vacía el mismo día con otro nombre.
 Solo se elimina si no tiene filas.

 Padres:
   dbo.TSql_Client_V2.IdObject
   dbo.TSql_Offers.IdObject
   dbo.TSql_Design_V2.SysObjectID
 ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.TSql_ClientDesign', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_ClientDesign)
BEGIN
    DROP TABLE dbo.TSql_ClientDesign;
    PRINT N'OK — dbo.TSql_ClientDesign eliminada (estaba vacía).';
END
ELSE IF OBJECT_ID(N'dbo.TSql_ClientDesign', N'U') IS NOT NULL
    PRINT N'Aviso — dbo.TSql_ClientDesign tiene filas; no se elimina.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DesgloseOfertaCliente'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DesgloseOfertaCliente]
    (
        /* ----- Clave primaria ----- */
        [IdObject] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DesgloseOfertaCliente PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel] NVARCHAR(500) NOT NULL,
        [LinkClient_V2] BIGINT NOT NULL,
        [LinkOffers] BIGINT NOT NULL,
        [LinkDesign_V2] BIGINT NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete] BIT NOT NULL
            CONSTRAINT DF_TSql_DesgloseOfertaCliente_Is_Delete DEFAULT (0),
        [Is_Active] BIT NOT NULL
            CONSTRAINT DF_TSql_DesgloseOfertaCliente_Is_Active DEFAULT (1),
        [LinkMadeBy] NVARCHAR(128) NOT NULL,
        [LinModifiedBy] NVARCHAR(128) NULL,
        [AddDateMade] DATETIME NOT NULL
            CONSTRAINT DF_TSql_DesgloseOfertaCliente_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange] DATETIME NULL,
        [Ntimeschanged] BIGINT NOT NULL
            CONSTRAINT DF_TSql_DesgloseOfertaCliente_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesgloseOfertaCliente_TSql_Client_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
   AND OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesgloseOfertaCliente] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesgloseOfertaCliente_TSql_Client_V2
        FOREIGN KEY ([LinkClient_V2])
        REFERENCES [dbo].[TSql_Client_V2] ([IdObject]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesgloseOfertaCliente_TSql_Offers'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
   AND OBJECT_ID(N'dbo.TSql_Offers', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesgloseOfertaCliente] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesgloseOfertaCliente_TSql_Offers
        FOREIGN KEY ([LinkOffers])
        REFERENCES [dbo].[TSql_Offers] ([IdObject]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_TSql_DesgloseOfertaCliente_TSql_Design_V2'
      AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
   AND OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TSql_DesgloseOfertaCliente] WITH CHECK
    ADD CONSTRAINT FK_TSql_DesgloseOfertaCliente_TSql_Design_V2
        FOREIGN KEY ([LinkDesign_V2])
        REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_DesgloseOfertaCliente_Client_Offer_Design'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_DesgloseOfertaCliente_Client_Offer_Design
    ON [dbo].[TSql_DesgloseOfertaCliente] ([LinkClient_V2], [LinkOffers], [LinkDesign_V2])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesgloseOfertaCliente_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesgloseOfertaCliente_TextLabel_Active
    ON [dbo].[TSql_DesgloseOfertaCliente] ([TextLabel])
    INCLUDE ([Is_Active])
    WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DesgloseOfertaCliente_LinkOffers_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseOfertaCliente')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DesgloseOfertaCliente_LinkOffers_Active
    ON [dbo].[TSql_DesgloseOfertaCliente] ([LinkOffers])
    INCLUDE ([LinkClient_V2], [LinkDesign_V2], [Is_Active])
    WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DesgloseOfertaCliente creada (o ya existía).';
GO
