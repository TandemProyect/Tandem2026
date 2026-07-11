# Handover Desing 2: muros, conexiones y arranque

Última actualización: 2026-07-07

## Objetivo

Este documento resume el trabajo realizado en el visor `Desing_2` y en el arranque local de la aplicación MVC. Sirve como guía rápida para continuar el desarrollo, probar los cambios y entender qué archivos quedaron afectados.

## Visor Desing 2 / muros

Archivo principal:

- `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`

Vistas relacionadas:

- `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml`
- `Desing/Views/Desing_2/Viewer.cshtml`

Cambios implementados:

- Se añadió un HUD de distancia y ángulo mientras se dibuja polilínea o muro.
- El input de distancia mantiene el comportamiento de longitud.
- El input de ángulo permite indicar dirección de forma separada.
- El cálculo de ángulo se muestra siempre positivo, estilo cota CAD.
- El comando polar tipo AutoCAD `@distancia<angulo` quedó soportado en el flujo de dibujo.
- La dirección de los ángulos se interpreta en sentido antihorario, por ejemplo `@10<45` dibuja hacia arriba.
- El cierre de muro con `c` + `Enter` conecta el punto actual con el primer punto del grupo cuando hay al menos dos tramos.
- El botón de muro empieza siempre un grupo nuevo; `Enter` reanuda desde el último vértice.
- El eje del muro se trata como información/base geométrica; las operaciones sobre caras se resuelven al eje asociado para editar el tramo completo.

## Ortho / F8

Objetivo:

- `F8` debe activar/desactivar Ortho como en CAD.
- Con Ortho activado, una conexión en T contra un muro debe proyectarse perpendicular al eje receptor.
- Con Ortho desactivado, la conexión oblicua es válida.

Cambios:

- `F8` ahora se captura aunque el foco esté en los inputs de distancia o ángulo.
- Se evita alternar repetidamente si la tecla se mantiene pulsada.
- El estado se sincroniza con el botón de Ortho del toolbar.
- Se añadió indicador visual en la esquina inferior izquierda:
  - `(F8) Act` en azul cuando Ortho está activado.
  - `(F8) Des` en rojo cuando Ortho está desactivado.
- El texto del indicador es clicable y alterna el estado.
- El indicador también responde con `Enter` o espacio si tiene foco.
- Los HUDs de coordenadas/distancia se subieron para no pisar el indicador de Ortho.

Funciones clave:

- `maStlSyncLineToolOrtho15ToggleUi`
- `maStlToggleLineToolOrtho15FromUi`
- `maStlLineToolTryOrthoTPerpendicularEndMm`
- `maStlLineToolNormalizeWallSnapCandidate`
- `maStlLineToolResolveWallAxisLineFromSnapLine`

## Conexiones de muro

Problema tratado:

- En conexiones en T, el cursor podía caer sobre una cara del muro y no sobre el eje. Eso hacía que la conexión no se resolviera correctamente.

Solución:

- Cuando se está dibujando un muro y el snap cae sobre una cara, se normaliza internamente al eje asociado.
- La previsualización y el clic final usan el mismo candidato normalizado.
- Si Ortho está activado, se proyecta el extremo del tramo al eje receptor para formar una T perpendicular.
- Si Ortho está desactivado, el punto de snap puede quedar oblicuo.

Notas:

- La lógica se apoya en `userData.maStlUserPlanLine.wallRole`.
- Las caras usan `wallRole = 'face'` y referencia `linkOffsetFromLineId`.
- Los ejes usan `wallRole = 'axis'`.

## Modelo muro 2D / 3D

Cambios previos integrados en el flujo:

- El selector de modo permite trabajar en:
  - Líneas
  - Muro 2D
  - Muro 3D
- Al entrar en modo Muro 2D o Muro 3D:
  - Las líneas fuente se ocultan, pero no se borran.
  - El modelo generado se limpia y se regenera cada vez.
  - Se muestra overlay de carga con mensaje `Renderizando`.
- **Ciclo de vida:** la línea fuente conserva atributos (`numberOffsetMm`, cotas); las mallas/polígonos generados (`maStlWall3dGenerated`) se eliminan al volver a Líneas o al cambiar de modo Muro 2D/3D.
- **Encofrado AT-60:** esquinas L y uniones T se resuelven en cliente (`ma-stl-atk60-formwork.js`); ver [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md).
- `Muro 3D` usa la misma lógica de color que `Muro 2D`.
- El sistema activo se muestra como `Atk-60`.
- El selector de sistema es desplegable; los sistemas no activos quedan deshabilitados.

## Inserción de recinto

Cambio:

- El comando `Dibujar Recuadro` / insertar recinto genera muros completos, no solo polilíneas.
- Se crean eje y caras, respetando la lógica del modo muro.

## WallConnections.json

Archivo:

- `Desing/IA/Communication/WallConnections.json`

Uso:

- Se genera/sobrescribe al cancelar o finalizar ciertas operaciones de dibujo.
- Guarda información de líneas, muros, ejes, caras y conexiones.
- Sirve para depurar L, T, cruces y geometría generada.

## Rendimiento de arranque local

Archivos principales:

- `Desing/Web.config`
- `Desing/Global.asax.cs`
- `Desing/Controllers/BaseController.cs`
- `Desing/Helpers/LanguageUiHelper.cs`
- `Desing/Helpers/DbBackedResourceManager.cs`
- `Desing/Design.csproj.user` (configuración local de Visual Studio)

Problemas detectados:

- La aplicación tardaba mucho arrancando desde Visual Studio.
- SQL local respondía razonablemente, así que el problema no parecía ser una consulta concreta lenta.
- Había muchas consultas pequeñas repetidas en:
  - `BaseController.OnActionExecuting`
  - resolución de idioma
  - carga de traducciones híbridas `.resx` + `TSql_UiTranslation`
- `compilation debug="true"` ralentizaba mucho el arranque MVC/Razor.
- Visual Studio abría una pestaña `about:blank` además de la página real.

Cambios aplicados:

- `Web.config`
  - `vs:EnableBrowserLink=false`
  - `TandemStartupTiming=true`
  - `compilation debug="false"`
- `BaseController`
  - Cache de plantilla por defecto.
  - Cache de plantilla por id.
  - Cache de idioma por id.
  - Cache de datos de navbar/usuario durante 5 minutos.
- `LanguageUiHelper`
  - Cache de resolución `TextCode -> IdObject`.
- `DbBackedResourceManager`
  - Cache de resolución de idioma para evitar repetir consulta por módulo.
  - La invalidación limpia también esta cache.
- `Global.asax.cs`, `BaseController.cs`, `DbBackedResourceManager.cs`
  - Trazas `[TandemStartupTiming]` con tiempos en milisegundos.
- `Design.csproj.user`
  - `StartAction` configurado como `NoStartPage` para evitar la pestaña automática `about:blank`.

Cómo probar rendimiento:

1. Cerrar IIS Express.
2. Arrancar sin depuración (`Ctrl+F5`) o aceptar `Ejecutar sin depuración`.
3. Abrir manualmente una sola pestaña:
   - `https://localhost:44384/Account/Login`
   - o la URL del visor que se esté probando.
4. Revisar `Output > Debug` en Visual Studio.
5. Buscar líneas:
   - `[TandemStartupTiming] Application_Start = ... ms`
   - `[TandemStartupTiming] BeginRequest ... = ... ms`
   - `[TandemStartupTiming] BaseController.OnActionExecuting ... = ... ms`
   - `[TandemStartupTiming] DbBackedResourceManager.Load ... = ... ms`

Nota importante:

- Para trabajar rápido, mantener `compilation debug="false"` y usar `Ctrl+F5`.
- Para depurar con breakpoints, Visual Studio puede pedir habilitar debug en `Web.config`; eso volverá a ralentizar el arranque.

## Configuración BD local

`Web.config` quedó apuntando a SQL Express local:

- Servidor: `NUCBOXG5\SQLEXPRESS`
- Base: `db_a197cd_desingproducction`
- Seguridad integrada: `Integrated Security=True`

Se corrigió previamente el nombre de base usado por la conexión local.

## Validaciones realizadas

Se ejecutó `git diff --check` sobre los bloques modificados durante las iteraciones. No quedaron errores de whitespace en las validaciones realizadas.

No se pudo compilar desde terminal con `dotnet msbuild` porque el proyecto MVC clásico requiere `Microsoft.WebApplication.targets`, disponible normalmente desde el MSBuild completo de Visual Studio.

## Pendientes recomendados

- Probar visualmente las conexiones T con:
  - Ortho activado.
  - Ortho desactivado.
  - Snap sobre eje.
  - Snap sobre cara.
- Revisar `WallConnections.json` después de una T real para confirmar que el punto final se guarda sobre el eje receptor.
- Cuando el rendimiento esté confirmado, valorar desactivar `TandemStartupTiming` para no ensuciar el Output.
- Si se necesitan breakpoints, activar debug solo durante esa sesión y volver a `debug="false"` al terminar.

