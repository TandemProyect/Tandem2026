# Protocolo personal: blindaje de compras online

**Estado:** activo · iniciado 2026-08-30  
**Motivo:** disputa parcial PayPal (~50 €, menos unidades de las pedidas) resuelta en contra pese a pruebas. PayPal dejó de valer como único filtro de confianza.

## Objetivo

Tener un sistema reutilizable para compras habituales y de gran importe: intermediario o cobertura real, no solo “acepta PayPal”.

## Regla de oro (decision tree)

| Importe / contexto | Qué usar |
|---|---|
| Tienda UE con sello Trusted Shops | Activar **Protección al Comprador** tras el pedido (España: gratis hasta **2.500 €**/pedido, ~30 días). Preferir pagar con **tarjeta de crédito**. Cubierta fuerte: no entrega / no reembolso / insolvencia — **no** la uses como único remedio para “faltan unidades”. |
| Compra grande / proveedor desconocido / fuera de UE / vendedor pide pago directo | **Escrow.com** (depósito en garantía). El dinero no se libera hasta que confirmas cantidad y estado. **Umbral por defecto: 1.500 €.** |
| Disputa de cantidad (pedido incompleto) | Escrow (ideal) o **crédito + chargeback** con pruebas de peso/fotos. Trusted Shops / PayPal solos = frágiles. |
| Tienda dudosa sin Escrow ni Trusted Shops | No comprar, o solo crédito + pruebas + importe que aceptes perder. |
| Nunca como único filtro | “Acepta PayPal” ≠ tienda seria. |

## Capas de protección (qué cubre cada una)

1. **Escrow (Escrow.com)** — Congela el pago. Revisión humana en disputa. Ideal para cantidades grandes y contrapartes nuevas. **Coste:** ~2,6 % (mín. 50 €) hasta 5.000 € en estándar EUR; ver `01-COMPARATIVA-ESCROW.md`.
2. **Trusted Shops (España)** — Filtro de tienda certificada + garantía de reembolso en no entrega / no reembolso / insolvencia. **PLUS 12 €/año hasta 20.000 € es sobre todo DE/AT/NL**; en ES la base gratuita hasta 2.500 € suele bastar. Ver `02-TRUSTED-SHOPS-Y-TARJETAS.md`.
3. **Tarjeta de crédito + chargeback** — Derecho de reclamación al emisor por mercancía no recibida / no conforme. Distinto del “seguro de compra” (robo/rotura 90 días). Débito y saldo PayPal suelen dar peor posición.
4. **Tarjeta virtual (Revolut/N26/banco)** — Protege datos; **no** evita envío incompleto.

## Checklist antes de pagar (cualquier compra relevante)

- [ ] Captura de la oferta (unidades, precio, envío).
- [ ] Confirmación de pedido / factura.
- [ ] Método de pago elegido según la tabla de arriba.
- [ ] Si Trusted Shops: activar protección en los primeros días tras el pedido.
- [ ] Si Escrow: acuerdo escrito de inspección (plazo + qué se verifica: unidades, peso, modelo).
- [ ] Chat/correo con el vendedor guardado.

## Checklist al recibir el paquete

- [ ] Foto de la caja **cerrada** y de la **etiqueta de transporte** (peso, nº tracking).
- [ ] Abrir y contar unidades; foto del contenido.
- [ ] Contrastar peso etiqueta vs peso teórico (unidades × peso unitario + embalaje).
- [ ] Si falta mercancía: **no** aceptar liberación de escrow; abrir disputa con pruebas en ≤ 48 h.
- [ ] No pagar “el resto” por otro canal (señal clásica de extorsión post-envío).

## Caso abierto: 50 € PayPal

Plantilla lista en `03-APELACION-PAYPAL-PESO.md`. Si el pago fue con tarjeta vinculada a PayPal, el banco puede ser plan B (chargeback), con plazos según emisor.

## Cómo montarlo (una sola vez)

Sigue la checklist ejecutable: [`04-PUESTA-EN-MARCHA.md`](04-PUESTA-EN-MARCHA.md).  
Hoja de 10 segundos: [`05-HOJA-RAPIDA.md`](05-HOJA-RAPIDA.md).  
Índice: [`README.md`](README.md).

## Pendiente de personalizar

- [ ] Banco y tarjetas actuales (rellenar checklist en `02-TRUSTED-SHOPS-Y-TARJETAS.md`).
- [ ] Volumen típico de compra (€) para afinar si Escrow compensa el mínimo de 50 €.
- [ ] Particular vs negocio (afecta facturación y opciones B2B).
- [ ] Origen habitual: UE / fuera UE / mixto.
- [ ] Umbral Escrow escrito en `05-HOJA-RAPIDA.md`.
- [ ] Caso 50 €: apelación enviada o abandonada a conciencia (`04` Fase 0).

## Fuentes oficiales (revisar si cambian tarifas)

- Escrow.com fee calculator: https://www.escrow.com/fee-calculator
- Trusted Shops ES protección: https://www.trustedshops.es/proteccion-al-comprador/
- Help Trusted Shops (mercados): https://help.etrusted.com/hc/es/articles/23970803902354
- Visa — retroceso de cargo: https://www.visa.es/paga-con-visa/seguridad-en-tus-pagos/retroceso-de-cargo.html
- CEC-España (chargeback con tarjeta): https://portal-cec.consumo.gob.es/
