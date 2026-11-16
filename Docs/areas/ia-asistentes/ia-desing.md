# IA Desing

Documento único del **Asistente IA del muro** de Desing_2 (visor STL y página del plugin de AutoCAD) y de la **IA del diseño** (botón IA de la barra de modos, §16): qué hace, cómo está hecho, qué queda abierto y cómo probarlo. Estado a **14/11/2026**, imports `?v=20261114i`.

El detalle de cada pantalla está en [`desing-2-stl-viewer-funciones.md`](ia-en-el-visor.md) (sección «Asistente IA en la edición de muro») y el del cálculo de encofrado en [`encofrado-atk-60.md`](../encofrado-atk60/encofrado-atk-60.md) (§9). Aquí se resume y se enlaza; no se repite.

## Índice

1. [Objetivo y filosofía](ia-asistente-muro.md#1-objetivo-y-filosofía)
2. [Arquitectura y mapa de código](ia-asistente-muro.md#2-arquitectura-y-mapa-de-código)
3. [El intérprete](ia-asistente-muro.md#3-el-intérprete)
4. [Órdenes que entiende (con ejemplos)](ia-asistente-muro.md#4-órdenes-que-entiende-con-ejemplos)
5. [Servidor (C#)](ia-asistente-muro.md#5-servidor-c)
6. [Modo compacto 450 × 800](ia-asistente-muro.md#6-modo-compacto-450--800)
7. [Micrófono](ia-asistente-muro.md#7-micrófono)
8. [Respuestas habladas](ia-asistente-muro.md#8-respuestas-habladas)
9. [Persistencia actual](ia-asistente-muro.md#9-persistencia-actual)
10. [Estudio: muro especial con piezas propias](ia-estudio-decisiones-pruebas.md#10-estudio-muro-especial-con-piezas-propias)
11. [Columnas de BD propuestas (no creadas)](ia-estudio-decisiones-pruebas.md#11-columnas-de-bd-propuestas-no-creadas)
12. [Limitaciones conocidas](ia-estudio-decisiones-pruebas.md#12-limitaciones-conocidas)
13. [Preguntas abiertas](ia-estudio-decisiones-pruebas.md#13-preguntas-abiertas)
14. [Historial de decisiones](ia-estudio-decisiones-pruebas.md#14-historial-de-decisiones)
15. [Cómo probar](ia-estudio-decisiones-pruebas.md#15-cómo-probar)
16. [IA del diseño (botón IA de la barra de modos)](ia-del-diseno.md#16-ia-del-diseño-botón-ia-de-la-barra-de-modos)

---

## Partes (un tema por archivo)

| Secciones | Archivo | Qué hay |
|-----------|---------|---------|
| §1–§9 | [`ia-asistente-muro.md`](ia-asistente-muro.md) | Asistente IA del muro: objetivo, código, intérprete, órdenes, servidor, modo compacto, voz y persistencia |
| §10–§15 | [`ia-estudio-decisiones-pruebas.md`](ia-estudio-decisiones-pruebas.md) | Muro especial, columnas de BD propuestas, limitaciones, preguntas abiertas, decisiones y cómo probar |
| §16.1–§16.8 | [`ia-del-diseno.md`](ia-del-diseno.md) | IA del diseño (botón IA de la barra de modos): arquitectura, registro de comandos, decisiones, pruebas |
| §16.9–§16.11 | [`ia-del-diseno-girar-catalogo-entornos.md`](ia-del-diseno-girar-catalogo-entornos.md) | IA del diseño: Girar/Simetría, catálogo de comandos por usuario en BD y entornos de conversación (14/11/2026) |
| §16.12 | [`ia-del-diseno-cotas-por-voz.md`](ia-del-diseno-cotas-por-voz.md) | IA del diseño: cotas por voz: ID de los muros, «acótame el muro 7» y cambiar longitud / espesor / altura (15/11/2026, `?v=20261115a`) |
