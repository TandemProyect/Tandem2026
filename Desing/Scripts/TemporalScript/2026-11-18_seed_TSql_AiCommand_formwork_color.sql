/* =============================================================================
   IA del diseño: modo Encofrado colores (18/11/2026). Idempotente.
   · dbo.TSql_AiCommand: MODE_FORMWORK_COLOR (pulsa el botón Colores; pide confirmación).
   · dbo.TSql_AiCommandPhrase: frases ES/EN. «encofra» a secas sigue en MODE_FORMWORK.
   · dbo.TSql_AiEnvironmentReply: plantillas design.confirmFormworkColor y el resto del modo.
   Requisitos: catálogo del 14/11/2026. Ejecutar en develop y producción.
   sqlcmd -S 'NUCBOXG5\SQLEXPRESS' -E -d TdesingDevelop -I -f 65001 -b -i <este fichero>
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @User NVARCHAR(128) =
(
    SELECT Id FROM dbo.AspNetUsers WHERE UserName = N'juan.godoy@vscad.com'
);
IF @User IS NULL
    SET @User = (SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id);

IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla de encofrado colores omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
     OR OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL
    PRINT N'Aviso: faltan las tablas del catálogo de la IA.';
ELSE
BEGIN
    BEGIN TRANSACTION;

    DECLARE @Cmd TABLE
    (
        TextCode NVARCHAR(60) NOT NULL PRIMARY KEY, TextContext NVARCHAR(20) NOT NULL, TextGroup NVARCHAR(100) NOT NULL,
        TextNameEs NVARCHAR(200) NOT NULL, TextNameEn NVARCHAR(200) NOT NULL, TextDescription NVARCHAR(500) NULL,
        TextHandler NVARCHAR(300) NOT NULL, TextAiCommandId NVARCHAR(60) NULL, TextParamsJson NVARCHAR(MAX) NULL,
        Is_Destructive BIT NOT NULL, Is_Confirm BIT NOT NULL, Is_AiImplemented BIT NOT NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Cmd (TextCode, TextContext, TextGroup, TextNameEs, TextNameEn, TextDescription, TextHandler, TextAiCommandId,
                      TextParamsJson, Is_Destructive, Is_Confirm, Is_AiImplemented, NumberOrder)
    VALUES
        (N'MODE_FORMWORK_COLOR', N'design', N'IA diseño · Modos', N'Modo encofrado colores', N'Color formwork mode',
         N'Encofra todo el diseño y pinta cada panel según su ancho. «encofra» a secas sigue siendo el encofrado normal.',
         N'ai:mode formworkColor|bridge.setMode(''formworkColor'')', N'mode',
         N'[{"name": "modo", "required": true, "help": "«encofrado colores», «modo colores», «cambia a encofrado colores» (pide confirmación)"}]',
         0, 1, 1, 145);

    INSERT INTO dbo.TSql_AiCommand
    (
        TextCode, TextContext, TextGroup, TextNameEs, TextNameEn, TextDescription, TextHandler, TextAiCommandId, TextParamsJson,
        Is_Destructive, Is_Confirm, Is_AiImplemented, NumberOrder,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT S.TextCode, S.TextContext, S.TextGroup, S.TextNameEs, S.TextNameEn, S.TextDescription, S.TextHandler, S.TextAiCommandId, S.TextParamsJson,
           S.Is_Destructive, S.Is_Confirm, S.Is_AiImplemented, S.NumberOrder,
           S.TextNameEs, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Cmd S
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommand T WHERE T.TextCode = S.TextCode AND T.Is_Delete = 0);

    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberOrder)
    VALUES
        (N'MODE_FORMWORK_COLOR', N'es', N'colores', N'colores', N'keyword', N'formworkColor', 10),
        (N'MODE_FORMWORK_COLOR', N'es', N'color', N'color', N'keyword', N'formworkColor', 20),
        (N'MODE_FORMWORK_COLOR', N'es', N'coloreado', N'coloreado', N'keyword', N'formworkColor', 30),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofrado colores', N'encofrado colores', N'keyword', N'formworkColor', 40),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofrado de colores', N'encofrado de colores', N'keyword', N'formworkColor', 50),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofrar en colores', N'encofrar en colores', N'keyword', N'formworkColor', 60),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofra en colores', N'encofra en colores', N'keyword', N'formworkColor', 70),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofrar colores', N'encofrar colores', N'keyword', N'formworkColor', 80),
        (N'MODE_FORMWORK_COLOR', N'es', N'modo colores', N'modo colores', N'keyword', N'formworkColor', 90),
        (N'MODE_FORMWORK_COLOR', N'es', N'modo encofrado colores', N'modo encofrado colores', N'keyword', N'formworkColor', 100),
        (N'MODE_FORMWORK_COLOR', N'es', N'modo encofrar colores', N'modo encofrar colores', N'keyword', N'formworkColor', 110),
        (N'MODE_FORMWORK_COLOR', N'es', N'pasa a colores', N'pasa a colores', N'keyword', N'formworkColor', 120),
        (N'MODE_FORMWORK_COLOR', N'es', N'cambia a encofrado colores', N'cambia a encofrado colores', N'keyword', N'formworkColor', 130),
        (N'MODE_FORMWORK_COLOR', N'es', N'cambiar de modelo a encofrado colores', N'cambiar de modelo a encofrado colores', N'keyword', N'formworkColor', 140),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofrado por colores', N'encofrado por colores', N'keyword', N'formworkColor', 150),
        (N'MODE_FORMWORK_COLOR', N'es', N'paneles de colores', N'paneles de colores', N'keyword', N'formworkColor', 160),
        (N'MODE_FORMWORK_COLOR', N'es', N'pon los paneles de colores', N'pon los paneles de colores', N'keyword', N'formworkColor', 170),
        (N'MODE_FORMWORK_COLOR', N'es', N'mete el encofrado de colores', N'mete el encofrado de colores', N'keyword', N'formworkColor', 180),
        (N'MODE_FORMWORK_COLOR', N'es', N'encofra con colores', N'encofra con colores', N'keyword', N'formworkColor', 190),
        (N'MODE_FORMWORK_COLOR', N'en', N'colors', N'colors', N'keyword', N'formworkColor', 200),
        (N'MODE_FORMWORK_COLOR', N'en', N'coloured', N'coloured', N'keyword', N'formworkColor', 210),
        (N'MODE_FORMWORK_COLOR', N'en', N'colored', N'colored', N'keyword', N'formworkColor', 220),
        (N'MODE_FORMWORK_COLOR', N'en', N'formwork colors', N'formwork colors', N'keyword', N'formworkColor', 230),
        (N'MODE_FORMWORK_COLOR', N'en', N'color formwork', N'color formwork', N'keyword', N'formworkColor', 240),
        (N'MODE_FORMWORK_COLOR', N'en', N'coloured formwork', N'coloured formwork', N'keyword', N'formworkColor', 250),
        (N'MODE_FORMWORK_COLOR', N'en', N'colored formwork', N'colored formwork', N'keyword', N'formworkColor', 260),
        (N'MODE_FORMWORK_COLOR', N'en', N'formwork in color', N'formwork in color', N'keyword', N'formworkColor', 270),
        (N'MODE_FORMWORK_COLOR', N'en', N'color mode', N'color mode', N'keyword', N'formworkColor', 280),
        (N'MODE_FORMWORK_COLOR', N'en', N'switch to color formwork', N'switch to color formwork', N'keyword', N'formworkColor', 290);

    SELECT S.TextCode, S.TextLanguageCode, S.TextPhraseNorm, OC.TextCode AS YaEnComando
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
      JOIN dbo.TSql_AiCommandPhrase O ON O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                                     AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0
                                     AND O.TextKind IN (N'keyword', N'example') AND O.LinkAiCommand <> C.IdObject
      JOIN dbo.TSql_AiCommand OC ON OC.IdObject = O.LinkAiCommand
     WHERE S.TextKind IN (N'keyword', N'example');

    INSERT INTO dbo.TSql_AiCommandPhrase
    (
        LinkAiCommand, TextContext, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberWeight, NumberOrder, LinkEnvironment,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT C.IdObject, C.TextContext, S.TextLanguageCode, S.TextPhrase, S.TextPhraseNorm, S.TextKind, S.TextConcept, NULL, S.NumberOrder, NULL,
           S.TextPhrase, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommandPhrase T
                        WHERE T.LinkAiCommand = C.IdObject AND T.TextLanguageCode = S.TextLanguageCode
                          AND T.TextPhraseNorm = S.TextPhraseNorm AND T.Is_Delete = 0)
       AND (S.TextKind NOT IN (N'keyword', N'example') OR NOT EXISTS (
               SELECT 1 FROM dbo.TSql_AiCommandPhrase O
                WHERE O.TextContext = C.TextContext AND O.TextLanguageCode = S.TextLanguageCode
                  AND O.TextPhraseNorm = S.TextPhraseNorm AND O.Is_Delete = 0 AND O.TextKind IN (N'keyword', N'example')));

    DECLARE @Rep TABLE
    (
        EnvCode NVARCHAR(40) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextMessageKey NVARCHAR(100) NOT NULL,
        TextTemplate NVARCHAR(2000) NOT NULL
    );
    INSERT INTO @Rep (EnvCode, TextLanguageCode, TextMessageKey, TextTemplate)
    VALUES
        (N'EXECUTIVE', N'es', N'design.confirmFormworkColor', N'Encofrado colores calcula el encofrado de todo el diseño ({0} muros) y pinta cada panel según su ancho. ¿Lo hago?'),
        (N'EXECUTIVE', N'es', N'design.formworkColorAlready', N'Ya está en encofrado colores. ¿Lo vuelvo a calcular para todo el diseño?'),
        (N'EXECUTIVE', N'es', N'design.formworkColorRunning', N'Encofrando en colores…'),
        (N'EXECUTIVE', N'es', N'design.formworkColorDone', N'Encofrado colores terminado: {0} piezas. Cada panel lleva el color de su ancho.'),
        (N'EXECUTIVE', N'es', N'design.formworkColorFail', N'No se ha podido encofrar en colores: {0}'),
        (N'EXECUTIVE', N'es', N'design.formworkColorCancelled', N'No se ha encofrado en colores (no se eligió la fase o se canceló).'),
        (N'EXECUTIVE', N'es', N'design.formworkColorNo', N'De acuerdo, no se encofra en colores.'),
        (N'COLLOQUIAL', N'es', N'design.confirmFormworkColor', N'Encofrado colores calcula todo el diseño ({0} muros) y pinta cada panel según su ancho. ¿Lo hago?'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorAlready', N'Ya estás en encofrado colores. ¿Lo vuelvo a calcular?'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorRunning', N'Encofrando en colores…'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorDone', N'¡Listo! Encofrado colores: {0} piezas. Cada panel con el color de su ancho.'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorFail', N'No he podido encofrar en colores: {0}'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorCancelled', N'No se ha encofrado en colores (no se eligió la fase o se canceló).'),
        (N'COLLOQUIAL', N'es', N'design.formworkColorNo', N'Vale, no encofro en colores.'),
        (N'STREET', N'es', N'design.confirmFormworkColor', N'Encofrado colores monta todo el diseño ({0} muros) y pinta cada panel según su ancho. ¿Le damos?'),
        (N'STREET', N'es', N'design.formworkColorAlready', N'Ya estás en encofrado colores, colega. ¿Lo vuelvo a calcular?'),
        (N'STREET', N'es', N'design.formworkColorRunning', N'Encofrando en colores…'),
        (N'STREET', N'es', N'design.formworkColorDone', N'¡Encofrado colores montado! {0} piezas. Cada panel con el color de su ancho.'),
        (N'STREET', N'es', N'design.formworkColorFail', N'No me deja encofrar en colores: {0}'),
        (N'STREET', N'es', N'design.formworkColorCancelled', N'Cancelado: no se eligió la fase.'),
        (N'STREET', N'es', N'design.formworkColorNo', N'Vale, no encofro en colores.'),
        (N'TECHNICAL', N'es', N'design.confirmFormworkColor', N'Encofrado colores: todo el diseño ({0} muros), color por ancho. ¿Calcular?'),
        (N'TECHNICAL', N'es', N'design.formworkColorAlready', N'Encofrado colores ya activo. ¿Recalcular?'),
        (N'TECHNICAL', N'es', N'design.formworkColorRunning', N'Calculando encofrado colores…'),
        (N'TECHNICAL', N'es', N'design.formworkColorDone', N'Encofrado colores: {0} piezas. Color = ancho.'),
        (N'TECHNICAL', N'es', N'design.formworkColorFail', N'Error encofrado colores: {0}'),
        (N'TECHNICAL', N'es', N'design.formworkColorCancelled', N'Encofrado colores cancelado.'),
        (N'TECHNICAL', N'es', N'design.formworkColorNo', N'Encofrado colores no ejecutado.'),
        (N'EXECUTIVE', N'en', N'design.confirmFormworkColor', N'Color formwork calculates the whole design ({0} walls) and paints each panel by its width. Shall I go ahead?'),
        (N'EXECUTIVE', N'en', N'design.formworkColorAlready', N'You are already in color formwork. Shall I calculate it again?'),
        (N'EXECUTIVE', N'en', N'design.formworkColorRunning', N'Calculating the color formwork…'),
        (N'EXECUTIVE', N'en', N'design.formworkColorDone', N'Color formwork finished: {0} pieces. Each panel uses the color of its width.'),
        (N'EXECUTIVE', N'en', N'design.formworkColorFail', N'The color formwork could not be calculated: {0}'),
        (N'EXECUTIVE', N'en', N'design.formworkColorCancelled', N'No color formwork was calculated (no phase was chosen or it was cancelled).'),
        (N'EXECUTIVE', N'en', N'design.formworkColorNo', N'All right, no color formwork.'),
        (N'COLLOQUIAL', N'en', N'design.confirmFormworkColor', N'Color formwork calculates the whole design ({0} walls) and paints each panel by its width. Shall I go ahead?'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorAlready', N'You are already in color formwork. Shall I calculate it again?'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorRunning', N'Calculating the color formwork…'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorDone', N'Done! Color formwork: {0} pieces. Each panel has the color of its width.'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorFail', N'I could not run the color formwork: {0}'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorCancelled', N'No color formwork (no phase was chosen or it was cancelled).'),
        (N'COLLOQUIAL', N'en', N'design.formworkColorNo', N'OK, no color formwork.'),
        (N'STREET', N'en', N'design.confirmFormworkColor', N'Color formwork builds the whole design ({0} walls) and paints each panel by its width. Shall we?'),
        (N'STREET', N'en', N'design.formworkColorAlready', N'You are already in color formwork, mate. Shall I calculate it again?'),
        (N'STREET', N'en', N'design.formworkColorRunning', N'Running the color formwork…'),
        (N'STREET', N'en', N'design.formworkColorDone', N'Color formwork is up: {0} pieces. Each panel has the color of its width.'),
        (N'STREET', N'en', N'design.formworkColorFail', N'Could not run the color formwork: {0}'),
        (N'STREET', N'en', N'design.formworkColorCancelled', N'Cancelled: no phase was chosen.'),
        (N'STREET', N'en', N'design.formworkColorNo', N'OK, no color formwork.'),
        (N'TECHNICAL', N'en', N'design.confirmFormworkColor', N'Color formwork: whole design ({0} walls), color by width. Calculate?'),
        (N'TECHNICAL', N'en', N'design.formworkColorAlready', N'Color formwork already active. Recalculate?'),
        (N'TECHNICAL', N'en', N'design.formworkColorRunning', N'Calculating color formwork…'),
        (N'TECHNICAL', N'en', N'design.formworkColorDone', N'Color formwork: {0} pieces. Color = width.'),
        (N'TECHNICAL', N'en', N'design.formworkColorFail', N'Color formwork error: {0}'),
        (N'TECHNICAL', N'en', N'design.formworkColorCancelled', N'Color formwork cancelled.'),
        (N'TECHNICAL', N'en', N'design.formworkColorNo', N'Color formwork not run.');

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
                          AND T.TextMessageKey = S.TextMessageKey AND T.Is_Delete = 0);

    COMMIT TRANSACTION;
    PRINT N'OK — modo encofrado colores: comando, frases y plantillas.';
END
GO
