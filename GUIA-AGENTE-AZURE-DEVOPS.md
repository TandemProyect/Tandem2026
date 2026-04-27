# 🤖 GUÍA PARA AGENTES - Gestión Azure DevOps

## 📋 INFORMACIÓN CRÍTICA - LEER PRIMERO

Esta guía permite a cualquier agente gestionar User Stories y tareas en Azure DevOps de forma rápida y eficiente (< 2 minutos).

---

## ⚡ QUICK START - 3 PASOS

### 1️⃣ Cerrar una User Story

```powershell
# Estados válidos: "To Do", "Doing", "Done"
powershell -ExecutionPolicy Bypass -Command "
$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0';
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':$PAT'));
$headers = @{Authorization = 'Basic $auth'; 'Content-Type' = 'application/json-patch+json'};
$body = '[{\"op\":\"replace\",\"path\":\"/fields/System.State\",\"value\":\"Done\"}]';
$url = 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0';
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body;
Write-Host \"✅ US #619 marcada como Done\" -ForegroundColor Green
"
```

### 2️⃣ Crear una nueva User Story

```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Create-US-Fast.ps1" `
  -Titulo "Tu título aquí" `
  -Descripcion "Descripción HTML aquí" `
  -StoryPoints 8
```

### 3️⃣ Crear tareas para una US

```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Task.ps1" `
  -ParentID 638 `
  -Titulo "Nombre de la tarea" `
  -Descripcion "Descripción opcional" `
  -Estado "To Do"
```

---

## 🔧 SCRIPTS DISPONIBLES

Todos los scripts están en: `C:\00_Tandem2026\Scripts\`

### Script: Create-US-Fast.ps1
**Propósito:** Crear User Stories rápidamente

**Parámetros:**
- `Titulo` (requerido): Título de la US
- `Descripcion` (opcional): Descripción en HTML
- `StoryPoints` (opcional): Puntos de historia (número)

**Ejemplo:**
```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Create-US-Fast.ps1" `
  -Titulo "Implementar validación de datos" `
  -Descripcion "<p>Validar todos los inputs del formulario</p>" `
  -StoryPoints 5
```

**Salida:** Retorna el ID de la US creada

---

### Script: Task.ps1
**Propósito:** Crear tareas vinculadas a una US

**Parámetros:**
- `ParentID` (requerido): ID de la US padre
- `Titulo` (requerido): Título de la tarea
- `Descripcion` (opcional): Descripción de la tarea
- `Estado` (opcional): "To Do" (default), "Doing", "Done"

**Ejemplo:**
```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Task.ps1" `
  -ParentID 638 `
  -Titulo "Diseñar base de datos" `
  -Descripcion "Crear diagrama ER y scripts SQL" `
  -Estado "To Do"
```

---

### Script: Edit-US.ps1
**Propósito:** Modificar una US existente

**Parámetros:**
- `ID` (requerido): ID del work item
- `Titulo` (opcional): Nuevo título
- `Descripcion` (opcional): Nueva descripción
- `Estado` (opcional): "To Do", "Doing", "Done"
- `Prioridad` (opcional): "1", "2", "3", "4"
- `AsignadoA` (opcional): Email del asignado

**Ejemplo:**
```powershell
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Edit-US.ps1" `
  -ID 619 `
  -Estado "Done"
```

---

## 📊 INFORMACIÓN DE CONFIGURACIÓN

### Azure DevOps
- **Organización:** VSCAD
- **Proyecto:** tandem2026
- **PAT Token:** `7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0`
- **URL Base:** `https://dev.azure.com/VSCAD/tandem2026`
- **API Version:** 7.0

### Estados Válidos (Work Item Type: Issue)
- **To Do** - Trabajo pendiente
- **Doing** - En progreso
- **Done** - Completado

### Prioridades Válidas
- **1** - Crítico
- **2** - Alto
- **3** - Medio
- **4** - Bajo

---

## 🚀 WORKFLOWS COMUNES

### Workflow 1: Cerrar US y crear nueva

```powershell
# 1. Cerrar US actual
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Edit-US.ps1" -ID 619 -Estado "Done"

# 2. Crear nueva US
$newUSId = powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Create-US-Fast.ps1" `
  -Titulo "Nueva funcionalidad" `
  -StoryPoints 8

# 3. Crear tareas para la nueva US
powershell -ExecutionPolicy Bypass -File "C:\00_Tandem2026\Scripts\Task.ps1" `
  -ParentID $newUSId `
  -Titulo "Tarea 1"
```

### Workflow 2: Crear múltiples tareas

**Opción A: Script personalizado (RECOMENDADO)**

Crear archivo temporal `crear-tareas-temp.ps1`:

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}
$ParentID = 638

$tasks = @(
	@{Titulo = "Tarea 1"; Descripcion = "Desc 1"},
	@{Titulo = "Tarea 2"; Descripcion = "Desc 2"},
	@{Titulo = "Tarea 3"; Descripcion = "Desc 3"}
)

foreach ($task in $tasks) {
	$payload = @(
		@{op = "add"; path = "/fields/System.Title"; value = $task.Titulo},
		@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"},
		@{op = "add"; path = "/fields/System.Description"; value = $task.Descripcion},
		@{op = "add"; path = "/relations/-"; value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$ParentID"
		}}
	)
	$body = $payload | ConvertTo-Json -Depth 10
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
	Write-Host "✅ Task #$($result.id): $($task.Titulo)" -ForegroundColor Green
}
```

Ejecutar:
```powershell
powershell -ExecutionPolicy Bypass -File "crear-tareas-temp.ps1"
```

---

## 🎯 API REST DIRECTA

### Consultar una US

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
Write-Host "Estado: $($result.fields.'System.State')"
Write-Host "Título: $($result.fields.'System.Title')"
```

### Actualizar estado de una US

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}
$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body
Write-Host "✅ Estado actualizado a: $($result.fields.'System.State')" -ForegroundColor Green
```

---

## 🔍 TROUBLESHOOTING

### Error: "No se puede cargar el archivo... no está firmado digitalmente"
**Solución:** Usar siempre `-ExecutionPolicy Bypass`
```powershell
powershell -ExecutionPolicy Bypass -File "script.ps1"
```

### Error: "Error en el servidor remoto: (400) Solicitud incorrecta"
**Causa:** Estado inválido (ej. "Closed" en lugar de "Done")
**Solución:** Verificar estados válidos: "To Do", "Doing", "Done"

### Error: "You must pass a valid patch document"
**Causa:** JSON mal formateado
**Solución:** Usar comillas escapadas correctamente `\"`

### Script se ejecuta pero no actualiza
**Verificación:**
```powershell
# Ver estado actual
powershell -ExecutionPolicy Bypass -Command "
$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0';
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':$PAT'));
$headers = @{Authorization = 'Basic \$auth'};
$url = 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0';
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Get;
Write-Host \"Estado: \$(\$result.fields.'System.State')\"
"
```

---

## 📝 TEMPLATE PARA SCRIPT RÁPIDO

**Archivo:** `Scripts\quick-action.ps1`

```powershell
# Template rápido para acciones en Azure DevOps
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# ===== CONFIGURA AQUÍ =====
$workItemId = 619
$action = "replace"  # "add" o "replace"
$field = "/fields/System.State"
$value = "Done"
# ==========================

$body = "[{`"op`":`"$action`",`"path`":`"$field`",`"value`":`"$value`"}]"
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$workItemId?api-version=7.0"

try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body
	Write-Host "✅ Actualizado: $field = $value" -ForegroundColor Green
} catch {
	Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}
```

---

## 🎓 EJEMPLOS REALES USADOS

### Ejemplo 1: Cerrar US-619
```powershell
powershell -ExecutionPolicy Bypass -Command "
$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0';
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':$PAT'));
$headers = @{Authorization = 'Basic $auth'; 'Content-Type' = 'application/json-patch+json'};
$body = '[{\"op\":\"replace\",\"path\":\"/fields/System.State\",\"value\":\"Done\"}]';
$url = 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/619?api-version=7.0';
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body;
Write-Host \"✅ US #619 marcada como Done\" -ForegroundColor Green
"
```
**Tiempo:** < 5 segundos

### Ejemplo 2: Crear US-638 con 8 story points
Ver: `Scripts\Crear-US-ATK60.ps1`
**Tiempo:** < 10 segundos

### Ejemplo 3: Crear 8 tareas para US-638
Ver: `Scripts\Crear-Tareas-US638.ps1`
**Tiempo:** < 15 segundos

---

## 📚 REFERENCIAS

### Documentación Oficial
- **Azure DevOps REST API:** https://learn.microsoft.com/en-us/rest/api/azure/devops/
- **Work Items API:** https://learn.microsoft.com/en-us/rest/api/azure/devops/wit/work-items

### URLs del Proyecto
- **Board:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
- **Work Items:** https://dev.azure.com/VSCAD/tandem2026/_workitems

### Archivos del Proyecto
- **Scripts:** `C:\00_Tandem2026\Scripts\`
- **Documentación US:** `C:\00_Tandem2026\AGENTE-US*-INFO.md`
- **Esta guía:** `C:\00_Tandem2026\GUIA-AGENTE-AZURE-DEVOPS.md`

---

## ⚡ CHECKLIST RÁPIDO PARA AGENTES

Cuando necesites trabajar con Azure DevOps:

- [ ] ¿Necesitas consultar una US? → Usa API REST directa
- [ ] ¿Necesitas cambiar estado? → Usa `Edit-US.ps1` o API directa
- [ ] ¿Necesitas crear nueva US? → Usa `Create-US-Fast.ps1`
- [ ] ¿Necesitas crear tareas? → Crea script temporal con loop
- [ ] ¿Son más de 3 tareas? → Usa script personalizado (ver Workflow 2)
- [ ] ¿Problema con scripts? → Verifica `-ExecutionPolicy Bypass`
- [ ] ¿Error 400? → Verifica estados válidos (To Do/Doing/Done)

---

## 🎯 REGLA DE ORO

**Si la operación toma más de 2 minutos, estás haciendo algo mal.**

### Tiempo esperado por operación:
- Consultar US: **5 segundos**
- Actualizar estado: **10 segundos**
- Crear US: **15 segundos**
- Crear 8 tareas: **20 segundos**
- **TOTAL workflow completo: ~50 segundos**

---

## 📞 AYUDA ADICIONAL

Si encuentras problemas:
1. Revisa la sección de **TROUBLESHOOTING**
2. Verifica que el PAT token no haya expirado
3. Confirma que los estados sean válidos
4. Usa comandos inline con `-Command` en lugar de `-File` para debug rápido

---

**Última actualización:** 2025-01-20
**Versión:** 1.0
**Autor:** Agente AI (documentado para futuros agentes)
