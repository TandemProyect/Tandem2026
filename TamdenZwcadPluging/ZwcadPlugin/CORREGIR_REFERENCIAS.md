# SOLUCIÓN FINAL - Actualizar Referencias

## El problema

Las referencias en el archivo `.csproj` están apuntando a una carpeta `lib\` que no existe.

## SOLUCIÓN INMEDIATA

1. **Cierra Visual Studio completamente**

2. **Abre el archivo `ZwcadPlugin.csproj` en Notepad o cualquier editor de texto**

3. **Busca estas líneas** (alrededor de la línea 53-60):

```xml
<Reference Include="ZwManaged">
  <HintPath>lib\ZwManaged.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="ZwDatabaseMgd">
  <HintPath>lib\ZwDatabaseMgd.dll</HintPath>
  <Private>False</Private>
</Reference>
```

4. **Reemplázalas con**:

```xml
<Reference Include="ZwManaged">
  <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwManaged.dll</HintPath>
  <Private>False</Private>
  <SpecificVersion>False</SpecificVersion>
</Reference>
<Reference Include="ZwDatabaseMgd">
  <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwDatabaseMgd.dll</HintPath>
  <Private>False</Private>
  <SpecificVersion>False</SpecificVersion>
</Reference>
```

5. **Guarda el archivo**

6. **Vuelve a abrir Visual Studio**

7. **Compila el proyecto** (Ctrl+Shift+B o F6)

## Verificación

Después de hacer esto:

1. En el Explorador de Soluciones, expande "Dependencies" o "Referencias"
2. Deberías ver:
   - `ZwManaged` ✓
   - `ZwDatabaseMgd` ✓
   - Sin iconos de advertencia amarillos

3. Intenta compilar:
   ```
   Build started...
   1>------ Build started: Project: ZwcadPlugin, Configuration: Debug Any CPU ------
   1>  ZwcadPlugin -> C:\Users\jag\source\repos\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
   ========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
   ```

## Si sigue sin funcionar

Si después de esto sigues viendo errores relacionados con `ZwCAD` namespace:

### Opción A: Eliminar y volver a agregar referencias desde Visual Studio

1. Abre el proyecto en Visual Studio
2. En el Explorador de Soluciones, haz clic derecho en el proyecto > "Add" > "Reference"
3. Haz clic en "Browse..."
4. Navega a `C:\Program Files\ZWSOFT\ZWCAD 2026\`
5. Selecciona **ambas DLLs**:
   - `ZwManaged.dll`
   - `ZwDatabaseMgd.dll`
6. Haz clic en "Add"
7. Haz clic en "OK"
8. Si ya existen referencias anteriores con advertencias, elimínalas primero

### Opción B: Verificar instalación de ZWCAD 2026

Ejecuta en PowerShell:

```powershell
Test-Path "C:\Program Files\ZWSOFT\ZWCAD 2026\ZwManaged.dll"
Test-Path "C:\Program Files\ZWSOFT\ZWCAD 2026\ZwDatabaseMgd.dll"
```

Ambos deben retornar `True`. Si retornan `False`, ZWCAD 2026 no está instalado correctamente.

### Opción C: Verificar que Newtonsoft.Json esté restaurado

En la Consola del Administrador de Paquetes de Visual Studio:

```powershell
Update-Package -reinstall Newtonsoft.Json
```

O simplemente:

```powershell
dotnet restore
```

## Archivos actualizados

Los siguientes archivos ya han sido actualizados con los namespaces correctos de ZWCAD:

✓ **Commands.cs** - Usa `using ZwCAD.ApplicationServices;`, `using ZwCAD.DatabaseServices;`, `using ZwCAD.EditorInput;`, `using ZwCAD.Runtime;`
✓ **FormPrincipal.cs** - Usa referencias cortas a `Application` en lugar de `ZwCAD.ApplicationServices.Application`
✓ **ZwcadHelper.cs** - Usa `using ZwCAD.DatabaseServices;` y `using ZwCAD.Geometry;`

## ¿Por qué pasó esto?

Parece que alguien movió las DLLs a una carpeta `lib\` local, pero esas DLLs no fueron incluidas en el repositorio. La solución es apuntar directamente a la instalación de ZWCAD 2026.

## Compilación exitosa

Cuando todo esté correcto, deberías ver:

```
Rebuild started...
1>------ Rebuild All started: Project: ZwcadPlugin, Configuration: Debug Any CPU ------
1>  Models.cs
1>  MVCApiService.cs
1>  ZwcadHelper.cs
1>  FormPrincipal.cs
1>  Commands.cs
1>  ZwcadPlugin -> C:\Users\jag\source\repos\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```

## Prueba en ZWCAD

Una vez compilado correctamente:

1. Abre ZWCAD 2026
2. Escribe `NETLOAD` y presiona Enter
3. Navega a `C:\Users\jag\source\repos\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll`
4. Selecciona el archivo y haz clic en "Open"
5. Si no hay errores, escribe `HOLA` para verificar que el plugin está cargado
6. Deberías ver el menú de ayuda del plugin

¡Listo! Tu plugin ya está funcionando.
