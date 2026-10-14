# Informe quincenal 02 — Tandem 2026

**Periodo:** 9 de octubre – 22 de octubre de 2026  
**Review / corte:** 22 de octubre de 2026  
**Siguiente informe:** el **23 de octubre de 2026** se crea el 03 automáticamente (sin que el usuario lo pida).  
**Norma:** `Desing/Report/LEEME.md` (apartados quincenales).  
**Anterior:** `2026-10-08_Informe-Quincenal.md` (01, cerrado).

Este fichero es el informe **abierto**. Mientras la fecha sea ≤ 22 oct 2026, los agentes **no preguntan** qué informe usar: abren o siguen un **apartado** aquí. Las horas van al trimestral (`2026-T4_Informe-Trimestral.md`), no aquí.

## Apartados

### 1. Proyecto XR

- **Estado:** abierto. Sesión en curso desde el **10 de octubre de 2026, 8:16**.
- **Sesiones 9 oct:** 16:40–17:34 (54 min) + 17:57–18:05 (8 min). **1 h 2 min** (en el trimestral T4).
- **Sesión 10 oct:** inicio **8:16**. Aún sin cierre.
- **Contexto:** se retoma el cliente XR conectado a Desing — dispositivos, Enviar a XR, app Unity. Base: `Docs/Proyectos/TandemXR/`.
- **Hardware de primera fase:** gafas **Meta Quest 3S 128GB**; tablet **Xiaomi Pad 6 128GB**.
- **Modelo de uso (cliente):** la app se instala **una vez**; luego el cliente diseña en Desing → **Enviar a XR** y ve el diseño en obra sin depender del equipo de desarrollo en cada envío.
- **Distribución (no es un solo Market):**
  - **Tablet (Pad 6):** Google Play Store.
  - **Gafas (Quest 3S):** Meta Horizon Store (no Play Store).
  - Misma marca Tandem XR; dos canales de instalación. En prototipo: APK a mano; en producto: tiendas.
- **Costes de tienda (orientativos, app gratuita para el cliente; revisar al publicar):**

  | Tienda | Coste fijo típico | Comisión |
  |--------|-------------------|----------|
  | **Google Play** | ~**25 USD una vez** (~23 €) cuenta desarrollador; sin cuota anual tipo Apple | ~15–30 % solo si se cobra la app o compras in-app |
  | **Meta Horizon Store** | Cuenta developer **gratis**; publicar gratis si la app es gratis | ~**30 %** solo de ventas de pago / IAP |

  Si Tandem XR es **gratis**, casi no hay comisión: Play ≈ 25 USD de alta; Meta ≈ 0 € de cuota. Costes reales aparte: desarrollo, capturas, privacidad, verificación de empresa y actualizaciones.
- **Qué se hizo:** relectura XR; docs quincenal 02; Play/Meta y costes; Unity fuera de Design.sln. Esqueleto proyecto en `TandemXR-Unity` (ProjectSettings, Packages, menú Setup escena). Pendiente activar licencia Personal en Hub para abrir el Editor.
- **Siguiente (al retomar):** Hub → Licencias → Personal; Importar `TandemXR-Unity`; **TandemXR → Setup escena principal**; Play con Desing en marcha.

### 2. Ménsulas de trabajo

- **Estado:** cerrado. **11 de octubre de 2026, 9:22–10:39.** **1 h 17 min** en el trimestral T4.
- **Qué quedó:** ménsulas `4120000042` en las dos caras, cada 2,40 m (a 600 y a 1 800), 300 mm bajo la coronación. Madera de protección una sola vez, en el primer muro y en la cara de inicio. El tubo vertical va en gris metal; el brazo sigue azul.
- **Pendiente de posición:** la ménsula no cae en el agujero que le toca. Queda en el fallo de posición de las piezas, no en este apartado.
- **Bug, 14 oct:** en el piso de la ménsula sobraba el tablón del extremo. Los tubos lo atravesaban. Se quita ese tablón; el piso queda en cuatro.
- **Código:** `WorkingBracket` en `Repository_Desing_2`.

### 3. Puntales

- **Estado:** abierto. Sesión en curso desde el **11 de octubre de 2026, 10:39**.
- **Sesión 11 oct:** inicio **10:39**. Aún sin cierre. Las horas van al trimestral T4.
- **Qué se hace:** puntales de aplome en las dos caras, a ras de suelo. En un módulo de 2 m o más, a 600 mm de cada extremo; entre 1,20 m y 2 m, a 300 mm. Solo si el muro llega a 2 m de alto. Origen: cara del panel, rotación y 120 mm.
- **Según la altura:** cuerpo y tubo interior hasta 4,95 m (`h060`…`h375` y su `_2`). De 2,26 m a 2,55 m se usa el de 2,40 m. Por encima de 1,20 m se añade la pieza de abajo (`h270Inf` o `h375Inf`). Desde 4,96 m el cuerpo es `Puntal_Tipo3`, `Tipo3-2` o `Tipo3-41`, sin tubo interior. El cuerpo va en gris y el tubo interior en gris claro.
- **Código:** `PushPullProp`.

### 4. Altura de las esquinas

- **Estado:** cerrado, solucionado. **12 de octubre de 2026, 9:41–9:52.** **11 min** en el trimestral T4.
- **Qué quedó:** con «Aplicar altura a todos los muros» marcado, las esquinas se extruyen a esa misma altura. Sin el check, la esquina no cambia.

### 5. Editar muro

- **Estado:** cerrado. **12 de octubre de 2026, 10:13–13:28.** **3 h 15 min** en el trimestral T4.
- **Qué quedó:** formulario «Modificar muro» con las pestañas Muro, Configurar Tapes y Configurar Puntales. En Muro: altura aparte, y otro check que copia ménsulas, rigidizadores, paneles y puntales a todos los muros y a las esquinas. Puntales con los cuatro checks. Sin paneles verticales, los módulos de 2,70, 2,40 y 1,20 van todos tumbados.
- **No se testeó bien.** Queda pendiente una pasada de verdad en el visor. La pestaña Tapes sigue sin hacer.
- **Código:** visor Desing_2 y catálogo de paneles ATK-60.

### 6. Salvar la configuración del muro

- **Estado:** cerrado. **12 de octubre de 2026, 13:28–15:05.** **1 h 37 min** en el trimestral T4.
- **Qué quedó:** al reabrir el diseño, cada muro recupera ménsulas, rigidizadores, puntales (superiores e inferiores, las dos caras), paneles de 0,75 y paneles verticales. También longitud, espesor, altura y coordenadas.
- **Bases:** `db_a197cd_desingproducction` y `db_a197cd_desingdevelop`. En develop se creó `TSql_DesignWall`.
- **Prueba:** pendiente de testeo. Si algo no vuelve igual, se abre un bug.

### 7. Poder crear oferta desde el panel principal de diseño

- **Estado:** abierto. Sesión en curso desde el **12 de octubre de 2026, 15:14**.
- **Sesión 12 oct:** inicio **15:14**. Aún sin cierre. Las horas van al trimestral T4 cuando se cierre.
- **Qué se hace:** poder crear una oferta desde el panel principal de diseño.
- **Hecho en el panel Diseños:** icono «+» en la cabecera para crear la oferta (obra, nombre, cliente, estado). Además, junto a cada número de oferta ya registrado, otro «+» crea el diseño dentro de esa oferta y solo pide el nombre. Diseños queda abajo, en tres de los cuatro huecos, con la columna Cliente, orden y buscador. El alta es el botón «Nuevo diseño» y el título indica la oferta. Arriba: obras, ofertas y clientes. Abrir un diseño usa el mismo ojo que el resto. Cada panel lleva paginación, selector de registros y exportación, como el resto de tablas, y la barra vertical en el listado. En Diseños, Ofertas muestra la fecha de creación. Título, pestañas, menú, buscador y alta van en una sola línea. Un botón amplía el panel a toda la pantalla y lo devuelve a su sitio. En Diseños la paginación queda fija bajo los registros, también con el panel ampliado, y el hueco entre la cabecera y la tabla es menor. En Ofertas hay una fila por oferta: número, descripción de la oferta, número de obra, descripción de la obra, número de diseños y un + para crear el diseño. En Clientes hay una fila por cliente con el número de diseños. En las dos, un más abre los meses y años; otro más, sobre cada mes, abre los diseños. En Ofertas cada diseño muestra nombre, cliente, fecha y el ojo. En Clientes muestra nombre, oferta, fecha y el ojo. Esas columnas llevan encabezado y el ojo queda al final. Las filas van más separadas. El fondo del desglose se queda blanco al pasar el ratón, en Ofertas y en Clientes. El + de crear el diseño se ve en azul. Su título dice crear diseño en el cliente o en la oferta, con el nombre entre comillas. El ojo dice «Abrir Diseño». El menos los pliega. Los desplegables llevan líneas de árbol: de la fila a la fecha y de la fecha a cada diseño, en Ofertas y en Clientes. El cliente y el número de oferta van en negrita y en azul, en las dos pestañas. El título Diseños queda solo; las pestañas van junto al selector de registros, como carpetas pegadas a la línea del listado, en la misma fila que el selector y el buscador. La activa se lee en blanco sobre azul. Hay una tercera pestaña, Gráficos. Al pasar el ratón, el ojo y el más se marcan en color para que se vean sobre la fila iluminada. En el inicio, Diseños ocupa todo el ancho de abajo. La paginación es texto pequeño, sin recuadro. El menú de registros va en blanco con línea gris, y desaparece la línea azul bajo los títulos. Todo encabezado, también el del desglose de diseños, va en gris. El orden de los paneles del inicio es el último movimiento de la tabla de diseños, el más reciente primero. Al ampliar Diseños, el menú izquierdo no se mete en el formulario: el panel empieza después del menú y el icono para volver queda visible bajo la barra superior y la paginación cabe dentro de la ventana. Obras, ofertas y clientes del inicio muestran los cinco últimos registros que dio de alta el usuario que ha entrado, con el ojo al detalle y el enlace al listado completo, sin buscador, paginación ni menú. Esas filas van más bajas, sin márgenes de sobra, para que los cinco quepan en el panel, y el ojo sigue viéndose. El estilo y el comportamiento de estos paneles y de sus tablas quedan escritos como regla del proyecto: encabezado gris, paginación en texto, menú de registros en blanco con línea gris, cinco últimos del usuario que entra, árbol de diseños en fila y detalles al tamaño del 80 % sin recortar el lado derecho.

### 8. Conexión SQL al publicar

- **Estado:** abierto. Sesión en curso desde el **12 de octubre de 2026, 18:09**.
- **Qué pasa:** en `ccvallecano-002-site5` el sitio publicado seguía con `.\SQLEXPRESS`. Ese equipo no tiene esa instancia (error 26).
- **Qué queda:** al publicar en Release, `Web.Release.config` cambia las dos cadenas a `SQL5113.site4now.net` y el catálogo de producción. En local sigue SQL Express. Hay que volver a publicar para que el servidor lo coja.
- **Editar empresa:** faltaban en el proyecto las vistas de delegaciones (`_CompanyBranchesDtCard` y las tres que van con ella). Sin eso, al publicar no se copian y Editar empresa falla. Hay que volver a publicar.
- **Otras páginas que tampoco se copiaban:** obra (`Details`), estados de oferta, detalle de idioma, acceso del plugin CAD y el layout de ZWCAD. Ya están en el proyecto. Las clases viejas de esquina ATK-60 que no están en el proyecto no se compilan y no rompen la publicación.
- **Google Maps al publicar:** `Web.GoogleMaps.config` no se sube porque Visual Studio excluye los `Web.*.config`. La clave pasa a `GoogleMaps.local.config`, que sí se publica y sigue fuera de Git. Hay que volver a publicar.

### 9. Adjuntar diseños al cliente

- **Estado:** abierto. Sesión en curso desde el **13 de octubre de 2026, 8:30**.
- **Qué se hace:** poder adjuntar diseños a un cliente. Los formularios van después.
- **Base:** tabla `TSql_DesgloseOfertaCliente`. El diseño sigue naciendo en su oferta. Cada línea guarda cliente, oferta y diseño; la misma terna no se repite. Sustituye a `TSql_ClientDesign`, que estaba vacía y se eliminó. Hecho en el SQL local de producción y en SQL5113 (tres claves foráneas). En develop local la tabla está, pero no hay clientes, ofertas ni diseños, así que las claves foráneas no se crearon.
- **Formularios:** el panel Diseños tiene dos pestañas. Ofertas es una fila por oferta: número, descripción de la oferta, número de obra, descripción de la obra, número de diseños y un + para crear el diseño. Un más abre el mes y el año; otro más abre los diseños, con nombre, cliente, fecha y el ojo. El + de la cabecera pasa a ser el botón «Nuevo diseño» y pide la oferta y el nombre. Clientes es una fila por cliente con el número de diseños. Un más abre el mes y el año; otro más abre los diseños de ese mes. El + azul de la fila pide el nombre y adjunta el diseño nuevo solo a ese cliente, sin pedir la oferta. El botón de arriba pide el cliente y el nombre. En obras y en el detalle de oferta las pestañas quedan como carpetas, igual que en el panel de diseños. El detalle de cliente pasa al mismo esquema: datos a la izquierda y, a la derecha, carpetas de documentos, obras, ofertas y diseños. Esos detalles, con el navegador al 100 %, quedan al tamaño que antes salía al 80 %. En la oferta, la carpeta Diseños es una tabla con el nombre, el usuario que lo creó, la fecha, el ojo para abrirlo y el borrado. En el listado de clientes el ojo abre el detalle, con documentos, obras, ofertas y diseños. En la pestaña Ofertas de la obra, el más de cada oferta abre el mismo árbol del panel: mes y año, y debajo cada diseño en su fila, con nombre, cliente, fecha y el ojo. Al abrir el año el fondo queda en blanco, como en el panel.

### 10. Inicio de sesión

- **Hecho:** si el correo no está registrado, o la contraseña no es correcta, el login lo dice en la pantalla. Ya no sale el error de servidor. «Olvidé mi contraseña» envía un correo con el enlace para cambiarla. Si el correo no existe, o el envío falla, también lo dice en la pantalla. Tras enviar el correo se vuelve al login y un aviso dice que el correo ha sido enviado; no se entra en la página principal. En el formulario de cambio el correo se muestra como texto. El ojo para ver la contraseña está en los formularios de contraseña. Al cambiarla se vuelve al login con un aviso, sin entrar en la aplicación.

### 11. Foto del diseño

- **Estado:** cerrado. **14 de octubre de 2026.** Sin horas en el trimestral.
- **Qué se hace:** desde el visor, bajar una foto del diseño tal como se ve, sin menús.
- **Hecho:** en la barra de arriba, junto a los iconos de entorno, hay una cámara. Guarda un PNG del lienzo completo, con el nombre del diseño. No entran la barra, el menú lateral ni las herramientas. El visor antiguo ya tenía esa cámara, pero esperaba un segundo, el fichero se llamaba «Hola.png» y a veces salía en blanco. Ahora la foto se toma en el mismo instante, con el cielo y la luz que haya en pantalla.
- **Al salvar y al imprimir:** la misma foto se escribe en el servidor, en `Content/images/desing`, dentro de una carpeta con el año y el mes (`2026-10`), y el fichero es el nombre del diseño. Si ya existe, se sustituye. En la tabla de diseños queda la ruta en `AttPhoto`. La columna de la miniatura STL no se toca. Al terminar, un aviso dice qué se hizo: muros y foto guardados, o imagen descargada y foto guardada. Si la foto no se puede guardar, el aviso sale en rojo. En el menú lateral, junto a Ayuda, el apartado Últimos diseños muestra la foto a 150×108, el número de oferta, el texto «Doble click para abrir» y el nombre. Un clic pasa al diseño anterior por fecha de creación. El doble clic abre el que se ve y sale el aviso «Abriendo diseño» con el reloj. Con el menú plegado la foto queda dentro del carril, sin el título, para no tapar el botón de plegar. Cómo seguir está en `Desing/IA/docs/agente-foto-diseno-y-menu.md`. En el servidor de publicación, la columna `AttPhoto` hay que crearla con el script del 14 de octubre si aún no está: sin ella la foto no se guarda y el apartado no sale.

### 12. Color del estado en las ofertas

- **Estado:** cerrado. **14 de octubre de 2026, 11:39–11:44.** Estimación de sesión: **5 min**.
- **Qué se hace:** el número de oferta lleva de fondo el color de su estado. En la ficha de la oferta ya estaba. Hay que verlo en los demás sitios donde sale la oferta, sobre todo en los paneles del inicio.
- **Hecho:** en el inicio, el número de las ofertas de arriba, el de la pestaña Ofertas y el del árbol de Clientes salen con el color del estado. También en los documentos de la obra, de la oferta y del cliente. El texto del número se lee en claro u oscuro según el fondo.

### 13. Pestaña Diseño en el panel de inicio

- **Estado:** cerrado. **14 de octubre de 2026, 11:52–12:30.** Estimación de sesión: **38 min**.
- **Qué se hace:** en el panel Diseños, una pestaña con todos los diseños, en dos vistas.
- **Hecho:** la pestaña se llama Diseños y va detrás de Ofertas. Al abrirla el panel se amplía. Datos es la tabla, con la foto a 100 de ancho en la primera columna, búsqueda, paginación y menú de registros. Imágenes son fichas con la foto, la oferta, el cliente, la obra, quién lo creó y el último cambio. Los enunciados van al 70 %, en gris claro y sin negrita. El nombre de quien lo creó va al 70 % e inclinado. No sale la fecha de creación. En la ficha se abre, se cambia el nombre y se borra. Esa vista lleva buscador, número de registros y paginación, sin menú de exportar.

## Heredado del informe 01 (sigue abierto)

1. Posición de las uniones, del panel de 0,30 × 2,70 m y de la ménsula (no cae en su agujero).
2. Gancho `1850164` dentro del taladro.
3. Grapa regulable en horizontal.
4. Inferior derecho del panel a mano.
5. Confirmar paneles a mano Desing_2 vs AutoCAD al reabrir.
6. US Azure «Encofrar manualmente».
7. FreeCAD: formularios MVC al activar Tandem 2026.

## Dónde está cada frente (esta quincena)

| Frente | Quién | En qué quedó |
|--------|-------|----------------|
| Proyecto XR | Cursor, 9–10 oct | Apartado 1 en curso desde el 10 oct, 8:16. El 9 oct, 1 h 2 min. Esqueleto Unity listo; falta licencia Hub |
| Ménsulas de trabajo | Cursor, 11 oct | Apartado 2 cerrado. 9:22–10:39, 1 h 17 min. El agujero de la ménsula queda en el fallo de posición. Bug 14 oct: tablón de más en el extremo, quitado |
| Puntales | Cursor, 11 oct | Apartado 3 en curso desde el 11 oct, 10:39 |
| Altura de las esquinas | Cursor, 12 oct | Apartado 4 cerrado, solucionado. 9:41–9:52, 11 min. Con el check, las esquinas suben con los muros |
| Editar muro | Cursor, 12 oct | Apartado 5 cerrado. 10:13–13:28, 3 h 15 min. No se testeó bien. Tapes sigue pendiente |
| Salvar la configuración del muro | Cursor, 12 oct | Apartado 6 cerrado. 13:28–15:05, 1 h 37 min. Pendiente de testeo; si falla, bug |
| Crear oferta desde el panel principal | Cursor, 12 oct | Apartado 7 en curso desde el 12 oct, 15:14. + de oferta en la cabecera; + de diseño junto al número, solo el nombre |
| Conexión SQL al publicar | Cursor, 12 oct | Apartado 8 en curso desde el 12 oct, 18:09. Release apunta a SQL5113; hay que republicar |
| Adjuntar diseños al cliente | Cursor, 13 oct | Apartado 9 en curso desde el 13 oct, 8:30. Pestañas Ofertas y Clientes en el panel Diseños. El alta de cliente escribe en `TSql_DesgloseOfertaCliente` |
| Foto del diseño | Cursor, 14 oct | Apartado 11 cerrado. Cámara, foto en el servidor, apartado Últimos diseños. Queda escrito en `Desing/IA/docs/agente-foto-diseno-y-menu.md` |
| Color del estado en las ofertas | Cursor, 14 oct | Apartado 12 cerrado. 11:39–11:44, estimación 5 min. El número lleva el color del estado en el inicio y en los documentos |
| Pestaña Diseño en el panel de inicio | Cursor, 14 oct | Apartado 13 cerrado. 11:52–12:30, estimación 38 min. Pestaña Diseños, tabla y fichas |

## Al cerrar el 22 de octubre

- Completar apartados abiertos o dejarlos explícitos como pendientes para el 03.
- Regenerar el PDF de este fichero.
- El **23 de octubre** el agente crea `yyyy-MM-dd_Informe-Quincenal.md` del 03 sin que se lo pidan, si ese día hay trabajo o al primer mensaje de ese día.
