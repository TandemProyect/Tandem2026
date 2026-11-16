/* =============================================================================
   Orden «ID de artículo» de la IA del diseño (pt116u, 15/11/2026). Idempotente: solo inserta lo que falta.
   · dbo.TSql_AiCommand: PANEL_IDS («muéstrame los id de los paneles» / «oculta los id de artículo») → panel.ids:
     activa o quita el tipo de cota «Id de Artículo» (cada cota de panel lleva su ID, «0,90 · 7.3»).
   · dbo.TSql_AiCommandPhrase: 38 frases ES/EN.
   · dbo.TSql_AiEnvironmentReply: 28 plantillas (design.articleIds*… de los 4 entornos).
   Sin huecos de IDENTITY entre develop y producción: si va a insertar filas nuevas, antes reajusta el contador
   de cada tabla al máximo IdObject existente (DBCC CHECKIDENT … RESEED), así ambas bases dan los mismos IdObject
   aunque una haya consumido valores en un ensayo con ROLLBACK.
   Requisitos: create / seed del 14/11/2026 (TSql_AiCommand, TSql_AiCommandPhrase, TSql_AiEnvironment*).
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
    PRINT N'Aviso: AspNetUsers vacío — semilla de ID de artículo omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
     OR OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL OR COL_LENGTH(N'dbo.TSql_AiCommandPhrase', N'LinkEnvironment') IS NULL
    PRINT N'Aviso: faltan los create / seed del 14/11/2026 (catálogo de la IA).';
ELSE
BEGIN
    BEGIN TRANSACTION;

    /* ---- Contadores IDENTITY = máximo IdObject (solo si faltan filas): mismos IdObject en develop y producción ---- */
    IF NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommand WHERE TextCode = N'PANEL_IDS' AND Is_Delete = 0)
    BEGIN
        DECLARE @MaxCmd INT = (SELECT ISNULL(MAX(IdObject), 0) FROM dbo.TSql_AiCommand);
        DECLARE @MaxPhr INT = (SELECT ISNULL(MAX(IdObject), 0) FROM dbo.TSql_AiCommandPhrase);
        DECLARE @MaxRep INT = (SELECT ISNULL(MAX(IdObject), 0) FROM dbo.TSql_AiEnvironmentReply);
        DBCC CHECKIDENT (N'dbo.TSql_AiCommand', RESEED, @MaxCmd) WITH NO_INFOMSGS;
        DBCC CHECKIDENT (N'dbo.TSql_AiCommandPhrase', RESEED, @MaxPhr) WITH NO_INFOMSGS;
        DBCC CHECKIDENT (N'dbo.TSql_AiEnvironmentReply', RESEED, @MaxRep) WITH NO_INFOMSGS;
    END

    /* ---- Comandos ---- */
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
        (N'PANEL_IDS', N'design', N'IA diseño · Cotas', N'Ver los ID de los paneles', N'Show panel IDs', N'Tipo de cota «Id de Artículo»: cada cota de panel lleva su ID («0,90 · 7.3» = muro 7, 3.ª columna); «oculta los id de los paneles» lo quita', N'ai:panel.ids', N'panel.ids', N'[{"name": "mostrar / ocultar", "required": false, "help": "«muéstrame los id de los paneles», «oculta los id de artículo»"}]', 0, 0, 1, 96);

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


    /* ---- Frases ---- */
    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberOrder)
    VALUES
        (N'PANEL_IDS', N'es', N'artículo', N'articulo', N'keyword', N'article', 10),
        (N'PANEL_IDS', N'es', N'artículos', N'articulos', N'keyword', N'article', 20),
        (N'PANEL_IDS', N'es', N'panel', N'panel', N'keyword', N'article', 30),
        (N'PANEL_IDS', N'es', N'paneles', N'paneles', N'keyword', N'article', 40),
        (N'PANEL_IDS', N'es', N'pieza', N'pieza', N'keyword', N'article', 50),
        (N'PANEL_IDS', N'es', N'piezas', N'piezas', N'keyword', N'article', 60),
        (N'PANEL_IDS', N'en', N'article', N'article', N'keyword', N'article', 70),
        (N'PANEL_IDS', N'en', N'articles', N'articles', N'keyword', N'article', 80),
        (N'PANEL_IDS', N'en', N'panels', N'panels', N'keyword', N'article', 90),
        (N'PANEL_IDS', N'en', N'pieces', N'pieces', N'keyword', N'article', 100),
        (N'PANEL_IDS', N'es', N'id de artículo', N'id de articulo', N'keyword', N'ids', 110),
        (N'PANEL_IDS', N'es', N'ids de artículo', N'ids de articulo', N'keyword', N'ids', 120),
        (N'PANEL_IDS', N'es', N'id de los artículos', N'id de los articulos', N'keyword', N'ids', 130),
        (N'PANEL_IDS', N'es', N'id del artículo', N'id del articulo', N'keyword', N'ids', 140),
        (N'PANEL_IDS', N'es', N'id de los paneles', N'id de los paneles', N'keyword', N'ids', 150),
        (N'PANEL_IDS', N'es', N'ids de los paneles', N'ids de los paneles', N'keyword', N'ids', 160),
        (N'PANEL_IDS', N'es', N'id de paneles', N'id de paneles', N'keyword', N'ids', 170),
        (N'PANEL_IDS', N'es', N'id de cada panel', N'id de cada panel', N'keyword', N'ids', 180),
        (N'PANEL_IDS', N'es', N'muéstrame los id de los paneles', N'muestrame los id de los paneles', N'keyword', N'ids', 190),
        (N'PANEL_IDS', N'es', N'enséñame los id de los paneles', N'ensename los id de los paneles', N'keyword', N'ids', 200),
        (N'PANEL_IDS', N'es', N'pon los id de los paneles', N'pon los id de los paneles', N'keyword', N'ids', 210),
        (N'PANEL_IDS', N'es', N'identifica los paneles', N'identifica los paneles', N'keyword', N'ids', 220),
        (N'PANEL_IDS', N'es', N'numera los paneles', N'numera los paneles', N'keyword', N'ids', 230),
        (N'PANEL_IDS', N'es', N'id de las piezas', N'id de las piezas', N'keyword', N'ids', 240),
        (N'PANEL_IDS', N'es', N'identifica las piezas', N'identifica las piezas', N'keyword', N'ids', 250),
        (N'PANEL_IDS', N'en', N'article ids', N'article ids', N'keyword', N'ids', 260),
        (N'PANEL_IDS', N'en', N'article id', N'article id', N'keyword', N'ids', 270),
        (N'PANEL_IDS', N'en', N'panel ids', N'panel ids', N'keyword', N'ids', 280),
        (N'PANEL_IDS', N'en', N'show the panel ids', N'show the panel ids', N'keyword', N'ids', 290),
        (N'PANEL_IDS', N'en', N'number the panels', N'number the panels', N'keyword', N'ids', 300),
        (N'PANEL_IDS', N'en', N'label the panels', N'label the panels', N'keyword', N'ids', 310),
        (N'PANEL_IDS', N'es', N'oculta los id de artículo', N'oculta los id de articulo', N'keyword', N'hide', 320),
        (N'PANEL_IDS', N'es', N'oculta los id de los paneles', N'oculta los id de los paneles', N'keyword', N'hide', 330),
        (N'PANEL_IDS', N'es', N'quita los id de los paneles', N'quita los id de los paneles', N'keyword', N'hide', 340),
        (N'PANEL_IDS', N'es', N'quita los id de artículo', N'quita los id de articulo', N'keyword', N'hide', 350),
        (N'PANEL_IDS', N'es', N'oculta los id de las piezas', N'oculta los id de las piezas', N'keyword', N'hide', 360),
        (N'PANEL_IDS', N'en', N'hide the panel ids', N'hide the panel ids', N'keyword', N'hide', 370),
        (N'PANEL_IDS', N'en', N'hide the article ids', N'hide the article ids', N'keyword', N'hide', 380);

    /* Frases keyword/example que ya activan otro comando del mismo contexto e idioma: se omiten y se listan. */
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

    /* ---- Plantillas de respuesta ---- */
    DECLARE @Rep TABLE (EnvCode NVARCHAR(40) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextMessageKey NVARCHAR(100) NOT NULL,
                        TextTemplate NVARCHAR(2000) NOT NULL);
    INSERT INTO @Rep (EnvCode, TextLanguageCode, TextMessageKey, TextTemplate)
    VALUES
        (N'EXECUTIVE', N'es', N'design.articleIdsShown', N'Le muestro el ID de cada panel junto a su cota ({0} paneles en {1} muros: {2}). Podrá referirse a uno, por ejemplo «el panel {3}».'),
        (N'EXECUTIVE', N'es', N'design.articleIdsAlready', N'Los ID de los paneles ya están junto a sus cotas ({0} paneles: {1}). Diga «oculta los id de los paneles» para quitarlos.'),
        (N'EXECUTIVE', N'es', N'design.articleIdsHidden', N'ID de los paneles retirados.'),
        (N'EXECUTIVE', N'es', N'design.articleIdsNotShown', N'Los ID de los paneles no estaban visibles.'),
        (N'EXECUTIVE', N'es', N'design.articleIdsNoFormwork', N'Los ID de artículo van en las cotas de los paneles y aún no hay encofrado. Diga «encofra» y se los muestro.'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsShown', N'¡Ahí van! Cada panel con su ID junto a la cota ({0} paneles en {1} muros: {2}). Luego me puedes decir, por ejemplo, «el panel {3}».'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsAlready', N'Ya tienes los ID junto a las cotas ({0} paneles: {1}). Si te estorban, dime «oculta los id de los paneles».'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsHidden', N'Listo, he quitado los ID de los paneles.'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsNotShown', N'Los ID de los paneles no estaban puestos.'),
        (N'COLLOQUIAL', N'es', N'design.articleIdsNoFormwork', N'Los ID van en las cotas de los paneles y todavía no hay encofrado. Dime «encofra» y te los pongo.'),
        (N'STREET', N'es', N'design.articleIdsShown', N'¡Marchando! Cada panel con su ID al lado de la cota ({0} paneles en {1} muros: {2}). Luego dime, por ejemplo, «el panel {3}».'),
        (N'STREET', N'es', N'design.articleIdsAlready', N'Que ya están los ID, colega ({0} paneles: {1}). Si te molestan, dime «oculta los id de los paneles».'),
        (N'STREET', N'es', N'design.articleIdsHidden', N'Hecho, ID de los paneles fuera.'),
        (N'STREET', N'es', N'design.articleIdsNotShown', N'No había ID de paneles puestos, máquina.'),
        (N'STREET', N'es', N'design.articleIdsNoFormwork', N'Los ID van en las cotas de los paneles y aún no hay encofrado, tío. Dime «encofra» y te los pongo.'),
        (N'TECHNICAL', N'es', N'design.articleIdsShown', N'ID de artículo visibles: {0} paneles, {1} muros ({2}). Referencia: «panel {3}».'),
        (N'TECHNICAL', N'es', N'design.articleIdsAlready', N'ID de artículo ya visibles: {0} ({1}).'),
        (N'TECHNICAL', N'es', N'design.articleIdsHidden', N'ID de artículo ocultos.'),
        (N'TECHNICAL', N'es', N'design.articleIdsNotShown', N'ID de artículo no visibles.'),
        (N'TECHNICAL', N'es', N'design.articleIdsNoFormwork', N'Sin encofrado: no hay cotas de panel. Ejecutar «encofra».'),
        (N'EXECUTIVE', N'en', N'design.articleIdsShown', N'Each panel now shows its ID next to its dimension ({0} panels on {1} walls: {2}). You may refer to one, e.g. «panel {3}».'),
        (N'EXECUTIVE', N'en', N'design.articleIdsHidden', N'Panel IDs removed.'),
        (N'COLLOQUIAL', N'en', N'design.articleIdsShown', N'There you go: every panel with its ID next to the dimension ({0} panels on {1} walls: {2}). Then just say, e.g., «panel {3}».'),
        (N'COLLOQUIAL', N'en', N'design.articleIdsHidden', N'Done, panel IDs gone.'),
        (N'STREET', N'en', N'design.articleIdsShown', N'Here you go, mate: every panel with its ID ({0} panels on {1} walls: {2}). Say «panel {3}» to pick one.'),
        (N'STREET', N'en', N'design.articleIdsHidden', N'Done, mate, panel IDs off.'),
        (N'TECHNICAL', N'en', N'design.articleIdsShown', N'Article IDs shown: {0} panels, {1} walls ({2}). Reference: «panel {3}».'),
        (N'TECHNICAL', N'en', N'design.articleIdsHidden', N'Article IDs hidden.');

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
    PRINT N'OK — ID de artículo: comando PANEL_IDS, 38 frases, 28 plantillas.';
END
GO
