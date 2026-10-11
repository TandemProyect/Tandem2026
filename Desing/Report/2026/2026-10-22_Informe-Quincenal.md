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

## Al cerrar el 22 de octubre

- Completar apartados abiertos o dejarlos explícitos como pendientes para el 03.
- Regenerar el PDF de este fichero.
- El **23 de octubre** el agente crea `yyyy-MM-dd_Informe-Quincenal.md` del 03 sin que se lo pidan, si ese día hay trabajo o al primer mensaje de ese día.
