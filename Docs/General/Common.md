# 🔗 Common - Código Compartido

Guía para usar y mantener el código compartido entre proyectos de Tandem 2026.

---

## 📦 ¿Qué es Common?

**Common** es una carpeta en la raíz del repositorio que contiene **código reutilizable** entre todos los proyectos:
- DAL
- Desing
- ZwcadPlugin

**Ubicación:** `C:\00_Tandem2026\Common\`

---

## 📂 Estructura

```
Common/
├── Utils/              # Utilidades generales
│   └── FileHelper.cs   # Ejemplo: helper de archivos
├── Models/             # Modelos de datos compartidos
├── Interfaces/         # Contratos e interfaces
├── Constants/          # Constantes de aplicación
├── Extensions/         # Métodos de extensión
└── README.md           # Documentación técnica
```

---

## 🎯 Propósito

**Common** evita duplicación de código almacenando:
- **Utilidades:** Helpers, validadores, conversiones
- **Modelos:** DTOs, entidades compartidas
- **Interfaces:** Contratos comunes entre capas
- **Constantes:** Valores fijos (rutas, configuración)
- **Extensions:** Métodos de extensión para tipos .NET

---

## 🔧 Cómo Usar Código Compartido

### **Opción 1: Enlaces de Archivo (Actual)**

**Agregar archivo de Common a un proyecto:**

1. Abre el `.csproj` en Visual Studio
2. Click derecho → **Add** → **Existing Item...**
3. Navega a `Common\Utils\FileHelper.cs`
4. En lugar de **Add**, click en la flecha → **Add As Link**

**Resultado:**
```xml
<ItemGroup>
  <Compile Include="..\Common\Utils\FileHelper.cs">
	<Link>Common\Utils\FileHelper.cs</Link>
  </Compile>
</ItemGroup>
```

**Ventajas:**
- Sin proyecto adicional
- Compilación directa
- No requiere referencias

**Desventajas:**
- Agregar manualmente cada archivo
- No hay IntelliSense automático

---

### **Opción 2: Proyecto Compartido (Futura)**

**Crear `Common.csproj`:**

1. Crear proyecto de biblioteca de clases
2. Target Framework: `.NET Framework 4.8`
3. Mover archivos de `Common\` al proyecto
4. Referenciar desde otros proyectos

**Ventajas:**
- Compilación única
- IntelliSense completo
- Gestión centralizada

**Desventajas:**
- Requiere configuración inicial
- Dependencia adicional

---

## 📝 Convenciones

### **Namespaces:**
```csharp
Tandem2026.Common.Utils
Tandem2026.Common.Models
Tandem2026.Common.Interfaces
Tandem2026.Common.Constants
Tandem2026.Common.Extensions
```

### **Nombres de Archivos:**
- Un tipo público por archivo
- Nombre del archivo = Nombre del tipo
- Ejemplos: `FileHelper.cs`, `ProjectModel.cs`, `IRepository.cs`

### **Documentación:**
```csharp
/// <summary>
/// Descripción breve del propósito
/// </summary>
/// <param name="parameter">Descripción del parámetro</param>
/// <returns>Descripción del valor de retorno</returns>
public static bool CanReadFile(string filePath)
{
	// Implementación
}
```

---

## 🚀 Ejemplo Práctico

### **1. Crear Utilidad Compartida:**

**Archivo:** `Common/Utils/StringHelper.cs`

```csharp
using System;

namespace Tandem2026.Common.Utils
{
	/// <summary>
	/// Utilidades para manipulación de cadenas
	/// </summary>
	public static class StringHelper
	{
		/// <summary>
		/// Verifica si una cadena es nula, vacía o solo espacios
		/// </summary>
		public static bool IsNullOrWhiteSpace(string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		/// <summary>
		/// Trunca una cadena a la longitud especificada
		/// </summary>
		public static string Truncate(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength 
				? value 
				: value.Substring(0, maxLength);
		}
	}
}
```

---

### **2. Usar en Proyecto:**

**Desde DAL o ZwcadPlugin:**

```csharp
using Tandem2026.Common.Utils;

namespace DAL.Repositories
{
	public class ProjectRepository
	{
		public void ValidateProjectName(string name)
		{
			if (StringHelper.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Nombre requerido");
			}

			var truncated = StringHelper.Truncate(name, 100);
			// ... más lógica
		}
	}
}
```

---

## 📋 Categorías de Código Compartido

### **Utils/** - Utilidades

**Propósito:** Helpers estáticos sin estado

**Ejemplos:**
- `FileHelper` - Operaciones de archivos
- `StringHelper` - Manipulación de texto
- `ValidationHelper` - Validaciones comunes
- `ConversionHelper` - Conversiones de tipos

---

### **Models/** - Modelos

**Propósito:** DTOs y entidades compartidas

**Ejemplos:**
- `ProjectModel` - Representación de proyecto
- `UserModel` - Datos de usuario
- `ConfigurationModel` - Settings de aplicación

---

### **Interfaces/** - Contratos

**Propósito:** Definiciones de servicios/repositorios

**Ejemplos:**
- `IRepository<T>` - Contrato de repositorio genérico
- `ILogger` - Contrato de logging
- `IValidator<T>` - Contrato de validación

---

### **Constants/** - Constantes

**Propósito:** Valores fijos de aplicación

**Ejemplos:**
```csharp
namespace Tandem2026.Common.Constants
{
	public static class FileConstants
	{
		public const string DefaultExtension = ".dwg";
		public const int MaxFileNameLength = 255;
		public const string TempFolder = "C:\\Temp\\Tandem";
	}

	public static class ErrorMessages
	{
		public const string FileNotFound = "Archivo no encontrado";
		public const string InvalidInput = "Entrada no válida";
	}
}
```

---

### **Extensions/** - Extensiones

**Propósito:** Métodos de extensión para tipos .NET

**Ejemplos:**
```csharp
namespace Tandem2026.Common.Extensions
{
	public static class StringExtensions
	{
		public static bool IsValidEmail(this string email)
		{
			// Validación de email
		}

		public static string ToTitleCase(this string text)
		{
			// Convertir a Title Case
		}
	}

	public static class CollectionExtensions
	{
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
		{
			return collection == null || !collection.Any();
		}
	}
}
```

---

## 🔄 Flujo de Trabajo

### **Agregar Nuevo Código Compartido:**

1. **Identifica** código duplicado entre proyectos
2. **Crea** archivo en `Common\<Categoria>\`
3. **Define** namespace `Tandem2026.Common.<Categoria>`
4. **Documenta** con XML comments
5. **Referencia** desde proyectos que lo necesiten
6. **Prueba** en al menos 2 proyectos
7. **Commit** con mensaje descriptivo

**Ejemplo:**
```bash
git add Common/Utils/ValidationHelper.cs
git commit -m "feat: Agregar ValidationHelper compartido AB#<ID>"
```

---

### **Modificar Código Compartido:**

⚠️ **CUIDADO:** Los cambios afectan todos los proyectos

1. **Verifica** impacto en todos los proyectos
2. **Actualiza** documentación XML
3. **Prueba** en todos los proyectos afectados
4. **Valida** compilación completa
5. **Commit** explicando el cambio

---

## ⚠️ Qué NO Poner en Common

**Evita agregar:**
- ❌ Código específico de un solo proyecto
- ❌ Dependencias pesadas (reduce referencias)
- ❌ Lógica de negocio compleja
- ❌ Configuración específica de entorno
- ❌ Código experimental o inestable

**Si solo 1 proyecto lo usa → NO va en Common**

---

## 🧪 Testing

**Validar cambios en Common:**

```powershell
# Compilar solución completa
cd C:\00_Tandem2026
msbuild Design.sln /t:Rebuild /p:Configuration=Debug
```

**Resultado esperado:**
```
Build succeeded.
	0 Warning(s)
	0 Error(s)
```

---

## 📊 Métricas

**Estado Actual:**
- Categorías: 5 (Utils, Models, Interfaces, Constants, Extensions)
- Archivos: 1 (FileHelper.cs)
- Proyectos usando Common: Pendiente

**Objetivo:**
- Reducir duplicación de código en 30%
- Centralizar utilidades comunes
- Mejorar mantenibilidad

---

## 🔗 Referencias

- **Documentación técnica:** [`Common/README.md`](../../Common/README.md)
- **Estructura del repositorio:** [`Docs/General/Estructura-Repositorio.md`](./Estructura-Repositorio.md)
- **Convenciones de código:** [`Docs/General/Convenciones.md`](./Convenciones.md)

---

## ❓ Preguntas Frecuentes

### **¿Cuándo crear código compartido?**
Cuando el mismo código se usa (o se usará) en **2 o más proyectos**.

### **¿Cómo decido la categoría?**
- Funciones estáticas sin estado → `Utils/`
- Clases de datos → `Models/`
- Definiciones de contrato → `Interfaces/`
- Valores fijos → `Constants/`
- Métodos de extensión → `Extensions/`

### **¿Puedo agregar subdirectorios?**
Sí, si mejora la organización:
```
Common/
└── Utils/
	├── File/
	│   └── FileHelper.cs
	└── String/
		└── StringHelper.cs
```

### **¿Qué hacer con código obsoleto?**
1. Marcar con `[Obsolete]`
2. Documentar alternativa
3. Dar tiempo de migración
4. Eliminar en próxima versión mayor

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
