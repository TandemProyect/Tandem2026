# 03 - Sistemas de encofrado

## Concepto
Un sistema de encofrado es una clase que implementa la interfaz `IEncofradoSystem`.
Recibe el modelo topologico y devuelve geometria 3D + metadata.
El nucleo del plugin no sabe nada del sistema concreto.

## Interfaz propuesta

```csharp
namespace ZwcadPlugin.Formwork
{
    public interface IEncofradoSystem
    {
        string Name { get; }

        // Geometria 3D de un tramo recto
        FormworkResult ResolveWallSegment(WallSegment segment);

        // Geometria 3D de una esquina L
        FormworkResult ResolveCornerL(JunctionNode node, WallModel model);

        // Geometria 3D de una union T
        FormworkResult ResolveJunctionT(JunctionNode node, WallModel model);

        // Geometria 3D de un cruce +
        FormworkResult ResolveCross(JunctionNode node, WallModel model);
    }

    public class FormworkResult
    {
        public List<Solid3dDefinition> Solids { get; set; }
        public List<FormworkPiece>     Pieces  { get; set; }  // metadata
    }
}
```

## Implementaciones previstas
| Clase | Sistema | Estado |
|-------|---------|--------|
| `Atk60System` | ATK60 (sistema ya en proyecto Design) | Por hacer |
| `GenericSystem` | Simplificado para pruebas | Por hacer (v1) |

## Salida esperada por nodo/segmento
- Lista de solidos 3D con posicion y dimensiones.
- Lista de piezas de encofrado con referencia, cantidad y posicion.

## Parametros configurables (por sistema)
- Dimensiones de paneles.
- Retornos y solapes en esquinas.
- Prioridad de muro principal en T y +.
- Tolerancias minimas.
