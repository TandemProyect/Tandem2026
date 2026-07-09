# Tandem XR — Arquitectura (tablet Android + Meta Quest 3S)

**Última actualización:** 2026-07-08  
**Estado:** v0 — esqueleto proyecto + API Desing

---

## Objetivo

Una **misma aplicación Tandem XR** (proyecto Unity) que se instala en:

| Dispositivo | Modo | Interacción |
|-------------|------|-------------|
| **Meta Quest 3S** | VR inmersivo | Mandos / manos — recorrer y manipular el diseño |
| **Tablet Android** | Visor 3D + AR (fases) | Touch — orbitar; AR en obra con cámara (ARCore) |

El **cerebro** sigue en **Desing** (Visual Studio): geometría, listas de montaje, APIs.  
Unity es un **cliente más** (como ZWCAD), no sustituye al servidor.

---

## Diagrama

```
                    ┌──────────────────────────┐
                    │  Desing (Visual Studio)   │
                    │  • Lógica ATK60 / diseño  │
                    │  • API /TandemXrApi/...   │
                    └─────────────┬────────────┘
                                  │ HTTPS + JSON
          ┌───────────────────────┼───────────────────────┐
          ▼                       ▼                       ▼
   Desing_2 (web)          ZWCAD / CAD…          TandemXR-Unity
   Three.js                plugins               APK Quest + APK Tablet
```

---

## Stack

| Capa | Herramienta |
|------|-------------|
| Servidor | Visual Studio — `Desing/` (.NET 4.8 MVC) |
| Cliente XR | **Unity Hub** + Unity 2022.3 LTS (o 6 LTS) + **Meta XR SDK** |
| Tablet AR | Unity **AR Foundation** (ARCore) — mismo proyecto, otro build |
| Contrato datos | JSON `TandemXrDesignManifestDto` (ver `Desing/Models/TandemXr/`) |
| Catálogo piezas | STL en `~/Content/DesignTools/Stl/` (URLs absolutas en el manifest) |

**Visual Studio no genera el APK de las gafas.** Edita Desing y, si quieres, los scripts C# dentro de Unity. **Unity Editor** empaqueta la app.

---

## Proyecto Unity (`TandemXR-Unity/`)

### Creación (primera vez)

1. Instalar [Unity Hub](https://unity.com/download)
2. Instalar **Unity 2022.3 LTS** (recomendado para estabilidad Meta SDK)
3. En Hub → **New project** → plantilla **Meta XR All-in-One SDK** (o 3D URP + añadir Meta XR SDK)
4. Copiar la carpeta `Assets/TandemXR/` de este repositorio al proyecto Unity
5. Configurar `TandemServerSettings` (URL Desing, `designId` de prueba)

### Dos builds desde el mismo proyecto

| Build | Plataforma Unity | Destino |
|-------|------------------|---------|
| `TandemXR-Quest` | Android | Meta Quest 3S (APK sideload o Quest Store) |
| `TandemXR-Tablet` | Android | Tablet (APK Play Store / distribución interna) |

En Quest: escena VR (XR Origin, Interaction Toolkit).  
En tablet: cámara táctil + fase posterior AR Foundation.

### Interacción con diseños (roadmap)

| Fase | Quest 3S | Tablet |
|------|----------|--------|
| v0 | Cargar manifest + 1 STL de prueba | Igual + orbit touch |
| v1 | Lista completa de instancias (`ListRenderElement`) | Igual |
| v2 | Agarrar / mover / ocultar piezas | AR: colocar diseño en suelo |
| v3 | Passthrough MR (diseño sobre entorno real) | Medición en obra |

---

## API Desing

`GET /TandemXrApi/Manifest?designId={id}&offerId={id}`

Respuesta: `TandemXrDesignManifestDto` (metadatos + instancias + URLs STL).

Autenticación: en desarrollo `[AllowAnonymous]`; producción → token / `TSql_PluginDeviceAuth` (mismo patrón que plugin ZWCAD).

---

## Relación con otros clientes

| Cliente | Comparte con XR |
|---------|-----------------|
| ZWCAD | DTOs, pipeline 2D→3D, reglas de negocio en Desing |
| Desing_2 web | Misma escena / mismos STL; web no sustituye al APK |
| Revit / AutoCAD / BricsCAD | Futuros renderers del mismo API |

---

## Web (`TandemXR/` Vite + WebXR)

Prototipo rápido en navegador. **No reemplaza** la app Unity instalable.  
Útil para validar URL y escena antes de compilar APK.

---

## Documentos relacionados

- [README operativo Unity](../../../TandemXR-Unity/README.md)
- [Regla cliente-servidor](../../General/Arquitectura-Cliente-Servidor.md)
