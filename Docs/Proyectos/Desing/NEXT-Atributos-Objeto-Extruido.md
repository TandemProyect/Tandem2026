# Próximo paso: Atributos en Objeto Extruido (Esquina Tipo L)

**Fecha:** 2026-04-29  
**Contexto:** Continuación de US-679 (Extrude Create Corner Type1)

---

## Estado actual

La polilínea extruida ya se genera en capa `ModelDesing` con `Thickness=2700mm`.  
El siguiente paso es **añadir metadata/atributos** al objeto extruido de cada esquina L.

---

## Opciones analizadas

### Opción A — XData (Extended Data) ⭐ Recomendada para metadata interna
- Se adjunta directamente a cualquier entidad (Polyline con Thickness)
- No visible al usuario en el dibujo, accesible por código
- Ideal para: ID de panel, tipo esquina, espesor muro, US origen, etc.

```csharp
// Ejemplo de implementación en Commands.cs
// Requiere registrar la AppName primero en RegAppTable
ResultBuffer xdata = new ResultBuffer(
    new TypedValue((int)DxfCode.ExtendedDataRegAppName, "TANDEM"),
    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "TipoEsquina:L"),
    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Capa:ModelDesing"),
    new TypedValue((int)DxfCode.ExtendedDataReal, 2700.0)
);
lwp.XData = xdata;
```

### Opción B — Block con AttributeDefinition
- El usuario puede ver y editar los atributos desde la UI de ZWCAD
- Más complejo de implementar
- Ideal si los atributos deben ser visibles/editables en el dibujo

### Opción C — Named Object Dictionary (NOD)
- Estructura clave/valor adjunta a la entidad o al documento
- Más rica que XData, soporta tipos complejos

---

## Decisión pendiente

- [ ] Confirmar qué atributos se quieren guardar (tipo, espesor, ID, etc.)
- [ ] Confirmar si deben ser visibles en ZWCAD (→ Opción B) o solo internos (→ Opción A)
- [ ] Crear US en Azure DevOps para esta tarea

---

## Archivos a modificar (estimado)

- `Desing/Models/ZwcadModels.cs` — añadir `Atributos` a `PolilineaDTO`
- `Desing/Services/LCornerDetector.cs` — poblar los atributos por panel
- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` — registrar AppName y escribir XData
