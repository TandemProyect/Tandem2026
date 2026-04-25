# ⚡ Create-US-Fast.ps1 - Script Rápido para Crear User Stories

## 📋 Descripción

Script optimizado para crear User Stories en Azure DevOps en menos de 20 segundos, con soporte completo para Story Points y encoding UTF-8 correcto.

## 🎯 Características

- ✅ Creación de User Stories en < 20 segundos
- 🎯 Soporte completo para Story Points
- 🔧 Encoding UTF-8 correcto (resuelve errores comunes de Azure DevOps)
- 🌐 Abre automáticamente el navegador con la US creada
- 📝 Retorna el ID de la US creada para automatización

## 🚀 Uso Rápido

### Ubicación
```
C:\00_Tandem2026\Scripts\Create-US-Fast.ps1
```

### Sintaxis
```powershell
.\Create-US-Fast.ps1 -Titulo <string> [-Descripcion <string>] [-StoryPoints <int>]
```

### Parámetros

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| **Titulo** | string | ✅ Sí | Título de la User Story |
| **Descripcion** | string | ❌ No | Descripción detallada (opcional) |
| **StoryPoints** | int | ❌ No | Puntos de valoración (0-13, default: 0) |

## 📖 Ejemplos

### Ejemplo 1: US Simple
```powershell
cd C:\00_Tandem2026\Scripts
.\Create-US-Fast.ps1 -Titulo "Agregar botón de exportar"
```
**Resultado:** US sin descripción ni puntos

### Ejemplo 2: US con Descripción
```powershell
.\Create-US-Fast.ps1 `
	-Titulo "Implementar filtros avanzados" `
	-Descripcion "Filtros por fecha, usuario y estado con búsqueda en tiempo real"
```

### Ejemplo 3: US con Story Points (⭐ Recomendado)
```powershell
.\Create-US-Fast.ps1 `
	-Titulo "Detectar esquinas tipo L" `
	-Descripcion "Implementar algoritmo para detectar esquinas tipo L en geometrías seleccionadas. El sistema debe identificar esquinas formadas por dos líneas perpendiculares y proporcionar 8 puntos de referencia." `
	-StoryPoints 8
```
**Resultado:**
```
US #637 creada con 8 puntos
https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/637
637
```

### Ejemplo 4: Crear Múltiples US en Lote
```powershell
cd C:\00_Tandem2026\Scripts

# US pequeña
.\Create-US-Fast.ps1 -Titulo "Agregar tooltips" -StoryPoints 2

# US mediana
.\Create-US-Fast.ps1 -Titulo "Implementar paginación" -Descripcion "Paginación en tabla de usuarios" -StoryPoints 5

# US grande
.\Create-US-Fast.ps1 -Titulo "Integración con API externa" -Descripcion "Conectar con API de terceros para sincronización automática" -StoryPoints 13
```

## 🔧 Detalles Técnicos

### ¿Por qué funciona cuando otros scripts fallan?

**Problema común:**
```
Error: You must pass a valid patch document in the body of the request
```

**Solución implementada:**
```powershell
# ❌ INCORRECTO (falla en Azure DevOps)
$body = $ops | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body

# ✅ CORRECTO (funciona siempre)
$body = $ops | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri $url -Headers $headers -Method Post `
	-Body ([System.Text.Encoding]::UTF8.GetBytes($body)) `
	-ContentType "application/json-patch+json; charset=utf-8"
```

### Estructura del Payload
```powershell
$ops = @(
	@{op="add"; path="/fields/System.Title"; value=$Titulo}
	@{op="add"; path="/fields/System.AreaPath"; value="tandem2026"}
)

if ($Descripcion) {
	$ops += @{op="add"; path="/fields/System.Description"; value=$Descripcion}
}

if ($StoryPoints -gt 0) {
	$ops += @{op="add"; path="/fields/Microsoft.VSTS.Scheduling.StoryPoints"; value=$StoryPoints}
}
```

## 📊 Escala de Story Points Recomendada

| Puntos | Complejidad | Tiempo Estimado | Ejemplo |
|--------|-------------|-----------------|---------|
| 1-2 | Trivial | < 2 horas | Cambiar texto, agregar tooltip |
| 3-5 | Simple | 2-8 horas | Formulario básico, filtros simples |
| 8 | Media | 1-2 días | Algoritmo complejo, integración API |
| 13 | Alta | 3-5 días | Sistema completo, refactorización mayor |

## 🔗 Integración con Workflow

### Flujo Completo de Trabajo
```powershell
# 1. Crear US
$usId = .\Create-US-Fast.ps1 -Titulo "Nueva funcionalidad" -StoryPoints 8

# 2. Desarrollar (GitHub Copilot hace el trabajo)
# ...

# 3. Commit con vínculo
git commit -m "feat: Implementar nueva funcionalidad AB#$usId"

# 4. Mover a Done
.\Edit-US.ps1 -ID $usId -Estado "Done"

# 5. Adjuntar documentación (si existe)
.\Attach-Document.ps1 -WorkItemId $usId -FilePath "US-$usId-DOCS.md" -Comment "Documentación técnica"
```

## ⚠️ Troubleshooting

### Error: "No se puede ejecutar este script"
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force
```

### Error: "Variable PAT no existe"
**Solución:** El PAT está hardcodeado en el script (línea 6). No requiere variables de entorno.

### US creada sin Story Points
**Causa:** Parámetro `-StoryPoints` omitido o con valor 0  
**Solución:** Asegúrate de incluir `-StoryPoints <número>` en el comando

### Navegador no abre automáticamente
**Causa:** El script intentó abrir pero el objeto `$result._links.html.href` estaba vacío  
**Solución:** El ID se retorna correctamente. Puedes abrir manualmente:
```
https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/<ID>
```

## 📝 Comparación con US.ps1

| Característica | Create-US-Fast.ps1 | US.ps1 |
|----------------|-------------------|--------|
| **Velocidad** | ⚡ < 20 seg | ⏳ Variable |
| **Story Points** | ✅ Sí | ❌ No |
| **Encoding UTF-8** | ✅ Correcto | ❌ Problemas |
| **Crea Tasks automáticas** | ❌ No | ✅ Sí (Develop, Test, CR) |
| **Estabilidad** | ✅ Alta | ⚠️ Media |

**Recomendación:** Usa `Create-US-Fast.ps1` para creación rápida de US. Usa `US.ps1` si necesitas las tasks automáticas.

## 🔄 Historial de Cambios

| Fecha | Versión | Cambio |
|-------|---------|--------|
| 2026-04-25 | 1.0 | Creación inicial. Solución encoding UTF-8 y soporte Story Points |

## 📞 Referencias

- **Documentación completa:** `C:\00_Tandem2026\Docs\General\GESTION-PANEL-AZURE-DEVOPS.md`
- **Script Edit-US:** `C:\00_Tandem2026\Scripts\Edit-US.ps1`
- **Script Attach-Document:** `C:\00_Tandem2026\Scripts\Attach-Document.ps1`
- **Azure DevOps REST API:** https://learn.microsoft.com/rest/api/azure/devops/wit/

---

**🎯 Tip:** Crea un alias para usar el script más rápido:
```powershell
# Agregar al perfil de PowerShell ($PROFILE)
function New-US {
	param($Titulo, $Desc = "", $Points = 0)
	C:\00_Tandem2026\Scripts\Create-US-Fast.ps1 -Titulo $Titulo -Descripcion $Desc -StoryPoints $Points
}

# Uso:
New-US "Mi US" "Descripción" 8
```
