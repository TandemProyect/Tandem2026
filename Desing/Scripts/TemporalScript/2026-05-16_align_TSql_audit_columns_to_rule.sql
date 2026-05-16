/* =============================================================================
   2026-05-16 — Alineación de auditoría a la regla
                .cursor/rules/sql-tsql-table-conventions.mdc

   Tablas afectadas:
     - dbo.TSql_Client_V2
     - dbo.TSql_Jobside
     - dbo.TSql_DocumentType

   Cambios:
     1) DROP COLUMN [AddChangeBy]                       (deprecado; sustituido por LinModifiedBy)
        - antes elimina cualquier DEFAULT constraint asociado.
        - antes elimina la FK FK_<Tabla>_AspNetUsers2  (apuntaba a AspNetUsers desde AddChangeBy).
     2) [LinModifiedBy]  NVARCHAR(128)  → NULL          (queda NULL hasta el primer UPDATE)
        - normaliza filas "no modificadas" (Ntimeschanged = 0 y AddLastDateChange = AddDateMade)
          poniendo LinModifiedBy = NULL y AddLastDateChange = NULL.
        - normaliza también valores vacíos ('') → NULL.
     3) [AddLastDateChange] DATETIME    → NULL          (queda NULL hasta el primer UPDATE)
        - antes elimina DEFAULT constraint (si lo hubiera).
     4) Verifica/añade DEFAULT constraints:
        Is_Delete=0, Is_Active=1, Ntimeschanged=0.

   Idempotente: el script puede ejecutarse N veces sin error
   (todos los bloques están protegidos por IF EXISTS / IF NOT EXISTS).

   Tras ejecutar este script, abrir DAL/Model.edmx en Visual Studio y hacer
   "Update Model from Database" para regenerar las clases EF
   (TSql_Client_V2.cs, TSql_Jobside.cs, TSql_DocumentType.cs) — en particular
   debe desaparecer la propiedad AddChangeBy y AddLastDateChange / LinModifiedBy
   deben pasar a admitir null.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* =============================================================================
   FUNCIÓN COMÚN: helper inline para localizar nombre de DEFAULT constraint
   asociado a una columna dada (no se crea como función, sino que cada bloque
   lo resuelve via sys.default_constraints + sys.columns).
   ============================================================================= */

/* =============================================================================
   ===== TABLA 1/3: dbo.TSql_Client_V2 =========================================
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_Client_V2', N'U') IS NOT NULL
BEGIN
    PRINT N'--- Alineando dbo.TSql_Client_V2 ---';

    /* ----- 1.A) DROP FK FK_TSql_Client_V2_AspNetUsers2 (AddChangeBy → AspNetUsers) ----- */
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_Client_V2_AspNetUsers2'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
    )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 DROP CONSTRAINT FK_TSql_Client_V2_AspNetUsers2;
        PRINT N'  -> DROP FK FK_TSql_Client_V2_AspNetUsers2 (AddChangeBy).';
    END

    /* ----- 1.B) DROP DEFAULT constraint asociado a AddChangeBy (si existe) ----- */
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddChangeBy') IS NOT NULL
    BEGIN
        DECLARE @dfClientAddChangeBy SYSNAME;
        SELECT @dfClientAddChangeBy = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
           AND c.name = N'AddChangeBy';

        IF @dfClientAddChangeBy IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_Client_V2 DROP CONSTRAINT ' + @dfClientAddChangeBy + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfClientAddChangeBy + N' (AddChangeBy).';
        END
    END

    /* ----- 1.C) DROP COLUMN AddChangeBy ----- */
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddChangeBy') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 DROP COLUMN AddChangeBy;
        PRINT N'  -> DROP COLUMN AddChangeBy.';
    END

    /* ============================================================
       1.D) Normalizar filas "no modificadas": LinModifiedBy = NULL
            y AddLastDateChange = NULL cuando Ntimeschanged = 0 y
            AddLastDateChange = AddDateMade.
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'LinModifiedBy') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Client_V2', N'AddLastDateChange') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Client_V2', N'AddDateMade') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Client_V2', N'Ntimeschanged') IS NOT NULL
    BEGIN
        /* Cadenas vacías → NULL */
        UPDATE dbo.TSql_Client_V2
           SET LinModifiedBy = NULL
         WHERE LinModifiedBy IS NOT NULL
           AND LTRIM(RTRIM(LinModifiedBy)) = N'';

        /* Filas que jamás se han modificado */
        UPDATE dbo.TSql_Client_V2
           SET LinModifiedBy    = NULL,
               AddLastDateChange = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND AddLastDateChange IS NOT NULL
           AND AddDateMade       IS NOT NULL
           AND AddLastDateChange = AddDateMade;

        /* Filas donde LinModifiedBy = LinkMadeBy y nunca se ha modificado */
        UPDATE dbo.TSql_Client_V2
           SET LinModifiedBy = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND LinModifiedBy IS NOT NULL
           AND LinkMadeBy    IS NOT NULL
           AND LinModifiedBy = LinkMadeBy;
    END

    /* ============================================================
       1.E) ALTER COLUMN LinModifiedBy → NVARCHAR(128) NULL
       ============================================================ */
    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_Client_V2')
           AND name       = N'LinModifiedBy'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        PRINT N'  -> ALTER LinModifiedBy NVARCHAR(128) NULL.';
    END

    /* ============================================================
       1.F) DROP DEFAULT constraint en AddLastDateChange (si existe)
            y luego ALTER COLUMN → NULL.
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'AddLastDateChange') IS NOT NULL
    BEGIN
        DECLARE @dfClientAddLast SYSNAME;
        SELECT @dfClientAddLast = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
           AND c.name = N'AddLastDateChange';

        IF @dfClientAddLast IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_Client_V2 DROP CONSTRAINT ' + @dfClientAddLast + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfClientAddLast + N' (AddLastDateChange).';
        END
    END

    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_Client_V2')
           AND name       = N'AddLastDateChange'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2 ALTER COLUMN AddLastDateChange DATETIME NULL;
        PRINT N'  -> ALTER AddLastDateChange DATETIME NULL.';
    END

    /* ============================================================
       1.G) Verificar/Añadir DEFAULTS: Is_Delete=0, Is_Active=1, Ntimeschanged=0
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Is_Delete') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
             AND  c.name = N'Is_Delete'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2
            ADD CONSTRAINT DF_TSql_Client_V2_Is_Delete DEFAULT (0) FOR Is_Delete;
        PRINT N'  -> ADD DEFAULT DF_TSql_Client_V2_Is_Delete = 0.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Is_Active') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
             AND  c.name = N'Is_Active'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2
            ADD CONSTRAINT DF_TSql_Client_V2_Is_Active DEFAULT (1) FOR Is_Active;
        PRINT N'  -> ADD DEFAULT DF_TSql_Client_V2_Is_Active = 1.';
    END

    IF COL_LENGTH(N'dbo.TSql_Client_V2', N'Ntimeschanged') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Client_V2')
             AND  c.name = N'Ntimeschanged'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Client_V2
            ADD CONSTRAINT DF_TSql_Client_V2_Ntimeschanged DEFAULT (0) FOR Ntimeschanged;
        PRINT N'  -> ADD DEFAULT DF_TSql_Client_V2_Ntimeschanged = 0.';
    END
END
ELSE
    PRINT N'Aviso: dbo.TSql_Client_V2 no existe; se omite.';
GO


/* =============================================================================
   ===== TABLA 2/3: dbo.TSql_Jobside ===========================================
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_Jobside', N'U') IS NOT NULL
BEGIN
    PRINT N'--- Alineando dbo.TSql_Jobside ---';

    /* ----- 2.A) DROP FK FK_TSql_Jobside_AspNetUsers2 (AddChangeBy → AspNetUsers) ----- */
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_Jobside_AspNetUsers2'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
    )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside DROP CONSTRAINT FK_TSql_Jobside_AspNetUsers2;
        PRINT N'  -> DROP FK FK_TSql_Jobside_AspNetUsers2 (AddChangeBy).';
    END

    /* ----- 2.B) DROP DEFAULT constraint asociado a AddChangeBy (si existe) ----- */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddChangeBy') IS NOT NULL
    BEGIN
        DECLARE @dfJobsideAddChangeBy SYSNAME;
        SELECT @dfJobsideAddChangeBy = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
           AND c.name = N'AddChangeBy';

        IF @dfJobsideAddChangeBy IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_Jobside DROP CONSTRAINT ' + @dfJobsideAddChangeBy + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfJobsideAddChangeBy + N' (AddChangeBy).';
        END
    END

    /* ----- 2.C) DROP COLUMN AddChangeBy ----- */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddChangeBy') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.TSql_Jobside DROP COLUMN AddChangeBy;
        PRINT N'  -> DROP COLUMN AddChangeBy.';
    END

    /* ============================================================
       2.D) Normalizar filas "no modificadas"
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'LinModifiedBy') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Jobside', N'AddLastDateChange') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Jobside', N'AddDateMade') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_Jobside', N'Ntimeschanged') IS NOT NULL
    BEGIN
        UPDATE dbo.TSql_Jobside
           SET LinModifiedBy = NULL
         WHERE LinModifiedBy IS NOT NULL
           AND LTRIM(RTRIM(LinModifiedBy)) = N'';

        UPDATE dbo.TSql_Jobside
           SET LinModifiedBy    = NULL,
               AddLastDateChange = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND AddLastDateChange IS NOT NULL
           AND AddDateMade       IS NOT NULL
           AND AddLastDateChange = AddDateMade;

        UPDATE dbo.TSql_Jobside
           SET LinModifiedBy = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND LinModifiedBy IS NOT NULL
           AND LinkMadeBy    IS NOT NULL
           AND LinModifiedBy = LinkMadeBy;
    END

    /* ============================================================
       2.E) ALTER COLUMN LinModifiedBy → NVARCHAR(128) NULL
       ============================================================ */
    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_Jobside')
           AND name       = N'LinModifiedBy'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        PRINT N'  -> ALTER LinModifiedBy NVARCHAR(128) NULL.';
    END

    /* ============================================================
       2.F) DROP DEFAULT en AddLastDateChange + ALTER COLUMN → NULL
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'AddLastDateChange') IS NOT NULL
    BEGIN
        DECLARE @dfJobsideAddLast SYSNAME;
        SELECT @dfJobsideAddLast = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
           AND c.name = N'AddLastDateChange';

        IF @dfJobsideAddLast IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_Jobside DROP CONSTRAINT ' + @dfJobsideAddLast + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfJobsideAddLast + N' (AddLastDateChange).';
        END
    END

    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_Jobside')
           AND name       = N'AddLastDateChange'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside ALTER COLUMN AddLastDateChange DATETIME NULL;
        PRINT N'  -> ALTER AddLastDateChange DATETIME NULL.';
    END

    /* ============================================================
       2.G) Verificar/Añadir DEFAULTS: Is_Delete=0, Is_Active=1, Ntimeschanged=0
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_Jobside', N'Is_Delete') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
             AND  c.name = N'Is_Delete'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside
            ADD CONSTRAINT DF_TSql_Jobside_Is_Delete DEFAULT (0) FOR Is_Delete;
        PRINT N'  -> ADD DEFAULT DF_TSql_Jobside_Is_Delete = 0.';
    END

    IF COL_LENGTH(N'dbo.TSql_Jobside', N'Is_Active') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
             AND  c.name = N'Is_Active'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside
            ADD CONSTRAINT DF_TSql_Jobside_Is_Active DEFAULT (1) FOR Is_Active;
        PRINT N'  -> ADD DEFAULT DF_TSql_Jobside_Is_Active = 1.';
    END

    IF COL_LENGTH(N'dbo.TSql_Jobside', N'Ntimeschanged') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_Jobside')
             AND  c.name = N'Ntimeschanged'
       )
    BEGIN
        ALTER TABLE dbo.TSql_Jobside
            ADD CONSTRAINT DF_TSql_Jobside_Ntimeschanged DEFAULT (0) FOR Ntimeschanged;
        PRINT N'  -> ADD DEFAULT DF_TSql_Jobside_Ntimeschanged = 0.';
    END
END
ELSE
    PRINT N'Aviso: dbo.TSql_Jobside no existe; se omite.';
GO


/* =============================================================================
   ===== TABLA 3/3: dbo.TSql_DocumentType ======================================
   ============================================================================= */
IF OBJECT_ID(N'dbo.TSql_DocumentType', N'U') IS NOT NULL
BEGIN
    PRINT N'--- Alineando dbo.TSql_DocumentType ---';

    /* ----- 3.A) DROP FK FK_TSql_DocumentType_AspNetUsers2 (AddChangeBy → AspNetUsers) ----- */
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_TSql_DocumentType_AspNetUsers2'
          AND parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
    )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType DROP CONSTRAINT FK_TSql_DocumentType_AspNetUsers2;
        PRINT N'  -> DROP FK FK_TSql_DocumentType_AspNetUsers2 (AddChangeBy).';
    END

    /* ----- 3.B) DROP DEFAULT constraint asociado a AddChangeBy (si existe) ----- */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'AddChangeBy') IS NOT NULL
    BEGIN
        DECLARE @dfDocAddChangeBy SYSNAME;
        SELECT @dfDocAddChangeBy = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
           AND c.name = N'AddChangeBy';

        IF @dfDocAddChangeBy IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_DocumentType DROP CONSTRAINT ' + @dfDocAddChangeBy + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfDocAddChangeBy + N' (AddChangeBy).';
        END
    END

    /* ----- 3.C) DROP COLUMN AddChangeBy ----- */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'AddChangeBy') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType DROP COLUMN AddChangeBy;
        PRINT N'  -> DROP COLUMN AddChangeBy.';
    END

    /* ============================================================
       3.D) Normalizar filas "no modificadas"
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'LinModifiedBy') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_DocumentType', N'AddLastDateChange') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_DocumentType', N'AddDateMade') IS NOT NULL
       AND COL_LENGTH(N'dbo.TSql_DocumentType', N'Ntimeschanged') IS NOT NULL
    BEGIN
        UPDATE dbo.TSql_DocumentType
           SET LinModifiedBy = NULL
         WHERE LinModifiedBy IS NOT NULL
           AND LTRIM(RTRIM(LinModifiedBy)) = N'';

        UPDATE dbo.TSql_DocumentType
           SET LinModifiedBy    = NULL,
               AddLastDateChange = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND AddLastDateChange IS NOT NULL
           AND AddDateMade       IS NOT NULL
           AND AddLastDateChange = AddDateMade;

        UPDATE dbo.TSql_DocumentType
           SET LinModifiedBy = NULL
         WHERE COALESCE(Ntimeschanged, 0) = 0
           AND LinModifiedBy IS NOT NULL
           AND LinkMadeBy    IS NOT NULL
           AND LinModifiedBy = LinkMadeBy;
    END

    /* ============================================================
       3.E) ALTER COLUMN LinModifiedBy → NVARCHAR(128) NULL
       ============================================================ */
    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_DocumentType')
           AND name       = N'LinModifiedBy'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        PRINT N'  -> ALTER LinModifiedBy NVARCHAR(128) NULL.';
    END

    /* ============================================================
       3.F) DROP DEFAULT en AddLastDateChange + ALTER COLUMN → NULL
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'AddLastDateChange') IS NOT NULL
    BEGIN
        DECLARE @dfDocAddLast SYSNAME;
        SELECT @dfDocAddLast = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c
            ON c.object_id = dc.parent_object_id
           AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
           AND c.name = N'AddLastDateChange';

        IF @dfDocAddLast IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.TSql_DocumentType DROP CONSTRAINT ' + @dfDocAddLast + N';');
            PRINT N'  -> DROP DEFAULT ' + @dfDocAddLast + N' (AddLastDateChange).';
        END
    END

    IF EXISTS (
        SELECT 1 FROM sys.columns
         WHERE object_id  = OBJECT_ID(N'dbo.TSql_DocumentType')
           AND name       = N'AddLastDateChange'
           AND is_nullable = 0
    )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType ALTER COLUMN AddLastDateChange DATETIME NULL;
        PRINT N'  -> ALTER AddLastDateChange DATETIME NULL.';
    END

    /* ============================================================
       3.G) Verificar/Añadir DEFAULTS: Is_Delete=0, Is_Active=1, Ntimeschanged=0
       ============================================================ */
    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'Is_Delete') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
             AND  c.name = N'Is_Delete'
       )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD CONSTRAINT DF_TSql_DocumentType_Is_Delete DEFAULT (0) FOR Is_Delete;
        PRINT N'  -> ADD DEFAULT DF_TSql_DocumentType_Is_Delete = 0.';
    END

    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'Is_Active') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
             AND  c.name = N'Is_Active'
       )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD CONSTRAINT DF_TSql_DocumentType_Is_Active DEFAULT (1) FOR Is_Active;
        PRINT N'  -> ADD DEFAULT DF_TSql_DocumentType_Is_Active = 1.';
    END

    IF COL_LENGTH(N'dbo.TSql_DocumentType', N'Ntimeschanged') IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM sys.default_constraints dc
           JOIN   sys.columns c
                  ON c.object_id = dc.parent_object_id
                 AND c.column_id = dc.parent_column_id
           WHERE  dc.parent_object_id = OBJECT_ID(N'dbo.TSql_DocumentType')
             AND  c.name = N'Ntimeschanged'
       )
    BEGIN
        ALTER TABLE dbo.TSql_DocumentType
            ADD CONSTRAINT DF_TSql_DocumentType_Ntimeschanged DEFAULT (0) FOR Ntimeschanged;
        PRINT N'  -> ADD DEFAULT DF_TSql_DocumentType_Ntimeschanged = 0.';
    END
END
ELSE
    PRINT N'Aviso: dbo.TSql_DocumentType no existe; se omite.';
GO


PRINT N'OK — Alineación auditoría (Client_V2 / Jobside / DocumentType) completada.';
PRINT N'IMPORTANTE: Regenere DAL/Model.edmx ("Update Model from Database") para reflejar:';
PRINT N'  - eliminación de AddChangeBy';
PRINT N'  - LinModifiedBy ahora nullable';
PRINT N'  - AddLastDateChange ahora nullable.';
GO
