/* =============================================================================
   Oferta al cliente (PDF editable, aún sin motor PDF).
   - TSql_ClientOfferBrand: logo y textos generales por empresa y, si se indica, sistema.
   - TSql_ClientOffer: documento editable enlazado a diseño, oferta, obra, cliente,
     empresa, plantilla y sistema.
   - TSql_ClientOfferSection: apartados (ENCABEZADO, TEXTO y los que se añadan).
   Solo develop en esta pasada. El esquema se copiará a producción cuando Juan lo pida.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferBrand'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferBrand]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferBrand PRIMARY KEY CLUSTERED,

        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [TextCode]              NVARCHAR(100)   NULL,
        [LinkCompany]           BIGINT          NOT NULL,
        [LinkSystem]            BIGINT          NULL,
        [LinkPlantilla]         BIGINT          NULL,
        [TextLogo]              NVARCHAR(500)   NULL,
        [TextCompanyName]       NVARCHAR(500)   NULL,
        [TextSubjectTemplate]   NVARCHAR(500)   NULL,
        [TextHeaderTemplate]    NVARCHAR(MAX)   NULL,
        [TextBodyTemplate]      NVARCHAR(MAX)   NULL,

        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrand_Is_Delete         DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrand_Is_Active         DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrand_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrand_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferBrand_Company'
)
    ALTER TABLE [dbo].[TSql_ClientOfferBrand]
        ADD CONSTRAINT FK_TSql_ClientOfferBrand_Company
        FOREIGN KEY ([LinkCompany]) REFERENCES [dbo].[TSql_Company] ([SysObjectID]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferBrand_System'
)
    ALTER TABLE [dbo].[TSql_ClientOfferBrand]
        ADD CONSTRAINT FK_TSql_ClientOfferBrand_System
        FOREIGN KEY ([LinkSystem]) REFERENCES [dbo].[TSql_System] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferBrand_Plantilla'
)
    ALTER TABLE [dbo].[TSql_ClientOfferBrand]
        ADD CONSTRAINT FK_TSql_ClientOfferBrand_Plantilla
        FOREIGN KEY ([LinkPlantilla]) REFERENCES [dbo].[TSql_Plantilla] ([SysObjectID]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_ClientOfferBrand_Company_System'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrand')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferBrand_Company_System
        ON [dbo].[TSql_ClientOfferBrand] ([LinkCompany], [LinkSystem])
        WHERE [Is_Delete] = 0 AND [LinkSystem] IS NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_ClientOfferBrand_Company_AllSystems'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrand')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferBrand_Company_AllSystems
        ON [dbo].[TSql_ClientOfferBrand] ([LinkCompany])
        WHERE [Is_Delete] = 0 AND [LinkSystem] IS NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOffer'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOffer]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOffer PRIMARY KEY CLUSTERED,

        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [TextCode]              NVARCHAR(50)    NULL,
        [LinkDesign]            BIGINT          NULL,
        [LinkOffers]            BIGINT          NULL,
        [LinkJobside]           BIGINT          NULL,
        [LinkClient_V2]         BIGINT          NULL,
        [LinkCompany]           BIGINT          NOT NULL,
        [LinkPlantilla]         BIGINT          NULL,
        [LinkSystem]            BIGINT          NOT NULL,
        [LinkClientOfferBrand]  BIGINT          NULL,
        [TextSubject]           NVARCHAR(500)   NOT NULL,
        [TextClientAddress]     NVARCHAR(1000)  NULL,
        [TextLogo]              NVARCHAR(500)   NULL,
        [TextRecuento]          NVARCHAR(MAX)   NULL,
        [TextStatus]            NVARCHAR(50)    NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_TextStatus DEFAULT (N'BORRADOR'),
        [TextPdfPath]           NVARCHAR(500)   NULL,
        [AddPdfDate]            DATETIME        NULL,

        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_Is_Delete         DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_Is_Active         DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Design')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Design
        FOREIGN KEY ([LinkDesign]) REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Offers')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Offers
        FOREIGN KEY ([LinkOffers]) REFERENCES [dbo].[TSql_Offers] ([IdObject]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Jobside')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Jobside
        FOREIGN KEY ([LinkJobside]) REFERENCES [dbo].[TSql_Jobside] ([IdObject]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Client')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Client
        FOREIGN KEY ([LinkClient_V2]) REFERENCES [dbo].[TSql_Client_V2] ([IdObject]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Company')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Company
        FOREIGN KEY ([LinkCompany]) REFERENCES [dbo].[TSql_Company] ([SysObjectID]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Plantilla')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Plantilla
        FOREIGN KEY ([LinkPlantilla]) REFERENCES [dbo].[TSql_Plantilla] ([SysObjectID]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_System')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_System
        FOREIGN KEY ([LinkSystem]) REFERENCES [dbo].[TSql_System] ([IdObject]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Brand')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Brand
        FOREIGN KEY ([LinkClientOfferBrand]) REFERENCES [dbo].[TSql_ClientOfferBrand] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientOffer_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOffer')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOffer_TextLabel_Active
        ON [dbo].[TSql_ClientOffer] ([TextLabel])
        INCLUDE ([Is_Active], [LinkSystem], [TextStatus])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferSection'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferSection]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferSection PRIMARY KEY CLUSTERED,

        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [LinkClientOffer]       BIGINT          NOT NULL,
        [TextCode]              NVARCHAR(50)    NOT NULL,
        [NumberOrder]           INT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferSection_NumberOrder DEFAULT (0),
        [TextBody]              NVARCHAR(MAX)   NULL,

        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferSection_Is_Delete         DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferSection_Is_Active         DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_ClientOffer_Section_AddDateMade      DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_ClientOfferSection_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferSection_Offer')
    ALTER TABLE [dbo].[TSql_ClientOfferSection]
        ADD CONSTRAINT FK_TSql_ClientOfferSection_Offer
        FOREIGN KEY ([LinkClientOffer]) REFERENCES [dbo].[TSql_ClientOffer] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientOfferSection_Offer_Order'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferSection')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferSection_Offer_Order
        ON [dbo].[TSql_ClientOfferSection] ([LinkClientOffer], [NumberOrder])
        INCLUDE ([TextCode])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferAiMessage'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferAiMessage]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferAiMessage PRIMARY KEY CLUSTERED,

        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [LinkClientOffer]       BIGINT          NOT NULL,
        [TextRole]              NVARCHAR(50)    NOT NULL,
        [TextMessage]           NVARCHAR(MAX)   NOT NULL,
        [AddPayloadJson]        NVARCHAR(MAX)   NULL,

        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferAiMessage_Is_Delete         DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferAiMessage_Is_Active         DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_ClientOfferAiMessage_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_ClientOfferAiMessage_Ntimeschanged     DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferAiMessage_Offer'
)
    ALTER TABLE [dbo].[TSql_ClientOfferAiMessage]
        ADD CONSTRAINT FK_TSql_ClientOfferAiMessage_Offer
        FOREIGN KEY ([LinkClientOffer]) REFERENCES [dbo].[TSql_ClientOffer] ([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientOfferAiMessage_Offer_Date'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferAiMessage')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferAiMessage_Offer_Date
        ON [dbo].[TSql_ClientOfferAiMessage] ([LinkClientOffer], [AddDateMade])
        INCLUDE ([TextRole])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_ClientOffer*, incluida TSql_ClientOfferAiMessage, creadas (o ya existían).';
GO
