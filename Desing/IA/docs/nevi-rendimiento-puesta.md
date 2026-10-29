# NEVI — rendimiento de puesta, hormigonado y desencofrado

29 de octubre de 2026. Para el encofrado por fases.

Estudia un cálculo que llegó de otro agente (cuadrilla de 4 personas, jornada de 8 horas) y lo contrasta con la Guía del Usuario NEVI (`No_Publicar/NEVI_GUIA_USUARIO.pdf`), la ficha de ULMA y los plazos de referencia de la EHE-08. El Código Estructural no fija horas.

AT-60 en el visor sigue el mismo sistema: paneles de 2,70, 2,40 y 1,20 m, anchos de 0,90 a 0,30 m, grapa y tirante. Las juntas están en `agente-grapas-atk60.md`.

## Número para planificar

Con 4 personas y 8 horas, un muro de vivienda con alguna esquina, a mano: **unos 40 a 50 m² de muro** (las dos caras ya contadas). Es el suelo. Un paño recto y con grúa puede acercarse al doble.

Esos 40 a 50 m² de muro son unos 80 a 100 m² de cara de panel, más grapas, tirantes y estabilizadores.

La guía no da metros al día. El número sale de la base de precios de Extremadura, partida `E04MEF020` (encofrado y desencofrado de muro a dos caras, paneles metálicos modulares de 3 m, 20 puestas, con grúa): 0,20 h de oficial y 0,20 h de ayudante por m².

- Una pareja en 8 horas hace 40 m² de esa unidad.
- Cuatro personas, unos 80 m² de esa unidad.

Si el m² de la base ya es el muro (las dos caras dentro del precio), cuatro personas en un paño fácil rondan 80 m² de muro. Si el m² es cada cara, bajan a unos 40 m² de muro. El otro agente se quedó en 40 a 50. Vale como suelo para obra a mano, con esquinas o compensaciones. No vale como techo de un paño recto con grúa.

Baja el rendimiento: muchas esquinas, quiebros, pilares, tapes y tablones hechos en obra. Sube: muro seguido, paneles repetidos y grúa para conjuntos ya grapados.

## Qué dice la guía del panel

Página 5. NEVI es encofrado vertical de edificación, manuportable en la mayoría de medidas, entre ORMA y COMAIN.

| Pieza | Peso en la guía |
|-------|-----------------|
| Panel 2,70 × 0,90 m (2,43 m²) | 82,6 kg |
| Panel 2,40 × 0,90 m (2,16 m²) | 74,5 kg |

Esos dos los manejan dos personas. Una lista comercial de ULMA publica otros pesos según el tablero (por ejemplo 77 kg en el 2,70 × 0,90). Para este proyecto manda la guía: 82,6 kg y 74,5 kg.

Alturas 2,70, 2,40 y 1,20 m. Anchos 0,90, 0,75 (universal), 0,60, 0,45 y 0,30 m. Pilar hasta 65 cm, también a 60 kN/m².

## Presión y tongada

Página 5: presión máxima admisible **60 kN/m²**. Flecha a esa presión: línea 7 de la DIN 18202.

Página 64, panel 2,70 × 0,90:

| Carga | Grupos 5, 6 y 7 | Tirante | Tirantes por m² |
|-------|-----------------|---------|-----------------|
| Hidrostática máxima | 67,5 kN/m² | 59 kN | 0,82 |
| Constante máxima | 60 kN/m² | 72,9 kN | 0,82 |

El 2,40 × 0,90 se queda en 60 kN/m² también en hidrostática. Densidad que usa la guía: 25 kN/m³. 2,70 m × 25 kN/m³ = 67,5 kN/m²: el panel de 2,70 aguanta el hormigón fresco a toda su altura, sin recortar la velocidad. La ficha de ULMA dice lo mismo: por debajo de 2,70 m, con ese panel, no hace falta controlar la velocidad de hormigonado. Por encima de 2,70 m el tope es 60 kN/m² y entra el diagrama de la DIN 18218 (página 63: hormigón a +15 °C, vibrador interno, fraguado en menos de 5 h, velocidad de llenado hasta 7 m/h).

La tongada no es de 50 cm. Páginas 83 y 84: capas uniformes de **30 a 45 cm**, caída como mucho de 2 m si no hay tubo, vibrador de aguja 10 o 15 cm dentro de la capa de abajo, 10 o 30 segundos, sin tocar el tablero. Si el encofrado se mueve, se para el vertido.

## Hormigonado

El otro agente, con muro de 25 cm: 40 a 50 m² son 10 a 12,5 m³. En este proyecto el espesor habitual es 0,30 m: los mismos metros son 12 a 15 m³.

Con bomba ya puesta y el conjunto amarrado, el vertido de ese volumen cabe en **1 o 2 horas**. La puesta de la bomba y la espera del camión van aparte. No se empieza un conjunto nuevo hasta que el anterior está amarrado (página 82).

## Desencofrado

La guía (páginas 84 y 85) no da horas ni habla de 10 N/mm². Pide resistencia mínima para no arrancar la piel, el mismo tiempo en elementos iguales, y desencofrar los verticales de arriba abajo. Luego, bastidor, limpieza y desencofrante. Cada 4 o 5 puestas, bastidor metálico. Antes de la puesta siguiente se revisa el material. Sin viento de más de 60 km/h, ni con hielo o nieve.

El Código Estructural (art. 53, RD 470/2021) tampoco fija horas: el hormigón tiene que aguantar los esfuerzos del desencofrado, y el plan lo aprueba la dirección facultativa.

Referencia vieja, EHE-08 art. 75, cemento de endurecimiento normal, cuando no hay mejor dato. Encofrado vertical, según temperatura superficial del hormigón:

| Temperatura | Plazo mínimo |
|-------------|--------------|
| 24 °C o más | 9 horas |
| 16 °C | 12 horas |
| 8 °C | 18 horas |
| unos 2 °C | 30 horas |

En tiempo templado, hormigonar por la tarde y quitar a la mañana siguiente entra en esa tabla. Con frío se alarga. Por debajo de cero no se hormigona. Un acelerante puede bajar el plazo; lo tiene que aceptar la dirección de obra. El «10 N/mm²» del otro agente no está en la guía ni en el artículo 53.

ACI 347, citado junto a la EHE en la literatura, pone 12 horas como mínimo en vertical con cemento normal curado a 10 °C.

## Qué ocupa una fase

1. Encofrar. Con 4 personas, del orden de 40 a 50 m² de muro en la jornada si el muro no es un paño limpio con grúa.
2. Hormigonar. 1 o 2 horas cuando el conjunto ya está amarrado. Si la jornada se ha ido en montar, el vertido cae al final del día o a la mañana siguiente.
3. Esperar. En tiempo bueno, a la mañana siguiente. Con frío, hacia 18 o 30 horas.
4. Desencofrar de arriba abajo, limpiar, desencofrante y revisar. El mismo material pasa a la puesta siguiente.

Los paneles de una fase siguen ocupados desde que se montan hasta la mañana posterior al vertido. En tiempo templado, un juego de paneles da una puesta al día, no dos.

## Lo que el otro agente dejó bien y lo que no

Bien: pesos 82,6 kg y 74,5 kg; tope de 60 kN/m²; dos caras por cada m² de muro; 40 a 50 m² como suelo de una cuadrilla de 4; vertido en 1 o 2 horas para ese lote; desencofrar al día siguiente en tiempo bueno.

No usar tal cual: tongada de 50 cm (la guía dice 30 a 45); exigir control de velocidad en un muro de 2,70 m con panel de 2,70 (ese panel admite la hidrostática de 67,5 kN/m²); dar 10 N/mm² como si fuera norma; tomar 40 a 50 m² como techo cuando hay grúa y el muro es seguido.

Sigue sin dato de obra: altura, espesor, si el muro es un paño o lleva muchas esquinas, y si los conjuntos se mueven a mano o con grúa.
