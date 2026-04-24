# 🔀 Git Workflow

Flujo de trabajo con Git y GitHub para Tandem 2026.

---

## 📋 Información del Repositorio

**Repositorio:** https://github.com/JuanGodoyLopez/Tandem-2026  
**Branch principal:** `master`  
**Remote:** `origin`  
**Ubicación local:** `C:\00_Tandem2026\`

---

## 🌿 Estrategia de Branches

### **Branch Principal: `master`**

- **Propósito:** Código estable y funcional
- **Protección:** Commits directos permitidos (proyecto pequeño)
- **Deployment:** Manual

**Reglas:**
- ✅ Solo código que compila
- ✅ Testing básico realizado
- ✅ Commits con mensajes descriptivos

---

### **Feature Branches (Opcional)**

**Formato:** `feature/<nombre>`

**Ejemplo:**
```bash
git checkout -b feature/ribbon-icons
# ... trabajo ...
git push origin feature/ribbon-icons
```

**Cuándo usar:**
- Trabajo experimental
- Cambios grandes que tardan días
- Colaboración en features específicas

**Merge:**
```bash
git checkout master
git merge feature/ribbon-icons
git branch -d feature/ribbon-icons
git push origin --delete feature/ribbon-icons
```

---

## 📝 Commits

### **Formato de Mensaje**

```
<tipo>: <descripción> AB#<ID>

[cuerpo opcional]
```

---

### **Tipos de Commit**

| Tipo | Propósito | Ejemplo |
|------|-----------|---------|
| `feat` | Nueva funcionalidad | `feat: Agregar comando de anotación AB#612` |
| `fix` | Corrección de bug | `fix: Corregir validación de nombre AB#615` |
| `docs` | Solo documentación | `docs: Actualizar README del plugin AB#612` |
| `refactor` | Refactorización | `refactor: Simplificar ProjectRepository` |
| `test` | Agregar/modificar tests | `test: Agregar pruebas de validación` |
| `chore` | Tareas de mantenimiento | `chore: Actualizar paquetes NuGet` |
| `style` | Formato, espacios | `style: Aplicar convenciones de código` |
| `perf` | Mejora de performance | `perf: Optimizar consulta de proyectos` |

---

### **Ejemplos de Buenos Commits**

```bash
# ✅ Feature con User Story
git commit -m "feat: Implementar sistema de iconos para ribbon AB#612"

# ✅ Fix con descripción
git commit -m "fix: Corregir error al cargar menu CUI AB#615

El archivo Tandem2026.cui no se copiaba a bin/Debug.
Agregado PostBuildEvent al .csproj."

# ✅ Docs
git commit -m "docs: Crear guía de Azure DevOps AB#612"

# ✅ Refactor
git commit -m "refactor: Extraer lógica de validación a helper común"

# ✅ Multiple files
git commit -m "feat: Agregar MainWindow con MVVM AB#612

- Crear MainWindow.xaml
- Crear MainWindowViewModel.cs
- Registrar comando TANDEM_ABRIR_PANEL"
```

---

### **Ejemplos de Malos Commits**

```bash
# ❌ Muy genérico
git commit -m "cambios"
git commit -m "fix"
git commit -m "update"

# ❌ Sin contexto
git commit -m "corregido"

# ❌ Múltiples cambios sin relación
git commit -m "fix menu, add icons, update docs, refactor service"
```

---

## 🔗 Vincular Commits a Azure DevOps

### **Sintaxis `AB#<ID>`**

**Agregar al final del título o en el cuerpo:**

```bash
# ✅ En título
git commit -m "feat: Nuevo sistema de login AB#612"

# ✅ En cuerpo
git commit -m "feat: Nuevo sistema de login

Implementación completa del flujo de autenticación.

AB#612"

# ✅ Múltiples Work Items
git commit -m "feat: Sistema completo AB#612 AB#615 AB#620"
```

---

### **Palabras Clave Especiales**

| Palabra | Acción en Azure DevOps |
|---------|------------------------|
| `AB#612` | Solo vincula el commit |
| `Fixes AB#612` | Marca como "Resolved" al merge |
| `Closes AB#612` | Marca como "Closed" al merge |
| `Resolves AB#612` | Marca como "Resolved" al merge |

**Ejemplos:**
```bash
git commit -m "feat: Implementar iconos Fixes AB#612"
git commit -m "fix: Corregir bug crítico Closes AB#615"
```

---

## 🚀 Workflow Diario

### **1. Comenzar el Día**

```bash
# Actualizar repositorio
cd C:\00_Tandem2026
git pull origin master
```

---

### **2. Trabajar en Features**

```bash
# Ver estado
git status

# Ver cambios
git diff

# Agregar archivos
git add ZwcadPlugin/Commands/PanelCommands.cs
git add ZwcadPlugin/UI/MainWindow.xaml

# O agregar todos
git add .

# Commit
git commit -m "feat: Agregar comando crear panel AB#612"
```

---

### **3. Subir Cambios**

```bash
# Push al remoto
git push origin master

# Verificar en GitHub
# https://github.com/JuanGodoyLopez/Tandem-2026/commits/master
```

---

### **4. Verificar Vínculo con Azure DevOps**

1. Ve a Azure DevOps: https://dev.azure.com/VSCAD/tandem2026
2. Abre la User Story (ej: #612)
3. Sección **"Development"** (lado derecho)
4. Deberías ver el commit vinculado

---

## 🔄 Operaciones Comunes

### **Deshacer Cambios No Commiteados**

```bash
# Descartar cambios en archivo específico
git checkout -- ZwcadPlugin/Commands/PanelCommands.cs

# Descartar TODOS los cambios
git reset --hard HEAD
```

---

### **Modificar Último Commit**

```bash
# Olvidaste agregar un archivo
git add archivo-olvidado.cs
git commit --amend --no-edit

# Cambiar mensaje del último commit
git commit --amend -m "feat: Nuevo mensaje correcto AB#612"
```

⚠️ **Solo si NO has hecho push todavía**

---

### **Ver Historial**

```bash
# Últimos 10 commits
git log --oneline -10

# Con cambios
git log -p -2

# Grafo de branches
git log --oneline --graph --all
```

---

### **Buscar en Historial**

```bash
# Buscar por mensaje
git log --grep="AB#612"

# Buscar por autor
git log --author="Juan"

# Buscar cambios en archivo
git log -- ZwcadPlugin/Commands/PanelCommands.cs
```

---

### **Ver Cambios de un Commit**

```bash
# Ver cambios del último commit
git show

# Ver commit específico
git show a1b2c3d

# Ver archivos cambiados
git show --name-only a1b2c3d
```

---

### **Stash (Guardar Cambios Temporalmente)**

```bash
# Guardar cambios sin commit
git stash

# Ver stashes
git stash list

# Recuperar último stash
git stash pop

# Aplicar stash específico
git stash apply stash@{0}
```

**Uso común:**
```bash
# Tienes cambios pero necesitas pull
git stash
git pull origin master
git stash pop
```

---

## 🏷️ Tags

### **Crear Release Tags**

```bash
# Tag simple
git tag v1.0.0

# Tag con mensaje
git tag -a v1.0.0 -m "Release 1.0.0 - Sistema de iconos completo"

# Push tag
git push origin v1.0.0

# Push todos los tags
git push origin --tags
```

---

### **Listar Tags**

```bash
git tag
git tag -l "v1.*"
```

---

## 🔍 Resolución de Conflictos

### **Cuando Ocurren**

```bash
git pull origin master
# Auto-merging file.cs
# CONFLICT (content): Merge conflict in file.cs
```

---

### **Resolver Manualmente**

1. **Abrir archivo con conflicto:**

```csharp
<<<<<<< HEAD
// Tu código
public void Method1() { }
=======
// Código remoto
public void Method2() { }
>>>>>>> origin/master
```

2. **Editar y decidir qué mantener:**

```csharp
// Decisión: mantener ambos
public void Method1() { }
public void Method2() { }
```

3. **Marcar como resuelto:**

```bash
git add file.cs
git commit -m "merge: Resolver conflicto en file.cs"
git push origin master
```

---

### **Prevenir Conflictos**

- ✅ Pull frecuentemente
- ✅ Commits pequeños y frecuentes
- ✅ Comunicar cambios grandes
- ✅ Evitar editar mismos archivos simultáneamente

---

## 🧹 Limpieza

### **Ver Archivos No Trackeados**

```bash
git status
git clean -n  # Preview
git clean -f  # Ejecutar
```

---

### **Actualizar .gitignore**

```bash
# Editar .gitignore
notepad .gitignore

# Refrescar índice
git rm -r --cached .
git add .
git commit -m "chore: Actualizar .gitignore"
```

---

## 📊 Estadísticas

### **Contribuciones**

```bash
# Commits por autor
git shortlog -sn

# Estadísticas del repo
git log --stat

# Líneas agregadas/eliminadas
git log --shortstat
```

---

## 🔐 Configuración

### **Usuario y Email**

```bash
# Ver configuración actual
git config user.name
git config user.email

# Configurar globalmente
git config --global user.name "Tu Nombre"
git config --global user.email "tu@email.com"

# Solo para este repo
git config user.name "Tu Nombre"
git config user.email "tu@email.com"
```

---

### **Editor Predeterminado**

```bash
# Visual Studio Code
git config --global core.editor "code --wait"

# Notepad++
git config --global core.editor "'C:/Program Files/Notepad++/notepad++.exe' -multiInst -notabbar -nosession -noPlugin"
```

---

## 🚫 Qué NO Subir a Git

**Ya configurado en `.gitignore`:**

```
.vs/                  # Visual Studio cache
bin/                  # Compilación
obj/                  # Compilación
packages/             # NuGet packages
*.user                # Configuración personal
*.suo                 # Estado Visual Studio
MNU/Iconos/Bootstrap-Icons/icons/*  # Biblioteca completa
```

**NUNCA subir:**
- ❌ Contraseñas o tokens
- ❌ Archivos compilados (DLL, EXE)
- ❌ Archivos temporales
- ❌ Configuración personal del IDE
- ❌ Bibliotecas de terceros (usar NuGet)

---

## 📖 Recursos

### **Comandos de Ayuda**

```bash
# Ayuda general
git help

# Ayuda de comando específico
git help commit
git help log
```

---

### **Enlaces Útiles**

- **GitHub:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Azure DevOps:** https://dev.azure.com/VSCAD/tandem2026
- **Commits:** https://github.com/JuanGodoyLopez/Tandem-2026/commits/master
- **Git Docs:** https://git-scm.com/doc

---

## ❓ Preguntas Frecuentes

### **¿Cómo ver mis cambios antes de commit?**
```bash
git status           # Archivos modificados
git diff             # Cambios detallados
git diff --staged    # Cambios en staging
```

### **¿Cómo deshacer el último commit?**
```bash
# Mantener cambios en working directory
git reset --soft HEAD~1

# Descartar cambios también
git reset --hard HEAD~1
```
⚠️ Solo si NO has hecho push

### **¿Cómo cambiar de branch?**
```bash
git checkout nombre-branch
git checkout -b nuevo-branch  # Crear y cambiar
```

### **¿Cómo actualizar mi fork?**
```bash
# Si trabajas con fork (no es el caso actual)
git remote add upstream <url-original>
git fetch upstream
git merge upstream/master
```

### **¿Commits frecuentes o grandes commits?**
**✅ Frecuentes:**
- Más fácil de revisar
- Mejor historial
- Más fácil de revertir
- Mejores mensajes

**Regla:** Un commit = un cambio lógico completo

---

## 🔧 PowerShell Aliases (Opcional)

**Agregar a tu `$PROFILE`:**

```powershell
# Abrir profile
notepad $PROFILE

# Agregar aliases
function gs { git status }
function ga { git add . }
function gc { param($msg) git commit -m $msg }
function gp { git push origin master }
function gl { git log --oneline -10 }
```

**Uso:**
```powershell
gs              # git status
ga              # git add .
gc "mensaje"    # git commit -m "mensaje"
gp              # git push origin master
gl              # git log
```

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
