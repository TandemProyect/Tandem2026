# 📦 Common - Código Compartido

Esta carpeta contiene código y recursos **compartidos** entre todos los proyectos de la solución Tandem 2026.

---

## 📁 Estructura

```
Common/
├── Utils/          # Utilidades y helpers compartidos
├── Models/         # Modelos de datos comunes
├── Interfaces/     # Contratos e interfaces compartidas
├── Constants/      # Constantes y enumeraciones
└── Extensions/     # Métodos de extensión
```

---

## 🎯 Propósito

Centralizar código reutilizable para evitar duplicación entre:
- **DAL** (Data Access Layer)
- **Desing** (Lógica de diseño)
- **ZwcadPlugin** (Plugin de ZWCAD)
- Otros proyectos futuros

---

## 📋 Uso

### **1. Referenciar desde otros proyectos**

En cada proyecto que necesite usar código común:

**Opción A: Link de archivos**
```xml
<ItemGroup>
  <Compile Include="..\Common\Utils\*.cs">
	<Link>Common\Utils\%(Filename)%(Extension)</Link>
  </Compile>
</ItemGroup>
```

**Opción B: Crear proyecto Common.csproj**
1. Crear proyecto de librería: `Common\Common.csproj`
2. Agregar referencia desde otros proyectos

---

## 📂 Contenido Sugerido

### **Utils/**
- `FileHelper.cs` - Operaciones de archivos
- `StringHelper.cs` - Manipulación de cadenas
- `LogHelper.cs` - Sistema de logging común

### **Models/**
- `Result<T>.cs` - Resultado genérico de operaciones
- `ApiResponse.cs` - Respuestas de API
- DTOs compartidos

### **Interfaces/**
- `IRepository<T>.cs` - Contrato de repositorio
- `ILogger.cs` - Interfaz de logging
- `IValidator<T>.cs` - Validación genérica

### **Constants/**
- `AppConstants.cs` - Constantes de aplicación
- `ErrorMessages.cs` - Mensajes de error
- `ConfigKeys.cs` - Claves de configuración

### **Extensions/**
- `StringExtensions.cs` - Extensiones para strings
- `DateTimeExtensions.cs` - Extensiones para fechas
- `CollectionExtensions.cs` - Extensiones para colecciones

---

## 🚀 Próximos Pasos

1. **Decidir estructura:** ¿Archivos sueltos o proyecto separado?
2. **Mover código duplicado** existente a `Common/`
3. **Actualizar referencias** en proyectos existentes

---

## 📝 Convenciones

- **Namespace:** `Tandem2026.Common.*`
- **Nombrado:** PascalCase para clases y métodos
- **Documentación:** XML comments en clases públicas
- **Tests:** Crear `Common.Tests` si es necesario

---

**Creado:** 24/04/2026  
**Última actualización:** 24/04/2026
