# Handover ATK60 - Estabilizacion y continuidad (2026-07-16)

Ultima actualizacion: 2026-07-16

Este documento esta pensado para continuidad con agentes de menor capacidad de contexto.
Objetivo: evitar regresiones y permitir cambios seguros en rotacion/posicion de paneles.

---

## 1) Estado actual resumido

### 1.1 Frontend (render ATK60)

Archivo principal:
- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js

Funcion clave:
- maStlDesing2RenderAtk60Elements

Comportamiento actual relevante:
- Escala de panel: uniforme por altura de pieza (scale setScalar)
- Rotacion panel:
  - X fijo a 0
  - Y usando rotacion de muro (RotY), con ajuste condicional para angulos no ortogonales
  - Z solo por orientacion de pieza (tumbado = -90 grados, vertical = 0)
- Posicion base: viene del backend (X, Y, Z por elemento)
- Ajustes de colocacion activos:
  - Ajuste en normal exterior (maxOut)
  - Anclaje vertical por base (worldBox min y)
  - Offset exterior fijo de 120 mm

Comportamiento retirado a proposito (para evitar caos):
- Compensaciones longitudinales ad hoc de 2700 mm
- Compensaciones longitudinales ad hoc de 900 mm
- Rotaciones forzadas encadenadas de prueba (+90, -90, +180)
- Selector de yaw por normal exterior A/B en su version mas compleja

### 1.2 Backend (catalogo de altura)

Archivo:
- Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs

Regla actual:
- La altura objetivo se redondea siempre hacia arriba al siguiente modulo de 150 mm
- Ejemplo: 0.50 m -> 0.60 m

Implementacion:
- NormalizeTargetHeightMm usa Math.Ceiling(h/150)*150

---

## 2) Contrato de datos que debe respetarse

Campos minimos esperados por elemento:
- X, Y, Z
- RotY
- Orientation
- NormalX, NormalZ
- FaceSign
- PieceHeightMm

Campos utiles para evolucion posterior:
- PieceWidthMm
- LocalAlongMm
- ModuleLengthMm

Regla de arquitectura:
- El backend define la progresion longitudinal sobre el muro
- El frontend no debe inventar correcciones longitudinales globales salvo requerimiento de negocio explicitamente documentado

---

## 3) Problemas observados y causa mas probable

### 3.1 Sintoma principal reciente
- Muros con rotacion distinta a 0/90/180/270 muestran paneles girados 90 grados

### 3.2 Causa probable
- Mezcla de marcos de referencia (RotX/RotZ heredados + ajustes de Y por heuristica)
- Correcciones de rotacion que se anulan entre si y terminan rompiendo otros casos

### 3.3 Riesgo alto
- Cualquier parche de rotacion que toque a la vez X, Y, Z y desplazamientos longitudinales produce regresion en cascada

---

## 4) Regla de oro para cambios (muy importante)

Cambiar una sola variable por iteracion:
- Primero solo rotacion
- Luego solo insercion longitudinal
- Luego solo normal exterior

Prohibido mezclar en el mismo commit:
- Rotacion + correccion longitudinal
- Rotacion + cambio de escala
- Rotacion + cambio de catalogo de altura

---

## 5) Protocolo de trabajo para agentes menos expertos

### Paso A - Antes de tocar codigo
1. Leer este archivo completo
2. Leer ENCOFRADO-AT60-IMPLEMENTACION.md
3. Confirmar archivo objetivo unico

### Paso B - Cambios permitidos en modo seguro
1. Tocar solo maStlDesing2RenderAtk60Elements
2. Mantener intactos:
   - ajuste normal exterior
   - anclaje vertical
   - offset exterior 120 mm
3. No introducir offsets 2700 o 900 sin bandera de feature

### Paso C - Validacion minima obligatoria
1. Caso ortogonal 90 grados
2. Caso diagonal 45 grados
3. Caso altura 0.50 m (debe seleccionar 0.60)
4. Caso altura 2.70 vertical con simetria

Si falla alguno:
- Revertir solo el bloque cambiado
- No encadenar segundo parche sin restablecer base valida

---

## 6) Matriz de pruebas manuales

### Escena base recomendada
- Tres muros independientes: 0 grados, 45 grados, 90 grados
- Dos alturas: 0.50 m y 2.70 m
- Simetria activa

### Checklist por caso
1. Rotacion correcta:
   - panel paralelo al eje del muro
2. Simetria correcta:
   - no corrimiento longitudinal visible de 0.90 o 2.70
3. Altura correcta:
   - 0.50 m resuelve layout 0.60 m
4. Soporte correcto:
   - panel apoyado en base Y

### Criterio de aprobacion
- 4/4 checks correctos en los 3 angulos

---

## 7) Plan de evolucion recomendado

1. Congelar la rotacion en una formula unica documentada
2. Mover toda decision longitudinal a backend usando LocalAlongMm como fuente de verdad
3. En frontend dejar solo orientacion y ajustes de apoyo/normal
4. Agregar modo debug con ejes locales del panel (helper visual)
5. Repetir patron en los 20 modulos siguientes sin copiar heuristicas antiguas

---

## 8) Antipatrones que NO repetir

- Probar secuencias de +90/-90/+180 sin un plan de pruebas fijo
- Corregir desplazamiento con constantes duras por tipo de panel sin bandera
- Reintroducir inferencia por bbox para decidir inicio longitudinal
- Mezclar en un solo parche: rotacion + insercion + escala

---

## 9) Archivos clave para continuidad

- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js
- Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs
- Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270PanelElementGenerator.cs
- Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs
- Desing/IA/docs/ENCOFRADO-AT60-IMPLEMENTACION.md
- Desing/IA/docs/ENCOFRADO-AT60-BASE.md

---

## 10) Decision log corto (sesion actual)

Decisiones mantenidas:
- Regla de altura por exceso (Ceiling)
- Simplificacion de rotacion para aislar el problema

Decisiones descartadas por regresion:
- Offsets longitudinales ad hoc por simetria sin contrato
- Cadena de compensaciones angulares acumuladas

Recomendacion final:
- Si hay duda, priorizar estabilidad sobre exactitud parcial y documentar antes de optimizar.
