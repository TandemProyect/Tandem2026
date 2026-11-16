# Sistema de Menús para Plugin ZWCAD 2026 - MVC

## 📋 Resumen

Se ha creado un sistema completo de menús para el plugin ZWCAD que incluye:

1. **Menú en la barra de menús de ZWCAD** - Se crea automáticamente al cargar el plugin
2. **Menú contextual interactivo** - Comando `MENUMVC` para acceso rápido
3. **Inicialización automática** - El menú se crea al cargar la DLL

## 🆕 Archivos Creados

### MenuManager.cs
- **Clase**: `MenuManager` que implementa `IExtensionApplication`
- **Funcionalidad**: 
  - Crea un menú "MVC Plugin" en la barra de menús de ZWCAD
  - Se ejecuta automáticamente al cargar el plugin
  - Gestiona la creación y eliminación del menú
  - Incluye comando `MENUMVC` para menú contextual interactivo

## 📝 Instrucciones de Instalación

### Paso 1: Agregar el archivo al proyecto

Como Visual Studio no permite editar el .csproj mientras está abierto:

1. **Cierra la solución** (Archivo → Cerrar solución)
2. **Abre el archivo** `ZwcadPlugin.csproj` con un editor de texto
3. Busca la sección `<ItemGroup>` que contiene los archivos `.cs` (alrededor de la línea 80)
4. Agrega esta línea después de `<Compile Include="Commands.cs" />`:

```xml
<Compile Include="MenuManager.cs" />
```

Debería verse así:

```xml
<ItemGroup>
  <Compile Include="Models.cs" />
  <Compile Include="MVCApiService.cs" />
  <Compile Include="ZwcadHelper.cs" />
  <Compile Include="FormPrincipal.cs">
    <SubType>Form</SubType>
  </Compile>
  <Compile Include="Commands.cs" />
  <Compile Include="MenuManager.cs" />
</ItemGroup>
```

5. **Guarda** el archivo
6. **Vuelve a abrir** la solución en Visual Studio

### Paso 2: Compilar el proyecto

1. **Compila** el proyecto (Ctrl+Shift+B)
2. Verifica que no haya errores

### Paso 3: Probar en ZWCAD

1. **Abre ZWCAD 2026**
2. **Carga el plugin** con el comando `NETLOAD`
3. Selecciona tu DLL compilada: `ZwcadPlugin.dll`

## ✨ Características del Menú

### Menú en la Barra de ZWCAD

Al cargar el plugin, verás un nuevo menú llamado **"MVC Plugin"** en la barra de menús con las siguientes opciones:

```
MVC Plugin
├── Formulario Principal      [MVCCONEXION]
├── ──────────────────────
├── Insertar Bloque           [INSERTARBLOQUE]
├── ──────────────────────
├── Leer Diseño               [LEERDISENOMVC]
├── Guardar Diseño            [GUARDARDISENOMVC]
├── ──────────────────────
└── Ayuda                     [HOLA]
```

### Comando Interactivo: MENUMVC

Escribe `MENUMVC` en la línea de comandos para mostrar un menú interactivo:

```
╔════════════════════════════════════════════════════════════╗
║              MENÚ MVC PLUGIN - ZWCAD 2026                 ║
╚════════════════════════════════════════════════════════════╝

Opciones del menú:

  1. Formulario Principal   → MVCCONEXION
     Abre el formulario de gestión de bloques y diseños

  2. Insertar Bloque        → INSERTARBLOQUE
     Inserta un bloque desde el servidor

  3. Leer Diseño            → LEERDISENOMVC
     Lee un diseño desde el servidor MVC

  4. Guardar Diseño         → GUARDARDISENOMVC
     Guarda el diseño actual en el servidor

  5. Ayuda                  → HOLA
     Muestra la ayuda de comandos disponibles

──────────────────────────────────────────────────────────────
Selecciona una opción (1-5) o ESC para cancelar:
```

## 🎯 Comandos Disponibles

| Comando | Descripción | Acceso |
|---------|-------------|--------|
| `MVCCONEXION` | Abre el formulario principal de gestión | Menú, MENUMVC, Comando directo |
| `INSERTARBLOQUE` | Inserta un bloque desde el servidor | Menú, MENUMVC, Comando directo |
| `LEERDISENOMVC` | Lee un diseño guardado del servidor | Menú, MENUMVC, Comando directo |
| `GUARDARDISENOMVC` | Guarda el diseño actual en el servidor | Menú, MENUMVC, Comando directo |
| `HOLA` | Muestra ayuda y comandos disponibles | Menú, MENUMVC, Comando directo |
| `MENUMVC` | Muestra el menú contextual interactivo | Comando directo |

## 🔧 Funcionalidad Técnica

### Inicialización Automática

La clase `MenuManager` implementa `IExtensionApplication`, lo que permite:

- **`Initialize()`**: Se ejecuta al cargar la DLL
  - Muestra mensaje de bienvenida
  - Crea el menú en la barra de ZWCAD
  - Usa reflexión COM para interactuar con ZWCAD

- **`Terminate()`**: Se ejecuta al descargar la DLL
  - Elimina el menú creado
  - Limpia recursos

### Acceso por COM Interop

El menú se crea usando el modelo COM de ZWCAD:
- Accede a `ZwcadApp.AcadApplication`
- Usa reflexión tardía (late binding) para compatibilidad
- Crea menús, items, separadores y macros

### Manejo de Errores

- Si falla la creación del menú, el plugin se carga igualmente
- Los comandos siguen funcionando independientemente del menú
- Mensajes informativos en caso de problemas

## 📌 Notas Importantes

1. **Único ExtensionApplication**: Solo puede haber un atributo `[assembly: ExtensionApplication]` por DLL, ahora está en `MenuManager.cs`

2. **Eliminación del comando MVCPLUGIN_INIT**: Se eliminó de `Commands.cs` porque la inicialización ahora está en `MenuManager.Initialize()`

3. **Compatibilidad**: El menú usa reflexión COM para máxima compatibilidad con diferentes versiones de ZWCAD

4. **Persistencia**: El menú se mantiene mientras ZWCAD esté abierto y el plugin cargado

## 🎨 Personalización

Si deseas modificar el menú:

1. **Agregar comandos**: Edita `MenuManager.CrearMenu()` y agrega más llamadas a `AgregarItemMenu()`
2. **Cambiar orden**: Reordena las llamadas a `AgregarItemMenu()`
3. **Modificar opciones de MENUMVC**: Edita `MenuManager.MostrarMenuContextual()`

## 🐛 Solución de Problemas

### El menú no aparece
- Verifica que ZWCAD tenga permisos COM
- Comprueba que no haya errores al cargar la DLL
- Revisa la línea de comandos para mensajes de error

### El comando MENUMVC no funciona
- Asegúrate de que el plugin esté cargado (`NETLOAD`)
- Verifica que `MenuManager.cs` esté compilado en el proyecto

### Errores de compilación
- Verifica que todas las referencias de ZWCAD estén correctas
- Asegúrate de que `RuntimeIdentifiers` esté configurado como `win-x64`

## ✅ Verificación Final

Después de cargar el plugin en ZWCAD, deberías ver:

1. ✅ Mensaje de bienvenida en la línea de comandos
2. ✅ Nuevo menú "MVC Plugin" en la barra de menús
3. ✅ Todos los comandos funcionando desde el menú
4. ✅ Comando `MENUMVC` mostrando menú interactivo
5. ✅ Comando `HOLA` mostrando ayuda completa

---

**Desarrollado para ZWCAD 2026 - Plugin MVC**
**Versión: 1.0**
