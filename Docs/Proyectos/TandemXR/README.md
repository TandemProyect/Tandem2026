# TandemXR — Extended Experience

**App instalable:** `TandemXR-Unity/` (Unity → Quest 3S + tablet Android)  
**Prototipo web (opcional):** `TandemXR/` (WebXR en navegador)  
**Arquitectura:** [ARQUITECTURA.md](./ARQUITECTURA.md)

---

## Propósito

Cliente XR de Tandem: interactuar con diseños de encofrado en **gafas VR** y **tablet**, conectado al servidor **Desing** (Visual Studio).

---

## Empezar (app instalable)

1. Instalar **Unity Hub** + **Unity 2022.3.62f3** (LTS) + módulo **Android**
2. Activar licencia **Personal** en el Hub
3. **Importar** `C:\00_Tandem2026\TandemXR-Unity\` (ya es proyecto reconocible)
4. En Unity: menú **TandemXR → Setup escena principal**
5. Arrancar **Desing** (Visual Studio F5)
6. Probar API: `https://localhost:44384/TandemXrApi/Manifest?designId=1&offerId=1`
7. Detalle operativo: [TandemXR-Unity/README.md](../../TandemXR-Unity/README.md)

---

## API Desing

`GET /TandemXrApi/Manifest?designId={id}&offerId={id}` → JSON con metadatos y URL STL.

Controlador: `Desing/Controllers/TandemXrApiController.cs`

---

## Roadmap (base de producto)

| Fase | Entregable | Estado |
|------|------------|--------|
| v0 | API + Unity carga manifest | Hecho (esqueleto) |
| v0.5 | Dispositivos XR + Enviar a XR + Pending/Ack | Hecho (Desing); Unity pendiente de licencia/Play |
| v1 | Manifest completo (piezas ATK-60) + Play en Editor / APK | Pendiente |
| v2 | APK tablet + Quest; tiendas más adelante | Pendiente |
| v3 | Marca / suelo: AR tablet + passthrough Quest 3S | Pendiente |
| v4 | Interacción XRI (agarrar, mover, ocultar) | Pendiente |
| v5 | API de comandos en Desing (crear muro, etc.) | Pendiente |
| v6 | Voz en Quest → comando → Desing → refrescar XR | Pendiente (futuro) |

**Voz:** las gafas pueden oír órdenes («muro 10 m × 0,30 × 3 m»). Unity hace STT e intención; **Desing** crea el muro con las mismas reglas que el diseñador; XR solo pinta el resultado. No se construye encofrado “a ojo” en las gafas.

**Informe:** quincenal **02**, apartado **1. Proyecto XR** (`Desing/Report/2026/2026-10-22_Informe-Quincenal.md` / `.pdf`).

---

## Dispositivos XR y «Enviar a XR»

1. Ejecutar scripts:
   - `Desing/Scripts/TemporalScript/2026-07-29_create_TSql_XrDevice.sql`
   - `Desing/Scripts/TemporalScript/2026-07-29_create_TSql_XrPushJob.sql`
   - `Desing/Scripts/TemporalScript/2026-07-29_seed_TSql_UiTranslation_Common_menu_XrDevices.sql`
2. Intranet → **Configuración → Dispositivos XR**: crear gafas/tablet y copiar el **código de emparejamiento**.
3. En el diseñador 3D: botón **Enviar a XR** → elegir dispositivo.
4. En Unity (`TandemServerSettings`): pegar `pairingCode` + URL del servidor. La app consulta `/TandemXrApi/Pending` y carga el diseño.
