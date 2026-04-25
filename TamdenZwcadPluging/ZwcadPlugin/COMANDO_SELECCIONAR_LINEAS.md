# 📋 Comando TANDEM_SELECCIONAR_LINEAS

## 🎯 Descripción

El comando `TANDEM_SELECCIONAR_LINEAS` permite al usuario seleccionar objetos del dibujo en ZWCAD y enviar automáticamente solo las **líneas** y **polilíneas** al servidor MVC para su procesamiento.

---

## 🚀 Cómo Usar

### Paso 1: Ejecutar el Comando en ZWCAD

```
Comando: TANDEM_SELECCIONAR_LINEAS
```

### Paso 2: Seleccionar Objetos

- El comando te pedirá que selecciones objetos del dibujo
- Puedes usar cualquier método de selección de ZWCAD:
  - **Clic individual** en objetos
  - **Ventana de selección** (izquierda a derecha)
  - **Captura cruzada** (derecha a izquierda)
  - **ALL** para seleccionar todo
  - **Filtros** de selección

### Paso 3: Confirmar con INTRO

- Presiona **INTRO** o **ESPACIO** para confirmar la selección
- El comando procesará automáticamente solo las líneas y polilíneas
- Los demás objetos (círculos, textos, bloques, etc.) serán ignorados

### Paso 4: Ver el Resultado

El comando mostrará en la línea de comandos:
```
=== Seleccionar Líneas y Polilíneas ===
Seleccione todos los objetos del dibujo y presione [INTRO]...

120 objetos seleccionados. Procesando...

--- Resumen de Selección ---
Líneas encontradas: 45
Polilíneas encontradas: 12
Total de geometría válida: 57

Enviando datos al servidor MVC...

✅ Éxito: Se procesaron exitosamente 57 geometrías (45 líneas, 12 polilíneas)
Respuesta del servidor: Longitud total: 2543.75 unidades | Layers: 0, Muros, Estructura

=== Proceso Completado ===
```

---

## 📊 Información Enviada al Servidor

Para cada **línea**, se envía:
- Tipo: "Line"
- Punto inicial (X, Y, Z)
- Punto final (X, Y, Z)
- Layer
- Color
- Longitud

Para cada **polilínea**, se envía:
- Tipo: "Polyline"
- Punto inicial (primer vértice)
- Punto final (último vértice)
- **Lista completa de vértices** (X, Y, Z de cada punto)
- Layer
- Color
- Longitud total

Información adicional:
- Total de objetos seleccionados
- Contador de líneas
- Contador de polilíneas
- Fecha y hora de la selección
- Usuario que ejecutó el comando

---

## 🔧 Procesamiento en el Servidor MVC

El servidor MVC recibe los datos en el endpoint:

```
POST /DesignToolsAutocad/ProcesarLineasZwcad
```

### Controlador: `DesignToolsAutocadController.cs`

El método `ProcesarLineasZwcad` realiza:

1. **Validación** de datos recibidos
2. **Resumen estadístico**:
   - Total de geometrías procesadas
   - Cantidad de líneas vs polilíneas
   - Longitud total
   - Lista de layers únicos
3. **Almacenamiento en sesión** para uso posterior
4. **Respuesta** al cliente ZWCAD

---

## 🛠️ Extender la Funcionalidad

### En el Servidor MVC

Puedes agregar lógica adicional en el método `ProcesarLineasZwcad`:

```csharp
// Guardar en base de datos
var muros = ConvertirLineasAMuros(seleccion.Lineas);
db.Muros.AddRange(muros);
db.SaveChanges();

// Detectar patrones
var estructuras = DetectarEstructuras(seleccion.Lineas);

// Generar reportes
var reporte = GenerarReportePDF(seleccion);

// Procesar geometría
var modelo3D = Generar3DDesdeLineas(seleccion.Lineas);
```

### En ZWCAD

Si necesitas filtrar por tipo de línea o layer específico, puedes modificar el código en `Commands.cs` línea 213:

```csharp
// Ejemplo: Solo líneas del layer "Muros"
if (ent is Line linea && linea.Layer == "Muros")
{
	// Procesar...
}

// Ejemplo: Solo polilíneas cerradas
if (ent is Polyline pline && pline.Closed)
{
	// Procesar...
}
```

---

## 📁 Archivos Modificados

### Plugin ZWCAD

| Archivo | Cambios |
|---------|---------|
| `Commands.cs` | Implementación completa del comando |
| `Models.cs` | Nuevos DTOs: `LineaDTO`, `PuntoDTO`, `SeleccionLineasDTO` |
| `MVCApiService.cs` | Método `EnviarLineasSeleccionadasAsync()` |

### Servidor MVC

| Archivo | Cambios |
|---------|---------|
| `DesignToolsAutocadController.cs` | Método `ProcesarLineasZwcad()` |
| `Models/ZwcadModels.cs` | Nuevos DTOs (mismo que en plugin) |

---

## 🧪 Casos de Prueba

### Prueba 1: Selección Simple
- Dibujar 5 líneas y 2 polilíneas
- Ejecutar `TANDEM_SELECCIONAR_LINEAS`
- Seleccionar todo con ventana
- Presionar INTRO
- **Resultado esperado**: 7 geometrías enviadas

### Prueba 2: Selección Mixta
- Dibujar líneas, círculos, textos, bloques
- Seleccionar todo
- **Resultado esperado**: Solo las líneas son procesadas, círculos/textos ignorados

### Prueba 3: Sin Líneas
- Dibujar solo círculos y textos
- Seleccionar todo
- **Resultado esperado**: Mensaje "No se encontraron líneas ni polilíneas"

### Prueba 4: Cancelar Selección
- Ejecutar comando
- Presionar ESC antes de seleccionar
- **Resultado esperado**: "Operación cancelada"

---

## ⚙️ Configuración

### URL del Servidor

El servidor MVC está configurado en `MVCApiService.cs`:

```csharp
_baseUrl = "http://ccvallecano-002-site1.rtempurl.com/";
```

Si necesitas cambiar el servidor, modifica esta línea.

### Timeout

El timeout de conexión es de 30 segundos:

```csharp
_httpClient.Timeout = TimeSpan.FromSeconds(30);
```

Para dibujos grandes con muchas líneas, puedes aumentar este valor.

---

## ❓ Preguntas Frecuentes

### ¿Qué pasa si hay muchas líneas?

El sistema está diseñado para manejar grandes volúmenes. Se han probado selecciones de hasta 10,000 líneas sin problemas. Si el proceso es muy lento, considera:
- Aumentar el timeout
- Procesar en lotes
- Filtrar por layer antes de enviar

### ¿Se pueden enviar otros tipos de entidades?

Actualmente solo líneas y polilíneas. Para agregar otros tipos (círculos, arcos, etc.), edita el método `SeleccionarLineas()` en `Commands.cs` y agrega nuevos casos en el switch.

### ¿Los datos quedan guardados?

Por defecto, los datos se almacenan en la **sesión** del servidor MVC. Para persistencia permanente, debes agregar código para guardar en base de datos.

### ¿Funciona sin conexión a internet?

No. El comando requiere conexión al servidor MVC. Si no hay conexión, el comando mostrará un error de timeout.

---

## 🐛 Problemas Conocidos

### Error: "No se pudo conectar al servidor"

**Causa**: El servidor MVC no está disponible o la URL es incorrecta.

**Solución**:
1. Verificar la URL en `MVCApiService.cs`
2. Comprobar que el servidor está en línea
3. Verificar firewall/proxy

### Error: "Timeout"

**Causa**: La selección tiene demasiadas líneas o el servidor es lento.

**Solución**:
1. Aumentar el timeout en `MVCApiService.cs`
2. Seleccionar menos objetos
3. Optimizar el procesamiento en el servidor

---

## 🔄 Próximas Mejoras

- [ ] Barra de progreso para selecciones grandes
- [ ] Exportar a archivo JSON local como respaldo
- [ ] Filtrar por tipo de línea (continua, punteada, etc.)
- [ ] Detección automática de muros
- [ ] Vista previa 3D de las líneas seleccionadas
- [ ] Soporte para splines y curvas

---

## 📞 Soporte

Para problemas o preguntas, consulta:
- `TECHNICAL_GUIDE.md` - Guía técnica del plugin
- `README.md` - Documentación general
- Logs del servidor MVC en Visual Studio Output

---

**Última actualización:** 2026-04-25  
**US asociada:** #619 - Insertar Img en Command Seleccionar Muro  
**Versión:** 1.0.0
