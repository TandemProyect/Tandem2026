# ATK60 - Logica de Modulos Wall (base para GetWallsForCadSystems)

## Contexto
Esta nota define la base funcional para implementar la logica de `GetWallsForCadSystems` en C#.

Objetivo: a partir de la longitud de cada muro, descomponer en modulos ATK60 validos usando el catalogo de paneles disponible.

## Clase objetivo
Se define como clase de trabajo:

- `ModulosAtk60Wall`

Responsabilidad esperada:
- Recibir la longitud util del muro.
- Calcular la combinacion de modulos/paneles ATK60 necesaria.
- Dejar salida reutilizable para CADs (Revit, BricsCAD, AutoCAD, etc.).

## Catalogo de paneles ATK60 (alto x ancho, en metros)

### Altura 2,70
- 2,70 x 0,30
- 2,70 x 0,45
- 2,70 x 0,60
- 2,70 x 0,75
- 2,70 x 0,90

### Altura 2,40
- 2,40 x 0,30
- 2,40 x 0,45
- 2,40 x 0,60
- 2,40 x 0,75
- 2,40 x 0,90

### Altura 1,20
- 1,20 x 0,30
- 1,20 x 0,45
- 1,20 x 0,60
- 1,20 x 0,75
- 1,20 x 0,90

## Modulos objetivo de longitud (m)
Con los anchos disponibles, se generan modulos (paso 0,15):

- 2,70
- 2,55
- 2,40
- 2,25
- 2,10
- 1,95
- 1,80
- 1,65
- 1,50
- 1,35
- 1,20
- 1,05
- 0,90
- 0,75
- 0,60
- 0,45
- 0,30

## Regla base acordada (fase actual)
1. Tomar la longitud del muro.
2. Obtener la secuencia de modulos ATK60 necesarios.
3. Mapear esa secuencia contra paneles permitidos del catalogo.

## Regla longitudinal del muro (importante)
En su longitud, el muro se divide en tres partes:

1. Inicio + bucle de modulos base
2. Modulo final
3. Remate (madera)

### Definiciones
- Longitud del muro: `L`
- Modulo base de bucle: `2,70 m`
- Catalogo de modulos finales validos: `2,70, 2,55, 2,40, 2,25, 2,10, 1,95, 1,80, 1,65, 1,50, 1,35, 1,20, 1,05, 0,90, 0,75, 0,60, 0,45, 0,30`

### Algoritmo (fase actual, simple y reproducible)
1. Calcular cuantos modulos de `2,70` caben completos:
	- `n = floor(L / 2,70)`
2. Calcular resto despues del bucle:
	- `r1 = L - (n * 2,70)`
3. Elegir modulo final `mf` como el mayor modulo de catalogo que cumpla `mf <= r1`.
4. Calcular remate:
	- `remate = r1 - mf`
5. Resultado longitudinal:
	- `n` modulos de `2,70`
	- `1` modulo final `mf`
	- `1` remate de madera `remate`

### Ejemplo aportado (muro de 10,542 m)
1. `L = 10,542`
2. `n = floor(10,542 / 2,70) = 3`
3. Longitud cubierta por bucle:
	- `3 * 2,70 = 8,10`
4. Resto:
	- `r1 = 10,542 - 8,10 = 2,442`
5. Modulo final mayor que cabe en `2,442`:
	- `mf = 2,40`
6. Remate:
	- `remate = 2,442 - 2,40 = 0,042`

Resultado final del ejemplo:
- 3 modulos de 2,70
- 1 modulo final de 2,40
- 1 remate de 0,042

## Notas para implementacion C#
- Trabajar internamente en milimetros para evitar errores de coma flotante:
  - `2,70 m = 2700 mm`, `0,042 m = 42 mm`, etc.
- Aplicar tolerancia de redondeo al comparar modulos (por ejemplo, 1 mm).
- Mantener esta version como estrategia inicial funcional antes de optimizaciones.

> Nota: la estrategia de optimizacion (minimo numero de paneles, criterio de cortes, prioridad por alto/ancho y reglas de montaje) se definira en siguientes iteraciones.

## Punto de implementacion
Metodo objetivo en repositorio ATK60:

- `GetWallsForCadSystems(walls)` en `Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs`

## Conexiones de los extremos (28 oct 2026)

El apartado del 26 oct, que quitaba el tablón en 0 y 1 conexión, ya no vale.

El reparto de módulos de arriba sigue siendo la base. El tablón se cuenta antes del recorte.

Si el largo que se empaqueta es múltiplo de 0,15, el muro lleva el taco de 0,15, tenga ninguna conexión, una o las dos. A lo largo no pasa de 0,15 m. El ancho es 0,12 m, el del panel. Grapa variable en los dos cantos.

Si el sobrante pasa de 0,15, el módulo final sube un paso de 0,15. Si entre 0,16 y 0,29 no cabe un panel de 0,30, se abre un 2,70 en 2,55 + 0,30 y el taco baja 0,15. El taco de 0,15 justo no se absorbe.

Si en el mismo muro hay otro remate y uno está en la esquina, los paneles de en medio se corren y el hueco del tramo se cierra. El tablón de la esquina y el de la otra cara se quedan, cada uno con su largo. Ninguno pasa de 0,15. No se suman: 0,12 más 0,15 no es un tablón de 0,27. Lo que pasa de 0,15 entra en el módulo, no en la madera. Los dos se acotan.

En L, el remate del tramo va pegado a la esquina, en las dos caras y enfrentado: `run − remate` si la L está al final del eje, `0` si está al inicio, y los módulos se corren (lead/trail). Con L en los dos extremos va al inicio. Las dos caras comparten módulos, así que ese remate es el mismo en las dos. La cara interior lleva además el tablón de la propia L (módulo − (espesor del otro + 0,30)), entre ese remate y la pata del panel de esquina. En la cara interior es **un solo tablón**: si el remate del tramo y el de la L caben juntos (≤ 0,15), se juntan en uno, con grapa variable solo en sus dos cantos (`PlaceInteriorWood` en `LCornerFormwork`). Ej.: 0,06 del tramo + 0,08 de la L → interior `… módulo · 0,14 · pata L`, exterior `… módulo · 0,06 · módulo de fuera`. Si pasan de 0,15 quedan los dos (pendiente de decidir con Juan). Nunca encima de la pata de `E27004210` ni del módulo de fuera, y sin hueco entre el último módulo y la esquina. Ej.: L 0,30/0,30, tramo 5,05 → exterior `… 0,45 · 0,10 · 0,60`, interior `… 0,45 · 0,10 · pata L`. Qué extremo es L lo decide `LCornerFormwork.CornerEnds`, con el mismo criterio que pinta la esquina (no `CornerFaces`). Reglas puras: `Atk60FaceRemateRules.CornerRemate`.

No volver a abrir el tope. `EndFiller` recorta a 0,15 aunque quien llame pase 0,60. El pintado solo dibuja un remate de más de 1 mm y como mucho 0,15 (`remateMm <= 150`). El filtro antiguo `remateMm < 300` es el que dejaba salir el 0,27. `PackAtk60LengthMm` no guarda un resto mayor de 0,15: si el muro es más corto que 0,30 y no hay un 2,70 que abrir, el tablón se queda en 0,15 y el resto no se pinta como madera.

Código: `CountConnectedEnds` y `PackAtk60LengthMm` en `Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs`. La suma en la esquina está en `SumRematesAtCorner` de `LCornerFormwork.cs`. La T de tipo 2 está en `Docs/areas/encofrado-atk60/esquina-T-atk60.md`.

## Estado
- Reparto por conexiones del 26 oct 2026 sustituido el 28 oct 2026.
- El múltiplo de 0,15 conserva el tablón en 0, 1 y 2 conexiones.
