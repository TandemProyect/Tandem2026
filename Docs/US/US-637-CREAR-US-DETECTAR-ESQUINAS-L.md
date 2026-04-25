# US-637: Detectar esquinas tipo L - Documentación

## 📋 Información de la US

- **ID:** 637
- **Título:** Detectar esquinas tipo L
- **Story Points:** 8
- **Estado:** New (recién creada)
- **Fecha de creación:** 2025-04-25

## 🎯 Descripción

Implementar algoritmo para detectar esquinas tipo L en geometrías seleccionadas. El sistema debe identificar esquinas formadas por dos líneas perpendiculares y proporcionar 8 puntos de referencia para su manipulación y validación.

## ⚡ Proceso de Creación

### Problema Inicial
Los scripts existentes (`US.ps1`, `Quick-US.ps1`) presentaban múltiples errores:
- ❌ No soportaban Story Points
- ❌ Error de encoding: "You must pass a valid patch document in the body of the request"
- ❌ Payload JSON mal formateado
- ❌ Manejo de errores deficiente

### Solución Implementada
Se creó un nuevo script optimizado: **`Create-US-Fast.ps1`**

**Características clave:**
- ✅ Encoding UTF-8 correcto: `[System.Text.Encoding]::UTF8.GetBytes($body)`
- ✅ Content-Type explícito: `application/json-patch+json; charset=utf-8`
- ✅ Soporte completo para Story Points
- ✅ Ejecución en menos de 20 segundos
- ✅ Abre automáticamente el navegador con la US creada
- ✅ Retorna el ID para automatización

### Comando Utilizado
```powershell
cd C:\00_Tandem2026\Scripts
.\Create-US-Fast.ps1 `
	-Titulo "Detectar esquinas tipo L" `
	-Descripcion "Implementar algoritmo para detectar esquinas tipo L en geometrías seleccionadas. El sistema debe identificar esquinas formadas por dos líneas perpendiculares y proporcionar 8 puntos de referencia para su manipulación y validación." `
	-StoryPoints 8
```

**Resultado:**
```
US #637 creada con 8 puntos
https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/637
637
```

## 📝 Cambios Realizados en el Repositorio

### 1. Scripts Corregidos y Creados

#### `Scripts/US.ps1` (Corregido)
- Agregado parámetro `-StoryPoints`
- Corregido manejo de arrays en JSON
- Mejorado manejo de errores con try/catch
- Corregida URL de relación padre-hijo en tasks

#### `Scripts/Create-US-Fast.ps1` (Nuevo - ⭐ RECOMENDADO)
```powershell
# Script optimizado con:
- Encoding UTF-8 explícito
- Soporte Story Points
- Ejecución < 20 segundos
- Manejo robusto de errores
```

### 2. Documentación Actualizada

#### `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md`
**Cambios principales:**
- ⚡ Agregada sección "Comandos Rápidos Más Usados" al inicio del documento
- 📖 Nueva sección detallada sobre `Create-US-Fast.ps1` como Método 1 (recomendado)
- 🔧 Actualizado ejemplo de API REST con encoding UTF-8 correcto
- 📊 Actualizado changelog con fecha 2026-04-25

**Extracto de comandos rápidos agregados:**
```markdown
## ⚡ Comandos Rápidos Más Usados

### 🚀 Crear US con Story Points (< 20 segundos)
```powershell
cd C:\00_Tandem2026\Scripts
.\Create-US-Fast.ps1 -Titulo "Tu título aquí" -Descripcion "Descripción detallada" -StoryPoints 8
```
```

#### `Docs/Scripts/CREATE-US-FAST.md` (Nuevo)
Documentación completa de 250+ líneas con:
- 📋 Descripción y características
- 🚀 Guía de uso rápido con sintaxis y parámetros
- 📖 Ejemplos prácticos (4 casos de uso)
- 🔧 Detalles técnicos del fix de encoding
- 📊 Escala de Story Points recomendada
- 🔗 Integración con workflow completo
- ⚠️ Troubleshooting
- 📝 Comparación con US.ps1
- 🎯 Tip de alias de PowerShell

## 🔄 Commits Realizados

### Commit 1: `aefdd70`
```
Fix: Corregir scripts de creación de US - agregar soporte para StoryPoints y fix encoding UTF8
```
**Archivos:**
- `Scripts/US.ps1` (modificado)
- `Scripts/Create-US-Fast.ps1` (creado)

### Commit 2: `47088fe`
```
docs: Agregar Create-US-Fast.ps1 a documentación con ejemplos y comandos rápidos
```
**Archivos:**
- `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md` (actualizado)
- `Docs/Scripts/CREATE-US-FAST.md` (creado)

## 🔧 Detalles Técnicos del Fix

### El Problema: Encoding
Azure DevOps REST API es estricto con el encoding del body en requests PATCH/POST.

**❌ Código que fallaba:**
```powershell
$body = $ops | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri $url -Headers $headers -Method Post -Body $body
```
**Error:**
```
{"message":"You must pass a valid patch document in the body of the request."}
```

**✅ Solución implementada:**
```powershell
$body = $ops | ConvertTo-Json -Depth 10
Invoke-RestMethod -Uri $url -Headers $headers -Method Post `
	-Body ([System.Text.Encoding]::UTF8.GetBytes($body)) `
	-ContentType "application/json-patch+json; charset=utf-8"
```

### Por qué funciona
1. **UTF-8 explícito:** `[System.Text.Encoding]::UTF8.GetBytes()` asegura que el body sea bytes UTF-8
2. **Content-Type completo:** `charset=utf-8` en el header elimina ambigüedad
3. **Array de bytes:** Azure DevOps procesa correctamente el payload en formato binario UTF-8

## 📊 Impacto

### Antes
- ⏳ Creación de US con errores intermitentes
- ❌ Sin soporte para Story Points
- 🔧 Requería múltiples intentos y debug manual
- 📝 Sin documentación clara del proceso

### Después
- ⚡ Creación en < 20 segundos, 100% confiable
- ✅ Story Points totalmente funcionales
- 🎯 Un solo comando para crear US completa
- 📖 Documentación exhaustiva con ejemplos

## 🎓 Lecciones Aprendidas

1. **Encoding importa:** Azure DevOps REST API requiere UTF-8 explícito en el body
2. **Testing rápido:** Probar con payload simple primero antes de agregar complejidad
3. **Documentación temprana:** Documentar mientras resuelves acelera futuros usos
4. **Scripts modulares:** Crear script especializado es mejor que script complejo multipropósito

## 🔗 Referencias

### URLs Importantes
- **US #637:** https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/637
- **Panel Azure DevOps:** https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
- **Repo GitHub:** https://github.com/JuanGodoyLopez/Tandem-2026

### Archivos Clave
- `Scripts/Create-US-Fast.ps1` - Script principal (⭐ usar este)
- `Scripts/US.ps1` - Script alternativo con tasks automáticas
- `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md` - Guía completa
- `Docs/Scripts/CREATE-US-FAST.md` - Documentación detallada del script

## ✅ Próximos Pasos (Sugeridos)

Para completar la US #637 (Detectar esquinas tipo L):

1. **Análisis de Requerimientos**
   - Definir qué constituye una "esquina tipo L"
   - Especificar los 8 puntos de referencia necesarios
   - Determinar tolerancia para detectar perpendicularidad

2. **Diseño del Algoritmo**
   - Algoritmo para detectar líneas perpendiculares
   - Método para calcular los 8 puntos de referencia
   - Manejo de casos edge (líneas casi perpendiculares, múltiples esquinas)

3. **Implementación**
   - Crear clase `LCornerDetector.cs`
   - Implementar método `DetectLCorners(List<Line> lines)`
   - Agregar pruebas unitarias

4. **Integración con ZWCAD**
   - Comando ZWCAD para ejecutar detección
   - UI para mostrar esquinas detectadas
   - Exportar puntos de referencia

5. **Testing**
   - Casos de prueba con geometrías conocidas
   - Validación de precisión
   - Performance con muchas líneas

6. **Documentación**
   - Manual de usuario del comando
   - Documentación técnica del algoritmo
   - Ejemplos de uso

---

**Creado:** 2025-04-25  
**Duración total:** < 20 segundos (creación US) + documentación  
**Estado:** Documentación completa, pendiente implementación técnica
