# 00 - Resumen ejecutivo del proyecto

## Objetivo
Crear un plugin en C# para ZWCAD 2026 que, a partir de un dibujo 2D de muros:
1. Construya un modelo topologico (muros y uniones L/T/+).
2. Genere una representacion 3D para visualizacion.
3. Produzca metadata necesaria para encofrar segun un sistema configurable.

## Tecnologia
- Lenguaje: C# / .NET Framework 4.8
- Plataforma: ZWCAD 2026 (API ZwSoft.ZwCAD.*)
- Comunicacion: HTTP con servidor MVC ASP.NET (proyecto `Desing`)
- Repositorio: https://github.com/JuanGodoyLopez/Design
- Proyecto plugin: `TamdenZwcadPluging/ZwcadPlugin/`

## Tipos logicos del modelo (v1)
- **Muro (WallSegment)**: tramo lineal con altura, espesor y nodos extremos.
- **Nodo L (JunctionNode tipo L)**: union de 2 direcciones (esquina).
- **Nodo T (JunctionNode tipo T)**: union de 3 direcciones.
- **Nodo + (JunctionNode tipo Cross)**: cruce de 4 direcciones.

Las orientaciones Esq_10/30/50/70 (L) y Esq_20/40/60/80 (T) son rotaciones del mismo tipo logico.

## Regla clave de diseno
La geometria 3D NO es una extrusion literal del 2D.
Cada sistema de encofrado puede cambiar el tamano de las esquinas y la resolucion de encuentros.
El 2D se conserva siempre como referencia/entrada.

## Infraestructura existente (lista para usar)
| Clase | Descripcion |
|-------|-------------|
| Commands.cs | Comandos ZWCAD registrados |
| ZwcadHelper.cs | Extraccion de entidades 2D del DWG |
| MVCApiService.cs | Comunicacion HTTP con servidor MVC |
| Models.cs | DTOs de transferencia |
| FormPrincipal.cs | Formulario WinForms base |

## Lo que hay que construir
Ver `04-pipeline-2d-a-3d.md` para el detalle de cada capa.
Ver `05-pendientes-y-preguntas.md` para las decisiones pendientes antes de codificar.
