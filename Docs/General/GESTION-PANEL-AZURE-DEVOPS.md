# 🎯 Guía Completa: Gestión del Panel Azure DevOps

> **Proyecto:** Tandem 2026  
> **Última actualización:** 2026-04-24  
> **Propósito:** Documentación centralizada para crear, editar y gestionar User Stories y Tasks en Azure DevOps

---

## ⚠️ IMPORTANTE: Configuración Correcta

### 🔴 ERROR COMÚN QUE DEBES EVITAR

**❌ INCORRECTO:**
```powershell
$org = "juangodoylopez"           # ❌ ORGANIZACIÓN INCORRECTA
$project = "Tandem 2026"          # ❌ NOMBRE INCORRECTO
$projectEncoded = "Tandem%202026" # ❌ ENCODING INCORRECTO
$PAT = "pknrhdrnq4wlkjrbnfykqhnnkqcn4f72h7ukb6f7g3ezrp3cg7ha"  # ❌ PAT INCORRECTO
```

**✅ CORRECTO:**
```powershell
$org = "VSCAD"                    # ✅ ORGANIZACIÓN CORRECTA
$project = "tandem2026"           # ✅ NOMBRE EN MINÚSCULAS
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
```

### 📋 Configuración de Referencia

**SIEMPRE usa estos valores:**

| Parámetro | Valor Correcto |
|-----------|----------------|
| **Organización** | `VSCAD` |
| **Proyecto** | `tandem2026` |
| **PAT** | Ver en `Scripts/US.ps1` línea 10 |
| **URL Base** | `https://dev.azure.com/VSCAD/tandem2026` |
| **Panel** | https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues |

**⚠️ Si la API devuelve error 404, verifica primero estos valores.**

---

## ⚡ Comandos Rápidos Más Usados

### 🚀 Crear US con Story Points (< 20 segundos)
```powershell
cd C:\00_Tandem2026\Scripts
.\Create-US-Fast.ps1 -Titulo "Tu título aquí" -Descripcion "Descripción detallada" -StoryPoints 8
```

### ✅ Mover US a Done
```powershell
cd C:\00_Tandem2026\Scripts
.\Edit-US.ps1 -ID 637 -Estado "Done"
```

### 📎 Adjuntar Documentación a US
```powershell
# 1. Subir archivo
$PAT = $env:AZURE_DEVOPS_PAT
$headers = @{Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT")))";}
$uploadUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/attachments?fileName=US-637-DOCS.md&api-version=7.0"
$uploadResult = Invoke-RestMethod -Uri $uploadUrl -Headers $headers -Method Post -InFile "TuArchivo.md" -ContentType "application/octet-stream"

# 2. Vincular a US
$headers["Content-Type"] = "application/json-patch+json"
$patchUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/637?api-version=7.0"
$body = @(@{op="add"; path="/relations/-"; value=@{rel="AttachedFile"; url=$uploadResult.url; attributes=@{comment="Documentación generada"}}}) | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri $patchUrl -Headers $headers -Method Patch -Body $body
```

---

## 📚 Índice Rápido

1. [Crear User Stories](#1-crear-user-stories)
2. [Crear Tasks (Tareas)](#2-crear-tasks-tareas)
3. [Editar User Stories](#3-editar-user-stories)
4. [Editar Tasks](#4-editar-tasks)
5. [Verificar Relaciones](#5-verificar-relaciones-us--tasks)
6. [Vincular Commits](#6-vincular-commits-a-user-stories)
7. [Scripts de Referencia](#7-scripts-de-referencia-completos)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. 🎯 Crear User Stories

### ⚡ Método 1: Script Rápido con Story Points (RECOMENDADO - <20 segundos)

**Ubicación:** `C:\00_Tandem2026\Scripts\Create-US-Fast.ps1`

Este script es la forma más rápida y confiable de crear User Stories con Story Points.

#### Uso Básico:
```powershell
cd C:\00_Tandem2026\Scripts

# US simple sin descripción ni puntos
.\Create-US-Fast.ps1 -Titulo "Implementar validación de formularios"

# US con descripción
.\Create-US-Fast.ps1 -Titulo "Detectar esquinas tipo L" -Descripcion "Implementar algoritmo para detectar esquinas tipo L en geometrías seleccionadas"

# US con descripción y Story Points (⭐ USO RECOMENDADO)
.\Create-US-Fast.ps1 -Titulo "Detectar esquinas tipo L" -Descripcion "Implementar algoritmo para detectar esquinas tipo L en geometrías seleccionadas. El sistema debe identificar esquinas formadas por dos líneas perpendiculares y proporcionar 8 puntos de referencia." -StoryPoints 8
```

#### Ejemplos Rápidos:
```powershell
# US de 3 puntos
.\Create-US-Fast.ps1 -Titulo "Agregar botón de exportar" -StoryPoints 3

# US de 5 puntos con descripción
.\Create-US-Fast.ps1 -Titulo "Implementar filtros avanzados" -Descripcion "Filtros por fecha, usuario y estado" -StoryPoints 5

# US de 8 puntos (compleja)
.\Create-US-Fast.ps1 -Titulo "Integración con API externa" -Descripcion "Conectar con API de terceros para sincronización" -StoryPoints 8
```

**Resultado esperado:**
```
US #637 creada con 8 puntos
https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/637
```

**✅ Ventajas:**
- ⚡ Ejecución en menos de 20 segundos
- 🎯 Soporte para Story Points
- 🔧 Encoding UTF-8 correcto (resuelve errores de Azure DevOps)
- 🌐 Abre automáticamente el navegador con la US creada

---

### Método 2: Script US.ps1 (Alternativo)

**Ubicación del script:** `C:\00_Tandem2026\Scripts\US.ps1`

#### Crear US con título:
```powershell
cd C:\00_Tandem2026\Scripts
.\US.ps1 "Implementar validación de formularios"
```

#### Crear US con título y descripción:
```powershell
.\US.ps1 "Implementar validación" "Validar todos los campos del formulario de registro"
```

#### Ejemplo completo:
```powershell
# Navegar a Scripts
cd C:\00_Tandem2026\Scripts

# Crear varias User Stories
.\US.ps1 "Agregar filtros a la tabla de usuarios"
.\US.ps1 "Implementar export CSV" "Permitir exportar datos a CSV"
.\US.ps1 "Mejorar UI del dashboard" "Actualizar diseño según mockups"
```

**Resultado esperado:**
```
✓ US #637 creada: Agregar filtros a la tabla de usuarios
✓ US #638 creada: Implementar export CSV
✓ US #639 creada: Mejorar UI del dashboard
```

### Método 3: API REST Directa

```powershell
# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# Datos de la US
$titulo = "Implementar autenticación OAuth"
$descripcion = "Agregar soporte para login con Google y Microsoft"

# Crear payload
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $titulo}
	@{op = "add"; path = "/fields/System.Description"; value = $descripcion}
	@{op = "add"; path = "/fields/Microsoft.VSTS.Scheduling.StoryPoints"; value = 5}
) | ConvertTo-Json -Depth 10

# Crear User Story con encoding UTF-8
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body ([System.Text.Encoding]::UTF8.GetBytes($payload)) -ContentType "application/json-patch+json; charset=utf-8"

Write-Host "✓ US #$($result.id) creada: $titulo" -ForegroundColor Green
```

### Método 4: Interfaz Web

1. Ir al panel: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
2. Clic en **"+ New Work Item"**
3. Seleccionar **"Issue"**
4. Completar título y descripción
5. Guardar (Ctrl+S)

---

## 2. 📋 Crear Tasks (Tareas)

### ⚡ Método Rápido: Script de 3 Tasks Estándar

**Uso más común:** Crear Develop, Test y CR para una US

```powershell
# ⚠️ CAMBIAR $usId por el ID de tu User Story
$usId = 613  # <-- ID de la User Story

# Configuración CORRECTA
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# Tasks estándar
$tasks = @("Develop", "Test", "CR")

Write-Host "Creando tareas para US #$usId..." -ForegroundColor Cyan

foreach ($taskType in $tasks) {
	# Crear payload (JSON en string para evitar problemas de encoding)
	$payload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'

	# URL correcta
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"

	# Crear Task
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

	Write-Host "✓ Task $taskType creada: #$($result.id)" -ForegroundColor Green
}
```

**Resultado esperado:**
```
Creando tareas para US #613...
✓ Task Develop creada: #634
✓ Task Test creada: #635
✓ Task CR creada: #636
```

### Método Alternativo: Script Task.ps1

**Ubicación:** `C:\00_Tandem2026\Scripts\Task.ps1`

```powershell
cd C:\00_Tandem2026\Scripts

# Crear Task vinculada a US #613
.\Task.ps1 613 "Develop"
.\Task.ps1 613 "Test" "Validar funcionalidad en ZWCAD"
.\Task.ps1 613 "CR" "Code review de Commands.cs"

# Con estado inicial
.\Task.ps1 613 "Develop" -Estado "Doing"
```

### Crear Task Individual con Descripción

```powershell
# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# Datos
$usId = 613
$titulo = "Implementar validación de entrada"
$descripcion = "Validar que los parámetros del comando sean correctos antes de procesar"

# Crear payload con descripción
$payload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $titulo}
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Task"}
	@{op = "add"; path = "/fields/System.Description"; value = $descripcion}
	@{
		op = "add"
		path = "/relations/-"
		value = @{
			rel = "System.LinkTypes.Hierarchy-Reverse"
			url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId"
		}
	}
) | ConvertTo-Json -Depth 10

# Crear Task
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

Write-Host "✓ Task #$($result.id) creada y vinculada a US #$usId" -ForegroundColor Green
```

---

## 3. ✏️ Editar User Stories

### Script Edit-US.ps1

**Ubicación:** `C:\00_Tandem2026\Scripts\Edit-US.ps1`

#### Cambiar estado:
```powershell
cd C:\00_Tandem2026\Scripts

# Marcar como "En progreso"
.\Edit-US.ps1 613 -Estado "Doing"

# Marcar como completada
.\Edit-US.ps1 613 -Estado "Done"
```

#### Cambiar título:
```powershell
.\Edit-US.ps1 613 -Titulo "Nuevo título de la US"
```

#### Cambiar prioridad:
```powershell
.\Edit-US.ps1 613 -Prioridad 1  # Alta
.\Edit-US.ps1 613 -Prioridad 2  # Media-Alta
.\Edit-US.ps1 613 -Prioridad 3  # Media
.\Edit-US.ps1 613 -Prioridad 4  # Baja
```

#### Asignar a usuario:
```powershell
.\Edit-US.ps1 613 -AsignadoA "juan@example.com"
```

#### Múltiples cambios:
```powershell
.\Edit-US.ps1 613 -Titulo "Nuevo título" -Estado "Doing" -Prioridad 1
```

### API REST Directa

```powershell
# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json; charset=utf-8"
}

# Cambiar estado a "Done"
$usId = 613
$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId?api-version=7.0"

$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body

Write-Host "✓ US #$usId actualizada" -ForegroundColor Green
```

---

## 4. 🔄 Editar Tasks

### Cambiar estado de una Task:

```powershell
# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json; charset=utf-8"
}

# Marcar Task #634 como "Doing"
$taskId = 634
$body = '[{"op":"replace","path":"/fields/System.State","value":"Doing"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"

Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body

Write-Host "✓ Task #$taskId marcada como 'Doing'" -ForegroundColor Green
```

### Estados válidos para Tasks:

| Estado | Cuándo usarlo |
|--------|---------------|
| `"To Do"` | Pendiente de iniciar |
| `"Doing"` | En progreso activo |
| `"Done"` | Completada |

### Marcar múltiples Tasks como Done:

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json; charset=utf-8"
}

# IDs de las tasks a completar
$taskIds = @(634, 635, 636)

foreach ($taskId in $taskIds) {
	$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
	$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"

	Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body

	Write-Host "✓ Task #$taskId marcada como Done" -ForegroundColor Green
}
```

---

## 5. 🔍 Verificar Relaciones (US → Tasks)

### Script para listar Tasks de una US:

```powershell
# Configuración
$usId = 613  # ⚠️ Cambiar por el ID de tu US
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

# Obtener US con relaciones
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/${usId}?`$expand=relations&api-version=7.0"
$us = Invoke-RestMethod -Uri $url -Headers $headers

Write-Host "`nUS #${usId}: $($us.fields.'System.Title')" -ForegroundColor Cyan
Write-Host "Estado: $($us.fields.'System.State')" -ForegroundColor Yellow

if ($us.relations) {
	$children = $us.relations | Where-Object { $_.attributes.name -eq "Child" }

	if ($children) {
		Write-Host "`n✓ Tasks vinculadas ($($children.Count)):" -ForegroundColor Green

		foreach ($child in $children) {
			$taskId = $child.url.Split('/')[-1]
			$task = Invoke-RestMethod -Uri $child.url -Headers $headers

			$estado = $task.fields.'System.State'
			$color = switch ($estado) {
				"To Do" { "Gray" }
				"Doing" { "Yellow" }
				"Done" { "Green" }
				default { "White" }
			}

			Write-Host "  #${taskId} - $($task.fields.'System.Title')" -ForegroundColor $color -NoNewline
			Write-Host " [$estado]" -ForegroundColor $color
		}
	} else {
		Write-Host "`n❌ No hay tasks vinculadas" -ForegroundColor Red
	}
} else {
	Write-Host "`n❌ No hay relaciones en esta US" -ForegroundColor Red
}
```

**Ejemplo de salida:**
```
US #613: CR
Estado: To Do

✓ Tasks vinculadas (3):
  #634 - Develop [To Do]
  #635 - Test [To Do]
  #636 - CR [To Do]
```

---

## 6. 🔗 Vincular Commits a User Stories

### Formato Correcto de Commits:

```bash
git commit -m "tipo: descripción AB#<ID>"
```

### Ejemplos:

```bash
# Solo vincular
git commit -m "feat: Implementar comando TANDEM_SELECCIONAR_LINEAS AB#613"
git commit -m "fix: Corregir validación de entrada AB#613"
git commit -m "docs: Actualizar README con ejemplos AB#613"

# Vincular y cerrar automáticamente al hacer merge
git commit -m "feat: Completar funcionalidad Fixes AB#613"
git commit -m "fix: Resolver bug crítico Closes AB#613"
```

### Tipos de Commit:

| Tipo | Uso |
|------|-----|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `docs` | Cambios en documentación |
| `style` | Formato, espacios, etc. |
| `refactor` | Refactorización |
| `test` | Agregar/modificar tests |
| `chore` | Tareas de mantenimiento |

### Palabras Clave Especiales:

| Palabra | Efecto |
|---------|--------|
| `AB#613` | Solo vincula |
| `Fixes AB#613` | Marca como "Resolved" al merge |
| `Closes AB#613` | Marca como "Closed" al merge |
| `Resolves AB#613` | Marca como "Resolved" al merge |

---

## 7. 📜 Scripts de Referencia Completos

### Script 1: Crear US + 3 Tasks en un solo comando

**Guardar como:** `Scripts\Crear-US-Completa.ps1`

```powershell
param(
	[Parameter(Mandatory=$true)]
	[string]$Titulo,

	[Parameter(Mandatory=$false)]
	[string]$Descripcion = ""
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json"
}

# Crear User Story
Write-Host "Creando User Story: $Titulo" -ForegroundColor Cyan

$usPayload = @(
	@{op = "add"; path = "/fields/System.Title"; value = $Titulo}
	@{op = "add"; path = "/fields/System.WorkItemType"; value = "Issue"}
)

if ($Descripcion) {
	$usPayload += @{op = "add"; path = "/fields/System.Description"; value = $Descripcion}
}

$usBody = $usPayload | ConvertTo-Json -Depth 10
$usUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Issue?api-version=7.0"
$us = Invoke-RestMethod -Uri $usUrl -Headers $headers -Method Post -Body $usBody

$usId = $us.id
Write-Host "✓ US #$usId creada" -ForegroundColor Green

# Crear Tasks
Write-Host "`nCreando Tasks para US #$usId..." -ForegroundColor Cyan

$tasks = @("Develop", "Test", "CR")

foreach ($taskType in $tasks) {
	$taskPayload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'

	$taskUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
	$task = Invoke-RestMethod -Uri $taskUrl -Headers $headers -Method Post -Body $taskPayload

	Write-Host "✓ Task $taskType creada: #$($task.id)" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✓ US #$usId lista con 3 tasks" -ForegroundColor Green
Write-Host "URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/$usId" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
```

**Uso:**
```powershell
.\Crear-US-Completa.ps1 "Implementar nueva funcionalidad"
.\Crear-US-Completa.ps1 "Agregar filtros" "Permitir filtrar por fecha y usuario"
```

### Script 2: Completar todas las Tasks de una US

**Guardar como:** `Scripts\Completar-Tasks-US.ps1`

```powershell
param(
	[Parameter(Mandatory=$true)]
	[int]$UsId
)

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json; charset=utf-8"
}

# Obtener Tasks de la US
$getHeaders = @{Authorization = "Basic $auth"}
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/${UsId}?`$expand=relations&api-version=7.0"
$us = Invoke-RestMethod -Uri $url -Headers $getHeaders

Write-Host "US #${UsId}: $($us.fields.'System.Title')" -ForegroundColor Cyan

if ($us.relations) {
	$children = $us.relations | Where-Object { $_.attributes.name -eq "Child" }

	if ($children) {
		Write-Host "`nCompletando $($children.Count) tasks..." -ForegroundColor Yellow

		foreach ($child in $children) {
			$taskId = $child.url.Split('/')[-1]
			$task = Invoke-RestMethod -Uri $child.url -Headers $getHeaders

			$estado = $task.fields.'System.State'

			if ($estado -ne "Done") {
				$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
				$updateUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$taskId?api-version=7.0"

				Invoke-RestMethod -Uri $updateUrl -Headers $headers -Method Patch -Body $body

				Write-Host "✓ Task #$taskId - $($task.fields.'System.Title') marcada como Done" -ForegroundColor Green
			} else {
				Write-Host "  Task #$taskId - $($task.fields.'System.Title') ya estaba Done" -ForegroundColor Gray
			}
		}

		Write-Host "`n✓ Todas las tasks completadas" -ForegroundColor Green
	} else {
		Write-Host "`n❌ No hay tasks vinculadas a esta US" -ForegroundColor Red
	}
} else {
	Write-Host "`n❌ Esta US no tiene relaciones" -ForegroundColor Red
}
```

**Uso:**
```powershell
.\Completar-Tasks-US.ps1 613
```

---

## 8. 🛠️ Troubleshooting

### Problema 1: Error 404 al crear Tasks

**Síntoma:**
```
✗ Error: Error en el servidor remoto: (404) No se encontró.
```

**Causa:**
- Organización incorrecta
- Nombre del proyecto incorrecto
- Encoding incorrecto de la URL

**Solución:**
```powershell
# ❌ INCORRECTO
$org = "juangodoylopez"
$project = "Tandem 2026"

# ✅ CORRECTO
$org = "VSCAD"
$project = "tandem2026"
```

### Problema 2: Error de autenticación (401)

**Síntoma:**
```
Error: No autorizado (401)
```

**Causa:**
- PAT incorrecto o expirado

**Solución:**
1. Verificar el PAT en `Scripts/US.ps1` línea 10
2. Usar exactamente ese PAT:
```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
```

### Problema 3: Task no se vincula a la US

**Síntoma:**
- La Task se crea pero aparece suelta, no vinculada

**Causa:**
- URL del parent incorrecta
- Rel type incorrecto

**Solución:**
```powershell
# ✅ CORRECTO
@{
	op = "add"
	path = "/relations/-"
	value = @{
		rel = "System.LinkTypes.Hierarchy-Reverse"  # ⚠️ IMPORTANTE
		url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId"
	}
}
```

### Problema 4: Respuesta encriptada/corrupta

**Síntoma:**
- El output de `Invoke-RestMethod` parece basura o texto encriptado

**Causa:**
- Problema de encoding o compresión en la respuesta

**Solución:**
```powershell
# Agregar manejo de encoding
$headers = @{
	Authorization = "Basic $auth"
	"Content-Type" = "application/json-patch+json; charset=utf-8"
	"Accept-Encoding" = "gzip, deflate"
}

# Usar try-catch
try {
	$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload
	Write-Host "✓ Éxito: #$($result.id)" -ForegroundColor Green
} catch {
	Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
	Write-Host "URL: $url" -ForegroundColor Yellow
}
```

### Problema 5: No encuentro el ID de mi US

**Solución: Listar últimas User Stories**

```powershell
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

$wiql = @{
	query = "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.WorkItemType] = 'Issue' ORDER BY [System.Id] DESC"
} | ConvertTo-Json

$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/wiql?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $wiql -ContentType "application/json"

Write-Host "`nÚltimas 10 User Stories:" -ForegroundColor Cyan

$result.workItems | Select-Object -First 10 | ForEach-Object {
	$wiUrl = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$($_.id)?api-version=7.0"
	$wi = Invoke-RestMethod -Uri $wiUrl -Headers $headers

	Write-Host "#$($wi.id) - $($wi.fields.'System.Title') [$($wi.fields.'System.State')]" -ForegroundColor Yellow
}
```

---

## 📊 Resumen de Comandos Más Usados

### Crear US + Tasks (flujo completo):

```powershell
# 1. Crear User Story
cd C:\00_Tandem2026\Scripts
.\US.ps1 "Título de la US"

# 2. Anotar el ID que devuelve (ej: #640)

# 3. Crear Tasks estándar (copiar y ejecutar, cambiando $usId)
$usId = 640; $PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"; $auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT")); $headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}; $tasks = @("Develop", "Test", "CR"); foreach ($taskType in $tasks) { $payload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'; $url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"; $result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload; Write-Host "✓ Task $taskType creada: #$($result.id)" -ForegroundColor Green }
```

### Completar US:

```powershell
# 1. Completar todas las tasks
cd C:\00_Tandem2026\Scripts
.\Completar-Tasks-US.ps1 640

# 2. Marcar US como Done
.\Edit-US.ps1 640 -Estado "Done"

# 3. **🆕 Adjuntar documentación** (si el agente generó documentación)
.\Attach-Document.ps1 -WorkItemId 640 -FilePath "C:\ruta\al\documento.md" -Comment "Documentación completa de implementación"

# 4. Commit final
git add .
git commit -m "feat: Completar funcionalidad Closes AB#640"
git push origin master
```

---

## 📎 Adjuntar Documentación a Work Items

### ⚠️ IMPORTANTE: Documentación del Agente

**Cuando GitHub Copilot o cualquier agente de IA trabaja en una User Story y genera documentación técnica (instrucciones, soluciones, troubleshooting), SIEMPRE se debe adjuntar al Work Item antes de moverlo a "Done".**

### ¿Por qué adjuntar documentación?

1. **✅ Trazabilidad completa:** Toda la información del trabajo realizado queda vinculada al Work Item
2. **✅ Facilita mantenimiento futuro:** Otros desarrolladores pueden entender qué se hizo y por qué
3. **✅ Documentación del contexto:** Problemas encontrados, soluciones aplicadas, decisiones técnicas
4. **✅ Base de conocimiento:** Construir una biblioteca de soluciones reutilizables

### Script: Attach-Document.ps1

**Ubicación:** `C:\00_Tandem2026\Scripts\Attach-Document.ps1`

**Uso básico:**
```powershell
cd C:\00_Tandem2026\Scripts

# Adjuntar un archivo a un Work Item
.\Attach-Document.ps1 -WorkItemId 613 -FilePath "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\US-613-INSTRUCCIONES.md" -Comment "Documentación completa de implementación"
```

**Parámetros:**
- `-WorkItemId`: ID del Work Item (US o Task)
- `-FilePath`: Ruta completa al archivo a adjuntar
- `-Comment`: Comentario descriptivo del archivo (opcional)

**Tipos de documentos a adjuntar:**
- 📘 Instrucciones de implementación (`US-XXX-INSTRUCCIONES.md`)
- 🔍 Investigaciones técnicas (`INVESTIGACION-XXX.md`)
- 📊 Resúmenes de cambios (`RESUMEN_CAMBIOS_XXX.md`)
- ⚠️ Solución de problemas (`TROUBLESHOOTING-XXX.md`)
- 📸 Capturas de pantalla de pruebas
- 📄 Diagramas de arquitectura

### Ejemplo completo: Flujo con documentación

```powershell
# Ejemplo real de US-613: Seleccionar líneas en ZWCAD

# 1. Desarrollar la funcionalidad
# (GitHub Copilot hace los cambios y genera US-613-INSTRUCCIONES.md)

# 2. Hacer commit
cd C:\00_Tandem2026
git add .
git commit -m "fix(US-613): Corregir error 404 en comunicación Plugin-Servidor MVC AB#613"
git push origin master

# 3. Mover US a Done
cd Scripts
.\Edit-US.ps1 613 -Estado "Done"

# 4. ⭐ ADJUNTAR DOCUMENTACIÓN (PASO CRÍTICO)
.\Attach-Document.ps1 `
    -WorkItemId 613 `
    -FilePath "C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\US-613-INSTRUCCIONES.md" `
    -Comment "Documentación completa US-613: Solución error 404, configuración de puertos, instrucciones de ejecución y troubleshooting"

# 5. Verificar en Azure DevOps que el archivo esté adjunto
# https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/613
```

### Verificar archivos adjuntos

```powershell
# Script para ver archivos adjuntos de una US
$PAT = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

$workItemId = 613
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$workItemId?`$expand=relations&api-version=7.0"
$wi = Invoke-RestMethod -Uri $url -Headers $headers

Write-Host "`nArchivos adjuntos en US #${workItemId}:" -ForegroundColor Cyan
$attachments = $wi.relations | Where-Object { $_.rel -eq 'AttachedFile' }

if ($attachments) {
    $attachments | ForEach-Object {
        Write-Host "  📎 $($_.attributes.name)" -ForegroundColor Green
        Write-Host "     $($_.attributes.comment)" -ForegroundColor Gray
    }
} else {
    Write-Host "  (ninguno)" -ForegroundColor Yellow
}
```

### ✅ Checklist: Completar US con documentación

Antes de mover una US a "Done", verifica:

- [ ] Código implementado y funcionando
- [ ] Tests pasando (si aplica)
- [ ] Commit realizado con mensaje descriptivo
- [ ] Push a GitHub completado
- [ ] **📎 Documentación adjunta al Work Item** ⭐
- [ ] Work Item movido a "Done"
- [ ] Tasks hijas completadas

**⚠️ NOTA:** El paso de adjuntar documentación NO es opcional cuando el agente generó documentación. Es parte del flujo estándar de completar una US.

---

## 🎓 Buenas Prácticas

### ✅ DO (Hacer):

1. **Siempre verificar la configuración** antes de ejecutar scripts en una US nueva
2. **Usar los scripts desde `Scripts/`** en lugar de copiar código manualmente
3. **Vincular commits desde el principio** con `AB#<ID>`
4. **Crear las 3 tasks estándar** (Develop, Test, CR) para cada US
5. **Actualizar estados regularmente** para reflejar el progreso real
6. **⭐ Adjuntar documentación generada por agentes** al Work Item antes de moverlo a "Done"
7. **Incluir contexto completo** en la documentación: problema, solución, troubleshooting
8. **Usar nombres descriptivos** para archivos adjuntos (ej: `US-613-INSTRUCCIONES.md`)

### ❌ DON'T (No hacer):

1. **No usar valores hardcodeados** de org/project sin verificar
2. **No mezclar PATs** de diferentes entornos
3. **No crear Tasks sin vincular** a una US
4. **No olvidar el prefijo `AB#`** en los commits
5. **No dejar Tasks en "Doing"** si el trabajo se pausó
6. **⭐ NO mover a "Done" sin adjuntar documentación** si el agente generó documentos técnicos
7. **No perder documentación valiosa** generada durante el desarrollo

---

## 📞 Enlaces Útiles

| Recurso | URL |
|---------|-----|
| **Panel del Proyecto** | https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues |
| **Crear Work Item** | https://dev.azure.com/VSCAD/tandem2026/_workitems/create/Issue |
| **API Documentation** | https://learn.microsoft.com/rest/api/azure/devops/wit/ |
| **GitHub Repo** | https://github.com/JuanGodoyLopez/Tandem-2026 |

---

## 📝 Changelog de este Documento

| Fecha | Cambio |
|-------|--------|
| 2026-04-25 | ⚡ **Agregado Create-US-Fast.ps1**: Script rápido (<20 seg) con soporte Story Points y encoding UTF-8. Añadida sección "Comandos Rápidos Más Usados" al inicio |
| 2026-04-25 | Agregada sección completa sobre adjuntar documentación a Work Items. Incluye script Attach-Document.ps1, ejemplos, checklist y mejores prácticas |
| 2026-04-24 | Creación inicial con énfasis en configuración correcta y troubleshooting del error 404 |

---

**Mantenido por:** Equipo Tandem 2026  
**Contacto:** Revisar `Scripts/` para scripts actualizados
