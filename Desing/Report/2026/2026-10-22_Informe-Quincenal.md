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

- **Estado:** cerrado. **16 de octubre de 2026, 11:19.** El reparto nuevo está en el apartado 17. No se inventa un tramo desde el 11 oct ni se vuelca al trimestral.
- **Sesión 11 oct:** inicio **10:39**. Quedó abierto hasta cerrar la distancia entre puntales.
- **Qué se hace:** puntales de aplome en las dos caras, a ras de suelo. El primer criterio fue 600 mm de cada extremo en un módulo de 2 m o más, y 300 mm entre 1,20 m y 2 m. Solo si el muro llega a 2 m de alto. Origen: cara del panel, rotación y 120 mm. Ese reparto por módulo queda sustituido por el del apartado 17.
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

- **Hecho:** si el correo no está registrado, o la contraseña no es correcta, el login lo dice en la pantalla. Ya no sale el error de servidor. «Olvidé mi contraseña» envía un correo con el enlace para cambiarla. Si el correo no existe, o el envío falla, también lo dice en la pantalla. Tras enviar el correo se vuelve al login y un aviso dice que el correo ha sido enviado; no se entra en la página principal. En el formulario de cambio el correo se muestra como texto. El ojo para ver la contraseña está en los formularios de contraseña. Al cambiarla se vuelve al login con un aviso, sin entrar en la aplicación. El logo del login sale de la cookie de plantilla. Si esa cookie es la plantilla por defecto, o no hay cookie, el logo es solo la mano (`Logo.ico`); el nombre va al lado, no dentro de la imagen. El logo de la otra marca solo sale cuando la cookie es otra plantilla.
- **15 oct, 18:09–18:14 (5 min) y 16 oct, 7:32–7:48 (16 min):** en el sitio publicado el login fallaba por una llave de más en la página. El logo de la plantilla por defecto queda solo la mano, sin el nombre dentro de la imagen. En el menú, Últimos diseños vuelve a listarse.
- **16 oct:** la plantilla por defecto usa los logos con fondo transparente. El logo con el nombre es `t-desingc.png`. La mano y el favicon son `tDesing.png`. Otra plantilla sigue con su propio logo.
- **16 oct, 9:54–10:53 (59 min):** logos transparentes en la plantilla por defecto y el del login más grande. Publicado. No va al trimestral.

### 11. Foto del diseño

- **Estado:** cerrado. **14 de octubre de 2026.** Sin horas en el trimestral.
- **Qué se hace:** desde el visor, bajar una foto del diseño tal como se ve, sin menús.
- **Hecho:** en la barra de arriba, junto a los iconos de entorno, hay una cámara. Guarda un PNG del lienzo completo, con el nombre del diseño. No entran la barra, el menú lateral ni las herramientas. El visor antiguo ya tenía esa cámara, pero esperaba un segundo, el fichero se llamaba «Hola.png» y a veces salía en blanco. Ahora la foto se toma en el mismo instante, con el cielo y la luz que haya en pantalla.
- **Al salvar y al imprimir:** la misma foto se escribe en el servidor, en `Content/images/desing`, dentro de una carpeta con el año y el mes (`2026-10`), y el fichero es el nombre del diseño. Si ya existe, se sustituye. En la tabla de diseños queda la ruta en `AttPhoto`. La columna de la miniatura STL no se toca. Al terminar, un aviso dice qué se hizo: muros y foto guardados, o imagen descargada y foto guardada. Si la foto no se puede guardar, el aviso sale en rojo. En el menú lateral, junto a Ayuda, el apartado Últimos diseños muestra la foto a 150×108, el número de oferta, el texto «Doble click para abrir» y el nombre. Un clic pasa al diseño anterior por fecha de creación. El doble clic abre el que se ve y sale el aviso «Abriendo diseño» con el reloj. Con el menú plegado la foto queda dentro del carril, sin el título, para no tapar el botón de plegar. Cómo seguir está en `Desing/IA/docs/agente-foto-diseno-y-menu.md`. En el servidor de publicación faltaba la columna `AttPhoto`, y por eso el apartado no salía. Ya está creada. El apartado lista los diseños de quien ha entrado. Aún no hay foto guardada de esos diseños: se ve el nombre, y la imagen cuando se guarda el diseño desde el visor.

### 12. Color del estado en las ofertas

- **Estado:** cerrado. **14 de octubre de 2026, 11:39–11:44.** Estimación de sesión: **5 min**.
- **Qué se hace:** el número de oferta lleva de fondo el color de su estado. En la ficha de la oferta ya estaba. Hay que verlo en los demás sitios donde sale la oferta, sobre todo en los paneles del inicio.
- **Hecho:** en el inicio, el número de las ofertas de arriba, el de la pestaña Ofertas y el del árbol de Clientes salen con el color del estado. También en los documentos de la obra, de la oferta y del cliente. El texto del número se lee en claro u oscuro según el fondo.

### 13. Pestaña Diseño en el panel de inicio

- **Estado:** cerrado. **14 de octubre de 2026, 11:52–12:30.** Estimación de sesión: **38 min**.
- **Qué se hace:** en el panel Diseños, una pestaña con todos los diseños, en dos vistas.
- **Hecho:** la pestaña se llama Diseños y va detrás de Ofertas. Al abrirla el panel se amplía. Datos es la tabla, con la foto a 100 de ancho en la primera columna, búsqueda, paginación y menú de registros. Imágenes son fichas con la foto, la oferta, el cliente, la obra, quién lo creó y el último cambio. Los enunciados van al 70 %, en gris claro y sin negrita. El nombre de quien lo creó va al 70 % e inclinado. No sale la fecha de creación. En la ficha se abre, se cambia el nombre y se borra. Esa vista lleva buscador, número de registros y paginación, sin menú de exportar.

### 14. Recuento de piezas

- **Estado:** en curso. Desde el **14 de octubre de 2026, 12:37.** No cerrado.
- **Sesiones (estimación de reloj, con el testeo; no van al trimestral mientras siga abierto):**
  - **15 oct:** 8:58–13:29 (**4 h 31 min**) y, tras la pausa, 15:48–16:28 (**40 min**). **5 h 11 min.**
  - **15 oct, al dejar el día:** 17:59–18:09 (**10 min**). Publicación con fecha, hora de Madrid y versión v2.00.
  - **16 oct:** 7:48–8:23 (**35 min**) encofrado en el sitio y el panel que faltaba; 8:40–8:57 (**17 min**) muro especial sin artículos; 8:57–9:05 (**8 min**) separación de las dos barras.
  - **16 oct, 11:19:** se retoma el recuento, al cerrar la distancia entre puntales. Sigue abierto.
- **Qué se hace:** el recuento de piezas del diseño, leyendo el maestro de artículos. En el visor antiguo era muy lento. Primero se estudia cómo estaba hecho, sin tocarlo.
- **Muro especial sin paneles:** si el muro está marcado como especial y ya no tiene ningún artículo, deja de ser especial. Se mira al generar el muro 3D y también al encofrar. Un muro especial vacío no se queda fuera del encofrado. El muro 1 del diseño 1 (oferta 1) estaba especial por un panel de CAD que no está en el muro, y Encofrar se saltaba el tramo entero. Esa marca se quitó en producción.
- **En marcha:** el icono de la lista solo se ve cuando el diseño ya está encofrado y abre el formulario con lo que hay en pantalla. El icono de guardar del diseño escribe los muros y la foto, sin el recuento. El recuento se guarda con el icono de la cabecera: escribe el recuento nuevo y el diseño con la foto, y el aviso desaparece al terminar. No reescribe las piezas viejas en ese momento. El listado solo cuenta el último lote. A las 02:00, una vez al día, la lanza el primer usuario que entra: ve «Un momento. Mantenimiento» y los segundos previstos, y se quitan las piezas que ya no son ese lote. El primer botón busca ese tipo en la escena: los paneles quedan al 70 % y el resto al 50 %, y el botón se pone azul; al pulsarlo otra vez vuelve al 100 %. El segundo botón oculta o muestra ese mismo tipo. El tercero deja ese artículo al 100 % y atenúa el resto, después de reiniciar. El cuarto hace lo mismo con la visibilidad: oculta el resto y, si se pulsa otra vez, lo vuelve a mostrar. Junto a exportar y guardar hay un botón que devuelve todas las piezas a visibles y al 100 %. La ménsula y el quitamiedos van en azul, igual que el brazo. Se puede hacer en tantas filas como se quiera. La pestaña Artículos queda alineada con la palabra Código. Los iconos de exportar y de cada fila se ven y se iluminan al pasar. La cabecera va en azul. El hueco de los totales queda a la mitad. Cambiar de modo no vuelve a generar el muro si el diseño no ha cambiado, ni guarda las piezas. El quinto botón abre el STL de ese artículo, con el color del maestro, en el mismo visor Three de Artículos maestros y del plugin, sin tocar el encofrado. El botón de guardar del menú superior no escribe el recuento: solo los muros, las piezas especiales y la foto. La foto se reduce para no quedarse minutos comprimiéndola. En el recuento se ve el quinto botón, hay un poco de blanco entre la lista y el marco, margen en los iconos de la cabecera y las pestañas van 25 px a la derecha. Ese botón abre a la derecha el mismo visor de artículo del plugin, con zoom, encuadre, fondo, recorte, ejes, cubo y cámaras, y el color del maestro. Si el maestro no tiene color, la ménsula sale en el azul del encofrado, no en negro. El visor del artículo queda del tamaño del plugin, y el panel usa el amarillo y el negro del plugin, con la misma luz. Se puede mover y redimensionar sin perder la proporción, y la posición y el tamaño quedan en una cookie. Junto al cierre hay un botón que lo pone a toda la pantalla y, al pulsarlo otra vez, lo devuelve a esa posición. Los quitamiedos van en madera, como el suelo. El azul de la ménsula es un poco más oscuro. La escena deja la curva fílmica para que los colores se vean tan vivos como en el visor del artículo. Generar el muro 3D quita el encofrado. Tras encofrar, el botón Encofrar queda activo y el de generar muro 3D no. El guardado del diseño termina en cuanto el servidor responde: la foto va después y no deja el aviso de «Guardando diseño». Los rigidizadores verticales se pintan otra vez. Publicado v2.00: la publicación muestra la fecha, la hora de Madrid y la versión. En el sitio publicado, Encofrar calculaba el muro y luego fallaba al escribir un fichero de depuración, así que el muro se quedaba en 3D. Ese fichero ya no corta el encofrado. En un muro de 11,05 m con paneles verticales apagados, el tramo final es de 2,40 m y su panel no estaba publicado: en Develop se veía y en el sitio no. Ya va con el resto de paneles que faltaban. Un muro sin paneles puestos a mano no se queda fuera del encofrado: el muro 1 del diseño 1 estaba marcado como especial por un panel de CAD que no está en el muro, y Encofrar se saltaba el tramo entero. Al generar el muro 3D, si un muro especial ya no tiene ningún artículo, deja de ser especial. Las dos barras de arriba quedan separadas por el ancho de dos iconos; si la pantalla no da, la de herramientas baja a la línea siguiente.

### 15. Tirantes en paneles tumbados

- **Estado:** en curso. Desde el **15 de octubre de 2026, 16:28.** No cerrado.
- **Sesión 15 oct:** 16:28–17:59. Estimación **1 h 31 min**. A las 17:59 se pasó a publicar. No va al trimestral mientras siga abierto.
- **Qué se hace:** con paneles tumbados, en todos los casos hay que insertar tirantes en la parte superior.
- **En marcha:** el tirante del panel tumbado queda en la parte superior de la franja, en las esquinas altas del marco, a los dos lados. No a media altura ni abajo.

### 16. Muros colineales en un solo tramo

- **Estado:** en curso. Desde el **16 de octubre de 2026, 8:23.** No cerrado.
- **Sesión 16 oct:** 8:23–8:40. Estimación **17 min**. A las 8:40 se pasó al muro que no encofraba. No va al trimestral mientras siga abierto.
- **Qué se hace:** al terminar de dibujar un tramo, si sigue en la misma recta y toca al anterior, queda un solo muro. Si el diseño ya los trae partidos, al generar el muro 3D o al encofrar se unen y se guardan los muros, sin la foto.
- **En marcha:** en el diseño de producción (oferta 2, diseño 2) los muros 17 y 20 están a continuación, en la misma recta. El 20 repite el tramo del 1. La esquina con el muro 4 no se une.

### 17. Distancia entre puntales

- **Estado:** cerrado. **16 de octubre de 2026, 11:19.** Se pasa al recuento. No va al trimestral.
- **Sesión 16 oct:** 9:16–9:54 (**38 min**) y, tras los logos, 10:53–11:19 (**26 min**). **1 h 4 min.**
- **Qué se hace:** en Configuración, el primer campo es la distancia entre puntales. Por defecto 2,40 m. Se puede cambiar en cada diseño.
- **Dónde se lee:** columna `NumberPropSpacingMm` en `dbo.TSql_Design_V2`. Si está vacía, vale 2,40 m. Quedó en la producción local y en la de SQL5113. En develop no existe `TSql_Design_V2`.
- **Al encofrar:** 2,40 m es el máximo. Los puntales se reparten a lo largo del muro sin rebasarlo, en el alojamiento del panel (guía NEVI: cabezal a 50 mm de la cara exterior de la costilla). Si el muro no da para 2,40, el paso baja (2,30, 2,10, …) hasta caer en un alojamiento.
- **Al cambiar el valor:** si el muro ya está encofrado, se borra el encofrado y se vuelve a encofrar con la distancia nueva.
- **Reparto:** el paso cabe en el muro sin pasar del máximo. La mitad de ese paso queda del inicio al primer puntal, y la misma mitad del último al final. Cada puntal cae en el alojamiento del panel más cercano.

### 18. Insertar trabajador

- **Estado:** cerrado de momento. **18 de octubre de 2026, 16:18.** Se pasa a la posición. No va al trimestral.
- **Sesión 18 oct:** 12:22–16:18. Estimación **3 h 56 min**.
- **Qué se hace:** insertar el trabajador del OBJ en el diseño, con los colores del diseño antiguo, y guardarlo en la base de datos.
- **Qué quedó:** el botón lo deja seguir al ratón; el doble clic lo deja y sale el reloj hasta que queda guardado; se pueden poner varios; el doble clic sobre él lo quita. Solo se ve en el modo de encofrado. En el suelo queda a ras y mide 1,75 m de alto. Sobre los tablones solo se estrecha el ancho para caber en la plataforma; la altura no se toca, así queda por encima del quitamiedos de 1,50 m. Mira al muro. Los colores siguen la foto. Al guardar se escribe también `NumberScale` (solo el ancho).
- **Al borrar el muro (19 oct):** el trabajador de ese muro se borra con él. Sobre los tablones es el de esa plataforma. En el suelo es el del muro más cercano, hasta 40 m. Un trabajador de otro muro se queda. Deshacer devuelve el muro y el trabajador. Entra en la base al pulsar Guardar.

### 19. Posición de las plataformas y de los puntales

- **Estado:** cerrado de momento. **18 de octubre de 2026, 17:41.** Queda para testear. Se pasa a los pilares. No va al trimestral.
- **Sesión 18 oct:** 16:18–17:41. Estimación **1 h 23 min**.
- **Qué se hace:** dejar la plataforma de trabajo y el puntal en el agujero que les toca, con el panel en vertical y con el panel tumbado. Se parte de la guía de usuario NEVI.
- **Plataforma (NEVI 2.12 y 5.4).** Hay dos montajes, según la costilla:
  - **Costillas horizontales** (panel en vertical): el vástago entra en los agujeros de las costillas tubulares más próximos al perfil exterior. El tope se apoya en las costillas de abajo.
  - **Costillas verticales** (panel tumbado: esas costillas quedan en vertical): siempre de lado, en los agujeros más próximos al perfil exterior. El amarre es con pasador y tuerca hexagonal 15, o con el gancho rigidizador y la tuerca 15 por el agujero lateral.
  - Separación máxima entre ménsulas: **2 m**.
- **Puntal (NEVI 2.9, cabezal puntal).** Va en los agujeros de las costillas más alejados del perfil exterior, a **50 mm** de la cara exterior de la costilla. El cabezal tensor y el de la barandilla van en los más próximos, a **40 mm**. El montaje es el mismo en los dos: pasador por la costilla y por el tubo del cabezal, y tuerca hexagonal 15. Sirve con el cabezal en vertical y en horizontal.
- **En el visor ahora:** la ménsula se pone a 300 mm bajo la coronación, a 600 y a 1 800 del tramo, sin caer en el agujero de la costilla. El puntal se reparte a lo largo del muro y se acerca a 50 mm del canto del panel, sin distinguir si la costilla está horizontal o vertical.

### 20. Pilares

- **Estado:** en curso. Desde el **18 de octubre de 2026, 17:41.** No cerrado.
- **Qué se hace:** llevar los pilares al visor nuevo, con los tamaños de la guía NEVI y con lo que ya hacía el Desing antiguo.
- **Tamaños (NEVI 3.8 y prestaciones).** El pilar sale de cuatro paneles universales de 0,75 m, unidos con gancho de cierre y tuerca placa campana. La costilla en U tiene 14 orificios cada 5 cm. El gancho entra por los orificios laterales. Cuadrado, de **10×10 cm** (orificios 12 y 3) a **65×65 cm** (orificios 1 y 14), de 5 en 5 cm. El ancho máximo que declara la guía es **65 cm**. Si el gancho va por fuera del perfil, en vez de por el orificio lateral, el pilar queda 10 cm más pequeño. Costillas con orificio: el de 2,70 m lleva 3, el de 2,40 m lleva 2 y el de 1,20 m lleva 2. El rectangular combina orificios distintos. El embebido en muro o en esquina es otro caso (apartado 3.6).
- **En el Desing antiguo:** longitud y espesor de **0,10 a 0,55 m**. El formulario corta por encima de 0,55 m; no llega al 0,65 de la guía. El mínimo de 0,10 coincide con el 10×10. La altura no tiene tope. Apila módulos de 2,70 m; el resto, si cabe en 1,20, va en 1,20, y si cabe en 2,40 y ese panel existe, va en 2,40. El check de panel de 0,75 regulable va fijo. Cuatro caras, panel `27104219` en el de 2,70.
- **Puntales en el Desing antiguo:** dos, en dos caras contiguas, y solo si el pilar mide al menos **2,00 m**. Por encima de 1,20 m añade también el inferior, así que desde 2,00 m van el de arriba y el de abajo. Uno queda 16 cm antes del origen, girado a 270°. El otro queda en el origen en X y 47 cm atrás en Y, girado a 180°. No pone el segundo puntal a lo largo de la misma cara. El formulario guarda los checks, pero el generador los inserta siempre. La guía, en el capítulo del pilar, no da otra estación: el amarre del pilar es el gancho en el orificio de la tabla.
- **En el visor:** el botón de insertar pilares queda activo. En línea y en muro el pilar es un cuadrado de **0,30 × 0,30 m**. Doble clic en vacío, con la herramienta activa, lo deja. El doble clic sobre el pilar no lo borra. Ni el pilar ni el trabajador se escriben en la base al insertarlos: entran cuando se pulsa Guardar.
- **Modificar pilar:** los campos del formulario antiguo (grupo, longitud, espesor, altura, X, Y, paneles 0,75, puntales superiores e inferiores, rigidizadores). El estilo es el del formulario del muro. Se abre con el botón derecho al posar el ratón encima. Eliminar pilar y Aceptar. No guarda hasta Guardar.
- **Encofrado:** los pilares no sustituyen a los muros. El pilar no usa el panel GLB del muro: ese pose lo dejaba desplazado y sin gancho. Lleva el panel regulable del Desing antiguo, marco y fenólico (`27104219` a 2,70 m, `24104224` a 2,40 m, `12104120` a 1,20 m). En cada esquina, gancho de cierre `1920811` y tuerca placa campana `10443020`, a las alturas del generador antiguo.
- **Dimensionar:** al seleccionarlo siguen saliendo las dos cotas de longitud y espesor. El cuadrado crece desde el centro. La altura y los checks se cambian en el mismo formulario del muro.
- **Al pasar a 3D:** si el pilar todavía no tiene altura, toma **2,70 m**. Si ya se escribió en el formulario, se conserva. En muro 2D sigue siendo el cuadrado en planta.
- **Al encofrar:** cuatro paneles universales de 0,75 m. El sobrante del panel cuelga del lado del orificio 1 y el hormigón ocupa el tramo de la medida. Dos puntales en caras contiguas si la altura llega a **2,00 m**. Por debajo de 2 m no pone puntales.
- **Puntales:** NEVI 2.9. El cabezal puntal va en el agujero de la costilla más alejado del perfil exterior, a 50 mm de la cara exterior de la costilla. En el panel de 2,70 m ese agujero superior está a 428 mm del origen del panel y a 2 368 mm de altura. El pasador del puntal entra ahí. El cabezal tensor usa el agujero más próximo, a 40 mm. El punto exacto del pasador queda para retomarlo.
- **Muro 3D solo con pilares:** el generador saltaba los pilares y, si no había muros, avisaba «No hay líneas fuente para generar el modelo». Con pilares y sin muros entra en 3D y levanta los pilares.
- **Puntal según el encofrado:** la pieza no sale de la altura del hormigón. Sale de la suma de los paneles. A 4,00 m van 2,70 y 2,40, y el puntal es el de 5,10 m. Igual en el resto de alturas.

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
| Puntales | Cursor, 11–16 oct | Apartado 3 cerrado el 16 oct, 11:19. El reparto queda en el apartado 17 |
| Altura de las esquinas | Cursor, 12 oct | Apartado 4 cerrado, solucionado. 9:41–9:52, 11 min. Con el check, las esquinas suben con los muros |
| Editar muro | Cursor, 12 oct | Apartado 5 cerrado. 10:13–13:28, 3 h 15 min. No se testeó bien. Tapes sigue pendiente |
| Salvar la configuración del muro | Cursor, 12 oct | Apartado 6 cerrado. 13:28–15:05, 1 h 37 min. Pendiente de testeo; si falla, bug |
| Crear oferta desde el panel principal | Cursor, 12 oct | Apartado 7 en curso desde el 12 oct, 15:14. + de oferta en la cabecera; + de diseño junto al número, solo el nombre |
| Conexión SQL al publicar | Cursor, 12 oct | Apartado 8 en curso desde el 12 oct, 18:09. Release apunta a SQL5113; hay que republicar |
| Adjuntar diseños al cliente | Cursor, 13 oct | Apartado 9 en curso desde el 13 oct, 8:30. Pestañas Ofertas y Clientes en el panel Diseños. El alta de cliente escribe en `TSql_DesgloseOfertaCliente` |
| Foto del diseño | Cursor, 14 oct | Apartado 11 cerrado. Cámara, foto en el servidor, apartado Últimos diseños. Queda escrito en `Desing/IA/docs/agente-foto-diseno-y-menu.md` |
| Color del estado en las ofertas | Cursor, 14 oct | Apartado 12 cerrado. 11:39–11:44, estimación 5 min. El número lleva el color del estado en el inicio y en los documentos |
| Pestaña Diseño en el panel de inicio | Cursor, 14 oct | Apartado 13 cerrado. 11:52–12:30, estimación 38 min. Pestaña Diseños, tabla y fichas |
| Recuento de piezas | Cursor, 14–16 oct | Apartado 14 en curso desde el 14 oct, 12:37. Se retoma el 16 oct, 11:19. El 15 oct, 5 h 11 min más 10 min al publicar. El 16 oct por la mañana, 35 min de encofrado en el sitio, 17 min del muro especial sin artículos y 8 min de las barras |
| Tirantes en paneles tumbados | Cursor, 15 oct | Apartado 15 en curso desde el 15 oct, 16:28–17:59 (1 h 31 min). El tirante va en la parte superior de la franja |
| Muros colineales | Cursor, 16 oct | Apartado 16 en curso desde el 16 oct, 8:23–8:40 (17 min). Un tramo seguido en la misma recta queda en un solo muro. Al generar el 3D o encofrar, si ya venían partidos, se unen |
| Distancia entre puntales | Cursor, 16 oct | Apartado 17 cerrado. 9:16–9:54 y 10:53–11:19, 1 h 4 min. Paso máximo 2,40 m, mitad del paso en los dos extremos, y al cambiar el valor se vuelve a encofrar |
| Insertar trabajador | Cursor, 18–19 oct | Apartado 18 cerrado de momento. 12:22–16:18, 3 h 56 min. El 19 oct, al borrar el muro se borra también el trabajador de ese muro |
| Posición de plataformas y puntales | Cursor, 18 oct | Apartado 19 cerrado de momento. 16:18–17:41, 1 h 23 min. Queda para testear |
| Pilares | Cursor, 18–19 oct | Apartado 20 en curso desde el 18 oct, 17:41. Botón derecho abre Modificar pilar, con los campos del formulario antiguo y el estilo del formulario del muro. Encofrado pendiente de revisar |

## Al cerrar el 22 de octubre

- Completar apartados abiertos o dejarlos explícitos como pendientes para el 03.
- Regenerar el PDF de este fichero.
- El **23 de octubre** el agente crea `yyyy-MM-dd_Informe-Quincenal.md` del 03 sin que se lo pidan, si ese día hay trabajo o al primer mensaje de ese día.
