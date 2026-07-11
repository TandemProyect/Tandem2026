# Tandem — Encofrado AT-60 (documento base)

Última actualización: 2026-07-10

Documento de referencia para el módulo de **encofrados** en Tandem (aplicación `Desing`). Resume el contexto del producto, el catálogo del sistema por defecto **AT-60**, el estado del visor de muros 3D y bocetos para seguir documentando uniones (L, T, +).

> Documento técnico detallado de uniones T y cruce: [`HANDOVER-MUROS-T-CRUCE.md`](HANDOVER-MUROS-T-CRUCE.md)

---

## 1. Contexto del producto

**Tandem** es un proyecto de **encofrado y andamios**. En esta fase el equipo está arrancando por el bloque de **encofrados**: a partir de la geometría de muros en planta (líneas, caras, uniones) se debe resolver qué **paneles** y **piezas** del sistema seleccionado cubren cada tramo y cada nudo.

| Concepto | Descripción |
|----------|-------------|
| **Muro (modelo)** | Geometría arquitectónica: eje, caras, espesor E, altura H, uniones L / T / + |
| **Sistema de encofrado** | Catálogo de paneles, esquinas y reglas de montaje; cambia la solución 3D |
| **Sistema por defecto** | **AT-60** (en UI Desing_2: `Atk-60`; en código legacy: `ATK60`) |
| **Andamios** | Fuera de alcance de este documento (fase posterior) |

La representación 3D del muro en el visor puede ser **genérica** (extrusión de hormigón) o **sustituida** por piezas STL del sistema cuando el resolver de encofrado esté conectado (`03-sistemas-de-encofrado.md`).

---

## 2. Sistema AT-60 — catálogo de paneles rectos

### 2.1 Módulos estándar (longitud en planta)

El AT-60 dispone de paneles rectos con estas longitudes **nominales** (metros):

| Código uso | Longitud (m) | Longitud (mm) |
|------------|--------------|---------------|
| P30 | **0,30** | 300 |
| P45 | **0,45** | 450 |
| P60 | **0,60** | 600 |
| P75 | **0,75** | 750 |
| P90 | **0,90** | 900 |

### 2.2 Patrón de módulo: paso 0,15 m

Todas las longitudes del catálogo siguen la progresión aritmética con **incremento 0,15 m**:

```
0,30 → 0,45 → 0,60 → 0,75 → 0,90 → 1,05 → 1,20 → 1,35 → …
```

**Regla de composición:** cualquier longitud de muro encofrable se descompone en **suma de paneles del catálogo** (y, si hace falta, un **resto** también alineado al paso de 0,15 m).

Ejemplos:

| Longitud objetivo | Composición típica | Notas |
|-------------------|-------------------|--------|
| 0,90 m | 1 × P90 | Panel único |
| 1,05 m | P45 + P60 | 0,45 + 0,60 = 1,05 (siguiente tras 0,90) |
| 1,20 m | P30 + P90 o P60 + P60 | Varias combinaciones válidas |
| 2,70 m | 3 × P90 | Coincide con altura de panel estándar en legacy (2700 mm) |

> **Implementación futura:** algoritmo de «corte en paneles» debe respetar el paso 0,15 m y priorizar combinaciones con menos piezas / menos cortes (criterio a fijar).

### 2.3 Panel de esquina (único)

| Pieza | Longitud en planta (cada pata de la L) | Notas |
|-------|----------------------------------------|--------|
| **Esquina AT-60** | **0,30 m** (300 mm) | Pieza **única** del sistema para esquina en L; no se sustituye por dos rectos |

En el repositorio legacy existen STL de esquina (`PanelE1200`, `PanelE2400`, `PanelE2700`, etc.) según altura de encofrado; la **huella en planta** de la esquina mantiene el módulo **0,30 m** en cada dirección.

### 2.4 Parámetros habituales del muro (independientes del panel)

| Parámetro | Símbolo | Default habitual | Unidad en visor |
|-----------|---------|------------------|-----------------|
| Espesor muro | E | 0,30 | m (mm en escena) |
| Altura muro | H | 2,70 | m (2700 mm extrusión 3D) |

El espesor E no coincide necesariamente con la longitud del panel recto (0,30 m es tanto un panel como un espesor típico de muro).

---

## 3. Estado del visor Desing_2 (muros 3D)

Resumen extraído del trabajo previo en muros; el detalle de uniones T/+ está en el handover enlazado.

| Elemento | 2D (ejes + caras) | 3D extruido | Encofrado AT-60 (forma blanca cliente) |
|----------|-------------------|-------------|----------------------------------------|
| Muro recto | ✅ | ✅ 4 vértices (API) | 🔲 descomposición P30…P90 en eje |
| Esquina L | ✅ | ✅ (cliente AT-60, 6v) | ✅ huella + meta (`ma-stl-atk60-formwork.js`) |
| Unión T | ✅ caras recortadas | ✅ (cliente AT-60, 8v) | ✅ huella + meta |
| Cruce + | ✅ caras recortadas | ❌ | ❌ pendiente |

**Implementación técnica:** [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md)

**Flujo actual 3D:**

1. Usuario dibuja muros 2D (líneas fuente persisten; atributos en `maStlUserPlanLine`).
2. Modo Muro 2D/3D: ocultar líneas, borrar mallas generadas, regenerar.
3. `POST` → `ProcesarLineasZwcad` → `LCornerDetector` (solo **rectos** 4v en cliente; esquinas API ≥6v se omiten).
4. Cliente: `maStlAtk60RenderJunctionFootprints*` en nodos L/T desde ejes fuente + espesor por eje.
5. Extrusión 3D: rectos API + huellas AT-60 (`ExtrudeGeometry`, H default 2700 mm).

**Archivos clave:** `master-article-details-stl-viewer.js`, `LCornerDetector.cs`, `DesignToolsAutocadController.cs`.

---

## 4. Bocetos de referencia (seguir documentando)

Los siguientes esquemas fijan convenciones para ampliar la documentación con diagramas CAD/imagen en `IA/Examples/`.

### 4.1 Convenciones del boceto

```
Planta Desing_2:
  +X → derecha
  −Z → «arriba» en papel (convención visor)
  Cota exterior = línea de referencia del perímetro
  Espesor E hacia interior del recinto
  Unidades: metros en documentación; mm en JSON/API (×1000)
```

### 4.2 Muro recto — descomposición AT-60

Caso: muro **2,50 m** de longitud exterior, E = 0,30 m, H = 2,70 m.

```
  Exterior (cota)
  ┌──────────────────────────────────────────┐
  │ P90      P90      P60      P10*           │
  │ [0,90]   [0,90]   [0,60]   resto 0,10?   │  ← revisar resto con paso 0,15
  └──────────────────────────────────────────┘
  Interior
        ←──────────── 2,50 m ──────────────→

  * Resto 2,50 − 2,40 = 0,10 m → NO alineado a 0,15; documentar regla de
    «último panel recortado» o ajuste de cota (pendiente definir con negocio).
```

**Composición objetivo 2,50 m (ejemplo a validar):** P90 + P90 + P60 + **ajuste 0,10** — el equipo debe cerrar si el resto admite cantos 0,15 o solo múltiplos desde 0,30.

### 4.3 Esquina L — panel esquina 0,30

```
        Exterior
          │
          │  P90 recto
          │
    ──────┼──────
          │\
          │ \  Esquina AT-60 (0,30 × 0,30 en planta)
          │  \
          │   P60 recto
          Exterior
```

- Nudo **L:** una pieza de esquina (0,30 m en cada pata) + paneles rectos en cada alzado.
- En 3D genérico: polígono de **6 vértices** (ver `HANDOVER-MUROS-T-CRUCE.md` §3.2).

### 4.4 Unión T — boceto (pendiente cerrar)

```
              │ rama (ej. P60)
              │
  ────────────┼────────────  atravesado (ej. P90 + P90 + …)
              │
```

**Datos por rellenar** (plantilla):

| Campo | Valor |
|-------|-------|
| Espesor atravesado E₁ | ___ m |
| Espesor rama E₂ | ___ m |
| ¿Pieza especial T en AT-60? | sí / no / panel recortado |
| Composición paneles en rama | ___ |
| Composición en atravesado (lados) | ___ |
| Imagen | `IA/Examples/…/t-90-at60.png` |

### 4.5 Cruce + — boceto (pendiente cerrar)

```
              │ brazo N
              │
  ────────────┼────────────  brazo W — cruce — brazo E
              │
              │ brazo S
```

**Datos por rellenar:**

| Campo | Valor |
|-------|-------|
| Espesor único E | ___ m |
| ¿Pieza central + o solo rectos? | ___ |
| Composición por brazo | ___ |
| Imagen | `IA/Examples/…/cross-at60.png` |

---

## 5. Relación muro ↔ encofrado AT-60 (visión objetivo)

```
Planta muros (Desing_2)
       │
       ▼
  Topología: segmentos + nodos (L / T / Cross / libre)
       │
       ▼
  Sistema seleccionado (default AT-60)
       │
       ├── Tramo recto → lista [P90, P60, …] + resto
       ├── Nudo L      → 1 × Esquina 0,30 + rectos
       ├── Nudo T      → (por definir)
       └── Nudo +      → (por definir)
       │
       ▼
  Geometría 3D: STL ATK60 o extrusión provisional
```

Interfaz prevista: `IEncofradoSystem` en `03-sistemas-de-encofrado.md` (`ResolveWallSegment`, `ResolveCornerL`, `ResolveJunctionT`, `ResolveCross`).

---

## 6. Próximos pasos de documentación

1. **Validar regla de restos:** si la longitud no es múltiplo de 0,15 m, ¿panel recortado, ajuste de cota o pieza especial?
2. **Caso numérico AT-60** por tipo de unión (recto, L, T, +) con tabla panel-a-panel.
3. **Subir ejemplos** a `IA/Examples/` siguiendo `IA/Examples/README.md` (casos 01–05).
4. **Enlazar** piezas STL reales (`Content/DesignTools/Stl/ATK60/`) con códigos P30…P90 y esquina.

---

## 7. Referencias cruzadas

| Documento | Contenido |
|-----------|-----------|
| [`HANDOVER-MUROS-T-CRUCE.md`](HANDOVER-MUROS-T-CRUCE.md) | Lógica 2D/3D uniones T y +, `WallConnections.json` |
| [`HANDOVER-US-IMG-MUROS.md`](HANDOVER-US-IMG-MUROS.md) | Muros rectos, tipos 1–4, imagen/CAD |
| [`HANDOVER-DESING2-MUROS-ARRANQUE.md`](HANDOVER-DESING2-MUROS-ARRANQUE.md) | Visor, Ortho F8, arranque local |
| [`SISTEMA-ATK60.md`](SISTEMA-ATK60.md) | Detección esquinas L (plugin ZWCAD) |
| [`ENCOFRADO-AT60-IMPLEMENTACION.md`](ENCOFRADO-AT60-IMPLEMENTACION.md) | Ciclo líneas/mallas, código L/T cliente, pruebas, extensibilidad |
| [`03-sistemas-de-encofrado.md`](03-sistemas-de-encofrado.md) | Interfaz `IEncofradoSystem` |

---

## Changelog

| Fecha | Cambio |
|-------|--------|
| 2026-07-10 | Estado visor: L/T AT-60 en cliente; enlace a IMPLEMENTACION |
| 2026-07-10 | Creación: contexto Tandem encofrado, catálogo AT-60 (0,15 m), esquina 0,30, bocetos L/T/+ |
