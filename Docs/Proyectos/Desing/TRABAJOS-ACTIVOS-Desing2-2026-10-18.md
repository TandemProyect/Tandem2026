# Trabajos activos Desing_2 (2026-10-18)

Documento vivo para continuar sin perder contexto. El handover completo esta en `HANDOVER-2026-10-18-Desing2-ATK60-Recuento-Guardado.md`.

## Estado actual
- Guardado de diseno: funcional y rapido segun validacion del usuario.
- Foto: se guarda en `TSql_Design_V2.AttPhoto` y se refresca con URL versionada.
- Camara: se guarda en cookie por diseno y se actualiza `AttActiveCameraType`; preparado para usar `AttViewerStateJson` si existe.
- Recuento: muestra carga, resumen, articulos, detalles y superficies.
- ATK60: altura alta corregida para caso 7 m.
- Barra superior: botones `Pilares` e `Insertar worker` visibles, pero deshabilitados.
- Esquinas L en modo muro 3D: doble click sobre mesh de esquina abre formulario para tipo de esquina (Angular / Panel regulable vertical / horizontal) y guarda `_Tape_0`.
- Al aceptar esquina L, el recorte de muros conectados ahora cambia segun tipo: Angular mantiene recorte ATK60 por panel; Universal_X / Universal_Y aplican recorte directo `espesor + 30 mm` en el eje seleccionado (legacy).
- Corregido cache de modelo 3D: `_Tape_0` entra en fingerprint y al cambiar tipo de esquina se invalida cache `wall3d`, forzando regeneracion visual inmediata.
- Si faltan lineas `face` en memoria, se sintetizan desde el eje para poder aplicar recortes en muro 3D sin depender del fallback del detector.
- Tipo de esquina por vertice de eje (`_CornerTypeP1/_CornerTypeP2`) para que cada esquina L aplique su solucion local sin contaminar otras esquinas del mismo muro.
- Regla de longitud de esquina L ajustada por tipo: `Angular = espesor opuesto + 0,30` (sin +0,03), `Universal_X/Y = espesor opuesto + 0,30 + 0,03`; Angular paneliza ambos catetos y Universal paneliza solo el no regulable.
- Esquina Angular: en la junta interior angular+panel se añade columna de tirantes (placa + Dywidag) con cotas por banda (2,70: 0,55/2,15; 2,40: 0,55/1,85; 1,20: 0,35/0,85).
- Insercion del tirante de esquina angular anclada al panel horizontal interior en el borde del muro (`+/- 43,714 mm` desde el borde), tanto en ruta de cover como en ruta de gap.
- Regla Universal (ajustada segun referencia de pilares): la cara sin panel regulable mantiene panelizacion tipo angular pero sin angular ni uniones de esquina; la cara regulable conserva la union de esquina (estilo pilar) sin angular.
- Regla Universal refinada: la cara regulable fuerza modulo de 0,75 m y conserva union de esquina de una sola cara (estilo pilar); la cara no regulable mantiene panelizacion sin angular ni uniones de esquina.
- En la cara regulable Universal se elimina la grapa fija de union de esquina y se sustituye por hardware tipo pilar (gancho `1850164` + placa `10443020`) en alturas de agujero por banda.
- Ajuste de posicion: el tirante de esquina angular se coloca en el agujero del panel (offset 43,714 mm desde la junta), no en el canto del angular.
- Ajuste robusto de posicion: el offset del tirante se calcula segun lado real de la junta (fuera por inicio/fin), sin depender de `atEnd`.
- Regla temporal solicitada: al elegir Universal en una esquina, se desactiva `_IsFormwork` en el eje regulable de ese vertice (muro completo) hasta implementar su encofrado dedicado.
- Acotacion de muro parcial/total: ahora lee el cateto real de esquina por vertice (Angular/Universal local) en vez del calculo fijo original por espesor.

## Pendiente 1: activar Pilares

### Decision pendiente
Definir que es un pilar en el modelo Desing_2:
- geometria 2D en planta,
- articulo maestro/STL asociado,
- dimensiones por defecto,
- si cuenta en materiales,
- si afecta a encofrado ATK60.

### Primer corte recomendado
1. Boton `ma-stl-tool-pillars` deja de estar disabled.
2. Al hacer click, entra en modo pick sobre suelo.
3. Primer click inserta un marcador cuadrado en planta.
4. Guardar como articulo manual o linea especial, segun encaje con `DesignWallArticleRepository`.
5. Incluirlo/ocultarlo correctamente en recuento.

### Archivos probables
- `Desing/Views/Desing_2/_Desing2TopToolBar.cshtml`
- `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`
- `Desing/Controllers/Desing_2Controller.cs`
- Repositorios de articulos si el pilar cuenta como material.

## Pendiente 2: activar Insertar worker

### Decision pendiente
El nombre `worker` necesita aclaracion funcional:
- puede ser operario/persona para escala,
- recurso de obra,
- bloque visual,
- o entidad de planificacion.

### Primer corte recomendado
1. Mantenerlo deshabilitado hasta definir semantica.
2. Si es visual, insertar sprite/modelo simple en escena y no contarlo en materiales.
3. Si es material/recurso, crear flujo separado de articulos.

## Pendiente 3: persistencia completa de vista en BD

### Situacion
- El codigo ya detecta columna `AttViewerStateJson` en `dbo.TSql_Design_V2`.
- Si existe, carga/guarda JSON automaticamente.
- Si no existe, se queda en cookie por diseno + `AttActiveCameraType`.

### Trabajo BD sugerido
```sql
ALTER TABLE dbo.TSql_Design_V2
ADD AttViewerStateJson nvarchar(max) NULL;
```

Despues validar:
- mover camara,
- guardar,
- cerrar sesion/navegador,
- abrir en otro navegador,
- comprobar vista restaurada.

## Pendiente 4: tests ATK60 altura

Casos minimos:
- 2700 mm -> una banda 2700.
- 7000 mm -> 2700 + 2700 + 1200 + 450.
- Altura fuera de rango -> clamp/control segun catalogo.

Archivo objetivo:
- `Desing/Repositories/RepositoryAtk60/ModulosATK60/Modulo270HeightPanelCatalog.cs`

## Pendiente 5: revisar cache de imagen en todas las superficies

Ya cubierto:
- menu inferior (`_SidebarMaterio.cshtml`),
- dashboard/listados via `HomeController.BuildDesignPhotoUrl`,
- visor emite evento `tandem:design-photo-updated`.

Validacion manual pendiente:
- guardar vista nueva,
- comprobar imagen inferior del menu sin recargar,
- volver al dashboard y comprobar tarjeta/fila,
- abrir otra pestana y comprobar evento `storage`.

## Riesgos activos
- Botones nuevos estan deshabilitados a proposito; no quitar `disabled` sin implementar JS.
- `TSql_DesignCamera` no es una tabla V2 pura.
- `_Desing2StlViewerWorkspace.cshtml` mezcla Razor y JS largo: parchear con anclas muy especificas.
- Build completo puede no ser fiable en esta maquina por proyecto WebForms/MVC legacy; usar diagnosticos por archivo y validacion manual.

## Validacion rapida antes de entregar nuevos cambios
1. `get_errors` sobre archivos tocados.
2. Abrir Desing_2 y revisar consola navegador.
3. Guardar diseno y confirmar foto/camara.
4. Abrir recuento y confirmar resumen/detalles.
5. Si se toca ATK60, probar muro 7 m.
