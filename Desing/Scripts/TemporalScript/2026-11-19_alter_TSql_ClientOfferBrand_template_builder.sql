/* =============================================================================
   Refactor Plantilla Oferta (constructor visual)
   - Añade layout JSON a TSql_ClientOfferBrand.
   - Crea N logos por plantilla (TSql_ClientOfferBrandLogo).
   - Crea N textos por plantilla (TSql_ClientOfferBrandText).
   - Migra datos existentes (TextLogo, TextHeaderTemplate, TextBodyTemplate).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.TSql_ClientOfferBrand', N'TextLayoutJson') IS NULL
    ALTER TABLE dbo.TSql_ClientOfferBrand ADD TextLayoutJson NVARCHAR(MAX) NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrand')
       AND name = N'UX_TSql_ClientOfferBrand_Company_System'
)
    DROP INDEX UX_TSql_ClientOfferBrand_Company_System ON dbo.TSql_ClientOfferBrand;
GO

IF EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrand')
       AND name = N'UX_TSql_ClientOfferBrand_Company_AllSystems'
)
    DROP INDEX UX_TSql_ClientOfferBrand_Company_AllSystems ON dbo.TSql_ClientOfferBrand;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrand')
       AND name = N'UX_TSql_ClientOfferBrand_Company_Name'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferBrand_Company_Name
        ON dbo.TSql_ClientOfferBrand(LinkCompany, TextLabel)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferBrandLogo'
)
BEGIN
    CREATE TABLE dbo.TSql_ClientOfferBrandLogo
    (
        IdObject              BIGINT        IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferBrandLogo PRIMARY KEY CLUSTERED,
        TextLabel             NVARCHAR(500) NOT NULL,
        LinkClientOfferBrand  BIGINT        NOT NULL,
        NumberOrder           INT           NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandLogo_NumberOrder DEFAULT (0),
        TextImagePath         NVARCHAR(1000) NOT NULL,
        Is_Delete             BIT           NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandLogo_Is_Delete DEFAULT (0),
        Is_Active             BIT           NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandLogo_Is_Active DEFAULT (1),
        LinkMadeBy            NVARCHAR(128) NOT NULL,
        LinModifiedBy         NVARCHAR(128) NULL,
        AddDateMade           DATETIME      NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandLogo_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange     DATETIME      NULL,
        Ntimeschanged         BIGINT        NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandLogo_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferBrandLogo_Brand')
    ALTER TABLE dbo.TSql_ClientOfferBrandLogo
        ADD CONSTRAINT FK_TSql_ClientOfferBrandLogo_Brand
        FOREIGN KEY (LinkClientOfferBrand) REFERENCES dbo.TSql_ClientOfferBrand(IdObject);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrandLogo')
       AND name = N'IX_TSql_ClientOfferBrandLogo_Brand_Order'
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferBrandLogo_Brand_Order
        ON dbo.TSql_ClientOfferBrandLogo(LinkClientOfferBrand, NumberOrder, IdObject)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.tables t
      JOIN sys.schemas s ON s.schema_id = t.schema_id
     WHERE s.name = N'dbo' AND t.name = N'TSql_ClientOfferBrandText'
)
BEGIN
    CREATE TABLE dbo.TSql_ClientOfferBrandText
    (
        IdObject              BIGINT         IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_ClientOfferBrandText PRIMARY KEY CLUSTERED,
        TextLabel             NVARCHAR(500)  NOT NULL,
        LinkClientOfferBrand  BIGINT         NOT NULL,
        NumberOrder           INT            NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandText_NumberOrder DEFAULT (0),
        TextBody              NVARCHAR(MAX)  NULL,
        Is_Delete             BIT            NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandText_Is_Delete DEFAULT (0),
        Is_Active             BIT            NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandText_Is_Active DEFAULT (1),
        LinkMadeBy            NVARCHAR(128)  NOT NULL,
        LinModifiedBy         NVARCHAR(128)  NULL,
        AddDateMade           DATETIME       NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandText_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange     DATETIME       NULL,
        Ntimeschanged         BIGINT         NOT NULL
            CONSTRAINT DF_TSql_ClientOfferBrandText_Ntimeschanged DEFAULT (0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOfferBrandText_Brand')
    ALTER TABLE dbo.TSql_ClientOfferBrandText
        ADD CONSTRAINT FK_TSql_ClientOfferBrandText_Brand
        FOREIGN KEY (LinkClientOfferBrand) REFERENCES dbo.TSql_ClientOfferBrand(IdObject);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferBrandText')
       AND name = N'IX_TSql_ClientOfferBrandText_Brand_Order'
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferBrandText_Brand_Order
        ON dbo.TSql_ClientOfferBrandText(LinkClientOfferBrand, NumberOrder, IdObject)
        WHERE Is_Delete = 0;
GO

INSERT INTO dbo.TSql_ClientOfferBrandLogo
    (TextLabel, LinkClientOfferBrand, NumberOrder, TextImagePath, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT N'Logo importado',
       b.IdObject,
       1,
       b.TextLogo,
       0,
       1,
       ISNULL(NULLIF(LTRIM(RTRIM(b.LinkMadeBy)), N''), N'system'),
       NULL,
       GETDATE(),
       NULL,
       0
  FROM dbo.TSql_ClientOfferBrand b
 WHERE b.Is_Delete = 0
   AND ISNULL(LTRIM(RTRIM(b.TextLogo)), N'') <> N''
   AND NOT EXISTS (
       SELECT 1
         FROM dbo.TSql_ClientOfferBrandLogo l
        WHERE l.LinkClientOfferBrand = b.IdObject
          AND l.Is_Delete = 0
   );
GO

INSERT INTO dbo.TSql_ClientOfferBrandText
    (TextLabel, LinkClientOfferBrand, NumberOrder, TextBody, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT N'Texto inicial',
       b.IdObject,
       1,
       b.TextHeaderTemplate,
       0,
       1,
       ISNULL(NULLIF(LTRIM(RTRIM(b.LinkMadeBy)), N''), N'system'),
       NULL,
       GETDATE(),
       NULL,
       0
  FROM dbo.TSql_ClientOfferBrand b
 WHERE b.Is_Delete = 0
   AND ISNULL(LTRIM(RTRIM(b.TextHeaderTemplate)), N'') <> N''
   AND NOT EXISTS (
       SELECT 1
         FROM dbo.TSql_ClientOfferBrandText t
        WHERE t.LinkClientOfferBrand = b.IdObject
          AND t.Is_Delete = 0
   );
GO

INSERT INTO dbo.TSql_ClientOfferBrandText
    (TextLabel, LinkClientOfferBrand, NumberOrder, TextBody, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT N'Texto final',
       b.IdObject,
       2,
       b.TextBodyTemplate,
       0,
       1,
       ISNULL(NULLIF(LTRIM(RTRIM(b.LinkMadeBy)), N''), N'system'),
       NULL,
       GETDATE(),
       NULL,
       0
  FROM dbo.TSql_ClientOfferBrand b
 WHERE b.Is_Delete = 0
   AND ISNULL(LTRIM(RTRIM(b.TextBodyTemplate)), N'') <> N''
   AND EXISTS (
       SELECT 1
         FROM dbo.TSql_ClientOfferBrandText t
        WHERE t.LinkClientOfferBrand = b.IdObject
          AND t.Is_Delete = 0
          AND t.NumberOrder = 1
   )
   AND NOT EXISTS (
       SELECT 1
         FROM dbo.TSql_ClientOfferBrandText t
        WHERE t.LinkClientOfferBrand = b.IdObject
          AND t.Is_Delete = 0
          AND t.NumberOrder = 2
   );
GO

UPDATE dbo.TSql_ClientOfferBrand
   SET TextLayoutJson = N'{"version":2,"logoPositions":[]}'
 WHERE Is_Delete = 0
   AND (TextLayoutJson IS NULL OR LTRIM(RTRIM(TextLayoutJson)) = N'');
GO

PRINT N'OK — constructor visual de plantilla oferta aplicado (o ya existía).';
