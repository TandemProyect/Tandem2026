# 📁 Estructura del Repositorio

Descripción de la organización de carpetas y archivos del proyecto Tandem 2026.

---

## 🌳 Estructura General

```
C:\00_Tandem2026\
├── Common/                 # Código compartido entre proyectos
├── DAL/                    # Data Access Layer
├── Desing/                 # Lógica de diseño
├── documentación/          # Docs de agentes, handovers y guías
├── Docs/                   # Documentación técnica del proyecto
│   ├── General/           # Documentación común
│   └── Proyectos/         # Documentación específica
├── IA/                     # Inteligencia Artificial
├── Language/               # Gestión de idiomas
├── Linq/                   # Extensiones LINQ
├── Scripts/                # Scripts PowerShell
├── TamdenZwcadPluging/     # Plugin de ZWCAD
│   └── ZwcadPlugin/
│       ├── Commands/      # Comandos ZWCAD
│       ├── MNU/           # Menús y ribbons
│       ├── UI/            # Interfaz WPF
│       └── Models/        # Modelos de datos
└── packages/               # NuGet packages (generado)
```

---

## 📦 Carpetas Principales

### **Common/** - Código Compartido
**Propósito:** Código reutilizable entre todos los proyectos

**Subcarpetas:**
- `Utils/` - Utilidades generales
- `Models/` - Modelos compartidos
- `Interfaces/` - Contratos comunes
- `Constants/` - Constantes de aplicación
- `Extensions/` - Métodos de extensión

**Documentación:** [`Common/README.md`](../../Common/README.md)

---

### **DAL/** - Data Access Layer
**Propósito:** Acceso a datos y repositorios

**Contenido:**
- Repositorios
- Contextos de base de datos
- Migrations
- Modelos de entidad

**Documentación:** [`Docs/Proyectos/DAL/`](../Proyectos/DAL/)

---

### **Desing/** - Lógica de Diseño
**Propósito:** Lógica de negocio y diseño

**Contenido:**
- Servicios de negocio
- Validadores
- Procesamiento de datos

**Documentación:** [`Docs/Proyectos/Desing/`](../Proyectos/Desing/)

---

### **TamdenZwcadPluging/ZwcadPlugin/** - Plugin ZWCAD
**Propósito:** Plugin para ZWCAD 2026

**Subcarpetas:**
- `Commands/` - Comandos registrados en ZWCAD
- `MNU/` - Menús (.cui) y ribbons
  - `Iconos/` - Iconos del ribbon
- `UI/` - Interfaz WPF
  - `Views/` - Ventanas XAML
  - `ViewModels/` - ViewModels MVVM
- `Models/` - Modelos de datos
- `Helpers/` - Utilidades del plugin

**Documentación:** 
- [`TamdenZwcadPluging/ZwcadPlugin/README.md`](../../TamdenZwcadPluging/ZwcadPlugin/README.md)
- [`TamdenZwcadPluging/ZwcadPlugin/TECHNICAL_GUIDE.md`](../../TamdenZwcadPluging/ZwcadPlugin/TECHNICAL_GUIDE.md)

---

### **Scripts/** - Automatización
**Propósito:** Scripts PowerShell para tareas comunes

**Contenido:**
- `US.ps1` - Crear User Stories
- `Edit-US.ps1` - Editar User Stories
- `Configurar-Board-*.ps1` - Configuración de Azure DevOps
- `Copiar-*.ps1` - Scripts de migración

**Documentación:** [`Docs/General/Azure-DevOps.md`](./Azure-DevOps.md)

---

### **Docs/** - Documentación
**Propósito:** Documentación del proyecto

**Estructura:**
```
Docs/
├── General/              # Documentación común
│   ├── README.md
│   ├── Azure-DevOps.md
│   ├── Estructura-Repositorio.md
│   ├── Common.md
│   ├── Convenciones.md
│   └── Git-Workflow.md
└── Proyectos/            # Documentación específica
	├── DAL/
	├── Desing/
	└── ZwcadPlugin/
```

---

## 🎯 Carpetas por Propósito

### **Desarrollo Activo:**
- `DAL/`
- `Desing/`
- `TamdenZwcadPluging/ZwcadPlugin/`
- `Common/`

### **Soporte:**
- `Scripts/` - Automatización
- `Docs/` - Documentación

### **Generadas (No tocar):**
- `.vs/` - Configuración Visual Studio
- `packages/` - NuGet packages
- `bin/`, `obj/` - Salidas de compilación

---

## 📝 Archivos Importantes

### **Raíz del Repositorio:**

| Archivo | Propósito |
|---------|-----------|
| `Design.sln` | Solución principal de Visual Studio |
| `.gitignore` | Archivos ignorados por Git |
| `README.md` | Documentación principal del repositorio |

### **Por Proyecto:**

| Archivo | Propósito |
|---------|-----------|
| `*.csproj` | Proyecto de Visual Studio |
| `packages.config` | Paquetes NuGet (.NET Framework) |
| `App.config` | Configuración de aplicación |

---

## 🚫 .gitignore

**Archivos/carpetas NO versionados:**
- `.vs/` - Configuración IDE
- `bin/`, `obj/` - Salidas compilación
- `packages/` - NuGet packages
- `*.user` - Configuración personal
- `*.suo` - Estado Visual Studio
- `MNU/Iconos/Bootstrap-Icons/icons/*` - Biblioteca completa de iconos (muy grande)

**Ver:** [`.gitignore`](../../.gitignore)

---

## 🔄 Flujo de Trabajo

### **Agregar Nuevo Proyecto:**

1. Crear carpeta en raíz
2. Agregar proyecto a `Design.sln`
3. Crear carpeta documentación: `Docs/Proyectos/<NombreProyecto>/`
4. Agregar README.md en el proyecto
5. Actualizar este documento

### **Agregar Código Compartido:**

1. Colocar en `Common/`
2. Seguir namespace `Tandem2026.Common.*`
3. Documentar en `Common/README.md`
4. Referenciar desde proyectos que lo necesiten

### **Agregar Script:**

1. Colocar en `Scripts/`
2. Documentar uso en `Docs/General/Azure-DevOps.md` o crear nueva guía
3. Incluir comentarios en el script

---

## 📊 Métricas

**Proyectos activos:** 3 (DAL, Desing, ZwcadPlugin)  
**Scripts PowerShell:** 7+  
**Documentos:** 10+  
**Target Framework:** .NET Framework 4.8  

---

## 🔗 Enlaces Útiles

- **GitHub:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Azure DevOps:** https://dev.azure.com/VSCAD/tandem2026
- **Panel de trabajo:** https://dev.azure.com/VSCAD/tandem2026/_boards

---

**Última actualización:** 24/04/2026
