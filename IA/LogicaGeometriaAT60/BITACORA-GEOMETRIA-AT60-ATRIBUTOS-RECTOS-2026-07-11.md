# BITACORA GEOMETRIA AT60 Y ATRIBUTOS DE MUROS RECTOS

Fecha de inicio: 2026-07-11
Estado: activa (documento vivo de seguimiento)

## 1) Proposito

Registrar en una sola bitacora:
- Datos y contratos de atributos en lineas de muro.
- Reglas geometricas vigentes (L, T, X) que impactan extrusion.
- Cambios aplicados por sesion.
- Pendientes, decisiones y validaciones.

Este archivo complementa el handover principal y se actualiza en cada avance.

## 2) Ubicacion y relacion con handover

Carpeta oficial:
- IA/LogicaGeometriaAT60

Documento base de contexto tecnico:
- HANDOVER-GEOMETRIA-AT60-LTX-2026-07-11.md

## 3) Contrato de atributos en maStlUserPlanLine (muros rectos)

Campos implementados:
- _idObject
- _TypeMesh
- _Datalong
- _DataWith
- _DataHeight
- _IsUniversalPanel
- _XCoordinate
- _YCoordinate
- _ZCoordinate
- _Tape_1
- _Tape_2
- _Idconnection_1
- _Idconnection_2
- _CHeckBracketInside
- _CHeckBracketOutside
- _CHeckRijiInside
- _CHeckRijiOutside
- _CHeckPropInside
- _CHeckPropOutside
- _CHeckPropInsideInf
- _CHeckPropOutsideInf

Defaults actuales:
- _TypeMesh = Wall
- _Datalong, _DataWith, _DataHeight en metros con 3 decimales
- _IsUniversalPanel = true
- _XCoordinate, _YCoordinate, _ZCoordinate = punto medio del segmento
- _Tape_1 = Tipo 1
- _Tape_2 = Tipo 1
- _Idconnection_1 = null
- _Idconnection_2 = null
- Checks de bracket/riji/prop = true

## 4) Reglas geometricas AT60 vigentes (impacto en rectos)

- L: funcional y estable con espesores independientes por pata.
- T: estable con constante 0,30 en hombros y total atravesado 0,90 cuando E=0,30.
- X: activa con constante 0,30 por brazo usando span = E/2 + 300 mm.

Nota de coherencia:
- Trims y huellas deben usar la misma formula por tipo de union para evitar desviaciones de cota.

## 5) Registro de cambios (chronologico)

### 2026-07-11 - Bloque A: Atributos base en lineas de muro

Objetivo:
- Sembrar el contrato de atributos en maStlUserPlanLine para muros rectos y mantenerlo en operaciones de edicion.

Implementado:
- Alta de lista completa de atributos.
- Inicializacion con defaults.
- Persistencia en serializacion/restauracion de snapshot.
- Propagacion en split y flujo de commit de segmentos.
- Inclusion en payload de conexiones para inspeccion.

Validacion tecnica:
- Diagnostico de errores sin incidencias en archivo modificado.

Abierto:
- Confirmar semantica final de tipo para _Datalong/_DataWith/_DataHeight (numerico vs texto).
- Confirmar si coordenadas finales deben ser midpoint o extremo del muro segun consumo aguas abajo.
- Poblar _Idconnection_1 y _Idconnection_2 desde endpoints reales del eje.

## 6) Checklist de validacion por cada cambio nuevo

1. Crear muro recto nuevo y revisar existencia de todos los atributos.
2. Hacer split de eje y confirmar que atributos persisten.
3. Ejecutar undo/redo y verificar estabilidad de _idObject.
4. Revisar payload de conexiones y confirmar presencia de campos esperados.
5. Verificar que no se rompen L, T y X en render 2D/3D.

## 7) Plantilla para siguientes entradas

### YYYY-MM-DD - Bloque X: titulo corto

Objetivo:
- 

Implementado:
- 

Validacion tecnica:
- 

Abierto:
- 

## 8) Criterio de aceptacion acumulado

Se considera correcto cuando:
- El contrato de atributos existe en todas las lineas de muro activas.
- Los atributos sobreviven crear, editar, split, undo/redo y serializacion.
- Las reglas L/T/X mantienen consistencia de cotas y no degradan rectos.
