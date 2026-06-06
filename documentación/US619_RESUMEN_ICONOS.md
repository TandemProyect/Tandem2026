# US #619 - Estado del Icono

## ⚠️ PENDIENTE DE COMPLETAR

**Comando:** ✅ Funciona  
**Botón visible:** ✅ Sí  
**Icono visible:** ❌ No (solo texto)

---

## 🔍 Problema Identificado

ZWCAD **solo muestra iconos** cuando están en:
```
C:\Program Files\ZWSOFT\ZWCAD 2026\Support\
```

Y el CUI usa **solo el nombre** del archivo:
```xml
<LargeImage>SelectLines.png</LargeImage>
```

❌ **NO funciona** con rutas relativas (`img\SelectLines.png`)  
❌ **NO funciona** con rutas absolutas (`C:\...\img\SelectLines.png`)

---

## ✅ Solución Validada

Creamos un CUI de prueba (`TestIconoV2.cui`) que **SÍ funciona**.

**Archivo de prueba:**
- `C:\00_Tandem2026\Docs\Proyectos\ZwcadPlugin\test_icons\TestIconoV2.cui`
- Icono: `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\simple_square.png`

---

## 📋 Pasos para Completar

1. **Generar PNG válido** de 32x32 px (actual: 243 bytes, corrupto)
2. **Copiar a:** `C:\Program Files\ZWSOFT\ZWCAD 2026\Support\SelectLines.png`
3. **Actualizar CuixBuilder.cs:**
   ```csharp
   <LargeImage>SelectLines.png</LargeImage>
   <SmallImage>SelectLines.png</SmallImage>
   ```
4. **Recompilar y probar en ZWCAD**

---

## 📄 Documentación Completa

Ver: `Docs/Proyectos/ZwcadPlugin/INVESTIGACION_ICONOS_US619.md`

---

**Tiempo estimado:** 15-30 minutos
