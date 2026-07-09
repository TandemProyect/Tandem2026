# TandemXR — Extended Experience

**App instalable:** `TandemXR-Unity/` (Unity → Quest 3S + tablet Android)  
**Prototipo web (opcional):** `TandemXR/` (WebXR en navegador)  
**Arquitectura:** [ARQUITECTURA.md](./ARQUITECTURA.md)

---

## Propósito

Cliente XR de Tandem: interactuar con diseños de encofrado en **gafas VR** y **tablet**, conectado al servidor **Desing** (Visual Studio).

---

## Empezar (app instalable)

1. Instalar **Unity Hub** + **Unity 2022.3 LTS** + módulo **Android**
2. Abrir `C:\00_Tandem2026\TandemXR-Unity\` en Unity Hub
3. Arrancar **Desing** (Visual Studio F5)
4. Probar API: `https://localhost:44384/TandemXrApi/Manifest?designId=1&offerId=1`
5. Seguir [TandemXR-Unity/README.md](../../TandemXR-Unity/README.md)

---

## API Desing

`GET /TandemXrApi/Manifest?designId={id}&offerId={id}` → JSON con metadatos y URL STL.

Controlador: `Desing/Controllers/TandemXrApiController.cs`

---

## Roadmap

| Fase | Entregable |
|------|------------|
| v0 | API + Unity carga manifest (hecho) |
| v1 | STL real + lista instancias completa |
| v2 | Interacción XRI (agarrar, mover) en Quest |
| v3 | AR tablet (AR Foundation) |
| v4 | Passthrough MR en Quest 3S |
