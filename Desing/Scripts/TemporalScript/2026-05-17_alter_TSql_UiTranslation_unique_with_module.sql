/* =============================================================================
   PROPUESTA — NO ejecutar sin aprobación.

   Motivo:
     El índice actual `UX_TSql_UiTranslation_Key_Language_Active` es único sobre
     (TextResourceKey, LinkLanguage) sin incluir TextModule. Con la entrada
     en juego del módulo `Common` (resx-backed) hay claves homónimas en varios
     módulos (p. ej. `Btn_Save` aparece en `ClientV2`, `MasterArticles` y
     `Common`). Al exportar / importar via la pantalla `/UiTranslation`, el
     segundo upsert con misma `(TextResourceKey, LinkLanguage)` y distinto
     `TextModule` viola el unique → import marca error de fila.

   Cambio:
     Sustituir el índice único por uno que incluya `TextModule` en la clave.
     Para tolerar filas históricas con `TextModule = NULL`, la unicidad se
     calcula sobre `ISNULL(TextModule, N'')` materializado en una columna
     persistida (compatible con SQL Server estándar; alternativa con índice
     filtrado si la columna persistida no fuese aceptable).

   Idempotencia:
     - DROP del índice viejo solo si existe.
     - ADD de la columna calculada solo si no existe.
     - CREATE del nuevo índice único solo si no existe.

   Tras ejecutar:
     - El controlador `UiTranslationController.UpsertTranslationNoSave` ya hace
       match por (TextResourceKey, LinkLanguage, TextModule) → no requiere
       cambios.
     - Se mantienen los índices `IX_TSql_UiTranslation_Lookup` y
       `IX_TSql_UiTranslation_Module_Active` para consultas habituales.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_UiTranslation', N'U') IS NULL
BEGIN
    PRINT N'Aviso: dbo.TSql_UiTranslation no existe. Script omitido.';
    RETURN;
END
GO

/* 1) Quitar el unique antiguo si está presente. */
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_UiTranslation_Key_Language_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
    DROP INDEX UX_TSql_UiTranslation_Key_Language_Active ON dbo.TSql_UiTranslation;
GO

/* 2) Columna calculada persistida con TextModule normalizado (NULL → ''). */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE name = N'TextModuleNorm'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
BEGIN
    ALTER TABLE dbo.TSql_UiTranslation
        ADD TextModuleNorm AS (ISNULL([TextModule], N'')) PERSISTED;
END
GO

/* 3) Unique nuevo: (TextResourceKey, LinkLanguage, TextModuleNorm) WHERE Is_Delete = 0. */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TSql_UiTranslation_Key_Language_Module_Active'
      AND object_id = OBJECT_ID(N'dbo.TSql_UiTranslation')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_UiTranslation_Key_Language_Module_Active
        ON dbo.TSql_UiTranslation (TextResourceKey, LinkLanguage, TextModuleNorm)
        WHERE Is_Delete = 0;
GO

PRINT N'OK — UX_TSql_UiTranslation_Key_Language_Module_Active creado (o ya existía).';
GO
