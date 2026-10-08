# Handover — Posición de los elementos ATK-60 (6 al 8 de octubre de 2026)

Los paneles manuales, el giro al abrir y el reflejo del visor están en [HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md](./HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md) y en [HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md](./HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md). Este fichero es lo que se hizo después: las uniones del generador Desing_2 y el fallo general de posición.

Agente de paneles: `.cursor/agents/encofrado-atk-60.md`.
Agente de posición: `.cursor/agents/atk60-posicion-uniones.md`.

Visor: `https://localhost:44384/Desing_2/Viewer?offerId=1&designId=10002`.
C# se ve al recompilar Desing. JS y STL, con Ctrl+F5.

Nada de esto está en un commit. No hacer commit ni push salvo que se pida.

---

## 0. Estado al cerrar el 8 de octubre

La posición es un problema general. No está resuelta.

- El rigidizador (`1850162` / `1850163`) y la placa (`10443020`) se meten en el panel. La inserción a 100 mm se pasó hacia el muro.
- La grapa fija (`10004220`) va desplazada.
- El panel de 0,30 × 2,70 m también está mal.
- El gancho (`1850164`) no entra en el taladro. El giro del que está al inicio del panel se aceptó. El del final puede hacer falta en simétrico.
- La grapa regulable no está confirmada.

---

## 1. Qué se hizo estos días

### 6 y 7 de octubre

Encofrado manual y visor de paneles. Quedó escrito en los handover del 6 y del 7: marcas, simétrico, inferior derecho sin tocar, giro al abrir (Debug121 / Debug122) y reflejo solo del espesor en Desing_2. El diseño de referencia sigue siendo el 10002.

### 8 de octubre — uniones, una familia cada vez

Se insertan en las dos caras, después de los paneles. El generador es `Modulo270PanelElementGenerator`. El visor no recentra los STL de unión y no les suma el +120 mm de los paneles.

**Grapa fija `10004220`.** Alturas desde la base de cada pieza, no desde el suelo del muro. Familia 2700: 450, 1350 y 2250 mm. Familia 2400: 400 y 2000. Familia 1200: 600. Tumbado: costura a 200, 1350 y 2500 a lo largo del módulo, y junta vertical al final del módulo si hay otro. Sin grapas en los extremos del muro. Sin elevación extra. El origen del STL es el punto de inserción. Giro del `InsertUnion` antiguo: Rx −90° delante y Rx +90° detrás, más el yaw del muro. En el tumbado, `270M` / `90M`. Si hay grapa regulable de remate, la fija no va en esa junta. La cota de profundidad (`faceGapMm` = 135) queda desplazada.

**Rigidizador.** Corto `1850162` (750 mm) en el montante, a 300 mm de cada extremo del módulo si mide al menos 1500 mm; si no, al centro. Largo `1850163` (1200 mm) solo cuando el mismo módulo tiene franja de 300 y de 450. Un muro de 3 m (2,70 + 0,30) lleva solo el corto. Azul. En un momento se dio por apoyado en el panel; al mirarlo en el perfil amarillo, la barra y la placa se meten dentro. La cota que lo mete es la inserción a 100 mm (`barOnPanelMm`): la cara del visor está a 120 mm y la malla de la barra ocupa de 20 a 80 mm hacia fuera de su origen, así que el cuerpo cae entre 120 y 180 mm, dentro del perfil.

**Placa `10443020`.** Se reescribió el STL: la cara de contacto es el origen y el agujero queda centrado en X y en Z. El cuerpo va en −Y local, que es hacia fuera. Apoya a 80 mm de la inserción de la barra (la cara exterior medida de `1850162`). Sin giro propio. Al correr la barra a 100 mm, la placa pasó a 180 mm y se mete con ella. Cuando la barra salga del panel, la placa tiene que ir con ella. No sumar otros 120 mm: `faceGap` ya incluye el desplazamiento del panel en el visor, y eso dobló el hueco en planta.

**Gancho `1850164`.** Es una L: ojo con tornillo de unos 205 mm y uña al lado. El STL se giró 180° respecto al original para que el tornillo vaya en +Y y la uña conserve el sentido; el origen está en la cara exterior del ojo y la punta del tornillo en +Y. Alturas ±200 / ±300, el mismo largo que la barra, las dos caras. El giro que se aceptó en el inicio del panel es −90° alrededor de la normal de la cara (eje frontal, en alzado), hacia el perfil. El del final del módulo (`BaseRotZ = 1`, el segundo `along` cuando el módulo lleva barra a 300 mm y a `largo − 300`) lleva +90°. El usuario dejó el giro como aceptable y avisó de que el de la derecha puede seguir mal y hacer falta el simétrico. La profundidad no está dada: el visor resta 240 mm sobre la normal (`faceOut * -240`) y el tornillo no coincide con el taladro.

**Grapa regulable del remate.** Solo si hay remate de 10 a 300 mm. `10000221` en el arranque y `10000221-2` (`10000221B`) separada el largo del remate. Alturas como `Remate180`: cada curso de 2700 a 450, 1350 y 2250; resto hasta 1200 a +200 y +700; resto hasta 2400 a +450 y +1900. En el visor, Ry de 180° y `scale.y = -1` con material de dos caras. El sentido vertical se vio bien; el horizontal, no. No seguir con ella hasta cerrar el gancho.

---

## 2. Ejes, para no repetir el fallo

Escena en milímetros, Y arriba. La normal guardada en la unión sale del muro. En el visor, si la cara es la de detrás, esa normal se invierte antes de calcular el yaw, para que las dos caras compartan el rumbo del muro.

`faceOut` en el gancho es la normal de esa cara, hacia fuera. Restar sobre `faceOut` mete la pieza hacia el muro. Sumar la saca.

El panel, en el visor, se encaja por la esquina y luego se separa 120 mm hacia fuera. Las uniones no hacen ese paso. Por eso un mismo número no significa lo mismo en un panel y en una unión.

Errores que ya se vieron y no hay que rehacer:

- Sumar 120 mm a la placa además de `faceGap`. En planta se desplazó el doble.
- Mover la placa en Y mundial o a lo largo del muro. El usuario lo corrigió: el eje era la profundidad.
- Girar la placa. Estaba bien sin giro.
- Elevar la grapa fija.
- Copiar las cotas de `SedUnionRigiHorizontal_0_Solape` y de `UnionRiji`. La dirección (placa fuera, gancho hacia el taladro) sirve; los números de SEMA03, no.
- Girar el gancho 180° alrededor de la vertical. Quedó al revés.
- Girar el gancho 90° alrededor de la vertical. El eje era el frontal.
- El +90° del eje frontal apuntaba al lado contrario del taladro. El signo que se dejó en el inicio es negativo.
- Meter el rigidizador a 100 mm para «pegarlo». Se mete en el panel.

---

## 3. Mallas

Origen sin centrar, salvo la placa y el gancho, que se reescribieron. Los ficheros son STL ASCII en metros; el visor escala ×1000 si la caja mide menos de 10.

| STL | Caja útil | Origen |
|---|---|---|
| `10004220` | Unos 195 × 104 × 30 mm. Eje largo en +X | El de fábrica. No recentrar |
| `1850162` | X unos 105 mm, Y de −80 a −20 mm (todo el cuerpo en −Y), Z 750 mm | El de fábrica. Tras Rx −90°, −Y es hacia fuera. Piel interior a 20 mm, cara exterior a 80 mm |
| `1850163` | Igual de sección, 1200 mm | No se reescribió |
| `10443020` | Contacto en Y = 0, cuerpo en −Y, agujero centrado | Reescrito el 8 de octubre. Hay copia en `Desing\bin\Content\DesignTools\Stl\ATK60\` |
| `1850164` | Tornillo de Y = 0 a unos 205 mm, radio 7,5 mm. La uña sale en +X y +Z | Reescrito: Rx 180° del original, punta centrada, cara exterior del ojo en Y = 0 |

Manual de las uniones: `Desing\Content\DesignTools\DWG\AtkSystem60\Snaps\NEVI_GUIA_USUARIO.pdf`. El tornillo del gancho atraviesa el taladro del perfil y la placa queda fuera de la barra.

---

## 4. Archivos tocados el 8 de octubre

- `Desing\Repositories\RepositoryAtk60\ModulosATK60\Modulo270PanelElementGenerator.cs` — grapas, rigidizadores, placa, gancho, regulable, y `barOnPanelMm = 100`.
- `Desing\Repositories\RepositoryAtk60\Atk60WallsRepository.cs` — el módulo anterior a un remate no pone la grapa fija del extremo.
- `Desing\Scripts\MasterArticles\master-article-details-stl-viewer.js` — giro del gancho y los −240 mm. Regulable con Ry 180° y `scale.y = -1`.
- `Desing\Content\DesignTools\Stl\ATK60\10443020.stl` y `1850164.stl`.

El 6 y el 7, el visor de paneles y el plugin quedaron como dicen los handover de esas fechas.
