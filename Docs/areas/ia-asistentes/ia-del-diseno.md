# IA Desing — IA del diseño (botón IA de la barra de modos): arquitectura, registro de comandos, decisiones, pruebas

> Parte de [`ia-desing.md`](ia-desing.md) (partido el 15/11/2026 sin quitar contenido; la numeración de secciones es la original). Área: [`INDEX.md`](INDEX.md).

## 16. IA del diseño (botón IA de la barra de modos)

Desde el 14/11/2026 (`?v=20261114i`), el botón **IA** (`#ma-stl-mode-ia`, el primero de la barra de modos) abre el panel **«Asistente IA – Diseño»**. Es el mismo chat que el del muro, pero sirve para **dibujar y modificar muros 2D** con órdenes escritas o dictadas. Primera fase: **muros rectos 2D**. Los demás comandos se irán añadiendo al registro (§16.3).

### 16.1 Cómo se ve y cómo se comporta

- **Formato**: el del modo compacto (§6). Mide **450 × 800** (o la altura disponible), va pegado a la derecha del visor y no tiene backdrop, así que el plano sigue siendo usable. Arriba lleva una cabecera oscura con el título «Asistente IA – Diseño» y una **×** para cerrar. **Esc** también lo cierra.
- **Chat**: es `mountWallAiChat` de `wall-ai-assistant.js`, sin copiar nada. Trae las burbujas, el saludo según la hora, el micrófono (botón y **Alt** sola, en modo continuo), las respuestas habladas, las tablas con «Copiar» y los botones (Sí/No, Deshacer).
- **Toggle**: si se pulsa el botón otra vez, el panel se cierra. Al cerrar se apagan el **micrófono** y la voz, se quita el resaltado y la conversación empieza de nuevo la próxima vez, como en el chat del muro. Mientras está abierto, el botón queda marcado (`aria-pressed="true"`, clase `active`).
- **Un solo asistente abierto a la vez** (coexistencia con la IA del muro):

| Situación | Qué pasa |
|-----------|----------|
| Se abre la IA del diseño y la del muro está en **modo compacto** (popup recto o modal curvo) | Se cierra el formulario del muro (`bridge.closeWallAi()`) |
| Se abre la IA del diseño y el formulario del muro está en **modo normal** (otras pestañas) | No se toca: flota en su sitio. Si luego se elige su pestaña Asistente IA, se cierra la del diseño (fila siguiente) |
| Se muestra la pestaña Asistente IA de un muro | El chat del muro emite `ma-stl-ai-chat-shown` con `{ owner: 'wall' }` y la IA del diseño se cierra |
| Alt con los dos montados | El micrófono solo responde en el chat visible (regla ya existente) |

### 16.2 Arquitectura

```
#ma-stl-mode-ia ──clic──► design-ai-assistant.js · maStlDesignAiInit({ bridge, button })
                              │ panel fijo + mountWallAiChat(body, { owner: 'design', handler })
                              ▼
                 maStlDesignAiHandleMessage(sesión, texto, historial)
                   1. «sí/no» a una confirmación pendiente (borrar, «¿Querías decir…?»)
                   2. respuesta a una pregunta pendiente (largo, dirección, muro, distancia, lado…)
                   3. mejor comando del REGISTRO (≥ 0,8 se ejecuta · 0,65–0,8 «¿Querías decir…?»)
                   4. saludo / «No he entendido» + ayuda
                              │
                              ▼
       window.maStlDesing2AiBridge (master-article-details-stl-viewer.js): solo funciones reales del visor
```

| Archivo | Papel |
|---------|-------|
| `Desing/Scripts/MasterArticles/design-ai-assistant.js` (nuevo, ≈1 500 líneas) | Panel, intérprete de diseño (números, unidades, direcciones, «muro N»), **registro de comandos**, conversación (preguntas, confirmaciones) |
| `wall-ai-assistant.js` | `mountWallAiChat` acepta `handler` (intérprete propio) y `owner` (`'wall'` por defecto). `reset` solo limpia el encofrado provisional y el resaltado del muro si el owner es `'wall'`. `focus()` emite `ma-stl-ai-chat-shown` |
| `master-article-details-stl-viewer.js` | Puente `maStlDesignAiBridge` (`window.maStlDesing2AiBridge`) y arranque `maStlDesignAiInit` solo en Desing_2. El núcleo de Alargar se separa en `maStlLineEditToolExtendWallEnd(axis, extendP2)`; la herramienta hace lo mismo que antes |
| `desing2-stl-viewer-toolbar-wiring.js` | `wireIaButton`: si el botón lleva `data-ma-design-ai="1"`, no saca el aviso «próximamente» (ese aviso solo queda si el módulo no carga) |
| `_Desing2ModeToolBar.cshtml` | Título del botón «Asistente IA» y `aria-pressed` |
| `Design.csproj` | `<Content Include="Scripts\MasterArticles\design-ai-assistant.js" />` |

**Puente del visor** (`window.maStlDesing2AiBridge`). Medidas en mm de escena. En planta, «arriba en pantalla» es **−Z**, como en la herramienta Línea.

| Función | Usa en el visor | Deshacer |
|---------|-----------------|----------|
| `drawWall(a, b, { thicknessMm, heightM, groupId })` | `maStlWall2dToolCommitWallSegmentMm` (eje + 2 caras, esquinas y cortes como la herramienta Muro 2D), con el grupo restaurado como en «Insertar recinto» | 1 paso |
| `setDims(id, { lengthMm, thicknessMm, heightM })` | `maStlCommitWallAxisLengthMm`, `maStlCommitWallAxisThicknessMm`, `_DataHeight` en el eje y en sus caras (como «Modificar muro» con «solo este muro») | 1 paso para todo |
| `moveWall(id, dx, dz)` / `copyWall(id, dx, dz)` | `maStlCopyToolCommitMoveMm` / `maStlCopyToolCommitDuplicateMm` con una selección temporal (la del usuario se restaura) | 1 paso |
| `deleteWall(id)` | `maStlCommitUserFloorLinesAndStlMeshesDelete` | 1 paso |
| `extendWall(id, 'p1' \| 'p2')` | `maStlLineEditToolExtendWallEnd` (herramienta Alargar: hasta el primer muro que encuentra) | 1 paso |
| `walls()`, `findWall(id)`, `selectedWall()` | Ejes del plano (`wallRole 'axis'`): id, número, p1/p2, largo, espesor, alto, grupo | — |
| `ensureLinesMode()`, `modelMode()` | `maStlActivateDesing2ModeToolbarMode('lines')` | — |
| `defaults()` | Configuración: espesor del Muro 2D (`ThicknessHalf × 2`), altura por defecto, mín./máx. de espesor, largo mínimo | — |
| `undo()`, `redo()`, `canUndo()`, `highlight(id)`, `clearHighlight()`, `closeWallAi()`, `origin()`, `resumePoint()`, `toast()` | Las del visor | — |

Cada orden se guarda con el flujo normal del visor (`maStlSaveWallConnectionsNow`). **No se escribe en BD** desde la IA y no se crean columnas.

### 16.3 Registro de comandos: cómo añadir uno

Cada comando es un objeto del array `MA_STL_DESIGN_AI_COMMANDS`:

```js
registerDesignAiCommand({
    id: 'wall.draw',                       // único; si se repite, sustituye al anterior
    edits: true,                           // cambia el plano: antes se pasa a la vista de líneas
    params: [{ name: 'largo', required: true, help: '«5 m», «3,5»…' }],
    score: function (p) { … return 0..1; },  // p = maStlDesignAiParse(texto)
    run: function (p, s, T) { … return { reply, table?, buttons?, speech? }; },
    help: { es: 'Dibujar un muro…', en: 'Draw a wall…' },   // línea de «ayuda» (null = no sale)
    short: { es: 'dibujar un muro', en: 'draw a wall' },     // para «¿Querías decir…?»
    examples: { es: ['dibuja un muro de 5 m hacia la derecha'], en: ['draw a 5 m wall to the right'] },
});
```

Pasos para añadir uno:

1. Si hacen falta palabras nuevas, se añaden a `MA_STL_DESIGN_AI_WORDS` (ES y EN, sin tildes). Se comparan con faltas: exacta 1, fonética 0,95, un error 0,8, dos errores 0,65.
2. `score(p)` usa `p.q[concepto]` (la calidad de cada concepto), `p.measures` (con `label` largo/espesor/alto y `sign` más/menos), `p.wallRef`, `p.dirDeg`, `p.start`, `p.end`… Conviene devolver 0 si en la frase manda otro verbo (`otherVerb`).
3. `run` usa **solo el puente** (`s.bridge`), nunca funciones inventadas. Para un dato que falta se usa `ask(s, this, p, need, texto)`: la respuesta siguiente se mezcla con la orden. Para algo destructivo, `s.pending = { yes, no }` y los botones Sí/No.
4. Textos en `MA_STL_DESIGN_AI_TEXT.es/.en`.
5. Si el visor no tiene la función, **no se inventa**: el puente se amplía en el visor con funciones reales y su propio paso de Deshacer, o la orden se apunta en «Propuestas» (§16.6).
6. Hay que añadir casos a `p20/unit/test.mjs` y, si toca el visor, a `p20/site/p20-test.js`.

Desde fuera del módulo también se puede añadir: `window.registerDesignAiCommand({...})` o `window.maStlDesignAi.register({...})`.

### 16.4 Comandos (fase 1: muros 2D)

| Comando | Qué hace | Ejemplos (con faltas y dictado) |
|---------|----------|---------------------------------|
| **Dibujar muro** (`wall.draw`) | Muro 2D de un largo, en una dirección, con espesor y alto. Si no se dicen, el espesor y el alto salen de Configuración. Por defecto **sigue** al último muro de la IA o al seleccionado; si no hay ninguno, empieza en el origen | «dibuja un muro de 5 m hacia la derecha» · «otro de 3 metros girando a la izquierda» · «haz un muro de 4 m hacia arriba de 20 de espesor y 3 de alto» · «dibuja un muro de 6 m desde el origen a 45 grados» · «traza un muro de dos setenta desde 2;3 hacia la izquierda» · «dibuja un muro de 3 m desde el final del muro 4 hacia abajo» · «dibujq un muor de 4 metros asia la izquierda» · «draw a 5 m wall to the right» |
| **Cambiar medidas** (`wall.edit`) | Largo, espesor o alto, absolutos o relativos, de un muro seleccionado, por número o el último de la IA. Varias medidas en una orden cuentan como un paso | «cambia el largo del muro 4 a 6 m» · «ponle 25 de espesor» · «alto 3» · «el muro seleccionado de 3 m de alto» · «alarga el muro 2 en 50 cm» · «acorta 1 m» · «dale 2 cm más de espesor» · «change the height of wall 2 to 3 m» · «make it 3 high» |
| **Desplazar** (`wall.move`) | Mueve el muro una distancia en una dirección | «mueve el muro 3 dos metros a la derecha» · «desplaza el seleccionado 50 cm hacia arriba» · «muvee el muro 3 un metro a la derecha» · «move wall 3 two meters up» |
| **Copiar / copia en paralelo** (`wall.copy`) | Copia desplazada; «en paralelo» la mueve perpendicular al muro, hacia el lado que se diga | «copia el muro 2 tres metros hacia arriba» · «haz una copia en paralelo del muro 5 a 4 m» → «¿hacia qué lado?» → «hacia abajo» |
| **Borrar** (`wall.delete`) | Borra el muro con sus caras. **Siempre pide confirmación** (Sí/No, escrito o dictado) | «borra el muro 7» → «¿Borro el muro 7 (largo…)?» → «sí» · «elimina el seleccionado» |
| **Alargar hasta el siguiente** (`wall.extend`) | Herramienta Alargar: hasta el primer muro en la prolongación. Prueba primero por el final y luego por el inicio. «hasta el muro N» comprueba que llega a ese muro; si no llega, lo deshace | «alarga el muro 3 hasta el siguiente muro» · «prolonga el seleccionado por el inicio» · «extiende el muro 2 hasta el muro 5» |
| **Listar / señalar** (`wall.list`) | Tabla con número, largo, espesor y alto de cada muro; con número, señala ese muro | «lista los muros» · «¿cuántos muros hay?» · «señala el muro 4» · «list the walls» |
| **Deshacer / rehacer** | El Deshacer del visor (un paso por orden) | «deshaz» · «rehaz» · «undo» |
| **Ayuda** | Lista los comandos con un ejemplo de cada uno | «ayuda» · «¿qué puedes hacer?» · «help» |

**Cómo lee el texto**:

- Números en cifras («3,5», «3.5», «3'5», «3 coma 5») y en palabras («tres metros y medio», «dos setenta», «tres metros con veinte», «un metro veinte», «two meters and a half»). Dictado «3 20» = 3,20.
- Unidades m, cm, mm y °. Sin unidad: el **largo** < 100 se toma en m y si no en cm; el **espesor** < 2 en m, < 100 en cm y si no en mm; el **alto** ≤ 20 en m, < 1000 en cm y si no en mm.
- Direcciones: derecha (+X), izquierda, arriba (−Z), abajo, «a 45°», «girando a la izquierda/derecha» (respecto al muro anterior) y «recto».
- Puntos de inicio: «desde el origen» (ancla de las reglas), «desde X;Z» en metros (como la herramienta Línea) y «desde el final/inicio del muro N».

**Preguntas**: si falta un dato obligatorio, lo pregunta y la respuesta completa la orden.

- Largo: «¿De cuántos metros?».
- Dirección, cuando sigue a otro muro: «¿Hacia dónde?». Si empieza de cero, va a la derecha y lo avisa.
- Muro: «¿Qué muro?».
- Distancia: «¿Cuánto lo muevo/copio?».
- Lado de la copia en paralelo.
- Qué medida cambiar.

Una orden nueva y clara anula la pregunta. «cancelar» la anula también.

**Respuesta**: cada orden contesta con las medidas resultantes, **resalta** el muro (lo selecciona en el visor, con sus cotas) y añade el botón **Deshacer**. Si un muro dibujado cruza otros, la herramienta Muro 2D lo parte, y también parte los cruzados; la IA dice en cuántos tramos ha quedado.

### 16.5 Decisiones

- **Vista**: si el visor no está en la vista de líneas (por ejemplo, en el modelo 3D), la IA **pasa a líneas y lo dice** («He pasado a la vista de líneas (2D)…»), en lugar de negarse. Las órdenes trabajan sobre el plano 2D.
- **Número de muro** (desde el 15/11/2026, §16.12): el **ID que pinta Cotas** en el plano («ID 7»: `wallId` del eje o, si no tiene, su `id`). «muro N» busca por esa etiqueta, luego por id, luego por dbId y por último por wallId o grupo, si es único. «muéstrame los id de muros» los pinta en el plano. *Antes (hasta `20261114m`)*: el número era el `wallDbId` si lo había, que no se veía en el plano.
- **«Último muro»**: el último que ha tocado la IA en esta conversación (dibujado, cambiado, movido o señalado).
- **Deshacer**: un paso por orden. Borrar es lo único que pide confirmación antes.
- **Una sola orden por mensaje**: «dibuja 5 m a la derecha y luego 3 arriba» todavía no se encadena.

### 16.6 Lo que no existe en el visor (propuestas, no inventadas)

| Orden | Respuesta hoy | Propuesta |
|-------|---------------|-----------|
| Empalme / chaflán entre muros | «todavía no existe en el visor» | Herramienta Empalme/Chaflán para ejes de muro |
| Recortar por voz | Igual (Recortar necesita el clic en el tramo que se quita) | Núcleo de Recortar sin clic (tramo por muro y lado) |
| ~~Numeración visible en el plano~~ | **Hecho** (15/11/2026, §16.12): «muéstrame los id de muros» / «oculta los id» (Cotas con solo el ID) | — |

### 16.7 Limitaciones

- Solo muros **rectos 2D**. No hay curvos, pilares ni huecos por voz. Encofrar por voz es el botón Encofrar de la barra (todo el diseño, §16.9).
- Una orden por mensaje, sin encadenar.
- «Alargar» llega solo al **primer** muro de la prolongación, como la herramienta.
- Textos en el JS (sin `.resx`), igual que la IA del muro.
- El panel es solo del visor web. La paleta de AutoCAD no carga la barra de modos.
- Encofrado: si había encofrado y una orden lo quita (Alargar lo hace, como la herramienta), la IA lo avisa, pero **no re-encofra sola**. Re-encofrar toca todos los muros y no se hace sin preguntar.
- Voz: las mismas condiciones que en §7 (HTTPS o localhost, Chrome o Edge).
- En la maqueta no se pudo comprobar el paso automático del modelo 3D a líneas, porque necesita el servidor. Se comprueba con el puente simulado.

### 16.8 Pruebas (p20)

| Suite | Comprobaciones | Cubre |
|-------|----------------|-------|
| `p20/unit/test.mjs` (Node, puente simulado) | 62 | Números y unidades, «muro N», direcciones, etiquetas, registro, conversaciones ES/EN con faltas, preguntas, Sí/No, propuestas |
| `p20/check.sh t=all` (visor **real** en la maqueta, Chrome headless) | 56 | Toggle, formato 450 × 800, cabecera, sin backdrop, micrófono que se apaga al cerrar, Esc, coexistencia con la IA del muro, dibujar, girar, faltas, dictado, preguntas, Deshacer/rehacer en un paso, listar, cambiar medidas, mover, copia en paralelo, borrar con confirmación, alargar hasta el muro N, propuestas, ayuda, «¿Querías decir…?», inglés, conversación nueva, sin errores JS |

Capturas en `/workspace/ia/p20/shots/` (00 abrir, 01 dibujar, 02 preguntas, 03 lista, 04 borrar con confirmación, 05 ayuda y faltas, 06 inglés). Las suites anteriores (p15, p13, p12, p11, p8, p6t, p5, p3, p2) siguen en verde con el `wall-ai-assistant.js` nuevo.
