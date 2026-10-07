# Plugins Tandem CAD (AutoCAD, BricsCAD, Revit, FreeCAD)

Continuación del trabajo de plugins Tandem 2026, **aparte de ZWCAD**.

## Regla arquitectonica principal

La logica C# de negocio, geometria, DTOs, conexion MVC, catalogos, validaciones y calculos debe ser comun para todos los plugins CAD y para el proyecto `Desing`. Los proyectos especificos de cada host CAD solo deben contener adaptadores de plataforma: carga del plugin, menus, comandos, seleccion de entidades, conversion entre tipos nativos del CAD y modelos comunes, y cualquier integracion que el host obligue a escribir de forma particular.

Puede haber clases C# especificas por plugin, y en casos necesarios tambien codigo en otros lenguajes impuesto por el host, como Python en FreeCAD. Ese codigo especifico no debe duplicar reglas de negocio ni geometria: debe llamar a codigo comun C# o a endpoints MVC comunes.

Cuando una funcionalidad sirva a mas de un plugin, primero debe moverse o nacer en una capa comun reutilizable. El plugin concreto solo decide como presentar el comando y como traducir entradas/salidas del CAD.

**Empezar aquí (AutoCAD, corte 2026-10-07):**

- **Retomar hoy:** [HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md](./HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md) — giro al abrir (`EncodePoseZ`, Debug122) y orientación en el visor Desing_2. El reflejo solo del espesor está sin confirmar en pantalla.
- Corte 2026-10-06: [HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md](./HANDOVER-2026-10-06-Encofrar-Manual-Conexiones.md) — insertado uno a uno, marcas del muro, vértices del panel, simétrico, guardado `AT:`, bug del inferior derecho.
- Corte 2026-10-03: [HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md](./HANDOVER-2026-10-03-Conexion-ATDESING-Encofrar-Manual.md) — NETLOAD vs ATDESING, test de conexión, Git `develo`/`master`.
- Sesión / home / bloquing (2026-10-02): [HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md](./HANDOVER-2026-10-02-Autocad-Sesion-Bloquing-Autoload.md) — sesión persistente, menú obras/ofertas, biblioteca en AppData, insert/convert, UnAtdesing.
- Muros / paletas modo-herramientas (base): [HANDOVER-2026-09-Plugins-Tandem-CAD.md](./HANDOVER-2026-09-Plugins-Tandem-CAD.md)
- Bloquing (formulario, contexto sept-29): [HANDOVER-2026-09-29-Autocad-Bloquing.md](./HANDOVER-2026-09-29-Autocad-Bloquing.md)
- Contrato de inserción ATK-60 (geométrico; el código **ya existe**): [HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md](./HANDOVER-2026-09-29-Autocad-Insercion-Bloques.md)

| Host | Código | Notas |
|------|--------|--------|
| AutoCAD 2026 | `TandemAutocadPlugin/` | NETLOAD |
| BricsCAD V26 | `TandemBricscadPlugin/` | NETLOAD |
| Revit 2026 | `TandemRevitPlugin/` | add-in + [MSI](../../TandemRevitPlugin/README.md) |
| FreeCAD 1.1 | `FreeCadPluging/` | Workbench Python minimo + host C# |

Geometría de muros: `LCornerDetector` vía `DesignToolsAutocad/ProcesarLineasZwcad`. No duplicar en los plugins.
