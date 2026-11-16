/* =============================================================================
   Semilla de entornos de conversación de la IA (pt107u). Idempotente: solo inserta lo que falta.
   · dbo.TSql_AiEnvironment: 4 entornos (EXECUTIVE por defecto, COLLOQUIAL, STREET, TECHNICAL).
   · dbo.TSql_AiCommand: 4 comandos ENV_* (contexto common) para cambiar de entorno hablando.
   · dbo.TSql_AiCommandPhrase: 58 frases (las de los ENV_*, «offset» que faltaba en Copiar en paralelo y
     21 frases de jerga ligadas a un entorno).
   · TextConcept de las comunes sí / no / cancelar (yes / no / cancel), que estaba NULL.
   · dbo.TSql_AiEnvironmentReply: 268 plantillas de respuesta ES/EN.
   Tono EXECUTIVE (decisión): trato de usted, frases cortas, primera persona del asistente solo cuando
   aclara («Le marco el muro 4»); en inglés el texto JS ya es neutro y solo se sobrescriben unas claves.
   STREET: jerga de barrio respetuosa, sin insultos ni contenido ofensivo.
   Requisitos: create TSql_AiEnvironment, TSql_AiEnvironmentReply, alter LinkEnvironment y seed TSql_AiCommand.
   ============================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @User NVARCHAR(128) =
(
    SELECT TOP (1) u.Id FROM dbo.AspNetUsers u ORDER BY u.Id
);

IF @User IS NULL
    PRINT N'Aviso: AspNetUsers vacío — semilla de entornos omitida.';
ELSE IF OBJECT_ID(N'dbo.TSql_AiEnvironment', N'U') IS NULL OR OBJECT_ID(N'dbo.TSql_AiEnvironmentReply', N'U') IS NULL
     OR COL_LENGTH(N'dbo.TSql_AiCommandPhrase', N'LinkEnvironment') IS NULL
    PRINT N'Aviso: faltan los create / alter del 14/11/2026 (entornos).';
ELSE
BEGIN
    /* ---- Entornos ---- */
    DECLARE @Env TABLE (TextCode NVARCHAR(40) NOT NULL PRIMARY KEY, TextNameEs NVARCHAR(200) NOT NULL, TextNameEn NVARCHAR(200) NOT NULL,
                        TextDescription NVARCHAR(1000) NULL, Is_Default BIT NOT NULL, NumberOrder INT NOT NULL);
    INSERT INTO @Env (TextCode, TextNameEs, TextNameEn, TextDescription, Is_Default, NumberOrder)
    VALUES
        (N'EXECUTIVE', N'Ejecutivo', N'Executive', N'Formal y conciso: trato de usted, frases cortas, sin coletillas. Entorno por defecto.', 1, 10),
        (N'COLLOQUIAL', N'Coloquial', N'Colloquial', N'Cercano: tuteo y expresiones de la calle, sin perder precisión.', 0, 20),
        (N'STREET', N'Colega barriobajero', N'Street buddy', N'Como un colega de obra: jerga de barrio, siempre respetuoso, sin insultos ni contenido ofensivo.', 0, 30),
        (N'TECHNICAL', N'Técnico', N'Technical', N'Telegráfico y con datos: abreviaturas técnicas (L, e, h), sin cortesías.', 0, 40);

    INSERT INTO dbo.TSql_AiEnvironment
    (
        TextCode, TextNameEs, TextNameEn, TextDescription, Is_Default, NumberOrder,
        TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
    )
    SELECT S.TextCode, S.TextNameEs, S.TextNameEn, S.TextDescription,
           CASE WHEN S.Is_Default = 1 AND NOT EXISTS (SELECT 1 FROM dbo.TSql_AiEnvironment D WHERE D.Is_Default = 1 AND D.Is_Delete = 0) THEN 1 ELSE 0 END,
           S.NumberOrder, S.TextNameEs, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Env S
     WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_AiEnvironment T WHERE T.TextCode = S.TextCode AND T.Is_Delete = 0);

    /* ---- Comandos ENV_* ---- */
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
        (N'ENV_EXECUTIVE', N'common', N'Común · Entorno', N'Entorno ejecutivo', N'Executive environment', N'Cambia el entorno de conversación del usuario (lo guarda en TSql_AiUserSettings)', N'ai:env EXECUTIVE|maStlAiCatalogSaveEnv', N'env', N'[{"name":"entorno","required":true,"help":"EXECUTIVE"}]', 0, 0, 1, 2000),
        (N'ENV_COLLOQUIAL', N'common', N'Común · Entorno', N'Entorno coloquial', N'Colloquial environment', N'Cambia el entorno de conversación del usuario (lo guarda en TSql_AiUserSettings)', N'ai:env COLLOQUIAL|maStlAiCatalogSaveEnv', N'env', N'[{"name":"entorno","required":true,"help":"COLLOQUIAL"}]', 0, 0, 1, 2010),
        (N'ENV_STREET', N'common', N'Común · Entorno', N'Entorno colega', N'Street buddy environment', N'Cambia el entorno de conversación del usuario (lo guarda en TSql_AiUserSettings)', N'ai:env STREET|maStlAiCatalogSaveEnv', N'env', N'[{"name":"entorno","required":true,"help":"STREET"}]', 0, 0, 1, 2020),
        (N'ENV_TECHNICAL', N'common', N'Común · Entorno', N'Entorno técnico', N'Technical environment', N'Cambia el entorno de conversación del usuario (lo guarda en TSql_AiUserSettings)', N'ai:env TECHNICAL|maStlAiCatalogSaveEnv', N'env', N'[{"name":"entorno","required":true,"help":"TECHNICAL"}]', 0, 0, 1, 2030);

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

    /* ---- Concepto JS de sí / no / cancelar (solo si está vacío) ---- */
    DECLARE @Fix TABLE (TextCode NVARCHAR(60) NOT NULL, TextConcept NVARCHAR(40) NOT NULL);
    INSERT INTO @Fix (TextCode, TextConcept)
    VALUES
        (N'COMMON_CANCEL', N'cancel'),
        (N'COMMON_YES', N'yes'),
        (N'COMMON_NO', N'no');
    UPDATE P
       SET TextConcept = F.TextConcept,
           LinModifiedBy = @User,
           AddLastDateChange = GETDATE(),
           Ntimeschanged = P.Ntimeschanged + 1
      FROM dbo.TSql_AiCommandPhrase P
      JOIN dbo.TSql_AiCommand C ON C.IdObject = P.LinkAiCommand
      JOIN @Fix F ON F.TextCode = C.TextCode
     WHERE P.TextConcept IS NULL AND P.Is_Delete = 0;

    /* ---- Frases ---- */
    DECLARE @Phr TABLE
    (
        TextCode NVARCHAR(60) NOT NULL, TextLanguageCode NVARCHAR(10) NOT NULL, TextPhrase NVARCHAR(200) NOT NULL,
        TextPhraseNorm NVARCHAR(200) NOT NULL, TextKind NVARCHAR(20) NOT NULL, TextConcept NVARCHAR(40) NULL,
        EnvCode NVARCHAR(40) NULL, NumberOrder INT NOT NULL
    );
    INSERT INTO @Phr (TextCode, TextLanguageCode, TextPhrase, TextPhraseNorm, TextKind, TextConcept, EnvCode, NumberOrder)
    VALUES
        (N'ENV_EXECUTIVE', N'es', N'modo ejecutivo', N'modo ejecutivo', N'keyword', NULL, NULL, 5010),
        (N'ENV_EXECUTIVE', N'es', N'entorno ejecutivo', N'entorno ejecutivo', N'keyword', NULL, NULL, 5020),
        (N'ENV_EXECUTIVE', N'es', N'ponte serio', N'ponte serio', N'keyword', NULL, NULL, 5030),
        (N'ENV_EXECUTIVE', N'es', N'habla formal', N'habla formal', N'keyword', NULL, NULL, 5040),
        (N'ENV_EXECUTIVE', N'es', N'modo formal', N'modo formal', N'keyword', NULL, NULL, 5050),
        (N'ENV_EXECUTIVE', N'es', N'trátame de usted', N'tratame de usted', N'keyword', NULL, NULL, 5060),
        (N'ENV_EXECUTIVE', N'en', N'executive mode', N'executive mode', N'keyword', NULL, NULL, 5070),
        (N'ENV_EXECUTIVE', N'en', N'be formal', N'be formal', N'keyword', NULL, NULL, 5080),
        (N'ENV_EXECUTIVE', N'en', N'formal mode', N'formal mode', N'keyword', NULL, NULL, 5090),
        (N'ENV_EXECUTIVE', N'en', N'executive environment', N'executive environment', N'keyword', NULL, NULL, 5100),
        (N'ENV_COLLOQUIAL', N'es', N'modo coloquial', N'modo coloquial', N'keyword', NULL, NULL, 5010),
        (N'ENV_COLLOQUIAL', N'es', N'entorno coloquial', N'entorno coloquial', N'keyword', NULL, NULL, 5020),
        (N'ENV_COLLOQUIAL', N'es', N'habla normal', N'habla normal', N'keyword', NULL, NULL, 5030),
        (N'ENV_COLLOQUIAL', N'es', N'modo normal', N'modo normal', N'keyword', NULL, NULL, 5040),
        (N'ENV_COLLOQUIAL', N'es', N'háblame de tú', N'hablame de tu', N'keyword', NULL, NULL, 5050),
        (N'ENV_COLLOQUIAL', N'en', N'casual mode', N'casual mode', N'keyword', NULL, NULL, 5060),
        (N'ENV_COLLOQUIAL', N'en', N'colloquial mode', N'colloquial mode', N'keyword', NULL, NULL, 5070),
        (N'ENV_COLLOQUIAL', N'en', N'talk normally', N'talk normally', N'keyword', NULL, NULL, 5080),
        (N'ENV_COLLOQUIAL', N'en', N'colloquial environment', N'colloquial environment', N'keyword', NULL, NULL, 5090),
        (N'ENV_STREET', N'es', N'modo colega', N'modo colega', N'keyword', NULL, NULL, 5010),
        (N'ENV_STREET', N'es', N'entorno colega', N'entorno colega', N'keyword', NULL, NULL, 5020),
        (N'ENV_STREET', N'es', N'habla como un colega', N'habla como un colega', N'keyword', NULL, NULL, 5030),
        (N'ENV_STREET', N'es', N'modo barrio', N'modo barrio', N'keyword', NULL, NULL, 5040),
        (N'ENV_STREET', N'es', N'háblame como un colega', N'hablame como un colega', N'keyword', NULL, NULL, 5050),
        (N'ENV_STREET', N'en', N'buddy mode', N'buddy mode', N'keyword', NULL, NULL, 5060),
        (N'ENV_STREET', N'en', N'talk like a mate', N'talk like a mate', N'keyword', NULL, NULL, 5070),
        (N'ENV_STREET', N'en', N'street mode', N'street mode', N'keyword', NULL, NULL, 5080),
        (N'ENV_STREET', N'en', N'mate mode', N'mate mode', N'keyword', NULL, NULL, 5090),
        (N'ENV_TECHNICAL', N'es', N'modo técnico', N'modo tecnico', N'keyword', NULL, NULL, 5010),
        (N'ENV_TECHNICAL', N'es', N'entorno técnico', N'entorno tecnico', N'keyword', NULL, NULL, 5020),
        (N'ENV_TECHNICAL', N'es', N'habla técnico', N'habla tecnico', N'keyword', NULL, NULL, 5030),
        (N'ENV_TECHNICAL', N'es', N'sé técnico', N'se tecnico', N'keyword', NULL, NULL, 5040),
        (N'ENV_TECHNICAL', N'en', N'technical mode', N'technical mode', N'keyword', NULL, NULL, 5050),
        (N'ENV_TECHNICAL', N'en', N'talk technical', N'talk technical', N'keyword', NULL, NULL, 5060),
        (N'ENV_TECHNICAL', N'en', N'technical environment', N'technical environment', N'keyword', NULL, NULL, 5070),
        (N'WALL_COPY', N'es', N'offset', N'offset', N'helper', N'parallel', NULL, 5010),
        (N'WALL_COPY', N'en', N'offset', N'offset', N'helper', N'parallel', NULL, 5020),
        (N'WALL_DRAW', N'es', N'plántame un muro', N'plantame un muro', N'keyword', N'draw', N'COLLOQUIAL', 5010),
        (N'WALL_DRAW', N'es', N'hazme una pared', N'hazme una pared', N'keyword', N'draw', N'COLLOQUIAL', 5020),
        (N'WALL_DELETE', N'es', N'quítame ese muro', N'quitame ese muro', N'keyword', N'del', N'COLLOQUIAL', 5010),
        (N'DESIGN_UNDO', N'es', N'déjalo como antes', N'dejalo como antes', N'keyword', N'undo', N'COLLOQUIAL', 5010),
        (N'WALL_DRAW', N'en', N'pop a wall in', N'pop a wall in', N'keyword', N'draw', N'COLLOQUIAL', 5030),
        (N'DESIGN_UNDO', N'en', N'put it back', N'put it back', N'keyword', N'undo', N'COLLOQUIAL', 5020),
        (N'WALL_DRAW', N'es', N'méteme un muro, tío', N'meteme un muro tio', N'keyword', N'draw', N'STREET', 5040),
        (N'WALL_DRAW', N'es', N'cáscame un muro', N'cascame un muro', N'keyword', N'draw', N'STREET', 5050),
        (N'WALL_DELETE', N'es', N'fulmina ese muro', N'fulmina ese muro', N'keyword', N'del', N'STREET', 5020),
        (N'WALL_MOVE', N'es', N'arrastra ese muro, tío', N'arrastra ese muro tio', N'keyword', N'move', N'STREET', 5010),
        (N'DESIGN_UNDO', N'es', N'marcha atrás, tío', N'marcha atras tio', N'keyword', N'undo', N'STREET', 5030),
        (N'MODE_FORMWORK', N'es', N'enchúfale el encofrado', N'enchufale el encofrado', N'keyword', N'formwork', N'STREET', 5010),
        (N'DESIGN_THANKS', N'es', N'gracias, máquina', N'gracias maquina', N'keyword', N'thanks', N'STREET', 5010),
        (N'WALL_DRAW', N'en', N'chuck in a wall', N'chuck in a wall', N'keyword', N'draw', N'STREET', 5060),
        (N'WALL_DELETE', N'en', N'bin that wall', N'bin that wall', N'keyword', N'del', N'STREET', 5030),
        (N'DESIGN_THANKS', N'en', N'cheers mate', N'cheers mate', N'keyword', N'thanks', N'STREET', 5020),
        (N'WALL_DRAW', N'es', N'crear muro recto', N'crear muro recto', N'keyword', N'draw', N'TECHNICAL', 5070),
        (N'WALL_DELETE', N'es', N'eliminar muro', N'eliminar muro', N'keyword', N'del', N'TECHNICAL', 5040),
        (N'WALL_LIST', N'es', N'inventario de muros', N'inventario de muros', N'keyword', N'list', N'TECHNICAL', 5010),
        (N'WALL_DRAW', N'en', N'create straight wall', N'create straight wall', N'keyword', N'draw', N'TECHNICAL', 5080),
        (N'WALL_DELETE', N'en', N'delete wall entity', N'delete wall entity', N'keyword', N'del', N'TECHNICAL', 5050);

    SELECT S.TextCode, S.TextLanguageCode, S.TextPhrase, OC.TextCode AS ConflictsWith
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
    SELECT C.IdObject, C.TextContext, S.TextLanguageCode, S.TextPhrase, S.TextPhraseNorm, S.TextKind, S.TextConcept, NULL, S.NumberOrder, E.IdObject,
           S.TextPhrase, 0, 1, @User, NULL, GETDATE(), NULL, 0
      FROM @Phr S
      JOIN dbo.TSql_AiCommand C ON C.TextCode = S.TextCode AND C.Is_Delete = 0
      LEFT JOIN dbo.TSql_AiEnvironment E ON E.TextCode = S.EnvCode AND E.Is_Delete = 0
     WHERE (S.EnvCode IS NULL OR E.IdObject IS NOT NULL)
       AND NOT EXISTS (SELECT 1 FROM dbo.TSql_AiCommandPhrase T
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
        (N'EXECUTIVE', N'es', N'design.notReady', N'El visor aún no está listo. Abra un diseño y vuelva a intentarlo.'),
        (N'EXECUTIVE', N'es', N'design.askWall', N'¿Qué muro? Selecciónelo en el visor o indique su número («el muro 4»).'),
        (N'EXECUTIVE', N'es', N'design.formworkCleared', N'El encofrado anterior ya no es válido y se ha retirado; vuelva a encofrar con el botón Encofrado al terminar.'),
        (N'EXECUTIVE', N'es', N'design.defaultDir', N'No se indicó la dirección: se ha dibujado hacia la derecha.'),
        (N'EXECUTIVE', N'es', N'design.drawn', N'Dibujado el {0}: {1} {2}, espesor {3}, alto {4}, {5}.'),
        (N'EXECUTIVE', N'es', N'design.confirmDelete', N'¿Confirma que borro el {0} ({1})?'),
        (N'EXECUTIVE', N'es', N'design.deleted', N'Borrado el {0}. Si ha sido un error, diga «deshaz».'),
        (N'EXECUTIVE', N'es', N'design.deleteCancel', N'De acuerdo, no se borra.'),
        (N'EXECUTIVE', N'es', N'design.moved', N'Movido el {0} {1} {2}.'),
        (N'EXECUTIVE', N'es', N'design.copied', N'Copiado el {0} {1} {2}: la copia es el {3}.'),
        (N'EXECUTIVE', N'es', N'design.rotated', N'Girado el {0} {1} en sentido {2} {3}.'),
        (N'EXECUTIVE', N'es', N'design.undone', N'Último paso deshecho.'),
        (N'EXECUTIVE', N'es', N'design.redone', N'Paso rehecho.'),
        (N'EXECUTIVE', N'es', N'design.shown', N'Le marco el {0}: {1}.'),
        (N'EXECUTIVE', N'es', N'design.notAvailable', N'«{0}» todavía no existe en el visor. Queda anotado como propuesta. Escriba «ayuda» para ver las opciones disponibles.'),
        (N'EXECUTIVE', N'es', N'design.didYouMean', N'¿Quería decir «{0}»?'),
        (N'EXECUTIVE', N'es', N'design.helpTail', N'Cada orden se deshace de una en una («deshaz»). Cada muro tiene su número; «lista los muros» los muestra.'),
        (N'EXECUTIVE', N'es', N'design.helpSpeech', N'Le dejo los ejemplos en pantalla.'),
        (N'EXECUTIVE', N'es', N'design.thanks', N'A su disposición.'),
        (N'EXECUTIVE', N'es', N'design.cancelled', N'De acuerdo, queda cancelado.'),
        (N'EXECUTIVE', N'es', N'design.modeAlready', N'Ya está en el modo {0}.'),
        (N'EXECUTIVE', N'es', N'design.modeBusy', N'El visor está calculando (3D o encofrado); espere a que termine y vuelva a solicitarlo.'),
        (N'EXECUTIVE', N'es', N'design.pivotDefaultNote', N'Por defecto giro sobre el centro del muro; indique «sobre su inicio» o «sobre su final» para otro punto.'),
        (N'EXECUTIVE', N'es', N'design.mode3dFromFormwork', N'El encofrado deja de mostrarse (el botón 3D enseña el modelo sin paneles); diga «encofra» para volver a calcularlo.'),
        (N'EXECUTIVE', N'es', N'design.formworkAlready', N'Ya está en el modo encofrar (el diseño está encofrado). ¿Lo vuelvo a calcular para todo el diseño?'),
        (N'EXECUTIVE', N'es', N'design.formworkNo', N'De acuerdo, no se encofra.'),
        (N'EXECUTIVE', N'es', N'design.envChanged', N'Entorno de conversación: {0}.'),
        (N'EXECUTIVE', N'es', N'design.envAlready', N'El entorno {0} ya está activo.'),
        (N'EXECUTIVE', N'es', N'design.envFail', N'No se ha podido cambiar el entorno. Inténtelo de nuevo más tarde.'),
        (N'EXECUTIVE', N'es', N'design.pendingAi', N'«{0}» aún no está disponible por voz o texto; utilice el botón de la barra.'),
        (N'EXECUTIVE', N'es', N'wall.thanks', N'A su disposición. Para el material de otro muro, ábralo y solicítelo.'),
        (N'EXECUTIVE', N'es', N'wall.error', N'No se ha podido procesar el mensaje: {0}'),
        (N'EXECUTIVE', N'es', N'wall.envFail', N'No se ha podido cambiar el entorno. Inténtelo de nuevo más tarde.'),
        (N'EXECUTIVE', N'es', N'wall.help', N'No he entendido «{0}». Opciones disponibles:
• Lista de materiales del encofrado de este muro: «dame el material de encofrado de este muro», «lista de materiales», «despiece», «¿qué necesito para encofrar este muro?».
• Paneles de los que no dispone: «no tengo paneles de 0,90, sustitúyelos» o «solo tengo 3 paneles de 0,90». Le muestro el antes y el después y solo lo aplico si lo confirma.
• «ayuda»: muestra esta lista.'),
        (N'EXECUTIVE', N'es', N'wall.replaceSoon', N'Indique qué paneles no tiene, por ejemplo «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'EXECUTIVE', N'es', N'wall.matNew', N'Este muro curvo aún no existe en el diseño. Créelo con «Aceptar» y calcule el encofrado; después le doy su material.'),
        (N'EXECUTIVE', N'es', N'wall.matNone', N'Este diseño todavía no tiene el encofrado calculado; no es posible darle su material.'),
        (N'EXECUTIVE', N'es', N'wall.matRunHint', N'«Calcular encofrado» calcula y pinta el encofrado de todo el diseño (botón «Encofrar»). Al terminar le doy la lista.'),
        (N'EXECUTIVE', N'es', N'wall.stockAskWidth', N'¿De qué ancho son los paneles que no tiene? (0,30 · 0,45 · 0,60 · 0,75 · 0,90). Por ejemplo: «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'EXECUTIVE', N'es', N'wall.stockNoChange', N'Este muro no usa más paneles de {0} de los indicados: no hay nada que cambiar.'),
        (N'EXECUTIVE', N'es', N'wall.stockMaxOver', N'Atención: aún se usan {0} paneles de {1} y dispone de {2} (el resto está en tapes o tramos sin otra combinación).'),
        (N'EXECUTIVE', N'es', N'wall.stockMaxUsed', N'Paneles de {1}: se usan {0} de los {2} disponibles (cuentan las dos caras).'),
        (N'EXECUTIVE', N'es', N'wall.stockApplied', N'Hecho: se ha vuelto a encofrar solo este muro ({0} piezas). La regla queda en el muro mientras trabaja en este diseño (todavía no se guarda en la base de datos).'),
        (N'EXECUTIVE', N'es', N'wall.stockCancelled', N'De acuerdo, no se cambia nada.'),
        (N'EXECUTIVE', N'es', N'wall.stockAlready', N'Ese cambio ya está aplicado; para volver atrás, pulse «Deshacer».'),
        (N'EXECUTIVE', N'es', N'wall.stockCancelledAlready', N'Ese cambio se canceló; vuelva a solicitarlo si lo necesita.'),
        (N'EXECUTIVE', N'es', N'wall.swapNeedHave', N'Dispone de {0}; le faltan {1}.'),
        (N'EXECUTIVE', N'es', N'wall.swapNeedMissing', N'Le faltan {0}.'),
        (N'EXECUTIVE', N'es', N'wall.swapNeedNone', N'No dispone de ninguno: hay que cambiar los {0}.'),
        (N'EXECUTIVE', N'es', N'wall.swapEnough', N'Con {0} es suficiente: en este muro hay {1}. No hace falta cambiar nada.'),
        (N'EXECUTIVE', N'es', N'wall.swapAskHave', N'En este muro hay {0} paneles de {1}×{2}. ¿Cuántos tiene (o cuántos le faltan)?'),
        (N'EXECUTIVE', N'es', N'wall.swapAskHeight', N'En este muro hay paneles de {0} de varias alturas ({1}). ¿De qué altura son los que le faltan?'),
        (N'EXECUTIVE', N'es', N'wall.swapAskOption', N'¿Qué opción? Pulse un botón o indíquela («la 2», «dos de 0,45»).'),
        (N'EXECUTIVE', N'es', N'wall.swapStep', N'Panel {0} de {1}: el nº {2} desde el inicio del muro (de {3} a {4} m{5}), en las dos caras. Aparece resaltado en naranja en el visor.'),
        (N'EXECUTIVE', N'es', N'wall.swapAskOptionOne', N'¿Por cuál lo cambio? Pulse un botón o indíquelo («la 2», «dos de 0,45», «otro panel»).'),
        (N'EXECUTIVE', N'es', N'wall.swapNotUnderstood', N'No le he entendido. Elija con los botones o indique, por ejemplo, «la 1» o «dos de 0,45».'),
        (N'EXECUTIVE', N'es', N'wall.swapStockLimited', N'Se retiran las opciones que gastan más paneles de los disponibles ({0}).'),
        (N'EXECUTIVE', N'es', N'wall.swapNoValidOption', N'Con los paneles disponibles no hay ninguna opción válida para un panel de {0}.'),
        (N'EXECUTIVE', N'es', N'wall.swapConfirm', N'¿Lo aplico? Se quitan esos paneles con sus grapas, tirantes y demás accesorios y se ponen los nuevos con sus piezas. Solo en esta sesión (no se guarda en BD ni marca el muro como especial).'),
        (N'EXECUTIVE', N'es', N'wall.swapCancelled', N'De acuerdo, no se cambia ningún panel.'),
        (N'EXECUTIVE', N'es', N'wall.curvedStockEnough', N'Necesita {0} y dispone de {1}.'),
        (N'EXECUTIVE', N'es', N'wall.curvedApplied', N'Hecho: modulación {0} en el formulario{1}. Pulse «Aceptar» para guardarla en el muro.'),
        (N'EXECUTIVE', N'es', N'wall.matRunUnavailable', N'No se encuentra el botón «Encofrar» en la barra; cierre esta ventana y pulse «Encofrar».'),
        (N'EXECUTIVE', N'es', N'wall.speakScreen', N'Le dejo el detalle en pantalla.'),
        (N'EXECUTIVE', N'es', N'wall.matSpeech', N'Este muro lleva {0} piezas de {1} referencias; le dejo la lista en pantalla.'),
        (N'EXECUTIVE', N'es', N'wall.swapSpeakConfirm', N'Le dejo el resumen y la tabla en pantalla. ¿Lo aplico?'),
        (N'EXECUTIVE', N'en', N'design.thanks', N'You are welcome.'),
        (N'EXECUTIVE', N'en', N'design.cancelled', N'Understood, cancelled.'),
        (N'EXECUTIVE', N'en', N'design.envChanged', N'Conversation environment: {0}.'),
        (N'EXECUTIVE', N'en', N'design.envAlready', N'The {0} environment is already active.'),
        (N'EXECUTIVE', N'en', N'design.envFail', N'The environment could not be changed. Please try again later.'),
        (N'EXECUTIVE', N'en', N'design.pendingAi', N'«{0}» is not available by voice or text yet; please use the toolbar button.'),
        (N'COLLOQUIAL', N'es', N'design.drawn', N'¡Hecho! Te he dibujado el {0}: {1} {2}, espesor {3}, alto {4}, {5}.'),
        (N'COLLOQUIAL', N'es', N'design.confirmDelete', N'¿Seguro que borro el {0} ({1})?'),
        (N'COLLOQUIAL', N'es', N'design.deleted', N'Listo, el {0} ya no está. Si te has equivocado, dime «deshaz».'),
        (N'COLLOQUIAL', N'es', N'design.deleteCancel', N'Vale, lo dejo como está.'),
        (N'COLLOQUIAL', N'es', N'design.didYouMean', N'¿Querías decir «{0}»?'),
        (N'COLLOQUIAL', N'es', N'design.notUnderstood', N'Uy, no he pillado «{0}».'),
        (N'COLLOQUIAL', N'es', N'design.thanks', N'¡De nada!'),
        (N'COLLOQUIAL', N'es', N'design.cancelled', N'Vale, lo dejamos.'),
        (N'COLLOQUIAL', N'es', N'design.undone', N'Deshecho, como estaba antes.'),
        (N'COLLOQUIAL', N'es', N'design.moved', N'Movido: el {0} va {1} {2}.'),
        (N'COLLOQUIAL', N'es', N'design.formworkDone', N'¡Encofrado listo! {0} piezas en el visor.'),
        (N'COLLOQUIAL', N'es', N'design.envChanged', N'Vale, hablamos en modo {0}.'),
        (N'COLLOQUIAL', N'es', N'design.envAlready', N'Ya estamos en modo {0}.'),
        (N'COLLOQUIAL', N'es', N'design.envFail', N'No he podido cambiar el modo de hablar; prueba otra vez en un rato.'),
        (N'COLLOQUIAL', N'es', N'design.pendingAi', N'«{0}» todavía no lo puedo hacer yo; usa el botón de la barra.'),
        (N'COLLOQUIAL', N'es', N'wall.morning', N'¡Buenos días!'),
        (N'COLLOQUIAL', N'es', N'wall.afternoon', N'¡Buenas tardes!'),
        (N'COLLOQUIAL', N'es', N'wall.night', N'¡Buenas noches!'),
        (N'COLLOQUIAL', N'es', N'wall.thanks', N'¡De nada! Si quieres el material de otro muro, ábrelo y me lo pides.'),
        (N'COLLOQUIAL', N'es', N'design.notReady', N'El visor aún no está listo para dibujar. Abre un diseño y vuelve a intentarlo.'),
        (N'COLLOQUIAL', N'es', N'design.askWall', N'¿Qué muro? Selecciónalo en el visor o dime su número («el muro 4»).'),
        (N'COLLOQUIAL', N'es', N'design.formworkCleared', N'El encofrado que había ya no vale y se ha quitado (como con las herramientas del visor); vuelve a encofrar con el botón Encofrado cuando termines.'),
        (N'COLLOQUIAL', N'es', N'design.defaultDir', N'No me has dicho la dirección: lo he hecho hacia la derecha.'),
        (N'COLLOQUIAL', N'es', N'design.copied', N'He copiado el {0} {1} {2}: la copia es el {3}.'),
        (N'COLLOQUIAL', N'es', N'design.rotated', N'He girado el {0} {1} en sentido {2} {3}.'),
        (N'COLLOQUIAL', N'es', N'design.redone', N'He rehecho el paso.'),
        (N'COLLOQUIAL', N'es', N'design.shown', N'Te marco el {0}: {1}.'),
        (N'COLLOQUIAL', N'es', N'design.notAvailable', N'«{0}» todavía no existe en el visor, así que no lo puedo hacer. Lo dejo apuntado como propuesta. Escribe «ayuda» para ver lo que sí sé hacer.'),
        (N'COLLOQUIAL', N'es', N'design.helpTail', N'Cada orden se deshace de una en una («deshaz»). Cada muro tiene su número; «lista los muros» te los enseña.'),
        (N'COLLOQUIAL', N'es', N'design.helpSpeech', N'Te dejo los ejemplos en pantalla.'),
        (N'COLLOQUIAL', N'es', N'design.modeAlready', N'Ya estás en el modo {0}.'),
        (N'COLLOQUIAL', N'es', N'design.modeBusy', N'El visor está calculando (3D o encofrado); espera a que termine y vuelve a pedírmelo.'),
        (N'COLLOQUIAL', N'es', N'design.pivotDefaultNote', N'Por defecto giro sobre el centro del muro; di «sobre su inicio» o «sobre su final» si lo quieres desde otro punto.'),
        (N'COLLOQUIAL', N'es', N'design.mode3dFromFormwork', N'El encofrado deja de verse (el botón 3D enseña el modelo sin paneles); di «encofra» para volver a calcularlo.'),
        (N'COLLOQUIAL', N'es', N'design.formworkAlready', N'Ya estás en el modo encofrar (el diseño está encofrado). ¿Lo vuelvo a calcular para todo el diseño?'),
        (N'COLLOQUIAL', N'es', N'design.formworkNo', N'Vale, no encofro.'),
        (N'COLLOQUIAL', N'es', N'wall.error', N'No he podido procesar el mensaje: {0}'),
        (N'COLLOQUIAL', N'es', N'wall.help', N'No he entendido «{0}». De momento sé hacer esto:
• Lista de materiales del encofrado de este muro: «dame el material de encofrado de este muro», «lista de materiales», «despiece», «¿qué necesito para encofrar este muro?».
• Paneles que no tienes: «no tengo paneles de 0,90, sustitúyelos» o «solo tengo 3 paneles de 0,90». Te enseño el antes y el después y lo aplico solo si me dices que sí.
• «ayuda»: muestra esta lista.'),
        (N'COLLOQUIAL', N'es', N'wall.replaceSoon', N'Dime qué paneles no tienes, por ejemplo «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'COLLOQUIAL', N'es', N'wall.matNew', N'Este muro curvo aún no existe en el diseño. Créalo con «Aceptar» y calcula el encofrado; después te doy su material.'),
        (N'COLLOQUIAL', N'es', N'wall.matNone', N'Este diseño todavía no tiene el encofrado calculado, así que no puedo darte su material.'),
        (N'COLLOQUIAL', N'es', N'wall.matRunHint', N'«Calcular encofrado» calcula y pinta el encofrado de todo el diseño (botón «Encofrar»). Cuando termine te doy la lista.'),
        (N'COLLOQUIAL', N'es', N'wall.stockAskWidth', N'¿De qué ancho son los paneles que no tienes? (0,30 · 0,45 · 0,60 · 0,75 · 0,90). Por ejemplo: «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'COLLOQUIAL', N'es', N'wall.stockNoChange', N'Este muro no usa más paneles de {0} de los que dices: no hay nada que cambiar.'),
        (N'COLLOQUIAL', N'es', N'wall.stockMaxOver', N'Ojo: aún se usan {0} paneles de {1} y tienes {2} (el resto está en tapes o tramos sin otra combinación).'),
        (N'COLLOQUIAL', N'es', N'wall.stockMaxUsed', N'Paneles de {1}: se usan {0} de los {2} que tienes (las dos caras cuentan).'),
        (N'COLLOQUIAL', N'es', N'wall.stockApplied', N'Hecho: he vuelto a encofrar solo este muro ({0} piezas). La regla queda en el muro mientras trabajas en este diseño (todavía no se guarda en la base de datos).'),
        (N'COLLOQUIAL', N'es', N'wall.stockCancelled', N'Vale, no cambio nada.'),
        (N'COLLOQUIAL', N'es', N'wall.stockAlready', N'Ese cambio ya está aplicado; si quieres volver atrás, pulsa «Deshacer».'),
        (N'COLLOQUIAL', N'es', N'wall.stockCancelledAlready', N'Ese cambio lo cancelaste; pídemelo otra vez si lo quieres.'),
        (N'COLLOQUIAL', N'es', N'wall.swapNeedHave', N'Tienes {0}, así que te faltan {1}.'),
        (N'COLLOQUIAL', N'es', N'wall.swapNeedMissing', N'Te faltan {0}.'),
        (N'COLLOQUIAL', N'es', N'wall.swapNeedNone', N'No tienes ninguno: hay que cambiar los {0}.'),
        (N'COLLOQUIAL', N'es', N'wall.swapEnough', N'Con {0} te llega: en este muro hay {1}. No hace falta cambiar nada.'),
        (N'COLLOQUIAL', N'es', N'wall.swapAskHave', N'En este muro hay {0} paneles de {1}×{2}. ¿Cuántos tienes (o cuántos te faltan)?'),
        (N'COLLOQUIAL', N'es', N'wall.swapAskHeight', N'En este muro hay paneles de {0} de varias alturas ({1}). ¿De qué altura son los que te faltan?'),
        (N'COLLOQUIAL', N'es', N'wall.swapAskOption', N'¿Qué opción? Pulsa un botón o dímelo («la 2», «dos de 0,45»).'),
        (N'COLLOQUIAL', N'es', N'wall.swapStep', N'Panel {0} de {1}: el nº {2} desde el inicio del muro (de {3} a {4} m{5}), en las dos caras. Lo tienes resaltado en naranja en el visor.'),
        (N'COLLOQUIAL', N'es', N'wall.swapAskOptionOne', N'¿Con qué lo cambio? Pulsa un botón o dímelo («la 2», «dos de 0,45», «otro panel»).'),
        (N'COLLOQUIAL', N'es', N'wall.swapNotUnderstood', N'No te he entendido. Elige con los botones o dime, por ejemplo, «la 1» o «dos de 0,45».'),
        (N'COLLOQUIAL', N'es', N'wall.swapStockLimited', N'Quito las opciones que gastan más paneles de los que tienes ({0}).'),
        (N'COLLOQUIAL', N'es', N'wall.swapNoValidOption', N'Con los paneles que tienes no hay ninguna opción válida para un panel de {0}.'),
        (N'COLLOQUIAL', N'es', N'wall.swapConfirm', N'¿Lo hago? Quito esos paneles con sus grapas, tirantes y demás accesorios y pongo los nuevos con sus piezas. Solo en esta sesión (no se guarda en BD ni marca el muro como especial).'),
        (N'COLLOQUIAL', N'es', N'wall.swapCancelled', N'Vale, no cambio ningún panel.'),
        (N'COLLOQUIAL', N'es', N'wall.curvedStockEnough', N'Necesita {0} y tienes {1}.'),
        (N'COLLOQUIAL', N'es', N'wall.curvedApplied', N'Hecho: modulación {0} en el formulario{1}. Pulsa «Aceptar» para guardarla en el muro.'),
        (N'COLLOQUIAL', N'es', N'wall.matRunUnavailable', N'No encuentro el botón «Encofrar» en la barra; cierra esta ventana y pulsa «Encofrar».'),
        (N'COLLOQUIAL', N'es', N'wall.speakScreen', N'Te dejo el detalle en pantalla.'),
        (N'COLLOQUIAL', N'es', N'wall.matSpeech', N'Este muro lleva {0} piezas de {1} referencias; te dejo la lista en pantalla.'),
        (N'COLLOQUIAL', N'es', N'wall.swapSpeakConfirm', N'Te dejo el resumen y la tabla en pantalla. ¿Lo hago?'),
        (N'COLLOQUIAL', N'es', N'wall.envFail', N'No he podido cambiar el modo de hablar; prueba otra vez en un rato.'),
        (N'COLLOQUIAL', N'en', N'design.drawn', N'Done! I drew {0}: {1} {2}, thickness {3}, height {4}, {5}.'),
        (N'COLLOQUIAL', N'en', N'design.notUnderstood', N'Sorry, I didn''t get «{0}».'),
        (N'COLLOQUIAL', N'en', N'design.thanks', N'No problem!'),
        (N'COLLOQUIAL', N'en', N'design.cancelled', N'OK, dropping it.'),
        (N'COLLOQUIAL', N'en', N'design.envChanged', N'Sure, switching to {0} mode.'),
        (N'COLLOQUIAL', N'en', N'design.envAlready', N'We are already in {0} mode.'),
        (N'COLLOQUIAL', N'en', N'design.pendingAi', N'I can''t do «{0}» myself yet; use the toolbar button.'),
        (N'COLLOQUIAL', N'en', N'wall.morning', N'Morning!'),
        (N'COLLOQUIAL', N'en', N'wall.afternoon', N'Afternoon!'),
        (N'COLLOQUIAL', N'en', N'wall.night', N'Evening!'),
        (N'STREET', N'es', N'design.drawn', N'¡Ahí lo llevas, tío! El {0} ya está: {1} {2}, espesor {3}, alto {4}, {5}.'),
        (N'STREET', N'es', N'design.confirmDelete', N'Oye, ¿me cargo el {0} ({1})? Dime sí o no.'),
        (N'STREET', N'es', N'design.deleted', N'Fuera, el {0} ya no está. Si la has liado, di «deshaz» y lo apaño.'),
        (N'STREET', N'es', N'design.deleteCancel', N'Tranqui, no lo toco.'),
        (N'STREET', N'es', N'design.didYouMean', N'¿Lo que quieres es «{0}», colega?'),
        (N'STREET', N'es', N'design.notUnderstood', N'Perdona, tío, no te he pillado lo de «{0}».'),
        (N'STREET', N'es', N'design.thanks', N'¡A mandar, colega!'),
        (N'STREET', N'es', N'design.cancelled', N'Vale, tío, lo dejamos estar.'),
        (N'STREET', N'es', N'design.undone', N'Hecho, marcha atrás.'),
        (N'STREET', N'es', N'design.redone', N'Venga, otra vez p''alante.'),
        (N'STREET', N'es', N'design.moved', N'Movido, tío: el {0} va {1} {2}.'),
        (N'STREET', N'es', N'design.copied', N'Ahí tienes copia del {0} {1} {2}: es el {3}.'),
        (N'STREET', N'es', N'design.rotated', N'Girado el {0} {1} en sentido {2} {3}, fino.'),
        (N'STREET', N'es', N'design.formworkDone', N'¡Encofrado montado, máquina! {0} piezas en el visor.'),
        (N'STREET', N'es', N'design.modeLinesDone', N'Hecho, estamos en líneas (2D).'),
        (N'STREET', N'es', N'design.envChanged', N'¡Hecho, colega! Ahora hablamos en modo {0}.'),
        (N'STREET', N'es', N'design.envAlready', N'Ya estamos en modo {0}, tío.'),
        (N'STREET', N'es', N'design.envFail', N'Uf, no he podido cambiar el rollo; prueba en un rato, colega.'),
        (N'STREET', N'es', N'design.pendingAi', N'Eso de «{0}» todavía no lo hago yo, colega; tira del botón de la barra.'),
        (N'STREET', N'es', N'wall.morning', N'¡Buenos días, colega!'),
        (N'STREET', N'es', N'wall.afternoon', N'¡Buenas tardes, máquina!'),
        (N'STREET', N'es', N'wall.night', N'¡Buenas noches, colega!'),
        (N'STREET', N'es', N'wall.thanks', N'¡A mandar! Si quieres el material de otro muro, ábrelo y me lo pides.'),
        (N'STREET', N'es', N'design.notReady', N'El visor aún no está listo para dibujar. Abre un diseño y vuelve a intentarlo.'),
        (N'STREET', N'es', N'design.askWall', N'¿Qué muro? Selecciónalo en el visor o dime su número («el muro 4»).'),
        (N'STREET', N'es', N'design.formworkCleared', N'El encofrado que había ya no vale y se ha quitado (como con las herramientas del visor); vuelve a encofrar con el botón Encofrado cuando termines.'),
        (N'STREET', N'es', N'design.defaultDir', N'No me has dicho la dirección: lo he hecho hacia la derecha.'),
        (N'STREET', N'es', N'design.shown', N'Te marco el {0}: {1}.'),
        (N'STREET', N'es', N'design.notAvailable', N'«{0}» todavía no existe en el visor, así que no lo puedo hacer. Lo dejo apuntado como propuesta. Escribe «ayuda» para ver lo que sí sé hacer.'),
        (N'STREET', N'es', N'design.helpTail', N'Cada orden se deshace de una en una («deshaz»). Cada muro tiene su número; «lista los muros» te los enseña.'),
        (N'STREET', N'es', N'design.helpSpeech', N'Te dejo los ejemplos en pantalla.'),
        (N'STREET', N'es', N'design.modeAlready', N'Ya estás en el modo {0}.'),
        (N'STREET', N'es', N'design.modeBusy', N'El visor está calculando (3D o encofrado); espera a que termine y vuelve a pedírmelo.'),
        (N'STREET', N'es', N'design.pivotDefaultNote', N'Por defecto giro sobre el centro del muro; di «sobre su inicio» o «sobre su final» si lo quieres desde otro punto.'),
        (N'STREET', N'es', N'design.mode3dFromFormwork', N'El encofrado deja de verse (el botón 3D enseña el modelo sin paneles); di «encofra» para volver a calcularlo.'),
        (N'STREET', N'es', N'design.formworkAlready', N'Ya estás en el modo encofrar (el diseño está encofrado). ¿Lo vuelvo a calcular para todo el diseño?'),
        (N'STREET', N'es', N'design.formworkNo', N'Vale, no encofro.'),
        (N'STREET', N'es', N'wall.error', N'No he podido procesar el mensaje: {0}'),
        (N'STREET', N'es', N'wall.help', N'No he entendido «{0}». De momento sé hacer esto:
• Lista de materiales del encofrado de este muro: «dame el material de encofrado de este muro», «lista de materiales», «despiece», «¿qué necesito para encofrar este muro?».
• Paneles que no tienes: «no tengo paneles de 0,90, sustitúyelos» o «solo tengo 3 paneles de 0,90». Te enseño el antes y el después y lo aplico solo si me dices que sí.
• «ayuda»: muestra esta lista.'),
        (N'STREET', N'es', N'wall.replaceSoon', N'Dime qué paneles no tienes, por ejemplo «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'STREET', N'es', N'wall.matNew', N'Este muro curvo aún no existe en el diseño. Créalo con «Aceptar» y calcula el encofrado; después te doy su material.'),
        (N'STREET', N'es', N'wall.matNone', N'Este diseño todavía no tiene el encofrado calculado, así que no puedo darte su material.'),
        (N'STREET', N'es', N'wall.matRunHint', N'«Calcular encofrado» calcula y pinta el encofrado de todo el diseño (botón «Encofrar»). Cuando termine te doy la lista.'),
        (N'STREET', N'es', N'wall.stockAskWidth', N'¿De qué ancho son los paneles que no tienes? (0,30 · 0,45 · 0,60 · 0,75 · 0,90). Por ejemplo: «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».'),
        (N'STREET', N'es', N'wall.stockNoChange', N'Este muro no usa más paneles de {0} de los que dices: no hay nada que cambiar.'),
        (N'STREET', N'es', N'wall.stockMaxOver', N'Ojo: aún se usan {0} paneles de {1} y tienes {2} (el resto está en tapes o tramos sin otra combinación).'),
        (N'STREET', N'es', N'wall.stockMaxUsed', N'Paneles de {1}: se usan {0} de los {2} que tienes (las dos caras cuentan).'),
        (N'STREET', N'es', N'wall.stockApplied', N'Hecho: he vuelto a encofrar solo este muro ({0} piezas). La regla queda en el muro mientras trabajas en este diseño (todavía no se guarda en la base de datos).'),
        (N'STREET', N'es', N'wall.stockCancelled', N'Vale, no cambio nada.'),
        (N'STREET', N'es', N'wall.stockAlready', N'Ese cambio ya está aplicado; si quieres volver atrás, pulsa «Deshacer».'),
        (N'STREET', N'es', N'wall.stockCancelledAlready', N'Ese cambio lo cancelaste; pídemelo otra vez si lo quieres.'),
        (N'STREET', N'es', N'wall.swapNeedHave', N'Tienes {0}, así que te faltan {1}.'),
        (N'STREET', N'es', N'wall.swapNeedMissing', N'Te faltan {0}.'),
        (N'STREET', N'es', N'wall.swapNeedNone', N'No tienes ninguno: hay que cambiar los {0}.'),
        (N'STREET', N'es', N'wall.swapEnough', N'Con {0} te llega: en este muro hay {1}. No hace falta cambiar nada.'),
        (N'STREET', N'es', N'wall.swapAskHave', N'En este muro hay {0} paneles de {1}×{2}. ¿Cuántos tienes (o cuántos te faltan)?'),
        (N'STREET', N'es', N'wall.swapAskHeight', N'En este muro hay paneles de {0} de varias alturas ({1}). ¿De qué altura son los que te faltan?'),
        (N'STREET', N'es', N'wall.swapAskOption', N'¿Qué opción? Pulsa un botón o dímelo («la 2», «dos de 0,45»).'),
        (N'STREET', N'es', N'wall.swapStep', N'Panel {0} de {1}: el nº {2} desde el inicio del muro (de {3} a {4} m{5}), en las dos caras. Lo tienes resaltado en naranja en el visor.'),
        (N'STREET', N'es', N'wall.swapAskOptionOne', N'¿Con qué lo cambio? Pulsa un botón o dímelo («la 2», «dos de 0,45», «otro panel»).'),
        (N'STREET', N'es', N'wall.swapNotUnderstood', N'No te he entendido. Elige con los botones o dime, por ejemplo, «la 1» o «dos de 0,45».'),
        (N'STREET', N'es', N'wall.swapStockLimited', N'Quito las opciones que gastan más paneles de los que tienes ({0}).'),
        (N'STREET', N'es', N'wall.swapNoValidOption', N'Con los paneles que tienes no hay ninguna opción válida para un panel de {0}.'),
        (N'STREET', N'es', N'wall.swapConfirm', N'¿Lo hago? Quito esos paneles con sus grapas, tirantes y demás accesorios y pongo los nuevos con sus piezas. Solo en esta sesión (no se guarda en BD ni marca el muro como especial).'),
        (N'STREET', N'es', N'wall.swapCancelled', N'Vale, no cambio ningún panel.'),
        (N'STREET', N'es', N'wall.curvedStockEnough', N'Necesita {0} y tienes {1}.'),
        (N'STREET', N'es', N'wall.curvedApplied', N'Hecho: modulación {0} en el formulario{1}. Pulsa «Aceptar» para guardarla en el muro.'),
        (N'STREET', N'es', N'wall.matRunUnavailable', N'No encuentro el botón «Encofrar» en la barra; cierra esta ventana y pulsa «Encofrar».'),
        (N'STREET', N'es', N'wall.speakScreen', N'Te dejo el detalle en pantalla.'),
        (N'STREET', N'es', N'wall.matSpeech', N'Este muro lleva {0} piezas de {1} referencias; te dejo la lista en pantalla.'),
        (N'STREET', N'es', N'wall.swapSpeakConfirm', N'Te dejo el resumen y la tabla en pantalla. ¿Lo hago?'),
        (N'STREET', N'es', N'wall.envFail', N'Uf, no he podido cambiar el rollo; prueba en un rato, colega.'),
        (N'STREET', N'en', N'design.drawn', N'There you go, mate! {0} is in: {1} {2}, thickness {3}, height {4}, {5}.'),
        (N'STREET', N'en', N'design.confirmDelete', N'Shall I bin {0} ({1}), mate?'),
        (N'STREET', N'en', N'design.deleted', N'Gone, {0} is binned. Say «undo» if that was a slip.'),
        (N'STREET', N'en', N'design.notUnderstood', N'Sorry mate, didn''t catch «{0}».'),
        (N'STREET', N'en', N'design.thanks', N'Anytime, mate!'),
        (N'STREET', N'en', N'design.cancelled', N'No worries, mate, leaving it.'),
        (N'STREET', N'en', N'design.envChanged', N'Sorted, mate! We''re talking {0} now.'),
        (N'STREET', N'en', N'design.envAlready', N'Already in {0} mode, mate.'),
        (N'STREET', N'en', N'design.pendingAi', N'Can''t do «{0}» myself yet, mate; hit the toolbar button.'),
        (N'STREET', N'en', N'wall.morning', N'Morning, mate!'),
        (N'STREET', N'en', N'wall.afternoon', N'Afternoon, mate!'),
        (N'STREET', N'en', N'wall.night', N'Evening, mate!'),
        (N'TECHNICAL', N'es', N'design.drawn', N'{0} creado. L={1}; dir. {2}; e={3}; h={4}; {5}.'),
        (N'TECHNICAL', N'es', N'design.confirmDelete', N'Confirmar borrado: {0} ({1}).'),
        (N'TECHNICAL', N'es', N'design.deleted', N'{0} eliminado. Reversible con «deshaz».'),
        (N'TECHNICAL', N'es', N'design.deleteCancel', N'Borrado cancelado.'),
        (N'TECHNICAL', N'es', N'design.moved', N'{0} desplazado {1} {2}.'),
        (N'TECHNICAL', N'es', N'design.copied', N'{0} copiado {1} {2} → {3}.'),
        (N'TECHNICAL', N'es', N'design.rotated', N'{0} girado {1} ({2}) {3}.'),
        (N'TECHNICAL', N'es', N'design.didYouMean', N'¿Orden «{0}»?'),
        (N'TECHNICAL', N'es', N'design.notUnderstood', N'Orden no reconocida: «{0}».'),
        (N'TECHNICAL', N'es', N'design.thanks', N'Recibido.'),
        (N'TECHNICAL', N'es', N'design.cancelled', N'Orden cancelada.'),
        (N'TECHNICAL', N'es', N'design.undone', N'Deshacer: 1 paso.'),
        (N'TECHNICAL', N'es', N'design.redone', N'Rehacer: 1 paso.'),
        (N'TECHNICAL', N'es', N'design.formworkDone', N'Encofrado calculado: {0} piezas.'),
        (N'TECHNICAL', N'es', N'design.modeLinesDone', N'Modo líneas (2D) activo.'),
        (N'TECHNICAL', N'es', N'design.envChanged', N'Entorno: {0}.'),
        (N'TECHNICAL', N'es', N'design.envAlready', N'Entorno {0} ya activo.'),
        (N'TECHNICAL', N'es', N'design.envFail', N'Error al cambiar el entorno.'),
        (N'TECHNICAL', N'es', N'design.pendingAi', N'«{0}»: sin orden de IA; usar el botón de la barra.'),
        (N'TECHNICAL', N'es', N'wall.morning', N'Buenos días.'),
        (N'TECHNICAL', N'es', N'wall.afternoon', N'Buenas tardes.'),
        (N'TECHNICAL', N'es', N'wall.night', N'Buenas noches.'),
        (N'TECHNICAL', N'es', N'wall.thanks', N'Recibido.'),
        (N'TECHNICAL', N'en', N'design.drawn', N'{0} created. L={1}; dir. {2}; t={3}; h={4}; {5}.'),
        (N'TECHNICAL', N'en', N'design.deleted', N'{0} deleted. Reversible with «undo».'),
        (N'TECHNICAL', N'en', N'design.notUnderstood', N'Unrecognised command: «{0}».'),
        (N'TECHNICAL', N'en', N'design.thanks', N'Acknowledged.'),
        (N'TECHNICAL', N'en', N'design.cancelled', N'Command cancelled.'),
        (N'TECHNICAL', N'en', N'design.envChanged', N'Environment: {0}.'),
        (N'TECHNICAL', N'en', N'design.envAlready', N'Environment {0} already active.'),
        (N'TECHNICAL', N'en', N'design.pendingAi', N'«{0}»: no AI command; use the toolbar button.');

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

    PRINT N'OK — semilla de entornos: 4 entornos, 4 comandos ENV_*, 58 frases, 268 plantillas.';
END
GO
