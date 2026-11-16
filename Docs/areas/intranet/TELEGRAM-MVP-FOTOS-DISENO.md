# Telegram MVP - Fotos a Diseno (ZWCAD)

## Objetivo

Preparar un MVP para recibir fotos desde Telegram y registrarlas referidas a `dbo.TSql_Design`.

**Configuracion temporal actual:** todas las fotos se amarran al diseno fijo `SysObjectID = 131`.

## Implementacion incluida

- `Desing/Controllers/TelegramWebhookController.cs`
  - `POST /TelegramWebhook/Inbound`
  - `GET /TelegramWebhook/FotosPendientes?designId=131&top=20`
  - `GET /TelegramWebhook/ResolverArchivo?inboxId=<id>`
- `Desing/Models/ZwcadModels.cs`
  - nuevo DTO `TelegramDesignPhotoDTO`
- `Scripts/sql/2026-05-08_create_telegram_inbox.sql`
  - tabla `dbo.TSql_TelegramDesignInbox`
  - tabla `dbo.TSql_TelegramDesignAccess` (control multiusuario por chat/usuario)
- `Desing/Web.config`
  - `TELEGRAM_BOT_TOKEN`
  - `TELEGRAM_WEBHOOK_SECRET`

## Flujo funcional

1. Usuario envia foto al bot de Telegram con caption que incluya diseno, por ejemplo:
   - `DIS-131`
   - `DESIGN 131`
2. Telegram invoca webhook `Inbound`.
3. Se valida secret header (`X-Telegram-Bot-Api-Secret-Token`) si esta configurado.
4. Se extrae la mejor resolucion de `message.photo`.
5. El webhook usa `DesignId` detectado en texto o fallback a `131`.
6. Se valida que exista `dbo.TSql_Design.SysObjectID = <designId>`.
7. Se valida autorizacion en `dbo.TSql_TelegramDesignAccess` por `TelegramChatId` o `TelegramUserId`.
8. Se guarda registro con estado `Pendiente` en SQL.
9. ZWCAD o backend consulta `FotosPendientes` por diseno.

## Script SQL

Ejecutar:

- `Scripts/sql/2026-05-08_create_telegram_inbox.sql`

## Configuracion Telegram (pendiente operativo)

### 1) Crear bot

- usar `@BotFather`
- obtener `BOT_TOKEN`

### 2) Configurar secretos en servidor

- `TELEGRAM_BOT_TOKEN`
- `TELEGRAM_WEBHOOK_SECRET`

### 3) Registrar webhook

Ejemplo:

`https://api.telegram.org/bot<BOT_TOKEN>/setWebhook?url=https://tu-dominio/TelegramWebhook/Inbound&secret_token=<TELEGRAM_WEBHOOK_SECRET>`

## Contrato de salida para ZWCAD

`GET /TelegramWebhook/FotosPendientes?designId=131`

Retorna `ApiResponse<List<TelegramDesignPhotoDTO>>` con:

- `FileId`, `FileUniqueId`
- `Caption`
- `FechaMensajeUtc`, `FechaRegistroUtc`
- `Estado` (`Pendiente`)

### Resolver URL real de descarga Telegram

`GET /TelegramWebhook/ResolverArchivo?inboxId=<id>`

Retorna:

- `FileId`
- `DownloadUrl` (URL temporal usando `getFile`)
- `DesignId`

## Integridad referencial SQL

El script crea FK:

- `FK_TSql_TelegramDesignInbox_TSql_Design`
  - `TSql_TelegramDesignInbox.LinDesign` -> `TSql_Design.SysObjectID`
- `FK_TSql_TelegramDesignAccess_TSql_Design`
  - `TSql_TelegramDesignAccess.LinDesign` -> `TSql_Design.SysObjectID`

## Proximo paso recomendado

Agregar endpoint para descargar y persistir binario (blob/local) y dejar `UrlLocal` estable, evitando depender de URL temporal de Telegram.
