# 📚 Desing - Business Logic Layer

Documentación del proyecto de Lógica de Negocio para Tandem 2026.

---

## 📋 Información General

**Proyecto:** Desing (Business Logic Layer)  
**Ubicación:** `C:\00_Tandem2026\Desing\`  
**Tipo:** Class Library (.NET Framework 4.8)  
**Namespace:** `Tandem2026.Desing`

---

## 🎯 Propósito

Desing es la **capa de lógica de negocio** que:
- Implementa reglas de negocio
- Orquesta operaciones entre capas
- Valida datos antes de persistir
- Proporciona servicios a la capa de presentación (ZwcadPlugin)

---

## 🏗️ Arquitectura

```
Desing/
├── Services/           # Servicios de negocio
├── Validators/         # Validadores de datos
├── DTOs/              # Data Transfer Objects
├── Processors/        # Procesadores de datos
├── Helpers/           # Utilidades específicas de negocio
└── Interfaces/        # Contratos de servicios
```

---

## 📦 Dependencias

**Referencias Internas:**
- `DAL/` - Acceso a datos
- `Common/` - Utilidades compartidas

**NuGet Packages:**
- FluentValidation (opcional)
- AutoMapper (opcional)

---

## 🎨 Patrón Service

### **Interface:**

```csharp
namespace Tandem2026.Desing.Interfaces
{
	public interface IProjectService
	{
		ProjectDTO GetProjectById(int id);
		IEnumerable<ProjectDTO> GetAllProjects();
		void CreateProject(ProjectDTO project);
		void UpdateProject(ProjectDTO project);
		void DeleteProject(int id);
	}
}
```

---

### **Implementación:**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Tandem2026.DAL.Context;
using Tandem2026.DAL.Repositories;
using Tandem2026.DAL.Models;
using Tandem2026.Desing.DTOs;
using Tandem2026.Desing.Interfaces;
using Tandem2026.Desing.Validators;

namespace Tandem2026.Desing.Services
{
	public class ProjectService : IProjectService
	{
		private readonly Repository<Project> _projectRepository;
		private readonly IProjectValidator _validator;

		public ProjectService()
		{
			var context = new Tandem2026Context();
			_projectRepository = new Repository<Project>(context);
			_validator = new ProjectValidator();
		}

		public ProjectDTO GetProjectById(int id)
		{
			var project = _projectRepository.GetById(id);
			return MapToDTO(project);
		}

		public IEnumerable<ProjectDTO> GetAllProjects()
		{
			var projects = _projectRepository.GetAll();
			return projects.Select(MapToDTO).ToList();
		}

		public void CreateProject(ProjectDTO projectDto)
		{
			// Validar
			var validationResult = _validator.Validate(projectDto);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}

			// Mapear y guardar
			var project = MapToEntity(projectDto);
			project.CreatedDate = DateTime.Now;
			project.IsActive = true;

			_projectRepository.Add(project);
			_projectRepository.Save();
		}

		public void UpdateProject(ProjectDTO projectDto)
		{
			var validationResult = _validator.Validate(projectDto);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}

			var project = MapToEntity(projectDto);
			_projectRepository.Update(project);
			_projectRepository.Save();
		}

		public void DeleteProject(int id)
		{
			_projectRepository.Delete(id);
			_projectRepository.Save();
		}

		private ProjectDTO MapToDTO(Project entity)
		{
			return new ProjectDTO
			{
				Id = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				CreatedDate = entity.CreatedDate,
				IsActive = entity.IsActive
			};
		}

		private Project MapToEntity(ProjectDTO dto)
		{
			return new Project
			{
				Id = dto.Id,
				Name = dto.Name,
				Description = dto.Description,
				CreatedDate = dto.CreatedDate,
				IsActive = dto.IsActive
			};
		}
	}
}
```

---

## 📋 Data Transfer Objects (DTOs)

### **Propósito:**
- Transferir datos entre capas
- Exponer solo campos necesarios
- Independencia de modelos de base de datos

### **Ejemplo:**

```csharp
namespace Tandem2026.Desing.DTOs
{
	/// <summary>
	/// DTO para transferir información de proyecto
	/// </summary>
	public class ProjectDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool IsActive { get; set; }
	}
}
```

---

## ✅ Validadores

### **Ejemplo con Validación Manual:**

```csharp
using System.Collections.Generic;
using Tandem2026.Desing.DTOs;
using Tandem2026.Desing.Interfaces;

namespace Tandem2026.Desing.Validators
{
	public class ProjectValidator : IProjectValidator
	{
		public ValidationResult Validate(ProjectDTO project)
		{
			var errors = new List<string>();

			if (string.IsNullOrWhiteSpace(project.Name))
			{
				errors.Add("El nombre del proyecto es requerido");
			}

			if (project.Name?.Length > 100)
			{
				errors.Add("El nombre no puede exceder 100 caracteres");
			}

			if (project.Description?.Length > 500)
			{
				errors.Add("La descripción no puede exceder 500 caracteres");
			}

			return new ValidationResult
			{
				IsValid = errors.Count == 0,
				Errors = errors
			};
		}
	}

	public class ValidationResult
	{
		public bool IsValid { get; set; }
		public List<string> Errors { get; set; }
	}
}
```

---

### **Ejemplo con FluentValidation (Opcional):**

```csharp
using FluentValidation;
using Tandem2026.Desing.DTOs;

namespace Tandem2026.Desing.Validators
{
	public class ProjectValidator : AbstractValidator<ProjectDTO>
	{
		public ProjectValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("El nombre es requerido")
				.MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");
		}
	}
}
```

---

## 🔄 Procesadores

### **Propósito:**
- Lógica de procesamiento compleja
- Transformaciones de datos
- Cálculos de negocio

### **Ejemplo:**

```csharp
namespace Tandem2026.Desing.Processors
{
	public class ProjectProcessor
	{
		/// <summary>
		/// Calcula el estado del proyecto basado en reglas de negocio
		/// </summary>
		public string CalculateProjectStatus(ProjectDTO project)
		{
			if (!project.IsActive)
				return "Inactivo";

			var daysSinceCreation = (DateTime.Now - project.CreatedDate).TotalDays;

			if (daysSinceCreation < 7)
				return "Nuevo";
			else if (daysSinceCreation < 30)
				return "Activo";
			else
				return "En revisión";
		}

		/// <summary>
		/// Aplica transformaciones de negocio al proyecto
		/// </summary>
		public ProjectDTO ProcessProject(ProjectDTO project)
		{
			// Normalizar nombre
			project.Name = project.Name?.Trim();

			// Generar descripción si está vacía
			if (string.IsNullOrEmpty(project.Description))
			{
				project.Description = $"Proyecto creado el {project.CreatedDate:dd/MM/yyyy}";
			}

			return project;
		}
	}
}
```

---

## 🔧 Uso desde ZwcadPlugin

### **Referencia:**

1. **Click derecho** en ZwcadPlugin → **Add Reference**
2. **Projects** → Seleccionar **Desing**
3. Click **OK**

---

### **Ejemplo de Uso:**

```csharp
using Tandem2026.Desing.Services;
using Tandem2026.Desing.DTOs;

namespace Tandem2026.ZwcadPlugin.Commands
{
	public class ProjectCommands
	{
		private readonly ProjectService _projectService;

		public ProjectCommands()
		{
			_projectService = new ProjectService();
		}

		[CommandMethod("TANDEM_CREAR_PROYECTO")]
		public void CrearProyecto()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;

			// Pedir datos al usuario
			var projectName = doc.Editor.GetString("\nNombre del proyecto: ");

			if (projectName.Status != PromptStatus.OK)
				return;

			// Crear proyecto usando servicio
			var projectDto = new ProjectDTO
			{
				Name = projectName.StringResult,
				Description = "Proyecto creado desde ZWCAD"
			};

			try
			{
				_projectService.CreateProject(projectDto);
				doc.Editor.WriteMessage("\nProyecto creado exitosamente");
			}
			catch (ValidationException ex)
			{
				doc.Editor.WriteMessage($"\nError: {ex.Message}");
			}
		}

		[CommandMethod("TANDEM_LISTAR_PROYECTOS")]
		public void ListarProyectos()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			var projects = _projectService.GetAllProjects();

			doc.Editor.WriteMessage("\n=== PROYECTOS ===");
			foreach (var project in projects)
			{
				doc.Editor.WriteMessage($"\n{project.Id}: {project.Name}");
			}
		}
	}
}
```

---

## 📝 Convenciones

### **Servicios:**
- Interfaz + Implementación
- Ubicación: `Desing/Services/`
- Namespace: `Tandem2026.Desing.Services`
- Nombrar: `<Entidad>Service`

### **DTOs:**
- Ubicación: `Desing/DTOs/`
- Namespace: `Tandem2026.Desing.DTOs`
- Nombrar: `<Entidad>DTO`

### **Validadores:**
- Ubicación: `Desing/Validators/`
- Namespace: `Tandem2026.Desing.Validators`
- Nombrar: `<Entidad>Validator`

### **Procesadores:**
- Ubicación: `Desing/Processors/`
- Namespace: `Tandem2026.Desing.Processors`
- Nombrar: `<Entidad>Processor`

---

## 🧪 Testing

### **Unit Tests:**

```csharp
[TestClass]
public class ProjectServiceTests
{
	private IProjectService _projectService;

	[TestInitialize]
	public void Setup()
	{
		// Usar mocks para repositorios
		_projectService = new ProjectService();
	}

	[TestMethod]
	public void CreateProject_ValidData_ShouldSucceed()
	{
		// Arrange
		var projectDto = new ProjectDTO
		{
			Name = "Test Project",
			Description = "Test Description"
		};

		// Act
		_projectService.CreateProject(projectDto);

		// Assert
		var projects = _projectService.GetAllProjects();
		Assert.IsTrue(projects.Any(p => p.Name == "Test Project"));
	}

	[TestMethod]
	[ExpectedException(typeof(ValidationException))]
	public void CreateProject_InvalidData_ShouldThrowException()
	{
		// Arrange
		var projectDto = new ProjectDTO
		{
			Name = "", // Inválido
			Description = "Test"
		};

		// Act
		_projectService.CreateProject(projectDto);

		// Assert - Espera excepción
	}
}
```

---

## 🎯 Reglas de Negocio

### **Ejemplos de Reglas:**

1. **Proyecto:**
   - Nombre único
   - Nombre entre 3-100 caracteres
   - Descripción máximo 500 caracteres
   - Solo usuarios autenticados pueden crear

2. **Validaciones:**
   - Validar antes de guardar
   - Mensajes de error claros
   - Logging de errores

3. **Procesamiento:**
   - Normalizar datos (trim, lowercase)
   - Generar valores por defecto
   - Aplicar transformaciones

---

## 🚀 Performance

### **Optimizaciones:**
- Cachear servicios costosos
- Lazy loading de datos relacionados
- Batch processing para operaciones múltiples
- Async/await para operaciones largas (futuro)

---

## 📊 Estado Actual

**Completado:**
- ✅ Estructura básica de proyecto
- ✅ Referencias a DAL configuradas

**Pendiente:**
- ⏳ Implementar servicios de negocio
- ⏳ Crear DTOs
- ⏳ Agregar validadores
- ⏳ Implementar procesadores
- ⏳ Unit tests

---

## 🔗 Referencias

- **DAL:** [`Docs/Proyectos/DAL/`](../DAL/)
- **ZwcadPlugin:** [`Docs/Proyectos/ZwcadPlugin/`](../ZwcadPlugin/)
- **Código Compartido:** [`Docs/General/Common.md`](../../General/Common.md)
- **Convenciones:** [`Docs/General/Convenciones.md`](../../General/Convenciones.md)

---

## 📞 Contacto

**Problemas o preguntas:**
- Revisar documentación general
- Consultar con el equipo
- Crear Issue en Azure DevOps

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
