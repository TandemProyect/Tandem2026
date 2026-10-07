# Handover — Encofrar manual: conexiones de paneles (2026-10-06)

Documento para retomar el insertado **uno a uno** de artículos ATK-60 en AutoCAD y su reflejo en Desing_2. El handover del 3 de octubre sigue valiendo para NETLOAD, ATDESING, sesión y test de conexión. Este fichero cubre lo que se hizo **después** y no se documentó: la fase de conexiones.

La posición al abrir y el visor Desing_2 (6–7 de octubre) están en [HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md](./HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md).

Conversación de origen (el agente se cortó el 2026-10-06 ~13:46, sin cerrar esta documentación): [Sesión plugin ATK-60](c403cd73-821f-4fe0-a7ea-5badfad9da9e).

---

## 0. START HERE

1. Leer este fichero antes de tocar el insertado manual.
2. Rama de trabajo: **`develo`**.
3. Plugin que deja bien el punto **inferior izquierdo** (0° y 90°):

   `TandemAutocadPlugin\AutocadPlugin\bin\Debug116\AutocadPlugin.dll`

4. Presentación el **jueves 2026-10-08**. Insertar por el **inferior izquierdo**. El inferior derecho es un bug conocido; no se toca el camino de `V_BL` para arreglarlo.
5. El issue de GitHub **no se abrió**: `gh` no está en esta máquina y el borrador local se borró. El bug queda descrito en la sección 6.
6. No hacer commit ni push salvo que se pida.
7. Código vivo del snap: `TandemAutocadPlugin/AutocadPlugin/BlockInsertCommand.cs` y `Atk60SnapCatalog.cs`.

---

## 1. Qué es esta fase

En bloquing se elige un panel (por defecto vista **3dref**) y se arrastra por el muro. Intro repite el último artículo. Al pulsar Insertar el bloquing **se pliega**; no se oculta.

Regla de negocio:

- Muro sin paneles: el panel engancha en el punto **inferior izquierdo** de la cara. Al clic se inserta y se duplica en la cara paralela.
- Ese muro pasa a **`Is_Special = 1`**. El encofrado automático (`SolveFromIdsJson`) **no** lo vuelve a modular.
- Los siguientes paneles enganchan en los vértices de los artículos ya puestos, no en una rejilla libre.
- Si el hueco ya tiene pieza, no se duplica.
- En base de datos solo se escribe al pulsar **Salvar** (popup «Salvando en base de datos…» y toast «Diseño salvado»). El clic de insertar no llama a la API en el hilo de AutoCAD.
- Al abrir el diseño, Desing pinta esos artículos como **STL** (el de pie; el 90° se tumba en el visor). No se encofran solos.
- Vista **2D**: se ocultan sólidos y paneles. Vista **3D**: se borran, se levantan los muros y se vuelven a insertar los paneles.

Tablas (develop y producción), scripts en `Desing/Scripts/TemporalScript/`:

| Script | Qué |
|--------|-----|
| `2026-10-05_alter_TSql_DesignWall_Is_Special.sql` | `Is_Special` BIT, default 0, en `TSql_DesignWall` |
| `2026-10-05_create_TSql_DesignWallArticle.sql` | Artículos manuales: `LinkDesignWall`, `LinkDesign_V2`, código, vista, punto, rotación y las 9 columnas de auditoría |

`Is_Special` vuelve a 0 si se borran todos los paneles manuales y el encofrado automático no se ha editado (`WallSpecialCad`).

---

## 2. Conexiones — contrato que hay que respetar

Cada marca tiene **su** vértice y **su** sentido a lo largo del muro. No se proyecta el panel sobre el eje `BottomLeft` del muro: eso corría todos los paneles hacia la izquierda.

### 2.1 Marcas del muro recto (4 por eje)

En la base del prisma, dos caras por dos extremos. En un encuentro en L se apartan del nudo (~150 mm, tope 20 % de la longitud) para que el snap sea del muro y no de la esquina.

Eje canónico, el mismo que el encofrado automático: p1→p2 hacia **+X**; si el muro es vertical en planta, hacia **+Y** (`PickBottom`).

| Cara | Marca | Dónde | `WidthDir` (hacia dónde crece el panel) |
|------|--------|--------|------------------------------------------|
| Espesor `+n` | `V_BL` | extremo izquierdo (p1) | `+along` |
| Espesor `+n` | `V_BR` | extremo derecho (p2) | `+along` |
| Espesor `−n` | `V_BL` | extremo que queda a la izquierda de esa cara (p2) | `−along` |
| Espesor `−n` | `V_BR` | el otro extremo (p1) | `−along` |

La marca se sienta en la **cara** (eje ± mitad de espesor), no en el eje. El cursor elige la cara: solo entran las marcas cuyo `ThickDir` mira hacia fuera respecto al cursor.

### 2.2 Vértices del artículo

Origen del bloque DWG = esquina **inferior izquierda** (`V_BL` = 0,0,0). Catálogo en `Atk60SnapCatalog` (JSON de snaps o, si no hay, 0,90 × 2,70 m):

| Id | Local (m), panel de pie |
|----|-------------------------|
| `V_BL` | (0, 0, 0) — origen. **Este camino funciona.** |
| `V_BR` | (ancho, 0, 0) |
| `V_TL` | (0, 0, alto) |
| `V_TR` | (ancho, 0, alto) |

La paleta manda 0° o 90°. El anfitrión solo gira en planta para compartir la cara (`MatchHostSentido`: el espesor del panel, eje Y, coincide con `ThickDir`). Si el usuario va a 0° no se copia un tumbado del anfitrión.

### 2.3 Cómo engancha cada punto

**Inferior izquierdo (`V_BL` / `V_TL`) — congelado, no tocarlo.**

- El origen del bloque se pone **en el punto**.
- 0° y 90° quedan en la cara, alineados con el muro.
- En muro vacío y panel de pie, al confirmar se aplica el giro de encofrado (sección 3). Ese giro es el que deja el DWG **fuera, sobre la cara**. Quitarlo mete todos los paneles **dentro** del muro.

**Inferior derecho (`V_BR` / `V_TR`) — bug, sección 6.**

- Hay que apoyar el vértice **derecho del panel** (con la misma rotación 0°/90°) sobre esa marca, y crecer **hacia el interior** siguiendo el `WidthDir` de **esa** marca.
- No reutilizar el desplazamiento del eje `BottomLeft`, ni el camino de `V_BL`.

**Panel ya insertado.**

- El cursor elige el vértice más cercano en pantalla.
- `Place` / `PickMate` elige qué esquina del panel nuevo cae en ese vértice (al lado o apilado), según hacia dónde está el cursor respecto al ancho y al alto del anfitrión.
- Si esa pose ya está ocupada (~40 mm, mismo muro), no se inserta otro.

**Muro vacío, sin enganchar marca.**

- Misma pose que el automático: inserto en la inferior izquierda de la cara del cursor; la cara opuesta lleva 180° y el punto avanza un ancho de pieza para no desplazar la huella (`TryFormworkFacePoses`).

---

## 3. Por qué el DWG lleva un giro de 180°

El DWG mira al revés que el STL. `ApplyFormworkPanelMatrix` gira **180° en planta sobre el centro de la base** para no mover la huella. Es el mismo criterio que `FormworkCommand`.

Ese giro solo entra en muro **sin paneles** y panel **de pie** (no a 90°). En cuanto el muro ya tiene paneles, o el artículo va a 90°, se usa `ApplyPanelMatrix` (origen + orientación, sin ese 180°).

La cara paralela no es un espejo zurdo: mismo ancho a lo largo del muro, espesor hacia fuera en la otra cara, separado el espesor. Voltear también el eje vertical mandaba el simétrico hacia abajo.

---

## 4. Qué se guarda (y qué no)

Al salvar, la coordenada de la tabla es el **origen lógico** guardado en XData `AT:x,y,z` (unidades DWG del inserto), no `BlockReference.Position`.

Tras el 180° del DWG, `Position` queda desplazado **un ancho de panel** (~900 mm en un 0,90). Grabar ese `Position` en `NumberInsertX/Y/Z` y volver a aplicar el 180° al reabrir corría los paneles en AutoCAD y en Desing_2.

Al reabrir, `InsertWorld` lee `AT:`. Si el visor vuelve a salvar, no debe pisar esas coordenadas con la pose de pantalla.

Los diseños salvados **antes** de este arreglo siguen mal en SQL. Hay que insertar otra vez y pulsar Salvar.

XData del artículo: código, vista, rol `PANEL`, rotación (1070), `WID:<id muro>`, `AT:x,y,z`.

---

## 5. Desing_2

- `GetDesignWalls` trae los artículos con `StlUrl` / `StlPhenolicUrl`.
- Se colocan en el punto y el giro guardados.
- Muro `Is_Special`: **Encofrar** lo omite.
- Panel a 90°: usar el **STL de pie** y tumbarlo 90° en la esquina de inserción. El `*T.stl` ya tumbado, puesto en la esquina del panel de pie, se ve corrido en alzado.
- El signo del tumbado en el visor no es el mismo que en AutoCAD: en CAD el tumbado es −90° alrededor del espesor; en Desing eso equivale a **+90° en Z local**, si no el panel cuelga bajo el inserto.

---

## 6. Bug conocido — punto inferior derecho

**Estado:** abierto. Decisión del 2026-10-06, a dos días de la demo: **no tocarlo**. El izquierdo (0° y 90°) queda bien en Debug116. Arreglar el derecho rompió el izquierdo varias veces.

**Síntoma (probado por el usuario, con foto):** al elegir el punto inferior derecho de la cara, el panel sale **rotado y fuera del muro**. El inferior izquierdo, en el mismo muro, queda bien.

**Qué no repetir:**

- Quitar el 180° del encofrado en el snap. Todos los paneles entran **dentro** del muro.
- Correr el origen «una anchura» proyectando sobre el eje del muro / `BottomLeft`. Los verticales se desplazan a la izquierda y fallan también los que ya iban bien.
- Aplicar al derecho el mismo inserto que al izquierdo sin cambiar de vértice: el origen es `V_BL`, así que el panel cuelga fuera del extremo derecho.
- Confirmar con un 180° distinto del preview: el clic metía el panel hacia dentro aunque el arrastre se veía bien.

**Arreglo pendiente (solo después del jueves):** colocar `V_BR` (y `V_TR` si es la marca alta) del panel, con su rotación 0°/90°, sobre esa marca, usando el `WidthDir` de la marca. No pasar por el código de `V_BL`.

Para la demo: insertar por el **inferior izquierdo**.

---

## 7. NETLOAD de esta fase (de más viejo a más nuevo)

| Build | Qué quedó |
|-------|-----------|
| Debug94–95 | Simétrico en la cara paralela; 2D oculta paneles, 3D los restaura |
| Debug96 | Popup salvar + toast |
| Debug97–100 | Giro alineado al eje horario; 180° de DWG como el automático |
| Debug106 | Hueco libre; no duplicar si ya hay panel |
| Debug107 | Login local, reloj, Wi‑Fi / servidor, color de plantilla |
| Debug111 | Guardar `AT:` y no `Position` (luego se tocó el inserto sin querer) |
| Debug112 | Inserto restaurado; bloquing se pliega; se mantiene el guardado `AT:` |
| Debug113–115 | Intentos del derecho que empeoraron el izquierdo. No usarlos |
| **Debug116** | Izquierdo restaurado (`V_BL`). Derecho sigue mal. **Esta es la build** |

Si AutoCAD tiene otra `DebugN` cargada, cerrar esa sesión o cargar esta ruta. Una DLL ya cargada no se sustituye en caliente.
