# Handover — AutoCAD inserción de bloques ATK-60

Contrato de diseño (debate 2026-09-29). **Aún no implementado.**

Formulario de biblioteca: [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md)  
Muros / sesión: [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)

---

## 0. START HERE (agente)

1. Leer este fichero antes de tocar INSERT.
2. **Primera release: solo vistas 3D y 3DRef.** Alzado y Planta existen en el modelo mental y en carpetas; no van en UI ni en el convertidor todavía.
3. **No XREF.** El DWG del servidor se **inserta** en el dibujo (definición de bloque + INSERT). Sin capas para simular vistas.
4. Artículo de prueba: panel Atk-60 **2,70 × 0,90**, código comercial **3120270090**, **CodeName** de pieza **`27904209`**.
5. Develop: `TANDEM_LOCAL` + IIS Express. Detalle servidor: bloquing §14.

No implementar hasta que el usuario cierre el debate (selector de vista, punto, acoplamiento).

---

## 1. Qué es (encofrado)

El sistema ATK-60 es un **encofrado**: las piezas se **acoplan** entre sí. La biblioteca no es un INSERT genérico de AutoCAD al azar; cada panel es un bloque de pieza con un origen que más adelante encajará con la siguiente.

Esta fase: **paneles**. Ejemplo: insertar un 2,70 × 0,90 en un punto. El acoplamiento entre piezas se especifica después.

---

## 2. Cuatro vistas (modelo completo)

Cada vista es un **DWG independiente** en el servidor, no un mismo bloque con UCS distinto.

| Vista UI | Carpeta bajo `Desing/Content/DesignTools/DWG/AtkSystem60/` | Sufijo de fichero | Primera release |
|----------|-----------------------------------------------------------|-------------------|-----------------|
| **3DRef** (default) | `3DRef\` | `R` → `27904209R.dwg` | **Sí** |
| **3D** | `3D\` | ninguno → `27904209.dwg` | **Sí** |
| Planta | `Plant\` | `P` → `27904209P.dwg` | No |
| Alzado | `From\` | `F` → `27904209F.dwg` | No |

Ruta absoluta de desarrollo:

`C:\00_Tandem2026\Desing\Content\DesignTools\DWG\AtkSystem60\`

El usuario escribió el alzado como `7904209F.dwg`; se interpreta **`27904209F.dwg`** (typo del `2` inicial) salvo que se corrija.

Default al insertar: **3DRef**.

Código comercial del maestro (`3120270090`) **no** es el nombre del DWG. El encofrado usa **CodeName** (`27904209`, `Atk60Element` / `Panel90270`), igual que los STL.

En el dibujo, las definiciones deben llamarse distinto (`27904209` vs `27904209R`). Si las cuatro se llamaran igual, la tabla de bloques se pisa.

---

## 3. Estado de ficheros (2026-09-29, máquina de desarrollo)

Carpetas `Plant`, `3D`, `3DRef`, `From` existen.

| Fichero esperado | En disco |
|------------------|----------|
| `3D\27904209.dwg` | Sí |
| `3DRef\27904209R.dwg` | **Sí** (118 597 bytes, 2026-09-29 15:58). Caja 0,90×2,70 m + 4 círculos. |
| `Plant\27904209P.dwg` | Carpeta vacía |
| `From\27904209F.dwg` | Carpeta vacía |

La librería histórica sigue en la **raíz** de `AtkSystem60` (p.ej. `27904209.dwg`, `27104219P.dwg`). Primera release: resolver **solo** desde `3D\` y `3DRef\` con el convenio de nombres de §2. Si 3DRef aún no tiene el sufijo `R`, o se renombra el fichero o el código acepta `27904209.dwg` dentro de `3DRef` como fallback — decidirlo al implementar, no mezclar con la raíz.

URL pública MVC típica (cuando se sirva):  
`~/Content/DesignTools/DWG/AtkSystem60/3DRef/27904209R.dwg`  
El plugin puede leer por HTTP (sesión) o por ruta local si Desing corre en la misma máquina. Decisión de I/O al implementar; el contrato es **carpeta + CodeName + sufijo**.

---

## 4. Insertar (primera release)

1. Usuario elige panel en la biblioteca (ya hecha).
2. Elige vista **3D** o **3DRef** (si no toca nada → **3DRef**). **Alzado** y **Planta** salen en el formulario **deshabilitados** (primera release).
3. Pica un punto en CAD.
4. AutoCAD **INSERT** del DWG de esa carpeta (no XREF). Misma idea que un bloque de librería clásico.

Si la vista es **3DRef** y la pieza a insertar **se une a otra ya puesta** (p.ej. grapa a panel), no se pica un punto libre: entran las **marcas de acoplamiento** (§6). El primer panel en dibujo vacío sí puede ser un punto libre.

El JSON actual `insert-block` (`id`, `dwg` del maestro, `caption`) **no basta**: hay que llevar **vista** + **CodeName** (+ familia de unión para filtrar marcas). El `LinkBlockDwgPlant3D` del maestro **no** es la fuente ATK-60.

---

## 5. Convertir un conjunto (paralelo, primera release)

Botón aparte: el usuario **selecciona** varios bloques ya insertados y elige el tipo destino (**3D** o **3DRef**). Todos los seleccionados que sean piezas Tandem pasan a esa vista.

**No XREF, no capas.**

Contrato de transformación (acordado):

1. Leer del INSERT actual: **punto**, **rotación** (y **escala** si no es 1).
2. **Borrar** ese bloque.
3. **Insertar** el DWG de la vista destino con la misma transformación.

Equivalente visual al LISP antiguo que, vía handle + `(entmod)` del nombre de bloque, cambiaba la definición **sin** borrar. El handle se pierde (grupos / un solo undo); para encofrar es aceptable. No reasignar handles en .NET.

Filtrar la selección: solo bloques Tandem (XData o nombre `27904209` / `27904209R`), no un círculo o un bloque ajeno.

Los cuatro DWG de una pieza deben compartir **el mismo punto base** (`INSBASE`). Si 3D y 3DRef tienen orígenes distintos, al convertir el panel **salta** y deja de acoplar. Eso se garantiza en los DWG, no con capas.

Alzado/Planta: misma mecánica **más adelante**; el botón de la primera release solo ofrece 3D y 3DRef.

---

## 6. Marcas de acoplamiento en 3DRef (sin capas)

Los bloques **3DRef** llevan marcas de ayuda. Según **qué pieza se va a insertar** (o a qué pieza se une), solo aplican **algunas** de esas marcas. Ejemplo: una **grapa de unión** (dos paneles) solo es legal en ciertos nudos del panel 2,70 × 0,90. Esos nudos se **iluminan**; al pasar el ratón se destacan; el clic coloca la grapa en la pose correcta.

**No se hace con capas** (ni congelar, ni una capa por tipo de marca). Eso era el truco viejo y ensucia el dibujo.

### Dónde vive el dato (en el DWG 3DRef)

Las marcas van **dentro de la definición** del bloque 3DRef, en coordenadas locales del panel. Cada marca es un nudo con:

- posición (y, si hace falta, rotación / eje) = pose de la pieza que se une;
- **rol**: a qué familia sirve (`GRAPA`, `PANEL`, `PUNTAL`, …).

Identidad del rol: **nombre de bloque anidado** (p.ej. insert `SNAP_GRAPA` dentro de `27904209R`) o **XData** `TANDEM` en un POINT. No el nombre de capa.

En el encofrado MVC las grapas/uniones ya tienen CodeName (`Unionvertical_*`, `UnionHorizonal` → p.ej. `10004220`); el rol de la marca debe cuadrar con esa familia.

Si hoy las marcas en 3DRef están dibujadas “a pelo” en una capa, al preparar librería se convierten a esos inserts/XData. El 3D “de producción” **no** lleva marcas (por eso existe 3DRef).

### Cómo se iluminan (en el plugin, no en el DWG)

Mientras el usuario está insertando una pieza que se acopla:

1. Recorre los paneles **3DRef** ya insertados.
2. Lee las marcas de su definición, **filtra por rol** (grapa → solo `SNAP_GRAPA`).
3. Pasa puntos a WCS (punto local × transformación del INSERT del panel).
4. Dibuja la ayuda con **Transient Graphics** (overlay de AutoCAD): círculos/cruces que **no son entidades**, no tienen capa, desaparecen al terminar el comando.
5. Hover: la marca más cercana al cursor (tolerancia de pick) se dibuja más intensa.
6. Clic: INSERT de la grapa (u otra pieza) con **el mismo punto y rotación que la marca**, no un punto suelto.

Al cancelar o terminar, los transients se limpian. El DWG del panel no se modifica.

### Insert libre vs insert con marcas

| Situación | Comportamiento |
|-----------|----------------|
| Primera pieza / no hay anfitrión | Punto (y ángulo si se define) libre. |
| Pieza de unión (grapa, etc.) sobre 3DRef | Solo marcas del rol; sin clic válido no se inserta. |
| Vista **3D** (sin marcas) | Primera release: insertar por punto, sin ayudas de nudo. Convertir a 3DRef para montar. |

### Qué no hacer aquí

- Encender/apagar capas `GRAPA` / `PANEL` para “ver” las marcas.
- Dejar las cruces de ayuda como líneas permanentes en el dibujo.
- XREF del 3DRef para recargar marcas.

Pendiente de confirmar con el usuario: el **paso real** de grapas (ahora 300 mm / inset 300) y si el origen de su caja coincide con §6.1.

### 6.1 Autoría a escala (~1000 bloques) — sin comprar software

No marcar a mano cada DWG ni comprar un editor de bloques. Las marcas **no son geometría de capa**; son datos.

- **Fuente de verdad:** `Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/{CodeName}.json`
- **Paneles rectangulares:** se generan con `family: panel-rect` (ancho, alto, pitch). El maestro (`NumberHigh` / `NumberWidth`) alimentará el generador cuando se implemente.
- **El 3DRef** es la caja/geometría. El plugin, al insertar una grapa, ilumina transients desde el JSON (× INSERT del panel). Los círculos del DXF son solo para que un humano vea el patrón.

Convención del panel real `27904209R` (medido 2026-09-29, Autodesk COM):

- Parte del bloque **3D** (mismo `INSBASE 0,0,0`). 3D: sólidos; 3DRef: caja simplificada + 4 círculos.
- Geometría en **metros** (X 0→0,90, Y −0,12→0, Z 0→2,70) aunque `INSUNITS=4` (mm). El plugin debe insertar en metros o corregir unidades.
- Marcas: 4 círculos r=0,01 en capa `ATK-60` (el runtime no usará la capa; solo las coordenadas):

| id | X | Y | Z |
|----|---|---|---|
| L055 | 0,044 | −0,119 | 0,55 |
| L215 | 0,044 | −0,119 | 2,15 |
| R055 | 0,856 | −0,119 | 0,55 |
| R215 | 0,856 | −0,119 | 2,15 |

Inset ~44 mm en X; 550 mm desde el pie y desde la cabeza. El JSON `Snaps/27904209.json` ya está alineado con esto. El DXF mm en XY (`27904209-marcas.dxf`) **no** coincide con este UCS; no usarlo para este panel.

Otras familias (esquinas, puntales) tendrán JSON propio; no se fuerza la regla del rectángulo.

---

## 7. Qué no hacer

- XREF / overlay / reload de un DWG externo.
- Encender/apagar capas para mostrar u ocultar marcas de unión.
- Cambiar de vista ocultando capas (3D en una capa, planta en otra).
- Usar `LinkBlockDwg*` del maestro como origen ATK-60 (otra taxonomía: planta/alzado mockup).
- Activar Alzado/Planta en inserción (están en la UI, deshabilitados).
- Inventar nudos que no estén en el 3DRef o en el rol de la pieza.
- Portar a BricsCAD/Revit en el mismo cambio salvo petición.

---

## 8. Relación con el formulario

La paleta (`PluginBlocks`) ya lista, busca y previsualiza STL. **Insertar** hoy solo escribe en la línea de comandos.

Siguiente código (cuando se pida):

- UI: selector 3D / 3DRef (default 3DRef) + botón convertir selección.
- Mensaje paleta: vista + CodeName + id.
- Plugin: INSERT; convertidor = borrar + insertar misma pos/rot; jig 3DRef = transients filtrados por rol.

---

## 9. Pendiente de que el usuario siga

- Selector de vista en la paleta (dónde y cómo).
- Picado libre del primer panel (solo punto, o punto + ángulo).
- Confirmar si las 4 grapas (z=0,55 y 2,15) son el patrón definitivo frente a un paso 300 mm.
- Lista de roles (grapa vertical/horizontal, panel-panel, puntal, …) alineada con `Atk60Element.GetUnion`.
