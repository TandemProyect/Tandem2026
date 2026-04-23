# IA - Proyecto Tandem 2026 (ZWCAD Muros/Encofrado)

Este directorio contiene toda la documentación técnica y el historial de conversación
del desarrollo del plugin ZWCAD para detección de muros y generación de encofrado 3D.

## Estructura

```
IA/
├── README.md                        <- este archivo (índice general)
├── chat/                            <- conversaciones guardadas por fecha
│   └── 2026-04-22-chat-inicio-proyecto.md
├── docs/                            <- especificación viva del proyecto
│   ├── 00-resumen.md
│   ├── 01-requisitos-y-alcance-v1.md
│   ├── 02-modelo-topologico.md
│   ├── 03-sistemas-de-encofrado.md
│   ├── 04-pipeline-2d-a-3d.md
│   └── 05-pendientes-y-preguntas.md
└── ejemplos/
    ├── README.md
    └── imagenes/
```

## Como usar esta documentacion con una IA

### Si retomas el trabajo (sesion nueva)
1. Copia el contenido de `docs/00-resumen.md` como primer mensaje.
2. Adjunta el chat del dia anterior desde `chat/`.
3. La IA tendra todo el contexto para continuar sin repetir.

### Si se incorpora un nuevo miembro del equipo
1. Leer `docs/00-resumen.md` -> vision general.
2. Leer `docs/01-requisitos-y-alcance-v1.md` -> que se hace y que no.
3. Leer `docs/04-pipeline-2d-a-3d.md` -> flujo tecnico completo.
4. Ver `docs/05-pendientes-y-preguntas.md` -> que esta por decidir.

## Estado actual del proyecto

| Capa | Descripcion | Estado |
|------|-------------|--------|
| Infraestructura plugin | Commands, ZwcadHelper, MVCApiService, Models | Hecho |
| Modelo topologico | WallSegment, JunctionNode | Por hacer |
| Lector 2D | Deteccion de muros y nodos desde DWG | Por hacer |
| Sistema encofrado | Interfaz IEncofradoSystem + ATK60 | Por hacer |
| Generacion 3D | Solidos en ZWCAD | Por hacer |
| Persistencia | XData/ExtensionDictionary en DWG | Por hacer |

## Repositorio

- GitHub: https://github.com/JuanGodoyLopez/Design
- Rama principal: master
- Proyecto plugin: TamdenZwcadPluging/ZwcadPlugin/ZwcadPlugin.csproj
