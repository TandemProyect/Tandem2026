# 📚 Documentación Gestión Azure DevOps - ACTUALIZADA

## ✅ Problema Resuelto

**Fecha:** 2026-04-24  
**US Relacionada:** #613  
**Commits:** 4c9b36e

---

## ⚠️ Error Que Se Estaba Cometiendo

### ❌ CONFIGURACIÓN INCORRECTA (causaba error 404):

```powershell
$org = "juangodoylopez"              # ❌ INCORRECTO
$project = "Tandem 2026"             # ❌ INCORRECTO
$projectEncoded = "Tandem%202026"    # ❌ INCORRECTO
$PAT = "pknrhdrnq4wlkjrbnfykqhnnkqcn4f72h7ukb6f7g3ezrp3cg7ha"  # ❌ PAT INCORRECTO
```

**Resultado:** Error 404 "No se encontró" al intentar crear Tasks

---

## ✅ Configuración Correcta

### ✅ VALORES CORRECTOS:

```powershell
$org = "VSCAD"                       # ✅ CORRECTO
$project = "tandem2026"              # ✅ CORRECTO (todo en minúsculas)
$PAT = "7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0"
```

**URLs correctas:**
- Base: `https://dev.azure.com/VSCAD/tandem2026`
- Panel: `https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues`

---

## 📘 Documentación Nueva Creada

### 1. **GESTION-PANEL-AZURE-DEVOPS.md** ⭐ **PRINCIPAL**

**Ubicación:** `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md`

**Contenido:**
- ⚠️ Sección destacada con el error común y cómo evitarlo
- 📋 Configuración de referencia (siempre usar estos valores)
- 🚀 Guía completa para crear User Stories
- 📋 Guía completa para crear Tasks (individual y por lotes)
- ✏️ Editar User Stories y Tasks
- 🔍 Verificar relaciones entre US y Tasks
- 🔗 Vincular commits correctamente
- 📜 Scripts de referencia completos listos para copiar/pegar
- 🛠️ **Troubleshooting detallado** de los 5 errores más comunes
- 📊 Resumen de comandos más usados
- 🎓 Buenas prácticas y anti-patrones

**Longitud:** ~1,200 líneas de documentación exhaustiva

---

### 2. **Scripts/README.md**

**Ubicación:** `Scripts/README.md`

**Contenido:**
- 📜 Documentación de todos los scripts PowerShell
- 🚀 `Quick-US.ps1` - Nuevo script para crear US + 3 Tasks en un comando
- 📋 Explicación detallada de cada script existente
- 🎯 Flujos de trabajo comunes (4 flujos documentados)
- ⚠️ Configuración y errores comunes
- 💡 Tips y mejores prácticas

---

### 3. **Scripts/Quick-US.ps1** ⚡ **NUEVO SCRIPT**

**Ubicación:** `Scripts/Quick-US.ps1`

**Propósito:** Crear User Story + 3 Tasks estándar (Develop, Test, CR) en un solo comando.

**Uso:**
```powershell
cd C:\00_Tandem2026\Scripts
.\Quick-US.ps1 "Título de la funcionalidad"
.\Quick-US.ps1 "Título" "Descripción detallada"
```

**Ejemplo:**
```powershell
.\Quick-US.ps1 "Implementar exportación CSV"
```

**Salida:**
```
========================================
✅ USER STORY COMPLETA
========================================

📋 User Story #640
   Título: Implementar exportación CSV

📌 Tasks creadas:
   #641 - Develop
   #642 - Test
   #643 - CR

🔗 Enlaces:
   US:    https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/640
   Board: https://dev.azure.com/VSCAD/tandem2026/_boards/board/...

💡 Próximos pasos:
   1. Revisar la US en Azure DevOps
   2. Ajustar prioridad si es necesario
   3. Vincular commits con: AB#640
```

**Características:**
- ✅ Usa la configuración correcta (VSCAD/tandem2026)
- ✅ Manejo de errores robusto
- ✅ Salida formateada con colores
- ✅ Enlaces directos a Azure DevOps
- ✅ Sugerencias de próximos pasos

---

### 4. Actualizaciones en Documentación Existente

#### `Docs/README.md`
- Agregado enlace destacado a `GESTION-PANEL-AZURE-DEVOPS.md` como documento prioritario
- Marcado con ⭐ para visibilidad

#### `Docs/General/Azure-DevOps.md`
- Agregado aviso al inicio redirigiendo a la nueva documentación completa
- Mantiene la documentación histórica intacta

---

## 🎯 Cómo Usar Esta Documentación

### Para Crear US + Tasks:

1. **Lee primero:** `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md` (especialmente la sección de configuración correcta)

2. **Usa el script rápido:**
```powershell
cd C:\00_Tandem2026\Scripts
.\Quick-US.ps1 "Mi nueva funcionalidad"
```

3. **Vincula commits:**
```bash
git commit -m "feat: Descripción AB#<ID>"
```

---

### Si Obtienes Error 404:

1. **Verifica la configuración** en `GESTION-PANEL-AZURE-DEVOPS.md` sección "⚠️ IMPORTANTE"
2. **Consulta Troubleshooting** en la sección "8. Troubleshooting"
3. **Usa exactamente estos valores:**
   - Organización: `VSCAD`
   - Proyecto: `tandem2026`
   - PAT: (ver en `Scripts/US.ps1` línea 10)

---

### Para Scripts PowerShell:

1. **Consulta:** `Scripts/README.md`
2. **Scripts disponibles:**
   - `Quick-US.ps1` - Crear US + Tasks completa ⚡
   - `US.ps1` - Crear solo US
   - `Task.ps1` - Crear Task individual
   - `Edit-US.ps1` - Editar US existente
   - `Completar-Tasks-US.ps1` - Completar todas las tasks de una US

---

## 📊 Estadísticas

**Archivos creados:** 3  
**Archivos modificados:** 2  
**Líneas de documentación:** ~1,500  
**Scripts nuevos:** 1 (`Quick-US.ps1`)

---

## ✅ Verificación

### US #613 - Estado Actual:

```
✓ US #613 creada
✓ Task #634 - Develop
✓ Task #635 - Test
✓ Task #636 - CR
✓ Documentación completa
✓ Commit y push completados
```

### Scripts Probados:

```powershell
# ✅ Funciona correctamente:
$usId = 613
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

**Resultado:**
```
✓ Task Develop creada: #634
✓ Task Test creada: #635
✓ Task CR creada: #636
```

---

## 🎓 Lecciones Aprendidas

### 1. **Validar configuración SIEMPRE**
- No asumir valores de org/project
- Verificar en la documentación oficial del proyecto
- Guardar valores correctos en un lugar central

### 2. **Documentar errores comunes**
- Destacar errores que causaron pérdida de tiempo
- Explicar la causa raíz
- Proporcionar la solución clara

### 3. **Crear scripts reutilizables**
- Un script bien documentado > múltiples scripts sin docs
- Incluir manejo de errores
- Proporcionar salida informativa

### 4. **Centralizar documentación**
- Un documento maestro > varios fragmentados
- Índice claro al inicio
- Enlaces cruzados entre documentos

---

## 🔗 Enlaces Rápidos

| Documento | Enlace |
|-----------|--------|
| **Guía Completa Azure DevOps** | [Docs/General/GESTION-PANEL-AZURE-DEVOPS.md](Docs/General/GESTION-PANEL-AZURE-DEVOPS.md) |
| **Scripts PowerShell** | [Scripts/README.md](Scripts/README.md) |
| **Script Rápido** | [Scripts/Quick-US.ps1](Scripts/Quick-US.ps1) |
| **Panel Azure DevOps** | https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues |

---

## 💡 Próximos Pasos

1. **Al crear nueva US:**
   - Usar `Quick-US.ps1` en lugar de crear manualmente
   - Verificar que se crearon las 3 tasks

2. **Al hacer commit:**
   - Siempre incluir `AB#<ID>` en el mensaje
   - Usar el formato: `tipo: descripción AB#<ID>`

3. **Si encuentras un error nuevo:**
   - Documentarlo en `GESTION-PANEL-AZURE-DEVOPS.md` sección Troubleshooting
   - Agregar la solución que funcionó

---

**Creado:** 2026-04-24  
**US Relacionada:** AB#613  
**Mantenido por:** Equipo Tandem 2026
