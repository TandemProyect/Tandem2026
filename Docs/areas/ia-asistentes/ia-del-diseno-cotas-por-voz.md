# IA Desing — IA del diseño: cotas por voz (ID de los muros, acotar un muro y cambiar sus medidas, ID de los paneles) (15/11/2026)

> Parte de [`ia-desing.md`](ia-desing.md). Área: [`INDEX.md`](INDEX.md). Sigue a §16.11 ([`ia-del-diseno-girar-catalogo-entornos.md`](ia-del-diseno-girar-catalogo-entornos.md)).

### 16.12 Cotas por voz (15/11/2026, `?v=20261115a`)

Órdenes de la IA del diseño que trabajan con la herramienta **Cotas** del visor (`#ma-stl-tool-wall-dim`): ver el número de cada muro, acotar uno y cambiarle la longitud, el espesor o la altura.

**Qué es el «id» de un muro**

- Es el número que pinta **Cotas** junto a cada muro («ID 7», tipo de cota `wall-id`), es decir, el `wallId` del eje o, si no tiene, su `id`. «El muro 7» es el que tiene «ID 7» en el plano.
- **Cambio**: hasta ahora la etiqueta de la IA era el `wallDbId` (por ejemplo 30652), que no se veía en el plano. Ahora la IA usa el ID de Cotas (`maStlWallVisibleIdLabels()`). Si dos ejes comparten `wallId`, se usa el `id` de cada uno para no repetir número.
- «muro N» se busca por: etiqueta visible → `id` del eje → `wallDbId` → `wallId` o grupo (si es único). Los números antiguos siguen valiendo.

**Comandos nuevos**

| Comando | Qué hace | Ejemplos |
|---------|----------|----------|
| **Ver los ID** (`wall.ids`, BD `WALL_IDS`) | Enciende Cotas **solo con el ID** de cada muro y los lista en la respuesta. Si ya estaban, lo dice. «Oculta los id» los quita. | «muéstrame los id de muros», «identifica los muros», «muéstrame los muros», «enséñame los muros», «numera los muros», «pon los números», «¿qué muros hay?», «identifícame las paredes», «oculta los id» |
| **Acotar un muro** (`wall.dims`, BD `WALL_DIMS` y `TOOL_WALL_DIM`) | Cotas **solo de ese muro**: largo total, eje, parciales, espesor e «ID 7 · h = 2,70 m» (con la altura). El muro pasa a ser el **muro actual**. «Acota» a secas: el muro seleccionado o el último que tocó la IA; «acota todo» o sin muro: las Cotas de todo el diseño, como el botón. «Quita las cotas» las quita (también las del botón). | «acótame el muro 7», «cota del muro 7», «mide el muro 7», «dime las medidas del muro 7», «acótalo», «acota todo», «quita las cotas» |
| **Cambiar medidas** (`wall.edit`, ya existía) | Sobre el muro actual: largo, espesor o alto, con un paso de **Deshacer**. Las cotas se recalculan; si estaban las de otro muro, pasan al muro editado. Sin muro actual, pregunta «¿Qué muro?». | «cámbiame la longitud a 4,5», «el espesor es 0,45», «Esperoser 0,45», «cámbiame la altura a 3», «altura 2,70», «la altura es 3,00», «atura 3», «altrua 2,70», de una vez: «muro 7 longitud 4,5», «muro 7 altura 3» |

**Palabras nuevas** (JS y BD, tipo `helper` en `WALL_EDIT_DIMS`):

- Cambiar: cámbiame, cámbiamelo.
- Longitud: longuitud, lonjitud, largura.
- Espesor: esperoser, espesoro, espersor, expesor, espresor, esperor.
- Altura: atura, altrua, altur, alturas, altuta, haltura.
- Frases: cámbiame la longitud / el espesor / la altura; la longitud es, el largo es, el espesor es, la altura es, el alto es.
- Además, los conceptos nuevos `ids` (id, identifica, etiqueta, numera…), `dims` (acota, cota, mide, dimensiona…) y `hide` (oculta, esconde, apaga…).

**Visor** (`master-article-details-stl-viewer.js`, aditivo):

- `maStlWallDimAiFocus` = `null` | `{ mode: 'ids' }` | `{ mode: 'wall', axisId }`. Con foco, Cotas pinta solo los tipos del foco (ids: `wall-id`; wall: `walls-total`, `walls-axes`, `walls-partial`, `wall-id`) y solo del eje pedido. No cambia la cookie de tipos del usuario. Las cotas marcadas `_CHeckDimWall = false` se pintan igual mientras haya foco. Parar Cotas (botón u orden) quita el foco.
- Las etiquetas son HTML: se leen igual con cualquier zoom.
- Deshacer / Rehacer (botones, Ctrl+Z y la IA) recalculan las Cotas si están encendidas.
- Puente `maStlDesing2AiBridge`: `showWallIds(on)`, `wallIdsShown()`, `dimensionWall(id)`, `showAllCotas()`, `clearCotas(all)`, `cotasState()`.

**BD** (`Scripts/TemporalScript/2026-11-15_seed_TSql_AiCommand_cotas.sql`, idempotente, con su transacción; ejecutado en develop y producción locales con `sqlcmd -E -f 65001`, idénticos):

- Comandos `WALL_IDS` (orden 92) y `WALL_DIMS` (94).
- `TOOL_WALL_DIM` pasa a `Is_AiImplemented = 1` y `ai:wall.dims|click:#ma-stl-tool-wall-dim`. Ya no responde «use el botón».
- 127 frases y 48 plantillas: `idsShown`, `idsAlready`, `idsHidden`, `idsNotShown`, `dimsShown`, `dimsAll`, `dimsHidden`, `dimsNone` y `cotasFail` × 4 entornos en ES, y `idsShown`, `dimsShown` e `idsHidden` en EN.
- **Lección**: un ensayo con `ROLLBACK` gasta valores `IDENTITY`. Tras ensayar solo en develop, los `IdObject` nuevos no coincidían con producción. Se borraron las filas nuevas, se hizo `DBCC CHECKIDENT … RESEED` al máximo anterior en las dos bases (103 / 11047 / 403) y se volvió a ejecutar. Resultado: `IdObject` y hashes idénticos. **Ensayar siempre en las dos bases o en ninguna, o hacer reseed antes de ejecutar.**

**Respuestas (Ejecutivo)**: «Le muestro el número de cada muro en el plano (15 muros: 1, 4, 7…). Para ver las medidas de uno, diga «acótame el muro 1».» · «Cotas del muro 7: largo 3,00 m, espesor 0,30 m, alto 2,70 m. Indique qué desea cambiar, por ejemplo «longitud 4,5», «espesor 0,45» o «altura 3».»

**Pruebas (p29)**

| Suite | Comprobaciones | Cubre |
|-------|----------------|-------|
| `p29/ia/unit/test.mjs` | 142 (101 + 41) | Etiquetas con faltas (atura, altrua, esperoser, longuitud), «¿Qué muro?», 8 frases de ID, ocultar, acotar, longitud / espesor / altura, Deshacer, «muro N altura 3», la cota sigue al muro editado, inglés |
| `p29/ia/unit/test-catalog.mjs` | 74 (53 + 21) | Con el catálogo de la BD (salida real del controlador C#): rutas de las frases, respuestas Ejecutivo y Técnico, «acota» ya no es «use el botón» |
| `p29/ia/check.sh t=all p29` (visor real en la maqueta) | 28 | IDs = los de la IA, legibles alejado, acotar solo el muro 7 con «h = 2,70 m», cotas que se recalculan, Deshacer con la cota, quitar / todo el diseño, sin errores JS |
| p20 · p21 · p23 | 56 · 86 · 15 | Sin cambios (p23 a veces falla al cargar el catálogo simulado en Chrome headless; también con el código anterior) |

Capturas: `/workspace/ia/p29/01-ids-muros.png`, `02-acotar-muro-7.png`, `03-longitud-4-5.png`, `04-espesor-0-45.png`, `05-altura-3.png`.

**Cómo probar**: solo JS + BD (ya ejecutada) → **Ctrl+F5**; no hace falta recompilar. Abrir un diseño en líneas, botón IA → «muéstrame los id de muros» → «acótame el muro 7» → «cámbiame la longitud a 4,5» → «el espesor es 0,45» → «cámbiame la altura a 3» → Deshacer.

**Limitaciones / abierto**

- Si dos ejes tienen el mismo `wallId`, el número de la IA es su `id`. En ese caso Cotas también pinta ese `id`, así que coinciden, pero no es el `wallId` del archivo.
- «Muéstrame los muros» enseña los ID (no la lista). «Lista los muros» sigue dando la lista.
- Con muchos muros juntos, las etiquetas ID pueden solaparse (igual que con el botón Cotas).

### 16.13 ID de los paneles por voz (15/11/2026, `?v=20261115b`)

Tipo de cota **«Id de Artículo»**: cada cota de panel lleva su ID, «0,90 · 7.3» (muro 7, 3.ª columna de paneles). El esquema, lo que lleva ID y la búsqueda por ID están en [`../desing2-dibujo-2d/cota-id-de-articulo.md`](../desing2-dibujo-2d/cota-id-de-articulo.md).

**Orden (`panel.ids`, BD `PANEL_IDS`)**

- «Muéstrame los id de los paneles», «id de artículo», «numera los paneles», «identifica las piezas»: marca la casilla «Id de Artículo» (igual que a mano, se guarda en la cookie) y abre Cotas si estaba cerrada. Quita el enfoque de «id de muros» o «acótame el muro N».
- «Oculta los id de artículo», «quita los id de los paneles»: desmarca la casilla.
- Sin encofrado no activa nada y responde «… aún no hay encofrado. Diga «encofra» y se los muestro».
- No choca con `wall.ids`: con «panel(es)», «artículo(s)» o «pieza(s)» gana `panel.ids` y `wall.ids` puntúa 0. «Muéstrame los id de muros» y «oculta los id» siguen en `wall.ids`.
- Concepto nuevo `article` (artículo, panel, pieza, article, panels, pieces). El puente expone `showArticleIds(on)`, `articleIdsShown()`, `articleIds()` y `findArticle('7.3')` para las órdenes siguientes («oculta el 7.3», «opacidad», «cuántos paneles hay», «pon dos de 0,45 en el 7.3»), que aún **no** están hechas.

**BD**: `Scripts/TemporalScript/2026-11-15_seed_TSql_AiCommand_article_ids.sql`, idempotente.

- Comando 106 `PANEL_IDS`, orden 96.
- 38 frases (11175–11212).
- 28 plantillas: `articleIdsShown`, `articleIdsAlready`, `articleIdsHidden`, `articleIdsNotShown` y `articleIdsNoFormwork` × 4 entornos en ES; `articleIdsShown` y `articleIdsHidden` en EN.
- Antes de insertar, si faltan filas, reajusta `IDENTITY` al máximo `IdObject` (`DBCC CHECKIDENT … RESEED`). Así develop y producción dan los mismos `IdObject` aunque una haya gastado valores en un ensayo.
- Ejecutado sin ensayo en las dos bases, y otra vez para comprobar que es idempotente. Hashes idénticos: comandos 102, frases 1212, plantillas 344.

**Pruebas (p30)**

| Suite | Comprobaciones | Cubre |
|-------|----------------|-------|
| `unit/test.mjs` | 155 (142 + 13) | Frases de mostrar y ocultar, sin encofrado, ya puestos, «id de muros» sigue en `wall.ids`, inglés |
| `unit/test-catalog.mjs` | 85 (74 + 11) | Con el catálogo de la BD: rutas de 7 frases, `wall.ids` intacto, respuestas Ejecutivo |
| `site/p30-test.js` (visor real, Encofrar con el solver real) | 29 | Casilla y cookie, índice por muro, IDs únicos «muro.n», chip en cada cota, esquinas sin ID, `findArticle` con piezas y mallas de las dos caras, legible alejado, casilla a mano, «Paneles» apagado, ocultar, sin errores JS |
| p20 · p21 · p23 · p29 | 56 · 86 · 15 · 28 | Sin cambios |

**Cómo probar**: hay recurso nuevo (`MasterArticles.resx` / `Designer.cs`) → **recompilar** y Ctrl+F5. Encofrar un diseño → IA → «muéstrame los id de los paneles» → «oculta los id de artículo». O doble clic en Cotas → «Id de Artículo».

### 16.14 Cotas del muro seleccionado y selección desde la IA (15/11/2026, `?v=20261115c`)

Lo normal es acotar solo el muro seleccionado; a veces, todo el diseño. Y lo que selecciona la IA es la selección real del visor (verde).

**Alcance de las Cotas** (botón, «acota» de la IA e «Id de Artículo»): ver [`../desing2-dibujo-2d/herramientas-2d.md`](../desing2-dibujo-2d/herramientas-2d.md) («Alcance»).

- Muro seleccionado → solo sus cotas. Siguen a la selección (clic en otro muro, o la IA).
- Sin selección → solo el «ID n» de cada muro, para elegir. La IA responde `design.dimsPick` («Las cotas son las del muro seleccionado y no hay ninguno… Diga «acótame el muro 7», «selecciona el muro 7» o «acota todo»»). Los ID de artículo, `design.articleIdsPick`.
- «Acota todo», «todas las cotas», «cotas de todo el diseño», «dimension everything» → todo el diseño (`showAllCotas`). En el menú, casilla «Todo el Diseño» (`all-walls`, recurso `StlPreview_DimKindAllWalls`).
- `design.dimsAll` ya no dice «como con el botón Cotas» (el botón acota el seleccionado).

**Selección real desde la IA**

- «Selecciona el muro 7», «seleccióname el muro 7», «marca el muro 7», «select wall 7» (`wall.list`), «acótame el muro 7» y «muro 7 altura 3» seleccionan el muro con `maStlSelectUserFloorLine` (puente `highlight`): la cara se pone en el verde de selección (0x39ff14) en Líneas.
- **Muro 3D**: el sólido del muro seleccionado lleva el tinte de selección (`maStlApplyStlMeshSelectedTint` sobre una copia de su material, color 0x39ff14). `maStlWall3dSyncSelectionTint` / `maStlWall3dResetSelectionTint`, también al repintar fases. Un clic en un sólido lo selecciona (`maStlWallInspectResolveSelectionAtPointer`) y un clic fuera quita la selección; el doble clic sigue abriendo la ficha.
- Clic en otro muro o Esc: la IA sigue al usuario. Su «muro actual» (`lastWallId`) solo se usa si el usuario no cambió la selección desde la última orden (`bridge.selectionSeq()`, que cuenta solo los cambios por clic en el visor o Esc, no los del visor al rehacer líneas tras deshacer o girar).
- Con «acótame el muro 7» activo, seleccionar otro muro pasa la cota a ese muro.

**BD**: `Scripts/TemporalScript/2026-11-15_seed_TSql_AiCommand_selection.sql`, idempotente (RESEED si faltan filas).

- 15 frases (11213–11227): `WALL_LIST` «selecciona el muro», «seleccióname el muro», «selecciona la pared», «marca el muro», «select wall», «select the wall», «highlight wall» y ejemplos; `WALL_DIMS` «acota todo», «acota todo el diseño», «todas las cotas», «cotas de todo el diseño», «dimension everything», «all dimensions» y ejemplos.
- 10 plantillas (480–489): `dimsPick` y `articleIdsPick` × 4 entornos ES y Ejecutivo EN.
- `design.dimsAll` ES de Ejecutivo, Coloquial y Calle sin «como con el botón Cotas» (solo si seguía el texto antiguo).
- Ejecutado en las dos bases y otra vez (idempotente). Hashes idénticos: comandos 102, frases 1227, plantillas 354.

**Pruebas (p31)**: `unit/test.mjs` 170, `unit/test-catalog.mjs` 89, `site/p31-test.js` 33 (visor real: alcance sin selección / seleccionado / sigue a la selección / «Todo el Diseño», Esc, IA selecciona el muro 7 en 2D y en 3D con el sólido teñido, clic en un sólido en 3D y clic fuera, «acota todo», ID de artículo solo del muro seleccionado con el solver real). p20 · p21 · p23 · p29 · p30 sin fallos (p30 marca «Todo el Diseño»).

**Abierto**

- En Líneas se pone verde la cara seleccionada (la selección normal), no las dos caras.
- En 3D se tiñe el tramo del muro; las esquinas y uniones (verde lima de siempre) no, y se parecen al verde de selección.
- Pasar a Muro 3D quita la selección (como antes). En 3D, con un muro seleccionado se ven las cotas de edición de la línea (HUD), como antes con la IA.
- Cerrar el asistente quita la selección que hizo (como antes).
- El encofrado (paneles) no se tiñe.

**Cómo probar**: recurso nuevo → **recompilar** y Ctrl+F5. Seleccionar un muro → Cotas; «selecciona el muro 7» en Líneas y en Muro 3D; «acota todo».
