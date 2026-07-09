# Tandem Extended Experience (TandemXR)

Experiencia **XR** (realidad virtual, aumentada y mixta) para visualizar y validar soluciones de encofrado/andamio **antes de ejecutarlas en obra**, alineada con el enfoque de [PERI Extended Experience](https://www.peri.es/productos/peri-extended-experience.html).

## Qué ofrece (v0.1)

| Modo | Uso |
|------|-----|
| **Escritorio** | Orbitar el modelo 3D (Three.js) con rejilla de referencia |
| **VR** (`ENTER VR`) | Inmersión en escala real (Quest, PC VR con WebXR) |
| **AR** (`START AR`) | Colocar el STL sobre el suelo real (móvil/tablet con ARCore/ARKit) |

## Requisitos

- Node.js 18+
- **HTTPS** en desarrollo (Vite + `@vitejs/plugin-basic-ssl`)
- Desing en marcha si quieres cargar STL desde la intranet (`https://localhost:44384`)

## Arranque rápido

```bash
cd TandemXR
npm install
npm run dev
```

Abre `https://localhost:5173` (acepta el certificado de desarrollo).

### Cargar un STL de Desing

Con Desing (IIS Express) activo:

```
https://localhost:5173/?stl=/Content/DesignTools/Stl/ATK60/3120270090P.stl
```

El proxy de Vite reenvía `/desing-stl/*` → `https://localhost:44384/*`.

## Estructura

```
TandemXR/
├── index.html          # Shell UI
├── vite.config.js      # HTTPS dev + proxy Desing
├── src/
│   ├── main.js
│   ├── tandem-xr-app.js  # Escena, WebXR, STLLoader
│   └── ui.css
└── package.json
```

## Roadmap (referencia PERI)

1. **v0.1** — WebXR VR/AR + STL demo (este scaffold)
2. **v0.2** — Enlace desde `Desing_2/Viewer` → «Abrir en XR» con `offerId` / `designId`
3. **v0.3** — Montaje completo de diseño (múltiples STL / JSON de escena Desing)
4. **v0.4** — App nativa (Capacitor) para App Store / Play Store
5. **v0.5** — Detección de interferencias y mediciones en AR

## Documentación

Ver `Docs/Proyectos/TandemXR/README.md` en el repositorio raíz.

## Build producción

```bash
npm run build
npm run preview
```

Los artefactos quedan en `dist/`. Se pueden servir desde IIS, Azure Static Web Apps o embeber en Desing MVC.
