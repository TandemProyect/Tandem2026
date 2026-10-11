# Tirantes ATK-60 en Desing_2

Cerrado el 9 de octubre de 2026. Tiempo de esa mañana: **2 h 40 min** (de 9:14 a 11:52), en Cursor.

El 7 y el 8 de octubre se llamó tirante al gancho `1850164`. No lo es. Ese gancho va con el rigidizador y sigue sin entrar en el taladro. El detalle de ese fallo está en [HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md](./HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md).

El tirante es otra cosa: una placa en cada cara del panel y una barra Dywidag que las atraviesa. Diseño de referencia: `https://localhost:44384/Desing_2/Viewer?offerId=1&designId=10002`.

C# se ve al recompilar Desing. JS y STL, con Ctrl+F5. El visor guarda la plantilla del STL por ruta.

Nada de esto está en un commit.

---

## 1. Regla de inserción

Para cualquier artículo, el punto de inserción es:

1. La coordenada del muro (la cara exterior del hormigón, el ancla del generador).
2. La rotación del muro, que da la normal hacia fuera.
3. 120 mm en esa dirección. Es el ancho del panel.

Ese punto del mundo es el origen del STL. En la placa, el origen es el punto rojo del centro de la cara cuadrada. No se mueve el STL para corregir la profundidad.

La constante es `PanelFaceMm = 120` en `Modulo270PanelElementGenerator`. La usan la grapa fija, la grapa regulable, el rigidizador, el gancho, la placa y las placas del tirante. El visor ya no resta 240 mm al gancho.

El panel, en el visor, se encaja por la esquina y luego se separa esos mismos 120 mm. La unión no repite ese paso: los 120 mm van en el generador.

## 2. Dónde van las placas

En el agujero del panel, no en la junta entre paneles. El agujero medido en el 3DRef está a **43,714 mm** del canto vertical. Se toma el de la izquierda de cada panel. En el último canto libre del muro también el de la derecha.

Alturas, desde el pie y desde la cabeza:

| Panel | Desde cada extremo |
|-------|--------------------|
| 2,70 y 2,40 m | 550 mm |
| 1,20 m | 350 mm |

Tumbado (el lado largo es el ancho de la pieza, casi siempre 2,70 m):

- Si debajo hay un panel vertical, o no hay nada tumbado encima: dos filas, a 43,7 mm de cada canto largo. A lo largo, a 550 mm y a `ancho − 550`.
- Si debajo hay otro tumbado: una sola fila, en la junta, puesta en el de encima, a 43,7 mm de su base. Un 0,45 con un 0,30 encima lleva una fila, no dos.
- Si encima hay un tumbado y debajo no hay vertical: este de abajo no pone filas. Las pone el de encima.
- Placa de ala `1920894` y tuerca hexagonal `7238001`: solo en la fila baja (la de 43,7 mm) del tumbado que arranca en el suelo. Las dos caras. La tuerca en el mismo punto que la placa de ala.

Las placas normales son `10443020`, zinc, las dos caras.

## 3. La barra

Una barra por agujero, no una por cara. El origen del STL es el **centro** de la barra (el modelo de fábrica empezaba en un extremo; el visor lo centra al cargar `dywidag.stl`). Ese origen cae en el centro del muro, a media espesor, en el mismo largo y la misma altura que las placas. Así atraviesa las dos.

La longitud es la del catálogo antiguo, escrita en corto:

`ancho del muro + 240 + 150 + 100`, al alza de 100 mm, entre 500 y 2500.

Los 240 mm son los dos paneles. Los 150 mm, la rosca. Los 100 mm, la holgura con la que el código antiguo subía antes de redondear.

| Ancho del muro | Barra | Código |
|----------------|-------|--------|
| 200 mm | 700 mm | `230070` |
| 300 mm | 800 mm | `230080` |

El código es `230` más la longitud en centímetros, con tres cifras. El modelo mide 1 m de largo y unos 22 mm de diámetro. El visor lo estira solo en el eje de la barra hasta la longitud del código. El material es zinc, el de las placas.

## 4. Mallas

STL ASCII en metros. El visor multiplica por 1000 si la caja mide menos de 10.

| STL | Origen | Cuerpo |
|-----|--------|--------|
| `10443020` | Cara de contacto, Y = 0. Agujero centrado, ±58,5 mm | 76 mm en −Y, que queda hacia fuera |
| `1920894` | Cara de contacto, Y = 0. Centrada, ±37,5 mm | 6 mm en −Y |
| `7238001` | Cara de contacto, Y = 0 | Tuerca, 76 mm en −Y |
| `dywidag` | Centro de la barra, puesto al cargar | 1 m en Y antes de estirar. X y Z unos ±11 mm |

## 5. Archivos

- `Desing\Repositories\RepositoryAtk60\ModulosATK60\Modulo270PanelElementGenerator.cs` — `AddTirantePlates`, `AddTiranteBar`, `DywidagCode`, `PanelFaceMm`.
- `Desing\Scripts\MasterArticles\master-article-details-stl-viewer.js` — centra `dywidag.stl` y estira los códigos `230xxx`.
- `Desing\Content\DesignTools\Stl\ATK60\10443020.stl`, `1920894.stl`, `7238001.stl`, `dywidag.stl`.

## 6. Qué no entra en este cierre

Grapa fija, rigidizador, gancho `1850164`, grapa regulable en horizontal y el panel de 0,30 × 2,70 m. Siguen en el handover del 8 de octubre. La regla de los 120 mm ya les pone el origen en la misma cara; no se ha vuelto a mirar en el visor pieza por pieza.
