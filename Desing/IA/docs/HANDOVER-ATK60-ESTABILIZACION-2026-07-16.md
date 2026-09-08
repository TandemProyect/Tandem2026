# Handover ATK60 - Estabilizacion y continuidad (2026-07-16)

Ultima actualizacion: 2026-07-18

Este documento esta pensado para continuidad con agentes de menor capacidad de contexto.
Objetivo: evitar regresiones y permitir cambios seguros en rotacion/posicion de paneles.

Actualizacion 2026-07-18: la pose final de paneles ATK60 se resuelve en C#. Three.js debe actuar como renderizador estricto: carga el GLB, escala por altura y aplica `X/Y/Z/RotX/RotY/RotZ/BaseRot*/InsertOffset*` sin inventar offsets longitudinales, normales ni simetrias.

---

## 1) Estado actual resumido

### 1.1 Frontend (render ATK60)

Archivo principal:
- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js

Funcion clave:
- maStlDesing2RenderAtk60Elements

Comportamiento actual relevante:
- Escala de panel: uniforme por altura de pieza (scale setScalar)
- Ruta estricta: activa cuando `UseStrictPose = true`.
- Rotacion panel en ruta estricta:
   - `RotX + BaseRotX`
   - `RotY + BaseRotY`
   - `RotZ + BaseRotZ`
- Posicion base: viene del backend (`X`, `Y`, `Z` por elemento).
- `InsertOffsetX/Y/Z` es offset local del panel y JS lo rota con la pose final.
- En ruta estricta no se aplican heuristicas legacy de bbox, normal exterior, anclaje vertical ni offset exterior fijo.

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
- Alturas 2.70, 2.40 y 1.20 usan paneles verticales.
- Alturas bajas y complementos superiores usan paneles tumbados.
- Panel tumbado: `BaseRotZ = -PI/2`; su elevacion se compensa en coordenada mundial `Y` desde el generador, no con `InsertOffsetY`.

Implementacion:
- NormalizeTargetHeightMm usa Math.Ceiling(h/150)*150

### 1.3 Backend (generador de paneles 270)

Archivo:
- Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270PanelElementGenerator.cs

Reglas actuales:
- Cada muro se resuelve individualmente; no se reordena ni se invierte por estar conectado a otros muros.
- `yawRad` se usa para calcular progresion longitudinal `X/Z` sobre el eje del muro.
- `renderYawRad = -yawRad` se usa para `RotY`, porque la convencion de giro Y de Three.js invierte el signo en el plano XZ.
- `FaceSign` decide que cara del eje es la principal:
   - `FaceSign >= 0`: cara principal `RotY = renderYawRad`; cara opuesta `RotY = renderYawRad + PI`.
   - `FaceSign < 0`: cara principal `RotY = renderYawRad + PI`; cara opuesta `RotY = renderYawRad`.
- La cara que rota 180 grados avanza `PieceWidthMm` sobre el eje del muro para mantener el punto inferior-izquierdo coherente.
- La cara opuesta se obtiene desplazando un espesor de muro sobre la normal opuesta.
- Para piezas `Tumbado`, `Y = anchor.Y + UpOffsetMm + PieceHeightMm`; no usar `InsertOffsetY` para esto porque es local y al rotar Z desplaza X/Z.
- Alcance visual actual: se pintan solo los modulos `M_270` y, si `M_Remate > 0`, el remate final. Los modulos secundarios (`M_180`, `M_240`, etc.) quedan pendientes.
- Si `M_Remate > 0`, se inserta `REMATE_WOOD` al final de los modulos `M_270`, siempre que mida menos de `150 mm`.
- `REMATE_WOOD` se renderiza como tablon de madera estricto: largo `M_Remate`, altura del muro y profundidad `120 mm`.
- Material visual del remate: textura madera web `https://threejs.org/examples/textures/hardwood2_diffuse.jpg` con fallback procedural en canvas si no carga.

### 1.4 DTO / contrato estricto

Archivo:
- Desing/Repositories/RepositoryCommun/FormworkDtos.cs

Campos clave actuales en `Atk60ElementPaintItem`:
- `X`, `Y`, `Z`
- `RotX`, `RotY`, `RotZ`
- `BaseRotX`, `BaseRotY`, `BaseRotZ`
- `InsertOffsetX`, `InsertOffsetY`, `InsertOffsetZ`
- `UseStrictPose`
- `FaceSign`, `NormalX`, `NormalZ`, `IsMirrored`
- `PieceWidthMm`, `PieceHeightMm`, `LocalAlongMm`, `LocalUpMm`

---

## 2) Contrato de datos que debe respetarse

Campos minimos esperados por elemento:
- X, Y, Z
- RotX, RotY, RotZ
- Orientation
- NormalX, NormalZ
- FaceSign
- PieceHeightMm
- UseStrictPose

Campos utiles para evolucion posterior:
- PieceWidthMm
- LocalAlongMm
- ModuleLengthMm
- BaseRotX, BaseRotY, BaseRotZ
- InsertOffsetX, InsertOffsetY, InsertOffsetZ

Regla de arquitectura:
- El backend define la progresion longitudinal sobre el muro
- El backend define simetria, rotacion final, elevacion de tumbados y cara opuesta
- El frontend no debe inventar correcciones longitudinales, normales, bbox ni offsets globales salvo requerimiento de negocio explicitamente documentado

---

## 3) Problemas observados y causa mas probable

### 3.1 Sintomas recientes ya corregidos
- Horizontales correctos y verticales desplazados/girados: causa en signo de `RotY` Three.js respecto al eje XZ. Solucion: `renderYawRad = -yawRad` para rotacion, manteniendo `yawRad` para posicion.
- Muros con mismo eje pero distinta cara (`FaceSign = -1`) giraban como si fueran `FaceSign = 1`. Solucion: la cara principal rota 180 grados cuando `FaceSign < 0`.
- Paneles tumbados caian hacia abajo. Solucion: subir `Y` mundial en el generador con `PieceHeightMm`; no usar `InsertOffsetY` porque es local y se rota.

### 3.2 Causa raiz recurrente
- Mezcla de marcos de referencia: eje de muro en C# (`yawRad`) frente a `rotation.y` de Three.js (`renderYawRad = -yawRad`).
- Uso de offsets locales (`InsertOffset*`) para problemas que son de coordenada mundial, especialmente elevacion `Y`.
- Intentar corregir simetria en JS en lugar de resolverla en el payload C#.

### 3.3 Riesgo alto
- Cualquier parche de rotacion que toque a la vez X, Y, Z y desplazamientos longitudinales produce regresion en cascada

---

## 4) Regla de oro para cambios (muy importante)

Cambiar una sola variable por iteracion:
- Primero solo rotacion
- Luego solo insercion longitudinal
- Luego solo elevacion mundial
- Luego solo normal/cara

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
1. Mirar primero `C:\temp\Atk60RequestWallsDebug.json` y comparar `Walls`, `ElementsForThreeJs.Walls` y `ElementsForThreeJs.Elements`.
2. Para pose/rotacion/simetria, tocar primero `Modulo270PanelElementGenerator.cs`.
3. Para altura/layout/tumbados, tocar primero `Modulo270HeightPanelCatalog.cs`.
4. No tocar `maStlDesing2RenderAtk60Elements` salvo que el contrato estricto no se aplique o haya un bug puro de carga/render GLB.
5. No introducir offsets 2700 o 900 sin derivarlos de `PieceWidthMm`, `ModuleLengthMm` o una regla de catalogo documentada.

### Paso C - Validacion minima obligatoria
1. Caso horizontal con `FaceSign = 1` y `FaceSign = -1`.
2. Caso vertical +90 y -90 (`id4/id10` fueron casos de referencia).
3. Caso altura 0.50 m (debe seleccionar 0.60 y quedar sobre suelo).
4. Caso altura 2.70 vertical con simetria.
5. Caso mixto/tumbado sobre vertical, por ejemplo 3.00 m.
6. Caso longitud 8.20 m: debe pintar `3 x 2.70` y `REMATE_WOOD` de `0.10`.

Si falla alguno:
- Revertir solo el bloque cambiado
- No encadenar segundo parche sin restablecer base valida

---

## 6) Matriz de pruebas manuales

### Escena base recomendada
- Muros independientes: 0 grados, 90 grados, -90 grados y 180 grados.
- Dos caras: una con `FaceSign = 1` y otra con `FaceSign = -1`.
- Alturas: 0.50 m, 1.20 m, 2.40 m, 2.70 m y 3.00 m.
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
5. Tumbado correcto:
   - no baja de Y=0 y no desplaza X/Z por correccion de elevacion

### Criterio de aprobacion
- Checks correctos en horizontales, verticales, ambas caras y alturas baja/mixta.

---

## 7) Plan de evolucion recomendado

1. Mantener la rotacion en una formula unica documentada (`renderYawRad = -yawRad`)
2. Mantener toda decision longitudinal en backend usando `LocalAlongMm` como fuente de verdad
3. En frontend dejar solo carga GLB, escala uniforme y aplicacion de pose estricta
4. Agregar modo debug con ejes locales del panel (helper visual)
5. Repetir patron en los 20 modulos siguientes sin copiar heuristicas antiguas

---

## 8) Antipatrones que NO repetir

- Probar secuencias de +90/-90/+180 sin un plan de pruebas fijo
- Corregir desplazamiento con constantes duras por tipo de panel sin bandera
- Reintroducir inferencia por bbox para decidir inicio longitudinal
- Mezclar en un solo parche: rotacion + insercion + escala
- Usar `InsertOffsetY` para corregir elevacion de tumbados: es offset local y puede desplazar X/Z tras `BaseRotZ`.
- Reordenar o invertir muros conectados si el JSON demuestra que cada muro individual ya tiene eje correcto.

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
- C# como solver de pose final para paneles ATK60.
- JS como renderizador estricto cuando `UseStrictPose = true`.
- Simetria resuelta por `FaceSign`, `PieceWidthMm`, normal y espesor de muro.
- `renderYawRad = -yawRad` para adaptar la convencion de Three.js.
- Paneles tumbados elevan su `Y` mundial en el generador; `InsertOffsetY` queda en cero para no mover X/Z al rotar.
- Remate de madera final si `M_Remate > 0 && M_Remate < 0.150`; el visor lo dibuja como caja, no como GLB. De momento se coloca tras los `M_270` pintados.

Decisiones descartadas por regresion:
- Offsets longitudinales ad hoc por simetria sin contrato
- Cadena de compensaciones angulares acumuladas
- Normalizar/reordenar muros conectados cuando el fallo esta en la pose individual del muro
- Corregir altura de tumbados con offset local rotado

Herramientas de diagnostico:
- `C:\temp\Atk60RequestWallsDebug.json`: payload completo con muros, anchors y elementos.
- `C:\temp\Atk60PanelsDebug.json`: paneles resumidos.
- `Desing/IA/tools/analyze-atk60-debug.mjs`: analizador de payload si hay Node disponible.

Recomendacion final:
- Ante un fallo visual, comparar primero dos IDs en el JSON. Si el payload esta mal, corregir C#; si el payload esta bien y JS no lo respeta, revisar solo la ruta estricta.
