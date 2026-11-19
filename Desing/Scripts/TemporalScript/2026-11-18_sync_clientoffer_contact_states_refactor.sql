/* ============================================================================
   Refactor OfertaCliente (contactos + estados)
   - Alinea esquema y datos para estados configurables de contacto.
   - Idempotente para develop y producción.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   1) Esquema mínimo esperado
--------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.TSql_ClientOfferContact', 'LinkClient_V2') IS NULL
    ALTER TABLE dbo.TSql_ClientOfferContact ADD LinkClient_V2 BIGINT NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOfferContact', 'Is_Default') IS NULL
    ALTER TABLE dbo.TSql_ClientOfferContact ADD Is_Default BIT NOT NULL CONSTRAINT DF_TSql_ClientOfferContact_Is_Default_refactor DEFAULT (0);
GO

IF COL_LENGTH('dbo.TSql_ClientOfferContactStateType', 'Is_Active') IS NULL
    ALTER TABLE dbo.TSql_ClientOfferContactStateType ADD Is_Active BIT NOT NULL CONSTRAINT DF_TSql_ClientOfferContactStateType_Is_Active_refactor DEFAULT (1);
GO

IF COL_LENGTH('dbo.TSql_ClientOffer', 'LinkClientOfferContact') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD LinkClientOfferContact BIGINT NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextContactName') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD TextContactName NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextContactEmail') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD TextContactEmail NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.TSql_ClientOffer', 'TextMaterial') IS NULL
    ALTER TABLE dbo.TSql_ClientOffer ADD TextMaterial NVARCHAR(MAX) NULL;
GO

/* ---------------------------------------------------------------------------
   2) Normalización de estados por código (evita duplicados por espacios/case)
--------------------------------------------------------------------------- */
UPDATE dbo.TSql_ClientOfferContactStateType
   SET TextCode = UPPER(LTRIM(RTRIM(TextCode))),
       TextLabel = LTRIM(RTRIM(TextLabel))
 WHERE Is_Delete = 0
   AND (
        TextCode <> UPPER(LTRIM(RTRIM(TextCode)))
        OR TextLabel <> LTRIM(RTRIM(TextLabel))
   );
GO

;WITH Dup AS
(
    SELECT IdObject,
           TextCodeNorm = UPPER(LTRIM(RTRIM(TextCode))),
           KeepId = MIN(IdObject) OVER (PARTITION BY UPPER(LTRIM(RTRIM(TextCode)))),
           Is_Delete
      FROM dbo.TSql_ClientOfferContactStateType
     WHERE Is_Delete = 0
)
UPDATE st
   SET LinkClientOfferContactStateType = d.KeepId
  FROM dbo.TSql_ClientOfferContactState st
  JOIN Dup d
    ON d.IdObject = st.LinkClientOfferContactStateType
 WHERE d.IdObject <> d.KeepId
   AND st.Is_Delete = 0;
GO

;WITH Dup AS
(
    SELECT IdObject,
           KeepId = MIN(IdObject) OVER (PARTITION BY UPPER(LTRIM(RTRIM(TextCode))))
      FROM dbo.TSql_ClientOfferContactStateType
     WHERE Is_Delete = 0
)
UPDATE t
   SET Is_Delete = 1,
       Is_Active = 0,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = ISNULL(Ntimeschanged, 0) + 1
  FROM dbo.TSql_ClientOfferContactStateType t
  JOIN Dup d ON d.IdObject = t.IdObject
 WHERE d.IdObject <> d.KeepId
   AND t.Is_Delete = 0;
GO

/* ---------------------------------------------------------------------------
   3) Estados base obligatorios y activos
--------------------------------------------------------------------------- */
MERGE dbo.TSql_ClientOfferContactStateType AS tgt
USING (VALUES
    (N'ALTA', N'Alta'),
    (N'BAJA', N'Baja'),
    (N'ENFERMO', N'Enfermo')
) AS src(TextCode, TextLabel)
ON tgt.Is_Delete = 0 AND tgt.TextCode = src.TextCode
WHEN MATCHED THEN
    UPDATE SET TextLabel = src.TextLabel,
               Is_Active = 1,
               AddLastDateChange = GETDATE(),
               Ntimeschanged = ISNULL(tgt.Ntimeschanged, 0) + 1
WHEN NOT MATCHED THEN
    INSERT (TextCode, TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
    VALUES (src.TextCode, src.TextLabel, 0, 1, N'system', NULL, GETDATE(), NULL, 0);
GO

/* ---------------------------------------------------------------------------
   4) Contactos sin estado -> estado inicial ALTA
--------------------------------------------------------------------------- */
INSERT INTO dbo.TSql_ClientOfferContactState
    (LinkClientOfferContact, LinkClientOfferContactStateType, AddStatusDate, TextNotes,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
SELECT c.IdObject,
       stAlta.IdObject,
       ISNULL(c.AddDateMade, GETDATE()),
       N'Estado inicial',
       0, 1, N'system', NULL, GETDATE(), NULL, 0
  FROM dbo.TSql_ClientOfferContact c
 CROSS JOIN (
    SELECT TOP (1) IdObject
      FROM dbo.TSql_ClientOfferContactStateType
     WHERE Is_Delete = 0 AND TextCode = N'ALTA'
 ) stAlta
 WHERE c.Is_Delete = 0
   AND NOT EXISTS (
       SELECT 1
         FROM dbo.TSql_ClientOfferContactState s
        WHERE s.Is_Delete = 0
          AND s.LinkClientOfferContact = c.IdObject
   );
GO

/* ---------------------------------------------------------------------------
   5) Regla de default único por cliente y no default en BAJA/ENFERMO
--------------------------------------------------------------------------- */
;WITH LastState AS
(
    SELECT c.IdObject,
           c.LinkClient_V2,
           c.Is_Default,
           rn = ROW_NUMBER() OVER (
                    PARTITION BY c.LinkClient_V2
                    ORDER BY CASE WHEN c.Is_Default = 1 THEN 0 ELSE 1 END,
                             c.IdObject DESC
                ),
           StateCode = ISNULL((
               SELECT TOP (1) stt.TextCode
                 FROM dbo.TSql_ClientOfferContactState st
                 JOIN dbo.TSql_ClientOfferContactStateType stt
                   ON stt.IdObject = st.LinkClientOfferContactStateType
                WHERE st.Is_Delete = 0
                  AND st.LinkClientOfferContact = c.IdObject
                ORDER BY st.AddStatusDate DESC, st.IdObject DESC
           ), N'ALTA')
      FROM dbo.TSql_ClientOfferContact c
     WHERE c.Is_Delete = 0
       AND c.LinkClient_V2 IS NOT NULL
)
UPDATE c
   SET Is_Default = CASE
                       WHEN ls.rn = 1 AND ls.StateCode NOT IN (N'BAJA', N'ENFERMO') THEN 1
                       ELSE 0
                    END
  FROM dbo.TSql_ClientOfferContact c
  JOIN LastState ls ON ls.IdObject = c.IdObject
 WHERE c.Is_Delete = 0;
GO

/* ---------------------------------------------------------------------------
   6) Integridad referencial e índices de apoyo
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_ClientOffer_Contact')
BEGIN
    UPDATE o
       SET LinkClientOfferContact = NULL
      FROM dbo.TSql_ClientOffer o
 LEFT JOIN dbo.TSql_ClientOfferContact c
        ON c.IdObject = o.LinkClientOfferContact
       AND c.Is_Delete = 0
     WHERE o.LinkClientOfferContact IS NOT NULL
       AND c.IdObject IS NULL;

    ALTER TABLE dbo.TSql_ClientOffer
      ADD CONSTRAINT FK_TSql_ClientOffer_Contact
      FOREIGN KEY (LinkClientOfferContact) REFERENCES dbo.TSql_ClientOfferContact(IdObject);
END
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContactStateType')
       AND name = N'UX_TSql_ClientOfferContactStateType_Code'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferContactStateType_Code
        ON dbo.TSql_ClientOfferContactStateType(TextCode)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContact')
       AND name = N'UX_TSql_ClientOfferContact_DefaultByClient'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_ClientOfferContact_DefaultByClient
        ON dbo.TSql_ClientOfferContact(LinkClient_V2)
        WHERE Is_Delete = 0 AND Is_Default = 1 AND LinkClient_V2 IS NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1
      FROM sys.indexes
     WHERE object_id = OBJECT_ID(N'dbo.TSql_ClientOfferContactState')
       AND name = N'IX_TSql_ClientOfferContactState_Contact_Date'
)
    CREATE NONCLUSTERED INDEX IX_TSql_ClientOfferContactState_Contact_Date
        ON dbo.TSql_ClientOfferContactState(LinkClientOfferContact, AddStatusDate DESC, IdObject DESC)
        INCLUDE (LinkClientOfferContactStateType)
        WHERE Is_Delete = 0;
GO

PRINT N'OK — Refactor OfertaCliente (contactos/estados) aplicado.';
GO
