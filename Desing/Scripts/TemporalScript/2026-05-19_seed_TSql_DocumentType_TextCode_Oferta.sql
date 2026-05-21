/* =============================================================================
   Catálogo: tipo de documento «Oferta» (TextCode = Oferta) para adjuntos del
   espacio de trabajo de oferta (Jobside/OfferDetails → UploadOfferDocument).

   Idempotente. Requiere al menos un dbo.AspNetUsers (FK LinkMadeBy).
   Tras ejecutar, si el tipo es nuevo en su entorno: revisar TSql_DocumentType
   en EF (Update Model from Database) solo si añadió columnas manualmente; el
   IdObject lo asigna IDENTITY.

   Índice opcional en TSql_Document.LinkOffer para listados por oferta.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_DocumentType no existe; omitido.';
END
ELSE IF COL_LENGTH(N'dbo.TSql_DocumentType', N'TextCode') IS NULL
BEGIN
    PRINT N'Aviso: TextCode ausente en TSql_DocumentType; omitido.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.TSql_DocumentType WHERE Is_Delete = 0 AND LTRIM(RTRIM(ISNULL(TextCode, N''))) = N'Oferta')
BEGIN
    DECLARE @Uid NVARCHAR(128) =
        (SELECT TOP (1) u.Id FROM dbo.AspNetUsers AS u ORDER BY u.Id);

    IF @Uid IS NULL OR LTRIM(RTRIM(@Uid)) = N''
    BEGIN
        PRINT N'Aviso: sin filas en AspNetUsers — no se inserta tipo Oferta (FK LinkMadeBy).';
    END
    ELSE
    BEGIN
        /* NumberMaxFileSizeBytes: según despliegue (columna opcional / legada). */
        IF COL_LENGTH(N'dbo.TSql_DocumentType', N'NumberMaxFileSizeBytes') IS NOT NULL
        BEGIN
            INSERT INTO dbo.TSql_DocumentType
            (
                TextLabel,
                Is_Delete,
                Is_Active,
                LinkMadeBy,
                LinModifiedBy,
                AddDateMade,
                AddLastDateChange,
                Ntimeschanged,
                TextCode,
                TextDescription,
                NumberMaxFileSizeBytes
            )
            VALUES
            (
                N'Oferta',
                0,
                1,
                @Uid,
                NULL,
                GETDATE(),
                NULL,
                0,
                N'Oferta',
                N'Adjuntos del espacio de trabajo de oferta (TSql_Document.LinkOffer).',
                0
            );
        END
        ELSE
        BEGIN
            INSERT INTO dbo.TSql_DocumentType
            (
                TextLabel,
                Is_Delete,
                Is_Active,
                LinkMadeBy,
                LinModifiedBy,
                AddDateMade,
                AddLastDateChange,
                Ntimeschanged,
                TextCode,
                TextDescription
            )
            VALUES
            (
                N'Oferta',
                0,
                1,
                @Uid,
                NULL,
                GETDATE(),
                NULL,
                0,
                N'Oferta',
                N'Adjuntos del espacio de trabajo de oferta (TSql_Document.LinkOffer).'
            );
        END

        PRINT N'OK — Insertado TSql_DocumentType TextCode = Oferta.';
    END
END
ELSE
BEGIN
    PRINT N'OK — TSql_DocumentType Oferta ya existía.';
END
GO

/* Listados por oferta (opcional) */
IF OBJECT_ID(N'dbo.TSql_Document', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Document', N'LinkOffer') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_TSql_Document_LinkOffer_NotDeleted'
          AND object_id = OBJECT_ID(N'dbo.TSql_Document')
   )
BEGIN
    CREATE NONCLUSTERED INDEX IX_TSql_Document_LinkOffer_NotDeleted
        ON dbo.TSql_Document (LinkOffer)
        WHERE Is_Delete = 0 AND LinkOffer IS NOT NULL;
    PRINT N'OK — Índice IX_TSql_Document_LinkOffer_NotDeleted creado.';
END
ELSE IF OBJECT_ID(N'dbo.TSql_Document', N'U') IS NULL
    PRINT N'Aviso: dbo.TSql_Document no existe; sin índice LinkOffer.';
ELSE IF COL_LENGTH(N'dbo.TSql_Document', N'LinkOffer') IS NULL
    PRINT N'Aviso: TSql_Document.LinkOffer ausente — ejecute migración / actualice EF.';
ELSE
    PRINT N'OK — Índice LinkOffer en TSql_Document ya existía o omitido.';
GO

PRINT N'OK — dbo.TSql_DocumentType TextCode Oferta (fin).';
GO
