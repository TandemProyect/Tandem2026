# TandemXR-Unity

Aplicación **instalable** para **Meta Quest 3S** y **Xiaomi Pad 6**.  
Habla con Desing por API (`/TandemXrApi/...`).

**Editor:** Unity **2022.3.62f3** (o 2022.3 LTS cercana) + módulo **Android**.

---

## Abrir el proyecto (primera vez)

1. En **Unity Hub** → licencia **Personal** activada (si pide login, hazlo).
2. **Proyectos** → **Importar** / **Add** → carpeta:
   ```
   C:\00_Tandem2026\TandemXR-Unity
   ```
3. Ábrelo con **2022.3.62f3**. La primera vez tarda (paquetes). No cierres.
4. En el menú de Unity: **TandemXR → Setup escena principal**
5. En el Project, abre `Assets/TandemXR/Settings/TandemServerSettings`
   - **Server Base Url:** `https://localhost:44384` (en dispositivo real: IP del PC)
   - **Pairing Code:** el de Intranet → Dispositivos XR
6. Arranca **Desing** (F5) → en Unity pulsa **Play**

Consola esperada: `[TandemXR] Diseño: ...` o un envío pendiente.

> Si Hub dice «no se encontraron proyectos», comprueba que entras en `TandemXR-Unity` (no en la raíz del repo).  
> Si Unity en línea de comandos falla con «No valid license», activa la licencia en el Hub (modo gráfico); no hace falta batchmode.

---

## Flujo «Enviar a XR»

1. Desing → Configuración → Dispositivos XR → crear dispositivo → copiar código  
2. Pegar en `TandemServerSettings.pairingCode`  
3. Diseño 3D → **Enviar a XR** → elegir dispositivo  
4. Play en Unity (o APK): carga el pendiente y hace ACK  

---

## Build APK (después de Play en Editor)

1. **File → Build Settings** → **Android** → Switch Platform  
2. Player Settings: Minimum API **29+**, arquitectura **ARM64**, package `com.tandem.xr`  
3. XR Plug-in Management (Android): **OpenXR**  
4. **Build And Run** con tablet/Quest en USB (modo desarrollador)

Quest: tienda Meta más adelante. Tablet: Play Store más adelante. Prototipo = APK a mano.

---

## Scripts (`Assets/TandemXR/`)

| Script | Función |
|--------|---------|
| `TandemServerSettings` | URL, pairingCode, IDs |
| `TandemDesignApiClient` | Manifest / Pending / Ack |
| `TandemSceneLoader` | Monta escena (v0: placeholder) |
| `TandemXrBootstrap` | Arranque + sondeo pendientes |
| `Editor/TandemXrProjectSetup` | Menú Setup escena |

Docs: `Docs/Proyectos/TandemXR/`
