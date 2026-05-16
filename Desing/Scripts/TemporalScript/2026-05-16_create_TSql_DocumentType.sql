/* =============================================================================
   dbo.TSql_DocumentType — Tipos de documento (intranet / Configuración)
   Columnas fijas intranet + TextCode, TextDescription.
   El tamaño máximo por fichero se define en dbo.TSql_Extension (catálogo extensiones).
   FK auditoría → dbo.AspNetUsers (idempotente, requiere al menos un usuario).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_DocumentType
    (
        /* ----- FIJAS (todas las tablas intranet) ----- */
        IdObject            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DocumentType PRIMARY KEY CLUSTERED,

        TextLabel           NVARCHAR(500)   NOT NULL,

        Is_Delete           BIT             NOT NULL
            CONSTRAINT DF_TSql_DocumentType_Is_Delete DEFAULT (0),
        Is_Active           BIT             NOT NULL
            CONSTRAINT DF_TSql_DocumentType_Is_Active DEFAULT (1),

        LinkMadeBy          NVARCHAR(128)   NOT NULL,
        LinModifiedBy       NVARCHAR(128)   NOT NULL,
        AddDateMade         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DocumentType_AddDateMade DEFAULT (GETDATE()),
        AddChangeBy         NVARCHAR(128)   NOT NULL,
        AddLastDateChange   DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DocumentType_AddLastDateChange DEFAULT (GETDATE()),
        Ntimeschanged       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DocumentType_Ntimeschanged DEFAULT (0),

        /* ----- Negocio ----- */
        TextCode            NVARCHAR(50)    NULL,
        TextDescription     NVARCHAR(500)   NULL
    );
END
GO

/* Listados activos por nombre */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DocumentType_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DocumentType_TextLabel_Active
        ON dbo.TSql_DocumentType (TextLabel)
        INCLUDE (Is_Active, TextCode)
        WHERE Is_Delete = 0;
GO

/* Búsqueda por código */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DocumentType_TextCode'
      AND object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DocumentType_TextCode
        ON dbo.TSql_DocumentType (TextCode)
        WHERE TextCode IS NOT NULL AND Is_Delete = 0;
GO

/* FK auditoría → AspNetUsers (patrón Script 4 Client_V2 / Jobside) */
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.AspNetUsers)
BEGIN
    DECLARE @FallbackUserId NVARCHAR(128);
    SELECT TOP (1) @FallbackUserId = Id FROM dbo.AspNetUsers ORDER BY Id;

    UPDATE d
       SET LinkMadeBy = COALESCE(NULLIF(LTRIM(RTRIM(d.LinkMadeBy)), N''), @FallbackUserId),
           LinModifiedBy = COALESCE(NULLIF(LTRIM(RTRIM(d.LinModifiedBy)), N''), @FallbackUserId),
           AddChangeBy = COALESCE(NULLIF(LTRIM(RTRIM(d.AddChangeBy)), N''), @FallbackUserId),
           AddDateMade = COALESCE(d.AddDateMade, GETDATE()),
           AddLastDateChange = COALESCE(d.AddLastDateChange, GETDATE()),
           Ntimeschanged = COALESCE(d.Ntimeschanged, 0)
      FROM dbo.TSql_DocumentType d
     WHERE d.LinkMadeBy IS NULL OR d.LinModifiedBy IS NULL OR d.AddChangeBy IS NULL
        OR d.AddDateMade IS NULL OR d.AddLastDateChange IS NULL OR d.Ntimeschanged IS NULL;

    UPDATE d SET LinkMadeBy = @FallbackUserId
      FROM dbo.TSql_DocumentType d
     WHERE d.LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

    UPDATE d SET LinModifiedBy = @FallbackUserId
      FROM dbo.TSql_DocumentType d
     WHERE d.LinModifiedBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

    UPDATE d SET AddChangeBy = @FallbackUserId
      FROM dbo.TSql_DocumentType d
     WHERE d.AddChangeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DocumentType_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType'))
        ALTER TABLE dbo.TSql_DocumentType ADD CONSTRAINT FK_TSql_DocumentType_AspNetUsers
            FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DocumentType_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType'))
        ALTER TABLE dbo.TSql_DocumentType ADD CONSTRAINT FK_TSql_DocumentType_AspNetUsers1
            FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_DocumentType_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType'))
        ALTER TABLE dbo.TSql_DocumentType ADD CONSTRAINT FK_TSql_DocumentType_AspNetUsers2
            FOREIGN KEY (AddChangeBy) REFERENCES dbo.AspNetUsers (Id);
END
ELSE IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL
    PRINT N'Aviso: FK auditoría omitidas — dbo.AspNetUsers vacía o inexistente.';
GO

PRINT N'OK — dbo.TSql_DocumentType creada (o ya existía).';
GO
