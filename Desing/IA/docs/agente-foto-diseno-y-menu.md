# Foto del diseño y apartado «Últimos diseños»

Para el siguiente agente. Hecho el **14 de octubre de 2026**. El frente del informe quincenal 02 (apartado 11) está cerrado.

No tocar el snap inferior derecho, el cubo de vistas ni el ZWCAD que tenga abierto el usuario.

## Qué hace

1. En el visor Desing_2, el botón de cámara descarga un PNG del lienzo (cielo y luz incluidos). No entran barras, menú ni herramientas.
2. Al guardar muros y al descargar la foto, el mismo PNG se escribe en el servidor y la ruta queda en `TSql_Design_V2.AttPhoto`.
3. En el menú lateral, bajo Ayuda, el apartado **Últimos diseños** muestra esa foto del usuario que ha entrado.

## Archivos

| Archivo | Rol |
|--------|-----|
| `Views/Desing_2/_Desing2StlViewerWorkspace.cshtml` | Botón `#ma-stl-screenshot` junto a los iconos de entorno. Avisos en `data-ma-stl-*`. |
| `Scripts/MasterArticles/master-article-details-stl-viewer.js` | `maStlCaptureDesignPngDataUrl`, guardado y toasts. |
| `Controllers/Desing_2Controller.cs` | `SaveDesignPhoto`. |
| `Helpers/LastUserDesignMenuHelper.cs` | Lista de diseños del usuario para el menú. |
| `Controllers/BaseController.cs` | `ViewBag.LastUserDesigns` en el chrome, solo con sesión. |
| `Views/Shared/_SidebarMaterio.cshtml` | Apartado, clic / doble clic y aviso «Abriendo diseño». |
| `assets/materio/css/site.css` | Tamaño de la foto y el aviso. |
| `Scripts/TemporalScript/2026-10-14_alter_TSql_Design_V2_AttPhoto.sql` | Columna `AttPhoto`. |
| `Scripts/TemporalScript/2026-10-14_seed_TSql_UiTranslation_Menu_LastDesigns.sql` | Textos del apartado. |

`AttPhoto` **no** está en el EDMX. No editar `DAL/Model.edmx` a mano. Se lee y se escribe con SQL. `AttThumbnail` es la ruta del STL: no usarla para la foto.

## Captura

El renderer se crea **sin** `preserveDrawingBuffer`. Hay que hacer `renderer.render` (o el composer SSAO) y, en el mismo instante, `gl.readPixels`. Volcar el buffer a un canvas 2D, darle la vuelta en Y y usar `toDataURL('image/png')`. `toBlob` puede salir en blanco porque el buffer ya se ha limpiado.

El código viejo (`Scripts/Design/3D/Design-3d-Screenshot.js`, `#btnPrintDesing`) esperaba un segundo y guardaba `Hola.png`. No volver a eso.

## Fichero y base de datos

- Carpeta: `~/Content/images/desing/yyyy-MM/`
- Nombre: `AttLabel` saneado + `.png`. Si ya existe, se sustituye.
- `POST Desing_2/SaveDesignPhoto` con `{ DesignId, PngBase64 }`.
- PNG real (`89 50 4E 47`), máximo 12 MB.
- El `UPDATE` toca `AttPhoto`, `AttChange`, `LinModifiedBy` y `SysUpdateNumber = SysUpdateNumber + 1`.
- El guardado de muros llama a la foto después del `Exito`. Si la foto falla, no se aborta la salida del visor. El botón de guardar sí muestra el fallo en rojo.

Avisos (es): muros y foto guardados; imagen descargada y foto guardada; imagen descargada; imagen descargada y la foto no se pudo guardar.

## Menú «Últimos diseños»

Consulta: diseños con `LinCreatedBy` del usuario, `AttIsDeleted = 0`, orden `AttCreated DESC`. El número de oferta es `TSql_Offers.AddOfferNumber`. Si la consulta falla (por ejemplo falta `AttPhoto` en el servidor), el apartado no sale: el helper traga la excepción.

- Un clic en la foto pasa al diseño anterior. Al llegar al último, vuelve al más reciente.
- El doble clic abre el visor y muestra el aviso **Abriendo diseño** con el reloj (`#tandem-opening-design`).
- Abierto: foto **150×108**, debajo el número de oferta, «Doble click para abrir» y el nombre.
- Plegado: la foto cabe en el carril (unos 46 px) y no sale el título.

Textos con `@Html.Ui` y reserva en español, módulo `Common`:

- `Menu.LastDesigns`
- `Menu.LastDesignsDoubleClick`
- `Menu.OpeningDesign`

La semilla local puede no estar aplicada: la reserva ya pinta el texto. No hace falta Node para los `.resx` de este apartado.

## No romper el plegado del menú

El botón es `#navbarMenuToggle`. En escritorio (≥ 1200 px) alterna `layout-menu-collapsed` en `<html>` y lo guarda en la cookie `tandem_menu_collapsed_<dt-user>`. Ver `Views/Shared/_LayoutMaterio.cshtml` y la sección 4 de `agente-ui-materio-datatables.md`.

**No poner `overflow: visible` en `#layout-menu` ni en `.menu-inner` cuando el menú está plegado.** Las entradas siguen midiendo el ancho abierto y, si se salen, tapan el botón: el clic no pliega ni despliega. La foto del diseño plegado tiene que quedar recortada dentro del carril.

## Producción

El código se publica con el perfil `Properties/PublishProfiles/tDesing.net.pubxml` (MSDeploy, Release, AnyCPU). La base **no** se publica.

En SQL de producción hay que ejecutar, si aún no está, `2026-10-14_alter_TSql_Design_V2_AttPhoto.sql`. Sin esa columna la foto no se guarda y el apartado del menú no aparece. El script es idempotente.

## Relacionado

- Paneles y DataTables del inicio: `.cursor/rules/intranet-paneles-datatables.mdc` y `agente-ui-materio-datatables.md`.
- Informe: `Report/2026/2026-10-22_Informe-Quincenal.md`, apartado 11.
