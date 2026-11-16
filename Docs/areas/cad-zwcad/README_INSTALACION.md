# Plugin ZWCAD 2026 - Conexión MVC

Plugin completo para ZWCAD 2026 que se conecta a tu proyecto MVC en: http://ccvallecano-002-site1.rtempurl.com/

## Estructura del Proyecto

ZwcadPlugin/
├── ZwcadPlugin.csproj     - Configuración del proyecto
├── Models.cs              - Modelos de datos (DTOs)
├── MVCApiService.cs       - Servicio HTTP para conectar con MVC
├── ZwcadHelper.cs         - Funciones helper para conversiones
├── FormPrincipal.cs       - Formulario Windows Forms
└── Commands.cs            - Comandos de ZWCAD

## Configuración Inicial

### 1. Referencias de ZWCAD

Asegúrate de que las rutas en `ZwcadPlugin.csproj` sean correctas para tu instalación:

```xml
<Reference Include="ZwManaged">
  <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwManaged.dll</HintPath>
</Reference>
<Reference Include="ZwDatabaseMgd">
  <HintPath>C:\Program Files\ZWSOFT\ZWCAD 2026\ZwDatabaseMgd.dll</HintPath>
</Reference>
```

### 2. URL del Servidor MVC

La URL está configurada en `MVCApiService.cs`:
```csharp
_baseUrl = "http://ccvallecano-002-site1.rtempurl.com/";
```

## Endpoints del Servidor MVC (API)

Tu proyecto MVC debe tener estos endpoints:

### Diseños:
- `GET /api/disenos` - Obtener lista de diseños
- `GET /api/disenos/{id}` - Obtener diseño específico
- `POST /api/disenos` - Crear nuevo diseño
- `PUT /api/disenos/{id}` - Actualizar diseño existente

### Bloques:
- `GET /api/bloques` - Obtener lista de bloques disponibles
- `GET /api/bloques/descargar/{nombre}` - Descargar archivo de bloque

## Modelos de Datos (DTOs)

Los datos se intercambian en formato JSON:

### DisenoDTO
```json
{
  "id": 1,
  "nombre": "Plano Casa",
  "descripcion": "Plano arquitectónico",
  "fechaCreacion": "2026-03-12T10:00:00",
  "fechaModificacion": "2026-03-12T11:00:00",
  "usuario": "jag",
  "entidades": [...],
  "bloques": [...],
  "layers": [...]
}
```

### BloqueDTO
```json
{
  "nombre": "Puerta",
  "puntoInsertX": 10.0,
  "puntoInsertY": 20.0,
  "puntoInsertZ": 0.0,
  "escala": 1.0,
  "rotacion": 90.0,
  "rutaArchivo": "/bloques/puerta.dwg"
}
```

## Comandos Disponibles en ZWCAD

### MVCCONEXION
Abre el formulario principal con tabs para:
- **Bloques**: Cargar e insertar bloques desde el servidor
- **Diseños**: Leer y guardar diseños completos

```
Comando: MVCCONEXION
```

### INSERTARBLOQUE
Insertar un bloque desde el servidor en el dibujo actual.

```
Comando: INSERTARBLOQUE
[Selecciona bloque del formulario]
Especifica punto de inserción: [clic en el dibujo]
```

### LEERDISENOMVC
Leer un diseño completo desde el servidor.

```
Comando: LEERDISENOMVC
Ingresa el ID del diseño: 1
```

### GUARDARDISENOMVC
Guardar el diseño actual en el servidor.

```
Comando: GUARDARDISENOMVC
Ingresa el nombre del diseño: Mi Plano
Ingresa una descripción (opcional): Plano de ejemplo
```

### HOLA
Muestra ayuda rápida de comandos disponibles.

```
Comando: HOLA
```

## Compilación

1. Abre el proyecto en Visual Studio 2019 o superior
2. Restaura paquetes NuGet (Newtonsoft.Json)
3. Compila el proyecto (Ctrl+Shift+B)
4. El DLL se generará en `bin\Debug\` o `bin\Release\`

## Instalación en ZWCAD

### Carga Manual
1. Abre ZWCAD 2026
2. Escribe `NETLOAD` y presiona Enter
3. Navega a `bin\Debug\ZwcadPlugin.dll` y ábrelo
4. El plugin se cargará y estará listo para usar

### Carga Automática
1. En ZWCAD, escribe `APPLOAD`
2. En el diálogo, haz clic en "Contents"
3. Agrega `ZwcadPlugin.dll` a la lista de Startup Suite
4. El plugin se cargará automáticamente al iniciar ZWCAD

## Uso Típico

### Insertar Bloques desde Servidor

1. Ejecuta `MVCCONEXION` o `INSERTARBLOQUE`
2. En la pestaña "Bloques", haz clic en "Cargar Bloques"
3. Selecciona el bloque deseado de la lista
4. Ajusta escala y rotación si es necesario
5. Haz clic en "Insertar Bloque"
6. Especifica el punto de inserción en el dibujo

### Guardar Diseño en Servidor

1. Crea tu diseño en ZWCAD (líneas, círculos, bloques, etc.)
2. Ejecuta `GUARDARDISENOMVC`
3. Ingresa nombre y descripción
4. El plugin extraerá todas las entidades y las enviará al servidor
5. Recibirás un ID del diseño guardado

### Leer Diseño desde Servidor

1. Ejecuta `LEERDISENOMVC`
2. Ingresa el ID del diseño que quieres cargar
3. El plugin descargará los datos del servidor
4. Se mostrará información del diseño en la línea de comandos

## Estructura del Formulario Windows Forms

El formulario tiene 2 pestañas:

### Pestaña "Bloques"
- ListBox con bloques disponibles
- TextBox para escala (default: 1.0)
- TextBox para rotación en grados (default: 0)
- Botón "Cargar Bloques" - Obtiene lista del servidor
- Botón "Insertar Bloque" - Inserta el bloque seleccionado

### Pestaña "Diseños"
- ListBox con diseños guardados
- TextBox para nombre del diseño
- TextBox multi-línea para descripción
- Botón "Cargar Lista" - Obtiene lista de diseños
- Botón "Leer del Servidor" - Carga el diseño seleccionado
- Botón "Guardar en Servidor" - Guarda el diseño actual

## Datos que se Extraen del Dibujo

Cuando guardas un diseño, el plugin extrae:

### Layers
- Nombre
- Color
- Visible/Oculto
- Bloqueado/Desbloqueado

### Entidades
- Líneas (puntos inicio/fin)
- Círculos (centro, radio)
- Arcos (centro, radio, ángulos)
- Polilíneas (vértices, cerrada/abierta)
- Referencias a bloques

### Bloques
- Nombre del bloque
- Punto de inserción (X, Y, Z)
- Escala
- Rotación
- Atributos (si existen)

## Personalización

### Cambiar URL del Servidor

Edita `MVCApiService.cs`, línea 15:
```csharp
_baseUrl = "http://tu-servidor.com/";
```

### Agregar Nuevos Comandos

En `Commands.cs`, agrega métodos con el atributo `[CommandMethod]`:

```csharp
[CommandMethod("MINUEVOCOMANDO")]
public void MiNuevoComando()
{
    Document doc = Application.DocumentManager.MdiActiveDocument;
    Editor ed = doc.Editor;
    
    ed.WriteMessage("\n¡Mi nuevo comando!");
    // Tu código aquí
}
```

### Modificar Formulario

Edita `FormPrincipal.cs` para agregar nuevos controles o funcionalidad.

## Solución de Problemas

### Error: "No se puede cargar ZwManaged.dll"
- Verifica la ruta en ZwcadPlugin.csproj
- Asegúrate de que ZWCAD 2026 esté instalado
- Compila para x64 (no x86)

### Error: "No se puede conectar al servidor"
- Verifica que la URL sea correcta
- Comprueba que el servidor esté en línea
- Revisa el firewall y permisos de red

### Error al compilar: "Framework not found"
- El proyecto requiere .NET Framework 4.8
- Instálalo desde: https://dotnet.microsoft.com/download/dotnet-framework/net48

## Código de Ejemplo del Servidor MVC

Tu proyecto MVC debe tener un controlador similar a este:

```csharp
[RoutePrefix("api/disenos")]
public class DisenosController : ApiController
{
    [HttpGet]
    [Route("")]
    public IHttpActionResult GetDisenos()
    {
        var disenos = // obtener de base de datos
        return Ok(disenos);
    }

    [HttpGet]
    [Route("{id}")]
    public IHttpActionResult GetDiseno(int id)
    {
        var diseno = // obtener de base de datos
        return Ok(diseno);
    }

    [HttpPost]
    [Route("")]
    public IHttpActionResult PostDiseno(DisenoDTO diseno)
    {
        // guardar en base de datos
        return Ok(diseno);
    }

    [HttpPut]
    [Route("{id}")]
    public IHttpActionResult PutDiseno(int id, DisenoDTO diseno)
    {
        // actualizar en base de datos
        return Ok(diseno);
    }
}
```

## Próximos Pasos

1. Completa la implementación de los endpoints en tu proyecto MVC
2. Prueba cada comando en ZWCAD
3. Agrega validaciones y manejo de errores adicional
4. Implementa autenticación si es necesario
5. Agrega funcionalidad para descargar/subir archivos DWG completos

## Soporte y Documentación

- Documentación ZWCAD .NET API: https://www.zwsoft.com/zwcad/developers
- Documentación ASP.NET MVC: https://docs.microsoft.com/aspnet/mvc
- Newtonsoft.Json: https://www.newtonsoft.com/json/help/html/Introduction.htm
