# Informe quincenal 03 — Tandem 2026

**Periodo:** 22 de octubre – 4 de noviembre de 2026  
**Review / corte:** 4 de noviembre de 2026  
**Siguiente informe:** el **4 de noviembre de 2026** se cierra este y se crea el 04, ya semanal (`2026-11-11_Informe-Semanal.md`, review el 11 nov).  
**Norma:** `Desing/Report/LEEME.md` (apartados quincenales).  
**Anterior:** `2026-10-22_Informe-Quincenal.md` (02, cerrado el 22 oct).

Este fichero es el informe **abierto desde el 22 oct 2026**. El 02 ya no se escribe.

## Apartados

### 1. Bloques de AutoCAD: grapas

- **Estado:** cerrado. **22 de octubre de 2026, 12:32.**
- **Persona:** inicio **8:45**. A las **8:52** dejó los DWG de la grapa fija. Fin **12:32**.
- **Cursor:** inicio **8:45**. Entrega de la grapa fija en `ENCOFRAR` **9:11**. Bloquín y DLL **9:19**. Giro corregido **9:43**. La marca Atenko, **11:34**. La grapa en el catálogo del bloquín, **11:51**. Vista STL e insertar, **12:07**. Líneas de junta, **12:24**. Fin **12:24**.
- **Viene de:** informe 02, apartado 30. La lista está en `Desing/IA/docs/articulos-desing2-sin-bloque-autocad.pdf`.
- **Qué se hace:** lo que coloca Desing 2 pasa al plugin de AutoCAD. Se empieza por la grapa de unión y la grapa de ancho variable. El usuario hace los bloques. El origen del DWG se deja en el mismo `0,0,0` del STL, sin recentrar la malla. El punto de inserción es el del visor: ancla del muro, giro del muro y 120 mm hacia fuera (`PanelFaceMm`).

#### Grapa fija `10004220`

- **DWG:** `3D/10004220.dwg` y `3DRef/10004220R.dwg`, guardados a las 8:52. La geometría se desplazó para que el `0,0,0` del bloque sea el origen del STL. `INSBASE` sigue en cero.
- **Caja del STL (no se toca):** 195 × 104 × 30 mm. Origen dentro de la pieza: X de −127 a +68 mm, Y de −74 a +29 mm, Z de −17 a +13 mm.
- **Caja del DWG tras el desplazamiento:** Y y Z coinciden con el STL (104 × 30 mm) y el mínimo de X también (−127 mm). La malla `3DFACE` queda 17 mm más corta hacia +X: 178 mm frente a 195 mm del STL. El punto de inserción es el del STL; el largo de la malla no se ha alargado.
- **Plugin:** `TANDEM_ENCOFRAR` inserta `10004220` con ese punto y con el giro de la grapa del visor (junta vertical, junta horizontal y normal del muro). No usa el giro de 180° del panel. El XData va con papel `UNION`. Si el dibujo ya tenía el bloque cargado, hay que purgarlo para que entre esta geometría. No se ha compilado.
- **Bloquín:** al elegir `10004220` se iluminan las marcas `GRAPA_V` de los paneles (también los de `TANDEM_ENCOFRADO`). El clic deja el origen del STL en esa marca. Sin marca no inserta. El DLL que había era del 12 oct (`Debug127`). Compilado `bin\Debug128\AutocadPlugin.dll` a las 9:19.
- **Giro:** en el muro de prueba la grapa quedaba de canto, con el gancho hacia arriba. El visor deja el canto de 30 mm en vertical y la boca hacia fuera. Corregido en `Debug129` a las 9:43. A las **9:59** el usuario lo deja aceptable. El ajuste fino de posición se hará a la vez en el visor y en el plugin.
- **Marca Atenko:** a las **11:31** la cara de detrás salía boca abajo, simétrica de la de delante, y la marca no se leía. En `Debug135` (11:34) las dos caras usan el mismo giro de pieza. La boca sigue hacia fuera porque el yaw da media vuelta con la normal de cada cara. La marca queda derecha.

![Grapas simétricas: unas con Atenko derecho y otras boca abajo.](img/2026-10-22-grapa-atenko.png)

- **En el bloquín:** a las **11:47** la grapa no salía en la lista. El catálogo solo trae artículos del maestro que tienen DWG enlazado, y `10004220` no tiene fila. En `Debug136` (11:51), si el DWG está en disco, el bloquín añade la fila «10004220 Grapa fija» aunque el maestro no la tenga. A las **11:57** la fila ya estaba, pero sin STL e Insertar no dejaba la grapa. En `Debug137` (12:07) la fila lleva el STL y los DWG. A las **12:18** Insertar volcaba el JSON en la línea de comandos (`id` nulo y rotación numérica) y no arrancaba. En `Debug138` (12:24) el comando sí arranca: entre los paneles se iluminan las líneas de junta. La grapa se mueve sobre la línea y el clic la deja ahí. Si la línea es horizontal, la grapa gira un cuarto de vuelta.
- **Grapa variable:** cuerpo `10000221` y corredera `10000221-2` (código `10000221B`). En el visor llevan un giro de 180° en Y y la escala Y en negativo. No entra en este cierre: aún no hay DWG.

### 2. Formulario del muro en el plugin

- **Estado:** abierto, a prueba. **22 de octubre de 2026, 9:59.**
- **Persona:** inicio **9:59**.
- **Cursor:** inicio **9:59**. Entrega **10:05**. El sólido al aceptar, **10:41**. El mismo formulario que Desing, **10:55**. La hilada tumbada, **11:13**. El doble clic del visor, **12:56**.
- **Qué faltaba:** el doble clic en el muro no abría nada. No había enganche. El primer formulario era una copia en el plugin: un cambio en Desing no llegaba a AutoCAD.
- **Qué hace:** doble clic en el eje, la cara, el sólido 3D o un panel del muro abre **Modificar muro**, el formulario de Desing (`Scripts/MasterArticles/wall-edit-form.js`). El visor y el plugin cargan ese archivo. Cambiarlo cambia los dos. Longitud y espesor mueven el eje y las caras. Altura, ménsulas, rigidizadores, paneles, puntales y tapes se guardan en el muro y entran en `ENCOFRAR`.
- **Sólido:** en `Debug130` Aceptar no tocaba el sólido ya dibujado. En `Debug131` (10:41) Aceptar guarda los atributos y vuelve a generar el sólido con la altura y la planta nuevas. Los paneles salen al encofrar otra vez.
- **DLL:** `bin\Debug133\AutocadPlugin.dll` a las 10:54. Hay que recompilar Desing para que exista la página del formulario y hacer NETLOAD de esa carpeta.
- **Hilada tumbada:** a las **11:07** un muro de 3 m deja la hilada de 0,30 elevada otros 0,30. El servidor coloca el origen en la cara de arriba porque en el visor el panel cuelga de ese punto. El DWG, al tumbarse, crece hacia arriba, y el plugin apoyaba ese origen. Pasa en toda hilada tumbada, no solo en 0,30: 0,45, 0,60, 0,75 y 0,90 suben su propia altura, también si van apiladas o encima de un tramo vertical. Los muros solo verticales (1,20, 2,40 y 2,70) no. En `Debug134` (11:13) el origen de cada tumbado baja su propia altura.
- **Doble clic en el visor:** a las **12:54** el mismo formulario, al usarse en el plugin, dejó el doble clic de Desing roto. El visor es un módulo y asignaba `maStlWallFormApi` sin declararla, así que el popup no se creaba. Tampoco estaba el pintado de tapes. Corregido a las **12:56**. Hay que recargar el visor.

![Alzado a 3 m. La hilada de 0,30 queda separada de los paneles de 2,70.](img/2026-10-22-paneles-030-elevados.png)

### 3. Bloques de AutoCAD: rigidizadores

- **Estado:** abierto. **22 de octubre de 2026, 12:40.**
- **Persona:** inicio **12:40**.
- **Cursor:** inicio **12:40**.
- **Qué se hace:** los dos rigidizadores que ya pinta Desing 2 pasan al plugin, igual que la grapa fija. Corto `1850162` y largo `1850163`. El fijador `1850164` no se dibuja en los cierres TapeS1 y TapeS2.
- **Cursor:** entrega **14:00**. `Debug139`. `ENCOFRAR` inserta el corto y el largo con el mismo giro que la grapa. El corto en junta horizontal suma 90° en planta, como en el visor. Los DWG guardados `18550162R` y `1850159R` se leen como `1850162` y `1850163`. Las casillas de rigidizador interior y exterior del formulario siguen apagándolos. El bloquín queda para después.
- **Manual, 14:06.** Antes del bloquín van el fijador `1850164` y la tuerca placa fija `10443020`. El visor ya los pone: dos uniones por barra, por encima y por debajo, en las dos caras. El modelo de 3 m del manual (cuatro uniones) no se emite. En `3D` y `3DRef` aún no hay DWG de estas dos piezas. Los de febrero, en la raíz de `AtkSystem60`, no se usan. En TapeS1 y TapeS2 el fijador sigue sin pintarse.
- **Cursor:** entrega **14:26**. `Debug140`. `ENCOFRAR` confirma paneles y grapas antes de abrir el DWG del rigidizador. Si ese bloque falla, el muro sigue encofrado y la línea dice cuál pieza no abrió.
- **Cursor:** entrega **14:45**. `Debug141`. El STL lleva el largo en Z y el cuerpo en −Y. El DWG lo tenía en +Y y el grueso en +Z. Antes del giro del visor se pasa el bloque a esos ejes. El centro del largo queda en el origen, como el STL.
- **Parón, 15:06.** Se deja el plugin aquí. Falta el fijador, la tuerca y el bloquín.

### 4. Segundo trabajador

- **Estado:** abierto. **22 de octubre de 2026, 15:06.**
- **Persona:** inicio **15:06**.
- **Cursor:** inicio **15:06**.
- **Qué se hace:** insertar un segundo trabajador. El que ya está es el OBJ `Content/DesignTools/Stl/Worker/Worker.obj`, en el visor (`desing2-worker-tool.js`). Mide 1,75 m. Solo se ve en el encofrado.
- **Dónde queda el de ahora:** si el clic cae en un tablón de plataforma (`Tablon24x15x5`), los pies van en la cara de arriba de ese tablón y mira al muro. El ancho se estrecha para caber, con 40 mm de margen; la altura no se toca. Si el clic no da en la plataforma, los pies van en el suelo (Y = 0), en el punto del ratón, sin girar hacia el muro. Ese del suelo pertenece al muro más cercano, hasta 40 m. El de la plataforma pertenece al muro de ese tablón.
- **Cursor:** entrega **15:28**. Insertar trabajador abre un popup para elegir el de siempre o `Worker2.obj`. El dibujo del popup es provisional. `Worker2` venía tumbado en Z; se pone de pie y se pinta como el primero: casco amarillo, chaleco con rayas, camiseta gris, pantalón azul y botas negras. Para que al guardar se recuerde el modelo, hace falta `Scripts/TemporalScript/2026-10-22_alter_TSql_DesignWorker_TextCode.sql`.
- **Cursor:** entrega **15:34**. `Worker2.obj` pasa de 181 MB a 4 MB. La pintura es la misma función que el primero.
- **Cursor:** entrega **15:50**. El selector pasa al mismo popup que el resto. Elegir uno carga el trabajador y vuelve a seguir al ratón. Las dos figuras miden igual y no llevan nombre.
- **Cursor:** entrega **16:17**. Trabajador 2 gira 180° para echar el hormigón al muro. La cara pasa a piel, el pantalón queda azul y el chaleco en el torso.
- **Cursor:** entrega **16:52**. Las dos fotos sustituyen los dibujos del formulario. Quedan en `Content/DesignTools/Stl/Worker/`, las dos a 480×640 y sin nombre.
- **Cursor:** entrega **17:10**. `Worker2.obj` entra en el proyecto, junto a las dos fotos. Sin eso, al publicar no se sube el segundo trabajador.
- **Cursor:** entrega **17:19**. Los scripts del trabajador quedan en develop y en producción, en local y en el servidor. La columna `TextCode` ya existe. En el servidor no hay base develop.

### 5. Direcciones Google en la intranet

- **Estado:** cerrado. **22 de octubre de 2026, 21:35.**
- **Persona:** primer tramo **20:31–21:09**. Segundo tramo **21:17–21:35**.
- **Cursor:** primer tramo **20:31–21:08**. Segundo tramo **21:17–21:19**.
- **Qué se hace:** al escribir una dirección, Google solo proponía resultados de España. Tiene que proponer también Uruguay, Argentina y el resto.
- **Cursor:** entrega **20:33**. Se quita el filtro de país del autocompletado. España sigue como sesgo del mapa.
- **Cursor:** entrega **20:50**. Sin lista de países el buscador deja de proponer, también en España. Vuelve la lista: España, Uruguay, Argentina y otros doce. Google admite 15 como máximo.
- **Cursor:** entrega **20:56**. Edge bloquea el cuadro de Google. Las propuestas salen en una lista propia: España, Uruguay, Argentina y los otros doce países.
- **Cursor:** entrega **21:05**. La lista se pinta debajo del campo, sin depender del cuadro de Google. La búsqueda ya no lleva filtro de país.
- **Cursor:** entrega **21:08**. En Crear obra la dirección se escribía en Dirección formateada, y ese campo no buscaba. Ahora también propone.
- **Persona:** a las **21:09** confirma que propone. El mismo script cubre obra, cliente, empresa y delegación. Esos ficheros ya están en el proyecto. Se publica.
- **Persona:** a las **21:17** la dirección se rellena y el mapa no. Si guarda y vuelve a entrar, el mapa sí sale.
- **Cursor:** entrega **21:19**. El mapa se pide al elegir la dirección y se reajusta cuando el recuadro ya tiene tamaño. Antes solo se pintaba al recargar.
- **Persona:** a las **21:35** confirma que el mapa sale al elegir. El mismo bloque es el de obra (local y facturación), cliente, empresa y delegación. En empresa, Dirección y Dirección 2 siguen siendo texto libre.

### 6. Botón Editar en las fichas

- **Estado:** abierto. **22 de octubre de 2026, 21:40.**
- **Persona:** inicio **21:40**.
- **Cursor:** inicio **21:40**.
- **Qué se hace:** en las fichas de detalle hay que poder pasar al formulario para cambiar los datos.
- **Cursor:** entrega **21:42**. Editar queda en la ficha de la obra, de la oferta y del cliente, junto a los datos. Empresa, delegación y los catálogos ya lo tenían arriba.

### 7. Documentación para el siguiente agente

- **Estado:** abierto. **23 de octubre de 2026, 8:58.**
- **Persona:** inicio **8:58**.
- **Cursor:** inicio **8:58**.
- **Qué se hace:** dejar por escrito la sesión del 22 y pasar el informe de quincenal a semanal.
- **Cursor:** entrega **9:01**. Guía en `Desing/IA/docs/HANDOVER-SESION-2026-10-23.md`. El 03 sigue hasta el 4 nov. El 04, ese día, ya es semanal: del 4 al 11 nov.
- **Persona:** a las **9:18** pide el informe `Informe Semanal 26102026-2112026` (26 oct – 2 nov), con lo del 22 y el fallo del cliente al editar la obra. El relato de esa semana va en ese fichero.

## Dónde está cada frente (esta quincena)

| Frente | Quién | En qué quedó |
|--------|-------|----------------|
| Bloques de AutoCAD: grapas | Cursor, 22 oct | Apartado 1 cerrado a las 12:32. `Debug138` a las 12:24: la grapa se mueve sobre la línea de junta. La variable queda fuera, sin DWG |
| Formulario del muro | Cursor, 22 oct | Apartado 2 a prueba. A las 12:56 el doble clic del visor vuelve a abrir Modificar muro |
| Bloques de AutoCAD: rigidizadores | Cursor, 22 oct | Apartado 3 en parón a las 15:06. `Debug141`. Falta fijador, tuerca y bloquín |
| Segundo trabajador | Cursor, 22 oct | Apartado 4 abierto. A las 17:19 `TextCode` está en develop y en producción |
| Direcciones Google | Cursor, 22 oct | Apartado 5 cerrado a las 21:35. El bloque compartido cubre obra, cliente, empresa y delegación |
| Botón Editar | Cursor, 22 oct | Apartado 6 abierto. A las 21:42 Editar está en la ficha de obra, oferta y cliente |
| Documentación | Cursor, 23 oct | Apartado 7 abierto. Guía de la sesión y paso a informe semanal el 4 nov |

## Horas cerradas (no se suman entre sí)

| Reloj | Tramo | Suma |
|-------|--------|------|
| Persona | Apartado 1, 8:45–12:32 | 3 h 47 min |
| Persona | Apartado 5, 20:31–21:09 | 38 min |
| Persona | Apartado 5, 21:17–21:35 | 18 min |
| Cursor | Apartado 1, 8:45–12:24 | 3 h 39 min |
| Cursor | Apartado 5, 20:31–21:08 | 37 min |
| Cursor | Apartado 5, 21:17–21:19 | 2 min |
| Copilot | — | 0 |
| **Persona** | Apartados 1 y 5 | **4 h 43 min** |
| **Cursor** | Apartados 1 y 5 | **4 h 18 min** |
| **Copilot** | — | **0** |

Los apartados 2, 3, 4, 6 y 7 siguen abiertos. Su tramo no entra en estas sumas.

## Al cerrar el 4 de noviembre

- Completar apartados abiertos o dejarlos explícitos como pendientes para el 04.
- Regenerar el PDF de este fichero.
- El **4 de noviembre** el agente crea el 04, lo registra en `Design.csproj` y deja de escribir en este fichero.
