# Tapes ATK-60 en el visor Desing_2

Para el siguiente agente. Hecho el **20 de octubre de 2026**. El frente del informe quincenal 02 (apartado 21) está cerrado.

No reabrir la posición ni la rotación de la placa del cierre de menos de 0,30 m. El usuario lo dio por bueno.

## Qué hace

El visor cierra los dos extremos libres de un muro ATK-60, en el sentido real del muro. Un extremo unido a otro muro no se cierra.

El formulario del botón derecho (pestaña Configurar Tapes) guarda el inicio en `_Tape_1` y el final en `_Tape_2`. Los solapes van en el JSON extra (`_ChekSolapeIni`, `_ChekSolapeEnd`).

| Espesor | Sin solape | Con solape |
|---------|------------|------------|
| Menos de 0,30 m | TapeS3 | TapeS1 / TapeS2 (tablón) |
| 0,30, 0,45, 0,60 y 0,90 m | TapeS4 (solución 1) o TapeS5 (solución 2) | TapeS1 / TapeS2 |
| 0,75 m | No entra en el bloque exacto | — |

## Archivos

| Archivo | Rol |
|---------|-----|
| `Repositories/Repository_Desing_2/Atk60/ExactWidthEndTape.cs` | Cierre de los dos extremos. Lo llama `Atk60WallsRepository.BuildPanel270ElementsForThreeJs`. |
| `Repositories/Repository_Desing_2/Atk60/FormTie.cs` | En espesor menor de 300 mm no pone tirante de panel a menos de 100 mm de un extremo. |
| `Scripts/MasterArticles/master-article-details-stl-viewer.js` | Malla del remate de madera y pose de pilar de la placa. |
| `Scripts/Design/3D/Design_3d_Object.js` | Referencia antigua: `FilerSEMeS01`. No editarla para este visor. |

## TapeS3 (menos de 0,30 m)

Como `FilerSEMeS01`. El origen del remate es el canto del panel: la madera va a ras, sin los 22 mm y 73 mm del Desing antiguo.

- Relleno naranja `0xD35400`: 22 mm a lo largo, ancho = espesor + 240 mm, a toda la altura.
- Dos tablones marrón `0xA78344`: 75 mm a lo largo y 120 mm de ancho, uno por fuera de cada cara de hormigón.
- Placa `10443020` en las dos caras, a las alturas del generador antiguo. Rotación de pilar: `RotX = π/2`, `RotZ = π/2`, `RotY = atan2(normal de la cara) + π`, y otro `π` solo en la placa del extremo final. No cambiarla.
- Posición aceptada: en el medio del ancho del tablón (60 mm desde la cara de hormigón) y, hacia fuera del canto, 37,5 mm más media anchura (60 mm). En total, 97,5 mm por fuera del canto, en el eje del muro.
- El fijador `1850164` no se dibuja en este cierre.

## TapeS4 y TapeS5

Solo 0,30, 0,45, 0,60 y 0,90 m.

- Solución 1: panel del espesor, angular en las dos esquinas y grapa fija `10004220`.
- Solución 2: panel regulable de 0,75 m. En 0,60 y 0,90 va junto a un panel de 0,30. Gancho `1920811` y tuerca `10443020`. En 0,30 el gancho va en las dos caras. De 0,45 en adelante el angular queda en una cara y el gancho en la del sobrante.

## Fuera de este cierre

No implementarlos salvo que se pida: el panel de 0,30 en un muro de 0,65 m, el panel que no entra por encima de 0,75 m, centrar las costillas, y los topes de longitud del remate (0,45 m con solape, 0,20 m sin solape, 0,29 m por debajo de 0,30 m).
