# Área: Asistentes IA (muro y diseño)

**Cubre:** Asistente IA del muro (chat, voz, recuento, cambio de paneles), IA del diseño (barra de modos), catálogo de comandos y frases en BD, entornos de conversación.

**No cubre:** el cálculo del encofrado en sí (→ `encofrado-atk60`).

## Código

- `Desing/Scripts/MasterArticles/wall-ai-assistant.js`, `design-ai-assistant.js`, `ai-command-catalog.js`
- `Desing/Controllers/Desing_2Controller.AiCommands.cs`
- `Desing/Controllers/Desing_2Controller.AiUnknownPhrases.cs` y `Desing/App_Data/IA/frases-no-comprendidas.json` (frases no comprendidas)
- Tablas `TSql_Ai*` (scripts `Desing/Scripts/TemporalScript/2026-11-14_*TSql_Ai*.sql`, `2026-11-15_seed_TSql_AiCommand_cotas.sql` y `2026-11-22_seed_TSql_AiCommand_insert_wall_pillar.sql`)

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Empezar (resumen y mapa de partes) | [`ia-desing.md`](ia-desing.md) |
| Asistente IA del muro: código, intérprete, órdenes, servidor, voz (§1–§9) | [`ia-asistente-muro.md`](ia-asistente-muro.md) |
| Muro especial, BD propuesta, limitaciones, decisiones, cómo probar (§10–§15) | [`ia-estudio-decisiones-pruebas.md`](ia-estudio-decisiones-pruebas.md) |
| IA del diseño: arquitectura, añadir un comando, pruebas (§16.1–§16.8) | [`ia-del-diseno.md`](ia-del-diseno.md) |
| IA del diseño: Girar/Simetría, catálogo por usuario, entornos (§16.9–§16.11) | [`ia-del-diseno-girar-catalogo-entornos.md`](ia-del-diseno-girar-catalogo-entornos.md) |
| IA del diseño: cotas por voz (ID de los muros, acotar, longitud / espesor / altura) (§16.12) e ID de los paneles (§16.13); cotas del muro seleccionado, «acota todo» y «selecciona el muro 7» en verde en 2D y 3D (§16.14) | [`ia-del-diseno-cotas-por-voz.md`](ia-del-diseno-cotas-por-voz.md) |
| Comandos y frases por defecto | [`ia-comandos-catalogo.md`](ia-comandos-catalogo.md) |
| Pantallas de la IA en el visor | [`ia-en-el-visor.md`](ia-en-el-visor.md) |
| Frases que la IA no comprende: JSON en `App_Data/IA`, endpoints y revisión «analiza el .json» (§16.15) | [`ia-frases-no-comprendidas.md`](ia-frases-no-comprendidas.md) |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar `wall-edit-form.js` / `wall-ai-assistant.js` (también los carga la paleta de los plugins) y el motor de encofrado.

## Reglas del área

- Tono por defecto: Ejecutivo.
- Tablas `TSql_Ai*` iguales en develop y producción.
- Sube `?v=` en todos los imports y en `WallForm.cshtml`.
- Botones, pestañas y endpoints de la IA están protegidos con la función `IA`. Si añades uno, sigue `Docs/areas/seguridad/recetas.md` (C, D, E).
