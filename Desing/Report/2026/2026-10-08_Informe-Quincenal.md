# Informe quincenal 01 — Tandem 2026

**Corte:** jueves 8 de octubre de 2026
**Periodo:** 5 de agosto – 8 de octubre de 2026
**Fuentes:** chats de Cursor, chats de Visual Studio Code (Copilot) y la documentación que esos agentes dejaron. No se ha usado el historial de git.

Este es el primer informe. Cubre dos meses. A partir del **15 de octubre de 2026** cada informe cubre una sola quincena: del 1 al 15, o del 16 al último día del mes. El siguiente no reescribe este.

Los chats personales (perfil, compras, camisetas) no entran.

Entre el 6 de agosto y el 8 de septiembre los agentes de producto no retoman Tandem. El hilo de muro recto en Visual Studio Code vuelve ese día, después de un descanso.

## Agosto

### 5 y 6 de agosto — Cursor, escaneo de edificio

Chat de Cursor del 5 de agosto por la noche y del 6 por la mañana, sobre el agente de escaneo de edificio en Desing_2.

- Se arregla el fallo de compilación al abrir el visor (`StlPreview_ImageSketchErrorModalTitle`).
- Seleccionar un edificio en el mapa: el botón izquierdo elige y la rueda desplaza. Al elegir, se enseña nombre y referencia catastral.
- Buscar vuelve a mostrar todos los edificios. Buscar edificio mantiene la búsqueda concreta.
- Reloj de carga mientras llegan los edificios.
- IFC de ejemplo para el futuro, sin tocar el importador IFC que ya existía. La idea queda en localizar el edificio y, más adelante, traer sus caras.
- El mismo día se pasa a `develo` lo hecho de XR (dispositivos y Enviar a XR) y se limpia la rama.

Documentación de apoyo: `Docs/Proyectos/TandemXR/README.md` y `Desing/Files/IfcSamples/README.md`.

## Septiembre

### 8 de septiembre — Muro recto ATK-60

Por la mañana, Visual Studio Code. Tres chats:

- «Muro Recto Encofrar ATK60» se retoma para testear.
- Otro chat arregla el arranque: `Atk60ElementPaintItem` no tenía `BaseRotX`, y renombra lo que el proyecto base Desing ya usaba.
- Otro chat añade, al doble clic de un muro, la longitud además de la altura.

A las 11:00 entra Cursor, en el chat que luego será la sesión larga de plugins. Pide estudiar la documentación de muro recto y cerrar las modulaciones ATK-60.

- Un muro de 15,25 m: cinco módulos de 2,70, uno de 1,20, remate de 0,10 y el cierre. El remate queda entre el módulo de 1,20 y el de 0,45, y siempre es el penúltimo. Se termina con un módulo. Ningún remate pasa de 0,15 m. La regla vale para todas las medidas.
- Se tumban solo 2,70, 2,40 y 1,20. Los módulos de 0,30 a 0,90 llevan un panel vertical de 1,20.
- Cotas de modulación en horizontal, pegadas a la cota de longitud. La cota vertical se deja para más adelante.
- Dos muros de la misma medida dibujados en sentidos distintos tienen que encofrar igual.
- Al encofrar, pantalla de «renderizando ATK-60» con una estimación de tiempo.
- Si se sale del modo encofrado, las cotas de encofrado se apagan.
- Caso 6,85 m: el remate que salía de más de 1,50 m era en realidad 0,15 m, y se corrige la lógica.

La línea de julio sigue en `Desing/IA/docs/HANDOVER-ATK60-ESTABILIZACION-2026-07-16.md`. Este día es la continuación, no un trabajo nuevo desde cero.

### 15 de septiembre — Guardar el diseño

Cursor. El usuario lo plantea como US nueva: guardar muros y encofrado para reabrir el diseño.

- Tabla de muros, preparada para crecer con atributos que cambian la solución de encofrado.
- El icono Salvar escribe los muros del diseño. Al abrirlo, se recuperan.
- Al salir (volver, salir o el explorador) pregunta si se guarda, solo cuando hay cambios.
- Aviso de «guardando diseño» mientras dura.
- Se revisan las conexiones de un muro simple a 90°. El usuario las da por recuperadas ese día y deja anotado que el guardado de conexiones aún se miraría aparte.

### 24 de septiembre — AutoCAD, BricsCAD, Revit y TandemIAHelp

Cursor, por la mañana y la tarde. A falta de cerrar el test del muro recto, se pide un plugin de AutoCAD 2026 al estilo de ZWCAD, dentro de Tandem 2026.

- El menú de herramientas se sustituye por formularios MVC: dos paletas, sin el submenú de paneles.
- Comandos de AutoCAD en inglés y con guion bajo (`_Move`), para que no dependan del idioma del usuario.
- Muro 2D con los comandos del propio AutoCAD (alargar y el resto). Al generar el 3D, el mismo aviso que Desing_2.
- Se copia el mismo plugin a BricsCAD y, por la tarde, a Revit 2026. Del plugin de empresa en `No_Publicar\Revit_2026` solo se mira cómo se instala. No se copia su código.

Por la tarde, Visual Studio Code abre TandemIAHelp: un vídeo de una función, una marca en el código y una narración. El avatar y la voz quedan para después. El primer caso es dibujar muro en planta.

Documentación: `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-09-Plugins-Tandem-CAD.md` (pedida el 28, de la semana del 22 al 28) e `IA/TandemIAHelp/README.md`.

### 28 de septiembre — Conexión del plugin

Cursor.

- Si la sesión no está abierta, sale el login de Desing. Con el equipo registrado, entra solo. Si no, pide usuario y, al acertar, registra el equipo.
- Mensaje «Comprobando autorización en TDesing», reloj, y el logo TDesing (no AT). Después, el logo de la empresa en las paletas.
- El mismo cargador se pide para la primera entrada a la intranet.
- Comparación de esquema entre la base local y la de producción (`Scripts/Compare-SqlSchema.ps1`).
- Publicación a producción y el 404 de `ma-stl-atk60-formwork.js`, resuelto metiendo el archivo en el proyecto.
- Fallo del token antifalsificación al entrar desde el plugin.
- Botón de inicio que abre el menú general (obra, proyecto, diseño).
- Correo con SendGrid: el envío no da error y el mensaje no llega. Se deja a la espera del proveedor.
- Se rompe el arranque por un `@keyframes` mal cerrado en la pantalla de carga. Se recupera el 29 por la mañana.

### 29 de septiembre — Bloquing

Cursor.

- Catálogo desde el maestro de artículos. La imagen de cada fila es `ImgIco`. El panel de prueba es el ATK-60 de 2,70 × 0,90.
- El botón de bloquing solo existe en el plugin.
- Visor STL con la misma orientación que Desing_2 (arriba = frente) y las herramientas de encuadre, color y corte.
- Cerrar, plegar y una caja pequeña para volver a abrirlo.
- Se documenta para que otro agente pueda seguir, incluida la doble conexión local / producción.
- Debate de inserción: bloques 3D y 3DRef en esta entrega. Alzado y planta quedan en el formulario, apagados. Sin referencias externas: el bloque se sustituye.
- Marcas de acople en el 3DRef. Opción de giro 0° o 90°.

Documentación: `HANDOVER-2026-09-29-Autocad-Bloquing.md`.

### 30 de septiembre — Inserción y biblioteca de paneles

Cursor.

- El panel sigue al ratón desde el primer momento. Escape cancela. Bloquing se pliega al insertar.
- Las marcas se iluminan al pasar cerca y, al hacer clic, enganchan. Si no se hace clic, el panel sigue libre.
- Los cuatro vértices enganchan. El panel nuevo hereda el giro del panel al que se une.
- Los tumbados dejan de caerse siempre al punto de abajo.
- Convertir una selección de bloques a 3D o 3DRef, con un aviso pequeño del color de la plantilla.
- A partir del manual NEVI y del maestro `27904209R.dwg` (punto de inserción inferior izquierdo) se generan el resto de paneles de referencia: marco, líneas y taladros del universal de 0,75.
- Conversión STL a DWG: queda llena de caras. Se acuerda partir de sólidos del DWG maestro, no de la malla.

Visual Studio Code, el mismo día: resumen de los bloques STL del visor (paneles, uniones, dywidag y puntales).

Documentación: `HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md`.

## Octubre

### 1 de octubre — Paneles 3D, 3DRef y huecos en el maestro

Cursor.

- Los universales 1,20 y 2,40 (`12104120`, `24104224`) salen del maestro `27104219`. Los taladros de esos tres los cierra el usuario.
- El resto de 3DRef, con perfil y círculos.
- En `Tsql_Master_Articles`, atributos para el DWG 3D, 3DRef y Xr, y el formulario de edición del artículo.
- El plugin inserta el bloque que haya en esa tabla: si se cambia el archivo, el cambio vale para todos.
- Error 11007 del EDMX al actualizar la entidad, resuelto volviendo a mapear la tabla.

### 2 de octubre — Biblioteca en el equipo y sesión

Cursor. Traer el DWG del servidor tarda unos 40 segundos. Se pasa a una copia local.

- Primera conexión: crea `%LocalAppData%\AtDesing\Content\Data\Block\` (`3D`, `3DRef`, …) y un JSON de catálogo. Insertar y convertir leen esa copia.
- Actualizar solo cuando el usuario lo pide, no mientras trabaja.
- Enlace de instalación desde la ficha de personal, por correo. Marca de desarrollador en el empleado, para seguir probando con `NETLOAD`.
- Comando de reseteo para los desarrolladores: `UnAtdesing`.
- La sesión vale para todos los dibujos de AutoCAD abiertos.
- Splash con el paso («copiando bloque i de N») y logo TDesing.
- El menú de obras no puede salir en blanco: ahí sí hay que hablar con el servidor.
- Paletas movibles, recordando el sitio. El aviso de convertir bloques, más bajo.
- Pie con calidad de wifi y de servidor (buena, aceptable o mala), en lenguaje de usuario.
- Se pide documentar el día. Sale `HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md`.
- Por la tarde se iguala el encofrado automático del plugin con Desing. La lógica es una. Los paneles salen al revés y se corrige el sentido.

### 3 de octubre — Conexión, review y la US que se pidió

Cursor.

- Se apaga la instalación automática al cargar. `NETLOAD` no debe volver a copiar los bloques.
- Salvar muros como en Desing. Al cargar un diseño, se cierra el formulario y se hace zoom extensión.
- El encofrado automático inserta 3DRef, que va más rápido.
- El semáforo de servidor medía listados pesados y salía «mala» incluso en local. Se cambia la medida. En Desing_2 se pone el mismo test, sin origen ni catálogo.
- Se pide merge de `develo` y `master`, y una US nueva: **Encofrar manualmente**. La documentación del día dice que el texto quedó escrito y que el panel de Azure no la creó (el script respondió 401).
- Code review de arquitectura pensando en que el proyecto conecta todo el rato. Queda en `Docs/General/CODE-REVIEW-PREP-2026-10-03.md`.
- Handover: `HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md`. Ahí el comando que instala la biblioteca pasa a llamarse `ATDESING`.

### 5 de octubre — Encofrado a mano y artículos por muro

Cursor. Fase siguiente: elegir un panel en bloquing, recorrerlo por el muro e insertarlo.

- El simétrico va a la cara paralela.
- En vista 2D no se ven los artículos. Al levantar el 3D vuelven.
- Se guardan al pulsar Salvar, con aviso y un aviso final de «diseño salvado».
- Un muro con artículos a mano no se encofra solo. En Desing_2, al abrir el diseño, se ven esos artículos en STL.
- Los tumbados a 90° salen desplazados y sin giro. Los simétricos salen girados 180°. Se pide el mismo criterio de sentido del muro que el encofrado automático.
- El panel tiene que buscar el hueco y no duplicar uno que ya está.

### 6 de octubre — Posición al insertar y al abrir

Cursor, chat largo, hasta dejar el inferior izquierdo bien y el inferior derecho apartado para no romperlo.

- Conexión local lenta, pantalla negra antes del login, y el ratón que no entra en el usuario y la contraseña.
- Inferior derecho: el panel se sale del muro o entra girado hacia dentro. El izquierdo se da por bueno. Se decide no seguir tocando el derecho ese día.
- En Desing_2, los paneles a 90° salen desplazados en elevación.
- Al reabrir, AutoCAD y Desing_2 no coinciden con lo dibujado. Se sospecha de la coordenada guardada.

A las 14:00, chat nuevo «Encofrado ATK-60 Manual»: lee lo anterior y se centra en que lo guardado vuelva al sitio. JSON de prueba en `C:\temp\Posicion.json` y `C:\temp\conexiones.json`. AutoCAD queda bien al reabrir. Desing_2 no.

Un agente aparte estudia solo el desplazamiento entre AutoCAD y Desing. Otro chat, tras un reinicio, recupera el mismo bug: en el visor, salvo los primeros paneles, el resto sale girado y los simétricos hacia dentro.

Documentación pedida al cerrar el día y escrita el 7: `HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md` y `HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md`. Presentación prevista el jueves 8. Camino de demo: inferior izquierdo.

El mismo 6 por la tarde, Visual Studio Code empieza FreeCAD: menú Tandem 2026, y la duda de qué objeto usar (STL pesado, DWG de sólidos, luego STEP, SAT e IFC).

### 7 de octubre — ZWCAD, uniones y FreeCAD

Cursor. Por la mañana se registra el agente de proyecto **Encofrado ATK-60** (`.cursor/agents/encofrado-atk-60.md`) y se documenta lo que faltaba del visor.

AutoCAD pide licencia y no deja trabajar. Se pasa el plugin de estos días a ZWCAD 2026, con la misma biblioteca en el disco del usuario y el mismo bloquing. Los 3DRef se ven mal por la plantilla: en pulgadas se ven, en milímetros no salen las líneas con altura. Se crea `Desing2026.dwt` desde la plantilla de ZWCAD que sí se veía bien. El usuario lo deja en que el bloque abierto se ve bien y el insertado no, y que no es un giro.

Por la tarde, con `C:\temp\Piezas de unión.pdf`, se insertan uniones en Desing, después de los paneles.

- Grapa fija `10004220`. Sale girada y metida en el muro. Se deja el STL para mover el punto de inserción a mano.
- Rigidizador corto `1850162` y largo `1850163`. El corto, en la altura de 3 m, solo cuando se combinan 0,30 y 0,45.

Visual Studio Code, en paralelo: convierte los IFC de ATK-60 a `.FCStd` (ZWCAD es el CAD que exporta; AutoCAD y BricsCAD sin licencia). Prueba de muro 2D, levantar sólidos y encofrar. Se pide que el workbench abra los formularios MVC, como AutoCAD y ZWCAD, y que retire los menús antiguos al activar Tandem 2026.

### 8 de octubre — Grapa, placa, gancho y el corte del informe

Cursor, agente Encofrado ATK-60.

- Placas (tuercas) a la altura del rigidizador, sumando el ancho de la pieza universal. El desplazamiento de elevación se corrige hasta dejar los tirantes como en la primera implementación. El fallo que queda es la placa.
- Grapa regulable de remate de madera (`10000221`). Si va la regulable, no va la fija. El sentido vertical se acepta. El horizontal no engancha los paneles.
- En planta, gancho y placa se habían ido el doble. La placa se rehace cambiando el origen del STL. El gancho `1850164` se gira en alzado hacia el perfil. El de la izquierda se acepta. El de la derecha puede necesitar el simétrico.
- Al cierre, el usuario lo deja así: el rigidizador y la placa se meten en el panel, la grapa general va desplazada y el panel de 0,30 × 2,70 m también. La posición es general, no de una sola pieza.
- Se pide un agente nuevo solo para la posición, con documentación. Queda `.cursor/agents/atk60-posicion-uniones.md` y `HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md`. Diseño de referencia: 10002.

Visual Studio Code, por la mañana, sigue FreeCAD hasta acercarlo a ZWCAD (formularios MVC). Por la tarde se deja para después. El resumen de agentes de septiembre y octubre está en `Docs/Proyectos/Plugins-CAD/RESUMEN-2026-09-10-Trabajo-Agentes.md`.

## Dónde está cada frente

| Frente | Quién lo llevó | Al 8 de octubre |
|--------|----------------|-----------------|
| Mapa y escaneo | Cursor, 5–6 ago | Edificio seleccionable, catastro de ejemplo e IFC de muestra. XR pasado a la rama de trabajo |
| Muro recto ATK-60 | Visual Studio Code y Cursor, 8 sep | Modulaciones, remate penúltimo y cotas horizontales. La cota vertical quedó a medias |
| Guardar diseño | Cursor, 15 sep | Muros en base de datos, pregunta al salir y aviso de guardado |
| AutoCAD | Cursor, desde el 24 sep | Paletas, bloquing, biblioteca local, encofrado automático y paneles a mano. Demo por el inferior izquierdo |
| BricsCAD y Revit | Cursor, 24 sep | Misma base que AutoCAD ese día. No son el camino de la semana del 6 al 8 |
| ZWCAD | Cursor, 7 oct | El plugin reciente, para poder presentar sin la licencia de AutoCAD |
| Desing_2, artículos a mano | Cursor, 5–7 oct | Se pintan. El reflejo y el giro al abrir no quedaron confirmados |
| Uniones | Cursor, 7–8 oct | Grapa, rigidizador, placa y gancho insertados. La posición no está cerrada |
| FreeCAD | Visual Studio Code, 6–8 oct | Workbench, piezas `.FCStd` y paletas MVC a medias |
| Ayuda en vídeo | Visual Studio Code, 24 sep | Ficha y narración de dibujar muro en planta |

## Abierto del 9 al 15 de octubre

1. Posición de las uniones y del panel de 0,30 × 2,70 m. Una familia cada vez. El agente de posición ya está creado.
2. Gancho dentro del taladro. El giro de la izquierda está aceptado. El de la derecha puede pedir el simétrico.
3. Grapa regulable en horizontal, cuando el gancho y la placa estén fuera del panel.
4. Inferior derecho del panel a mano, sin tocar el inferior izquierdo.
5. Confirmar en Desing_2 que los paneles a mano coinciden con AutoCAD al reabrir.
6. Crear en Azure la US «Encofrar manualmente». El 3 de octubre se pidió y el panel no la aceptó.
7. FreeCAD: que al activar Tandem 2026 salgan los formularios MVC y no los menús de prueba.

## Siguientes cortes

| Informe | Periodo | Día de corte |
|---------|---------|--------------|
| 01, este | 5 ago – 8 oct 2026 | 8 oct 2026 |
| 02 | 9 oct – 15 oct 2026 | 15 oct 2026 |
| 03 | 16 oct – 31 oct 2026 | 31 oct 2026 |
