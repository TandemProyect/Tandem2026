# Tapes ATK-60 en el visor Desing_2

Para el siguiente agente. Hecho el **19, 20 y 21 de octubre de 2026**.

- Apartado 21 del informe quincenal 02: tapes del formulario y el cierre sin solape. Cerrado el 20 oct, 10:03.
- Apartado 24: Tape con solape (TapeS1). Cerrado el 20 oct, 15:54. El usuario lo dio por bueno.
- Apartado 25: TapeS2. Cerrado el 21 oct, 10:37.
- Apartado 26: módulos, rigidizadores, solape repartido y placas del final. Cerrado el 21 oct, 10:37.
- Apartado 27: TapeS7 y TapeS8. Cerrado el 21 oct, 12:43. El usuario lo dio por bueno. Pendiente un testeo total de altura.

No reabrir la posición ni la rotación de la placa del TapeS3. El giro del TapeS1 en el inicio no se toca. En el final las dos placas van a 180° por `RotZ`, no cruzando `RotY`.

## Qué hace

El visor cierra los dos extremos libres de un muro ATK-60, en el sentido real del muro. Un extremo unido a otro muro no se cierra (`EndIsFree`).

El formulario del botón derecho (pestaña Configurar Tapes) guarda el inicio en `_Tape_1` y el final en `_Tape_2`. Los solapes van en el JSON extra (`_ChekSolapeIni`, `_ChekSolapeEnd`).

| Espesor | Sin solape | Con solape |
|---------|------------|------------|
| Menos de 0,30 m | TapeS3 | TapeS1 o TapeS2 |
| 0,30, 0,45, 0,60 y 0,90 m | TapeS4 (solución 1) o TapeS5 (solución 2) | TapeS1 o TapeS2 |
| 0,75 m | No entra en el bloque exacto | — |
| Mayor de 0,30, menor de 0,60, no exacto | TapeS7 o TapeS8: angular, panel que quepa y taco | TapeS1 o TapeS2 |

`ResolveTape`: si está guardado TapeS1, TapeS2, TapeS5, TapeS7 o TapeS8, manda eso. `Solucion_2` se lee como TapeS8. Si no, con solape es TapeS1 y sin solape es TapeS4.

## Archivos

| Archivo | Rol |
|---------|-----|
| `Repositories/Repository_Desing_2/Atk60/ExactWidthEndTape.cs` | Cierre de los dos extremos. Lo llama `Atk60WallsRepository.BuildPanel270ElementsForThreeJs`. |
| `Repositories/Repository_Desing_2/Atk60/FormTie.cs` | Tirante del canto solo si el extremo es TapeS2. En el resto no. En espesor menor de 300 mm tampoco pone tirante de panel a menos de 100 mm de un extremo, salvo el suplemento del TapeS2. |
| `Scripts/MasterArticles/master-article-details-stl-viewer.js` | Malla del remate de madera, pose de pilar de la placa y el giro en planta del rigidizador horizontal. |
| `Scripts/Design/3D/Design_3d_Object.js` | Referencia antigua: `FilerSEMeS01`, `FilerSExS01`, `FilerRigiSExS01`. No editarla para este visor. |
| `Repositories/Atk60/Wall/CommonElement.cs` | `SedUnionRigiHorizontal_0`: un `1850162` centrado en el espesor y placas `10443020`. El fijador `1850164` el filer antiguo no lo dibuja. |
| `Repositories/Atk60/Wall/WallTapeSolape/ModuloTape_0_01.cs` | Alturas antiguas del solape, en cm: 65, 140 y 220 en el panel de 2,70. El visor nuevo usa los taladros, no esas cotas. |

El C# se ve al volver a encofrar. El JavaScript del rigidizador pide Ctrl+F5. No hace falta tocar el plugin CAD.

## TapeS3 (menos de 0,30 m, sin solape)

Como `FilerSEMeS01`. El origen del remate es el canto del panel: la madera va a ras, sin los 22 mm y 73 mm del Desing antiguo.

- Relleno naranja `0xD35400`: 22 mm a lo largo, ancho = espesor + 240 mm, a toda la altura.
- Dos tablones marrón `0xA78344`: 75 mm a lo largo y 120 mm de ancho, uno por fuera de cada cara de hormigón.
- Placa `10443020` en las dos caras, a las alturas del generador antiguo. Quien llama a `AddSidePlate` sin los argumentos nuevos conserva esta pose. No cambiarla.
- Rotación de pilar: `RotX = π/2`, `RotZ = π/2`, `RotY = atan2(normal de la cara) + π`, y otro `π` solo en la placa del extremo final.
- Posición aceptada: en el medio del ancho del tablón (60 mm desde la cara de hormigón) y, hacia fuera del canto, 37,5 mm más media anchura (60 mm). En total, 97,5 mm por fuera del canto, en el eje del muro.
- El fijador `1850164` no se dibuja en este cierre.

## TapeS4 y TapeS5

Solo 0,30, 0,45, 0,60 y 0,90 m. Sin solape.

- Solución 1 (TapeS4): panel del espesor, angular en las dos esquinas y grapa fija `10004220`.
- Solución 2 (TapeS5): panel regulable de 0,75 m. En 0,60 y 0,90 va junto a un panel de 0,30. Gancho `1920811` y tuerca `10443020`. En 0,30 el gancho va en las dos caras. De 0,45 en adelante el angular queda en una cara y el gancho en la del sobrante.

## TapeS1 (con solape)

Aceptado el 20 oct, 15:54. Sustituye al tablón claro del canto. Puede ir solo al inicio, solo al final, o en los dos. El final es el simétrico del inicio: `outSign` es −1 al inicio y +1 al final.

Suplemento de 300 mm de panel amarillo (`Atk60ModuleAssembly.Build`). El inicio ocupa [−300, 0] y el final [L, L+300]. Las barras de ese módulo que caen fuera de [30, L−30] no se pintan.

### Fenólico y tablones

El origen nuevo es el canto. No repetir el desplazamiento de +22 mm del Desing antiguo.

- Naranja `wood-orange` `0xD35400`: ancho = espesor, 22 mm hacia fuera, `offsetOut = 0`. La cara interior va pegada al canto.
- Dos marrones `wood-side` `0xA78344`: 75 mm de ancho, en los dos bordes del fenólico (`offsetAcross` 0 y espesor−75). Empiezan en la cara exterior del fenólico (`offsetOut = 22`) y llegan al final de los 300 mm (fondo 278 mm). No salen de los paneles amarillos.

La malla `REMATE_WOOD` crece en +X a lo ancho, +Y hacia arriba y +Z a lo largo. El fondo usa `WallThicknessMm`, así que 22 mm se conserva.

### Rigidizador `1850162`

Uno solo, no uno por cara. STL en metros; el visor escala ×1000. Caja aproximada 105 × 60 × 750 mm. El largo (750) es el eje Z del STL. Tras el giro, el largo cruza el espesor, los 105 mm quedan en vertical y los 60 mm a lo largo del muro.

- `rotZ = π/2` para que el visor lo trate como junta horizontal. El vertical del panel no se toca.
- En `master-article-details-stl-viewer.js`, solo si el código es `1850162` y la orientación es horizontal, se antepone un giro de 90° en planta (`Ry(+π/2)`). No aplicarlo a los rigidizadores verticales del panel.
- El centro geométrico del STL cae en el eje del muro (mitad del espesor). El descentrado del STL es unos (−1,6, −50,3, 19,8) mm; `AddTapeS1Stiffener` lo compensa.
- A lo largo: centro en el final del suplemento más 30 mm (mitad de los 60 mm), pegado por fuera del panel.
- En altura, el centro cae en el taladro del panel (`HookHeightsMm`, no las cotas 650 / 1400 / 2200 del Desing antiguo):
  - hasta 1,20 m: 350 y 1150
  - hasta 2,40 m: 400 y 2000
  - panel de 2,70 m: 550, 1350 y 2150

### Placa `10443020`

La misma pieza y la misma pose de pilar que el TapeS3. Va en las dos caras, a la altura del rigidizador, en la cara exterior del rigidizador (30 mm más hacia fuera que el centro de la barra).

Argumentos que solo usa el TapeS1. El TapeS3 no los pasa y no debe heredarlos:

- `lockMateYaw`: el giro de la cara de detrás es el de la cara principal más π. No aplica el volteo extra del extremo final del TapeS3.
- Cara principal: `extraHalfTurn`. Cara de detrás: sin ese medio giro. Igual en el inicio y en el final.
- En el inicio las dos llevan `extraRotZ = π`, así que `RotZ = 3π/2`.
- En el final las dos llevan `extraRotZ = 0`, así que `RotZ = π/2`. Esos 180° respecto al inicio. El ángulo tiene que caber en una vuelta: si pasa de `2π`, el visor lo lee como grados y en pantalla se quedan 90°.

No volver a cruzar solo las banderas de `RotY`: una cara gira 180° y la otra solo unos grados. El medio giro común va en `RotZ`. El inicio no se vuelve a girar.

### Gancho de cierre `1920811`

Uno por placa, en las dos caras. Es el tornillo con el pico, de unos 270 mm en catálogo. El pico entra en el perfil del panel y el tornillo pasa por el centro de la estrella de la placa.

- Pose de pilar ya usada en esquinas y en el TapeS5: `RotX = 3π/2`, `RotZ = π`, `RotY` hacia fuera del muro.
- El eje del tornillo no está en el origen del STL. Con esa pose queda a 0,6 mm en altura y 83,6 mm en el plano. `AddTapeS1Hook` resta ese descentrado para que el eje caiga en el centro de la placa.
- Desde ese punto se corre 50 mm hacia el muro, a lo largo del muro (`TapeS1HookIntoProfileMm`). En un muro a rotación 0 ese eje es X. La placa no se mueve: el tornillo sigue pasando por su centro.

El fijador `1850164` no se pinta. `FilerRigiSExS01` sale enseguida cuando el código es ese.

## TapeS2

Aceptado el 21 oct, 10:37. Mismo suplemento de 300 mm y mismo fenólico de canto que el TapeS1. Donde el TapeS1 lleva tablón de 75 mm, aquí hay fenólico naranja de 22 mm. No lleva el rigidizador `1850162` ni el gancho `1920811`.

Los tirantes son los del panel suplementario (`tiesBeyondWall`). El del panel anterior, a unos 44 mm por dentro del canto, no se pinta: ventana de 100 mm hacia dentro del canto y 20 mm hacia fuera. Igual si el TapeS2 está al inicio.

La grapa fija `10004220` va en la junta con el panel de 0,30 m, en las dos caras, también en el TapeS1. Esa junta queda fuera del muro y el módulo solo no la pintaba.

## Tirante del canto

Solo el TapeS2. En TapeS1, TapeS3, TapeS4 y TapeS5 el extremo lo cierran el angular, la unión o las grapas, y no se pinta el tirante del final del panel ni el del arranque del primer panel. `SkipsPanelEndTie`. En el suplemento del TapeS1 tampoco va el tirante del canto exterior, el que quedaba junto al rigidizador.

## Solape en los dos extremos

Si el TapeS1 o el TapeS2 está en los dos cantos, no se pintan dos suplementos de 0,30 m. Entra un módulo de 0,60 m. El sobrante se reparte: la misma salida antes del inicio y después del final. La madera del tape mide esa mitad, no los 300 mm enteros en un solo lado.

## Módulos y rigidizadores del muro

Aceptado el 21 oct, 10:37.

- Módulo de 1,20, 2,40 o 2,70: dos rigidizadores, a 300 mm de cada extremo.
- Panel solo (0,30, 0,45, 0,60, 0,75 o 0,90): uno, al centro.
- Modulación de mayor a menor: 2,70, si no 2,40, si no 1,20, y después 0,90, 0,75, 0,60, 0,45 y 0,30. No se parte un 2,70 en paneles pequeños.
- El remate de madera, si queda, es el resto por debajo de 300 mm.

## TapeS7 y TapeS8

Aceptado el 21 oct, 12:43. Espesor mayor de 0,30 y menor de 0,60 que no es 0,30, 0,45 ni 0,60. En 0,44 m el formulario guarda TapeS8. El TapeS7 del formulario es la solución 2 de 0,35, 0,40, 0,50 y 0,55. Los dos pintan `AddTapeS7`.

- El panel es el mayor de 0,30, 0,45, 0,60, 0,75 o 0,90 que quepa. En 0,44: panel de 0,30 y taco de 0,14.
- Angular en las dos caras. Sin gancho `1920811`, sin placa `10443020` y sin rigidizador `1850162`.
- Grapa fija `10004220` en las dos caras del muro, en el canto, y otra en el panel a 40 mm del tablón. Unen el angular con el panel.
- Grapa variable `10000221` en el taco, 200 mm más arriba que las fijas, para no coincidir.
- En 0,44 el formulario solo ofrece la solución 1. Al cambiar el espesor, las soluciones del inicio y del final se recalculan en el acto.
- Si un extremo es TapeS7 o TapeS8, el otro extremo libre pinta el mismo cierre.
- No lleva tirante en el canto del panel.
- Pendiente: un testeo total de altura.

## Remate

Si un extremo libre, o los dos, es TapeS1 o TapeS2, no se pinta el remate de madera. Esos tapes lo sustituyen. El fenólico naranja sigue en el canto del hormigón. El cierre de panel y taco no lo quita: en ese muro el remate sigue.

## Fuera de este cierre

No implementarlos salvo que se pida: el panel de 0,30 en un muro de 0,65 m, el panel que no entra por encima de 0,75 m, centrar las costillas, y los topes de longitud del remate (0,45 m con solape, 0,20 m sin solape, 0,29 m por debajo de 0,30 m).
