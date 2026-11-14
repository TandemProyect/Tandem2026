# IA Desing

Documento único del **Asistente IA del muro** de Desing_2 (visor STL y página del plugin de AutoCAD): qué hace, cómo está hecho, qué queda abierto y cómo probarlo. Estado a **14/11/2026**, imports `?v=20261114h`.

El detalle de cada pantalla está en [`desing-2-stl-viewer-funciones.md`](desing-2-stl-viewer-funciones.md) (sección «Asistente IA en la edición de muro») y el del cálculo de encofrado en [`encofrado-atk-60.md`](encofrado-atk-60.md) (§9). Aquí se resume y se enlaza; no se repite.

## Índice

1. [Objetivo y filosofía](#1-objetivo-y-filosofía)
2. [Arquitectura y mapa de código](#2-arquitectura-y-mapa-de-código)
3. [El intérprete](#3-el-intérprete)
4. [Órdenes que entiende (con ejemplos)](#4-órdenes-que-entiende-con-ejemplos)
5. [Servidor (C#)](#5-servidor-c)
6. [Modo compacto 450 × 800](#6-modo-compacto-450--800)
7. [Micrófono](#7-micrófono)
8. [Respuestas habladas](#8-respuestas-habladas)
9. [Persistencia actual](#9-persistencia-actual)
10. [Estudio: muro especial con piezas propias](#10-estudio-muro-especial-con-piezas-propias)
11. [Columnas de BD propuestas (no creadas)](#11-columnas-de-bd-propuestas-no-creadas)
12. [Limitaciones conocidas](#12-limitaciones-conocidas)
13. [Preguntas abiertas](#13-preguntas-abiertas)
14. [Historial de decisiones](#14-historial-de-decisiones)
15. [Cómo probar](#15-cómo-probar)

---

## 1. Objetivo y filosofía

- **Para qué**: que el usuario, con el formulario de un muro abierto, pida en lenguaje natural (escrito o hablado) el material de encofrado de **ese muro** y adapte el encofrado a lo que tiene en obra («no tengo paneles de 0,90», «de 13 paneles de 2,70×0,90 solo tengo 10»), viendo el resultado en el visor antes de aceptarlo.
- **Intérprete local, coste 0**: no hay modelo de lenguaje ni API de pago. Todo el entendimiento está en JS (`wall-ai-assistant.js`): normalización, abreviaturas, fonética, distancia de edición y puntuación por intención.
- **Sin servicios externos**, salvo uno: el **reconocimiento de voz del navegador** (Web Speech API; en Chrome/Edge el audio va a Google/Microsoft). La lectura en voz alta usa `speechSynthesis` con las voces del sistema, preferentemente locales.
- **No inventar**: lo que no entiende lo pregunta (p. ej. «del 19»); si el catálogo no responde, enseña solo códigos y cantidades.
- **No romper nada**: vista previa antes de cambiar, «Sí/No» antes de aplicar, «Deshacer» después; solo cambia el muro editado; no escribe en BD. En el servidor, todo es **aditivo** (sin los atributos nuevos la salida es byte a byte la de antes).
- **Español e inglés**: responde en el idioma del mensaje.

## 2. Arquitectura y mapa de código

```
Usuario (texto / voz)
   │
   ▼
mountWallAiChat (chat común: burbujas, micrófono, altavoz, botones, tablas)
   │  send() ──► window.maStlWallAiHandleMessage(wallCtx, text, history)   ← gancho
   │                 ├─ intérprete local (maStlWallAiAnalyze …)
   │                 └─ ctx.formwork (lo da el visor): piezas, previewWall, applyWall, resaltado…
   ▼                                   │
pushAssistant(res) ◄───────────────────┘   res = { reply, speech?, table?, buttons?, pending?, confirm?, swap? }
   │
   ├─ pinta la respuesta (tabla con Total y Copiar; botones con run())
   └─ speak(res) si el altavoz está encendido
```

| Archivo | Papel |
|---------|-------|
| `Desing/Scripts/MasterArticles/wall-ai-assistant.js` (≈4 250 líneas) | Todo el asistente: textos ES/EN, intérprete, números, material del muro, `_PanelStock`, cambio guiado, voz, altavoz, modo compacto, `mountWallAiChat` |
| `Desing/Scripts/MasterArticles/wall-edit-form.js` | Formulario «Modificar muro» (recto): pestaña «Asistente IA», `enterAiCompact` / `exitAiCompact`, `resetAi`, corte de voz al cambiar de pestaña |
| `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js` | Visor: contexto del muro recto (`maStlWallAiBuildStraightContext`) y curvo (`maStlCurvedWallAiContext`, `maStlCurvedWallEnsureAiTab`), acceso al encofrado (`maStlWallAiFormworkAccess`), encofrado provisional (`maStlWallAiShowTempFormwork` / `maStlWallAiClearTempFormwork`), resaltado (`maStlWallAiHighlightPanels` / `maStlWallAiClearPanelHighlight`) |
| `Desing/Scripts/Desing2/desing2-stl-viewer-toolbar-wiring.js` | `window.maStlDesing2PreviewWallFormworkAtk60(keys, axisMm, { attrs })`: cálculo de **un solo muro** con vecinos a 2 saltos y `Preview=true` (ver `encofrado-atk-60.md` §9.1) |
| `Desing/Views/DesignToolsAutocad/WallForm.cshtml` | Página del plugin de AutoCAD: carga `wall-edit-form.js?v=…` (mismo formulario y chat) |

**Gancho**: `maStlWallAiHandleMessage(wallCtx, text, history)` se publica en `window` solo si no existe otro; asignar `window.maStlWallAiHandleMessage` antes permite sustituir el intérprete sin tocar el chat. Devuelve `{ reply, actions: [] }` o una Promise; extras: `speech` (lo que se lee), `table`, `buttons` (`{ label, primary?, run() }`), `pending` (texto mientras trabaja), `confirm` + `apply`/`cancel` (para «sí/no» escrito o dictado), `swap.handle` (siguiente paso del cambio guiado), `lang`.

**Cableado recto / curvo**

| | Muro recto | Muro curvo |
|-|------------|------------|
| Dónde | Popup flotante «Modificar muro» (`mountWallEditForm`), quinta pestaña | Modal `#ma-stl-curved-wall-modal`, tercera pestaña creada por JS |
| Contexto | `maStlWallAiBuildStraightContext()` → `kind 'straight'`, clave, L, e, h, tapes, `formwork` | `maStlCurvedWallAiContext()` → `kind 'curved'`/`'curved-new'`, Ø, e, h, modulación, `formwork` |
| Abrir / cerrar | `maStlWallInspectOpenPopup` → `resetAi(clave)`; `maStlWallInspectClosePopup` → `resetAi(null)` | `hidden.bs.modal` → `reset(null)` |
| Compacto | `enterAiCompact` (solo popup flotante) | `maStlWallAiBootstrapCompact(modalEl, …)` |
| Plugin AutoCAD | `WallForm.cshtml` (contexto del modelo recibido; sin compacto) | — |

**`ctx.formwork`** (visor): `hasAny()`, `elements()`, `anchors()`, `countRows(filter, list)`, `isEdited(id)`, `previewWall({ attrs })`, `applyWall(idWall, items, attrs)` (devuelve lo anterior para deshacer), `wallAttr(key)`, `runFormwork()`, `showTempWall`, `clearTempWall`, `tempWallInfo`, `highlightPanels`, `clearHighlight`.

**Versión actual**: `?v=20261114h` en los imports de `wall-edit-form.js`, `wall-ai-assistant.js` y `WallForm.cshtml`. El visor se versiona por la fecha del archivo (`Viewer.cshtml`). Cada cambio de JS sube la letra.

## 3. El intérprete

Cadena: **idioma → normalizar → abreviaturas → coincidencia por palabra → puntuación por intención → confianza**.

| Paso | Función | Regla |
|------|---------|-------|
| Idioma | `maStlWallAiDetectLanguage(text, fallback)` | Palabras propias de cada idioma 1 punto; con faltas 0,6; tildes/ñ/¿¡ +2 al español; contracciones inglesas +2 al inglés; compartidas (material, panel, ok, kilos…) no cuentan. Empate → idioma de la conversación, y si no, el de la interfaz (`maStlWallAiUiLang`: `<html lang>` / cookies `tandem_lang`, `tandem_ui_culture`) |
| Normalizar | `maStlWallAiNormalize` | Minúsculas, sin tildes ni signos, espacios simples; opción de colapsar letras repetidas («MATERIALESSS!!») |
| Abreviaturas | `maStlWallAiTokens` | ES: q/k→que, pa/xa→para, xq/pq→porque, d→de, deste→de este, mat/mats→material(es), tng→tengo, nec→necesito, porfa/xfa→por favor, ok→vale… EN: u→you, pls→please, thx→thanks, wat→what, gimme→give me, mats→materials, pcs→pieces, yep→yes… |
| Fonética | `maStlWallAiPhonetic` | c(e,i)/z/s→s · c(a,o,u)/qu/k→k · b/v/w→b · ll/y→y · h muda · g(e,i)/j→j · gu(e,i)→g. «nesesito»=«necesito», «yeva»=«lleva», «piesas»=«piezas» |
| Difuso | `maStlWallAiDamerau` + `maStlWallAiFuzzyLimit` | Damerau-Levenshtein (OSA, transposición = 1) sobre la palabra y su clave fonética. Errores admitidos según la palabra del vocabulario: ≤3 letras 0 · 4–8 letras 1 · >8 letras 2. Palabras escritas de ≤3 letras solo exactas o fonéticas |
| Calidad | `maStlWallAiWordMatch` | Exacta 1 · fonética 0,95 · 1 error 0,8 · 2 errores 0,65. Pronombres pegados («sustitúyelos», «encofrarlo», «dámelo») se prueban sin el pronombre |
| Puntuación | `MA_STL_WALL_AI_VOCAB`, `maStlWallAiAnalyze` | Intenciones `materials`, `replace`, `help`, `thanks`, cada una con conceptos y pesos (material/despiece 3, piezas 1,8, necesito 1,5, peso 1,5, lista 1, encofrar 1, cuánto 0,8, dame 0,6, muro 0,3…; **borra/mueve/gira/copia/crea/deshaz −3**). Cada palabra cuenta solo para su mejor concepto |
| Confianza | `MA_STL_WALL_AI_SCORE = { high: 2, mid: 1, margin: 0,6 }` | **Alta**: ≥2 y 0,6 por encima de la segunda → responde. **Media**: ≥1 → en «material» pregunta **«¿Querías decir…?» / «Did you mean…?»** con Sí/No (vale contestar «sí», «vale», «ok», «no», también dictado: `maStlWallAiYesNo`). **Baja** → «No he entendido «…»» + ayuda |
| Números | `maStlWallAiParseMeasure`, `maStlWallAiParseNumberWords` | Medidas como se dicen en obra, ES y EN: «0,90» «0.90» «0'90» «,90» «90» (entero ≥10 = cm) «noventa» «cero noventa» «point nine» → 0,90 · «uno veinte» «un metro veinte» «1,2» «120» «ciento veinte» «one twenty» → 1,20 · «medio metro» · unidades mm/cm/m. Enteros en palabras 0…999 999, tolerante a faltas («nobenta», «ninty») |
| Saludo | `maStlWallAiGreeting(date, lang)` | Hora local del navegador: 06:00–13:59 «Buenos días» / Good morning · 14:00–20:59 «Buenas tardes» / Good afternoon · 21:00–05:59 «Buenas noches» / Good evening. Es el único mensaje inicial |

**Orden de decisión** en `maStlWallAiHandleMessage`: (1) respuesta corta a una confirmación pendiente; (2) paso siguiente de un cambio guiado en curso; (3) orden de cambio guiado (muro recto, un solo ancho del catálogo); (4) orden de stock (`maStlWallAiParseStockOrder`) o intención `replace`; (5) `materials` (media → «¿Querías decir…?»; alta → lista); (6) `help` / `thanks`; (7) desconocida → ayuda citando el texto.

Detalle completo y frases probadas: `desing-2-stl-viewer-funciones.md`, «Fase 2» y «Español e inglés».

## 4. Órdenes que entiende (con ejemplos)

### 4.1 Recuento de material del muro

«dame el material de encofrado de este muro», «lista de piezas», «despiece», «¿qué necesito para encofrar este muro?», «cuántos paneles lleva», «cuánto pesa», «give me the formwork material for this wall»; con faltas: «dame el matrial de encofado deste muro», «q nesesito pa encofrar», «kuanto pesa».

- **Si el muro tiene piezas pintadas** (último «Encofrar»): usa esas piezas, filtradas por `IdWall`.
- **Si no**: `previewWall()` calcula **solo ese muro** (vecinos a 2 saltos como contexto de esquinas, `Preview=true`) y **dibuja en el visor el encofrado provisional de solo ese muro**: grupo propio `maStlWallAiTempFormwork`, fuera del grupo ATK-60, no se guarda, no se selecciona, no cuenta como encofrado. Se quita al pedir otro recuento, cambiar de muro, cerrar, pulsar «Encofrar», limpiar el encofrado o aplicar cambios.
- **Respuesta**: título con el muro, resumen (referencias · piezas · kg) y tabla **Código · Descripción · Cant. · Peso** con fila Total y botón **Copiar** (TSV para Excel). Mismas reglas de código y mismo endpoint que el popup «Cantidad de material» (`Desing_2/PreviewMaterialCount`).
- Si el cálculo del muro falla: botón **Calcular / Recalcular encofrado** (todo el diseño, como el botón «Encofrar»).

Detalle: `funciones.md` «Recuento = encofrar solo el muro editado» y `encofrado-atk-60.md` §9.1.

### 4.2 Stock en obra: excluir o limitar anchos (`_PanelStock`)

«no tengo paneles de 0,90, sustitúyelos», «solo tengo 3 paneles de 0,90», «solo tengo 3 de 0,90 y 2 de 0,75», «no tengo de 0,9 ni de 0,75», «quita los de 45», «I only have twenty two panels of 0.60».

- Regla del muro `{ excludeMm: [...], maxMm: { ancho: n } }` (se fusiona con la que ya tenga: `maStlWallAiMergeStock`).
- **ANTES / DESPUÉS**: paneles por ancho, piezas, kg y tabla **Código · Descripción · Antes · Después · Dif.**; avisos (tramo sin combinación y compensación del manual, tapes que no cambian, uso frente a límite, ancho que el muro no usa).
- **Sí, aplicar / No** → `applyWall` re-encofra en la escena solo ese muro → botón **Deshacer** (un uso).
- En el **muro curvo** (un solo ancho) propone la modulación válida más cercana y, al aplicar, cambia el desplegable de modulación (se guarda con Aceptar).
- En el muro recto, desde el 14/11 una orden de **un solo ancho** va al cambio guiado (4.3); `_PanelStock` sigue para varios anchos, para el curvo y con el botón «Recolocar el módulo entero (automático)».

Detalle: `funciones.md` «Fase 3» y `encofrado-atk-60.md` §9.2–9.3.

### 4.3 Cambio guiado de paneles (`_PanelReplace`)

«de 13 paneles de atk60 de 2,70 x 90 solo tengo 10», «me faltan 3 de 0,90», «no tengo de 0,90», «I only have 10 panels of 2.70 by 0.90».

1. **Recuento real**: «En este muro hay 13 paneles de 2,70×0,90: 6 parejas enfrentadas y 1 suelto en una esquina o remate. Tienes 10, así que te faltan 3.» Si el usuario dice otro total, se le avisa. Si le llega, no cambia nada. Varias alturas del mismo ancho → pregunta cuál.
2. **Por parejas** (el tirante ata las dos caras): faltan 3 → 2 parejas = 4 paneles.
3. **¿Todos de una vez o uno a uno?** (botones, texto o voz: «todos», «de una vez», «uno a uno», «one by one»; «cancelar»/«no» cancela).
   - **Todos**: parejas desde el final del muro hacia el inicio, listadas («nº 6 desde el inicio del muro, de 4,50 a 5,40 m, en las dos caras») y **resaltadas en naranja**.
   - **Uno a uno**: «Panel 1 de 2: el nº 6…», resaltado; «otro panel» lista las parejas libres («el 3»).
4. **Opciones válidas** (`maStlWallAiSwapCombos`): anchos del catálogo (0,30–0,90 de 15 en 15 cm) que suman el ancho del panel, 2 o 3 piezas, mismo alto, nunca el ancho que falta, respetando exclusiones y máximos de `_PanelStock`; menos piezas primero y, a igualdad, el más ancho. 0,90 → 1) 0,60 + 0,30 · 2) 0,45 + 0,45 · 3) 0,30 + 0,30 + 0,30. 0,45 y 0,30 no tienen opción (se dice y se ofrece «Recolocar el módulo entero»). Elección por botón, texto o voz: «la 2», «la segunda», «dos de 0,45», «0,60 + 0,30», «una de 60 y otra de 30», «number 2» (`maStlWallAiParseSwapChoice`).
5. **Vista previa** con `_PanelReplace` → ANTES / DESPUÉS + resumen («Paneles de 2,70×0,90: 13 → 9 (usas 9 de tus 10)») y Sí/No. Comprueba en la respuesta que el servidor ha cambiado cada panel; si no ha cambiado ninguno, avisa de que falta recompilar.
6. **Aplicar** = `applyWall(idWall, piezas, { _PanelReplace })`: solo cambian las piezas de ese muro (paneles, grapas, tirantes, rigidizadores, puntales). **Deshacer** deja piezas y atributo como estaban.

No se ofrecen paneles de esquina L, cara E, tapes ni remates (una vista previa de sonda detecta los que el servidor no cambia). Detalle: `funciones.md` «Cambio guiado de paneles» y `encofrado-atk-60.md` §9.4.

### 4.4 Otras

- **Ayuda / saludo**: «ayuda», «hola», «q sabes acer», «help», «what can you do» → lista de órdenes.
- **Gracias**: «gracias», «thx» → «De nada» / «You're welcome».
- **Lo que no es suyo** («borra el muro», «gira el muro», «undo»): no se ejecuta; «No he entendido…» + ayuda.
- **Lo que no entiende dentro de una orden** («solo tengo 3 de 0,90 **del 19**»): «Entiendo: como mucho 3 paneles de 0,90. Pero no sé qué quiere decir «del 19»…» con botón «Seguir sin «del 19»». No se interpreta.

## 5. Servidor (C#)

Todo **aditivo**: sin los atributos nuevos el resultado es idéntico al de antes. **Hace falta recompilar y publicar Desing** para `Preview`, `_PanelStock` y `_PanelReplace`. Con el JS nuevo y el servidor antiguo: el recuento funciona (con aviso), los cambios de paneles responden que el servidor aún no sabe hacerlo y no cambian nada.

| Pieza | Dónde | Qué |
|-------|-------|-----|
| `Preview` | `Desing2FormworkIdsRequestDto.Preview` | Con `true` no escribe el JSON de depuración del último Encofrar (`SaveAtk60RequestDebugToTemp`); la respuesta añade `Preview` y `PanelStockNotes` |
| `_PanelStock` | `Repositories/Repository_Desing_2/Atk60/PanelStock.cs` (`Atk60PanelStock`) | Atributo del muro en `IdsJson → Attributes`. Tramo recto: `Modulo270HeightPanelCatalog.ResolveForModule → ReshapeWithStock`; también esquina L / cara E / hueco en `LCornerFormwork`. No tapes, tablones ni curvo. Sin combinación → módulo original y nota `sin-combinacion:<L>` |
| `_PanelReplace` | `Repositories/Repository_Desing_2/Atk60/PanelReplace.cs` (`Atk60PanelReplace`) + `Modulo270HeightPanelCatalog.ResizePanelPiece` (`Repositories/RepositoryAtk60/ModulosATK60/`) | Lista de reglas `{ alongMm, upMm, pieceWidthMm, pieceHeightMm, orientation, useMm }` (±2 mm). Enganche en `Atk60ModuleAssembly.Build` (tramo recto principal, `moduleLayout = replace.Apply(layout, moduleBaseAlongMm)`); grapas, rigidizadores, tirantes y puntales salen del módulo nuevo. Con `_PanelStock` a la vez: primero el stock, luego el cambio |
| `Design.csproj` | — | `<Compile Include>` de `PanelStock.cs` y `PanelReplace.cs` |

**Regresión**: 74 casos rectos + 13 curvos, salida **byte a byte idéntica** sin los atributos (`/workspace/ia/p13/regress.sh`). Sin DTO ni controlador nuevos para `_PanelReplace`.

## 6. Modo compacto 450 × 800

Al elegir la pestaña «Asistente IA» (recto y curvo), el formulario pasa a un **panel anclado a la derecha del visor**, **450 × 800 px** con 12 px de margen (`MA_STL_WALL_AI_COMPACT`; si la ventana es más baja, el alto disponible, mínimo 260).

- **Sin backdrop ni bloqueo**: el visor 2D/3D sigue respondiendo a clics, arrastre y rueda; los clics fuera no cierran.
- Solo conversación y entrada; cabecera «Asistente IA – Muro 12» / «AI Assistant – Wall 12» y el icono **Volver al formulario** (`ri-arrow-go-back-line`), que restaura tamaño, posición y pestaña.
- La conversación se conserva al cambiar de modo; abrir otro muro con el compacto abierto se queda en la conversación.
- Recto: `enterAiCompact`, `exitAiCompact`, `refreshAiCompact` (`wall-edit-form.js`, solo popup flotante; la página del plugin no cambia). Curvo: `maStlWallAiBootstrapCompact` (sin backdrop, `pointer-events: none` en `.modal`, trampa de foco desactivada).

Detalle: `funciones.md` «Modo compacto».

## 7. Micrófono

- **Icono** entre la caja y los botones (`ri-mic-line`; escuchando `ri-mic-fill`, gris 70% `#4d4d4d` con pulso). **Tecla Alt sola** (pulsar y soltar, sin otra tecla) con la pestaña IA visible, también con el foco en la caja; Alt+Tab, Alt+F4, Alt+letra, Mayús+Alt y AltGr no hacen nada (`maStlWallAiIsBareAltKey`).
- **Modo continuo**: no se apaga al terminar una orden. Cada frase final se envía sola por el mismo camino que Enviar y sigue escuchando; tras el corte de Chrome (~60 s o silencio) se reinicia solo. Sin duplicados; si la IA está contestando, las órdenes esperan en cola.
- **Indicador**: franja «Escuchando… habla ahora» / «Listening… speak now» con barras del **volumen real** (`getUserMedia` + `AnalyserNode`); texto parcial en la caja.
- **Idioma**: `es-ES`, o `en-US` si el último mensaje del usuario es inglés o la interfaz lo es (`maStlWallAiVoiceLang`).
- **Errores** (`maStlWallAiVoiceErrorPolicy`): sin permiso / sin micrófono → aviso y se apaga; `no-speech` / `aborted` → reinicio en silencio; `network` → reintenta (1 s, 2 s) y a los 3 fallos se apaga; antibucle (>6 reinicios en 4 s sin oír nada). Sin soporte (Firefox, sin HTTPS/localhost) → icono deshabilitado con explicación.
- **Solo se apaga** con el icono, con Alt, al salir de la pestaña IA (incluido volver del compacto), al cambiar de muro, al cerrar el formulario o por error grave. Al apagar libera el micrófono y el `AudioContext`.

Detalle: `funciones.md` «Micrófono».

## 8. Respuestas habladas

- **Altavoz** a la derecha del micrófono (`ri-volume-up-line` / `ri-volume-mute-line`). **Encendido por defecto**, recordado en `localStorage` (`maStlWallAi.speak`).
- **Voz** (`maStlWallAiPickVoice`, «la más eficiente»): mismo idioma obligatorio; región exacta 100, otra región del idioma 50; **local +30** (voces de Windows, sin red, empiezan antes), natural/online +8, Google +6, Microsoft +4, nombres conocidos +3, predeterminada +1. Sin voz del idioma → icono deshabilitado con cómo instalarla.
- **Números** (`maStlWallAiSpeakable`, `maStlWallAiSayNumber`): medidas de panel en **centímetros** («0,90» → «90», «2,70×0,90» → «270 por 90», «0,60 + 0,30» → «60 más 30»); otros decimales «X **punto** Y» («90,45» → «90 punto 45», «3,60 m» → «3 punto 60 metros»); decimales cero solo la parte entera («90.00» → «90», «1.667,0 kg» → «1667 kilos»). En inglés «point».
- **Qué lee** (`maStlWallAiSpeechText`): lenguaje natural, **nunca tablas ni códigos**. Recuento: «Este muro lleva N piezas de M referencias; te dejo la lista en pantalla.» Cambio guiado: resumen corto + «¿Lo hago?». Opciones en corto (máx. 4). Listas → «Te dejo el detalle en pantalla.» No lee las medidas del título del muro, «Calculando…» ni la franja del micrófono. Saludo: una vez por conversación al mostrarse la pestaña (no se intenta sin interacción previa y no se reintenta tras `not-allowed`). Avisos del micrófono: sí.
- **Trozos** de ≤180 caracteres (`maStlWallAiSpeechChunks`) por el corte de Chrome en frases largas.
- **Pausa del micrófono**: mientras habla, el reconocimiento se para («Te estoy contestando… (micrófono en pausa)») y no se escucha a sí misma; vuelve 300 ms después. Pulsar el micrófono, Alt, Enviar o un botón corta la voz.

Detalle: `funciones.md` «Respuestas habladas».

## 9. Persistencia actual

- **Conversación**: por muro y en memoria; se vacía al cerrar o al abrir otro muro.
- **`_PanelStock` y `_PanelReplace`**: en el `userData` del muro (eje y sólido 3D) **solo durante la sesión**; un «Encofrar» posterior los respeta (van en `IdsJson`). **No se guardan en BD** (no hay columna; ver §11). No cambian `_IsFormwork` ni marcan el muro como especial.
- **Encofrado provisional y resaltado**: solo pantalla; nunca se guardan.
- **Preferencia de altavoz**: `localStorage`.
- **Por qué no se usa `Is_Formwork` para el recuento de un muro**: `_IsFormwork = false` (BD `TSql_DesignWall.Is_Formwork`) significa **muro especial** (no se encofra). Ponérselo a los demás muros sería peligroso: `maStlDesing2ReleaseEmptySpecialWalls` vuelve a `true` los especiales sin piezas manuales **y guarda el diseño en el servidor** al pulsar «Encofrar» o pasar a 3D, y el guardado normal también lo persistiría; además, quitar los vecinos cambiaría las esquinas. Por eso «solo este muro» se aplica sobre **una copia de la petición** de la vista previa, sin tocar `_IsFormwork` en memoria ni en BD.

## 10. Estudio: muro especial con piezas propias

Estudio (14/11/2026, **solo análisis**, sin código ni escritura en BD) para que el asistente pueda dejar un muro con piezas propias guardadas en BD.

**Modelo de datos**

- `TSql_DesignWall.Is_Special` (bit, not null, por defecto 0), ligado a `Is_Formwork` (bit, por defecto 1): un muro especial siempre tiene `Is_Formwork = 0` (`DesignWallRepository.cs`).
- `TSql_DesignWallArticle`: una fila por pieza (no hay columna de cantidad: el recuento es contar filas). FKs `LinkDesign_V2` → `TSql_Design_V2`, `LinkDesignWall` → `TSql_DesignWall`, `LinkMasterArticle` → `Tsql_Master_Articles` (nullable). Columnas: `TextCode` (50), `TextView` (20, por defecto '3D'), `NumberSequence`, `NumberInsertX/Y/Z` (mm), `NumberX/Y/ZRotation` (grados), `TextHandleCad`, `NumberPhase`, auditoría (`Is_Delete`, `Is_Active`, `AddDateMade` = `getdate()`).
- **Bases de datos**: la local de desarrollo `db_a197cd_desingdevelop` **no tiene** `TSql_DesignWallArticle` ni `Is_Special`; la copia local `db_a197cd_desingproducction` sí. Datos vistos: 516 muros, 8 especiales (solo 1 vivo: diseño 2, muro 161, 1 pieza); 1 238 piezas en 4 diseños; las filas del encofrado automático (p. ej. diseño 30009, 121 filas) están en la misma tabla con el muro no especial. Consultas solo `SELECT` en `.\SQLEXPRESS`.

**Tandas y purga**

- Las piezas se guardan por **tandas por diseño** (no por muro): `QueryLatestActive` devuelve solo la tanda con el `AddDateMade` más reciente (ventana de ±50 ms).
- `ReplaceAll` (guardado del visor y del plugin, `DesignToolsAutocadController`) escribe una tanda nueva de **todo el diseño**: usarlo desde el asistente con un solo muro ocultaría las piezas del resto.
- **Purga diaria**: el primer usuario a partir de las **02:00** la arranca (`TryBeginDailyPurge`), borra en lotes de 2 000 todo lo que no es la última tanda de cada diseño. **Irreversible**.
- `ReplaceArticlesOnSpecialWalls(designId, articles, userId, trustWallIds)` (`DesignWallArticleRepository`): borra (lógico) las piezas de los muros especiales y escribe las nuevas **con la fecha de la tanda visible**, para no tapar el encofrado generado; con `trustWallIds` usa `WallDbId` y marca ese muro `Is_Special = true`, `Is_Formwork = false`. Sin `WallDbId` busca o crea el eje por P1/P2.
- Riesgo: `ResolveExistingWall` puede caer al primer muro (muro equivocado). La ruta `ArticlesOnly` no la usa el JS. El popup «Cantidad de material» no suma las piezas manuales. Los plugins de AutoCAD y ZWCAD se comportan igual.

**Propuesta por fases** (no implementada)

1. Guardar por `ReplaceArticlesOnSpecialWalls` filtrado al muro, con `trustWallIds = true` (el muro queda especial). Necesita un endpoint nuevo o un modo en `SaveDesignWalls`.
2. Cambiar piezas de un muro ya especial (la sustitución de `_PanelStock` / `_PanelReplace`) reescribiendo solo las de ese muro.
3. Esquinas compartidas y recálculo de vecinos.

**Riesgos**: tandas y purga; sin deshacer en BD tras la purga; formato de posición/rotación de las piezas; vuelta automática a «no especial» (`maStlDesing2ReleaseEmptySpecialWalls`); esquinas y vecinos; curvos y accesorios (una fila por unidad); muro equivocado.

## 11. Columnas de BD propuestas (no creadas)

`TSql_DesignWall` no tiene columna JSON libre. Propuestas, **pendientes de aprobación** (no se ha creado nada):

```sql
ALTER TABLE dbo.TSql_DesignWall ADD TextPanelStockJson nvarchar(max) NULL;    -- _PanelStock
ALTER TABLE dbo.TSql_DesignWall ADD TextPanelReplaceJson nvarchar(max) NULL;  -- _PanelReplace
```

Alternativa: una sola columna JSON para las dos reglas. Además haría falta leerla y escribirla en el guardado del diseño, como `NumberCurveModulation`. Ver `encofrado-atk-60.md` §9.2 y §9.4.

## 12. Limitaciones conocidas

- Las reglas de paneles **duran la sesión** (sin columna en BD).
- Cambio guiado: solo **muro recto y un solo ancho**; no esquinas L / cara E / tapes / remates / curvo. Si cambia la geometría del muro, las reglas que ya no casan no hacen nada.
- Esquinas: una pieza compartida cuenta en el muro al que la asignó el cálculo; un vecino a más de 2 saltos que cambie la esquina (muy raro) no se ve.
- Piezas movidas o borradas a mano: el recuento refleja el cálculo.
- Descripciones solo en el idioma del catálogo (`TextLabel`): no hay nombres en inglés en BD.
- Voz: el reconocimiento necesita conexión y **HTTPS o localhost**; Firefox no lo soporta. La calidad del habla depende de las voces instaladas. Con altavoz muy alto, el final de la frase puede colarse al reanudar el micrófono.
- Servidor sin recompilar: el recuento sale con aviso; los cambios de paneles no hacen nada.
- `Deshacer` solo deshace en la escena (un uso); no hay deshacer en BD porque no se escribe.
- Textos en el JS (sin `.resx`).

## 13. Preguntas abiertas

**Muro especial** (§10):

1. ¿Qué BD debemos usar? La local de desarrollo no tiene estas tablas. ¿Hay que aplicar los scripts del 2026-10-05 y del 2026-10-30, o se trabaja contra la copia «producction»?
2. Si el asistente hace especial un muro, ¿hay que encofrar de nuevo los vecinos, o se dejan como están?
3. Una pieza de esquina compartida, ¿a qué muro pertenece?
4. ¿Deshacer es solo de la sesión, o tiene que sobrevivir a la purga diaria?
5. ¿Debe entrar en el especial solo el muro elegido, o también sus accesorios (puntales, rigidizadores…)?
6. ¿El popup de Cantidad de material debe sumar las piezas manuales? Hoy no lo hace.
7. ¿Los muros curvos quedan fuera de esta primera fase?
8. ¿Qué es «del 19»? No lo he interpretado.

**Cambio guiado de paneles** (§4.3; hoy funciona con los valores por defecto indicados):

1. ¿Solo parejas, o también una cara suelta cuando faltan impares? (Hoy: parejas; faltan 3 → se cambian 2 parejas.)
2. ¿Elegir por defecto desde el final del muro o desde el inicio? (Hoy: desde el final.)
3. ¿Orden de opciones (menos piezas primero) y máximo de 3 piezas por panel? (Hoy: así.)
4. Muros altos: ¿cambiar por banda?
5. Con `_PanelStock` a la vez, ¿contar el consumo de stock antes del cambio? (Hoy: antes.)
6. ¿Crear la columna en BD (§11)?

**Muro curvo**: desempate de la modulación propuesta. Hoy se prefiere remate exterior ≤ 0,15 e interior ≥ 0,05 y, **en empate, la menor** (`encofrado-atk-60.md` §9.3). Confirmar si es el criterio.

## 14. Historial de decisiones

| Fecha | `?v` | Decisión |
|-------|------|----------|
| 13/11/2026 | — | Fase 1: pestaña «Asistente IA» en el muro recto, el curvo y la página del plugin; gancho `maStlWallAiHandleMessage`; saludo según la hora; conversación por muro en memoria |
| 13/11/2026 | — | Fase 2: intérprete **local** tolerante a faltas (sin servicios externos) y «material de encofrado de este muro» con el mismo mecanismo que «Cantidad de material» |
| 13/11/2026 | `20261113e` | Español e inglés: responde en el idioma del mensaje; empate → conversación → interfaz |
| 13/11/2026 | `20261113f` | Fase 3: calcular **solo ese muro** con vecinos a 2 saltos (no aislado: perdería las esquinas) y `Preview=true`; `_PanelStock`; curvo por modulación. C# aditivo, regresión 74 + 13 idéntica |
| 14/11/2026 | `20261114a` | Modo compacto 450 × 800 a la derecha, sin bloquear el visor |
| 14/11/2026 | `20261114b`–`d` | Micrófono: Web Speech API; gris 70%; **Alt sola** en vez de Espacio; **modo continuo** |
| 14/11/2026 | `20261114e` | Recuento = encofrado **provisional de solo ese muro** en el visor; **no** usar `_IsFormwork` en los demás muros |
| 14/11/2026 | `20261114f` | Cambio guiado de paneles por parejas, «todos / uno a uno», opciones válidas, `_PanelReplace` en C#; persistencia solo de sesión; no marcar el muro como especial ni escribir en BD |
| 14/11/2026 | — | Estudio del muro especial (`Is_Special`, `TSql_DesignWallArticle`): solo análisis, con 8 preguntas abiertas |
| 14/11/2026 | `20261114g` | Respuestas habladas: altavoz encendido por defecto; no lee tablas ni códigos; pausa del micrófono mientras habla |
| 14/11/2026 | `20261114h` | Números: «90» para 0,90/90.00 y «90 punto 45» para 90,45; voz **local** primero («la más eficiente»); saludo y avisos del micrófono en voz alta |
| 14/11/2026 | — | Este documento |

## 15. Cómo probar

**Solo JS** (intérprete, chat, compacto, micrófono, voz, recuento provisional): copiar los archivos y **Ctrl+F5** en el navegador. El `?v` nuevo obliga a recargar los módulos.

**Servidor** (`Preview`, `_PanelStock`, `_PanelReplace`): **recompilar y publicar** Desing. Con Visual Studio 2022 o su msbuild, pasando `/p:VisualStudioVersion=17.0`. Sin recompilar, el asistente avisa y no cambia paneles.

**Prueba manual rápida** (HTTPS o `https://localhost:44384` para el micrófono):

1. Abrir un diseño, seleccionar un muro recto sin encofrar → «Modificar muro» → pestaña **Asistente IA** (pasa a compacto y saluda).
2. «dame el material de encofrado de este muro» → tabla + encofrado provisional de solo ese muro.
3. «de 13 paneles de 2,70 x 0,90 solo tengo 10» → recuento, «todos», «la 2» → ANTES/DESPUÉS → «sí» → **Deshacer**.
4. «no tengo paneles de 0,75 ni de 0,90» → `_PanelStock` con ANTES/DESPUÉS.
5. Alt → dictar «cuánto pesa» → respuesta hablada con el micrófono en pausa.
6. Repetir 2 en inglés y en un muro curvo.

**Pruebas automáticas** (en `/workspace/ia/`, Node + Chrome headless con datos reales del solver):

| Suite | Comprobaciones | Cubre |
|-------|----------------|-------|
| `p15/test.mjs` (+ `harness-p15.html`) | 242 | Voz, números, altavoz, saludo, avisos (sustituye a p14) |
| `p13/test.mjs` (+ `harness-p13.html?check=1`) | 65 (+11) | Cambio guiado con el solver real |
| `p12/test.mjs` | 31 | Encofrado provisional de solo el muro |
| `p11/test.mjs` | 238 | Micrófono (harness con el altavoz apagado) |
| `p8/test.mjs` | 139 | Modo compacto |
| `p6t/test.mjs` | 78 | Solo este muro, `_PanelStock`, curvo |
| `p5/test.mjs` · `p3/test.mjs` · `p2/test.mjs` | 172 · 173 · 32 | Inglés · intérprete con faltas · material del muro |
| `p13/regress.sh` | 74 + 13 | Servidor byte a byte idéntico sin atributos |

Antes de copiar al repositorio: `node --check` de los JS y comprobar hashes caja ↔ PC.
