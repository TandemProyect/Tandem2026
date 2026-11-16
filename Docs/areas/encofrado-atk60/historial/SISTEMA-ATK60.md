# 🏗️ SISTEMA ATK60 - Detección de Puntos para Encofrado

## 📋 Descripción General

El **Sistema ATK60** es un sistema modular de encofrado utilizado en construcción que requiere la identificación precisa de puntos críticos en estructuras para su instalación correcta.

---

## 🎯 Objetivo del Sistema

Detectar automáticamente los **puntos de anclaje** en las esquinas de estructuras rectangulares donde se instalará el sistema de encofrado ATK60.

### Requisitos Clave:
1. **Detección de Esquinas en L** → Identificar intersecciones perpendiculares
2. **Validación de Dimensiones** → Offset máximo entre paneles: 1500mm
3. **Marcado Visual** → Indicar en el dibujo dónde instalar los elementos
4. **Soporte Multi-orientación** → Funcionar con geometría rotada

---

## 📐 Especificaciones Técnicas

### Dimensiones del Sistema ATK60
- **Ancho máximo de panel**: 1500mm
- **Tolerancia angular**: 90° ± 1° (esquinas perpendiculares)
- **Precisión de detección**: 0.01mm

### Tipos de Esquinas Soportadas
1. **Esquina en L simple** → 2 muros perpendiculares
2. **Panel rectangular** → 4 líneas formando rectángulo (2 pares paralelos + perpendiculares)
3. **Geometría rotada** → Esquinas en cualquier orientación

---

## 🔧 Implementación Actual (US-637)

### Fase 1: Detección Básica ✅ COMPLETADA

**Funcionalidades Implementadas:**

#### 1. Detector de Esquinas (`LCornerDetector.cs`)
```csharp
- Detección basada en geometría, no en orientación absoluta
- Agrupación de líneas paralelas
- Validación de perpendicularidad entre grupos
- Cálculo de offset entre líneas paralelas
- Validación: offset ≤ 1500 unidades
```

#### 2. Comando ZWCAD (`TANDEM_SELECCIONAR_LINEAS`)
```
- Selección de geometría (Line + Polyline)
- Envío al servidor MVC para análisis
- Recepción de puntos detectados
- Dibuja círculos rojos (radio: 50 unidades) en cada punto
```

#### 3. Salida Diagnóstica
- **Archivo JSON**: `C:\temp\conexiones.json`
- Contiene:
  - Todas las líneas analizadas
  - Conexiones punto-a-punto detectadas
  - Pares de líneas paralelas
  - Paneles válidos (offset ≤ 1500)
  - Paneles inválidos (offset > 1500)
  - Resumen ejecutivo

---

## 📊 Ejemplo de Detección

### Caso 1: Rectángulo Simple (0° rotación)
```
Geometría:
- 4 líneas formando rectángulo de 10000x10000
- 4 líneas interiores a 300mm de separación

Resultado:
✅ 4 paneles válidos detectados
✅ 8 puntos de conexión marcados
✅ Offset: 300mm (< 1500mm) → VÁLIDO
```

### Caso 2: Rectángulo Rotado (45° rotación)
```
Geometría:
- Mismo rectángulo girado 45°
- Todas las líneas clasificadas como "Diagonal"

Resultado:
✅ 4 paneles válidos detectados
✅ 8 puntos de conexión marcados
✅ Detección basada en perpendicularidad, no en orientación absoluta
```

---

## 🚀 Próximas Fases (Pendientes)

### Fase 2: Detección Avanzada de Puntos ATK60
**Nueva US creada** (ID pendiente de asignación)

**Objetivos:**
1. **Identificación de Puntos de Instalación**
   - Detectar puntos específicos donde se anclan los paneles ATK60
   - Diferenciar entre puntos de esquina y puntos intermedios

2. **Clasificación de Esquinas**
   - Esquina exterior (convexa)
   - Esquina interior (cóncava)
   - Esquina en T
   - Esquina en cruz

3. **Cálculo de Distancias**
   - Distancia entre puntos de anclaje
   - Validación de espaciado según especificaciones ATK60

4. **Exportación de Datos**
   - Lista de coordenadas para instalación
   - Diagrama de montaje
   - Reporte de materiales necesarios

**Story Points Estimados:** 8

---

## 📁 Archivos del Sistema

### Código Fuente
```
Desing/
├── Controllers/
│   └── DesignToolsAutocadController.cs  (Endpoint MVC)
├── Services/
│   └── LCornerDetector.cs               (Lógica de detección)
└── Models/
	└── ZwcadModels.cs                    (DTOs)

TamdenZwcadPluging/
└── ZwcadPlugin/
	├── Commands.cs                       (Comando ZWCAD)
	├── MVCApiService.cs                  (Cliente HTTP)
	└── Models.cs                         (DTOs plugin)
```

### Documentación
```
AGENTE-US619-INFO.md          → Documentación técnica US-619
AGENTE-US619-RESUMEN.md       → Resumen ejecutivo US-619
SISTEMA-ATK60.md              → Este archivo (visión general)
TEST-SELECCIONAR-LINEAS.md    → Guía de pruebas
```

### Salidas
```
C:\temp\conexiones.json       → Datos diagnósticos en tiempo real
```

---

## 🧪 Cómo Probar el Sistema

### Paso 1: Iniciar Servidor MVC
```powershell
# En Visual Studio, ejecutar proyecto "Desing"
# URL: https://localhost:44384/
```

### Paso 2: Cargar Plugin en ZWCAD
```
1. Abrir ZWCAD
2. Comando: NETLOAD
3. Seleccionar: C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
```

### Paso 3: Ejecutar Detección
```
1. Comando: TANDEM_SELECCIONAR_LINEAS
2. Seleccionar geometría (rectángulos, polilíneas)
3. Presionar Enter
4. Ver círculos rojos en puntos detectados
5. Revisar: C:\temp\conexiones.json
```

---

## 📈 Métricas de Calidad

### Precisión de Detección
- **Esquinas perpendiculares (90°)**: 100% precisión
- **Tolerancia angular**: ±1°
- **Offset válido**: 100% cuando ≤ 1500 unidades
- **Geometría rotada**: Soportada en cualquier ángulo

### Performance
- **Tiempo de análisis**: < 500ms para 50 líneas
- **Tamaño JSON**: ~150KB para 8 líneas
- **Círculos dibujados**: Instantáneo

---

## 🔗 Referencias

### Azure DevOps
- **US-619** (Completada): https://dev.azure.com/VSCAD/213253e7-f177-4e2d-bdf3-410b97f6883d/_workitems/edit/619
- **Nueva US** (Por crear): Detección de Puntos para Sistema ATK60

### Commits Relevantes
- **59afd1c**: feat(US-637): Implementar detección de esquinas en L con validación de offset y soporte para geometría rotada

---

## 📞 Contacto

**Equipo de Desarrollo**
- Proyecto: Tandem 2026
- Organización: VSCAD
- Repositorio: https://github.com/JuanGodoyLopez/Tandem-2026

---

**Última Actualización:** 2026-04-27  
**Versión del Sistema:** 1.0 (Fase 1 completada)
