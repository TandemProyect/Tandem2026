# Handover - Plantilla Login Materio (2026-05-08)

## Objetivo

Actualizar la pantalla de login en `Desing` para usar una plantilla visual tipo Materio con:

- imagen inferior personalizada de obra/construccion,
- formulario centrado y legible,
- funcionalidad estable para mostrar/ocultar contrasena.

## Archivos tocados

- `Desing/Views/Account/Login.cshtml`
- `Desing/assets/img/login-construction-skyline.png`

## Cambios aplicados en Login

### 1) Fondo decorativo inferior personalizado

Se reemplazo la decoracion de ilustraciones por una sola imagen:

- `~/assets/img/login-construction-skyline.png`

Ajustes visuales aplicados en CSS:

- tamano amplio y centrado horizontalmente,
- posicion fija en la zona inferior con desplazamiento vertical configurable,
- `mix-blend-mode: multiply` para integrar mejor el fondo claro de la imagen sobre el fondo gris de la pantalla,
- `pointer-events: none` para no interferir con clics del formulario.

### 2) Ajuste de layout para que no tape el formulario

En `.authentication-inner` se mantiene el formulario por encima (`z-index` del card) y la imagen en capa de fondo para conservar usabilidad.

### 3) Mostrar/Ocultar contrasena (fix funcional)

Se reforzo la interaccion del icono de ojo para evitar problemas por scripts del tema:

- control de toggle en boton (`type="button"`),
- funcion global `togglePasswordVisibility(event)`,
- click directo en `onclick` y ademas listener delegado para mayor compatibilidad,
- actualizacion de `type` del input (`password`/`text`) e icono visual.

## Resultado esperado

- Login con imagen inferior alineada al estilo solicitado.
- Formulario no bloqueado ni tapado.
- Boton de mostrar contrasena funcionando de forma consistente.

## Ajustes rapidos para futuro

En `Desing/Views/Account/Login.cshtml`, clase `.tandem-login-skyline`:

- subir/bajar imagen: propiedad `bottom`
- hacerla mas grande/pequena: `max-height`
- atenuar o reforzar integracion: `opacity`, `mix-blend-mode`, `filter`

