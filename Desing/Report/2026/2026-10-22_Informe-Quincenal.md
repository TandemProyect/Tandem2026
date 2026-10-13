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
- **Hecho en el panel Diseños:** icono «+» en la cabecera para crear la oferta (obra, nombre, cliente, estado). Además, junto a cada número de oferta ya registrado, otro «+» crea el diseño dentro de esa oferta y solo pide el nombre. Diseños queda abajo, en tres de los cuatro huecos, con la columna Cliente, orden y buscador. El alta es el botón «Nuevo diseño» y el título indica la oferta. Arriba: obras, ofertas y clientes. Abrir un diseño usa el mismo ojo que el resto. Cada panel lleva su barra de desplazamiento vertical.

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
- **Formularios:** el panel Diseños tiene dos pestañas. Ofertas deja el listado de ahora. El «Nuevo diseño» de cada fila es más pequeño y va separado del número; solo pide el nombre. El + de la cabecera pasa a ser el botón «Nuevo diseño» y pide la oferta y el nombre. Clientes muestra primero el cliente y después la oferta, si la tiene. El «Nuevo diseño» de la fila pide el nombre y adjunta el diseño nuevo solo a ese cliente, sin pedir la oferta. El botón de arriba pide el cliente y el nombre.

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
| Ménsulas de trabajo | Cursor, 11 oct | Apartado 2 cerrado. 9:22–10:39, 1 h 17 min. El agujero de la ménsula queda en el fallo de posición |
| Puntales | Cursor, 11 oct | Apartado 3 en curso desde el 11 oct, 10:39 |
| Altura de las esquinas | Cursor, 12 oct | Apartado 4 cerrado, solucionado. 9:41–9:52, 11 min. Con el check, las esquinas suben con los muros |
| Editar muro | Cursor, 12 oct | Apartado 5 cerrado. 10:13–13:28, 3 h 15 min. No se testeó bien. Tapes sigue pendiente |
| Salvar la configuración del muro | Cursor, 12 oct | Apartado 6 cerrado. 13:28–15:05, 1 h 37 min. Pendiente de testeo; si falla, bug |
| Crear oferta desde el panel principal | Cursor, 12 oct | Apartado 7 en curso desde el 12 oct, 15:14. + de oferta en la cabecera; + de diseño junto al número, solo el nombre |
| Conexión SQL al publicar | Cursor, 12 oct | Apartado 8 en curso desde el 12 oct, 18:09. Release apunta a SQL5113; hay que republicar |
| Adjuntar diseños al cliente | Cursor, 13 oct | Apartado 9 en curso desde el 13 oct, 8:30. Pestañas Ofertas y Clientes en el panel Diseños. El alta de cliente escribe en `TSql_DesgloseOfertaCliente` |

## Al cerrar el 22 de octubre

- Completar apartados abiertos o dejarlos explícitos como pendientes para el 03.
- Regenerar el PDF de este fichero.
- El **23 de octubre** el agente crea `yyyy-MM-dd_Informe-Quincenal.md` del 03 sin que se lo pidan, si ese día hay trabajo o al primer mensaje de ese día.
