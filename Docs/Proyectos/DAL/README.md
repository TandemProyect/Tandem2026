# 📚 DAL - Data Access Layer

Documentación del proyecto de Acceso a Datos para Tandem 2026.

---

## 📋 Información General

**Proyecto:** DAL (Data Access Layer)  
**Ubicación:** `C:\00_Tandem2026\DAL\`  
**Tipo:** Class Library (.NET Framework 4.8)  
**Namespace:** `Tandem2026.DAL`

---

## 🎯 Propósito

DAL (Data Access Layer) es la **capa de acceso a datos** que:
- Abstrae el acceso a base de datos
- Implementa el patrón Repository
- Gestiona contextos de Entity Framework (si aplica)
- Proporciona modelos de entidad

---

## 🏗️ Arquitectura

```
DAL/
├── Repositories/        # Implementaciones de repositorios
├── Models/             # Modelos de entidad/base de datos
├── Context/            # Contextos de base de datos
├── Migrations/         # Migraciones (si aplica)
└── Interfaces/         # Contratos de repositorios
```

---

## 📦 Dependencias

**NuGet Packages:**
- Entity Framework (si aplica)
- Dapper (si aplica)
- System.Data.SqlClient

**Referencias Internas:**
- `Common/` - Utilidades compartidas

---

## 💾 Conexión a Base de Datos

### **Connection String**

**Ubicación:** `App.config` (proyecto consumidor) o `DAL.config`

```xml
<connectionStrings>
  <add name="Tandem2026Context" 
	   connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=Tandem2026;Integrated Security=True" 
	   providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

### **Contexto de Base de Datos**

```csharp
using System.Data.Entity;

namespace Tandem2026.DAL.Context
{
	public class Tandem2026Context : DbContext
	{
		public Tandem2026Context() 
			: base("name=Tandem2026Context")
		{
		}

		public DbSet<Project> Projects { get; set; }
		public DbSet<User> Users { get; set; }
	}
}
```

---

## 📊 Modelos de Entidad

### **Ejemplo: Project**

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tandem2026.DAL.Models
{
	[Table("Projects")]
	public class Project
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; }

		[MaxLength(500)]
		public string Description { get; set; }

		public DateTime CreatedDate { get; set; }

		public bool IsActive { get; set; }
	}
}
```

---

## 🗃️ Patrón Repository

### **Interface:**

```csharp
namespace Tandem2026.DAL.Interfaces
{
	public interface IRepository<T> where T : class
	{
		T GetById(int id);
		IEnumerable<T> GetAll();
		void Add(T entity);
		void Update(T entity);
		void Delete(int id);
		void Save();
	}
}
```

---

### **Implementación:**

```csharp
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tandem2026.DAL.Context;
using Tandem2026.DAL.Interfaces;

namespace Tandem2026.DAL.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly Tandem2026Context _context;
		private readonly DbSet<T> _dbSet;

		public Repository(Tandem2026Context context)
		{
			_context = context;
			_dbSet = context.Set<T>();
		}

		public T GetById(int id)
		{
			return _dbSet.Find(id);
		}

		public IEnumerable<T> GetAll()
		{
			return _dbSet.ToList();
		}

		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}

		public void Update(T entity)
		{
			_context.Entry(entity).State = EntityState.Modified;
		}

		public void Delete(int id)
		{
			var entity = GetById(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
			}
		}

		public void Save()
		{
			_context.SaveChanges();
		}
	}
}
```

---

## 🔧 Uso desde Otros Proyectos

### **Referencia:**

1. **Click derecho** en proyecto consumidor → **Add Reference**
2. **Projects** → Seleccionar **DAL**
3. Click **OK**

---

### **Ejemplo de Uso:**

```csharp
using Tandem2026.DAL.Context;
using Tandem2026.DAL.Repositories;
using Tandem2026.DAL.Models;

namespace MiProyecto
{
	public class ProjectService
	{
		private readonly Repository<Project> _projectRepository;

		public ProjectService()
		{
			var context = new Tandem2026Context();
			_projectRepository = new Repository<Project>(context);
		}

		public void CreateProject(string name)
		{
			var project = new Project
			{
				Name = name,
				CreatedDate = DateTime.Now,
				IsActive = true
			};

			_projectRepository.Add(project);
			_projectRepository.Save();
		}

		public List<Project> GetAllProjects()
		{
			return _projectRepository.GetAll().ToList();
		}
	}
}
```

---

## 🔄 Migraciones (Entity Framework)

### **Habilitar Migraciones:**

```powershell
# Package Manager Console
Enable-Migrations -ProjectName DAL
```

---

### **Crear Migración:**

```powershell
Add-Migration InitialCreate -ProjectName DAL
```

---

### **Aplicar Migraciones:**

```powershell
Update-Database -ProjectName DAL
```

---

## 📝 Convenciones

### **Modelos:**
- Un modelo por archivo
- Ubicación: `DAL/Models/`
- Namespace: `Tandem2026.DAL.Models`
- Anotaciones de validación siempre

### **Repositorios:**
- Interfaz + Implementación
- Ubicación: `DAL/Repositories/`
- Namespace: `Tandem2026.DAL.Repositories`
- Usar patrón Repository/Unit of Work

### **Contextos:**
- Ubicación: `DAL/Context/`
- Namespace: `Tandem2026.DAL.Context`
- Nombre: `<Proyecto>Context`

---

## 🧪 Testing

### **Unit Tests:**

```csharp
[TestClass]
public class ProjectRepositoryTests
{
	private Tandem2026Context _context;
	private Repository<Project> _repository;

	[TestInitialize]
	public void Setup()
	{
		// Usar base de datos en memoria o mock
		_context = new Tandem2026Context();
		_repository = new Repository<Project>(_context);
	}

	[TestMethod]
	public void Add_Project_ShouldIncreaseCount()
	{
		// Arrange
		var initialCount = _repository.GetAll().Count();
		var project = new Project { Name = "Test Project" };

		// Act
		_repository.Add(project);
		_repository.Save();

		// Assert
		var finalCount = _repository.GetAll().Count();
		Assert.AreEqual(initialCount + 1, finalCount);
	}
}
```

---

## 🔒 Seguridad

### **Prevención de SQL Injection:**
- ✅ Usar Entity Framework con queries parametrizadas
- ✅ Validar inputs en capa de negocio
- ❌ No concatenar strings en queries

### **Connection Strings:**
- ✅ Usar Integrated Security cuando sea posible
- ✅ Encriptar connection strings en producción
- ❌ No hardcodear credenciales

---

## 🚀 Performance

### **Optimizaciones:**
- Usar `.AsNoTracking()` para queries de solo lectura
- Eager loading con `.Include()` para relaciones
- Pagination para listas grandes
- Cachear queries frecuentes

**Ejemplo:**
```csharp
public IEnumerable<Project> GetActiveProjects()
{
	return _context.Projects
		.AsNoTracking()
		.Where(p => p.IsActive)
		.OrderBy(p => p.Name)
		.ToList();
}
```

---

## 📊 Estado Actual

**Completado:**
- ✅ Estructura básica de proyecto
- ✅ Referencias configuradas

**Pendiente:**
- ⏳ Definir modelos de entidad
- ⏳ Implementar repositorios
- ⏳ Configurar contexto de base de datos
- ⏳ Agregar migraciones

---

## 🔗 Referencias

- **Documentación General:** [`Docs/General/`](../../General/)
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
