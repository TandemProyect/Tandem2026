---
name: encofrado-atk-60
description: Encofrado ATK-60. Corrige orientación y reflejo de paneles ATK-60 en el visor Desing_2 (diseño 10002, panel 27904209) sin tocar el plugin CAD. Usar cuando los paneles manuales salen girados, simétricos hacia dentro, o el reflejo los desplaza a lo largo del muro.
---

# Encofrado ATK-60

Eres el agente de encofrado ATK-60 de Tandem2026. Trabajas en español. El encargo es que los paneles manuales se vean en Desing_2 como en AutoCAD.

## Alcance

- Tocar solo el visor Desing_2: `Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js`, funciones `maStlDesing2RenderManualWallArticles`, `maStlDesing2ReflectManualArticleOutward` y `maStlDesing2FaceOutwardNormal`.
- No modificar el plugin CAD (`TandemAutocadPlugin`, `BlockInsertCommand.cs`, snaps, conexiones) salvo que el usuario lo pida en ese mensaje.
- No tocar cubo de vistas, órbita ni la regla `desing-2-view-cube-orbit`.

## Contrato de orientación (no regredir)

- Caso de referencia: diseño **10002**, muro de 10 m, diez paneles `27904209`. Posiciones en `C:\temp\Posicion.json` si el usuario lo deja ahí.
- Caja del panel de pie: **0,90 × 0,10 × 2,70 m**. Varios STL de ATK-60 son texto ASCII; no leerlos como binario.
- Giro codificado en `RotationZ`: `|Z| >= 4500` es el primer panel (`spun`, descuento 5000 y el 180° ya está en el punto lógico). `|Z| >= 3000` y menor que eso es el resto (`4000`).
- El primer panel (`spun`) **no se refleja**.
- Los demás guardan el yaw del bloque. El STL con ese yaw mete el cuerpo hacia el eje: reflejar **solo el espesor**, por la normal de la cara (`maStlDesing2ReflectManualArticleOutward`).
- Prohibido reflejar a lo largo del muro. Eso monta los paneles encima de los dos primeros.
- Tumbado (`RotationX ≈ 90`): el ancho 0,90 queda arriba y el largo 2,70 a lo largo del muro; apoyar el mínimo Y en la cota de inserción.

## Comprobación en el diseño 10002

Tras un cambio, pedir recarga con **Ctrl+F5** y este resultado:

- De 0 a 0,90 m: par de pie, enrejado hacia fuera.
- De 0,90 a 2,70 m: los otros dos de pie, por fuera en las dos caras.
- De 2,70 a 8,10 m: horizontales de 0,90 m de alto, en las dos caras.

Si una captura contradice esto, revertir el reflejo que lo empeoró antes de probar otro giro.

## Contexto de inserción (solo lectura)

Antes de interpretar `RotationX/Y/Z` o el punto de inserción, leer:

- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md` — giro al abrir y estado del visor.
- `Docs/Proyectos/Plugins-CAD/HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md` — marcas, simétrico e inferior derecho.

No reabrir el camino de plugin para arreglar el visor.
