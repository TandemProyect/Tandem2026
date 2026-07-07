# Handover - Plugin Device Authorization

## Scope implemented

This iteration adds device-based authorization for the ZWCAD plugin, wired across:

- MVC backend (`Desing`)
- ZWCAD plugin client (`TamdenZwcadPluging`)
- Employee admin UI (`Employee/Create_Employee`)

## End-to-end flow

1. Plugin computes a deterministic `DeviceId` hash from machine context.
2. Before executing main commands, plugin calls:
   - `POST /DesignToolsAutocad/ValidarEquipoPlugin`
3. Backend checks `dbo.TSql_PluginDeviceAuth`.
4. Backend returns allow/deny + reason.
5. Plugin blocks execution if denied and shows reason in editor output.

## Backend changes

### Models

- `Desing/Models/ZwcadModels.cs`
  - `PluginAuthRequestDTO`
  - `PluginAuthResultDTO`

### Controller endpoint

- `Desing/Controllers/DesignToolsAutocadController.cs`
  - Added `ValidarEquipoPlugin(PluginAuthRequestDTO request)`
  - SQL checks use `IdentityConnection`
  - Defensive behavior:
    - denies if table missing
    - denies if record not found
    - supports multiple status columns (`Allowed`, `IsActive`, `IsRevoked`, `AttIsDeleted`, `Estado`)
  - Heartbeat update for last check if columns exist.

## Plugin changes

### API client + models

- `TamdenZwcadPluging/ZwcadPlugin/MVCApiService.cs`
  - `ValidarEquipoPluginAsync(...)`
- `TamdenZwcadPluging/ZwcadPlugin/Models.cs`
  - matching DTOs

### Commands guard

- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs`
  - Added `ValidarAccesoPlugin(Editor ed)` guard
  - Guard applied in main commands and init path
  - Added command:
    - `TANDEM_DEVICE_ID` (prints `DeviceId` + `MachineName`)

## Employee admin integration

### UI fields added

- `Desing/Views/Employee/Create_Employee.cshtml`
  - `DeviceName`
  - `DeviceId`
  - `DeviceAllowed`

### ViewModel

- `Desing/Models/EmployeeViewModel.cs`
  - `DeviceName`, `DeviceId`, `DeviceAllowed`
  - `EmployeeID`, `IsEdit`

### Controller behavior

- `Desing/Controllers/EmployeeController.cs`
  - loads current device auth for employee user
  - upserts into `dbo.TSql_PluginDeviceAuth`
  - create/edit split:
    - `Create_Employee` (create)
    - `Update_Employee` (edit)
  - both route through shared save method to avoid duplicated logic.

## Important bug fixed in this pass

Issue:
- editing employee accidentally created a new row due to session reset.

Fix:
- do not clear `Session["EmployeeID"]` in edit mode
- use explicit `IsEdit` + `EmployeeID`
- route edit form to `Update_Employee`.

## Email behavior

For now, employee creation flow does not rely on SMTP to complete core save.
SMTP follow-up is intentionally deferred.

## SQL assumptions

Expected table:
- `dbo.TSql_PluginDeviceAuth`

Minimum required:
- `DeviceId` (unique identifier)

Recommended columns (supported by current code):
- `LinAspNetUsert`, `MachineName`, `UsuarioWindows`, `PluginVersion`
- `Allowed`, `IsActive`, `IsRevoked`, `Estado`, `AttIsDeleted`
- `LastCheckUtc`, `AttLastModification`

## Operational steps

1. Get device data from plugin command:
   - `TANDEM_DEVICE_ID`
2. Open employee form:
   - `/Employee/Create_Employee`
3. Fill device fields and save.
4. Toggle allowed/block by editing employee device auth.

## Security notes

- Secrets were moved away from hardcoded PAT/API patterns where touched.
- See `SECURITY-INCIDENT-RESPONSE.md` for cleanup and history rewrite procedure.
