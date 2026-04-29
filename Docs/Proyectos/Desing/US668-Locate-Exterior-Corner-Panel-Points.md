# US-668: Locate Exterior Corner Panel Points

**US:** #668 - Locate exterior corner panel points  
**Fecha:** 2026-04-29  
**Estado:** ✅ Implementado - Pendiente de test

---

## Resumen

Se calculan y dibujan **6 puntos de referencia** por cada esquina en L detectada.  
Cada punto se representa como un círculo en ZWCAD con color diferenciado.

---

## Puntos implementados

| Punto    | ColorIndex ZWCAD | Origen  | Dirección             | Distancia           |
|----------|------------------|---------|----------------------|---------------------|
| Azul     | 5                | —       | Intersección caras interiores | —           |
| Rojo     | 1                | —       | Intersección caras exteriores | —           |
| Verde    | 3                | ptAzul  | Brazo interior HORIZONTAL    | 300 mm      |
| Amarillo | 2                | ptAzul  | Brazo interior VERTICAL      | 300 mm      |
| Blanco   | 7                | ptRojo  | Cara exterior HORIZONTAL     | espV + 300 mm |
| Cian     | 4                | ptRojo  | Cara exterior VERTICAL       | espH + 300 mm |

> **espV** = espesor del muro vertical (distancia entre las dos líneas paralelas verticales)  
> **espH** = espesor del muro horizontal (distancia entre las dos líneas paralelas horizontales)

---

## Archivos modificados

### `Desing/Services/LCornerDetector.cs`

**Método `ExpandirPolilineas(List<LineaDTO>)`** *(nuevo)*  
Convierte cada polilínea L en sus segmentos `Line` individuales antes de la detección.  
Esto permite que el algoritmo de grupos paralelos trabaje sobre segmentos rectos.

**Método `CalcularPuntosPanel(l1a, l1b, l2a, l2b)`** *(nuevo, reemplaza `CalcularPuntoVerde`)*  
Devuelve tupla `(Verde, Amarillo, Blanco, Cian)`:
1. Identifica línea interior/exterior de cada grupo por distancia al centroide del otro grupo.
2. Determina qué grupo es horizontal y cuál vertical (`g1EsH`).
3. Calcula `espV` y `espH` (distancias entre paralelas de cada grupo).
4. Llama a `PuntoPolar()` para cada uno de los 4 puntos.

**Método `PuntoPolar(ptBase, linea, distancia, tipo)`** *(nuevo)*  
Calcula un punto a `distancia` unidades desde `ptBase` en la dirección del extremo más lejano de `linea`.

**Loop principal `DetectarEsquinasL`**  
- Añade listas `todosPuntosAmarillo`, `todosPuntosBlanco`, `todosPuntosCian`.
- Sustituye la llamada a `CalcularPuntoVerde` por `CalcularPuntosPanel`.
- Agrega los 4 nuevos grupos al `resultado.PuntosADibujar`.

### `TamdenZwcadPluging/ZwcadPlugin/Commands.cs`

Añade `else if` para los tipos `"Amarillo"`, `"Blanco"` y `"Cian"` con sus `ColorIndex` correspondientes.

---

## Lógica geométrica

```
Ejemplo con muro de 300mm:

ptAzul = (300, 300)   ← intersección caras interiores
ptRojo = (0, 0)       ← intersección caras exteriores

Verde    = (600, 300)   ptAzul + 300mm →
Amarillo = (300, 600)   ptAzul + 300mm ↑
Blanco   = (600, 0)     ptRojo + (300+300)mm →  (espV=300)
Cian     = (0, 600)     ptRojo + (300+300)mm ↑  (espH=300)
```

---

## Notas de compilación

1. Compilar `Desing` (servidor MVC) — rebuild del proyecto web.
2. Compilar `ZwcadPlugin` — copiar DLL a la carpeta de plugins de ZWCAD.
3. Recargar ZWCAD y ejecutar `TANDEM_SELECCIONAR_LINEAS` sobre geometría con esquinas en L.
