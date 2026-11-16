# IA Desing — IA del diseño: Girar/Simetría, catálogo de comandos por usuario en BD y entornos de conversación (14/11/2026)

> Parte de [`ia-desing.md`](ia-desing.md) (partido el 15/11/2026 sin quitar contenido; la numeración de secciones es la original). Área: [`INDEX.md`](INDEX.md).

### 16.9 Girar, Simetría y modos (14/11/2026, `?v=20261114k`)

Las herramientas **Girar** y **Simetría** de la barra 2D (ver `desing-2-stl-viewer-funciones.md`, «Girar y Simetría») también se pueden usar desde la IA. Los **modos** se cambian con los mismos botones de la barra.

**Comandos nuevos**

| Comando | Qué hace | Ejemplos |
|---------|----------|----------|
| **Girar** (`wall.rotate`) | Gira el muro los grados pedidos: + = antihorario en planta («a la izquierda»), − = horario («a la derecha», «menos 30»). Si no se dicen los grados, los pregunta. Con «copia» o «manteniendo el original» gira una copia | «gira el muro 4 90 grados» · «gira 45° a la izquierda» (el último o el seleccionado) · «gira el muro 2 30 grados sobre su inicio» · «… sobre su final» · «… sobre el origen» · «… sobre 2;3» · «gira el muro 4 90 grados haciendo una copia» · «gria el muor 4 noventa grados» · «rotate wall 4 90 degrees» |
| **Simetría** (`wall.mirror`) | Refleja el muro respecto a una recta. Si no se dice si se borra el original, pregunta: «¿Borro el original?» con **Sí, bórralo** / **No, mantenlo**. Si no se dice la recta, la pregunta | «haz simetría del muro 3 respecto al eje Y» → «no, mantenlo» · «simetría del muro 3 respecto al eje X borrando el original» · «haz simetría del muro 3 respecto al muro 5 y borra el original» · «haz simetría del muro 1 vertical por su extremo» · «… horizontal por su centro» · «… a 45° por su inicio» · «mirror wall 3 about the Y axis keeping the original» |
| **Modo** (`mode`) | Pulsa el botón del modo de la barra (los mismos manejadores: parejas de líneas → muro, soltar especiales vacíos, fases…) y dice el resultado. Si ya está en ese modo, lo dice y no pulsa nada. Si el visor está calculando, pide esperar | «modo líneas» · «modo linias» · «pasa a 2D» · «genera 3D» · «modo 3D» · «modo tres de» (dictado) · «encofra» · «modo encofrar» · «line mode» · «switch to lines mode» · «generate 3D» · «formwork» |

**Punto de giro (decisión)**: por defecto, el **centro del muro**. La respuesta lo dice y explica cómo cambiarlo («Por defecto giro sobre el centro del muro; di «sobre su inicio» o «sobre su final»…»). Inicio y final son los del propio tramo en el sentido en que se dibujó: si un cruce partió el muro, cuentan los extremos del tramo, no los del trazo entero.

**Recta de simetría**

| Se dice | Recta |
|---------|-------|
| «respecto al eje X» / «eje Y» | Ejes que pasan por el **origen** (ancla de las reglas): eje X horizontal, eje Y vertical en planta |
| «respecto al muro 5» | El eje del muro 5 |
| «vertical / horizontal por su extremo, inicio, final o centro» | Recta vertical u horizontal por ese punto del muro (por defecto el centro) |
| «a 45° por su centro» | Recta con ese ángulo por ese punto |

Si el muro está sobre la recta, el reflejo sería el mismo muro: lo dice y no cambia nada.

**Encofrar**: calcula el encofrado de todo el diseño y puede tardar, así que antes pregunta «Encofrar calcula el encofrado de todo el diseño (N muros)… ¿Lo hago?». Con «sí», pulsa **Encofrar** (con su selector de fase, si lo hay) y, al acabar, dice las piezas, el error del servidor o que se canceló. Con «no», no pulsa nada. Si ya está encofrado, pregunta si lo vuelve a calcular.

**Puente** (nuevo en `window.maStlDesing2AiBridge`)

| Función | Usa en el visor | Deshacer |
|---------|-----------------|----------|
| `rotateWall(id, centro, grados, { copy })` | `maStlXformToolCommitMm` (el núcleo de la herramienta Girar) con una selección temporal | 1 paso |
| `mirrorWall(id, a, b, { keep })` | El mismo núcleo con la recta a→b; `keep` por defecto `true` | 1 paso |
| `modeInfo()` | Modo como lo pintan los botones: `lines` / `wall3d` / `formwork` (clase `active` de Encofrar), `busy`, número de muros y de piezas | — |
| `setMode('lines' \| 'wall3d' \| 'formwork')` | `click()` en `#ma-stl-mode-lines` / `#ma-stl-mode-wall-3d` / `#ma-stl-mode-formwork`. Promesa que espera `desing2-wall3d-ready` o `desing2-formwork-settled`. Si se cierra el selector de fase sin elegir, no hay evento: se da por cancelado cuando el botón vuelve a estar libre y sin modal abierto | — |

**Pruebas (p21)**

| Suite | Comprobaciones | Cubre |
|-------|----------------|-------|
| `p21/unit/test.mjs` (Node, puente simulado) | 101 | Ángulos con signo, izquierda/derecha, punto de giro, copia, recta de simetría (ejes, muro, vertical por su extremo), preguntas (grados, recta, borrar), modos con faltas, dictado e inglés, Sí/No de Encofrar, «ya estás en…», «gira a la izquierda» de dibujar no se confunde |
| `p21/check.sh t=all` (visor real en la maqueta) | 86 | Barra: botones, iconos sin repetir, sin pisar el cubo de vistas a 1600, 1366, 1280 y 1024 px. Herramientas a mano: selección por ventana, Intro, base tecleada, vista previa con el ratón, ángulo tecleado y con clic, C / B, un solo Deshacer, eje canónico, atributos de extremo, aperturas de cara, lado de las caras, Intro repite, Esc. IA: todas las frases de arriba y sus Deshacer. Modos: clics reales en los botones y respuesta |
| `p21/check.sh t=all p20` | 56 | La batería p20 sigue en verde (la frase de «no existe» pasa a ser «empalme») |

Capturas en `/workspace/ia/p21/shots/`: 01 barra, 02 Girar con vista previa, 03 Simetría con vista previa, 04 IA girar, 05 IA simetría (pregunta), 06 IA Encofrar (confirmación).

**Limitaciones**

- Simetría no refleja los **objetos STL** ni los **muros curvos** (su arco no se refleja); se dejan como están (con aviso si había muros curvos en la selección). Girar sí gira los STL y los curvos.
- Las **copias** (Girar con copia, Simetría manteniendo) son como Copiar: grupos nuevos, sin aperturas de cara ni atributos de extremo. Al girar o reflejar en su sitio sí se conservan: las aperturas siguen en su cara, los atributos de cada extremo (`_Tape_1/2`, `_CornerTypeP1/P2`, `_ChekSolapeIni/End`, `_Idconnection_1/2`) siguen a su punto y el lado de las caras se mantiene.
- Si la copia girada o reflejada cruza el original, el visor parte los dos (como al dibujar). La IA nombra la copia y no un tramo del original.
- «Eje X / Y» son los ejes del **origen** de las reglas, no los del muro.
- En la maqueta no hay servidor: «genera 3D» y «encofra» pulsan el botón y dan el error del servidor. El camino con éxito está probado con el puente simulado.
- Textos de los botones y HUD nuevos en el `.cshtml` (español) y en el JS (inglés si `<html lang="en">`). No hay `.resx` nuevo, así que no hace falta recompilar.

### 16.10 Catálogo de comandos y palabras por usuario en BD (14/11/2026)

Todos los comandos de la IA (diseño, muro, comunes) y todas las herramientas del visor quedan en BD con sus palabras de activación por defecto ES/EN, para que cada usuario pueda añadir las suyas (jergas). Inventario completo: `ia-comandos-catalogo.md`. **Conectado al visor desde pt107u** (§16.11).

| Tabla | Contenido |
|-------|-----------|
| `TSql_AiCommand` | Catálogo: `TextCode` (WALL_DRAW, MODE_FORMWORK, TOOL_ROTATE…), contexto `design` / `wall` / `common`, grupo, nombre ES/EN, `TextHandler` (`ai:<id>`, `toolbar:<id>`, `click:<selector>`, `key:<tecla>`), parámetros JSON, `Is_Destructive`, `Is_Confirm`, `Is_AiImplemented` (0 = pendiente de IA) |
| `TSql_AiCommandPhrase` | Palabras por defecto: idioma, frase, frase normalizada, tipo (`keyword` activa sola · `helper` suma · `negative` resta · `example`), concepto JS y peso (IA del muro) |
| `TSql_AiCommandUser` | Solo si el usuario cambia algo de un comando: desactivarlo (`Is_Active = 0`) o reemplazar las palabras por defecto (`Is_UseDefaultPhrases = 0`) |
| `TSql_AiCommandUserPhrase` | Palabras propias del usuario, o exclusión de una por defecto (`Is_Exclude = 1`) |

Semilla: 95 comandos (25 con IA, 70 pendiente de IA) y 989 palabras/frases (594 ES, 395 EN). Scripts en `Scripts/TemporalScript/2026-11-14_*AiCommand*.sql`, ejecutados en develop y producción locales (14/11/2026); `DAL/Model.edmx` pendiente de actualizar desde Visual Studio.

**Reglas acordadas**

- **Por usuario solo se guardan cambios**: sin fila, el usuario usa los valores por defecto. «Todos mis comandos» = catálogo cruzado con sus filas.
- **Las palabras del usuario se suman** a las por defecto; en un comando concreto puede elegir que **las reemplacen**.
- **Varias palabras por comando**, sin límite (miles de jergas por idioma).
- **Idioma**: código BCP-47 en minúsculas (`es`, `en`, `es-es`, `es-ar`, `en-us`…). Una variante usa sus palabras **más** las del idioma base (`es-ar` → `es-ar` + `es`).
- **Una palabra no puede activar dos comandos** en el mismo contexto e idioma (índice único sobre la frase normalizada: minúsculas, sin tildes, sin signos). Las comunes (sí, no, cancelar) se comprueban contra cualquier contexto.
- **Si una frase contiene otra, gana la coincidencia más larga**: «herramienta girar» abre la herramienta Girar aunque contenga «girar» (orden de IA que gira el muro); «dibuja líneas» gana a «dibuja».
- Develop y producción deben tener siempre las mismas tablas y datos de este catálogo.

**Siguiente paso (propuesta)**: pantalla para que el usuario añada o quite palabras (el endpoint ya existe, §16.11).

El esquema completo de develop y producción se igualó el 14/11/2026. Qué se añadió, qué falta por decidir y qué hay que limpiar en el futuro: [`base-de-datos-alineacion.md`](../base-de-datos/base-de-datos-alineacion.md).

### 16.11 Entornos de conversación y catálogo conectado al visor (14/11/2026, `?v=20261114m`)

El usuario puede hablar con la IA en distintos **entornos** (tono y jerga). Por defecto, **Ejecutivo**.

| Entorno | Tono | Ejemplo («dibuja un muro de 5 m hacia la derecha») |
|---------|------|------------------------------------------------------|
| `EXECUTIVE` Ejecutivo (por defecto) | Usted, conciso | «Dibujado el muro 4: 5,00 m hacia la derecha, espesor 0,30 m…» |
| `COLLOQUIAL` Coloquial | Tuteo cercano | «¡Hecho! Te he dibujado el muro 4: …» |
| `STREET` Colega barriobajero | Jerga de obra, respetuosa | «¡Ahí lo llevas, tío! El muro 4 ya está: …» |
| `TECHNICAL` Técnico | Telegráfico | «Muro 4 creado. L=5,00 m; dir. hacia la derecha; e=0,30 m; h=2,70 m; …» |

**Tablas nuevas** (scripts `Scripts/TemporalScript/2026-11-14_*AiEnvironment*`, `*AiUserSettings*`, `alter_TSql_AiCommandPhrase_LinkEnvironment`; ejecutadas en develop y producción locales, idénticas):

| Tabla | Contenido |
|-------|-----------|
| `TSql_AiEnvironment` | Entornos: código, nombre ES/EN, descripción, `Is_Default` (solo uno: índice único filtrado) |
| `TSql_AiEnvironmentReply` | Plantillas de respuesta por entorno + idioma + clave (`design.drawn`, `wall.thanks`…; huecos `{0}`, `{1}` como en el JS) |
| `TSql_AiUserSettings` | Una fila por usuario: entorno e idioma preferido (sin fila = Ejecutivo e idioma de la interfaz) |
| `LinkEnvironment` en `TSql_AiCommandPhrase` / `TSql_AiCommandUserPhrase` | NULL = frase de todos los entornos; con valor = jerga solo de ese entorno («méteme un muro, tío» en STREET) |

**Respuesta** = plantilla del entorno → la de Ejecutivo → texto del JS. Coloquial y Colega llevan el texto JS (tuteo) en las claves que Ejecutivo pasa a «usted», para no mezclar tratamientos; Técnico hereda Ejecutivo.

**Endpoints** (`Controllers/Desing_2Controller.AiCommands.cs`, clase `partial`; SQL directo con `db.Database.SqlQuery`, sin entidades de `Model.edmx`, para que funcione aunque el modelo aún no tenga las tablas):

- `GET Desing_2/AiCommands?context=design|wall&lang=es&env=&v=` → `{ ok, version, context, lang, environment, environments[], commands[{ code, context, group, nameEs, nameEn, handler, aiId, confirm, destructive, implemented, useDefaults, excluded?, phrases[{ p, n, k, c, w, l, e?, u? }] }], replies{ es, en } }`. Comandos del contexto + comunes; frases efectivas del usuario (por defecto si no las sustituye − excluidas + propias; sin entorno + las del entorno activo; idiomas base + la variante pedida o la preferida del usuario). `version` = SHA-1 del contenido (también `ETag`); si `v` o `If-None-Match` coinciden → `{ ok, notModified }`. Sin tablas → `{ ok:false, missing:true }`.
- `POST Desing_2/SaveAiUserSettings` `{ env, lang? }` → guarda el entorno (y el idioma si viene) en `TSql_AiUserSettings`.

**JS** (`Scripts/MasterArticles/ai-command-catalog.js`, sin dependencias):

- La IA del diseño carga el catálogo **al abrir el panel**; la del muro, al mostrarse su chat. Caché en `localStorage` (`ma-stl-ai-catalog:v1:<contexto>:<idioma>`) con la versión; sin servidor, última copia buena; sin nada, **vocabulario JS** (como antes).
- Diseño: cada concepto = palabras JS + las del catálogo − las excluidas por el usuario (solo las suyas si sustituyó las de por defecto). Las frases de varias palabras se buscan antes: **gana la más larga** y sus palabras dejan de contar para otras órdenes («vista en planta» → modo líneas; «levanta un muro» → dibujar). En frases, cada palabra debe coincidir casi exacta (sin tildes o con enclítico; «gira el muro» no es «tira el muro»).
- Muro: vocabulario por intención (materiales / sustituir / ayuda / gracias) con los pesos de la BD; frases largas suman su peso; sí / no = JS + comunes de la BD («de acuerdo», «ni hablar», «go ahead»).
- Textos: los de la BD (claves `design.*` / `wall.*`) sobre los del JS.
- **Cambiar de entorno hablando**: «modo coloquial», «háblame de tú», «habla como un colega», «modo técnico», «ponte serio», «be formal»… (comandos `ENV_*` de la BD; sin catálogo, las mismas frases en JS). Se guarda en BD y la confirmación ya sale con la voz del entorno nuevo. La cabecera del panel muestra el entorno activo.
- Comandos del catálogo «pendientes de IA» (herramientas de la barra): la IA dice cuál es y que se use el botón.
- `?v=20261114m` en `master-article-details-stl-viewer.js`, `design-ai-assistant.js`, `wall-ai-assistant.js`, `wall-edit-form.js`, `WallForm.cshtml` y `ai-command-catalog.js` (todos los imports con la misma versión).

**Cómo probar**: recompilar en VS2022, abrir un diseño, botón IA → la cabecera muestra «Ejecutivo»; «dibuja un muro de 5 m hacia la derecha»; «modo colega» y repetir; `GET /Desing_2/AiCommands?context=design&lang=es` en el navegador (con sesión) devuelve el JSON.

**Limitaciones**: sin pantalla de gestión de palabras (se añaden en BD); sin catálogo los textos son los del JS (tuteo); las etiquetas fijas del chat (micrófono, altavoz, botones) no cambian con el entorno; un comando sustituido por el usuario sin frases propias no se detecta como «sustituido» en el diseño; `DAL/Model.edmx` sin las 7 tablas `TSql_Ai*` (no hace falta para el endpoint).
