# Handover Desing_2 ATK60, recuento y guardado (2026-10-18)

## Proposito
Dejar contexto tecnico suficiente para que otro agente pueda continuar el trabajo de Desing_2 sin rehacer la investigacion. Este documento resume los cambios recientes en recuento de materiales ATK60, visualizacion de componentes, guardado de foto/camara, menu y barra superior.

## Resumen ejecutivo
- El visor principal sigue estando en `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`.
- La vista principal del workspace esta en `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml`.
- El controlador principal es `Desing/Controllers/Desing_2Controller.cs`.
- Se centralizo y estabilizo el recuento de materiales para ATK60, incluyendo pesos, m2 y detalle por articulo.
- Se ajusto el comportamiento legacy de puntales/grapa regulable: la escena puede mostrar componentes, pero el recuento debe agrupar/cambiar a codigos legacy cuando toca.
- Se corrigio la composicion vertical ATK60 para muros altos, especialmente el caso de 7 m: `2700 + 2700 + 1200 + 450`.
- El guardado del diseno ahora captura foto, persiste `AttPhoto`, emite evento de actualizacion y guarda/restaura estado de camara cuando hay soporte disponible.
- Se actualizaron la imagen del menu inferior y tarjetas/listados para evitar cache de imagen vieja.
- Se anadieron iconos nuevos en la barra superior de dibujo: `Pilares` y `Insertar worker` como placeholders visibles, aun sin herramienta funcional.

## Flujo de guardado actual

### Foto del diseno
1. El JS captura el canvas Three.js mediante `maStlCaptureDesignPngDataUrl()`.
2. La foto se envia a `Desing_2/SaveDesignPhoto`.
3. `SaveDesignPhoto` guarda PNG en `~/Content/images/desing/yyyy-MM/`.
4. Actualiza `dbo.TSql_Design_V2.AttPhoto` por SQL directo, porque la entidad EF `TSql_Design_V2` no expone `AttPhoto`.
5. Devuelve `AttPhoto` y `PhotoUrl` con `?v=...` para romper cache.
6. El visor emite `tandem:design-photo-updated` y escribe tambien en `localStorage` para otras pestanas.

### Camara/vista
1. La serializacion vive en `maStlDesing2BuildViewerStateSnapshot()`.
2. Se guarda cookie por diseno y fallback global.
3. Existe endpoint `SaveViewerState` en `Desing_2Controller`.
4. El endpoint actualiza `TSql_Design_V2.AttActiveCameraType`.
5. Si existe una columna futura `AttViewerStateJson` en `TSql_Design_V2`, el codigo la usara automaticamente para persistir/restaurar estado completo.
6. Si existe registro espejo en `TSql_Design`, se intenta guardar tambien en `TSql_DesignCamera`.

### Limitacion importante de BD
- `TSql_DesignCamera` tiene FK/navegacion a `TSql_Design`, no a `TSql_Design_V2`.
- No asumir que se puede insertar ahi para todos los disenos V2.
- Para persistencia completa multi-equipo/multi-navegador, crear columna `AttViewerStateJson` en `TSql_Design_V2` o una tabla V2 especifica.

## Refresco de imagen en UI

### Visor
- `maStlPersistDesignPhoto()` ahora lee `resp.PhotoUrl` y llama a `maStlNotifyDesignPhotoUpdated(photo)`.
- Evento: `tandem:design-photo-updated`.
- Payload esperado:

```json
{
  "designId": 123,
  "photoUrl": "/Content/images/desing/2026-10/Diseno_123.png?v=...",
  "attPhoto": "~/Content/images/desing/2026-10/Diseno_123.png",
  "ts": 179... 
}
```

### Menu inferior
- Vista: `Desing/Views/Shared/_SidebarMaterio.cshtml`.
- La lista `lastDesignPayload` incluye `designId` y `photo`.
- El script escucha el evento y `storage` para actualizar el `src` de la imagen inferior sin recargar.

### Dashboard/listados de disenos
- Controlador: `Desing/Controllers/HomeController.cs`.
- Se agrego `BuildDesignPhotoUrl(photoRaw, changed)` para normalizar ruta y anadir `?v=AttChange`.
- Este helper se usa en filas y tarjetas de disenos para evitar cache.

## Recuento de materiales ATK60

### Objetivos cumplidos
- Mostrar reloj/carga mientras se calculan datos.
- No mostrar recuento hasta que los datos esten listos.
- Footer comun para pestanas: numero de articulos, peso total y total m2.
- Pestana `Detalles` con columnas: numero, codigo, nombre, cantidad, peso uds, peso total, m2, m2 total.
- En `Detalles` desaparecen acciones/botones de la pestana de articulos.
- Estilos ajustados para que cabecera y tabla encajen con DataTables existente.

### Backend relevante
- `Desing_2Controller.ListDesignMaterialCount()`.
- `Desing_2Controller.PreviewMaterialCount()`.
- `MasterArticleCountRow` incluye pesos y m2.
- Repositorio/articulos: `DesignWallArticleRepository`.

### Legacy puntales y grapa regulable
- El usuario confirmo que al seleccionar grapa regulable o puntal, el visor debe mostrar los componentes que lo forman.
- En recuento, sin embargo, determinados componentes se agrupan/transforman a codigos legacy.
- En `Desing_2Controller` revisar helpers como `AddSceneMaterialCountGroup`, `LegacyPropMaterialCodes`, `IsLegacyPropHiddenPart`, `ResolveCountArticleStl`, `IsAdjustableClampCode`.
- El preview pequeno de STL soporta multiples URLs en `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js`.
- Para puntales/grapas, el loader fuerza color gris para evitar que salgan amarillo/negro de forma no deseada.

## ATK60 altura de muro alto

### Problema corregido
- Para muro de 7 m, el usuario esperaba composicion por altura: `2.70 + 2.70 + 1.20 + 0.45`.
- El problema no era longitud sino altura.

### Archivo clave
- `Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs`.

### Cambio de criterio
- Se subio el cap de altura y se implemento stack vertical para alturas altas.
- Para paredes altas se apilan bandas de 2700 y se resuelve el remate superior.
- Revisar metodos: `NormalizeTargetHeightMm`, `BuildTallVerticalStackLayout`, `Add2700VerticalBand`, `OffsetPiece`.

## Barra superior de dibujo

### Archivo
- `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`.

### Cambios recientes
- Se anadieron dos botones despues de `insert-enclosure` y antes del separador hacia modificar:
  - `ma-stl-tool-pillars`, `data-ma-stl-tool="pillars"`, titulo `Pilares`.
  - `ma-stl-tool-insert-worker`, `data-ma-stl-tool="insert-worker"`, titulo `Insertar worker`.
- Ambos estan visibles pero `disabled` porque no hay logica JS implementada todavia.
- El icono de `Pilares` se sustituyo por un SVG propio tipo puntal/pilar con travesanos y apoyos.

### Para activar funcionalidad futura
En `master-article-details-stl-viewer.js` hay que anadir:
- referencias DOM para los botones,
- estado de herramienta,
- sincronizacion de `active`/`aria-pressed`,
- handlers de click,
- casos en `maStlDesing2ActivateToolbarCommandById`,
- logica de insercion/seleccion en canvas.

No basta con quitar `disabled`: si no se implementa el JS, el boton no hara nada o quedara incoherente con el ultimo comando.

## Bugs corregidos durante esta fase
- `CS1513: } expected` en `_Desing2StlViewerWorkspace.cshtml`: un bloque `.then(...)` quedo insertado accidentalmente dentro del bloque Razor inicial. Se retiro y se reinsertó en la cadena JS correcta del guardado del recuento.
- `BuildDesignPhotoUrl` no existia en `HomeController`: se creo helper local.
- Imagen de menu inferior no se refrescaba al guardar: se anadio evento + listener + cache-busting.
- Foto se guardaba pero no se esperaba el resultado: el flujo de guardado ahora espera la persistencia de foto antes de mostrar exito completo.

## Validacion realizada
- Se uso `get_errors` de VS Code sobre archivos tocados porque el build completo del proyecto web legacy puede fallar por dependencias de `Microsoft.WebApplication.targets` en esta maquina.
- Archivos validados sin diagnosticos tras cambios recientes:
  - `Desing/Controllers/Desing_2Controller.cs`
  - `Desing/Controllers/HomeController.cs`
  - `Desing/Models/Desing2ViewModels.cs`
  - `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml`
  - `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`
  - `Desing/Views/Shared/_SidebarMaterio.cshtml`
  - `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`
  - `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js`
  - `Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs`

## Archivos principales tocados
- `Desing/Controllers/Desing_2Controller.cs`
- `Desing/Controllers/HomeController.cs`
- `Desing/Models/Desing2ViewModels.cs`
- `Desing/Views/Desing_2/_Desing2StlViewerWorkspace.cshtml`
- `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`
- `Desing/Views/Shared/_SidebarMaterio.cshtml`
- `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`
- `Desing/Scripts/DesignToolsAutocad/plugin-cad-block-preview.js`
- `Desing/assets/materio/css/site.css`
- `Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs`

## Reglas practicas para el siguiente agente
1. No usar `TSql_DesignCamera` como tabla V2 salvo que exista registro espejo en `TSql_Design`.
2. No guardar foto con nombre cambiante salvo necesidad; ahora se usa mismo nombre + `?v=` para cache.
3. Si se toca `_Desing2StlViewerWorkspace.cshtml`, revisar siempre las primeras 20 lineas: Razor es sensible a inserciones JS fuera de sitio.
4. Si se agregan botones `data-ma-stl-tool`, implementar tambien su estado JS o dejarlos deshabilitados.
5. Para recuento, distinguir entre visualizacion de componentes y agrupacion material legacy.
6. Para ATK60, separar altura de longitud: no resolver altura alta como si fuera despiece longitudinal.

## Pendientes directos
- Definir comportamiento de `Pilares`.
- Definir comportamiento de `Insertar worker`.
- Crear persistencia completa de viewer state en BD V2 (`AttViewerStateJson` o tabla propia).
- Anadir tests unitarios/minimos para `Modulo270HeightPanelCatalog` con 7000 mm y casos borde.
- Confirmar en navegador real que menu inferior, tarjetas de diseno y pagina de disenos refrescan foto sin recarga dura.
