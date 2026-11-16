/* =============================================================================
   dbo.TSql_AiCommandUserPhrase — Palabras / frases propias de cada usuario para activar un comando.
   Requiere antes: 2026-11-14_create_TSql_AiCommand.sql y 2026-11-14_create_TSql_AiCommandPhrase.sql.

   Is_Exclude = 0 → frase propia que activa LinkAiCommand para ese usuario.
   Is_Exclude = 1 → «no uses esta frase por defecto para mí» (LinkAiCommandPhrase obligatorio).
   Frases efectivas (usuario, comando, idioma) =
       (por defecto activas, si Is_UseDefaultPhrases <> 0, menos las excluidas) ∪ (propias activas).
   Unicidad: un usuario no puede repetir la misma frase en el mismo contexto e idioma (índice único).
     El cruce con las frases por defecto de OTRO comando lo valida el servicio al guardar
     (consulta de validación al final de este script).
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

IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TSql_AiCommandUserPhrase
    (
        IdObject      BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TSql_AiCommandUserPhrase PRIMARY KEY CLUSTERED,

        /* ----- Negocio ----- */
        LinkAspNetUsers     NVARCHAR(128) NOT NULL,
        LinkAiCommand       BIGINT        NOT NULL,
        LinkAiCommandPhrase BIGINT        NULL,
        TextContext         NVARCHAR(20)  NOT NULL,
        TextLanguageCode    NVARCHAR(10)  NOT NULL,
        TextPhrase          NVARCHAR(200) NOT NULL,
        TextPhraseNorm      NVARCHAR(200) NOT NULL,
        Is_Exclude          BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUserPhrase_Is_Exclude DEFAULT (0),
        CONSTRAINT CK_TSql_AiCommandUserPhrase_Exclude CHECK (Is_Exclude = 0 OR LinkAiCommandPhrase IS NOT NULL),
        /* ----- Auditoría obligatoria ----- */
        TextLabel     NVARCHAR(500) NOT NULL,
        Is_Delete     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUserPhrase_Is_Delete DEFAULT (0),
        Is_Active     BIT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUserPhrase_Is_Active DEFAULT (1),
        LinkMadeBy    NVARCHAR(128) NOT NULL,
        LinModifiedBy NVARCHAR(128) NULL,
        AddDateMade   DATETIME NOT NULL
            CONSTRAINT DF_TSql_AiCommandUserPhrase_AddDateMade DEFAULT (GETDATE()),
        AddLastDateChange DATETIME NULL,
        Ntimeschanged BIGINT NOT NULL
            CONSTRAINT DF_TSql_AiCommandUserPhrase_Ntimeschanged DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_TSql_AiCommand' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_TSql_AiCommand
        FOREIGN KEY (LinkAiCommand) REFERENCES dbo.TSql_AiCommand (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_TSql_AiCommandPhrase' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_TSql_AiCommandPhrase
        FOREIGN KEY (LinkAiCommandPhrase) REFERENCES dbo.TSql_AiCommandPhrase (IdObject);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_AspNetUsers2' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_AspNetUsers2
        FOREIGN KEY (LinkAspNetUsers) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'UX_TSql_AiCommandUserPhrase_User_Context_Lang_Norm' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase')
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_TSql_AiCommandUserPhrase_User_Context_Lang_Norm
        ON dbo.TSql_AiCommandUserPhrase (LinkAspNetUsers, TextContext, TextLanguageCode, TextPhraseNorm)
        WHERE Is_Delete = 0 AND Is_Exclude = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandUserPhrase_Command_Lang' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandUserPhrase_Command_Lang
        ON dbo.TSql_AiCommandUserPhrase (LinkAiCommand, TextLanguageCode)
        INCLUDE (LinkAspNetUsers, TextPhraseNorm, Is_Exclude, Is_Active)
        WHERE Is_Delete = 0;
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_TSql_AiCommandUserPhrase_User' AND object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase')
)
    CREATE NONCLUSTERED INDEX IX_TSql_AiCommandUserPhrase_User
        ON dbo.TSql_AiCommandUserPhrase (LinkAspNetUsers, TextContext, TextLanguageCode)
        INCLUDE (LinkAiCommand, LinkAiCommandPhrase, TextPhrase, TextPhraseNorm, Is_Exclude, Is_Active)
        WHERE Is_Delete = 0;
GO
/* Validación (solo lectura) que debe hacer el servicio antes de guardar una frase propia:
   ¿la frase ya activa OTRO comando por defecto en el mismo contexto e idioma (o su idioma base)?

   SELECT C.TextCode
     FROM dbo.TSql_AiCommandPhrase P
     JOIN dbo.TSql_AiCommand C ON C.IdObject = P.LinkAiCommand
    WHERE P.Is_Delete = 0 AND P.Is_Active = 1 AND P.TextKind IN (N'keyword', N'example')
      AND P.TextContext IN (@Context, N'common')
      AND P.TextLanguageCode IN (@Lang, LEFT(@Lang, CHARINDEX(N'-', @Lang + N'-') - 1))  -- variante + base
      AND P.TextPhraseNorm = @Norm AND P.LinkAiCommand <> @LinkAiCommand;
*/
/* FK auditoría → AspNetUsers (patrón TSql_UiTranslation) */
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_AspNetUsers' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_AspNetUsers
        FOREIGN KEY (LinkMadeBy) REFERENCES dbo.AspNetUsers (Id);
GO
IF OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TSql_AiCommandUserPhrase_AspNetUsers1' AND parent_object_id = OBJECT_ID(N'dbo.TSql_AiCommandUserPhrase'))
    ALTER TABLE dbo.TSql_AiCommandUserPhrase WITH CHECK ADD CONSTRAINT FK_TSql_AiCommandUserPhrase_AspNetUsers1
        FOREIGN KEY (LinModifiedBy) REFERENCES dbo.AspNetUsers (Id);
GO

PRINT N'OK — dbo.TSql_AiCommandUserPhrase creada (o ya existía).';
GO
