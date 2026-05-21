# Scroll en layout Materio (`tandem-intranet-chrome`)

## Comportamiento

- En `_LayoutMaterio.cshtml`, `<html>` lleva la clase **`tandem-intranet-chrome`**. Ahí **`html`** y **`body`** usan **`overflow: hidden`** para que **no** haya dos barras de scroll (documento + sidebar).
- El scroll vertical **normal** debe ocurrir solo en **`.tandem-layout-main-scroll`** (el `container-fluid` que envuelve `@RenderBody()`).

## Por qué “desaparece” la barra de scroll

Si la cadena flex **no acota** la altura del contenedor scrollable (`flex-basis` efectivo ≠ `0` o falta `min-height: 0`), esa caja **crece con todo el contenido**. Entonces **`overflow-y: auto` no activa** (no hay desbordamiento interno), pero **`body` sigue ocultando el overflow**: el usuario ve la página “cortada” y parece que la barra desapareció **tras cargar** fuentes, Maps o DataTables (cuando el alto real del contenido se estabiliza).

## Reglas fuertes en `site.css`

- **`flex-basis: 0`** + **`min-height: 0`** en **`layout-wrapper` → `layout-page` → `content-wrapper` → `.tandem-layout-main-scroll`**, con **`!important`** donde Bootstrap (`flex-grow-1 !important`) puede romper el contrato.
- Pantallas que delegan el scroll **dentro** (`.tandem-dt-list-page`, `.tandem-jobside-workspace`, `.tandem-desing2-stl-viewport`) usan **`:has(...)`** y vuelven a **`overflow: hidden !important`** en el mismo contenedor para no pisar el scroll interno.

## Verificación rápida

1. Abrir `/TSql_Company/Edit/1`: debe hacer scroll la zona principal (entre navbar y pie); pie visible al fondo del viewport.
2. Abrir un índice DataTables con `.tandem-dt-list-page`: scroll en la tabla, no un documento entero suelto.
3. Abrir visor Desing_2 STL (`body.desing2-stl-fullpage` si aplica): lienzo a pantalla completa en el área de contenido.
