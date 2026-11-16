# Catálogo de comandos IA (pt104u + pt107u, 14/11/2026; pt114u cotas por voz, 15/11/2026)

Generado desde `Scripts/TemporalScript/2026-11-14_seed_TSql_AiCommand.sql`, `2026-11-14_seed_TSql_AiEnvironment.sql` y `2026-11-15_seed_TSql_AiCommand_cotas.sql` (§16.12: `WALL_IDS`, `WALL_DIMS`, `TOOL_WALL_DIM` con IA) y `2026-11-15_seed_TSql_AiCommand_article_ids.sql` (§16.13: `PANEL_IDS`) y `2026-11-15_seed_TSql_AiCommand_selection.sql` (§16.14: «selecciona el muro», «acota todo») (mismo inventario). Estado: **ejecutado en develop y producción locales** (idénticos) y **conectado al visor**: `Desing_2/AiCommands` (ver `ia-del-diseno-girar-catalogo-entornos.md` §16.11).

- Tablas: `TSql_AiCommand` (catálogo) · `TSql_AiCommandPhrase` (palabras por defecto) · `TSql_AiCommandUser` (ajuste por usuario y comando) · `TSql_AiCommandUserPhrase` (palabras propias / exclusiones) · `TSql_AiEnvironment` (entornos de conversación) · `TSql_AiEnvironmentReply` (plantillas de respuesta por entorno) · `TSql_AiUserSettings` (entorno e idioma del usuario).
- Frases efectivas de un usuario = por defecto (si `Is_UseDefaultPhrases`) − excluidas + propias; solo las sin entorno (`LinkEnvironment` NULL) + las de su entorno activo. Una frase `keyword`/`example` no puede activar dos comandos del mismo contexto e idioma.
- Si una frase larga contiene la palabra de otro comando («herramienta girar» ⊃ «girar»), gana la coincidencia más larga.
- Idioma: código BCP-47 en minúsculas (`es`, `en`, `es-ar`, `en-us`…); una variante usa sus frases + las del idioma base. Sin límite de frases por comando (jergas).
- Por usuario solo se guardan cambios (sin fila = por defecto); sus frases se suman a las por defecto salvo que en ese comando elija reemplazarlas (`Is_UseDefaultPhrases = 0`).
- `Is_AiImplemented = 0` = «pendiente de IA»: botón del visor sin orden de IA; si se pide por voz o texto, la IA dice cuál es y que se use el botón.

## Entornos de conversación

Sin fila en `TSql_AiUserSettings` = **Ejecutivo**. Respuesta = plantilla del entorno → la de Ejecutivo → el texto del JS.

| Código | ES / EN | Por defecto | Descripción | Plantillas ES / EN |
|---|---|---|---|---|
| EXECUTIVE | Ejecutivo / Executive | sí | Formal y conciso: trato de usted, frases cortas, sin coletillas. Entorno por defecto. | 75 / 9 |
| COLLOQUIAL | Coloquial / Colloquial | — | Cercano: tuteo y expresiones de la calle, sin perder precisión. | 80 / 13 |
| STREET | Colega barriobajero / Street buddy | — | Como un colega de obra: jerga de barrio, siempre respetuoso, sin insultos ni contenido ofensivo. | 81 / 15 |
| TECHNICAL | Técnico / Technical | — | Telegráfico y con datos: abreviaturas técnicas (L, e, h), sin cortesías. | 32 / 11 |

Cambiar de entorno hablando (contexto común, IA del diseño y del muro):

| Código | Entorno | Frases ES | Frases EN |
|---|---|---|---|
| ENV_EXECUTIVE | EXECUTIVE | modo ejecutivo, entorno ejecutivo, ponte serio, habla formal, modo formal, trátame de usted | executive mode, be formal, formal mode, executive environment |
| ENV_COLLOQUIAL | COLLOQUIAL | modo coloquial, entorno coloquial, habla normal, modo normal, háblame de tú | casual mode, colloquial mode, talk normally, colloquial environment |
| ENV_STREET | STREET | modo colega, entorno colega, habla como un colega, modo barrio, háblame como un colega | buddy mode, talk like a mate, street mode, mate mode |
| ENV_TECHNICAL | TECHNICAL | modo técnico, entorno técnico, habla técnico, sé técnico | technical mode, talk technical, technical environment |

Jerga ligada a un entorno (solo cuenta con ese entorno activo):

| Entorno | Comando | Idioma | Frase |
|---|---|---|---|
| COLLOQUIAL | WALL_DRAW | es | plántame un muro |
| COLLOQUIAL | WALL_DRAW | es | hazme una pared |
| COLLOQUIAL | WALL_DELETE | es | quítame ese muro |
| COLLOQUIAL | DESIGN_UNDO | es | déjalo como antes |
| COLLOQUIAL | WALL_DRAW | en | pop a wall in |
| COLLOQUIAL | DESIGN_UNDO | en | put it back |
| STREET | WALL_DRAW | es | méteme un muro, tío |
| STREET | WALL_DRAW | es | cáscame un muro |
| STREET | WALL_DELETE | es | fulmina ese muro |
| STREET | WALL_MOVE | es | arrastra ese muro, tío |
| STREET | DESIGN_UNDO | es | marcha atrás, tío |
| STREET | MODE_FORMWORK | es | enchúfale el encofrado |
| STREET | DESIGN_THANKS | es | gracias, máquina |
| STREET | WALL_DRAW | en | chuck in a wall |
| STREET | WALL_DELETE | en | bin that wall |
| STREET | DESIGN_THANKS | en | cheers mate |
| TECHNICAL | WALL_DRAW | es | crear muro recto |
| TECHNICAL | WALL_DELETE | es | eliminar muro |
| TECHNICAL | WALL_LIST | es | inventario de muros |
| TECHNICAL | WALL_DRAW | en | create straight wall |
| TECHNICAL | WALL_DELETE | en | delete wall entity |

## Comandos

| Código | Ctx | Grupo | ES / EN | Handler | Parámetros | Destr./Conf. | IA | Palabras ES | Palabras EN |
|---|---|---|---|---|---|---|---|---|---|
| WALL_DRAW | design | IA diseño · Muros | Dibujar muro / Draw wall | `ai:wall.draw` | largo*, dirección, espesor, alto, inicio | -/- | sí | dibuja, dibujar, dibujame, traza, trazar, crea, crear, creame, añade, agrega, agregar, inserta, insertar, nuevo, nueva, levanta, construye, levanta un muro, tira un muro, echa un muro, monta un muro | draw, create, add, insert, new, build, raise a wall, put up a wall |
| WALL_EDIT_DIMS | design | IA diseño · Muros | Cambiar medidas del muro / Change wall size | `ai:wall.edit` | muro*, largo / espesor / alto* | -/- | sí | cambia, cambiar, cambiale, cambialo, modifica, modificar, modificalo, ajusta, ajustar, ponle, acorta, acortar, acortalo, reduce, recorta el largo, hazlo más corto, hazlo más largo, súbele la altura, bájale la altura, más grueso, más fino, cámbiame la longitud, cámbiame el espesor, cámbiame la altura, la longitud es, el largo es, el espesor es, la altura es, el alto es (+ faltas: longuitud, esperoser, expesor, atura, altrua…) | change, modify, set, adjust, resize, shorten, make it longer, make it shorter, make it thicker, make it thinner |
| WALL_MOVE | design | IA diseño · Muros | Mover muro / Move wall | `ai:wall.move` | muro*, distancia*, dirección* | -/- | sí | mueve, mover, muevelo, desplaza, desplazar, desplazalo, traslada, trasladar, trasladalo, corre el muro, córrelo, arrímalo, empuja el muro | move, shift, slide the wall, push the wall |
| WALL_COPY | design | IA diseño · Muros | Copiar muro / Copy wall | `ai:wall.copy` | muro*, distancia*, dirección / lado* | -/- | sí | copia, copiar, copialo, duplica, duplicar, duplicalo, otro igual, repítelo, haz una copia | copy, duplicate, clone, same again, make a copy |
| WALL_DELETE | design | IA diseño · Muros | Borrar muro / Delete wall | `ai:wall.delete` | muro* | D/C | sí | borra, borrar, borralo, elimina, eliminar, eliminalo, quita, quitar, quitalo, suprime, suprimir, tira el muro, derriba, fuera ese muro, cárgate el muro | delete, remove, erase, get rid of, knock down |
| WALL_EXTEND | design | IA diseño · Muros | Alargar muro hasta otro / Extend wall to another | `ai:wall.extend` | muro*, extremo, hasta | -/- | sí | alarga, alargar, alargalo, prolonga, prolongar, prolongalo, extiende, extender, extiendelo, estira, estirar, llévalo hasta, que llegue hasta, empálmalo con | extend, lengthen, stretch, run it to, bring it to |
| WALL_ROTATE | design | IA diseño · Muros | Girar muro / Rotate wall | `ai:wall.rotate` | muro*, ángulo*, punto, copia | -/- | sí | gira, girar, giralo, girale, rota, rotar, rotalo, voltea, rotacion, dale la vuelta, tuércelo, ponlo en ángulo | rotate, turn, spin, rotation, swing it, turn it around |
| WALL_MIRROR | design | IA diseño · Muros | Simetría de muro / Mirror wall | `ai:wall.mirror` | muro*, eje*, original* | -/C | sí | simetria, simetrica, simetrico, espejo, refleja, reflejar, reflejalo, haz la simetría, en espejo, al otro lado igual | mirror, reflect, flip, mirror image, flip over |
| WALL_LIST | design | IA diseño · Muros | Listar / señalar muros / List / highlight walls | `ai:wall.list` | muro | -/- | sí | lista, listar, listado, enumera, cuantos, cuales, qué muros hay, cuántos muros hay, dime los muros, selecciona el muro, seleccióname el muro, selecciona la pared, marca el muro | list, many, which walls, what walls, how many walls, select wall, select the wall, highlight wall |
| WALL_IDS | design | IA diseño · Muros | Ver los ID de los muros / Show wall IDs | `ai:wall.ids` | mostrar / ocultar | -/- | sí | id, ids, identifica, identifícalos, etiqueta, rotula, muéstrame los id, id de muros, identifica los muros, numera los muros, muéstrame los muros, enséñame los muros, pon los números, qué número tiene cada muro, oculta los id, quita los números | identify, label, show wall ids, wall ids, number the walls, label the walls, wall numbers, hide the ids |
| WALL_DIMS | design | IA diseño · Muros | Acotar un muro / Dimension a wall | `ai:wall.dims` | muro (sin muro: el seleccionado; sin selección, los ID para elegir; «acota todo»: todo el diseño) | -/- | sí | acótame, acótalo, cota, cotas, mide, mídelo, dimensiona, cota del muro, medidas del muro, dime las medidas, cuánto mide, acótame el muro, muéstrame las cotas, quita las cotas, oculta las cotas, acota todo, acota todo el diseño, todas las cotas, cotas de todo el diseño | measure, dimensions, dimension wall, measure wall, show the dimensions, hide the dimensions, dimension everything, all dimensions |
| PANEL_IDS | design | IA diseño · Cotas | Ver los ID de los paneles / Show panel IDs | `ai:panel.ids` | mostrar / ocultar | -/- | sí | artículo(s), panel(es), pieza(s), id de artículo, id de los paneles, muéstrame los id de los paneles, pon los id de los paneles, identifica los paneles, numera los paneles, identifica las piezas, oculta los id de artículo, oculta los id de los paneles, quita los id de los paneles | article(s), panels, pieces, article ids, panel ids, show the panel ids, number the panels, label the panels, hide the panel ids |
| WALL_TRIM | design | IA diseño · Muros | Recortar muro (IA) / Trim wall (AI) | `ai:notAvailable` | — | -/- | pendiente | recorta, recortar, recortalo, corta, cortar | trim, cut |
| WALL_FILLET | design | IA diseño · Muros | Empalme / chaflán (IA) / Fillet / chamfer (AI) | `ai:notAvailable` | — | -/- | pendiente | empalme, chaflan, redondea, achaflana, redondea la esquina | fillet, chamfer |
| MODE_LINES | design | IA diseño · Modos | Modo líneas (2D) / Lines mode (2D) | `ai:mode lines\|bridge.setMode('lines')` | modo* | -/- | sí | modo líneas, modo línea, pasa a 2D, vista en planta, vuelve a líneas, modo 2D | lines mode, line mode, go to 2D, plan view, back to lines, 2D mode |
| MODE_3D | design | IA diseño · Modos | Modo generar 3D / Generate 3D mode | `ai:mode wall3d\|bridge.setMode('wall3d')` | modo* | -/- | sí | genera, generar, generalo, generame, tridimensional, modo 3D, genera el 3D, pasa a 3D, levanta en 3D, saca el 3D | generate, 3D mode, go to 3D, build the 3D, generate 3D |
| MODE_FORMWORK | design | IA diseño · Modos | Modo encofrar / Formwork mode | `ai:mode formwork\|bridge.setMode('formwork')` | modo* | -/C | sí | encofra, encofrar, encofralo, encofrado, encoframe, encofre, modo encofrar, modo encofrado, mete el encofrado, pon los paneles | formwork, formwork mode, add the formwork, place the panels |
| DESIGN_UNDO | design | IA diseño · Historial | Deshacer / Undo | `ai:undo\|maStlDesing2Undo` | — | -/- | sí | deshaz, deshacer, deshazlo, revierte, vuelve atrás, como estaba, eso no, atrás | undo, revert, take it back, step back |
| DESIGN_REDO | design | IA diseño · Historial | Rehacer / Redo | `ai:redo\|maStlDesing2Redo` | — | -/- | sí | rehaz, rehacer, vuelve a hacerlo, recupera lo deshecho | redo, do it again, bring it back |
| DESIGN_HELP | design | IA diseño · Conversación | Ayuda (IA diseño) / Help (design AI) | `ai:help` | — | -/- | sí | ayuda, comandos, ordenes, qué puedes hacer, qué sabes hacer, qué comandos hay | help, commands, what can you do |
| DESIGN_THANKS | design | IA diseño · Conversación | Gracias (IA diseño) / Thanks (design AI) | `ai:thanks` | — | -/- | sí | gracias, muchas gracias, perfecto gracias, buen trabajo | thanks, thank, thx, thank you, good job |
| DESIGN_GREET | design | IA diseño · Conversación | Saludo (IA diseño) / Greeting (design AI) | `ai:greet` | — | -/- | sí | hola, buenas, buenos, buenos días, buenas tardes, qué tal | hello, hi, hey, good morning, good afternoon |
| WALLAI_MATERIALS | wall | IA muro · Materiales | Lista de materiales del muro / Wall bill of materials | `ai:wall materials\|maStlWallAiMaterialsReply` | muro* | -/- | sí | material, materiales, bom, despiece, despieces, desglose, qué lleva el muro, cuánto pesa el muro, saca el despiece, lista de material | material, materials, bom, takeoff, breakdown, bill of materials, what does it take, parts list |
| WALLAI_PANEL_STOCK | wall | IA muro · Paneles | Stock de paneles (excluir / limitar anchos) / Panel stock (exclude / limit widths) | `ai:wall stock\|maStlWallAiParseStockOrder→maStlWallAiStockReply` | ancho*, cantidad | -/C | sí | sustituye, sustituir, sustituyelos, sustitucion, reemplaza, reemplazar, reemplazalos, cambia, cambiar, cambialos, quita, quitar, quitalos, excluye, no tengo paneles de, se han acabado los de, sin paneles de, agotados los de | replace, substitute, swap, replacement, change, exchange, remove, exclude, out of panels, no panels of, ran out of |
| WALLAI_PANEL_SWAP | wall | IA muro · Paneles | Cambio guiado de panel / Guided panel swap | `ai:wall swap\|maStlWallAiParseSwapOrder→maStlWallAiSwapReply` | ancho*, combinación | -/C | sí | me falta un panel, me faltan paneles, combínalo con otros, cámbialo por dos | i am short of, missing a panel, combine with others |
| WALLAI_HELP | wall | IA muro · Conversación | Ayuda (IA muro) / Help (wall AI) | `ai:wall help` | — | -/- | sí | ayuda, ayudame, comandos, instrucciones, hola, buenas, buenos, opciones | help, commands, instructions, hello, hi, hey, options |
| WALLAI_THANKS | wall | IA muro · Conversación | Gracias (IA muro) / Thanks (wall AI) | `ai:wall thanks` | — | -/- | sí | gracias, muchas gracias | thanks, thank, cheers, thank you |
| COMMON_YES | common | Común · Respuestas | Sí / confirmar / Yes / confirm | `ai:maStlWallAiYesNo=yes` | — | -/- | sí | si, vale, claro, exacto, correcto, eso, afirmativo, venga, aja, perfecto, bien, ok, adelante, de acuerdo, hazlo | yes, sure, ok, correct, exactly, absolutely, indeed, please, go ahead, do it, yep |
| COMMON_NO | common | Común · Respuestas | No / rechazar / No / decline | `ai:maStlWallAiYesNo=no` | — | -/- | sí | no, nada, negativo, tampoco, mejor no, ni hablar | no, not, nope, wrong, no thanks, not now |
| COMMON_CANCEL | common | Común · Respuestas | Cancelar la orden en curso / Cancel current order | `ai:cancel` | — | -/- | sí | cancela, cancelar, cancelalo, olvidalo, déjalo estar, olvida eso, para ya | cancel, nevermind, stop, forget, forget it, never mind |
| AI_OPEN_DESIGN | common | Común · Asistente | Abrir IA de diseño / Open design AI | `click:#ma-stl-mode-ia` | — | -/- | pendiente | abre la IA, abre el asistente, oye asistente | open the AI, open assistant |
| AI_CLOSE | common | Común · Asistente | Cerrar la IA / Close the AI | `click:[data-ma-stl-wall-ai-back]\|Esc` | — | -/- | pendiente | cierra la IA, cierra el asistente, adiós asistente | close the AI, close assistant |
| AI_MIC_TOGGLE | common | Común · Asistente | Micrófono (dictado) / Microphone (dictation) | `click:[data-ma-stl-wall-ai-mic]\|Alt` | — | -/- | pendiente | activa el micro, apaga el micro, deja de escuchar | mic on, mic off, stop listening |
| AI_SPEAKER_TOGGLE | common | Común · Asistente | Voz de respuesta (altavoz) / Spoken replies (speaker) | `click:[data-ma-stl-wall-ai-spk]\|maStlWallAiSetSpeakPref` | — | -/- | pendiente | háblame, no hables, cállate, activa la voz, quita la voz | talk to me, be quiet, voice on, voice off |
| VIEW_SYSTEM_SELECT | design | Visor · Modos | Elegir sistema de encofrado / Choose formwork system | `click:#ma-stl-mode-system-dropdown` | sistema* | -/- | pendiente | cambia el sistema, elige sistema, sistema de encofrado | change system, formwork system |
| TOOL_POLYLINE | design | Visor · Dibujo | Polilínea / Polyline | `toolbar:polyline\|maStlDesing2ActivateToolbarCommandById('polyline')` | — | -/- | pendiente | herramienta polilínea, dibuja líneas, polilínea | polyline tool, draw lines, polyline |
| TOOL_WALL_2D | design | Visor · Dibujo | Muro en planta (herramienta) / Plan wall (tool) | `toolbar:wall-2d\|maStlDesing2ActivateToolbarCommandById('wall-2d')` | — | -/- | pendiente | herramienta muro, muro a mano, dibujar muro con ratón | wall tool, draw wall by hand |
| TOOL_INSERT_ENCLOSURE | design | Visor · Dibujo | Insertar recinto / Insert enclosure | `toolbar:insert-enclosure\|maStlDesing2ActivateToolbarCommandById('insert-enclosure')` | — | -/- | pendiente | inserta un recinto, recinto rectangular, haz una caja de muros | insert enclosure, rectangular room |
| TOOL_PILLARS | design | Visor · Dibujo | Insertar pilar / Insert pillar | `toolbar:pillars\|maStlDesing2ActivateToolbarCommandById('pillars')` | — | -/- | pendiente | inserta un pilar, pon un pilar, pilar | insert pillar, add a column, pillar |
| TOOL_INSERT_WORKER | design | Visor · Dibujo | Insertar trabajador / Insert worker | `toolbar:insert-worker\|maStlDesing2ActivateToolbarCommandById('insert-worker')` | — | -/- | pendiente | inserta un trabajador, pon un operario, muñeco | insert worker, add a worker |
| TOOL_CURVED_WALL | design | Visor · Dibujo | Muro curvo / Curved wall | `toolbar:insert-curved-wall\|maStlDesing2ActivateToolbarCommandById('insert-curved-wall')` | — | -/- | pendiente | muro curvo, muro redondo, inserta un muro curvo | curved wall, round wall |
| TOOL_INSERT_CORNER | design | Visor · Dibujo | Insertar esquina L / Insert L corner | `toolbar:insert-corner\|maStlDesing2ActivateToolbarCommandById('insert-corner')` | — | -/- | pendiente | inserta una esquina, esquina en L, esquina | insert corner, L corner |
| TOOL_COPY | design | Visor · Modificar | Herramienta copiar / Copy tool | `toolbar:copy\|maStlDesing2ActivateToolbarCommandById('copy')` | — | -/- | pendiente | herramienta copiar, comando copiar | copy tool, copy command |
| TOOL_MOVE | design | Visor · Modificar | Herramienta desplazar / Move tool | `toolbar:move\|maStlDesing2ActivateToolbarCommandById('move')` | — | -/- | pendiente | herramienta desplazar, comando mover | move tool, move command |
| TOOL_ROTATE | design | Visor · Modificar | Herramienta girar / Rotate tool | `toolbar:rotate\|maStlDesing2ActivateToolbarCommandById('rotate')` | — | -/- | pendiente | herramienta girar, comando girar | rotate tool, rotate command |
| TOOL_MIRROR | design | Visor · Modificar | Herramienta simetría / Mirror tool | `toolbar:mirror\|maStlDesing2ActivateToolbarCommandById('mirror')` | — | -/- | pendiente | herramienta simetría, comando simetría | mirror tool, mirror command |
| TOOL_TRIM | design | Visor · Modificar | Herramienta recortar / Trim tool | `toolbar:trim\|maStlDesing2ActivateToolbarCommandById('trim')` | — | -/- | pendiente | herramienta recortar, comando recortar | trim tool, trim command |
| TOOL_EXTEND | design | Visor · Modificar | Herramienta alargar / Extend tool | `toolbar:extend\|maStlDesing2ActivateToolbarCommandById('extend')` | — | -/- | pendiente | herramienta alargar, comando alargar | extend tool, extend command |
| TOOL_STRETCH | design | Visor · Modificar | Herramienta estirar / Stretch tool | `toolbar:stretch\|maStlDesing2ActivateToolbarCommandById('stretch')` | — | -/- | pendiente | herramienta estirar, comando estirar | stretch tool, stretch command |
| TOOL_OFFSET | design | Visor · Modificar | Herramienta offset (paralela) / Offset tool | `toolbar:offset\|maStlDesing2ActivateToolbarCommandById('offset')` | — | -/- | pendiente | herramienta offset, comando offset, herramienta paralela | offset tool, offset command |
| TOOL_DELETE | design | Visor · Modificar | Herramienta borrar / Delete tool | `toolbar:delete\|maStlDesing2ActivateToolbarCommandById('delete')` | — | D/C | pendiente | herramienta borrar, comando borrar | delete tool, delete command |
| TOOL_WALL_DIM | design | Visor · Modificar | Acotar / Dimension | `ai:wall.dims\|click:#ma-stl-tool-wall-dim` | — | -/- | sí | acota, pon cotas, herramienta cota, acotar | dimension tool, add dimensions |
| TOOL_PHASE | design | Visor · Fases e intercambio | Encofrado por fases / Formwork by phases | `toolbar:phase\|desing2-phase-start.js` | — | -/- | pendiente | fases, encofrado por fases, divide en fases | phases, formwork by phases |
| WALL_EXPORT | design | Visor · Fases e intercambio | Exportar muros / Export walls | `toolbar:wall-export\|desing2-wall-exchange.js` | — | -/- | pendiente | exporta los muros, guarda los muros en archivo | export walls |
| WALL_IMPORT | design | Visor · Fases e intercambio | Importar muros / Import walls | `toolbar:wall-import\|desing2-wall-exchange.js` | — | -/C | pendiente | importa muros, carga muros de archivo | import walls |
| DWG_IMPORT | design | Visor · Fases e intercambio | Importar DWG / Import DWG | `toolbar:dwg-import\|desing2-dwg-import.js` | — | -/- | pendiente | importa el DWG, abre un plano DWG, carga el plano | import DWG, load the drawing |
| MAP_BUILDING_IMPORT | design | Visor · Fases e intercambio | Edificio desde mapa 3D / Building from 3D map | `click:#desing2-stl-right-panel-map-building` | — | -/- | pendiente | edificio desde mapa, busca en el mapa | building from map |
| IMAGE_SKETCH_IMPORT | design | Visor · Fases e intercambio | Boceto desde imagen / Sketch from image | `click:#desing2-stl-right-panel-image-sketch\|maStlOpenImageSketchModal` | — | -/- | pendiente | boceto desde imagen, importa una foto, croquis de foto | sketch from image, import a photo |
| ORTHO_15_TOGGLE | design | Visor · Edición y teclado | Ortogonal 15° / Ortho 15° | `click:#ma-stl-tool-ortho-15\|maStlToggleLineToolOrtho15FromUi\|F8` | — | -/- | pendiente | orto, ortogonal, activa el orto, quita el orto | ortho, ortho on, ortho off |
| REPEAT_LAST | design | Visor · Edición y teclado | Repetir último comando / Repeat last command | `key:Enter` | — | -/- | pendiente | repite, otra vez lo mismo, repite el comando | repeat, repeat last |
| CANCEL_TOOL | design | Visor · Edición y teclado | Cancelar herramienta / Cancel tool | `key:Escape` | — | -/- | pendiente | sal de la herramienta, escape, suelta la herramienta | exit tool, escape |
| DELETE_SELECTION | design | Visor · Edición y teclado | Borrar selección / Delete selection | `key:Delete` | — | D/C | pendiente | borra lo seleccionado, suprime la selección | delete selection |
| VIEW_GRID_TOGGLE | design | Visor · Vista | Rejilla / Grid | `click:#ma-stl-grid-toggle` | — | -/- | pendiente | rejilla, muestra la rejilla, quita la rejilla, cuadrícula | grid, grid on, grid off |
| VIEW_RULERS_FLYOUT | design | Visor · Vista | Menú reglas y anclaje / Rulers & anchor menu | `click:#desing2-stl-rulers-flyout-toggle` | — | -/- | pendiente | menú de reglas, reglas y anclaje | rulers menu |
| VIEW_UCS_RULERS_TOGGLE | design | Visor · Vista | Reglas de suelo / Floor rulers | `click:#ma-stl-ucs-rulers-toggle` | — | -/- | pendiente | reglas del suelo, reglas de plano | floor rulers |
| VIEW_EDGE_RULERS_TOGGLE | design | Visor · Vista | Reglas de borde / Edge rulers | `click:#ma-stl-edge-rulers-toggle` | — | -/- | pendiente | reglas de borde, reglas del visor | edge rulers |
| VIEW_RULER_ANCHOR_GRID | design | Visor · Vista | Anclar reglas en rejilla / Anchor rulers to grid | `click:#ma-stl-ruler-anchor-pick-toggle` | — | -/- | pendiente | ancla las reglas, reglas en la rejilla | anchor rulers |
| VIEW_RULER_ANCHOR_OBJECT | design | Visor · Vista | Anclar reglas en pieza / Anchor rulers to part | `click:#ma-stl-ruler-anchor-object-pick-toggle` | — | -/- | pendiente | reglas desde la pieza, ancla en la pieza | anchor to part |
| VIEW_TOP | design | Visor · Vista | Vista superior / Top view | `click:[data-face="top"]` | — | -/- | pendiente | vista superior, vista de arriba, desde arriba | top view, from above |
| VIEW_BOTTOM | design | Visor · Vista | Vista inferior / Bottom view | `click:[data-face="bottom"]` | — | -/- | pendiente | vista inferior, desde abajo | bottom view, from below |
| VIEW_FRONT | design | Visor · Vista | Vista frontal / Front view | `click:[data-face="front"]` | — | -/- | pendiente | vista frontal, de frente, alzado | front view, elevation |
| VIEW_BACK | design | Visor · Vista | Vista posterior / Back view | `click:[data-face="back"]` | — | -/- | pendiente | vista trasera, desde atrás | back view, rear view |
| VIEW_LEFT | design | Visor · Vista | Vista izquierda / Left view | `click:[data-face="left"]` | — | -/- | pendiente | vista izquierda, lateral izquierdo | left view |
| VIEW_RIGHT | design | Visor · Vista | Vista derecha / Right view | `click:[data-face="right"]` | — | -/- | pendiente | vista derecha, lateral derecho | right view |
| VIEW_CAMERA_ORTHO | design | Visor · Vista | Cámara ortogonal / Orthographic camera | `click:#ma-stl-cam-ortho` | — | -/- | pendiente | vista ortogonal, cámara ortogonal, sin perspectiva | orthographic, no perspective |
| VIEW_CAMERA_ISO | design | Visor · Vista | Vista isométrica / Isometric view | `click:#ma-stl-cam-iso` | — | -/- | pendiente | vista isométrica, isométrica, en perspectiva | isometric view, perspective |
| VIEW_FULLSCREEN | design | Visor · Vista | Pantalla completa / Fullscreen | `click:#ma-stl-fullscreen-toggle` | — | -/- | pendiente | pantalla completa, maximiza el visor | fullscreen, full screen |
| VIEW_SKY_TOGGLE | design | Visor · Vista | Cielo / Sky | `click:#ma-stl-sky-toggle` | — | -/- | pendiente | cielo, muestra el cielo, quita el cielo | sky, sky on, sky off |
| VIEW_GROUND_SHADOW_TOGGLE | design | Visor · Vista | Sombra en suelo / Ground shadow | `click:#ma-stl-ground-shadow-toggle` | — | -/- | pendiente | sombra, sombra del suelo | shadow, ground shadow |
| VIEW_DARK_BG_TOGGLE | design | Visor · Vista | Fondo negro / Dark background | `click:#ma-stl-dark-bg-toggle` | — | -/- | pendiente | fondo negro, fondo oscuro, modo oscuro | dark background, dark mode |
| VIEW_CLIP_TOGGLE | design | Visor · Vista | Cortes / Clipping | `click:#ma-stl-clip-toggle` | — | -/- | pendiente | cortes, muestra los cortes, sección | clipping, section cut |
| VIEW_XYZ_AXES_TOGGLE | design | Visor · Vista | Ejes XYZ / XYZ axes | `click:#ma-stl-xyz-axes-toggle` | — | -/- | pendiente | ejes, muestra los ejes, ejes XYZ | axes, XYZ axes |
| VIEW_SIDE_PANEL_COLLAPSE | design | Visor · Vista | Ocultar panel lateral / Hide side panel | `click:#desing2-stl-side-panel-collapse` | — | -/- | pendiente | oculta el panel, esconde el panel lateral | hide side panel |
| DESIGN_SAVE_VIEW | design | Visor · Diseño y datos | Guardar vista / Save view | `click:#ma-stl-save-viewer-state` | — | -/- | pendiente | guarda la vista, guardar vista, guarda el estado | save view, save state |
| DESIGN_SCREENSHOT | design | Visor · Diseño y datos | Captura de pantalla / Screenshot | `click:#ma-stl-screenshot\|maStlDownloadDesignScreenshot` | — | -/- | pendiente | haz una captura, foto del diseño, captura | screenshot, take a picture |
| DESIGN_MATERIAL_COUNT | design | Visor · Diseño y datos | Recuento de materiales (diseño) / Material count (design) | `click:#ma-stl-material-count` | — | -/- | pendiente | recuento de materiales, material de todo el diseño, cuenta el material | material count, count materials |
| DESIGN_GROUPS | design | Visor · Diseño y datos | Grupos del diseño / Design groups | `click:#ma-stl-design-groups` | — | -/- | pendiente | grupos, abre los grupos, grupos de muros | groups, design groups |
| DESIGN_WALL_LIST_FORM | design | Visor · Diseño y datos | Listado masivo de muros / Bulk wall list | `click:#ma-stl-wall-list` | — | -/- | pendiente | tabla de muros, listado de muros, edición masiva | wall table, bulk wall list |
| WALL3D_CONCRETE_MAP | design | Visor · Diseño y datos | Mapeo de hormigón / Concrete mapping | `click:#desing2-stl-right-panel-wall-map\|maStlWall3dToggleConcreteMapping` | — | -/- | pendiente | mapeo de hormigón, textura de hormigón, hormigón | concrete mapping, concrete texture |
| CONNECTION_TEST | design | Visor · Diseño y datos | Estado de la conexión / Connection status | `click:#desing2-stl-right-panel-connection-test` | — | -/- | pendiente | estado de la conexión, prueba la conexión | connection status, test connection |
| SIDE_TAB_ENVIRONMENT | design | Visor · Diseño y datos | Pestaña Entorno / Environment tab | `click:[data-tab=entorno]` | — | -/- | pendiente | pestaña entorno, abre entorno | environment tab |
| SIDE_TAB_CONFIG | design | Visor · Diseño y datos | Pestaña Configuración / Settings tab | `click:[data-tab=config]` | — | -/- | pendiente | pestaña configuración, abre la configuración, ajustes | settings tab, open settings |
| DESIGN_LEAVE | design | Visor · Diseño y datos | Salir del diseño / Leave design | `click:#ma-stl-leave-design` | — | -/C | pendiente | sal del diseño, cierra el diseño, salir del diseño | leave design, close design |
| PLUGIN_UPDATE_LIBRARY | design | Visor · Plugin CAD | Actualizar biblioteca / Update library | `click:#ma-stl-plugin-update` | — | -/- | pendiente | actualiza la biblioteca, actualizar bloques | update library |
| PLUGIN_HOME | design | Visor · Plugin CAD | Menú general / Main menu | `click:#ma-stl-plugin-home` | — | -/- | pendiente | menú general, menú principal | main menu, home menu |
| PLUGIN_BLOCKS | design | Visor · Plugin CAD | Biblioteca de bloques / Block library | `click:#ma-stl-plugin-blocks` | — | -/- | pendiente | biblioteca de bloques, abre los bloques | block library, open blocks |
| PLUGIN_CONVERT | design | Visor · Plugin CAD | Cambiar tipo de bloques / Change block type | `click:#ma-stl-plugin-convert` | — | -/- | pendiente | cambia el tipo de bloques, convierte bloques | convert blocks, change block type |
| PLUGIN_SAVE_WALLS | design | Visor · Plugin CAD | Salvar muros (CAD) / Save walls (CAD) | `click:#ma-stl-plugin-save` | — | -/- | pendiente | salva los muros, guarda los muros en CAD | save walls, save walls to CAD |
| ENV_EXECUTIVE | common | Común · Entorno | Entorno ejecutivo / Executive environment | `ai:env EXECUTIVE` | — | -/- | sí | modo ejecutivo, entorno ejecutivo, ponte serio, habla formal, modo formal, trátame de usted | executive mode, be formal, formal mode, executive environment |
| ENV_COLLOQUIAL | common | Común · Entorno | Entorno coloquial / Colloquial environment | `ai:env COLLOQUIAL` | — | -/- | sí | modo coloquial, entorno coloquial, habla normal, modo normal, háblame de tú | casual mode, colloquial mode, talk normally, colloquial environment |
| ENV_STREET | common | Común · Entorno | Entorno colega / Street buddy environment | `ai:env STREET` | — | -/- | sí | modo colega, entorno colega, habla como un colega, modo barrio, háblame como un colega | buddy mode, talk like a mate, street mode, mate mode |
| ENV_TECHNICAL | common | Común · Entorno | Entorno técnico / Technical environment | `ai:env TECHNICAL` | — | -/- | sí | modo técnico, entorno técnico, habla técnico, sé técnico | technical mode, talk technical, technical environment |

Las palabras `helper` / `negative` (pesos), los ejemplos y las plantillas de respuesta están solo en los scripts de semilla.
