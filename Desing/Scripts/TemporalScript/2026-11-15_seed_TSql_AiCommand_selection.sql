/* =============================================================================
   Muro seleccionado y «acota todo» en la IA del diseño (pt117u, 15/11/2026). Idempotente.
   · dbo.TSql_AiCommandPhrase: 15 frases ES/EN («selecciona el muro 7» → WALL_LIST, que lo selecciona en verde;
     «acota todo» / «todas las cotas» → WALL_DIMS, cotas de todo el diseño).
   · dbo.TSql_AiEnvironmentReply: 10 plantillas (design.dimsPick / design.articleIdsPick) y
     design.dimsAll ES sin «como con el botón Cotas» (el botón ya acota solo el muro seleccionado).
   Sin huecos de IDENTITY entre develop y producción: si va a insertar filas, antes reajusta el contador de cada
   tabla al máximo IdObject existente (DBCC CHECKIDENT … RESEED).
   Requisitos: create / seed del 14/11/2026 y semilla de cotas por voz (WALL_LIST, WALL_DIMS).
   Ejecutar con: sqlcmd -S .\SQLEXPRESS -E -d <bd> -f 65001 -i <este fichero>  (develop y producción).
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @User NVARCHAR(128) =
(
    SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id
);

IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
     OR OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL
     OR NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommand WHERE TextCode = N'WALL_DIMS' AND Is_Delete = 0)
     OR NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommand WHERE TextCode = N'WALL_LIST' AND Is_Delete = 0)
    PRINT N'Aviso: faltan el catálogo de la IA o la semilla de cotas por voz (WALL_LIST / WALL_DIMS).';
ELSE
BEGIN
    BEGIN TRANSACTION;

    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberOrder)
    VALUES
        (N'WALL_LIST', N'es', N'selecciona el muro', N'selecciona el muro', N'keyword', N'show', 1010),
        (N'WALL_LIST', N'es', N'seleccióname el muro', N'seleccioname el muro', N'keyword', N'show', 1020),
        (N'WALL_LIST', N'es', N'selecciona la pared', N'selecciona la pared', N'keyword', N'show', 1030),
        (N'WALL_LIST', N'es', N'marca el muro', N'marca el muro', N'keyword', N'show', 1040),
        (N'WALL_LIST', N'en', N'select wall', N'select wall', N'keyword', N'show', 1050),
        (N'WALL_LIST', N'en', N'select the wall', N'select the wall', N'keyword', N'show', 1060),
        (N'WALL_LIST', N'en', N'highlight wall', N'highlight wall', N'keyword', N'show', 1070),
        (N'WALL_LIST', N'es', N'selecciona el muro 7', N'selecciona el muro 7', N'example', NULL, 1080),
        (N'WALL_LIST', N'en', N'select wall 7', N'select wall 7', N'example', NULL, 1090),
        (N'WALL_DIMS', N'es', N'acota todo', N'acota todo', N'keyword', N'dims', 1010),
        (N'WALL_DIMS', N'es', N'acota todo el diseño', N'acota todo el diseno', N'keyword', N'dims', 1020),
        (N'WALL_DIMS', N'es', N'todas las cotas', N'todas las cotas', N'keyword', N'dims', 1030),
        (N'WALL_DIMS', N'es', N'cotas de todo el diseño', N'cotas de todo el diseno', N'keyword', N'dims', 1040),
        (N'WALL_DIMS', N'en', N'dimension everything', N'dimension everything', N'keyword', N'dims', 1050),
        (N'WALL_DIMS', N'en', N'all dimensions', N'all dimensions', N'keyword', N'dims', 1060);
    DECLARE @Rep TABLE (EnvCode NVARCHAR(40) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextMessageKey NVARCHAR(100) NOT NULL,
                        TextTemplate NVARCHAR(2000) NOT NULL);
    INSERT INTO @Rep (EnvCode, TextLanguageCode, TextMessageKey, TextTemplate)
    VALUES
        (N'EXECUTIVE', N'es', N'design.dimsPick', N'Las cotas son las del muro seleccionado y no hay ninguno: le muestro el número de cada muro ({1}). Diga «acótame el muro {0}», «selecciona el muro {0}» o «acota todo».'),
        (N'EXECUTIVE', N'es', N'design.articleIdsPick', N'Los ID de los paneles se muestran en el muro seleccionado y no hay ninguno. Diga «selecciona el muro {0}» o «acota todo».'),
        (N'COLLOQUIAL', N'es', N'design.dimsPick', N'Las cotas van con el muro seleccionado y no tienes ninguno: te pongo el número de cada muro ({1}). Dime «acótame el muro {0}», «selecciona el muro {0}» o «acota todo».'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsPick', N'Los ID de los paneles van con el muro seleccionado y no tienes ninguno. Dime «selecciona el muro {0}» o «acota todo».'),
        (N'STREET', N'es', N'design.dimsPick', N'No hay muro seleccionado, colega: te pongo los números ({1}). Dime «acótame el muro {0}», «selecciona el muro {0}» o «acota todo».'),
        (N'STREET', N'es', N'design.articleIdsPick', N'No hay muro seleccionado, máquina. Dime «selecciona el muro {0}» o «acota todo».'),
        (N'TECHNICAL', N'es', N'design.dimsPick', N'Sin muro seleccionado: ID de muros visibles ({1}). Usar «acota el muro {0}», «selecciona el muro {0}» o «acota todo».'),
        (N'TECHNICAL', N'es', N'design.articleIdsPick', N'Sin muro seleccionado. Usar «selecciona el muro {0}» o «acota todo».'),
        (N'EXECUTIVE', N'en', N'design.dimsPick', N'Dimensions are shown for the selected wall and none is selected: here are the wall numbers ({1}). Say «dimension wall {0}», «select wall {0}» or «dimension everything».'),
        (N'EXECUTIVE', N'en', N'design.articleIdsPick', N'Panel IDs are shown on the selected wall and none is selected. Say «select wall {0}» or «dimension everything».');

    /* Frases que se insertarían (las keyword/example que ya activan cualquier comando del contexto se omiten). */
    DECLARE @NewPhr TABLE (TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhraseNorm NVARCHAR(200) NOT NULL);
    INSERT INTO @NewPhr (TextCode, TextLanguageCode, TextPhraseNorm)
    SELECT S.TextCode, S.TextLanguageCode, S.TextPhraseNorm
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommandPhrase T
                        WHERE T.LinkAiCommand = C.IdObject AND T.TextLanguageCode = S.TextLanguageCode
                          AND T.TextPhraseNorm = S.TextPhraseNorm AND T.Is_Delete = 0)
       AND (S.TextKind NOT IN (N'keyword', N'example') OR NOT EXISTS (
               SELECT 1 FROM dbo.TSql_AiCommandPhrase O
                WHERE O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                  AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0 AND O.TextKind IN (N'keyword', N'example')));

    SELECT S.TextCode, S.TextLanguageCode, S.TextPhraseNorm, OC.TextCode AS YaEnComando
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
      JOIN dbo.TSql_AiCommandPhrase O ON O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                                     AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0
                                     AND O.TextKind IN (N'keyword', N'example') AND O.LinkAiCommand <> C.IdObject
      JOIN dbo.TSql_AiCommand OC ON OC.IdObject = O.LinkAiCommand
     WHERE S.TextKind IN (N'keyword', N'example');

    /* ---- Contadores IDENTITY = máximo IdObject (solo si faltan filas): mismos IdObject en develop y producción ---- */
    IF EXISTS (SELECT 1 FROM @NewPhr)
    BEGIN
        DECLARE @MaxPhr INT = (SELECT ISNULL(MAX(IdObject), 0) FROM dbo.TSql_AiCommandPhrase);
        DBCC CHECKIDENT (N'dbo.TSql_AiCommandPhrase', RESEED, @MaxPhr) WITH NO_INFOMSGS;
    END
    IF EXISTS (SELECT 1 FROM @Rep S JOIN dbo.TSql_AiEnvironment E ON E.TextCode = S.EnvCode AND E.Is_Delete = 0
                WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiEnvironmentReply T WHERE T.LinkEnvironment = E.IdObject
                                     AND T.TextLanguageCode = S.TextLanguageCode AND T.TextMessageKey = S.TextMessageKey AND T.Is_Delete = 0))
    BEGIN
        DECLARE @MaxRep INT = (SELECT ISNULL(MAX(IdObject), 0) FROM dbo.TSql_AiEnvironmentReply);
        DBCC CHECKIDENT (N'dbo.TSql_AiEnvironmentReply', RESEED, @MaxRep) WITH NO_INFOMSGS;
    END

    INSERT INTO dbo.TSql_AiCommandPhrase
    (
        LinkAiCommand, TextContext, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, NumberOrder, LinkEnvironment,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT C.IdObject, C.TextContext, S.TextLanguageCode, S.TextPhrase, S.TextPhraseNorm, S.TextKind, S.TextConcept, NULL, S.NumberOrder, NULL,
           S.TextPhrase, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
      JOIN @NewPhr N ON N.TextCode = S.TextCode AND N.TextLanguageCode = S.TextLanguageCode AND N.TextPhraseNorm = S.TextPhraseNorm
     ORDER BY S.TextCode DESC, S.NumberOrder;

    INSERT INTO dbo.TSql_AiEnvironmentReply
    (
        LinkEnvironment, TextLanguageCode, TextMessageKey, TextTemplate,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT E.IdObject, S.TextLanguageCode, S.TextMessageKey, S.TextTemplate,
           S.TextMessageKey, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Rep S
      JOIN dbo.TSql_AiEnvironment E ON E.TextCode = S.EnvCode AND E.Is_Delete = 0
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiEnvironmentReply T
                        WHERE T.LinkEnvironment = E.IdObject AND T.TextLanguageCode = S.TextLanguageCode
                          AND T.TextMessageKey = S.TextMessageKey AND T.Is_Delete = 0)
     ORDER BY E.IdObject, S.TextLanguageCode DESC, S.TextMessageKey;

    /* ---- design.dimsAll (ES): sin «como con el botón Cotas», solo si sigue el texto antiguo ---- */
    DECLARE @Upd TABLE (EnvCode NVARCHAR(40) NOT NULL, OldTemplate NVARCHAR(2000) NOT NULL, NewTemplate NVARCHAR(2000) NOT NULL);
    INSERT INTO @Upd (EnvCode, OldTemplate, NewTemplate)
    VALUES
        (N'EXECUTIVE', N'Cotas de todo el diseño activadas, como con el botón Cotas. Para un solo muro, diga «acótame el muro {0}».', N'Cotas de todo el diseño activadas. Para un solo muro, diga «acótame el muro {0}».'),
        (N'COLLOQUIAL', N'Te he puesto las cotas de todo el diseño, como con el botón Cotas. Si quieres solo uno, dime «acótame el muro {0}».', N'Te he puesto las cotas de todo el diseño. Si quieres solo uno, dime «acótame el muro {0}».'),
        (N'STREET', N'Cotas de todo el diseño puestas, como con el botón Cotas. Si quieres solo uno, dime «acótame el muro {0}».', N'Cotas de todo el diseño puestas. Si quieres solo uno, dime «acótame el muro {0}».');
    UPDATE T SET T.TextTemplate = U.NewTemplate, T.Ntimeschanged = ISNULL(T.Ntimeschanged, 0) + 1
      FROM dbo.TSql_AiEnvironmentReply T
      JOIN dbo.TSql_AiEnvironment E ON E.IdObject = T.LinkEnvironment AND E.Is_Delete = 0
      JOIN @Upd U ON U.EnvCode = E.TextCode
     WHERE T.TextLanguageCode = N'es' AND T.TextMessageKey = N'design.dimsAll' AND T.Is_Delete = 0
       AND T.TextTemplate = U.OldTemplate;

    COMMIT TRANSACTION;
    PRINT N'OK — muro seleccionado: 15 frases, 10 plantillas, design.dimsAll al día.';
END
GO
