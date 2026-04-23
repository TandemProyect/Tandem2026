# Solución a los Errores de Compilación

## Problema

Estás viendo errores como:
```
CS0246: El nombre del tipo o del espacio de nombres 'ZwCAD' no se encontró
```

## Causa

Las DLLs de ZWCAD (`ZwManaged.dll` y `ZwDatabaseMgd.dll`) requieren que el proyecto se compile con configuraciones específicas y que Visual Studio esté configurado correctamente.

## Solución 1: Compilar desde la Línea de Comandos de Visual Studio

1. Abre "Developer Command Prompt for VS 2022" (o tu versión de Visual Studio)
2. Navega a la carpeta del proyecto:
   ```cmd
   cd "C:\Users\jag\source\repos\ZwcadPlugin"
   ```
3. Compila usando MSBuild:
   ```cmd
   msbuild ZwcadPlugin.csproj /p:Configuration=Release /p:Platform="Any CPU"
   ```

## Solución 2: Verificar las Referencias en Visual Studio

1. Abre el proyecto en Visual Studio
2. En el Explorador de Soluciones, expande "Dependencias" > "Ensamblados"
3. Verifica que `ZwManaged` y `ZwDatabaseMgd` aparezcan listados
4. Si tienen un icono de advertencia amarillo:
   - Haz clic derecho en cada uno
   - Selecciona "Eliminar"
   - Haz clic derecho en "Referencias" > "Agregar Referencia"
   - Haz clic en "Examinar"
   - Navega a `C:\Program Files\ZWSOFT\ZWCAD 2026\`
   - Selecciona `ZwManaged.dll`
   - Repite para `ZwDatabaseMgd.dll`

## Solución 3: Cambiar a Formato de Proyecto Legacy

Si las soluciones anteriores no funcionan, puedes convertir el proyecto a formato legacy (.NET Framework clásico):

1. Crea un nuevo archivo `.csproj` con el siguiente contenido:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />

  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{YOUR-GUID-HERE}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>ZwcadPlugin</RootNamespace>
    <AssemblyName>ZwcadPlugin</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>

  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>

  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Windows.Forms" />
    <Reference Include="System.Net.Http" />
    <Reference Include="ZwManaged">
      <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwManaged.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="ZwDatabaseMgd">
      <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwDatabaseMgd.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed">
      <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll</HintPath>
    </Reference>
  </ItemGroup>

  <ItemGroup>
    <Compile Include="Models.cs" />
    <Compile Include="MVCApiService.cs" />
    <Compile Include="ZwcadHelper.cs" />
    <Compile Include="FormPrincipal.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="Commands.cs" />
  </ItemGroup>

  <ItemGroup>
    <None Include="packages.config" />
  </ItemGroup>

  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

2. Crea también un archivo `packages.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
</packages>
```

## Solución 4: Ignorar Errores y Probar Directamente en ZWCAD

**IMPORTANTE**: Aunque Visual Studio muestre errores de compilación, si los archivos `.dll` se generan correctamente en `bin\Debug\` o `bin\Release\`, el plugin funcionará cuando se cargue en ZWCAD.

Para probarlo:

1. Cierra los errores de Visual Studio
2. Verifica si existe `bin\Debug\ZwcadPlugin.dll`
3. Si existe, cárgalo en ZWCAD:
   - Abre ZWCAD 2026
   - Escribe `NETLOAD`
   - Selecciona `bin\Debug\ZwcadPlugin.dll`
   - Si se carga sin errores, ¡el plugin funciona!

## Solución 5: Usar el Proyecto como Referencia

Si nada funciona, es posible que necesites:

1. Copiar los archivos fuente a un proyecto nuevo creado directamente en Visual Studio
2. Usar la plantilla "Class Library (.NET Framework)" con .NET Framework 4.8
3. Agregar las referencias manualmente mediante el diálogo "Add Reference"

## Verificación de Archivos Creados

Los siguientes archivos deben existir en tu proyecto:

```
ZwcadPlugin/
├── ZwcadPlugin.csproj     ✓ Configuración del proyecto
├── Models.cs              ✓ Modelos de datos (DTOs)
├── MVCApiService.cs       ✓ Servicio HTTP (recién creado)
├── ZwcadHelper.cs         ✓ Funciones helper (recién creado)
├── FormPrincipal.cs       ✓ Windows Forms (recién creado)
├── Commands.cs            ✓ Comandos ZWCAD (recién creado)
└── README_INSTALACION.md  ✓ Documentación
```

## Próximos Pasos

Una vez que resuelvas los errores de compilación:

1. Compila el proyecto (Ctrl+Shift+B)
2. Verifica que se genere `bin\Debug\ZwcadPlugin.dll`
3. Carga el plugin en ZWCAD con `NETLOAD`
4. Prueba el comando `HOLA` para verificar que funcionó
5. Usa `MVCCONEXION` para acceder a la funcionalidad principal

## Nota Importante sobre Desarrollo con API de CAD

Las API de CAD (ZWCAD, AutoCAD, etc.) tienen una particularidad: las DLLs de la API solo se pueden utilizar completamente cuando se ejecutan dentro del entorno de CAD. Por eso Visual Studio puede mostrar errores incluso cuando el código es correcto y funcionará perfectamente en ZWCAD.

Esto es normal y esperado. El flujo de trabajo típico es:

1. Escribir código en Visual Studio (puede mostrar errores de IntelliSense)
2. Compilar (puede fallar o tener advertencias)
3. Si se genera el DLL, cargarlo en ZWCAD
4. Probar en ZWCAD (donde realmente se ejecuta)
5. Ver errores de tiempo de ejecución en la línea de comandos de ZWCAD
6. Volver a paso 1 para corregir

## Soporte Adicional

Si los problemas persisten:
- Verifica que ZWCAD 2026 esté instalado correctamente
- Asegúrate de estar usando Visual Studio 2019 o superior
- Confirma que .NET Framework 4.8 esté instalado
- Verifica que tu usuario tenga permisos en `C:\Program Files\ZWSOFT\`
