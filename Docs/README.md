# 📚 Documentación Tandem 2026

> **Agentes:** empezad por [`AGENTS.md`](../AGENTS.md) y el `INDEX.md` de vuestra área (`Docs/areas/`). Este índice es la documentación anterior.

Sistema de documentación completo para el proyecto Tandem 2026.

---

## 🎯 Organización

La documentación está dividida en **dos niveles**:

1. **📖 Documentación General** - Procesos y prácticas comunes para todos los proyectos
2. **🔧 Documentación por Proyecto** - Implementación específica de cada proyecto

---

## 📖 Documentación General

**Ubicación:** [`Docs/areas/general/`](areas/general/)

Contiene guías y prácticas que aplican a **todos los proyectos**:

| Documento | Descripción | Cuándo consultar |
|-----------|-------------|------------------|
| [**🎯 GESTION-PANEL-AZURE-DEVOPS**](areas/azure-devops/GESTION-PANEL-AZURE-DEVOPS.md) ⭐ | **Guía completa de Azure DevOps** - Crear US/Tasks, scripts, troubleshooting | **LEER PRIMERO** antes de crear US o Tasks |
| [**Code review prep 2026-10-03**](areas/general/historial/CODE-REVIEW-PREP-2026-10-03.md) | Arquitectura, conexión frecuente, qué no revertir, deuda conocida | Antes de un review interno |
| [**Azure DevOps**](areas/azure-devops/Azure-DevOps.md) | Documentación anterior (redirige a la nueva) | Referencia histórica |
| [**Estructura del Repositorio**](areas/general/Estructura-Repositorio.md) | Organización de carpetas y archivos | Navegar el proyecto, ubicar archivos |
| [**Common - Código Compartido**](areas/general/Common.md) | Usar y mantener código compartido | Crear/usar utilidades compartidas |
| [**Convenciones de Código**](areas/general/Convenciones.md) | Estándares de nomenclatura y estilo | Escribir código, revisiones |
| [**Git Workflow**](areas/general/Git-Workflow.md) | Flujo de trabajo con Git y GitHub | Hacer commits, resolver conflictos |

---

## 🔧 Documentación por Proyecto

**Ubicación:** [`Docs/areas/`](areas/)

Contiene documentación **específica de implementación** para cada proyecto:

### **1. DAL - Data Access Layer**
📂 [`Docs/areas/base-de-datos/DAL.md`](areas/base-de-datos/DAL.md)

**Propósito:** Acceso a datos y repositorios

**Contenido:**
- Modelos de entidad
- Patrón Repository
- Contextos de base de datos
- Migraciones

**Consultar cuando:**
- Crear/modificar entidades
- Implementar repositorios
- Trabajar con base de datos

---

### **2. Desing - Business Logic Layer**
📂 [`Docs/areas/general/Desing-proyecto.md`](areas/general/Desing-proyecto.md)

**Propósito:** Lógica de negocio y servicios

**Contenido:**
- Servicios de negocio
- Validadores
- DTOs (Data Transfer Objects)
- Procesadores

**Consultar cuando:**
- Implementar reglas de negocio
- Crear servicios
- Validar datos

---

### **3. Plugins Tandem CAD (AutoCAD, BricsCAD, Revit)**
📂 [`Docs/areas/cad-comun/plugins-cad.md`](areas/cad-comun/plugins-cad.md)

**Propósito:** plugins Tandem 2026 alineados con Desing_2 (paletas MVC, muro 2D, generar 3D).

**Continuar aquí:** [Resumen agentes sep/oct 2026](areas/cad-comun/RESUMEN-2026-09-10-Trabajo-Agentes.md), y despues el handover tecnico actual [HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md](areas/cad-autocad/historial/HANDOVER-2026-10-07-Encofrar-Manual-Posicion-Visor.md).

También: [AutocadPlugin](areas/cad-autocad/autocad-plugin.md), [BricscadPlugin](areas/cad-bricscad-revit/bricscad-plugin.md), [RevitPlugin](areas/cad-bricscad-revit/revit-plugin.md).

---

### **4. ZwcadPlugin - Plugin ZWCAD 2026**
📂 [`Docs/areas/cad-zwcad/zwcad-plugin.md`](areas/cad-zwcad/zwcad-plugin.md)

**Propósito:** Plugin para ZWCAD con interfaz WPF

**Contenido:**
- Comandos ZWCAD
- Interfaz WPF (MVVM)
- Ribbon y menús CUI
- Integración con ZWCAD API

**Consultar cuando:**
- Crear comandos ZWCAD
- Implementar UI
- Configurar ribbon/menús

**Documentación adicional:**
- [Plugin README](../TamdenZwcadPluging/ZwcadPlugin/README.md)
- [Guía Técnica](areas/cad-zwcad/TECHNICAL_GUIDE.md)
- [Sistema de Iconos](areas/cad-zwcad/iconos/README_ICONOS.md)

---

## 🚀 Guías de Inicio Rápido

### **Para Nuevos Desarrolladores:**

1. **Leer primero:**
   - [README.md principal](../README.md)
   - [Estructura del Repositorio](areas/general/Estructura-Repositorio.md)
   - [Convenciones de Código](areas/general/Convenciones.md)

2. **Configurar entorno:**
   - Clonar repositorio
   - Abrir `Design.sln` en Visual Studio
   - Compilar solución

3. **Familiarizarse con:**
   - [Git Workflow](areas/general/Git-Workflow.md)
   - [Azure DevOps](areas/azure-devops/Azure-DevOps.md)
   - Documentación del proyecto en el que trabajarás

---

### **Para Tareas Específicas:**

#### **Crear User Story:**
📖 [Azure DevOps → Crear User Stories](areas/azure-devops/Azure-DevOps.md#-crear-user-stories--issues-rápidamente)

```powershell
.\Scripts\US.ps1 "Título de la historia"
```

---

#### **Vincular Commits a User Stories:**
📖 [Azure DevOps → Vincular Commits](areas/azure-devops/Azure-DevOps.md#-vincular-commits-a-user-stories)

```bash
git commit -m "feat: Descripción del cambio AB#612"
```

---

#### **Agregar Código Compartido:**
📖 [Common → Flujo de Trabajo](areas/general/Common.md#-flujo-de-trabajo)

1. Crear archivo en `Common/<Categoria>/`
2. Definir namespace `Tandem2026.Common.<Categoria>`
3. Documentar con XML comments
4. Referenciar desde proyectos

---

#### **Crear Comando ZWCAD:**
📖 [ZwcadPlugin → Comandos](areas/cad-zwcad/zwcad-plugin.md#-comandos-zwcad)

```csharp
[CommandMethod("TANDEM_MI_COMANDO")]
public void MiComando()
{
	// Implementación
}
```

---

## 📋 Índice Completo

### **Documentación General:**
```
Docs/areas/general/
├── README.md                         # Índice de docs generales
├── Azure-DevOps.md                   # Gestión de trabajo
├── Estructura-Repositorio.md         # Organización del repo
├── Common.md                         # Código compartido
├── Convenciones.md                   # Estándares de código
└── Git-Workflow.md                   # Flujo Git/GitHub
```

---

### **Documentación por Proyecto:**
```
Docs/areas/
├── DAL/
│   └── README.md                     # Data Access Layer
├── Desing/
│   └── README.md                     # Business Logic Layer
└── ZwcadPlugin/
	└── README.md                     # Plugin ZWCAD
```

---

## 🔄 Mantenimiento de Documentación

### **Cuándo Actualizar:**

**Documentación General:**
- Cambios en procesos del equipo
- Nuevas herramientas o scripts
- Actualizaciones de convenciones
- Cambios en flujo de trabajo

**Documentación por Proyecto:**
- Nueva funcionalidad implementada
- Cambios en arquitectura
- Nuevas dependencias
- Actualizaciones de API

---

### **Cómo Actualizar:**

1. **Editar** el documento correspondiente
2. **Actualizar** fecha al final del documento
3. **Commit** con mensaje descriptivo:
   ```bash
   git commit -m "docs: Actualizar guía de Azure DevOps AB#<ID>"
   ```
4. **Notificar** al equipo si es cambio importante

---

## 📊 Métricas de Documentación

**Documentos Generales:** 6  
**Documentos por Proyecto:** 3  
**Scripts Documentados:** 7+  
**Cobertura:** ~90% del código tiene documentación

---

## 🔗 Enlaces Útiles

### **Repositorio:**
- GitHub: https://github.com/JuanGodoyLopez/Tandem-2026
- README Principal: [README.md](../README.md)

### **Azure DevOps:**
- Proyecto: https://dev.azure.com/VSCAD/tandem2026
- Panel: https://dev.azure.com/VSCAD/tandem2026/_boards

### **Código:**
- Common: [Common/README.md](../Common/README.md)
- ZwcadPlugin: [TamdenZwcadPluging/ZwcadPlugin/README.md](../TamdenZwcadPluging/ZwcadPlugin/README.md)

---

## ❓ Preguntas Frecuentes

### **¿Qué documentación leer primero?**
1. README principal
2. Estructura del Repositorio
3. Convenciones de Código
4. Documentación del proyecto en el que trabajarás

---

### **¿Cómo buscar algo específico?**
1. **Ctrl+F** en los documentos
2. Revisar índices de cada sección
3. Buscar en GitHub: https://github.com/JuanGodoyLopez/Tandem-2026

---

### **¿La documentación está desactualizada?**
1. Verifica la fecha al final del documento
2. Actualiza el contenido si es necesario
3. Haz commit con `docs:` prefix

---

### **¿Falta documentación?**
1. Identifica el tema/proyecto
2. Crea el documento correspondiente
3. Actualiza este índice
4. Haz commit y notifica al equipo

---

## 📞 Soporte

**Problemas con documentación:**
- Crear Issue: https://dev.azure.com/VSCAD/tandem2026/_workitems/create/Issue
- Tag: `documentation`

**Sugerencias de mejora:**
- Discutir con el equipo
- Crear User Story con mejoras propuestas

---

## 🎯 Objetivos de la Documentación

✅ **Claridad:** Explicaciones simples y directas  
✅ **Ejemplos:** Código práctico y casos de uso  
✅ **Actualización:** Mantener sincronizada con el código  
✅ **Accesibilidad:** Fácil de encontrar y navegar  
✅ **Completitud:** Cubrir todos los aspectos importantes  

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026

---

**💡 Recuerda:** La documentación es tan importante como el código. Mantenerla actualizada facilita el trabajo de todo el equipo.
