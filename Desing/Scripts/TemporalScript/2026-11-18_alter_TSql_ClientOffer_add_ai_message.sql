/* =============================================================================
   dbo.TSql_ClientOfferAiMessage — conversación IA para la oferta cliente.
   Guarda turnos de usuario y asistente enlazados a TSql_ClientOffer.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
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

PRINT N'OK — dbo.TSql_ClientOfferAiMessage creada (o ya existía).';
GO
