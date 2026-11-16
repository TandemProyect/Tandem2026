# 01 - Requisitos y alcance v1

## Requisitos funcionales
- RF01: Leer geometria 2D del dibujo (por seleccion manual o por capa).
- RF02: Construir modelo topologico de muros y uniones.
- RF03: Clasificar nodos en L / T / + con orientacion.
- RF04: Seleccionar sistema de encofrado (v1: ATK60 o generico).
- RF05: Generar solidos 3D de visualizacion a partir del modelo + sistema.
- RF06: Conservar el 2D original sin modificarlo.
- RF07: Permitir regenerar el 3D si cambia el sistema o los parametros.
- RF08: Persistir IDs y parametros en el DWG (XData o ExtensionDictionary).

## Restricciones iniciales (v1)
- Geometria principalmente ortogonal (0/90 grados).
- Tolerancias de cierre/encuentro a definir por proyecto.
- Solo muros rectos (sin arcos ni curvas).

## Fuera de alcance (v1)
- Puertas, ventanas, huecos.
- Geometria curva o no ortogonal.
- Resolucion automatica de dibujos con gaps grandes sin reglas claras.
- Panelizacion/despiece detallado (fase posterior).

## Criterio de exito v1
Dado un dibujo 2D ortogonal con muros y encuentros L/T/+,
el plugin debe generar el modelo topologico correcto y los solidos 3D
de un muro recto y una esquina L con el sistema seleccionado.
