/* =============================================================================
   dbo.TSql_AiCommandPhrase — Palabras / frases POR DEFECTO que activan cada comando (todas las personas).
   Requiere antes: 2026-11-14_create_TSql_AiCommand.sql.

   TextKind:
       keyword  activa el comando por sí sola (verbo o frase hecha)
       helper   suma puntuación pero no activa sola (p. ej. «haz» + «muro», «paralelo», pesos < 2 de la IA del muro)
       negative resta (IA del muro: verbos de otras órdenes, NumberWeight < 0)
       example  frase de ejemplo (ayuda y «¿Querías decir…?»)
   TextConcept: clave del vocabulario JS (draw, del, rotate, gen3d…) para reconstruir MA_STL_DESIGN_AI_WORDS.
   NumberWeight: peso de MA_STL_WALL_AI_VOCAB (IA del muro); NULL en la IA del diseño.
   TextContext: copia de TSql_AiCommand.TextContext (necesaria para el índice único).
   Unicidad: una frase keyword/example no puede activar dos comandos del mismo contexto e idioma
     (UX_..._Context_Lang_Norm). helper/negative sí pueden repetirse.
   TextPhraseNorm: clave de comparación = minúsculas, sin tildes (ñ→n), sin signos (¿?¡!.,;:«»"),
     espacios simples. La calcula quien inserta (seed / servicio C#) con la misma regla que
     maStlWallAiNormalize del intérprete JS; la unicidad se aplica sobre ella, no sobre TextPhrase.
   TextLanguageCode: etiqueta BCP-47 en minúsculas, NVARCHAR(10): idioma base ('es', 'en') o variante
     regional / jerga ('es-es', 'es-ar', 'es-mx', 'en-us'…). Respaldo: una variante usa sus frases
     MÁS las del idioma base ('es-ar' → 'es-ar' + 'es'); sin variante, solo el base.
     Es texto y no FK porque develop solo tiene el legado TSql_language (SysObjectID, 'sp') y producción
     TSql_Language (IdObject, TextCode 'es'/'en'); el idioma base coincide con TSql_Language.TextCode.
   Volumen: sin límite de frases por comando ni por idioma (miles de jergas); índices por
     (comando, idioma) para cargar y (contexto, idioma, frase normalizada) para resolver y validar.
   Convención: .cursor/rules/sql-tsql-table-conventions.mdc (9 columnas de auditoría, borrado lógico).
   Script idempotente. Tras ejecutar: Update Model from Database (DAL/Model.edmx).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiCommandPhrase
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiCommandPhrase PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        LinkAiCommand    BIGINT        NOT NULL,
        TextContext      NVARCHAR(20)  NOT NULL,
        TextLanguageCode NVARCHAR(10)  NOT NULL,
        TextPhrase       NVARCHAR(200) NOT NULL,
        TextPhraseNorm   NVARCHAR(200) NOT NULL,
        TextKind         NVARCHAR(20)  NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_TextKind DEFAULT (N'keyword')
            CONSTRAINT CK_TSql_AiCommandPhrase_TextKind CHECK (TextKind IN (N'keyword', N'helper', N'negative', N'example')),
        TextConcept      NVARCHAR(40)  NULL,
        NumberWeight     DECIMAL(6,2)  NULL,
        NumberOrder      INT NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_NumberOrder DEFAULT (0),
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiCommandPhrase_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandPhrase_TSql_AiCommand' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase'))
    ALTER TABLE dbo.TSql_AiCommandPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandPhrase_TSql_AiCommand
        FOREIGN KEY (LinkAiCommand) REFERENCES dbo.TSql_AiCommand (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiCommandPhrase_Command_Lang_Norm' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiCommandPhrase_Command_Lang_Norm
        ON dbo.TSql_AiCommandPhrase (LinkAiCommand, TextLanguageCode, TextPhraseNorm)
        WHERE Is_Delete = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiCommandPhrase_Context_Lang_Norm' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiCommandPhrase_Context_Lang_Norm
        ON dbo.TSql_AiCommandPhrase (TextContext, TextLanguageCode, TextPhraseNorm)
        WHERE Is_Delete = 0 AND TextKind IN (N'keyword', N'example');
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandPhrase_Command_Lang' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandPhrase_Command_Lang
        ON dbo.TSql_AiCommandPhrase (LinkAiCommand, TextLanguageCode)
        INCLUDE (TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, NumberOrder, Is_Active)
        WHERE Is_Delete = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandPhrase_Lookup' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandPhrase_Lookup
        ON dbo.TSql_AiCommandPhrase (TextContext, TextLanguageCode)
        INCLUDE (LinkAiCommand, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, Is_Active)
        WHERE Is_Delete = 0;
GO
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandPhrase_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase'))
    ALTER TABLE dbo.TSql_AiCommandPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandPhrase_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandPhrase_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandPhrase'))
    ALTER TABLE dbo.TSql_AiCommandPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandPhrase_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiCommandPhrase creada (o ya existía).';
GO
