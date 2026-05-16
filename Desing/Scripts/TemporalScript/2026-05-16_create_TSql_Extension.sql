/* =============================================================================
   dbo.TSql_Extension — Catálogo de extensiones de fichero (.pdf, .docx, .dwg, .stl, ...)
   Sigue la regla .cursor/rules/sql-tsql-table-conventions.mdc:
     - 9 columnas de auditoría (sin AddChangeBy)
     - LinModifiedBy / AddLastDateChange NULL hasta el primer UPDATE
   Idempotente: el script puede ejecutarse N veces sin error.

   NOTA: Este script se incluye como fuente de verdad documental del esquema.
         La tabla puede ya existir en la BD (el usuario ha indicado que sí).
         El bloque "IF NOT EXISTS" garantiza que no se rompa nada en ese caso.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = N'dbo' AND t.name = N'TSql_Extension'
)
BEGIN
    CREATE TABLE [dbo].[TSql_Extension]
    (
        /* ----- Clave primaria ----- */
        [IdObject]            BIGINT          IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Extension PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        [TextLabel]           NVARCHAR(500)   NOT NULL,   -- Nombre legible: "PDF", "Word", "AutoCAD DWG"...
        [TextValue]           NVARCHAR(50)    NOT NULL,   -- Extensión con punto: ".pdf", ".docx", ".dwg"...

        /* ----- Auditoría obligatoria ----- */
        [Is_Delete]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Extension_Is_Delete         DEFAULT (0),
        [Is_Active]           BIT             NOT NULL
            CONSTRAINT DF_TSql_Extension_Is_Active         DEFAULT (1),
        [LinkMadeBy]          NVARCHAR(128)   NOT NULL,
        [LinModifiedBy]       NVARCHAR(128)   NULL,
        [AddDateMade]         DATETIME        NOT NULL
            CONSTRAINT DF_TSql_Extension_AddDateMade       DEFAULT (GETDATE()),
        [AddLastDateChange]   DATETIME        NULL,
        [Ntimeschanged]       BIGINT          NOT NULL
            CONSTRAINT DF_TSql_Extension_Ntimeschanged     DEFAULT (0)
    );
END
GO

/* Listado activo por nombre */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Extension_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Extension')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Extension_TextLabel_Active
        ON [dbo].[TSql_Extension] ([TextLabel])
        INCLUDE ([Is_Active], [TextValue])
        WHERE [Is_Delete] = 0;
GO

/* Búsqueda por valor de extensión (".pdf", ".dwg"...) */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Extension_TextValue'
      AND object_id = OBJECT_ID(N'dbo.TSql_Extension')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Extension_TextValue
        ON [dbo].[TSql_Extension] ([TextValue])
        WHERE [Is_Delete] = 0;
GO

/* Unicidad funcional del valor (ignorando borrados lógicos) */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Extension_TextValue_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Extension')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Extension_TextValue_Active
        ON [dbo].[TSql_Extension] ([TextValue])
        WHERE [Is_Delete] = 0;
GO

PRINT N'OK — dbo.TSql_Extension creada (o ya existía).';
GO
