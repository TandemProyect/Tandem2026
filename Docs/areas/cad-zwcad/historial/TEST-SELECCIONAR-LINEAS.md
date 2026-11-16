# 🧪 Checklist de Testing: TANDEM_SELECCIONAR_LINEAS

**US:** #613  
**Fecha:** 2026-04-24  
**Tester:** _________

---

## 🎯 Objetivo del Test

Validar que el comando `TANDEM_SELECCIONAR_LINEAS` funciona correctamente en ZWCAD, filtra solo líneas y polilíneas, y envía los datos al servidor MVC.

---

## ⚙️ Preparación del Test

### 1. Verificar que el servidor MVC está corriendo

```powershell
# Desde Visual Studio:
# - Proyecto "Desing" como StartUp Project
# - Presionar F5 o ejecutar
# - Verificar que el navegador abre en http://localhost:XXXX
```

**Puerto del servidor:** _____________ (anotar para referencia)

**✅ Servidor corriendo correctamente:** [ ]

---

### 2. Cargar el plugin en ZWCAD

```
1. Abrir ZWCAD
2. Escribir comando: NETLOAD
3. Seleccionar: C:\00_Tandem2026\TamdenZwcadPluging\ZwcadPlugin\bin\Debug\ZwcadPlugin.dll
4. Verificar que carga sin errores
```

**✅ Plugin cargado correctamente:** [ ]

---

### 3. Verificar configuración del servidor en el plugin

**Archivo:** `ZwcadPlugin/MVCApiService.cs`  
**Verificar línea ~24:** `BaseAddress = new Uri("http://localhost:XXXX/")`

**⚠️ IMPORTANTE:** El puerto debe coincidir con el del servidor MVC

**✅ Puerto configurado correctamente:** [ ]

---

## 🧪 Casos de Prueba

### Test 1: Selección de solo líneas

**Preparación:**
1. Crear un dibujo nuevo en ZWCAD
2. Dibujar 3-5 líneas usando el comando `LINE`
3. Variar longitudes y orientaciones

**Ejecución:**
```
1. Escribir comando: TANDEM_SELECCIONAR_LINEAS
2. Seleccionar todas las líneas (ventana de selección o clic individual)
3. Presionar ENTER
```

**Resultado esperado en línea de comandos de ZWCAD:**
```
Comando: TANDEM_SELECCIONAR_LINEAS
Seleccione objetos (líneas y polilíneas):
[Usuario selecciona objetos y presiona Enter]
Procesando selección...
Total de objetos seleccionados: X
  - Líneas: X
  - Polilíneas: 0
Enviando datos al servidor...
✓ Respuesta del servidor: Procesadas X líneas y 0 polilíneas. Longitud total: XX.XX unidades
```

**Validaciones:**
- [ ] El comando se ejecuta sin errores
- [ ] Se muestran estadísticas correctas en ZWCAD
- [ ] El número de líneas coincide con lo dibujado
- [ ] La longitud total parece correcta

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 2: Selección de solo polilíneas

**Preparación:**
1. Dibujo nuevo
2. Dibujar 2-3 polilíneas usando `PLINE`
3. Variar número de vértices (ej: triángulo, rectángulo, forma irregular)

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Seleccionar todas las polilíneas
3. ENTER
```

**Resultado esperado:**
```
Procesando selección...
Total de objetos seleccionados: X
  - Líneas: 0
  - Polilíneas: X
✓ Respuesta del servidor: Procesadas 0 líneas y X polilíneas...
```

**Validaciones:**
- [ ] Detecta correctamente las polilíneas
- [ ] Cuenta correcta de polilíneas
- [ ] No muestra errores

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 3: Selección mixta (líneas + polilíneas)

**Preparación:**
1. Dibujar 3 líneas
2. Dibujar 2 polilíneas

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Seleccionar todo con ventana
3. ENTER
```

**Resultado esperado:**
```
Total de objetos seleccionados: 5
  - Líneas: 3
  - Polilíneas: 2
✓ Respuesta del servidor: Procesadas 3 líneas y 2 polilíneas...
```

**Validaciones:**
- [ ] Detecta ambos tipos correctamente
- [ ] Las cuentas son correctas
- [ ] La longitud total incluye ambos tipos

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 4: Filtrado de objetos no válidos

**Preparación:**
1. Dibujar 2 líneas
2. Dibujar 1 círculo (CIRCLE)
3. Dibujar 1 rectángulo sólido (RECTANGLE como región)
4. Agregar texto (TEXT)

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Seleccionar TODO con ventana (incluye círculo, texto, etc.)
3. ENTER
```

**Resultado esperado:**
```
Total de objetos seleccionados: 5
  - Líneas: 2
  - Polilíneas: 0
✓ Solo se enviaron las líneas, ignorando círculo, texto y otros objetos
```

**Validaciones:**
- [ ] Solo cuenta líneas y polilíneas
- [ ] Ignora círculos, texto y otros objetos
- [ ] No muestra errores al procesar objetos no válidos

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 5: Selección vacía

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Presionar ENTER sin seleccionar nada
```

**Resultado esperado:**
```
Seleccione objetos (líneas y polilíneas):
[Usuario presiona Enter sin seleccionar]
⚠ No se seleccionaron líneas o polilíneas válidas
```

**Validaciones:**
- [ ] Maneja correctamente la selección vacía
- [ ] Muestra mensaje apropiado
- [ ] No intenta enviar datos al servidor

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 6: Cancelación de selección

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Presionar ESC durante la selección
```

**Resultado esperado:**
```
Seleccione objetos (líneas y polilíneas):
[Usuario presiona ESC]
Comando cancelado
```

**Validaciones:**
- [ ] Permite cancelar con ESC
- [ ] No muestra errores
- [ ] No intenta procesar nada

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 7: Servidor MVC caído/no disponible

**Preparación:**
1. **DETENER** el servidor MVC (cerrar Visual Studio o detener debugging)
2. Dibujar 2 líneas en ZWCAD

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Seleccionar las líneas
3. ENTER
```

**Resultado esperado:**
```
Procesando selección...
Total de objetos seleccionados: 2
  - Líneas: 2
  - Polilíneas: 0
Enviando datos al servidor...
✗ Error al comunicarse con el servidor: [mensaje de error de conexión]
```

**Validaciones:**
- [ ] Maneja el error de conexión correctamente
- [ ] Muestra mensaje de error claro
- [ ] No crashea ZWCAD

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 8: Validación en el servidor MVC

**Preparación:**
1. Asegurarse de que el servidor MVC está corriendo
2. Dibujar 3 líneas en ZWCAD
3. Ejecutar el comando y seleccionar las líneas

**Validación en el servidor:**

1. **Poner breakpoint** en `DesignToolsAutocadController.cs` línea ~XXX (método `ProcesarLineasZwcad`)

2. **Ejecutar el comando** en ZWCAD

3. **Verificar en el debugger:**
   - [ ] El breakpoint se activa
   - [ ] `seleccion` no es null
   - [ ] `seleccion.Lineas` contiene 3 elementos
   - [ ] Cada `LineaDTO` tiene datos válidos:
	 - [ ] `Tipo` = "Line" o "Polyline"
	 - [ ] `Layer` tiene valor
	 - [ ] `Color` tiene valor
	 - [ ] `Longitud` > 0
	 - [ ] Coordenadas de inicio/fin o vértices presentes

4. **Continuar ejecución (F5)**

5. **Verificar respuesta:**
   - [ ] Devuelve `ApiResponse<string>` con `Success = true`
   - [ ] `Data` contiene mensaje descriptivo

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 9: Verificación de datos en Session

**Después del Test 8:**

1. En el debugger, inspeccionar: `Session["UltimaSeleccionLineas"]`
2. Verificar que contiene el objeto `SeleccionLineasDTO` guardado

**Validaciones:**
- [ ] La sesión guarda correctamente los datos
- [ ] Se pueden recuperar después

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

### Test 10: Polilíneas con diferentes números de vértices

**Preparación:**
1. Crear polilínea con 3 vértices (triángulo)
2. Crear polilínea con 10+ vértices (forma compleja)

**Ejecución:**
```
1. TANDEM_SELECCIONAR_LINEAS
2. Seleccionar ambas polilíneas
3. ENTER
```

**Validaciones:**
- [ ] Ambas se detectan correctamente
- [ ] Los vértices se capturan completamente
- [ ] No hay pérdida de datos en polilíneas complejas

**Resultado del test:** ✅ PASS / ❌ FAIL  
**Notas:** ____________________________________________

---

## 📊 Resumen de Resultados

| Test | Descripción | Resultado |
|------|-------------|-----------|
| 1 | Solo líneas | ⬜ |
| 2 | Solo polilíneas | ⬜ |
| 3 | Mixto | ⬜ |
| 4 | Filtrado objetos | ⬜ |
| 5 | Selección vacía | ⬜ |
| 6 | Cancelación | ⬜ |
| 7 | Servidor caído | ⬜ |
| 8 | Validación servidor | ⬜ |
| 9 | Session MVC | ⬜ |
| 10 | Polilíneas complejas | ⬜ |

**Tests pasados:** _____ / 10  
**Tests fallados:** _____ / 10

---

## 🐛 Bugs Encontrados

### Bug #1
**Descripción:** _____________________________________________  
**Severidad:** 🔴 Crítico / 🟡 Medio / 🟢 Bajo  
**Pasos para reproducir:**
1. _____________________________________________
2. _____________________________________________

**Comportamiento esperado:** _____________________________________________  
**Comportamiento actual:** _____________________________________________

---

### Bug #2
(Duplicar sección si es necesario)

---

## ✅ Criterios de Aceptación

- [ ] Todos los tests PASS (o bugs documentados como aceptables)
- [ ] El comando funciona sin crashear ZWCAD
- [ ] El servidor MVC recibe los datos correctamente
- [ ] Los datos en el servidor son precisos (tipos, coordenadas, longitudes)
- [ ] Manejo correcto de errores (servidor caído, selección vacía)
- [ ] El código está listo para marcar Task #635 (Test) como Done

---

## 📝 Notas Adicionales

_____________________________________________
_____________________________________________
_____________________________________________

---

## 🎯 Próximos Pasos

Una vez completado el testing:

```powershell
# Si todos los tests PASS:
cd C:\00_Tandem2026\Scripts
.\Edit-US.ps1 635 -Estado "Done"  # Marcar Task Test como Done

# Si hay bugs críticos:
# 1. Documentar en Azure DevOps
# 2. Crear tasks de corrección
# 3. Volver a testear después de fixes
```

---

**Tester:** _____________  
**Fecha inicio:** ____________  
**Fecha fin:** ____________  
**Tiempo total:** ____________  
**Aprobado por:** ____________
