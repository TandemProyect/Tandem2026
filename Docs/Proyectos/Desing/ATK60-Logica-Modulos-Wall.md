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

## Conexiones de los extremos (26 oct 2026)

El reparto de arriba sigue siendo la base. El tablon de 0,15 del eje depende de cuantos extremos tocan otro muro. Se cuenta antes del recorte: despues los extremos ya no se tocan.

1. Ninguna conexion. Muro suelto.
2. Una conexion, en un solo extremo.
3. Conexion en los dos extremos.

Casos 1 y 2. Si el largo que se empaqueta es multiplo de 0,15, no hay tablon. El desencofrado sale por el tape. Si el reparto base dejaba un tablon de 0,15, se abre un 2,70: ese 2,70 mas los 0,15 pasan a un modulo de 2,40 y un panel de 0,45.

Ejemplo, muro suelto de 8,25 m:

- dos modulos de 2,70
- un modulo de 2,40
- un panel de 0,45
- sin tablon

Caso 3. Se queda el reparto de ahora. Un 8,25 con las dos conexiones sigue en tres de 2,70 y un tablon de 0,15, salvo el recorte de las esquinas. El tablon de la esquina por diferencia de espesor es otro remate: va en la cara interior, entre la esquina interior y el panel, con grapa variable. No se quita por esta regla.

Codigo: `CountConnectedEnds` y `PackAtk60LengthMm` en `Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs`.

## Estado
- Reparto por conexiones cerrado el 26 oct 2026, 10:51.
- El caso 3 (las dos conexiones) conserva el tablon de 0,15 del eje.
