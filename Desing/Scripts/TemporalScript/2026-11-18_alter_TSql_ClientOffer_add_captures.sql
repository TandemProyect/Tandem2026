/* ============================================================================
   OfertaCliente - capturas de planos adjuntas a la oferta.
   Idempotente para develop y producción.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferCapture'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferCapture]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferCapture PRIMARY KEY CLUSTERED,
        [TextLabel]             NVARCHAR(500)   NULL,
        [LinkClientOffer]       BIGINT          NOT NULL,
        [NumberOrder]           INT             NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_NumberOrder DEFAULT (1),
        [TextImagePath]         NVARCHAR(1000)  NOT NULL,
        [Is_Delete]             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_Is_Delete DEFAULT (0),
        [Is_Active]             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_Is_Active DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_Ntimeschanged DEFAULT (0)
    );
END
GO

IF COL_LENGTH('dbo.TSql_ClientOfferCapture', 'NumberOrder') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOfferCapture] ADD [NumberOrder] INT NOT NULL CONSTRAINT DF_TSql_ClientOfferCapture_NumberOrder_2 DEFAULT (1);
GO

IF COL_LENGTH('dbo.TSql_ClientOfferCapture', 'TextImagePath') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOfferCapture] ADD [TextImagePath] NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferCapture_ClientOffer')
    ALTER TABLE [dbo].[TSql_ClientOfferCapture]
        ADD CONSTRAINT FK_TSql_ClientOfferCapture_ClientOffer
        FOREIGN KEY ([LinkClientOffer]) REFERENCES [dbo].[TSql_ClientOffer]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientOfferCapture_Offer_Order'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferCapture')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferCapture_Offer_Order
        ON dbo.TSql_ClientOfferCapture(LinkClientOffer, NumberOrder, IdObject)
        INCLUDE (TextImagePath, TextLabel)
        WHERE Is_Delete = 0;
GO

;WITH MissingOrder AS
(
    SELECT IdObject,
           NewOrder = ROW_NUMBER() OVER (
               PARTITION BY LinkClientOffer
               ORDER BY AddDateMade, IdObject
           )
      FROM dbo.TSql_ClientOfferCapture
     WHERE Is_Delete = 0
       AND (NumberOrder IS NULL OR NumberOrder <= 0)
)
UPDATE c
   SET NumberOrder = m.NewOrder
  FROM dbo.TSql_ClientOfferCapture c
  JOIN MissingOrder m ON m.IdObject = c.IdObject;
GO

PRINT N'OK — OfertaCliente capturas aplicadas.';
GO
