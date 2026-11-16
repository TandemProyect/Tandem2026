# Encofrado ATK-60 (resumen de consulta)

Escrito el 12 nov 2026. Resume el manual NEVI de ULMA y los documentos de la carpeta. Si una ficha detallada y este resumen no coinciden, manda la ficha. **[HUECO]** marca lo que no dice ninguna fuente: no se inventa.

Fuentes:

- Manual: Guía del Usuario NEVI (ULMA, 89 págs.), en `Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/NEVI_GUIA_USUARIO.pdf`. Páginas entre paréntesis.
- Fichas: `ENCOFRADO-AT60-BASE.md`, `ENCOFRADO-AT60-IMPLEMENTACION.md`, `esquina-L-atk60.md`, `esquina-T-atk60.md`, `agente-tapes-atk60.md`, `tapes-tipos.md`, `agente-grapas-atk60.md`, `HANDOVER-MUROS-T-CRUCE.md`, y `Docs/areas/encofrado-atk60/ATK60-Logica-Modulos-Wall.md`.

ATK-60 es el nombre del sistema en Desing. El manual es el del NEVI, que es el mismo sistema. Los códigos del manual (192xxxx) no son los del catálogo Desing (1000xxxx, E27…). Los dos se dan cuando se conocen.

## Partes (un tema por archivo)

| Secciones | Archivo | Qué hay |
|-----------|---------|---------|
| §1–§3 | [`encofrado-atk-60-manual.md`](encofrado-atk-60-manual.md) | Piezas, reglas del manual NEVI y reparto del muro recto |
| §4–§7 | [`encofrado-atk-60-codigo-historial.md`](encofrado-atk-60-codigo-historial.md) | Dónde está en el código, historial L/T/+, puntos abiertos y regresiones conocidas |
| §8–§9 | [`encofrado-atk-60-curvos-un-muro.md`](encofrado-atk-60-curvos-un-muro.md) | Muros curvos, encofrado de un solo muro, paneles que no hay (`_PanelStock`) y cambio guiado (`_PanelReplace`) |
