/* =============================================================================
   dbo.TSql_DocumentTypeExtension — Tabla puente N:N entre
       dbo.TSql_DocumentType (IdDocumentType)  y
       dbo.TSql_Extension    (IdExtension).

   Sigue la regla .cursor/rules/sql-tsql-table-conventions.mdc:
     - 9 columnas de auditoría (sin AddChangeBy)
     - Borrado lógico vía Is_Delete (nunca DELETE físico)
     - LinModifiedBy / AddLastDateChange NULL hasta el primer UPDATE

   Idempotente: el script puede ejecutarse N veces sin error.

   NOTA: El usuario ha indicado que la tabla puede ya existir en BD.
         El bloque "IF NOT EXISTS" garantiza que no se rompa nada en ese caso.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_DocumentTypeExtension'
)
BEGIN
    CREATE TABLE [dbo].[TSql_DocumentTypeExtension]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_DocumentTypeExtension PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL
            CONSTRAINT DF_TSql_DocumentTypeExtension_TextLabel DEFAULT (N''),
        [IdDocumentType]      BIGINT          NOT NULL,
        [IdExtension]         BIGINT          NOT NULL,

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DocumentTypeExtension_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_DocumentTypeExtension_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_DocumentTypeExtension_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_DocumentTypeExtension_Ntimeschanged     DEFAULT (0)
    );
END
GO

/* FK -> TSql_DocumentType */
IF OBJECT_ID(N'dbo.TSql_DocumentTypeExtension', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_DocumentType', N'U')         IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_DocumentTypeExtension_DocumentType'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentTypeExtension')
   )
BEGIN
    ALTER TABLE [dbo].[TSql_DocumentTypeExtension]
        ADD CONSTRAINT FK_TSql_DocumentTypeExtension_DocumentType
        FOREIGN KEY ([IdDocumentType]) REFERENCES [dbo].[TSql_DocumentType] ([IdObject]);
END
GO

/* FK -> TSql_Extension */
IF OBJECT_ID(N'dbo.TSql_DocumentTypeExtension', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_Extension', N'U')             IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_DocumentTypeExtension_Extension'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentTypeExtension')
   )
BEGIN
    ALTER TABLE [dbo].[TSql_DocumentTypeExtension]
        ADD CONSTRAINT FK_TSql_DocumentTypeExtension_Extension
        FOREIGN KEY ([IdExtension]) REFERENCES [dbo].[TSql_Extension] ([IdObject]);
END
GO

/* Index para resolver "extensiones de un tipo" rápido */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DocumentTypeExtension_DocumentType_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DocumentTypeExtension')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DocumentTypeExtension_DocumentType_Active
        ON [dbo].[TSql_DocumentTypeExtension] ([IdDocumentType])
        INCLUDE ([IdExtension], [Is_Active])
        WHERE [Is_Delete] = 0;
GO

/* Index inverso para "tipos que admiten esta extensión" */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_DocumentTypeExtension_Extension_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DocumentTypeExtension')
)
    CREATE NONCLUSTERED INDEX IX_TSql_DocumentTypeExtension_Extension_Active
        ON [dbo].[TSql_DocumentTypeExtension] ([IdExtension])
        INCLUDE ([IdDocumentType], [Is_Active])
        WHERE [Is_Delete] = 0;
GO

/* Unicidad funcional (DocumentType, Extension) ignorando borrados lógicos */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_DocumentTypeExtension_Pair_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_DocumentTypeExtension')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_DocumentTypeExtension_Pair_Active
        ON [dbo].[TSql_DocumentTypeExtension] ([IdDocumentType], [IdExtension])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_DocumentTypeExtension creada (o ya existía).';
GO
