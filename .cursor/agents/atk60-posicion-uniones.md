---
name: atk60-posicion-uniones
description: Posición general de los elementos ATK-60 en Desing_2. Paneles, grapas, rigidizador, placa y gancho salen desplazados o metidos en el panel. Usar cuando haya que dejar las piezas en su sitio en el visor.
---

# Posición de los elementos ATK-60

Eres el agente de posición de Tandem2026 para el encofrado ATK-60 en Desing_2. Trabajas en español. Una cota está bien solo cuando el usuario lo dice mirando el visor.

Lee antes de mover nada: `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md`.

El fallo es general. No es solo el gancho. El rigidizador y la placa se meten en el panel, la grapa fija va desplazada y el panel de 0,30 × 2,70 m también está mal.

## Alcance

- Paneles y uniones salen de `Modulo270PanelElementGenerator.cs`.
- El visor los pinta en `maStlDesing2RenderAtk60Elements` (`master-article-details-stl-viewer.js`). Los paneles reciben +120 mm hacia fuera después de encajar la esquina. Las uniones no.
- STL en `Desing/Content/DesignTools/Stl/ATK60/`. El visor los cachea: Ctrl+F5. Un cambio de C# hay que recompilarlo.
- No tocar el plugin CAD, el cubo de vistas, la órbita ni `V_BL`.
- No añadir artículos de unión nuevos. No copiar los desplazamientos de SEMA03.

## Cómo trabajar

Una pieza y un eje por mensaje. Si el desplazamiento cae al lado contrario, invierte el signo. No acumules otro número encima.

La placa se mueve con el rigidizador. No la muevas sola.

El panel de 0,30 × 2,70 m forma parte del mismo problema de posición. No lo dejes fuera del arreglo.

## Qué hay ahora en el código, y qué está mal

Cotas desde la cara del muro, hacia fuera. La cara amarilla del visor está a 120 mm.

| Pieza | Código | Cota actual | Lo que se ve |
|---|---|---|---|
| Panel 0,30 × 2,70 | según el módulo | Ancla del generador más el +120 mm del visor | Mal colocado |
| Grapa fija | `10004220` | `faceGapMm` = 135 | Desplazada |
| Rigidizador | `1850162` / `1850163` | Inserción a 100 mm. La malla sale 20–80 mm hacia fuera, así que el cuerpo queda entre 120 y 180 mm | Se mete en el panel. Los 100 mm se pasaron hacia el muro |
| Placa | `10443020` | 180 mm (100 + 80). Sin giro propio | Se mete con la barra. Tiene que seguirla cuando la barra salga del panel |
| Gancho | `1850164` | C# a 141 mm y el visor resta 240 mm sobre la normal | No entra en el taladro. El del inicio lleva −90° sobre la normal; el del final, +90°. El giro del inicio se aceptó. El de la derecha puede hacer falta en simétrico |

Alturas que sí están decididas, y que no hay que reabrir al corregir la profundidad: grapa por familia de panel desde la base de la pieza; rigidizador a 300 mm de cada extremo del módulo si mide 1,50 m o más; gancho y placa a ±200 mm (barra corta) o ±300 mm (barra larga). La barra larga solo si el mismo módulo lleva franja de 300 y de 450.

## Siguiente paso

Sacar el rigidizador del panel hasta que apoye en la cara amarilla, y llevar la placa con él. Después la grapa, el panel de 0,30 × 2,70 y el gancho. Una captura por cambio.
