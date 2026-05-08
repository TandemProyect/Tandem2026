# Handover Tecnico - Flujo Imagen a Muros (ZWCAD)

## Objetivo funcional de la US

Partiendo de una imagen de esquema de muros:

1. Detectar lineas de muro.
2. Leer anotaciones de espesor y altura (`E/e`, `H/h`).
3. Transformar la geometria para alimentar el flujo comun de deteccion.
4. Reutilizar el pipeline existente para esquinas/muros.

> Regla de negocio acordada con usuario:
> - La cota exterior es referencia de dimension principal (ej: `10000 x 5000`).
> - El espesor (`E=0,30`) debe aplicarse hacia adentro para construir cara interior.

---

## Archivos tocados en esta iteracion

### Backend (servidor)

- `Desing/Services/ImageAnalysisService.cs`
- `Desing/Controllers/DesignToolsAutocadController.cs`

### Cliente ZWCAD (sin cambio estructural fuerte en esta iteracion)

- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` (usa flujo existente `TANDEM_ANALIZAR_IMAGEN`)
- `TamdenZwcadPluging/ZwcadPlugin/MVCApiService.cs` (llamada a endpoint imagen)

---

## Cambios implementados

## 1) Parsing de datos en imagen (E/H)

En `ImageAnalysisService.cs`:

- Se amplio el prompt para extraer:
  - `espesorMuro` desde `E/e` (ej: `E 0,30`, `e=0.30`).
  - `alturaMuro` desde `H/h` (ej: `H 2,70`, `h=2.30`).
- Se acepta separador decimal con coma o punto.
- Se robustecio parseo con helper para extraer valor numerico cuando el modelo devuelva texto mixto.

Resultado esperado del JSON de IA:

- `escala`
- `espesorMuro`
- `alturaMuro`
- `lineas[]`

---

## 2) Validaciones funcionales en endpoint de imagen

En `DesignToolsAutocadController.cs`, metodo `DetectarEsquinasImagen`:

- Si no hay lineas detectadas: error guiado.
- Si falta `espesorMuro`: error guiado (bloqueante).
- Si falta `alturaMuro`: no bloquea, usa default 2.70m y agrega aviso.

---

## 3) Transformacion geometrica para pipeline comun

Se agrego logica para convertir las lineas detectadas en caras de muro:

- Funcion principal: `ExpandirLineasCentroACaras(...)`.
- Modelo de trabajo: `OffsetLineWork`.
- Criterio actual:
  - Cara exterior mantiene referencia.
  - Cara interior se desplaza por espesor hacia el interior (segun centroide).

Para cierre en esquinas:

- `AjustarEncuentrosInteriores(...)`
- matching de extremos cercanos con tolerancia.
- interseccion de rectas para unir cara interior en encuentros.

---

## 4) Control de residuos numericos

Se incorporo `SnapLinea(...)` y `Snap(...)` para mitigar residuos de coma flotante.

- Ajuste actual: snap de coordenadas a milimetro entero (`Math.Round(value, 0)`).

Motivo: eliminar desviaciones tipo `9700.08`, `5300.06` derivadas de intersecciones.

---

## 5) Integracion con el flujo comun

Una vez armadas las caras de muro:

- Se invoca `LCornerDetector.DetectarEsquinasL(...)`.
- Se reutiliza el pipeline existente de puntos/polilineas a dibujar.
- Se mantiene comportamiento de `DibujarResultado(...)` en plugin.

---

## Estado actual observado

Se logro:

- Detectar lineas desde imagen.
- Leer `E/H`.
- Dibujar geometria exterior/interior.
- Ejecutar detector comun sobre resultado.

Pendiente fino:

- Asegurar consistencia dimensional en todos los casos (sin deriva en encuentros).
- Validar de forma determinista que, para un caso nominal:
  - exterior = `10000 x 5000`
  - interior = `9700 x 4700` (con `E=300`).

---

## Criterio de aceptacion recomendado para cerrar definitivamente

Caso base rectangular:

- Entrada:
  - 4 lineas formando rectangulo.
  - `E=0,30`
  - `H=2,70`

- Esperado:
  1. Exterior preserva cota principal (`10000`, `5000`).
  2. Interior respeta `-2E` por dimension global (`9700`, `4700`).
  3. Esquinas cerradas sin chaflan no deseado.
  4. Sin residuos decimales en cotas finales.

---

## Riesgos tecnicos para siguiente agente

1. **Ambiguedad de referencia geometrica**
   - El modelo de IA puede devolver eje o cara.
   - Confirmar contrato explicito: "linea detectada representa cara exterior".

2. **Cierre por interseccion**
   - La interseccion infinita puede desplazar extremos fuera de tolerancia visual.
   - Si reaparece deriva, aplicar estrategia ortogonal determinista en casos H/V.

3. **Tolerancias**
   - `TOLERANCIA_ENCUENTRO_MM` influye en emparejamientos incorrectos.
   - Ajustar con dataset real.

4. **Dependencia del OCR semantico**
   - Si no se detecta `E`, hoy el flujo bloquea (correcto por regla de negocio).
   - Revisar UX de mensaje si se integra en UI de produccion.

---

## Propuesta para siguiente iteracion (si hay tiempo)

1. Agregar modo "determinista ortogonal":
   - Si lineas son H/V (con tolerancia angular), resolver interior por caja exacta.
2. Mantener modo geometrico general para rotaciones arbitrarias.
3. Registrar en log:
   - bbox exterior/interior,
   - espesor calculado,
   - diferencia contra cotas objetivo.
4. Agregar test de regresion de dimensiones.

---

## Resumen ejecutivo para continuidad

- Ya existe flujo usable de imagen -> geometria -> detector comun.
- La captura de `E/H` esta incorporada y operativa.
- Queda una capa de estabilizacion dimensional para garantizar cotas exactas en todos los casos.
- Base de codigo lista para que otro agente continúe sin reabrir decisiones de arquitectura.

