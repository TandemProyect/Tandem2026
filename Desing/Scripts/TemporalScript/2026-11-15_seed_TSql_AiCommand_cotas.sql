/* =============================================================================
   Órdenes de cotas por voz de la IA del diseño (pt114u, 15/11/2026). Idempotente: solo inserta lo que falta
   y solo actualiza TOOL_WALL_DIM si sigue como «pendiente de IA».
   · dbo.TSql_AiCommand: WALL_IDS («muéstrame los id de los muros») y WALL_DIMS («acótame el muro 7»);
     TOOL_WALL_DIM (botón Cotas) pasa a Is_AiImplemented = 1 → wall.dims y sus frases sin concepto → «dims».
   · dbo.TSql_AiCommandPhrase: 127 frases ES/EN (WALL_IDS, WALL_DIMS y faltas / frases nuevas de WALL_EDIT_DIMS:
     «esperoser», «atura», «cámbiame la altura», «el espesor es»…).
   · dbo.TSql_AiEnvironmentReply: 48 plantillas (design.idsShown, design.dimsShown… de los 4 entornos).
   «muro N» = el ID que enseñan las Cotas (wallId del eje o su id), no el IdObject de TSql_DesignWall.
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
    PRINT N'Aviso: AspNetUsers vacío — semilla de cotas por voz omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiCommand', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiCommandPhrase', N'U') IS NULL
     OR OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL OR COL_LENGTH(N'dbo.TSql_AiCommandPhrase', N'LinkEnvironment') IS NULL
    PRINT N'Aviso: faltan los create / seed del 14/11/2026 (catálogo de la IA).';
ELSE
BEGIN
    BEGIN TRANSACTION;

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
        (N'WALL_IDS', N'design', N'IA diseño · Muros', N'Ver los ID de los muros', N'Show wall IDs', N'Cotas con solo el ID de cada muro (el número de «el muro 7»); «oculta los id» lo quita', N'ai:wall.ids', N'wall.ids', N'[{"name": "mostrar / ocultar", "required": false, "help": "«muéstrame los id», «oculta los id»"}]', 0, 0, 1, 92),
        (N'WALL_DIMS', N'design', N'IA diseño · Muros', N'Acotar un muro', N'Dimension a wall', N'Cotas de un muro (queda como muro actual: «longitud 4,5», «espesor 0,45»); sin muro, todo el diseño; «quita las cotas»', N'ai:wall.dims', N'wall.dims', N'[{"name": "muro", "required": false, "help": "«el muro 7», el seleccionado o «acótalo»; sin muro, todo el diseño"}]', 0, 0, 1, 94);

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

    /* Botón Cotas: ya se puede pedir a la IA («acota», «pon cotas» → wall.dims). */
    UPDATE dbo.TSql_AiCommand
       SET Is_AiImplemented = 1, TextAiCommandId = N'wall.dims', TextHandler = N'ai:wall.dims|click:#ma-stl-tool-wall-dim',
           LinModifiedBy = @User, AddLastDateChange = GETDATE(), Ntimeschanged = ISNULL(Ntimeschanged, 0) + 1
     WHERE TextCode = N'TOOL_WALL_DIM' AND Is_Delete = 0 AND Is_AiImplemented = 0;
    UPDATE P
       SET P.TextConcept = N'dims', P.LinModifiedBy = @User, P.AddLastDateChange = GETDATE(), P.Ntimeschanged = ISNULL(P.Ntimeschanged, 0) + 1
      FROM dbo.TSql_AiCommandPhrase P
      JOIN dbo.TSql_AiCommand C ON C.IdObject = P.LinkAiCommand
     WHERE C.TextCode = N'TOOL_WALL_DIM' AND P.Is_Delete = 0 AND P.TextConcept IS NULL AND P.TextKind = N'keyword';

    /* ---- Frases ---- */
    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, NumberOrder)
    VALUES
        (N'WALL_IDS', N'es', N'id', N'id', N'keyword', N'ids', 10),
        (N'WALL_IDS', N'es', N'ids', N'ids', N'keyword', N'ids', 20),
        (N'WALL_IDS', N'es', N'identifica', N'identifica', N'keyword', N'ids', 30),
        (N'WALL_IDS', N'es', N'identificar', N'identificar', N'keyword', N'ids', 40),
        (N'WALL_IDS', N'es', N'identifícalos', N'identificalos', N'keyword', N'ids', 50),
        (N'WALL_IDS', N'es', N'identifícame', N'identificame', N'keyword', N'ids', 60),
        (N'WALL_IDS', N'es', N'identificador', N'identificador', N'keyword', N'ids', 70),
        (N'WALL_IDS', N'es', N'identificadores', N'identificadores', N'keyword', N'ids', 80),
        (N'WALL_IDS', N'es', N'etiqueta', N'etiqueta', N'keyword', N'ids', 90),
        (N'WALL_IDS', N'es', N'etiquetas', N'etiquetas', N'keyword', N'ids', 100),
        (N'WALL_IDS', N'es', N'etiquétalos', N'etiquetalos', N'keyword', N'ids', 110),
        (N'WALL_IDS', N'es', N'rotula', N'rotula', N'keyword', N'ids', 120),
        (N'WALL_IDS', N'es', N'rotúlalos', N'rotulalos', N'keyword', N'ids', 130),
        (N'WALL_IDS', N'en', N'identify', N'identify', N'keyword', N'ids', 140),
        (N'WALL_IDS', N'en', N'label', N'label', N'keyword', N'ids', 150),
        (N'WALL_IDS', N'en', N'labels', N'labels', N'keyword', N'ids', 160),
        (N'WALL_IDS', N'es', N'muéstrame los id', N'muestrame los id', N'keyword', N'ids', 170),
        (N'WALL_IDS', N'es', N'id de los muros', N'id de los muros', N'keyword', N'ids', 180),
        (N'WALL_IDS', N'es', N'id de muros', N'id de muros', N'keyword', N'ids', 190),
        (N'WALL_IDS', N'es', N'ids de los muros', N'ids de los muros', N'keyword', N'ids', 200),
        (N'WALL_IDS', N'es', N'identifica los muros', N'identifica los muros', N'keyword', N'ids', 210),
        (N'WALL_IDS', N'es', N'numera los muros', N'numera los muros', N'keyword', N'ids', 220),
        (N'WALL_IDS', N'es', N'números de los muros', N'numeros de los muros', N'keyword', N'ids', 230),
        (N'WALL_IDS', N'es', N'muéstrame los muros', N'muestrame los muros', N'keyword', N'ids', 240),
        (N'WALL_IDS', N'es', N'enséñame los muros', N'ensename los muros', N'keyword', N'ids', 250),
        (N'WALL_IDS', N'es', N'pon los números', N'pon los numeros', N'keyword', N'ids', 260),
        (N'WALL_IDS', N'es', N'qué número tiene cada muro', N'que numero tiene cada muro', N'keyword', N'ids', 270),
        (N'WALL_IDS', N'es', N'qué muro es cada uno', N'que muro es cada uno', N'keyword', N'ids', 280),
        (N'WALL_IDS', N'en', N'show wall ids', N'show wall ids', N'keyword', N'ids', 290),
        (N'WALL_IDS', N'en', N'wall ids', N'wall ids', N'keyword', N'ids', 300),
        (N'WALL_IDS', N'en', N'number the walls', N'number the walls', N'keyword', N'ids', 310),
        (N'WALL_IDS', N'en', N'label the walls', N'label the walls', N'keyword', N'ids', 320),
        (N'WALL_IDS', N'en', N'wall numbers', N'wall numbers', N'keyword', N'ids', 330),
        (N'WALL_IDS', N'es', N'oculta', N'oculta', N'keyword', N'hide', 340),
        (N'WALL_IDS', N'es', N'ocultar', N'ocultar', N'keyword', N'hide', 350),
        (N'WALL_IDS', N'es', N'ocúltalos', N'ocultalos', N'keyword', N'hide', 360),
        (N'WALL_IDS', N'es', N'ocúltalas', N'ocultalas', N'keyword', N'hide', 370),
        (N'WALL_IDS', N'es', N'esconde', N'esconde', N'keyword', N'hide', 380),
        (N'WALL_IDS', N'es', N'escóndelos', N'escondelos', N'keyword', N'hide', 390),
        (N'WALL_IDS', N'es', N'apaga', N'apaga', N'keyword', N'hide', 400),
        (N'WALL_IDS', N'es', N'desactiva', N'desactiva', N'keyword', N'hide', 410),
        (N'WALL_IDS', N'es', N'oculta los id', N'oculta los id', N'keyword', N'hide', 420),
        (N'WALL_IDS', N'es', N'quita los id', N'quita los id', N'keyword', N'hide', 430),
        (N'WALL_IDS', N'es', N'quita los números', N'quita los numeros', N'keyword', N'hide', 440),
        (N'WALL_IDS', N'es', N'oculta los números', N'oculta los numeros', N'keyword', N'hide', 450),
        (N'WALL_IDS', N'en', N'hide', N'hide', N'keyword', N'hide', 460),
        (N'WALL_IDS', N'en', N'hide the ids', N'hide the ids', N'keyword', N'hide', 470),
        (N'WALL_IDS', N'en', N'hide wall numbers', N'hide wall numbers', N'keyword', N'hide', 480),
        (N'WALL_IDS', N'es', N'muéstrame los id de los muros', N'muestrame los id de los muros', N'example', NULL, 490),
        (N'WALL_IDS', N'en', N'show the wall ids', N'show the wall ids', N'example', NULL, 500),
        (N'WALL_DIMS', N'es', N'acótame', N'acotame', N'keyword', N'dims', 10),
        (N'WALL_DIMS', N'es', N'acótalo', N'acotalo', N'keyword', N'dims', 20),
        (N'WALL_DIMS', N'es', N'acótamelo', N'acotamelo', N'keyword', N'dims', 30),
        (N'WALL_DIMS', N'es', N'acótalos', N'acotalos', N'keyword', N'dims', 40),
        (N'WALL_DIMS', N'es', N'acotación', N'acotacion', N'keyword', N'dims', 50),
        (N'WALL_DIMS', N'es', N'acotaciones', N'acotaciones', N'keyword', N'dims', 60),
        (N'WALL_DIMS', N'es', N'cota', N'cota', N'keyword', N'dims', 70),
        (N'WALL_DIMS', N'es', N'cotas', N'cotas', N'keyword', N'dims', 80),
        (N'WALL_DIMS', N'es', N'mide', N'mide', N'keyword', N'dims', 90),
        (N'WALL_DIMS', N'es', N'medir', N'medir', N'keyword', N'dims', 100),
        (N'WALL_DIMS', N'es', N'mídeme', N'mideme', N'keyword', N'dims', 110),
        (N'WALL_DIMS', N'es', N'mídelo', N'midelo', N'keyword', N'dims', 120),
        (N'WALL_DIMS', N'es', N'dimensiona', N'dimensiona', N'keyword', N'dims', 130),
        (N'WALL_DIMS', N'es', N'dimensionar', N'dimensionar', N'keyword', N'dims', 140),
        (N'WALL_DIMS', N'es', N'dimensiones', N'dimensiones', N'keyword', N'dims', 150),
        (N'WALL_DIMS', N'en', N'measure', N'measure', N'keyword', N'dims', 160),
        (N'WALL_DIMS', N'en', N'dimensions', N'dimensions', N'keyword', N'dims', 170),
        (N'WALL_DIMS', N'es', N'cota del muro', N'cota del muro', N'keyword', N'dims', 180),
        (N'WALL_DIMS', N'es', N'cotas del muro', N'cotas del muro', N'keyword', N'dims', 190),
        (N'WALL_DIMS', N'es', N'medidas del muro', N'medidas del muro', N'keyword', N'dims', 200),
        (N'WALL_DIMS', N'es', N'dime las medidas', N'dime las medidas', N'keyword', N'dims', 210),
        (N'WALL_DIMS', N'es', N'cuánto mide', N'cuanto mide', N'keyword', N'dims', 220),
        (N'WALL_DIMS', N'es', N'qué mide', N'que mide', N'keyword', N'dims', 230),
        (N'WALL_DIMS', N'es', N'muéstrame las cotas', N'muestrame las cotas', N'keyword', N'dims', 240),
        (N'WALL_DIMS', N'es', N'enséñame las cotas', N'ensename las cotas', N'keyword', N'dims', 250),
        (N'WALL_DIMS', N'es', N'acota el muro', N'acota el muro', N'keyword', N'dims', 260),
        (N'WALL_DIMS', N'es', N'acótame el muro', N'acotame el muro', N'keyword', N'dims', 270),
        (N'WALL_DIMS', N'en', N'dimension wall', N'dimension wall', N'keyword', N'dims', 280),
        (N'WALL_DIMS', N'en', N'measure wall', N'measure wall', N'keyword', N'dims', 290),
        (N'WALL_DIMS', N'en', N'dimensions of wall', N'dimensions of wall', N'keyword', N'dims', 300),
        (N'WALL_DIMS', N'en', N'show the dimensions', N'show the dimensions', N'keyword', N'dims', 310),
        (N'WALL_DIMS', N'es', N'quita las cotas', N'quita las cotas', N'keyword', N'hide', 320),
        (N'WALL_DIMS', N'es', N'oculta las cotas', N'oculta las cotas', N'keyword', N'hide', 330),
        (N'WALL_DIMS', N'es', N'borra las cotas', N'borra las cotas', N'keyword', N'hide', 340),
        (N'WALL_DIMS', N'es', N'fuera cotas', N'fuera cotas', N'keyword', N'hide', 350),
        (N'WALL_DIMS', N'en', N'hide the dimensions', N'hide the dimensions', N'keyword', N'hide', 360),
        (N'WALL_DIMS', N'en', N'remove the dimensions', N'remove the dimensions', N'keyword', N'hide', 370),
        (N'WALL_DIMS', N'es', N'acótame el muro 7', N'acotame el muro 7', N'example', NULL, 380),
        (N'WALL_DIMS', N'es', N'cota del muro 7', N'cota del muro 7', N'example', NULL, 390),
        (N'WALL_DIMS', N'es', N'mide el muro 7', N'mide el muro 7', N'example', NULL, 400),
        (N'WALL_DIMS', N'en', N'dimension wall 7', N'dimension wall 7', N'example', NULL, 410),
        (N'WALL_DIMS', N'en', N'measure wall 7', N'measure wall 7', N'example', NULL, 420),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame', N'cambiame', N'helper', N'change', 5010),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiamelo', N'cambiamelo', N'helper', N'change', 5020),
        (N'WALL_EDIT_DIMS', N'es', N'longuitud', N'longuitud', N'helper', N'length', 5030),
        (N'WALL_EDIT_DIMS', N'es', N'lonjitud', N'lonjitud', N'helper', N'length', 5040),
        (N'WALL_EDIT_DIMS', N'es', N'largura', N'largura', N'helper', N'length', 5050),
        (N'WALL_EDIT_DIMS', N'es', N'esperoser', N'esperoser', N'helper', N'thickness', 5060),
        (N'WALL_EDIT_DIMS', N'es', N'espesoro', N'espesoro', N'helper', N'thickness', 5070),
        (N'WALL_EDIT_DIMS', N'es', N'espersor', N'espersor', N'helper', N'thickness', 5080),
        (N'WALL_EDIT_DIMS', N'es', N'expesor', N'expesor', N'helper', N'thickness', 5090),
        (N'WALL_EDIT_DIMS', N'es', N'espresor', N'espresor', N'helper', N'thickness', 5100),
        (N'WALL_EDIT_DIMS', N'es', N'esperor', N'esperor', N'helper', N'thickness', 5110),
        (N'WALL_EDIT_DIMS', N'es', N'atura', N'atura', N'helper', N'height', 5120),
        (N'WALL_EDIT_DIMS', N'es', N'altrua', N'altrua', N'helper', N'height', 5130),
        (N'WALL_EDIT_DIMS', N'es', N'altur', N'altur', N'helper', N'height', 5140),
        (N'WALL_EDIT_DIMS', N'es', N'alturas', N'alturas', N'helper', N'height', 5150),
        (N'WALL_EDIT_DIMS', N'es', N'altuta', N'altuta', N'helper', N'height', 5160),
        (N'WALL_EDIT_DIMS', N'es', N'haltura', N'haltura', N'helper', N'height', 5170),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame la longitud', N'cambiame la longitud', N'keyword', N'change', 5180),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame el espesor', N'cambiame el espesor', N'keyword', N'change', 5190),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame la altura', N'cambiame la altura', N'keyword', N'change', 5200),
        (N'WALL_EDIT_DIMS', N'es', N'el espesor es', N'el espesor es', N'keyword', N'change', 5210),
        (N'WALL_EDIT_DIMS', N'es', N'la longitud es', N'la longitud es', N'keyword', N'change', 5220),
        (N'WALL_EDIT_DIMS', N'es', N'el largo es', N'el largo es', N'keyword', N'change', 5230),
        (N'WALL_EDIT_DIMS', N'es', N'la altura es', N'la altura es', N'keyword', N'change', 5240),
        (N'WALL_EDIT_DIMS', N'es', N'el alto es', N'el alto es', N'keyword', N'change', 5250),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame la longitud a 4,5', N'cambiame la longitud a 4 5', N'example', NULL, 5260),
        (N'WALL_EDIT_DIMS', N'es', N'el espesor es 0,45', N'el espesor es 0 45', N'example', NULL, 5270),
        (N'WALL_EDIT_DIMS', N'es', N'muro 7 longitud 4,5', N'muro 7 longitud 4 5', N'example', NULL, 5280),
        (N'WALL_EDIT_DIMS', N'es', N'cámbiame la altura a 3', N'cambiame la altura a 3', N'example', NULL, 5290),
        (N'WALL_EDIT_DIMS', N'es', N'altura 2,70', N'altura 2 70', N'example', NULL, 5300),
        (N'WALL_EDIT_DIMS', N'es', N'la altura es 3,00', N'la altura es 3 00', N'example', NULL, 5310),
        (N'WALL_EDIT_DIMS', N'es', N'muro 7 altura 3', N'muro 7 altura 3', N'example', NULL, 5320),
        (N'WALL_EDIT_DIMS', N'en', N'length 4.5', N'length 4 5', N'example', NULL, 5330),
        (N'WALL_EDIT_DIMS', N'en', N'thickness 0.45', N'thickness 0 45', N'example', NULL, 5340),
        (N'WALL_EDIT_DIMS', N'en', N'height 2.70', N'height 2 70', N'example', NULL, 5350);

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
        (N'EXECUTIVE', N'es', N'design.idsShown', N'Le muestro el número de cada muro en el plano ({0} muros: {1}). Para ver las medidas de uno, diga «acótame el muro {2}».'),
        (N'EXECUTIVE', N'es', N'design.idsAlready', N'Los números ya están en el plano ({0} muros: {1}). Diga «oculta los id» para quitarlos.'),
        (N'EXECUTIVE', N'es', N'design.idsHidden', N'Números de los muros retirados.'),
        (N'EXECUTIVE', N'es', N'design.idsNotShown', N'Los números de los muros no estaban visibles.'),
        (N'EXECUTIVE', N'es', N'design.dimsShown', N'Cotas del {0}: {1}. Indique qué desea cambiar, por ejemplo «longitud 4,5», «espesor 0,45» o «altura 3».'),
        (N'EXECUTIVE', N'es', N'design.dimsAll', N'Cotas de todo el diseño activadas, como con el botón Cotas. Para un solo muro, diga «acótame el muro {0}».'),
        (N'EXECUTIVE', N'es', N'design.dimsHidden', N'Cotas retiradas.'),
        (N'EXECUTIVE', N'es', N'design.dimsNone', N'No había cotas visibles.'),
        (N'EXECUTIVE', N'es', N'design.cotasFail', N'No ha sido posible mostrar las cotas en el visor.'),
        (N'COLLOQUIAL', N'es', N'design.idsShown', N'¡Ahí van! Cada muro con su número en el plano ({0} muros: {1}). Si quieres las medidas de uno, dime «acótame el muro {2}».'),
        (N'COLLOQUIAL', N'es', N'design.idsAlready', N'Ya tienes los números en el plano ({0} muros: {1}). Si te estorban, dime «oculta los id».'),
        (N'COLLOQUIAL', N'es', N'design.idsHidden', N'Listo, he quitado los números.'),
        (N'COLLOQUIAL', N'es', N'design.idsNotShown', N'Los números no estaban puestos.'),
        (N'COLLOQUIAL', N'es', N'design.dimsShown', N'Aquí tienes el {0} acotado: {1}. ¿Qué le cambio? Por ejemplo «longitud 4,5», «espesor 0,45» o «altura 3».'),
        (N'COLLOQUIAL', N'es', N'design.dimsAll', N'Te he puesto las cotas de todo el diseño, como con el botón Cotas. Si quieres solo uno, dime «acótame el muro {0}».'),
        (N'COLLOQUIAL', N'es', N'design.dimsHidden', N'Listo, cotas fuera.'),
        (N'COLLOQUIAL', N'es', N'design.dimsNone', N'No había cotas puestas.'),
        (N'COLLOQUIAL', N'es', N'design.cotasFail', N'Vaya, no he podido poner las cotas en el visor.'),
        (N'STREET', N'es', N'design.idsShown', N'¡Marchando, colega! Ahí tienes cada muro con su número ({0} muros: {1}). Si quieres las medidas de uno, dime «acótame el muro {2}».'),
        (N'STREET', N'es', N'design.idsAlready', N'Que ya están los números, tío ({0} muros: {1}). Si te molestan, dime «oculta los id».'),
        (N'STREET', N'es', N'design.idsHidden', N'Hecho, números fuera.'),
        (N'STREET', N'es', N'design.idsNotShown', N'No había números puestos, máquina.'),
        (N'STREET', N'es', N'design.dimsShown', N'¡Ahí lo llevas! El {0} acotado: {1}. ¿Qué le metemos? Dime «longitud 4,5», «espesor 0,45» o «altura 3».'),
        (N'STREET', N'es', N'design.dimsAll', N'Cotas de todo el diseño puestas, como con el botón Cotas. Si quieres solo uno, dime «acótame el muro {0}».'),
        (N'STREET', N'es', N'design.dimsHidden', N'Hecho, cotas fuera.'),
        (N'STREET', N'es', N'design.dimsNone', N'No había cotas puestas, colega.'),
        (N'STREET', N'es', N'design.cotasFail', N'Uf, no me deja poner las cotas en el visor.'),
        (N'TECHNICAL', N'es', N'design.idsShown', N'ID de muros visibles: {0} ({1}). Acotar: «acota el muro {2}».'),
        (N'TECHNICAL', N'es', N'design.idsAlready', N'ID ya visibles: {0} ({1}).'),
        (N'TECHNICAL', N'es', N'design.idsHidden', N'ID de muros ocultos.'),
        (N'TECHNICAL', N'es', N'design.idsNotShown', N'ID de muros no visibles.'),
        (N'TECHNICAL', N'es', N'design.dimsShown', N'{0}: {1}. Editar: «longitud 4,5» / «espesor 0,45» / «altura 3».'),
        (N'TECHNICAL', N'es', N'design.dimsAll', N'Cotas: todo el diseño. Un muro: «acota el muro {0}».'),
        (N'TECHNICAL', N'es', N'design.dimsHidden', N'Cotas ocultas.'),
        (N'TECHNICAL', N'es', N'design.dimsNone', N'Sin cotas visibles.'),
        (N'TECHNICAL', N'es', N'design.cotasFail', N'Error al mostrar las cotas.'),
        (N'EXECUTIVE', N'en', N'design.idsShown', N'The number of each wall is now shown on the plan ({0} walls: {1}). To see the sizes of one, say «dimension wall {2}».'),
        (N'EXECUTIVE', N'en', N'design.dimsShown', N'Dimensions of {0}: {1}. Tell me what to change, e.g. «length 4.5», «thickness 0.45» or «height 3».'),
        (N'EXECUTIVE', N'en', N'design.idsHidden', N'Wall numbers removed.'),
        (N'COLLOQUIAL', N'en', N'design.idsShown', N'There you go: every wall with its number ({0} walls: {1}). Want the sizes of one? Say «dimension wall {2}».'),
        (N'COLLOQUIAL', N'en', N'design.dimsShown', N'Here is {0} with its dimensions: {1}. What shall I change? E.g. «length 4.5», «thickness 0.45» or «height 3».'),
        (N'COLLOQUIAL', N'en', N'design.idsHidden', N'Done, numbers gone.'),
        (N'STREET', N'en', N'design.idsShown', N'Here you go, mate: every wall with its number ({0} walls: {1}). Say «dimension wall {2}» for the sizes of one.'),
        (N'STREET', N'en', N'design.dimsShown', N'Sorted, mate! {0} dimensioned: {1}. What do we change? «length 4.5», «thickness 0.45» or «height 3».'),
        (N'STREET', N'en', N'design.idsHidden', N'Done, mate, numbers off.'),
        (N'TECHNICAL', N'en', N'design.idsShown', N'Wall IDs shown: {0} ({1}). Dimension: «dimension wall {2}».'),
        (N'TECHNICAL', N'en', N'design.dimsShown', N'{0}: {1}. Edit: «length 4.5» / «thickness 0.45» / «height 3».'),
        (N'TECHNICAL', N'en', N'design.idsHidden', N'Wall IDs hidden.');

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
    PRINT N'OK — cotas por voz: 2 comandos (WALL_IDS, WALL_DIMS), TOOL_WALL_DIM con IA, 127 frases, 48 plantillas.';
END
GO
