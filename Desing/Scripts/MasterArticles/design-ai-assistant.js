/**
 * IA del diseño (US 14/11/2026, fase 1): botón «IA» de la barra de modos del visor Desing_2.
 * Abre el mismo chat que el Asistente IA del muro (wall-ai-assistant.js: burbujas, «¿Querías decir…?», saludo,
 * micrófono con Alt y modo continuo, respuestas habladas) en el mismo formato compacto (450 × 800 a la derecha,
 * sin backdrop), pero con otro intérprete: un REGISTRO DE COMANDOS de diseño (contexto 'design').
 *
 * Primeros comandos (solo 2D, solo funciones reales del visor a través de `window.maStlDesing2AiBridge`):
 * dibujar muro · cambiar largo/espesor/alto · desplazar · copiar (y copia en paralelo) · borrar (con confirmación) ·
 * alargar hasta el siguiente muro · listar / señalar muros · deshacer / rehacer · ayuda.
 * Fase 2 (14/11/2026): girar (herramienta Girar) · simetría (herramienta Simetría) · modos de la barra
 * (Líneas / 3D / Encofrar / Encofrado colores, pulsando sus botones; Encofrar pide confirmación y responde al terminar).
 * Cada orden = un solo paso de Deshacer (el del visor). Nada de BD: lo guarda el flujo normal del visor.
 *
 * Cotas por voz (pt114u, 15/11/2026): «muéstrame los id de los muros» / «identifica los muros» / «numera los muros»
 * (Cotas con solo el ID de cada muro; «oculta los id» los quita) · «acótame el muro 7» / «cota del muro 7» / «mide el
 * muro 7» (Cotas solo de ese muro; queda como muro actual) · «cámbiame la longitud a 4,5», «el espesor es 0,45»,
 * «muro 7 longitud 4,5», «cámbiame la altura a 3», «altura 2,70» (wall.edit sobre el muro actual; las cotas se rehacen solas). «muro N» = el ID de Cotas.
 *
 * Catálogo de la BD (pt107u): al abrir el panel se carga `Desing_2/AiCommands` (ai-command-catalog.js): las palabras
 * del usuario sustituyen a las listas JS de cada concepto, las frases de varias palabras se buscan antes (gana la más
 * larga), las respuestas usan las plantillas del entorno de conversación (Ejecutivo por defecto) y «modo coloquial» /
 * «ponte serio»… cambia el entorno (se guarda en BD). Sin servidor: todo como antes (vocabulario y textos JS).
 *
 * Añadir un comando: `registerDesignAiCommand({ id, score(p), run(p, s, T), params, examples, help, short })`
 * (ver MA_STL_DESIGN_AI_COMMANDS y Desing/IA/docs/ia-desing.md, «IA del diseño»).
 */
import {
    mountWallAiChat, maStlWallAiNormalize, maStlWallAiWordMatch, maStlWallAiDetectLanguage, maStlWallAiUiLang,
    maStlWallAiParseNumberWords, maStlWallAiParseMeasure, maStlWallAiYesNo, maStlWallAiCompactArea, maStlWallAiCompactRect,
} from './wall-ai-assistant.js?v=20261116a';
import {
    maStlAiCatalogLoad, maStlAiCatalogOnChange, maStlAiCatalogReplies, maStlAiCatalogPhrases, maStlAiCatalogLongest,
    maStlAiCatalogEnv, maStlAiCatalogEnvLabel, maStlAiCatalogSaveEnv, maStlAiCatalogEnvTarget, maStlAiCatalogGet, maStlAiCatalogUserRules,
    maStlAiCatalogUrls, maStlAiLogUnknownPhrase,
} from './ai-command-catalog.js?v=20261118a';

/* ---------------------------------- Textos ---------------------------------- */

export const MA_STL_DESIGN_AI_TEXT = {
    es: {
        lang: 'es',
        title: 'Asistente IA \u2013 Diseño',
        close: 'Cerrar',
        placeholder: 'Ej.: dibuja un muro de 5 m hacia la derecha',
        wall: 'muro',
        yes: 'Sí',
        no: 'No',
        undo: 'Deshacer',
        dir: { 0: 'hacia la derecha', 90: 'hacia arriba', 180: 'hacia la izquierda', 270: 'hacia abajo' },
        dirAngle: 'a {0}°',
        notReady: 'El visor aún no está listo para dibujar. Abre un diseño y vuelve a intentarlo.',
        switchedLines: 'He pasado a la vista de líneas (2D) para trabajar sobre el plano.',
        askLength: '¿De cuántos metros hago el muro? (por ejemplo «5 m» o «3,5»)',
        askDirection: '¿Hacia dónde? Derecha, izquierda, arriba, abajo o un ángulo («a 45°»).',
        askWall: '¿Qué muro? Selecciónalo en el visor o dime su número («el muro 4»).',
        askDistance: '¿Cuánto lo {0}? (por ejemplo «2 m a la derecha»)',
        askSide: '¿Hacia qué lado hago la copia en paralelo? Arriba, abajo, izquierda o derecha.',
        askWhat: '¿Qué le cambio al muro {0}? Largo, espesor o alto (por ejemplo «6 m de largo»).',
        drawn: 'He dibujado el {0}: {1} {2}, espesor {3}, alto {4}, {5}.',
        drawnMerged: 'He dibujado {1} {2} (espesor {3}, alto {4}), {5}; queda en línea con el {0}, que ahora mide {6}.',
        formworkCleared: 'El encofrado que había ya no vale y se ha quitado (como con las herramientas del visor); vuelve a encofrar con el botón Encofrado cuando termines.',
        splitNote: 'Cruza otros muros, así que queda partido en {0} tramos: {1} (y los muros cruzados también se parten, como con la herramienta Muro 2D).',
        fromOrigin: 'desde el origen',
        fromPoint: 'desde el punto {0}',
        fromEnd: 'seguido del final del {0}',
        fromStart: 'desde el inicio del {0}',
        defaultDir: 'No me has dicho la dirección: lo he hecho hacia la derecha.',
        tooShort: 'Ese largo es demasiado corto (mínimo {0}).',
        thickRange: 'El espesor tiene que estar entre {0} y {1}.',
        drawFail: 'No he podido dibujar el muro en ese sitio.',
        noWall: 'No encuentro el muro {0}. Muros del diseño: {1}.',
        noWalls: 'El diseño no tiene muros rectos todavía.',
        edited: '{0}: {1}.',
        editFail: 'No he podido cambiar {0} del {1}.',
        nothingToChange: 'El {0} ya tiene esas medidas.',
        moved: 'He movido el {0} {1} {2}.',
        copied: 'He copiado el {0} {1} {2}: la copia es el {3}.',
        copiedParallel: 'He hecho una copia en paralelo del {0} a {1} {2}: la copia es el {3}.',
        moveFail: 'No he podido mover el {0}.',
        copyFail: 'No he podido copiar el {0}.',
        confirmDelete: '¿Borro el {0} ({1})?',
        deleted: 'He borrado el {0}. Si te has equivocado, dime «deshaz».',
        deleteCancel: 'Vale, no lo borro.',
        deleteFail: 'No he podido borrar el {0}.',
        extended: 'He alargado el {0} {1} hasta el siguiente muro: ahora mide {2}.',
        extendEnd: { p1: 'por el inicio', p2: 'por el final' },
        extendFail: 'No hay ningún muro en la prolongación del {0} (o ya está unido por ese lado).',
        extendNotTarget: 'Alargando el {0} el primer muro que encuentro no es el {1}, así que no lo he cambiado.',
        undone: 'He deshecho el último paso.',
        redone: 'He rehecho el paso.',
        nothingUndo: 'No hay nada que deshacer.',
        nothingRedo: 'No hay nada que rehacer.',
        listTitle: 'Muros del diseño: {0}.',
        shown: 'Te marco el {0}: {1}.',
        wallDims: 'largo {0}, espesor {1}, alto {2}',
        notAvailable: '«{0}» todavía no existe en el visor, así que no lo puedo hacer. Lo dejo apuntado como propuesta. Escribe «ayuda» para ver lo que sí sé hacer.',
        didYouMean: '¿Querías decir «{0}»?',
        notUnderstood: 'No he entendido «{0}».',
        helpHead: 'Puedo trabajar sobre el plano 2D y cambiar de modo:',
        helpTail: 'Cada orden se deshace de una en una («deshaz»). Cada muro tiene su número; «lista los muros» te los enseña.',
        helpSpeech: 'Te dejo los ejemplos en pantalla.',
        thanks: 'De nada.',
        cancelled: 'Vale, lo dejo.',
        cols: { wall: 'Muro', length: 'Largo', thickness: 'Espesor', height: 'Alto' },
        copy: 'Copiar', copied2: 'Copiado', copyFail2: 'No se pudo copiar',
        names: { length: 'largo', thickness: 'espesor', height: 'alto' },
        verbsMove: 'muevo', verbsCopy: 'copio',
        askAngle: '¿Cuántos grados lo giro? (por ejemplo «90 grados» o «45° a la derecha»; + = antihorario)',
        rotated: 'He girado el {0} {1} en sentido {2} {3}.',
        rotatedCopy: 'He girado una copia del {0} {1} en sentido {2} {3}: la copia es el {4}.',
        rotateZero: 'Girar 0° (o una vuelta entera) lo deja igual: no he cambiado nada.',
        rotateFail: 'No he podido girar el {0}.',
        ccw: 'antihorario', cw: 'horario',
        pivotCenter: 'sobre su centro', pivotStart: 'sobre su inicio', pivotEnd: 'sobre su final', pivotOrigin: 'sobre el origen', pivotPoint: 'sobre el punto {0}',
        byCenter: 'por su centro', byStart: 'por su inicio', byEnd: 'por su final', byOrigin: 'por el origen', byPoint: 'por el punto {0}',
        pivotDefaultNote: 'Por defecto giro sobre el centro del muro; di «sobre su inicio» o «sobre su final» si lo quieres desde otro punto.',
        askMirrorAxis: '¿Respecto a qué hago la simetría del {0}? Por ejemplo «respecto al eje Y», «respecto al muro 5» o «vertical por su extremo».',
        askMirrorErase: 'Simetría del {0} {1}. ¿Borro el original? (Sí = solo queda el reflejado; No = quedan los dos)',
        eraseYes: 'Sí, bórralo', eraseNo: 'No, mantenlo',
        mirroredKeep: 'He hecho la simetría del {0} {1} manteniendo el original: el reflejado es el {2}.',
        mirroredMove: 'He hecho la simetría del {0} {1} y he borrado el original: queda el {2}.',
        mirrorSame: 'El {0} está sobre la recta de simetría ({1}): el reflejo sería el mismo muro, no he cambiado nada.',
        mirrorFail: 'No he podido hacer la simetría del {0}.',
        axisX: 'respecto al eje X', axisY: 'respecto al eje Y', axisWall: 'respecto al {0}',
        axisVertical: 'respecto a la vertical {0}', axisHorizontal: 'respecto a la horizontal {0}', axisAngle: 'respecto a la recta a {1} {0}',
        modeNames: { lines: 'líneas (2D)', wall3d: '3D', formwork: 'encofrar', formworkColor: 'encofrado colores' },
        modeAlready: 'Ya estás en el modo {0}.',
        modeBusy: 'El visor está calculando (3D o encofrado); espera a que termine y vuelve a pedírmelo.',
        modeLinesDone: 'He pasado al modo líneas (2D), como con el botón Líneas.',
        modeFail: 'No he podido pasar al modo {0}.',
        mode3dDone: 'He generado el 3D, como con el botón 3D: {0} piezas.',
        mode3dDoneCached: 'He pasado al 3D, como con el botón 3D (el modelo ya estaba calculado).',
        mode3dFromFormwork: 'El encofrado deja de verse (el botón 3D enseña el modelo sin paneles); di «encofra» para volver a calcularlo.',
        mode3dFail: 'No he podido generar el 3D: {0}',
        confirmFormwork: 'Encofrar calcula el encofrado de todo el diseño ({0} muros) y puede tardar un rato. ¿Lo hago?',
        formworkAlready: 'Ya estás en el modo encofrar (el diseño está encofrado). ¿Lo vuelvo a calcular para todo el diseño?',
        formworkRunning: 'Encofrando todo el diseño…',
        formworkDone: 'Encofrado terminado: {0} piezas en el visor.',
        formworkFail: 'No se ha podido encofrar: {0}',
        formworkCancelled: 'No se ha encofrado (no se eligió la fase o se canceló).',
        formworkNo: 'Vale, no encofro.',
        confirmFormworkColor: 'Encofrado colores calcula el encofrado de todo el diseño ({0} muros) y pinta cada panel según su ancho. ¿Lo hago?',
        formworkColorAlready: 'Ya estás en encofrado colores. ¿Lo vuelvo a calcular para todo el diseño?',
        formworkColorRunning: 'Encofrando en colores…',
        formworkColorDone: 'Encofrado colores terminado: {0} piezas. Cada panel lleva el color de su ancho.',
        formworkColorFail: 'No se ha podido encofrar en colores: {0}',
        formworkColorCancelled: 'No se ha encofrado en colores (no se eligió la fase o se canceló).',
        formworkColorNo: 'Vale, no encofro en colores.',
        envChanged: 'Entorno de conversación: {0}.',
        envAlready: 'El entorno {0} ya está activo.',
        envFail: 'No he podido cambiar el entorno de conversación. Inténtalo más tarde.',
        envBadge: 'Entorno de conversación: {0}',
        pendingAi: '«{0}» aún no se puede pedir a la IA; usa el botón de la barra.',
        toolDone: 'He pulsado «{0}», igual que el botón.',
        toolMissing: 'No encuentro «{0}» en el visor.',
        confirmTool: '¿Ejecuto «{0}»?',
        toolNo: 'Vale, no lo hago.',
        aiOpened: 'Asistente abierto.',
        aiAlready: 'El asistente ya está abierto.',
        aiClosed: 'Cierro el asistente.',
        proposals: { fillet: 'Empalme / chaflán', trim: 'Recortar (activa la herramienta; el clic en el tramo lo haces tú)' },
        idsShown: 'Te pongo el número de cada muro en el plano ({0} muros: {1}). Para ver las medidas de uno, dime «acótame el muro {2}».',
        idsAlready: 'Los números ya están en el plano ({0} muros: {1}). Di «oculta los id» para quitarlos.',
        idsHidden: 'He quitado los números de los muros.',
        idsNotShown: 'Los números de los muros no estaban puestos.',
        dimsShown: 'Te acoto el {0}: {1}. Dime qué le cambio, por ejemplo «longitud 4,5», «espesor 0,45» o «altura 3».',
        dimsAll: 'He puesto las cotas de todo el diseño. Para un solo muro, dime «acótame el muro {0}».',
        dimsPick: 'Las cotas son las del muro seleccionado y no hay ninguno: te pongo el número de cada muro ({1}). Dime «acótame el muro {0}», «selecciona el muro {0}» o «acota todo».',
        articleIdsPick: 'Los ID de los paneles se ven en el muro seleccionado y no hay ninguno. Dime «selecciona el muro {0}» o «acota todo».',
        dimsHidden: 'He quitado las cotas.',
        dimsNone: 'No había cotas puestas.',
        cotasFail: 'No he podido poner las cotas en el visor.',
        articleIdsShown: 'Te pongo el ID de cada panel junto a su cota ({0} paneles en {1} muros: {2}). Así podrás referirte a uno, por ejemplo «el panel {3}».',
        articleIdsAlready: 'Los ID de los paneles ya están junto a sus cotas ({0} paneles: {1}). Di «oculta los id de los paneles» para quitarlos.',
        articleIdsHidden: 'He quitado los ID de los paneles.',
        articleIdsNotShown: 'Los ID de los paneles no estaban puestos.',
        articleIdsNoFormwork: 'Los ID de artículo van en las cotas de los paneles y aún no hay encofrado. Di «encofra» y te los pongo.',
        learnEmpty: 'No hay frases pendientes de diseño.',
        learnOtherPending: 'Hay frases de otro asistente; esta revisión es solo del diseño.',
        learnFail: 'No he podido leer las frases pendientes.',
        learnIntro: 'Hay {0} pendiente(s). Las pregunto una a una. {1}',
        learnScopeAll: 'Lo que aceptes queda para todos.',
        learnScopeUser: 'Lo que aceptes queda solo para ti.',
        learnAsk: 'No entendí «{0}» ({1}/{2}).',
        learnGuess: 'Se parece a «{0}». ¿Lo asigno a ese comando?',
        learnWhich: '¿A qué comando lo asigno? Dime el nombre (por ejemplo «Cotas»), «descarta» o «siguiente».',
        learnUnknownCmd: 'No conozco «{0}» como comando del visor. Dime otro, o «descarta».',
        learnMany: 'Hay varios: {0}. Dime uno.',
        learnOther: '«{0}» ejecutará «{1}». ¿Cómo se dice en {2}? Si no hay otra forma, di «no». No la invento.',
        learnTypeOther: 'Escríbela. Si no hay otra forma, di «no».',
        learnSaved: 'Guardado{0}. La próxima vez «{1}» ejecutará «{2}», también si cambia una letra.',
        learnScopeSavedAll: ' para todos',
        learnScopeSavedUser: ' solo para ti',
        learnConflict: 'Esa frase ya pertenece a «{0}». Dime otro comando o «descarta».',
        learnUnavailable: '«{0}» no está en el visor. Dime otro comando.',
        learnDiscarded: 'Descartada.',
        learnSkipped: 'La dejo pendiente.',
        learnDone: 'No quedan más frases pendientes.',
        learnStopped: 'Paro la revisión. Lo que no hemos mirado sigue pendiente.',
        learnSaveFail: 'No he podido guardar la frase: {0}',
        learnDiscardBtn: 'Descartar',
        learnSkipBtn: 'Siguiente',
        learnStopBtn: 'Parar',
    },
    en: {
        lang: 'en',
        title: 'AI Assistant \u2013 Design',
        close: 'Close',
        placeholder: 'E.g.: draw a 5 m wall to the right',
        wall: 'wall',
        yes: 'Yes',
        no: 'No',
        undo: 'Undo',
        dir: { 0: 'to the right', 90: 'upwards', 180: 'to the left', 270: 'downwards' },
        dirAngle: 'at {0}°',
        notReady: 'The viewer is not ready yet. Open a design and try again.',
        switchedLines: 'I switched to the lines (2D) view to work on the plan.',
        askLength: 'How long should the wall be? (e.g. «5 m» or «3.5»)',
        askDirection: 'Which way? Right, left, up, down or an angle («at 45°»).',
        askWall: 'Which wall? Select it in the viewer or tell me its number («wall 4»).',
        askDistance: 'How far do I {0} it? (e.g. «2 m to the right»)',
        askSide: 'Which side for the parallel copy? Up, down, left or right.',
        askWhat: 'What should I change on wall {0}? Length, thickness or height (e.g. «6 m long»).',
        drawn: 'I drew {0}: {1} {2}, thickness {3}, height {4}, {5}.',
        drawnMerged: 'I drew {1} {2} (thickness {3}, height {4}), {5}; it is in line with {0}, which is now {6} long.',
        formworkCleared: 'The existing formwork no longer fits and has been removed (as with the viewer tools); run Formwork again when you are done.',
        splitNote: 'It crosses other walls, so it is split into {0} pieces: {1} (the crossed walls are split too, as with the 2D Wall tool).',
        fromOrigin: 'from the origin',
        fromPoint: 'from point {0}',
        fromEnd: 'continuing from the end of {0}',
        fromStart: 'from the start of {0}',
        defaultDir: 'You did not give a direction, so I drew it to the right.',
        tooShort: 'That length is too short (minimum {0}).',
        thickRange: 'Thickness must be between {0} and {1}.',
        drawFail: 'I could not draw the wall there.',
        noWall: 'I cannot find wall {0}. Walls in the design: {1}.',
        noWalls: 'The design has no straight walls yet.',
        edited: '{0}: {1}.',
        editFail: 'I could not change {0} of {1}.',
        nothingToChange: '{0} already has those sizes.',
        moved: 'I moved {0} {1} {2}.',
        copied: 'I copied {0} {1} {2}: the copy is {3}.',
        copiedParallel: 'I made a parallel copy of {0} at {1} {2}: the copy is {3}.',
        moveFail: 'I could not move {0}.',
        copyFail: 'I could not copy {0}.',
        confirmDelete: 'Shall I delete {0} ({1})?',
        deleted: 'I deleted {0}. If it was a mistake, say «undo».',
        deleteCancel: 'OK, I will not delete it.',
        deleteFail: 'I could not delete {0}.',
        extended: 'I extended {0} {1} up to the next wall: it is now {2} long.',
        extendEnd: { p1: 'at its start', p2: 'at its end' },
        extendFail: 'There is no wall in line with {0} (or it is already joined on that side).',
        extendNotTarget: 'Extending {0}, the first wall it meets is not {1}, so I left it as it was.',
        undone: 'I undid the last step.',
        redone: 'I redid the step.',
        nothingUndo: 'There is nothing to undo.',
        nothingRedo: 'There is nothing to redo.',
        listTitle: 'Walls in the design: {0}.',
        shown: 'Here is {0}: {1}.',
        wallDims: 'length {0}, thickness {1}, height {2}',
        notAvailable: '«{0}» does not exist in the viewer yet, so I cannot do it. I have noted it as a proposal. Type «help» to see what I can do.',
        didYouMean: 'Did you mean «{0}»?',
        notUnderstood: 'I did not understand «{0}».',
        helpHead: 'I can work on the 2D plan and switch modes:',
        helpTail: 'Every order can be undone one by one («undo»). Each wall has a number; «list the walls» shows them.',
        helpSpeech: 'The examples are on screen.',
        thanks: 'You\'re welcome.',
        cancelled: 'OK, I will leave it.',
        cols: { wall: 'Wall', length: 'Length', thickness: 'Thickness', height: 'Height' },
        copy: 'Copy', copied2: 'Copied', copyFail2: 'Could not copy',
        names: { length: 'length', thickness: 'thickness', height: 'height' },
        verbsMove: 'move', verbsCopy: 'copy',
        askAngle: 'How many degrees? (e.g. «90 degrees» or «45° clockwise»; + = counterclockwise)',
        rotated: 'I rotated {0} {1} {2} {3}.',
        rotatedCopy: 'I rotated a copy of {0} {1} {2} {3}: the copy is {4}.',
        rotateZero: 'Rotating 0° (or a full turn) leaves it as it is: nothing changed.',
        rotateFail: 'I could not rotate {0}.',
        ccw: 'counterclockwise', cw: 'clockwise',
        pivotCenter: 'about its centre', pivotStart: 'about its start', pivotEnd: 'about its end', pivotOrigin: 'about the origin', pivotPoint: 'about point {0}',
        byCenter: 'through its centre', byStart: 'through its start', byEnd: 'through its end', byOrigin: 'through the origin', byPoint: 'through point {0}',
        pivotDefaultNote: 'By default I rotate about the centre of the wall; say «about its start» or «about its end» for another point.',
        askMirrorAxis: 'Mirror {0} about what? E.g. «about the Y axis», «about wall 5» or «vertical through its end».',
        askMirrorErase: 'Mirror {0} {1}. Erase the original? (Yes = only the mirrored one remains; No = both remain)',
        eraseYes: 'Yes, erase it', eraseNo: 'No, keep it',
        mirroredKeep: 'I mirrored {0} {1} keeping the original: the mirrored wall is {2}.',
        mirroredMove: 'I mirrored {0} {1} and erased the original: it is now {2}.',
        mirrorSame: '{0} lies on the mirror line ({1}): the mirror image would be the same wall, nothing changed.',
        mirrorFail: 'I could not mirror {0}.',
        axisX: 'about the X axis', axisY: 'about the Y axis', axisWall: 'about {0}',
        axisVertical: 'about the vertical {0}', axisHorizontal: 'about the horizontal {0}', axisAngle: 'about the line at {1} {0}',
        modeNames: { lines: 'lines (2D)', wall3d: '3D', formwork: 'formwork', formworkColor: 'color formwork' },
        modeAlready: 'You are already in {0} mode.',
        modeBusy: 'The viewer is busy (3D or formwork); wait until it finishes and ask me again.',
        modeLinesDone: 'I switched to lines (2D) mode, as with the Lines button.',
        modeFail: 'I could not switch to {0} mode.',
        mode3dDone: 'I generated the 3D model, as with the 3D button: {0} pieces.',
        mode3dDoneCached: 'I switched to 3D, as with the 3D button (the model was already calculated).',
        mode3dFromFormwork: 'The formwork is no longer shown (the 3D button shows the model without panels); say «formwork» to calculate it again.',
        mode3dFail: 'I could not generate the 3D model: {0}',
        confirmFormwork: 'Formwork calculates the whole design ({0} walls) and may take a while. Shall I go ahead?',
        formworkAlready: 'You are already in formwork mode (the design has formwork). Shall I calculate it again for the whole design?',
        formworkRunning: 'Calculating the formwork of the whole design…',
        formworkDone: 'Formwork finished: {0} pieces in the viewer.',
        formworkFail: 'The formwork could not be calculated: {0}',
        formworkCancelled: 'No formwork was calculated (no phase was chosen or it was cancelled).',
        formworkNo: 'OK, no formwork.',
        confirmFormworkColor: 'Color formwork calculates the whole design ({0} walls) and paints each panel by its width. Shall I go ahead?',
        formworkColorAlready: 'You are already in color formwork. Shall I calculate it again for the whole design?',
        formworkColorRunning: 'Calculating the color formwork…',
        formworkColorDone: 'Color formwork finished: {0} pieces. Each panel uses the color of its width.',
        formworkColorFail: 'The color formwork could not be calculated: {0}',
        formworkColorCancelled: 'No color formwork was calculated (no phase was chosen or it was cancelled).',
        formworkColorNo: 'OK, no color formwork.',
        envChanged: 'Conversation environment: {0}.',
        envAlready: 'The {0} environment is already active.',
        envFail: 'The conversation environment could not be changed. Please try again later.',
        envBadge: 'Conversation environment: {0}',
        pendingAi: '«{0}» cannot be requested from the AI yet; use the toolbar button.',
        toolDone: 'I pressed «{0}», the same as the button.',
        toolMissing: 'I cannot find «{0}» in the viewer.',
        confirmTool: 'Shall I run «{0}»?',
        toolNo: 'OK, I will not do it.',
        aiOpened: 'Assistant open.',
        aiAlready: 'The assistant is already open.',
        aiClosed: 'Closing the assistant.',
        proposals: { fillet: 'Fillet / chamfer', trim: 'Trim (starts the tool; you still click the piece)' },
        idsShown: 'The number of each wall is now on the plan ({0} walls: {1}). To see the sizes of one, say «dimension wall {2}».',
        idsAlready: 'The numbers are already on the plan ({0} walls: {1}). Say «hide the ids» to remove them.',
        idsHidden: 'I removed the wall numbers.',
        idsNotShown: 'The wall numbers were not shown.',
        dimsShown: 'Dimensions of {0}: {1}. Tell me what to change, e.g. «length 4.5», «thickness 0.45» or «height 3».',
        dimsAll: 'I turned on the dimensions of the whole design. For one wall, say «dimension wall {0}».',
        dimsPick: 'Dimensions are shown for the selected wall and none is selected: here are the wall numbers ({1}). Say «dimension wall {0}», «select wall {0}» or «dimension everything».',
        articleIdsPick: 'Panel IDs are shown on the selected wall and none is selected. Say «select wall {0}» or «dimension everything».',
        dimsHidden: 'I removed the dimensions.',
        dimsNone: 'There were no dimensions shown.',
        cotasFail: 'I could not show the dimensions in the viewer.',
        articleIdsShown: 'Each panel now shows its ID next to its dimension ({0} panels on {1} walls: {2}). You can refer to one, e.g. «panel {3}».',
        articleIdsAlready: 'The panel IDs are already shown ({0} panels: {1}). Say «hide the panel ids» to remove them.',
        articleIdsHidden: 'Panel IDs removed.',
        articleIdsNotShown: 'The panel IDs were not shown.',
        articleIdsNoFormwork: 'Article IDs go on the panel dimensions and there is no formwork yet. Say «formwork» and I will show them.',
        learnEmpty: 'There are no pending design phrases.',
        learnOtherPending: 'There are phrases from another assistant; this review is only for design.',
        learnFail: 'I could not read the pending phrases.',
        learnIntro: 'There are {0} pending. I will ask them one by one. {1}',
        learnScopeAll: 'What you accept becomes the default for everyone.',
        learnScopeUser: 'What you accept is saved only for you.',
        learnAsk: 'I did not understand «{0}» ({1}/{2}).',
        learnGuess: 'It looks like «{0}». Shall I assign it to that command?',
        learnWhich: 'Which command should it run? Tell me the name (for example «Dimensions»), «discard» or «skip».',
        learnUnknownCmd: 'I do not know «{0}» as a viewer command. Tell me another, or «discard».',
        learnMany: 'There are several: {0}. Tell me one.',
        learnOther: '«{0}» will run «{1}». How is it said in {2}? If there is no other form, say «no». I will not invent it.',
        learnTypeOther: 'Type it. If there is no other form, say «no».',
        learnSaved: 'Saved{0}. Next time «{1}» will run «{2}», even if one letter changes.',
        learnScopeSavedAll: ' for everyone',
        learnScopeSavedUser: ' only for you',
        learnConflict: 'That phrase already belongs to «{0}». Tell me another command or «discard».',
        learnUnavailable: '«{0}» is not in the viewer. Tell me another command.',
        learnDiscarded: 'Discarded.',
        learnSkipped: 'I will leave it pending.',
        learnDone: 'There are no more pending phrases.',
        learnStopped: 'I am stopping the review. What we have not looked at stays pending.',
        learnSaveFail: 'I could not save the phrase: {0}',
        learnDiscardBtn: 'Discard',
        learnSkipBtn: 'Skip',
        learnStopBtn: 'Stop',
    },
};

const TEXT_MERGED = {};

/**
 * Textos del idioma con las plantillas del entorno de conversación (catálogo de la BD, claves `design.*`) encima;
 * sin catálogo, los del JS. Solo se sustituyen textos (no listas ni objetos).
 */
function texts(lang) {
    const L = lang === 'en' ? 'en' : 'es';
    const base = MA_STL_DESIGN_AI_TEXT[L];
    const r = maStlAiCatalogReplies(L);
    const keys = Object.keys(r).filter(function (k) { return k.indexOf('design.') === 0 && typeof r[k] === 'string' && r[k]; });
    if (!keys.length) return base;
    const memo = TEXT_MERGED[L];
    if (memo && memo.src === r && memo.base === base) return memo.out;
    const out = Object.assign({}, base);
    keys.forEach(function (k) {
        const jk = k.slice(7);
        if (base[jk] === undefined || typeof base[jk] === 'string') out[jk] = r[k];
    });
    TEXT_MERGED[L] = { src: r, base: base, out: out };
    return out;
}

/** Textos efectivos (pruebas y otras partes del visor). */
export function maStlDesignAiTexts(lang) { return texts(lang); }

function fmt(tpl) {
    let out = String(tpl == null ? '' : tpl);
    for (let i = 1; i < arguments.length; i++) out = out.split('{' + (i - 1) + '}').join(String(arguments[i]));
    return out;
}

/** «5,00 m» / «5.00 m». */
export function maStlDesignAiFmtM(mm, lang) {
    const v = (Number(mm) || 0) / 1000;
    const s = v.toFixed(2);
    return (lang === 'en' ? s : s.replace('.', ',')) + ' m';
}

function wallName(w, T) { return T.wall + ' ' + (w && w.label != null ? w.label : '?'); }

/* ------------------------------ Lectura del texto ------------------------------ */

const UNIT_WORDS = {
    m: ['m', 'mt', 'mts', 'mtrs', 'metro', 'metros', 'meter', 'meters', 'metre', 'metres'],
    cm: ['cm', 'cms', 'centimetro', 'centimetros', 'centimeter', 'centimeters', 'centimetre', 'centimetres'],
    mm: ['mm', 'milimetro', 'milimetros', 'millimeter', 'millimeters', 'millimetre', 'millimetres'],
    deg: ['grado', 'grados', 'deg', 'degree', 'degrees', '\u00b0'],
};
const NUM_JOIN = ['y', 'and', 'coma', 'punto', 'point', 'dot', 'con'];
const NUM_WORDS = new Set(['cero', 'uno', 'dos', 'tres', 'cuatro', 'cinco', 'seis', 'siete', 'ocho', 'nueve', 'diez', 'once', 'doce',
    'trece', 'catorce', 'quince', 'dieciseis', 'diecisiete', 'dieciocho', 'diecinueve', 'veinte', 'veintiuno', 'veintidos',
    'veintitres', 'veinticuatro', 'veinticinco', 'veintiseis', 'veintisiete', 'veintiocho', 'veintinueve', 'treinta', 'cuarenta',
    'cincuenta', 'sesenta', 'setenta', 'ochenta', 'noventa', 'cien', 'ciento', 'doscientos', 'trescientos', 'cuatrocientos',
    'quinientos', 'seiscientos', 'setecientos', 'ochocientos', 'novecientos', 'mil', 'medio',
    'zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'eleven', 'twelve', 'thirteen',
    'fourteen', 'fifteen', 'sixteen', 'seventeen', 'eighteen', 'nineteen', 'twenty', 'thirty', 'forty', 'fifty', 'sixty',
    'seventy', 'eighty', 'ninety', 'hundred', 'thousand', 'half']);
const SMALL_NUM = /^(cero|uno|dos|tres|cuatro|cinco|seis|siete|ocho|nueve|zero|one|two|three|four|five|six|seven|eight|nine)$/;
const ABBR = { q: 'que', k: 'que', x: 'por', pa: 'para', xa: 'para', d: 'de', dl: 'del', porfa: 'por favor', xfa: 'por favor', pls: 'please' };

function unitOf(w) {
    const keys = Object.keys(UNIT_WORDS);
    for (let i = 0; i < keys.length; i++) if (UNIT_WORDS[keys[i]].indexOf(w) >= 0) return keys[i];
    return null;
}

let VOCAB_SET = null;
/** Palabras de los comandos: nunca son números con faltas («mueve» no es «nueve», «right» no es «eight»). */
function vocabHas(w) {
    if (!VOCAB_SET) {
        VOCAB_SET = new Set();
        Object.keys(MA_STL_DESIGN_AI_WORDS).forEach(function (k) {
            MA_STL_DESIGN_AI_WORDS[k].forEach(function (x) { VOCAB_SET.add(maStlWallAiNormalize(x)); });
        });
    }
    return VOCAB_SET.has(w);
}

function isNumWord(w) {
    if (NUM_WORDS.has(w)) return true;
    return w.length >= 5 && !vocabHas(w) && !unitOf(w) && maStlWallAiParseNumberWords(w) != null;
}

/** «un/una/a» solo es número delante de «metro»/«medio» («un metro veinte», «a meter»); si no, es artículo. */
function articleAsNumber(raw, i) {
    const w = raw[i] && raw[i].t === 'w' ? raw[i].w : '';
    if (w !== 'un' && w !== 'una' && w !== 'a') return false;
    const nx = raw[i + 1];
    return !!(nx && nx.t === 'w' && (unitOf(nx.w) === 'm' || nx.w === 'medio' || nx.w === 'half'));
}

/**
 * Tokens del mensaje: { t: 'w', w } palabras normalizadas (sin tildes) · { t: 'n', v, unit?, dec } números
 * (cifras «3,5» «3.5» «3'5», «3 coma 5», o en palabras «tres coma cinco», «dos setenta», «un metro veinte») ·
 * { t: 'xy', x, z } («2;3», coordenadas como la herramienta Línea).
 */
export function maStlDesignAiLex(text) {
    let s = String(text || '').toLowerCase();
    try { s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (_e) { /* sin normalize */ }
    s = s.replace(/[º°]/g, ' \u00b0 ').replace(/[’`´]/g, "'");
    s = s.replace(/(\d)\s+(?:coma|punto|point|dot)\s+(\d)/g, '$1.$2');
    const raw = [];
    const re = /(-?\d+(?:[.,']\d+)?)\s*;\s*(-?\d+(?:[.,']\d+)?)|-?\d+(?:[.,']\d+)?|[a-zñ]+|\u00b0|#/g;
    let m;
    while ((m = re.exec(s))) {
        if (m[1] != null) {
            raw.push({ t: 'xy', x: Number(m[1].replace(/[,']/g, '.')), z: Number(m[2].replace(/[,']/g, '.')) });
        } else if (/^-?\d/.test(m[0])) {
            raw.push({ t: 'n', v: Number(m[0].replace(/[,']/g, '.')), raw: m[0], digits: true, dec: /[.,']/.test(m[0]) });
        } else if (ABBR[m[0]]) {
            ABBR[m[0]].split(' ').forEach(function (x) { raw.push({ t: 'w', w: x }); });
        } else {
            raw.push({ t: 'w', w: m[0] === '#' ? 'numero' : m[0] });
        }
    }
    const out = [];
    for (let i = 0; i < raw.length; i++) {
        const tk = raw[i];
        if (!(tk.t === 'w' && (isNumWord(tk.w) || articleAsNumber(raw, i)))) { out.push(tk); continue; }
        let j = i;
        const words = [];
        for (;;) {
            const cur = raw[j];
            if (!cur || cur.t !== 'w') break;
            const nx = raw[j + 1];
            const nxNum = !!(nx && nx.t === 'w' && isNumWord(nx.w));
            const ok = isNumWord(cur.w) || (j === i && articleAsNumber(raw, j))
                || (words.length && NUM_JOIN.indexOf(cur.w) >= 0 && nxNum)
                || (words.length && unitOf(cur.w) === 'm' && nxNum);
            if (!ok) break;
            words.push(cur.w);
            j++;
        }
        const txt = words.join(' ');
        let v = null;
        let dec = false;
        if (/\b(coma|punto|point|dot)\b/.test(txt) || (/^(un|una|a|uno|one)\b/.test(txt) && /\b(metro|meter|metre)\b/.test(txt))) {
            const ms = maStlWallAiParseMeasure(txt);
            if (ms) { v = ms.m; dec = true; }
        } else if (words.length >= 2 && SMALL_NUM.test(words[0]) && !/^(y|and|mil|hundred|thousand)$/.test(words[1])) {
            const ms = maStlWallAiParseMeasure(txt);
            if (ms) { v = ms.m; dec = true; }
        } else if (/^((un|una|a) )?(medio|half)$/.test(txt)) {
            v = 0.5; dec = true;
        }
        if (v == null && /^(.+) (y|and a|and) (medio|half)$/.test(txt)) {
            const base = maStlWallAiParseNumberWords(txt.replace(/ (y|and a|and) (medio|half)$/, '').replace(/^(un|una|a) /, 'uno '));
            if (base != null) { v = base + 0.5; dec = true; }
        }
        if (v == null) v = maStlWallAiParseNumberWords(txt.replace(/^(un|una|a) /, 'uno '));
        if (v == null) { for (let k = i; k < j; k++) out.push(raw[k]); i = j - 1; continue; }
        out.push({ t: 'n', v: v, raw: txt, words: true, dec: dec });
        i = j - 1;
    }
    /* Unidad pegada («5 m», «30 cm», «45 °») y «2 70» dictado (metro y centímetros) → 2,70. */
    const res = [];
    for (let i = 0; i < out.length; i++) {
        const tk = out[i];
        if (tk.t === 'n') {
            const nx = out[i + 1];
            const prev = res[res.length - 1];
            if (nx && nx.t === 'n' && tk.digits && nx.digits && !tk.dec && !nx.dec && tk.v >= 0 && tk.v <= 9 && /^\d{2}$/.test(nx.raw)
                && !(prev && prev.t === 'w' && /^(muro|muros|pared|wall|numero|number|el|the)$/.test(prev.w))) {
                tk.v = tk.v + Number(nx.raw) / 100;
                tk.dec = true;
                i++;
            }
            const after = out[i + 1];
            if (after && after.t === 'w' && unitOf(after.w)) { tk.unit = unitOf(after.w); i++; }
            /* «tres metros y medio», «two meters and a half». */
            const y = out[i + 1];
            const half = out[i + 2] && out[i + 2].t === 'n' && out[i + 2].v === 0.5 && out[i + 2].words ? out[i + 2]
                : (out[i + 3] && out[i + 3].t === 'n' && out[i + 3].v === 0.5 && out[i + 3].words && out[i + 2] && out[i + 2].t === 'w' && out[i + 2].w === 'a' ? out[i + 3] : null);
            /* «tres metros con veinte» → 3,20. */
            const cn = out[i + 2];
            if (tk.unit === 'm' && !tk.dec && y && y.t === 'w' && y.w === 'con' && cn && cn.t === 'n' && !cn.dec && !cn.unit && cn.v > 0 && cn.v < 100) {
                tk.v += cn.v / 100;
                tk.dec = true;
                i += 2;
            } else if (tk.unit && y && y.t === 'w' && (y.w === 'y' || y.w === 'and') && half) {
                tk.v += 0.5;
                tk.dec = true;
                i += half === out[i + 2] ? 2 : 3;
            }
        }
        res.push(tk);
    }
    return res;
}

/* Vocabulario de comandos (español + inglés). Las palabras se comparan con faltas (maStlWallAiWordMatch). */
export const MA_STL_DESIGN_AI_WORDS = {
    draw: ['dibuja', 'dibujar', 'dibujame', 'traza', 'trazar', 'crea', 'crear', 'creame', 'añade', 'anade', 'agrega',
        'agregar', 'inserta', 'insertar', 'nuevo', 'nueva', 'levanta', 'construye', 'draw', 'create', 'add', 'insert', 'new', 'build'],
    make: ['haz', 'hazme', 'hacer', 'pon', 'ponme', 'poner', 'make', 'put', 'otro', 'otra', 'another'],
    wall: ['muro', 'muros', 'pared', 'paredes', 'tabique', 'wall', 'walls'],
    move: ['mueve', 'mover', 'muevelo', 'desplaza', 'desplazar', 'desplazalo', 'traslada', 'trasladar', 'trasladalo', 'move', 'shift'],
    copy: ['copia', 'copiar', 'copialo', 'duplica', 'duplicar', 'duplicalo', 'copy', 'duplicate', 'clone'],
    parallel: ['paralelo', 'paralela', 'offset', 'equidistancia', 'equidistante', 'parallel'],
    del: ['borra', 'borrar', 'borralo', 'elimina', 'eliminar', 'eliminalo', 'quita', 'quitar', 'quitalo', 'suprime', 'suprimir', 'delete', 'remove', 'erase'],
    rotate: ['gira', 'girar', 'giralo', 'girale', 'rota', 'rotar', 'rotalo', 'voltea', 'rotacion', 'rotate', 'turn', 'spin', 'rotation'],
    mirror: ['simetria', 'simetrica', 'simetrico', 'espejo', 'refleja', 'reflejar', 'reflejalo', 'mirror', 'reflect', 'flip'],
    trim: ['recorta', 'recortar', 'recortalo', 'corta', 'cortar', 'trim', 'cut'],
    fillet: ['empalme', 'chaflan', 'redondea', 'fillet', 'chamfer'],
    extend: ['alarga', 'alargar', 'alargalo', 'prolonga', 'prolongar', 'prolongalo', 'extiende', 'extender', 'extiendelo', 'estira', 'estirar', 'extend', 'lengthen', 'stretch'],
    shorten: ['acorta', 'acortar', 'acortalo', 'reduce', 'shorten'],
    change: ['cambia', 'cambiar', 'cambiale', 'cambialo', 'cambiame', 'cambiamelo', 'modifica', 'modificar', 'modificalo', 'ajusta', 'ajustar', 'ponle', 'dale', 'dejale', 'dejalo', 'change', 'modify', 'set', 'adjust', 'resize'],
    undo: ['deshaz', 'deshacer', 'deshazlo', 'undo', 'revierte', 'revert'],
    redo: ['rehaz', 'rehacer', 'redo'],
    list: ['lista', 'listar', 'listado', 'enumera', 'cuantos', 'cuales', 'list', 'many'],
    show: ['selecciona', 'seleccionar', 'seleccionalo', 'marca', 'marcame', 'resalta', 'señala', 'senala', 'muestra', 'muestrame', 'enseña', 'ensename', 'donde',
        'select', 'highlight', 'show', 'where', 'find'],
    help: ['ayuda', 'help', 'comandos', 'commands', 'ordenes'],
    thanks: ['gracias', 'thanks', 'thank', 'thx'],
    greet: ['hola', 'buenas', 'buenos', 'hello', 'hi', 'hey'],
    cancel: ['cancela', 'cancelar', 'olvidalo', 'cancel', 'nevermind'],
    length: ['largo', 'larga', 'longitud', 'longuitud', 'lonjitud', 'largura', 'long', 'length'],
    /* «esperoser», «espersor», «expesor»…: faltas de dictado y de teclado que se oyen en obra (pt114u). */
    thickness: ['espesor', 'grosor', 'ancho', 'anchura', 'grueso', 'esperoser', 'espesoro', 'espersor', 'expesor', 'espresor', 'esperor',
        'thickness', 'thick', 'width', 'wide'],
    /* «atura», «altrua»…: faltas de dictado y de teclado (pt114u). */
    height: ['alto', 'alta', 'altura', 'atura', 'altrua', 'altur', 'alturas', 'altuta', 'haltura', 'height', 'high', 'tall'],
    right: ['derecha', 'right', 'east', 'oriente'],
    left: ['izquierda', 'left', 'west', 'oeste', 'occidente'],
    up: ['arriba', 'up', 'upwards', 'north', 'norte'],
    down: ['abajo', 'down', 'downwards', 'south', 'sur'],
    straight: ['recto', 'seguido', 'straight'],
    from: ['desde', 'from', 'empezando', 'starting'],
    origin: ['origen', 'origin'],
    end: ['final', 'extremo', 'end'],
    start: ['inicio', 'principio', 'comienzo', 'start', 'beginning'],
    to: ['hasta', 'until', 'till'],
    selected: ['seleccionado', 'seleccionada', 'selected', 'este', 'esta', 'ese', 'esa', 'this', 'that'],
    last: ['ultimo', 'ultima', 'anterior', 'last', 'previous'],
    next: ['siguiente', 'proximo', 'next', 'nearest', 'cercano'],
    more: ['mas', 'more'],
    less: ['menos', 'less'],
    /* Girar / Simetría / modos (14/11/2026). */
    center: ['centro', 'mitad', 'center', 'centre', 'middle', 'midpoint'],
    keep: ['mantener', 'manteniendo', 'manten', 'mantenga', 'conserva', 'conservar', 'conservando', 'keep', 'keeping', 'retain'],
    original: ['original', 'originales', 'originals'],
    about: ['respecto', 'sobre', 'alrededor', 'about', 'around', 'across', 'over'],
    vertical: ['vertical', 'verticalmente', 'vertically'],
    horizontal: ['horizontal', 'horizontalmente', 'horizontally'],
    cw: ['horario', 'clockwise'],
    ccw: ['antihorario', 'contrarreloj', 'counterclockwise', 'anticlockwise'],
    mode: ['modo', 'mode', 'vista', 'view'],
    lines: ['linea', 'lineas', 'line', 'lines'],
    gen3d: ['genera', 'generar', 'generalo', 'generame', 'generate', 'modelo', 'model', 'tridimensional'],
    formwork: ['encofra', 'encofrar', 'encofralo', 'encofrado', 'encoframe', 'encofre', 'formwork'],
    formworkColor: ['colores', 'color', 'coloreado', 'coloreados', 'coloured', 'colored', 'colours', 'colors'],
    go: ['pasa', 'pasar', 'pasame', 'vuelve', 'volver', 'switch', 'go', 'back'],
    /* Cotas por voz (pt114u). «numero(s)» y «numera» no van aquí (se parecen a «número» de «el muro número 4»). */
    ids: ['id', 'ids', 'identifica', 'identificar', 'identificalos', 'identificame', 'identificamelos', 'identificador', 'identificadores',
        'etiqueta', 'etiquetas', 'etiquetalos', 'rotula', 'rotulalos', 'identify', 'label', 'labels'],
    dims: ['acota', 'acotar', 'acotame', 'acotalo', 'acotamelo', 'acotalos', 'acotacion', 'acotaciones', 'cota', 'cotas', 'mide', 'medir',
        'mideme', 'midelo', 'dimensiona', 'dimensionar', 'dimension', 'dimensiones', 'measure', 'dimensions', 'dims'],
    hide: ['oculta', 'ocultar', 'ocultalos', 'ocultalas', 'ocultame', 'ocultalo', 'esconde', 'esconder', 'escondelos', 'apaga', 'apagar',
        'apagalos', 'desactiva', 'hide', 'off'],
    /* ID de artículo (pt116u): «los id de los paneles / de artículo». */
    article: ['articulo', 'articulos', 'article', 'articles', 'panel', 'paneles', 'panels', 'pieza', 'piezas', 'pieces'],
};
const WORD_KEYS = Object.keys(MA_STL_DESIGN_AI_WORDS);
const WORDS_NORM = {};
WORD_KEYS.forEach(function (k) { WORDS_NORM[k] = MA_STL_DESIGN_AI_WORDS[k].map(function (w) { return maStlWallAiNormalize(w); }); });
const JS_WORDS_NORM = {};
WORD_KEYS.forEach(function (k) { JS_WORDS_NORM[k] = WORDS_NORM[k].slice(); });

/* ------------------------- Catálogo de la BD (pt107u) ------------------------- */

/** Frases del catálogo que se buscan antes de los conceptos: de varias palabras, o de una sin concepto JS. */
let CATALOG_PHRASES = [];
/** Conceptos «verbo» que una frase larga se queda (sus palabras ya no cuentan para otra orden). */
const PHRASE_VERBS = ['draw', 'make', 'move', 'copy', 'del', 'rotate', 'mirror', 'trim', 'fillet', 'extend', 'shorten', 'change', 'undo', 'redo',
    'list', 'show', 'mode', 'go', 'gen3d', 'lines', 'formwork', 'formworkColor', 'help', 'thanks', 'cancel', 'greet'];
const MODE_BY_CODE = { MODE_LINES: 'lines', MODE_3D: 'wall3d', MODE_FORMWORK: 'formwork', MODE_FORMWORK_COLOR: 'formworkColor' };

/** Comparación palabra a palabra de las frases: cortas (≤ 3 letras) exactas; el resto, con faltas. */
function phraseWordMatch(a, b) {
    return a.length <= 3 || b.length <= 3 ? (a === b ? 1 : 0) : maStlWallAiWordMatch(a, b);
}

/** Entre dos coincidencias, gana la más larga; a igualdad, la de mejor calidad. */
function bestPhraseHit(a, b) {
    if (!a) return b || null;
    if (!b) return a;
    if (a.len !== b.len) return a.len > b.len ? a : b;
    return a.q >= b.q ? a : b;
}

/**
 * Aplica el catálogo del contexto 'design' (o lo quita con null): cada concepto = lista JS + palabras sueltas del
 * catálogo (las del usuario incluidas) − las que el usuario excluyó; si el usuario sustituyó las de por defecto de un
 * comando, ese concepto queda solo con las suyas. Devuelve { concepts, phrases } (pruebas).
 */
export function maStlDesignAiApplyCatalog(data) {
    WORD_KEYS.forEach(function (k) { WORDS_NORM[k] = JS_WORDS_NORM[k].slice(); });
    CATALOG_PHRASES = [];
    if (!data) return { concepts: 0, phrases: 0 };
    const byC = {};
    maStlAiCatalogPhrases(data, maStlWallAiNormalize, ['keyword', 'helper', 'learned']).forEach(function (ph) {
        if (/^COMMON_(YES|NO)$/.test(ph.code)) return;
        /* Frase aprendida: no entra en un concepto. Se ejecuta el comando, también con una letra cambiada. */
        if (ph.kind === 'learned') {
            if (ph.implemented && ph.aiId !== 'notAvailable' && !/notAvailable/i.test(ph.handler || '')) CATALOG_PHRASES.push(ph);
            return;
        }
        if (ph.toks.length === 1 && ph.concept && WORDS_NORM[ph.concept]) {
            const list = byC[ph.concept] || (byC[ph.concept] = []);
            if (list.indexOf(ph.toks[0]) < 0) list.push(ph.toks[0]);
            return;
        }
        if (ph.kind === 'keyword' && (ph.toks.length >= 2 || !ph.concept)) CATALOG_PHRASES.push(ph);
    });
    const rules = maStlAiCatalogUserRules(data, maStlWallAiNormalize, function (c) {
        const seen = {};
        (c.phrases || []).forEach(function (ph) { if (ph.c && ph.k === 'keyword') seen[ph.c] = 1; });
        return Object.keys(seen);
    });
    Object.keys(byC).forEach(function (c) {
        const ex = rules.excluded[c] || [];
        const base = rules.replaced[c] ? [] : JS_WORDS_NORM[c].filter(function (w) { return ex.indexOf(w) < 0; });
        WORDS_NORM[c] = base.concat(byC[c].filter(function (w) { return base.indexOf(w) < 0 && ex.indexOf(w) < 0; }));
    });
    Object.keys(rules.excluded).forEach(function (c) {
        if (byC[c] || !WORDS_NORM[c]) return;
        WORDS_NORM[c] = WORDS_NORM[c].filter(function (w) { return rules.excluded[c].indexOf(w) < 0; });
    });
    return { concepts: Object.keys(byC).length, phrases: CATALOG_PHRASES.length };
}

/** Lista de palabras efectiva de un concepto (pruebas). */
export function maStlDesignAiConceptWords(concept) { return (WORDS_NORM[concept] || []).slice(); }

/* Frase del catálogo en el mensaje (gana la más larga): sus palabras dejan de contar para otras órdenes. */
function applyPhraseLayer(p, toks) {
    p.phrase = null;
    p.catalogCmd = null;
    if (!CATALOG_PHRASES.length) return;
    const wt = toks.filter(function (tk) { return tk.t === 'w'; });
    const words = wt.map(function (tk) { return tk.w; });
    const learned = CATALOG_PHRASES.filter(function (ph) { return ph.kind === 'learned'; });
    const rest = CATALOG_PHRASES.filter(function (ph) { return ph.kind !== 'learned'; });
    const learnedHit = maStlAiCatalogLongest(words, learned, phraseWordMatch, 0.8);
    const learnedOk = learnedHit && (learnedHit.len >= words.length || (learnedHit.len >= 2 && learnedHit.len + 3 >= words.length)) ? learnedHit : null;
    const m = bestPhraseHit(learnedOk, maStlAiCatalogLongest(words, rest, phraseWordMatch, 0.95));
    if (!m) return;
    const it = m.item;
    p.phrase = { code: it.code, concept: it.concept, len: m.len, q: m.q, text: it.text, implemented: it.implemented,
        handler: it.handler || '', confirm: !!it.confirm, destructive: !!it.destructive, kind: it.kind || 'keyword',
        nameEs: it.nameEs, nameEn: it.nameEn, aiId: it.aiId, start: m.start };
    if (m.len >= 2) {
        for (let j = 0; j < m.len; j++) {
            const tk = wt[m.start + j];
            Object.keys(tk.c).forEach(function (c) { if (PHRASE_VERBS.indexOf(c) >= 0 && c !== it.concept) delete tk.c[c]; });
            tk.phrase = true;
        }
    }
    if (it.concept) {
        const tk0 = wt[m.start];
        tk0.c[it.concept] = Math.max(tk0.c[it.concept] || 0, m.q);
    } else {
        p.catalogCmd = p.phrase;
    }
}

maStlAiCatalogOnChange(function (ctx, data) { if (ctx === 'design') maStlDesignAiApplyCatalog(data); });
if (maStlAiCatalogGet('design')) maStlDesignAiApplyCatalog(maStlAiCatalogGet('design'));

/** Conceptos de una palabra: { concepto: calidad } (exacta 1 · fonética 0,95 · 1 error 0,8 · 2 errores 0,65). */
function conceptsOf(w) {
    const out = {};
    for (let k = 0; k < WORD_KEYS.length; k++) {
        const list = WORDS_NORM[WORD_KEYS[k]];
        let best = 0;
        for (let i = 0; i < list.length && best < 1; i++) {
            const lw = list[i];
            const qv = w.length <= 3 || lw.length <= 3 ? (w === lw ? 1 : 0) : maStlWallAiWordMatch(w, lw);
            if (qv > best) best = qv;
        }
        if (best >= 0.65) out[WORD_KEYS[k]] = best;
    }
    /* Cada palabra cuenta para su mejor concepto (y empates); «muro» no es «muestra» ni «mueve». */
    const top = Math.max.apply(null, Object.keys(out).map(function (c) { return out[c]; }).concat([0]));
    Object.keys(out).forEach(function (c) { if (out[c] < top) delete out[c]; });
    return out;
}

const LABEL_GLUE_AFTER = /^(de|del|of|en|in|metros|m|mas|menos|more|less)$/;
const LABEL_GLUE_BEFORE = /^(de|del|el|la|a|su|of|the|to|is|es|con|with|un|una|en|in|al|por|by)$/;

function labelOfWord(t) {
    if (!t || t.t !== 'w' || !t.c) return null;
    if (t.c.length >= 0.8) return 'length';
    if (t.c.thickness >= 0.8) return 'thickness';
    if (t.c.height >= 0.8) return 'height';
    return null;
}

/**
 * Lectura del mensaje: conceptos (con su calidad), medidas con etiqueta (largo / espesor / alto), muro citado,
 * dirección, punto de inicio, extremo… Pura (sin visor): la usan los comandos y las pruebas.
 */
export function maStlDesignAiParse(text) {
    const toks = maStlDesignAiLex(text);
    const norm = maStlWallAiNormalize(text);
    const p = { text: String(text || '').trim(), norm: norm, toks: toks, has: {}, q: {}, measures: [], wallRef: null, wallRefs: [],
        dirDeg: null, dirFromAngle: false, straight: false, relTurn: null, start: null, end: null, xy: null, words: [] };
    const mark = function (c, qv) { if (!(p.q[c] >= qv)) p.q[c] = qv; p.has[c] = true; };
    toks.forEach(function (tk, i) {
        tk.i = i;
        if (tk.t !== 'w') return;
        p.words.push(tk.w);
        tk.c = conceptsOf(tk.w);
    });
    const W = function (i) { const t = toks[i]; return t && t.t === 'w' ? t : null; };
    const isC = function (i, c, min) { const t = W(i); return !!(t && t.c && t.c[c] >= (min || 0.8)); };
    /* «este»: «hacia el este» es dirección; «este muro», demostrativo. «up to» no es «arriba». */
    toks.forEach(function (tk, i) {
        if (tk.t !== 'w') return;
        if (tk.w === 'este' && W(i - 1) && /^(hacia|al|direccion)$/.test(W(i - 1).w)) tk.c = { right: 1 };
        if (tk.w === 'up' && W(i + 1) && W(i + 1).w === 'to') { tk.c = { to: 1 }; }
        if (tk.w === 'mas' || tk.w === 'more') tk.c = { more: 1 };
        if (/^(girando|giro|tuerce|torciendo|turning)$/.test(tk.w)) tk.c = {};
    });
    /* «otro de 3 metros gira a la izquierda»: dibujar girando, no la herramienta Girar (hay largo, sin grados ni muro citado). */
    const hasLen = toks.some(function (tk) { return tk.t === 'n' && (tk.unit === 'm' || tk.unit === 'cm' || tk.unit === 'mm'); });
    const hasDeg = toks.some(function (tk) { return tk.t === 'n' && tk.unit === 'deg'; });
    if (hasLen && !hasDeg && !/\b(muro|pared|wall) ?(n|no|numero|number)? ?\d/.test(norm)) {
        toks.forEach(function (tk, i) {
            if (tk.t !== 'w' || !/^(gira|gire|turn)$/.test(tk.w)) return;
            const next = [W(i + 1), W(i + 2), W(i + 3)];
            if (next.some(function (t) { return t && /^(izquierda|derecha|left|right)$/.test(t.w); })) tk.c = {};
        });
    }
    applyPhraseLayer(p, toks);
    toks.forEach(function (tk) { if (tk.t === 'w') Object.keys(tk.c).forEach(function (c) { mark(c, tk.c[c]); }); });
    /* Muro citado: «muro 4», «pared nº 4», «wall #4»; también «el 4» tras borrar/mover/copiar/alargar/señalar. */
    toks.forEach(function (tk, i) {
        if (tk.t !== 'n' || tk.unit || tk.dec) return;
        const a = W(i - 1);
        const b = W(i - 2);
        const c3 = W(i - 3);
        const wallBefore = (a && a.c && a.c.wall >= 0.8)
            || (a && /^(numero|number|n|no|nro|num)$/.test(a.w) && ((b && b.c && b.c.wall >= 0.8) || (b && /^(el|la|the)$/.test(b.w))));
        const verbBefore = a && /^(el|la|the)$/.test(a.w) && b && b.c && (b.c.del || b.c.move || b.c.copy || b.c.extend || b.c.show || b.c.change || b.c.rotate || b.c.mirror)
            && !(toks[i + 1] && toks[i + 1].t === 'w' && unitOf(toks[i + 1].w));
        if ((wallBefore || verbBefore) && Number.isInteger(tk.v) && tk.v >= 0) {
            tk.ref = true;
            p.wallRefs.push(String(tk.v));
        }
        void c3;
    });
    if (p.wallRefs.length) p.wallRef = p.wallRefs[0];
    p.selectedRef = toks.some(function (tk, i) {
        if (!(tk.t === 'w' && tk.c && tk.c.selected >= 0.95)) return false;
        return /^(seleccionado|seleccionada|selected)$/.test(tk.w) || isC(i + 1, 'wall');
    });
    p.lastRef = toks.some(function (tk, i) { return isC(i, 'last', 0.95) && (isC(i + 1, 'wall') || isC(i - 1, 'wall')); });
    /* Ángulo: «a 45°», «45 grados». */
    toks.forEach(function (tk) {
        if (tk.t === 'n' && tk.unit === 'deg') { p.dirDeg = ((tk.v % 360) + 360) % 360; tk.angle = true; p.dirFromAngle = true; }
    });
    /* Dirección (la última dicha). «girando a la izquierda» = giro relativo al muro anterior. */
    toks.forEach(function (tk, i) {
        if (tk.t !== 'w' || !tk.c) return;
        const d = tk.c.right >= 0.8 ? 0 : tk.c.up >= 0.95 ? 90 : tk.c.left >= 0.8 ? 180 : tk.c.down >= 0.95 ? 270 : null;
        if (d == null) return;
        const turn = [W(i - 1), W(i - 2), W(i - 3)].some(function (t) { return t && /^(girando|gira|giro|tuerce|turning|turn)$/.test(t.w); });
        if (turn && (d === 0 || d === 180)) { p.relTurn = d === 180 ? 90 : -90; return; }
        if (!p.dirFromAngle) p.dirDeg = d;
    });
    p.straight = toks.some(function (tk, i) { return isC(i, 'straight', 0.95); }) || /\b(misma direccion|same direction)\b/.test(norm);
    /* Punto de inicio: «desde el origen», «desde 2;3», «desde el final/inicio del muro 4». */
    const fromIdx = toks.findIndex(function (tk, i) { return isC(i, 'from', 0.95); });
    const xy = toks.find(function (tk) { return tk.t === 'xy'; });
    if (xy) p.xy = { x: xy.x, z: xy.z };
    if (fromIdx >= 0) {
        const after = toks.slice(fromIdx + 1, fromIdx + 6);
        const has = function (c, min) { return after.some(function (tk) { return tk.t === 'w' && tk.c && tk.c[c] >= (min || 0.8); }); };
        if (has('origin')) p.start = { kind: 'origin' };
        else if (xy) p.start = { kind: 'xy', x: xy.x, z: xy.z };
        else if (has('start')) p.start = { kind: 'wallStart' };
        else if (has('end') || has('last', 0.95) || p.wallRef || has('selected', 0.95)) p.start = { kind: 'wallEnd' };
    } else if (xy) {
        p.start = { kind: 'xy', x: xy.x, z: xy.z };
    }
    const inFrom = function (i) { return fromIdx >= 0 && i > fromIdx && i <= fromIdx + 5; };
    toks.forEach(function (tk, i) {
        if (inFrom(i)) return;
        if (isC(i, 'end')) p.end = 'end';
        if (isC(i, 'start')) p.end = 'start';
    });
    p.toTarget = toks.some(function (tk, i) { return isC(i, 'to', 0.95); });
    /* Medidas con su etiqueta: «5 m de largo», «30 de espesor», «alto 2,70», «largo de 6». «más/menos» = cambio relativo. */
    toks.forEach(function (tk, i) {
        if (tk.t !== 'n' || tk.ref || tk.angle) return;
        let label = null;
        for (let k = i + 1; k <= i + 3 && k < toks.length; k++) {
            const t = toks[k];
            if (t.t !== 'w') break;
            label = labelOfWord(t);
            if (label) break;
            if (!LABEL_GLUE_AFTER.test(t.w)) break;
        }
        if (!label) {
            for (let k = i - 1; k >= i - 4 && k >= 0; k--) {
                const t = toks[k];
                if (t.t !== 'w') break;
                label = labelOfWord(t);
                if (label) break;
                if (!LABEL_GLUE_BEFORE.test(t.w)) break;
            }
        }
        let sign = 0;
        for (let k = i - 1; k >= i - 2 && k >= 0; k--) {
            const t = W(k);
            if (!t) break;
            if (t.c && t.c.more) { sign = 1; break; }
            if (t.c && t.c.less) { sign = -1; break; }
        }
        if (!sign) {
            const t = W(i + 1);
            if (t && t.c && t.c.more) sign = 1;
            else if (t && t.c && t.c.less) sign = -1;
        }
        p.measures.push({ v: tk.v, unit: tk.unit || null, label: label, dec: !!tk.dec, i: i, sign: sign });
    });
    /* «cambia la altura del muro 4 a 3 m»: una sola medida sin etiqueta y una sola etiqueta en la frase → es esa. */
    /* Girar: grados con signo («-30°», «menos 30 grados») y sentido (izquierda / antihorario = +, derecha / horario = −). */
    const degTok = toks.find(function (tk) { return tk.t === 'n' && tk.unit === 'deg'; });
    p.angleDeg = null;
    if (degTok) {
        const before = W(degTok.i - 1);
        p.angleDeg = degTok.v * (before && before.c && before.c.less ? -1 : 1);
    }
    p.turnSense = 0;
    toks.forEach(function (tk) {
        if (tk.t !== 'w' || !tk.c) return;
        if (tk.c.ccw >= 0.8) p.turnSense = 1;
        else if (tk.c.cw >= 0.8) p.turnSense = -1;
    });
    if (!p.turnSense) {
        toks.forEach(function (tk) {
            if (tk.t !== 'w' || !tk.c) return;
            if (tk.c.left >= 0.8) p.turnSense = 1;
            else if (tk.c.right >= 0.8) p.turnSense = -1;
        });
    }
    const free = p.measures.filter(function (m) { return !m.label; });
    const labs = ['length', 'thickness', 'height'].filter(function (c) {
        return toks.some(function (tk) { return tk.t === 'w' && labelOfWord(tk) === c; });
    });
    const used = p.measures.some(function (m) { return m.label; });
    if (free.length === 1 && labs.length === 1 && !used) free[0].label = labs[0];
    return p;
}

/**
 * Medida → mm según el dato; sin unidad, como se dice en obra.
 * Largo / distancia: < 100 → m («5», «3,5»); ≥ 100 → cm («350»). Espesor: < 2 → m («0,30»); < 100 → cm («30»); si no, mm.
 * Alto: ≤ 20 → m («2,70»); < 1000 → cm («270»); si no, mm.
 */
export function maStlDesignAiMeasureMm(ms, kind) {
    if (!ms) return null;
    const v = Number(ms.v);
    if (!Number.isFinite(v)) return null;
    if (ms.unit === 'm') return Math.round(v * 1000);
    if (ms.unit === 'cm') return Math.round(v * 10);
    if (ms.unit === 'mm') return Math.round(v);
    if (kind === 'thickness') return Math.round(v < 2 ? v * 1000 : v < 100 ? v * 10 : v);
    if (kind === 'height') return Math.round(v <= 20 ? v * 1000 : v < 1000 ? v * 10 : v);
    return Math.round(v < 100 ? v * 1000 : v * 10);
}

/** Dirección (grados, antihorario, 0 = derecha) → vector en planta (arriba en pantalla = −Z, como la herramienta Línea). */
export function maStlDesignAiDirVector(deg) {
    const r = (Number(deg) || 0) * Math.PI / 180;
    const ux = Math.cos(r);
    const uz = -Math.sin(r);
    return { x: Math.abs(ux) < 1e-12 ? 0 : ux, z: Math.abs(uz) < 1e-12 ? 0 : uz };
}

/** Dirección de a → b en grados (0 = derecha, 90 = arriba en pantalla). */
export function maStlDesignAiDirOf(a, b) {
    const d = Math.atan2(-(b.z - a.z), b.x - a.x) * 180 / Math.PI;
    return Math.round((((d % 360) + 360) % 360) * 1000) / 1000;
}

function dirLabel(deg, T) {
    const d = Math.round((((deg % 360) + 360) % 360) * 100) / 100;
    if (T.dir[d] != null) return T.dir[d];
    return fmt(T.dirAngle, T.lang === 'en' ? String(d) : String(d).replace('.', ','));
}

/* ------------------------------ Registro de comandos ------------------------------ */

/**
 * Comandos de la IA del diseño. Cada uno:
 * { id, score(p) → 0..1 (≥ 0,8 se ejecuta; 0,65..0,8 «¿Querías decir…?»), run(p, s, T) → respuesta del chat,
 *   edits (true si cambia el plano: se pasa antes a la vista de líneas), params [{ name, required, help }],
 *   examples { es: [...], en: [...] }, help { es, en } (línea de «ayuda»), short { es, en } (para «¿Querías decir…?») }.
 */
export const MA_STL_DESIGN_AI_COMMANDS = [];

export function registerDesignAiCommand(cmd) {
    if (!cmd || !cmd.id || typeof cmd.score !== 'function' || typeof cmd.run !== 'function') {
        throw new Error('registerDesignAiCommand: id, score y run son obligatorios');
    }
    const i = MA_STL_DESIGN_AI_COMMANDS.findIndex(function (c) { return c.id === cmd.id; });
    if (i >= 0) MA_STL_DESIGN_AI_COMMANDS[i] = cmd;
    else MA_STL_DESIGN_AI_COMMANDS.push(cmd);
    return cmd;
}

if (typeof window !== 'undefined') window.registerDesignAiCommand = registerDesignAiCommand;

/** Mejor comando para el mensaje: { cmd, score } (o null). */
export function maStlDesignAiBestCommand(p) {
    let best = null;
    for (let i = 0; i < MA_STL_DESIGN_AI_COMMANDS.length; i++) {
        const c = MA_STL_DESIGN_AI_COMMANDS[i];
        let sc = 0;
        try { sc = Number(c.score(p)) || 0; } catch (_e) { sc = 0; }
        if (sc > 0 && (!best || sc > best.score)) best = { cmd: c, score: sc };
    }
    return best;
}

/* Calidad de un concepto (0 si no está) y verbos que «mandan» en la frase. */
function q(p, c) { return p.q[c] || 0; }
function anyOf(p, list) { return Math.max.apply(null, list.map(function (c) { return q(p, c); })); }
const EDIT_VERBS = ['move', 'copy', 'del', 'rotate', 'mirror', 'trim', 'fillet', 'extend', 'shorten', 'change', 'undo', 'redo', 'list', 'show',
    'ids', 'dims', 'hide'];
function otherVerb(p, except) {
    return EDIT_VERBS.filter(function (v) { return except.indexOf(v) < 0; }).some(function (v) { return q(p, v) >= 0.8; });
}
function measureBy(p, label) { return p.measures.find(function (m) { return m.label === label; }) || null; }
function unlabelled(p) { return p.measures.filter(function (m) { return !m.label; }); }
function near(a, b, tol) { return Math.hypot(a.x - b.x, a.z - b.z) <= (tol || 1); }
function bridgeOf(s) { return s && s.bridge; }

/**
 * Muro citado: número (etiqueta = el ID que se ve en Cotas → id del eje → dbId → wallId/grupo si es único),
 * «este/seleccionado», «el último», o el seleccionado.
 */
export function maStlDesignAiResolveWall(p, s, opts) {
    opts = opts || {};
    const B = bridgeOf(s);
    const walls = B.walls();
    const ref = opts.ref != null ? opts.ref : p.wallRef;
    if (ref != null) {
        const r = String(ref);
        let w = walls.find(function (x) { return x.label === r; })
            || walls.find(function (x) { return String(x.id) === r; })
            || walls.find(function (x) { return x.dbId != null && String(x.dbId) === r; });
        if (!w) {
            const byWall = walls.filter(function (x) { return x.wallId === r || String(x.groupId) === r; });
            if (byWall.length === 1) w = byWall[0];
        }
        return w ? { wall: w } : { missing: r, walls: walls };
    }
    if (p.lastRef && s.lastWallId != null) {
        const w = walls.find(function (x) { return String(x.id) === String(s.lastWallId); });
        if (w) return { wall: w };
    }
    const sel = B.selectedWall();
    if (sel) return { wall: sel };
    if (opts.allowLast !== false && s.lastWallId != null && selectionUnchanged(s, B)) {
        const w = walls.find(function (x) { return String(x.id) === String(s.lastWallId); });
        if (w) return { wall: w };
    }
    return { none: true, walls: walls };
}

function wallList(walls) { return walls.map(function (w) { return w.label; }).join(', ') || '—'; }

function dimsText(w, T) {
    return fmt(T.wallDims, maStlDesignAiFmtM(w.lengthMm, T.lang), maStlDesignAiFmtM(w.thicknessMm, T.lang), maStlDesignAiFmtM(w.heightMm, T.lang));
}

function undoButton(s, T) {
    return { label: T.undo, run: function () { return undoReply(s, T); } };
}

function undoReply(s, T) {
    const B = bridgeOf(s);
    if (!B.canUndo()) return { reply: T.nothingUndo, lang: T.lang };
    B.undo();
    B.clearHighlight();
    return { reply: T.undone, lang: T.lang };
}

function wallMissingReply(r, T) {
    if (r.missing != null) return { reply: r.walls.length ? fmt(T.noWall, r.missing, wallList(r.walls)) : T.noWalls };
    return null;
}

/** Pide un dato y deja la orden a medias (la respuesta siguiente se mezcla con ella). */
function ask(s, cmd, p, need, text) {
    s.ask = { cmd: cmd, p: p, need: need };
    return { reply: text };
}

function afterChange(s, w) {
    const B = bridgeOf(s);
    if (!w) return;
    s.lastWallId = w.id;
    try { B.highlight(w.id); } catch (_e) { /* sin caras */ }
    /* pt117u: si luego el usuario cambia la selección (clic, Esc), el «muro actual» pasa a ser el suyo. */
    s.lastSelSeq = typeof B.selectionSeq === 'function' ? B.selectionSeq() : null;
}

/** ¿La selección del visor sigue siendo la que dejó la IA? (sin bridge.selectionSeq: sí). */
function selectionUnchanged(s, B) {
    if (typeof B.selectionSeq !== 'function' || s.lastSelSeq == null) return true;
    return B.selectionSeq() === s.lastSelSeq;
}

/** Vector del muro (orden de dibujo) y su dirección en grados. */
function wallAxis(w) {
    const a = w.drawP1 || w.p1;
    const b = w.drawP2 || w.p2;
    const len = Math.hypot(b.x - a.x, b.z - a.z) || 1;
    return { a: a, b: b, ux: (b.x - a.x) / len, uz: (b.z - a.z) / len, deg: maStlDesignAiDirOf(a, b) };
}

/** Extremo de eje ('p1' | 'p2') que corresponde al final de dibujo (o al inicio con `start`). */
function axisEndKey(w, start) {
    const ax = wallAxis(w);
    const pt = start ? ax.a : ax.b;
    return Math.hypot(pt.x - w.p2.x, pt.z - w.p2.z) <= Math.hypot(pt.x - w.p1.x, pt.z - w.p1.z) ? 'p2' : 'p1';
}

/** Distancia de un punto al segmento del eje (mm, en planta). */
function distToAxis(pt, w) {
    const ax = w.p2.x - w.p1.x;
    const az = w.p2.z - w.p1.z;
    const l2 = ax * ax + az * az || 1;
    let t = ((pt.x - w.p1.x) * ax + (pt.z - w.p1.z) * az) / l2;
    t = Math.max(0, Math.min(1, t));
    return Math.hypot(pt.x - (w.p1.x + t * ax), pt.z - (w.p1.z + t * az));
}

/* ---------------------------------- Comandos ---------------------------------- */

registerDesignAiCommand({
    id: 'help',
    score: function (p) {
        if (q(p, 'help') >= 0.8 && p.words.length <= 6) return q(p, 'help');
        if (/\b(que (puedes|sabes) hacer|what can you do|que comandos)\b/.test(p.norm)) return 1;
        return q(p, 'help') >= 0.65 && p.words.length <= 3 ? q(p, 'help') : 0;
    },
    run: function (p, s, T) {
        const lines = [T.helpHead];
        MA_STL_DESIGN_AI_COMMANDS.forEach(function (c) {
            if (!c.help) return;
            const ex = c.examples && c.examples[T.lang] && c.examples[T.lang].length ? ' \u2014 «' + c.examples[T.lang][0] + '»' : '';
            lines.push('\u2022 ' + c.help[T.lang] + ex);
        });
        lines.push(T.helpTail);
        return { reply: lines.join('\n'), speech: T.helpHead + ' ' + T.helpSpeech };
    },
    help: null,
    short: { es: 'ayuda', en: 'help' },
    examples: { es: ['ayuda', '¿qué puedes hacer?'], en: ['help', 'what can you do?'] },
});

/* Dibujar muro: largo (obligatorio), espesor y alto (Configuración si no se dicen), dirección, inicio. */
registerDesignAiCommand({
    id: 'wall.draw',
    edits: true,
    params: [
        { name: 'largo', required: true, help: '«5 m», «3,5», «350 cm», «tres metros y medio»' },
        { name: 'dirección', required: false, help: 'derecha / izquierda / arriba / abajo / «a 45°» / «girando a la izquierda» / «recto» (por defecto derecha si empieza de cero; si sigue a otro muro, se pregunta)' },
        { name: 'espesor', required: false, help: '«de 30 de espesor» (por defecto el de Configuración)' },
        { name: 'alto', required: false, help: '«2,70 de alto» (por defecto el de Configuración)' },
        { name: 'inicio', required: false, help: '«desde el origen», «desde 2;3», «desde el final del muro 4» (por defecto sigue al último muro de la IA o al seleccionado)' },
    ],
    score: function (p) {
        if (otherVerb(p, ['shorten'])) return 0;
        const d = q(p, 'draw');
        const m = q(p, 'make');
        const w = q(p, 'wall');
        if (d && w) return Math.min(d, w);
        if (m && w) return Math.min(m, w) * 0.97;
        if (m && p.measures.length && !p.measures.some(function (x) { return x.label && x.label !== 'length'; }) && !p.wallRef) return m * 0.9;
        if (d && p.measures.length) return d * 0.9;
        if (w && p.measures.length && (p.dirDeg != null || p.relTurn != null) && !p.wallRef) return w * 0.85;
        if (d) return d * 0.8;
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const D = B.defaults();
        const lenMs = measureBy(p, 'length') || unlabelled(p)[0];
        const lengthMm = maStlDesignAiMeasureMm(lenMs, 'length');
        const thMs = measureBy(p, 'thickness');
        const thicknessMm = thMs ? maStlDesignAiMeasureMm(thMs, 'thickness') : D.thicknessMm;
        const hMs = measureBy(p, 'height');
        const heightMm = hMs ? maStlDesignAiMeasureMm(hMs, 'height') : D.heightMm;
        /* Inicio y muro del que sigue. */
        const walls = B.walls();
        const byId = function (id) { return walls.find(function (x) { return String(x.id) === String(id); }) || null; };
        let a = null;
        let prev = null;
        let fromText = '';
        const st = p.start;
        if (st && st.kind === 'origin') {
            a = B.origin();
            fromText = T.fromOrigin;
        } else if (st && st.kind === 'xy') {
            a = { x: st.x * 1000, z: st.z * 1000 };
            fromText = fmt(T.fromPoint, (T.lang === 'en' ? String(st.x) : String(st.x).replace('.', ',')) + ';' + (T.lang === 'en' ? String(st.z) : String(st.z).replace('.', ',')));
        } else {
            let r = null;
            if (p.wallRef != null) {
                r = maStlDesignAiResolveWall(p, s);
                if (r.missing != null) return wallMissingReply(r, T);
                prev = r.wall;
            } else if (p.selectedRef) {
                prev = B.selectedWall();
            } else {
                prev = (s.lastWallId != null && byId(s.lastWallId)) || B.selectedWall();
            }
            if (prev) {
                const ax = wallAxis(prev);
                const atStart = st && st.kind === 'wallStart';
                a = atStart ? ax.a : ax.b;
                fromText = fmt(atStart ? T.fromStart : T.fromEnd, wallName(prev, T));
            } else {
                a = B.origin();
                fromText = T.fromOrigin;
            }
        }
        if (!(lengthMm > 0)) return ask(s, this, p, 'length', T.askLength);
        if (lengthMm < D.minLengthMm) return { reply: fmt(T.tooShort, maStlDesignAiFmtM(D.minLengthMm, T.lang)) };
        if (!(thicknessMm >= D.minThicknessMm && thicknessMm <= D.maxThicknessMm)) {
            return { reply: fmt(T.thickRange, maStlDesignAiFmtM(D.minThicknessMm, T.lang), maStlDesignAiFmtM(D.maxThicknessMm, T.lang)) };
        }
        /* Dirección: absoluta, giro relativo, recto (sigue la del anterior) o pregunta si sigue a otro muro. */
        let deg = p.dirDeg;
        let note = '';
        if (deg == null && prev && (p.relTurn != null || p.straight)) deg = wallAxis(prev).deg + (p.relTurn || 0);
        if (deg == null && prev) return ask(s, this, p, 'dir', T.askDirection);
        if (deg == null) { deg = 0; note = T.defaultDir; }
        deg = ((deg % 360) + 360) % 360;
        const v = maStlDesignAiDirVector(deg);
        const b = { x: a.x + v.x * lengthMm, z: a.z + v.z * lengthMm };
        const res = B.drawWall({ x: a.x, y: a.y, z: a.z }, { x: b.x, y: a.y, z: b.z },
            { thicknessMm: thicknessMm, heightM: heightMm / 1000, groupId: prev ? prev.groupId : null });
        if (!res || !res.ok) return { reply: T.drawFail };
        let w = res.walls && res.walls.length ? res.walls[res.walls.length - 1] : null;
        const fmtArgs = [maStlDesignAiFmtM(lengthMm, T.lang), dirLabel(deg, T), maStlDesignAiFmtM(thicknessMm, T.lang), maStlDesignAiFmtM(heightMm, T.lang), fromText];
        let reply;
        if (w) {
            /* La herramienta Muro 2D parte el tramo donde cruza otros muros: se nombra el último (el del final) y se avisa. */
            const seg = { p1: { x: a.x, z: a.z }, p2: b };
            const own = res.walls.filter(function (x) { return distToAxis(x.p1, seg) < 2 && distToAxis(x.p2, seg) < 2; });
            const ends = own.filter(function (x) { return near(x.p1, b, 2) || near(x.p2, b, 2); });
            if (ends.length) w = ends[0];
            reply = fmt.apply(null, [T.drawn, wallName(w, T)].concat(fmtArgs));
            if (own.length > 1) reply += '\n' + fmt(T.splitNote, own.length, own.map(function (x) { return x.label; }).join(', '));
        } else {
            /* En línea con el anterior: la herramienta Muro 2D lo une a ese tramo. */
            w = B.walls().find(function (x) { return near(x.p1, b, 2) || near(x.p2, b, 2); }) || null;
            reply = fmt.apply(null, [T.drawnMerged, wallName(w, T)].concat(fmtArgs).concat([maStlDesignAiFmtM(w ? w.lengthMm : lengthMm, T.lang)]));
        }
        afterChange(s, w);
        return { reply: reply + (note ? '\n' + note : ''), buttons: [undoButton(s, T)] };
    },
    help: { es: 'Dibujar un muro (largo, dirección, espesor, alto, desde dónde)', en: 'Draw a wall (length, direction, thickness, height, start)' },
    short: { es: 'dibujar un muro', en: 'draw a wall' },
    examples: {
        es: ['dibuja un muro de 5 m hacia la derecha', 'otro de 3 metros girando a la izquierda', 'haz un muro de 4 m hacia arriba de 20 de espesor y 3 de alto',
            'dibuja un muro de 6 m desde el origen', 'traza un muro de 2,5 desde 2;3 a 45 grados', 'dibuja un muro de 3 m desde el final del muro 4 hacia abajo'],
        en: ['draw a 5 m wall to the right', 'another 3 meters turning left', 'draw a 4 m wall up 20 thick and 3 high', 'draw a 6 m wall from the origin'],
    },
});

/* Cambiar medidas: largo / espesor / alto, en absoluto o relativo («alarga 1 m», «2 cm más de espesor»). */
registerDesignAiCommand({
    id: 'wall.edit',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA' },
        { name: 'largo / espesor / alto', required: true, help: '«6 m de largo», «espesor 25», «alto 3», «1 m más largo», «alarga 50 cm», «acorta 1 m»' },
    ],
    score: function (p) {
        if (otherVerb(p, ['change', 'extend', 'shorten', 'show', 'dims'])) return 0;
        const labelled = p.measures.some(function (m) { return m.label; });
        const ch = q(p, 'change');
        /* «el muro 7 mide 4,5»: medir con una cifra es cambiar el largo. */
        if (q(p, 'dims') >= 0.8 && !ch && p.measures.length && !q(p, 'draw')) return labelled ? 0.86 : 0.85;
        if (ch && (labelled || p.measures.length)) return ch;
        if (q(p, 'shorten') && p.measures.length) return q(p, 'shorten');
        if (q(p, 'shorten')) return q(p, 'shorten') * 0.9;
        if (q(p, 'extend') && p.measures.length && !p.toTarget) return q(p, 'extend');
        if (labelled && (p.wallRef || p.selectedRef) && !q(p, 'draw') && !q(p, 'make')) return 0.85;
        if (labelled && p.measures.some(function (m) { return m.sign; }) && !q(p, 'draw')) return 0.85;
        if (ch && q(p, 'wall')) return Math.min(ch, q(p, 'wall')) * 0.9;
        if (q(p, 'make') && labelled && !q(p, 'wall') && !q(p, 'draw')) return 0.85;
        if (labelled && !q(p, 'draw') && !q(p, 'make') && p.words.length <= 4) return 0.82;
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        const D = B.defaults();
        const dims = {};
        const done = [];
        const rel = q(p, 'extend') ? 1 : q(p, 'shorten') ? -1 : 0;
        const pick = function (label) {
            const m = measureBy(p, label) || (label === 'length' && (rel || unlabelled(p).length) && !measureBy(p, 'thickness') && !measureBy(p, 'height') ? unlabelled(p)[0] : null);
            if (!m) return null;
            const mm = maStlDesignAiMeasureMm(m, label);
            const cur = label === 'length' ? w.lengthMm : label === 'thickness' ? w.thicknessMm : w.heightMm;
            const sign = m.sign || (label === 'length' ? rel : 0);
            return sign ? cur + sign * mm : mm;
        };
        const L = pick('length');
        const Th = pick('thickness');
        const H = pick('height');
        if (L == null && Th == null && H == null) {
            if (p.measures.length) return ask(s, this, p, 'what', fmt(T.askWhat, w.label));
            if (rel) return ask(s, this, p, 'length', T.askLength);
            return ask(s, this, p, 'what', fmt(T.askWhat, w.label));
        }
        if (L != null) {
            if (L < D.minLengthMm) return { reply: fmt(T.tooShort, maStlDesignAiFmtM(D.minLengthMm, T.lang)) };
            if (Math.abs(L - w.lengthMm) > 0.5) { dims.lengthMm = L; done.push(T.names.length + ' ' + maStlDesignAiFmtM(L, T.lang)); }
        }
        if (Th != null) {
            if (!(Th >= D.minThicknessMm && Th <= D.maxThicknessMm)) {
                return { reply: fmt(T.thickRange, maStlDesignAiFmtM(D.minThicknessMm, T.lang), maStlDesignAiFmtM(D.maxThicknessMm, T.lang)) };
            }
            if (Math.abs(Th - w.thicknessMm) > 0.5) { dims.thicknessMm = Th; done.push(T.names.thickness + ' ' + maStlDesignAiFmtM(Th, T.lang)); }
        }
        if (H != null && H > 0 && Math.abs(H - w.heightMm) > 0.5) { dims.heightM = H / 1000; done.push(T.names.height + ' ' + maStlDesignAiFmtM(H, T.lang)); }
        if (!done.length) return { reply: fmt(T.nothingToChange, wallName(w, T)) };
        const res = B.setDims(w.id, dims);
        if (!res || !res.ok) return { reply: fmt(T.editFail, done.join(', '), wallName(w, T)) };
        if (res.failed && res.failed.length) {
            return { reply: fmt(T.editFail, res.failed.map(function (k) { return T.names[k]; }).join(', '), wallName(w, T)), buttons: [undoButton(s, T)] };
        }
        const now = res.wall || w;
        /* Si estaban las cotas de otro muro («acótame el muro 7» y luego «muro 9 longitud 4»), pasan a este. */
        try {
            const cs = B.cotasState ? B.cotasState() : null;
            if (cs && cs.mode === 'wall' && String(cs.axisId) !== String(now.id) && B.dimensionWall) B.dimensionWall(now.id);
        } catch (_e) { /* sin cotas */ }
        afterChange(s, now);
        return { reply: fmt(T.edited, wallName(now, T), dimsText(now, T)), buttons: [undoButton(s, T)] };
    },
    help: { es: 'Cambiar largo, espesor o alto de un muro', en: 'Change a wall\'s length, thickness or height' },
    short: { es: 'cambiar las medidas del muro', en: 'change the wall sizes' },
    examples: {
        es: ['cambia el largo del muro 4 a 6 m', 'ponle 25 de espesor', 'el muro seleccionado de 3 m de alto', 'alarga el muro 2 en 50 cm', 'acorta 1 m', 'dale 2 cm más de espesor',
            'cámbiame la longitud a 4,5', 'el espesor es 0,45', 'muro 7 longitud 4,5', 'cámbiame la altura a 3', 'altura 2,70', 'la altura es 3,00', 'muro 7 altura 3'],
        en: ['set wall 4 length to 6 m', 'make it 25 thick', 'change the height to 3 m', 'lengthen wall 2 by 50 cm', 'length 4.5', 'thickness 0.45', 'height 2.70'],
    },
});

/** Distancia + dirección para Desplazar / Copiar. */
function moveVector(p) {
    const ms = measureBy(p, 'length') || unlabelled(p)[0];
    const d = maStlDesignAiMeasureMm(ms, 'length');
    return { d: d, deg: p.dirDeg };
}

registerDesignAiCommand({
    id: 'wall.move',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA' },
        { name: 'distancia', required: true, help: '«2 m», «50 cm»' },
        { name: 'dirección', required: true, help: 'derecha / izquierda / arriba / abajo / «a 30°»' },
    ],
    score: function (p) {
        if (otherVerb(p, ['move'])) return 0;
        return q(p, 'move');
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        const mv = moveVector(p);
        if (!(mv.d > 0) || mv.deg == null) return ask(s, this, p, 'distance', fmt(T.askDistance, T.verbsMove));
        const v = maStlDesignAiDirVector(mv.deg);
        const res = B.moveWall(w.id, v.x * mv.d, v.z * mv.d);
        if (!res || !res.ok) return { reply: fmt(T.moveFail, wallName(w, T)) };
        afterChange(s, res.wall || w);
        return { reply: fmt(T.moved, wallName(w, T), maStlDesignAiFmtM(mv.d, T.lang), dirLabel(mv.deg, T)), buttons: [undoButton(s, T)] };
    },
    help: { es: 'Desplazar un muro', en: 'Move a wall' },
    short: { es: 'mover el muro', en: 'move the wall' },
    examples: { es: ['mueve el muro 3 dos metros a la derecha', 'desplaza el seleccionado 50 cm hacia arriba'], en: ['move wall 3 two meters to the right', 'shift it 50 cm up'] },
});

registerDesignAiCommand({
    id: 'wall.copy',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA' },
        { name: 'distancia', required: true, help: '«3 m»' },
        { name: 'dirección / lado', required: true, help: 'derecha / izquierda / arriba / abajo; «en paralelo» = perpendicular al muro, hacia ese lado' },
    ],
    score: function (p) {
        if (otherVerb(p, ['copy'])) return 0;
        if (q(p, 'copy')) return q(p, 'copy');
        if (q(p, 'parallel') >= 0.95 && (q(p, 'wall') || p.measures.length) && !q(p, 'draw')) return 0.9;
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        const mv = moveVector(p);
        const parallel = q(p, 'parallel') >= 0.8 || !!p.parallel;
        let vx;
        let vz;
        let deg = mv.deg;
        if (parallel) {
            if (!(mv.d > 0)) return ask(s, this, Object.assign({}, p, { parallel: true }), 'distance', fmt(T.askDistance, T.verbsCopy));
            if (deg == null) return ask(s, this, Object.assign({}, p, { parallel: true }), 'side', T.askSide);
            const ax = wallAxis(w);
            const dv = maStlDesignAiDirVector(deg);
            let nx = -ax.uz;
            let nz = ax.ux;
            const dot = dv.x * nx + dv.z * nz;
            if (Math.abs(dot) < 0.2) return ask(s, this, Object.assign({}, p, { parallel: true, dirDeg: null }), 'side', T.askSide);
            if (dot < 0) { nx = -nx; nz = -nz; }
            vx = nx;
            vz = nz;
            deg = maStlDesignAiDirOf({ x: 0, z: 0 }, { x: nx, z: nz });
        } else {
            if (!(mv.d > 0) || deg == null) return ask(s, this, p, 'distance', fmt(T.askDistance, T.verbsCopy));
            const v = maStlDesignAiDirVector(deg);
            vx = v.x;
            vz = v.z;
        }
        const res = B.copyWall(w.id, vx * mv.d, vz * mv.d);
        if (!res || !res.ok) return { reply: fmt(T.copyFail, wallName(w, T)) };
        const c = res.walls && res.walls.length ? res.walls[0] : null;
        afterChange(s, c || w);
        const tpl = parallel ? T.copiedParallel : T.copied;
        return { reply: fmt(tpl, wallName(w, T), maStlDesignAiFmtM(mv.d, T.lang), dirLabel(deg, T), c ? wallName(c, T) : '?'), buttons: [undoButton(s, T)] };
    },
    help: { es: 'Copiar un muro (desplazado o en paralelo)', en: 'Copy a wall (moved or parallel)' },
    short: { es: 'copiar el muro', en: 'copy the wall' },
    examples: { es: ['copia el muro 2 tres metros hacia arriba', 'haz una copia en paralelo del muro 5 a 4 m hacia la derecha'], en: ['copy wall 2 three meters up', 'parallel copy of wall 5 at 4 m to the right'] },
});

registerDesignAiCommand({
    id: 'wall.delete',
    edits: true,
    params: [{ name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA (siempre pide confirmación)' }],
    score: function (p) {
        if (otherVerb(p, ['del'])) return 0;
        return q(p, 'del');
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        try { B.highlight(w.id); } catch (_e) { /* sin caras */ }
        const yes = function () {
            s.pending = null;
            const res = B.deleteWall(w.id);
            if (!res || !res.ok) return { reply: fmt(T.deleteFail, wallName(w, T)), lang: T.lang };
            if (String(s.lastWallId) === String(w.id)) s.lastWallId = null;
            return { reply: fmt(T.deleted, wallName(w, T)), lang: T.lang, buttons: [undoButton(s, T)] };
        };
        const no = function () {
            s.pending = null;
            B.clearHighlight();
            return { reply: T.deleteCancel, lang: T.lang };
        };
        s.pending = { yes: yes, no: no };
        return {
            reply: fmt(T.confirmDelete, wallName(w, T), dimsText(w, T)),
            buttons: [
                { label: T.yes, primary: true, run: function () { return s.pending && s.pending.yes === yes ? yes() : null; } },
                { label: T.no, run: function () { return s.pending && s.pending.no === no ? no() : null; } },
            ],
        };
    },
    help: { es: 'Borrar un muro (pide confirmación)', en: 'Delete a wall (asks first)' },
    short: { es: 'borrar el muro', en: 'delete the wall' },
    examples: { es: ['borra el muro 7', 'elimina el seleccionado'], en: ['delete wall 7', 'remove the selected wall'] },
});

registerDesignAiCommand({
    id: 'wall.extend',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA' },
        { name: 'extremo', required: false, help: '«por el final» / «por el inicio» (si no, prueba el final y luego el inicio)' },
        { name: 'hasta', required: false, help: '«hasta el siguiente muro» o «hasta el muro 5» (comprueba que llega a ese)' },
    ],
    score: function (p) {
        if (otherVerb(p, ['extend'])) return 0;
        const e = q(p, 'extend');
        if (!e) return 0;
        if (p.measures.some(function (m) { return !m.ref; }) && !p.toTarget) return 0;
        return e;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        let target = null;
        if (p.wallRefs.length > 1) {
            const rt = maStlDesignAiResolveWall(p, s, { ref: p.wallRefs[1] });
            if (rt.missing != null) return wallMissingReply(rt, T);
            target = rt.wall;
        }
        const first = p.end === 'start' ? axisEndKey(w, true) : axisEndKey(w, false);
        const order = p.end ? [first] : [first, first === 'p2' ? 'p1' : 'p2'];
        for (let i = 0; i < order.length; i++) {
            const res = B.extendWall(w.id, order[i]);
            if (!res || !res.ok) continue;
            const now = res.wall || w;
            if (target) {
                const endPt = order[i] === 'p2' ? now.p2 : now.p1;
                if (distToAxis(endPt, target) > Math.max(target.thicknessMm, now.thicknessMm) + 5) {
                    B.undo();
                    return { reply: fmt(T.extendNotTarget, wallName(w, T), wallName(target, T)) };
                }
            }
            afterChange(s, now);
            const endName = T.extendEnd[order[i] === axisEndKey(w, false) ? 'p2' : 'p1'];
            return { reply: fmt(T.extended, wallName(now, T), endName, maStlDesignAiFmtM(now.lengthMm, T.lang)), buttons: [undoButton(s, T)] };
        }
        return { reply: fmt(T.extendFail, wallName(w, T)) };
    },
    help: { es: 'Alargar un muro hasta el siguiente muro', en: 'Extend a wall up to the next wall' },
    short: { es: 'alargar el muro hasta el siguiente', en: 'extend the wall to the next one' },
    examples: { es: ['alarga el muro 3 hasta el siguiente muro', 'prolonga el seleccionado por el inicio', 'extiende el muro 2 hasta el muro 5'], en: ['extend wall 3 to the next wall', 'extend wall 2 up to wall 5'] },
});

/* ------------------- Girar / Simetría (14/11/2026): herramientas Girar y Simetría del visor ------------------- */

/** Texto sin tildes y en minúsculas (para «eje X», «2D», «3D», que el léxico parte en trozos). */
function plainText(p) {
    let t = String((p && p.text) || '').toLowerCase();
    try { t = t.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (_e) { /* sin normalize */ }
    return t;
}

function fmtDeg(v, T) {
    let d = Math.round(Math.abs(v) * 100) / 100;
    const s = String(d);
    return (T.lang === 'en' ? s : s.replace('.', ',')) + '°';
}

/** Grados con signo (+ antihorario): «90 grados», «-30°», «45° a la derecha» (−), «gira el muro 4 90» (número suelto). */
function rotateAngleOf(p) {
    let v = p.angleDeg != null ? p.angleDeg : null;
    if (v == null) {
        const free = p.measures.filter(function (m) { return !m.label && !m.unit; });
        if (free.length) v = free[free.length - 1].v * (free[free.length - 1].sign < 0 ? -1 : 1);
    }
    if (v == null) return null;
    if (p.turnSense) v = Math.abs(v) * p.turnSense;
    return v;
}

/**
 * El muro girado / reflejado entre los ejes nuevos: si la copia cruza el original, el visor parte los dos y
 * aparecen también tramos del original; se descartan los que siguen sobre la recta del original y se elige el
 * más cercano a donde cae el centro del original transformado (`mapPt`).
 */
function pickTransformedWall(list, w, mapPt) {
    if (!list || !list.length) return null;
    const a = w.p1;
    const b = w.p2;
    const ux = b.x - a.x;
    const uz = b.z - a.z;
    const l = Math.hypot(ux, uz) || 1;
    const off = function (pt) { return Math.abs((pt.x - a.x) * uz - (pt.z - a.z) * ux) / l; };
    const others = list.filter(function (x) { return !(off(x.p1) < 2 && off(x.p2) < 2); });
    const pool = others.length ? others : list;
    const m = mapPt({ x: (a.x + b.x) / 2, z: (a.z + b.z) / 2 });
    const dSeg = function (x) {
        const sx = x.p2.x - x.p1.x;
        const sz = x.p2.z - x.p1.z;
        const ll = sx * sx + sz * sz || 1;
        const t = Math.max(0, Math.min(1, ((m.x - x.p1.x) * sx + (m.z - x.p1.z) * sz) / ll));
        return Math.hypot(x.p1.x + sx * t - m.x, x.p1.z + sz * t - m.z);
    };
    return pool.slice().sort(function (x, y) { return dSeg(x) - dSeg(y); })[0];
}

function rotateMap(c, deg) {
    const r = (deg * Math.PI) / 180;
    const co = Math.cos(r);
    const si = Math.sin(r);
    return function (p) { const dx = p.x - c.x; const dz = p.z - c.z; return { x: c.x + dx * co + dz * si, z: c.z - dx * si + dz * co }; };
}

function mirrorMap(a, b) {
    const ux = b.x - a.x;
    const uz = b.z - a.z;
    const ll = ux * ux + uz * uz || 1;
    return function (p) {
        const t = ((p.x - a.x) * ux + (p.z - a.z) * uz) / ll;
        const fx = a.x + ux * t;
        const fz = a.z + uz * t;
        return { x: 2 * fx - p.x, z: 2 * fz - p.z };
    };
}

/** Punto de giro / de la recta de simetría: «sobre su inicio / final / centro», «sobre el origen», «sobre 2;3». Por defecto, el centro. */
function pivotOf(p, w, s, T) {
    const B = bridgeOf(s);
    if (p.xy) {
        const n = function (v) { return T.lang === 'en' ? String(v) : String(v).replace('.', ','); };
        const xy = n(p.xy.x) + ';' + n(p.xy.z);
        return { pt: { x: p.xy.x * 1000, z: p.xy.z * 1000 }, text: fmt(T.pivotPoint, xy), by: fmt(T.byPoint, xy) };
    }
    if (q(p, 'origin') >= 0.8) {
        const o = B.origin();
        return { pt: { x: o.x, z: o.z }, text: T.pivotOrigin, by: T.byOrigin };
    }
    /* Extremos del propio tramo (un muro partido por un cruce guarda el trazo entero en drawP1/drawP2). */
    if (p.end === 'start') { const e = w[axisEndKey(w, true)]; return { pt: { x: e.x, z: e.z }, text: T.pivotStart, by: T.byStart, key: 'start' }; }
    if (p.end === 'end') { const e = w[axisEndKey(w, false)]; return { pt: { x: e.x, z: e.z }, text: T.pivotEnd, by: T.byEnd, key: 'end' }; }
    return { pt: { x: (w.p1.x + w.p2.x) / 2, z: (w.p1.z + w.p2.z) / 2 }, text: T.pivotCenter, by: T.byCenter, key: 'center', isDefault: q(p, 'center') < 0.8 };
}

/** ¿Dejar el original? true (copia) / false (borrarlo) / null (no se ha dicho). */
function keepChoiceOf(p) {
    const t = plainText(p);
    if (/\b(sin borrar|no (lo |los )?borr|sin eliminar|no (lo |los )?elimin|without (deleting|erasing|removing)|(dont|don't|do not) (delete|erase|remove))\b/.test(t)) return true;
    if (q(p, 'keep') >= 0.8 || q(p, 'copy') >= 0.8) return true;
    if (q(p, 'del') >= 0.8 || /\b(borrando|eliminando|quitando|deleting|erasing|removing|sin (el |los )?origina(l|les)|sin copia|without (the )?originals?|replac)/.test(t)) return false;
    return null;
}

registerDesignAiCommand({
    id: 'wall.rotate',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 4» o el último de la IA' },
        { name: 'ángulo', required: true, help: '«90 grados», «45°», «-30°»; «a la izquierda» / antihorario = +, «a la derecha» / horario = −' },
        { name: 'punto', required: false, help: '«sobre su centro» (por defecto), «sobre su inicio», «sobre su final», «sobre el origen», «sobre 2;3»' },
        { name: 'copia', required: false, help: '«gira una copia…», «manteniendo el original» (por defecto se gira el muro)' },
    ],
    score: function (p) {
        if (otherVerb(p, ['rotate', 'copy'])) return 0;
        return q(p, 'rotate');
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const r = maStlDesignAiResolveWall(p, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        const ang = rotateAngleOf(p);
        if (ang == null || !Number.isFinite(ang)) return ask(s, this, p, 'angle', T.askAngle);
        if (Math.abs(ang % 360) < 1e-9) return { reply: T.rotateZero };
        const pv = pivotOf(p, w, s, T);
        const copy = keepChoiceOf(p) === true;
        const res = B.rotateWall(w.id, pv.pt, ang, { copy: copy });
        if (!res || !res.ok) return { reply: fmt(T.rotateFail, wallName(w, T)) };
        const sense = ang > 0 ? T.ccw : T.cw;
        if (copy) {
            const c = pickTransformedWall(res.walls, w, rotateMap(pv.pt, ang));
            afterChange(s, c || w);
            return { reply: fmt(T.rotatedCopy, wallName(w, T), fmtDeg(ang, T), sense, pv.text, c ? wallName(c, T) : '?'), buttons: [undoButton(s, T)] };
        }
        const now = res.wall || w;
        afterChange(s, now);
        return {
            reply: fmt(T.rotated, wallName(w, T), fmtDeg(ang, T), sense, pv.text) + (pv.isDefault ? '\n' + T.pivotDefaultNote : ''),
            buttons: [undoButton(s, T)],
        };
    },
    help: { es: 'Girar un muro (grados; sobre su centro, inicio o final; con copia)', en: 'Rotate a wall (degrees; about its centre, start or end; with copy)' },
    short: { es: 'girar el muro', en: 'rotate the wall' },
    examples: {
        es: ['gira el muro 4 90 grados', 'gira 45° a la izquierda', 'gira el muro 2 30 grados sobre su inicio', 'gira una copia del muro 3 90° sobre su final'],
        en: ['rotate wall 4 by 90 degrees', 'turn it 45 degrees clockwise', 'rotate wall 2 30 degrees about its start'],
    },
});

/**
 * Recta de simetría pedida: «respecto al eje X / Y» (ejes del origen), «respecto al muro 5» (su eje),
 * «vertical / horizontal por su extremo / inicio / centro» (por defecto el centro), «a 45° por su centro».
 */
function mirrorAxisSpecOf(pp) {
    const toks = pp.toks || [];
    const t = plainText(pp);
    const aboutAt = function (i) {
        for (let k = i - 1; k >= i - 4 && k >= 0; k--) {
            const tk = toks[k];
            if (tk && tk.t === 'w' && tk.c && tk.c.about >= 0.8) return true;
        }
        return false;
    };
    const refs = toks.filter(function (tk) { return tk.t === 'n' && tk.ref; });
    const aboutRef = refs.find(function (tk) { return aboutAt(tk.i); });
    if (aboutRef) return { kind: 'wall', ref: String(aboutRef.v), targetRef: refs.filter(function (x) { return x !== aboutRef; }).map(function (x) { return String(x.v); })[0] || null };
    if (refs.length >= 2) return { kind: 'wall', ref: String(refs[1].v), targetRef: String(refs[0].v) };
    if (/\b(eje|axis) ?(x|equis)\b|\bx[- ]?axis\b/.test(t)) return { kind: 'axisX' };
    if (/\b(eje|axis) ?(y|i griega|ye)\b|\by[- ]?axis\b/.test(t)) return { kind: 'axisY' };
    if (q(pp, 'vertical') >= 0.8) return { kind: 'vertical' };
    if (q(pp, 'horizontal') >= 0.8) return { kind: 'horizontal' };
    if (pp.angleDeg != null) return { kind: 'angle', deg: pp.angleDeg };
    return null;
}

registerDesignAiCommand({
    id: 'wall.mirror',
    edits: true,
    params: [
        { name: 'muro', required: true, help: 'el seleccionado, «el muro 3» o el último de la IA' },
        { name: 'eje', required: true, help: '«respecto al eje X / Y» (ejes del origen), «respecto al muro 5», «vertical / horizontal por su extremo / inicio / centro», «a 45° por su centro»' },
        { name: 'original', required: true, help: '«manteniendo el original» / «borrando el original»; si no se dice, pregunta' },
    ],
    score: function (p) {
        if (otherVerb(p, ['mirror', 'copy', 'del'])) return 0;
        return q(p, 'mirror');
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const spec = mirrorAxisSpecOf(p.axisAnswer || p);
        /* «respecto al muro 5»: ese es el eje; el muro a reflejar es el otro citado (o el seleccionado / el último). */
        const pr = spec && spec.kind === 'wall' && !p.axisAnswer
            ? Object.assign({}, p, { wallRef: spec.targetRef, wallRefs: spec.targetRef ? [spec.targetRef] : [] })
            : p;
        const r = maStlDesignAiResolveWall(pr, s);
        if (r.missing != null) return wallMissingReply(r, T);
        if (!r.wall) return ask(s, this, p, 'wall', T.askWall);
        const w = r.wall;
        if (!spec) return ask(s, this, p, 'axis', fmt(T.askMirrorAxis, wallName(w, T)));
        let a;
        let b;
        let axisText;
        if (spec.kind === 'wall') {
            const rr = maStlDesignAiResolveWall(p, s, { ref: spec.ref });
            if (rr.missing != null) return wallMissingReply(rr, T);
            a = rr.wall.p1;
            b = rr.wall.p2;
            axisText = fmt(T.axisWall, wallName(rr.wall, T));
        } else if (spec.kind === 'axisX' || spec.kind === 'axisY') {
            const o = B.origin();
            a = { x: o.x, z: o.z };
            b = spec.kind === 'axisX' ? { x: o.x + 1000, z: o.z } : { x: o.x, z: o.z - 1000 };
            axisText = spec.kind === 'axisX' ? T.axisX : T.axisY;
        } else {
            const at = pivotOf(p, w, s, T);
            const deg = spec.kind === 'vertical' ? 90 : spec.kind === 'horizontal' ? 0 : spec.deg;
            const v = maStlDesignAiDirVector(deg);
            a = at.pt;
            b = { x: a.x + v.x * 1000, z: a.z + v.z * 1000 };
            axisText = fmt(spec.kind === 'vertical' ? T.axisVertical : spec.kind === 'horizontal' ? T.axisHorizontal : T.axisAngle, at.by, fmtDeg(deg, T));
        }
        /* El muro sobre la propia recta: el reflejo es él mismo. */
        const dist = function (pt) {
            const ux = b.x - a.x;
            const uz = b.z - a.z;
            const l = Math.hypot(ux, uz) || 1;
            return Math.abs((pt.x - a.x) * uz - (pt.z - a.z) * ux) / l;
        };
        if (dist(w.p1) < 1 && dist(w.p2) < 1) return { reply: fmt(T.mirrorSame, wallName(w, T), axisText) };
        const doIt = function (keep) {
            s.pending = null;
            const res = B.mirrorWall(w.id, { x: a.x, z: a.z }, { x: b.x, z: b.z }, { keep: keep });
            if (!res || !res.ok) return { reply: fmt(T.mirrorFail, wallName(w, T)), lang: T.lang };
            /* Mantener: el reflejado es un eje nuevo; borrar: el mismo muro se refleja en su sitio. */
            const c = keep ? pickTransformedWall(res.walls, w, mirrorMap(a, b)) : null;
            const now = keep ? (c || w) : (res.wall || w);
            afterChange(s, now);
            const tpl = keep ? T.mirroredKeep : T.mirroredMove;
            return { reply: fmt(tpl, wallName(w, T), axisText, wallName(now, T)), lang: T.lang, buttons: [undoButton(s, T)] };
        };
        const keep = keepChoiceOf(p);
        if (keep != null) return doIt(keep);
        try { B.highlight(w.id); } catch (_e) { /* sin caras */ }
        const yes = function () { return doIt(false); };
        const no = function () { return doIt(true); };
        s.pending = { yes: yes, no: no };
        return {
            reply: fmt(T.askMirrorErase, wallName(w, T), axisText),
            buttons: [
                { label: T.eraseYes, primary: true, run: function () { return s.pending && s.pending.yes === yes ? yes() : null; } },
                { label: T.eraseNo, run: function () { return s.pending && s.pending.no === no ? no() : null; } },
            ],
        };
    },
    help: { es: 'Simetría de un muro (eje X/Y, otro muro o vertical/horizontal por un punto; mantener o borrar el original)', en: 'Mirror a wall (X/Y axis, another wall or vertical/horizontal through a point; keep or erase the original)' },
    short: { es: 'simetría del muro', en: 'mirror the wall' },
    examples: {
        es: ['haz simetría del muro 3 respecto al eje Y', 'simetría del muro 3 respecto al muro 5 manteniendo el original', 'simetría vertical del muro 2 por su extremo borrando el original'],
        en: ['mirror wall 3 about the Y axis', 'mirror wall 3 about wall 5 keeping the original'],
    },
});

/* ------------------- Modos de la barra (14/11/2026): Líneas · 3D · Encofrar, con sus botones ------------------- */

/** '2d' | '3d' | null: «2D», «3 d», «tres de» (dictado) junto a palabras de modo. */
function dimOf(p) {
    const t = plainText(p);
    const m = /(^|[^a-z0-9])([23]) ?d($|[^a-z])/.exec(t);
    if (m) return m[2] === '2' ? '2d' : '3d';
    const v = /\b(dos|two|tres|three) (d|de|di)$/.exec(t.replace(/[^a-z0-9 ]/g, ' ').replace(/\s+/g, ' ').trim());
    if (v && (q(p, 'mode') || q(p, 'go') || q(p, 'gen3d') || /^(pasa|pasar|vista|ver|modo|genera|generar)\b/.test(t.trim()))) {
        return v[1] === 'dos' || v[1] === 'two' ? '2d' : '3d';
    }
    return null;
}

/** «colores» / «color» junto a encofrar o a un cambio de modo. «encofra» a secas no entra. */
function formworkColorAsked(p) {
    const t = plainText(p);
    return q(p, 'formworkColor') >= 0.8 || /\bcolou?rs?\b/.test(t);
}

/** Modo pedido: 'lines' | 'wall3d' | 'formwork' | 'formworkColor' | null. */
export function maStlDesignAiModeTarget(p) {
    const t = plainText(p);
    const color = formworkColorAsked(p);
    if (p.phrase && MODE_BY_CODE[p.phrase.code] && (p.phrase.len >= 2 || p.phrase.kind === 'learned')) {
        const coded = MODE_BY_CODE[p.phrase.code];
        if (!(coded === 'formwork' && color)) return coded;
    }
    if (color && (q(p, 'formwork') >= 0.8 || q(p, 'mode') >= 0.8 || q(p, 'go') >= 0.8 || q(p, 'change') >= 0.8
        || /\ben ?cofr/.test(t) || p.words.length <= 3)) return 'formworkColor';
    if (q(p, 'formwork') >= 0.8 || /\ben ?cofr/.test(t)) return 'formwork';
    const dim = dimOf(p);
    if (dim === '3d') return 'wall3d';
    if (q(p, 'gen3d') >= 0.8 && (q(p, 'mode') >= 0.8 || q(p, 'go') >= 0.8 || p.words.length <= 3)) return 'wall3d';
    if (dim === '2d') return 'lines';
    if (q(p, 'lines') >= 0.8 && (q(p, 'mode') >= 0.8 || q(p, 'go') >= 0.8 || p.words.length <= 2)) return 'lines';
    return null;
}

registerDesignAiCommand({
    id: 'mode',
    params: [{ name: 'modo', required: true, help: '«modo líneas» / «pasa a 2D» · «genera 3D» / «modo 3D» · «encofra» / «modo encofrar» · «encofrado colores» (Encofrar pide confirmación)' }],
    score: function (p) {
        if (!maStlDesignAiModeTarget(p)) return 0;
        if (otherVerb(p, ['change', 'list', 'show'])) return 0;
        return 0.95;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        if (!B || !B.isReady() || typeof B.setMode !== 'function') return { reply: T.notReady };
        const target = maStlDesignAiModeTarget(p);
        const info = B.modeInfo();
        const name = T.modeNames[target];
        if (info.busy) return { reply: T.modeBusy };
        if (target === 'lines') {
            if (info.raw === 'lines') return { reply: fmt(T.modeAlready, name) };
            return B.setMode('lines').then(function (r) {
                return { reply: r && r.ok ? T.modeLinesDone : fmt(T.modeFail, name) };
            });
        }
        if (target === 'wall3d') {
            if (info.mode === 'wall3d') return { reply: fmt(T.modeAlready, name) };
            const fromFormwork = info.formwork;
            return B.setMode('wall3d').then(function (r) {
                if (!r || r.busy) return { reply: T.modeBusy };
                if (!r.ok) return { reply: fmt(T.mode3dFail, r.message || '') };
                const head = r.count != null ? fmt(T.mode3dDone, r.count) : T.mode3dDoneCached;
                return { reply: head + (fromFormwork ? '\n' + T.mode3dFromFormwork : '') };
            });
        }
        /* Encofrar y Encofrado colores: todo el diseño, puede tardar → Sí / No antes. */
        const color = target === 'formworkColor';
        const run = function () {
            s.pending = null;
            return B.setMode(color ? 'formworkColor' : 'formwork').then(function (r) {
                if (!r || r.busy) return { reply: T.modeBusy, lang: T.lang };
                if (r.cancelled) return { reply: (color ? T.formworkColorCancelled : T.formworkCancelled), lang: T.lang };
                if (!r.ok) return { reply: fmt(color ? T.formworkColorFail : T.formworkFail, r.message || ''), lang: T.lang };
                const done = fmt(color ? T.formworkColorDone : T.formworkDone, r.pieces || 0);
                return { reply: done + (r.message ? ' ' + r.message : ''), lang: T.lang };
            });
        };
        const no = function () { s.pending = null; return { reply: color ? T.formworkColorNo : T.formworkNo, lang: T.lang }; };
        s.pending = { yes: run, no: no };
        const already = color ? !!info.color : !!info.formwork;
        return {
            reply: already
                ? (color ? T.formworkColorAlready : T.formworkAlready)
                : fmt(color ? T.confirmFormworkColor : T.confirmFormwork, info.walls),
            buttons: [
                { label: T.yes, primary: true, pendingText: color ? T.formworkColorRunning : T.formworkRunning, run: function () { return s.pending && s.pending.yes === run ? run() : null; } },
                { label: T.no, run: function () { return s.pending && s.pending.no === no ? no() : null; } },
            ],
        };
    },
    help: { es: 'Cambiar de modo: líneas (2D), 3D, encofrar o encofrado colores (con los botones de la barra)', en: 'Switch mode: lines (2D), 3D, formwork or color formwork (same as the toolbar buttons)' },
    short: { es: 'cambiar de modo', en: 'switch mode' },
    examples: { es: ['modo líneas', 'pasa a 2D', 'genera 3D', 'encofra', 'encofrado colores'], en: ['line mode', 'generate 3D', 'formwork', 'color formwork'] },
});

/* ------------------------- Cotas por voz (pt114u, 15/11/2026) ------------------------- */

const IDS_RE = /\b(numera|numerar|numeralos|numerame|numeramelos|numeracion|numeros? de (los |las )?(muros|paredes)|(muros|paredes) numerad[oa]s|number (the )?walls|wall numbers|numbering)\b/;
const MEASURES_RE = /\b(medidas|dimensiones|dimensions|measurements|sizes)\b/;
function idsQ(p) { return Math.max(q(p, 'ids'), IDS_RE.test(p.norm) ? 1 : 0); }
function hideQ(p) { return Math.max(q(p, 'hide'), q(p, 'del')); }
function wallsPlural(p) { return p.words.some(function (w) { return /^(muros|paredes|tabiques|walls)$/.test(w); }); }
/** «acótalo», «mídelo», «acótamelo»: el muro del que se habla. */
function pronounRef(p) { return p.words.some(function (w) { return /^(acota|mide|dimensiona|ensena|muestra)(me)?(lo|la)$/.test(w) || w === 'it'; }); }
function cotasReady(B) { return !!(B && typeof B.showWallIds === 'function' && typeof B.dimensionWall === 'function'); }

/* «Muéstrame los id de los muros», «identifica los muros», «numera los muros», «enséñame los muros», «¿qué muros hay?»
 * → Cotas con solo el ID de cada muro (el número que vale para «el muro 7»). «Oculta los id» / «quita los números» lo quita. */
registerDesignAiCommand({
    id: 'wall.ids',
    params: [{ name: 'mostrar / ocultar', required: false, help: '«muéstrame los id», «oculta los id»' }],
    score: function (p) {
        if (otherVerb(p, ['ids', 'hide', 'del', 'show', 'list'])) return 0;
        if (articleQ(p) >= 0.8) return 0;
        const ids = idsQ(p);
        const hide = hideQ(p) >= 0.8;
        if (ids >= 0.8) return hide ? 1 : (q(p, 'dims') >= 0.8 ? 0 : ids * 0.99);
        if (q(p, 'dims') >= 0.8) return 0;
        if (/\b(que muros hay|que paredes hay|which walls|what walls)\b/.test(p.norm)) return 1;
        const sh = q(p, 'show');
        if (sh >= 0.8 && wallsPlural(p) && p.wallRef == null && !hide) return sh * 0.99;
        /* «ocúltalos» / «quítalos» justo después: lo último que puso la IA. */
        if (q(p, 'hide') >= 0.8 && p.words.length <= 2) return 0.9;
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        if (!B || !B.isReady()) return { reply: T.notReady };
        if (!cotasReady(B)) return { reply: T.cotasFail };
        if (hideQ(p) >= 0.8) {
            const st = B.cotasState ? B.cotasState() : null;
            if (st && st.mode === 'wall') {
                const r0 = B.clearCotas(false);
                return { reply: r0 && r0.changed ? T.dimsHidden : T.dimsNone };
            }
            const r = B.showWallIds(false);
            return { reply: r && r.changed ? T.idsHidden : T.idsNotShown };
        }
        const walls = B.walls();
        if (!walls.length) return { reply: T.noWalls };
        const r = B.showWallIds(true);
        if (!r || !r.ok) return { reply: T.cotasFail };
        return { reply: fmt(r.already ? T.idsAlready : T.idsShown, walls.length, wallList(walls), walls[0].label) };
    },
    help: { es: 'Ver u ocultar el número (ID) de cada muro en el plano', en: 'Show or hide the number (ID) of each wall on the plan' },
    short: { es: 'mostrar los ID de los muros', en: 'show the wall IDs' },
    examples: {
        es: ['muéstrame los id de los muros', 'identifica los muros', 'numera los muros', 'enséñame los muros', '¿qué muros hay?', 'oculta los id'],
        en: ['show the wall ids', 'number the walls', 'label the walls', 'hide the ids'],
    },
});

/* «Muéstrame los id de los paneles», «id de artículo», «numera los paneles» → tipo de cota «Id de Artículo» (pt116u): cada
 * cota de panel lleva su ID («0,90 · 7.3» = muro 7, 3.ª columna de paneles). «Oculta los id de los paneles» lo quita.
 * Solo el interruptor; las órdenes sobre un ID («oculta el 7.3», «opacidad», «pon dos de 0,45») vendrán después
 * (el puente ya expone articleIds() y findArticle('7.3')). */
const ARTICLE_NUM_RE = /\b(numera|numerar|numeralos|numerame|numeramelos|numeracion|number|numbering)\b/;
function articleQ(p) { return q(p, 'article'); }
registerDesignAiCommand({
    id: 'panel.ids',
    params: [{ name: 'mostrar / ocultar', required: false, help: '«muéstrame los id de los paneles», «oculta los id de artículo»' }],
    score: function (p) {
        if (otherVerb(p, ['ids', 'hide', 'del', 'show', 'list'])) return 0;
        const art = articleQ(p);
        if (art < 0.8) return 0;
        if (idsQ(p) >= 0.8 || ARTICLE_NUM_RE.test(p.norm)) return Math.min(1, art);
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        if (!B || !B.isReady()) return { reply: T.notReady };
        if (typeof B.showArticleIds !== 'function') return { reply: T.cotasFail };
        if (hideQ(p) >= 0.8) {
            const r0 = B.showArticleIds(false);
            return { reply: r0 && r0.changed ? T.articleIdsHidden : T.articleIdsNotShown };
        }
        const r = B.showArticleIds(true);
        if (!r) return { reply: T.cotasFail };
        if (r.noFormwork) return { reply: T.articleIdsNoFormwork };
        if (!r.ok) return { reply: T.cotasFail };
        if (r.noneSelected) {
            const ws = B.walls();
            return { reply: fmt(T.articleIdsPick, ws.length ? ws[0].label : '1') };
        }
        const first = (r.sample || '').split(/[–,…]/)[0].trim() || '1.1';
        if (r.already) return { reply: fmt(T.articleIdsAlready, r.count, r.sample || '—') };
        return { reply: fmt(T.articleIdsShown, r.count, r.walls, r.sample || '—', first) };
    },
    help: { es: 'Ver u ocultar el ID de cada panel junto a su cota («0,90 · 7.3»)', en: 'Show or hide the ID of each panel next to its dimension («0.90 · 7.3»)' },
    short: { es: 'mostrar los ID de los paneles', en: 'show the panel IDs' },
    examples: {
        es: ['muéstrame los id de los paneles', 'id de artículo', 'numera los paneles', 'oculta los id de los paneles'],
        en: ['show the panel ids', 'article ids', 'hide the panel ids'],
    },
});

/* «Acótame el muro 7», «cota del muro 7», «mide el muro 7», «dime las medidas del muro 7» → Cotas de ese muro, que
 * queda como muro actual («longitud 4,5» / «el espesor es 0,45» lo cambian). Sin muro: las Cotas de todo el diseño.
 * «Quita las cotas» las quita (también las del botón). */
registerDesignAiCommand({
    id: 'wall.dims',
    params: [{ name: 'muro', required: false, help: '«el muro 7», el seleccionado o «acótalo»; sin muro, todo el diseño' }],
    score: function (p) {
        if (otherVerb(p, ['dims', 'hide', 'del', 'show', 'list', 'ids'])) return 0;
        if (idsQ(p) >= 0.8 && q(p, 'dims') < 0.8) return 0;
        const d = q(p, 'dims');
        if (d >= 0.8) return p.measures.length ? 0 : d;
        if (MEASURES_RE.test(p.norm) && (p.wallRef != null || p.selectedRef || p.lastRef) && !p.measures.length) return 0.95;
        return d;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        if (!B || !B.isReady()) return { reply: T.notReady };
        if (!cotasReady(B)) return { reply: T.cotasFail };
        if (hideQ(p) >= 0.8) {
            const st = B.cotasState ? B.cotasState() : { active: true };
            const r0 = B.clearCotas(true);
            return { reply: (r0 && r0.changed) || (st && st.active) ? T.dimsHidden : T.dimsNone };
        }
        const walls = B.walls();
        if (!walls.length) return { reply: T.noWalls };
        const named = p.wallRef != null || p.selectedRef || p.lastRef || pronounRef(p);
        let w = null;
        if (named) {
            const r = maStlDesignAiResolveWall(p, s);
            if (r.missing != null) return wallMissingReply(r, T);
            w = r.wall || null;
        } else if (!/\b(todo|todos|todas|entero|completo|all|everything|whole)\b/.test(p.norm)) {
            /* «acota» a secas: el muro seleccionado (o el último que tocó la IA); «acota todo»: todo el diseño. */
            w = B.selectedWall();
        }
        const allQ = /\b(todo|todos|todas|entero|completo|all|everything|whole)\b/.test(p.norm);
        if (!w && !named && !allQ && typeof B.showCotas === 'function') {
            /* pt117u: lo normal es acotar el muro seleccionado; sin selección, los números de los muros para elegir. */
            const r1 = B.showCotas();
            if (!r1 || !r1.ok) return { reply: T.cotasFail };
            return { reply: fmt(T.dimsPick, walls[0].label, wallList(walls)) };
        }
        if (!w) {
            const all = B.showAllCotas ? B.showAllCotas() : { ok: false };
            if (!all || !all.ok) return { reply: T.cotasFail };
            return { reply: fmt(T.dimsAll, walls[0].label) };
        }
        const res = B.dimensionWall(w.id);
        if (!res || !res.ok) return { reply: T.cotasFail };
        const now = res.wall || w;
        afterChange(s, now);
        return { reply: fmt(T.dimsShown, wallName(now, T), dimsText(now, T)) };
    },
    help: { es: 'Acotar un muro (sus cotas en el plano; luego «longitud 4,5», «espesor 0,45» o «altura 3»)', en: 'Dimension a wall (then «length 4.5», «thickness 0.45» or «height 3»)' },
    short: { es: 'acotar el muro', en: 'dimension the wall' },
    examples: {
        es: ['acótame el muro 7', 'cota del muro 7', 'mide el muro 7', 'dime las medidas del muro 7', 'acota', 'acota todo', 'quita las cotas'],
        en: ['dimension wall 7', 'measure wall 7', 'show the dimensions of wall 7', 'dimension everything', 'hide the dimensions'],
    },
});

registerDesignAiCommand({
    id: 'wall.list',
    params: [{ name: 'muro', required: false, help: 'sin número, la lista; con número, lo señala' }],
    score: function (p) {
        if (otherVerb(p, ['list', 'show'])) return 0;
        const l = q(p, 'list');
        const sh = q(p, 'show');
        if (l && (q(p, 'wall') || p.words.length <= 2)) return Math.min(l, q(p, 'wall') || l);
        /* «muéstrame los muros» (sin número) = sus ID en el plano (wall.ids); «señala el muro 4» = este. */
        if (sh && (p.wallRef || (q(p, 'wall') && !wallsPlural(p)))) return Math.min(sh, p.wallRef ? 1 : q(p, 'wall'));
        return 0;
    },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const walls = B.walls();
        if (!walls.length) return { reply: T.noWalls };
        if (p.wallRef != null || (q(p, 'show') && (p.selectedRef || p.lastRef))) {
            const r = maStlDesignAiResolveWall(p, s);
            if (r.missing != null) return wallMissingReply(r, T);
            if (r.wall) {
                afterChange(s, r.wall);
                return { reply: fmt(T.shown, wallName(r.wall, T), dimsText(r.wall, T)) };
            }
        }
        const rows = walls.map(function (w) {
            return { wall: w.label, length: maStlDesignAiFmtM(w.lengthMm, T.lang), thickness: maStlDesignAiFmtM(w.thicknessMm, T.lang), height: maStlDesignAiFmtM(w.heightMm, T.lang) };
        });
        return {
            reply: fmt(T.listTitle, walls.length),
            table: {
                columns: [
                    { key: 'wall', label: T.cols.wall, align: 'left' },
                    { key: 'length', label: T.cols.length, align: 'right' },
                    { key: 'thickness', label: T.cols.thickness, align: 'right' },
                    { key: 'height', label: T.cols.height, align: 'right' },
                ],
                rows: rows,
                labels: { copy: T.copy, copied: T.copied2, copyFail: T.copyFail2 },
                maxHeight: '260px',
            },
        };
    },
    help: { es: 'Listar los muros con su número, o señalar uno', en: 'List the walls with their number, or show one' },
    short: { es: 'listar los muros', en: 'list the walls' },
    examples: { es: ['lista los muros', 'señala el muro 4', '¿cuántos muros hay?'], en: ['list the walls', 'show wall 4'] },
});

registerDesignAiCommand({
    id: 'undo',
    score: function (p) { return p.words.length <= 5 && !otherVerb(p, ['undo']) ? q(p, 'undo') : 0; },
    run: function (p, s, T) { return undoReply(s, T); },
    help: { es: 'Deshacer / rehacer el último paso', en: 'Undo / redo the last step' },
    short: { es: 'deshacer', en: 'undo' },
    examples: { es: ['deshaz', 'rehaz'], en: ['undo', 'redo'] },
});

registerDesignAiCommand({
    id: 'redo',
    score: function (p) { return p.words.length <= 5 && !otherVerb(p, ['redo']) ? q(p, 'redo') : 0; },
    run: function (p, s, T) {
        const B = bridgeOf(s);
        const ok = B.redo();
        if (ok === false) return { reply: T.nothingRedo };
        B.clearHighlight();
        return { reply: T.redone };
    },
    help: null,
    short: { es: 'rehacer', en: 'redo' },
    examples: { es: ['rehaz'], en: ['redo'] },
});

/* Lo que el visor no tiene: no se inventa; queda como propuesta (ia-desing.md, «Propuestas»). */
registerDesignAiCommand({
    id: 'notAvailable',
    score: function (p) {
        const v = anyOf(p, ['fillet', 'trim']);
        return v >= 0.8 ? v * 0.99 : v;
    },
    run: function (p, s, T) {
        const which = ['fillet', 'trim'].reduce(function (best, k) { return q(p, k) > q(p, best) ? k : best; }, 'fillet');
        /* Recortar sí existe como herramienta: la voz la activa; el clic en el tramo sigue siendo del usuario. */
        if (which === 'trim') {
            return runCatalogCmd({
                code: 'TOOL_TRIM', handler: 'toolbar:trim', confirm: false,
                nameEs: T.proposals.trim, nameEn: T.proposals.trim,
            }, s, T);
        }
        s.proposals = (s.proposals || []).concat([which]);
        return { reply: fmt(T.notAvailable, T.proposals[which]) };
    },
    help: null,
    short: { es: 'empalme / recortar', en: 'fillet / trim' },
    examples: { es: ['haz un empalme entre el muro 1 y el 2'], en: ['fillet wall 1 and wall 2'] },
});

registerDesignAiCommand({
    id: 'thanks',
    score: function (p) { return p.words.length <= 4 ? q(p, 'thanks') : 0; },
    run: function (p, s, T) { return { reply: T.thanks }; },
    help: null,
    short: { es: 'gracias', en: 'thanks' },
    examples: { es: ['gracias'], en: ['thanks'] },
});

/* Entorno de conversación (pt107u): «modo coloquial», «habla como un colega», «ponte serio», «technical mode»…
 * Frases ENV_* del catálogo (gana la más larga) o, sin catálogo, las de ai-command-catalog.js. Se guarda en BD. */
function envTargetOf(p) { return maStlAiCatalogEnvTarget(p.norm, CATALOG_PHRASES, phraseWordMatch); }

registerDesignAiCommand({
    id: 'env',
    score: function (p) { return envTargetOf(p) ? 0.99 : 0; },
    run: function (p, s, T) {
        const code = envTargetOf(p);
        const lang = T.lang;
        const cur = maStlAiCatalogEnv();
        if (cur && cur.code === code) return { reply: fmt(T.envAlready, maStlAiCatalogEnvLabel(code, lang)) };
        return maStlAiCatalogSaveEnv(code, { lang: lang }).then(function (r) {
            if (!r || !r.ok) return { reply: texts(lang).envFail };
            /* La confirmación ya sale con la voz del entorno nuevo (catálogo recargado). */
            return { reply: fmt(texts(lang).envChanged, maStlAiCatalogEnvLabel(code, lang)) };
        });
    },
    help: null,
    short: { es: 'cambiar el entorno de conversación', en: 'change the conversation environment' },
    examples: { es: ['modo coloquial', 'ponte serio'], en: ['casual mode', 'be formal'] },
});

/**
 * Handler ejecutable del catálogo: solo la parte antes de «|» y solo toolbar / click / key conocidos.
 * `ai:…` (geometría ya interpretada) y cualquier selector raro devuelven null.
 * @returns {{ kind: 'toolbar'|'click'|'key', arg: string } | null}
 */
export function maStlDesignAiParseHandler(raw) {
    const head = String(raw || '').split('|')[0].trim();
    const i = head.indexOf(':');
    if (i < 1) return null;
    const kind = head.slice(0, i);
    const arg = head.slice(i + 1).trim();
    if (kind === 'toolbar' && /^[a-z0-9-]{1,40}$/.test(arg)) return { kind: 'toolbar', arg: arg };
    if (kind === 'click' && /^[#\[][^\s]{1,80}$/.test(arg)) return { kind: 'click', arg: arg };
    if (kind === 'key' && /^(Enter|Escape|Delete|F8)$/.test(arg)) return { kind: 'key', arg: arg };
    return null;
}

/** Casos borde del parser (toolbar con nota, selector con comillas, tecla inválida, handler de geometría). */
export function maStlDesignAiHandlerSelfCheck() {
    const cases = [
        ['toolbar:polyline|maStlDesing2ActivateToolbarCommandById(\'polyline\')', 'toolbar', 'polyline'],
        ['click:[data-face="top"]', 'click', '[data-face="top"]'],
        ['key:Enter', 'key', 'Enter'],
        ['key:F9', null, null],
        ['ai:notAvailable', null, null],
        ['toolbar:../x', null, null],
        ['click:javascript:alert(1)', null, null],
    ];
    const bad = [];
    cases.forEach(function (c) {
        const got = maStlDesignAiParseHandler(c[0]);
        const ok = c[1] == null ? !got : !!(got && got.kind === c[1] && got.arg === c[2]);
        if (!ok) bad.push(c[0]);
    });
    return bad;
}

/** Casos borde de la revisión: orden, vacío, empalme, frase ya de otro comando no se inventa, otro idioma vacío. */
export function maStlDesignAiLearnSelfCheck() {
    const bad = [];
    const cmds = [
        { code: 'WALL_DIMS', context: 'design', implemented: true, handler: 'ai:wall.dims|click:#ma-stl-tool-wall-dim', aiId: 'wall.dims', nameEs: 'Cotas', nameEn: 'Dimensions', phrases: [{ p: 'acota el muro', k: 'keyword' }] },
        { code: 'WALL_FILLET', context: 'design', implemented: false, handler: 'ai:notAvailable', aiId: 'notAvailable', nameEs: 'Empalme', nameEn: 'Fillet', phrases: [{ p: 'empalme', k: 'keyword' }] },
        { code: 'ENV_STREET', context: 'design', implemented: true, handler: 'ai:env', aiId: 'env', nameEs: 'Colega', nameEn: 'Street', phrases: [] },
    ];
    if (!maStlDesignAiIsLearnReview('Analiza el .json') || !maStlDesignAiIsLearnReview('analyze the json')) bad.push('trigger');
    if (maStlDesignAiIsLearnReview('analiza el muro')) bad.push('trigger-false');
    const learnable = maStlDesignAiLearnableCommands(cmds);
    if (learnable.length !== 1 || learnable[0].code !== 'WALL_DIMS') bad.push('learnable');
    if (maStlDesignAiPendingDesign([]).length || maStlDesignAiPendingDesign([{ id: 1, estado: 'pendiente', asistente: 'wall', frase: 'x' }]).length) bad.push('empty');
    if (maStlDesignAiPendingDesign([{ id: 2, estado: 'pendiente', asistente: 'design', frase: 'abre cotas' }]).length !== 1) bad.push('pending');
    if (maStlDesignAiPendingDesign([{ id: 3, estado: 'pendiente', asistente: 'design', frase: '   ' }]).length) bad.push('blank');
    const prop = maStlDesignAiProposeLearnCommand('acota el muro', learnable);
    if (!prop || prop.cmd.code !== 'WALL_DIMS') bad.push('propose');
    if (maStlDesignAiProposeLearnCommand('pon un tejado verde', learnable)) bad.push('propose-none');
    if (maStlDesignAiMatchLearnCommand('cotas', learnable).length !== 1) bad.push('match');
    if (maStlDesignAiMatchLearnCommand('empalme', learnable).length) bad.push('fillet-learnable');
    if (maStlDesignAiMatchLearnCommand('empalme', cmds.filter(function (c) { return c.code === 'WALL_FILLET'; })).length !== 1) bad.push('fillet-blocked');
    if (maStlDesignAiLearnScope(true) !== 'default' || maStlDesignAiLearnScope(false) !== 'user') bad.push('scope');
    if (maStlDesignAiOtherPhrase('no', 'es') !== '' || maStlDesignAiOtherPhrase('sí', 'es') !== null) bad.push('other-empty');
    if (maStlDesignAiOtherPhrase('open the dimensions', 'es') !== 'open the dimensions') bad.push('other-text');
    const one = phraseWordMatch('cotas', 'cotes');
    const two = phraseWordMatch('cotas', 'cotxx');
    if (!(one >= 0.8 && one < 0.95) || !(two < 0.8)) bad.push('margin');
    if (!bestPhraseHit({ len: 2, q: 1 }, { len: 3, q: 0.8 }) || bestPhraseHit({ len: 2, q: 1 }, { len: 3, q: 0.8 }).len !== 3) bad.push('longer');
    const prev = maStlAiCatalogGet('design');
    try {
        maStlDesignAiApplyCatalog({
            commands: [{
                code: 'WALL_DIMS', context: 'design', implemented: true, handler: 'ai:wall.dims|click:#ma-stl-tool-wall-dim',
                aiId: 'wall.dims', nameEs: 'Cotas', nameEn: 'Dimensions', confirm: false,
                phrases: [{ p: 'abre el formulario de cotas', k: 'learned' }, { p: 'cotas', k: 'learned' }],
            }, {
                code: 'WALL_FILLET', context: 'design', implemented: false, handler: 'ai:notAvailable', aiId: 'notAvailable',
                nameEs: 'Empalme', nameEn: 'Fillet', phrases: [{ p: 'haz un empalme', k: 'learned' }],
            }],
        });
        const exact = maStlDesignAiParse('abre el formulario de cotas');
        const fuzzy = maStlDesignAiParse('abre el formulario de cotes');
        const two = maStlDesignAiParse('abre el formulario de cotxx');
        const stolen = maStlDesignAiParse('oculta las cotas');
        if (!exact.catalogCmd || exact.catalogCmd.code !== 'WALL_DIMS' || exact.catalogCmd.kind !== 'learned') bad.push('apply');
        if (!fuzzy.catalogCmd || fuzzy.catalogCmd.code !== 'WALL_DIMS') bad.push('one-letter');
        if (two.catalogCmd) bad.push('two-letters');
        if (stolen.catalogCmd) bad.push('short-steal');
        if (maStlDesignAiParse('haz un empalme').catalogCmd) bad.push('fillet-catalog');
        const best = maStlDesignAiBestCommand(exact);
        if (!best || best.cmd.id !== 'catalog' || best.score < 1) bad.push('score');
    } finally {
        maStlDesignAiApplyCatalog(prev);
    }
    return bad;
}

/**
 * Pulsa el botón o la tecla que ya usa el visor. No inventa geometría.
 * @returns {{ ok: boolean, reason?: string, replyKey?: string, after?: function }}
 */
export function maStlDesignAiInvokeHandler(cmd, doc) {
    doc = doc || (typeof document !== 'undefined' ? document : null);
    if (!cmd || !doc) return { ok: false, reason: 'no-doc' };
    const code = cmd.code || '';
    const w = doc.defaultView || null;
    const ai = w && w.maStlDesignAi;
    if (code === 'AI_OPEN_DESIGN') {
        if (ai && ai.isOpen && ai.isOpen()) return { ok: true, replyKey: 'aiAlready' };
        if (ai && ai.open) { ai.open(); return { ok: true, replyKey: 'aiOpened' }; }
    }
    if (code === 'AI_CLOSE') {
        return {
            ok: true, replyKey: 'aiClosed', after: function () {
                try { if (ai && ai.close) ai.close(); } catch (_e) { /* panel */ }
                try { if (w && w.maStlWallAi && w.maStlWallAi.close) w.maStlWallAi.close(); } catch (_e2) { /* muro */ }
            },
        };
    }
    if (code === 'AI_MIC_TOGGLE' || code === 'AI_SPEAKER_TOGGLE') {
        if (ai && ai.open && (!ai.isOpen || !ai.isOpen())) ai.open();
        const sel = code === 'AI_MIC_TOGGLE' ? '[data-ma-stl-wall-ai-mic]' : '[data-ma-stl-wall-ai-spk]';
        const root = doc.getElementById('ma-stl-design-ai') || doc;
        const el = root.querySelector(sel) || doc.querySelector(sel);
        if (!el) return { ok: false, reason: 'missing' };
        el.click();
        return { ok: true };
    }
    const act = maStlDesignAiParseHandler(cmd.handler);
    if (!act) return { ok: false, reason: 'unsupported' };
    if (act.kind === 'toolbar') {
        const id = act.arg.indexOf('ma-stl-') === 0 ? act.arg : 'ma-stl-tool-' + act.arg;
        const el = doc.getElementById(id);
        if (!el) return { ok: false, reason: 'missing' };
        el.click();
        return { ok: true };
    }
    if (act.kind === 'click') {
        const root = doc.getElementById('ma-stl-design-ai');
        const el = (root && root.querySelector(act.arg)) || doc.querySelector(act.arg);
        if (!el) return { ok: false, reason: 'missing' };
        el.click();
        return { ok: true };
    }
    const ae = doc.activeElement;
    if (ae && ae.blur && /^(INPUT|TEXTAREA)$/.test(ae.tagName)) ae.blur();
    if (!w || typeof w.KeyboardEvent !== 'function') return { ok: false, reason: 'no-doc' };
    w.dispatchEvent(new w.KeyboardEvent('keydown', { key: act.arg, code: act.arg, bubbles: true, cancelable: true }));
    return { ok: true };
}

function catalogReply(c, s, T, invoked) {
    const name = T.lang === 'en' ? (c.nameEn || c.code) : (c.nameEs || c.code);
    if (invoked && invoked.after) {
        const w = typeof window !== 'undefined' ? window : null;
        if (w && w.setTimeout) w.setTimeout(invoked.after, 400);
    }
    if (invoked && invoked.replyKey && T[invoked.replyKey]) return { reply: T[invoked.replyKey], lang: T.lang };
    if (!invoked || !invoked.ok) {
        if (invoked && invoked.reason === 'unsupported') return { reply: fmt(T.pendingAi, name), lang: T.lang };
        return { reply: fmt(T.toolMissing, name), lang: T.lang };
    }
    return { reply: fmt(T.toolDone, name), lang: T.lang };
}

function runCatalogCmd(c, s, T) {
    const name = T.lang === 'en' ? (c.nameEn || c.code) : (c.nameEs || c.code);
    const go = function () {
        s.pending = null;
        return catalogReply(c, s, T, maStlDesignAiInvokeHandler(c));
    };
    if (c.confirm) {
        const no = function () { s.pending = null; return { reply: T.toolNo, lang: T.lang }; };
        s.pending = { yes: go, no: no };
        return {
            reply: fmt(T.confirmTool, name),
            buttons: [
                { label: T.yes, primary: true, run: function () { return s.pending && s.pending.yes === go ? go() : null; } },
                { label: T.no, run: function () { return s.pending && s.pending.no === no ? no() : null; } },
            ],
        };
    }
    return go();
}

/** Parte toolbar/click/key de un handler (`ai:wall.dims|click:#…` → la parte que se puede pulsar). */
function executableHandler(raw) {
    const parts = String(raw || '').split('|');
    for (let i = 0; i < parts.length; i++) {
        if (maStlDesignAiParseHandler(parts[i])) return parts[i].trim();
    }
    return '';
}

/**
 * Frase aprendida: ejecuta el comando ya registrado (aiId), el modo, o el botón/tecla del handler.
 * No evalúa el texto del handler ni crea geometría.
 */
function runLearnedCmd(c, p, s, T) {
    if (c && MODE_BY_CODE[c.code]) {
        const mode = MA_STL_DESIGN_AI_COMMANDS.find(function (x) { return x.id === 'mode'; });
        if (mode) return mode.run(p, s, T);
    }
    const id = c && c.aiId && c.aiId !== 'notAvailable' ? c.aiId : '';
    const cmd = id ? MA_STL_DESIGN_AI_COMMANDS.find(function (x) { return x.id === id; }) : null;
    if (cmd && cmd.id !== 'catalog' && cmd.id !== 'notAvailable' && cmd.id !== 'help') return cmd.run(p, s, T);
    const exec = executableHandler(c && c.handler);
    if (exec) return runCatalogCmd(Object.assign({}, c, { handler: exec, kind: 'keyword' }), s, T);
    const name = T.lang === 'en' ? (c.nameEn || c.code) : (c.nameEs || c.code);
    return { reply: fmt(T.pendingAi, name), lang: T.lang };
}

/* Herramientas, vistas y botones del catálogo: se pulsa el mismo control que la barra (no se inventa geometría). */
registerDesignAiCommand({
    id: 'catalog',
    score: function (p) {
        const c = p.catalogCmd;
        if (!c || /^(ENV_|COMMON_)/.test(c.code)) return 0;
        if (c.kind === 'learned') return /notAvailable/i.test(c.handler || '') || c.aiId === 'notAvailable' ? 0 : 1.01;
        if (!maStlDesignAiParseHandler(c.handler)) return 0;
        if (c.code === 'AI_OPEN_DESIGN' || c.code === 'AI_CLOSE' || c.code === 'AI_MIC_TOGGLE' || c.code === 'AI_SPEAKER_TOGGLE') return 0.98;
        if (c.len >= 2) return 0.98;
        return p.words.length <= 3 && !otherVerb(p, []) && q(p, 'draw') < 0.8 ? 0.9 : 0;
    },
    run: function (p, s, T) {
        return p.catalogCmd && p.catalogCmd.kind === 'learned' ? runLearnedCmd(p.catalogCmd, p, s, T) : runCatalogCmd(p.catalogCmd, s, T);
    },
    help: null,
    short: null,
    examples: null,
});

/* ------------------------------ Conversación ------------------------------ */

const EN_HINTS = new Set(['draw', 'wall', 'walls', 'move', 'copy', 'delete', 'remove', 'the', 'to', 'right', 'left', 'up', 'down', 'long', 'thick',
    'high', 'tall', 'from', 'undo', 'redo', 'list', 'show', 'help', 'meters', 'meter', 'another', 'turning', 'by', 'it', 'make', 'set', 'extend',
    'next', 'yes', 'no', 'please', 'thanks', 'what', 'can', 'you', 'do', 'length', 'thickness', 'height', 'parallel', 'of', 'at', 'and', 'rotate', 'mirror',
    'mode', 'line', 'lines', 'generate', 'formwork', 'color', 'colors', 'coloured', 'colored', 'axis', 'about', 'keep', 'keeping', 'original', 'clockwise', 'counterclockwise', 'degrees', 'switch',
    'dimension', 'dimensions', 'measure', 'hide', 'label', 'number', 'numbers']);
const ES_HINTS = new Set(['dibuja', 'muro', 'muros', 'pared', 'mueve', 'copia', 'borra', 'el', 'la', 'los', 'de', 'del', 'hacia', 'derecha', 'izquierda',
    'arriba', 'abajo', 'largo', 'espesor', 'alto', 'desde', 'deshaz', 'lista', 'ayuda', 'metros', 'metro', 'otro', 'girando', 'en', 'un', 'una',
    'haz', 'ponle', 'cambia', 'alarga', 'siguiente', 'si', 'gracias', 'que', 'hasta', 'y', 'con', 'gira', 'paralelo',
    'modo', 'linea', 'lineas', 'genera', 'encofra', 'encofrar', 'colores', 'color', 'eje', 'respecto', 'sobre', 'simetria', 'grados', 'pasa', 'centro', 'inicio', 'final', 'manteniendo', 'borrando',
    'acota', 'acotame', 'cota', 'cotas', 'mide', 'longitud', 'altura', 'oculta', 'identifica', 'muestrame', 'ensename', 'cambiame', 'es']);

/** Idioma del mensaje: palabras de los comandos + el detector del muro; si empata, el de la conversación. */
export function maStlDesignAiLang(text, fallback) {
    const words = maStlWallAiNormalize(text).split(' ').filter(Boolean);
    let es = 0;
    let en = 0;
    words.forEach(function (w) { if (EN_HINTS.has(w)) en++; if (ES_HINTS.has(w)) es++; });
    if (/[ñáéíóú¿¡]/i.test(String(text || ''))) es += 2;
    let d = null;
    try { d = maStlWallAiDetectLanguage(text, fallback); } catch (_e) { d = null; }
    if (d && d.decided) { if (d.lang === 'en') en += 1; else es += 1; }
    if (es === en) return fallback === 'en' ? 'en' : 'es';
    return en > es ? 'en' : 'es';
}

/** La respuesta a una pregunta pendiente completa la orden (largo, dirección, muro, distancia, lado, qué medida). */
function mergeAnswer(prevP, ans, need) {
    const p = Object.assign({}, prevP);
    p.q = Object.assign({}, prevP.q);
    p.has = Object.assign({}, prevP.has);
    /* Simetría: la respuesta dice el eje (y quizá el punto y si se borra el original); el muro sigue siendo el de la orden. */
    if (need === 'axis') {
        p.axisAnswer = ans;
        p.text = prevP.text + ' ' + ans.text;
        if (ans.end) p.end = ans.end;
        if (ans.xy) p.xy = ans.xy;
        ['center', 'origin', 'keep', 'copy', 'del'].forEach(function (c) { if (ans.q[c]) { p.q[c] = ans.q[c]; p.has[c] = true; } });
        return p;
    }
    if (ans.angleDeg != null) p.angleDeg = ans.angleDeg;
    if (ans.turnSense) p.turnSense = ans.turnSense;
    const ms = ans.measures.slice();
    if (need === 'what' && !ms.some(function (m) { return m.label; })) {
        const lab = labelOfWord({ t: 'w', c: conceptsOf((ans.words[0] || '')) });
        if (lab && prevP.measures.length) {
            ms.push(Object.assign({}, prevP.measures.find(function (m) { return !m.label; }) || prevP.measures[0], { label: lab }));
        }
    }
    if ((need === 'length' || need === 'distance') && ms.length && !ms[0].label) ms[0] = Object.assign({}, ms[0], { label: 'length' });
    p.measures = need === 'what' ? ms.concat(prevP.measures.filter(function (m) { return m.label; })) : prevP.measures.concat(ms);
    if (ans.dirDeg != null) { p.dirDeg = ans.dirDeg; p.dirFromAngle = ans.dirFromAngle; }
    if (ans.relTurn != null) p.relTurn = ans.relTurn;
    if (ans.straight) p.straight = true;
    if (ans.start) p.start = ans.start;
    if (ans.end) p.end = ans.end;
    if (ans.wallRef != null) { p.wallRef = ans.wallRef; p.wallRefs = ans.wallRefs; }
    if (need === 'wall' && p.wallRef == null) {
        const n = ans.toks.find(function (t) { return t.t === 'n' && Number.isInteger(t.v) && !t.unit; });
        if (n) { p.wallRef = String(n.v); p.wallRefs = [p.wallRef]; p.measures = prevP.measures; }
    }
    if (need === 'wall' && ans.selectedRef) p.selectedRef = true;
    ['more', 'less', 'parallel'].forEach(function (c) { if (ans.q[c]) { p.q[c] = ans.q[c]; p.has[c] = true; } });
    return p;
}

/** ¿El mensaje responde a la pregunta pendiente? */
function answersAsk(ans, need) {
    if (need === 'length' || need === 'distance') return ans.measures.length > 0 || (need === 'distance' && ans.dirDeg != null);
    if (need === 'dir' || need === 'side') return ans.dirDeg != null || ans.relTurn != null || ans.straight;
    if (need === 'wall') return ans.wallRef != null || ans.selectedRef || ans.toks.some(function (t) { return t.t === 'n'; });
    if (need === 'what') return ans.measures.length > 0 || !!labelOfWord({ t: 'w', c: conceptsOf(ans.words[0] || '') });
    if (need === 'angle') return ans.angleDeg != null || ans.measures.some(function (m) { return !m.label; });
    if (need === 'axis') return !!mirrorAxisSpecOf(ans);
    return false;
}

/** Sesión nueva (al abrir/cerrar el panel): último muro de la IA, pregunta pendiente, confirmación pendiente. */
export function maStlDesignAiNewSession(bridge) {
    return { bridge: bridge || null, lastWallId: null, ask: null, pending: null, lang: null, proposals: [], learn: null };
}

function runCmd(cmd, p, s, T) {
    const B = bridgeOf(s);
    let pre = '';
    let hadFormwork = false;
    if (cmd.edits) {
        if (!B || !B.isReady()) return { reply: T.notReady };
        try {
            const m = B.ensureLinesMode();
            if (m && m.changed) pre = T.switchedLines + '\n';
        } catch (_e) { /* el visor decide */ }
        try { hadFormwork = !!(B.hasFormwork && B.hasFormwork()); } catch (_e) { hadFormwork = false; }
    }
    s.ask = null;
    const out = cmd.run(p, s, T) || { reply: '' };
    /* Órdenes que esperan al visor (modos 3D / Encofrar): la respuesta llega con la promesa. */
    if (typeof out.then === 'function') {
        return out.then(function (r) {
            const res = r || { reply: '' };
            if (pre) res.reply = pre + (res.reply || '');
            res.command = cmd.id;
            return res;
        });
    }
    const res = out;
    if (pre) res.reply = pre + (res.reply || '');
    /* Alargar (y lo que el visor trate igual) quita el encofrado puesto: se avisa; no se re-encofra solo. */
    let lostFormwork = false;
    try { lostFormwork = hadFormwork && !B.hasFormwork(); } catch (_e) { lostFormwork = false; }
    if (lostFormwork) res.reply = (res.reply || '') + '\n' + T.formworkCleared;
    res.command = cmd.id;
    return res;
}

/* ------------------------- Revisión «analiza el .json» ------------------------- */

/** «analiza el .json» / «analyze the json». No vale «analiza el muro». */
export function maStlDesignAiIsLearnReview(text) {
    const n = maStlWallAiNormalize(text);
    return n === 'analiza el json' || n === 'analiza el archivo' || n === 'analiza el archivo json'
        || n === 'analyze the json' || n === 'analyze the file' || n === 'review the json';
}

/** Comandos del diseño que se pueden ejecutar. Empalme y no implementados no entran. */
export function maStlDesignAiLearnableCommands(commands) {
    return (commands || []).filter(function (c) {
        if (!c || (c.context && c.context !== 'design')) return false;
        if (/^(ENV_|COMMON_)/.test(c.code || '')) return false;
        if (!c.implemented) return false;
        if (c.aiId === 'notAvailable' || /notAvailable/i.test(c.handler || '')) return false;
        return true;
    });
}

/** Pendientes del diseño con frase e id. Las del muro no se preguntan aquí. */
export function maStlDesignAiPendingDesign(items) {
    return (items || []).filter(function (e) {
        const st = String(e && e.estado || '').toLowerCase();
        const as = String(e && e.asistente || '').toLowerCase();
        return st === 'pendiente' && (!as || as === 'design') && String(e && e.frase || '').trim() && e.id != null;
    });
}

/** Programador → defecto de todos. Si no, solo el usuario. El servidor vuelve a imponerlo. */
export function maStlDesignAiLearnScope(programmer) {
    return programmer ? 'default' : 'user';
}

/**
 * Otro idioma escrito por la persona. '' = no hay otra forma. null = ha dicho sí, pero aún no la ha escrito.
 * No traduce.
 */
export function maStlDesignAiOtherPhrase(text, lang) {
    const n = maStlWallAiNormalize(text);
    const yn = maStlWallAiYesNo(text, lang);
    if (!n) return '';
    if (yn === 'no' || n === 'igual' || n === 'la misma' || n === 'same' || n === 'no hay') return '';
    if (yn === 'yes' && n.split(' ').length <= 2) return null;
    return String(text || '').trim();
}

function learnCmdName(c, lang) {
    return lang === 'en' ? (c.nameEn || c.code) : (c.nameEs || c.code);
}

function learnBaseLang(raw) {
    return String(raw || '').toLowerCase().indexOf('en') === 0 ? 'en' : 'es';
}

function otherLangLabel(phraseLang, uiLang) {
    const other = phraseLang === 'en' ? 'es' : 'en';
    if (uiLang === 'en') return other === 'en' ? 'English' : 'Spanish';
    return other === 'en' ? 'inglés' : 'español';
}

/** Coincidencia cercana (una falta vale) para proponer un comando que ya existe. */
export function maStlDesignAiProposeLearnCommand(phrase, commands) {
    const words = maStlWallAiNormalize(phrase).split(' ').filter(Boolean);
    if (!words.length) return null;
    const phrases = [];
    (commands || []).forEach(function (c) {
        (c.phrases || []).forEach(function (ph) {
            if ((ph.k || 'keyword') === 'negative') return;
            const n = maStlWallAiNormalize(ph.p || ph.n || '');
            if (n) phrases.push({ cmd: c, toks: n.split(' ') });
        });
        [c.nameEs, c.nameEn, String(c.code || '').replace(/_/g, ' ')].forEach(function (label) {
            const n = maStlWallAiNormalize(label);
            if (n) phrases.push({ cmd: c, toks: n.split(' ') });
        });
    });
    const hit = maStlAiCatalogLongest(words, phrases, phraseWordMatch, 0.8);
    if (!hit) return null;
    if (hit.len < 2 && words.length > 2) return null;
    return { cmd: hit.item.cmd, q: hit.q, len: hit.len };
}

/** El nombre o el código que dice la persona. Varios si hay empate. No inventa un comando. */
export function maStlDesignAiMatchLearnCommand(text, commands) {
    const words = maStlWallAiNormalize(text).split(' ').filter(Boolean);
    if (!words.length) return [];
    const joined = words.join(' ');
    const compact = words.join('');
    const hits = [];
    (commands || []).forEach(function (c) {
        const codeNorm = maStlWallAiNormalize(String(c.code || '').replace(/_/g, ' '));
        if (codeNorm.replace(/ /g, '') === compact || codeNorm === joined) { hits.push(c); return; }
        const names = [c.nameEs, c.nameEn].map(function (n) { return maStlWallAiNormalize(n); }).filter(Boolean);
        if (names.some(function (n) { return n === joined; })) { hits.push(c); return; }
        if (words.length <= 4 && names.some(function (n) { return n.length >= 4 && words.indexOf(n) >= 0; })) hits.push(c);
    });
    const uniq = [];
    hits.forEach(function (c) { if (!uniq.some(function (x) { return x.code === c.code; })) uniq.push(c); });
    if (uniq.length) return uniq;
    const phrases = [];
    (commands || []).forEach(function (c) {
        (c.phrases || []).forEach(function (ph) {
            const n = maStlWallAiNormalize(ph.p || '');
            if (n) phrases.push({ cmd: c, toks: n.split(' ') });
        });
    });
    const hit = maStlAiCatalogLongest(words, phrases, phraseWordMatch, 0.95);
    if (hit && (hit.len >= words.length - 1) && (hit.len >= 2 || words.length <= 2)) return [hit.item.cmd];
    return [];
}

function aiEndpoint(action, doc) {
    const c = maStlAiCatalogUrls(doc).commands || '';
    return /\/AiCommands(\?.*)?$/i.test(c) ? c.replace(/\/AiCommands(\?.*)?$/i, '/' + action) : '';
}

function aiJson(url, method, body) {
    const f = typeof fetch === 'function' ? fetch : null;
    if (!url || !f) return Promise.resolve(null);
    const opt = {
        method: method, credentials: 'same-origin',
        headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
    };
    if (body != null) {
        opt.headers['Content-Type'] = 'application/json; charset=utf-8';
        opt.body = JSON.stringify(body);
    }
    return Promise.resolve(f(url, opt)).then(function (r) {
        if (!r || typeof r.json !== 'function') return null;
        return r.json().then(function (j) { return j; }, function () { return null; });
    }, function () { return null; });
}

function learnControl(text) {
    const n = maStlWallAiNormalize(text);
    const words = n.split(' ').filter(Boolean);
    if (!words.length || words.length > 3) return null;
    if (words.length === 1 && /^(para|parar|stop|cancela|cancelar|cancel|dejalo|basta)$/.test(words[0])) return 'stop';
    if (/^(descarta|descartar|descartala|discard)$/.test(words[0])) return 'discard';
    if (/^(siguiente|saltar|salta|skip|pasala)$/.test(words[0])) return 'skip';
    return null;
}

function learnButtons(s, T, withYes) {
    const L = s.learn;
    const mine = function (fn) {
        return function () { return s.learn === L ? fn() : null; };
    };
    const btns = [];
    if (withYes) btns.push({ label: T.yes, primary: true, run: mine(function () { return L.proposal ? beginLearnOther(s, L.proposal.cmd, T) : null; }) });
    btns.push({ label: T.learnDiscardBtn, run: mine(function () { return discardLearn(s, T); }) });
    btns.push({ label: T.learnSkipBtn, run: mine(function () { return skipLearn(s, T); }) });
    btns.push({ label: T.learnStopBtn, run: mine(function () { return stopLearn(s, T); }) });
    return btns;
}

function learnCatalogCommands() {
    const data = maStlAiCatalogGet('design');
    return (data && data.commands) || [];
}

function askLearnCurrent(s, T) {
    const L = s.learn;
    if (!L || L.i >= L.items.length) {
        s.learn = null;
        return { reply: T.learnDone, lang: T.lang };
    }
    const item = L.items[L.i];
    L.step = 'ask';
    L.code = null;
    L.cmd = null;
    L.phrase = String(item.frase || '').trim();
    L.lang = learnBaseLang(item.idioma || T.lang);
    const proposal = maStlDesignAiProposeLearnCommand(L.phrase, maStlDesignAiLearnableCommands(learnCatalogCommands()));
    L.proposal = proposal;
    const head = fmt(T.learnAsk, L.phrase, L.i + 1, L.items.length);
    const tail = proposal ? fmt(T.learnGuess, learnCmdName(proposal.cmd, T.lang)) : T.learnWhich;
    const intro = L.i === 0 ? fmt(T.learnIntro, L.items.length, L.programmer ? T.learnScopeAll : T.learnScopeUser) + '\n' : '';
    return { reply: intro + head + ' ' + tail, lang: T.lang, buttons: learnButtons(s, T, !!proposal) };
}

function stopLearn(s, T) {
    s.learn = null;
    s.pending = null;
    return { reply: T.learnStopped, lang: T.lang };
}

function skipLearn(s, T) {
    if (!s.learn) return stopLearn(s, T);
    s.learn.i += 1;
    const next = askLearnCurrent(s, T);
    return { reply: T.learnSkipped + '\n' + next.reply, lang: T.lang, buttons: next.buttons };
}

function beginLearnOther(s, cmd, T) {
    const L = s.learn;
    if (!L || !cmd) return { reply: T.learnWhich, lang: T.lang, buttons: learnButtons(s, T, false) };
    L.cmd = cmd;
    L.code = cmd.code;
    L.step = 'other';
    L.proposal = null;
    return {
        reply: fmt(T.learnOther, L.phrase, learnCmdName(cmd, T.lang), otherLangLabel(L.lang, T.lang)),
        lang: T.lang,
        buttons: [
            { label: T.no, run: function () { return s.learn === L ? saveLearn(s, T, '') : null; } },
            { label: T.learnSkipBtn, run: function () { return s.learn === L ? skipLearn(s, T) : null; } },
            { label: T.learnStopBtn, run: function () { return s.learn === L ? stopLearn(s, T) : null; } },
        ],
    };
}

function learnFailReply(s, T, j) {
    const L = s.learn;
    if (L) { L.step = 'name'; L.proposal = null; }
    if (j && j.reason === 'conflict') return { reply: fmt(T.learnConflict, j.command || ''), lang: T.lang, buttons: learnButtons(s, T, false) };
    if (j && j.reason === 'unavailable') {
        const name = L && L.cmd ? learnCmdName(L.cmd, T.lang) : '';
        return { reply: fmt(T.learnUnavailable, name), lang: T.lang, buttons: learnButtons(s, T, false) };
    }
    return { reply: fmt(T.learnSaveFail, (j && (j.message || j.reason)) || ''), lang: T.lang, buttons: learnButtons(s, T, false) };
}

function afterLearn(s, T, head) {
    if (!s.learn) return { reply: head, lang: T.lang };
    s.learn.i += 1;
    const next = askLearnCurrent(s, T);
    return { reply: head + '\n' + next.reply, lang: T.lang, buttons: next.buttons };
}

function saveLearn(s, T, otherPhrase) {
    const L = s.learn;
    if (!L || !L.code) return { reply: T.learnWhich, lang: T.lang, buttons: learnButtons(s, T, false) };
    const url = aiEndpoint('LearnAiUnknownPhrase');
    if (!url) return { reply: fmt(T.learnSaveFail, ''), lang: T.lang };
    const other = otherPhrase && maStlWallAiNormalize(otherPhrase) === maStlWallAiNormalize(L.phrase) ? '' : String(otherPhrase || '').slice(0, 200);
    return aiJson(url, 'POST', {
        id: L.items[L.i].id,
        code: L.code,
        phrase: String(L.phrase || '').slice(0, 200),
        lang: L.lang,
        otherPhrase: other,
        scope: maStlDesignAiLearnScope(L.programmer),
    }).then(function (j) {
        if (!j || !j.ok) return learnFailReply(s, T, j);
        const name = L.cmd ? learnCmdName(L.cmd, T.lang) : L.code;
        const scope = j.scope === 'default' ? T.learnScopeSavedAll : T.learnScopeSavedUser;
        const head = fmt(T.learnSaved, scope, L.phrase, name);
        return maStlAiCatalogLoad('design', T.lang, { force: true }).then(function () {
            return afterLearn(s, T, head);
        }, function () { return afterLearn(s, T, head); });
    });
}

function discardLearn(s, T) {
    const L = s.learn;
    if (!L) return stopLearn(s, T);
    const url = aiEndpoint('LearnAiUnknownPhrase');
    if (!url) return { reply: fmt(T.learnSaveFail, ''), lang: T.lang };
    return aiJson(url, 'POST', { id: L.items[L.i].id, estado: 'descartada' }).then(function (j) {
        if (!j || !j.ok) return learnFailReply(s, T, j);
        return afterLearn(s, T, T.learnDiscarded);
    });
}

function startLearnReview(s, T) {
    const url = aiEndpoint('AiUnknownPhrases');
    if (!url) return { reply: T.learnFail, lang: T.lang };
    return aiJson(url + (url.indexOf('?') >= 0 ? '&' : '?') + 'estado=pendiente', 'GET', null).then(function (j) {
        if (!j || !j.ok || !Array.isArray(j.items)) return { reply: T.learnFail, lang: T.lang };
        const items = maStlDesignAiPendingDesign(j.items);
        if (!items.length) {
            const other = j.items.some(function (e) { return String(e && e.estado || '') === 'pendiente'; });
            return { reply: other ? T.learnOtherPending : T.learnEmpty, lang: T.lang };
        }
        s.learn = { items: items, i: 0, step: 'ask', programmer: !!j.programmer, proposal: null, code: null, cmd: null, phrase: '', lang: 'es' };
        return askLearnCurrent(s, T);
    });
}

function answerLearnReview(s, text, T) {
    const L = s.learn;
    if (!L) return { reply: T.learnDone, lang: T.lang };
    const act = learnControl(text);
    if (act === 'stop') return stopLearn(s, T);
    if (L.step === 'other' || L.step === 'otherWait') {
        if (act === 'skip') return skipLearn(s, T);
        if (act === 'discard') return discardLearn(s, T);
        const other = maStlDesignAiOtherPhrase(text, T.lang);
        if (other == null) {
            L.step = 'otherWait';
            return { reply: T.learnTypeOther, lang: T.lang, buttons: [
                { label: T.no, run: function () { return s.learn === L ? saveLearn(s, T, '') : null; } },
                { label: T.learnStopBtn, run: function () { return s.learn === L ? stopLearn(s, T) : null; } },
            ] };
        }
        return saveLearn(s, T, other);
    }
    if (act === 'discard') return discardLearn(s, T);
    if (act === 'skip') return skipLearn(s, T);
    const yn = maStlWallAiYesNo(text, T.lang);
    if (yn === 'yes' && L.proposal && L.step === 'ask') return beginLearnOther(s, L.proposal.cmd, T);
    if (yn === 'no' && L.step === 'ask') {
        L.step = 'name';
        L.proposal = null;
        return { reply: T.learnWhich, lang: T.lang, buttons: learnButtons(s, T, false) };
    }
    const all = learnCatalogCommands();
    const learnable = maStlDesignAiLearnableCommands(all);
    const hits = maStlDesignAiMatchLearnCommand(text, learnable);
    if (hits.length === 1) return beginLearnOther(s, hits[0], T);
    if (hits.length > 1) {
        return { reply: fmt(T.learnMany, hits.map(function (c) { return learnCmdName(c, T.lang); }).join(', ')), lang: T.lang, buttons: learnButtons(s, T, false) };
    }
    const blocked = maStlDesignAiMatchLearnCommand(text, all.filter(function (c) { return learnable.indexOf(c) < 0; }));
    if (blocked.length) return { reply: fmt(T.learnUnavailable, learnCmdName(blocked[0], T.lang)), lang: T.lang, buttons: learnButtons(s, T, false) };
    return { reply: fmt(T.learnUnknownCmd, String(text || '').trim()), lang: T.lang, buttons: learnButtons(s, T, false) };
}

/**
 * Intérprete del chat de diseño: confirmación pendiente → respuesta a una pregunta → mejor comando
 * (≥ 0,8 se ejecuta; 0,65..0,8 «¿Querías decir…?») → saludo / no entendido + ayuda.
 * «analiza el .json» revisa las frases pendientes antes que el resto.
 */
export function maStlDesignAiHandleMessage(session, text, history) {
    const s = session;
    const hist = Array.isArray(history) ? history.slice(0, -1) : [];
    const convLang = s.lang || hist.slice().reverse().reduce(function (acc, h) { return acc || (h && h.lang) || null; }, null) || maStlWallAiUiLang();
    const lang = maStlDesignAiLang(text, convLang);
    s.lang = lang;
    const T = texts(lang);
    const tag = function (res) {
        if (res && typeof res.then === 'function') return res.then(tag);
        if (res && !res.lang) res.lang = lang;
        /* Plantillas del entorno que empiezan por un hueco («{0} creado…» → «Muro 4 creado…»). */
        if (res && typeof res.reply === 'string' && /^[a-zà-ÿ]/.test(res.reply) && Object.keys(maStlAiCatalogReplies(lang)).length) {
            res.reply = res.reply.charAt(0).toUpperCase() + res.reply.slice(1);
        }
        return res;
    };
    /* «analiza el .json»: revisar frases no comprendidas antes que cualquier otra orden. */
    if (maStlDesignAiIsLearnReview(text)) {
        s.pending = null;
        s.ask = null;
        s.learn = null;
        return tag(startLearnReview(s, T));
    }
    if (s.learn) return tag(answerLearnReview(s, text, T));
    const p = maStlDesignAiParse(text);
    /* «sí» / «no» a una confirmación («¿Borro el muro 4?», «¿Querías decir…?»). */
    if (s.pending) {
        const yn = maStlWallAiYesNo(text, lang);
        if (yn === 'yes') return tag(s.pending.yes());
        if (yn === 'no') return tag(s.pending.no());
        s.pending = null;
    }
    if (q(p, 'cancel') >= 0.8 && p.words.length <= 3) {
        s.ask = null;
        s.pending = null;
        return tag({ reply: T.cancelled });
    }
    const best = maStlDesignAiBestCommand(p);
    /* Respuesta a la pregunta pendiente, salvo que sea claramente otra orden. */
    if (s.ask) {
        const a = s.ask;
        const ownVerb = EDIT_VERBS.concat(['draw']).some(function (v) { return q(p, v) >= 0.65; });
        const other = best && best.score >= 0.8 && best.cmd.id !== 'help' && (best.cmd !== a.cmd || ownVerb);
        if (!other && answersAsk(p, a.need)) return tag(runCmd(a.cmd, mergeAnswer(a.p, p, a.need), s, T));
        if (!other && best && best.cmd === a.cmd) return tag(runCmd(a.cmd, mergeAnswer(a.p, p, a.need), s, T));
        s.ask = null;
    }
    if (best && best.score >= 0.8) return tag(runCmd(best.cmd, p, s, T));
    if (best && best.score >= 0.65 && best.cmd.short) {
        const cmd = best.cmd;
        const yes = function () { s.pending = null; return tag(runCmd(cmd, p, s, T)); };
        const no = function () { s.pending = null; return tag(MA_STL_DESIGN_AI_COMMANDS[0].run(p, s, T)); };
        s.pending = { yes: yes, no: no };
        return tag({
            reply: fmt(T.didYouMean, cmd.short[lang]),
            buttons: [
                { label: T.yes, primary: true, run: function () { return s.pending && s.pending.yes === yes ? yes() : null; } },
                { label: T.no, run: function () { return s.pending && s.pending.no === no ? no() : null; } },
            ],
        });
    }
    if (q(p, 'greet') >= 0.8 && p.words.length <= 4) {
        return tag({ reply: T.helpHead + '\n' + MA_STL_DESIGN_AI_COMMANDS.filter(function (c) { return c.help && c.examples; })
            .slice(0, 3).map(function (c) { return '\u2022 «' + c.examples[lang][0] + '»'; }).join('\n') + '\n' + T.helpTail });
    }
    const help = MA_STL_DESIGN_AI_COMMANDS[0].run(p, s, T);
    help.reply = fmt(T.notUnderstood, p.text) + '\n' + help.reply;
    help.speech = fmt(T.notUnderstood, p.text) + ' ' + T.helpSpeech;
    logUnknown(s, text, lang, fmt(T.notUnderstood, p.text));
    return tag(help);
}

/**
 * «No he entendido…» (pt119u): guarda la frase en el servidor (App_Data/IA/frases-no-comprendidas.json) con el modo
 * del visor y el muro seleccionado. Sin esperar y sin avisar si falla.
 */
function logUnknown(s, text, lang, reply) {
    try {
        const B = bridgeOf(s);
        let modo = null;
        let muro = null;
        try { if (B && B.modeInfo) modo = B.modeInfo().mode || null; } catch (_e) { modo = null; }
        try { const w = B && B.selectedWall ? B.selectedWall() : null; if (w) muro = w.label != null ? String(w.label) : String(w.id); } catch (_e) { muro = null; }
        const w = typeof window !== 'undefined' ? window : null;
        const fn = (s && s.logUnknown) || (w && typeof w.__maStlAiLogUnknown === 'function' ? w.__maStlAiLogUnknown : maStlAiLogUnknownPhrase);
        Promise.resolve(fn({ frase: String(text || ''), asistente: 'design', idioma: lang, contexto: { modo: modo, muroSeleccionado: muro, respuesta: reply } }))
            .catch(function () { return false; });
    } catch (_e) { /* nunca molesta al usuario */ }
}

/* ------------------------------ Panel (botón IA) ------------------------------ */

const CLOSE_SVG = '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true" focusable="false"><path fill="currentColor" d="M12 10.586l4.95-4.95 1.414 1.414-4.95 4.95 4.95 4.95-1.414 1.414-4.95-4.95-4.95 4.95-1.414-1.414 4.95-4.95-4.95-4.95L7.05 5.636z"/></svg>';

/**
 * Monta el panel «Asistente IA – Diseño» y lo engancha al botón IA de la barra de modos (`#ma-stl-mode-ia`).
 * Mismo formato que el modo compacto del asistente del muro: fijo a la derecha del visor, 450 × 800 (o lo que quepa),
 * sin backdrop. El botón abre/cierra (toggle); al cerrar se apaga el micrófono y la voz y se empieza conversación nueva.
 * Un solo asistente a la vez: al abrir se cierra el del muro (si está en modo compacto) y, si se abre el del muro,
 * este se cierra (evento `ma-stl-ai-chat-shown`).
 * @param {{ bridge: object, button?: HTMLElement, doc?: Document }} opts
 */
export function maStlDesignAiInit(opts) {
    opts = opts || {};
    const doc = opts.doc || document;
    const bridge = opts.bridge;
    if (typeof window !== 'undefined' && window.maStlDesignAi && window.maStlDesignAi.root && window.maStlDesignAi.root.isConnected) {
        return window.maStlDesignAi;
    }
    const lang = maStlWallAiUiLang();
    const T = texts(lang);
    let session = maStlDesignAiNewSession(bridge);

    const root = doc.createElement('div');
    root.id = 'ma-stl-design-ai';
    root.className = 'ma-stl-design-ai';
    root.setAttribute('role', 'dialog');
    root.setAttribute('aria-label', T.title);
    root.style.cssText = 'position:fixed;z-index:2147483200;display:none;flex-direction:column;box-sizing:border-box;'
        + 'background:#ffffff;border:1px solid #cbd5e1;border-radius:10px;box-shadow:0 12px 32px rgba(15,23,42,0.28);overflow:hidden;';

    const head = doc.createElement('div');
    head.className = 'ma-stl-design-ai__head';
    head.style.cssText = 'display:flex;align-items:center;gap:8px;padding:8px 10px 8px 14px;background:#0f172a;color:#ffffff;flex:0 0 auto;';
    const title = doc.createElement('div');
    title.className = 'ma-stl-design-ai__title';
    title.textContent = T.title;
    title.style.cssText = 'flex:1;font-size:14px;font-weight:600;color:#ffffff;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;';
    const closeBtn = doc.createElement('button');
    closeBtn.type = 'button';
    closeBtn.className = 'ma-stl-design-ai__close';
    closeBtn.title = T.close;
    closeBtn.setAttribute('aria-label', T.close);
    closeBtn.innerHTML = CLOSE_SVG;
    closeBtn.style.cssText = 'display:inline-flex;align-items:center;justify-content:center;width:30px;height:30px;padding:0;'
        + 'border:none;border-radius:6px;background:transparent;color:#ffffff;cursor:pointer;';
    /* Entorno de conversación activo (catálogo de la BD); oculto hasta cargarlo. */
    const envBadge = doc.createElement('span');
    envBadge.className = 'ma-stl-design-ai__env';
    envBadge.setAttribute('data-ma-stl-ai-env', '');
    envBadge.style.cssText = 'display:none;flex:0 0 auto;max-width:150px;padding:2px 8px;border:1px solid rgba(255,255,255,0.35);border-radius:999px;'
        + 'font-size:11px;font-weight:600;line-height:16px;color:#e2e8f0;background:rgba(255,255,255,0.08);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;';
    head.appendChild(title);
    head.appendChild(envBadge);
    head.appendChild(closeBtn);
    root.appendChild(head);

    const body = doc.createElement('div');
    body.className = 'ma-stl-design-ai__body';
    body.style.cssText = 'flex:1 1 auto;min-height:0;display:flex;flex-direction:column;padding:10px;box-sizing:border-box;';
    root.appendChild(body);
    (doc.body || doc.documentElement).appendChild(root);

    const chat = mountWallAiChat(body, {
        owner: 'design',
        texts: { placeholder: T.placeholder },
        handler: function (_ctx, text, history) { return maStlDesignAiHandleMessage(session, text, history); },
    });
    body.appendChild(chat.root);
    chat.root.style.flex = '1 1 auto';
    chat.root.style.minHeight = '0';

    function layout() {
        const r = maStlWallAiCompactRect(maStlWallAiCompactArea(doc, doc.defaultView || window));
        root.style.left = r.left + 'px';
        root.style.top = r.top + 'px';
        root.style.width = r.width + 'px';
        root.style.height = r.height + 'px';
        /* Conversación = alto del panel menos cabecera, entrada y márgenes. */
        const headH = head.offsetHeight || 46;
        const inputH = chat.input && chat.input.parentNode ? (chat.input.parentNode.offsetHeight || 60) : 60;
        chat.log.style.height = Math.max(120, r.height - headH - inputH - 40) + 'px';
    }

    const btn = opts.button || null;
    function paintBtn(on) {
        if (!btn) return;
        btn.setAttribute('aria-pressed', on ? 'true' : 'false');
        btn.classList.toggle('active', !!on);
        btn.classList.toggle('ma-stl-design-ai-on', !!on);
    }

    function isOpen() { return root.style.display !== 'none'; }

    function paintEnv() {
        const e = maStlAiCatalogEnv();
        if (!e || !e.code) { envBadge.style.display = 'none'; envBadge.textContent = ''; return; }
        const name = maStlAiCatalogEnvLabel(e.code, lang);
        envBadge.textContent = name;
        envBadge.setAttribute('data-ma-stl-ai-env', e.code);
        envBadge.title = fmt(texts(lang).envBadge, name);
        envBadge.style.display = 'inline-block';
    }
    maStlAiCatalogOnChange(function () { paintEnv(); });

    function open() {
        if (isOpen()) { chat.focus(); return api; }
        try { if (bridge && bridge.closeWallAi) bridge.closeWallAi(); } catch (_e) { /* sin asistente del muro */ }
        root.style.display = 'flex';
        layout();
        paintBtn(true);
        chat.focus();
        /* Vocabulario y entorno del usuario (BD); si falla, sigue el vocabulario JS. */
        maStlAiCatalogLoad('design', lang, { doc: doc }).then(function () { paintEnv(); layout(); });
        return api;
    }

    function close() {
        if (!isOpen()) return api;
        try { chat.stopVoice(false); } catch (_e) { /* sin micro */ }
        try { chat.stopSpeaking(); } catch (_e) { /* sin voz */ }
        root.style.display = 'none';
        paintBtn(false);
        chat.reset();
        session = maStlDesignAiNewSession(bridge);
        try { if (bridge && bridge.clearHighlight) bridge.clearHighlight(); } catch (_e) { /* visor */ }
        return api;
    }

    function toggle() { return isOpen() ? close() : open(); }

    closeBtn.addEventListener('click', function () { close(); });
    root.addEventListener('keydown', function (ev) {
        if (ev.key === 'Escape') { ev.stopPropagation(); close(); }
    });
    /* El asistente del muro se ha mostrado: este se cierra (un solo asistente abierto). */
    doc.addEventListener('ma-stl-ai-chat-shown', function (ev) {
        const owner = ev && ev.detail ? ev.detail.owner : null;
        if (owner && owner !== 'design' && isOpen()) close();
    });
    const win = doc.defaultView || window;
    win.addEventListener('resize', function () { if (isOpen()) layout(); });

    if (btn) {
        btn.setAttribute('data-ma-design-ai', '1');
        btn.setAttribute('title', T.title);
        btn.setAttribute('aria-label', T.title);
        btn.setAttribute('aria-pressed', 'false');
        btn.addEventListener('click', function (ev) {
            ev.preventDefault();
            toggle();
        });
    }

    const api = {
        open: open,
        close: close,
        toggle: toggle,
        isOpen: isOpen,
        chat: chat,
        root: root,
        session: function () { return session; },
        /** Envía un mensaje como si se escribiera (pruebas y otras partes del visor). */
        say: function (text) {
            if (!isOpen()) open();
            chat.input.value = String(text == null ? '' : text);
            chat.send();
            return api;
        },
        commands: MA_STL_DESIGN_AI_COMMANDS,
        register: registerDesignAiCommand,
    };
    if (typeof window !== 'undefined') window.maStlDesignAi = api;
    return api;
}
