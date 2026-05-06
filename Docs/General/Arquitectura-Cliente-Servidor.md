# 🏛️ Arquitectura Cliente-Servidor — Regla de Oro

> **Última actualización:** 2026-05-05
> **Ámbito:** Todo lógica geométrica / de negocio de Tandem 2026

---

## 🔑 Regla de oro

> **Toda la lógica geométrica y de negocio vive en el proyecto `Desing` (C# MVC).**
> Los clientes son **renderers pasivos** que consumen los DTOs por HTTP y solo se preocupan del API gráfico nativo de su plataforma.

Ningún cálculo (intersecciones, offsets, medidas estándar, extrusión, detección de muros, etc.) debe duplicarse en los clientes.

---

## 🎯 Responsabilidades por capa

### Servidor — `Desing/` (C# .NET Framework 4.8)

- Toda la **lógica geométrica**: detección de esquinas L, cálculo de puntos (Verde, Amarillo, Blanco, Cian, Magenta, Criss, Interior, Exterior), identificación de muros rectos entre esquinas, medidas estándar de panel, etc.
- Todas las **decisiones de negocio**: qué punto se dibuja, en qué capa, con qué color, con qué altura de extrusión.
- Todas las **transformaciones de datos**: expandir polilíneas → segmentos, eliminar duplicados, emparejar paneles, etc.
- Emite un único DTO de salida (`DeteccionEsquinasLDTO`) con **todo resuelto**.

### Clientes — renderers pasivos

Deben ser lo más "tontos" posible. Reciben el DTO y llaman al API gráfico de su plataforma.

| Cliente | Proyecto | Estado |
|---------|----------|--------|
| ZWCAD Plugin | `TamdenZwcadPluging/ZwcadPlugin/` | ✅ En uso |
| Three.js Web | *(futuro)* | ⏳ Planificado |
| Otros (móvil, etc.) | *(futuro)* | ⏳ Planificado |

**Cada cliente traduce 1-a-1 los DTOs al API nativo:**

- `PuntoDTO` → círculo / cuadrado / esfera / marker según su `TipoPunto` y `ColorIndex`
- `PolilineaDTO` → polilínea cerrada / `THREE.Shape` plana si `AlturaExtrusion = 0`
- `PolilineaDTO` con `AlturaExtrusion > 0` → polilínea con `Thickness` / `ExtrudeGeometry` con `depth = AlturaExtrusion`

---

## 📦 Contrato (DTOs) — fuente única de verdad

Definidos en `Desing/Models/ZwcadModels.cs` (y replicados en el plugin como clases espejo idénticas).

### Entrada — `List<LineaDTO>`
Líneas/polilíneas seleccionadas por el usuario en el dibujo.

### Salida — `DeteccionEsquinasLDTO`
```csharp
{
  List<EsquinaLDTO>   Esquinas;            // conexiones simples (informativo)
  List<PuntoDTO>      PuntosADibujar;      // TODOS los puntos con TipoPunto + ColorIndex
  List<PolilineaDTO>  PolilineasADibujar;  // TODAS las polilíneas con Capa, ColorIndex, AlturaExtrusion
  string              Mensaje;
}
```

Los clientes **nunca** calculan puntos ni vértices adicionales.

---

## 🚫 Anti-patrones a evitar

- ❌ Calcular un punto de panel dentro de `Commands.cs` del plugin.
- ❌ Replicar la tabla de `MEDIDAS_PANEL` en JavaScript cuando llegue el cliente three.js.
- ❌ Que el cliente decida en qué capa pintar una polilínea (la capa la manda el servidor).
- ❌ Que el cliente aplique heurísticas geométricas ("si está cerca de X, entonces Y").

Si aparece una necesidad nueva → **primero se añade al DTO en el servidor**, luego los clientes lo consumen.

---

## 🔄 Ciclo de desarrollo de una US

1. Definir qué información nueva necesita el cliente para renderizar.
2. Extender `ZwcadModels.cs` si hacen falta campos nuevos en los DTOs.
3. Implementar la lógica en `LCornerDetector.cs` (o servicio equivalente).
4. Compilar `Desing` → `Desing.dll`.
5. Si hubo cambios en DTOs, replicar el espejo en `ZwcadPlugin/Models.cs` y recompilar plugin.
6. En cada cliente, añadir el `switch`/`if` que traduce el nuevo campo al API nativo — **sin lógica de negocio**.

---

## 📚 Referencias

- `@c:\00_Tandem2026\Docs\Proyectos\Desing\README.md` — arquitectura del servidor
- `@c:\00_Tandem2026\Docs\Proyectos\Desing\US679-Extrude-Create-Corner-Type1.md` — ejemplo del patrón (2 `PolilineaDTO` por panel: plana + extruida)
- `@c:\00_Tandem2026\Docs\Proyectos\Desing\US668-Locate-Exterior-Corner-Panel-Points.md` — 6 puntos por esquina L
- `@c:\00_Tandem2026\Docs\Proyectos\Desing\US671-Locate-Termination-Points.md` — puntos condicionales Magenta/Criss
