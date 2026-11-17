# Tandem 2026 — entrada para agentes

Lee **solo** este archivo y el `INDEX.md` de tu área. No leas «toda la documentación».

## Reglas de siempre

- Repo `C:\Users\jag\00_Tandem2026`, rama `develo`. Sin commit, push ni stash salvo que Juan Andrés lo pida. No descartes sus cambios pendientes.
- No toques `Desing/IA/Communication/WallConnections.json`.
- Bases de datos: `db_a197cd_desingdevelop` y `db_a197cd_desingproducction` deben ser **idénticas** hasta enero de 2027. Todo cambio de esquema va a las dos, con script idempotente en `Desing/Scripts/TemporalScript/`. Local: `.\SQLEXPRESS` con `-E`. No uses las credenciales de `Web.config`. Tablas nuevas: `.cursor/rules/sql-tsql-table-conventions.mdc`.
- La lógica de negocio y la geometría viven en `Desing` (C#). El visor y los plugins CAD solo piden y pintan: `Docs/areas/general/Arquitectura-Cliente-Servidor.md`.
- JS cambiado: `node --check` y sube `?v=` donde se importa. C#: compila con VS2022 (MSBuild 17), 0 errores.
- ¿Tocas código que usan varias áreas? Lee antes `Docs/codigo-comun.md`.
- Responde en español, breve. Horas en Europe/Madrid.
- Informe de trabajo, solo si lo piden: `Desing/Report/LEEME.md`.

## Si trabajas en X, lee solo Y

| Área | Lee | Código principal |
|------|-----|------------------|
| Visor Desing_2: cámara, reglas, paneles, cookie | `Docs/areas/desing2-visor/INDEX.md` | `master-article-details-stl-viewer.js`, `Views/Desing_2/` |
| Dibujo 2D en Desing_2 | `Docs/areas/desing2-dibujo-2d/INDEX.md` | herramientas de la barra superior del visor |
| Muros 3D en Desing_2 | `Docs/areas/desing2-muros-3d/INDEX.md` | muro recto, curvo, L/T/+ |
| Encofrado ATK-60 | `Docs/areas/encofrado-atk60/INDEX.md` | `Desing/Repositories/RepositoryAtk60/`, `Repository_Desing_2/Atk60/` |
| Asistentes IA (muro y diseño) | `Docs/areas/ia-asistentes/INDEX.md` | `wall-ai-assistant.js`, `design-ai-assistant.js` |
| Base de datos y DAL | `Docs/areas/base-de-datos/INDEX.md` | `DAL/`, `Desing/Scripts/TemporalScript/` |
| Intranet: CRUD, Materio, traducciones | `Docs/areas/intranet/INDEX.md` | controladores y vistas de la intranet |
| Plugins CAD, parte común (API, sesión, paleta) | `Docs/areas/cad-comun/INDEX.md` | `DesignToolsAutocadController.cs` |
| ZWCAD | `Docs/areas/cad-zwcad/INDEX.md` | `TamdenZwcadPluging/` |
| AutoCAD | `Docs/areas/cad-autocad/INDEX.md` | `TandemAutocadPlugin/` |
| BricsCAD y Revit | `Docs/areas/cad-bricscad-revit/INDEX.md` | `TandemBricscadPlugin/`, `TandemRevitPlugin/` |
| OpenCADStudio | `Docs/areas/cad-opencad/INDEX.md` | `OpenCadStudioPluging/`, `TandemOpenCadStudioPlugin/` |
| FreeCAD | `Docs/areas/cad-freecad/INDEX.md` | `FreeCadPluging/` |
| TandemXR (gafas) | `Docs/areas/tandemxr/INDEX.md` | `TandemXR/`, `TandemXR-Unity/` |
| Azure DevOps, US, scripts | `Docs/areas/azure-devops/INDEX.md` | `Scripts/*.ps1` |
| Despliegue | `Docs/areas/despliegue/INDEX.md` | `.github/workflows/` |
| LinkedIn | `Docs/areas/linkedin/INDEX.md` | `LinkedIn/` |
| General: arquitectura, convenciones, Git | `Docs/areas/general/INDEX.md` | — |

## No leas salvo que el INDEX te lo pida

- Carpetas `historial/` de cada área y `Docs/historial/`.
- `IA/` en la raíz: restos antiguos (ver `IA/LEEME-OBSOLETO.md`).
- `Desing/docs/recovered-chats-*`, `Desing/Report/` (informes) y `Docs/Personal/`.
- Si el INDEX no basta, abre **un** documento más: el que el INDEX indique para tu tarea.

Cómo se mantiene esta estructura: `Docs/COMO-TRABAJAR.md`.
