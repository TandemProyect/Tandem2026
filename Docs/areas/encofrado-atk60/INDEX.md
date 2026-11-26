# Área: Encofrado ATK-60 (NEVI)

**Cubre:** piezas y reglas NEVI, reparto del muro recto, esquinas L/T/X, tapes, grapas, rigidizadores, tirantes, muro curvo, encofrado por fases, recuento.

**No cubre:** el chat de la IA (→ `ia-asistentes`), la inserción de bloques en AutoCAD (→ `cad-autocad`).

## Código

- Servidor: `Desing/Repositories/RepositoryAtk60/`, `Repository_Desing_2/Atk60/`, `RepositoryCommun/`
- Cliente: `Desing/Scripts/MasterArticles/ma-stl-atk60-formwork.js`, `desing2-stl-viewer-toolbar-wiring.js`, fases `Desing/Scripts/Desing2/desing2-phase-*.js`
- Manual: `Desing/Content/DesignTools/DWG/AtkSystem60/Snaps/NEVI_GUIA_USUARIO.pdf`

## Qué leer (solo lo que necesites)

| Si vas a… | Abre |
|-----------|------|
| Empezar (resumen y mapa de partes) | [`encofrado-atk-60.md`](encofrado-atk-60.md) |
| Piezas, reglas del manual NEVI, reparto del muro recto (§1–§3) | [`encofrado-atk-60-manual.md`](encofrado-atk-60-manual.md) |
| Dónde está en el código, historial L/T/+, regresiones (§4–§7) | [`encofrado-atk-60-codigo-historial.md`](encofrado-atk-60-codigo-historial.md) |
| Muros curvos, un solo muro, `_PanelStock`, `_PanelReplace` (§8–§9) | [`encofrado-atk-60-curvos-un-muro.md`](encofrado-atk-60-curvos-un-muro.md) |
| Catálogo y reglas base | [`ENCOFRADO-AT60-BASE.md`](ENCOFRADO-AT60-BASE.md) |
| Implementación en el visor | [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md) |
| Esquina L / T | [`esquina-L-atk60.md`](esquina-L-atk60.md), [`esquina-T-atk60.md`](esquina-T-atk60.md) |
| Tapes | [`agente-tapes-atk60.md`](agente-tapes-atk60.md), [`tapes-tipos.md`](tapes-tipos.md) |
| Grapas, rigidizador, tirante | [`agente-grapas-atk60.md`](agente-grapas-atk60.md) |
| Encofrado por fases y rendimiento | [`HANDOVER-ENCOFRADO-FASES.md`](HANDOVER-ENCOFRADO-FASES.md), [`nevi-rendimiento-puesta.md`](nevi-rendimiento-puesta.md), [`encofrado-fases-diseno-30008.md`](encofrado-fases-diseno-30008.md) |
| Reparto del muro (múltiplos de 0,15, tablón) | [`ATK60-Logica-Modulos-Wall.md`](ATK60-Logica-Modulos-Wall.md) |
| Plano de montaje: caras F-n, vistas por cara y sus tablas | [`plano-montaje.md`](plano-montaje.md) |
| Esquinas regulables, recuento y guardado | [`ESQUINAS-REGULABLES-GUIA-IMPLEMENTACION.md`](ESQUINAS-REGULABLES-GUIA-IMPLEMENTACION.md), [`HANDOVER-2026-10-18-…-Recuento-Guardado.md`](historial/HANDOVER-2026-10-18-Desing2-ATK60-Recuento-Guardado.md) |
| Posición de piezas y tirantes (con el visor) | [`HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md`](historial/HANDOVER-2026-10-08-ATK60-Posicion-Uniones.md), [`HANDOVER-2026-10-09-ATK60-Tirantes.md`](historial/HANDOVER-2026-10-09-ATK60-Tirantes.md) |
| Uniones T/+ en el modelo de muros | [`../desing2-muros-3d/HANDOVER-MUROS-T-CRUCE.md`](../desing2-muros-3d/HANDOVER-MUROS-T-CRUCE.md) |

## Código común

Usa código común: lee [`Docs/codigo-comun.md`](../../codigo-comun.md) antes de cambiar el motor: lo usan el visor (`Desing_2Controller`) **y** los plugins (`PluginEncofrarAtk60`).

## Reglas del área

- El motor debe pasar la regresión del solver 74+13 byte a byte.
- Una cota está bien solo cuando Juan Andrés la confirma mirando el visor.
- Agentes de Cursor del área: `.cursor/agents/encofrado-atk-60.md`, `.cursor/agents/atk60-posicion-uniones.md`.

## Historial (no leer salvo que haga falta)

- `historial/HANDOVER-ATK60-ESTABILIZACION-2026-07-16.md`, `historial/HANDOVER-ENCOFRADO-AT60-SESION-T-2026-07-10.md`
- `historial/BITACORA-GEOMETRIA-AT60-…`, `historial/HANDOVER-GEOMETRIA-AT60-LTX-2026-07-11.md` (antes en `IA/LogicaGeometriaAT60/`)
- `historial/SISTEMA-ATK60.md`, `historial/HANDOVER-LCornerDetector.md` (detección de esquinas, abril 2026, era ZWCAD)
- `historial/Desing2-ATK60-Bitacora-Agente-2026-07-13-14.md`, `historial/TRABAJOS-ACTIVOS-Desing2-2026-10-18.md`
- `historial/US668…`, `historial/US671…`, `historial/US679…`, `historial/NEXT-Atributos-Objeto-Extruido.md` (abril 2026)
