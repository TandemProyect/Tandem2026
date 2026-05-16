/* =============================================================================
   2026-05-16 — Alineación de [LinModifiedBy] a la regla
                .cursor/rules/sql-tsql-table-conventions.mdc

   Tablas afectadas:
     - dbo.TSql_Extension
     - dbo.TSql_DocumentTypeExtension

   Cambios:
     1) [LinModifiedBy] NVARCHAR(128) NOT NULL → NULL.
        Razón: la regla establece que LinModifiedBy se rellena SOLO en UPDATE,
        por lo que en el INSERT debe quedar NULL. EntityFramework / el helper
        IntranetAuditHelper envia NULL al crear y la columna NOT NULL hacia
        que SaveChanges lanzase: "Cannot insert the value NULL into column
        'LinModifiedBy'".

     2) Normaliza filas existentes:
        - LinModifiedBy = ''   → NULL.
        - LinModifiedBy = LinkMadeBy con Ntimeschanged = 0 → NULL.

   Idempotente: puede ejecutarse N veces sin error (todos los bloques estan
   protegidos por IF EXISTS / chequeo de is_nullable).

   Tras ejecutarlo NO hace falta regenerar el EDMX: en EF la propiedad
   LinModifiedBy ya es `string` (nullable en C#), independientemente de la
   nulabilidad declarada en SSDL. Esta es la razon por la que el helper podia
   enviar NULL sin que el compilador protestara.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* =============================================================================
   ===== TABLA 1/2: dbo.TSql_Extension =========================================
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_Extension', N'U') IS NOT NULL
BEGIN
    PRINT N'--- Alineando dbo.TSql_Extension.LinModifiedBy ---';

    /* 1.A) Si actualmente es NOT NULL, pasarla a NULL */
    IF EXISTS (
        SELECT 1
          FROM sys.columns c
          JOIN sys.tables  t ON c.object_id = t.object_id
         WHERE t.name = N'TSql_Extension'
           AND c.name = N'LinModifiedBy'
           AND c.is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_Extension
            ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        PRINT N'  -> ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL.';
    END
    ELSE
    BEGIN
        PRINT N'  -> LinModifiedBy ya era NULL.';
    END

    /* 1.B) Normalizar valores: '' y LinkMadeBy con Ntimeschanged = 0  → NULL */
    UPDATE dbo.TSql_Extension
       SET LinModifiedBy = NULL
     WHERE LinModifiedBy IS NOT NULL
       AND (
                LTRIM(RTRIM(LinModifiedBy)) = N''
             OR (Ntimeschanged = 0 AND LinModifiedBy = LinkMadeBy)
           );
    PRINT N'  -> Normalizadas ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' filas (LinModifiedBy → NULL).';
END
ELSE
BEGIN
    PRINT N'Aviso: dbo.TSql_Extension no existe; ejecutar antes 2026-05-16_create_TSql_Extension.sql';
END
GO


/* =============================================================================
   ===== TABLA 2/2: dbo.TSql_DocumentTypeExtension =============================
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_DocumentTypeExtension', N'U') IS NOT NULL
BEGIN
    PRINT N'--- Alineando dbo.TSql_DocumentTypeExtension.LinModifiedBy ---';

    /* 2.A) Si actualmente es NOT NULL, pasarla a NULL */
    IF EXISTS (
        SELECT 1
          FROM sys.columns c
          JOIN sys.tables  t ON c.object_id = t.object_id
         WHERE t.name = N'TSql_DocumentTypeExtension'
           AND c.name = N'LinModifiedBy'
           AND c.is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentTypeExtension
            ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        PRINT N'  -> ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL.';
    END
    ELSE
    BEGIN
        PRINT N'  -> LinModifiedBy ya era NULL.';
    END

    /* 2.B) Normalizar valores */
    UPDATE dbo.TSql_DocumentTypeExtension
       SET LinModifiedBy = NULL
     WHERE LinModifiedBy IS NOT NULL
       AND (
                LTRIM(RTRIM(LinModifiedBy)) = N''
             OR (Ntimeschanged = 0 AND LinModifiedBy = LinkMadeBy)
           );
    PRINT N'  -> Normalizadas ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' filas (LinModifiedBy → NULL).';
END
ELSE
BEGIN
    PRINT N'Aviso: dbo.TSql_DocumentTypeExtension no existe; ejecutar antes 2026-05-16_create_TSql_DocumentTypeExtension.sql';
END
GO

PRINT N'OK — Alineación LinModifiedBy completada en TSql_Extension + TSql_DocumentTypeExtension.';
GO
