# Tandem XR — Instalación software (fase 0)

**Objetivo mañana:** dejar listo PC + Unity + conexión con Desing.  
**Presupuesto software:** 0 € (todo gratuito). Hardware (Quest) aparte.

---

## Antes de empezar (5 min)

- [ ] PC con **Windows 10/11**, admin local
- [ ] **~40 GB** libres en disco
- [ ] Conexión a internet estable
- [ ] Anotar ruta del repo: `C:\00_Tandem2026`

---

## 1. Visual Studio Community (si no está)

1. [Descargar VS Community](https://visualstudio.microsoft.com/es/vs/community/)
2. En el instalador, marcar:
   - **Desarrollo de ASP.NET y web**
   - **Desarrollo de .NET desktop** (opcional, útil)
3. Instalar y reiniciar si pide
4. Comprobar: abrir `C:\00_Tandem2026\Design.sln` → F5 → Desing arranca en `https://localhost:44384`

---

## 2. Unity Hub + Unity Editor

1. [Descargar Unity Hub](https://unity.com/download)
2. Crear cuenta Unity (gratis) e iniciar sesión en Hub
3. **Installs → Install Editor**
   - Versión: **2022.3 LTS** (ej. 2022.3.62f1)
   - Módulos a marcar:
     - [x] **Android Build Support**
     - [x] Android SDK & NDK Tools
     - [x] OpenJDK
4. Esperar descarga (puede tardar 30–60 min)

---

## 3. Abrir proyecto Tandem XR

1. Unity Hub → **Open** → `C:\00_Tandem2026\TandemXR-Unity`
2. Primera apertura: Unity resuelve paquetes (Meta XR SDK, etc.) — **no interrumpir**
3. Si falla versión de paquetes: Hub → plantilla **Meta XR All-in-One** y copiar `Assets/TandemXR/` del repo

---

## 4. Cuenta desarrollador Meta (gratis)

1. [developer.oculus.com](https://developer.oculus.com/) → registro
2. Crear **organización** (nombre Tandem / tu empresa)
3. Más adelante (con Quest): activar **modo desarrollador** en la app Meta Quest del móvil

---

## 5. Node.js (opcional — solo prototipo web)

1. [nodejs.org](https://nodejs.org/) → LTS
2. Solo si queréis probar `C:\00_Tandem2026\TandemXR` (WebXR en navegador)
3. No obligatorio para la app Unity

---

## 6. Comprobar API Desing

Con Desing en marcha (F5):

```
https://localhost:44384/TandemXrApi/Manifest?designId=1&offerId=1
```

Debe devolver JSON con `"exito": true` y un `manifest`.

---

## 7. Unity — primera escena (tras instalar)

1. Crear escena `TandemXR_Main`
2. Añadir **XR Origin** (menú Meta XR o GameObject → XR)
3. GameObject vacío `TandemApp` + script `TandemXrBootstrap`
4. Crear asset: clic derecho → Create → **TandemXR → Server Settings**
   - `serverBaseUrl`: `https://localhost:44384` (en Quest luego: IP del PC)
   - `designId` / `offerId`: 1
5. Asignar settings en `TandemXrBootstrap`
6. Play en Editor → consola debe mostrar `[TandemXR] Diseño: ...`

---

## Orden recomendado mañana

| Paso | Tiempo aprox. |
|------|----------------|
| VS Community (si falta) | 20–40 min |
| Unity Hub + Editor + Android | 45–90 min |
| Abrir `TandemXR-Unity` | 15–30 min |
| Cuenta Meta | 10 min |
| Probar API + Play en Unity | 20 min |

**Total:** ~2–3 h la primera vez (sobre todo descargas).

---

## Problemas frecuentes

| Síntoma | Solución |
|---------|----------|
| Unity no abre el proyecto | Instalar exactamente **2022.3 LTS** |
| Paquetes Meta fallan | Usar plantilla Meta XR en Hub y copiar `Assets/TandemXR` |
| API no responde | Desing debe estar en F5; probar URL en navegador |
| HTTPS / certificado | Normal en local; Unity usa `TandemDevCertificateHandler` solo en dev |

---

## Después del software (cuando haya presupuesto)

- Comprar **Meta Quest 3S** (~350 €)
- Build APK e instalar en gafas (guía en `TandemXR-Unity/README.md`)

---

## Referencias

- [ARQUITECTURA.md](./ARQUITECTURA.md)
- [TandemXR-Unity/README.md](../../TandemXR-Unity/README.md)
