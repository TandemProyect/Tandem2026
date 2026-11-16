# Cota «Id de Artículo» (ID de cada panel)

Parte del área [`INDEX.md`](INDEX.md). Añadido el 15/11/2026 (pt116u). Herramienta Cotas: ver [`herramientas-2d.md`](herramientas-2d.md) («Herramienta cota muro»).

## Qué hace

Tipo de cota **«Id de Artículo»** en el menú de Cotas (doble clic en `#ma-stl-tool-wall-dim` → `#desing2-dim-kinds-pop`). Al activarlo, cada cota de panel ATK-60 lleva al lado el ID del panel: **«0,90 · 7.3»**. El ID va en un chip morado de la misma etiqueta HTML, así que se lee igual con cualquier zoom.

- Casilla `data-dim-kind="article-id"`, **apagada por defecto**. Se guarda en la cookie de los tipos de cota (`desing2-dim-kinds.js`, `KINDS`), como las demás.
- Si está activa, también se pintan las cotas de panel aunque «Dimensión de Paneles» esté apagada: el ID va en esa cota.
- Las cotas que pide la IA («muéstrame los id de muros», «acótame el muro 7») no llevan IDs de artículo. «Acota todo» y el botón Cotas sí.
- **Desde el 15/11 (pt117u) solo en el muro seleccionado**, como el resto de cotas: sin selección no se ve ningún ID de artículo (solo el «ID n» de los muros) y la IA pide elegir muro («selecciona el muro 7» o «acota todo»). Con «Todo el Diseño» marcado o «acota todo», todos. El recuento de la respuesta de la IA es el de lo que se ve.
- Texto: recurso `StlPreview_DimKindArticleId` («Id de Artículo» / «Article id»), generado con `Scripts/ResourceGenerators/_gen_masterarticles_resources.js`.

## Esquema del ID: «muro.n»

| Parte | Valor |
|-------|-------|
| `muro` | El ID del muro que enseñan las Cotas («ID 7», `maStlWallVisibleIdLabels`), el mismo de «el muro 7» en la IA. Se busca con `maStlDesing2FindAxisLinesByWallKey(IdWall)`. Si no hay eje o el número ya está usado, se usa el `IdWall` del encofrado |
| `n` | Orden 1, 2, 3… de las columnas de paneles con cota, por `LocalAlongMm` desde el inicio del muro |

- **Agrupado por muro**: 7.1, 7.2… son los paneles del muro 7.
- **Corto y fácil de decir**: «el siete punto tres».
- **Estable**: sale solo del encofrado pintado (`maStlDesing2LastAtk60PaintPayload`). Mismo encofrado y mismos ID de muro dan los mismos IDs. Solo cambia si se vuelve a encofrar ese muro con otra modulación. Clave interna: `IdWall` + `alongMm`.
- **Único**: si dos muros diesen el mismo prefijo, el segundo usa su `IdWall`.
- **Columna** = el mismo agrupado que la cota de panel (`maStlWallDimClusterAtk60Spans`): piezas de igual ancho que se solapan a lo largo del muro (capas apiladas) son un solo ID, igual que tienen una sola cota.

## Qué lleva ID y qué no

| Pieza | ID |
|-------|----|
| Panel de catálogo 300 / 450 / 600 / 750 / 900 sin `IsMirrored` (las que llevan cota, de cualquier `FaceSign`) | Sí. `front` del ID |
| Panel espejo (`IsMirrored`, la otra cara) | Va con el ID cuya columna contiene su centro. `back` del ID. Si no cae en ninguna, no tiene ID |
| Esquinas (`E27004210`, `E24004217`, `E12004216`) | No. Su cota «0,30» se sigue viendo, sin ID |
| Uniones, remates, accesorios, piezas fuera de catálogo (p. ej. 120 mm) | No |
| Muro curvo | No (de momento) |
| Encofrado temporal del Recuento (`maStlWallAiTempGroup`) | No: no está en el encofrado pintado |

## Para órdenes futuras (ocultar, opacidad, recuento, sustituir)

| Función | Devuelve |
|---------|----------|
| `maStlAtk60PanelIdIndex()` | `{ list, byId, byWall, backWithoutId }`. Cada entrada: `{ id, wallKey, wallLabel, n, alongMm, lenMm, front, back, codes }` |
| `maStlAtk60FindPanelById('7.3')` · `window.maStlDesing2FindPanelById` | `{ id, wall, wallKey, n, widthMm, alongMm, codes, front, back, meshes }` o `null`. Acepta «7.3», «7,3», «7-3» y «7 3» |
| `maStlAtk60PanelIdSummary()` · `window.maStlDesing2PanelIds` | `[{ wall, ids: [{ id, widthMm, layers, back }] }]` |
| Puente IA (`maStlDesing2AiBridge`) | `showArticleIds(on)`, `articleIdsShown()`, `articleIds()`, `findArticle(id)`. `cotasState()` añade `articleIds` y el `articleId` de cada etiqueta |

`meshes` son los objetos de `maStlDesing2Atk60SampleGroup` de las dos caras (`maStlAtk60SampleWallId`, `maStlAtk60ElementCode`, `maStlAtk60LocalAlongMm`). Sirven para ocultar o dar opacidad. `maStlWallAiHighlightPanels` ya sabe resaltarlos. No hay ID de base de datos por pieza: el ID apunta a los objetos del encofrado de la sesión.

## Código

`master-article-details-stl-viewer.js`:

- `maStlWallDimArticleIdsOn` y `maStlWallDimKindEnabled` (`'article-id'`).
- `maStlWallDimCollectAtk60ModulePlacements` pone `placement.articleId`.
- El chip `.desing2-stl-wall-dim-readout__article-id` se crea en `maStlWallDimRebuildFromPlacements`.
- `maStlWallDimLabelTextMm` cuenta el ancho del ID.
- `maStlWallDimSetKind`.

Maqueta y pruebas: `/workspace/ia/p30/` (`site/p30-test.js`, 29 comprobaciones con el solver real; capturas `01-ids-articulo.png`, `02-menu-cotas.png` y `03-ids-articulo-lejos.png`).
