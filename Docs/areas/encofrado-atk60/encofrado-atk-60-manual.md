# Encofrado ATK-60 — Piezas, reglas del manual NEVI y reparto del muro recto

> Parte de [`encofrado-atk-60.md`](encofrado-atk-60.md) (partido el 15/11/2026 sin quitar contenido; la numeración de secciones es la original). Área: [`INDEX.md`](INDEX.md).

## 1. Piezas

| Pieza | Manual NEVI | Desing | Para qué |
|-------|-------------|--------|----------|
| Panel estándar | Alto 2,7 / 2,4 / 1,2 m. Ancho 0,90 / 0,60 / 0,45 / 0,30 m. Fenólico de 15 mm. Presión máx. 60 kN/m² | Catálogo 0,30 a 0,90 en paso 0,15 (`ATK60-Logica-Modulos-Wall.md`) | Cara del muro |
| Panel universal 0,75 | Costillas en U con 14 orificios cada 5 cm (p. 44) | Tapes tipo 3 (`TapeS5`…) | Esquina exterior a 90°, tapes, pilares |
| Escuadra interior | 0,30 × 0,30 m. 1920881 (2,7), 1920887 (2,4), 1920893 (1,2) | `MA_ATK60_CORNER_PANEL_MM = 300`. Códigos E27004210 / E24004217 / E12004216 | Rincón de L, T, X |
| Angular | No aparece con ese nombre **[HUECO]** | 0,12 m; une, no encofra. 27000000 / 24000000 / 12000000 | Esquina exterior de la L en Desing |
| Escuadra G (giratoria) | 1920943 / 47 / 51 | No implementada **[HUECO]** | Esquinas distintas de 90° |
| Grapa fija | 1920818. Solo a tope | `10004220` | Toda junta entre dos paneles |
| Grapa regulable | 1920851. Admite suplemento hasta 50 cm | `10000221` / `10000221B` | Compensación 0–5 cm y tablón de remate |
| Rigidizador COMAIN | 0,75 / 1,6 / 3 m (1850162 / 1850159 / 1850433), fijador 1850164 | `1850162` en `CommonElement.SedUnionRigiHorizontal_0` | Compensaciones, juntas horizontales, tapes |
| Gancho cierre + tuerca placa | 1920811; tuerca placa fija / campana | Usados en tapes | Atar universal y rigidizador |
| Tirante | Barra roscada Ø15 (15/1 … 15/6) | Placa `10443020` + barra; a 43,7 mm del canto | Atar las dos caras |
| Compensadores | 5 cm (1920901/04/09), 5–18 (1920863/67/71), 8–30 (1920916/20/24) | Tablón de madera ≤ 0,15 (remate) | Cerrar medidas no modulares |
| Puntal / estabilizador | Tensores 1,1–1,7 / 2,4–3,5 / 3,6–4,8 / 5–6 m; cabezal 1920839; base 1900144 | `PushPullProp.cs` | Aplomar |
| Ménsula de trabajo | 1861094 | `WorkingBracket.cs` | Pasarela |
| Gancho de izado | 1920835, máx. 1 200 kg | — | Grúa |

## 2. Reglas del manual

### Juntas (3.1, p. 38–39)

- Grapa fija siempre que se pueda. Junta vertical: 3 grapas en panel de 2,7 m; 2 en 2,4 y 1,2 m.
- Con compensación 0–5 cm: grapas regulables (3 / 3 / 2).
- Con compensación 5–18 cm: grapas fijas 3 / 2 / 2 y rigidizadores 2 / 2 / 1.
- Una junta junto a una esquina exterior a 90° o a un tape lleva una grapa más.
- Alturas de grapa usadas en Desing: 2,70 → 450, 1 350, 2 250 mm; 2,40 → 400, 2 000; 1,20 → 200, 800. Tirante: 550 / 2 150 (2,70), 550 / 1 850 (2,40), 350 / 850 (1,20). No poner grapa a la altura del tirante (`agente-grapas-atk60.md`).

### Compensaciones (3.2, p. 41–43)

| Hueco | Solución |
|-------|----------|
| 0–5 cm | Taco de madera en obra + grapa regulable + tuerca placa campana |
| 5 cm | Compensación 5 cm + grapa regulable |
| 5–18 cm | Compensador 5–18 (un panel solapa la chapa) + rigidizadores + grapas fijas |
| 8–30 cm | Compensador 8–30 (los dos paneles solapan) + rigidizadores 0,75 (4 / 4 / 3) |

En Desing el hueco se cierra con un tablón de madera: ≤ 0,15 m a lo largo, 0,12 m de ancho, grapa regulable en los dos cantos. **[HUECO]** El manual no da ese tablón: es regla de Desing.

### Esquina L a 90° (3.3, p. 44–45)

- Rincón: escuadra interior 0,30 × 0,30, con grapas fijas 3 / 2 / 2.
- Exterior: panel universal con estándar, o dos universales, atados con gancho cierre y tuerca placa fija.
- Desing (`esquina-L-atk60.md`): cara exterior = `selectPanel(espesor del otro muro + 0,30)`, esquina interior 0,30 y angular 0,12. Ejemplo: 0,47 + 0,30 = 0,77 → panel de 0,90.

### Esquinas que no son de 90° (3.4, p. 46–48)

- 75–85°: escuadra G dentro y fuera. Dentro va con gancho rigidizador y placa excéntrica, y lleva compensación ≥ 5 cm.
- 85–180°: escuadra G en los dos lados con grapas fijas (5 / 5 / 3).
- Rigidizadores 0,75 fuera (2 / 2 / 1); dentro solo a partir de 100°.
- **[HUECO]** Desing no lo hace.

### T y X (3.5, p. 49)

- El manual solo dice: escuadra interior combinada con anchos de panel según el espesor. El resto es dibujo (dos escuadras en la T, cuatro en la X).
- Desing, T (`esquina-T-atk60.md`):
  - Cara E (testero frente a la rama) = espesor de la rama + 0,30 + 0,30, sin angular. Hasta 0,90 es un panel con el remate al final; con más, dos paneles con el remate en medio.
  - Recorte de ejes: el de paso se retira ½ espesor de la rama + 0,30. La rama se retira ½ espesor del de paso + 0,30. El espesor del de paso es la **media** de los dos tramos (`TryTCornerTrimMm`).
  - Tipo 2: los dos tramos de paso tienen distinto espesor. El fino se corre (grueso − fino)/2 alejándose de la rama. El escalón queda en la cara de la rama y se cierra con tablón ≤ 0,15 y grapa variable.
- Desing, X: cuatro escuadras interiores. Cada brazo se retira ½ espesor del que cruza + 0,30 (`TryPlusCornerTrimMm`). Sin cara E ni angular. La X de igual espesor está aceptada.
- **[HUECO]** El remate con espesores distintos en T y X está pendiente desde el 29 oct (`esquina-T-atk60.md`, «Pendiente»). La T tipo 3 está nombrada pero sin definir.

### Tapes (3.7, p. 52–53)

- Con rigidizador: atado con gancho cierre y tuerca placa fija por la parte exterior del perfil. Rigidizadores 4 / 3 / 2. La junta contigua lleva una grapa más.
- Con panel universal: gancho cierre y tuerca placa campana.
- Desing usa los tipos 1 a 7 (`tapes-tipos.md`):

| Espesor | Sin solape | Con solape |
|---------|------------|------------|
| < 0,30 | Tipo 2 (`TapeS3`) | Tipo 1 (`TapeS1` / `TapeS2`) |
| 0,30 / 0,45 / 0,60 / 0,90 exacto | Tipo 4 (`TapeS4`) o tipo 3 (`TapeS5`) | Tipo 1 |
| 0,30–0,60, no exacto | `TapeS6` / `TapeS7` (múltiplo de 0,05) o `TapeS8` | Tipo 1 |
| > 0,60, no exacto | `TapeS9` / `TapeS10` / `TapeS11` (múltiplo) o `TapeS13` / `TapeS12` | Tipo 1 |

- Solo se cierra un extremo libre (`EndIsFree`). Un extremo unido a otro muro no lleva tape.

### Otras soluciones (3.6, 3.8, 3.9, p. 50–57)

- Cambio de espesor de muro: hasta 15 cm con tacos de madera y rigidizadores 0,75; 20 cm con escuadra interior, gancho rigidizador y tuerca campana.
- Retranqueo: escuadra interior con paneles universales y estándar.
- Pilar: paneles universales unidos entre sí; tabla de orificios de 10×10 a 65×65 cm en paso de 5 (p. 54–55).
- Pilar embebido en muro o esquina: escuadra interior con universal y estándar. Saliente mínimo en esquina 200 × 300.
- **[HUECO]** En Desing solo `PillarFormwork.cs`; no se ha comprobado contra estas tablas.

## 3. Reparto del muro recto

`PackAtk60LengthMm` (`Atk60WallsRepository.cs`):

- n paneles de 2,70 + el mayor módulo final que quepa + el remate.
- El remate no pasa de 0,15. Si sobra más, el módulo final sube un paso.
- Si el sobrante está entre 0,16 y 0,29, un 2,70 se abre en 2,55 + 0,30.
- Extremo conectado: se recorta 450 mm (`ConnectedEndTrimMm`). La L usa `LCornerFormwork.AxisTrimMm`, la T `TryTCornerTrimMm` y la X `TryPlusCornerTrimMm`.
