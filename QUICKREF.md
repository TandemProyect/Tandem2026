# ⚡ Comandos Rápidos - Cheat Sheet

> Referencia rápida de comandos más usados en el proyecto Tandem 2026

---

## 🏥 DIAGNÓSTICO

### Verificar estado del proyecto
```powershell
.\Scripts\HealthCheck.ps1
```

### Ver estado del board
```powershell
.\Scripts\Verificar-Board.ps1
```

### Ver columnas actuales
```powershell
.\Scripts\Ver-Board.ps1
```

---

## 📝 WORK ITEMS (Azure DevOps)

### Crear User Story
```powershell
.\Scripts\US.ps1 "Título de la US" "Descripción opcional"
# Devuelve el ID de la US creada
```

### Editar User Story
```powershell
# Cambiar título
.\Scripts\Edit-US.ps1 615 -Titulo "Nuevo título"

# Cambiar descripción
.\Scripts\Edit-US.ps1 615 -Descripcion "Nueva descripción"

# Cambiar estado
.\Scripts\Edit-US.ps1 615 -Estado "Done"

# Cambiar prioridad
.\Scripts\Edit-US.ps1 615 -Prioridad 1

# Múltiples cambios
.\Scripts\Edit-US.ps1 615 -Titulo "Nuevo" -Estado "Doing" -Prioridad 2
```

### Estados válidos
- `To Do`
- `Doing`
- `Done`

---

## 🔄 GIT

### Ver commits recientes
```powershell
git log --oneline -10
```

### Ver estado actual
```powershell
git status
```

### Commit con enlace a US
```powershell
git add .
git commit -m "feat: Descripción del cambio AB#615"
git push origin master
```

### Ver historial de un archivo
```powershell
git log --oneline -- ruta/archivo.cs
```

---

## 📚 DOCUMENTACIÓN

### Leer documentación de continuidad
```powershell
Get-Content CONTINUITY.md | more
```

### Buscar en documentación
```powershell
Get-ChildItem -Path Docs -Recurse -Filter *.md | Select-String "palabra clave"
```

### Listar toda la documentación
```powershell
Get-ChildItem -Path Docs -Recurse -Filter *.md | Select-Object FullName
```

---

## 🔍 BÚSQUEDA

### Buscar en código
```powershell
Get-ChildItem -Recurse -Include *.cs,*.xaml | Select-String "texto a buscar"
```

### Buscar archivos por nombre
```powershell
Get-ChildItem -Recurse -Filter "*nombre*"
```

### Ver estructura del proyecto
```powershell
tree /F
```

---

## 🏗️ BUILD

### Compilar solución
```powershell
msbuild Design.sln /t:Build /p:Configuration=Release
```

### Limpiar solución
```powershell
msbuild Design.sln /t:Clean
```

---

## 📊 AZURE DEVOPS (Manual)

### Ver board
```
https://dev.azure.com/VSCAD/tandem2026/_boards/board/t/tandem2026%20Team/Issues
```

### Ver proyecto
```
https://dev.azure.com/VSCAD/tandem2026
```

### Ver work items
```
https://dev.azure.com/VSCAD/tandem2026/_workitems
```

---

## 🛠️ VISUAL STUDIO

### Abrir solución
```powershell
Start-Process Design.sln
```

### Abrir VS Code en la carpeta actual
```powershell
code .
```

---

## 💡 TIPS

### Ejecutar script sin restricciones
```powershell
powershell -ExecutionPolicy Bypass -File .\Scripts\script.ps1
```

### Ver ayuda de un script
```powershell
Get-Help .\Scripts\US.ps1 -Detailed
```

### Alias útiles (agregar a $PROFILE)
```powershell
function us { .\Scripts\US.ps1 $args }
function edit-us { .\Scripts\Edit-US.ps1 $args }
function check { .\Scripts\HealthCheck.ps1 }
```

---

## 📖 DOCUMENTOS CLAVE

| Documento | Para qué sirve |
|-----------|----------------|
| `CONTINUITY.md` | Contexto completo para futuros agentes |
| `README.md` | Descripción general del proyecto |
| `Docs/README.md` | Índice de documentación |
| `Docs/General/Azure-DevOps.md` | Proceso y automatización Azure DevOps |
| `Docs/General/Git-Workflow.md` | Workflow Git y mejores prácticas |
| `Docs/General/Convenciones.md` | Estándares de código |

---

## 🚨 SOLUCIÓN DE PROBLEMAS

### Error: "No se puede ejecutar el script"
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### Error: "Git no reconocido"
```powershell
# Verificar instalación
git --version
# Si no está instalado: https://git-scm.com/download/win
```

### Error: "PAT inválido o expirado"
- Revisar PAT en `Scripts/US.ps1` línea 9
- Generar nuevo PAT en Azure DevOps si es necesario
- Actualizar todos los scripts con el nuevo PAT

### Error: "No se puede conectar a Azure DevOps"
- Verificar conexión a internet
- Verificar PAT
- Ejecutar `.\Scripts\HealthCheck.ps1` para diagnóstico

---

## 🎯 WORKFLOWS COMUNES

### Workflow 1: Crear nueva feature
```powershell
# 1. Crear US
.\Scripts\US.ps1 "Implementar función X"
# Anota el ID (ej: 616)

# 2. Desarrollar
# ... código ...

# 3. Commit
git add .
git commit -m "feat: Implementar función X AB#616"
git push origin master

# 4. Marcar como Done
.\Scripts\Edit-US.ps1 616 -Estado "Done"
```

### Workflow 2: Verificar y diagnosticar
```powershell
# 1. Health check general
.\Scripts\HealthCheck.ps1

# 2. Ver estado del board
.\Scripts\Verificar-Board.ps1

# 3. Ver commits recientes
git log --oneline -10

# 4. Ver archivos modificados
git status
```

### Workflow 3: Actualizar documentación
```powershell
# 1. Editar archivo
code CONTINUITY.md

# 2. Commit
git add CONTINUITY.md
git commit -m "docs: Actualizar contexto de continuidad AB#613"
git push origin master
```

---

**Última actualización:** 2026-04-25
**Versión:** 1.0
