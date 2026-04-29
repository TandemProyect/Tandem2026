# US-679: Extrude Create Corner Type1

**US:** #679 - Extrude Create Corner Type1  
**Tareas:** #680 (Develop) · #681 (CR) · #682 (Test)  
**Fecha inicio:** 2026-04-29  
**Fecha fin:** 2026-04-29  
**Estado:** ✅ Implementado y validado  
**Predecesora:** US-675 "Create Corner Type1 Lines DB" (polilínea ObjetoDB2d)

---

## 📋 Objetivo

A partir de la polilínea generada en US-675 (capa `ObjetoDB2d`), crear una **copia extruida 2700mm en Z** en la capa `ModelDesing`, conservando la polilínea original intacta.

---

## 💻 Implementación

### Arquitectura

Toda la lógica está en el **MVC** (`LCornerDetector.cs`). El plugin ZWCAD simplemente dibuja lo que recibe.

Por cada panel detectado se generan **2 polilíneas** en `PolilineasADibujar`:

| Capa | AlturaExtrusion | Resultado ZWCAD |
|------|----------------|-----------------|
| `ObjetoDB2d` | 0 | Polilínea plana (original) |
| `ModelDesing` | 2700 | Polilínea con Thickness=2700mm |

### PolilineaDTO — nueva propiedad

```csharp
public class PolilineaDTO
{
    public List<PuntoDTO> Vertices { get; set; }
    public bool Cerrada { get; set; }
    public string Capa { get; set; }
    public int ColorIndex { get; set; }
    public double AlturaExtrusion { get; set; }  // 0 = sin extrusión
}
```

### LCornerDetector.cs — dos polilíneas por panel

```csharp
var verticesEsquina = new List<PuntoDTO> { ptVerde, ptInterior, ptAmarillo, ptCian, ptExterior, ptBlanco };

// Original — ObjetoDB2d
resultado.PolilineasADibujar.Add(new PolilineaDTO
{
    Cerrada = true, Capa = "ObjetoDB2d", ColorIndex = 256, AlturaExtrusion = 0,
    Vertices = verticesEsquina
});

// Extruida — ModelDesing 2700mm
resultado.PolilineasADibujar.Add(new PolilineaDTO
{
    Cerrada = true, Capa = "ModelDesing", ColorIndex = 256, AlturaExtrusion = 2700,
    Vertices = verticesEsquina
});
```

### Commands.cs — aplicar Thickness y crear capas

```csharp
foreach (var nombreCapa in new[] { "ObjetoDB2d", "ModelDesing" })
{
    if (!lt.Has(nombreCapa)) { /* crear capa */ }
}

// Al dibujar cada polilínea:
if (poly.AlturaExtrusion > 0)
    lwp.Thickness = poly.AlturaExtrusion;
```

---

## ✅ Resultado del test

- Polilínea plana en capa `ObjetoDB2d` ✅
- Polilínea extruida 2700mm en capa `ModelDesing` (visible en vista 3D isométrica) ✅
- Ambas capas creadas automáticamente si no existen ✅

---

## 🔗 Archivos modificados

- `Desing/Models/ZwcadModels.cs` — `AlturaExtrusion` en `PolilineaDTO`
- `Desing/Services/LCornerDetector.cs` — genera dos polilíneas por panel
- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` — aplica `Thickness`, crea capas
- `TamdenZwcadPluging/ZwcadPlugin/Models.cs` — `AlturaExtrusion` en `PolilineaDTO`
