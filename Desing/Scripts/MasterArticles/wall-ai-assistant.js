/**
 * Asistente IA de la edición de muro (Desing_2).
 * Fase 1: interfaz (pestaña + chat). Fase 2: intérprete local (palabras clave, sin
 * servicios externos) con la primera orden: «dame el material de encofrado de este muro».
 * Pestaña de chat compartida por «Modificar muro» (wall-edit-form.js) y el
 * formulario del muro curvo (#ma-stl-curved-wall-modal).
 *
 * Gancho: `maStlWallAiHandleMessage(wallCtx, text)` → `{ reply, actions: [], table?, buttons? }`
 * (o una Promise con ese objeto). Se puede sustituir desde fuera asignando
 * `window.maStlWallAiHandleMessage` antes de enviar.
 *   table   → { columns:[{key,label,align,format}], rows:[], totals:{} } (se pinta con botón «Copiar»)
 *   buttons → [{ label, primary, pendingText, run(): respuesta | Promise<respuesta> }]
 * Próximas órdenes: «no tengo paneles de 0,90, sustitúyelos» · «solo tengo 3 paneles de 0,90 del 19».
 * Voz (14/11/2026): micrófono (dictado) y altavoz (respuestas habladas con speechSynthesis; `speech` en la
 * respuesta = texto a leer en vez de `reply`).
 */

/** Textos por defecto (es). El curvo puede sobrescribir tab/placeholder con data-msg-ai-* en su modal. */
export const MA_STL_WALL_AI_TEXT = {
    lang: 'es',
    tab: 'Asistente IA',
    /* Mensaje inicial: vacío → saludo según la hora local (maStlWallAiGreeting). */
    intro: '',
    placeholder: 'Ej.: dame el material de encofrado de este muro',
    send: 'Enviar',
    hint: 'Enter envía · Mayús+Enter salto de línea',
    you: 'Tú',
    assistant: 'Asistente',
    error: 'No he podido procesar el mensaje: {0}',
    help: 'No he entendido «{0}». De momento sé hacer esto:\n• Lista de materiales del encofrado de este muro: «dame el material de encofrado de este muro», «lista de materiales», «despiece», «¿qué necesito para encofrar este muro?».\n• Paneles que no tienes: «no tengo paneles de 0,90, sustitúyelos» o «solo tengo 3 paneles de 0,90». Te enseño el antes y el después y lo aplico solo si me dices que sí.\n• «ayuda»: muestra esta lista.',
    helpShort: 'Puedo darte la lista de materiales del encofrado de este muro («dame el material de encofrado de este muro», «despiece») y cambiar paneles que no tengas («no tengo paneles de 0,90, sustitúyelos», «solo tengo 3 paneles de 0,90»).',
    replaceSoon: 'Dime qué paneles no tienes, por ejemplo «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».',
    thanks: '¡De nada! Si necesitas el material de otro muro, ábrelo y pídemelo.',
    matTitle: 'Material de encofrado del {0}',
    matSummary: '{0} referencias · {1} piezas{2}. Según el último encofrado calculado.',
    matWeight: ' · {0} kg',
    matCornerNote: 'Las piezas de esquina compartidas cuentan en el muro al que las asignó el cálculo.',
    matTempShown: 'He encofrado en el visor solo este muro ({0} piezas, las de la lista). Es provisional: no se guarda y se quita al pedir otro recuento, cambiar de muro, cerrar el formulario o pulsar «Encofrar».',
    matTempHidden: 'He preparado el encofrado de solo este muro ({0} piezas); se ve en la vista 3D de muros. Es provisional: no se guarda.',
    matTempFail: 'No he podido dibujar el encofrado de este muro en el visor ({0}).',
    matEdited: 'Ojo: hay piezas de este muro movidas o borradas a mano; la lista refleja el cálculo, no esas ediciones.',
    matNoCatalog: 'No he podido leer el catálogo ({0}); muestro solo códigos y cantidades.',
    matNone: 'Este diseño todavía no tiene el encofrado calculado, así que no puedo darte su material.',
    matNoneForWall: 'El último encofrado calculado no tiene piezas para este muro (quizá se calculó antes de crearlo o de modificarlo).',
    matNew: 'Este muro curvo aún no existe en el diseño. Créalo con «Aceptar» y calcula el encofrado; después te doy su material.',
    matNoAccess: 'No tengo acceso a los datos de encofrado desde esta ventana.',
    matRun: 'Calcular encofrado',
    matRerun: 'Recalcular encofrado',
    matRunHint: '«Calcular encofrado» calcula y pinta el encofrado de todo el diseño (botón «Encofrar»). Cuando termine te doy la lista.',
    matSummaryPreview: '{0} referencias · {1} piezas{2}. Calculado solo para este muro (vista previa: no cambia ni guarda el encofrado del diseño).',
    matPreviewContext: 'Para las esquinas se tienen en cuenta los muros vecinos ({0} en total), pero solo se cuentan las piezas de este.',
    matPreviewFail: 'No he podido calcular el encofrado de este muro ({0}).',
    matPreviewOld: 'Ojo: el servidor publicado aún no tiene el cálculo por muro de esta versión (hay que recompilar y publicar); la lista puede no coincidir con «Encofrar».',
    colBefore: 'Antes',
    colAfter: 'Después',
    colDiff: 'Dif.',
    stockNoWidth: 'sin paneles de {0}',
    stockMaxWidth: 'como mucho {0} paneles de {1}',
    stockAskWidth: '¿De qué ancho son los paneles que no tienes? (0,30 · 0,45 · 0,60 · 0,75 · 0,90). Por ejemplo: «no tengo paneles de 0,90» o «solo tengo 3 paneles de 0,90».',
    stockAskRef: 'Entiendo: {0}. Pero no sé qué quiere decir {1} (¿altura?, ¿una referencia?, ¿un almacén?), así que no lo tengo en cuenta. ¿Me lo aclaras o sigo sin eso?',
    stockIgnoreRef: 'Seguir sin «{0}»',
    stockNotStraight: 'En un muro recto no hay paneles de {0} (anchos: 0,30 · 0,45 · 0,60 · 0,75 · 0,90).',
    stockTitle: 'Cambio de paneles en el {0}: {1}',
    stockPanels: 'Paneles por ancho (antes → después):',
    stockSummary: 'Piezas: {0} → {1}{2}. En la tabla solo salen las referencias que cambian.',
    stockWeight: ' · {0} kg → {1} kg',
    stockBeforePreview: '«Antes» calculado ahora solo para este muro (no había encofrado pintado de él).',
    stockNoChange: 'Este muro no usa más paneles de {0} de los que dices: no hay nada que cambiar.',
    stockNothingPossible: 'No puedo quitar los paneles de {0} de este muro: no cambio nada.',
    stockImpossible: 'En un tramo de {0} no hay combinación con los paneles que quedan: ahí dejo los paneles de antes. Sin ellos quedaría un hueco de {1} mm; según el manual (NEVI 3.2) se resolvería con {2}.',
    stockLeft: 'Quedan {0} paneles de {1} que no se pueden cambiar (tapes de extremo, que exigen ese ancho, o tramos sin otra combinación).',
    stockMaxOver: 'Ojo: aún se usan {0} paneles de {1} y tienes {2} (el resto está en tapes o tramos sin otra combinación).',
    stockMaxUsed: 'Paneles de {1}: se usan {0} de los {2} que tienes (las dos caras cuentan).',
    stockConfirm: '¿Aplico el cambio? Solo se vuelve a encofrar este muro; el resto del encofrado no se toca.',
    stockApply: 'Sí, aplicar',
    stockApplying: 'Aplicando…',
    stockApplied: 'Hecho: he vuelto a encofrar solo este muro ({0} piezas). La regla queda en el muro mientras trabajas en este diseño (todavía no se guarda en la base de datos).',
    stockUndo: 'Deshacer',
    stockUndone: 'Deshecho: el muro vuelve a como estaba.',
    stockCancelled: 'Vale, no cambio nada.',
    stockAlready: 'Ese cambio ya está aplicado; si quieres volver atrás, pulsa «Deshacer».',
    stockCancelledAlready: 'Ese cambio lo cancelaste; pídemelo otra vez si lo quieres.',
    stockUndoneAlready: 'Ya está deshecho.',
    stockServerOld: 'El servidor publicado todavía no sabe cambiar paneles (hay que recompilar y publicar Desing). No he cambiado nada.',
    /* Cambio guiado de paneles (14/11/2026): «de 13 paneles de 2,70×0,90 solo tengo 10». */
    swapTitle: 'Cambiar paneles de {0}×{1} · {2}',
    swapCount: 'En este muro hay {0} paneles de {1}×{2}: {3} parejas enfrentadas (una en cada cara){4}.',
    swapSingles: ' y {0} suelto en una esquina o remate',
    swapSinglesN: ' y {0} sueltos en esquinas o remates',
    swapPair1: 'pareja',
    swapPairN: 'parejas',
    swapSaidTotal: 'Dices {0}, pero en este muro cuento {1}.',
    swapNeedHave: 'Tienes {0}, así que te faltan {1}.',
    swapNeedMissing: 'Te faltan {0}.',
    swapNeedNone: 'No tienes ninguno: hay que cambiar los {0}.',
    swapEnough: 'Con {0} te llega: en este muro hay {1}. No hace falta cambiar nada.',
    swapPairs: 'Cada panel va enfrentado al de la otra cara (el tirante ata los dos), así que se cambian por parejas: cambio {0} = {1} paneles y quedan {2} de {3}{4}.',
    swapUses: ' (usas {0} de tus {1})',
    swapAskMode: '¿Los cambio todos de una vez con la misma opción o uno a uno?',
    swapModeAll: 'Todos de una vez',
    swapModeOne: 'Uno a uno',
    swapCancel: 'Cancelar',
    swapCancelled: 'Vale, no cambio ningún panel.',
    swapAskHave: 'En este muro hay {0} paneles de {1}×{2}. ¿Cuántos tienes (o cuántos te faltan)?',
    swapAskHeight: 'En este muro hay paneles de {0} de varias alturas ({1}). ¿De qué altura son los que te faltan?',
    swapNoPanels: 'En este muro no hay paneles de {0}. Paneles del muro: {1}.',
    swapNoPanelsAtAll: 'En este muro no hay paneles.',
    swapNoOption: 'Un panel de {0} no se puede partir en otros más estrechos del catálogo (anchos de 0,30 a 0,90 de 0,15 en 0,15: el más estrecho es 0,30), así que no hay opción panel a panel.',
    swapAuto: 'Recolocar el módulo entero (automático)',
    swapNotReplaceable: 'Esos paneles no se pueden cambiar uno a uno: están en esquinas, remates o tapes (ahí manda el cálculo de la esquina).',
    swapFewReplaceable: 'Solo puedo cambiar {0} panel a panel; el resto está en esquinas o remates.',
    swapServerOld: 'El servidor publicado todavía no sabe cambiar paneles sueltos (hay que recompilar y publicar Desing). No he cambiado nada.',
    swapOptions: 'Opciones para cada panel de {0} (mismo alto, las dos caras):',
    swapOptionTotal: '{0}) {1}  → en total {2}',
    swapOptionShort: '{0}) {1}',
    swapAskOption: '¿Qué opción? Pulsa un botón o dímelo («la 2», «dos de 0,45»).',
    swapChosenAll: 'Cambio {0} (resaltados en naranja en el visor):',
    swapPanelLine: '· nº {0} desde el inicio del muro, de {1} a {2} m{3}, en las dos caras',
    swapBand: ', banda a {0} m de altura',
    swapTumbado: ', tumbado a {0} m de altura',
    swapStep: 'Panel {0} de {1}: el nº {2} desde el inicio del muro (de {3} a {4} m{5}), en las dos caras. Lo tienes resaltado en naranja en el visor.',
    swapAskOptionOne: '¿Con qué lo cambio? Pulsa un botón o dímelo («la 2», «dos de 0,45», «otro panel»).',
    swapOtherPanel: 'Otro panel',
    swapPickPanel: '¿Cuál cambio? Numerados desde el inicio del muro (también vale «el 3»):',
    swapPanelBtn: 'nº {0} ({1}–{2} m)',
    swapNotUnderstood: 'No te he entendido. Elige con los botones o dime, por ejemplo, «la 1» o «dos de 0,45».',
    swapStockLimited: 'Quito las opciones que gastan más paneles de los que tienes ({0}).',
    swapNoValidOption: 'Con los paneles que tienes no hay ninguna opción válida para un panel de {0}.',
    swapReview: 'Resumen del cambio:',
    swapReviewLine: '· nº {0} ({1}–{2} m): {3} → {4}, las dos caras',
    swapReviewCount: 'Paneles de {0}: {1} → {2}{3}.',
    swapReviewPartial: 'Ojo: {0} cambio(s) no los ha podido hacer el servidor y esos paneles se quedan como están.',
    swapConfirm: '¿Lo hago? Quito esos paneles con sus grapas, tirantes y demás accesorios y pongo los nuevos con sus piezas. Solo en esta sesión (no se guarda en BD ni marca el muro como especial).',
    swapApplied: 'Hecho: {0} paneles cambiados en el visor con grapas, tirantes, rigidizadores y puntales recalculados ({1} piezas en el muro). Solo en esta sesión.',
    swapNothingChanged: 'El servidor ha devuelto el muro igual: no hay nada que cambiar.',
    comp0_5: 'taco de madera en obra y grapa regulable (0–5 cm)',
    comp5_18: 'compensador 5–18 cm con rigidizadores y grapas fijas',
    comp8_30: 'compensador 8–30 cm (solapan los dos paneles) con rigidizadores 0,75',
    compNone: 'nada del manual (más de 30 cm): hace falta otro panel',
    curvedStockNone: 'Este muro curvo tiene modulación {0} y no usa los paneles de la orden: no le afecta.',
    curvedStockEnough: 'Necesita {0} y tienes {1}.',
    curvedStockPropose: 'Este muro curvo se encofra con un único ancho de panel ({0}), así que hay que cambiar la modulación.',
    curvedMaxNeed: 'Con {0} paneles de {1} no llega: necesita {2} (las dos caras).',
    curvedNow: 'Ahora:',
    curvedProposal: 'Propuesta:',
    curvedOther: 'Otra opción:',
    curvedLine: '{0} → {1} paneles por cara, giro {2}° por junta, remate exterior {3} mm e interior {4} mm',
    curvedLineNone: '{0} → no válida para este muro',
    curvedWarnOuter: 'remate exterior > 0,15: el tablón se queda en 0,15',
    curvedWarnInner: 'remate interior < 0,05',
    curvedNoOption: 'No hay ninguna modulación válida sin esos paneles para este muro curvo.',
    curvedConfirm: '¿Cambio la modulación a {0}?',
    curvedApplied: 'Hecho: modulación {0} en el formulario{1}. Pulsa «Aceptar» para guardarla en el muro.',
    curvedRepainted: ' y encofrado de este muro actualizado',
    curvedFormMissing: 'No he podido cambiar el desplegable de modulación; cámbialo a mano.',
    matRunning: 'Calculando el encofrado…',
    matRunFail: 'El cálculo del encofrado no se ha completado{0}.',
    matRunUnavailable: 'No encuentro el botón «Encofrar» en la barra; cierra esta ventana y pulsa «Encofrar».',
    colCode: 'Código',
    colName: 'Descripción',
    colQty: 'Cant.',
    colWeight: 'Peso (kg)',
    total: 'Total',
    copy: 'Copiar',
    copied: 'Copiado',
    copyFail: 'No se pudo copiar',
    confirmMaterials: '¿Querías decir: «dame el material de encofrado de este muro»?',
    yes: 'Sí',
    no: 'No',
    wall: 'muro',
    curvedWall: 'muro curvo',
    newCurvedWall: 'muro curvo nuevo',
    /* Voz (14/11/2026): dictado con el micrófono (Web Speech API) y respuestas habladas (speechSynthesis). */
    mic: 'Hablar (modo continuo) · Alt',
    micStop: 'Escuchando (modo continuo)… pulsa aquí o Alt para apagar',
    micOff: 'Micrófono apagado.',
    micUnsupported: 'Este navegador no permite dictar por voz (usa Chrome o Edge)',
    micInsecure: 'El dictado por voz necesita HTTPS o localhost',
    micErrNotAllowed: 'No tengo permiso para usar el micrófono. Actívalo en el candado de la barra de direcciones.',
    micErrNoSpeech: 'No he oído nada. Pulsa el micrófono y vuelve a intentarlo.',
    micErrNetwork: 'El reconocimiento de voz no responde (sin conexión con el servicio de voz del navegador).',
    micErrAudio: 'No encuentro ningún micrófono.',
    micErrOther: 'No he podido usar el micrófono ({0}).',
    micListening: 'Escuchando… habla ahora',
    micPaused: 'Te estoy contestando… (micrófono en pausa)',
    spk: 'Respuestas habladas: activadas (pulsa para silenciar)',
    spkOff: 'Respuestas habladas: desactivadas (pulsa para que te conteste en voz alta)',
    spkUnsupported: 'Este navegador no puede leer en voz alta (usa Chrome o Edge)',
    spkNoVoice: 'No hay ninguna voz de {0} instalada en este equipo (Windows: Configuración › Hora e idioma › Voz)',
    speakOption: 'Opción {0}: {1}',
    speakScreen: 'Te dejo el detalle en pantalla.',
    matSpeech: 'Este muro lleva {0} piezas de {1} referencias; te dejo la lista en pantalla.',
    swapSpeakConfirm: 'Te dejo el resumen y la tabla en pantalla. ¿Lo hago?',
    morning: 'Buenos días',
    afternoon: 'Buenas tardes',
    night: 'Buenas noches',
};

/** Textos en inglés (mismas claves). */
export const MA_STL_WALL_AI_TEXT_EN = {
    lang: 'en',
    tab: 'AI Assistant',
    intro: '',
    placeholder: 'E.g.: give me the formwork material for this wall',
    send: 'Send',
    hint: 'Enter sends · Shift+Enter new line',
    you: 'You',
    assistant: 'Assistant',
    error: 'I could not process the message: {0}',
    help: 'I did not understand “{0}”. For now I can do this:\n• Formwork bill of materials for this wall: “give me the formwork material for this wall”, “materials list”, “BOM”, “what do I need to form this wall?”.\n• Panels you don\'t have: “I don\'t have 0.90 panels, replace them” or “I only have 3 panels of 0.90”. I show you the before and after and apply it only if you say yes.\n• “help”: shows this list.',
    helpShort: 'I can give you the formwork bill of materials for this wall (“give me the formwork material for this wall”, “BOM”) and change panels you don\'t have (“I don\'t have 0.90 panels, replace them”, “I only have 3 panels of 0.90”).',
    replaceSoon: 'Tell me which panels you don\'t have, e.g. “I don\'t have 0.90 panels” or “I only have 3 panels of 0.90”.',
    thanks: 'You\'re welcome! If you need the material for another wall, open it and ask me.',
    matTitle: 'Formwork material for {0}',
    matSummary: '{0} items · {1} pieces{2}. Based on the last formwork calculation.',
    matWeight: ' · {0} kg',
    matCornerNote: 'Shared corner pieces are counted on the wall the calculation assigned them to.',
    matTempShown: 'I have drawn the formwork of this wall only in the viewer ({0} pieces, the ones in the list). It is temporary: it is not saved and it is removed when you ask for another count, change wall, close the form or press “Formwork”.',
    matTempHidden: 'I have prepared the formwork of this wall only ({0} pieces); it shows in the 3D wall view. It is temporary: it is not saved.',
    matTempFail: 'I could not draw the formwork of this wall in the viewer ({0}).',
    matEdited: 'Note: some pieces of this wall were moved or deleted by hand; the list reflects the calculation, not those edits.',
    matNoCatalog: 'I could not read the catalogue ({0}); showing codes and quantities only.',
    matNone: 'This design has no formwork calculated yet, so I can\'t give you its material.',
    matNoneForWall: 'The last formwork calculation has no pieces for this wall (it may have been calculated before the wall was created or changed).',
    matNew: 'This curved wall doesn\'t exist in the design yet. Create it with “Accept” and calculate the formwork; then I\'ll give you its material.',
    matNoAccess: 'I don\'t have access to the formwork data from this window.',
    matRun: 'Calculate formwork',
    matRerun: 'Recalculate formwork',
    matRunHint: '“Calculate formwork” calculates and paints the formwork of the whole design (“Formwork” button). When it finishes I\'ll give you the list.',
    matSummaryPreview: '{0} items · {1} pieces{2}. Calculated for this wall only (preview: the design\'s formwork is not changed or saved).',
    matPreviewContext: 'Neighbouring walls are taken into account for the corners ({0} in total), but only this wall\'s pieces are counted.',
    matPreviewFail: 'I could not calculate the formwork of this wall ({0}).',
    matPreviewOld: 'Note: the published server does not have this version\'s per-wall calculation yet (it needs to be rebuilt and published); the list may differ from “Formwork”.',
    colBefore: 'Before',
    colAfter: 'After',
    colDiff: 'Diff.',
    stockNoWidth: 'no {0} panels',
    stockMaxWidth: 'at most {0} panels of {1}',
    stockAskWidth: 'Which panel width don\'t you have? (0.30 · 0.45 · 0.60 · 0.75 · 0.90). For example: “I don\'t have 0.90 panels” or “I only have 3 panels of 0.90”.',
    stockAskRef: 'I understand: {0}. But I don\'t know what {1} means (height? a reference? a warehouse?), so I\'m leaving it out. Can you clarify, or shall I go on without it?',
    stockIgnoreRef: 'Go on without “{0}”',
    stockNotStraight: 'A straight wall has no {0} panels (widths: 0.30 · 0.45 · 0.60 · 0.75 · 0.90).',
    stockTitle: 'Panel change on {0}: {1}',
    stockPanels: 'Panels by width (before → after):',
    stockSummary: 'Pieces: {0} → {1}{2}. The table only shows the items that change.',
    stockWeight: ' · {0} kg → {1} kg',
    stockBeforePreview: '“Before” was calculated now for this wall only (it had no painted formwork).',
    stockNoChange: 'This wall doesn\'t use more {0} panels than you have: nothing to change.',
    stockNothingPossible: 'I can\'t remove the {0} panels from this wall: I\'m not changing anything.',
    stockImpossible: 'A {0} run has no combination with the remaining panels: I keep the previous panels there. Without them there would be a {1} mm gap; per the manual (NEVI 3.2) it would be solved with {2}.',
    stockLeft: '{0} panels of {1} remain that can\'t be changed (end stop-ends, which need that width, or runs with no other combination).',
    stockMaxOver: 'Note: {0} panels of {1} are still used and you have {2} (the rest are in stop-ends or runs with no other combination).',
    stockMaxUsed: '{1} panels: {0} used out of the {2} you have (both faces count).',
    stockConfirm: 'Shall I apply it? Only this wall is formed again; the rest of the formwork is not touched.',
    stockApply: 'Yes, apply',
    stockApplying: 'Applying…',
    stockApplied: 'Done: I formed this wall again ({0} pieces). The rule stays on the wall while you work on this design (it is not saved in the database yet).',
    stockUndo: 'Undo',
    stockUndone: 'Undone: the wall is back as it was.',
    stockCancelled: 'OK, I won\'t change anything.',
    stockAlready: 'That change is already applied; press “Undo” to go back.',
    stockCancelledAlready: 'You cancelled that change; ask me again if you want it.',
    stockUndoneAlready: 'It is already undone.',
    stockServerOld: 'The published server can\'t change panels yet (Desing needs to be rebuilt and published). I didn\'t change anything.',
    /* Guided panel swap (14/11/2026): “of 13 panels of 2.70×0.90 I only have 10”. */
    swapTitle: 'Swap {0}×{1} panels · {2}',
    swapCount: 'This wall has {0} panels of {1}×{2}: {3} facing pairs (one on each face){4}.',
    swapSingles: ' and {0} single one at a corner or end',
    swapSinglesN: ' and {0} single ones at corners or ends',
    swapPair1: 'pair',
    swapPairN: 'pairs',
    swapSaidTotal: 'You say {0}, but I count {1} on this wall.',
    swapNeedHave: 'You have {0}, so you are {1} short.',
    swapNeedMissing: 'You are {0} short.',
    swapNeedNone: 'You have none: all {0} have to be swapped.',
    swapEnough: '{0} is enough: this wall has {1}. Nothing needs to change.',
    swapPairs: 'Each panel faces the one on the other face (the tie holds both), so they are swapped in pairs: I swap {0} = {1} panels and {2} of {3} remain{4}.',
    swapUses: ' (you use {0} of your {1})',
    swapAskMode: 'Shall I swap them all at once with the same option, or one by one?',
    swapModeAll: 'All at once',
    swapModeOne: 'One by one',
    swapCancel: 'Cancel',
    swapCancelled: 'OK, I won\'t swap any panel.',
    swapAskHave: 'This wall has {0} panels of {1}×{2}. How many do you have (or how many are you short)?',
    swapAskHeight: 'This wall has {0} panels of several heights ({1}). Which height are the ones you are missing?',
    swapNoPanels: 'This wall has no {0} panels. Panels on the wall: {1}.',
    swapNoPanelsAtAll: 'This wall has no panels.',
    swapNoOption: 'A {0} panel can\'t be split into narrower catalogue panels (widths 0.30 to 0.90 in 0.15 steps: the narrowest is 0.30), so there is no panel-by-panel option.',
    swapAuto: 'Re-lay the whole module (automatic)',
    swapNotReplaceable: 'Those panels can\'t be swapped one by one: they are at corners, ends or tapes (the corner calculation rules there).',
    swapFewReplaceable: 'I can only swap {0} panel by panel; the rest are at corners or ends.',
    swapServerOld: 'The published server can\'t swap single panels yet (Desing needs to be rebuilt and published). I didn\'t change anything.',
    swapOptions: 'Options for each {0} panel (same height, both faces):',
    swapOptionTotal: '{0}) {1}  → in total {2}',
    swapOptionShort: '{0}) {1}',
    swapAskOption: 'Which option? Press a button or tell me (“number 2”, “two of 0.45”).',
    swapChosenAll: 'I\'ll swap {0} (highlighted in orange in the viewer):',
    swapPanelLine: '· no. {0} from the start of the wall, {1} to {2} m{3}, both faces',
    swapBand: ', band at {0} m high',
    swapTumbado: ', horizontal at {0} m high',
    swapStep: 'Panel {0} of {1}: no. {2} from the start of the wall ({3} to {4} m{5}), both faces. It is highlighted in orange in the viewer.',
    swapAskOptionOne: 'What shall I replace it with? Press a button or tell me (“number 2”, “two of 0.45”, “another panel”).',
    swapOtherPanel: 'Another panel',
    swapPickPanel: 'Which one? Numbered from the start of the wall (you can also say “panel 3”):',
    swapPanelBtn: 'no. {0} ({1}–{2} m)',
    swapNotUnderstood: 'I didn\'t understand. Choose with the buttons or tell me, e.g., “number 1” or “two of 0.45”.',
    swapStockLimited: 'I leave out the options that use more panels than you have ({0}).',
    swapNoValidOption: 'With the panels you have there is no valid option for a {0} panel.',
    swapReview: 'Summary of the change:',
    swapReviewLine: '· no. {0} ({1}–{2} m): {3} → {4}, both faces',
    swapReviewCount: '{0} panels: {1} → {2}{3}.',
    swapReviewPartial: 'Note: the server couldn\'t make {0} change(s); those panels stay as they are.',
    swapConfirm: 'Shall I do it? I remove those panels with their clamps, ties and other accessories and put in the new ones with their pieces. This session only (not saved to the DB, the wall is not marked as special).',
    swapApplied: 'Done: {0} panels swapped in the viewer with clamps, ties, stiffeners and props recalculated ({1} pieces on the wall). This session only.',
    swapNothingChanged: 'The server returned the wall unchanged: there is nothing to swap.',
    comp0_5: 'an on-site timber filler and adjustable clamp (0–5 cm)',
    comp5_18: 'a 5–18 cm compensator with stiffeners and fixed clamps',
    comp8_30: 'an 8–30 cm compensator (both panels overlap) with 0.75 stiffeners',
    compNone: 'nothing in the manual (over 30 cm): another panel is needed',
    curvedStockNone: 'This curved wall has a {0} modulation and does not use the panels in the order: it is not affected.',
    curvedStockEnough: 'It needs {0} and you have {1}.',
    curvedStockPropose: 'This curved wall uses a single panel width ({0}), so the modulation has to change.',
    curvedMaxNeed: '{0} panels of {1} are not enough: it needs {2} (both faces).',
    curvedNow: 'Now:',
    curvedProposal: 'Proposal:',
    curvedOther: 'Other option:',
    curvedLine: '{0} → {1} panels per face, {2}° turn per joint, outer filler {3} mm and inner {4} mm',
    curvedLineNone: '{0} → not valid for this wall',
    curvedWarnOuter: 'outer filler > 0.15: the board stays at 0.15',
    curvedWarnInner: 'inner filler < 0.05',
    curvedNoOption: 'There is no valid modulation without those panels for this curved wall.',
    curvedConfirm: 'Shall I change the modulation to {0}?',
    curvedApplied: 'Done: modulation {0} in the form{1}. Press “Accept” to save it on the wall.',
    curvedRepainted: ' and this wall\'s formwork updated',
    curvedFormMissing: 'I could not change the modulation dropdown; change it by hand.',
    matRunning: 'Calculating formwork…',
    matRunFail: 'The formwork calculation did not complete{0}.',
    matRunUnavailable: 'I can\'t find the “Formwork” button in the toolbar; close this window and press it.',
    colCode: 'Code',
    colName: 'Description',
    colQty: 'Qty',
    colWeight: 'Weight (kg)',
    total: 'Total',
    copy: 'Copy',
    copied: 'Copied',
    copyFail: 'Could not copy',
    confirmMaterials: 'Did you mean: “give me the formwork material for this wall”?',
    yes: 'Yes',
    no: 'No',
    wall: 'wall',
    curvedWall: 'curved wall',
    newCurvedWall: 'new curved wall',
    mic: 'Speak (continuous mode) · Alt',
    micStop: 'Listening (continuous mode)… click here or press Alt to turn off',
    micOff: 'Microphone off.',
    micUnsupported: 'This browser does not support voice input (use Chrome or Edge)',
    micInsecure: 'Voice input needs HTTPS or localhost',
    micErrNotAllowed: 'I am not allowed to use the microphone. Enable it from the padlock in the address bar.',
    micErrNoSpeech: 'I did not hear anything. Click the microphone and try again.',
    micErrNetwork: 'Speech recognition is not responding (no connection to the browser speech service).',
    micErrAudio: 'No microphone found.',
    micErrOther: 'I could not use the microphone ({0}).',
    micListening: 'Listening… speak now',
    micPaused: 'Answering… (microphone paused)',
    spk: 'Spoken answers: on (click to mute)',
    spkOff: 'Spoken answers: off (click to hear the answers)',
    spkUnsupported: 'This browser cannot read aloud (use Chrome or Edge)',
    spkNoVoice: 'No {0} voice is installed on this computer (Windows: Settings › Time & language › Speech)',
    speakOption: 'Option {0}: {1}',
    speakScreen: 'The details are on screen.',
    matSpeech: 'This wall takes {0} pieces of {1} items; the list is on screen.',
    swapSpeakConfirm: 'The summary and the table are on screen. Shall I do it?',
    morning: 'Good morning',
    afternoon: 'Good afternoon',
    night: 'Good evening',
};

/** Textos del idioma pedido ('en' → inglés; cualquier otro → español). */
export function maStlWallAiTexts(lang) {
    return lang === 'en' ? MA_STL_WALL_AI_TEXT_EN : MA_STL_WALL_AI_TEXT;
}

function readCookie(name) {
    if (typeof document === 'undefined' || !document.cookie) return '';
    const parts = document.cookie.split(';');
    for (let i = 0; i < parts.length; i++) {
        const kv = parts[i].split('=');
        if (kv[0] && kv[0].trim() === name) {
            try { return decodeURIComponent(kv.slice(1).join('=')).trim(); } catch (_e) { return kv.slice(1).join('=').trim(); }
        }
    }
    return '';
}

/**
 * Idioma de la interfaz de la app ('es' | 'en'). Orden: `<html lang>` (lo pone _LayoutMaterio con la
 * cultura UI resuelta en servidor: cookie `tandem_lang` / `tandem_ui_culture` o idioma de la empresa)
 * → cookies `tandem_lang` / `tandem_ui_culture` si traen un código (no un IdObject) → 'es'.
 */
export function maStlWallAiUiLang() {
    const pick = function (v) {
        const s = String(v || '').trim().toLowerCase();
        if (!/^[a-z]{2}/.test(s)) return null;
        return s.indexOf('en') === 0 ? 'en' : 'es';
    };
    if (typeof window !== 'undefined' && window.maStlWallAiForceUiLang) return pick(window.maStlWallAiForceUiLang) || 'es';
    const htmlLang = typeof document !== 'undefined' && document.documentElement ? document.documentElement.getAttribute('lang') : '';
    return pick(htmlLang) || pick(readCookie('tandem_lang')) || pick(readCookie('tandem_ui_culture')) || 'es';
}

/** Textos de la interfaz (pestaña, placeholder, botones del chat) según el idioma de la app. */
export function maStlWallAiUiTexts() {
    return maStlWallAiTexts(maStlWallAiUiLang());
}

function fmtDec(v, lang) {
    const s = v.toFixed(2);
    return lang === 'en' ? s : s.replace('.', ',');
}

function fmtWallLabel(ctx, T) {
    T = T || MA_STL_WALL_AI_TEXT;
    if (!ctx) return '';
    const parts = [];
    if (ctx.kind === 'curved') parts.push(T.curvedWall);
    else if (ctx.kind === 'curved-new') parts.push(T.newCurvedWall);
    else parts.push(T.wall);
    if (ctx.wallKey != null && ctx.wallKey !== '') parts.push(String(ctx.wallKey));
    const dims = [];
    if (Number.isFinite(ctx.lengthM) && ctx.lengthM > 0) dims.push('L ' + fmtDec(ctx.lengthM, T.lang));
    if (Number.isFinite(ctx.outerDiameterM) && ctx.outerDiameterM > 0) dims.push('Ø ' + fmtDec(ctx.outerDiameterM, T.lang));
    if (Number.isFinite(ctx.thicknessM) && ctx.thicknessM > 0) dims.push((T.lang === 'en' ? 't ' : 'e ') + fmtDec(ctx.thicknessM, T.lang));
    if (Number.isFinite(ctx.heightM) && ctx.heightM > 0) dims.push('h ' + fmtDec(ctx.heightM, T.lang));
    return parts.join(' ') + (dims.length ? ' (' + dims.join(' · ') + ')' : '');
}

/**
 * Saludo según la hora local del navegador (mismos tramos en los dos idiomas):
 * 06:00–13:59 «Buenos días» / Good morning · 14:00–20:59 «Buenas tardes» / Good afternoon ·
 * 21:00–05:59 «Buenas noches» / Good evening.
 * @param {Date} [date]
 * @param {'es'|'en'} [lang] por defecto español
 */
export function maStlWallAiGreeting(date, lang) {
    const T = maStlWallAiTexts(lang);
    const d = date instanceof Date && !Number.isNaN(date.getTime()) ? date : new Date();
    const h = d.getHours();
    if (h >= 6 && h < 14) return T.morning;
    if (h >= 14 && h < 21) return T.afternoon;
    return T.night;
}

function fmt(tpl) {
    let out = String(tpl || '');
    for (let i = 1; i < arguments.length; i++) out = out.split('{' + (i - 1) + '}').join(String(arguments[i]));
    return out;
}

/* ===================== Intérprete local tolerante a faltas · español e inglés =====================
 * idioma del mensaje → normalizar → abreviaturas del idioma → coincidencia por palabra (exacta,
 * fonética, Damerau-Levenshtein con umbral por longitud) → puntuación por intención (suma de pesos). */

/**
 * Texto en minúsculas, sin tildes/diéresis (ñ→n salvo opts.keepEnye), sin signos, espacios simples.
 * Con opts.collapse colapsa letras repetidas («materialesss» → «materiales», «llevaa» → «leva»).
 */
export function maStlWallAiNormalize(text, opts) {
    opts = opts || {};
    let s = String(text || '').toLowerCase();
    if (opts.keepEnye) s = s.replace(/ñ/g, '\u0001');
    try { s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (_e) { /* ignore */ }
    if (opts.keepEnye) s = s.replace(/\u0001/g, 'ñ');
    s = s.replace(/[^a-z0-9ñ\s]/g, ' ').replace(/\s+/g, ' ').trim();
    if (opts.collapse) s = s.replace(/([a-zñ])\1+/g, '$1');
    return s;
}

/** Abreviaturas y formas de chat habituales (español) → palabra completa (puede devolver varias). */
const MA_STL_WALL_AI_ABBR = {
    q: 'que', k: 'que', ke: 'que', qe: 'que', qu: 'que',
    pa: 'para', pra: 'para', xa: 'para', p: 'para',
    xq: 'porque', pq: 'porque', porq: 'porque', xk: 'porque', pk: 'porque',
    d: 'de', dl: 'del', x: 'por', tb: 'tambien', tmb: 'tambien', tbn: 'tambien', tambn: 'tambien',
    mat: 'material', mats: 'materiales', matl: 'material', mater: 'material', materia: 'material',
    tng: 'tengo', tngo: 'tengo', nd: 'nada', cn: 'con', sta: 'esta', ste: 'este', est: 'este',
    deste: 'de este', dste: 'de este', deeste: 'de este', dest: 'de este',
    kg: 'kilos', kgs: 'kilos', kilo: 'kilos', kl: 'kilos',
    nec: 'necesito', ncsito: 'necesito', nscto: 'necesito', necs: 'necesito',
    lst: 'lista', info: 'informacion', porfa: 'por favor', xfa: 'por favor', pls: 'por favor', plis: 'por favor',
    cuant: 'cuanto', cnt: 'cuanto',
    dm: 'dame', dme: 'dame', dam: 'dame', dmelo: 'damelo',
    ok: 'vale', okey: 'vale', oki: 'vale', sip: 'si', sii: 'si', yes: 'si', nop: 'no', nope: 'no',
};

/** Abreviaturas de chat en inglés. */
const MA_STL_WALL_AI_ABBR_EN = {
    u: 'you', ur: 'your', r: 'are', ya: 'you', pls: 'please', plz: 'please', plez: 'please', plss: 'please',
    thx: 'thanks', thnx: 'thanks', thanx: 'thanks', tnx: 'thanks', ty: 'thanks', tks: 'thanks',
    wat: 'what', wht: 'what', wut: 'what', whats: 'what is', wats: 'what is', hw: 'how', mny: 'many', mch: 'much',
    n: 'and', w: 'with', b4: 'before', im: 'i am', dont: 'do not', dnt: 'do not', cant: 'can not', doesnt: 'does not',
    gimme: 'give me', wanna: 'want to', gonna: 'going to', idk: 'i do not know',
    mat: 'material', mats: 'materials', matl: 'material', frmwk: 'formwork', fw: 'formwork',
    qty: 'quantity', pcs: 'pieces', pc: 'piece', kg: 'kilos', kgs: 'kilos', lbs: 'pounds', lb: 'pounds',
    k: 'ok', okay: 'ok', okey: 'ok', kk: 'ok', yeah: 'yes', yep: 'yes', yup: 'yes', ye: 'yes', nope: 'no', nah: 'no',
};

/* Palabras propias de cada idioma (las compartidas —material, panel, no, ok…— no puntúan). */
const MA_STL_WALL_AI_LANG_WORDS = {
    es: ('de del el la los las lo que para por este esta estos esto ese esa con un una unos unas es son hay y o pero como '
        + 'cuanto cuantos cuanta cuantas donde cual necesito necesita necesitamos tengo tenemos dame dime damelo quiero muestrame '
        + 'ensename saca pasame muro muros pared piezas pieza paneles lista listado despiece encofrado encofrar encofrarlo pesa peso '
        + 'pesan lleva llevan hace falta hola buenas buenos dias tardes noches ayuda gracias si solo tambien sustituye sustituyelos '
        + 'sustituir cambia cambialos quita borra mueve gira copia crea deshaz materiales grapas barras tuercas mensulas puntales '
        + 'q pa xq pq tb tmb deste porfa xfa vale claro sabes puedes hacer favor mi mis tu').split(' '),
    en: ('the of this that these those what which how many much do does did i you your is are it its give show get tell need '
        + 'needs needed have has got not only just please wall walls parts part pieces piece list weigh weighs weight heavy formwork '
        + 'materials bill breakdown help hello hi hey thanks thank replace substitute swap change remove want can could would for '
        + 'to my and with panels clamps bars nuts brackets props ties delete erase move rotate copy undo draw create good morning '
        + 'afternoon evening yes yeah sure take takes use uses u ur r pls plz thx wat wht gimme idk dont cant takeoff make are am '
        + 'whats there here be been was were will an').split(' '),
};
const MA_STL_WALL_AI_LANG_SETS = {
    es: new Set(MA_STL_WALL_AI_LANG_WORDS.es),
    en: new Set(MA_STL_WALL_AI_LANG_WORDS.en),
};
/* Palabras iguales en los dos idiomas: no deciden el idioma. */
const MA_STL_WALL_AI_LANG_SHARED = new Set(['material', 'panel', 'ok', 'no', 'bom', 'kilos', 'total', 'normal', 'metal', 'error', 'plan', 'radio', 'final', 'info', 'mm', 'cm', 'kg']);
/* Palabras de contenido (largas) con las que se prueba la coincidencia difusa para el idioma. */
const MA_STL_WALL_AI_LANG_FUZZY = {
    es: ['materiales', 'despiece', 'encofrado', 'encofrar', 'necesito', 'piezas', 'paneles', 'cuantos', 'lleva', 'muestrame', 'sustituye', 'listado'],
    en: ['materials', 'formwork', 'breakdown', 'pieces', 'panels', 'weight', 'weigh', 'need', 'replace', 'substitute', 'please', 'thanks', 'what', 'help'],
};

/**
 * Idioma de un mensaje ('es' | 'en') por palabras funcionales y vocabulario de cada idioma
 * (exactas 1 punto, con faltas 0,6; tildes/ñ/¿¡ suman al español). Empate → `fallback`
 * (idioma de la conversación o de la interfaz).
 * @returns {{ lang: 'es'|'en', es: number, en: number, decided: boolean }}
 */
export function maStlWallAiDetectLanguage(text, fallback) {
    const raw = String(text || '');
    const toks = maStlWallAiNormalize(raw).split(' ').filter(Boolean);
    let es = /[ñáéíóú¿¡]/i.test(raw) ? 2 : 0;
    let en = /\b(i'm|don't|can't|doesn't|what's|it's|i've|you're)\b/i.test(raw) ? 2 : 0;
    for (let i = 0; i < toks.length; i++) {
        const t = toks[i];
        const ie = MA_STL_WALL_AI_LANG_SETS.es.has(t);
        const ii = MA_STL_WALL_AI_LANG_SETS.en.has(t);
        if (ie && !ii) { es += 1; continue; }
        if (ii && !ie) { en += 1; continue; }
        if (ie || ii || t.length < 4 || MA_STL_WALL_AI_LANG_SHARED.has(t) || /^\d/.test(t)) continue;
        let fe = 0;
        let fi = 0;
        MA_STL_WALL_AI_LANG_FUZZY.es.forEach(function (w) { fe = Math.max(fe, maStlWallAiWordMatch(t, w)); });
        MA_STL_WALL_AI_LANG_FUZZY.en.forEach(function (w) { fi = Math.max(fi, maStlWallAiWordMatch(t, w)); });
        if (fe >= 0.8 && fe > fi) es += 0.6;
        else if (fi >= 0.8 && fi > fe) en += 0.6;
    }
    const fb = fallback === 'en' ? 'en' : 'es';
    if (es === en) return { lang: fb, es: es, en: en, decided: false };
    return { lang: es > en ? 'es' : 'en', es: es, en: en, decided: true };
}

/** Tokens normalizados con las abreviaturas del idioma expandidas (sin idioma: se detecta, empate → español). */
export function maStlWallAiTokens(text, lang) {
    const base = maStlWallAiNormalize(text);
    if (!base) return [];
    const L = lang === 'en' || lang === 'es' ? lang : maStlWallAiDetectLanguage(text, 'es').lang;
    const map = L === 'en' ? MA_STL_WALL_AI_ABBR_EN : MA_STL_WALL_AI_ABBR;
    const out = [];
    base.split(' ').forEach(function (t) {
        const c = t.replace(/([a-z])\1+/g, '$1');
        const exp = map[t] || map[c];
        if (exp) exp.split(' ').forEach(function (e) { out.push(e); });
        else out.push(t);
    });
    return out;
}

/**
 * Clave fonética (reglas españolas; también absorbe faltas típicas en inglés): c(e,i)/z/s → s ·
 * c(a,o,u)/qu/k → k · b/v/w → b · ll/y → y · h muda · g(e,i)/j → j · gu(e,i) → g · repetidas colapsadas.
 */
export function maStlWallAiPhonetic(word) {
    let s = maStlWallAiNormalize(word).replace(/\s+/g, '');
    s = s.replace(/ch/g, '#');
    s = s.replace(/qu([ei])/g, 'k$1').replace(/q/g, 'k');
    s = s.replace(/c([ei])/g, 's$1').replace(/c/g, 'k').replace(/z/g, 's');
    s = s.replace(/gu([ei])/g, 'g$1').replace(/g([ei])/g, 'j$1');
    s = s.replace(/ll/g, 'y').replace(/[vw]/g, 'b').replace(/h/g, '');
    s = s.replace(/x/g, 'ks').replace(/y$/, 'i');
    s = s.replace(/([a-z#])\1+/g, '$1').replace(/#/g, 'ch');
    return s;
}

/** Distancia Damerau-Levenshtein (variante OSA: transposición de adyacentes = 1). */
export function maStlWallAiDamerau(a, b) {
    a = String(a || '');
    b = String(b || '');
    const n = a.length;
    const m = b.length;
    if (!n) return m;
    if (!m) return n;
    const d = [];
    for (let i = 0; i <= n; i++) { d[i] = [i]; }
    for (let j = 0; j <= m; j++) d[0][j] = j;
    for (let i = 1; i <= n; i++) {
        for (let j = 1; j <= m; j++) {
            const cost = a[i - 1] === b[j - 1] ? 0 : 1;
            let v = Math.min(d[i - 1][j] + 1, d[i][j - 1] + 1, d[i - 1][j - 1] + cost);
            if (i > 1 && j > 1 && a[i - 1] === b[j - 2] && a[i - 2] === b[j - 1]) v = Math.min(v, d[i - 2][j - 2] + 1);
            d[i][j] = v;
        }
    }
    return d[n][m];
}

/** Errores admitidos según la longitud de la palabra del vocabulario: ≤3 → 0 · 4..8 → 1 · >8 → 2. */
export function maStlWallAiFuzzyLimit(len) {
    if (len <= 3) return 0;
    if (len <= 8) return 1;
    return 2;
}

const MA_STL_WALL_AI_ENCLITIC = /(melo|selo|selos|telo|los|las|lo|la|le|les|me)$/;

/**
 * Calidad de coincidencia de una palabra escrita con una del vocabulario:
 * 1 exacta · 0,95 fonética · 0,8 a 1 error · 0,65 a 2 errores · 0 nada.
 */
export function maStlWallAiWordMatch(token, word) {
    const t = String(token || '');
    const w = String(word || '');
    if (!t || !w) return 0;
    const tc = t.replace(/([a-z])\1+/g, '$1');
    const wc = w.replace(/([a-z])\1+/g, '$1');
    if (t === w || tc === wc) return 1;
    const tp = maStlWallAiPhonetic(t);
    const wp = maStlWallAiPhonetic(w);
    if (tp && tp === wp && Math.min(t.length, w.length) >= 3) return 0.95;
    let best = 0;
    /* Con faltas solo palabras de ≥ 4 letras (las cortas —«are», «you», «el»— solo exactas o fonéticas). */
    if (tc.length >= 4) {
        const lim = maStlWallAiFuzzyLimit(wc.length);
        const dist = Math.min(maStlWallAiDamerau(tc, wc), maStlWallAiDamerau(tp, wp));
        if (dist <= lim) best = dist <= 1 ? 0.8 : 0.65;
    }
    /* Pronombres pegados: «sustituyelos», «damelo», «encofrarlo». */
    if (best < 0.8 && t.length > 6 && MA_STL_WALL_AI_ENCLITIC.test(t)) {
        const stem = t.replace(MA_STL_WALL_AI_ENCLITIC, '');
        if (stem.length >= 4 && stem !== t) best = Math.max(best, 0.95 * maStlWallAiWordMatch(stem, w));
    }
    return best;
}

/**
 * Vocabulario por intención (español + inglés en el mismo concepto): { w: peso, words: [...] }.
 * Peso negativo = resta (órdenes que no son «material»: borrar, mover, girar…).
 */
export const MA_STL_WALL_AI_VOCAB = {
    materials: [
        { w: 3, words: ['material', 'materiales', 'materials', 'bom', 'takeoff'] },
        { w: 3, words: ['despiece', 'despieces', 'desglose', 'breakdown'] },
        { w: 1.8, words: ['pieza', 'piezas', 'articulo', 'articulos', 'parts', 'part', 'pieces', 'piece', 'items', 'components'] },
        { w: 1, words: ['lista', 'listado', 'relacion', 'listar', 'list', 'listing'] },
        { w: 1, words: ['encofrado', 'encofrar', 'encofrados', 'encofrarlo', 'formwork', 'formworks', 'forms', 'form', 'shuttering'] },
        { w: 1.5, words: ['necesito', 'necesita', 'necesitamos', 'necesitan', 'falta', 'faltan', 'required', 'require', 'requires'] },
        { w: 1.8, words: ['need', 'needs', 'needed'] },
        { w: 1.5, words: ['lleva', 'llevan', 'llevo', 'llevar', 'usa', 'utiliza', 'uses'] },
        { w: 1, words: ['take', 'takes'] },
        { w: 0.4, words: ['hace', 'hacen'] },
        { w: 0.8, words: ['cuanto', 'cuantos', 'cuantas', 'cuanta', 'how', 'many', 'much'] },
        { w: 1.5, words: ['pesa', 'peso', 'pesan', 'kilos', 'heavy', 'pounds'] },
        { w: 1.8, words: ['weigh', 'weighs', 'weight'] },
        { w: 0.8, words: ['panel', 'paneles', 'grapa', 'grapas', 'barra', 'barras', 'tuerca', 'tuercas', 'mensula', 'mensulas', 'puntal', 'puntales',
            'panels', 'clamp', 'clamps', 'bars', 'nuts', 'bracket', 'brackets', 'prop', 'props', 'ties'] },
        { w: 0.6, words: ['dame', 'dime', 'damelo', 'muestrame', 'ensename', 'saca', 'sacame', 'quiero', 'ver', 'pasame', 'calcula',
            'give', 'show', 'get', 'send', 'display', 'want', 'see', 'tell'] },
        { w: 0.3, words: ['que', 'what', 'which'] },
        { w: 0.3, words: ['muro', 'muros', 'pared', 'wall', 'walls'] },
        { w: -3, words: ['borra', 'borrar', 'elimina', 'eliminar', 'mueve', 'mover', 'gira', 'girar', 'copia', 'copiar', 'dibuja', 'dibujar', 'crea', 'crear', 'deshaz', 'deshacer', 'rota', 'rotar',
            'delete', 'erase', 'remove', 'move', 'rotate', 'turn', 'copy', 'draw', 'create', 'undo', 'duplicate', 'mirror'] },
    ],
    replace: [
        { w: 3, words: ['sustituye', 'sustituir', 'sustituyelos', 'sustitucion', 'reemplaza', 'reemplazar', 'reemplazalos', 'replace', 'substitute', 'swap', 'replacement'] },
        { w: 2, words: ['cambia', 'cambiar', 'cambialos', 'quita', 'quitar', 'quitalos', 'change', 'exchange', 'remove'] },
        { w: 1.3, words: ['tengo', 'tenemos', 'dispongo', 'quedan', 'queda', 'have', 'got', 'own'] },
        { w: 0.7, words: ['no', 'solo', 'solamente', 'nada', 'ninguno', 'sin', 'not', 'only', 'just', 'without', 'none'] },
    ],
    help: [
        { w: 3, words: ['ayuda', 'ayudame', 'help', 'comandos', 'instrucciones', 'commands', 'instructions'] },
        { w: 2, words: ['hola', 'buenas', 'buenos', 'opciones', 'hello', 'hi', 'hey', 'options'] },
        { w: 1.5, words: ['sabes', 'puedes', 'can', 'could'] },
        { w: 0.8, words: ['hacer', 'haces', 'funciona', 'funcionas', 'do', 'does', 'work', 'works'] },
    ],
    thanks: [
        { w: 3, words: ['gracias', 'thanks', 'thank', 'cheers'] },
    ],
};

export const MA_STL_WALL_AI_SCORE = { high: 2, mid: 1, margin: 0.6 };

/**
 * Analiza el mensaje: idioma, puntuación por intención y confianza.
 * @param {string} text
 * @param {'es'|'en'} [fallbackLang] idioma si el mensaje no lo deja claro (conversación / interfaz)
 * @returns {{ intent: string|null, score: number, confidence: 'high'|'medium'|'low', scores: object, tokens: string[], hits: object, lang: 'es'|'en' }}
 */
export function maStlWallAiAnalyze(text, fallbackLang) {
    const lang = maStlWallAiDetectLanguage(text, fallbackLang || 'es').lang;
    const tokens = maStlWallAiTokens(text, lang);
    const scores = {};
    const hits = {};
    Object.keys(MA_STL_WALL_AI_VOCAB).forEach(function (intent) {
        let sum = 0;
        hits[intent] = [];
        const concepts = MA_STL_WALL_AI_VOCAB[intent];
        /* Cada palabra cuenta solo para su mejor concepto (evita «pesa» = peso + pieza). */
        const best = concepts.map(function () { return { q: 0, tok: '' }; });
        for (let i = 0; i < tokens.length; i++) {
            let bq = 0;
            let bc = -1;
            let bw = '';
            for (let c = 0; c < concepts.length; c++) {
                for (let j = 0; j < concepts[c].words.length; j++) {
                    const q = maStlWallAiWordMatch(tokens[i], concepts[c].words[j]);
                    if (q > bq) { bq = q; bc = c; bw = concepts[c].words[j]; }
                }
            }
            if (bc >= 0 && bq > best[bc].q) best[bc] = { q: bq, tok: tokens[i] + '→' + bw };
        }
        concepts.forEach(function (concept, c) {
            let q = best[c].q;
            if (concept.w < 0 && q < 0.8) q = 0;
            if (q > 0) {
                sum += concept.w * q;
                hits[intent].push(best[c].tok);
            }
        });
        scores[intent] = Math.round(sum * 100) / 100;
    });
    const order = Object.keys(scores).sort(function (a, b) { return scores[b] - scores[a]; });
    const top = order[0];
    const score = scores[top] || 0;
    const second = order.length > 1 ? scores[order[1]] : 0;
    let confidence = 'low';
    if (score >= MA_STL_WALL_AI_SCORE.high && score - second >= MA_STL_WALL_AI_SCORE.margin) confidence = 'high';
    else if (score >= MA_STL_WALL_AI_SCORE.mid) confidence = 'medium';
    return { intent: confidence === 'low' ? null : top, score: score, confidence: confidence, scores: scores, tokens: tokens, hits: hits, lang: lang };
}

/** Intención reconocida (confianza alta o media) o null. */
export function maStlWallAiDetectIntent(text, fallbackLang) {
    return maStlWallAiAnalyze(text, fallbackLang).intent;
}

const MA_STL_WALL_AI_YES = ['si', 'vale', 'claro', 'exacto', 'correcto', 'eso', 'afirmativo', 'venga', 'dale', 'aja', 'perfecto', 'bien',
    'yes', 'sure', 'ok', 'right', 'correct', 'exactly', 'absolutely', 'indeed', 'please'];
const MA_STL_WALL_AI_NO = ['no', 'nada', 'negativo', 'tampoco', 'not', 'nope', 'wrong'];
const MA_STL_WALL_AI_YESNO_SKIP = ['por', 'favor', 'gracias', 'thanks', 'thank', 'you'];

/** 'yes' | 'no' | null para respuestas cortas a «¿Querías decir…?» / «Did you mean…?». */
export function maStlWallAiYesNo(text, lang) {
    const toks = maStlWallAiTokens(text, lang).filter(function (t) { return MA_STL_WALL_AI_YESNO_SKIP.indexOf(t) < 0; });
    if (!toks.length || toks.length > 4) return null;
    const has = function (list) {
        return toks.some(function (t) { return list.some(function (w) { return maStlWallAiWordMatch(t, w) >= 0.95; }); });
    };
    if (has(MA_STL_WALL_AI_NO)) return 'no';
    if (has(MA_STL_WALL_AI_YES)) return 'yes';
    return null;
}

/* ---------- Números en español e inglés (siguiente fase: «solo tengo 3 paneles de 0,90») ---------- */

const MA_STL_WALL_AI_NUM_UNITS = {
    cero: 0, un: 1, uno: 1, una: 1, dos: 2, tres: 3, cuatro: 4, cinco: 5, seis: 6, siete: 7, ocho: 8, nueve: 9,
    diez: 10, once: 11, doce: 12, trece: 13, catorce: 14, quince: 15, dieciseis: 16, diecisiete: 17,
    dieciocho: 18, diecinueve: 19, veinte: 20, veintiun: 21, veintiuno: 21, veintiuna: 21, veintidos: 22,
    veintitres: 23, veinticuatro: 24, veinticinco: 25, veintiseis: 26, veintisiete: 27, veintiocho: 28,
    veintinueve: 29,
    zero: 0, oh: 0, one: 1, two: 2, three: 3, four: 4, five: 5, six: 6, seven: 7, eight: 8, nine: 9,
    ten: 10, eleven: 11, twelve: 12, thirteen: 13, fourteen: 14, fifteen: 15, sixteen: 16, seventeen: 17,
    eighteen: 18, nineteen: 19,
};
const MA_STL_WALL_AI_NUM_TENS = {
    treinta: 30, cuarenta: 40, cincuenta: 50, sesenta: 60, setenta: 70, ochenta: 80, noventa: 90,
    twenty: 20, thirty: 30, forty: 40, fourty: 40, fifty: 50, sixty: 60, seventy: 70, eighty: 80, ninety: 90,
};
const MA_STL_WALL_AI_NUM_HUNDREDS = {
    cien: 100, ciento: 100, doscientos: 200, doscientas: 200, trescientos: 300, trescientas: 300,
    cuatrocientos: 400, quinientos: 500, seiscientos: 600, setecientos: 700, ochocientos: 800, novecientos: 900,
};

function numWord(t) {
    const all = [MA_STL_WALL_AI_NUM_UNITS, MA_STL_WALL_AI_NUM_TENS, MA_STL_WALL_AI_NUM_HUNDREDS];
    for (let i = 0; i < all.length; i++) if (Object.prototype.hasOwnProperty.call(all[i], t)) return { v: all[i][t], k: i };
    /* tolerante a faltas: «nobenta», «ninty», «seventi»… (solo palabras de ≥5 letras) */
    if (t.length >= 5) {
        for (let i = 0; i < all.length; i++) {
            const keys = Object.keys(all[i]);
            for (let j = 0; j < keys.length; j++) {
                if (keys[j].length >= 5 && maStlWallAiWordMatch(t, keys[j]) >= 0.95) return { v: all[i][keys[j]], k: i };
            }
        }
        for (let i = 0; i < 2; i++) {
            const keys = Object.keys(all[i]);
            for (let j = 0; j < keys.length; j++) {
                if (keys[j].length >= 5 && maStlWallAiDamerau(t, keys[j]) <= 1) return { v: all[i][keys[j]], k: i };
            }
        }
    }
    return null;
}

/**
 * Número entero escrito en palabras, español o inglés (0..999 999): «noventa»/«ninety» 90 ·
 * «setenta y cinco»/«seventy five» 75 · «ciento veinte»/«one hundred and twenty» 120 ·
 * «dos mil cuatrocientos»/«two thousand four hundred» 2400. null si no es un número.
 */
export function maStlWallAiParseNumberWords(text) {
    const toks = maStlWallAiNormalize(text).split(' ').filter(Boolean);
    if (!toks.length) return null;
    let total = 0;
    let cur = 0;
    let any = false;
    for (let i = 0; i < toks.length; i++) {
        const t = toks[i];
        if ((t === 'y' || t === 'and') && any) continue;
        if (t === 'a' && i + 1 < toks.length && (toks[i + 1] === 'hundred' || toks[i + 1] === 'thousand')) continue;
        if (t === 'mil' || t === 'thousand') {
            total += (cur || 1) * 1000;
            cur = 0;
            any = true;
            continue;
        }
        if (t === 'hundred') {
            cur = (cur || 1) * 100;
            any = true;
            continue;
        }
        const w = numWord(t);
        if (!w) return null;
        cur += w.v;
        any = true;
    }
    return any ? total + cur : null;
}

/** Cifras de una parte decimal dicha en palabras: «nine» → '9' · «nine zero» → '90' · «ninety» → '90'. */
function decimalDigitsFromWords(txt) {
    const toks = maStlWallAiNormalize(txt).split(' ').filter(Boolean);
    if (!toks.length) return null;
    if (/^\d+$/.test(toks.join(''))) return toks.join('');
    let out = '';
    let i = 0;
    while (i < toks.length) {
        const w = numWord(toks[i]);
        if (!w) return null;
        if (w.k === 0 && w.v <= 9) { out += String(w.v); i++; continue; }
        const n = maStlWallAiParseNumberWords(toks.slice(i).join(' '));
        if (n == null) return null;
        return out + String(n);
    }
    return out;
}

/**
 * Medida en metros a partir de cómo se dice en obra (español o inglés):
 * «0,90» «0.90» «0'90» «,90» → 0,90 · «90» (entero ≥ 10 = cm) → 0,90 · «noventa»/«ninety» → 0,90 ·
 * «cero noventa»/«zero ninety» → 0,90 · «point nine»/«zero point nine» → 0,90 ·
 * «uno veinte»/«one twenty» / «un metro veinte»/«one meter twenty» / «1 20» → 1,20 · «1,2» → 1,20 ·
 * «one point two» → 1,20 · «120» / «ciento veinte» / «one hundred twenty» → 1,20 ·
 * «setenta y cinco»/«seventy five» → 0,75 · «medio metro»/«half a meter» → 0,50 · «90 cm» → 0,90 · «2 m» → 2.
 * @returns {{ m: number, mm: number }|null}
 */
export function maStlWallAiParseMeasure(text) {
    let s = String(text || '').toLowerCase().trim();
    try { s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (_e) { /* ignore */ }
    s = s.replace(/[’`´]/g, "'").replace(/-/g, ' ').replace(/\s+/g, ' ');
    const done = function (m) {
        if (!Number.isFinite(m) || m < 0) return null;
        const mm = Math.round(m * 1000);
        return { m: mm / 1000, mm: mm };
    };
    let unit = null;
    s = s.replace(/\s*(milimetros|milimetro|millimeters|millimetres|millimeter|millimetre|mm)$/, function () { unit = 'mm'; return ''; })
        .replace(/\s*(centimetros|centimetro|centimeters|centimetres|centimeter|centimetre|cms|cm)$/, function () { unit = 'cm'; return ''; })
        .replace(/\s*(metros|metro|meters|metres|meter|metre|mts|mt|m)$/, function () { unit = unit || 'm'; return ''; })
        .trim();
    if (/^(medio|medio metro|half|half a|a half|half a meter|half a metre|half meter|half metre)$/.test(s)) return done(0.5);
    /* cifras: 0,90 · 0.90 · 0'90 · ,90 · 1,2 */
    let mt = /^(\d*)\s*[,.']\s*(\d+)$/.exec(s);
    if (mt) {
        const v = Number((mt[1] || '0') + '.' + mt[2]);
        return done(unit === 'cm' ? v / 100 : unit === 'mm' ? v / 1000 : v);
    }
    /* «point nine» · «zero point nine» · «one point two» · «0 point 9» */
    mt = /^(.*?)\s*\b(point|dot|coma)\b\s*(.+)$/.exec(s);
    if (mt) {
        const intTxt = mt[1].trim();
        const ip = !intTxt ? 0 : (/^\d+$/.test(intTxt) ? Number(intTxt) : maStlWallAiParseNumberWords(intTxt));
        const dec = decimalDigitsFromWords(mt[3]);
        if (ip != null && dec) return done(Number(String(ip) + '.' + dec));
        return null;
    }
    /* «1 20» (metro y centímetros) */
    mt = /^(\d)\s+(\d{1,2})$/.exec(s);
    if (mt) return done(Number(mt[1]) + Number(mt[2]) / (mt[2].length === 1 ? 10 : 100));
    mt = /^\d+$/.exec(s);
    if (mt) {
        const v = Number(s);
        if (unit === 'mm') return done(v / 1000);
        if (unit === 'cm') return done(v / 100);
        if (unit === 'm') return done(v);
        return done(v >= 10 ? v / 100 : v);
    }
    /* palabras: «X metro(s) Y» / «X Y» con X en 0..9 y Y en 1..99 → X,YY */
    const toks = s.replace(/\b(metros|metro|meters|metres|meter|metre|con)\b/g, ' ').replace(/\s+/g, ' ').trim().split(' ').filter(Boolean);
    if (!toks.length) return null;
    const wholeWords = maStlWallAiParseNumberWords(toks.join(' '));
    if (toks.length >= 2) {
        const first = numWord(toks[0]);
        const rest = maStlWallAiParseNumberWords(toks.slice(1).join(' '));
        if (first && first.k === 0 && first.v <= 9 && rest != null && rest > 0 && rest < 100
            && toks[1] !== 'y' && toks[1] !== 'and' && toks[1] !== 'hundred' && toks[1] !== 'thousand') {
            const restTxt = toks.slice(1).join(' ');
            const dec = rest < 10 && !/^(cero|zero|oh|0)/.test(restTxt) ? rest / 10 : rest / 100;
            return done(first.v + dec);
        }
    }
    if (wholeWords != null) {
        if (unit === 'mm') return done(wholeWords / 1000);
        if (unit === 'cm') return done(wholeWords / 100);
        if (unit === 'm') return done(wholeWords);
        return done(wholeWords >= 10 ? wholeWords / 100 : wholeWords);
    }
    return null;
}

function finiteNum(v) {
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
}

function segDistMm(px, pz, ax, az, bx, bz) {
    const dx = bx - ax;
    const dz = bz - az;
    const len2 = dx * dx + dz * dz;
    if (len2 < 1e-9) return Math.hypot(px - ax, pz - az);
    const t = ((px - ax) * dx + (pz - az) * dz) / len2;
    return { line: Math.abs((px - ax) * dz - (pz - az) * dx) / Math.sqrt(len2), t: t };
}

/**
 * IdWall del cálculo ATK-60 que corresponden al muro editado.
 * 1) Claves del muro (`fw.idWallKeys`, en orden) que aparecen como `IdWall` en las piezas.
 * 2) Si ninguna aparece y hay eje recto (`fw.axisMm`), anclas del cálculo (`Debug.Start/End`)
 *    colineales (≤ 5 mm) y solapadas con el eje.
 * @returns {string[]}
 */
export function maStlWallAiResolveIdWalls(fw) {
    if (!fw) return [];
    const elements = typeof fw.elements === 'function' ? (fw.elements() || []) : [];
    const present = Object.create(null);
    for (let i = 0; i < elements.length; i++) {
        const id = elements[i] && elements[i].IdWall;
        if (id != null && id !== '') present[String(id)] = true;
    }
    const keys = Array.isArray(fw.idWallKeys) ? fw.idWallKeys : [];
    for (let i = 0; i < keys.length; i++) {
        if (keys[i] == null || keys[i] === '') continue;
        if (present[String(keys[i])]) return [String(keys[i])];
    }
    const axis = fw.axisMm;
    const anchors = typeof fw.anchors === 'function' ? (fw.anchors() || []) : [];
    if (!axis || !axis.p1 || !axis.p2 || !anchors.length) return [];
    const ax = finiteNum(axis.p1.x);
    const az = finiteNum(axis.p1.z);
    const bx = finiteNum(axis.p2.x);
    const bz = finiteNum(axis.p2.z);
    if (ax == null || az == null || bx == null || bz == null) return [];
    const out = [];
    for (let i = 0; i < anchors.length; i++) {
        const a = anchors[i] || {};
        const d = a.Debug || a.debug;
        const id = a.IdWall != null ? String(a.IdWall) : '';
        if (!d || !id || !present[id] || out.indexOf(id) >= 0) continue;
        const sx = finiteNum(d.StartX);
        const sz = finiteNum(d.StartZ);
        const ex = finiteNum(d.EndX);
        const ez = finiteNum(d.EndZ);
        if (sx == null || sz == null || ex == null || ez == null) continue;
        const r1 = segDistMm(sx, sz, ax, az, bx, bz);
        const r2 = segDistMm(ex, ez, ax, az, bx, bz);
        if (typeof r1 === 'number' || typeof r2 === 'number') continue;
        if (r1.line > 5 || r2.line > 5) continue;
        const lo = Math.min(r1.t, r2.t);
        const hi = Math.max(r1.t, r2.t);
        if (hi <= 0.001 || lo >= 0.999) continue;
        out.push(id);
    }
    return out;
}

/** POST al mismo endpoint que el popup «Cantidad de material» (PreviewMaterialCount). */
export function maStlWallAiFetchMaterialRows(url, items) {
    if (!url) return Promise.reject(new Error('sin URL de catálogo'));
    if (typeof fetch !== 'function') return Promise.reject(new Error('fetch no disponible'));
    return fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json; charset=utf-8', 'X-Requested-With': 'XMLHttpRequest' },
        body: JSON.stringify({ items: items }),
    }).then(function (resp) {
        if (!resp.ok) throw new Error('HTTP ' + resp.status);
        return resp.json();
    }).then(function (data) {
        if (!data || data.ok === false || !Array.isArray(data.rows)) {
            throw new Error((data && data.message) || 'respuesta sin filas');
        }
        return data.rows;
    });
}

/** Número con separadores del idioma: es 1.667,0 · en 1,667.0. */
function fmtNum(n, decimals, lang) {
    const v = Number(n);
    if (!Number.isFinite(v)) return '';
    const fixed = v.toFixed(decimals);
    const parts = fixed.split('.');
    const en = lang === 'en';
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, en ? ',' : '.');
    return parts.length > 1 ? parts[0] + (en ? '.' : ',') + parts[1] : parts[0];
}

/** Tabla de la burbuja a partir de las filas del servidor (o de las filas de escena si falla el catálogo). */
export function maStlWallAiBuildMaterialTable(rows, T) {
    T = Object.assign({}, maStlWallAiTexts(T && T.lang), T || {});
    const lang = T.lang;
    const list = (Array.isArray(rows) ? rows : []).filter(function (r) { return r && Number(r.qty) > 0; });
    let qty = 0;
    let weight = 0;
    let hasWeight = false;
    const out = list.map(function (r) {
        const q = Number(r.qty) || 0;
        const w = Number(r.totalWeight);
        qty += q;
        if (Number.isFinite(w) && w > 0) {
            weight += w;
            hasWeight = true;
        }
        return {
            code: String(r.code || r.sceneCode || ''),
            /* Descripción del catálogo: en inglés solo si el servidor la trae (nameEn/textLabelEn);
               si no, la original. No se traducen nombres de artículos. */
            name: String((lang === 'en' && (r.nameEn || r.textLabelEn || r.TextLabelEn)) || r.name || ''),
            qty: q,
            weight: Number.isFinite(w) && w > 0 ? w : null,
        };
    });
    out.sort(function (a, b) { return a.code < b.code ? -1 : (a.code > b.code ? 1 : 0); });
    const columns = [
        { key: 'code', label: T.colCode },
        { key: 'name', label: T.colName },
        { key: 'qty', label: T.colQty, align: 'right', format: function (v) { return fmtNum(v, 0, lang); } },
    ];
    if (hasWeight) {
        columns.push({ key: 'weight', label: T.colWeight, align: 'right', format: function (v) { return v == null ? '' : fmtNum(v, 1, lang); } });
    }
    return {
        columns: columns,
        rows: out,
        totals: { code: T.total, name: '', qty: qty, weight: hasWeight ? weight : null },
        totalQty: qty,
        totalWeight: hasWeight ? weight : null,
        lang: lang,
        labels: { copy: T.copy, copied: T.copied, copyFail: T.copyFail },
    };
}

/** Texto TSV (para pegar en Excel) de una tabla de la burbuja. */
export function maStlWallAiTableToTsv(table) {
    if (!table || !Array.isArray(table.columns)) return '';
    const cell = function (col, row) {
        const v = row[col.key];
        const txt = col.format ? col.format(v) : (v == null ? '' : String(v));
        return String(txt).replace(/[\t\r\n]+/g, ' ');
    };
    const lines = [table.columns.map(function (c) { return c.label; }).join('\t')];
    for (let i = 0; i < table.rows.length; i++) {
        lines.push(table.columns.map(function (c) { return cell(c, table.rows[i]); }).join('\t'));
    }
    if (table.totals) lines.push(table.columns.map(function (c) { return cell(c, table.totals); }).join('\t'));
    return lines.join('\r\n');
}

function wallTitle(ctx, T) {
    T = T || MA_STL_WALL_AI_TEXT;
    const label = fmtWallLabel(ctx, T);
    return label || T.wall;
}

/**
 * Orden «material de encofrado de este muro».
 * Usa `ctx.formwork` (lo rellena el visor): mismas piezas y mismo endpoint que el popup
 * «Cantidad de material», filtradas por el `IdWall` de este muro.
 */
export function maStlWallAiMaterialsReply(ctx, T) {
    T = Object.assign({}, maStlWallAiTexts(T && T.lang), T || {});
    if (ctx && ctx.kind === 'curved-new') return Promise.resolve({ reply: T.matNew, actions: [] });
    const fw = ctx && ctx.formwork;
    if (!fw || typeof fw.countRows !== 'function') return Promise.resolve({ reply: T.matNoAccess, actions: [] });
    const runButton = function (label) {
        if (typeof fw.runFormwork !== 'function') return [];
        return [{
            label: label,
            primary: true,
            run: function () {
                const p = fw.runFormwork();
                if (!p || typeof p.then !== 'function') return { reply: T.matRunUnavailable, actions: [] };
                return p.then(function (detail) {
                    if (detail && detail.ok === false) {
                        return { reply: fmt(T.matRunFail, detail.message ? ': ' + detail.message : ''), actions: [] };
                    }
                    return maStlWallAiMaterialsReply(ctx, T);
                });
            },
            pendingText: T.matRunning,
        }];
    };
    const any = typeof fw.hasAny === 'function' ? !!fw.hasAny() : true;
    const ids = any ? maStlWallAiResolveIdWalls(fw) : [];
    const set = Object.create(null);
    ids.forEach(function (id) { set[id] = true; });
    const sceneRows = ids.length
        ? (fw.countRows(function (item) { return !!(item && item.IdWall != null && set[String(item.IdWall)]); }) || [])
        : [];
    if (!sceneRows.length) {
        /* Sin piezas pintadas de este muro: se calcula SOLO este muro (vista previa, no se pinta). */
        if (typeof fw.previewWall === 'function') {
            return Promise.resolve(fw.previewWall({})).then(function (res) {
                const list = res && res.ok && Array.isArray(res.elements) ? res.elements : [];
                const rows = list.length ? (fw.countRows(null, list) || []) : [];
                if (!rows.length) {
                    if (typeof fw.clearTempWall === 'function') fw.clearTempWall();
                    const why = res && !res.ok ? fmt(T.matPreviewFail, res.message || '?') + '\n' : '';
                    return { reply: why + (any ? T.matNoneForWall : T.matNone) + '\n' + T.matRunHint, actions: [], buttons: runButton(any ? T.matRerun : T.matRun) };
                }
                const pnotes = [];
                if (res.contextWalls > 1) pnotes.push(fmt(T.matPreviewContext, res.contextWalls));
                if (res.notes === null) pnotes.push(T.matPreviewOld);
                if (ctx.kind === 'straight') pnotes.push(T.matCornerNote);
                /* Recuento = encofrar SOLO este muro en el visor (temporal; mismas piezas que la lista). */
                const draw = typeof fw.showTempWall === 'function'
                    ? Promise.resolve().then(function () { return fw.showTempWall(res); }).then(function (shown) {
                        if (shown && shown.ok) pnotes.push(fmt(shown.visible === false ? T.matTempHidden : T.matTempShown, fmtNum(rows.reduce(function (a, r) { return a + (Number(r.qty) || 0); }, 0), 0, T.lang)));
                        else if (shown && !shown.cancelled) pnotes.push(fmt(T.matTempFail, '0'));
                    }, function (err) { pnotes.push(fmt(T.matTempFail, err && err.message ? err.message : String(err))); })
                    : Promise.resolve();
                return draw.then(function () { return materialsFinish(ctx, T, fw, [String(res.idWall)], rows, pnotes, true, res); });
            });
        }
        if (!any) return Promise.resolve({ reply: T.matNone + '\n' + T.matRunHint, actions: [], buttons: runButton(T.matRun) });
        return Promise.resolve({ reply: T.matNoneForWall + '\n' + T.matRunHint, actions: [], buttons: runButton(T.matRerun) });
    }
    /* El muro ya tiene piezas del «Encofrar» real: no hace falta el provisional. */
    if (typeof fw.clearTempWall === 'function') fw.clearTempWall();
    const notes = [];
    if (typeof fw.isEdited === 'function' && ids.some(function (id) { return fw.isEdited(id); })) notes.push(T.matEdited);
    if (ctx.kind === 'straight') notes.push(T.matCornerNote);
    return materialsFinish(ctx, T, fw, ids, sceneRows, notes, false, null);
}

/** Tabla de materiales con nombres del catálogo (PreviewMaterialCount) a partir de las filas de escena. */
function materialsFinish(ctx, T, fw, ids, sceneRows, notes, preview, previewRes) {
    const finish = function (rows, warn) {
        const table = maStlWallAiBuildMaterialTable(rows, T);
        const head = fmt(T.matTitle, wallTitle(ctx, T));
        const sum = fmt(preview ? T.matSummaryPreview : T.matSummary, table.rows.length, fmtNum(table.totalQty, 0, T.lang),
            table.totalWeight != null ? fmt(T.matWeight, fmtNum(table.totalWeight, 1, T.lang)) : '');
        const lines = [head, sum].concat(warn ? [warn] : [], notes);
        return {
            reply: lines.join('\n'),
            /* En voz alta, solo el resumen (la tabla se queda en pantalla). */
            speech: fmt(T.matSpeech, fmtNum(table.totalQty, 0, T.lang), table.rows.length),
            actions: [],
            table: table,
            data: { idWalls: ids, sceneRows: sceneRows, rows: rows, preview: !!preview, previewResult: previewRes || null },
        };
    };
    const fetcher = typeof fw.fetchRows === 'function'
        ? fw.fetchRows
        : function (items) { return maStlWallAiFetchMaterialRows(fw.previewUrl, items); };
    return Promise.resolve()
        .then(function () { return fetcher(sceneRows); })
        .then(function (rows) {
            const keys = sceneRows.map(function (r) { return r.code; }).sort();
            for (let i = 0; i < rows.length; i++) {
                if (rows[i] && !rows[i].sceneCode && keys[i]) rows[i].sceneCode = keys[i];
            }
            return finish(rows, '');
        })
        .catch(function (err) {
            const fallback = sceneRows.map(function (r) { return { code: r.code, sceneCode: r.code, name: '', qty: r.qty }; });
            return finish(fallback, fmt(T.matNoCatalog, err && err.message ? err.message : String(err)));
        });
}

/* ===================== Paneles que hay en obra (fase 3) =====================
 * «no tengo paneles de 0,90, sustitúyelos» → excluir el 0,90 en este muro.
 * «solo tengo 3 paneles de 0,90» → como mucho 3 de 0,90 en este muro (las dos caras).
 * Un número que no es cantidad ni ancho («del 19») no se interpreta: se pregunta. */

/** Anchos de panel del muro recto (mm) y modulaciones del curvo. */
export const MA_STL_WALL_AI_PANEL_WIDTHS_MM = [300, 450, 600, 750, 900];
const MA_STL_WALL_AI_ANY_WIDTHS_MM = [300, 450, 600, 750, 900, 1050, 1200];
const STOCK_PANEL_WORDS = ['panel', 'paneles', 'panels', 'placa', 'placas', 'modulo', 'modulos', 'module', 'modules',
    'unidades', 'unidad', 'units', 'unit', 'uds', 'ud', 'pieces', 'piezas'];
const STOCK_QTY_MARKERS = ['solo', 'solamente', 'unicamente', 'only', 'just', 'tengo', 'tenemos', 'quedan', 'queda', 'hay',
    'have', 'got', 'left', 'dispongo', 'disponemos', 'maximo', 'max', 'maximum'];
const STOCK_NEG = ['no', 'ninguno', 'ninguna', 'ningun', 'sin', 'nada', 'not', 'none', 'without', 'agotado', 'agotados',
    'acabado', 'acabados', 'out', 'nunca', 'never'];
const STOCK_VERBS = ['sustituye', 'sustituir', 'sustituyelos', 'reemplaza', 'reemplazar', 'reemplazalos', 'cambia', 'cambiar',
    'cambialos', 'quita', 'quitar', 'quitalos', 'replace', 'substitute', 'swap', 'change', 'remove', 'exclude', 'excluye'];
const STOCK_GLUE = ['de', 'del', 'of', 'the', 'los', 'las', 'el', 'la', 'a', 'en', 'with', 'con', 'ancho', 'width', 'wide'];

function stockIsNumTok(t) {
    return /^\d+$/.test(t) || !!numWord(t) || ['point', 'dot', 'coma', 'medio', 'half'].indexOf(t) >= 0;
}

function stockWordIn(t, list) {
    return list.some(function (w) { return maStlWallAiWordMatch(t, w) >= (w.length <= 3 ? 1 : 0.8); });
}

/**
 * Lee la orden de paneles: anchos, cantidades y lo que no se entiende.
 * @returns {{ items: Array<{mm:number, max:number|null, exclude:boolean}>, unknown: string[], negative: boolean,
 *            verb: boolean, widthsMm: number[] }}
 */
export function maStlWallAiParseStockOrder(text, lang) {
    const toks = maStlWallAiTokens(text, lang);
    const used = toks.map(function () { return false; });
    const qtys = [];
    const intOf = function (from, to) {
        const words = toks.slice(from, to + 1);
        if (words.length === 1 && /^\d+$/.test(words[0])) return Number(words[0]);
        if (words.some(function (w) { return /^\d+$/.test(w); })) return null;
        const v = maStlWallAiParseNumberWords(words.join(' '));
        return v != null && Number.isInteger(v) ? v : null;
    };
    /* 1) cantidades: «3 paneles», «tres placas», «solo tengo 3 de 0,90», «only have three of 0.90» */
    for (let i = 0; i < toks.length; i++) {
        if (used[i] || !stockIsNumTok(toks[i]) || /^(point|dot|coma|medio|half)$/.test(toks[i])) continue;
        let best = null;
        for (let j = Math.min(toks.length - 2, i + 2); j >= i; j--) {
            const v = intOf(i, j);
            if (v == null) continue;
            const next = toks[j + 1];
            const isPanel = next != null && stockWordIn(next, STOCK_PANEL_WORDS);
            const prev = toks.slice(Math.max(0, i - 3), i);
            const afterMarker = prev.some(function (p) { return STOCK_QTY_MARKERS.indexOf(p) >= 0; })
                && next != null && (next === 'de' || next === 'of' || isPanel);
            const afterDe = i > 0 && (toks[i - 1] === 'de' || toks[i - 1] === 'of' || toks[i - 1] === 'del');
            /* «0,90 paneles» → «0 90 paneles»: el 90 es parte de la medida, no una cantidad */
            const inMeasure = i > 0 && (/^\d+$/.test(toks[i - 1]) || ['point', 'dot', 'coma', 'cero', 'zero'].indexOf(toks[i - 1]) >= 0);
            /* «solo tengo 3 de 0,90 y 2 de 0,75»: la segunda cantidad sigue a «y»/«and» */
            const listed = qtys.length > 0 && i > 0 && (toks[i - 1] === 'y' || toks[i - 1] === 'and' || toks[i - 1] === 'e')
                && next != null && (next === 'de' || next === 'of' || isPanel);
            if ((isPanel || afterMarker || listed) && !afterDe && !inMeasure) { best = { from: i, to: j, v: v }; break; }
        }
        if (best) {
            for (let k = best.from; k <= best.to; k++) used[k] = true;
            qtys.push({ at: best.from, v: best.v });
            i = best.to;
        }
    }
    /* 2) anchos: la ventana más larga que ParseMeasure lee como ancho de panel */
    const widths = [];
    for (let i = 0; i < toks.length; i++) {
        if (used[i] || !stockIsNumTok(toks[i]) || toks[i] === 'y' || toks[i] === 'and') continue;
        let hit = null;
        for (let j = Math.min(toks.length - 1, i + 4); j >= i; j--) {
            let blocked = false;
            for (let k = i; k <= j; k++) if (used[k] || stockWordIn(toks[k], STOCK_PANEL_WORDS)) blocked = true;
            if (blocked) continue;
            const m = maStlWallAiParseMeasure(toks.slice(i, j + 1).join(' '));
            if (m && MA_STL_WALL_AI_ANY_WIDTHS_MM.indexOf(m.mm) >= 0) { hit = { from: i, to: j, mm: m.mm }; break; }
        }
        if (hit) {
            for (let k = hit.from; k <= hit.to; k++) used[k] = true;
            widths.push(hit);
            i = hit.to;
        }
    }
    /* 3) números que sobran («del 19»): no se interpretan */
    const unknown = [];
    for (let i = 0; i < toks.length; i++) {
        if (used[i] || !/^\d+$/.test(toks[i]) && !(numWord(toks[i]) && toks[i].length > 2)) continue;
        let from = i;
        while (from > 0 && !used[from - 1] && STOCK_GLUE.indexOf(toks[from - 1]) >= 0 && i - from < 2) from--;
        unknown.push(toks.slice(from, i + 1).join(' '));
        used[i] = true;
    }
    const negative = toks.some(function (t) { return STOCK_NEG.indexOf(t) >= 0; }) || /[a-z]n['’]t\b/i.test(String(text || ''));
    const verb = toks.some(function (t) { return stockWordIn(t, STOCK_VERBS); });
    /* cada ancho se queda con la cantidad que tiene delante (desde el ancho anterior) */
    const items = widths.map(function (w, n) {
        const lo = n > 0 ? widths[n - 1].to : -1;
        const q = qtys.filter(function (x) { return x.at > lo && x.at < w.from; }).pop()
            || (n === widths.length - 1 ? qtys.filter(function (x) { return x.at > w.to; })[0] : null);
        if (q) return { mm: w.mm, max: q.v > 0 ? q.v : null, exclude: q.v === 0 };
        return { mm: w.mm, max: null, exclude: true };
    });
    return {
        items: items,
        unknown: unknown,
        negative: negative,
        verb: verb,
        widthsMm: widths.map(function (w) { return w.mm; }),
    };
}

/** { excludeMm:[...], maxMm:{ '900': 3 } } normalizado (o null si no hay regla). */
export function maStlWallAiNormalizeStock(raw) {
    let src = raw;
    if (typeof src === 'string') {
        try { src = JSON.parse(src); } catch (_e) { src = null; }
    }
    const out = { excludeMm: [], maxMm: {} };
    if (src && Array.isArray(src.excludeMm)) {
        src.excludeMm.forEach(function (v) {
            const mm = Math.round(Number(v) < 10 ? Number(v) * 1000 : Number(v));
            if (MA_STL_WALL_AI_PANEL_WIDTHS_MM.indexOf(mm) >= 0 && out.excludeMm.indexOf(mm) < 0) out.excludeMm.push(mm);
        });
    }
    if (src && src.maxMm && typeof src.maxMm === 'object') {
        Object.keys(src.maxMm).forEach(function (k) {
            const mm = Math.round(Number(k) < 10 ? Number(k) * 1000 : Number(k));
            const n = Math.floor(Number(src.maxMm[k]));
            if (MA_STL_WALL_AI_PANEL_WIDTHS_MM.indexOf(mm) < 0 || !Number.isFinite(n) || n < 0) return;
            if (n === 0) { if (out.excludeMm.indexOf(mm) < 0) out.excludeMm.push(mm); } else out.maxMm[String(mm)] = n;
        });
    }
    out.excludeMm.sort(function (a, b) { return b - a; });
    return out.excludeMm.length || Object.keys(out.maxMm).length ? out : null;
}

/** Regla actual del muro + orden nueva (la orden manda sobre el mismo ancho). */
export function maStlWallAiMergeStock(current, items) {
    const cur = maStlWallAiNormalizeStock(current) || { excludeMm: [], maxMm: {} };
    const out = { excludeMm: cur.excludeMm.slice(), maxMm: Object.assign({}, cur.maxMm) };
    (items || []).forEach(function (it) {
        const k = String(it.mm);
        if (it.exclude) {
            delete out.maxMm[k];
            if (out.excludeMm.indexOf(it.mm) < 0) out.excludeMm.push(it.mm);
        } else if (it.max != null) {
            out.excludeMm = out.excludeMm.filter(function (v) { return v !== it.mm; });
            out.maxMm[k] = it.max;
        }
    });
    return maStlWallAiNormalizeStock(out);
}

/** Ancho de un panel por su código de catálogo (27904209 → 900); null si no es un panel. */
export function maStlWallAiPanelWidthMm(code) {
    const m = /^(?:PANEL_)?(27|24|12)(30|45|60|75|90)\d{4}$/i.exec(String(code || ''));
    return m ? Number(m[2]) * 10 : null;
}

function panelsByWidth(rows) {
    const map = {};
    (rows || []).forEach(function (r) {
        const w = maStlWallAiPanelWidthMm(r.code);
        if (w) map[w] = (map[w] || 0) + (Number(r.qty) || 0);
    });
    return map;
}

function fmtWidth(mm, lang) {
    return fmtDec(mm / 1000, lang);
}

/** Compensación del manual NEVI (3.2, p. 41–43) para un hueco en mm. */
function compensationFor(gapMm, T) {
    if (gapMm <= 50) return T.comp0_5;
    if (gapMm <= 180) return T.comp5_18;
    if (gapMm <= 300) return T.comp8_30;
    return T.compNone;
}

/** Mayor suma ≤ L con los anchos permitidos (para explicar el hueco que quedaría). */
function bestFitMm(lengthMm, allowed) {
    let best = 0;
    const reach = new Array(lengthMm + 1).fill(false);
    reach[0] = true;
    for (let s = 0; s <= lengthMm; s += 50) {
        if (!reach[s]) continue;
        if (s > best) best = s;
        allowed.forEach(function (w) { if (s + w <= lengthMm) reach[s + w] = true; });
    }
    return best;
}

function stockLabel(stock, T) {
    if (!stock) return '';
    const parts = [];
    stock.excludeMm.forEach(function (mm) { parts.push(fmt(T.stockNoWidth, fmtWidth(mm, T.lang))); });
    Object.keys(stock.maxMm).forEach(function (k) { parts.push(fmt(T.stockMaxWidth, stock.maxMm[k], fmtWidth(Number(k), T.lang))); });
    return parts.join(' · ');
}

function itemsLabel(items, T) {
    return (items || []).map(function (it) {
        return it.exclude ? fmt(T.stockNoWidth, fmtWidth(it.mm, T.lang)) : fmt(T.stockMaxWidth, it.max, fmtWidth(it.mm, T.lang));
    }).join(' · ');
}

/** Filas con nombre (PreviewMaterialCount) de una lista de piezas. */
function namedRows(fw, items) {
    const sceneRows = typeof fw.countRows === 'function' ? (fw.countRows(null, items) || []) : [];
    const fetcher = typeof fw.fetchRows === 'function'
        ? fw.fetchRows
        : function (rows) { return maStlWallAiFetchMaterialRows(fw.previewUrl, rows); };
    if (!sceneRows.length) return Promise.resolve({ sceneRows: sceneRows, rows: [], catalogError: null });
    return Promise.resolve()
        .then(function () { return fetcher(sceneRows); })
        .then(function (rows) { return { sceneRows: sceneRows, rows: rows, catalogError: null }; })
        .catch(function (err) {
            return {
                sceneRows: sceneRows,
                rows: sceneRows.map(function (r) { return { code: r.code, sceneCode: r.code, name: '', qty: r.qty }; }),
                catalogError: err && err.message ? err.message : String(err),
            };
        });
}

/** Tabla ANTES / DESPUÉS / DIFERENCIA (solo las referencias que cambian). */
export function maStlWallAiBuildDiffTable(beforeRows, afterRows, T) {
    T = Object.assign({}, maStlWallAiTexts(T && T.lang), T || {});
    const lang = T.lang;
    const map = {};
    const take = function (rows, key) {
        (rows || []).forEach(function (r) {
            const code = String(r.code || r.sceneCode || '');
            if (!code) return;
            if (!map[code]) map[code] = { code: code, name: '', before: 0, after: 0, wb: 0, wa: 0, w: null };
            if (map[code].w == null) map[code].w = maStlWallAiPanelWidthMm(r.sceneCode) || maStlWallAiPanelWidthMm(code);
            const name = (lang === 'en' && (r.nameEn || r.textLabelEn)) || r.name || '';
            if (name && !map[code].name) map[code].name = String(name);
            map[code][key] += Number(r.qty) || 0;
            const w = Number(r.totalWeight);
            if (Number.isFinite(w)) map[code][key === 'before' ? 'wb' : 'wa'] += w;
        });
    };
    take(beforeRows, 'before');
    take(afterRows, 'after');
    /* primero los paneles (de más ancho a menos), después el resto por código */
    const all = Object.keys(map).map(function (k) { return map[k]; }).sort(function (a, b) {
        const pa = a.w ? 0 : 1;
        const pb = b.w ? 0 : 1;
        if (pa !== pb) return pa - pb;
        if (a.w && b.w && a.w !== b.w) return b.w - a.w;
        return a.code < b.code ? -1 : (a.code > b.code ? 1 : 0);
    });
    const rows = all.filter(function (r) { return r.before !== r.after; }).map(function (r) {
        return { code: r.code, name: r.name, before: r.before, after: r.after, diff: r.after - r.before };
    });
    const sum = function (k) { return all.reduce(function (a, r) { return a + r[k]; }, 0); };
    const signed = function (v) { return (v > 0 ? '+' : '') + fmtNum(v, 0, lang); };
    return {
        columns: [
            { key: 'code', label: T.colCode },
            { key: 'name', label: T.colName },
            { key: 'before', label: T.colBefore, align: 'right', format: function (v) { return fmtNum(v, 0, lang); } },
            { key: 'after', label: T.colAfter, align: 'right', format: function (v) { return fmtNum(v, 0, lang); } },
            { key: 'diff', label: T.colDiff, align: 'right', format: signed },
        ],
        rows: rows,
        totals: { code: T.total, name: '', before: sum('before'), after: sum('after'), diff: sum('after') - sum('before') },
        weightBefore: sum('wb'),
        weightAfter: sum('wa'),
        unchanged: all.length - rows.length,
        /* antes/después: que se vean todas las filas que cambian (suelen ser pocas) */
        maxHeight: '235px',
        lang: lang,
        labels: { copy: T.copy, copied: T.copied, copyFail: T.copyFail },
    };
}

function sceneItemsOf(fw, idWall) {
    const els = typeof fw.elements === 'function' ? (fw.elements() || []) : [];
    return els.filter(function (e) { return e && e.IdWall != null && String(e.IdWall) === String(idWall); });
}

/** Piezas de «antes»: las pintadas del muro si las hay; si no, cálculo silencioso de solo este muro. */
function beforeOf(fw, after, attrs) {
    const scene = after && after.ok ? sceneItemsOf(fw, after.idWall) : [];
    if (scene.length && !attrs) return Promise.resolve({ ok: true, idWall: after.idWall, elements: scene, fromScene: true });
    return fw.previewWall(attrs ? { attrs: attrs } : {});
}

function undoButton(fw, idWall, applied, attrsKey, T, extraUndo) {
    return onceButton(T.stockUndo, function () {
        return (function () {
            const prevAttrs = {};
            prevAttrs[attrsKey] = applied.previousAttrs ? applied.previousAttrs[attrsKey] : null;
            if (typeof extraUndo === 'function') extraUndo();
            return Promise.resolve(fw.applyWall(idWall, applied.previous || [], attrsKey ? prevAttrs : {}))
                .then(function () { return { reply: T.stockUndone, actions: [] }; });
        })();
    }, T);
}

/**
 * Botones Sí/No de un cambio pendiente: «Sí» aplica una sola vez (botón o «sí» escrito); «No» después
 * de aplicar no deshace (para eso está «Deshacer»).
 */
function confirmButtons(applyFn, T) {
    const st = { state: 'pending', result: null };
    const apply = function () {
        if (st.state === 'applied') return { reply: T.stockAlready, actions: [] };
        if (st.state === 'cancelled') return { reply: T.stockCancelledAlready, actions: [] };
        if (st.state === 'running') return st.result;
        st.state = 'running';
        st.result = Promise.resolve().then(applyFn).then(function (r) { st.state = 'applied'; return r; }, function (err) { st.state = 'pending'; throw err; });
        return st.result;
    };
    const cancel = function () {
        if (st.state === 'applied' || st.state === 'running') return { reply: T.stockAlready, actions: [] };
        st.state = 'cancelled';
        return { reply: T.stockCancelled, actions: [] };
    };
    return {
        apply: apply,
        cancel: cancel,
        buttons: [
            { label: T.stockApply, primary: true, run: apply, pendingText: T.stockApplying },
            { label: T.no, run: cancel },
        ],
    };
}

/** «Deshacer» de un solo uso. */
function onceButton(label, fn, T) {
    let used = false;
    return {
        label: label,
        run: function () {
            if (used) return { reply: T.stockUndoneAlready, actions: [] };
            used = true;
            return fn();
        },
    };
}

/**
 * Orden de paneles en obra para el muro del contexto (recto o curvo): ANTES / DESPUÉS y botones Sí/No.
 * @param {object} ctx contexto del muro (formwork: previewWall/applyWall/wallAttr del visor)
 * @param {object} order maStlWallAiParseStockOrder(...)
 * @param {object} T textos del idioma
 * @param {{ ignoreUnknown?: boolean, text?: string }} [opts]
 */
export function maStlWallAiStockReply(ctx, order, T, opts) {
    T = Object.assign({}, maStlWallAiTexts(T && T.lang), T || {});
    opts = opts || {};
    if (!order || !order.items.length) return Promise.resolve({ reply: T.stockAskWidth, actions: [] });
    if (order.unknown.length && !opts.ignoreUnknown) {
        const understood = itemsLabel(order.items, T);
        const odd = order.unknown.map(function (u) { return '«' + u + '»'; }).join(', ');
        return Promise.resolve({
            reply: fmt(T.stockAskRef, understood, odd),
            actions: [],
            buttons: [{
                label: fmt(T.stockIgnoreRef, order.unknown.join(', ')),
                primary: true,
                run: function () { return maStlWallAiStockReply(ctx, order, T, Object.assign({}, opts, { ignoreUnknown: true })); },
            }],
        });
    }
    if (ctx && (ctx.kind === 'curved' || ctx.kind === 'curved-new')) return curvedStockReply(ctx, order, T);
    const bad = order.items.filter(function (it) { return MA_STL_WALL_AI_PANEL_WIDTHS_MM.indexOf(it.mm) < 0; });
    if (bad.length) return Promise.resolve({ reply: fmt(T.stockNotStraight, fmtWidth(bad[0].mm, T.lang)), actions: [] });
    const fw = ctx && ctx.formwork;
    if (!fw || typeof fw.previewWall !== 'function' || typeof fw.applyWall !== 'function') {
        return Promise.resolve({ reply: T.matNoAccess, actions: [] });
    }
    const current = maStlWallAiNormalizeStock(typeof fw.wallAttr === 'function' ? fw.wallAttr('_PanelStock') : null);
    const next = maStlWallAiMergeStock(current, order.items);
    const title = fmt(T.stockTitle, wallTitle(ctx, T), stockLabel(next, T));
    return fw.previewWall({ attrs: { _PanelStock: next } }).then(function (after) {
        if (!after || !after.ok) return { reply: fmt(T.matPreviewFail, after && after.message ? after.message : '?'), actions: [] };
        if (after.notes === null) return { reply: title + '\n' + T.stockServerOld, actions: [] };
        return beforeOf(fw, after, current ? null : null).then(function (before) {
            if (!before || !before.ok) return { reply: fmt(T.matPreviewFail, before && before.message ? before.message : '?'), actions: [] };
            return Promise.all([namedRows(fw, before.elements), namedRows(fw, after.elements)]).then(function (pair) {
                const pb = panelsByWidth(pair[0].sceneRows);
                const pa = panelsByWidth(pair[1].sceneRows);
                const table = maStlWallAiBuildDiffTable(pair[0].rows, pair[1].rows, T);
                const lines = [title];
                /* ¿la regla (la del muro + la orden) pide cambiar algo de lo que hay ahora? */
                const touched = next.excludeMm.some(function (w) { return (pb[w] || 0) > 0; })
                    || Object.keys(next.maxMm).some(function (k) { return (pb[k] || 0) > next.maxMm[k]; });
                if (!table.rows.length || !touched) {
                    const w = order.items.map(function (it) { return fmtWidth(it.mm, T.lang); }).join(', ');
                    const why = [title, fmt(touched ? T.stockNothingPossible : T.stockNoChange, w)];
                    order.items.forEach(function (it) {
                        const left = pa[it.mm] || 0;
                        if (touched && it.exclude && left > 0) why.push(fmt(T.stockLeft, left, fmtWidth(it.mm, T.lang)));
                        if (touched && !it.exclude && left > it.max) why.push(fmt(T.stockMaxOver, left, fmtWidth(it.mm, T.lang), it.max));
                    });
                    return { reply: why.join('\n'), actions: [], data: { stock: next, before: before, after: after } };
                }
                const widthsShown = MA_STL_WALL_AI_PANEL_WIDTHS_MM.filter(function (w) { return pb[w] || pa[w]; });
                lines.push(T.stockPanels + ' ' + widthsShown.map(function (w) {
                    return fmtWidth(w, T.lang) + ': ' + (pb[w] || 0) + ' → ' + (pa[w] || 0);
                }).join(' · '));
                lines.push(fmt(T.stockSummary, fmtNum(table.totals.before, 0, T.lang), fmtNum(table.totals.after, 0, T.lang),
                    table.weightBefore > 0 ? fmt(T.stockWeight, fmtNum(table.weightBefore, 1, T.lang), fmtNum(table.weightAfter, 1, T.lang)) : ''));
                if (before.fromScene !== true) lines.push(T.stockBeforePreview);
                const info = (after.notes || [])[0] || {};
                const allowed = MA_STL_WALL_AI_PANEL_WIDTHS_MM.filter(function (w) { return (next.excludeMm || []).indexOf(w) < 0; });
                (info.Notes || []).forEach(function (n) {
                    const m = /^sin-combinacion:(\d+)$/.exec(String(n));
                    if (!m) return;
                    const L = Number(m[1]);
                    const fit = bestFitMm(L, allowed);
                    lines.push(fmt(T.stockImpossible, fmtWidth(L, T.lang), fmtNum(L - fit, 0, T.lang), compensationFor(L - fit, T)));
                });
                order.items.forEach(function (it) {
                    const left = pa[it.mm] || 0;
                    if (it.exclude && left > 0) lines.push(fmt(T.stockLeft, left, fmtWidth(it.mm, T.lang)));
                    if (!it.exclude && it.max != null && left > it.max) lines.push(fmt(T.stockMaxOver, left, fmtWidth(it.mm, T.lang), it.max));
                    if (!it.exclude && it.max != null && left <= it.max) lines.push(fmt(T.stockMaxUsed, left, fmtWidth(it.mm, T.lang), it.max));
                });
                if (pair[1].catalogError) lines.push(fmt(T.matNoCatalog, pair[1].catalogError));
                lines.push(T.stockConfirm);
                const apply = function () {
                    return Promise.resolve(fw.applyWall(after.idWall, after.elements, { _PanelStock: next })).then(function (applied) {
                        return {
                            reply: fmt(T.stockApplied, fmtNum(after.elements.length, 0, T.lang)),
                            actions: [],
                            buttons: [undoButton(fw, after.idWall, applied, '_PanelStock', T)],
                        };
                    });
                };
                const cb = confirmButtons(apply, T);
                return {
                    reply: lines.join('\n'),
                    actions: [],
                    table: table,
                    confirm: 'stock',
                    apply: cb.apply,
                    cancel: cb.cancel,
                    data: { stock: next, before: before, after: after },
                    buttons: cb.buttons,
                };
            });
        });
    });
}

/** Muro curvo: un único ancho (la modulación). Sin ese panel → la modulación válida más cercana. */
function curvedStockReply(ctx, order, T) {
    const b = Number(ctx.modulationMm);
    const info = typeof ctx.modulationInfo === 'function' ? ctx.modulationInfo : null;
    if (!Number.isFinite(b) || !info) return Promise.resolve({ reply: T.matNoAccess, actions: [] });
    const excl = {};
    const max = {};
    order.items.forEach(function (it) { if (it.exclude) excl[it.mm] = true; else if (it.max != null) max[it.mm] = it.max; });
    /* Paneles de cada módulo del curvo: hasta 0,90 uno solo; 1,05 = 0,75 + 0,30 y 1,20 = 0,90 + 0,30 (solver). */
    const parts = function (mm) { return mm === 1050 ? [750, 300] : (mm === 1200 ? [900, 300] : [mm]); };
    const need = function (mm, w) {
        const mi = info(mm);
        if (!mi) return null;
        const levels = Number(ctx.heightM) > 2.7 + 1e-6 ? 2 : 1;
        const per = parts(mm).filter(function (x) { return x === (w == null ? mm : w); }).length;
        return mi.panels * 2 * levels * per;
    };
    const fits = function (mm) {
        return parts(mm).every(function (w) {
            if (excl[w]) return false;
            if (max[w] != null) { const n = need(mm, w); return n != null && n <= max[w]; }
            return true;
        });
    };
    const line = function (mm) {
        const mi = info(mm);
        if (!mi) return fmt(T.curvedLineNone, fmtWidth(mm, T.lang));
        const warn = [];
        if (mi.outerRemateMm > 150 + 1e-6) warn.push(T.curvedWarnOuter);
        if (mi.innerRemateMm < 50 - 1e-6) warn.push(T.curvedWarnInner);
        return fmt(T.curvedLine, fmtWidth(mm, T.lang), mi.panels, fmtNum(mi.turnDeg, 2, T.lang),
            fmtNum(mi.outerRemateMm, 1, T.lang), fmtNum(mi.innerRemateMm, 1, T.lang)) + (warn.length ? ' (' + warn.join('; ') + ')' : '');
    };
    if (fits(b)) {
        const capped = parts(b).filter(function (w) { return max[w] != null; });
        const extra = capped.map(function (w) { return fmt(T.curvedStockEnough, need(b, w), max[w]); }).join(' ');
        return Promise.resolve({ reply: fmt(T.curvedStockNone, fmtWidth(b, T.lang)) + (extra ? ' ' + extra : ''), actions: [] });
    }
    const mods = (Array.isArray(ctx.modulationsMm) ? ctx.modulationsMm : MA_STL_WALL_AI_ANY_WIDTHS_MM).filter(function (m) {
        return m !== b && fits(m) && info(m);
    });
    const good = function (m) { const mi = info(m); return mi.outerRemateMm <= 150 + 1e-6 && mi.innerRemateMm >= 50 - 1e-6; };
    mods.sort(function (x, y) {
        const gx = good(x) ? 0 : 1;
        const gy = good(y) ? 0 : 1;
        if (gx !== gy) return gx - gy;
        const dx = Math.abs(x - b);
        const dy = Math.abs(y - b);
        return dx !== dy ? dx - dy : x - y;
    });
    const head = [fmt(T.curvedStockPropose, fmtWidth(b, T.lang))];
    parts(b).forEach(function (w) {
        if (max[w] != null && need(b, w) > max[w]) head.push(fmt(T.curvedMaxNeed, max[w], fmtWidth(w, T.lang), need(b, w)));
    });
    if (!mods.length) return Promise.resolve({ reply: head.concat([T.curvedNoOption]).join('\n'), actions: [] });
    const pick = mods[0];
    const lines = head.concat([T.curvedNow + ' ' + line(b), T.curvedProposal + ' ' + line(pick)]);
    if (mods.length > 1) lines.push(T.curvedOther + ' ' + line(mods[1]));
    const fw = ctx.kind === 'curved' ? ctx.formwork : null;
    const canPreview = !!(fw && typeof fw.previewWall === 'function' && typeof fw.applyWall === 'function');
    let before0 = null;
    const finish = function (table, after, beforeFromScene) {
        if (table && !beforeFromScene) lines.push(T.stockBeforePreview);
        lines.push(fmt(T.curvedConfirm, fmtWidth(pick, T.lang)));
        const apply = function () {
            const okForm = typeof ctx.setModulation === 'function' ? ctx.setModulation(pick) : false;
            const paint = canPreview && after && after.ok
                ? Promise.resolve(fw.applyWall(after.idWall, after.elements, {}))
                : Promise.resolve(null);
            return paint.then(function (applied) {
                const res = {
                    reply: fmt(T.curvedApplied, fmtWidth(pick, T.lang), applied ? T.curvedRepainted : '') + (okForm ? '' : '\n' + T.curvedFormMissing),
                    actions: [],
                };
                res.buttons = [onceButton(T.stockUndo, function () {
                    if (typeof ctx.setModulation === 'function') ctx.setModulation(b);
                    const back = applied ? fw.applyWall(after.idWall, applied.previous || [], {}) : null;
                    return Promise.resolve(back).then(function () { return { reply: T.stockUndone, actions: [] }; });
                }, T)];
                return res;
            });
        };
        const cb = confirmButtons(apply, T);
        return {
            reply: lines.join('\n'),
            actions: [],
            table: table || undefined,
            confirm: 'stock',
            apply: cb.apply,
            cancel: cb.cancel,
            data: { modulationMm: pick, before: before0, after: after },
            buttons: cb.buttons,
        };
    };
    if (!canPreview) return Promise.resolve(finish(null, null, false));
    return fw.previewWall({ attrs: { _CurveModulationMm: pick } }).then(function (after) {
        if (!after || !after.ok) return finish(null, null, false);
        return beforeOf(fw, after, null).then(function (before) {
            if (!before || !before.ok) return finish(null, after, false);
            before0 = before;
            return Promise.all([namedRows(fw, before.elements), namedRows(fw, after.elements)]).then(function (pair) {
                return finish(maStlWallAiBuildDiffTable(pair[0].rows, pair[1].rows, T), after, before.fromScene === true);
            });
        });
    }).catch(function () { return finish(null, null, false); });
}

/* ===================== Cambio guiado de paneles (14/11/2026) =====================
 * «de 13 paneles de 2,70 x 0,90 solo tengo 10» / «me faltan 3 de 0,90» / «no tengo de 0,90»:
 *  1) cuenta los paneles de ese ancho y alto en el muro y cuántos faltan;
 *  2) pregunta «¿todos de una vez o uno a uno?» (uno a uno: identifica y resalta cada panel en el visor);
 *  3) ofrece solo opciones válidas del manual ATK-60: paneles del catálogo (0,30–0,90 de 0,15 en 0,15) que suman
 *     el ancho del panel, mismo alto, las dos caras a la vez, nunca el ancho que falta, sin pasarse del stock conocido;
 *  4) vista previa en el servidor con `_PanelReplace` (grapas, tirantes, rigidizadores y puntales recalculados),
 *     ANTES / DESPUÉS y Sí/No; aplicar = applyWall (solo ese muro, en sesión) con «Deshacer».
 * No escribe en BD ni cambia `_IsFormwork`. */

const SWAP_HEIGHTS_MM = [2700, 2400, 1200];
const SWAP_HEIGHT_OF_FAMILY = { 27: 2700, 24: 2400, 12: 1200 };
const SWAP_MISSING_WORDS = ['falta', 'faltan', 'faltaria', 'faltarian', 'missing', 'short', 'lack', 'lacking', 'need', 'necesito', 'necesitaria'];
const SWAP_HAVE_WORDS = ['tengo', 'tenemos', 'tiene', 'dispongo', 'disponemos', 'quedan', 'queda', 'have', 'got', 'has',
    'solo', 'solamente', 'unicamente', 'only', 'just'];
const SWAP_TOTAL_WORDS = ['hay', 'son', 'are', 'there', 'total', 'muro', 'wall'];
const SWAP_NEG_WORDS = ['no', 'sin', 'ningun', 'ninguno', 'ninguna', 'none', 'not', 'without', 'agotado', 'agotados', 'dont'];
const SWAP_CANCEL_WORDS = ['cancelar', 'cancela', 'cancelalo', 'dejalo', 'olvidalo', 'cancel', 'stop', 'forget', 'nada'];
const SWAP_ORDINALS = { primera: 1, primero: 1, first: 1, segunda: 2, segundo: 2, second: 2, tercera: 3, tercero: 3, third: 3,
    cuarta: 4, cuarto: 4, fourth: 4, quinta: 5, fifth: 5 };

/** Texto → tokens con números en cifras («dos setenta» → 2 70, «setenta y cinco» → 75, «2,70» → 2.70, «x» → X). */
function swapTokens(text) {
    let s = String(text || '').toLowerCase();
    try { s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (_e) { /* ignore */ }
    s = s.replace(/\batk\s*-?\s*60\b/g, ' ').replace(/\bnevi\b/g, ' ');
    s = s.replace(/(\d)\s*[,.'’]\s*(\d)/g, '$1.$2').replace(/(^|\s)[,.](\d)/g, '$1 0.$2');
    s = s.replace(/(\d)\s*[x×*]\s*(?=[\d.])/g, '$1 X ');
    s = s.replace(/n[ºo°]\s*(?=\d)/g, ' numero ');
    s = s.replace(/[’']/g, '').replace(/[^a-z0-9.+X ]/g, ' ');
    const raw = s.split(/\s+/).filter(Boolean);
    const out = [];
    for (let i = 0; i < raw.length; i++) {
        const t = raw[i];
        const w = /^[a-z]+$/.test(t) && t !== 'oh' && t !== 'a' ? numWord(t) : null;
        if (!w) { out.push(t); continue; }
        let v = w.v;
        if (w.k === 2) {
            const n1 = raw[i + 1] ? numWord(raw[i + 1]) : null;
            if (n1 && n1.k < 2) { v += n1.v; i++; const n2 = raw[i + 1] && n1.k === 1 ? numWord(raw[i + 1]) : null; if (n2 && n2.k === 0 && n2.v < 10) { v += n2.v; i++; } }
        } else if (w.k === 1) {
            const join = raw[i + 1] === 'y' || raw[i + 1] === 'and';
            const n1 = raw[i + (join ? 2 : 1)] ? numWord(raw[i + (join ? 2 : 1)]) : null;
            if (n1 && n1.k === 0 && n1.v > 0 && n1.v < 10) { v += n1.v; i += join ? 2 : 1; }
        }
        out.push(String(v));
    }
    return out.map(function (t) { return t === 'por' || t === 'by' ? 'por' : t; });
}

/** Medidas en los tokens: [{ at, to, mm, kind:'width'|'height' }]. Marca los tokens usados en `used`. */
function swapMeasures(toks, used) {
    const found = [];
    const isInt = function (t) { return /^\d+$/.test(t); };
    const ctxBefore = function (i) { return i > 0 ? toks[i - 1] : ''; };
    const kindOf = function (mm) {
        if (MA_STL_WALL_AI_PANEL_WIDTHS_MM.indexOf(mm) >= 0) return 'width';
        if (SWAP_HEIGHTS_MM.indexOf(mm) >= 0) return 'height';
        return null;
    };
    for (let i = 0; i < toks.length; i++) {
        if (used[i]) continue;
        const t = toks[i];
        let mm = null;
        let to = i;
        if (/^\d*\.\d+$/.test(t)) {
            mm = Math.round(Number(t) * 1000);
        } else if (/^\d$/.test(t) && toks[i + 1] && /^\d{2}$/.test(toks[i + 1]) && kindOf(Number(t) * 1000 + Number(toks[i + 1]) * 10)) {
            mm = Number(t) * 1000 + Number(toks[i + 1]) * 10;
            to = i + 1;
        } else if (isInt(t)) {
            const v = Number(t);
            const unit = toks[i + 1] || '';
            const prev = ctxBefore(i);
            const measureCtx = prev === 'de' || prev === 'of' || prev === 'del' || prev === 'X' || prev === 'por' || prev === '+'
                || toks[i + 1] === 'X' || toks[i + 1] === 'por' || toks[i + 1] === '+' || /^(cm|cms|centimetros|centimeters|mm|milimetros)$/.test(unit);
            if (/^(mm|milimetros)$/.test(unit)) mm = v;
            else if ([30, 45, 60, 75, 90, 120, 240, 270].indexOf(v) >= 0 && measureCtx) mm = v * 10;
            else if ([300, 450, 600, 750, 900, 1200, 2400, 2700].indexOf(v) >= 0) mm = v;
            if (mm != null && /^(cm|cms|centimetros|centimeters|mm|milimetros)$/.test(unit)) to = i + 1;
        }
        const kind = mm != null ? kindOf(mm) : null;
        if (!kind) continue;
        for (let k = i; k <= to; k++) used[k] = true;
        found.push({ at: i, to: to, mm: mm, kind: kind });
        i = to;
    }
    return found;
}

/**
 * Lee la orden de cambio de paneles (español/inglés, tolerante a faltas y a dictado).
 * @returns {{ widthMm:number|null, heightMm:number|null, total:number|null, have:number|null, missing:number|null,
 *             none:boolean, verb:boolean, widthsMm:number[] }}
 */
export function maStlWallAiParseSwapOrder(text) {
    const toks = swapTokens(text);
    const used = toks.map(function () { return false; });
    const ms = swapMeasures(toks, used);
    const out = { widthMm: null, heightMm: null, total: null, have: null, missing: null, none: false, verb: false, widthsMm: [], unknown: [] };
    ms.forEach(function (m) {
        if (m.kind === 'width') { out.widthsMm.push(m.mm); if (out.widthMm == null) out.widthMm = m.mm; }
        else if (out.heightMm == null) out.heightMm = m.mm;
    });
    for (let i = 0; i < toks.length; i++) {
        if (used[i] || !/^\d+$/.test(toks[i])) continue;
        const v = Number(toks[i]);
        const back = [];
        for (let k = i - 1; k >= 0 && k >= i - 3; k--) { if (/^\d/.test(toks[k]) && !used[k]) break; back.push(toks[k]); }
        const next = toks[i + 1] || '';
        const panelNext = stockWordIn(next, STOCK_PANEL_WORDS);
        if (back.some(function (t) { return stockWordIn(t, SWAP_MISSING_WORDS); }) || /^(missing|short|menos)$/.test(next)
            || (stockWordIn(next, ['faltan', 'falta']))) {
            out.missing = v;
        } else if (back.some(function (t) { return stockWordIn(t, SWAP_HAVE_WORDS); })) {
            out.have = v;
        } else if ((panelNext || back.some(function (t) { return SWAP_TOTAL_WORDS.indexOf(t) >= 0; }) || back[0] === 'de' || back[0] === 'of') && out.total == null) {
            out.total = v;
        } else {
            /* número que no es medida ni cantidad («del 19»): no se interpreta, se pregunta */
            out.unknown.push((back[0] && STOCK_GLUE.indexOf(back[0]) >= 0 ? back[0] + ' ' : '') + toks[i]);
        }
    }
    out.verb = toks.some(function (t) { return stockWordIn(t, STOCK_VERBS); });
    out.none = out.have == null && out.missing == null
        && (toks.some(function (t) { return SWAP_NEG_WORDS.indexOf(t) >= 0; }) || /\b(don|doesn|haven|havent)\b/.test(toks.join(' ')));
    if (out.none) out.have = 0;
    return out;
}

/** ¿El mensaje es una orden de cambio de paneles de UN ancho? */
export function maStlWallAiIsSwapOrder(order) {
    if (!order || order.widthMm == null || order.widthsMm.length !== 1) return false;
    return order.have != null || order.missing != null || order.none || order.verb || order.total != null;
}

/**
 * Combinaciones válidas para sustituir un panel de `widthMm` (anchos del catálogo, sin el que falta), de menos
 * piezas a más y, a igualdad, con el panel más ancho primero. Ej.: 900 → [600,300] · [450,450] · [300,300,300].
 * @param {number} widthMm
 * @param {{ excludeMm?: number[], maxPieces?: number }} [opts]
 */
export function maStlWallAiSwapCombos(widthMm, opts) {
    opts = opts || {};
    const ex = (opts.excludeMm || []).concat([widthMm]);
    const widths = MA_STL_WALL_AI_PANEL_WIDTHS_MM.filter(function (w) { return w < widthMm && ex.indexOf(w) < 0; })
        .sort(function (a, b) { return b - a; });
    const maxPieces = opts.maxPieces || 3;
    const out = [];
    const walk = function (left, from, acc) {
        if (left === 0) { if (acc.length >= 2) out.push(acc.slice()); return; }
        if (acc.length >= maxPieces) return;
        for (let i = from; i < widths.length; i++) {
            if (widths[i] > left) continue;
            acc.push(widths[i]);
            walk(left - widths[i], i, acc);
            acc.pop();
        }
    };
    walk(widthMm, 0, []);
    out.sort(function (a, b) {
        if (a.length !== b.length) return a.length - b.length;
        for (let i = 0; i < Math.min(a.length, b.length); i++) if (a[i] !== b[i]) return b[i] - a[i];
        return 0;
    });
    return out;
}

/** Lee la respuesta a una pregunta del cambio guiado. */
export function maStlWallAiParseSwapChoice(text, combos) {
    const toks = swapTokens(text);
    const res = { cancel: false, mode: null, other: false, panel: null, index: null, combo: null };
    if (!toks.length) return res;
    const joined = ' ' + toks.join(' ') + ' ';
    if (toks.length <= 2 && (toks[0] === 'no' || toks[0] === 'nope') && !/\d/.test(toks.join(''))) { res.cancel = true; return res; }
    if (toks.length <= 4 && toks.some(function (t) { return SWAP_CANCEL_WORDS.some(function (w) { return maStlWallAiWordMatch(t, w) >= 0.9; }); })) {
        res.cancel = true;
        return res;
    }
    if (/ (todos|todas|todo|all|everything|together|juntos|juntas) | (de|a) (una|1) vez | at once | a la vez /.test(joined)) res.mode = 'all';
    if (/ (uno|1) (a|por|y) (uno|1) | de (uno|1) en (uno|1) | one (by|at a) (one|time) | individual(es|mente)? | separately | por separado | each /.test(joined)) res.mode = 'one';
    if (/ (otro|otra|other|another|different|distinto|siguiente|next) /.test(joined)) res.other = true;
    const pm = / (?:panel|numero|el|no) (\d+) /.exec(joined);
    if (pm && !/ (la|opcion|option) /.test(joined)) res.panel = Number(pm[1]);
    const used = toks.map(function () { return false; });
    const ms = swapMeasures(toks, used).filter(function (m) { return m.kind === 'width'; });
    if (ms.length) {
        const want = [];
        ms.forEach(function (m) {
            let n = 1;
            for (let k = m.at - 1; k >= Math.max(0, m.at - 3); k--) {
                if (/^\d+$/.test(toks[k]) && !used[k]) { const v = Number(toks[k]); if (v >= 1 && v <= 6) n = v; break; }
                if (['de', 'of', 'X', 'por', 'panel', 'paneles', 'panels', 'placas'].indexOf(toks[k]) < 0) break;
            }
            for (let c = 0; c < n; c++) want.push(m.mm);
        });
        want.sort(function (a, b) { return b - a; });
        const list = Array.isArray(combos) ? combos : [];
        let idx = list.findIndex(function (c) { return c.length === want.length && c.every(function (w, i) { return w === want[i]; }); });
        if (idx < 0 && ms.length === 1) {
            const only = list.map(function (c, i) { return c.indexOf(ms[0].mm) >= 0 ? i : -1; }).filter(function (i) { return i >= 0; });
            if (only.length === 1) idx = only[0];
        }
        if (idx >= 0) { res.index = idx; res.combo = list[idx]; }
        res.panel = null;
        return res;
    }
    if (res.panel != null) return res;
    let n = null;
    const om = / (?:la|opcion|option|numero|number) (\d+) /.exec(joined);
    if (om) n = Number(om[1]);
    if (n == null) toks.forEach(function (t) { if (n == null && SWAP_ORDINALS[t]) n = SWAP_ORDINALS[t]; });
    if (n == null && toks.length === 1 && /^\d+$/.test(toks[0])) n = Number(toks[0]);
    if (n != null && Array.isArray(combos) && n >= 1 && n <= combos.length) { res.index = n - 1; res.combo = combos[n - 1]; }
    return res;
}

/** Datos de un panel de catálogo a partir de una pieza del servidor (null si no es panel 27/24/12). */
function swapPanelOf(e) {
    if (!e || String(e.ElementType || '').toLowerCase() !== 'panel') return null;
    const m = /^(?:PANEL_)?(27|24|12)(30|45|60|75|90)\d{4}$/i.exec(String(e.ElementCode || ''));
    if (!m) return null;
    return { heightMm: SWAP_HEIGHT_OF_FAMILY[m[1]], widthMm: Number(m[2]) * 10 };
}

/**
 * Parejas de paneles (cara A + cara B en la misma posición) de un muro.
 * @returns {Array<{ key, code, widthMm, heightMm, alongMm, upMm, orientation, pieceWidthMm, pieceHeightMm, faces:number,
 *                   pos:number, fromMm:number, toMm:number }>}  pos = nº desde el inicio del muro en su banda.
 */
export function maStlWallAiPanelPairs(elements) {
    const map = {};
    const list = [];
    (elements || []).forEach(function (e) {
        const p = swapPanelOf(e);
        if (!p) return;
        const along = Math.round(Number(e.LocalAlongMm));
        const up = Math.round(Number(e.LocalUpMm) || 0);
        if (!Number.isFinite(along)) return;
        const orientation = String(e.Orientation || 'Vertical');
        const key = [String(e.ElementCode), along, up, orientation.toLowerCase(), Math.round(Number(e.PieceWidthMm)), Math.round(Number(e.PieceHeightMm))].join('|');
        if (!map[key]) {
            map[key] = {
                key: key, code: String(e.ElementCode), widthMm: p.widthMm, heightMm: p.heightMm, alongMm: along, upMm: up,
                orientation: orientation, pieceWidthMm: Math.round(Number(e.PieceWidthMm)), pieceHeightMm: Math.round(Number(e.PieceHeightMm)),
                faces: 0, sides: {},
            };
            list.push(map[key]);
        }
        const side = e.IsMirrored === true ? 'B' : 'A';
        if (!map[key].sides[side]) { map[key].sides[side] = true; map[key].faces++; } else map[key].extra = (map[key].extra || 0) + 1;
    });
    const tumbado = function (p) { return p.orientation.toLowerCase().indexOf('tumbado') >= 0; };
    /* nº de orden: desde el inicio del muro, entre los paneles de pie de la misma banda (los tumbados, por altura) */
    list.forEach(function (p) {
        /* solo las parejas (las dos caras): el panel suelto de una esquina no cuenta */
        const band = list.filter(function (o) { return o.faces === 2 && tumbado(o) === tumbado(p) && (tumbado(p) ? true : o.upMm === p.upMm); });
        band.sort(function (a, b) { return tumbado(p) ? (a.upMm - b.upMm) : (a.alongMm - b.alongMm); });
        const seen = [];
        band.forEach(function (o) { const k = tumbado(p) ? o.upMm : o.alongMm; if (seen.indexOf(k) < 0) seen.push(k); });
        p.pos = p.faces === 2 ? seen.indexOf(tumbado(p) ? p.upMm : p.alongMm) + 1 : 0;
        p.fromMm = p.alongMm;
        p.toMm = p.alongMm + (tumbado(p) ? p.pieceWidthMm : p.pieceWidthMm);
        p.tumbado = tumbado(p);
    });
    return list;
}

/** Regla `_PanelReplace` de una pareja con una combinación. */
function swapRule(pair, combo) {
    return {
        alongMm: pair.alongMm, upMm: pair.upMm, pieceWidthMm: pair.pieceWidthMm, pieceHeightMm: pair.pieceHeightMm,
        orientation: pair.orientation, useMm: combo.slice(),
    };
}

/** Reglas del muro + nuevas (una nueva manda sobre la de la misma posición). */
export function maStlWallAiMergeReplace(current, rules) {
    let cur = current;
    if (typeof cur === 'string') { try { cur = JSON.parse(cur); } catch (_e) { cur = null; } }
    const out = (Array.isArray(cur) ? cur : []).filter(function (r) {
        return r && !(rules || []).some(function (n) {
            return Math.abs(Number(n.alongMm) - Number(r.alongMm)) <= 2 && Math.abs(Number(n.upMm) - Number(r.upMm)) <= 2
                && String(n.orientation).toLowerCase() === String(r.orientation).toLowerCase();
        });
    });
    return out.concat((rules || []).map(function (r) { return JSON.parse(JSON.stringify(r)); }));
}

function swapComboLabel(combo, lang) {
    return combo.map(function (w) { return fmtWidth(w, lang); }).join(' + ');
}

function swapComboTotal(combo, pairs, lang) {
    const per = {};
    combo.forEach(function (w) { per[w] = (per[w] || 0) + 1; });
    return Object.keys(per).map(Number).sort(function (a, b) { return b - a; }).map(function (w) {
        return '+' + (per[w] * 2 * pairs) + (lang === 'en' ? ' of ' : ' de ') + fmtWidth(w, lang);
    }).join(lang === 'en' ? ' and ' : ' y ');
}

function swapSizeLabel(h, w, lang) {
    return fmtWidth(h, lang) + '×' + fmtWidth(w, lang);
}

function swapPanelExtra(p, T) {
    if (p.tumbado) return fmt(T.swapTumbado, fmtDec(p.upMm / 1000, T.lang));
    return p.upMm > 0 ? fmt(T.swapBand, fmtDec(p.upMm / 1000, T.lang)) : '';
}

function swapPairsLabel(n, T) {
    return n + ' ' + (n === 1 ? T.swapPair1 : T.swapPairN);
}

function swapCancelButton(st, T) {
    return { label: T.swapCancel, run: function () { return st.cancel(); } };
}

/**
 * Diálogo guiado de cambio de paneles para el muro del contexto (solo muro recto).
 * @param {object} ctx contexto del muro (ctx.formwork del visor)
 * @param {object} order maStlWallAiParseSwapOrder(...)
 * @param {object} T textos
 */
export function maStlWallAiSwapReply(ctx, order, T) {
    T = Object.assign({}, maStlWallAiTexts(T && T.lang), T || {});
    const lang = T.lang;
    const fw = ctx && ctx.formwork;
    if (!fw || typeof fw.previewWall !== 'function' || typeof fw.applyWall !== 'function') {
        return Promise.resolve({ reply: T.matNoAccess, actions: [] });
    }
    const st = {
        ctx: ctx, fw: fw, T: T, order: order, closed: false,
        cancel: function () {
            st.closed = true;
            if (typeof fw.clearHighlight === 'function') fw.clearHighlight();
            return { reply: T.swapCancelled, actions: [] };
        },
    };
    return fw.previewWall({}).then(function (pre) {
        if (!pre || !pre.ok) return { reply: fmt(T.matPreviewFail, pre && pre.message ? pre.message : '?'), actions: [] };
        st.idWall = pre.idWall;
        st.pre = pre;
        const scene = sceneItemsOf(fw, pre.idWall);
        st.fromScene = scene.length > 0;
        st.before = st.fromScene ? scene : (pre.elements || []);
        return swapStart(st);
    });
}

function swapStart(st) {
    const T = st.T;
    const lang = T.lang;
    const order = st.order;
    const w = order.widthMm;
    const pairsAll = maStlWallAiPanelPairs(st.before);
    if (!pairsAll.length) return { reply: T.swapNoPanelsAtAll, actions: [] };
    const heights = [];
    pairsAll.forEach(function (p) { if (p.widthMm === w && heights.indexOf(p.heightMm) < 0) heights.push(p.heightMm); });
    heights.sort(function (a, b) { return b - a; });
    if (!heights.length || (order.heightMm != null && heights.indexOf(order.heightMm) < 0)) {
        const have = {};
        pairsAll.forEach(function (p) { const k = swapSizeLabel(p.heightMm, p.widthMm, lang); have[k] = (have[k] || 0) + p.faces + (p.extra || 0); });
        const what = order.heightMm != null ? swapSizeLabel(order.heightMm, w, lang) : fmtWidth(w, lang);
        return { reply: fmt(T.swapNoPanels, what, Object.keys(have).map(function (k) { return k + ' × ' + have[k]; }).join(' · ')), actions: [] };
    }
    if (order.heightMm == null && heights.length > 1) {
        const ask = {
            reply: fmt(T.swapAskHeight, fmtWidth(w, lang), heights.map(function (h) { return fmtWidth(h, lang); }).join(' · ')),
            actions: [],
            buttons: heights.map(function (h) {
                return { label: swapSizeLabel(h, w, lang), run: function () { st.order = Object.assign({}, order, { heightMm: h }); return swapStart(st); } };
            }).concat([swapCancelButton(st, T)]),
        };
        ask.swap = {
            handle: function (text) {
                const o = maStlWallAiParseSwapOrder(text);
                const c = maStlWallAiParseSwapChoice(text, []);
                if (c.cancel) return st.cancel();
                if (o.heightMm != null && heights.indexOf(o.heightMm) >= 0) { st.order = Object.assign({}, order, { heightMm: o.heightMm }); return swapStart(st); }
                return null;
            },
        };
        return ask;
    }
    const h = order.heightMm != null ? order.heightMm : heights[0];
    st.heightMm = h;
    const size = swapSizeLabel(h, w, lang);
    st.size = size;
    const mine = pairsAll.filter(function (p) { return p.widthMm === w && p.heightMm === h; });
    const total = mine.reduce(function (a, p) { return a + p.faces + (p.extra || 0); }, 0);
    const paired = mine.filter(function (p) { return p.faces === 2; });
    const singles = total - paired.length * 2;
    st.total = total;
    const title = fmt(T.swapTitle, fmtWidth(h, lang), fmtWidth(w, lang), wallTitle(st.ctx, T));
    st.title = title;
    const lines = [title];
    if (order.total != null && order.total !== total) lines.push(fmt(T.swapSaidTotal, order.total, total));
    lines.push(fmt(T.swapCount, total, fmtWidth(h, lang), fmtWidth(w, lang), paired.length, singles > 0 ? fmt(singles === 1 ? T.swapSingles : T.swapSinglesN, singles) : ''));
    let need = null;
    if (order.none) { need = total; lines.push(fmt(T.swapNeedNone, total)); }
    else if (order.have != null) {
        need = total - order.have;
        if (need <= 0) return { reply: [title, fmt(T.swapEnough, order.have, total)].join('\n'), actions: [] };
        lines.push(fmt(T.swapNeedHave, order.have, need));
    } else if (order.missing != null) {
        need = Math.min(order.missing, total);
        lines.push(fmt(T.swapNeedMissing, order.missing));
    }
    if (need == null || need <= 0) {
        const ask = { reply: fmt(T.swapAskHave, total, fmtWidth(h, lang), fmtWidth(w, lang)), actions: [], buttons: [swapCancelButton(st, T)] };
        ask.swap = {
            handle: function (text) {
                const o = maStlWallAiParseSwapOrder(text);
                const c = maStlWallAiParseSwapChoice(text, []);
                if (c.cancel) return st.cancel();
                const toks = swapTokens(text);
                if (o.have == null && o.missing == null && !o.none && toks.length <= 3) {
                    const num = toks.filter(function (t) { return /^\d+$/.test(t); });
                    if (num.length === 1) o.have = Number(num[0]);
                }
                if (o.have == null && o.missing == null && !o.none) return null;
                st.order = Object.assign({}, order, { have: o.have, missing: o.missing, none: o.none });
                return swapStart(st);
            },
        };
        return ask;
    }
    st.need = need;
    /* Stock conocido: la regla _PanelStock del muro (anchos que no hay, máximos) + lo que dice la orden. */
    const stock = maStlWallAiNormalizeStock(typeof st.fw.wallAttr === 'function' ? st.fw.wallAttr('_PanelStock') : null) || { excludeMm: [], maxMm: {} };
    st.stock = stock;
    st.combosAll = maStlWallAiSwapCombos(w, { excludeMm: stock.excludeMm });
    const autoBtn = {
        label: T.swapAuto,
        run: function () {
            if (typeof st.fw.clearHighlight === 'function') st.fw.clearHighlight();
            /* la fase 3 de siempre (_PanelStock): el servidor rehace los módulos con lo que hay */
            const max = order.none ? 0 : (order.have != null ? order.have : Math.max(0, total - (order.missing || 0)));
            const item = max > 0 ? { mm: w, max: max, exclude: false } : { mm: w, max: null, exclude: true };
            return maStlWallAiStockReply(st.ctx, { items: [item], unknown: [], negative: item.exclude, verb: true, widthsMm: [w] }, T, { ignoreUnknown: true });
        },
    };
    st.autoBtn = autoBtn;
    if (!maStlWallAiSwapCombos(w, {}).length) {
        lines.push(fmt(T.swapNoOption, fmtWidth(w, lang)));
        return { reply: lines.join('\n'), actions: [], buttons: [autoBtn] };
    }
    if (!paired.length) {
        lines.push(T.swapNotReplaceable);
        return { reply: lines.join('\n'), actions: [], buttons: [autoBtn] };
    }
    /* Sonda: ¿qué parejas cambia el servidor? (las de esquina / remate / tape no casan con _PanelReplace) */
    const probeCombo = maStlWallAiSwapCombos(w, {})[0];
    const current = typeof st.fw.wallAttr === 'function' ? st.fw.wallAttr('_PanelReplace') : null;
    const probeRules = maStlWallAiMergeReplace(current, paired.map(function (p) { return swapRule(p, probeCombo); }));
    return st.fw.previewWall({ attrs: { _PanelReplace: probeRules } }).then(function (probe) {
        if (!probe || !probe.ok) return { reply: lines.concat([fmt(T.matPreviewFail, probe && probe.message ? probe.message : '?')]).join('\n'), actions: [] };
        const left = {};
        maStlWallAiPanelPairs(probe.elements).forEach(function (p) { left[p.key] = true; });
        const usable = paired.filter(function (p) { return !left[p.key]; });
        if (!usable.length) {
            const changedAny = (probe.elements || []).length !== (st.pre.elements || []).length;
            lines.push(changedAny ? T.swapNotReplaceable : T.swapServerOld);
            return { reply: lines.join('\n'), actions: [], buttons: changedAny ? [autoBtn] : [] };
        }
        /* orden por defecto: desde el final del muro hacia el inicio */
        usable.sort(function (a, b) { return (b.upMm - a.upMm) || (b.alongMm - a.alongMm); });
        st.usable = usable;
        let pairsNeeded = Math.ceil(need / 2);
        if (usable.length < pairsNeeded) { lines.push(fmt(T.swapFewReplaceable, swapPairsLabel(usable.length, T))); pairsNeeded = usable.length; }
        st.pairsNeeded = pairsNeeded;
        const remain = total - pairsNeeded * 2;
        const uses = order.have != null && !order.none ? fmt(T.swapUses, Math.min(remain, order.have), order.have) : '';
        lines.push(fmt(T.swapPairs, swapPairsLabel(pairsNeeded, T), pairsNeeded * 2, remain, size, uses));
        st.headLines = lines.slice();
        st.picks = [];
        if (pairsNeeded === 1) return swapShowFirst(st).then(function () { return swapStep(st, lines); });
        lines.push(T.swapAskMode);
        const res = {
            reply: lines.join('\n'),
            actions: [],
            buttons: [
                { label: T.swapModeAll, primary: true, run: function () { return swapAll(st); } },
                { label: T.swapModeOne, run: function () { return swapShowFirst(st).then(function () { return swapStep(st, []); }); } },
                swapCancelButton(st, T),
            ],
        };
        res.swap = {
            handle: function (text) {
                const c = maStlWallAiParseSwapChoice(text, []);
                if (c.cancel) return st.cancel();
                if (c.mode === 'all') return swapAll(st);
                if (c.mode === 'one') return swapShowFirst(st).then(function () { return swapStep(st, []); });
                return swapNotUnderstood(st, text, res);
            },
        };
        return res;
    });
}

/** Si el muro no está pintado, dibuja su encofrado temporal (para poder resaltar los paneles). */
function swapShowFirst(st) {
    if (st.fromScene || st.tempShown || typeof st.fw.showTempWall !== 'function') return Promise.resolve();
    st.tempShown = true;
    return Promise.resolve(st.fw.showTempWall(st.pre)).catch(function () { return null; });
}

function swapHighlight(st, pairs) {
    if (typeof st.fw.highlightPanels !== 'function') return null;
    try {
        return st.fw.highlightPanels(st.idWall, pairs.map(function (p) { return { code: p.code, alongMm: p.alongMm, upMm: p.upMm }; }));
    } catch (_e) { return null; }
}

function swapNotUnderstood(st, text, res) {
    /* otra orden distinta: la contesta el intérprete normal */
    const o = maStlWallAiParseSwapOrder(text);
    if (maStlWallAiIsSwapOrder(o)) return null;
    const a = maStlWallAiAnalyze(text, st.T.lang);
    if (a.intent === 'materials' && a.confidence === 'high') return null;
    return Object.assign({}, res, { reply: st.T.swapNotUnderstood });
}

/** Opciones válidas para la pareja (o parejas) que se van a cambiar, con el stock conocido. */
function swapCombosFor(st, nPairs) {
    const lang = st.T.lang;
    const counts = {};
    maStlWallAiPanelPairs(st.before).forEach(function (p) { counts[p.widthMm] = (counts[p.widthMm] || 0) + p.faces + (p.extra || 0); });
    st.picks.forEach(function (pk) { pk.combo.forEach(function (w) { counts[w] = (counts[w] || 0) + 2; }); });
    const dropped = [];
    const ok = st.combosAll.filter(function (c) {
        const add = {};
        c.forEach(function (w) { add[w] = (add[w] || 0) + 2 * nPairs; });
        return Object.keys(add).every(function (k) {
            const max = st.stock.maxMm[k];
            if (max == null || (counts[k] || 0) + add[k] <= max) return true;
            if (dropped.indexOf(k) < 0) dropped.push(k);
            return false;
        });
    });
    return {
        combos: ok,
        note: dropped.length ? fmt(st.T.swapStockLimited, dropped.map(function (k) { return fmt(st.T.stockMaxWidth, st.stock.maxMm[k], fmtWidth(Number(k), lang)); }).join(' · ')) : '',
    };
}

function swapOptionLines(st, combos, nPairs) {
    const lang = st.T.lang;
    return combos.map(function (c, i) {
        return nPairs > 0
            ? fmt(st.T.swapOptionTotal, i + 1, swapComboLabel(c, lang), swapComboTotal(c, nPairs, lang))
            : fmt(st.T.swapOptionShort, i + 1, swapComboLabel(c, lang));
    });
}

/** Todos de una vez: las N parejas por defecto (desde el final), resaltadas, y las opciones. */
function swapAll(st) {
    if (st.closed) return { reply: st.T.stockCancelledAlready, actions: [] };
    const T = st.T;
    const lang = T.lang;
    const chosen = st.usable.slice(0, st.pairsNeeded);
    return swapShowFirst(st).then(function () {
        swapHighlight(st, chosen);
        const lines = [fmt(T.swapChosenAll, swapPairsLabel(chosen.length, T))];
        chosen.slice().sort(function (a, b) { return a.alongMm - b.alongMm; }).forEach(function (p) {
            lines.push(fmt(T.swapPanelLine, p.pos, fmtDec(p.fromMm / 1000, lang), fmtDec(p.toMm / 1000, lang), swapPanelExtra(p, T)));
        });
        const opt = swapCombosFor(st, chosen.length);
        if (!opt.combos.length) {
            lines.push(fmt(T.swapNoValidOption, fmtWidth(st.order.widthMm, lang)));
            if (opt.note) lines.push(opt.note);
            return { reply: lines.join('\n'), actions: [], buttons: [st.autoBtn] };
        }
        lines.push(fmt(T.swapOptions, fmtWidth(st.order.widthMm, lang)));
        swapOptionLines(st, opt.combos, chosen.length).forEach(function (l) { lines.push(l); });
        if (opt.note) lines.push(opt.note);
        lines.push(T.swapAskOption);
        const pickAll = function (combo) {
            st.picks = chosen.map(function (p) { return { pair: p, combo: combo }; });
            return swapReview(st);
        };
        const res = {
            reply: lines.join('\n'),
            actions: [],
            buttons: opt.combos.map(function (c, i) {
                return { label: (i + 1) + ') ' + swapComboLabel(c, lang), primary: i === 0, run: function () { return pickAll(c); } };
            }).concat([swapCancelButton(st, T)]),
        };
        res.swap = {
            handle: function (text) {
                const c = maStlWallAiParseSwapChoice(text, opt.combos);
                if (c.cancel) return st.cancel();
                if (c.combo) return pickAll(c.combo);
                return swapNotUnderstood(st, text, res);
            },
        };
        return res;
    });
}

/** Uno a uno: propone la siguiente pareja (resaltada) y pregunta con qué se cambia. */
function swapStep(st, head, forced) {
    if (st.closed) return { reply: st.T.stockCancelledAlready, actions: [] };
    const T = st.T;
    const lang = T.lang;
    const taken = st.picks.map(function (pk) { return pk.pair.key; });
    const free = st.usable.filter(function (p) { return taken.indexOf(p.key) < 0; });
    const pair = forced || free[0];
    if (!pair) return swapReview(st);
    swapHighlight(st, [pair]);
    const n = st.picks.length + 1;
    const lines = (head || []).slice();
    lines.push(fmt(T.swapStep, n, st.pairsNeeded, pair.pos, fmtDec(pair.fromMm / 1000, lang), fmtDec(pair.toMm / 1000, lang), swapPanelExtra(pair, T)));
    const opt = swapCombosFor(st, 1);
    if (!opt.combos.length) {
        lines.push(fmt(T.swapNoValidOption, fmtWidth(st.order.widthMm, lang)));
        if (opt.note) lines.push(opt.note);
        return { reply: lines.join('\n'), actions: [], buttons: [st.autoBtn] };
    }
    swapOptionLines(st, opt.combos, 0).forEach(function (l) { lines.push(l); });
    if (opt.note) lines.push(opt.note);
    lines.push(T.swapAskOptionOne);
    const choose = function (combo) {
        st.picks.push({ pair: pair, combo: combo });
        return st.picks.length >= st.pairsNeeded ? swapReview(st) : swapStep(st, []);
    };
    const pickPanel = function () { return swapPickPanel(st, free); };
    const res = {
        reply: lines.join('\n'),
        actions: [],
        buttons: opt.combos.map(function (c, i) {
            return { label: (i + 1) + ') ' + swapComboLabel(c, lang), primary: i === 0, run: function () { return choose(c); } };
        }).concat(free.length > 1 ? [{ label: T.swapOtherPanel, run: pickPanel }] : []).concat([swapCancelButton(st, T)]),
    };
    res.swap = {
        handle: function (text) {
            const c = maStlWallAiParseSwapChoice(text, opt.combos);
            if (c.cancel) return st.cancel();
            if (c.combo) return choose(c.combo);
            if (c.panel != null) {
                const want = free.find(function (p) { return p.pos === c.panel; });
                if (want) return swapStep(st, [], want);
            }
            if (c.other && free.length > 1) return pickPanel();
            return swapNotUnderstood(st, text, res);
        },
    };
    return res;
}

function swapPickPanel(st, free) {
    const T = st.T;
    const lang = T.lang;
    const list = free.slice().sort(function (a, b) { return (a.upMm - b.upMm) || (a.alongMm - b.alongMm); });
    swapHighlight(st, list);
    const res = {
        reply: T.swapPickPanel,
        actions: [],
        buttons: list.map(function (p) {
            return {
                label: fmt(T.swapPanelBtn, p.pos, fmtDec(p.fromMm / 1000, lang), fmtDec(p.toMm / 1000, lang)),
                run: function () { return swapStep(st, [], p); },
            };
        }).concat([swapCancelButton(st, T)]),
    };
    res.swap = {
        handle: function (text) {
            const c = maStlWallAiParseSwapChoice(text, []);
            if (c.cancel) return st.cancel();
            const toks = swapTokens(text);
            let n = c.panel;
            if (n == null && toks.length <= 2) { const d = toks.filter(function (t) { return /^\d+$/.test(t); }); if (d.length === 1) n = Number(d[0]); }
            const want = n != null ? list.find(function (p) { return p.pos === n; }) : null;
            if (want) return swapStep(st, [], want);
            return swapNotUnderstood(st, text, res);
        },
    };
    return res;
}

/** Vista previa con las reglas elegidas: ANTES / DESPUÉS, resumen y Sí/No. */
function swapReview(st) {
    const T = st.T;
    const lang = T.lang;
    const fw = st.fw;
    const rules = maStlWallAiMergeReplace(typeof fw.wallAttr === 'function' ? fw.wallAttr('_PanelReplace') : null,
        st.picks.map(function (pk) { return swapRule(pk.pair, pk.combo); }));
    swapHighlight(st, st.picks.map(function (pk) { return pk.pair; }));
    return fw.previewWall({ attrs: { _PanelReplace: rules } }).then(function (after) {
        if (!after || !after.ok) return { reply: fmt(T.matPreviewFail, after && after.message ? after.message : '?'), actions: [] };
        const left = {};
        maStlWallAiPanelPairs(after.elements).forEach(function (p) { left[p.key] = true; });
        const done = st.picks.filter(function (pk) { return !left[pk.pair.key]; });
        if (!done.length) return { reply: [st.title, (after.elements || []).length === st.before.length ? T.swapServerOld : T.swapNothingChanged].join('\n'), actions: [] };
        return Promise.all([namedRows(fw, st.before), namedRows(fw, after.elements)]).then(function (pair) {
            const table = maStlWallAiBuildDiffTable(pair[0].rows, pair[1].rows, T);
            const count = function (els) {
                return maStlWallAiPanelPairs(els).filter(function (p) { return p.widthMm === st.order.widthMm && p.heightMm === st.heightMm; })
                    .reduce(function (a, p) { return a + p.faces + (p.extra || 0); }, 0);
            };
            const nAfter = count(after.elements);
            const lines = [st.title, T.swapReview];
            st.picks.slice().sort(function (a, b) { return a.pair.alongMm - b.pair.alongMm; }).forEach(function (pk) {
                lines.push(fmt(T.swapReviewLine, pk.pair.pos, fmtDec(pk.pair.fromMm / 1000, lang), fmtDec(pk.pair.toMm / 1000, lang),
                    fmtWidth(pk.pair.widthMm, lang), swapComboLabel(pk.combo, lang)));
            });
            const o = st.order;
            const uses = o.have != null && !o.none ? fmt(T.swapUses, Math.min(nAfter, o.have), o.have) : '';
            lines.push(fmt(T.swapReviewCount, st.size, st.total, nAfter, uses));
            if (done.length < st.picks.length) lines.push(fmt(T.swapReviewPartial, st.picks.length - done.length));
            lines.push(fmt(T.stockSummary, fmtNum(table.totals.before, 0, lang), fmtNum(table.totals.after, 0, lang),
                table.weightBefore > 0 ? fmt(T.stockWeight, fmtNum(table.weightBefore, 1, lang), fmtNum(table.weightAfter, 1, lang)) : ''));
            if (!st.fromScene) lines.push(T.stockBeforePreview);
            if (pair[1].catalogError) lines.push(fmt(T.matNoCatalog, pair[1].catalogError));
            lines.push(T.swapConfirm);
            const speech = [fmt(T.swapReviewCount, st.size, st.total, nAfter, uses)];
            if (done.length < st.picks.length) speech.push(fmt(T.swapReviewPartial, st.picks.length - done.length));
            speech.push(T.swapSpeakConfirm);
            const apply = function () {
                return Promise.resolve(fw.applyWall(after.idWall, after.elements, { _PanelReplace: rules })).then(function (applied) {
                    if (typeof fw.clearHighlight === 'function') fw.clearHighlight();
                    return {
                        reply: fmt(T.swapApplied, done.length * 2, fmtNum(after.elements.length, 0, lang)),
                        actions: [],
                        buttons: [undoButton(fw, after.idWall, applied, '_PanelReplace', T)],
                    };
                });
            };
            const cb = confirmButtons(apply, T);
            const cancel = function () {
                const r = cb.cancel();
                if (typeof fw.clearHighlight === 'function' && r && r.reply === T.stockCancelled) fw.clearHighlight();
                return r;
            };
            return {
                reply: lines.join('\n'),
                speech: speech.join('\n'),
                actions: [],
                table: table,
                confirm: 'stock',
                apply: cb.apply,
                cancel: cancel,
                data: { rules: rules, before: st.before, after: after, picks: st.picks },
                buttons: [cb.buttons[0], { label: T.no, run: cancel }],
            };
        });
    });
}

/**
 * Intérprete local (fase 2, tolerante a faltas): analiza con `maStlWallAiAnalyze`; confianza media en
 * «material» → pregunta «¿Querías decir…?» con botones Sí/No (también vale contestar «sí»/«vale»/«no»).
 * @param {object} wallCtx contexto del muro (ver getAiContext de cada popup; `formwork` lo aporta el visor)
 * @param {string} text mensaje del usuario
 * @returns {Promise<{ reply: string, actions: Array<object>, table?: object, buttons?: Array<object> }>}
 */
export function maStlWallAiHandleMessage(wallCtx, text, history) {
    const hist = Array.isArray(history) ? history.slice(0, -1) : [];
    const prev = hist.slice().reverse().find(function (h) { return h && h.role === 'assistant'; }) || null;
    /* Idioma: el del mensaje; si no queda claro, el de la conversación y si no, el de la interfaz. */
    const convLang = hist.slice().reverse().reduce(function (acc, h) { return acc || (h && h.lang) || null; }, null);
    const a = maStlWallAiAnalyze(text, convLang || maStlWallAiUiLang());
    const T = maStlWallAiTexts(a.lang);
    const tag = function (res) { if (res && !res.lang) res.lang = T.lang; return res; };
    const done = function (res) { return Promise.resolve(res).then(tag); };
    /* Respuesta corta («sí», «vale», «yes», «no») a un «¿Querías decir…?» / «Did you mean…?» pendiente. */
    if (prev && prev.confirm) {
        const yn = maStlWallAiYesNo(text, a.lang);
        if (yn === 'yes' && prev.confirm === 'materials') return done(maStlWallAiMaterialsReply(wallCtx, T));
        if (yn === 'yes' && prev.confirm === 'stock' && typeof prev.apply === 'function') return done(prev.apply());
        if (yn === 'no' && prev.confirm === 'stock') {
            return done(typeof prev.cancel === 'function' ? prev.cancel() : { reply: T.stockCancelled, actions: [] });
        }
        if (yn === 'no') return done({ reply: T.helpShort, actions: [] });
    }
    /* Cambio guiado de paneles en curso: la respuesta a la última pregunta (opción, modo, panel, «cancelar»). */
    if (prev && prev.swap && typeof prev.swap.handle === 'function') {
        const r = prev.swap.handle(text, a.lang);
        if (r) return done(r);
    }
    /* Cambio guiado (muro recto, un solo ancho): «de 13 paneles de 2,70 x 0,90 solo tengo 10», «me faltan 3 de 0,90». */
    const swapOrder = maStlWallAiParseSwapOrder(text);
    const straight = !wallCtx || (wallCtx.kind !== 'curved' && wallCtx.kind !== 'curved-new');
    if (straight && maStlWallAiIsSwapOrder(swapOrder) && MA_STL_WALL_AI_PANEL_WIDTHS_MM.indexOf(swapOrder.widthMm) >= 0) {
        if (swapOrder.unknown.length) {
            /* «solo tengo 3 de 0,90 del 19»: lo que no se entiende no se interpreta; se pregunta (como la fase 3) */
            const odd = swapOrder.unknown.map(function (u) { return '«' + u + '»'; }).join(', ');
            const understood = fmtWidth(swapOrder.widthMm, T.lang) + (swapOrder.have != null ? ' · ' + fmt(T.stockMaxWidth, swapOrder.have, fmtWidth(swapOrder.widthMm, T.lang)) : '');
            const go = Object.assign({}, swapOrder, { unknown: [] });
            return done({
                reply: fmt(T.stockAskRef, understood, odd),
                actions: [],
                buttons: [{ label: fmt(T.stockIgnoreRef, swapOrder.unknown.join(', ')), primary: true, run: function () { return maStlWallAiSwapReply(wallCtx, go, T).then(tag); } }],
            });
        }
        return done(maStlWallAiSwapReply(wallCtx, swapOrder, T));
    }
    /* Orden de paneles en obra: «no tengo paneles de 0,90», «solo tengo 3 paneles de 0,90». */
    const order = maStlWallAiParseStockOrder(text, a.lang);
    const stockLike = order.items.length > 0 && (order.negative || order.verb
        || order.items.some(function (it) { return it.max != null; }));
    if (a.intent === 'replace' || stockLike) {
        if (!order.items.length) return done({ reply: T.stockAskWidth, actions: [] });
        return done(maStlWallAiStockReply(wallCtx, order, T, { text: text }));
    }
    if (a.intent === 'materials' && a.confidence === 'medium') {
        return done({
            reply: T.confirmMaterials,
            actions: [],
            confirm: 'materials',
            buttons: [
                { label: T.yes, primary: true, run: function () { return Promise.resolve(maStlWallAiMaterialsReply(wallCtx, T)).then(tag); } },
                { label: T.no, run: function () { return tag({ reply: T.helpShort, actions: [] }); } },
            ],
        });
    }
    if (a.intent === 'materials') return done(maStlWallAiMaterialsReply(wallCtx, T));
    if (a.intent === 'replace') return done({ reply: T.replaceSoon, actions: [] });
    if (a.intent === 'help') return done({ reply: T.helpShort, actions: [] });
    if (a.intent === 'thanks') return done({ reply: T.thanks, actions: [] });
    return done({ reply: fmt(T.help, String(text || '').trim()), actions: [] });
}

if (typeof window !== 'undefined' && typeof window.maStlWallAiHandleMessage !== 'function') {
    window.maStlWallAiHandleMessage = maStlWallAiHandleMessage;
}

/* ---------- Voz (14/11/2026) ---------- */

/** Constructor de reconocimiento de voz del navegador (Chrome/Edge: webkitSpeechRecognition) o null. */
export function maStlWallAiSpeechRecognitionCtor(win) {
    win = win || (typeof window !== 'undefined' ? window : null);
    if (!win) return null;
    return win.SpeechRecognition || win.webkitSpeechRecognition || null;
}

/**
 * Idioma del dictado: el del último mensaje del usuario si se distingue (maStlWallAiDetectLanguage);
 * si no, el de la interfaz. → 'es-ES' | 'en-US'.
 */
export function maStlWallAiVoiceLang(history, uiLang) {
    const ui = uiLang === 'en' ? 'en' : 'es';
    let lang = ui;
    const list = Array.isArray(history) ? history : [];
    for (let i = list.length - 1; i >= 0; i--) {
        const h = list[i];
        if (!h || h.role !== 'user' || !h.text) continue;
        const d = maStlWallAiDetectLanguage(h.text, ui);
        if (d.decided) lang = d.lang;
        break;
    }
    return lang === 'en' ? 'en-US' : 'es-ES';
}

/** Mensaje breve para un error de SpeechRecognition (`event.error`). */
/**
 * Tecla Alt sola (activa/apaga el micrófono): Alt sin Ctrl/Mayús/Meta. AltGr (`key` 'AltGraph' o Ctrl+Alt) no cuenta.
 * El toggle se hace al SOLTAR Alt y solo si no se ha pulsado otra tecla entretanto (no rompe Alt+Tab ni atajos).
 */
export function maStlWallAiIsBareAltKey(ev) {
    if (!ev || ev.key !== 'Alt') return false;
    return !ev.ctrlKey && !ev.metaKey && !ev.shiftKey;
}

/**
 * Qué hacer con un error de SpeechRecognition en modo continuo:
 * 'stop' (permiso, sin micrófono, otros: aviso y se apaga), 'silent' (no-speech, aborted: se reinicia sin aviso),
 * 'retry' (network: reintenta con espera; tras varios fallos seguidos avisa y se apaga).
 */
export function maStlWallAiVoiceErrorPolicy(code) {
    switch (String(code || '')) {
        case 'no-speech':
        case 'aborted':
            return 'silent';
        case 'network':
            return 'retry';
        default:
            return 'stop';
    }
}

/** ¿El foco está en un campo donde se escribe? (pruebas / utilidades; el micrófono usa Alt, que vale también en la entrada). */
export function maStlWallAiIsEditableTarget(el) {
    if (!el || el.nodeType !== 1) return false;
    const tag = String(el.tagName || '').toLowerCase();
    if (tag === 'textarea' || tag === 'select') return true;
    if (tag === 'input') {
        const type = String(el.type || 'text').toLowerCase();
        return ['button', 'submit', 'reset', 'checkbox', 'radio', 'range', 'color', 'file', 'image'].indexOf(type) < 0;
    }
    return !!el.isContentEditable;
}

export function maStlWallAiVoiceErrorText(code, T) {
    T = T || MA_STL_WALL_AI_TEXT;
    switch (String(code || '')) {
        case 'not-allowed':
        case 'service-not-allowed':
            return T.micErrNotAllowed;
        case 'no-speech':
            return T.micErrNoSpeech;
        case 'network':
            return T.micErrNetwork;
        case 'audio-capture':
            return T.micErrAudio;
        case 'aborted':
            return '';
        default:
            return T.micErrOther.replace('{0}', String(code || '?'));
    }
}

/* ---------- Respuestas habladas (14/11/2026): speechSynthesis, voces del sistema ---------- */

/** Clave de localStorage del icono del altavoz ('1' encendido · '0' apagado; sin valor = encendido). */
export const MA_STL_WALL_AI_SPEAK_KEY = 'maStlWallAi.speak';

/** ¿Respuestas habladas encendidas? Por defecto sí (lo pidió el usuario); se recuerda en localStorage. */
export function maStlWallAiSpeakPref(storage) {
    try {
        const st = storage || (typeof localStorage !== 'undefined' ? localStorage : null);
        return !st || st.getItem(MA_STL_WALL_AI_SPEAK_KEY) !== '0';
    } catch (_e) {
        return true;
    }
}

export function maStlWallAiSetSpeakPref(on, storage) {
    try {
        const st = storage || (typeof localStorage !== 'undefined' ? localStorage : null);
        if (st) st.setItem(MA_STL_WALL_AI_SPEAK_KEY, on ? '1' : '0');
    } catch (_e) { /* modo privado: solo esta sesión */ }
}

/* Voces preferidas (Windows/Edge/Chrome); el resto puntúa por idioma exacto y calidad. */
const MA_STL_WALL_AI_VOICE_NAMES = {
    es: /\b(Helena|Laura|Pablo|Elvira|[AÁ]lvaro|Abril|Arnau|Dar[ií]o|Elias|Estrella|Irene|Lia|Nil|Saul|Teo|Triana|Vera|M[oó]nica|Jorge|Paulina)\b|Google espa[nñ]ol/i,
    en: /\b(Aria|Jenny|Guy|Zira|David|Mark|Ava|Andrew|Emma|Brian|Samantha|Alex)\b|Google US English/i,
};

/**
 * Mejor voz del sistema para `tag` ('es-ES' | 'en-US'): mismo idioma obligatorio; prefiere la región exacta
 * (es-ES antes que es-MX/es-US); dentro de la región, voces locales del sistema antes que remotas; luego Edge natural,
 * Google, Microsoft y nombres conocidos.
 * @returns {SpeechSynthesisVoice|null}
 */
export function maStlWallAiPickVoice(voices, tag) {
    const want = String(tag || 'es-ES').replace('_', '-').toLowerCase();
    const base = want.split('-')[0];
    const names = MA_STL_WALL_AI_VOICE_NAMES[base] || null;
    let best = null;
    let bestScore = -1;
    (voices || []).forEach(function (v) {
        if (!v) return;
        const l = String(v.lang || '').replace('_', '-').toLowerCase();
        if (l.split('-')[0] !== base) return;
        const name = String(v.name || '');
        let score = l === want ? 100 : 50;
        /* Eficiencia (respuesta del usuario 14/11/2026): primero las voces LOCALES del sistema (localService:
           Microsoft Helena/Laura/Pablo, David/Zira…: sin red y sin latencia); sin local del idioma, la remota
           (Edge natural > Google). */
        if (v.localService) score += 30;
        if (/natural|neural|online/i.test(name)) score += 8;
        if (/google/i.test(name)) score += 6;
        if (/microsoft/i.test(name)) score += 4;
        if (names && names.test(name)) score += 3;
        if (v.default) score += 1;
        if (score > bestScore) { best = v; bestScore = score; }
    });
    return best;
}

/**
 * Número tal como se dice (con cifras, para que la voz diga «90» natural): sin separador de miles; si la parte
 * decimal es cero, solo la entera («90.00» → «90», «1.667,0» → «1667»); si no, «X punto Y» / «X point Y»
 * («90,45» → «90 punto 45», «3,60» → «3 punto 60»). `raw` en el formato del idioma (es 1.234,5 · en 1,234.5).
 */
export function maStlWallAiSayNumber(raw, lang) {
    const en = lang === 'en';
    let t = String(raw == null ? '' : raw).trim();
    const grouped = en ? /^\d{1,3}(?:,\d{3})+(?:\.\d+)?$/ : /^\d{1,3}(?:\.\d{3})+(?:,\d+)?$/;
    if (grouped.test(t)) t = en ? t.replace(/,/g, '') : t.replace(/\./g, '').replace(',', '.');
    else t = t.replace(',', '.');
    const m = /^(\d+)(?:\.(\d+))?$/.exec(t);
    if (!m) return String(raw);
    const int = m[1].replace(/^0+(?=\d)/, '');
    if (!m[2] || /^0+$/.test(m[2])) return int;
    return int + (en ? ' point ' : ' punto ') + m[2];
}

/**
 * Texto preparado para leerlo en voz alta (es/en): sin emojis ni símbolos y unidades con su nombre.
 * Números (respuestas del usuario 14/11/2026): medidas de panel/pieza (m con 2 decimales, sin unidad) en
 * centímetros: «0,90» → «90», «2,70×0,90» → «270 por 90», «0,60 + 0,30» → «60 más 30»; cualquier otro número
 * con decimales no nulos «X punto Y» («90,45» → «90 punto 45», «3,60 m» → «3 punto 60 metros») y con decimales
 * cero solo la parte entera («90.00» → «90»). Siempre con cifras.
 */
export function maStlWallAiSpeakable(text, lang) {
    const en = lang === 'en';
    const L = en ? 'en' : 'es';
    const NUM = en ? '\\d{1,3}(?:,\\d{3})+(?:\\.\\d+)?|\\d+(?:[.,]\\d+)?' : '\\d{1,3}(?:\\.\\d{3})+(?:,\\d+)?|\\d+(?:[.,]\\d+)?';
    const say = function (n) { return maStlWallAiSayNumber(n, L); };
    let s = ' ' + String(text || '').replace(/\r?\n+/g, ' ') + ' ';
    s = s.replace(/[\u{1F000}-\u{1FAFF}\u{2600}-\u{27BF}\u{2B00}-\u{2BFF}\u{1F1E6}-\u{1F1FF}\uFE0F\u200D\u2713\u2714\u2717\u2718\u2605\u2606]/gu, ' ');
    s = s.replace(/[«»"“”„`*_#]/g, '');
    s = s.replace(/…/g, '.');
    s = s.replace(/\bp\.\s?ej\./gi, en ? 'for example' : 'por ejemplo');
    s = s.replace(/\bej\.:?/gi, en ? 'for example' : 'por ejemplo');
    s = s.replace(/\bn[º°]\s*/gi, en ? 'number ' : 'número ');
    s = s.replace(/\bno\.\s*(?=\d)/g, 'number ');
    s = s.replace(/Ø\s*/g, en ? 'diameter ' : 'diámetro ');
    /* con unidad: «X punto Y metros» */
    s = s.replace(new RegExp('(' + NUM + ')\\s*(–|-|\\ba\\b|\\bto\\b)\\s*(' + NUM + ')\\s*m\\b(?![²³])', 'g'), function (_m, a, sep, b) {
        return say(a) + (en ? ' to ' : ' a ') + say(b) + (en ? ' metres' : ' metros');
    });
    const unit = function (re, es, eng) {
        s = s.replace(new RegExp('(' + NUM + ')\\s*' + re, 'g'), function (_m, a) { return say(a) + ' ' + (en ? eng : es); });
    };
    unit('m\\b(?![²³])', 'metros', 'metres');
    unit('mm\\b', 'milímetros', 'millimetres');
    unit('cm\\b', 'centímetros', 'centimetres');
    unit('kg\\b', 'kilos', 'kilograms');
    unit('°', 'grados', 'degrees');
    /* medidas de panel/pieza (m con 2 decimales, sin unidad) → centímetros: 0,90 → 90 · 2,70 → 270 */
    s = s.replace(/(^|[^\d.,])(\d)[.,](\d\d)(?![\d.,]\d|\d)/g, function (_m, pre, i, d) {
        return pre + String(Number(i) * 100 + Number(d));
    });
    /* el resto de números con decimales: «X punto Y» (o solo X si los decimales son cero) */
    s = s.replace(new RegExp('(^|[^\\d])(' + NUM + ')(?!\\d)', 'g'), function (_m, pre, n) {
        return pre + (/[.,]/.test(n) ? say(n) : n);
    });
    s = s.replace(/\s*×\s*/g, en ? ' by ' : ' por ');
    s = s.replace(/\s*\+\s*(?=\d)/g, en ? ' plus ' : ' más ');
    s = s.replace(/(^|\s)\+(?=\d)/g, '$1');
    s = s.replace(/\s*→\s*/g, en ? ' to ' : ' pasan a ');
    s = s.replace(/\s*=\s*/g, en ? ', that is, ' : ', es decir, ');
    s = s.replace(/(\d)\s*[–-]\s*(\d)/g, en ? '$1 to $2' : '$1 a $2');
    s = s.replace(/\s[–—-]\s/g, ', ');
    s = s.replace(/\bBD\b/g, en ? 'database' : 'base de datos');
    s = s.replace(/\(s\)/g, '');
    s = s.replace(/[·•|]/g, ', ');
    s = s.replace(/[()[\]{}]/g, ', ');
    s = s.replace(/[›>]/g, ', ');
    s = s.replace(/[\u2190-\u21FF\u2200-\u22FF\u25A0-\u25FF]/g, ' ');
    s = s.replace(/\s+([,.;:?!])/g, '$1');
    s = s.replace(/,(\s*,)+/g, ',');
    s = s.replace(/[,;:]\s*([.?!])/g, '$1');
    s = s.replace(/([.?!])\s*,/g, '$1');
    s = s.replace(/([¿¡])\s*,\s*/g, '$1');
    s = s.replace(/\s{2,}/g, ' ');
    s = s.replace(/^[\s,.;:]+/, '').replace(/[\s,;:]+$/, '');
    return s.trim();
}

/**
 * Qué se lee de una respuesta del asistente ({ reply, speech?, table?, speak? }):
 * `speech` si la respuesta lo trae (recuento, resumen del cambio); si no, el texto sin listas
 * («·», «•»: se resumen con «te dejo el detalle en pantalla») y las opciones en corto («Opción 1: 60 más 30»,
 * máximo 4). Con tabla y sin `speech`: la primera línea y las preguntas. Máximo ~600 caracteres.
 */
export function maStlWallAiSpeechText(res, lang) {
    if (!res || res.speak === false || res.pending) return '';
    const L = lang === 'en' ? 'en' : 'es';
    const T = maStlWallAiTexts(L);
    const explicit = typeof res.speech === 'string';
    const src = explicit ? res.speech : String(res.reply == null ? '' : res.reply);
    const hasTable = !!(res.table && Array.isArray(res.table.rows) && res.table.rows.length);
    const lines = src.split(/\n+/).map(function (l) { return l.trim(); }).filter(Boolean);
    let out = [];
    let hidden = 0;
    let options = 0;
    lines.forEach(function (l) {
        /* Medidas del título del muro («(L 6,70 · e 0,30 · h 2,70)»): están en pantalla, no se leen. */
        l = l.replace(/\s*\((?:L|Ø) [^)]*\)/g, '');
        const opt = /^(\d+)\)\s*(.+)$/.exec(l);
        if (opt) {
            options++;
            if (options <= 4) out.push(fmt(T.speakOption, opt[1], opt[2].split('→')[0].trim()));
            else hidden++;
            return;
        }
        if (/^[·•*–-]\s*/.test(l)) { hidden++; return; }
        out.push(l);
    });
    if (hasTable && !explicit) {
        const firstLine = out.slice(0, 1);
        const asks = out.slice(1).filter(function (l) { return /\?/.test(l); });
        out = firstLine.concat(asks);
        hidden++;
    }
    const sent = out.map(function (l) {
        l = l.replace(/:\s*$/, '.');
        return /[.!?…;]$/.test(l) ? l : l + '.';
    });
    /* El aviso de pantalla va antes de la pregunta final, si la hay. */
    if (hidden && !explicit) {
        if (sent.length > 1 && /\?/.test(sent[sent.length - 1])) sent.splice(sent.length - 1, 0, T.speakScreen);
        else sent.push(T.speakScreen);
    }
    let text = sent.join(' ');
    const MAX = 600;
    if (text.length > MAX) {
        const cut = text.slice(0, MAX);
        const at = Math.max(cut.lastIndexOf('. '), cut.lastIndexOf('? '), cut.lastIndexOf('! '));
        text = (at > 80 ? cut.slice(0, at + 1) : cut.replace(/\s+\S*$/, '') + '.') + ' ' + T.speakScreen;
    }
    return maStlWallAiSpeakable(text, L);
}

/**
 * Trozos para speechSynthesis (Chrome corta las frases de más de ~15 s): por frases, juntando las cortas
 * hasta `max` caracteres; una frase larga se parte por comas y, si hace falta, por palabras.
 */
export function maStlWallAiSpeechChunks(text, max) {
    max = max || 180;
    const t = String(text || '').replace(/\s+/g, ' ').trim();
    if (!t) return [];
    const pieces = [];
    t.split(/(?<=[.!?;:])\s+/).forEach(function (sent) {
        if (sent.length <= max) { pieces.push(sent); return; }
        let cur = '';
        sent.split(/(?<=,)\s+/).forEach(function (part) {
            const words = part.length <= max ? [part] : part.split(' ');
            words.forEach(function (w) {
                if (cur && cur.length + 1 + w.length > max) { pieces.push(cur); cur = w; } else cur = cur ? cur + ' ' + w : w;
            });
        });
        if (cur) pieces.push(cur);
    });
    const out = [];
    pieces.forEach(function (p) {
        const last = out.length - 1;
        if (last >= 0 && out[last].length + 1 + p.length <= max) out[last] += ' ' + p; else out.push(p);
    });
    return out;
}

const MA_STL_WALL_AI_ICON = {
    /* Remix Icon (Apache 2.0): ri-mic-line y ri-mic-fill. */
    mic: 'M12 3C10.3431 3 9 4.34315 9 6V10C9 11.6569 10.3431 13 12 13C13.6569 13 15 11.6569 15 10V6C15 4.34315 13.6569 3 12 3ZM12 1C14.7614 1 17 3.23858 17 6V10C17 12.7614 14.7614 15 12 15C9.23858 15 7 12.7614 7 10V6C7 3.23858 9.23858 1 12 1ZM3.05493 11H5.07082C5.55608 14.3923 8.47353 17 12 17C15.5265 17 18.4439 14.3923 18.9292 11H20.9451C20.4839 15.1716 17.1716 18.4839 13 18.9451V23H11V18.9451C6.82838 18.4839 3.51608 15.1716 3.05493 11Z',
    micFill: 'M11.9998 1C14.7612 1 16.9998 3.23858 16.9998 6V10C16.9998 12.7614 14.7612 15 11.9998 15C9.23833 15 6.99976 12.7614 6.99976 10V6C6.99976 3.23858 9.23833 1 11.9998 1ZM3.05469 11H5.07065C5.55588 14.3923 8.47329 17 11.9998 17C15.5262 17 18.4436 14.3923 18.9289 11H20.9448C20.4837 15.1716 17.1714 18.4839 12.9998 18.9451V23H10.9998V18.9451C6.82814 18.4839 3.51584 15.1716 3.05469 11Z',
    /* Remix Icon (Apache 2.0): ri-volume-up-line y ri-volume-mute-line. */
    spk: 'M6.60282 10.0001L10 7.22056V16.7796L6.60282 14.0001H3V10.0001H6.60282ZM2 16.0001H5.88889L11.1834 20.3319C11.2727 20.405 11.3846 20.4449 11.5 20.4449C11.7761 20.4449 12 20.2211 12 19.9449V4.05519C12 3.93977 11.9601 3.8279 11.887 3.73857C11.7121 3.52485 11.3971 3.49335 11.1834 3.66821L5.88889 8.00007H2C1.44772 8.00007 1 8.44778 1 9.00007V15.0001C1 15.5524 1.44772 16.0001 2 16.0001ZM23 12C23 15.292 21.5539 18.2463 19.2622 20.2622L17.8445 18.8444C19.7758 17.1937 21 14.7398 21 12C21 9.26016 19.7758 6.80629 17.8445 5.15557L19.2622 3.73779C21.5539 5.75368 23 8.70795 23 12ZM18 12C18 10.0883 17.106 8.38548 15.7133 7.28673L14.2842 8.71584C15.3213 9.43855 16 10.64 16 12C16 13.36 15.3213 14.5614 14.2842 15.2841L15.7133 16.7132C17.106 15.6145 18 13.9116 18 12Z',
    spkMute: 'M10 7.22056L6.60282 10.0001H3V14.0001H6.60282L10 16.7796V7.22056ZM5.88889 16.0001H2C1.44772 16.0001 1 15.5524 1 15.0001V9.00007C1 8.44778 1.44772 8.00007 2 8.00007H5.88889L11.1834 3.66821C11.3971 3.49335 11.7121 3.52485 11.887 3.73857C11.9601 3.8279 12 3.93977 12 4.05519V19.9449C12 20.2211 11.7761 20.4449 11.5 20.4449C11.3846 20.4449 11.2727 20.405 11.1834 20.3319L5.88889 16.0001ZM20.4142 12.0001L23.9497 15.5356L22.5355 16.9498L19 13.4143L15.4645 16.9498L14.0503 15.5356L17.5858 12.0001L14.0503 8.46454L15.4645 7.05032L19 10.5859L22.5355 7.05032L23.9497 8.46454L20.4142 12.0001Z',
};

function voiceIcon(name) {
    return '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true" focusable="false"><path d="' + MA_STL_WALL_AI_ICON[name] + '"/></svg>';
}

function ensureVoiceStyle() {
    if (typeof document === 'undefined' || document.getElementById('ma-stl-wall-ai-voice-style')) return;
    const st = document.createElement('style');
    st.id = 'ma-stl-wall-ai-voice-style';
    st.textContent = '@keyframes maStlWallAiPulse{0%{box-shadow:0 0 0 0 rgba(77,77,77,.55)}70%{box-shadow:0 0 0 9px rgba(77,77,77,0)}100%{box-shadow:0 0 0 0 rgba(77,77,77,0)}}'
        + '.ma-stl-wall-ai__mic--on{animation:maStlWallAiPulse 1.2s ease-out infinite}'
        + '.ma-stl-wall-ai__spk--talking{animation:maStlWallAiPulse 1.2s ease-out infinite}'
        + '@keyframes maStlWallAiBars{0%,100%{height:4px}50%{height:18px}}'
        + '.ma-stl-wall-ai__listening--anim .ma-stl-wall-ai__bar{animation:maStlWallAiBars .9s ease-in-out infinite}'
        + '.ma-stl-wall-ai__listening--anim .ma-stl-wall-ai__bar:nth-child(2n){animation-delay:.15s}'
        + '.ma-stl-wall-ai__listening--anim .ma-stl-wall-ai__bar:nth-child(3n){animation-delay:.3s}'
        + '.ma-stl-wall-ai__listening--anim .ma-stl-wall-ai__bar:nth-child(4n){animation-delay:.45s}'
        + '.ma-stl-wall-ai__listening-dot{animation:maStlWallAiPulse 1.2s ease-out infinite}';
    document.head.appendChild(st);
}

/**
 * Monta el chat dentro de `container`.
 * @param {HTMLElement} container
 * @param {{ getContext?: () => object, texts?: object, height?: string,
 *           onActions?: (actions: Array<object>, ctx: object) => void }} [options]
 */
export function mountWallAiChat(container, options) {
    options = options || {};
    const T = Object.assign({}, maStlWallAiUiTexts(), options.texts || {});
    let history = [];
    let busy = false;
    let ctxKey = null;

    const root = document.createElement('div');
    root.className = 'ma-stl-wall-ai';
    root.style.display = 'flex';
    root.style.flexDirection = 'column';
    root.style.gap = '8px';
    root.style.fontFamily = 'inherit';
    root.style.userSelect = 'text';

    const log = document.createElement('div');
    log.className = 'ma-stl-wall-ai__log';
    log.setAttribute('role', 'log');
    log.setAttribute('aria-live', 'polite');
    log.style.height = options.height || '300px';
    log.style.overflowY = 'auto';
    log.style.border = '1px solid #e2e8f0';
    log.style.borderRadius = '8px';
    log.style.background = '#f8fafc';
    log.style.padding = '10px';
    log.style.display = 'flex';
    log.style.flexDirection = 'column';
    log.style.gap = '8px';
    log.style.position = 'relative';
    root.appendChild(log);

    const inputRow = document.createElement('div');
    inputRow.style.display = 'flex';
    inputRow.style.gap = '8px';
    inputRow.style.alignItems = 'flex-end';

    const input = document.createElement('textarea');
    input.className = 'form-control ma-stl-wall-ai__input';
    input.rows = 2;
    input.placeholder = T.placeholder;
    input.setAttribute('aria-label', T.placeholder);
    input.style.flex = '1';
    input.style.minHeight = '44px';
    input.style.maxHeight = '140px';
    input.style.resize = 'none';
    input.style.boxSizing = 'border-box';
    input.style.padding = '8px 10px';
    input.style.border = '1px solid #cbd5e1';
    input.style.borderRadius = '8px';
    input.style.fontSize = '13px';
    input.style.lineHeight = '1.35';
    input.style.fontFamily = 'inherit';
    inputRow.appendChild(input);

    const sendBtn = document.createElement('button');
    sendBtn.type = 'button';
    sendBtn.className = 'btn btn-primary ma-stl-wall-ai__send';
    sendBtn.textContent = T.send;
    /* Botón de color: texto blanco (regla de Viewer.cshtml). */
    sendBtn.style.background = '#2563eb';
    sendBtn.style.borderColor = '#2563eb';
    sendBtn.style.color = '#ffffff';
    sendBtn.style.border = 'none';
    sendBtn.style.borderRadius = '8px';
    sendBtn.style.padding = '9px 14px';
    sendBtn.style.fontSize = '13px';
    sendBtn.style.cursor = 'pointer';
    sendBtn.style.whiteSpace = 'nowrap';

    /* Micrófono (14/11/2026): solo al pulsarlo; otra pulsación lo cancela. Sin tecla rápida ni palabra de activación. */
    ensureVoiceStyle();
    const SpeechCtor = maStlWallAiSpeechRecognitionCtor();
    const voiceSecure = typeof window === 'undefined' || window.isSecureContext !== false;
    const micWrap = document.createElement('span');
    micWrap.style.display = 'inline-flex';
    micWrap.style.flex = '0 0 auto';
    const micBtn = document.createElement('button');
    micBtn.type = 'button';
    micBtn.className = 'btn btn-sm ma-stl-wall-ai__mic';
    micBtn.setAttribute('data-ma-stl-wall-ai-mic', '1');
    micBtn.style.display = 'inline-flex';
    micBtn.style.alignItems = 'center';
    micBtn.style.justifyContent = 'center';
    micBtn.style.width = '38px';
    micBtn.style.height = '38px';
    micBtn.style.padding = '0';
    micBtn.style.borderRadius = '8px';
    micBtn.style.cursor = 'pointer';
    micBtn.style.transition = 'none';
    micWrap.appendChild(micBtn);
    inputRow.appendChild(micWrap);
    /* Altavoz (14/11/2026): respuestas habladas con speechSynthesis (voces del sistema, local y gratis).
       Toggle propio, por defecto encendido y recordado en localStorage (maStlWallAiSpeakPref). */
    const synth = typeof window !== 'undefined' && window.speechSynthesis && typeof window.SpeechSynthesisUtterance === 'function'
        ? window.speechSynthesis : null;
    const spkWrap = document.createElement('span');
    spkWrap.style.display = 'inline-flex';
    spkWrap.style.flex = '0 0 auto';
    const spkBtn = document.createElement('button');
    spkBtn.type = 'button';
    spkBtn.className = 'btn btn-sm ma-stl-wall-ai__spk';
    spkBtn.setAttribute('data-ma-stl-wall-ai-spk', '1');
    spkBtn.style.display = 'inline-flex';
    spkBtn.style.alignItems = 'center';
    spkBtn.style.justifyContent = 'center';
    spkBtn.style.width = '38px';
    spkBtn.style.height = '38px';
    spkBtn.style.padding = '0';
    spkBtn.style.borderRadius = '8px';
    spkBtn.style.cursor = 'pointer';
    spkBtn.style.transition = 'none';
    spkWrap.appendChild(spkBtn);
    inputRow.appendChild(spkWrap);
    inputRow.appendChild(sendBtn);
    let rec = null;
    let voiceBase = '';

    function paintMic(on) {
        micBtn.innerHTML = voiceIcon(on ? 'micFill' : 'mic');
        micBtn.classList.toggle('ma-stl-wall-ai__mic--on', !!on);
        micBtn.setAttribute('aria-pressed', on ? 'true' : 'false');
        /* Escuchando: botón gris 70% (#4d4d4d) (icono blanco) y pulsante. */
        micBtn.style.background = on ? '#4d4d4d' : '#ffffff';
        micBtn.style.color = on ? '#ffffff' : '#2563eb';
        micBtn.style.border = '1px solid ' + (on ? '#4d4d4d' : '#cbd5e1');
        const tip = on ? T.micStop : T.mic;
        micBtn.title = tip;
        micBtn.setAttribute('aria-label', tip);
        micWrap.title = '';
    }
    paintMic(false);
    if (!SpeechCtor || !voiceSecure) {
        const why = !SpeechCtor ? T.micUnsupported : T.micInsecure;
        micBtn.disabled = true;
        micBtn.style.opacity = '0.45';
        micBtn.style.cursor = 'not-allowed';
        micBtn.title = why;
        micBtn.setAttribute('aria-label', why);
        /* El tooltip va también en el envoltorio: un botón deshabilitado no siempre lo muestra. */
        micWrap.title = why;
    }

    /* Indicador «Escuchando…» sobre la entrada: barras que siguen el volumen real del micro
       (getUserMedia + AnalyserNode); si no hay acceso, animación fija. */
    const meter = document.createElement('div');
    meter.className = 'ma-stl-wall-ai__listening';
    meter.setAttribute('role', 'status');
    meter.style.display = 'none';
    meter.style.alignItems = 'center';
    meter.style.gap = '8px';
    meter.style.padding = '5px 10px';
    meter.style.border = '1px solid #c4c4c4';
    meter.style.borderRadius = '8px';
    meter.style.background = '#f2f2f2';
    meter.style.color = '#4d4d4d';
    meter.style.fontSize = '12px';
    meter.style.fontWeight = '600';
    const meterDot = document.createElement('span');
    meterDot.className = 'ma-stl-wall-ai__listening-dot';
    meterDot.style.width = '9px';
    meterDot.style.height = '9px';
    meterDot.style.borderRadius = '50%';
    meterDot.style.background = '#4d4d4d';
    meterDot.style.flex = '0 0 auto';
    const meterLabel = document.createElement('span');
    meterLabel.style.flex = '1 1 auto';
    const meterBars = document.createElement('span');
    meterBars.style.display = 'inline-flex';
    meterBars.style.alignItems = 'center';
    meterBars.style.gap = '3px';
    meterBars.style.height = '20px';
    const bars = [];
    for (let i = 0; i < 9; i++) {
        const bar = document.createElement('span');
        bar.className = 'ma-stl-wall-ai__bar';
        bar.style.display = 'inline-block';
        bar.style.width = '3px';
        bar.style.height = '4px';
        bar.style.borderRadius = '2px';
        bar.style.background = '#4d4d4d';
        bar.style.transition = 'height 70ms linear';
        meterBars.appendChild(bar);
        bars.push(bar);
    }
    meter.appendChild(meterDot);
    meter.appendChild(meterLabel);
    meter.appendChild(meterBars);
    root.appendChild(meter);
    let meterRun = null;
    let meterToken = 0;

    function stopMeter() {
        meterToken++;
        meter.style.display = 'none';
        meter.classList.remove('ma-stl-wall-ai__listening--anim');
        const m = meterRun;
        meterRun = null;
        for (let i = 0; i < bars.length; i++) bars[i].style.height = '4px';
        if (!m) return;
        if (m.raf) window.cancelAnimationFrame(m.raf);
        /* Libera el micrófono (se apaga el indicador del navegador) y el contexto de audio. */
        if (m.stream) m.stream.getTracks().forEach(function (t) { try { t.stop(); } catch (_e) { /* parado */ } });
        if (m.ctx && typeof m.ctx.close === 'function') { try { m.ctx.close(); } catch (_e) { /* cerrado */ } }
    }

    function startMeter(VT) {
        stopMeter();
        const token = meterToken;
        meterLabel.textContent = VT.micListening;
        meter.style.display = 'flex';
        meter.classList.add('ma-stl-wall-ai__listening--anim');
        meterRun = { live: false };
        const md = typeof navigator !== 'undefined' ? navigator.mediaDevices : null;
        const AC = typeof window !== 'undefined' ? (window.AudioContext || window.webkitAudioContext) : null;
        if (!md || typeof md.getUserMedia !== 'function' || !AC) return;
        md.getUserMedia({ audio: true }).then(function (stream) {
            if (token !== meterToken || !meterRun) {
                stream.getTracks().forEach(function (t) { t.stop(); });
                return;
            }
            const m = meterRun;
            m.stream = stream;
            try {
                m.ctx = new AC();
                const src = m.ctx.createMediaStreamSource(stream);
                const an = m.ctx.createAnalyser();
                an.fftSize = 512;
                src.connect(an);
                const data = new Uint8Array(an.fftSize);
                m.live = true;
                meter.classList.remove('ma-stl-wall-ai__listening--anim');
                const shape = [0.45, 0.7, 0.9, 1, 0.8, 1, 0.9, 0.7, 0.45];
                const tick = function () {
                    if (meterRun !== m) return;
                    an.getByteTimeDomainData(data);
                    let sum = 0;
                    for (let i = 0; i < data.length; i++) {
                        const v = (data[i] - 128) / 128;
                        sum += v * v;
                    }
                    const level = Math.min(1, Math.sqrt(sum / data.length) * 5);
                    m.level = level;
                    for (let b = 0; b < bars.length; b++) {
                        const h = 4 + Math.round(level * 16 * shape[b] * (0.75 + Math.random() * 0.25));
                        bars[b].style.height = h + 'px';
                    }
                    m.raf = window.requestAnimationFrame(tick);
                };
                tick();
            } catch (_e) {
                m.live = false;
                meter.classList.add('ma-stl-wall-ai__listening--anim');
            }
        }).catch(function () { /* sin acceso al volumen: se queda la animación fija */ });
    }

    /*
     * Tecla Alt sola: activa/apaga el micrófono con la pestaña IA visible, también con el foco en la entrada del chat.
     * Se arma al pulsar Alt y actúa al soltarlo si no se ha pulsado otra tecla (Alt+Tab, Alt+F4, AltGr y atajos siguen igual).
     * preventDefault solo en ese keyup (evita que el navegador active la barra de menú).
     */
    let altArmed = false;
    if (typeof window !== 'undefined') {
        window.addEventListener('keydown', function (ev) {
            if (ev.key === 'Alt' && !ev.repeat) { altArmed = maStlWallAiIsBareAltKey(ev); return; }
            if (ev.key !== 'Alt') altArmed = false;
        }, true);
        window.addEventListener('keyup', function (ev) {
            if (ev.key !== 'Alt') return;
            const armed = altArmed;
            altArmed = false;
            if (!armed || !maStlWallAiIsBareAltKey(ev)) return;
            if (!root.isConnected || !root.getClientRects().length) return;
            if (micBtn.disabled) return;
            ev.preventDefault();
            ev.stopPropagation();
            micToggle();
        }, true);
        window.addEventListener('mousedown', function () { altArmed = false; }, true);
        window.addEventListener('blur', function () { altArmed = false; });
    }

    function joinVoice(base, said) {
        return base && said ? base + ' ' + said : (base || said || '');
    }

    /** Aviso del micrófono en el chat (no entra en el historial del intérprete). */
    function voiceNote(text) {
        if (!text) return;
        bubble('assistant', text);
        /* También en voz alta, en el idioma del micrófono (respuesta del usuario 14/11/2026). */
        speakRes({ reply: text, lang: (voiceVT || T).lang });
    }

    /*
     * Modo continuo: el micrófono sigue encendido tras cada orden. `voiceOn` = lo ha encendido el usuario;
     * `rec` = reconocimiento en curso (Chrome lo corta tras un silencio o ~60 s: se reinicia solo en `onend`).
     * Solo se apaga con el icono, Alt, al cambiar de pestaña/muro, al volver del compacto o al cerrar, o por error grave.
     */
    let voiceOn = false;
    let voiceVT = null;
    let voiceTimer = null;
    let voiceNetFails = 0;
    let voiceRestarts = [];
    let voiceQueue = [];
    let voiceLastSent = { text: '', at: 0 };
    const VOICE_NET_MAX = 3;

    /** Apaga el micrófono. Por defecto quita de la entrada lo que se estaba dictando (no envía nada a medias). */
    function stopVoice(keepText) {
        /* Salir de la pestaña, cambiar de muro o cerrar: también se corta la voz. */
        speakCancel();
        if (!voiceOn && !rec) return;
        voiceOn = false;
        if (voiceTimer) { clearTimeout(voiceTimer); voiceTimer = null; }
        voiceQueue = [];
        const r = rec;
        rec = null;
        if (r) { try { r.abort(); } catch (_e) { /* ya parado */ } }
        stopMeter();
        if (!keepText) {
            input.value = voiceBase;
            autoGrow();
        }
        voiceBase = '';
        paintMic(false);
    }

    function voiceGiveUp(text) {
        const VT = voiceVT || T;
        stopVoice(false);
        voiceNote(text ? text + ' ' + VT.micOff : VT.micOff);
    }

    /** Envía una orden dictada por el mismo camino que Enviar; si la IA está ocupada, espera su turno. Sin duplicados. */
    function voiceSubmit(text) {
        const t = String(text || '').replace(/\s+/g, ' ').trim();
        if (!t) return;
        const now = Date.now();
        if (t === voiceLastSent.text && now - voiceLastSent.at < 1500) return;
        voiceLastSent = { text: t, at: now };
        if (busy) { voiceQueue.push(t); return; }
        input.value = t;
        autoGrow();
        send(true);
    }

    function voiceScheduleRestart(ms) {
        if (!voiceOn || speaking) return;
        if (voiceTimer) clearTimeout(voiceTimer);
        voiceTimer = setTimeout(function () { voiceTimer = null; if (voiceOn && !rec && !speaking) voiceListenOnce(); }, ms);
    }

    function voiceListenOnce() {
        const tag = maStlWallAiVoiceLang(history, T.lang);
        const VT = Object.assign({}, maStlWallAiTexts(tag === 'en-US' ? 'en' : 'es'));
        voiceVT = VT;
        /* Evita bucles: demasiados reinicios seguidos sin oír nada → se apaga. */
        const now = Date.now();
        voiceRestarts = voiceRestarts.filter(function (t) { return now - t < 4000; });
        voiceRestarts.push(now);
        if (voiceRestarts.length > 6) { voiceGiveUp(VT.micErrOther.replace('{0}', 'restart')); return; }
        let r;
        try { r = new SpeechCtor(); } catch (e) {
            voiceGiveUp(maStlWallAiVoiceErrorText(e && e.name ? e.name : 'error', VT));
            return;
        }
        let sentUpTo = 0;
        let retryMs = 0;
        r.lang = tag;
        r.interimResults = true;
        r.continuous = true;
        r.maxAlternatives = 1;
        r.onresult = function (ev) {
            /* Mientras la IA habla no se toma nada como orden (no se escucha a sí misma). */
            if (rec !== r || !voiceOn || speaking) return;
            const list = ev && ev.results ? ev.results : [];
            let part = '';
            const finals = [];
            for (let i = 0; i < list.length; i++) {
                const one = list[i];
                const t = one && one[0] && one[0].transcript ? one[0].transcript : '';
                if (one && one.isFinal) {
                    if (i >= sentUpTo) { finals.push(t); sentUpTo = i + 1; }
                } else if (i >= sentUpTo) part += t;
            }
            if (finals.length || part.trim()) { voiceNetFails = 0; voiceRestarts = []; }
            finals.forEach(function (t) {
                const said = String(t).replace(/\s+/g, ' ').trim();
                if (!said) return;
                const full = joinVoice(voiceBase, said);
                voiceBase = '';
                voiceSubmit(full);
            });
            if (!busy) {
                input.value = joinVoice(voiceBase, part.replace(/\s+/g, ' ').trim());
                autoGrow();
            }
        };
        r.onerror = function (ev) {
            if (rec !== r) return;
            const code = ev && ev.error ? ev.error : '';
            const policy = maStlWallAiVoiceErrorPolicy(code);
            if (policy === 'silent') return;
            if (policy === 'retry') {
                voiceNetFails++;
                if (voiceNetFails >= VOICE_NET_MAX) { voiceGiveUp(maStlWallAiVoiceErrorText(code, VT)); return; }
                retryMs = 1000 * voiceNetFails;
                return;
            }
            voiceGiveUp(maStlWallAiVoiceErrorText(code, VT));
        };
        r.onend = function () {
            if (rec !== r) return;
            rec = null;
            if (!voiceOn) return;
            /* Fin de una sesión de Chrome (silencio, ~60 s, error leve): seguir escuchando. */
            if (!busy) {
                input.value = voiceBase;
                autoGrow();
            }
            voiceScheduleRestart(retryMs || 150);
        };
        rec = r;
        try {
            r.start();
        } catch (e) {
            rec = null;
            voiceGiveUp(maStlWallAiVoiceErrorText(e && e.name === 'NotAllowedError' ? 'not-allowed' : (e && e.name ? e.name : 'error'), VT));
        }
    }

    function startVoice() {
        if (voiceOn || rec || !SpeechCtor || !voiceSecure) return;
        voiceOn = true;
        voiceNetFails = 0;
        voiceRestarts = [];
        voiceQueue = [];
        voiceBase = String(input.value || '').trim();
        paintMic(true);
        voiceListenOnce();
        if (voiceOn) startMeter(voiceVT || T);
    }

    /*
     * Respuestas habladas. Mientras habla, el reconocimiento se para (no se oye a sí mismo) y vuelve al terminar;
     * pulsar el micro/Alt, enviar o pulsar un botón corta la voz. También se corta al cambiar de pestaña o de muro,
     * cerrar el formulario u ocultar la página. Sin voz del idioma (o sin API) el icono queda deshabilitado.
     */
    let speakOn = maStlWallAiSpeakPref();
    let speaking = false;
    /* Saludo hablado: una vez por conversación, al mostrarse la pestaña (focus). */
    let greetText = '';
    let greeted = false;
    let speakBlocked = 0;
    let speakToken = 0;
    let speakTimer = null;
    let voicesKnown = false;
    let speakLast = null;

    function speakVoices() {
        try { return synth ? (synth.getVoices() || []) : []; } catch (_e) { return []; }
    }

    function speakTag(lang) {
        if (lang === 'en') return 'en-US';
        if (lang === 'es') return 'es-ES';
        return maStlWallAiVoiceLang(history, T.lang);
    }

    /** ¿Se puede hablar en `tag`? { ok, why, voice }. Sin voces cargadas todavía: sí, pero no habla hasta tenerlas. */
    function speakAvailable(tag) {
        if (!synth) return { ok: false, why: T.spkUnsupported, voice: null };
        const voice = maStlWallAiPickVoice(speakVoices(), tag);
        if (!voice && voicesKnown) return { ok: false, why: fmt(T.spkNoVoice, tag), voice: null };
        return { ok: true, why: '', voice: voice };
    }

    function paintSpk() {
        const av = speakAvailable(speakTag());
        const on = speakOn && av.ok;
        spkBtn.innerHTML = voiceIcon(on ? 'spk' : 'spkMute');
        spkBtn.classList.toggle('ma-stl-wall-ai__spk--on', on);
        spkBtn.classList.toggle('ma-stl-wall-ai__spk--talking', on && speaking);
        spkBtn.setAttribute('aria-pressed', on ? 'true' : 'false');
        /* Encendido: gris 70% (#4d4d4d) con icono blanco, como el micrófono escuchando. */
        spkBtn.style.background = on ? '#4d4d4d' : '#ffffff';
        spkBtn.style.color = on ? '#ffffff' : '#2563eb';
        spkBtn.style.border = '1px solid ' + (on ? '#4d4d4d' : '#cbd5e1');
        spkBtn.disabled = !av.ok;
        spkBtn.style.opacity = av.ok ? '1' : '0.45';
        spkBtn.style.cursor = av.ok ? 'pointer' : 'not-allowed';
        const tip = !av.ok ? av.why : (speakOn ? T.spk : T.spkOff);
        spkBtn.title = tip;
        spkBtn.setAttribute('aria-label', tip);
        /* Un botón deshabilitado no siempre muestra el tooltip: va también en el envoltorio. */
        spkWrap.title = av.ok ? '' : tip;
    }

    /** Pausa el reconocimiento mientras habla (sin perder el modo continuo). */
    function voicePauseForSpeech() {
        if (!voiceOn) return;
        if (voiceTimer) { clearTimeout(voiceTimer); voiceTimer = null; }
        const r = rec;
        rec = null;
        if (r) { try { r.abort(); } catch (_e) { /* ya parado */ } }
        if (!busy) {
            input.value = voiceBase;
            autoGrow();
        }
        meterLabel.textContent = (voiceVT || T).micPaused;
    }

    function voiceResumeAfterSpeech() {
        if (!voiceOn) return;
        meterLabel.textContent = (voiceVT || T).micListening;
        voiceRestarts = [];
        voiceScheduleRestart(300);
    }

    /** Corta la voz (si hablaba) y reanuda el micrófono si estaba en modo continuo. */
    function speakCancel() {
        speakToken++;
        if (speakTimer) { clearTimeout(speakTimer); speakTimer = null; }
        const was = speaking;
        speaking = false;
        if (synth && was) { try { synth.cancel(); } catch (_e) { /* nada que cortar */ } }
        if (was) {
            paintSpk();
            voiceResumeAfterSpeech();
        }
    }

    function speakVisible() {
        return root.isConnected && root.getClientRects().length > 0;
    }

    /** Lee en voz alta una respuesta ({ reply, speech?, table?, lang? }) por trozos (fallo de Chrome con frases largas). */
    function speakRes(res) {
        if (!synth || !speakOn || !res || res.pending || res.speak === false) return false;
        if (!speakVisible()) return false;
        const tag = speakTag(res.lang);
        const av = speakAvailable(tag);
        if (!av.ok || !av.voice) { paintSpk(); return false; }
        const chunks = maStlWallAiSpeechChunks(maStlWallAiSpeechText(res, tag === 'en-US' ? 'en' : 'es'));
        if (!chunks.length) return false;
        speakCancel();
        const token = ++speakToken;
        speaking = true;
        speakLast = { tag: tag, voice: av.voice.name, chunks: chunks.slice() };
        voicePauseForSpeech();
        paintSpk();
        try { synth.cancel(); } catch (_e) { /* cola vacía */ }
        let i = 0;
        let tail = 0;
        const finish = function () {
            if (token !== speakToken) return;
            speaking = false;
            speakTimer = null;
            paintSpk();
            voiceResumeAfterSpeech();
        };
        const next = function () {
            if (token !== speakToken) return;
            if (speakTimer) { clearTimeout(speakTimer); speakTimer = null; }
            if (!speakVisible()) { speakCancel(); return; }
            if (i >= chunks.length) {
                /* Si el navegador aún suena (aviso perdido), espera un poco antes de reabrir el micro. */
                if (synth.speaking && tail++ < 40) { speakTimer = setTimeout(next, 250); return; }
                finish();
                return;
            }
            const text = chunks[i++];
            const u = new window.SpeechSynthesisUtterance(text);
            u.lang = av.voice.lang || tag;
            u.voice = av.voice;
            u.rate = 1;
            u.pitch = 1;
            u.volume = 1;
            let fired = false;
            const done = function () { if (fired) return; fired = true; next(); };
            u.onend = done;
            u.onerror = function (ev) {
                /* Bloqueado por el navegador (sin interacción previa, autoplay): no se reintenta. */
                if (ev && ev.error === 'not-allowed') { speakBlocked++; i = chunks.length; }
                done();
            };
            /* Vigilante: Chrome a veces no lanza onend. */
            speakTimer = setTimeout(done, 4000 + text.length * 110);
            try {
                if (synth.paused) synth.resume();
                synth.speak(u);
            } catch (_e) { done(); }
        };
        next();
        return true;
    }

    /**
     * Saludo inicial en voz alta, una sola vez por conversación. Sin interacción previa con la página
     * (`navigator.userActivation`) el navegador no deja hablar: no se intenta ni se reintenta.
     */
    function speakGreeting() {
        if (greeted || history.length) return false;
        greeted = true;
        const ua = typeof navigator !== 'undefined' ? navigator.userActivation : null;
        if (ua && ua.hasBeenActive === false) return false;
        return speakRes({ reply: greetText, lang: T.lang });
    }

    if (synth) {
        if (speakVoices().length) voicesKnown = true;
        const onVoices = function () { voicesKnown = true; paintSpk(); };
        if (typeof synth.addEventListener === 'function') synth.addEventListener('voiceschanged', onVoices);
        else synth.onvoiceschanged = onVoices;
        /* Sin evento (navegadores sin voces): a los 2,5 s se da por cargada la lista. */
        setTimeout(function () { if (!voicesKnown) { voicesKnown = true; paintSpk(); } }, 2500);
    }
    if (typeof document !== 'undefined') {
        document.addEventListener('visibilitychange', function () { if (document.hidden) speakCancel(); });
    }
    if (typeof window !== 'undefined') window.addEventListener('pagehide', function () { speakCancel(); });
    paintSpk();

    spkBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        speakOn = !speakOn;
        maStlWallAiSetSpeakPref(speakOn);
        if (!speakOn) speakCancel();
        paintSpk();
    });

    /** Micro/Alt: si la IA está hablando, primero se corta la voz (y el micro queda escuchando). */
    function micToggle() {
        if (speaking) {
            speakCancel();
            if (!voiceOn) startVoice();
            return;
        }
        if (voiceOn) stopVoice(false); else startVoice();
    }

    micBtn.addEventListener('click', function (ev) {
        ev.preventDefault();
        micToggle();
    });
    root.appendChild(inputRow);

    const hint = document.createElement('div');
    hint.textContent = T.hint;
    hint.style.fontSize = '11px';
    hint.style.color = '#64748b';
    hint.style.marginTop = '-4px';
    root.appendChild(hint);

    container.appendChild(root);

    function legacyCopy(text) {
        return new Promise(function (resolve, reject) {
            const ta = document.createElement('textarea');
            ta.value = text;
            ta.setAttribute('readonly', '');
            ta.style.position = 'fixed';
            ta.style.left = '-9999px';
            document.body.appendChild(ta);
            ta.select();
            let ok = false;
            try { ok = document.execCommand('copy'); } catch (_e) { ok = false; }
            document.body.removeChild(ta);
            if (ok) resolve(); else reject(new Error('copy'));
        });
    }

    function copyText(text) {
        if (typeof navigator !== 'undefined' && navigator.clipboard && navigator.clipboard.writeText) {
            return navigator.clipboard.writeText(text).catch(function () { return legacyCopy(text); });
        }
        return legacyCopy(text);
    }

    function smallBtn(label, primary) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = primary ? 'btn btn-sm btn-primary' : 'btn btn-sm btn-outline-secondary';
        btn.textContent = label;
        btn.style.fontSize = '12px';
        btn.style.padding = '3px 10px';
        btn.style.borderRadius = '6px';
        btn.style.cursor = 'pointer';
        if (primary) {
            /* Botón de color: texto blanco (regla de Viewer.cshtml). */
            btn.style.background = '#2563eb';
            btn.style.borderColor = '#2563eb';
            btn.style.color = '#ffffff';
        }
        return btn;
    }

    function renderTable(table) {
        const wrap = document.createElement('div');
        wrap.className = 'ma-stl-wall-ai__table';
        wrap.style.marginTop = '6px';
        wrap.style.whiteSpace = 'normal';
        const scroller = document.createElement('div');
        scroller.style.maxHeight = table.maxHeight || '165px';
        scroller.style.overflow = 'auto';
        scroller.style.border = '1px solid #e2e8f0';
        scroller.style.borderRadius = '6px';
        const tbl = document.createElement('table');
        tbl.className = 'table table-sm mb-0';
        tbl.style.fontSize = '12px';
        tbl.style.margin = '0';
        tbl.style.width = '100%';
        tbl.style.borderCollapse = 'collapse';
        const cellStyle = function (td, col, head) {
            td.style.padding = '3px 6px';
            td.style.textAlign = col.align === 'right' ? 'right' : 'left';
            td.style.borderBottom = '1px solid #eef2f7';
            td.style.verticalAlign = 'top';
            if (col.key === 'code' || col.align === 'right') td.style.whiteSpace = 'nowrap';
            if (head) {
                td.style.position = 'sticky';
                td.style.top = '0';
                td.style.background = '#f1f5f9';
                td.style.fontWeight = '700';
            }
        };
        const fmtCell = function (col, row) {
            const v = row[col.key];
            return col.format ? col.format(v) : (v == null ? '' : String(v));
        };
        const thead = document.createElement('thead');
        const hr = document.createElement('tr');
        table.columns.forEach(function (col) {
            const th = document.createElement('th');
            th.textContent = col.label;
            cellStyle(th, col, true);
            hr.appendChild(th);
        });
        thead.appendChild(hr);
        tbl.appendChild(thead);
        const tbody = document.createElement('tbody');
        table.rows.forEach(function (row) {
            const tr = document.createElement('tr');
            table.columns.forEach(function (col) {
                const td = document.createElement('td');
                const txt = fmtCell(col, row);
                td.textContent = txt;
                if (col.key === 'name' && !txt) {
                    td.textContent = '—';
                    td.style.color = '#94a3b8';
                }
                cellStyle(td, col, false);
                tr.appendChild(td);
            });
            tbody.appendChild(tr);
        });
        tbl.appendChild(tbody);
        if (table.totals) {
            const tfoot = document.createElement('tfoot');
            const tr = document.createElement('tr');
            table.columns.forEach(function (col) {
                const td = document.createElement('td');
                td.textContent = fmtCell(col, table.totals);
                cellStyle(td, col, false);
                td.style.fontWeight = '700';
                td.style.background = '#f8fafc';
                td.style.position = 'sticky';
                td.style.bottom = '0';
                tr.appendChild(td);
            });
            tfoot.appendChild(tr);
            tbl.appendChild(tfoot);
        }
        scroller.appendChild(tbl);
        wrap.appendChild(scroller);
        const bar = document.createElement('div');
        bar.style.display = 'flex';
        bar.style.justifyContent = 'flex-end';
        bar.style.marginTop = '6px';
        const L = table.labels || T;
        const copyBtn = smallBtn(L.copy, false);
        copyBtn.className += ' ma-stl-wall-ai__copy';
        copyBtn.addEventListener('click', function () {
            copyText(maStlWallAiTableToTsv(table)).then(function () {
                copyBtn.textContent = L.copied;
            }, function () {
                copyBtn.textContent = L.copyFail;
            }).then(function () {
                window.setTimeout(function () { copyBtn.textContent = L.copy; }, 1600);
            });
        });
        bar.appendChild(copyBtn);
        wrap.appendChild(bar);
        return wrap;
    }

    function renderButtons(buttons) {
        const bar = document.createElement('div');
        bar.style.display = 'flex';
        bar.style.flexWrap = 'wrap';
        bar.style.gap = '6px';
        bar.style.marginTop = '6px';
        bar.style.whiteSpace = 'normal';
        buttons.forEach(function (def) {
            if (!def || !def.label || typeof def.run !== 'function') return;
            const btn = smallBtn(def.label, !!def.primary);
            btn.className += ' ma-stl-wall-ai__action';
            btn.addEventListener('click', function () {
                if (btn.disabled) return;
                speakCancel();
                btn.disabled = true;
                btn.style.opacity = '0.6';
                if (def.pendingText) pushAssistant({ reply: def.pendingText, pending: true });
                Promise.resolve()
                    .then(function () { return def.run(); })
                    .then(function (res) { if (res) pushAssistant(res); })
                    .catch(function (err) {
                        pushAssistant({ reply: T.error.replace('{0}', err && err.message ? err.message : String(err)) });
                    })
                    .then(function () {
                        btn.disabled = false;
                        btn.style.opacity = '1';
                    });
            });
            bar.appendChild(btn);
        });
        return bar;
    }

    function bubble(role, text, extra) {
        const hasTable = !!(extra && extra.table && Array.isArray(extra.table.rows) && extra.table.rows.length);
        const row = document.createElement('div');
        row.style.display = 'flex';
        row.style.justifyContent = role === 'user' ? 'flex-end' : 'flex-start';
        const b = document.createElement('div');
        b.className = 'ma-stl-wall-ai__msg ma-stl-wall-ai__msg--' + role;
        b.style.maxWidth = hasTable ? '100%' : '82%';
        if (hasTable) b.style.width = '100%';
        b.style.boxSizing = 'border-box';
        b.style.padding = '7px 10px';
        b.style.borderRadius = role === 'user' ? '12px 12px 2px 12px' : '12px 12px 12px 2px';
        b.style.fontSize = '13px';
        b.style.lineHeight = '1.4';
        b.style.whiteSpace = 'pre-wrap';
        b.style.wordBreak = 'break-word';
        if (role === 'user') {
            b.style.background = '#2563eb';
            b.style.color = '#ffffff';
        } else {
            b.style.background = '#ffffff';
            b.style.color = '#0f172a';
            b.style.border = '1px solid #e2e8f0';
        }
        const who = document.createElement('div');
        who.textContent = role === 'user' ? T.you : T.assistant;
        who.style.fontSize = '10px';
        who.style.fontWeight = '700';
        who.style.opacity = '0.75';
        who.style.marginBottom = '2px';
        const body = document.createElement('div');
        body.textContent = text;
        b.appendChild(who);
        b.appendChild(body);
        if (hasTable) b.appendChild(renderTable(extra.table));
        if (extra && Array.isArray(extra.buttons) && extra.buttons.length) b.appendChild(renderButtons(extra.buttons));
        row.appendChild(b);
        log.appendChild(row);
        /* Con tabla, que se vea el principio de la respuesta (título y resumen); si no, al final. */
        log.scrollTop = hasTable ? Math.max(0, row.offsetTop - 6) : log.scrollHeight;
    }

    /** Añade una respuesta del asistente ({ reply, table?, buttons? }) al historial y al log. */
    function pushAssistant(res) {
        const text = res && res.reply != null ? String(res.reply) : '';
        const entry = { role: 'assistant', text: text, table: res && res.table, buttons: res && res.buttons, confirm: res && res.confirm, apply: res && res.apply, cancel: res && res.cancel, swap: res && res.swap, lang: res && res.lang };
        if (!entry.text && !entry.table) return;
        history.push(entry);
        bubble('assistant', entry.text, entry);
        /* En voz alta (si el altavoz está encendido); «Calculando…» y similares no. */
        speakRes(res);
    }

    function render() {
        log.textContent = '';
        greetText = T.intro || maStlWallAiGreeting(new Date(), T.lang);
        bubble('assistant', greetText);
        for (let i = 0; i < history.length; i++) bubble(history[i].role, history[i].text, history[i]);
    }

    function autoGrow() {
        input.style.height = 'auto';
        input.style.height = Math.min(140, Math.max(44, input.scrollHeight + 2)) + 'px';
    }

    function setBusy(v) {
        busy = v;
        sendBtn.disabled = v;
        sendBtn.style.opacity = v ? '0.6' : '1';
    }

    function send(fromVoice) {
        /* Enviar a mano mientras escucha: manda lo escrito y el micrófono sigue (modo continuo). */
        if (fromVoice !== true && voiceOn) voiceBase = '';
        const text = String(input.value || '').trim();
        if (!text || busy) return;
        /* Un mensaje nuevo corta lo que estuviera diciendo. */
        speakCancel();
        const ctx = options.getContext ? options.getContext() : null;
        history.push({ role: 'user', text: text });
        bubble('user', text);
        paintSpk();
        input.value = '';
        autoGrow();
        setBusy(true);
        const handler = typeof window !== 'undefined' && typeof window.maStlWallAiHandleMessage === 'function'
            ? window.maStlWallAiHandleMessage
            : maStlWallAiHandleMessage;
        Promise.resolve()
            .then(function () { return handler(ctx, text, history.slice()); })
            .then(function (res) {
                if (res) pushAssistant(res);
                const acts = res && Array.isArray(res.actions) ? res.actions : [];
                if (acts.length && options.onActions) options.onActions(acts, ctx);
            })
            .catch(function (err) {
                const msg = T.error.replace('{0}', err && err.message ? err.message : String(err));
                history.push({ role: 'assistant', text: msg });
                bubble('assistant', msg);
                speakRes({ reply: msg });
            })
            .then(function () {
                setBusy(false);
                try { input.focus({ preventScroll: true }); } catch (_e) { input.focus(); }
                /* Órdenes dictadas mientras la IA contestaba: van en orden. */
                if (voiceOn && voiceQueue.length) voiceSubmit(voiceQueue.shift());
            });
    }

    sendBtn.addEventListener('click', function () { send(); });
    input.addEventListener('input', autoGrow);
    /* Las teclas del chat no deben llegar a los atajos del visor (Enter repite comando, Supr, Ctrl+Z…). */
    input.addEventListener('keydown', function (ev) {
        if (ev.key !== 'Escape') ev.stopPropagation();
        if (ev.key === 'Enter' && !ev.shiftKey && !ev.isComposing) {
            ev.preventDefault();
            send();
        }
    });
    input.addEventListener('keyup', function (ev) { if (ev.key !== 'Escape') ev.stopPropagation(); });
    input.addEventListener('keypress', function (ev) { ev.stopPropagation(); });

    render();

    return {
        root: root,
        input: input,
        sendBtn: sendBtn,
        log: log,
        /** Conversación nueva (al abrir el popup con otro muro o al cerrarlo). */
        reset: function (key) {
            stopVoice(false);
            /* Encofrado provisional del recuento: se quita al cambiar de muro o cerrar el formulario. */
            if (typeof window !== 'undefined' && typeof window.maStlWallAiClearTempFormwork === 'function') {
                try { window.maStlWallAiClearTempFormwork(); } catch (_e) { /* visor no disponible */ }
            }
            /* Paneles resaltados por el cambio guiado: fuera al cambiar de muro o cerrar. */
            if (typeof window !== 'undefined' && typeof window.maStlWallAiClearPanelHighlight === 'function') {
                try { window.maStlWallAiClearPanelHighlight(); } catch (_e) { /* visor no disponible */ }
            }
            ctxKey = key != null ? String(key) : null;
            history = [];
            greeted = false;
            input.value = '';
            autoGrow();
            setBusy(false);
            render();
            /* Otro chat (recto/curvo) puede haber cambiado el altavoz. */
            speakOn = maStlWallAiSpeakPref();
            paintSpk();
        },
        /** Mantiene la conversación si es el mismo muro; si no, la reinicia. */
        ensureContext: function (key) {
            const k = key != null ? String(key) : null;
            if (k !== ctxKey) this.reset(k);
        },
        history: function () { return history.slice(); },
        /** Añade una respuesta del asistente sin mensaje del usuario (p. ej. tras un cálculo). */
        pushAssistant: pushAssistant,
        focus: function () {
            speakOn = maStlWallAiSpeakPref();
            paintSpk();
            log.scrollTop = log.scrollHeight;
            try { input.focus({ preventScroll: true }); } catch (_e) { input.focus(); }
            /* Al mostrarse la pestaña IA: saludo hablado (una vez por conversación). */
            speakGreeting();
        },
        send: send,
        /** Micrófono: botón, ¿escuchando?, y parar (cancela lo dictado salvo `keepText`). */
        micBtn: micBtn,
        isListening: function () { return voiceOn; },
        /** Estado del indicador (pruebas): visible, volumen real (analizador) o animación fija, micro abierto. */
        meterInfo: function () {
            return {
                visible: meter.style.display !== 'none',
                live: !!(meterRun && meterRun.live),
                level: meterRun && meterRun.level != null ? meterRun.level : 0,
                stream: !!(meterRun && meterRun.stream && meterRun.stream.getTracks().some(function (t) { return t.readyState === 'live'; })),
                label: meterLabel.textContent,
            };
        },
        meter: meter,
        stopVoice: stopVoice,
        /** Altavoz: botón, ¿hablando?, cortar la voz, leer una respuesta y el último texto leído (pruebas). */
        speakBtn: spkBtn,
        isSpeaking: function () { return speaking; },
        isSpeakOn: function () { return speakOn; },
        stopSpeaking: speakCancel,
        speak: speakRes,
        speakInfo: function () { return speakLast; },
        /** Veces que el navegador ha bloqueado la voz (autoplay) (pruebas). */
        speakBlockedCount: function () { return speakBlocked; },
    };
}

/*
 * Modo compacto del Asistente IA (US 14/11/2026): al elegir la pestaña «Asistente IA» el formulario
 * del muro (recto: popup de wall-edit-form.js; curvo: modal #ma-stl-curved-wall-modal) se ancla a la
 * DERECHA del visor, 450 × 800 px (o la altura disponible), sin backdrop ni bloquear el visor.
 * Solo conversación + entrada; arriba, título y un icono para volver al formulario.
 */
export const MA_STL_WALL_AI_COMPACT = { width: 450, height: 800, margin: 12, minHeight: 260 };

export const MA_STL_WALL_AI_COMPACT_TEXT = {
    es: { back: 'Volver al formulario' },
    en: { back: 'Back to form' },
};

/** Texto del icono «volver» en el idioma de la interfaz. */
export function maStlWallAiCompactBackLabel(lang) {
    const L = lang === 'en' || lang === 'es' ? lang : maStlWallAiUiLang();
    return MA_STL_WALL_AI_COMPACT_TEXT[L === 'en' ? 'en' : 'es'].back;
}

/** «Asistente IA – Muro 12» / «AI Assistant – Curved wall c3». */
export function maStlWallAiCompactTitle(ctx, T) {
    T = T || maStlWallAiUiTexts();
    ctx = ctx || {};
    let name = ctx.kind === 'curved' ? T.curvedWall : (ctx.kind === 'curved-new' ? T.newCurvedWall : T.wall);
    name = String(name || '');
    name = name.charAt(0).toUpperCase() + name.slice(1);
    const key = ctx.wallKey != null && ctx.wallKey !== '' && ctx.kind !== 'curved-new' ? ' ' + String(ctx.wallKey) : '';
    return T.tab + ' \u2013 ' + name + key;
}

/**
 * Zona donde anclar el panel: el visor (#ma-stl-viewer-shell) recortado a la ventana; sin visor, la ventana.
 * @returns {{ left: number, top: number, right: number, bottom: number }}
 */
export function maStlWallAiCompactArea(doc, win) {
    doc = doc || (typeof document !== 'undefined' ? document : null);
    win = win || (typeof window !== 'undefined' ? window : null);
    const vw = (win && win.innerWidth) || (doc && doc.documentElement && doc.documentElement.clientWidth) || 1280;
    const vh = (win && win.innerHeight) || (doc && doc.documentElement && doc.documentElement.clientHeight) || 720;
    const area = { left: 0, top: 0, right: vw, bottom: vh };
    const shell = doc && doc.getElementById ? doc.getElementById('ma-stl-viewer-shell') : null;
    if (shell && shell.getBoundingClientRect) {
        const r = shell.getBoundingClientRect();
        if (r && r.width > 200 && r.height > 200) {
            area.left = Math.max(0, r.left);
            area.top = Math.max(0, r.top);
            area.right = Math.min(vw, r.right);
            area.bottom = Math.min(vh, r.bottom);
            if (area.right - area.left < 200 || area.bottom - area.top < 200) return { left: 0, top: 0, right: vw, bottom: vh };
        }
    }
    return area;
}

/**
 * Rectángulo del panel compacto: pegado a la derecha de `area`, 450 × 800 como máximo,
 * con margen; si no cabe, el ancho/alto disponibles (la conversación hace scroll dentro).
 */
export function maStlWallAiCompactRect(area) {
    const C = MA_STL_WALL_AI_COMPACT;
    const a = area || { left: 0, top: 0, right: 1280, bottom: 720 };
    const availW = Math.max(0, a.right - a.left - 2 * C.margin);
    const availH = Math.max(0, a.bottom - a.top - 2 * C.margin);
    const width = Math.round(Math.max(Math.min(C.width, availW), Math.min(280, availW)));
    const height = Math.round(Math.max(Math.min(C.height, availH), Math.min(C.minHeight, availH)));
    return { left: Math.round(a.right - C.margin - width), top: Math.round(a.top + C.margin), width: width, height: height };
}

const MA_STL_WALL_AI_BACK_SVG = 'M5.828 7l2.536 2.536L6.95 10.95 2 6l4.95-4.95 1.414 1.414L5.828 5H13a8 8 0 1 1 0 16H4v-2h9a6 6 0 1 0 0-12H5.828z';

/**
 * Icono «Volver al formulario» (mismo dibujo que Remix `ri-arrow-go-back-line`, el set de Materio,
 * en SVG para que se vea también donde no está cargada la fuente de iconos).
 * @param {{ lang?: string, color?: string, onClick?: () => void }} [opts]
 */
export function maStlWallAiCompactBackButton(opts) {
    opts = opts || {};
    const label = maStlWallAiCompactBackLabel(opts.lang);
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-sm btn-icon ma-stl-wall-ai-compact__back';
    btn.setAttribute('data-ma-stl-wall-ai-back', '1');
    btn.title = label;
    btn.setAttribute('aria-label', label);
    btn.style.display = 'none';
    btn.style.alignItems = 'center';
    btn.style.justifyContent = 'center';
    btn.style.width = '28px';
    btn.style.height = '28px';
    btn.style.padding = '0';
    btn.style.margin = '0 4px 0 auto';
    btn.style.border = 'none';
    btn.style.borderRadius = '6px';
    btn.style.background = 'transparent';
    btn.style.color = opts.color || 'inherit';
    btn.style.cursor = 'pointer';
    btn.style.flex = '0 0 auto';
    btn.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true" focusable="false"><path d="' + MA_STL_WALL_AI_BACK_SVG + '"/></svg>';
    btn.addEventListener('mouseenter', function () { btn.style.background = 'rgba(148,163,184,0.25)'; });
    btn.addEventListener('mouseleave', function () { btn.style.background = 'transparent'; });
    if (opts.onClick) btn.addEventListener('click', function (ev) { ev.preventDefault(); ev.stopPropagation(); opts.onClick(); });
    return btn;
}

/** Guarda estilos inline para restaurarlos tal cual al volver al formulario. */
function compactSaveStyles(list) {
    const saved = [];
    for (let i = 0; i < list.length; i++) {
        const el = list[i][0];
        if (!el) continue;
        const props = list[i].slice(1);
        const vals = {};
        for (let p = 0; p < props.length; p++) vals[props[p]] = el.style[props[p]];
        saved.push({ el: el, vals: vals });
    }
    return saved;
}

function compactRestoreStyles(saved) {
    for (let i = 0; i < saved.length; i++) {
        const s = saved[i];
        Object.keys(s.vals).forEach(function (k) { s.el.style[k] = s.vals[k]; });
    }
}

/** El chat ocupa el alto que queda (la conversación hace scroll dentro). */
function compactFlexChat(chat) {
    const saved = compactSaveStyles([
        [chat.root, 'flex', 'minHeight'],
        [chat.log, 'height', 'flex', 'minHeight'],
    ]);
    chat.root.style.flex = '1 1 auto';
    chat.root.style.minHeight = '0';
    chat.log.style.height = 'auto';
    chat.log.style.flex = '1 1 0';
    chat.log.style.minHeight = '120px';
    return saved;
}

/**
 * Modo compacto para el modal Bootstrap del muro curvo. Sin backdrop (oculto), sin trampa de foco,
 * el contenedor `.modal` deja pasar los clics al visor y solo el diálogo los recibe.
 * Cerrar (×, Esc) sigue cerrando el modal; al cerrarse (hidden.bs.modal) se deshace el modo compacto.
 * @param {HTMLElement} modalEl
 * @param {{ chat: object, aiTabBtn: HTMLElement, getTitle?: () => string, lang?: string,
 *           bootstrap?: object, onChange?: (compact: boolean) => void }} opts
 */
export function maStlWallAiBootstrapCompact(modalEl, opts) {
    opts = opts || {};
    const chat = opts.chat;
    let state = null;
    let prevTabBtn = null;
    const bs = function () { return opts.bootstrap || (typeof window !== 'undefined' ? window.bootstrap : null); };
    const dialog = modalEl.querySelector('.modal-dialog');
    const content = modalEl.querySelector('.modal-content');
    const header = modalEl.querySelector('.modal-header');
    const titleEl = header ? header.querySelector('.modal-title') : null;
    const closeEl = header ? header.querySelector('[data-bs-dismiss="modal"]') : null;
    const footer = modalEl.querySelector('.modal-footer');
    const bodyEl = modalEl.querySelector('.modal-body');
    const tabList = opts.aiTabBtn ? opts.aiTabBtn.closest('ul') : null;
    const tabContent = opts.aiTabBtn ? modalEl.querySelector(opts.aiTabBtn.getAttribute('data-bs-target')) : null;
    const tabContentWrap = tabContent ? tabContent.parentElement : null;
    const backBtn = maStlWallAiCompactBackButton({ lang: opts.lang, onClick: function () { api.exit(true); } });
    if (header) header.insertBefore(backBtn, closeEl || null);

    function instance() {
        const B = bs();
        return B && B.Modal ? B.Modal.getInstance(modalEl) : null;
    }

    function backdrops() {
        const out = [];
        const list = document.querySelectorAll('.modal-backdrop');
        if (list.length) out.push(list[list.length - 1]);
        return out;
    }

    function layout() {
        if (!state || !dialog) return;
        const r = maStlWallAiCompactRect(maStlWallAiCompactArea());
        dialog.style.left = r.left + 'px';
        dialog.style.top = r.top + 'px';
        dialog.style.width = r.width + 'px';
        dialog.style.height = r.height + 'px';
    }

    function onResize() { layout(); }

    const api = {
        isCompact: function () { return !!state; },
        backBtn: backBtn,
        enter: function (fromTabBtn) {
            if (state || !dialog || !content) return;
            prevTabBtn = fromTabBtn && fromTabBtn !== opts.aiTabBtn ? fromTabBtn : prevTabBtn;
            const inst = instance();
            state = {
                styles: compactSaveStyles([
                    [modalEl, 'pointerEvents'],
                    [dialog, 'position', 'left', 'top', 'width', 'height', 'maxWidth', 'margin', 'transform', 'minHeight', 'pointerEvents'],
                    [content, 'height', 'display', 'flexDirection', 'boxShadow'],
                    [bodyEl, 'flex', 'minHeight', 'display', 'flexDirection', 'overflow', 'paddingBottom'],
                    [tabContentWrap, 'flex', 'minHeight', 'display', 'flexDirection'],
                    [tabContent, 'flex', 'minHeight', 'display', 'flexDirection', 'paddingBottom'],
                    [tabList, 'display'],
                    [footer, 'display'],
                ]),
                centered: dialog.classList.contains('modal-dialog-centered'),
                title: titleEl ? titleEl.textContent : '',
                backdrops: backdrops().map(function (b) { return { el: b, display: b.style.display }; }),
                chat: chat ? compactFlexChat(chat) : [],
                trap: !!(inst && inst._focustrap && typeof inst._focustrap.deactivate === 'function'),
            };
            modalEl.classList.add('ma-stl-wall-ai-compact');
            modalEl.style.pointerEvents = 'none';
            if (state.centered) dialog.classList.remove('modal-dialog-centered');
            dialog.style.position = 'fixed';
            dialog.style.margin = '0';
            dialog.style.maxWidth = 'none';
            dialog.style.transform = 'none';
            dialog.style.minHeight = '0';
            dialog.style.pointerEvents = 'auto';
            content.style.height = '100%';
            content.style.display = 'flex';
            content.style.flexDirection = 'column';
            content.style.boxShadow = '0 16px 36px rgba(2,6,23,0.22)';
            if (bodyEl) {
                bodyEl.style.flex = '1 1 auto';
                bodyEl.style.minHeight = '0';
                bodyEl.style.display = 'flex';
                bodyEl.style.flexDirection = 'column';
                bodyEl.style.overflow = 'hidden';
            }
            [tabContentWrap, tabContent].forEach(function (el) {
                if (!el) return;
                el.style.flex = '1 1 auto';
                el.style.minHeight = '0';
                el.style.display = 'flex';
                el.style.flexDirection = 'column';
            });
            if (tabContent) tabContent.style.paddingBottom = '0';
            if (tabList) tabList.style.display = 'none';
            if (footer) footer.style.display = 'none';
            state.backdrops.forEach(function (b) { b.el.style.display = 'none'; });
            if (state.trap) inst._focustrap.deactivate();
            if (titleEl) titleEl.textContent = opts.getTitle ? opts.getTitle() : maStlWallAiCompactTitle({ kind: 'curved' });
            backBtn.style.display = 'inline-flex';
            layout();
            window.addEventListener('resize', onResize);
            if (chat) chat.focus();
            if (opts.onChange) opts.onChange(true);
        },
        /** Vuelve al formulario (tamaño, posición y pestaña anteriores). `showPrev` = false al cerrarse el modal. */
        exit: function (showPrev) {
            if (!state) return;
            const s = state;
            state = null;
            window.removeEventListener('resize', onResize);
            compactRestoreStyles(s.styles);
            compactRestoreStyles(s.chat);
            if (s.centered) dialog.classList.add('modal-dialog-centered');
            modalEl.classList.remove('ma-stl-wall-ai-compact');
            s.backdrops.forEach(function (b) { b.el.style.display = b.display; });
            if (titleEl) titleEl.textContent = s.title;
            backBtn.style.display = 'none';
            if (showPrev !== false) {
                const inst = instance();
                if (s.trap && inst && inst._focustrap && typeof inst._focustrap.activate === 'function' && inst._isShown) inst._focustrap.activate();
                const B = bs();
                const btn = prevTabBtn || modalEl.querySelector('[role="tab"]');
                if (btn && B && B.Tab) B.Tab.getOrCreateInstance(btn).show();
            }
            if (opts.onChange) opts.onChange(false);
        },
        /** Título al cambiar de muro sin cerrar el panel. */
        refreshTitle: function () {
            if (state && titleEl) titleEl.textContent = opts.getTitle ? opts.getTitle() : titleEl.textContent;
        },
        layout: layout,
    };

    if (opts.aiTabBtn) {
        opts.aiTabBtn.addEventListener('shown.bs.tab', function (ev) { api.enter(ev && ev.relatedTarget ? ev.relatedTarget : null); });
    }
    modalEl.addEventListener('hidden.bs.modal', function () { api.exit(false); });
    return api;
}
