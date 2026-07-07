# 04 - Pipeline 2D a 3D

## Flujo completo

```
[DWG 2D]
    |
    v
1. CAPTURA 2D
   - Seleccion manual por el usuario o filtrado por capa.
   - Entidades: Line, Polyline (cerrada o abierta), LWPolyline.
   - Clase: WallReader.cs

    |
    v
2. NORMALIZACION GEOMETRICA
   - Snap por tolerancia (unir extremos cercanos).
   - Partir segmentos en sus intersecciones.
   - Eliminar duplicados y segmentos de longitud cero.
   - Clase: GeometryNormalizer.cs

    |
    v
3. CONSTRUCCION DEL GRAFO
   - Nodos = puntos de inicio/fin de segmentos.
   - Aristas = tramos de muro.
   - Clase: TopologyBuilder.cs

    |
    v
4. CLASIFICACION DE UNIONES
   - Contar segmentos por nodo -> L(2) / T(3) / +(4).
   - Calcular vectores de direccion y orientacion.
   - Resultado: WallModel con WallSegments y JunctionNodes.
   - Clase: JunctionClassifier.cs

    |
    v
5. SELECCION DEL SISTEMA DE ENCOFRADO
   - Comando ZWCAD pide al usuario el sistema (o lee config JSON).
   - Instancia el IEncofradoSystem correspondiente.

    |
    v
6. GENERACION 3D
   - Itera WallSegments y JunctionNodes del WallModel.
   - Llama a IEncofradoSystem.ResolveXxx() para cada elemento.
   - Inserta solidos 3D en el DWG (ModelSpace).
   - Clase: Solid3dBuilder.cs

    |
    v
7. PERSISTENCIA
   - Guarda Id y parametros como XData en cada entidad 2D.
   - Permite vincular 2D <-> 3D <-> metadata.
   - Clase: DwgPersistence.cs

    |
    v
8. REGENERACION
   - Comando REGENERAR3D: borra solidos anteriores y repite pasos 5-6.
   - Util cuando cambia sistema o parametros.
```

## Comandos ZWCAD nuevos previstos

| Comando | Accion |
|---------|--------|
| `DETECTARMUROS` | Ejecuta pasos 1-4, muestra resumen en consola |
| `GENERAR3D` | Ejecuta pasos 5-6 sobre modelo existente |
| `REGENERAR3D` | Borra 3D anterior y repite pasos 5-6 |
| `CONFIGENCOFRADO` | Selecciona sistema y parametros |

## Clases C# previstas

```
ZwcadPlugin/
  Topology/
    WallSegment.cs
    JunctionNode.cs
    BranchInfo.cs
    JunctionType.cs
    WallModel.cs
  Reading/
    WallReader.cs
    GeometryNormalizer.cs
    TopologyBuilder.cs
    JunctionClassifier.cs
  Formwork/
    IEncofradoSystem.cs
    FormworkResult.cs
    FormworkPiece.cs
    Solid3dDefinition.cs
    Systems/
      GenericSystem.cs
      Atk60System.cs
  Output/
    Solid3dBuilder.cs
    DwgPersistence.cs
```
