# 🎯 Azure DevOps - Gestión de Trabajo

Guía para crear y gestionar **User Stories** e **Issues** en Azure DevOps para Tandem 2026.

---

## 📊 Panel de Trabajo

**URL:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues

---

## 🚀 Crear User Stories / Issues Rápidamente

### **Método 1: Scripts PowerShell (Recomendado)**

#### **Crear nueva User Story:**
```powershell
cd C:\00_Tandem2026\Scripts
.\US.ps1 "Titulo de la user story"
.\US.ps1 "Titulo" "Descripcion opcional"
```

**Ejemplo:**
```powershell
.\US.ps1 "Implementar autenticación de usuarios"
.\US.ps1 "Agregar validación de formularios" "Validar campos requeridos en formulario de registro"
```

---

#### **Editar User Story existente:**
```powershell
.\Edit-US.ps1 <ID> -Titulo "Nuevo titulo"
.\Edit-US.ps1 <ID> -Estado "Doing"
.\Edit-US.ps1 <ID> -Prioridad 1
.\Edit-US.ps1 <ID> -Estado "Done" -AsignadoA "juan@example.com"
```

**Parámetros disponibles:**
- `-Titulo "..."`
- `-Descripcion "..."`
- `-Estado "To Do" | "Doing" | "Done"`
- `-Prioridad 1-4` (1 = Alta, 4 = Baja)
- `-AsignadoA "email@example.com"`

**Ejemplo:**
```powershell
.\Edit-US.ps1 612 -Estado "Done"
.\Edit-US.ps1 613 -Titulo "Nuevo nombre" -Prioridad 1
```

---

### **Método 2: Interfaz Web**

1. Ve al panel: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
2. Haz clic en **"+ New Work Item"**
3. Selecciona **"Issue"**
4. Completa los campos y guarda

---

## 📌 Crear Tasks (Tareas de User Stories)

### **¿Cuándo crear Tasks?**

Las **Tasks** son subtareas que descomponen una User Story en pasos ejecutables. Se recomienda crear Tasks para:

- **Develop**: Implementación del código
- **Test**: Pruebas funcionales
- **CR (Code Review)**: Revisión de código
- **Deploy**: Despliegue
- **Docs**: Documentación

---

### **Método 1: PowerShell con API REST (Recomendado)**

> ⚠️ **IMPORTANTE**: Usa el PAT correcto que está en `Scripts/US.ps1` (línea 10)

#### **Crear Task individual vinculada a una User Story:**

```powershell
# Configuración (usa el PAT de Scripts/US.ps1)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
    Authorization = "Basic $auth"
    "Content-Type" = "application/json-patch+json"
}

# Crear Task vinculada a US #619 (cambiar el número por tu US)
$usId = 619
$taskTitle = "Develop"

$payload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskTitle + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'

$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

Write-Host "✓ Task creada: #$($result.id)" -ForegroundColor Green
```

**Cambiar `$usId` por el ID de tu User Story.**

---

#### **Actualizar estado de una Task:**

```powershell
# Configuración
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{
    Authorization = "Basic $auth"
    "Content-Type" = "application/json-patch+json; charset=utf-8"
}

# Marcar Task #616 como Done
$body = '[{"op":"replace","path":"/fields/System.State","value":"Done"}]'
$url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/616?api-version=7.0"
$result = Invoke-RestMethod -Uri $url -Headers $headers -Method Patch -Body $body

Write-Host "✓ Task #616 marcada como Done" -ForegroundColor Green
```

**Estados válidos para Tasks:**
- `"To Do"` - Por hacer
- `"Doing"` - En progreso
- `"Done"` - Completada

---

#### **Script rápido para crear 3 Tasks estándar (Develop, Test, CR):**

```powershell
# ⚠️ CAMBIAR $usId por el ID de tu User Story
$usId = 619

# Configuración (PAT desde Scripts/US.ps1)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

# Tasks estándar
$tasks = @("Develop", "Test", "CR")

foreach ($taskType in $tasks) {
    $payload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'

    $url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
    $result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

    Write-Host "✓ Task $taskType creada: #$($result.id)" -ForegroundColor Green
}
```

**Ejemplo de uso:**
```powershell
# Crear Tasks para US #619
$usId = 619  # <-- Cambiar este número

# ... copiar el resto del script y ejecutar
```

**Resultado esperado:**
```
✓ Task Develop creada: #624
✓ Task Test creada: #625
✓ Task CR creada: #626
```

---

#### **Verificar Tasks vinculadas a una User Story:**

```powershell
# Configuración
$usId = 619  # ⚠️ Cambiar por el ID de tu User Story
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

# Obtener US con relaciones
$us = Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/${usId}?`$expand=relations&api-version=7.0" -Headers $headers

Write-Host "US #${usId}: $($us.fields.'System.Title')" -ForegroundColor Cyan

if ($us.relations) {
    $children = $us.relations | Where-Object { $_.attributes.name -eq "Child" }

    if ($children) {
        Write-Host "`n✓ Tasks vinculadas:" -ForegroundColor Green

        foreach ($child in $children) {
            $taskId = $child.url.Split('/')[-1]
            $task = Invoke-RestMethod -Uri $child.url -Headers $headers

            Write-Host "  #${taskId} - $($task.fields.'System.Title') [$($task.fields.'System.State')]" -ForegroundColor Yellow
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
US #619: Insertar Img en Command Seleccionar Muro

✓ Tasks vinculadas:
  #624 - Develop [To Do]
  #625 - Test [To Do]
  #626 - CR [To Do]
```

---

### **Método 2: Interfaz Web**

1. Abre la User Story en Azure DevOps
2. En la pestaña **"Related Work"** o **"Links"**, haz clic en **"Add link"**
3. Selecciona **"New item"**
4. Tipo: **"Task"**
5. Link type: **"Child"**
6. Completa título y guarda

---

## 📋 Columnas del Board

| Columna | Propósito | WIP Limit |
|---------|-----------|-----------|
| **Tareas a Analizar** | Nuevas tareas sin analizar | - |
| **Esperando documentación** | Bloqueadas por falta de info | 5 |
| **Preparado para Realizar** | Listas para desarrollo | 25 |
| **Mal Testeo Volver a Realizar** | Falló testing, requiere rehacer | 5 |
| **Realizando** | En desarrollo activo | 5 |
| **Preparando a testear** | Desarrollo completo, pendiente QA | 100 |
| **Preparado para presentar** | Completado y validado | - |

---

## 🔗 Vincular Commits a User Stories

### **Formato de Commit:**

```
tipo: descripcion AB#<ID>
```

**Ejemplos:**
```bash
git commit -m "feat: Nuevo sistema de login AB#612"
git commit -m "fix: Corregir validacion de email AB#615"
git commit -m "docs: Actualizar README AB#612"
```

### **Palabras Clave Especiales:**

| Palabra | Acción |
|---------|--------|
| `AB#612` | Solo vincula |
| `Fixes AB#612` | Marca como "Resolved" al hacer merge |
| `Closes AB#612` | Marca como "Closed" al hacer merge |
| `Resolves AB#612` | Marca como "Resolved" al hacer merge |

**Ejemplo:**
```bash
git commit -m "feat: Sistema de iconos completo Fixes AB#612"
```

---

## 🏷️ Prioridades

| Nivel | Significado | Uso |
|-------|-------------|-----|
| **1** | Crítico | Bloqueante, requiere atención inmediata |
| **2** | Alta | Importante, debe hacerse pronto |
| **3** | Media | Normal, puede esperar |
| **4** | Baja | Nice to have, no urgente |

---

## 📝 Plantilla de User Story

**Título:**
```
[Funcionalidad] Acción específica
```

**Descripción:**
```
Como [rol]
Quiero [funcionalidad]
Para [beneficio/objetivo]

Criterios de aceptación:
- [ ] Criterio 1
- [ ] Criterio 2
- [ ] Criterio 3

Notas técnicas:
- Consideración 1
- Consideración 2
```

**Ejemplo:**
```
Título: Implementar búsqueda de proyectos

Como usuario del sistema
Quiero buscar proyectos por nombre o ID
Para encontrar rápidamente información específica

Criterios de aceptación:
- [ ] El campo de búsqueda está visible en la barra superior
- [ ] La búsqueda filtra en tiempo real
- [ ] Muestra resultados con nombre, ID y estado
- [ ] Permite hacer clic para abrir el proyecto

Notas técnicas:
- Usar índice de búsqueda existente
- Implementar debounce de 300ms
- Máximo 50 resultados por consulta
```

---

## 🐛 Reportar Bugs

### **Información Requerida:**

1. **Título claro:** `[Componente] Error al...`
2. **Pasos para reproducir:**
   ```
   1. Ir a...
   2. Hacer clic en...
   3. Observar que...
   ```
3. **Resultado esperado:** Qué debería pasar
4. **Resultado actual:** Qué está pasando
5. **Entorno:**
   - Versión de la aplicación
   - Sistema operativo
   - Navegador (si aplica)
6. **Screenshots/Logs:** Si están disponibles

---

## 🔍 Búsqueda y Filtros

### **Buscar por ID:**
```
#612
```

### **Buscar por texto:**
```
titulo: "sistema de iconos"
```

### **Filtrar por estado:**
```
State = "Doing"
```

### **Filtrar por asignado:**
```
Assigned To = @Me
```

### **Combinaciones:**
```
State = "Doing" AND Priority = 1
```

---

## 📊 Integración con GitHub

**Repositorio:** https://github.com/JuanGodoyLopez/Tandem-2026

Los commits con formato `AB#<ID>` se vinculan automáticamente al Work Item correspondiente en Azure DevOps.

**Ver commits de una US:**
- Abre la User Story en Azure DevOps
- Ve a la sección **"Development"** (lado derecho)
- Verás todos los commits vinculados

---

## 🛠️ Scripts Disponibles

| Script | Ubicación | Propósito |
|--------|-----------|-----------|
| `US.ps1` | `Scripts/US.ps1` | Crear User Stories rápidamente |
| `Edit-US.ps1` | `Scripts/Edit-US.ps1` | Editar User Stories |
| `Configurar-Board-Manual.ps1` | `Scripts/` | Guía configuración del board |

---

## ❓ Preguntas Frecuentes

### **¿Cómo cierro una User Story?**
```powershell
.\Edit-US.ps1 612 -Estado "Done"
```

### **¿Cómo asigno una tarea a alguien?**
```powershell
.\Edit-US.ps1 612 -AsignadoA "juan@example.com"
```

### **¿Cómo cambio la prioridad?**
```powershell
.\Edit-US.ps1 612 -Prioridad 1
```

### **¿Puedo crear múltiples User Stories a la vez?**
Sí, ejecuta el script varias veces:
```powershell
.\US.ps1 "Tarea 1"
.\US.ps1 "Tarea 2"
.\US.ps1 "Tarea 3"
```

---

## 📞 Soporte

**Problemas con Azure DevOps:**
- Documentación oficial: https://learn.microsoft.com/azure/devops/
- Panel de proyecto: https://dev.azure.com/VSCAD/tandem2026

**Problemas con scripts:**
- Revisa `Scripts/` en el repositorio
- Asegúrate de tener el PAT configurado
- Verifica permisos en Azure DevOps

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
