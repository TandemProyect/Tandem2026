# TandemXR-Unity

Aplicación **instalable** para **Meta Quest 3S** y **tablet Android**.  
Interactúa con diseños Tandem vía API de Desing (Visual Studio).

---

## Requisitos (instalar una vez)

1. **[Unity Hub](https://unity.com/download)**
2. **Unity 2022.3 LTS** (Editor)
3. Módulo **Android Build Support** (en Hub → Installs → Add modules)
4. Cuenta desarrollador Meta (gratis) — [developer.oculus.com](https://developer.oculus.com/) — para instalar APK en Quest
5. **Visual Studio** o **VS Build Tools** — Unity lo usa para compilar C#

---

## Crear el proyecto Unity (primera vez)

### Opción A — Recomendada (plantilla Meta)

1. Unity Hub → **New project**
2. Elegir plantilla **Meta XR All-in-One SDK** (o **VR Core**)
3. Nombre: `TandemXR-Unity` (carpeta fuera o dentro del repo)
4. Copiar al proyecto los assets de este repo:
   ```
   C:\00_Tandem2026\TandemXR-Unity\Assets\TandemXR\  →  <tu proyecto>\Assets\TandemXR\
   ```

### Opción B — Abrir esta carpeta como proyecto

Si ya tienes Unity 2022.3, puedes abrir directamente `C:\00_Tandem2026\TandemXR-Unity` en Hub.  
Unity regenerará librerías al primer abrir (tarda varios minutos).

---

## Configuración

1. En Unity: **Edit → Project Settings → XR Plug-in Management → Android**
   - Activar **OpenXR**
   - En OpenXR: perfil **Meta Quest Touch Plus Controller**
2. Crear en escena un GameObject `TandemXR` con script `TandemXrBootstrap`
3. En el Inspector, componente `TandemServerSettings`:
   - **Server Base Url:** `https://localhost:44384` (o IP de tu PC en red local)
   - **Pairing Code:** el código del dispositivo en Intranet → Dispositivos XR
   - **Design Id / Offer Id:** IDs de respaldo si no hay envío pendiente
4. Arrancar **Desing** (F5 en Visual Studio) antes de probar la API

### Flujo «Enviar a XR»

1. En Desing: Configuración → Dispositivos XR → crear dispositivo → copiar código
2. Pegar el código en `TandemServerSettings.pairingCode`
3. Abrir un diseño en Desing → **Enviar a XR** → elegir dispositivo
4. Play en Unity (o APK en Quest): carga el envío pendiente y hace ACK

---

## Build APK — Quest 3S

1. **File → Build Settings**
2. Plataforma **Android** → Switch Platform
3. **Player Settings → Android:**
   - Minimum API Level: 29+
   - Target architectures: **ARM64**
4. **Player Settings → XR Plug-in Management:** OpenXR + Quest
5. Conectar Quest por USB (modo desarrollador activado)
6. **Build And Run** → se instala el APK en las gafas

Documentación Meta: [Unity Develop for Meta Quest](https://developers.meta.com/horizon/documentation/unity/unity-project-setup)

---

## Build APK — Tablet Android

Mismo proyecto, mismo build Android. En tablet:

- v0: visor 3D táctil (sin gafas)
- v1+: activar **AR Foundation** + ARCore en Project Settings

Instalar APK en tablet (USB o fichero `.apk`).

---

## Flujo de datos

```
Unity (TandemDesignApiClient)
    → GET /TandemXrApi/Manifest?designId=1
    ← JSON con instancias + URLs STL
Unity (TandemSceneLoader)
    → descarga STL, coloca en escena
Usuario
    → mandos (Quest) o touch (tablet)
```

---

## Estructura de scripts (`Assets/TandemXR/`)

| Script | Función |
|--------|---------|
| `TandemServerSettings` | URL servidor, IDs diseño |
| `TandemDesignApiClient` | Llama API Desing |
| `TandemSceneLoader` | Monta piezas en la escena |
| `TandemXrBootstrap` | Arranque: API → carga → modo VR |

---

## Ayuda

No hace falta experiencia previa en XR: cada fase tiene una prueba concreta (API responde → 1 pieza visible → interacción).  
Arquitectura completa: `Docs/Proyectos/TandemXR/ARQUITECTURA.md`
