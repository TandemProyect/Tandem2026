/* =============================================================================
   Oferta cliente: contactos por cliente + estados históricos + plantilla base.
   Idempotente. Ejecutar en develop y, cuando se autorice, en producción.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferContact'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferContact]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferContact PRIMARY KEY CLUSTERED,
        [TextLabel]             NVARCHAR(500)   NOT NULL,
        [LinkCompany]           BIGINT          NOT NULL,
        [LinkClient_V2]         BIGINT          NULL,
        [TextEmail]             NVARCHAR(500)   NULL,
        [TextPhone]             NVARCHAR(100)   NULL,
        [Is_Default]            BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferContact_Is_Default DEFAULT (0),
        [Is_Delete]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferContact_Is_Delete DEFAULT (0),
        [Is_Active]             BIT             NOT NULL
            CONSTRAINT DF_TSql_ClientOfferContact_Is_Active DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL
            CONSTRAINT DF_TSql_ClientOfferContact_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL
            CONSTRAINT DF_TSql_ClientOfferContact_Ntimeschanged DEFAULT (0)
    );
END
GO

IF COL_LENGTH('dbo.TSql_ClientOfferContact', 'Is_Default') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOfferContact] ADD [Is_Default] BIT NOT NULL CONSTRAINT DF_TSql_ClientOfferContact_Is_Default_2 DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferContact_Company')
    ALTER TABLE [dbo].[TSql_ClientOfferContact]
        ADD CONSTRAINT FK_TSql_ClientOfferContact_Company
        FOREIGN KEY ([LinkCompany]) REFERENCES [dbo].[TSql_Company]([SysObjectID]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferContact_Client')
    ALTER TABLE [dbo].[TSql_ClientOfferContact]
        ADD CONSTRAINT FK_TSql_ClientOfferContact_Client
        FOREIGN KEY ([LinkClient_V2]) REFERENCES [dbo].[TSql_Client_V2]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE name = N'IX_TSql_ClientOfferContact_Company_Client_Active'
       AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContact')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferContact_Company_Client_Active
        ON [dbo].[TSql_ClientOfferContact]([LinkCompany], [LinkClient_V2], [TextLabel])
        INCLUDE ([TextEmail], [Is_Default])
        WHERE [Is_Delete] = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE name = N'UX_TSql_ClientOfferContact_DefaultByClient'
       AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContact')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferContact_DefaultByClient
        ON [dbo].[TSql_ClientOfferContact]([LinkClient_V2])
        WHERE [Is_Delete] = 0 AND [Is_Default] = 1 AND [LinkClient_V2] IS NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferContactStateType'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferContactStateType]
    (
        [IdObject]              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferContactStateType PRIMARY KEY CLUSTERED,
        [TextCode]              NVARCHAR(50)    NOT NULL,
        [TextLabel]             NVARCHAR(100)   NOT NULL,
        [Is_Delete]             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferContactStateType_Is_Delete DEFAULT (0),
        [Is_Active]             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferContactStateType_Is_Active DEFAULT (1),
        [LinkMadeBy]            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]         NVARCHAR(128)   NULL,
        [AddDateMade]           DATETIME        NOT NULL CONSTRAINT DF_TSql_ClientOfferContactStateType_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange]     DATETIME        NULL,
        [Ntimeschanged]         BIGINT          NOT NULL CONSTRAINT DF_TSql_ClientOfferContactStateType_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_ClientOfferContactStateType_Code'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContactStateType')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferContactStateType_Code
        ON dbo.TSql_ClientOfferContactStateType(TextCode)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferContactState'
)
BEGIN
    CREATE TABLE [dbo].[TSql_ClientOfferContactState]
    (
        [IdObject]                              BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferContactState PRIMARY KEY CLUSTERED,
        [LinkClientOfferContact]                BIGINT          NOT NULL,
        [LinkClientOfferContactStateType]       BIGINT          NOT NULL,
        [AddStatusDate]                         DATETIME        NOT NULL CONSTRAINT DF_TSql_ClientOfferContactState_AddStatusDate DEFAULT (GETDATE()),
        [TextNotes]                             NVARCHAR(500)   NULL,
        [Is_Delete]                             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferContactState_Is_Delete DEFAULT (0),
        [Is_Active]                             BIT             NOT NULL CONSTRAINT DF_TSql_ClientOfferContactState_Is_Active DEFAULT (1),
        [LinkMadeBy]                            NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]                         NVARCHAR(128)   NULL,
        [AddDateMade]                           DATETIME        NOT NULL CONSTRAINT DF_TSql_ClientOfferContactState_AddDateMade DEFAULT (GETDATE()),
        [AddLastDateChange]                     DATETIME        NULL,
        [Ntimeschanged]                         BIGINT          NOT NULL CONSTRAINT DF_TSql_ClientOfferContactState_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferContactState_Contact')
    ALTER TABLE [dbo].[TSql_ClientOfferContactState]
        ADD CONSTRAINT FK_TSql_ClientOfferContactState_Contact
        FOREIGN KEY ([LinkClientOfferContact]) REFERENCES [dbo].[TSql_ClientOfferContact]([IdObject]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferContactState_StateType')
    ALTER TABLE [dbo].[TSql_ClientOfferContactState]
        ADD CONSTRAINT FK_TSql_ClientOfferContactState_StateType
        FOREIGN KEY ([LinkClientOfferContactStateType]) REFERENCES [dbo].[TSql_ClientOfferContactStateType]([IdObject]);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_ClientOfferContactState_Contact_Date'
      AND object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContactState')
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferContactState_Contact_Date
        ON dbo.TSql_ClientOfferContactState(LinkClientOfferContact, AddStatusDate DESC, IdObject DESC)
        INCLUDE (LinkClientOfferContactStateType)
        WHERE Is_Delete = 0;
GO

MERGE dbo.TSql_ClientOfferContactStateType AS tgt
USING (VALUES
    (N'ALTA', N'Alta'),
    (N'BAJA', N'Baja'),
    (N'ENFERMO', N'Enfermo')
) AS src(TextCode, TextLabel)
ON tgt.TextCode = src.TextCode AND tgt.Is_Delete = 0
WHEN NOT MATCHED THEN
    INSERT (TextCode, TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES (src.TextCode, src.TextLabel, 0, 1, N'system', NULL, GETDATE(), NULL, 0);
GO

/* Campos nuevos en oferta */
IF COL_LENGTH('dbo.TSql_ClientOffer', 'LinkClientOfferContact') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOffer] ADD [LinkClientOfferContact] BIGINT NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextContactName') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOffer] ADD [TextContactName] NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextContactEmail') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOffer] ADD [TextContactEmail] NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextMaterial') IS NULL
    ALTER TABLE [dbo].[TSql_ClientOffer] ADD [TextMaterial] NVARCHAR(MAX) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Contact')
    ALTER TABLE [dbo].[TSql_ClientOffer]
        ADD CONSTRAINT FK_TSql_ClientOffer_Contact
        FOREIGN KEY ([LinkClientOfferContact]) REFERENCES [dbo].[TSql_ClientOfferContact]([IdObject]);
GO

/* Para contactos sin estado, crear ALTA inicial */
INSERT INTO dbo.TSql_ClientOfferContactState
    (LinkClientOfferContact, LinkClientOfferContactStateType, AddStatusDate, TextNotes,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT
    c.IdObject,
    st.IdObject,
    ISNULL(c.AddDateMade, GETDATE()),
    N'Estado inicial',
    0, 1, N'system', NULL, GETDATE(), NULL, 0
FROM dbo.TSql_ClientOfferContact c
CROSS JOIN (
    SELECT TOP (1) IdObject
    FROM dbo.TSql_ClientOfferContactStateType
    WHERE Is_Delete = 0 AND TextCode = N'ALTA'
) st
WHERE c.Is_Delete = 0
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.TSql_ClientOfferContactState s
      WHERE s.Is_Delete = 0
        AND s.LinkClientOfferContact = c.IdObject
  );
GO

/* Plantilla base inicial por empresa/sistema nulo */
INSERT INTO dbo.TSql_ClientOfferBrand
    (TextLabel, TextCode, LinkCompany, LinkSystem, LinkPlantilla, TextLogo, TextCompanyName,
     TextSubjectTemplate, TextHeaderTemplate, TextBodyTemplate,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT
    N'Plantilla base oferta',
    N'BASE',
    c.SysObjectID,
    NULL,
    c.LinPlantilla,
    p.AttLogo,
    c.TextLabel,
    N'Oferta {Sistema} — {Obra}',
    N'{Empresa}' + CHAR(13) + CHAR(10) + N'{DireccionEmpresa}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
      + N'Dirección cliente: {DireccionCliente}' + CHAR(13) + CHAR(10)
      + N'Contacto: {NombreContacto} ({EmailContacto})' + CHAR(13) + CHAR(10)
      + N'Asunto: {Asunto}',
    N'Diseño: {Diseno}' + CHAR(13) + CHAR(10)
      + N'Oferta: {Oferta}' + CHAR(13) + CHAR(10)
      + N'Obra: {Obra}' + CHAR(13) + CHAR(10)
      + N'Dirección cliente: {DireccionCliente}' + CHAR(13) + CHAR(10)
      + N'Contacto: {NombreContacto} ({EmailContacto})' + CHAR(13) + CHAR(10)
      + N'Empresa: {Empresa}' + CHAR(13) + CHAR(10)
      + N'Sistema: {Sistema}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
      + N'Materiales:' + CHAR(13) + CHAR(10) + N'{Material}' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
      + N'Recuento:' + CHAR(13) + CHAR(10) + N'{Recuento}',
    0, 1, N'system', NULL, GETDATE(), NULL, 0
FROM dbo.TSql_Company c
LEFT JOIN dbo.TSql_Plantilla p ON p.SysObjectID = c.LinPlantilla AND p.AttIsDeleted = 0
WHERE c.BitIsDeleted = 0
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.TSql_ClientOfferBrand b
      WHERE b.Is_Delete = 0
        AND b.LinkCompany = c.SysObjectID
        AND b.LinkSystem IS NULL
  );
GO

PRINT N'OK — OfertaCliente: contactos por cliente, estados y plantilla base aplicados.';
GO
