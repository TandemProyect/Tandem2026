# Esquina en T AT-60

Escrito el 28 de octubre de 2026 con lo que quedó aceptado ese día. El tipo 3 está nombrado y no está definido: no se inventa. El 29 oct quedó pendiente el remate cuando los espesores no son iguales, en la T y en la cruz: está en «Pendiente», al final. No se arregla de paso. La cruz de igual espesor ya está aceptada.

La L cerrada sigue en `esquina-L-atk60.md`. Las grapas y el tablón, en `agente-grapas-atk60.md`. El reparto del eje, en `Docs/Proyectos/Desing/ATK60-Logica-Modulos-Wall.md`.

Código vivo:

- Encofrado: `Desing/Repositories/Repository_Desing_2/Atk60/LCornerFormwork.cs` (`PlaceTPair`, `PlacePlus`, `TryTipo2Step`, `ShiftWall`, `PlaceTPanel`, `PlaceStepRemate`)
- Reparto del eje: `PackAtk60LengthMm` en `Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs`
- Muro 3D fino: `maStlAtk60ThinThroughShiftByAxisId` en `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`. El signo no se invierte.

## Qué es una T

Dos tramos colineales y un tercero que llega al hueco. Si hay un tercer muro a menos de 1,10 m del vértice, no se pinta también una L. El giro de los dos paneles de esquina es `Math.PI` en los dos. No se vuelve a girar.

Cara E: el panel que cierra el testero de la rama. Mide el espesor de la rama más 0,30 más 0,30. Sin angular. Si cabe en 0,90, un panel y el remate al final. Si pasa de 0,90, dos paneles y el remate en medio. Grapa fija de unión donde el recto toca la esquina y donde toca la cara E, en las dos caras.

## Tipo 2

Los dos atravesados no miden igual. El ejemplo que se dejó es 0,22 contra 0,30. El escalón es la diferencia de espesor.

Caras, como quedaron el 28 oct:

- Cara A y Cara B van en la misma recta. El escalón no está en esa cara.
- El escalón está solo en la cara de la rama.
- Cara D es la del brazo grueso hacia la rama y no se mueve.
- Cara C apoya en el brazo fino.
- Cara Y baja hasta Cara X, la cara de fuera. El muro fino se corre hacia fuera, no hacia la rama.
- El muro vertical termina en el extremo del vástago.

Encofrado:

- El horizontal del muro fino se pega a Cara A.
- La esquina de ese lado se inserta en la intersección del escalón. El lado fino va en Cara A. El grueso, en Cara D.
- El hueco entre las dos caras se cierra con tablón. El largo es el espesor que falta y no pasa de 0,15 m. El ancho es 0,12 m, el del panel. Grapa variable en los dos cantos. La grapa fija de ese hueco no se pone: no une dos paneles.
- El tablón queda entre la esquina interior y el panel vertical, justo después de la pata de 0,30 m (0,294 m en el panel de 1,20).

El muro fino se corre `(espesor grueso − espesor fino) / 2`, alejándose de la rama. Junto a la esquina no se corre: el panel y el remate se metían en el muro. Tampoco se corre el panel recto cuyo cuerpo llega a la esquina, aunque el ancla esté a más de 0,80 m. Los remates (`Remate`, `REMATE_WOOD`) y las grapas variables (`10000221`, `10000221B`) no se corren. El resto del muro fino sigue en Cara A.

## Remate

Un remate es un hueco. Ahí entra un tablón y grapas variables en los dos cantos.

- A lo largo del muro el tablón no pasa de 0,15 m. Es lo que deja la modulación de 0,15.
- El ancho es 0,12 m, la cara del panel. Un tablón de 0,30 a lo largo no vale.
- Si el largo es múltiplo de 0,15, el muro lleva ese taco, tenga ninguna conexión, una o las dos. La regla del 26 oct que quitaba el taco en 0 y 1 conexión ya no vale.
- Si el sobrante pasa de 0,15, el módulo final sube un paso de 0,15. Si no cabe un panel de 0,30, se abre un 2,70 en 2,55 + 0,30 y el taco baja 0,15. El taco de 0,15 justo no se absorbe.
- En la L el módulo de fuera ya cierra el extremo: espesor del otro más 0,30, al módulo siguiente. Si esa cuenta deja resto, el tablón va solo en la cara de dentro y no pasa de 0,15. No se vuelve a medir ese extremo para poner otro panel y otro tablón. Eso es lo que salía junto a la esquina: el tablón estrecho y el panel de más.
- Si el eje tiene remate, el tablón y las grapas van en las dos caras. Un 0,07 que ya se acotaba y solo salía en una cara sale en las dos.
- El panel recto de un extremo en L va solo en la cara de fuera. No tapa el hueco interior.

- Si el muro tiene remate en la esquina y otro en el tramo, los paneles que hay entre los dos se corren y el hueco del tramo se cierra. Cada tablón conserva su largo. Ninguno pasa de 0,15. No se suman los dos huecos. La grapa variable queda con el golpe arriba. La fija de ese tramo no se pone. Los dos se acotan.

## Visor

En Develop, solo para Juan:

- «Puntos y leyendas» sale marcada. Al quitarla desaparecen puntos y leyendas.
- «Oculta muro» sale sin marcar. Al marcarla desaparecen el muro recto y la esquina. El encofrado se queda.

El color del muro y el de la esquina (`#A9F527`) se colaban en el fenólico al alejar o al mover la cámara. El muro y la esquina entran 6 mm (`MA_STL_WALL3D_FACE_INSET_MM`). El encofrado se pinta después (`renderOrder` 145). La cámara de perspectiva ya no usa un near de 1 mm con un far de 10 km.

## Lo que ya se probó y no vale

- Volver a girar los dos paneles de la T.
- Invertir el corrimiento del muro fino, ni en C# ni en el visor.
- Correr el panel que llega a la esquina: entra en el muro. Correr el remate o la grapa variable de ese recorte los saca de sitio.
- Borrar todos los remates a menos de 0,80 m de la T para arreglar ese panel.
- Un taco de 0,15 × 0,15, o uno de 0,30 a lo largo. El ancho es 0,12.
- Sumar el remate de la esquina y el del tramo en un solo tablón. 0,12 + 0,15 daba 0,27. El tope es 0,15. Lo que pasa entra en el módulo.
- Subir el tope de `EndFiller` a 0,60, o pintar el remate si mide menos de 0,30. Ese filtro dejaba salir el 0,27.
- Quitar el taco cuando el muro no tiene las dos conexiones.
- Poner el panel recto del extremo también en la cara interior de la L: tapa el hueco.
- Medir otra vez el extremo cuando el módulo de fuera ya está puesto, y añadir ahí otro panel y otro tablón. El tablón estrecho de la foto salía de esa segunda medida.
- Implementar el tipo 3 sin definición.
- Arreglar de paso el remate de la T o de la cruz cuando los espesores no son iguales. Está pendiente y se hace aparte. El intento del 29 oct por caras quitó el tablón largo y desplazó el resto.

## Cruce en +

Cuatro brazos, dos ejes. En planta es una cruz. Las esquinas interiores son las de la T, y al otro lado se repite la misma pareja. Son cuatro paneles de esquina interior: dos simétricos a un lado del eje y los otros dos al lado contrario.

No lleva cara E. No lleva angular. El panel recto de la T cierra un testero que aquí no existe, porque el muro sigue. Cada brazo sí lleva su tape: entre las dos esquinas el canto queda abierto y, sin ese cierre, el hormigón se escapa. Si el muro de enfrente no entra en esta fase, ese canto también se cierra con tape.

Cada brazo se retira medio espesor del eje que cruza, más 0,30 m. Ahí apoya la pata del panel de esquina. `TryPlusCornerTrimMm` en `Atk60WallsRepository`. El recorte de 0,45 m de un extremo suelto no vale en la cruz: en un muro de 0,30 coincide por casualidad y en otro espesor no.

Con el mismo espesor en los cuatro brazos quedó aceptado el 29 oct. El panel sale al medio de cada muro.

Si los espesores no coinciden, el remate del escalón no se pone. Es el mismo bug que en la T y sigue pendiente. No se arregla al publicar la cruz.

## Pendiente

Arreglar el bug de los remates cuando los espesores no son iguales. Vale para la T y para la cruz. En la T es el tipo 2: los dos atravesados no miden igual. El ejemplo es 0,22 contra 0,30. El escalón es la diferencia, 0,08, y ese tablón es más corto que el remate de la otra cara. En la cruz, con espesores distintos, el fallo es el mismo: el tablón del escalón no está resuelto. La cruz de espesor igual ya está aceptada y no se toca para arreglar esto.

El 29 oct la posición de uno quedó bien y el largo se cruzó con el otro. Sacar cada remate por su cara quitó el tablón largo y movió los demás. No se repite ese cambio.

Hasta que se abra este bug, se deja como está. No se suma el hueco de la esquina con el del tramo. Ningún tablón pasa de 0,15. El corrimiento de paneles que cierra el hueco del tramo se queda. No se vuelve a medir el extremo de la L para poner otro panel y otro tablón.
