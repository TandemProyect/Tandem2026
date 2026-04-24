# Documentación del Plugin Tandem 2026 para ZWCAD

**Última actualización:** 24 de Abril de 2026  
**Commit actual:** #616 (Limpieza de archivos temporales)  
**Estado:** ✅ Compila correctamente | ⚠️ Menú CUI pendiente de cargar en ZWCAD

---

## 📋 Índice

1. [Resumen del Proyecto](#resumen-del-proyecto)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Comandos ZWCAD Disponibles](#comandos-zwcad-disponibles)
4. [Menú CUI](#menú-cui)
5. [Interfaz WPF](#interfaz-wpf)
6. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)
7. [Próximos Pasos](#próximos-pasos)
8. [Historial de Cambios Recientes](#historial-de-cambios-recientes)

---

## 🎯 Resumen del Proyecto

**Tandem 2026** es un plugin para ZWCAD que permite:
- Detectar muros en planos 2D
- Generar modelos 3D de estructuras
- Configurar sistemas de encofrado
- Integración con servidor MVC para leer/guardar diseños

**Tecnologías:**
- .NET Framework 4.8
- WPF (Windows Presentation Foundation)
- ZWCAD API (ZwManaged.dll, ZwDatabaseMgd.dll)
- Patrón MVVM para la UI

---

## 📁 Estructura del Proyecto

```
C:\00_Tandem2026\
├── Desing\                          # Proyecto principal web/MVC
├── DAL\                             # Capa de acceso a datos
└── TamdenZwcadPluging\
	└── ZwcadPlugin\                 # 🔵 PLUGIN ZWCAD (foco principal)
		├── bin\Debug\
		│   ├── ZwcadPlugin.dll      # ✅ DLL del plugin compilada
		│   └── MNU\
		│       └── Tandem2026.cui   # ✅ Menú CUI copiado al output
		├── MNU\
		│   └── Tandem2026.cui       # ✅ Menú CUI fuente
		├── UI\
		│   ├── ViewModels\
		│   │   ├── MainViewModel.cs        # ViewModel principal
		│   │   ├── RelayCommand.cs         # Implementación de ICommand
		│   │   └── ViewModelBase.cs        # Base para MVVM
		│   └── Views\
		│       ├── MainWindow.xaml         # Ventana WPF principal
		│       └── MainWindow.xaml.cs      # Code-behind
		├── MenuManager.cs           # ✅ Punto de entrada del plugin
		├── CuixBuilder.cs           # Utilidad para construir CUI
		├── ZwcadPlugin.csproj       # ✅ Proyecto limpio
		└── packages.config          # NuGet packages
```

### Archivos Clave

| Archivo | Propósito | Estado |
|---------|-----------|--------|
| `MenuManager.cs` | Punto de entrada, registra comandos ZWCAD | ✅ Funcionando |
| `MNU\Tandem2026.cui` | Definición del menú ribbon | ✅ Completo |
| `UI\Views\MainWindow.xaml` | Panel principal WPF | ✅ Diseñado |
| `UI\ViewModels\MainViewModel.cs` | Lógica del panel | ✅ Implementado |
| `ZwcadPlugin.csproj` | Configuración del proyecto | ✅ Limpio |

---

## 🔧 Comandos ZWCAD Disponibles

Todos los comandos están definidos en `MenuManager.cs` y registrados con el atributo `[CommandMethod]`:

| Comando | Descripción | Estado |
|---------|-------------|--------|
| `TANDEM` | Muestra lista de comandos disponibles | ✅ Implementado |
| `MVCCONEXION` | Abre el panel principal WPF | ⚠️ Pendiente implementar |
| `DETECTARMUROS` | Lee geometría 2D y construye modelo topológico | ⚠️ Pendiente implementar |
| `GENERAR3D` | Genera sólidos 3D del modelo | ⚠️ Pendiente implementar |
| `REGENERAR3D` | Borra y regenera sólidos 3D | ⚠️ Pendiente implementar |
| `CONFIGENCOFRADO` | Configura sistema de encofrado | ⚠️ Pendiente implementar |
| `LEERDISENOMVC` | Lee diseño desde servidor MVC | ⚠️ Pendiente implementar |
| `GUARDARDISENOMVC` | Guarda diseño en servidor MVC | ⚠️ Pendiente implementar |

### Código Actual de MenuManager.cs

```csharp
[ZwSoft.ZwCAD.Runtime.CommandMethod("TANDEM")]
public void MostrarComandos()
{
	Document doc = ZwcadApp.DocumentManager.MdiActiveDocument;
	if (doc == null) return;
	Editor ed = doc.Editor;
	ed.WriteMessage("\n--- Tandem 2026 ---");
	ed.WriteMessage("\n  MVCCONEXION      Panel principal");
	ed.WriteMessage("\n  DETECTARMUROS    Detecta muros en la planta 2D");
	ed.WriteMessage("\n  GENERAR3D        Genera solidos 3D");
	ed.WriteMessage("\n  REGENERAR3D      Borra y regenera solidos 3D");
	ed.WriteMessage("\n  CONFIGENCOFRADO  Configura el sistema de encofrado");
	ed.WriteMessage("\n  LEERDISENOMVC    Lee un diseno desde el servidor");
	ed.WriteMessage("\n  GUARDARDISENOMVC Guarda el diseno en el servidor\n");
}
```

**⚠️ NOTA:** Solo `TANDEM` está implementado. Los demás comandos están documentados pero **no tienen métodos asociados** todavía.

---

## 🎨 Menú CUI

**Ubicación:** `TamdenZwcadPluging\ZwcadPlugin\MNU\Tandem2026.cui`

### Estructura del Menú

```
Pestaña: "Tandem 2026"
│
├─ Panel 1: "Principal"
│  └─ Botón: Panel → MVCCONEXION
│
├─ Panel 2: "Modelo 3D"
│  ├─ Botón: Detectar → DETECTARMUROS
│  ├─ Botón: Generar 3D → GENERAR3D
│  ├─ Botón: Regenerar → REGENERAR3D
│  └─ Botón: Encofrado → CONFIGENCOFRADO
│
└─ Panel 3: "Datos MVC"
   ├─ Botón: Leer → LEERDISENOMVC
   └─ Botón: Guardar → GUARDARDISENOMVC
```

### Cómo Cargar el Menú en ZWCAD

#### ⚠️ PROBLEMA CONOCIDO: Permisos de Windows

ZWCAD necesita permisos de escritura en `C:\Program Files\ZWSOFT\ZWCAD 2026\`.

**Solución:**
1. Cierra ZWCAD
2. **Clic derecho** en el icono de ZWCAD → **"Ejecutar como administrador"**
3. En ZWCAD, ejecuta: `MENULOAD`
4. Navega a: `C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\bin\Debug\MNU\Tandem2026.cui`
5. Haz clic en **"Cargar"**
6. La pestaña "Tandem 2026" debería aparecer en el ribbon

#### Alternativa: Crear el Menú Manualmente

Si `MENULOAD` no funciona, puedes crear el menú usando el editor CUI de ZWCAD:

1. En ZWCAD: comando `CUI`
2. Crear nueva pestaña: "Tandem 2026"
3. Crear 3 paneles: "Principal", "Modelo 3D", "Datos MVC"
4. Crear 8 comandos (ver tabla de comandos arriba)
5. Asignar comandos a paneles
6. Asignar paneles a la pestaña
7. **Aplicar** y **Aceptar**

---

## 🖼️ Interfaz WPF

**Archivo principal:** `UI\Views\MainWindow.xaml`

### Diseño Actual

```
┌────────────────────────────────────────┐
│  Tandem 2026                           │
│  Plugin de Muros y Encofrado para ZWCAD│
├────────────────────────────────────────┤
│  [ 🔍  Detectar Muros ]                │
│  [ 🧱  Generar 3D     ]                │
│  [ ⚙️  Configurar Encofrado ]          │
├────────────────────────────────────────┤
│  Plugin Tandem 2026 listo.             │
└────────────────────────────────────────┘
```

### ViewModel (MainViewModel.cs)

**Propiedades:**
- `EstadoConexion`: Estado de conexión con el servidor MVC
- `MensajeEstado`: Mensaje de estado mostrado al usuario

**Comandos:**
- `DetectarMurosCommand` → Ejecuta `DETECTARMUROS`
- `Generar3dCommand` → Ejecuta `GENERAR3D`
- `ConfigEncofradoCommand` → Ejecuta `CONFIGENCOFRADO`

**⚠️ ESTADO:** Los comandos están cableados pero **no invocan los comandos ZWCAD** todavía. Solo actualizan `MensajeEstado`.

### Cómo Abrir el Panel desde ZWCAD

**Pendiente implementar** el comando `MVCCONEXION` que debe:
1. Crear instancia de `MainWindow`
2. Mostrar la ventana modal o no modal
3. Pasar contexto de ZWCAD al ViewModel si es necesario

---

## ⚠️ Problemas Conocidos y Soluciones

### 1. ZWCAD no Carga el Menú CUI

**Síntoma:**
```
Write permission denied. Unable to create:
c:\program files\zwsoft\zwcad 2026\userdatacache\en-us\support\zwcad.cuix
```

**Causa:** ZWCAD no tiene permisos de escritura en `C:\Program Files\`.

**Solución:**
- **Opción A:** Ejecutar ZWCAD como administrador (temporal)
- **Opción B:** Dar permisos permanentes a la carpeta de ZWCAD

**PowerShell (como administrador):**
```powershell
$path = "C:\Program Files\ZWSOFT\ZWCAD 2026"
$acl = Get-Acl $path
$userName = "NUCBOXG5\jag"  # Cambiar por tu usuario
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($userName,"FullControl","ContainerInherit,ObjectInherit","None","Allow")
$acl.SetAccessRule($rule)
Set-Acl $path $acl
```

### 2. Vulnerabilidad en Paquetes NuGet

**Resuelto en commit #615**

Se actualizó `System.Linq.Dynamic.Core` de `1.5.1` a `1.7.2` para remediar vulnerabilidad NU1903 (alta severidad).

**Archivos modificados:**
- `DAL\packages.config`
- `Desing\packages.config`
- `DAL\DAL.csproj`
- `Desing\Design.csproj`

### 3. Errores de Compilación Anteriores

**Resueltos:**

✅ **Error:** Faltaba referencia a `Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1`  
   **Solución:** Eliminada referencia obsoleta de `Desing\Design.csproj`

✅ **Error:** Duplicación de import de EntityFramework en `DAL\DAL.csproj`  
   **Solución:** Eliminada referencia a `EntityFramework 6.4.4`, mantenida `6.5.1`

✅ **Error:** Faltaba archivo `MNU\Tandem2026.mnu`  
   **Solución:** Eliminado paso de copia del archivo `.mnu` (solo se usa `.cui`)

---

## 🚀 Próximos Pasos

### Prioridad Alta

1. **Cargar el menú CUI en ZWCAD**
   - Ejecutar ZWCAD como administrador
   - Usar `MENULOAD` para cargar `Tandem2026.cui`
   - Verificar que aparezca la pestaña "Tandem 2026"

2. **Implementar comando `MVCCONEXION`**
   ```csharp
   [ZwSoft.ZwCAD.Runtime.CommandMethod("MVCCONEXION")]
   public void AbrirPanelPrincipal()
   {
	   var ventana = new UI.Views.MainWindow();
	   ZwSoft.ZwCAD.ApplicationServices.Application.ShowModalWindow(ventana);
   }
   ```

3. **Conectar comandos WPF con comandos ZWCAD**
   - Hacer que los botones del panel WPF invoquen los comandos ZWCAD usando `SendStringToExecute`

### Prioridad Media

4. **Implementar lógica de detección de muros**
   - Comando `DETECTARMUROS` debe leer entidades 2D (líneas, polilíneas)
   - Construir modelo topológico

5. **Implementar generación 3D**
   - Comando `GENERAR3D` debe crear sólidos 3D a partir del modelo

### Prioridad Baja

6. **Integración con servidor MVC**
   - Implementar `LEERDISENOMVC` y `GUARDARDISENOMVC`
   - Configurar endpoint del servidor

7. **Agregar iconos a los botones del menú CUI**
   - Crear archivos `.bmp` o `.png` de 16x16 y 32x32
   - Referenciar en el archivo `.cui`

---

## 📜 Historial de Cambios Recientes

### Commit #616 - Limpieza (24/04/2026 10:30)
- ✅ Eliminados archivos temporales de ayuda y debugging
- ✅ Borrado `DIAGNOSTICO_COMPILACION.md` (247 líneas)
- ✅ Borrado `MNU\At.cui` (menú de prueba)
- ✅ Borrados archivos `.scr`, `.txt` de ayuda temporal
- ✅ Revertido `ZwcadPlugin.csproj` a versión limpia

### Commit #615 - Actualización NuGet (24/04/2026 09:28)
- ✅ Actualizado `System.Linq.Dynamic.Core` de 1.5.1 → 1.7.2
- ✅ Remediada vulnerabilidad de seguridad NU1903
- ✅ Actualizado `DAL\packages.config`
- ✅ Actualizado `Desing\packages.config`
- ✅ Actualizado `DAL\DAL.csproj`
- ✅ Actualizado `Desing\Design.csproj`
- ✅ Descargado y restaurado paquete en carpeta local `packages\`

### Commit #614 - Corrección de Errores de Build (23/04/2026)
- ✅ Eliminada duplicación de import EntityFramework en `DAL\DAL.csproj`
- ✅ Eliminado paso de copia de `MNU\Tandem2026.mnu` (no existe, solo `.cui`)
- ✅ Corregido error de referencia a `DAL.dll`

### Commit #613 - Cambios en Menú/UI (fecha anterior)
- ✅ Creación del menú `Tandem2026.cui`
- ✅ Implementación de interfaz WPF

---

## 🔄 Cómo Continuar en una Nueva Sesión

Si estás trabajando con este proyecto en una nueva sesión de chat:

1. **Lee esta documentación completa** para entender el estado actual
2. **Verifica el último commit:**
   ```bash
   cd C:\00_Tandem2026
   git log --oneline -5
   ```
3. **Compila el proyecto** para asegurarte de que todo funciona:
   ```bash
   msbuild Design.sln /t:Rebuild /p:Configuration=Debug
   ```
4. **Verifica el estado de Git:**
   ```bash
   git status
   ```
5. **Continúa desde "Próximos Pasos"** según la prioridad

---

## 📞 Información de Contexto

- **Usuario:** jag (NUCBOXG5\jag)
- **Ruta del proyecto:** `C:\00_Tandem2026\`
- **IDE:** Microsoft Visual Studio Community 2026 (18.6.0-insiders)
- **Shell:** PowerShell
- **ZWCAD:** Versión 2026 instalada en `C:\Program Files\ZWSOFT\ZWCAD 2026`
- **Git Remote:** https://github.com/JuanGodoyLopez/Tandem-2026
- **Branch:** master

---

## 📝 Notas Adicionales

### Patrón MVVM Usado
- `ViewModelBase`: Implementa `INotifyPropertyChanged`
- `RelayCommand`: Implementación simple de `ICommand`
- `MainViewModel`: Lógica de negocio para `MainWindow`

### Compilación
El proyecto compila correctamente con MSBuild:
```bash
msbuild Design.sln /t:Rebuild /p:Configuration=Debug /v:minimal
```

### Estructura de Commits
Los commits siguen el formato:
```
Título breve descriptivo

- Detalle 1
- Detalle 2
```

---

**Fin de la documentación**

*Última actualización: 24 de Abril de 2026, 10:45*
