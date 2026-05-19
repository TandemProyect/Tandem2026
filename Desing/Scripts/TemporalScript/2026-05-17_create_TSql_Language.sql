/* =============================================================================
   dbo.TSql_Language — Catálogo de idiomas / culturas (UI + futura importación Excel)

   TextCode: cultura corta recomendada para MVC (es, en, ca…); puede ampliarse a es-ES si lo preferís.
   Is_Default: solo una fila activa con 1 (índice único filtrado).
   Tras ejecutar: actualizar DAL/Model.edmx (Update Model from Database).

   Orden: ejecutar ANTES que 2026-05-17_create_TSql_UiTranslation.sql

   Si dbo.TSql_Language ya existía vacía o con columnas antiguas, el CREATE no se
   ejecuta; el bloque "Tabla legada" añade columnas estándar que falten.
   Si la PK no se llama IdObject, el relleno de TextLabel/TextCode usa ROW_NUMBER.
   Tablas legadas con SysObjectID NOT NULL sin IDENTITY: la semilla usa sp_executesql (MAX+1).
   Antes de forzar NOT NULL en TextLabel/TextCode se eliminan índices que los referencian.
   Semilla: literales y LinkMadeBy/LinModifiedBy/LinCreatedBy se truncan según sys.columns (evita error 8152).
   Legado AttCreated / AttLastModification: GETDATE() en UPDATE + INSERT si existen.
   Legado SysUpdateNumber (contador): 0 en UPDATE + INSERT si existe la columna.
   Legado AttIsDeleted (BIT): 0 en UPDATE + INSERT si existe (no borrado lógico).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_Language
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_Language PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        TextCode      NVARCHAR(20)  NOT NULL,
        TextNativeName NVARCHAR(100) NULL,
        LinkCountry   BIGINT NULL, /* FK opcional → dbo.TSql_Countrys.IdObject */

        Is_Default    BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Default DEFAULT (0),

        /* ----- Auditoría obligatoria ----- */
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_Language_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_Language_Ntimeschanged DEFAULT (0)
    );
END
GO

/* -----------------------------------------------------------------------------
   Tabla legada: si dbo.TSql_Language ya existía con otro esquema, el CREATE
   anterior no hace nada. Se añaden aquí las columnas estándar que falten y se
   rellenan filas antiguas antes de crear índices / FK / semilla.
   ----------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.TSql_Language', N'TextLabel') IS NULL
        ALTER TABLE dbo.TSql_Language ADD TextLabel NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'TextCode') IS NULL
        ALTER TABLE dbo.TSql_Language ADD TextCode NVARCHAR(20) NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'TextNativeName') IS NULL
        ALTER TABLE dbo.TSql_Language ADD TextNativeName NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'LinkCountry') IS NULL
        ALTER TABLE dbo.TSql_Language ADD LinkCountry BIGINT NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'Is_Default') IS NULL
        ALTER TABLE dbo.TSql_Language ADD Is_Default BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Default_Align DEFAULT (0);

    IF COL_LENGTH(N'dbo.TSql_Language', N'Is_Delete') IS NULL
        ALTER TABLE dbo.TSql_Language ADD Is_Delete BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Delete_Align DEFAULT (0);

    IF COL_LENGTH(N'dbo.TSql_Language', N'Is_Active') IS NULL
        ALTER TABLE dbo.TSql_Language ADD Is_Active BIT NOT NULL
            CONSTRAINT DF_TSql_Language_Is_Active_Align DEFAULT (1);

    IF COL_LENGTH(N'dbo.TSql_Language', N'LinkMadeBy') IS NULL
        ALTER TABLE dbo.TSql_Language ADD LinkMadeBy NVARCHAR(128) NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'LinModifiedBy') IS NULL
        ALTER TABLE dbo.TSql_Language ADD LinModifiedBy NVARCHAR(128) NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'AddDateMade') IS NULL
        ALTER TABLE dbo.TSql_Language ADD AddDateMade DATETIME NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'AddLastDateChange') IS NULL
        ALTER TABLE dbo.TSql_Language ADD AddLastDateChange DATETIME NULL;

    IF COL_LENGTH(N'dbo.TSql_Language', N'Ntimeschanged') IS NULL
        ALTER TABLE dbo.TSql_Language ADD Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_Language_Ntimeschanged_Align DEFAULT (0);
END
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
BEGIN
    /* Relleno sin IdObject: SQL Server valida nombres de columna en todo el batch;
       por eso no se usa IF IdObject + UPDATE estático; ROW_NUMBER sirve también si IdObject existe. */
    ;WITH R AS (
        SELECT *,
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
          FROM dbo.TSql_Language
    )
    UPDATE R
       SET TextLabel = CASE
               WHEN TextLabel IS NULL OR LTRIM(RTRIM(TextLabel)) = N''
                   THEN N'Idioma ' + CAST(rn AS NVARCHAR(32))
               ELSE TextLabel
           END,
           TextCode = CASE
               WHEN TextCode IS NULL OR LTRIM(RTRIM(TextCode)) = N''
                   THEN N'lang-' + CAST(rn AS NVARCHAR(32))
               ELSE TextCode
           END
     WHERE (TextLabel IS NULL OR LTRIM(RTRIM(TextLabel)) = N'')
        OR (TextCode IS NULL OR LTRIM(RTRIM(TextCode)) = N'');

    UPDATE dbo.TSql_Language
       SET Is_Delete = 0
     WHERE Is_Delete IS NULL;

    UPDATE dbo.TSql_Language
       SET Is_Active = 1
     WHERE Is_Active IS NULL;

    UPDATE dbo.TSql_Language
       SET Is_Default = 0
     WHERE Is_Default IS NULL;

    UPDATE dbo.TSql_Language
       SET AddDateMade = GETDATE()
     WHERE AddDateMade IS NULL;

    UPDATE dbo.TSql_Language
       SET Ntimeschanged = 0
     WHERE Ntimeschanged IS NULL;

    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
       AND EXISTS (SELECT 1 FROM dbo.AspNetUsers)
    BEGIN
        DECLARE @AlignUser NVARCHAR(128);
        SELECT TOP (1) @AlignUser = Id FROM dbo.AspNetUsers ORDER BY Id;

        UPDATE dbo.TSql_Language
           SET LinkMadeBy = @AlignUser
         WHERE LinkMadeBy IS NULL OR LTRIM(RTRIM(LinkMadeBy)) = N'';

        UPDATE dbo.TSql_Language
           SET LinkMadeBy = @AlignUser
         WHERE LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

        IF COL_LENGTH(N'dbo.TSql_Language', N'LinCreatedBy') IS NOT NULL
            EXEC sp_executesql
                N'
UPDATE dbo.TSql_Language
   SET LinCreatedBy = @au
 WHERE LinCreatedBy IS NULL
    OR LEN(LTRIM(RTRIM(LinCreatedBy))) = 0
    OR LinCreatedBy NOT IN (SELECT Id FROM dbo.AspNetUsers)',
                N'@au NVARCHAR(128)',
                @au = @AlignUser;
    END
END
GO

/* Legado: AttLabel NOT NULL (duplicado habitual de etiqueta) — sin referencia estática en batch si no existe en todas las BDs */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AttLabel') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET AttLabel = TextLabel
 WHERE TextLabel IS NOT NULL
   AND (AttLabel IS NULL OR LEN(LTRIM(RTRIM(AttLabel))) = 0)';
END
GO

/* Legado: AttSinglature NOT NULL — mismo criterio que AttLabel (texto mostrable / firma corta) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AttSinglature') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET AttSinglature = TextLabel
 WHERE TextLabel IS NOT NULL
   AND (AttSinglature IS NULL OR LEN(LTRIM(RTRIM(AttSinglature))) = 0)';
END
GO

/* Legado: Linflag NOT NULL — habitualmente BIT o FK numérico; 0 = sin marca / neutro */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'Linflag') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET Linflag = 0
 WHERE Linflag IS NULL';
END
GO

/* Legado: AttCreated NOT NULL — fecha de creación en esquema antiguo */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AttCreated') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET AttCreated = GETDATE()
 WHERE AttCreated IS NULL';
END
GO

/* Legado: AttLastModification NOT NULL — fecha última modificación (en alta = misma que creación) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AttLastModification') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET AttLastModification = GETDATE()
 WHERE AttLastModification IS NULL';
END
GO

/* Legado: SysUpdateNumber NOT NULL — contador de actualizaciones (alta = 0, como Ntimeschanged) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'SysUpdateNumber') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET SysUpdateNumber = 0
 WHERE SysUpdateNumber IS NULL';
END
GO

/* Legado: AttIsDeleted NOT NULL — borrado lógico antiguo (0 = activo) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AttIsDeleted') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
UPDATE dbo.TSql_Language
   SET AttIsDeleted = CAST(0 AS BIT)
 WHERE AttIsDeleted IS NULL';
END
GO

/* Convención intranet: LinModifiedBy NULL en altas. Tablas legadas NOT NULL → intentar alinear. */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinModifiedBy') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
          FROM sys.columns c
          INNER JOIN sys.tables t ON t.object_id = c.object_id
          INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
         WHERE s.name = N'dbo'
           AND t.name = N'TSql_Language'
           AND c.name = N'LinModifiedBy'
           AND c.is_nullable = 0
    )
    BEGIN
        BEGIN TRY
            ALTER TABLE dbo.TSql_Language ALTER COLUMN LinModifiedBy NVARCHAR(128) NULL;
        END TRY
        BEGIN CATCH
            PRINT N'Aviso: LinModifiedBy sigue NOT NULL (legacy). El INSERT de semilla usará el mismo usuario que LinkMadeBy.';
        END CATCH
    END
END
GO

/* Índices filtrados / INCLUDE bloquean ALTER COLUMN en TextLabel/TextCode */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes
         WHERE object_id = OBJECT_ID(N'dbo.TSql_Language')
           AND name = N'IX_TSql_Language_TextLabel_Active'
    )
        DROP INDEX IX_TSql_Language_TextLabel_Active ON dbo.TSql_Language;

    IF EXISTS (
        SELECT 1 FROM sys.indexes
         WHERE object_id = OBJECT_ID(N'dbo.TSql_Language')
           AND name = N'UX_TSql_Language_TextCode_Active'
    )
        DROP INDEX UX_TSql_Language_TextCode_Active ON dbo.TSql_Language;
END
GO

/* TextLabel / TextCode / AddDateMade NOT NULL solo si ya no hay NULLs */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'TextLabel') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE TextLabel IS NULL)
BEGIN
    ALTER TABLE dbo.TSql_Language ALTER COLUMN TextLabel NVARCHAR(500) NOT NULL;
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'TextLabel') IS NOT NULL
    PRINT N'Aviso: TextLabel aún con NULL — no se fuerza NOT NULL. Completar datos y re-ejecutar.';
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'TextCode') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE TextCode IS NULL)
BEGIN
    ALTER TABLE dbo.TSql_Language ALTER COLUMN TextCode NVARCHAR(20) NOT NULL;
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'TextCode') IS NOT NULL
    PRINT N'Aviso: TextCode aún con NULL — no se fuerza NOT NULL. Completar datos y re-ejecutar.';
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinkMadeBy') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE LinkMadeBy IS NULL)
BEGIN
    ALTER TABLE dbo.TSql_Language ALTER COLUMN LinkMadeBy NVARCHAR(128) NOT NULL;
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinkMadeBy') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE LinkMadeBy IS NULL)
    PRINT N'Aviso: TSql_Language.LinkMadeBy tiene NULL — hay filas sin usuario de auditoría y AspNetUsers vacío o sin coincidencia. Rellenar a mano antes de FK.';
GO

IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AddDateMade') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE AddDateMade IS NULL)
BEGIN
    ALTER TABLE dbo.TSql_Language ALTER COLUMN AddDateMade DATETIME NOT NULL;
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'AddDateMade') IS NOT NULL
    PRINT N'Aviso: AddDateMade aún con NULL — no se fuerza NOT NULL. Completar datos y re-ejecutar.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Language_TextCode_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Language')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Language_TextCode_Active
        ON dbo.TSql_Language (TextCode)
        WHERE Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_Language_Default_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Language')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_Language_Default_Active
        ON dbo.TSql_Language (Is_Default)
        WHERE Is_Default = 1 AND Is_Delete = 0;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_TSql_Language_TextLabel_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_Language')
)
    CREATE NONCLUSTERED INDEX IX_TSql_Language_TextLabel_Active
        ON dbo.TSql_Language (TextLabel)
        INCLUDE (Is_Active, TextCode)
        WHERE Is_Delete = 0;
GO

/* FK auditoría → AspNetUsers */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.AspNetUsers)
BEGIN
    DECLARE @FallbackUserId NVARCHAR(128);
    SELECT TOP (1) @FallbackUserId = Id FROM dbo.AspNetUsers ORDER BY Id;

    UPDATE L
       SET LinkMadeBy = COALESCE(NULLIF(LTRIM(RTRIM(L.LinkMadeBy)), N''), @FallbackUserId),
           AddDateMade = COALESCE(L.AddDateMade, GETDATE()),
           Ntimeschanged = COALESCE(L.Ntimeschanged, 0)
      FROM dbo.TSql_Language L
     WHERE L.LinkMadeBy IS NULL OR L.AddDateMade IS NULL OR L.Ntimeschanged IS NULL;

    UPDATE L SET LinkMadeBy = @FallbackUserId
      FROM dbo.TSql_Language L
     WHERE L.LinkMadeBy NOT IN (SELECT Id FROM dbo.AspNetUsers);

    IF NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE LinkMadeBy IS NULL)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Language_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Language'))
            ALTER TABLE dbo.TSql_Language ADD CONSTRAINT FK_TSql_Language_AspNetUsers
                FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);

        IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_Language_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_Language'))
            ALTER TABLE dbo.TSql_Language ADD CONSTRAINT FK_TSql_Language_AspNetUsers1
                FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
    END
    ELSE
        PRINT N'Aviso: FK_TSql_Language_AspNetUsers omitida — LinkMadeBy NULL en alguna fila. Rellenar LinkMadeBy y volver a ejecutar el script.';
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
    PRINT N'Aviso: FK auditoría TSql_Language omitidas — dbo.AspNetUsers vacía o inexistente.';
GO

/* FK opcional: idioma → país (dbo.TSql_Countrys.IdObject) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TSql_Language', N'LinkCountry') IS NOT NULL
   AND OBJECT_ID(N'dbo.TSql_Countrys', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
         FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_TSql_Language_TSql_Countrys'
          AND fk.parent_object_id = OBJECT_ID(N'dbo.TSql_Language')
   )
BEGIN
    ALTER TABLE dbo.TSql_Language
        ADD CONSTRAINT FK_TSql_Language_TSql_Countrys
            FOREIGN KEY (LinkCountry) REFERENCES dbo.TSql_Countrys (IdObject);
END
GO

/* Idioma por defecto: español (es) */
IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.AspNetUsers)
   AND NOT EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE TextCode = N'es' AND Is_Delete = 0)
BEGIN
    DECLARE @SeedUser NVARCHAR(128);
    DECLARE @SeedLinMod NVARCHAR(128);
    SELECT TOP (1) @SeedUser = Id FROM dbo.AspNetUsers ORDER BY Id;

    SELECT @SeedLinMod = CASE
        WHEN EXISTS (
            SELECT 1
              FROM sys.columns c
              INNER JOIN sys.tables t ON t.object_id = c.object_id
              INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
             WHERE s.name = N'dbo'
               AND t.name = N'TSql_Language'
               AND c.name = N'LinModifiedBy'
               AND c.is_nullable = 1
        )
        THEN NULL
        ELSE @SeedUser
    END;

    DECLARE @LangOid INT = OBJECT_ID(N'dbo.TSql_Language');
    DECLARE @HasSysObjectID BIT = CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'SysObjectID') IS NOT NULL THEN 1 ELSE 0 END;
    DECLARE @SysObjectIDIsIdentity BIT = 0;

    IF @HasSysObjectID = 1
        SET @SysObjectIDIsIdentity = CASE WHEN COLUMNPROPERTY(@LangOid, N'SysObjectID', N'IsIdentity') = 1 THEN 1 ELSE 0 END;

    DECLARE @HasAttLabel BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'AttLabel') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasAttSinglature BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'AttSinglature') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasLinflag BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'Linflag') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasLinCreatedBy BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'LinCreatedBy') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasAttCreated BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'AttCreated') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasAttLastModification BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'AttLastModification') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasSysUpdateNumber BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'SysUpdateNumber') IS NOT NULL THEN 1 ELSE 0 END;

    DECLARE @HasAttIsDeleted BIT =
        CASE WHEN COL_LENGTH(N'dbo.TSql_Language', N'AttIsDeleted') IS NOT NULL THEN 1 ELSE 0 END;

    /* Evitar 8152: truncar literales al max_length real de cada columna (legado suele ser más estrecho). */
    DECLARE @MxTextLabel INT = 500;
    DECLARE @MxTextCode INT = 20;
    DECLARE @MxTextNative INT = 100;
    DECLARE @MxAttLabel INT = 500;
    DECLARE @MxAttSing INT = 500;

    SELECT @MxTextLabel = CASE
            WHEN c.max_length < 1 THEN 4000
            WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
            WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
            ELSE c.max_length
        END
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE c.object_id = @LangOid AND c.name = N'TextLabel';

    SELECT @MxTextCode = CASE
            WHEN c.max_length < 1 THEN 4000
            WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
            WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
            ELSE c.max_length
        END
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE c.object_id = @LangOid AND c.name = N'TextCode';

    SELECT @MxTextNative = CASE
            WHEN c.max_length < 1 THEN 4000
            WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
            WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
            ELSE c.max_length
        END
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE c.object_id = @LangOid AND c.name = N'TextNativeName';

    IF @HasAttLabel = 1
        SELECT @MxAttLabel = CASE
                WHEN c.max_length < 1 THEN 4000
                WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
                WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
                ELSE c.max_length
            END
        FROM sys.columns c
        INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE c.object_id = @LangOid AND c.name = N'AttLabel';

    IF @HasAttSinglature = 1
        SELECT @MxAttSing = CASE
                WHEN c.max_length < 1 THEN 4000
                WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
                WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
                ELSE c.max_length
            END
        FROM sys.columns c
        INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE c.object_id = @LangOid AND c.name = N'AttSinglature';

    DECLARE @MxLM INT = 128;
    DECLARE @MxLMM INT = 128;

    SELECT @MxLM = CASE
            WHEN c.max_length < 1 THEN 4000
            WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
            WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
            ELSE c.max_length
        END
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE c.object_id = @LangOid AND c.name = N'LinkMadeBy';

    SELECT @MxLMM = CASE
            WHEN c.max_length < 1 THEN 4000
            WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
            WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
            ELSE c.max_length
        END
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    WHERE c.object_id = @LangOid AND c.name = N'LinModifiedBy';

    DECLARE @MxLinCr INT = 128;

    IF @HasLinCreatedBy = 1
        SELECT @MxLinCr = CASE
                WHEN c.max_length < 1 THEN 4000
                WHEN ty.name IN (N'nvarchar', N'nchar') THEN c.max_length / 2
                WHEN ty.name IN (N'varchar', N'char', N'varbinary', N'binary') THEN c.max_length
                ELSE c.max_length
            END
        FROM sys.columns c
        INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        WHERE c.object_id = @LangOid AND c.name = N'LinCreatedBy';

    DECLARE @SeedLbl NVARCHAR(4000) = LEFT(N'Español', CASE WHEN @MxTextLabel < 1 THEN 1 ELSE @MxTextLabel END);
    DECLARE @SeedCode NVARCHAR(4000) = LEFT(N'es', CASE WHEN @MxTextCode < 1 THEN 1 ELSE @MxTextCode END);
    DECLARE @SeedNat NVARCHAR(4000) = LEFT(N'Español', CASE WHEN @MxTextNative < 1 THEN 1 ELSE @MxTextNative END);
    DECLARE @SeedAttLbl NVARCHAR(4000) = LEFT(N'Español', CASE WHEN @MxAttLabel < 1 THEN 1 ELSE @MxAttLabel END);
    DECLARE @SeedAttSing NVARCHAR(4000) = LEFT(N'Español', CASE WHEN @MxAttSing < 1 THEN 1 ELSE @MxAttSing END);

    DECLARE @SeedUserT NVARCHAR(128) = LEFT(@SeedUser, CASE WHEN @MxLM < 1 THEN 1 WHEN @MxLM > 128 THEN 128 ELSE @MxLM END);
    DECLARE @SeedLinModT NVARCHAR(128) =
        CASE
            WHEN @SeedLinMod IS NULL THEN NULL
            ELSE LEFT(@SeedLinMod, CASE WHEN @MxLMM < 1 THEN 1 WHEN @MxLMM > 128 THEN 128 ELSE @MxLMM END)
        END;
    DECLARE @SeedLinCrT NVARCHAR(128) = LEFT(@SeedUser, CASE WHEN @MxLinCr < 1 THEN 1 WHEN @MxLinCr > 128 THEN 128 ELSE @MxLinCr END);

    DECLARE @InsCols NVARCHAR(MAX) = N'';
    DECLARE @InsVals NVARCHAR(MAX) = N'';

    IF @HasSysObjectID = 1 AND @SysObjectIDIsIdentity = 0
    BEGIN
        SET @InsCols = N'SysObjectID, ';
        SET @InsVals = N'@sid, ';
    END

    IF @HasAttLabel = 1
    BEGIN
        SET @InsCols += N'AttLabel, ';
        SET @InsVals += N'@al, ';
    END

    IF @HasAttSinglature = 1
    BEGIN
        SET @InsCols += N'AttSinglature, ';
        SET @InsVals += N'@asg, ';
    END

    IF @HasLinflag = 1
    BEGIN
        SET @InsCols += N'Linflag, ';
        SET @InsVals += N'0, ';
    END

    SET @InsCols += N'TextLabel, TextCode, TextNativeName, Is_Default, Is_Delete, Is_Active, ';
    SET @InsVals += N'@tl, @tc, @tn, 1, 0, 1, ';

    IF @HasAttCreated = 1
    BEGIN
        SET @InsCols += N'AttCreated, ';
        SET @InsVals += N'GETDATE(), ';
    END

    IF @HasAttLastModification = 1
    BEGIN
        SET @InsCols += N'AttLastModification, ';
        SET @InsVals += N'GETDATE(), ';
    END

    IF @HasSysUpdateNumber = 1
    BEGIN
        SET @InsCols += N'SysUpdateNumber, ';
        SET @InsVals += N'0, ';
    END

    IF @HasAttIsDeleted = 1
    BEGIN
        SET @InsCols += N'AttIsDeleted, ';
        SET @InsVals += N'CAST(0 AS BIT), ';
    END

    IF @HasLinCreatedBy = 1
    BEGIN
        SET @InsCols += N'LinCreatedBy, ';
        SET @InsVals += N'@lcr, ';
    END

    SET @InsCols += N'LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged';
    SET @InsVals += N'@u, @lm, GETDATE(), NULL, 0';

    DECLARE @InsSql NVARCHAR(MAX) =
        N'INSERT INTO dbo.TSql_Language (' + @InsCols + N') VALUES (' + @InsVals + N');';

    IF @HasSysObjectID = 1 AND @SysObjectIDIsIdentity = 0
    BEGIN
        DECLARE @NextSysObjectID BIGINT;

        EXEC sp_executesql
            N'SELECT @o = ISNULL(MAX(SysObjectID), 0) + 1 FROM dbo.TSql_Language;',
            N'@o BIGINT OUTPUT',
            @o = @NextSysObjectID OUTPUT;

        EXEC sp_executesql
            @InsSql,
            N'@sid BIGINT, @u NVARCHAR(128), @lm NVARCHAR(128), @lcr NVARCHAR(128), @tl NVARCHAR(4000), @tc NVARCHAR(4000), @tn NVARCHAR(4000), @al NVARCHAR(4000), @asg NVARCHAR(4000)',
            @sid = @NextSysObjectID,
            @u = @SeedUserT,
            @lm = @SeedLinModT,
            @lcr = @SeedLinCrT,
            @tl = @SeedLbl,
            @tc = @SeedCode,
            @tn = @SeedNat,
            @al = @SeedAttLbl,
            @asg = @SeedAttSing;
    END
    ELSE
        EXEC sp_executesql
            @InsSql,
            N'@u NVARCHAR(128), @lm NVARCHAR(128), @lcr NVARCHAR(128), @tl NVARCHAR(4000), @tc NVARCHAR(4000), @tn NVARCHAR(4000), @al NVARCHAR(4000), @asg NVARCHAR(4000)',
            @u = @SeedUserT,
            @lm = @SeedLinModT,
            @lcr = @SeedLinCrT,
            @tl = @SeedLbl,
            @tc = @SeedCode,
            @tn = @SeedNat,
            @al = @SeedAttLbl,
            @asg = @SeedAttSing;

    PRINT N'OK — Insertado idioma por defecto TextCode = es.';
END
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
     AND EXISTS (SELECT 1 FROM dbo.TSql_Language WHERE TextCode = N'es' AND Is_Delete = 0)
    PRINT N'OK — Idioma es ya existía.';
ELSE IF OBJECT_ID(N'dbo.TSql_Language', N'U') IS NOT NULL
    PRINT N'Aviso: no se insertó idioma es — falta dbo.AspNetUsers con al menos un usuario.';
GO

PRINT N'OK — dbo.TSql_Language creada (o ya existía).';
GO
