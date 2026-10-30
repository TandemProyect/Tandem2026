# Encofrado por fases — diseño 30008

29 de octubre de 2026. Estudio del archivo `muros-30008-20261029-1510.tnwall` y de las dos vistas del mismo plano (muros con pilares verdes, y el mismo plano ya encofrado). No hay función nueva. El encofrado del visor no se ha tocado.

El archivo se abrió con `Desing/IA/tools/decrypt-wall-export.mjs`. Diseño 30008, exportado el 29 oct 2026 a las 15:10. 54 líneas: 18 ejes y 36 caras (dos por eje). Ningún muro especial. El encofrado generado de la segunda imagen no viaja en el archivo.

Rendimiento, tongada y desencofrado: `nevi-rendimiento-puesta.md`. Reparto del largo: `ATK60-Logica-Modulos-Wall.md` y `PackAtk60LengthMm`.

## Qué es este plano

Todos los tramos miden 2,70 m de alto y 0,30 m de espesor. Encofrado activo, panel universal, cinta tipo 1 en los dos extremos, rigidizador y estabilizador en las dos caras. Sin estabilizador inferior y sin consola.

El metro cuadrado de este estudio es largo por alto. Las dos caras van dentro de ese número, igual que en el suelo de planificación de 40 a 50 m². La cara de panel es el doble.

| | |
|---|---|
| Tramos entre juntas | 18 |
| Muros seguidos (el visor los parte en la T y en el cruce) | 13 |
| Largo de ejes | 124,707 m |
| Superficie de muro | 336,71 m² |
| Hormigón (0,30 m) | 101,01 m³ |
| Cara de panel | 673,42 m² |
| Extremo libre | uno, el muro 1 en (0, 0) |
| Esquinas en L | 11 |
| Uniones en T | 3 (`1-22-28`, `25-28-31`, `4-49-52`) |
| Cruces | 1 (`22-34-37-40`) |

Con 4 personas a mano y este número de esquinas, el suelo de 40 a 50 m² por jornada deja el plano en unas 7 puestas. Con grúa y paño recto se acerca al doble de metros, y aquí casi todos los extremos son esquina, así que el suelo de 40 a 50 es el que vale. El material de una fase sigue ocupado hasta la mañana siguiente al vertido: el juego de paneles es el de la fase mayor, no la suma de las siete.

Un lote de unos 45 m² son unos 13,5 m³. Con la bomba ya puesta, el vertido cabe en 1 o 2 horas. En tiempo templado se desencofrá a la mañana siguiente, de arriba abajo, y ese juego pasa a la puesta siguiente.

## Los 13 muros como se dibujaron

El eje guardado es el tramo entre juntas. Donde el muro dibujado es más largo, la T o el cruce lo han partido. El largo dibujado es el que se ve de un pilar verde al siguiente cuando el tramo no está partido.

| Muro dibujado | Tramos | Largo | m² | De dónde a dónde |
|---|---|---:|---:|---|
| Espolón | 1 + 22 + 34 | 24,000 m | 64,80 | (0, 0) → (24000, 0). Libre, T, cruce y L |
| Bajada de la T | 25 + 28 | 7,000 m | 18,90 | (11250, 0) → (11250, −7000) |
| Rama de la T | 31 | 5,000 m | 13,50 | (11250, −3707) → (16250, −3707) |
| Eje del cruce | 37 + 40 | 9,207 m | 24,86 | (16250, −3707) → (16250, 5500) |
| Bajada este | 4 + 49 | 8,250 m | 22,28 | (24000, 0) → (24000, −8250) |
| Ala sureste | 7 | 7,000 m | 18,90 | (24000, −8250) → (31000, −8250) |
| Cierre sur-este | 10 | 6,250 m | 16,88 | (31000, −8250) → (31000, −14500) |
| Muro sur | 13 | 13,500 m | 36,45 | (31000, −14500) → (17500, −14500) |
| Subida sur | 16 | 7,500 m | 20,25 | (17500, −14500) → (17500, −7000) |
| Cierre del patio | 19 | 6,250 m | 16,88 | (17500, −7000) → (11250, −7000) |
| Muro norte | 43 | 14,750 m | 39,83 | (16250, 5500) → (31000, 5500) |
| Bajada noreste | 46 | 9,000 m | 24,30 | (31000, 5500) → (31000, −3500) |
| Patio este | 52 | 7,000 m | 18,90 | (24000, −3500) → (31000, −3500) |

El espolón de 24 m son 64,80 m². Solo, ya pasa de una jornada a mano. Por eso la fase no puede ser «el muro dibujado» cuando la T o el cruce lo alargan.

## Cada tramo y el reparto recto

El reparto es el de `PackAtk60LengthMm` sobre el largo guardado, en una cara. La otra cara repite la misma lista: el espesor es 0,30 en los dieciocho. Un módulo de 2,70 a lo largo son, en el caso habitual, tres paneles de 2,70 de alto por 0,90 de ancho. El módulo final se reparte con los anchos del catálogo (0,90, 0,75, 0,60, 0,45, 0,30 y las sumas de 0,15). El tablón no pasa de 0,15 m.

Esto es el tramo entero, antes de la esquina. En una L el largo recto se acorta: espesor del otro muro más el panel de esquina de 0,30 m. Aquí ese descuento es 0,60 m por extremo en L. La T y el cruce tienen sus piezas propias. La lista de abajo es el techo del tramo recto, no el recuento de la segunda imagen.

| Id | Largo | m² | Una cara, sin recorte de esquina | Juntas |
|---:|---:|---:|---|---|
| 1 | 11,250 | 30,38 | 4 × 2,70 + 0,45 | libre y T `1-22-28` |
| 22 | 5,000 | 13,50 | 2,70 + 2,25 + tablón 0,05 | T `1-22-28` y cruce |
| 28 | 3,707 | 10,01 | 2,70 + 0,90 + tablón 0,11 | T `1-22-28` y T `25-28-31` |
| 25 | 3,293 | 8,89 | 2,70 + 0,45 + tablón 0,14 | T `25-28-31` y L `19-25` |
| 31 | 5,000 | 13,50 | 2,70 + 2,25 + tablón 0,05 | T `25-28-31` y L `31-37` |
| 19 | 6,250 | 16,88 | 2 × 2,70 + 0,75 + tablón 0,10 | L `19-25` y L `16-19` |
| 34 | 7,750 | 20,93 | 2 × 2,70 + 2,25 + tablón 0,10 | cruce y L `34-49` |
| 37 | 3,707 | 10,01 | 2,70 + 0,90 + tablón 0,11 | L `31-37` y cruce |
| 40 | 5,500 | 14,85 | 2 × 2,70 + tablón 0,10 | cruce y L `40-43` |
| 16 | 7,500 | 20,25 | 2 × 2,70 + 2,10 | L `13-16` y L `16-19` |
| 13 | 13,500 | 36,45 | 5 × 2,70 | L `10-13` y L `13-16` |
| 10 | 6,250 | 16,88 | 2 × 2,70 + 0,75 + tablón 0,10 | L `7-10` y L `10-13` |
| 7 | 7,000 | 18,90 | 2 × 2,70 + 1,50 + tablón 0,10 | L `4-7` y L `7-10` |
| 4 | 4,750 | 12,83 | 2,70 + 1,95 + tablón 0,10 | L `4-7` y T `4-49-52` |
| 49 | 3,500 | 9,45 | 2,70 + 0,75 + tablón 0,05 | L `34-49` y T `4-49-52` |
| 52 | 7,000 | 18,90 | 2 × 2,70 + 1,50 + tablón 0,10 | T `4-49-52` y L `46-52` |
| 46 | 9,000 | 24,30 | 3 × 2,70 + 0,90 | L `43-46` y L `46-52` |
| 43 | 14,750 | 39,83 | 5 × 2,70 + 1,20 + tablón 0,05 | L `40-43` y L `43-46` |

En los dieciocho tramos, una cara suma 38 módulos de 2,70 y 17 módulos finales (el 13 cierra justo en cinco de 2,70). Las dos caras doblan eso: 76 módulos de 2,70. Si cada uno se llena con tres paneles de 0,90, son 228 paneles de 2,70 × 0,90, más los módulos finales, los tablones, las 11 esquinas en L, las 3 T y el cruce. Eso sería encofrar el plano de una vez. Por fases, el juego es el de la fase grande.

La guía, en el panel de 2,70 × 0,90, da 0,82 tirantes por m² de cara. En una fase de unos 50 m² de muro (100 m² de cara) son del orden de 80 tirantes. En el plano entero, del orden de 550. Es la tasa de la guía, no un recuento del visor. Grapa, rigidizador y estabilizador salen de las juntas y de las casillas del muro; el archivo no trae la cantidad.

## Un orden para seguir hablando

Se entra por el único extremo libre, el muro 1, y cada fase sigue pegada a lo ya puesto. La fase se corta cerca de 40 a 50 m². Dos se pasan un poco porque el tramo de al lado, si se separa, deja una jornada corta y una esquina a medias.

| Fase | Tramos, en orden | m² | Qué cierra |
|---:|---|---:|---|
| 1 | 1 y 22 | 43,88 | El espolón, desde (0, 0) hasta el cruce. La T queda sin el 28. |
| 2 | 28, 25, 31 y 19 | 49,28 | Las dos T del eje x = 11250 y el cierre hasta el 19. |
| 3 | 34, 37 y 40 | 45,78 | Las tres patas que faltan del cruce. |
| 4 | 16 y 13 | 56,70 | Desde el 19, baja y recorre el muro sur. Pasa de 50. |
| 5 | 10, 7 y 4 | 48,60 | El contorno sureste, hasta la T. |
| 6 | 49, 52 y 46 | 52,65 | La T `4-49-52` y la subida este. Pasa un poco de 50. |
| 7 | 43 | 39,83 | El muro norte, entre el cruce y el 46. |

La fase 4 son 56,70 m² porque el 13 ya son 36,45 y el 16, que es el tramo anterior, 20,25. Separarlos deja el 16 en una jornada de 20 m². La 6 agrupa la T con el 46 para no dejar la unión `4-49-52` partida.

El juego de una fase, en el caso más alto de esta lista, anda por 57 m² de muro, unos 114 m² de cara. El plano entero son 673 m² de cara. La diferencia es el motivo de hacerlo por fases: un solo juego da una puesta al día y al día siguiente pasa al tramo que toca.

## Qué queda por decidir

El orden de la tabla es una lectura del plano para seguir hablando. No es la función. Falta acordar si una fase puede pasar de 50 m² cuando el tramo siguiente es corto, y si una T o un cruce tienen que cerrarse en la misma puesta o pueden completarse al día siguiente, como en las fases 1 y 3.

El cuadro de abajo solo anota cuántas fases, cuántos equipos y el muro por el que arranca cada uno. No reparte el resto de los tramos.

## Cuadro de fases y equipos

29 oct 2026, 15:25. El mismo cuadro del recuento (cabecera, pestaña, tabla con fila azul). A las 16:51 cada equipo se rellena en esa tabla y el muro de inicio se elige en un visor Three.js, abajo del todo.

El botón es el de las capas, en la barra superior, a la izquierda de exportar e importar.

1. Al abrir, el cuadro es pequeño: solo pregunta en cuántas fases se encofra y cuántos equipos van a trabajar. Las dos cifras son enteros de 1 en adelante.
2. Cuando las dos están escritas, el cuadro crece al tamaño del recuento y salen tantas filas como equipos. Cada fila tiene nombre (por defecto «Equipo 1»), número de trabajadores (por defecto 4), un color y el botón «Elegir muro».
3. El color de partida es un rosa apagado, `#c9a0a8`. Se puede cambiar en cada equipo.
4. «Elegir muro» abre abajo un visor Three.js con los muros del diseño. Un clic en un muro lo deja como inicio de ese equipo y lo pinta con su color. Otro equipo no puede coger el mismo muro.
5. Aceptar exige nombre, trabajadores y muro en todas las filas. Cancelar no deja la anotación a medias.
6. El aviso «Anotado» cierra el cuadro. El reparto de los demás tramos no se hace todavía.

No puede haber más equipos que tramos. Las fases sí pueden ser menos o más que los equipos: dos equipos y siete fases quieren decir que el primer día arrancan dos, y las puestas siguientes siguen con ese material.

El juego que se trae es el máximo de cada pieza entre todas las fases, no la suma. En una fase, la pieza que solo hace falta en otra se queda en el suelo. Vale igual si hay 3 fases o más. La cuenta usa la configuración de cada muro: alto, largo, tape de cada extremo, puntal, rigidizador y ménsula. La esquina solo se monta en la fase que reúne sus muros.

## Estudio: primero los m², después las esquinas

30 oct 2026, 11:09. El plano de ejemplo sigue siendo el 30008, ahora en 3 fases porque es el número que se ha pedido en el cuadro. El botón todavía no usa este corte. Es el estudio. La experiencia de obras ya hechas y la planificación a mano vendrán después y pueden mover el corte.

El orden es este:

1. Se parte el plano por metros cuadrados. Con 336,71 m² y 3 fases, cada fase busca unos 112 m². Se entra por el muro de origen (el 1) y se añade el tramo de al lado que más acerca la fase a 112 sin pasarse, mientras queden tramos que quepan.
2. Después se miran las esquinas que el corte ha dejado partidas. Cerrar una esquina solo compensa si las fases siguen cerca de 112. Si para cerrarla hay que meter en la fase 1 un tramo grande, la esquina espera a la fase siguiente.
3. El material de una fase es el de sus muros, no el del plano entero. El camión lleva el máximo de cada pieza entre las fases. Lo que esa fase no monta se queda en el suelo. La fase 1 no tiene por qué gastar el juego completo ni llevarse las 15 esquinas.

### Corte por m², antes de tocar las esquinas

| Fase | Tramos | m² | Distancia a 112 |
|---:|---|---:|---:|
| 1 | 1, 22, 34, 40, 37, 28, 49 | 109,13 | 3 |
| 2 | 43, 52, 31, 4, 25, 19 | 110,83 | 1 |
| 3 | 16, 13, 10, 7, 46 | 116,78 | 5 |

La fase 1 cierra entera la T `1-22-28`, el cruce `22-34-37-40` y la L `34-49`. Son 3 esquinas, no las 15. El resto del juego de esa fase es el de esos siete tramos.

### Esquinas que el corte deja abiertas

En cada fila, «cerrar» es meter los brazos que faltan en la fase que ya tiene uno. El número es cuánto se alejan las dos fases de 112 si se hace. Dejarla abierta es el corte de la tabla de arriba.

| Esquina | Cómo queda | Si se cierra | Si se deja |
|---|---|---:|---:|
| L `40-43` | 40 en fase 1, 43 en fase 2 | 78 | 4 |
| T `25-28-31` | 28 en fase 1, 25 y 31 en fase 2 | 43, o 22 si el 28 pasa a la fase 2 | 4 |
| T `4-49-52` | 49 en fase 1, 4 y 52 en fase 2 | 62, o 20 si el 49 pasa a la fase 2 | 4 |
| L `46-52` | 52 en fase 2, 46 en fase 3 | 43 | 6 |
| L `4-7` | 4 en fase 2, 7 en fase 3 | 32 | 6 |

En este plano, con 3 fases iguales, cerrar esas cinco esquinas empeora el reparto de m². La mejor solución de este estudio es dejarlas para la fase siguiente. En obra, el tape de la salida cierra el hormigón y la fase siguiente solapa unos 0,15 m. No hace falta meter la esquina entera en la fase 1 solo por existir.

La T `1-22-28` y el cruce sí caen dentro de los 112 m² de la fase 1, así que ahí sí van enteros. No se buscan más esquinas para rellenar la fase.

### Material

Cada fase monta sus tramos. Unos 109, 111 y 117 m² de muro, el doble en cara de panel. El juego que viaja es el de la fase mayor, cerca de 117 m² de muro, no los 337 del plano ni la suma de las tres. En la fase 1, el panel que solo entra en la 2 o en la 3 está en el suelo.

Qué pieza conviene de verdad en cada fase no se cierra aquí. Hace falta la experiencia de diseños ya puestos. Hasta entonces el estudio no cambia el botón.

### Planificación a mano

Lo siguiente, aparte de esos datos, es poder pintar las fases a mano: elegir qué muros van en cada fase, ver el color y el material de esa elección, y guardarla. El estudio de arriba es una propuesta automática. La manual será la que mande cuando no coincidan.
