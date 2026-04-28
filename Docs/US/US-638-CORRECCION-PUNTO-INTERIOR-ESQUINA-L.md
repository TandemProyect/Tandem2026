# US-638: Detectar puntos para sistema ATK60 - Corrección punto interior esquina L

## 📋 Información de la US

- **ID:** 638
- **Título:** Detectar puntos para sistema ATK60
- **Estado:** Done
- **Fecha de resolución:** 2026-04-28
- **Desarrollador:** Cascade AI (Windsurf)

---

## 🐛 Problema identificado

**Síntoma:** El comando `TANDEM_SELECCIONAR_LINEAS` no dibujaba ningún punto interior (círculo azul) al detectar esquinas tipo L, aunque sí se detectaban los paneles válidos.

**Archivo afectado:** `Desing/Services/LCornerDetector.cs`

### Causa raíz

El método `ClasificarVerticesInteriorExterior` tenía un guard estricto:

```csharp
if (vertices.Count != 4)
    return (new List<PuntoDTO>(), new List<PuntoDTO>()); // siempre vacío para esquinas L
```

El método previo `ObtenerVerticesRectangulo` recopilaba los 8 extremos de las 4 líneas y los deduplicaba. Para una esquina L:

- Si las caras interiores **se tocan** en el vértice → 7 puntos únicos (no 4)
- Si las caras interiores **no se tocan** (gap) → 8 puntos únicos (no 4)

En ambos casos el guard devolvía listas vacías → **ningún punto se dibujaba**.

**Error conceptual de fondo:** El algoritmo anterior asumía que las 4 líneas forman un rectángulo cerrado, pero una esquina L es una figura **abierta** en forma de "L".

---

## ✅ Solución implementada

### Nuevo método: `CalcularPuntosEsquinaL`

Reemplaza la llamada a `ObtenerVerticesRectangulo` + `ClasificarVerticesInteriorExterior` por cálculo geométrico directo:

```csharp
private (List<PuntoDTO> Interior, List<PuntoDTO> Exterior) CalcularPuntosEsquinaL(
    LineaDTO l1a, LineaDTO l1b,   // Par paralelo grupo 1
    LineaDTO l2a, LineaDTO l2b)   // Par paralelo grupo 2 (perpendicular al 1)
```

**Lógica:**

1. **Identificar línea interior de cada grupo** → la más cercana al centroide del grupo perpendicular
   - `innerG1` = cara interior del muro horizontal (la que mira hacia dentro de la esquina)
   - `innerG2` = cara interior del muro vertical (la que mira hacia dentro de la esquina)

2. **Intersección de líneas interiores** → punto interior exacto de la esquina (círculo azul en ZWCAD)

3. **Intersección de líneas exteriores** → punto exterior exacto de la esquina (círculo rojo en ZWCAD)

### Métodos helper añadidos

| Método | Descripción |
|--------|-------------|
| `IntersectarLineas(l1, l2)` | Intersección geométrica de dos rectas (extensión infinita). Devuelve `null` si son paralelas. |
| `DistanciaLineaPunto(linea, px, py)` | Distancia perpendicular de un punto a una línea. Usado para determinar cuál es la cara interior. |

### Diagrama de la solución

```
        outerG1 (y = -200)  ─────────────────────
        innerG1 (y =    0)  ──────────┐
                                      │ innerG2 (x = 0)
                                      │
                                        outerG2 (x = -200)

Punto interior = intersección(innerG1, innerG2) = (0, 0)   → círculo AZUL
Punto exterior = intersección(outerG1, outerG2) = (-200, -200) → círculo ROJO
```

---

## 📁 Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `Desing/Services/LCornerDetector.cs` | Reemplazado `ObtenerVerticesRectangulo` + `ClasificarVerticesInteriorExterior` por `CalcularPuntosEsquinaL`. Añadidos métodos `IntersectarLineas` y `DistanciaLineaPunto`. |

---

## 🧪 Verificación

**Prueba realizada:** Selección de 4 líneas formando una esquina L en ZWCAD (2 horizontales paralelas + 2 verticales paralelas).

**Resultado esperado y obtenido:**
- ✅ Círculo **azul** en el vértice interior de la esquina
- ✅ Círculo **rojo** en el vértice exterior de la esquina
- ✅ El punto interior es geométricamente correcto (intersección de caras interiores)

---

## 🔗 Referencias

- **Panel Azure DevOps:** https://dev.azure.com/VSCAD/tandem2026/_boards
- **US #638:** https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/638

---

**Creado:** 2026-04-28  
**Estado:** Implementado y verificado
