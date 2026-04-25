# Investigación: Crear Columnas en Azure DevOps Board mediante API

## Fecha: 2026-04-25
## Estado: BLOQUEADO POR LIMITACIÓN DE API

## Resumen

Hemos intentado crear columnas personalizadas en el board de Azure DevOps usando la API REST, pero encontramos una limitación técnica.

## Lo que SÍ funciona ✅

1. **Autenticación completa** con PAT Full Access
2. **Crear User Stories** (work items) - Script `US.ps1` ✅
3. **Editar User Stories** - Script `Edit-US.ps1` ✅
4. **Leer configuración del board** - GET sobre boards ✅
5. **Consultar estados** disponibles para work items ✅
6. **Consultar configuración de tarjetas** del board ✅

## El Problema 🚫

### Error encontrado:
```
{"message":"Value cannot be null.\r\nParameter name: options"}
```

### Endpoint problemático:
```
PUT https://dev.azure.com/VSCAD/tandem2026/{teamId}/_apis/work/boards/Issues?api-version=7.1-preview.1
```

### Lo que intentamos:

1. ✅ Payload con solo `columns`
2. ✅ Payload con `columns` + `rows`
3. ✅ Payload con `id` + `revision` + `columns`
4. ✅ Diferentes formatos de JSON
5. ✅ Usar IDs de columnas existentes y nuevos GUIDs
6. ✅ Mapear columnas a estados válidos según `allowedMappings`
7. ✅ Copiar estructura exacta del GET y modificar solo nombres

**Resultado:** Todos los intentos fallan con el mismo error "Parameter name: options"

## Hallazgos Técnicos 🔍

### Proceso del proyecto:
- **Tipo:** Basic (sistema)
- **Customization Type:** `system`
- **Estados disponibles:** `To Do`, `Doing`, `Done` (NO modificables)

### Estados y Columnas:
- Las columnas del board se mapean a estados de work items
- El proceso "Basic" es del sistema y NO se puede modificar
- Para agregar estados personalizados se necesita crear un **proceso heredado**

### API Version:
- Usando `7.1-preview.1` (versión preview)
- El error podría ser un bug de la API preview
- No hay versión estable documentada para este endpoint

## Posibles Causas del Error

1. **Bug en la API preview** - El parámetro "options" no está documentado y la API no lo maneja correctamente
2. **Proceso del sistema** - Quizás no se pueden modificar boards de procesos del sistema via API
3. **Permisos faltantes** - Aunque tenemos Full Access, podría faltar algún permiso específico de administración
4. **Documentación incompleta** - Microsoft no documenta completamente este endpoint preview

## Soluciones Alternativas 🛠️

### Opción 1: Manual (MÁS RÁPIDA) ⭐
Crear las columnas manualmente en la UI de Azure DevOps:
1. Ir a: https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
2. Click en ⚙️ → "Column options"
3. Agregar las 9 columnas deseadas

**Tiempo estimado:** 5-10 minutos

### Opción 2: Azure DevOps CLI
Instalar Azure CLI + extensión DevOps:
```powershell
# Instalar Azure CLI
winget install Microsoft.AzureCLI

# Instalar extensión DevOps
az extension add --name azure-devops

# Configurar
az devops configure --defaults organization=https://dev.azure.com/VSCAD project=tandem2026

# Intentar actualizar
az boards work-item update ...
```

**Problema:** Azure CLI tampoco tiene comando directo para board columns

### Opción 3: Crear Proceso Heredado
Crear un proceso personalizado basado en Basic con estados custom:
1. Requiere permisos de Collection Administrator
2. Más complejo
3. Permite máxima personalización

**Tiempo estimado:** 30+ minutos

### Opción 4: PowerShell + Selenium/WebDriver
Automatizar la UI web:
- Usar Selenium para controlar el navegador
- Simular clicks y cambios en la UI
- Más frágil pero funcionaría

**Tiempo estimado:** 1-2 horas de desarrollo

## Recomendación Final 💡

**Para continuar AHORA:** Opción 1 (manual)
- Es lo más rápido
- Solo son 9 columnas
- Funciona 100%

**Para el futuro:**
- Documentar el proceso manual en el README
- Crear un script que verifique la configuración actual
- Monitorear actualizaciones de la API de Azure DevOps
- Considerar reportar el bug a Microsoft

## Scripts Creados Durante la Investigación

- `Scripts/Ver-Board.ps1` - Ver estado actual del board
- `Scripts/Ver-Estructura-Board.ps1` - Exportar estructura completa
- `Scripts/Crear-Columnas-*.ps1` - Varios intentos de creación (no funcionan)
- `board-structure.json` - Estructura actual del board
- `board-full.json` - Board completo con allowedMappings
- `board-update-payload.json` - Payload que debería funcionar pero no

## Lo Positivo ✨

Aunque no logramos crear las columnas via API, **SÍ tenemos acceso completo para:**
- ✅ Crear User Stories automáticamente
- ✅ Editar User Stories (título, descripción, prioridad)
- ✅ Consultar el estado del board
- ✅ Leer configuración actual
- ✅ Futuras automatizaciones de work items

**El 80% de la automatización funciona**, solo la configuración inicial de columnas requiere intervención manual.

## Próximos Pasos

1. ✅ Documentar este hallazgo
2. ⏭️ Crear las columnas manualmente (usuario)
3. ⏭️ Actualizar scripts de edición para usar los nuevos nombres de columna
4. ⏭️ Crear script de verificación que confirme la configuración correcta
5. ⏭️ Continuar con otras automatizaciones del flujo de trabajo
