# 05 - Pendientes y preguntas

## BLOQUEO: Preguntas que necesitan respuesta antes de codificar

### P1 - Tipo de input 2D
Opciones:
- A) Polilinea cerrada (banda de dos caras, eje = centro).
- B) Doble linea (dos Line o dos segmentos de Polyline paralelos).
- C) Eje simple + espesor como parametro (lo mas sencillo para empezar).

**Decision actual**: sin confirmar.
**Impacto**: cambia completamente WallReader.cs y GeometryNormalizer.cs.

---

### P2 - Sistema de encofrado para v1
Opciones:
- A) ATK60 (ya existe logica en el proyecto Design/Repositories/Atk60/).
- B) Sistema generico simplificado (cuboides, para validar el pipeline).

**Decision actual**: sin confirmar.
**Recomendacion**: empezar con B (generico) para validar el pipeline completo,
luego implementar A (ATK60) sobre la misma interfaz.

---

### P3 - Primer comando a implementar
Opciones:
- A) `DETECTARMUROS`: lee 2D, construye modelo topologico, muestra resultado en consola. Sin 3D todavia.
- B) `GENERAR3D` directo: lee 2D y genera 3D en un solo paso.

**Decision actual**: sin confirmar.
**Recomendacion**: empezar con A para validar la deteccion antes de generar 3D.

---

## Pendientes tecnicos (a definir con el equipo)

### Input
- [ ] Confirmar si los muros vienen en una capa especifica (nombre de capa).
- [ ] Tolerancia de snap entre extremos (ej. 5mm, 10mm, 50mm).
- [ ] Unidades del dibujo (mm, cm, m).
- [ ] Parametros por defecto para altura y espesor si no estan en el DWG.

### Modelo
- [ ] Como se define el muro "principal" en uniones T y + (por capa, por longitud, por seleccion).
- [ ] Necesidad de flags interior/exterior en v1.

### Salida
- [ ] Capa donde se insertan los solidos 3D (nueva capa "3D_ENCOFRADO" o por sistema).
- [ ] Formato de metadata: XData vs ExtensionDictionary vs JSON externo.

### Integracion con servidor MVC
- [ ] Se enviara el WallModel al servidor o solo se trabaja localmente en el DWG.
- [ ] Si se envia, que endpoints del servidor MVC deben crearse.

---

## Historial de decisiones tomadas

| Fecha | Decision | Detalle |
|-------|----------|---------|
| 2026-04-22 | Topologia independiente del sistema | WallModel no conoce IEncofradoSystem |
| 2026-04-22 | Orientaciones = rotaciones del mismo tipo | Esq_10/30/50/70 son L con diferente angulo |
| 2026-04-22 | El 2D no se modifica | Solo se usan los solidos 3D como capa nueva |
| 2026-04-22 | Input 2D como eje simple (opcion C) | Pendiente confirmar |
