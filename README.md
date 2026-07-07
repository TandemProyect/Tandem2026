# 🏗️ Tandem 2026

Sistema de gestión de proyectos integrado con ZWCAD 2026.

---

## 🤖 PARA AGENTES IA / FUTUROS CHATS

**👉 SI ERES UN AGENTE IA CONTINUANDO ESTE PROYECTO:**
**LEE PRIMERO → [`documentación/CONTINUITY.md`](documentación/CONTINUITY.md) ← LEE PRIMERO**

Este documento contiene:
- ✅ Todo el contexto de sesiones anteriores
- ✅ Qué funciona y qué no
- ✅ Scripts disponibles y cómo usarlos
- ✅ Limitaciones conocidas y soluciones
- ✅ Próximos pasos sugeridos

**NO pierdas tiempo re-investigando cosas ya resueltas.**

---

## 📋 Descripción

Tandem 2026 es un sistema completo que proporciona:
- Plugin para ZWCAD con comandos personalizados
- Gestión de proyectos y datos
- Interfaz de usuario WPF integrada
- Acceso a datos y lógica de negocio

---

## 🚀 Inicio Rápido

### **1. Clonar Repositorio**

```bash
git clone https://github.com/JuanGodoyLopez/Tandem-2026.git
cd Tandem-2026
```

---

### **2. Abrir en Visual Studio**

```bash
# Abrir solución
.\Design.sln
```

**Requisitos:**
- Visual Studio 2022 o superior
- .NET Framework 4.8
- ZWCAD 2026 (para plugin)

---

### **3. Compilar**

```bash
# Desde PowerShell
msbuild Design.sln /t:Rebuild /p:Configuration=Debug

# O desde Visual Studio: Ctrl+Shift+B
```

---

### **4. Probar Plugin ZWCAD**

1. Abrir ZWCAD 2026
2. Ejecutar comando: `NETLOAD`
3. Seleccionar: `TamdenZwcadPluging\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll`
4. Probar comando: `TANDEM_ABRIR_PANEL`

---

## 📂 Estructura del Proyecto

```
Tandem-2026/
├── Common/                 # Código compartido
├── DAL/                    # Data Access Layer
├── Desing/                 # Business Logic Layer
├── LinkedIn/               # Presencia publica de trdesing.net
├── TamdenZwcadPluging/
│   └── ZwcadPlugin/       # Plugin ZWCAD 2026
├── Scripts/                # Scripts PowerShell
├── documentación/          # 📚 Docs de agentes, handovers y guías
├── Docs/                   # 📚 Documentación técnica del proyecto
│   ├── General/           # Docs comunes
│   └── Proyectos/         # Docs específicas
└── Design.sln              # Solución principal
```

---

## 📚 Documentación

### **📖 Documentación General (para todos los proyectos)**

| Documento | Descripción |
|-----------|-------------|
| [**Azure DevOps**](Docs/General/Azure-DevOps.md) | Crear User Stories, vincular commits, gestionar board |
| [**Estructura del Repositorio**](Docs/General/Estructura-Repositorio.md) | Organización de carpetas y archivos |
| [**Common - Código Compartido**](Docs/General/Common.md) | Usar y mantener código compartido |
| [**Convenciones de Código**](Docs/General/Convenciones.md) | Estándares de codificación |
| [**Git Workflow**](Docs/General/Git-Workflow.md) | Flujo de trabajo con Git y GitHub |

---

### **🔧 Documentación por Proyecto**

| Proyecto | Documentación |
|----------|---------------|
| **DAL** | [Docs/Proyectos/DAL/](Docs/Proyectos/DAL/) |
| **Desing** | [Docs/Proyectos/Desing/](Docs/Proyectos/Desing/) |
| **ZwcadPlugin** | [Docs/Proyectos/ZwcadPlugin/](Docs/Proyectos/ZwcadPlugin/) |
| **LinkedIn / trdesing.net** | [Docs/Proyectos/LinkedIn/](Docs/Proyectos/LinkedIn/) |

**También disponible:**
- [ZwcadPlugin README](TamdenZwcadPluging/ZwcadPlugin/README.md)
- [ZwcadPlugin — Documentación](Docs/Proyectos/ZwcadPlugin/README.md)
- [ZwcadPlugin Guía Técnica](Docs/Proyectos/ZwcadPlugin/TECHNICAL_GUIDE.md)
- [Sistema de Iconos](Docs/Proyectos/ZwcadPlugin/Iconos/README_ICONOS.md)

---

## 🎯 Proyectos

### **1. Common** - Código Compartido
**Propósito:** Utilidades y recursos compartidos

**Contenido:**
- Utils, Models, Interfaces
- Constants, Extensions

**📖 [Ver documentación completa](Docs/General/Common.md)**

---

### **2. DAL** - Data Access Layer
**Propósito:** Acceso a datos y repositorios

**Características:**
- Patrón Repository
- Entity Framework
- Modelos de entidad

**📖 [Ver documentación completa](Docs/Proyectos/DAL/)**

---

### **3. Desing** - Business Logic Layer
**Propósito:** Lógica de negocio y servicios

**Características:**
- Servicios de negocio
- Validadores
- DTOs y procesadores

**📖 [Ver documentación completa](Docs/Proyectos/Desing/)**

---

### **4. ZwcadPlugin** - Plugin ZWCAD 2026
**Propósito:** Plugin para ZWCAD con UI

**Características:**
- Comandos personalizados
- Interfaz WPF (MVVM)
- Ribbon y menús CUI
- Integración con ZWCAD API

**📖 [Ver documentación completa](Docs/Proyectos/ZwcadPlugin/)**

---

### **5. LinkedIn / trdesing.net** - Laboratorio de diseño y producto
**Propósito:** Preparar la presencia pública de `trdesing.net` en LinkedIn

**Características:**
- Página de proyecto administrada desde cuenta personal
- Posicionamiento como laboratorio de diseño/producto
- Calendario de publicaciones y demos conceptuales
- Checklist de privacidad antes de publicar videos
- Carpeta operativa en [`LinkedIn/`](LinkedIn/) para posts, guiones, assets y seguimiento

**📖 [Ver documentación completa](Docs/Proyectos/LinkedIn/)**

---

## ⚡ Comandos ZWCAD Disponibles

| Comando | Descripción |
|---------|-------------|
| `TANDEM_ABRIR_PANEL` | Abrir ventana principal |
| `TANDEM_CREAR_PROYECTO` | Crear nuevo proyecto |
| `TANDEM_LISTAR_PROYECTOS` | Listar proyectos existentes |

**📖 [Ver todos los comandos](Docs/Proyectos/ZwcadPlugin/)**

---

## 🔧 Scripts PowerShell

### **Azure DevOps - Gestión Rápida**

```powershell
# Crear User Story
.\Scripts\US.ps1 "Título de la historia"
.\Scripts\US.ps1 "Título" "Descripción opcional"

# Editar User Story
.\Scripts\Edit-US.ps1 <ID> -Estado "Done"
.\Scripts\Edit-US.ps1 <ID> -Titulo "Nuevo título" -Prioridad 1
```

**📖 [Ver guía completa de Azure DevOps](Docs/General/Azure-DevOps.md)**

---

## 🔗 Enlaces

### **Repositorio y Proyecto**
- **GitHub:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Azure DevOps:** https://dev.azure.com/VSCAD/tandem2026
- **Panel de Trabajo:** https://dev.azure.com/VSCAD/tandem2026/_boards

---

### **Documentación Externa**
- **ZWCAD API:** Documentación incluida con ZWCAD SDK
- **Entity Framework:** https://docs.microsoft.com/ef/
- **WPF MVVM:** https://docs.microsoft.com/dotnet/desktop/wpf/

---

## 🛠️ Tecnologías

- **.NET Framework 4.8**
- **ZWCAD 2026 API**
- **WPF (MVVM)**
- **Entity Framework** (opcional)
- **PowerShell** (scripts)

---

## 📝 Flujo de Trabajo

### **1. Crear User Story en Azure DevOps**

```powershell
.\Scripts\US.ps1 "Implementar nueva funcionalidad"
# Output: Created Issue #625
```

---

### **2. Trabajar en el Código**

```bash
# Crear feature branch (opcional)
git checkout -b feature/nueva-funcionalidad

# ... implementar cambios ...

# Commit vinculado a User Story
git commit -m "feat: Implementar nueva funcionalidad AB#625"
```

**📖 [Ver guía completa de Git](Docs/General/Git-Workflow.md)**

---

### **3. Compilar y Probar**

```bash
# Compilar
msbuild Design.sln /t:Rebuild /p:Configuration=Debug

# Probar en ZWCAD (si aplica)
# - Abrir ZWCAD
# - NETLOAD → ZwcadPlugin.dll
# - Ejecutar comando
```

---

### **4. Push y Vincular**

```bash
# Push con vínculo a User Story
git push origin master
```

El commit aparecerá automáticamente en Azure DevOps → User Story #625 → Development

**📖 [Ver guía de vinculación Azure/GitHub](Docs/General/Azure-DevOps.md)**

---

## 🎨 Convenciones de Código

### **Nomenclatura:**
- **Clases:** PascalCase (`ProjectService`)
- **Métodos:** PascalCase (`GetProjectById()`)
- **Variables:** camelCase (`projectName`)
- **Constantes:** PascalCase (`MaxRetries`)
- **Comandos ZWCAD:** `TANDEM_<ACCION>_<OBJETO>`

### **Estructura:**
- Un tipo público por archivo
- Nombre archivo = Nombre tipo
- Namespace: `Tandem2026.<Proyecto>.<Categoria>`

**📖 [Ver convenciones completas](Docs/General/Convenciones.md)**

---

## 🧪 Testing

### **Compilación:**

```powershell
# Build completo
msbuild Design.sln /t:Rebuild /p:Configuration=Debug

# Solo un proyecto
msbuild DAL\DAL.csproj /t:Rebuild
```

---

### **Testing Manual ZWCAD:**

1. Compilar proyecto `ZwcadPlugin`
2. Abrir ZWCAD 2026
3. `NETLOAD` → Seleccionar DLL
4. Ejecutar comandos
5. Verificar funcionalidad

---

## 🚨 Solución de Problemas

### **Plugin no carga en ZWCAD**
```
Error: Could not load file or assembly...
```

**Solución:**
- Verificar referencias ZWCAD en `.csproj`
- Verificar ruta de `.dll` en carpeta `Support`
- Compilar en modo `Debug` con .NET 4.8

---

### **NuGet Packages faltantes**
```
Error: This project references NuGet package(s)...
```

**Solución:**
```powershell
# Restaurar paquetes
nuget restore Design.sln
```

---

### **Git: Commits no aparecen en Azure DevOps**

**Solución:**
- Verificar formato: `AB#<ID>`
- Verificar integración GitHub ↔ Azure Boards
- Ver [Guía de Azure DevOps](Docs/General/Azure-DevOps.md)

---

## 📞 Soporte

**Documentación:**
- [Docs/General/](Docs/General/) - Documentación común
- [Docs/Proyectos/](Docs/Proyectos/) - Documentación específica

**Azure DevOps:**
- Crear Issue: https://dev.azure.com/VSCAD/tandem2026/_workitems/create/Issue

**Repositorio:**
- Issues: https://github.com/JuanGodoyLopez/Tandem-2026/issues

---

## 👥 Equipo

**Proyecto:** Tandem 2026  
**Organización:** VSCAD  
**Año:** 2026

---

## 📄 Licencia

[Definir licencia del proyecto]

---

## 🔄 Actualizaciones

**Última actualización:** 24/04/2026

**Changelog:**
- Sistema de documentación completo
- Integración Azure DevOps + GitHub
- Plugin ZWCAD con MVVM
- Sistema de iconos Bootstrap
- Scripts PowerShell de gestión

---

**🎉 ¡Gracias por contribuir a Tandem 2026!**

Para cualquier duda, consulta la [documentación completa](Docs/) o crea un Issue en Azure DevOps.
