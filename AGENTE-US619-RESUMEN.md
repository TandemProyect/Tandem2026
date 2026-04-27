# 🚀 RESUMEN RÁPIDO - US-619 para Nuevo Agente

## Contexto
- **US-619:** "Insertar Img en Command Seleccionar Muro"
- **Estado:** Doing
- **Problema:** El comando funciona pero el icono no aparece en el ribbon de ZWCAD
- **Causa:** PNG corrupto (243 bytes) y rutas incorrectas en CuixBuilder.cs

## Solución Validada
ZWCAD solo muestra iconos desde: `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\`
Y el CUI debe usar solo el nombre: `<LargeImage>SelectLines.png</LargeImage>`

## Archivos Clave
1. `TamdenZwcadPluging/ZwcadPlugin/CuixBuilder.cs` - Cambiar rutas de iconos
2. `TamdenZwcadPluging/ZwcadPlugin/MNU/img/SelectLines.png` - Generar PNG válido 32x32
3. Copiar PNG a: `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\SelectLines.png`

## Documentación Completa
- **Archivo detallado:** `C:\00_Tandem2026\AGENTE-US619-INFO.md` (leer primero)
- **Investigación:** `Docs/Proyectos/ZwcadPlugin/INVESTIGACION_ICONOS_US619.md`
- **Gestión Azure DevOps:** `Docs/General/GESTION-PANEL-AZURE-DEVOPS.md`

## Comandos Útiles
```powershell
# Ver US-619 en panel
Start-Process "https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/619"

# Mover a Done al terminar
cd C:\00_Tandem2026\Scripts
.\Edit-US.ps1 -ID 619 -Estado "Done"

# Adjuntar documentación
.\Attach-Document.ps1 -WorkItemId 619 -FilePath "US-619-IMPLEMENTACION.md" -Comment "Implementación icono"
```

## Workspace
- **Ruta:** C:\00_Tandem2026\
- **Usuario Git:** jag@vscad.com
- **Repo:** https://github.com/JuanGodoyLopez/Tandem-2026

---
**Lee el archivo completo AGENTE-US619-INFO.md para todos los detalles técnicos.**
