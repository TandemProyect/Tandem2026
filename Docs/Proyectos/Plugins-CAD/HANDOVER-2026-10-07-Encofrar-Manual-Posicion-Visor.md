# Handover — Encofrar manual: posición al abrir y visor Desing (2026-10-07)

Lo de marcas, simétrico y bug del inferior derecho sigue en [HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md](./HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md). Este fichero es lo que se hizo **después** de ese corte (tarde del 6 y mañana del 7) y no se documentó.

Conversaciones: [ATK-60 manual](0beee047-0c8b-4592-9c27-6b4e732d61cc) y [Trabajo perdido por reinicio](8c1de869-f2da-406b-ac37-4e98da2c5f32). Agente de proyecto: `.cursor/agents/encofrado-atk-60.md`.

---

## 0. START HERE

1. Leer este fichero antes de tocar la recuperación de artículos o el visor Desing_2.
2. Rama de trabajo: **`develo`**. Último commit de esta línea: `62588e2` (2026-10-06, «Pluging conexion»). El código de esta fase está sin commit.
3. Plugin que hay que cargar para reabrir un dibujo: **Debug122**

   `TandemAutocadPlugin\AutocadPlugin\bin\Debug122\AutocadPlugin.dll`

   Si esa carpeta no está, recompilar el fuente actual: ya incluye `EncodePoseZ`. Debug120 dejó los artículos fuera de la cara; no usarlo.
4. Un diseño salvado con una build anterior hay que **Salvarlo otra vez** con 121 o 122. La base vieja guarda el rumbo del eje, no el giro del bloque.
5. Presentación el **jueves 2026-10-08**. Insertar por el **inferior izquierdo**. El inferior derecho sigue abierto; no se toca `V_BL`.
6. En Desing, el último cambio del visor (reflejo solo del espesor) se pidió comprobar con **Ctrl+F5** y **no hubo confirmación**.
7. No hacer commit ni push salvo que se pida.

---

## 1. Qué es esta fase

AutoCAD ya dejaba la huella bien. Al abrir el diseño, y al pintarlo en Desing_2, los paneles se iban.

Casos usados:

| JSON | Qué es |
|------|--------|
| `C:\temp\Posicion.json` | Diseño **10002** (antes también el diseño 2, muro 161): muro de 10 m, paneles `27904209` en las dos caras |
| `C:\temp\conexiones.json` | El mismo muro al abrir: caras de 2.822 a 12.822 (10 m), separadas 300 mm, Y = 1250 y Y = 1550 |

Caja del panel de pie: **0,90 × 0,10 × 2,70 m**. Varios STL de ATK-60 son texto ASCII; leerlos como binario revienta el lector.

El punto que se manda a la base es el origen del bloque (`sentToDatabaseMm` / `InsertMm`). El `Position` de AutoCAD va corrido por el giro del DWG (900 mm) y no es lo que se guarda.

---

## 2. Qué se arregló en el plugin

### Solape al abrir

El punto guardado ya estaba en las dos caras, a 150 mm del eje (mitad de 300 mm). El solape venía de otra parte:

- Dos bloques iguales, `2FB` y `2FD`, en el mismo punto y con el mismo giro. Al cargar se insertaban los dos.
- Un muro manual (`Is_Special`) que ya no estaba en el dibujo no se retiraba al salvar, y al abrir se volvía a dibujar junto al nuevo.

### Simétrico a 90°

El de la otra cara se desplazaba 2,70 m. Si ahí había un panel de pie, se daba el hueco por ocupado y no se insertaba.

Quedó así (`BlockInsertCommand`, comentario junto a `TryOppositeFromSnapped`): el 180° da la vuelta al panel y el origen se corre el largo de la pieza (0,90 m de pie, 2,70 m tumbado) para que la huella siga en la misma estación. Un panel de pie no ocupa el hueco de uno a 90°.

Build de ese paso: **Debug119**.

### Giro al abrir

El origen en base cae en la cara. Al abrir, el cuerpo salía al otro lado de la línea.

Causa: se guardaba el rumbo del eje (`ArticleRotations`), no el giro con el que el bloque estaba puesto. Un panel enganchado a otro que ya dio la vuelta no coincide con `RotationY`. Al insertar, el nuevo copia la cara del que ya está; si ese dio la vuelta, el nuevo también.

**Debug120** quitó el 180° extra al abrir y empeoró: los artículos quedaron al lado de las líneas. No usar esa build.

**Debug121** guarda el giro visual del bloque:

| `RotationZ` | Significado |
|-------------|-------------|
| `\|Z\| < 3000` | Giro antiguo, sin pose codificada. El 180° de cara sigue en `RotationY` |
| `4000 + yaw` | Pose visual. La cara Y=0 del bloque queda contra el muro |
| `5000 + yaw` (`\|Z\| >= 4500`) | Primer panel de muro vacío. Lleva el 180° de encofrado sobre el centro |

Código: `EncodePoseZ` / `DecodePoseZ` en `BlockInsertCommand.cs`. Al capturar, `WallArticleCad` calcula el rumbo del eje y **después** lo sustituye con `EncodePoseZ`. Al restaurar, si la pose va codificada y es el primer panel de pie, se coloca con `ApplyFormworkPanelMatrix` (yaw − 180°).

Salvar de nuevo es obligatorio: lo que ya estaba en la base no trae este giro.

### Panel del extremo que desaparecía

El último horizontal, de **5,4 m a 8,1 m**, sí estaba guardado. En el visor el largo de 2,70 m salía hacia atrás y tapaba al anterior, así que a la derecha el muro se veía vacío.

En AutoCAD se perdía además un panel de la cara de atrás porque compartía la esquina con el primero. Eso queda en **Debug122**. Cargarlo antes de levantar el 3D.

---

## 3. Visor Desing_2

Archivo: `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`.

Funciones: `maStlDesing2RenderManualWallArticles`, `maStlDesing2ReflectManualArticleOutward`, `maStlDesing2FaceOutwardNormal`.

Cuando AutoCAD ya estaba bien, el visor tumbaba otra vez el panel a 90° y le sumaba el giro de la cara de atrás. Salían losas planas y huecos.

A la mañana del 7 el usuario vio otra cosa: los dos primeros bien; el resto girados y los simétricos hacia dentro.

Un reflejo a lo largo del muro (usando el largo como si fuera el espesor) montó los otros ocho encima de los dos primeros y estropeó también esos dos. Ese reflejo se quitó.

Estado dejado en el código, **sin confirmar en pantalla**:

- El primer panel (`spun`, `RotationZ` con 5000) no se refleja. El 180° ya está en el punto lógico y el STL no lleva ese giro del DWG.
- Los demás se reflejan solo en el espesor, por la normal de la cara hacia fuera.
- Tumbado (`RotationX ≈ 90`): el ancho 0,90 queda arriba y el largo 2,70 a lo largo del muro; el mínimo Y se apoya en la cota de inserción.

Comprobación pedida en el diseño **10002**, con Ctrl+F5:

- De 0 a 0,90 m: par de pie, enrejado hacia fuera.
- De 0,90 a 2,70 m: los otros dos de pie, por fuera en las dos caras.
- De 2,70 a 8,10 m: horizontales de 0,90 m de alto, en las dos caras.

---

## 4. Qué no repetir

- Reflejar el panel a lo largo del muro en el visor. Se montan encima de los primeros.
- Volver a aplicar el 180° del primer panel al abrir (Debug120).
- Guardar solo el rumbo del eje. El panel copiado de uno que ya dio la vuelta sale al otro lado de la línea.
- Tratar un panel de pie como ocupante del hueco de uno a 90°.
- Desplazar el simétrico 2,70 m y dar el hueco por ocupado.
- Tumbar el STL en el visor si el giro de 90° ya viene en la pose.
- Leer el STL `27904209` como binario.
- Tocar `V_BL` para arreglar el punto inferior derecho.

---

## 5. NETLOAD de esta fase

El corte del 6 termina en **Debug116** (izquierdo bien, derecho mal). Desde ahí:

| Build | Qué quedó |
|-------|-----------|
| Debug119 | Simétrico a 90° en la misma estación. Origen corrido el largo de la pieza |
| Debug120 | Se quitó el 180° al abrir. Los artículos salen fuera de la cara. No usar |
| Debug121 | Se guarda el giro visual del bloque (`EncodePoseZ`). Hay que Salvar de nuevo |
| **Debug122** | Cara de atrás que compartía esquina con el primero. **Esta es la build** para AutoCAD |

El visor no va en la DLL. Tras cambiar `master-article-details-stl-viewer.js`, recargar el diseño con Ctrl+F5.
