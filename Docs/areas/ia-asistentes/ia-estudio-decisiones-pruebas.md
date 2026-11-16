# IA Desing — Muro especial, columnas de BD propuestas, limitaciones, preguntas abiertas, decisiones y cómo probar

> Parte de [`ia-desing.md`](ia-desing.md) (partido el 15/11/2026 sin quitar contenido; la numeración de secciones es la original). Área: [`INDEX.md`](INDEX.md).

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

Alternativa: una sola columna JSON para las dos reglas. Además haría falta leerla y escribirla en el guardado del diseño, como `NumberCurveModulation`. Ver `encofrado-atk-60-curvos-un-muro.md` §9.2 y §9.4.

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

**Muro curvo**: desempate de la modulación propuesta. Hoy se prefiere remate exterior ≤ 0,15 e interior ≥ 0,05 y, **en empate, la menor** (`encofrado-atk-60-curvos-un-muro.md` §9.3). Confirmar si es el criterio.

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
| 14/11/2026 | `20261114i` (barra) | Botón **IA** al inicio de la barra de modos del visor (`#ma-stl-mode-ia`), aún sin función («IA: próximamente»); se quita la brújula y las barras se juntan a la izquierda (ver `desing-2-stl-viewer-funciones.md`, «Barras superiores del lienzo») |
| 14/11/2026 | `20261114i` (módulos) · `20261114j` (barra) | **IA del diseño** (§16): el botón IA abre «Asistente IA – Diseño» (compacto 450 × 800, toggle, mismo chat con `owner 'design'`). Registro de comandos extensible y primeros comandos de muros 2D: dibujar, cambiar medidas, mover, copiar/paralelo, borrar con confirmación, alargar, listar, deshacer y ayuda. Si no está en líneas, pasa a líneas y lo dice. Un solo asistente abierto. Girar, simetría, empalme, recortar por voz y numeración visible quedan como propuestas |
| 14/11/2026 | `20261114k` (visor → design-ai) | Herramientas **Girar** y **Simetría** en la barra 2D (`desing-2-stl-viewer-funciones.md`, «Girar y Simetría») y en la IA del diseño (§16.9): «gira el muro 4 90 grados» (por defecto **sobre su centro**, o sobre su inicio/final/origen/X;Z, con copia), «gira 45° a la izquierda», «haz simetría del muro 3 respecto al eje Y / al muro 5 / vertical por su extremo» (pregunta si borra el original). **Modos** por la IA con los botones de la barra: «modo líneas», «pasa a 2D», «genera 3D», «encofra» (pide Sí/No). Girar y simetría salen de «no existe» |
| 15/11/2026 | `20261115a` | **Cotas por voz** (§16.12): «muéstrame los id de muros» (Cotas con solo el ID), «acótame el muro 7» (cotas de ese muro con «h = altura»), luego «longitud 4,5» / «espesor 0,45» / «altura 3» con faltas (esperoser, atura, altrua). El número de muro de la IA pasa a ser el **ID de Cotas** (antes `wallDbId`). BD: `WALL_IDS`, `WALL_DIMS`, `TOOL_WALL_DIM` con IA; develop = producción |

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
| `p20/unit/test.mjs` · `p20/check.sh t=all` | 62 · 56 | IA del diseño: intérprete con puente simulado · visor real en la maqueta (§16.8) |
| `p21/unit/test.mjs` · `p21/check.sh t=all` | 101 · 86 | Girar, Simetría y modos: intérprete · herramientas a mano y por la IA en el visor real (§16.9) |
| `p29/ia/unit/test.mjs` · `test-catalog.mjs` · `p29/ia/check.sh t=all p29` | 142 · 74 · 28 | Cotas por voz: ID de los muros, acotar, longitud / espesor / altura, Deshacer, catálogo y entornos (§16.12) |

Antes de copiar al repositorio: `node --check` de los JS y comprobar hashes caja ↔ PC.
