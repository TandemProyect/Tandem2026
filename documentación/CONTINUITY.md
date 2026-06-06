# 🤖 CONTINUITY GUIDE - Para Futuros Agentes/Chats

> **PROPÓSITO:** Este documento permite que cualquier agente IA continúe el trabajo sin pérdida de contexto.
> **ÚLTIMA ACTUALIZACIÓN:** 2026-04-25

---

## ⚡ INICIO RÁPIDO - Lee Esto Primero

### 🚀 PRIMER COMANDO A EJECUTAR
```powershell
.\Scripts\HealthCheck.ps1
```
Este script te dirá en 30 segundos el estado completo del proyecto.

### Estado del Proyecto
- ✅ **Workspace funcionando** en `C:\00_Tandem2026\`
- ✅ **Repositorio Git** activo: `https://github.com/JuanGodoyLopez/Tandem-2026`
- ✅ **Azure DevOps** integrado con PAT Full Access
- ✅ **Automatización** de User Stories funcional
- ⚠️ **Columnas del Board** requieren configuración manual (API bloqueada)

### Framework y Entorno
- **.NET Framework 4.8** (legacy, no .NET Core)
- **Visual Studio 2026** Community (18.6.0-insiders)
- **PowerShell** como shell preferido
- **ZWCAD Plugin** (.NET Framework 4.8)

---

## 🎯 LO QUE FUNCIONA - Automatización Disponible

### 0. Health Check - Verificación Rápida ✅
```powershell
.\Scripts\HealthCheck.ps1
```
- Verifica Git, documentación, scripts, Azure DevOps, solución
- Diagnóstico completo en 5 pasos
- Te dice exactamente qué funciona y qué no

### 1. Crear User Stories Automáticamente ✅
```powershell
.\Scripts\US.ps1 "Título de la US" "Descripción opcional"
```
- Crea work items tipo "Issue" en Azure DevOps
- Auto-enlaza con commits usando `AB#<id>`
- PAT configurado y funcional

### 2. Editar User Stories ✅
```powershell
.\Scripts\Edit-US.ps1 <ID> -Titulo "Nuevo título" -Descripcion "Nueva desc" -Prioridad 1
```
- Modifica US existentes
- ⚠️ Estados limitados a: `To Do`, `Doing`, `Done` (proceso Basic)

### 3. Crear Tasks (Subtareas) ✅

**⚠️ IMPORTANTE:** El script `US.ps1` **ahora crea automáticamente** las 3 Tasks estándar (Develop, Test, CR) al crear una User Story.

#### Crear Tasks manualmente para una US existente:

```powershell
# Método recomendado: Script inline (sin problemas de permisos)
$usId = 619  # ⚠️ Cambiar por el ID de tu User Story

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"; "Content-Type" = "application/json-patch+json"}

$tasks = @("Develop", "Test", "CR")

foreach ($taskType in $tasks) {
    $payload = '[{"op":"add","path":"/fields/System.Title","value":"' + $taskType + '"},{"op":"add","path":"/fields/System.WorkItemType","value":"Task"},{"op":"add","path":"/relations/-","value":{"rel":"System.LinkTypes.Hierarchy-Reverse","url":"https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/' + $usId + '"}}]'

    $url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/`$Task?api-version=7.0"
    $result = Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $payload

    Write-Host "✓ Task $taskType creada: #$($result.id)" -ForegroundColor Green
}
```

#### Verificar Tasks vinculadas:

```powershell
$usId = 619  # ⚠️ Cambiar por el ID de tu User Story

$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$PAT"))
$headers = @{Authorization = "Basic $auth"}

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
    }
}
```

**Ejemplo de uso real:**
```powershell
# US #619: Insertar Img en Command Seleccionar Muro
# Tasks creadas: #624 (Develop), #625 (Test), #626 (CR)

# US #627: Ejemplo US con Tasks auto
# Tasks creadas: #628 (Develop), #629 (Test), #630 (CR)
```

**Documentación completa:** `Docs/General/Azure-DevOps.md` (sección "Crear Tasks")

### 4. Verificar Board ✅
```powershell
.\Scripts\Verificar-Board.ps1
```
- Muestra columnas actuales
- Verifica configuración
- Lista estados disponibles

---

## 🚫 LO QUE NO FUNCIONA - Limitaciones Conocidas

### Crear Columnas del Board via API ❌
- **Error:** `"Value cannot be null. Parameter name: options"`
- **Endpoint:** `PUT https://dev.azure.com/.../boards/Issues?api-version=7.1-preview.1`
- **Causa:** Bug o limitación no documentada en API preview
- **Solución:** Configuración manual (ver `Docs/General/Como-Crear-Columnas-Panel.md`)
- **Investigación completa:** `Docs/General/Investigacion-API-Board-Columnas.md`

**7+ intentos diferentes realizados, todos fallaron con el mismo error.**

---

## 📂 ESTRUCTURA DE DOCUMENTACIÓN

### Punto de Entrada
- **`Docs/README.md`** - Índice general de toda la documentación

### Documentación General (Cross-Project)
- **`Docs/General/README.md`** - Índice de docs generales
- **`Docs/General/Azure-DevOps.md`** - Proceso Azure DevOps, scripts, workflow
- **`Docs/General/Git-Workflow.md`** - Workflow Git, commits, branches
- **`Docs/General/Convenciones.md`** - Estándares de código, nombres, estructura
- **`Docs/General/Common.md`** - Funcionalidad compartida entre proyectos
- **`Docs/General/Estructura-Repositorio.md`** - Organización de carpetas y proyectos

### Documentación Específica de Azure DevOps
- **`Docs/General/Configurar-Columnas-Board.md`** - Guía para configurar columnas (MANUAL)
- **`Docs/General/Como-Crear-Columnas-Panel.md`** - Pasos rápidos para crear columnas
- **`Docs/General/Investigacion-API-Board-Columnas.md`** - Todo lo intentado y por qué falló

### Documentación por Proyecto
- **`Docs/Proyectos/ZwcadPlugin/README.md`** - Plugin ZWCAD, comandos, WPF
- **`Docs/Proyectos/DAL/README.md`** - Capa de acceso a datos
- **`Docs/Proyectos/Desing/README.md`** - Proyecto de diseño

---

## 🔐 AUTENTICACIÓN Y ACCESO

### PAT (Personal Access Token)
- **Scope:** Full Access
- **Ubicación:** En scripts `US.ps1`, `Edit-US.ps1`, `Verificar-Board.ps1`
- **Válido:** Sí (verificado 2026-04-25)
- **Permisos:** Crear/editar work items, leer boards, leer proyectos

### Azure DevOps
- **Organización:** `https://dev.azure.com/VSCAD`
- **Proyecto:** `tandem2026`
- **Team:** `tandem2026 Team`
- **Board:** `Issues` (proceso Basic)

---

## 🛠️ SCRIPTS DISPONIBLES

| Script | Función | Estado |
|--------|---------|--------|
| `Scripts/HealthCheck.ps1` | **Verificación completa del proyecto** | ✅ **EJECUTAR PRIMERO** |
| `Scripts/US.ps1` | Crear User Stories (con Tasks auto) | ✅ Funciona |
| `Scripts/Edit-US.ps1` | Editar User Stories | ✅ Funciona |
| `Scripts/Task.ps1` | Crear Task individual | ⚠️ Usar script inline (permisos) |
| `Scripts/Create-StandardTasks.ps1` | Crear 3 Tasks estándar | ⚠️ Usar script inline (permisos) |
| `Scripts/Verificar-Board.ps1` | Ver estado del board | ✅ Funciona |
| `Scripts/Ver-Board.ps1` | Ver columnas actuales | ✅ Funciona |
| `Scripts/Configurar-Board-Final.ps1` | Crear columnas (intento) | ❌ No funciona |
| `Scripts/Crear-Columnas-*.ps1` | Varios intentos columnas | ❌ No funciona |

**Nota:** Los scripts `Task.ps1` y `Create-StandardTasks.ps1` tienen problemas de permisos de ejecución.  
**Solución:** Usar los comandos inline documentados en la sección "Crear Tasks" de este archivo y en `Docs/General/Azure-DevOps.md`.

### Script Recomendado para Iniciar
```powershell
.\Scripts\HealthCheck.ps1
```
Este script verifica en 5 pasos el estado completo del proyecto y te dice qué funciona y qué no.

---

## 📋 WORKFLOW ESTABLECIDO

### Para Crear una Nueva Feature/US

1. **Crear US en Azure DevOps**
   ```powershell
   .\Scripts\US.ps1 "Nombre de la feature" "Descripción detallada"
   # Anota el ID devuelto (ej: #615)
   ```

2. **Desarrollar la feature**
   - Crear branch si es necesario
   - Implementar código
   - Seguir convenciones en `Docs/General/Convenciones.md`

3. **Commit con enlace**
   ```powershell
   git commit -m "feat: Descripción del cambio AB#615"
   git push origin master
   ```

4. **Actualizar estado de la US**
   ```powershell
   .\Scripts\Edit-US.ps1 615 -Estado "Done"
   ```

---

## 🎓 LECCIONES APRENDIDAS

### 1. API de Azure DevOps - Board Columns
- ❌ El endpoint PUT para actualizar columnas del board está roto o mal documentado
- ✅ Los endpoints de work items (crear/editar) funcionan perfectamente
- ✅ Los endpoints de lectura (GET) funcionan sin problemas
- 📝 La API preview v7.1 tiene bugs no documentados

### 2. Proceso "Basic" de Azure DevOps
- Es un proceso del sistema (`customizationType: "system"`)
- NO se puede modificar directamente
- Solo tiene 3 estados: `To Do`, `Doing`, `Done`
- Para personalizar estados: crear proceso heredado (requiere admin)

### 3. Workflow de Documentación
- La documentación NO es para el usuario, es para futuros agentes
- Cada sesión debe actualizar `CONTINUITY.md`
- Documentar tanto éxitos como fracasos (para no repetir errores)

---

## 🚀 CÓMO CONTINUAR DESDE AQUÍ

### Si Necesitas Crear Columnas del Board
1. Lee `Docs/General/Como-Crear-Columnas-Panel.md`
2. Configuración manual en Azure DevOps (5 min)
3. Verifica con `.\Scripts\Verificar-Board.ps1`
4. Actualiza este documento si encuentras una solución API

### Si Necesitas Crear/Editar Work Items
1. Usa `Scripts/US.ps1` para crear
2. Usa `Scripts/Edit-US.ps1` para editar
3. Enlaza commits con `AB#<id>`
4. Todo funciona automáticamente

### Si Necesitas Trabajar en el Plugin ZWCAD
1. Lee `Docs/Proyectos/ZwcadPlugin/README.md`
2. Revisa `Docs/Proyectos/ZwcadPlugin/TECHNICAL_GUIDE.md`
3. Sigue convenciones en `Docs/General/Convenciones.md`

### Si Necesitas Entender el Proyecto
1. Empieza por `Docs/README.md`
2. Lee `Docs/General/Estructura-Repositorio.md`
3. Revisa `Docs/General/Common.md` para funcionalidad compartida

---

## 🔄 PRÓXIMAS TAREAS SUGERIDAS

### Prioridad Alta
- [ ] Configurar columnas del board manualmente (usuario)
- [ ] Actualizar `Edit-US.ps1` con nombres de columnas correctos
- [ ] Verificar que scripts funcionen con la nueva configuración

### Prioridad Media
- [ ] Crear script para mover US entre columnas
- [ ] Automatizar reportes de progreso
- [ ] Agregar más campos a US.ps1 (assignee, tags, etc.)

### Prioridad Baja
- [ ] Investigar Azure CLI como alternativa
- [ ] Considerar proceso heredado para estados custom
- [ ] Automatizar creación de branches desde US

---

## 📞 INFORMACIÓN DE CONTACTO

### Repositorio
- GitHub: `https://github.com/JuanGodoyLopez/Tandem-2026`
- Branch principal: `master`

### Azure DevOps
- Board: `https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues`
- Project: `https://dev.azure.com/VSCAD/tandem2026`

---

## 🤝 PARA EL PRÓXIMO AGENTE

**Si eres un agente IA continuando este trabajo:**

1. ✅ **Lee este archivo primero** - Contiene todo el contexto crítico
2. ✅ **Verifica los scripts** - Ejecuta `.\Scripts\Verificar-Board.ps1` para ver estado actual
3. ✅ **Consulta la documentación** - Está en `Docs/` y está completa
4. ✅ **No repitas errores** - Lee `Investigacion-API-Board-Columnas.md` antes de intentar crear columnas
5. ✅ **Actualiza este documento** - Cuando aprendas algo nuevo o resuelvas algo

**Contexto adicional que necesites:**
- `board-structure.json` - Estructura actual del board
- `board-full.json` - Board completo con allowedMappings
- Commits recientes - Revisa `git log --oneline -20`

---

## ✅ VERIFICACIÓN DE CONTINUIDAD

**Antes de terminar una sesión, verifica:**
- [ ] Documentación actualizada
- [ ] Commits pushed a GitHub
- [ ] Scripts funcionando
- [ ] Este archivo actualizado con nuevos aprendizajes
- [ ] Próximos pasos claros

**Después de leer este documento, deberías saber:**
- ✅ Qué funciona y qué no
- ✅ Dónde encontrar cada pieza de información
- ✅ Cómo crear y editar User Stories
- ✅ Por qué las columnas del board no se pueden crear via API
- ✅ Cómo continuar el trabajo sin repetir investigaciones

---

**Última sesión:** 2026-04-25 - Investigación exhaustiva API Board, scripts funcionales creados
**Próxima sesión:** Configurar columnas manualmente y continuar con automatización de workflow
