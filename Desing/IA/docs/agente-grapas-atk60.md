# Juntas ATK-60 (NEVI, guía del usuario)

Para el siguiente agente. **24 de octubre de 2026.**

Fuente: Guía del Usuario NEVI, páginas 38 a 41 (`Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/NEVI_GUIA_USUARIO.pdf`). Apartados 3.1, 3.1.1, 3.1.2 y 3.2.1. Las alturas de la grapa fija coinciden con el Desing antiguo.

No inventar una altura que estas páginas no den. Si falta el puesto de una grapa de más, no se coloca.

## Tres piezas distintas

| Pieza | Código | Dónde va |
|-------|--------|----------|
| Grapa fija | `10004220` | En la junta, en el perfil que une dos paneles. Es la que se usa siempre que se pueda. |
| Grapa regulable | `10000221` | Solo en la compensación de 0 a 5 cm. Lleva la corredera `10000221-2`. |
| Tirante | placa `10443020` y barra Dywidag | En el agujero del panel, a 43,7 mm del canto. No es una grapa. |

Si la grapa fija se pone a la altura del tirante, choca con la placa.

| Panel | Grapa fija, desde el pie | Tirante, desde el pie |
|-------|--------------------------|------------------------|
| 2,70 m | 450, 1 350 y 2 250 mm (tres) | 550 y 2 150 mm |
| 2,40 m | 400 y 2 000 mm (dos) | 550 y 1 850 mm |
| 1,20 m | 200 y 800 mm (dos) | 350 y 850 mm |

El error que no hay que repetir: leer 550 mm en el panel y usarlo para la grapa. 550 mm (y 350 mm en el de 1,20) es el taladro del tirante. La serie 550 + 800 + 800 + 550 deja la grapa encima de la placa.

La grapa se inserta con `Atk60FaceInsertion.BothFaces` en el `along` de la junta. No se desplaza 43,7 mm hacia dentro del panel: ese desplazamiento es solo del tirante.

## Dos paneles juntos

Regla de obra, 25 oct 2026. Cada vez que dos paneles se tocan, esa junta lleva una grapa fija de unión, en las dos caras, a la altura del manual. No vale ponerla solo en el angular: el angular es otra pieza y otra junta.

Alturas, desde el pie de cada panel:

- 2,70 m: 450, 1 350 y 2 250 mm.
- 2,40 m: 400 y 2 000 mm.
- 1,20 m: 200 y 800 mm.

En el muro recto lo hace `PanelClamp.PlaceOnModule`: recorre las piezas del módulo y, si el final de una coincide con el inicio de otra, pone la grapa. No la pone en los 30 mm de los extremos del eje; esa junta la pone la esquina.

En la esquina en L (`LCornerFormwork`):

- Si el módulo de la cara parte en varios paneles (por ejemplo 0,90 + 0,30), cada corte entre ellos lleva la suya. `PlaceLayoutUnions`.
- El canto del angular lleva la suya. No sustituye a las de los paneles.
- El extremo del muro recto, donde acaba el último panel y empieza la cara de la esquina, también. `PlaceClamps`. Vale aunque los espesores no coincidan: el tablón del escalón no quita esta grapa.
- No poner una grapa a 300 mm del angular si ahí no se tocan dos paneles. Ese punto cae en el vano. La foto del 25 oct lo marca en rojo.

La T, el cruce y el pilar embebido se encofran igual: si se coloca un panel al lado de otro, esa junta lleva la grapa. No dejar el módulo pintado solo con `PanelFormwork.Place`.

## Junta vertical normal (3.1.1)

Grapa fija, en las dos caras:

- 2,70 m: tres.
- 2,40 m: dos.
- 1,20 m: dos.

Ya está en `PanelClamp.VerticalHeightsMm`, en `LCornerFormwork.JointHeights` y en `ExactWidthEndTape.ClampHeightsMm`.

En el panel tumbado el lado largo queda en horizontal. La grapa de esa junta usa las mismas estaciones, medidas sobre ese lado (`PanelClamp`, rama `Tumbado`). Un 2,70 tumbado lleva 450, 1 350 y 2 250 mm. No 200, 1 350 y 2 500.

## Compensación de 0 a 5 cm (3.2.1)

Taco de madera cortado en obra. El atado de la junta es el normal, con la grapa regulable. Sobre el tirante va la tuerca placa campana 15, que regula de 0 a 5 cm.

Grapas regulables por junta:

- 2,70 m: tres.
- 2,40 m: tres. Aquí no valen las dos de la grapa fija.
- 1,20 m: dos.

## Compensación de más de 5 cm hasta 18 cm

Grapa fija, más rigidizadores:

- 2,70 m: tres grapas y dos rigidizadores.
- 2,40 m: dos grapas y dos rigidizadores.
- 1,20 m: dos grapas y un rigidizador.

Eso es la compensación a lo largo de un muro recto. No es el escalón de una esquina en L con dos espesores.

## Esquina: espesor del otro muro más el panel de 0,30

El muro que llega a una esquina no se encofra con su largo entero. En ese extremo se descuenta el espesor del muro que conecta más el panel de esquina de 0,30 m.

Esa suma se cubre con el panel de catálogo siguiente, de 0,15 en 0,15: 0,30, 0,45, 0,60, 0,75, 0,90, 1,05, 1,20. El panel va en la cara exterior. Lo que sobra hasta ese módulo es el remate de madera de la cara interior, con grapa variable.

Ejemplo: 0,47 + 0,30 = 0,77. El exterior es un panel de 0,90. El interior es un remate de 0,13. Ese muro se redimensiona en ese extremo: el recorte pasa de 0,45 a 0,90, que es la cota del muro. La otra esquina no sube a 0,90, así que no lleva este remate y sigue con el recorte de 0,45. Las grapas fijas de unión de la esquina que ya estaba bien se dejan para después.

Cada extremo del muro es un caso. El otro extremo, si el vecino tiene otro espesor, usa la misma cuenta con ese vecino. No hace falta un JSON para el primero. Cuando los dos extremos escalonen a la vez, un volcado corto de los dos extremos (espesor, vecino, cara) sirve para comprobar que no se han cruzado. No un volcado de todas las piezas.

El angular y la esquina interior no se mueven. La grapa del angular va en su canto. Las de los paneles van en cada junta entre dos paneles, como en el apartado «Dos paneles juntos». El tablón del escalón no las sustituye.

El sobrante de la cara (módulo menos la necesidad) es el tablón. Va en la cara interior, entre la esquina interior y el panel, con grapa variable en los dos cantos. No va en el rincón del angular: ahí está el angular, pegado al panel exterior. En ese hueco interior no se pone grapa fija, porque no une dos paneles. El remate que el eje pintaba en el recorte se quita: ese extremo ya no es el hueco, y si se deja el tablón y las variables quedan metidos hacia el muro.

Ese tablón es el de la esquina, por el espesor. El tablón del eje es otro. Si el muro no tiene las dos conexiones y el largo es múltiplo de 0,15, el eje no lleva tablón: el desencofrado sale por el tape. Un 8,25 suelto son dos de 2,70, uno de 2,40 y un panel de 0,45. Con conexión en los dos extremos, el tablón de 0,15 del eje se queda. El reparto está en `Docs/Proyectos/Desing/ATK60-Logica-Modulos-Wall.md`.

## Esquina que no es de 90°, con escuadra G

Grapa fija, más rigidizadores:

- 2,70 m: cinco grapas y dos rigidizadores.
- 2,40 m: cinco grapas y dos rigidizadores.
- 1,20 m: tres grapas y un rigidizador.

La junta de al lado lleva una grapa fija más: cuatro en el de 2,70 y tres en el de 2,40 y en el de 1,20.

## Una grapa fija más en la junta de al lado

También lleva una grapa de más la junta contigua a:

- una esquina exterior de 90°;
- un tape de muro, lleve rigidizador o panel universal.

El manual no da la altura de esa grapa de más. No se inventa. Hoy el visor no la pinta.

## Junta horizontal (3.1.2)

Sale al apilar paneles. Puede cerrarse solo con grapas o con grapas y rigidizadores. Aunque valgan los dos modelos de grapa, se prioriza la fija.

El rigidizador de 0,75 m y el de 1,6 m se atán al panel con dos fijadores y la tuerca placa fija 15, en los agujeros de las costillas. En el modelo de 3 m el atado es de cuatro, no de dos.

El visor usa el corto `1850162` (0,75 m) en la junta y el largo `1850163` (1,20 m) cuando el mismo módulo junta franja de 0,30 y de 0,45. Cada barra lleva dos fijadores `1850164` y dos placas `10443020`. El atado de cuatro del modelo de 3 m no está pintado.

Los ejemplos del manual apilan así, con el rigidizador en la junta:

- 2,70 sobre 2,70 (5,40 m).
- 2,40 sobre 2,40 (4,80 m).
- 1,20 sobre 2,70 (3,90 m).
- 1,20 sobre 2,40 (3,60 m).
- 0,90 sobre 2,70 (3,60 m). En el curso de 0,90 el rigidizador va en vertical.
- 0,90 sobre 2,40 (3,30 m), con paneles de 0,60 de ancho.
- 1,20 sobre 1,20 (2,40 m).

## Dónde está en el código

| Archivo | Qué |
|---------|-----|
| `Repositories/Repository_Desing_2/Atk60/PanelClamp.cs` | Grapa fija de la junta vertical. `VerticalHeightsMm`. |
| `Repositories/Repository_Desing_2/Atk60/LCornerFormwork.cs` | Grapa fija de la esquina en L. `PlaceUnionAlong` en cada junta de paneles y en el angular. `PlaceLayoutUnions` si el módulo se parte. El escalón lleva tablón y grapa variable, y además la grapa fija de cada junta de paneles. |
| `Repositories/Repository_Desing_2/Atk60/ExactWidthEndTape.cs` | Grapa del tape. `ClampHeightsMm`. La regulable de este archivo es la del taco del tape, no la compensación de 0 a 5 cm. |
| `Repositories/Repository_Desing_2/Atk60/PanelStiffener.cs` | Rigidizador de la junta horizontal, con dos fijadores y dos placas. |
| `Repositories/Repository_Desing_2/Atk60/FormTie.cs` | Tirante. No cambiar estas alturas para alinear la grapa. |
