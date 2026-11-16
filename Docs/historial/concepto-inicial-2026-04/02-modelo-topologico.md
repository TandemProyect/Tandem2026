# 02 - Modelo topologico (independiente del sistema)

## Principio
Este modelo debe ser estable aunque cambie el sistema de encofrado.
Es la capa de dominio pura: no sabe nada de ZWCAD ni de ningun sistema concreto.

## Entidades

### WallSegment
```
Id            : Guid
StartNodeId   : Guid
EndNodeId     : Guid
Axis          : LineSegment2d   (linea eje del muro en 2D)
Height        : double          (altura en mm)
Thickness     : double          (espesor en mm)
IsExterior    : bool
Layer         : string          (capa de origen en el DWG)
```

### JunctionNode
```
Id            : Guid
Position      : Point2d
Type          : JunctionType    (L / T / Cross)
Branches      : List<BranchInfo>
```

### BranchInfo
```
SegmentId     : Guid
Direction     : Vector2d        (vector unitario saliente del nodo)
IsPrimary     : bool            (muro principal en T y +)
```

### JunctionType (enum)
```
L     = 2 direcciones (esquina)
T     = 3 direcciones
Cross = 4 direcciones
```

## Clases C# sugeridas
Namespace: `ZwcadPlugin.Topology`

```csharp
// ZwcadPlugin/Topology/WallSegment.cs
// ZwcadPlugin/Topology/JunctionNode.cs
// ZwcadPlugin/Topology/BranchInfo.cs
// ZwcadPlugin/Topology/JunctionType.cs
// ZwcadPlugin/Topology/WallModel.cs  <- contenedor del modelo completo
```

## Observacion sobre orientaciones
Las etiquetas Esq_10/30/50/70 (L) y Esq_20/40/60/80 (T) no son tipos distintos en el codigo.
Son la misma entidad con diferente rotacion/orientacion calculada a partir de los vectores de Branch.
