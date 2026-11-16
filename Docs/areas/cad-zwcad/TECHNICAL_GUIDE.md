# Guía de Implementación Técnica - Tandem 2026

**Documento complementario al README.md**

---

## 🏗️ Arquitectura del Plugin

### Flujo de Ejecución

```
ZWCAD Inicia
	↓
MenuManager.Initialize()
	↓
Registra comandos con [CommandMethod]
	↓
Usuario ejecuta comando (ej: MVCCONEXION)
	↓
ZWCAD invoca método marcado con [CommandMethod("MVCCONEXION")]
	↓
Código del plugin se ejecuta
	↓
(Puede abrir ventanas WPF, modificar dibujo, etc.)
```

### Puntos de Entrada

**1. `IExtensionApplication` (MenuManager.cs)**
- Se ejecuta al cargar ZWCAD
- Método `Initialize()` se llama automáticamente
- Método `Terminate()` se llama al cerrar ZWCAD

**2. Comandos `[CommandMethod]`**
- Cada método marcado con este atributo se convierte en comando ZWCAD
- El usuario puede invocarlos desde la línea de comandos o ribbon

---

## 🔌 API de ZWCAD Usada

### Namespaces Principales

```csharp
using ZwSoft.ZwCAD.ApplicationServices;  // Application, Document
using ZwSoft.ZwCAD.EditorInput;          // Editor, PromptOptions
using ZwSoft.ZwCAD.Runtime;              // Atributos: CommandMethod, ExtensionApplication
```

### Clases Clave

| Clase | Propósito | Ejemplo |
|-------|-----------|---------|
| `Application` | Punto de entrada principal | `Application.DocumentManager` |
| `Document` | Representa un dibujo abierto | `doc.Editor`, `doc.Database` |
| `Editor` | Interacción con usuario (mensajes, selección) | `ed.WriteMessage()` |
| `Database` | Acceso a entidades del dibujo | Lectura/escritura de líneas, bloques, etc. |

---

## 💻 Código de Ejemplo: Implementar un Comando

### Comando Simple (TANDEM)

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("TANDEM")]
public void MostrarComandos()
{
	// 1. Obtener documento activo
	Document doc = Application.DocumentManager.MdiActiveDocument;
	if (doc == null) return;

	// 2. Obtener editor (para escribir mensajes)
	Editor ed = doc.Editor;

	// 3. Escribir en línea de comandos
	ed.WriteMessage("\n--- Tandem 2026 ---");
	ed.WriteMessage("\n  MVCCONEXION      Panel principal");
}
```

### Comando que Abre Ventana WPF (MVCCONEXION)

**⚠️ PENDIENTE DE IMPLEMENTAR**

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("MVCCONEXION")]
public void AbrirPanelPrincipal()
{
	try
	{
		// Crear ventana WPF
		var ventana = new UI.Views.MainWindow();

		// Mostrar como modal (bloquea ZWCAD hasta cerrar)
		ZwSoft.ZwCAD.ApplicationServices.Application.ShowModalWindow(ventana);

		// O mostrar como no modal (permite trabajar en ZWCAD)
		// ventana.Show();
	}
	catch (System.Exception ex)
	{
		Document doc = Application.DocumentManager.MdiActiveDocument;
		doc?.Editor.WriteMessage($"\nError al abrir panel: {ex.Message}");
	}
}
```

### Comando que Lee Entidades del Dibujo (DETECTARMUROS)

**⚠️ PENDIENTE DE IMPLEMENTAR - PLANTILLA**

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("DETECTARMUROS")]
public void DetectarMuros()
{
	Document doc = Application.DocumentManager.MdiActiveDocument;
	if (doc == null) return;

	Editor ed = doc.Editor;
	Database db = doc.Database;

	// Iniciar transacción (obligatorio para acceder a la base de datos)
	using (Transaction tr = db.TransactionManager.StartTransaction())
	{
		try
		{
			// Acceder a la tabla de bloques (ModelSpace)
			BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
			BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

			// Iterar sobre entidades en ModelSpace
			foreach (ObjectId objId in ms)
			{
				Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);

				// Ejemplo: Detectar líneas
				if (ent is Line linea)
				{
					ed.WriteMessage($"\nLínea encontrada: {linea.StartPoint} → {linea.EndPoint}");
					// Aquí va la lógica de detección de muros
				}

				// Ejemplo: Detectar polilíneas
				if (ent is Polyline pline)
				{
					ed.WriteMessage($"\nPolilínea con {pline.NumberOfVertices} vértices");
				}
			}

			tr.Commit();
			ed.WriteMessage("\nDetección de muros completada.");
		}
		catch (System.Exception ex)
		{
			ed.WriteMessage($"\nError: {ex.Message}");
			tr.Abort();
		}
	}
}
```

### Comando que Crea Entidades 3D (GENERAR3D)

**⚠️ PENDIENTE DE IMPLEMENTAR - PLANTILLA**

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("GENERAR3D")]
public void Generar3D()
{
	Document doc = Application.DocumentManager.MdiActiveDocument;
	if (doc == null) return;

	Editor ed = doc.Editor;
	Database db = doc.Database;

	using (Transaction tr = db.TransactionManager.StartTransaction())
	{
		try
		{
			BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
			BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

			// Ejemplo: Crear un sólido 3D (caja)
			using (Solid3d solido = new Solid3d())
			{
				// Crear caja de 100x200x300
				solido.CreateBox(100, 200, 300);

				// Posicionar en el origen
				solido.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, 0)));

				// Agregar a ModelSpace
				ms.AppendEntity(solido);
				tr.AddNewlyCreatedDBObject(solido, true);
			}

			tr.Commit();
			ed.WriteMessage("\nModelo 3D generado.");
		}
		catch (System.Exception ex)
		{
			ed.WriteMessage($"\nError: {ex.Message}");
			tr.Abort();
		}
	}
}
```

---

## 🎨 Integración WPF con ZWCAD

### Invocar Comandos ZWCAD desde WPF

**Método 1: SendStringToExecute**

```csharp
// En MainViewModel.cs
private void DetectarMuros()
{
	try
	{
		Document doc = Application.DocumentManager.MdiActiveDocument;
		if (doc != null)
		{
			// Envía comando a la línea de comandos de ZWCAD
			doc.SendStringToExecute("DETECTARMUROS ", true, false, false);
			MensajeEstado = "Ejecutando DETECTARMUROS...";
		}
	}
	catch (System.Exception ex)
	{
		MensajeEstado = $"Error: {ex.Message}";
	}
}
```

**Método 2: Invocar método directamente**

```csharp
// En MainViewModel.cs
private void DetectarMuros()
{
	try
	{
		// Obtener instancia de MenuManager
		var menuManager = new MenuManager();
		menuManager.DetectarMuros();  // Llamar directamente al método
		MensajeEstado = "Detección completada";
	}
	catch (System.Exception ex)
	{
		MensajeEstado = $"Error: {ex.Message}";
	}
}
```

### Mostrar Ventana WPF desde ZWCAD

```csharp
// Modal (bloquea ZWCAD)
ZwSoft.ZwCAD.ApplicationServices.Application.ShowModalWindow(ventana);

// No modal (permite trabajar en ZWCAD)
ventana.Show();

// Establecer ventana padre (recomendado)
new System.Windows.Interop.WindowInteropHelper(ventana)
{
	Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
};
```

---

## 📦 Gestión de Paquetes NuGet

### Paquetes Actuales

**En packages.config:**
```xml
<package id="EntityFramework" version="6.5.1" targetFramework="net48" />
<package id="System.Linq.Dynamic.Core" version="1.7.2" targetFramework="net48" />
<package id="System.ValueTuple" version="4.5.0" targetFramework="net48" />
```

### Actualizar Paquete Manualmente

1. Editar `packages.config`:
   ```xml
   <package id="NombrePaquete" version="X.Y.Z" targetFramework="net48" />
   ```

2. Editar `.csproj` (actualizar `HintPath`):
   ```xml
   <Reference Include="NombrePaquete, Version=X.Y.Z, ...">
	 <HintPath>..\packages\NombrePaquete.X.Y.Z\lib\net48\NombrePaquete.dll</HintPath>
   </Reference>
   ```

3. Descargar paquete:
   ```powershell
   nuget install NombrePaquete -Version X.Y.Z -OutputDirectory packages
   ```

4. Compilar y verificar

---

## 🛠️ Compilación y Depuración

### Compilar desde Línea de Comandos

```powershell
# Desde la raíz del proyecto
msbuild Design.sln /t:Rebuild /p:Configuration=Debug /v:minimal /nologo

# Solo el plugin
msbuild TamdenZwcadPluging\ZwcadPlugin\ZwcadPlugin.csproj /t:Rebuild /p:Configuration=Debug
```

### Depurar el Plugin en Visual Studio

1. **Configurar proyecto de inicio:**
   - Clic derecho en `ZwcadPlugin` → Propiedades
   - Pestaña "Depuración"
   - Acción de inicio: "Programa externo"
   - Ruta: `C:\Program Files\ZWSOFT\ZWCAD 2026\zwcad.exe`

2. **Establecer puntos de interrupción** en tu código

3. **Presionar F5** (Iniciar depuración)
   - Visual Studio iniciará ZWCAD
   - ZWCAD cargará tu plugin
   - Los puntos de interrupción se activarán cuando se ejecuten los comandos

4. **Cargar el plugin manualmente en ZWCAD:**
   ```
   Comando: NETLOAD
   Seleccionar: C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
   ```

### Logs y Diagnóstico

```csharp
// Escribir en ventana de comandos de ZWCAD
Editor ed = doc.Editor;
ed.WriteMessage("\n[DEBUG] Valor de variable: " + variable);

// Escribir en Output de Visual Studio (solo durante depuración)
System.Diagnostics.Debug.WriteLine("Mensaje de debug");

// Mostrar cuadro de diálogo
ZwSoft.ZwCAD.ApplicationServices.Application.ShowAlertDialog("Mensaje de alerta");
```

---

## 🔒 Manejo de Errores

### Patrón Recomendado

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("MICOMANDO")]
public void MiComando()
{
	Document doc = Application.DocumentManager.MdiActiveDocument;
	if (doc == null)
	{
		Application.ShowAlertDialog("No hay documento activo.");
		return;
	}

	Editor ed = doc.Editor;
	Database db = doc.Database;

	using (Transaction tr = db.TransactionManager.StartTransaction())
	{
		try
		{
			// Lógica del comando aquí

			tr.Commit();
			ed.WriteMessage("\nComando completado exitosamente.");
		}
		catch (System.Exception ex)
		{
			ed.WriteMessage($"\n❌ Error: {ex.Message}");
			ed.WriteMessage($"\nStack: {ex.StackTrace}");
			tr.Abort();
		}
	}
}
```

---

## 🧪 Testing

### Comandos de Prueba

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("TESTPLUGIN")]
public void TestPlugin()
{
	Document doc = Application.DocumentManager.MdiActiveDocument;
	if (doc == null) return;

	Editor ed = doc.Editor;
	ed.WriteMessage("\n=== TEST PLUGIN ===");
	ed.WriteMessage($"\nDocumento: {doc.Name}");
	ed.WriteMessage($"\nEntidades: {doc.Database.Handseed}");
	ed.WriteMessage($"\nUnidades: {doc.Database.Insunits}");
	ed.WriteMessage("\n===================");
}
```

### Verificar Carga del Plugin

```csharp
public void Initialize()
{
	try
	{
		Document doc = Application.DocumentManager.MdiActiveDocument;
		doc?.Editor.WriteMessage("\n✅ Tandem 2026 cargado correctamente.");
		doc?.Editor.WriteMessage($"\nVersion: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
	}
	catch (System.Exception ex)
	{
		System.Windows.MessageBox.Show($"Error al cargar plugin: {ex.Message}");
	}
}
```

---

## 📐 Convenciones de Código

### Nombres de Comandos
- TODO EN MAYÚSCULAS (convención de AutoCAD/ZWCAD)
- Sin espacios: `DETECTARMUROS` no `DETECTAR MUROS`
- Descriptivos pero concisos

### Estructura de Archivos
```
UI\
  ViewModels\
	MainViewModel.cs      # Un ViewModel por vista
	[Nombre]ViewModel.cs
  Views\
	MainWindow.xaml       # Vista principal
	[Nombre]Window.xaml   # Otras ventanas/diálogos
Commands\
  [Categoria]Commands.cs  # Agrupar comandos relacionados
Helpers\
  [Utilidad]Helper.cs     # Funciones auxiliares
```

### Comentarios
```csharp
/// <summary>
/// Detecta muros en el dibujo actual y construye el modelo topológico.
/// </summary>
/// <remarks>
/// El comando lee todas las líneas y polilíneas en ModelSpace,
/// identifica muros basándose en su orientación y longitud,
/// y construye un grafo topológico para análisis posterior.
/// </remarks>
[ZwSoft.ZwCAD.Runtime.CommandMethod("DETECTARMUROS")]
public void DetectarMuros()
{
	// ...
}
```

---

## 🚀 Checklist de Implementación

Cuando implementes un nuevo comando, sigue estos pasos:

- [ ] 1. Agregar método con `[CommandMethod("NOMBRECOMANDO")]`
- [ ] 2. Implementar lógica dentro de `using (Transaction tr = ...)`
- [ ] 3. Manejar errores con try-catch
- [ ] 4. Escribir mensajes informativos con `ed.WriteMessage()`
- [ ] 5. Hacer commit con mensaje descriptivo
- [ ] 6. Probar en ZWCAD ejecutando el comando
- [ ] 7. Actualizar documentación si es necesario
- [ ] 8. Verificar que compile sin warnings

---

## 📚 Referencias Útiles

### Documentación Oficial
- ZWCAD API Reference (instalada con ZWCAD SDK)
- AutoCAD .NET API (compatible en gran medida)

### Tipos de Entidades Comunes
- `Line` - Línea
- `Polyline` - Polilínea 2D
- `Polyline3d` - Polilínea 3D
- `Arc` - Arco
- `Circle` - Círculo
- `Solid3d` - Sólido 3D
- `Text` - Texto de línea simple
- `MText` - Texto multilínea
- `BlockReference` - Referencia a bloque (insert)

### Propiedades Útiles
```csharp
// Geometría
line.StartPoint, line.EndPoint
pline.NumberOfVertices
circle.Center, circle.Radius

// Propiedades
entity.Layer
entity.Color
entity.Linetype

// Transformaciones
entity.TransformBy(Matrix3d.Displacement(vector))
entity.TransformBy(Matrix3d.Rotation(angle, axis, point))
```

---

**Fin de la guía técnica**

*Complementa el README.md principal*
