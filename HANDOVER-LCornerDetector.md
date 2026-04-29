# HANDOVER — LCornerDetector (US-664)

> Documento para el siguiente agente. Última actualización: 2026-04-29

---

## 🎯 Contexto del proyecto

Plugin ZWCAD (.NET Framework 4.8) que detecta esquinas tipo L en un plano y dibuja círculos de referencia.
El usuario selecciona líneas en ZWCAD → el plugin llama a la API → el servicio calcula puntos → ZWCAD dibuja círculos.

**Stack:**
- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` — comando ZWCAD, dibuja los círculos
- `Desing/Services/LCornerDetector.cs` — toda la lógica de detección y cálculo
- `Desing/DTOs/` — PuntoDTO, LineaDTO, DeteccionEsquinasLDTO
- Azure DevOps: https://dev.azure.com/VSCAD/tandem2026 | Proceso: Agile

---

## ✅ Estado actual — Lo que está implementado y funcionando

### Puntos que se dibujan por esquina L (3 círculos)

| TipoPunto | Color ZWCAD | ColorIndex | Descripción |
|-----------|------------|------------|-------------|
| `"Interior"` | Azul | 5 | Intersección de las dos caras interiores del muro |
| `"Exterior"` | Rojo | 1 | Intersección de las dos caras exteriores del muro |
| `"Verde"` | Verde | 3 | 300 unidades desde el azul, hacia el interior del muro |

### Algoritmo de detección (LCornerDetector.cs)

1. **Agrupar líneas paralelas** → grupos por pendiente
2. **Buscar pares de grupos perpendiculares** → candidatos a panel
3. **Filtrar paneles válidos:**
   - `OFFSET_MINIMO_PANEL = 50` — rechaza colineales de distintas esquinas (dist ≈ 0)
   - `OFFSET_MAXIMO_PANEL = 1500` — rechaza líneas demasiado separadas
4. **Procesar una esquina a la vez** — `lineasUsadas` (HashSet) garantiza que cada línea se usa en UN SOLO panel
5. **Calcular 3 puntos** por panel: `CalcularPuntosEsquinaL` + `CalcularPuntoVerde`
6. **Deduplicar** con `EliminarPuntosDuplicados`

### Método clave: CalcularPuntoVerde (líneas 837-877)

```csharp
// Punto polar: desde ptAzul, dirección del extremo más lejano de innerVertical, distancia 300
// innerVertical = la línea interior con mayor componente vertical (o la que tenga más dy)
// El extremo MÁS LEJANO del azul = dirección hacia el interior del muro
double dx = refX - ptAzul.Value.X;
double dy = refY - ptAzul.Value.Y;
return new PuntoDTO {
    X = ptAzul.Value.X + (dx / dist) * 300.0,
    Y = ptAzul.Value.Y + (dy / dist) * 300.0,
    TipoPunto = "Verde"
};
```

---

## 🔧 Archivos clave

| Archivo | Líneas relevantes | Descripción |
|---------|------------------|-------------|
| `Desing/Services/LCornerDetector.cs` | 14-17 | Constantes TOLERANCIA, OFFSET_MINIMO/MAXIMO |
| `Desing/Services/LCornerDetector.cs` | 237-384 | Detección de grupos paralelos y paneles válidos |
| `Desing/Services/LCornerDetector.cs` | 355-435 | Loop principal: lineasUsadas + cálculo de los 3 puntos |
| `Desing/Services/LCornerDetector.cs` | 837-877 | CalcularPuntoVerde |
| `Desing/Services/LCornerDetector.cs` | 770-830 | CalcularPuntosEsquinaL (azul + rojo) |
| `TamdenZwcadPluging/ZwcadPlugin/Commands.cs` | 370-410 | Dibuja círculos con color por TipoPunto |

---

## 📋 Azure DevOps

- **US #664** "Create Faces on L-corner type" → `Resolved` (Ready to Present)
- **Task #665** CR → `Closed`
- **Task #666** Test → `Closed`
- **Último commit:** `8c41565` — fix: corregir calculo punto verde y eliminar paneles duplicados

### Cómo crear nueva US + Tasks

```powershell
.\Scripts\US.ps1 "Título de la US" "Descripción"
# Devuelve el ID → usar AB#<ID> en commits
```

### Cómo cerrar tasks vía API (proceso Agile)

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}
$body = '[{"op":"replace","path":"/fields/System.State","value":"Closed"}]'
Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/<ID>?api-version=7.0" -Headers $headers -Method Patch -Body $body
```

**Estados válidos (Agile):** `New` | `Active` | `Resolved` | `Closed`

---

## 🔨 Cómo compilar

Visual Studio: Build → Build Solution  
El DLL se copia manualmente a la carpeta de ZWCAD para probar.

---

## ⏭️ Próximos pasos

**A definir con el usuario** — hay más puntos/geometría a implementar para la esquina L.
El usuario los describirá al inicio de la siguiente sesión.

---

**Repo:** https://github.com/JuanGodoyLopez/Tandem-2026  
**Branch:** master
