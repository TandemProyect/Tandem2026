# US-671: Locate Termination Points (Puntos de Remate)

**US:** #671 - Locate termination points  
**Tareas:** #672 (Develop) · #673 (CR) · #674 (Test)  
**Fecha inicio:** 2026-04-29  
**Fecha fin:** 2026-04-29  
**Estado:** ✅ Implementado y validado  
**Predecesora:** US-668 (6 puntos ya implementados: Azul, Rojo, Verde, Amarillo, Blanco, Cian)

---

## 📋 Objetivo

Añadir **dos nuevos puntos condicionales** (puntos de remate) a cada esquina en L:

- **Magenta** (ColorIndex=6) — brazo horizontal exterior, desde ptRojo
- **Criss** (ColorIndex=9) — brazo vertical exterior, desde ptRojo

Estos puntos aparecen **únicamente** cuando la distancia `(espesor_muro + 300 mm)` NO coincide con una medida estándar de panel, señalando el estándar más cercano inferior.

---

## 🎯 Medidas estándar de panel (mm)

```
300, 450, 600, 750, 1050, 1200, 1350, 1500, 1650, 1800, 2100
```

### Regla de decisión

```
distBlanco = espV + 300
distCian   = espH + 300

Si distBlanco NO es estándar → Magenta en ptRojo + mayor_estándar_menor_que(distBlanco)
Si distBlanco ES estándar    → NO se dibuja Magenta

Si distCian NO es estándar   → Criss en ptRojo + mayor_estándar_menor_que(distCian)
Si distCian ES estándar      → NO se dibuja Criss
```

### Ejemplo validado

| Valor | Descripción |
|-------|-------------|
| espV = espH = 250 mm | Espesor del muro |
| distBlanco = 250 + 300 = **550 mm** | NO es estándar |
| Magenta en **450 mm** desde ptRojo | Mayor estándar < 550 |
| distCian = 250 + 300 = **550 mm** | NO es estándar |
| Criss en **450 mm** desde ptRojo | Mayor estándar < 550 |

**Caso sin remate:** espV = 150 → distBlanco = 450 (SÍ estándar) → Magenta no se dibuja.

---

## 🏗️ Tabla completa de puntos — 8 por esquina L

| Tipo     | Color   | ColorIndex | Origen | Dirección | Distancia               | Condicional |
|----------|---------|-----------|--------|-----------|-------------------------|-------------|
| Interior | Azul    | 5         | —      | —         | Intersección interior   | Siempre     |
| Exterior | Rojo    | 1         | —      | —         | Intersección exterior   | Siempre     |
| Verde    | Verde   | 3         | ptAzul | innerH    | 300 mm                  | Siempre     |
| Amarillo | Amarillo| 2         | ptAzul | innerV    | 300 mm                  | Siempre     |
| Blanco   | Blanco  | 7         | ptRojo | outerH    | espV + 300              | Siempre     |
| Cian     | Cian    | 4         | ptRojo | outerV    | espH + 300              | Siempre     |
| Magenta  | Magenta | 6         | ptRojo | outerH    | mayor estándar < distBlanco | Solo si distBlanco no es estándar |
| Criss    | Gris    | 9         | ptRojo | outerV    | mayor estándar < distCian   | Solo si distCian no es estándar   |

---

## � Implementación realizada

### Archivos modificados

- `Desing/Services/LCornerDetector.cs`
- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs`

### Cambios en `LCornerDetector.cs`

**1. Constantes y helpers añadidos (líneas ~19-33):**

```csharp
private static readonly double[] MEDIDAS_PANEL = 
    { 300, 450, 600, 750, 1050, 1200, 1350, 1500, 1650, 1800, 2100 };

private bool EsMedidaEstandar(double dist)
{
    const double TOL = 1.0;
    return MEDIDAS_PANEL.Any(m => Math.Abs(dist - m) <= TOL);
}

private double MayorEstandarMenorQue(double dist)
{
    double resultado = -1;
    foreach (var m in MEDIDAS_PANEL)
        if (m < dist - 1.0) resultado = m;
    return resultado;
}
```

**2. `CalcularPuntosPanel` — nueva firma y lógica de remate (líneas ~869-923):**

```csharp
private (PuntoDTO Verde, PuntoDTO Amarillo, PuntoDTO Blanco, PuntoDTO Cian,
         PuntoDTO Magenta, PuntoDTO Criss) CalcularPuntosPanel(...)
{
    // ... cálculo existente de verde, amarillo, blanco, cian ...

    PuntoDTO magenta = null, criss = null;

    double distBlanco = espV + DIST;
    if (!EsMedidaEstandar(distBlanco))
    {
        double dMagenta = MayorEstandarMenorQue(distBlanco);
        if (dMagenta > 0) magenta = PuntoPolar(ptRojo.Value, outerH, dMagenta, "Magenta");
    }

    double distCian = espH + DIST;
    if (!EsMedidaEstandar(distCian))
    {
        double dCriss = MayorEstandarMenorQue(distCian);
        if (dCriss > 0) criss = PuntoPolar(ptRojo.Value, outerV, dCriss, "Criss");
    }

    return (verde, amarillo, blanco, cian, magenta, criss);
}
```

**3. Loop principal** — añadidas listas `todosPuntosMagenta` y `todosPuntosCriss` con sus `foreach` de salida.

### Cambios en `Commands.cs`

```csharp
else if (punto.TipoPunto == "Magenta") { circulo.ColorIndex = 6; }
else if (punto.TipoPunto == "Criss")   { circulo.ColorIndex = 9; }
```

---

## ✅ Resultado del test

- Compilación correcta sin errores
- Test en ZWCAD con espesor 250 mm: puntos Magenta y Criss aparecen en 450 mm desde ptRojo ✅
- Test con espesor 150 mm (distBlanco=450, estándar): puntos de remate no aparecen ✅

---

## 🔗 Contexto de trabajo

- **Stack**: C# .NET Framework 4.8, ZWCAD plugin
- **Git**: commit `feat: añadir puntos de remate Magenta/Criss AB#671`
- **Compilación**: Visual Studio → Build Solution → copiar DLL a carpeta plugins ZWCAD
- **Test**: `TANDEM_SELECCIONAR_LINEAS` en ZWCAD sobre geometría L
