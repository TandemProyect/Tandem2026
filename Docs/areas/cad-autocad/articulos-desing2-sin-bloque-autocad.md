# Artículos de Desing 2 sin bloque en AutoCAD

21 de octubre de 2026.

AutoCAD ya tiene bloque de los 15 paneles universales (0,30, 0,45, 0,60, 0,75 y 0,90 m, en 1,20, 2,40 y 2,70 m), en las carpetas `3D`, `3DRef` y `Xr`. El visor de Desing 2 coloca además estas piezas, y de esas no hay DWG.

Los paneles tumbados y la cara de fenólico (`_F`) son el mismo bloque, girado o con otra cara. No hacen falta otros.

## Uniones y cierre

| Código | Pieza |
|--------|-------|
| `10004220` | Grapa fija |
| `10000221` | Grapa variable, cuerpo |
| `10000221-2` | Grapa variable, corredera. El visor la guarda como `10000221B` |
| `1850162` | Rigidizador corto |
| `1850163` | Rigidizador largo |
| `1850164` | Fijador del rigidizador |
| `10443020` | Tuerca placa campana |
| `1920811` | Gancho de cierre |
| `1920894` | Placa de ala del tirante |
| `7238001` | Tuerca hexagonal |
| `dywidag` | Barra del tirante. Es un solo STL; la longitud cambia con el muro |

## Ménsula, madera y angulares

| Código | Pieza |
|--------|-------|
| `4120000042` | Ménsula |
| `Tablon24x15x5` | Tablón de la plataforma |
| `Tablon24x15x5-2` | Quitamiedos |
| `EEx12000000` | Angular de 1,20 m |
| `EEx24000000` | Angular de 2,40 m |
| `EEx27000000` | Angular de 2,70 m |

El remate de madera no tiene STL: el visor lo genera a la medida del hueco.

## Puntales

Cada altura son dos piezas, cuerpo y tubo interior. Varias alturas comparten el mismo código de artículo (`HTipo0`, `HTipo1`), así que el bloque tiene que salir del fichero, no de ese código.

| Cuerpo | Tubo | Altura |
|--------|------|--------|
| `h045` | `h045_2` | 0,45 m |
| `h060` | `h060_2` | 0,60 m |
| `h075` | `h075_2` | 0,75 m |
| `h090` | `h090_2` | 0,90 m |
| `h110` | `h110_2` | 1,10 m |
| `h120` | `h120_2` | 1,20 m |
| `h150` | `h150_2` | 1,50 m |
| `h180` | `h180_2` | 1,80 m |
| `h195` | `h195_2` | 1,95 m |
| `h210` | `h210_2` | 2,10 m |
| `h225` | `h225_2` | 2,25 m |
| `h240` | `h240_2` | 2,40 m |
| `h270` | `h270_2` | 2,70 m |
| `h300` | `h300_2` | 3,00 m |
| `h375` | `h375_2` | 3,75 m |

A partir de 1,20 m entra además la pieza de abajo: `h270Inf` y `h270Inf_2`. En 3,75 m esa pieza es `h375Inf` y `h375Inf_2`. Por encima de 4,95 m ya no hay tubo interior: `Puntal_Tipo3`, `Puntal_Tipo3-2` y `Puntal_Tipo3-41`, con la base `Puntal_Tipo3Inf`.

## Esquinas

Hay STL de esquina y tampoco tienen bloque, pero el encofrado nuevo todavía no las coloca.

| Código | Pieza |
|--------|-------|
| `E12004216` | Esquina de 1,20 m |
| `E24004217` | Esquina de 2,40 m |
| `E27004210` | Esquina de 2,70 m |
