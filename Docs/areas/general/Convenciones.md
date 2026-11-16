# 📏 Convenciones de Código

Estándares de codificación y nomenclatura para el proyecto Tandem 2026.

---

## 🎯 Propósito

Mantener **consistencia** y **legibilidad** en todo el código base:
- Facilita colaboración
- Reduce errores
- Acelera revisiones de código
- Mejora mantenibilidad

---

## 📝 Nomenclatura

### **C# - Clases y Tipos**

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Clase | PascalCase | `ProjectRepository`, `MainWindow` |
| Interfaz | PascalCase con `I` | `IRepository`, `ILogger` |
| Struct | PascalCase | `Point3D`, `ColorInfo` |
| Enum | PascalCase (singular) | `ProjectState`, `UserRole` |
| Enum Valor | PascalCase | `ProjectState.Active` |

**Ejemplos:**
```csharp
// ✅ Correcto
public class ProjectManager { }
public interface IDataService { }
public enum FileType { Dwg, Dxf, Pdf }

// ❌ Incorrecto
public class projectmanager { }
public interface DataService { }
public enum fileTypes { DWG, DXF, PDF }
```

---

### **C# - Miembros de Clase**

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Método público | PascalCase | `GetProjectById()`, `SaveData()` |
| Propiedad | PascalCase | `ProjectName`, `IsActive` |
| Campo privado | camelCase con `_` | `_projectName`, `_isActive` |
| Constante | PascalCase | `MaxRetries`, `DefaultPath` |
| Parámetro | camelCase | `projectId`, `userName` |
| Variable local | camelCase | `currentProject`, `tempFile` |

**Ejemplos:**
```csharp
public class ProjectService
{
	// ✅ Correcto
	private readonly string _connectionString;
	private const int MaxRetries = 3;

	public string ProjectName { get; set; }

	public void SaveProject(int projectId, string projectName)
	{
		var currentPath = GetPath();
		// ...
	}

	// ❌ Incorrecto
	private readonly string ConnectionString;  // Sin _
	private const int max_retries = 3;         // No snake_case

	public string projectName { get; set; }    // No camelCase
}
```

---

### **Archivos y Carpetas**

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Archivo C# | PascalCase + extensión | `ProjectRepository.cs` |
| Archivo XAML | PascalCase + extensión | `MainWindow.xaml` |
| Carpeta | PascalCase | `Commands/`, `Models/` |
| Script PS1 | PascalCase con `-` | `US.ps1`, `Edit-US.ps1` |

**Estructura:**
```
ZwcadPlugin/
├── Commands/
│   └── AnnotationCommands.cs
├── UI/
│   ├── Views/
│   │   └── MainWindow.xaml
│   └── ViewModels/
│       └── MainWindowViewModel.cs
└── Models/
	└── ProjectModel.cs
```

---

## 🔤 Namespaces

### **Estructura:**
```
Tandem2026.<Proyecto>.<Categoria>
```

**Ejemplos:**
```csharp
// DAL
namespace Tandem2026.DAL.Repositories { }
namespace Tandem2026.DAL.Models { }

// Desing
namespace Tandem2026.Desing.Services { }
namespace Tandem2026.Desing.Validators { }

// ZwcadPlugin
namespace Tandem2026.ZwcadPlugin.Commands { }
namespace Tandem2026.ZwcadPlugin.UI.Views { }
namespace Tandem2026.ZwcadPlugin.UI.ViewModels { }

// Common
namespace Tandem2026.Common.Utils { }
namespace Tandem2026.Common.Models { }
```

---

## 📦 Organización de Archivos

### **Un Tipo Público por Archivo**

**✅ Correcto:**
```
ProjectRepository.cs    → class ProjectRepository
UserRepository.cs       → class UserRepository
```

**❌ Incorrecto:**
```
Repositories.cs         → class ProjectRepository + class UserRepository
```

**Excepción:** Clases helper privadas pequeñas

---

### **Nombre de Archivo = Nombre de Tipo**

**✅ Correcto:**
```csharp
// Archivo: ProjectService.cs
public class ProjectService { }

// Archivo: IDataService.cs
public interface IDataService { }
```

**❌ Incorrecto:**
```csharp
// Archivo: Service.cs
public class ProjectService { }  // Nombre no coincide
```

---

## 💬 Comentarios y Documentación

### **XML Documentation (Obligatorio para APIs públicas)**

```csharp
/// <summary>
/// Obtiene un proyecto por su identificador único
/// </summary>
/// <param name="projectId">ID del proyecto a buscar</param>
/// <returns>Proyecto encontrado o null si no existe</returns>
/// <exception cref="ArgumentException">Si projectId es menor a 1</exception>
public Project GetProjectById(int projectId)
{
	if (projectId < 1)
		throw new ArgumentException("ID debe ser positivo", nameof(projectId));

	// ...
}
```

---

### **Comentarios de Línea (Solo cuando sea necesario)**

**✅ Usar comentarios para:**
- Explicar **por qué**, no **qué**
- Algoritmos complejos
- Workarounds temporales
- Decisiones no obvias

```csharp
// ✅ Correcto - Explica el "por qué"
// ZWCAD requiere transacciones incluso para operaciones de solo lectura
using (var tr = db.TransactionManager.StartTransaction())
{
	// ...
}

// ❌ Incorrecto - Repite lo obvio
// Incrementar contador
counter++;
```

---

### **TODO y HACK**

```csharp
// TODO: Implementar caché para mejorar performance
// HACK: Workaround temporal para bug en ZWCAD 2026 - revisar en v2027
// FIXME: Este método falla con archivos grandes (>100MB)
```

---

## 🎨 Formato y Estilo

### **Llaves (Braces)**

**Estilo Allman (línea nueva):**

```csharp
// ✅ Correcto
public void SaveProject()
{
	if (IsValid())
	{
		// ...
	}
}

// ❌ Incorrecto
public void SaveProject() {
	if (IsValid()) {
		// ...
	}
}
```

---

### **Indentación**

- **Espacios:** 4 espacios (no tabs)
- **Anidación:** Máximo 4 niveles (considera extraer métodos)

```csharp
// ✅ Correcto
public void Process()
{
	if (condition1)
	{
		foreach (var item in items)
		{
			if (item.IsValid)
			{
				Process(item);
			}
		}
	}
}
```

---

### **Longitud de Línea**

- **Máximo:** 120 caracteres
- **Ideal:** 80-100 caracteres

```csharp
// ✅ Correcto
var result = repository.GetProjectsByUserIdAndStatus(
	userId, 
	ProjectStatus.Active, 
	includeArchived: false
);

// ❌ Incorrecto (línea muy larga)
var result = repository.GetProjectsByUserIdAndStatus(userId, ProjectStatus.Active, includeArchived: false, includeDeleted: false, orderBy: "Name");
```

---

### **Espacios en Blanco**

```csharp
// ✅ Correcto
public class ProjectService
{
	private readonly IRepository _repository;

	public ProjectService(IRepository repository)
	{
		_repository = repository;
	}

	public void SaveProject(Project project)
	{
		Validate(project);
		_repository.Save(project);
	}

	private void Validate(Project project)
	{
		// ...
	}
}

// ❌ Incorrecto (sin separación)
public class ProjectService
{
	private readonly IRepository _repository;
	public ProjectService(IRepository repository)
	{
		_repository = repository;
	}
	public void SaveProject(Project project)
	{
		Validate(project);
		_repository.Save(project);
	}
	private void Validate(Project project)
	{
		// ...
	}
}
```

**Reglas:**
- Línea en blanco entre métodos
- Línea en blanco entre propiedades y métodos
- Sin líneas en blanco múltiples (máximo 1)

---

## 🔧 Buenas Prácticas

### **Principios SOLID**

```csharp
// ✅ Single Responsibility
public class ProjectRepository
{
	public Project GetById(int id) { }
	public void Save(Project project) { }
}

public class ProjectValidator
{
	public bool IsValid(Project project) { }
}

// ❌ Múltiples responsabilidades
public class ProjectManager
{
	public Project GetById(int id) { }
	public void Save(Project project) { }
	public bool IsValid(Project project) { }
	public void SendEmail(Project project) { }
}
```

---

### **Nombres Descriptivos**

```csharp
// ✅ Correcto - Nombres claros
public void SaveProjectToDatabase(Project project)
{
	var activeProjects = GetActiveProjectsByUserId(currentUserId);
	// ...
}

// ❌ Incorrecto - Nombres ambiguos
public void Save(Project p)
{
	var list = Get(id);
	// ...
}
```

---

### **Evitar Números Mágicos**

```csharp
// ✅ Correcto - Constantes con nombre
private const int MaxRetryAttempts = 3;
private const int TimeoutSeconds = 30;

if (retryCount > MaxRetryAttempts)
{
	throw new TimeoutException();
}

// ❌ Incorrecto - Números mágicos
if (retryCount > 3)
{
	throw new TimeoutException();
}
```

---

### **Guard Clauses**

```csharp
// ✅ Correcto - Salida temprana
public void ProcessProject(Project project)
{
	if (project == null)
		throw new ArgumentNullException(nameof(project));

	if (string.IsNullOrEmpty(project.Name))
		throw new ArgumentException("Nombre requerido");

	// Lógica principal
	SaveToDatabase(project);
}

// ❌ Incorrecto - Anidación profunda
public void ProcessProject(Project project)
{
	if (project != null)
	{
		if (!string.IsNullOrEmpty(project.Name))
		{
			// Lógica principal
			SaveToDatabase(project);
		}
		else
		{
			throw new ArgumentException("Nombre requerido");
		}
	}
	else
	{
		throw new ArgumentNullException(nameof(project));
	}
}
```

---

## 🎯 ZWCAD Plugin - Convenciones Específicas

### **Comandos**

```csharp
[CommandMethod("TANDEM_CREAR_PANEL")]
public void CrearPanel()
{
	// Patrón: TANDEM_<ACCION>_<OBJETO>
}
```

**Formato:**
- Prefijo: `TANDEM_`
- Todo en mayúsculas
- Separado por `_`
- Verbos en infinitivo: CREAR, EDITAR, ELIMINAR

---

### **Transacciones ZWCAD**

```csharp
// ✅ Correcto - Always use using
public void AddLine()
{
	var doc = Application.DocumentManager.MdiActiveDocument;
	var db = doc.Database;

	using (var tr = db.TransactionManager.StartTransaction())
	{
		try
		{
			// Operaciones
			tr.Commit();
		}
		catch
		{
			tr.Abort();
			throw;
		}
	}
}
```

---

### **UI - MVVM**

```
UI/
├── Views/
│   └── MainWindow.xaml           # Solo XAML
│       └── MainWindow.xaml.cs    # Solo code-behind mínimo
└── ViewModels/
	└── MainWindowViewModel.cs    # Toda la lógica
```

**View (mínimo):**
```csharp
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		DataContext = new MainWindowViewModel();
	}
}
```

**ViewModel (lógica):**
```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
	private string _projectName;

	public string ProjectName
	{
		get => _projectName;
		set
		{
			_projectName = value;
			OnPropertyChanged();
		}
	}

	// Lógica de negocio aquí
}
```

---

## 📊 Métricas de Calidad

**Objetivos:**
- Complejidad ciclomática: < 10 por método
- Longitud de método: < 50 líneas
- Parámetros por método: < 5
- Cobertura de pruebas: > 70%

---

## 🔗 Herramientas

**Visual Studio:**
- **Format Document:** `Ctrl+K, Ctrl+D`
- **Rename:** `Ctrl+R, Ctrl+R`
- **Extract Method:** `Ctrl+R, Ctrl+M`

**Configuración:**
- Tools → Options → Text Editor → C# → Code Style
- Importar configuración del equipo (si existe)

---

## ❓ Preguntas Frecuentes

### **¿Usar `var` o tipo explícito?**
**Regla:** Usa `var` cuando el tipo es obvio del lado derecho

```csharp
// ✅ Tipo obvio - usar var
var projects = new List<Project>();
var name = GetProjectName();

// ✅ Tipo no obvio - explícito
Project project = repository.Get(id);
IEnumerable<int> ids = GetIds();
```

### **¿Cuándo usar propiedades vs métodos?**
- **Propiedad:** Acceso rápido, sin efectos secundarios, puede ser getter/setter
- **Método:** Operación costosa, tiene efectos secundarios, realiza lógica compleja

```csharp
// ✅ Propiedad
public string ProjectName { get; set; }
public bool IsActive { get; private set; }

// ✅ Método
public void SaveToDatabase() { }
public List<Project> GetActiveProjects() { }
```

### **¿Cómo nombrar métodos booleanos?**
Usa prefijos: `Is`, `Has`, `Can`, `Should`

```csharp
public bool IsValid() { }
public bool HasPermission() { }
public bool CanEdit() { }
public bool ShouldRetry() { }
```

---

## 🔄 Aplicar Convenciones

**En código nuevo:**
- Aplicar desde el inicio

**En código existente:**
- Aplicar al modificar (Boy Scout Rule)
- No refactorizar todo de golpe
- Priorizar código crítico o frecuentemente modificado

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
