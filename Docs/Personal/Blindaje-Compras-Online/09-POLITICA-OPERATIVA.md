# POLÍTICA OPERATIVA — en vigor desde 2026-08-30

Este archivo es el **sistema montado**. No es un borrador: aplica a toda compra online relevante a partir de hoy.

## Reglas vinculantes

1. **PayPal no es filtro de confianza.** “Acepta PayPal” no autoriza la compra por sí solo.
2. **Umbral Escrow = 1.500 €** (salvo que lo cambies por escrito aquí: _____ €).
3. Si importe **≥ umbral** y el vendedor no es tienda Trusted Shops de confianza absoluta:
   - Exigir **Escrow.com**, o
   - **No comprar**.
4. Si hay sello **Trusted Shops** (ES): comprar preferible; **activar** Protección al Comprador (gratis hasta 2.500 €/pedido); pagar con **crédito**.
5. Compras online relevantes: **tarjeta de crédito** (no débito como medio principal). Virtual OK para datos; no sustituye Escrow.
6. Al recibir: foto caja + etiqueta (peso) + conteo. Incidencia en **≤ 48 h**. Nunca pagar “el resto” fuera del canal protegido.
7. Caso 50 € PayPal: **no reclamar** (abandonado). Reactivar solo si lo pides explícitamente.

## Estado de capas

| Capa | Estado | Acción pendiente |
|---|---|---|
| Política + protocolo | **ACTIVA** | Ninguna |
| Regla Cursor | **ACTIVA** | Ninguna |
| Trusted Shops | Lista para usar en checkout | Registro opcional ahora; obligatorio activar protección en cada pedido TS |
| Escrow.com | **Bloqueo hasta alta** | Crear cuenta + verificar ID **antes** de la próxima compra ≥ 1.500 € |
| Tarjeta crédito | Política activa; emisor sin anotar | Anotar banco/tarjeta en `02` cuando puedas (1 minuto) |

## Gate antes de la próxima compra grande

```
¿Importe ≥ 1.500 €?
  NO → crédito (+ TS si hay sello)
  SÍ → ¿Tengo Escrow verificado Y el vendedor acepta?
         SÍ → comprar por Escrow
         NO → NO COMPRAR (o solo tienda TS conocida + crédito + activar protección)
```

## Mensaje fijo al vendedor (≥ umbral)

```
Uso Escrow.com. Yo pago a Escrow; tú envías; verifico unidades en 3–5 días; se liberan fondos. ¿Aceptas?
```

## Puesta en marcha residual (una vez)

- [ ] `abrir-registros.ps1` → cuenta Escrow + verificación  
- [ ] (Opcional) cuenta Trusted Shops  
- [ ] Rellenar banco/tarjeta en `02-TRUSTED-SHOPS-Y-TARJETAS.md`  

Hasta marcar Escrow, la política **prohíbe** compras ≥ 1.500 € fuera de Trusted Shops de confianza.
