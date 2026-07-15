# Bitacora Agente Copilot - Desing_2 ATK-60 (2026-07-13 a 2026-07-14)

## Objetivo del trabajo
Documentar los cambios realizados en el flujo de encofrado ATK-60 en Desing_2 (frontend + backend), incluyendo diagnostico, estabilizacion de envio de muros, insercion de panel GLB y separacion por repositorios en C#.

## Alcance ejecutado
- Flujo UI de Encofrar en Desing_2.
- Endpoint backend GetWallsAtk-60 para recepcion de muros.
- Normalizacion de atributos de muros para formwork.
- Insercion de panel GLB en escena Three.js por cada muro.
- Ajustes de posicion/rotacion/escala/visibilidad del panel.
- Refactor backend a repositorios por sistema + repositorio comun.

## Cronologia resumida

### Dia 1 (2026-07-13)
1. Se analizo la logica legacy ATK-60 (DesignToolsController + scripts legacy) para replicar comportamiento en Desing_2.
2. Se creo y conecto el boton Encofrar en la UI de Desing_2, visible solo en modo 3D.
3. Se implemento endpoint GetWallsAtk-60 en Desing_2Controller.
4. Se agrego soporte de atributo IsFormwork (default true) y se amplio set de atributos (_XRotation, _YrRtation, _ZRotation, etc.).
5. Se estabilizo la obtencion de muros rectos desde escena/conexiones con dedupe por LineId.
6. Se paso por una fase de diagnostico de binding (request vacio/null) y se valido transporte por prueba minima de IDs.
7. Se evoluciono de IDs simples a lista con atributos por muro para el request temporal.
8. Se retorno Walls en respuesta para depuracion rapida y validacion visual.

### Dia 2 (2026-07-14)
1. Se activo insercion de GLB 27904209 por muro en Three.js.
2. Se agrego marcador de punto de insercion para debug visual.
3. Se corrigio visibilidad para mostrar paneles solo en modo Muro 3D.
4. Se ajusto escala del GLB a altura de muro y anclaje al punto de insercion.
5. Se corrigio posicion respecto al eje del muro y cara exterior (offset medio espesor).
6. Se corrigio ajuste de esquina para evitar desplazamiento longitudinal no deseado.
7. Se ajusto la logica de color del panel (marco/fenolico) con clasificacion por nombre/material y fallback.
8. Se refactorizo backend para separar responsabilidades por sistema/repositorio comun.

## Cambios tecnicos principales

### Frontend (JS)
Archivo principal:
- Desing/Scripts/Desing2/desing2-stl-viewer-toolbar-wiring.js

Cambios clave:
- Orquestacion del boton Encofrar para sistema ATK-60.
- Recoleccion y normalizacion de muros.
- Envio de IdsJson con lista de muros + atributos.
- Llamada a API global de insercion GLB por muro.
- Reduccion de alertas de bloqueo y uso de logs para diagnostico.

Archivo de motor visor:
- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js

Cambios clave:
- Exposicion de API global maStlDesing2InsertAtk60SampleOnWalls.
- Carga de GLB con GLTFLoader.
- Grupo de escena dedicado para paneles ATK-60.
- Insercion por muro con:
  - punto base de insercion,
  - rotacion alineada al muro,
  - ajuste de escala a altura objetivo,
  - ajuste de posicion a cara exterior (medio espesor).
- Marcador de insercion para debug.
- Visibilidad controlada por modo wall3d.
- Ajustes de color de marco/fenolico.

### Backend (C# MVC)
Archivo controlador:
- Desing/Controllers/Desing_2Controller.cs

Cambios clave:
- Endpoint GetWallsAtk-60 conservado como fachada principal.
- Limpieza de logica pesada dentro del controlador.
- Delegacion de parseo/normalizacion a repositorios.

## Nuevo diseno por repositorios (implementado)

### RepositoryAtk60
- Carpeta: Desing/Repositories/RepositoryAtk60
- Archivo: Atk60WallsRepository.cs
- Rol: logica del sistema ATK-60 para construir payload de muros.

### RepositoryCommun
- Carpeta: Desing/Repositories/RepositoryCommun
- Archivo: FormworkJsonCommonRepository.cs
- Rol: parseo/normalizacion comun de IdsJson a lista de muros.

- Archivo: FormworkDtos.cs
- Rol: DTOs comunes de formwork (request, wall dto, attributes, wall geom).

### Proyecto
- Se agregaron los nuevos .cs al Design.csproj para compilacion legacy no-SDK.

## Estado actual
- GetWallsAtk-60 funciona como entrada unica desde controlador principal.
- La logica ATK-60 y la logica comun ya estan aisladas en clases independientes.
- Insercion de panel GLB y posicionamiento estan en fase fina de ajuste visual (iterativo).

## Actualizacion tecnica (2026-07-15)

### Punto de insercion por muro (criterio final aplicado)
1. El calculo se hace en C# en `Atk60WallsRepository.BuildThreeJsPaintPayload`.
2. Se toma el inicio explicito del muro cuando existe (`Inicio/p1`) y no el centro como ancla principal.
3. Se calcula normal al eje y se aplica offset de cara exterior de `E/2` para sacar el punto del eje central del muro.
4. El frontend no recalcula geometria de anclaje; solo pinta el punto que llega en `ElementsForThreeJs`.

### De donde cogemos los datos de muro
1. Fuente prioritaria: muros 3D de `wallModelSource` (`_TypeMesh: Wall`) desde `maStlDesing2GetStraightWallsFromWallModelSource`.
2. Fallback: ejes 3D de escena (`maStlDesing2GetStraightWallsFromScene`) si no hay `wallModelSource`.
3. Antes de enviar a C#, en el wiring se normaliza `IdsJson` con `idsDetailed` y se sobreescriben atributos clave por geometria real 3D (`InicioX/FinX`, `_Datalong`, `_DataWith`, `__Geom3D`).
4. La longitud util enviada al backend se ajusta con trim en extremos conectados (`trimStartMm/trimEndMm`), dejando casos como 9.70 -> 8.80 cuando aplica.

### Trazabilidad JSON de comunicacion
- Se mantiene export de diagnostico de request/resolucion en:
  - `C:\temp\Atk60RequestWallsDebug.json`
- Se alinea con los otros JSON de comunicacion ya usados por el equipo en `C:\temp`.

## Riesgos/pendientes detectados
1. Afinar al 100% el criterio de clasificacion de materiales del GLB (marco/fenolico) segun nombres reales internos del modelo.
2. Afinar criterio geometrico final de esquina de insercion para todos los casos de orientacion.
3. Crear interfaz comun para repositorios por sistema (ej. IFormworkSystemRepository) cuando entren nuevos sistemas.
4. Añadir pruebas de regresion del parseo de IdsJson y dedupe de muros.

## Archivos nuevos creados en este periodo
- Desing/Repositories/RepositoryAtk60/Atk60WallsRepository.cs
- Desing/Repositories/RepositoryCommun/FormworkDtos.cs
- Desing/Repositories/RepositoryCommun/FormworkJsonCommonRepository.cs
- Docs/Proyectos/Desing/Desing2-ATK60-Bitacora-Agente-2026-07-13-14.md

## Archivos principales modificados en este periodo
- Desing/Controllers/Desing_2Controller.cs
- Desing/Scripts/Desing2/desing2-stl-viewer-toolbar-wiring.js
- Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js
- Desing/Design.csproj

## Criterio de cierre de esta etapa
- Arquitectura base por repositorios lista para escalar a mas sistemas.
- Endpoint principal mantenido estable.
- Documentacion de los dos dias consolidada en este archivo.
