# ⚡ CHEATSHEET - Azure DevOps (< 2 minutos)

## 🎯 LO ESENCIAL

**PAT Token:** `7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0`
**Base URL:** `https://dev.azure.com/VSCAD/tandem2026`
**Estados válidos:** `To Do`, `Doing`, `Done`

## ⚠️ OBLIGATORIO
**Toda US debe tener:** ✅ Tarea Test + 🔍 Tarea Code Review

---

## 🚀 COMANDOS RÁPIDOS

### Cambiar estado de US a Done
```powershell
powershell -ExecutionPolicy Bypass -Command "$PAT='7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0';$auth=[Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':$PAT'));$headers=@{Authorization='Basic $auth';'Content-Type'='application/json-patch+json'};$body='[{\"op\":\"replace\",\"path\":\"/fields/System.State\",\"value\":\"Done\"}]';Invoke-RestMethod -Uri 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0' -Headers $headers -Method Patch -Body $body|Out-Null;Write-Host '✅ Done' -F Green"
```
*Reemplaza `619` con tu US ID*

### Crear nueva US
```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Create-US-Fast.ps1" -Titulo "Tu título" -StoryPoints 8
```

### Crear tarea
```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Task.ps1" -ParentID 638 -Titulo "Tarea 1"
```

---

## 📋 SCRIPTS DISPONIBLES

| Script | Uso |
|--------|-----|
| `Create-US-Fast.ps1` | Crear US rápido |
| `Task.ps1` | Crear tarea individual |
| `Edit-US.ps1` | Modificar US existente |
| `Crear-US-ATK60.ps1` | Ejemplo US con HTML |
| `Crear-Tareas-US638.ps1` | Ejemplo múltiples tareas |

**Ubicación:** `C:\00_Tandem2026\Scripts\`

---

## 🔧 TEMPLATE MÚLTIPLES TAREAS

Crear `temp-tasks.ps1`:
```powershell
$PAT="7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth=[Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers=@{Authorization="Basic $auth";"Content-Type"="application/json-patch+json"}
$ParentID=638
$tasks=@(@{Titulo="Tarea 1";Descripcion="Desc 1"},@{Titulo="Tarea 2";Descripcion="Desc 2"})
foreach($t in $tasks){$payload=@(@{op="add";path="/fields/System.Title";value=$t.Titulo},@{op="add";path="/fields/System.WorkItemType";value="Task"},@{op="add";path="/fields/System.Description";value=$t.Descripcion},@{op="add";path="/relations/-";value=@{rel="System.LinkTypes.Hierarchy-Reverse";url="https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ParentID"}});$body=$payload|ConvertTo-Json -Depth 10;$result=Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0" -Headers $headers -Method Post -Body $body;Write-Host "✅ #$($result.id): $($t.Titulo)" -F Green}
```

Ejecutar:
```powershell
powershell -ExecutionPolicy Bypass -File "temp-tasks.ps1"
```

---

## ❌ ERRORES COMUNES

| Error | Solución |
|-------|----------|
| Script no firmado | Agregar `-ExecutionPolicy Bypass` |
| Estado inválido (400) | Usar `To Do`, `Doing`, `Done` |
| JSON inválido | Escapar comillas: `\"` |
| Token expirado | Regenerar PAT en Azure DevOps |

---

## 📊 TIEMPO ESPERADO

- Consultar: **5s**
- Actualizar: **10s**
- Crear US: **15s**
- 8 tareas: **20s**
- **Total: ~50s**

---

## 🔗 ENLACES

- **Board:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
- **Guía completa:** `GUIA-AGENTE-AZURE-DEVOPS.md`

---

**Regla:** Si toma > 2 min, lo estás haciendo mal. Revisa la guía completa.
