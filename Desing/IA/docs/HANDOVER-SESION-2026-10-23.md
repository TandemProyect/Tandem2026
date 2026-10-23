# Handover — 22 y 23 de octubre de 2026

Para el siguiente agente. Las horas y el relato corto están en el informe abierto `Desing/Report/2026/2026-11-04_Informe-Quincenal.md` (informe 03). Este fichero dice dónde está el código y qué no hay que reabrir.

No publicar. Publica la persona. No hacer commit salvo que lo pida. No reescribir los informes 01 y 02.

## Qué está cerrado

### Direcciones Google (apartado 5, cerrado 22 oct 21:35)

Al escribir una dirección, Google solo proponía España. Ahora la lista es propia y no filtra por país.

- Script: `Desing/Scripts/Intranet/tandem-address-places.js`
- Vista y caché `?v=20261022j`: `Desing/Views/Shared/_GooglePlacesAddressScripts.cshtml`
- Bloque: `Desing/Views/Shared/_GooglePlacesAddressBlock.cshtml`
- Estilo: `Desing/Content/Intranet/tandem-address-places.css`
- Guía: `Desing/IA/Intranet/Google-Places-Direcciones.md`

Buscan **Buscar dirección** (`{Prefix}_PlacesSearch`) y **Dirección formateada** (`{Prefix}_Formatted_Address`). La lista se coloca bajo el campo con el foco. Al elegir, `importLibrary('maps')` pinta el mapa y lo reajusta; las coordenadas se guardan igual. `GoogleMaps:Region=ES` solo sesga el mapa.

Mismo bloque en obra (local y facturación), cliente, empresa y delegación, incluido el modal de delegación. En empresa, Dirección y Dirección 2 siguen siendo texto libre. El visor 3D busca edificios por OpenStreetMap (`BuscarDireccionOsm`), no por este script.

La clave del mapa vive en `GoogleMaps.local.config` (está en el proyecto y se publica; no está en git). No copiar la clave a un documento ni a un chat.

Consola que no es el fallo: `google.maps.Marker is deprecated`, Tracking Prevention de Edge, y `No Listener: tabs:outgoing.message.ready` (extensión del navegador).

### Grapas de AutoCAD (apartado 1, cerrado 22 oct 12:32)

`Debug138`. La grapa fija `10004220` se mueve sobre la línea de junta. No seguir con la grapa variable: aún no hay DWG. No recentrar mallas ni cambiar el origen del STL.

## Qué sigue abierto

### Formulario del muro (apartado 2, a prueba desde las 9:59)

Doble clic en el visor abre Modificar muro. El formulario compartido es `Scripts/MasterArticles/wall-edit-form.js`. No retocar la pose del visor ni el clamp.

### Rigidizadores (apartado 3, parón 15:06)

Último DLL: `TandemAutocadPlugin/AutocadPlugin/bin/Debug141/AutocadPlugin.dll` (22 oct 2026 14:44). Corto `1850162` y largo `1850163` entran en `ENCOFRAR`. Si se retoma, el orden que dejó la persona es: fijador `1850164`, tuerca `10443020`, y después el bloquín. En TapeS1 y TapeS2 el fijador no se pinta. No hay DWG de fijador ni tuerca en `3D` / `3DRef`. No retocar giro ni posición ya aceptados.

### Segundo trabajador (apartado 4, abierto 15:06)

Fotos del selector: `Desing/Content/DesignTools/Stl/Worker/Worker.png` y `Worker2.png`, 480×640, sin nombre. Malla `Worker2.obj` en el proyecto. `TextCode` está aplicado en develop local, producción local y producción de `SQL5113`. En el servidor no hay base develop. No volver a publicar ni a ejecutar esos scripts. No repintar el trabajador ni quitar el giro de 180° de `Worker2`.

### Botón Editar (apartado 6, entrega 22 oct 21:42, sin cerrar)

En la tarjeta de datos de la ficha:

- Obra: enlace a `Jobside/Edit` en `_JobsideWorkspaceLeftPanel.cshtml`. La barra de arriba ya lo tenía.
- Oferta: botón `.js-offer-workspace-open-edit` en la tarjeta y en la barra. Abre el formulario modal, no una página Edit. El listener está en `OfferDetails.cshtml`.
- Cliente: enlace a `ClientV2/Edit` en la tarjeta de `ClientV2/Details.cshtml`.

Empresa, delegación y los catálogos (país, idioma, tipo de documento, extensión, estado de oferta, dispositivo, plantilla, artículo) ya tenían Editar en la cabecera. No duplicarlo.

## Informe: pasa a ser semanal

Hasta el **4 nov 2026** se sigue escribiendo el 03. Ese día se cierra y el siguiente fichero es semanal. Norma en `Desing/Report/LEEME.md` y en `.cursor/rules/informe-quincenal.mdc`. No crear el 04 antes del 4 nov. No abrir un informe trimestral.

El PDF unido `Desing/Report/2026/Informe 01-08 a 22-10 2026.md` es una copia ordenada por fecha. El diario de esta semana es `Desing/Report/2026/Informe Semanal 26102026-2112026.md`.

## Cliente al editar la obra (23 oct 2026)

En Editar obra el desplegable salía en «Sin cliente» aunque `LinkClient_V2` tuviera valor. `ViewBag` se llamaba igual que el campo y `DropDownListFor` no marcaba la opción. La lista ahora es `ViewBag.ClientOptions` (y la delegación, `ViewBag.BranchOptions`). El cliente adjudicado se incluye aunque no esté activo, para verlo y poder cambiarlo. Vistas: `_JobsideFormFields.cshtml` y `_JobsideOfferForm.cshtml`. Falta que se recargue Editar obra y se confirme.

## Recuento junto a abrir diseño (23 oct 2026, 10:00)

Si el diseño tiene piezas en `TSql_DesignWallArticle` (`Is_Delete = 0` y `Is_Active = 1`), junto al icono de abrir diseño sale otro icono. Abre el cuadro `#designMaterialCountModal` y lee `GET /Desing_2/ListDesignMaterialCount?designId=`. El cuadro muestra código, nombre, cantidad, peso unidad, peso total, m² y m² total, más el resumen. Si no hay recuento, el icono no se pinta.

- Icono y consulta por lote: `Desing/Helpers/DesignMaterialCountUi.cs`.
- Cuadro: `Desing/Views/Shared/_DesignMaterialCountModal.cshtml`, incluido en `_LayoutMaterio.cshtml`. Script: `Desing/Scripts/Intranet/design-material-count-popup.js`.
- Listas: inicio (tabla, fotos y árboles de oferta y cliente) en `HomeController`; diseños de la oferta y árbol de ofertas de la obra en `JobsideController`; diseños del cliente en `ClientV2Controller`; tarjeta de diseño recién creado en `_OfferWorkspaceDesignRow.cshtml` (ahí casi nunca hay recuento).
- Textos nuevos en `_gen_masterarticles_resources.js` (peso, m² y resumen). No poner el icono junto al ojo de obra, oferta o cliente: esos abren la ficha, no el diseño.
- El plugin de AutoCAD abre el diseño con la fila entera y usa otro layout. No lleva este cuadro.
- 23 oct 10:18: el cuadro salía vacío aunque había piezas. `QueryLatestActive` comparaba `AddDateMade` al milisegundo y la tanda no coincidía. Ahora usa una ventana de 50 ms. En el inicio, el hover de la fila ya no enciende a la vez el ojo y el recuento.
