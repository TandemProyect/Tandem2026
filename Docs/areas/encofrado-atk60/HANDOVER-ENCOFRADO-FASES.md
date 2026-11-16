# Handover — Encofrado por fases (1 nov 2026)

Documento para el siguiente agente. La función ya está en el visor Desing_2. No hace falta inventarla otra vez.

Lecturas previas, en este orden:

1. Este fichero.
2. `nevi-rendimiento-puesta.md` — rendimiento, tongada y desencofrado. La guía NEVI, páginas 81 a 89, no describe el algoritmo.
3. `encofrado-fases-diseno-30008.md` — metros del diseño 30008 y una lectura de 7 puestas. Esa lectura no es el botón.
4. `esquina-L-atk60.md` y `esquina-T-atk60.md` — el angular de la L y la T ya aceptados. No reabrirlos.
5. `agente-tapes-atk60.md` — tapes del muro entero. El tape de una fase no cambia esos tipos.

El relato día a día está en `Desing/Report/2026/Informe Semanal 26102026-2112026.md`, apartado `### Encofrado por fases`. El reloj de la persona sigue abierto desde el 29 oct 2026, 14:34. No cerrarlo al documentar.

---

## 1. Qué tiene que hacer

Planificar un diseño en N fases y varios equipos, pintar los muros por fase y, al encofrar, montar solo la fase pedida.

Reglas que el usuario ya ha confirmado:

- El juego de paneles es el **máximo** de cada pieza entre las fases, no la suma. Lo que una fase no monta queda en el suelo, también en la fase 1.
- La fase 1 no se queda el diseño entero ni todas las esquinas.
- Una esquina en L, T o cruz que entra en una fase se encofra **entera**, con el **angular** de siempre. No se cierra con tablón. El tablón en la esquina no es una solución.
- El tape no va en la esquina. Va al final del encofrado, un poco más allá de la esquina (solape de 0,15 m, o más si el recorte de la esquina se come el cabo). La fase siguiente solapa ese tramo y ahí no lleva tape, porque el hormigón ya está.
- No se encofra el muro entero de la fase siguiente. Solo el cabo.
- Al encofrar la fase 2, la fase 1 se ve en hormigón. Al encofrar la fase 3, las fases 1 y 2 se ven en hormigón.
- La planificación (fases, m² y material, incluido el extra del solape) se calcula al elegir el muro de origen. Encofrar solo pinta. No volver a guardar el recuento al encofrar: la fase 2 tardó unos 5 minutos por eso.
- Borrar la planificación es borrado lógico. Nunca `DELETE` físico.
- Dos bases locales. La remota de site4now está comentada en `Web.config` y no se toca. No copiar la contraseña de ese comentario.

---

## 2. Lo que se hizo y se deshizo

No repetir estas interpretaciones. El usuario las rechazó.

| Fecha | Qué se hizo | Por qué no vale |
|---|---|---|
| 29 oct 19:33 | Tapes en los cuatro brazos del cruce, y soltar el canto si el vecino no está | El tape de un cruce cerrado no va en los brazos. Revertido el 30 oct por la mañana. |
| 30 oct 10:58 | `sealSplitCorners` metía toda la esquina compartida en la fase mínima | La fase 1 se quedó los 336,7 m² del 30008 y las otras salían a 0. La función sigue en el fichero y **no se llama**. |
| 30 oct 11:31 | El tape en la cara del muro de la fase, sin encofrar la esquina | El hormigón se escapa. La esquina tiene que estar resuelta. |
| 30 oct 13:51 | Al encofrar, guardar cada panel en `TSql_DesignWallArticle` y reenviar el plan | La fase 2 tardaba minutos. El material se estima al elegir el origen. |
| 30 oct 14:44 | No recortar el cabo `-exit` para que los paneles llegaran al tape | El 31 oct la esquina en L salía con tablón en vez del angular. El recorte de esquina vuelve a aplicarse también al cabo. |

`pullToFirst` existe y no se llama. No fuerces la fase 1 a llevar el juego mayor.

El remate de T o cruce con espesores distintos sigue pendiente. No hacerlo de paso.

---

## 3. Cómo se usa

Botón de capas, en la barra superior del visor, a la izquierda de exportar e importar.

1. El cuadro abre pequeño: número de fases y número de equipos.
2. Con las dos cifras, el cuadro crece, salen las filas y se ve el diseño para elegir el muro.
3. Cada fila: nombre (defecto «Equipo 1»), trabajadores (defecto 4), color (defecto `#c9a0a8`) y «Muro de origen».
4. Un clic en un muro lo asigna a ese equipo. Otro equipo no puede coger el mismo. Al tener fases, equipos y todos los orígenes, se resuelve y se pintan los colores. El cuadro no se cierra solo.
5. «Otra solución» pasa al siguiente reparto distinto.
6. Aceptar, si el plan ya se ve, cierra el cuadro enseguida y guarda en segundo plano (muros, foto, cámara y plan). El aviso lleva la hora. El primer Aceptar, si aún no hay colores, solo muestra el plan.
7. «Borrar planificación» pide confirmación, da de baja las filas de ese diseño y vacía el JSON. El cuadro vuelve al paso pequeño y el visor de origen vuelve a pintar el diseño.

Al pulsar Encofrar: si hay 2 fases o más guardadas, pregunta cuál ver. Con una o ninguna, encofra el diseño entero como antes.

Caché actual en `Desing/Views/Desing_2/Viewer.cshtml`:

- `desing2-stl-viewer-toolbar-wiring.js?v=20261030i`
- `desing2-phase-start.js?v=20261030i`, que importa `desing2-phase-solve.js?v=20261030f`

El visor grande (`master-article-details-stl-viewer.js`) se cachea por la fecha del fichero. Un cambio de C# exige reiniciar el sitio: el 31 oct a las 9:20 se compiló `Desing.dll` para devolver el recorte de la esquina. Tras un cambio de script, Ctrl+F5.

El 31 oct a las 9:06 `MSSQL$SQLEXPRESS` estaba parado y el inicio de sesión fallaba con el error 26. El servicio arranca en Automático, pero desde el agente no se pudo iniciar (hace falta administrador). Si el sitio no entra en la base, mirar primero si el servicio está en ejecución.

---

## 4. Ficheros

| Fichero | Qué hace |
|---|---|
| `Desing/Scripts/Desing2/desing2-phase-solve.js` | Reparte muros y estima el material. No pinta. |
| `Desing/Scripts/Desing2/desing2-phase-start.js` | Cuadro, visor de origen, leyenda, guardar y borrar. |
| `Desing/Scripts/Desing2/desing2-stl-viewer-toolbar-wiring.js` | Al encofrar, filtra la fase, añade cabos, solapes y tapes. |
| `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js` | Colores de fase en líneas y muros 3D, hormigón de las fases ya hechas, solape dibujado. |
| `Desing/Views/Desing_2/Viewer.cshtml` | Modal `#desing2PhaseModal` y las URLs de guardar y borrar. |
| `Desing/Controllers/Desing_2Controller.cs` | `SavePhasePlan`, `ClearPhasePlan`. |
| `Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs` | Recorte de nudo. El cabo `-exit` se recorta igual que cualquier esquina. |
| `Desing/Repositories/Repository_Desing_2/Atk60/LCornerFormwork.cs` | Angular de L, paneles de T y de cruce. No tiene un camino distinto para fases. |
| `Desing/Repositories/Repository_Desing_2/Atk60/ExactWidthEndTape.cs` | `FlushPhaseExitTape`: el cabo `-exit` no hereda TapeS1 ni TapeS2. El cierre queda en el último panel. |
| `Desing/Repositories/RepositoryDesing2/DesignWallArticleRepository.cs` | `NumberPhase` y `StampPhase`. Encofrar ya no llama a esto. |

Textos nuevos de la pantalla: `Desing/Scripts/ResourceGenerators/_gen_masterarticles_resources.js`, luego el generador. No editar `.resx` ni `Designer.cs` a mano. Node es `C:\Users\jag\AppData\Local\Programs\cursor\resources\app\resources\helpers\node.exe`.

---

## 5. Base de datos

Local: `.\SQLEXPRESS`, seguridad integrada.

- Catálogo del sitio: `db_a197cd_desingproducction`. Tiene `dbo.TSql_Design_V2`.
- Segundo catálogo: `db_a197cd_desingdevelop`. No tiene `TSql_Design_V2`. La tabla de detalle existe ahí sin clave foránea.

`dbo.TSql_Design_V2` (solo producción local):

- `AttPhasePlanJson` — equipos, colores, suelo, juego y cámara del cuadro.
- `AttViewerStateJson` — cámara del visor grande.

La verdad de muro → fase es `dbo.TSql_DesingFasesDetail`. Una fila por muro: `LinkDesign_V2`, `NumberPhase`, `TextWallId`. Índice único filtrado `(LinkDesign_V2, TextWallId) WHERE Is_Delete = 0`. Al guardar se da de baja lo anterior y se inserta. `TextLabel` = `Fase {n} · muro {id}`.

Borrado lógico: `Is_Delete = 1`, `Is_Active = 0`, `LinModifiedBy`, `AddLastDateChange`, `Ntimeschanged + 1`.

`dbo.TSql_DesignWallArticle.NumberPhase` (`BIGINT NULL`) ya está en las dos bases locales. El script es `Desing/Scripts/TemporalScript/2026-10-30_alter_TSql_DesignWallArticle_NumberPhase.sql`. NULL significa sin fase. El EDMX no conoce la columna: se escribe por SQL. No editar `DAL/Model.edmx` ni `DAL/Model.Context.cs` a mano.

Al cargar, el JSON trae el resto del plan y las filas de la tabla pisan `byWall`.

---

## 6. Cómo reparte

`solvePhasePlans(walls, origins, phaseCount)` prueba cinco estrategias y se queda las que no repiten el mismo muro:fase.

| Id | Título |
|---|---|
| `corners-long` | Esquinas primero, tramos largos |
| `corners-short` | Esquinas primero, tramos cortos |
| `long` | Tramos largos |
| `short` | Tramos cortos |
| `two-starts` | Reparte los arranques |

Los orígenes entran en la fase 1. El objetivo de cada fase es el total de m² partido por el número de fases. `claimJoints` mete en la fase del muro recién cogido los miembros aún libres de sus L, T y cruces. Un solo nivel: la esquina siguiente puede ser de otra fase.

Colores de fase: `#3b82f6`, `#16a34a`, `#d97706`, `#9333ea`, `#dc2626`, `#0891b2`.

En el 30008, desde el muro 1 y 3 fases, la estrategia de esquinas y tramos largos salió en unos 131,5 / 131,6 / 73,6 m². El estudio de las 11:09 (primero ~112 m² y luego esquinas) está en el documento del 30008 y **no está cableado al botón**. La planificación a mano tampoco está hecha.

`fillMaterial` estima el material al elegir el origen:

- Una esquina cerrada (todos sus miembros en la misma fase) suma `Esquina L`, `Esquina T` o `Cruce` en la fase dueña. El dueño es la fase mínima, no el primer id de la lista.
- Una esquina partida: la fase mínima se queda la pieza de esquina, un cabo de hasta 1350 mm de cada brazo posterior y un `Tape extremo`.
- El largo propio de un muro posterior se acorta 1200 mm por cada extremo que solo toca una fase anterior (mínimo 300 mm). Ese extremo no lleva tape.
- Un extremo hacia una fase posterior, o un extremo libre, lleva `Tape {tipo}` según `_Tape_1` / `_Tape_2`.
- Puntal, puntal inferior, rigidizador y ménsula, una vez por muro si la casilla está marcada.
- `kit[pieza] = máximo entre fases`. `ground = kit − usado`.

Esas cifras de 1350 y 1200 no coinciden con el cabo geométrico (1850 mm, ver abajo). El 30 oct a las 14:20 el usuario dijo que los datos de la leyenda estaban bien. No cambiar el estimador para «cuadrarlo» con el dibujo si no lo pide.

---

## 7. Cómo encofra una fase

En `desing2-stl-viewer-toolbar-wiring.js`, solo si `formPhase > 0`. El orden es fijo. El `pool` se clona **antes** de filtrar; si se clona después, el cabo no tiene muro de origen.

```
pool = clonar todos los muros
walls = solo los de esta fase
addCornerStubs
extendPhaseExits
trimPouredOverlaps
releasePhaseEnds
markPhaseOverlaps
```

No poner `window.desing2FormPhase`. No llamar a un guardado de artículos. El POST `GetWallsAtk-60` solo encofra los muros que van en `IdsJson`. No recarga el diseño desde SQL.

Identificar muros: `Id`, `LineId` y `_idObject` contra el id del eje; `WallId` contra el id de muro. Los dos extremos, a 500 mm de suma de cuadrados, también valen. **No** meter `wallGroupId` ni `WallGroupId` en la comparación: eso encofraba el diseño entero.

### Cabo de la esquina

`cornerStubMm()` = 1200 + 500 + 150 = **1850 mm**.

`addCornerStubs` / `makeCornerStub`: el cabo es un muro nuevo, id `original-exit`. P1 en el vértice, P2 en la punta. El extremo del vértice conserva la junta (`L:`, `T:` o `Cross:`). El extremo libre queda con la conexión vacía para que entre el tape. `picked.cornerEnds` protege ese extremo del muro de la fase para que `releasePhaseEnds` no lo borre.

C# `CanonicalizeClockwiseWallAxis` da la vuelta al eje si mira hacia −X (o, si va vertical, hacia −Z) y **no** mueve `_Idconnection_1` / `_Idconnection_2`. Por eso `connectionKeyForPoint` y `axisFlips` escriben la conexión en el índice que C# va a leer después del giro. Sin eso el tape cae en la esquina y la punta queda abierta.

`FlushPhaseExitTape` quita TapeS1, TapeS2 y el solape del cabo. Si no, el cierre queda 0,30 m detrás del último panel amarillo.

El 31 oct 2026, 9:18, el cabo **sí se recorta** en `ApplyConnectedEndTrim`, igual que una esquina normal. Hace falta para que `LCornerFormwork.TryPlace` ponga el angular. Si no se recorta, la esquina se tapa con tablón. El tape de la punta no se recorta: ese extremo no tiene grado 2.

`StripTapePanelAtEnd` borra piezas de orientación `pillar` a menos de 500 mm del extremo de esquina. Por eso el cabo mide 1850 y no 1350: tras el recorte de la esquina, la punta tiene que seguir a más de 500 mm.

### Solape

`PHASE_OVERLAP_MM` = 150.

- Hacia una fase **posterior**, en recta (`continuationAt`, producto > 0,85 y extremos a menos de 80 mm): se alarga el muro hasta 150 mm y el canto se deja libre para el tape.
- Hacia una fase **anterior**, en esquina: `trimPouredOverlaps` entra el extremo `cornerStubMm() - 150` mm (ahora 1700) si el muro es más largo que eso más 300 mm, y marca `Overlap`. `EndIsFree` es falso si la conexión no está vacía, así que no hay tape. La fase 2 no vuelve a encofrar lo ya hormigonado.
- `Overlap` en un extremo que solo toca fase anterior. Una junta que abre a una fase posterior se deja: el cabo la resuelve.

Al pintar la fase N > 1, `maStlDesing2ShowPouredPhases` pone hormigón (`concrete.png`) en las mallas cuyas fases son todas anteriores, esconde las que incluyen la fase actual y deja el color de fase en las posteriores. Hace falta haber generado el muro 3D; si no hay malla, no hay hormigón. `maStlDesing2ShowPhaseOverlapConcrete` dibuja, en la escena, una caja de hormigón de 1700 mm desde la junta a lo largo del muro de la fase actual. Esas cajas se quitan al salir del encofrado (`ShowPouredPhases(0)` desde `ClearAtk60Visuals`).

El hueco gris entre el hormigón de la fase 1 y el amarillo de la fase 2 era ese tramo: la malla del muro de la fase 2 está escondida y, sin la caja, no se veía.

---

## 8. Qué queda pedido y no está hecho

1. **31 oct 2026, 9:40.** En el visor de origen, iluminar el muro bajo el ratón. Y una marca en el punto donde se empieza a encofrar. No está escrito. El clic de hoy solo asigna el muro (`pickAt` → `assignWall`). El visor está en `desing2-phase-start.js`, `ensureViewer` y `rebuildMeshes`.
2. Planificación a mano. El usuario la anunció. No hay pantalla.
3. El corte del 11:09 (primero los m², luego las esquinas) no sustituye al botón hasta que lo confirmen.
4. Remate de T y cruce con espesores distintos. No hacerlo de paso.
5. Igualar el estimador (1350 / 1200) con el cabo de 1850 solo si lo piden. La leyenda se dio por buena el 30 oct.
6. `NumberPhase` no está en el EDMX. Un «Update Model from Database» no está pedido.

---

## 9. Cómo probar

Diseño de referencia: 30008. 18 ejes, alto 2,70 m, espesor 0,30 m, 336,71 m². Libre: muro 1 en (0, 0). Tres T (`1-22-28`, `25-28-31`, `4-49-52`) y un cruce (`22-34-37-40`).

1. Reiniciar el sitio si hubo cambio de C#. Ctrl+F5.
2. Tres fases, un equipo, origen en el muro 1. Tienen que pintarse azul, verde y naranja. La fase 1 no puede ser los 336 m².
3. Aceptar. El cuadro se cierra y el aviso trae la hora.
4. Encofrar la fase 1. La esquina en L lleva angular, como sin planificación. El tape queda al final del cabo, no en la cara del muro ni encima del encofrado. El muro de la fase siguiente no se encofra entero.
5. Encofrar la fase 2. La fase 1 en hormigón, incluido el solape. Sin tape donde ya hay muro. Sin volver a encofrar el cabo de la fase 1. No debe tardar minutos.
6. Borrar la planificación. El cuadro vuelve a pedir solo fases y equipos, y el diseño se puede elegir otra vez.

Si el inicio de sesión dice que la contraseña no existe y el correo de recuperación tira el error 26, `SQLEXPRESS` está parado. No es la contraseña.
