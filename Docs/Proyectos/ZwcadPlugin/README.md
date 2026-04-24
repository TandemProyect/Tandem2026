# 📚 ZwcadPlugin - Plugin para ZWCAD

Documentación del proyecto Plugin ZWCAD para Tandem 2026.

---

## 📋 Información General

**Proyecto:** ZwcadPlugin  
**Ubicación:** `C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\`  
**Tipo:** Class Library (.NET Framework 4.8)  
**Namespace:** `Tandem2026.ZwcadPlugin`  
**Target:** ZWCAD 2026

---

## 🎯 Propósito

Plugin para ZWCAD que proporciona:
- Comandos personalizados para Tandem
- Interfaz de usuario WPF
- Integración con ribbon/menús
- Acceso a funcionalidad de negocio (Desing)

---

## 🏗️ Arquitectura

```
ZwcadPlugin/
├── Commands/              # Comandos ZWCAD
│   ├── PanelCommands.cs
│   ├── AnnotationCommands.cs
│   └── ...
├── UI/                    # Interfaz WPF
│   ├── Views/            # Ventanas XAML
│   │   ├── MainWindow.xaml
│   │   └── MainWindow.xaml.cs
│   └── ViewModels/       # ViewModels MVVM
│       └── MainWindowViewModel.cs
├── MNU/                   # Menús y Ribbons
│   ├── Tandem2026.cui    # Archivo de menú principal
│   └── Iconos/           # Iconos para ribbon
│       ├── Bootstrap-Icons/
│       └── png/
├── Models/                # Modelos específicos del plugin
├── Helpers/               # Utilidades del plugin
└── Properties/            # AssemblyInfo, etc.
```

---

## 📦 Dependencias

### **Referencias ZWCAD:**
- `ZwSoft.ZwCAD.ApplicationServices.dll`
- `ZwSoft.ZwCAD.DatabaseServices.dll`
- `ZwSoft.ZwCAD.Runtime.dll`
- `ZwSoft.ZwCAD.EditorInput.dll`

**Ubicación:** `C:\Program Files\ZWCAD 2026\ZRX\`

---

### **Referencias Internas:**
- `Desing` - Lógica de negocio
- `DAL` - Acceso a datos
- `Common` - Utilidades compartidas

---

### **NuGet Packages:**
- System.Windows.Interactivity (para WPF)

---

## 🔌 Comandos ZWCAD

### **Estructura Básica:**

```csharp
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;

namespace Tandem2026.ZwcadPlugin.Commands
{
	public class PanelCommands
	{
		[CommandMethod("TANDEM_ABRIR_PANEL")]
		public void AbrirPanel()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;

			if (doc == null)
			{
				Application.ShowAlertDialog("No hay documento activo");
				return;
			}

			try
			{
				var mainWindow = new UI.Views.MainWindow();
				Application.ShowModelessWindow(mainWindow);
			}
			catch (System.Exception ex)
			{
				doc.Editor.WriteMessage($"\nError: {ex.Message}");
			}
		}
	}
}
```

---

### **Patrón de Transacciones:**

```csharp
[CommandMethod("TANDEM_CREAR_LINEA")]
public void CrearLinea()
{
	var doc = Application.DocumentManager.MdiActiveDocument;
	var db = doc.Database;
	var ed = doc.Editor;

	// Pedir puntos al usuario
	var pt1Result = ed.GetPoint("\nPunto inicial: ");
	if (pt1Result.Status != PromptStatus.OK) return;

	var pt2Result = ed.GetPoint("\nPunto final: ");
	if (pt2Result.Status != PromptStatus.OK) return;

	// Transacción
	using (var tr = db.TransactionManager.StartTransaction())
	{
		try
		{
			// Abrir BlockTableRecord para escritura
			var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
			var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

			// Crear línea
			var line = new Line(pt1Result.Value, pt2Result.Value);

			// Agregar a la base de datos
			btr.AppendEntity(line);
			tr.AddNewlyCreatedDBObject(line, true);

			tr.Commit();
			ed.WriteMessage("\nLínea creada exitosamente");
		}
		catch (System.Exception ex)
		{
			tr.Abort();
			ed.WriteMessage($"\nError: {ex.Message}");
		}
	}
}
```

---

## 🖥️ Interfaz WPF (MVVM)

### **View (MainWindow.xaml):**

```xml
<Window x:Class="Tandem2026.ZwcadPlugin.UI.Views.MainWindow"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		Title="Tandem 2026" Height="450" Width="800">
	<Grid>
		<Grid.RowDefinitions>
			<RowDefinition Height="Auto"/>
			<RowDefinition Height="*"/>
			<RowDefinition Height="Auto"/>
		</Grid.RowDefinitions>

		<!-- Header -->
		<TextBlock Grid.Row="0" 
				   Text="Gestión de Proyectos" 
				   FontSize="20" 
				   Margin="10"/>

		<!-- Content -->
		<ListBox Grid.Row="1" 
				 ItemsSource="{Binding Projects}"
				 SelectedItem="{Binding SelectedProject}"
				 DisplayMemberPath="Name"
				 Margin="10"/>

		<!-- Footer -->
		<StackPanel Grid.Row="2" 
					Orientation="Horizontal" 
					HorizontalAlignment="Right" 
					Margin="10">
			<Button Content="Crear" 
					Command="{Binding CreateCommand}" 
					Width="80" 
					Margin="5"/>
			<Button Content="Editar" 
					Command="{Binding EditCommand}" 
					Width="80" 
					Margin="5"/>
			<Button Content="Eliminar" 
					Command="{Binding DeleteCommand}" 
					Width="80" 
					Margin="5"/>
		</StackPanel>
	</Grid>
</Window>
```

---

### **Code-Behind (MainWindow.xaml.cs):**

```csharp
using System.Windows;
using Tandem2026.ZwcadPlugin.UI.ViewModels;

namespace Tandem2026.ZwcadPlugin.UI.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel();
		}
	}
}
```

---

### **ViewModel (MainWindowViewModel.cs):**

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Tandem2026.Desing.Services;
using Tandem2026.Desing.DTOs;

namespace Tandem2026.ZwcadPlugin.UI.ViewModels
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		private readonly ProjectService _projectService;
		private ObservableCollection<ProjectDTO> _projects;
		private ProjectDTO _selectedProject;

		public MainWindowViewModel()
		{
			_projectService = new ProjectService();
			LoadProjects();

			CreateCommand = new RelayCommand(CreateProject);
			EditCommand = new RelayCommand(EditProject, CanEditOrDelete);
			DeleteCommand = new RelayCommand(DeleteProject, CanEditOrDelete);
		}

		public ObservableCollection<ProjectDTO> Projects
		{
			get => _projects;
			set
			{
				_projects = value;
				OnPropertyChanged(nameof(Projects));
			}
		}

		public ProjectDTO SelectedProject
		{
			get => _selectedProject;
			set
			{
				_selectedProject = value;
				OnPropertyChanged(nameof(SelectedProject));
				CommandManager.InvalidateRequerySuggested();
			}
		}

		public ICommand CreateCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand DeleteCommand { get; }

		private void LoadProjects()
		{
			var projects = _projectService.GetAllProjects();
			Projects = new ObservableCollection<ProjectDTO>(projects);
		}

		private void CreateProject(object parameter)
		{
			// Abrir ventana de creación
			// ...
		}

		private void EditProject(object parameter)
		{
			if (SelectedProject != null)
			{
				// Abrir ventana de edición
				// ...
			}
		}

		private void DeleteProject(object parameter)
		{
			if (SelectedProject != null)
			{
				_projectService.DeleteProject(SelectedProject.Id);
				LoadProjects();
			}
		}

		private bool CanEditOrDelete(object parameter)
		{
			return SelectedProject != null;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
```

---

### **RelayCommand Helper:**

```csharp
using System;
using System.Windows.Input;

namespace Tandem2026.ZwcadPlugin.Helpers
{
	public class RelayCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
```

---

## 🎨 Ribbon y Menús

### **Archivo CUI:**
**Ubicación:** `MNU/Tandem2026.cui`

**Cargar en ZWCAD:**
```
CUICONFIG → Load → Tandem2026.cui
```

---

### **Estructura del Ribbon:**

```
Ribbon Tab: TANDEM
├── Panel: Proyectos
│   ├── Button: Abrir Panel (TANDEM_ABRIR_PANEL)
│   ├── Button: Crear Proyecto (TANDEM_CREAR_PROYECTO)
│   └── Button: Listar Proyectos (TANDEM_LISTAR_PROYECTOS)
└── Panel: Herramientas
	├── Button: Configuración
	└── Button: Ayuda
```

---

### **Iconos:**

**Ubicación:** `MNU/Iconos/png/`

**Formatos necesarios:**
- 16x16 px (pequeño)
- 32x32 px (grande)

**Convención:**
```
nombre-16x16.png
nombre-32x32.png
```

**Ejemplo:**
```
folder-16x16.png
folder-32x32.png
```

---

## 🛠️ Compilación y Deploy

### **Post-Build Event:**

**Propósito:** Copiar `.dll` y `.cui` a carpeta de ZWCAD

```xml
<PropertyGroup>
  <PostBuildEvent>
	xcopy "$(TargetPath)" "C:\Program Files\ZWCAD 2026\UserDataCache\Support\" /Y /I
	xcopy "$(ProjectDir)MNU\Tandem2026.cui" "C:\Program Files\ZWCAD 2026\UserDataCache\Support\" /Y /I
  </PostBuildEvent>
</PropertyGroup>
```

---

### **Cargar Plugin en ZWCAD:**

**Método 1: Autoload (Automático)**
1. Compilar proyecto
2. Copiar `.dll` a `C:\Program Files\ZWCAD 2026\UserDataCache\Support\`
3. ZWCAD carga automáticamente al iniciar

**Método 2: NETLOAD (Manual)**
```
NETLOAD → Seleccionar ZwcadPlugin.dll
```

---

## 📝 Convenciones

### **Comandos:**
- Prefijo: `TANDEM_`
- Formato: `TANDEM_<ACCION>_<OBJETO>`
- Mayúsculas
- Ejemplo: `TANDEM_CREAR_PANEL`

### **Ventanas WPF:**
- Ubicación: `UI/Views/`
- Patrón MVVM
- Code-behind mínimo
- Toda lógica en ViewModel

### **ViewModels:**
- Ubicación: `UI/ViewModels/`
- Implementar `INotifyPropertyChanged`
- Usar `RelayCommand` para comandos
- Nombre: `<View>ViewModel`

---

## 🧪 Testing

### **Testing Manual en ZWCAD:**

1. Compilar proyecto
2. Abrir ZWCAD
3. Ejecutar `NETLOAD` (si no autoload)
4. Probar comando: `TANDEM_ABRIR_PANEL`
5. Verificar funcionalidad

---

### **Debugging:**

1. **Visual Studio:**
   - Project Properties → Debug
   - Start external program: `C:\Program Files\ZWCAD 2026\zwcad.exe`
   - F5 para iniciar debugging

2. **Breakpoints:**
   - Colocar en comandos o eventos
   - ZWCAD se abrirá en modo debug

---

## 🚀 Ciclo de Desarrollo

1. **Implementar funcionalidad** en Visual Studio
2. **Compilar** (Ctrl+Shift+B)
3. **Copiar** `.dll` y `.cui` (automático con Post-Build)
4. **Abrir ZWCAD** o recargar con `NETLOAD`
5. **Probar** comando
6. **Iterar** según necesidad

---

## 📊 Estado Actual

**Completado:**
- ✅ Estructura de proyecto
- ✅ Referencias ZWCAD configuradas
- ✅ MainWindow con MVVM creado
- ✅ Comando TANDEM_ABRIR_PANEL funcionando
- ✅ Sistema de iconos Bootstrap implementado
- ✅ Menú CUI configurado
- ✅ Documentación técnica completa

**En Progreso:**
- 🔄 Implementar comandos de negocio
- 🔄 Completar ribbon con todos los comandos

**Pendiente:**
- ⏳ Integración completa con Desing/DAL
- ⏳ Testing exhaustivo
- ⏳ Optimizaciones de performance

---

## 🔗 Referencias Importantes

### **Documentación del Plugin:**
- **README Principal:** [`TamdenZwcadPluging/ZwcadPlugin/README.md`](../../../TamdenZwcadPluging/ZwcadPlugin/README.md)
- **Guía Técnica:** [`TamdenZwcadPluging/ZwcadPlugin/TECHNICAL_GUIDE.md`](../../../TamdenZwcadPluging/ZwcadPlugin/TECHNICAL_GUIDE.md)
- **Iconos:** [`TamdenZwcadPluging/ZwcadPlugin/MNU/Iconos/README_ICONOS.md`](../../../TamdenZwcadPluging/ZwcadPlugin/MNU/Iconos/README_ICONOS.md)

### **Documentación General:**
- **Desing:** [`Docs/Proyectos/Desing/`](../Desing/)
- **DAL:** [`Docs/Proyectos/DAL/`](../DAL/)
- **Common:** [`Docs/General/Common.md`](../../General/Common.md)
- **Convenciones:** [`Docs/General/Convenciones.md`](../../General/Convenciones.md)

---

## ❓ Preguntas Frecuentes

### **¿Cómo agregar nuevo comando?**
1. Crear método en `Commands/`
2. Decorar con `[CommandMethod("TANDEM_NOMBRE")]`
3. Compilar
4. Agregar al ribbon en CUI
5. Probar en ZWCAD

### **¿Cómo abrir ventana WPF desde comando?**
```csharp
[CommandMethod("TANDEM_MI_VENTANA")]
public void AbrirMiVentana()
{
	var window = new UI.Views.MiVentana();
	Application.ShowModelessWindow(window);
}
```

### **¿Cómo acceder a servicios de negocio?**
```csharp
using Tandem2026.Desing.Services;

var projectService = new ProjectService();
var projects = projectService.GetAllProjects();
```

### **¿Dónde poner los iconos?**
`MNU/Iconos/png/16x16/` y `MNU/Iconos/png/32x32/`

### **¿Cómo debuggear en ZWCAD?**
- Configurar ZWCAD como external program en Debug settings
- F5 para iniciar
- Colocar breakpoints

---

## 📞 Contacto y Soporte

**Problemas comunes:**
- Plugin no carga → Verificar `.dll` en carpeta Support
- Comando no se reconoce → Verificar `[CommandMethod]`
- Menú no aparece → Recargar CUI con `CUICONFIG`

**Ayuda:**
- Revisar documentación técnica del plugin
- Consultar `TECHNICAL_GUIDE.md`
- Crear Issue en Azure DevOps

---

**Última actualización:** 24/04/2026  
**Mantenido por:** Equipo Tandem 2026
