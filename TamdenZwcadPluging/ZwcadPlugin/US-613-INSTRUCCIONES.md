# 📘 US-613: Integración ZWCAD Plugin con Servidor MVC

## 🎯 Objetivo
Permitir que el plugin de ZWCAD envíe líneas y polilíneas seleccionadas al servidor MVC para su procesamiento.

---

## ✅ Cambios Realizados

### 1. **MVCApiService.cs** - Actualización de URL y Certificados SSL

**Archivo:** `TamdenZwcadPluging\ZwcadPlugin\MVCApiService.cs`

**Cambios:**
- ✅ URL actualizada de `http://localhost:5000/` a `https://localhost:44384/`
- ✅ Agregado manejo de certificados SSL para desarrollo local
- ✅ Agregados `using System.Net` y `using System.Net.Security`

**Código actualizado:**
```csharp
public MVCApiService()
{
	_httpClient = new HttpClient();
	// Para testing local, usa localhost. Para producción, usa el servidor remoto.
	_baseUrl = "https://localhost:44384/"; // ✅ Puerto IIS Express del proyecto Desing
	// _baseUrl = "http://ccvallecano-002-site1.rtempurl.com/"; // Producción
	_httpClient.BaseAddress = new Uri(_baseUrl);
	_httpClient.Timeout = TimeSpan.FromSeconds(30);

	// ⚠️ SOLO PARA DESARROLLO: Ignorar errores de certificado SSL en localhost
	// Quitar en producción o cuando uses un certificado válido
	ServicePointManager.ServerCertificateValidationCallback += 
		(sender, certificate, chain, sslPolicyErrors) => true;
}
```

---

## 🚀 Cómo Ejecutar

### **Paso 1: Iniciar el Servidor MVC (Proyecto Desing)**

1. En Visual Studio, establece el proyecto **Desing** como proyecto de inicio:
   - Click derecho en el proyecto `Desing` (Design.csproj)
   - Selecciona **"Set as Startup Project"**

2. Presiona **F5** o haz clic en **"IIS Express"** para iniciar el servidor

3. Verifica que el navegador se abra en `https://localhost:44384/`

4. **⚠️ IMPORTANTE:** Mantén el servidor corriendo mientras usas el plugin

### **Paso 2: Usar el Plugin en ZWCAD**

1. Abre ZWCAD
2. Carga el plugin (si no está cargado automáticamente)
3. Selecciona líneas o polilíneas en el dibujo
4. Ejecuta el comando del plugin para enviar las selecciones
5. El plugin ahora se conectará correctamente al servidor MVC

---

## 🔍 Verificación del Flujo

### **Plugin → Servidor**

1. **Plugin selecciona geometría:**
   ```
   📐 ZWCAD Plugin
   ├─ Comando: SeleccionarLineas()
   ├─ Captura: Líneas y Polilíneas
   └─ Envía: POST a MVCApiService
   ```

2. **MVCApiService envía datos:**
   ```
   🌐 HTTP Request
   ├─ URL: https://localhost:44384/DesignToolsAutocad/ProcesarLineasZwcad
   ├─ Método: POST
   ├─ Content-Type: application/json
   └─ Body: SeleccionLineasDTO
   ```

3. **Servidor procesa:**
   ```
   🖥️ Servidor MVC (Desing)
   ├─ Controlador: DesignToolsAutocadController
   ├─ Acción: ProcesarLineasZwcad()
   ├─ Procesa: Líneas, Polilíneas, Layers, Longitudes
   └─ Responde: ApiResponse<string>
   ```

---

## 📊 Endpoint del Servidor

### **POST /DesignToolsAutocad/ProcesarLineasZwcad**

**Archivo:** `Desing\Controllers\DesignToolsAutocadController.cs`

**Request Body:**
```json
{
  "TotalLineas": 5,
  "TotalPolilineas": 2,
  "Usuario": "JuanGodoyLopez",
  "FechaSeleccion": "2024-01-15T10:30:00",
  "Lineas": [
	{
	  "Tipo": "Line",
	  "PuntoInicio": { "X": 0, "Y": 0, "Z": 0 },
	  "PuntoFin": { "X": 100, "Y": 0, "Z": 0 },
	  "Longitud": 100.0,
	  "Layer": "0",
	  "Color": "ByLayer"
	}
  ]
}
```

**Response:**
```json
{
  "Exito": true,
  "Mensaje": "Se procesaron exitosamente 7 geometrías (5 líneas, 2 polilíneas)",
  "Datos": "Longitud total: 850.25 unidades | Layers: 0, Muros, Estructura"
}
```

---

## 🛠️ Configuración de Puertos

| Componente | Puerto | Protocolo | URL Completa |
|------------|--------|-----------|--------------|
| **Servidor MVC (Desing)** | 44384 | HTTPS | `https://localhost:44384/` |
| **Plugin ZWCAD** | - | - | Se conecta al servidor MVC |

**Configuración IIS Express (Desing/Design.csproj):**
```xml
<IISUrl>https://localhost:44384/</IISUrl>
<IISExpressSSLPort>44384</IISExpressSSLPort>
<UseIISExpress>true</UseIISExpress>
```

---

## ⚠️ Solución de Problemas

### **Error 404 (Not Found)**

**Síntomas:**
```
❌ Error: Error al enviar líneas: El código de estado de la respuesta no indica 
un resultado correcto: 404 (Not Found).
```

**Causas y Soluciones:**

1. **El servidor MVC NO está corriendo**
   - ✅ Solución: Iniciar el proyecto Desing en Visual Studio (F5)
   - ✅ Verificar que el navegador muestre la aplicación web

2. **Puerto incorrecto en MVCApiService**
   - ✅ Solución: Ya corregido - ahora usa `https://localhost:44384/`

3. **Firewall bloqueando conexión**
   - ✅ Solución: Permitir Visual Studio / IIS Express en el firewall de Windows

### **Error de Certificado SSL**

**Síntomas:**
```
❌ Error: Could not establish trust relationship for the SSL/TLS secure channel
```

**Solución:**
- ✅ Ya implementada: `ServicePointManager.ServerCertificateValidationCallback`
- ⚠️ Solo para desarrollo local
- 🔒 En producción, usa un certificado válido

### **Timeout al enviar datos**

**Síntomas:**
```
❌ Error: The operation has timed out
```

**Soluciones:**
1. Verificar que el servidor responde (abrir URL en navegador)
2. Aumentar timeout si es necesario:
   ```csharp
   _httpClient.Timeout = TimeSpan.FromSeconds(60); // Aumentar a 60 segundos
   ```

---

## 📈 Próximos Pasos (Futuras Mejoras)

1. **Procesamiento de Geometría**
   - Detección automática de muros
   - Cálculo de áreas y perímetros
   - Análisis de intersecciones

2. **Persistencia de Datos**
   - Guardar selecciones en base de datos
   - Historial de operaciones por usuario
   - Exportar a formatos DXF/DWG

3. **Interfaz de Usuario Web**
   - Visualización 3D de geometrías enviadas
   - Dashboard de estadísticas
   - Editor de propiedades de elementos

4. **Seguridad**
   - Autenticación de usuarios
   - Autorización por roles
   - Encriptación de datos sensibles

---

## 📝 Notas de Desarrollo

### **Arquitectura Actual**

```
┌─────────────────┐         HTTPS          ┌──────────────────┐
│  ZWCAD Plugin   │ ───────────────────────>│  Servidor MVC    │
│  (C# .NET 4.8)  │   POST /DesignTools... │  (Desing)        │
│                 │<─────────────────────── │                  │
└─────────────────┘      JSON Response      └──────────────────┘
		 │                                            │
		 │                                            │
		 v                                            v
  ┌──────────────┐                          ┌─────────────────┐
  │ MVCApiService│                          │DesignToolsAuto- │
  │              │                          │cadController    │
  └──────────────┘                          └─────────────────┘
```

### **Modelos de Datos (DTOs)**

**SeleccionLineasDTO:**
```csharp
public class SeleccionLineasDTO
{
	public int TotalLineas { get; set; }
	public int TotalPolilineas { get; set; }
	public string Usuario { get; set; }
	public DateTime FechaSeleccion { get; set; }
	public List<LineaDTO> Lineas { get; set; }
}
```

**LineaDTO:**
```csharp
public class LineaDTO
{
	public string Tipo { get; set; } // "Line" o "Polyline"
	public PuntoDTO PuntoInicio { get; set; }
	public PuntoDTO PuntoFin { get; set; }
	public double Longitud { get; set; }
	public string Layer { get; set; }
	public string Color { get; set; }
	public List<PuntoDTO> Vertices { get; set; } // Solo para Polyline
}
```

---

## ✅ Checklist de Verificación

Antes de usar el sistema, verifica:

- [ ] El proyecto **Desing** está compilado sin errores
- [ ] El proyecto **Desing** está corriendo en IIS Express (puerto 44384)
- [ ] El navegador muestra la aplicación web correctamente
- [ ] ZWCAD está abierto con un dibujo
- [ ] El plugin está cargado en ZWCAD
- [ ] Hay líneas o polilíneas dibujadas para seleccionar

---

## 📞 Soporte

**Desarrollador:** Juan Godoy López  
**Proyecto:** Tandem 2026  
**Repositorio:** https://github.com/JuanGodoyLopez/Tandem-2026  
**Ubicación:** `C:\00_Tandem2026\`

---

## 📅 Historial de Cambios

| Fecha | Cambio | Autor |
|-------|--------|-------|
| 2024-01-XX | Corrección error 404 - Actualización de puerto a 44384 | GitHub Copilot |
| 2024-01-XX | Implementación inicial US-613 | Juan Godoy López |

---

## 🎉 Éxito

Si todo está configurado correctamente, deberías ver:

```
✅ Polilíneas encontradas: 2
✅ Total de geometría válida: 2
✅ Enviando datos al servidor MVC...
✅ Respuesta del servidor: Se procesaron exitosamente 2 geometrías (0 líneas, 2 polilíneas)
✅ Longitud total: 450.75 unidades | Layers: 0, Muros
```

---

**¡Feliz codificación! 🚀**
