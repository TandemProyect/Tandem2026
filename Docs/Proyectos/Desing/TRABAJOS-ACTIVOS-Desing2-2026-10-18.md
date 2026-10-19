# Trabajos activos Desing_2 (2026-10-18)

Documento vivo para continuar sin perder contexto. El handover completo esta en `HANDOVER-2026-10-18-Desing2-ATK60-Recuento-Guardado.md`.

## Estado actual
- Guardado de diseno: funcional y rapido segun validacion del usuario.
- Foto: se guarda en `TSql_Design_V2.AttPhoto` y se refresca con URL versionada.
- Camara: se guarda en cookie por diseno y se actualiza `AttActiveCameraType`; preparado para usar `AttViewerStateJson` si existe.
- Recuento: muestra carga, resumen, articulos, detalles y superficies.
- ATK60: altura alta corregida para caso 7 m.
- Barra superior: `Pilares` e `Insertar worker` están activos. El relato está en el informe quincenal, apartados 18 y 20.
- Publicación: el OBJ del trabajador tiene que estar en `Design.csproj`. Git ignora `*.obj`; la excepción es `Desing/Content/DesignTools/Stl/Worker/Worker.obj`.

## Hecho: Pilares (cerrado el 19 oct 2026)

El botón inserta el pilar, el formulario lo modifica y el encofrado lleva paneles, ganchos y puntales. El relato está en el informe quincenal, apartado 20. El punto exacto del pasador del puntal queda para retomarlo.

## Hecho: Insertar trabajador (18–19 oct 2026)

El doble clic lo deja en el suelo o sobre los tablones. Al borrar el muro, se borra el trabajador de ese muro. El modelo que carga el visor es `Content/DesignTools/Stl/Worker/Worker.obj` y tiene que ir en `Design.csproj`. El relato está en el apartado 18.

## Pendiente: persistencia completa de vista en BD

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
