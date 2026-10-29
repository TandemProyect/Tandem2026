# Esquina en L AT-60 — método de puntos

Cerrado el 25 de octubre de 2026. La T, el cruce en + y los pilares embebidos se hacen **igual**: primero el muro 3D, luego puntos y líneas, y la pieza solo se mueve cuando el punto ya está marcado.

Código vivo:

- Pintado: `Desing/Repositories/Repository_Desing_2/Atk60/LCornerFormwork.cs`
- Huella del muro: `maAtk60BuildLFootprintMm` en `Desing/Scripts/MasterArticles/ma-stl-atk60-formwork.js`
- Ayudas: `master-article-details-stl-viewer.js` (`maStlWall3dAddLCornerFaceGuides`, `maStlWall3dAddInteriorCornerPoints`)

## Cómo se guía la siguiente esquina

1. Generar el muro 3D. Las ayudas salen ahí, no al encofrar.
2. Dibujar las líneas numeradas sobre la cara de arriba y un punto rojo en el sitio al que tiene que llegar la pieza. No mover la pieza todavía.
3. Esperar a que ese rojo quede confirmado.
4. Dibujar el punto verde en la inserción actual de la pieza (su X/Z de pintado).
5. La inserción es el vértice que se marque sobre la pieza. Cambiar solo el offset local para que el verde pase a ese vértice. No usar el origen del STL ni el extremo contrario.
6. Cuando el verde ya es esa inserción, trasladar la pieza hasta que el verde coincida con el rojo.
7. No cambiar el giro si no se pide. El angular, si ya está bien, no se mueve.
8. Al encofrar se ocultan líneas, cotas y números. Los puntos se quedan.

## Quién ve las ayudas

Temporal, hasta que existan roles. Puntos, líneas, cotas y números solo si `meta[name="dt-user"]` es `juan.godoy@vscad.com` (`_LayoutMaterio.cshtml`, `User.Identity.Name`). Otro usuario no las genera, y si quedara alguna se oculta. El muro y el encofrado se ven igual.

## Lenguaje de la L

Orden de la huella, sin cambiarlo: 0 exterior, 1 exterior de la pata 1, 2 interior de esa pata, 3 esquina interior, 4 interior de la pata 2, 5 exterior de la pata 2.

`pts[3] = exterior − normal1 × espesor1 − normal2 × espesor2`. Las líneas 2 y 4 se cruzan ahí.

| Línea | Color | De dónde a dónde |
|---|---|---|
| 1 | cian `#00e5ff` | canto exterior, `pts[0]` → `pts[1]`. Cota 320 mm por fuera. |
| 2 | rojo `#ff1744` | interior, paralela a la 1, de `pts[0] − pts[1] + pts[2]` a `pts[2]`. Cota al lado contrario de la 1. |
| 3 | cian | el otro canto exterior, `pts[0]` → `pts[5]`. Cota 320 mm por fuera. |
| 4 | azul `#2962ff` | interior, paralela a la 3, de `pts[0] − pts[5] + pts[4]` a `pts[4]`. Cota al lado contrario de la 3. |

Esfera roja, radio 15 mm, en `pts[3]`. Esfera verde, `#00c853`, radio 15 mm, en el X/Z de pintado del panel de esquina interior. Una verde por cada X:Z redondeado. La Y de la verde es la de la roja. Las dos van 15 mm por encima del muro.

## Muro

La cara a lo largo de un eje mide `selectPanel(espesor del otro + 300 mm)`. 0,47 + 0,30 = 0,77, así que esa cara es 0,90. Un espesor de 50 o menos está en metros y se pasa a milímetros. El muro gris usa el mismo recorte. El hueco de la esquina interior no se tapa con paneles de muro. El recorte del extremo conectado sigue en 450 mm. No volver al recorte temporal de 300 mm en la L.

## Angular

Queda donde está. El talón está en el cruce de las dos caras exteriores (`cornerX` / `cornerZ` de `FaceCorner`) y las alas salen hacia fuera. Códigos `27000000`, `24000000`, `12000000`. No se le aplica el offset de la esquina interior.

## Panel de esquina interior

Códigos `E27004210` (2,70), `E24004217` (2,40) y `E12004216` (1,20). La piel `_F` lleva el mismo offset. No se reescala (`keepNativePillarPanel`).

Planta del STL, en mm, después de pasar el ASCII de metros a milímetros:

- 2,70 y 2,40: (0, 0), (120, 0), (300, 180), (300, 300), (0, 300). La muesca entra por (120, 180).
- 1,20: la misma L con la Y corrida −6 mm. El extremo es (0, 294).

Las caras fenólicas del bastidor están en X = 0 e Y = 0 y se cruzan en (0, 0). Ese origen **no** es la inserción.

La inserción aceptada es el extremo de la pata que se marcó: local **(0, 300)** en 2,40 y 2,70, y **(0, 294)** en 1,20. El offset es (0, −300) o (0, −294), así ese vértice cae en el X/Z de pintado. El verde se dibuja en ese X/Z, no después del offset.

Ese X/Z es la esquina interior:

`esquina exterior + hacia el cuerpo del muro × su espesor + hacia el cuerpo del otro × su espesor`

Es el mismo punto que `pts[3]` cuando la esquina exterior es el cruce del angular. El desplazamiento de más que restaba el espesor del muro vertical en X se quitó: era lo que dejaba el verde fuera del rojo.

Giro, sin tocarlo: `PanelYaw` con las dos normales hacia el cuerpo, y luego −90°. Pose: `RotX = −π/2`, `RotZ = yaw`, `UseStrictPose`.

## Grapas

Cada vez que dos paneles se tocan, esa junta lleva una grapa fija, en las dos caras, a la altura del manual. El detalle está en `agente-grapas-atk60.md`, apartado «Dos paneles juntos».

La grapa solo va donde dos paneles se tocan, o en el canto del angular. A 300 mm del angular, si no hay otra pieza, es el vano: ahí no se pone. Si la cara se parte en varios paneles, cada corte lleva la suya.

El tablón del sobrante de la esquina va en la cara interior, entre la esquina interior y el panel, con grapa variable en los dos cantos. No va en el rincón del angular. Ese sobrante es el de la cuenta del módulo (espesor del otro más 0,30, al módulo siguiente). Si la cuenta da 0, no hay tablón. No se mide otra vez el extremo para poner un panel y un tablón de más: el módulo de fuera ya cerró ese sitio. El detalle está en `agente-grapas-atk60.md`. El del eje mide igual desde el 28 oct 2026: como mucho 0,15 m a lo largo y 0,12 m de ancho, también si el muro no tiene las dos conexiones. Está en `Docs/Proyectos/Desing/ATK60-Logica-Modulos-Wall.md`.

## Lo que ya se probó y no vale

- Llevar el origen del STL (0, 0) al rojo, sin cambiar antes qué vértice es la inserción.
- Usar el local (300, 300) como extremo. Ese es el otro lado de la L.
- Volver a restar el espesor del muro vertical en X después de tener el extremo bueno.
