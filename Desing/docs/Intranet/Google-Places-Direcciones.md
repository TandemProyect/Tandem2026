# Google Places — direcciones en formularios Intranet

Integración reutilizable: autocompletado, desglose de componentes y mapa. Basada en **`TandemAddressPlaces`** (JS), parciales Razor y columnas `*_Place_Id`, `*_Formatted_Address`, `*_Lat`, `*_Lng`, etc. en la entidad.

**Relacionado:** [UI-Formularios-y-Estilo.md](./UI-Formularios-y-Estilo.md) · [README](./README.md)

---

## 1. Archivos del sistema

| Archivo | Rol |
|---------|-----|
| `Views/Shared/_GooglePlacesAddressBlock.cshtml` | Markup del bloque (búsqueda, campos, mapa a la derecha) |
| `Views/Shared/_GooglePlacesAddressScripts.cshtml` | CSS, config JS, carga de `tandem-address-places.js` y script de página opcional |
| `Scripts/Intranet/tandem-address-places.js` | API global `window.TandemAddressPlaces` |
| `Scripts/Intranet/jobside-google-places.js` | Lógica Jobside: «facturación = local», reinicio bloque Bill |
| `Content/Intranet/tandem-address-places.css` | Mapa, bloque compacto, z-index PAC |
| `Models/GooglePlacesAddressBlockModel.cs` | Modelo del parcial + factory `FromJobside` |
| `Scripts/TemporalScript/GOOGLE_MAPS_LOCALHOST_REFERRERS.txt` | Referentes HTTP y diagnóstico local |

Registrar en **`Design.csproj`** (ya incluidos en Jobside):

```xml
<Compile Include="Models\GooglePlacesAddressBlockModel.cs" />
<Content Include="Views\Shared\_GooglePlacesAddressBlock.cshtml" />
<Content Include="Views\Shared\_GooglePlacesAddressScripts.cshtml" />
<Content Include="Content\Intranet\tandem-address-places.css" />
<Content Include="Scripts\Intranet\tandem-address-places.js" />
<Content Include="Scripts\Intranet\jobside-google-places.js" />
```

---

## 2. Integración en 3 pasos (nuevo formulario con dirección)

### Paso 1 — Base de datos y entidad

Columnas por prefijo (`Loc`, `Bill`, o uno solo), por ejemplo:

- `Place_Id`, `Formatted_Address`, `Lat`, `Lng`
- `Street_Number`, `Route`, `Subpremise`, `Locality`, `Admin_Area_1`, `Admin_Area_2`, `Postal_Code`, `Country_Code`, `Country_Name`
- `Address_Components_Json`

Actualizar EDMX/DAL y el `[Bind]` del controlador con todos los nombres que envía el formulario.

### Paso 2 — Vista de campos

Incluir el parcial con un `GooglePlacesAddressBlockModel` (o factory similar a `FromJobside`):

```cshtml
@Html.Partial("~/Views/Shared/_GooglePlacesAddressBlock.cshtml",
    GooglePlacesAddressBlockModel.FromJobside(Model, "Loc", "Dirección local"))
```

Convención de nombres POST: `{Prefix}_Formatted_Address`, `{Prefix}_Lat`, etc. (el prefijo coincide con `data-prefix` del fieldset).

**Layout del bloque:** campos en `col-md-7`, mapa en `col-md-5` (clase `tandem-address-map-col`). Clase del fieldset: `tandem-address-block tandem-address-block--compact`.

### Paso 3 — Scripts al pie de Create/Edit

En `@section scripts`:

```cshtml
@{
    ViewData["GooglePlacesPageScript"] = "~/Scripts/Intranet/mi-modulo-google-places.js"; // opcional
}
@Html.Partial("~/Views/Shared/_GooglePlacesAddressScripts.cshtml")
```

El parcial escribe `window.TandemAddressPlacesConfig` desde **Web.config** y llama a `tandem-address-places.js`. Si no hay lógica extra, omitir `GooglePlacesPageScript` y en un script inline:

```javascript
$(function () {
  if (window.TandemAddressPlaces) TandemAddressPlaces.init();
});
```

---

## 3. Modelo y factory

`GooglePlacesAddressBlockModel` expone `Prefix`, `MapElementId`, `Title` y todos los campos de dirección para el parcial.

Para Jobside:

```csharp
GooglePlacesAddressBlockModel.FromJobside(Model, "Loc", "Dirección local")
GooglePlacesAddressBlockModel.FromJobside(Model, "Bill", "Dirección facturación")
```

Para otra entidad, añadir un método estático `FromMiEntidad(...)` en el mismo archivo o un mapper en el controlador.

---

## 4. Patrón Jobside — doble dirección y checkbox

Orden **obligatorio** en `_JobsideFormFields.cshtml`:

1. Bloque **Loc** (`col-12`)
2. Fila **`#billSameAsLocRow`** con `BitBillSameAsLoc` — **fuera** de `#billAddressSection`
3. Contenedor **`#billAddressSection`** solo con el bloque **Bill**

```cshtml
<div class="col-12">
    @Html.Partial("~/Views/Shared/_GooglePlacesAddressBlock.cshtml",
        GooglePlacesAddressBlockModel.FromJobside(Model, "Loc", "Dirección local"))
</div>

<div class="col-12 mb-2" id="billSameAsLocRow">
    <div class="form-check">
        @Html.CheckBoxFor(m => m.BitBillSameAsLoc, new { @class = "form-check-input", id = "BitBillSameAsLoc" })
        <label class="form-check-label" for="BitBillSameAsLoc">Facturación = misma dirección que local</label>
    </div>
</div>

<div class="col-12" id="billAddressSection">
    @Html.Partial("~/Views/Shared/_GooglePlacesAddressBlock.cshtml",
        GooglePlacesAddressBlockModel.FromJobside(Model, "Bill", "Dirección facturación"))
</div>
```

**Importante:** `BitBillSameAsLoc` va en `#billSameAsLocRow`, **no** dentro de `#billAddressSection`, para que el script pueda ocultar solo el bloque de facturación.

En el controlador, si `BitBillSameAsLoc` en POST: `CopyLocToBill(model)` antes de validar.

`jobside-google-places.js`:

- Oculta `#billAddressSection` si el check está marcado.
- Deshabilita inputs de facturación cuando «misma dirección».
- Tras `TandemAddressPlaces.init()`, vuelve a aplicar el toggle; al desmarcar, reinicializa solo el bloque Bill.

---

## 5. Web.config y clave local

`Web.config` lleva `GoogleMaps:ApiKey` vacío y merge opcional `file="Web.GoogleMaps.config"`:

```xml
<appSettings file="Web.GoogleMaps.config">
  ...
  <add key="GoogleMaps:ApiKey" value="" />
  <add key="GoogleMaps:Language" value="es" />
  <add key="GoogleMaps:Region" value="ES" />
</appSettings>
```

**Desarrollo local:** copiar `Web.GoogleMaps.config.example` → `Web.GoogleMaps.config` (gitignored) o variable `GOOGLE_MAPS_API_KEY`. Ver `Scripts/TemporalScript/GOOGLE_MAPS_LOCALHOST_REFERRERS.txt`.

- **No** commitear claves reales; rotar cualquier clave expuesta en GitHub.
- Sin clave: aviso amarillo en formulario; el usuario puede rellenar dirección **manualmente** (el JS degrada con gracia).

---

## 6. Google Cloud Console (resumen)

Documento detallado: `Scripts/TemporalScript/GOOGLE_MAPS_LOCALHOST_REFERRERS.txt`.

### APIs a habilitar (mismo proyecto que la clave)

| API | Motivo |
|-----|--------|
| **Maps JavaScript API** | Carga `maps/api/js` y el mapa |
| **Places API (New)** | `PlaceAutocompleteElement` (autocompletado actual) |
| **Places API** (legacy, opcional) | Fallback `google.maps.places.Autocomplete` si falla el widget nuevo |

Si la clave tiene «Restringir a APIs seleccionadas», la lista **debe incluir** Maps JavaScript API y **Places API (New)** como mínimo.

### Restricción HTTP (desarrollo local)

IIS Express SSL del proyecto (`Design.csproj`): **`https://localhost:44384/`**

Añadir en la clave de navegador (referentes HTTP):

```
https://localhost:44384/*
http://localhost:44384/*
```

Opcional: `127.0.0.1` con el mismo puerto. Si usa otro puerto (p. ej. Cassini `55506`), añadir ese origen.

**Prueba rápida:** restricción de aplicación = «Ninguna». Si el mapa funciona, el problema son solo referentes.

### Facturación

Cuenta de facturación vinculada al proyecto (requerida aunque haya crédito gratuito).

### Diagnóstico en navegador

- Consola: `[TandemAddressPlaces] Google Maps authentication failed` → muestra `origin` y `referrerToAdd`.
- Red: petición a `maps.googleapis.com/maps/api/js` — si `TandemAddressPlacesConfig.apiKey` es `null`, revisar Web.config y reiniciar IIS Express.
- Credenciales: https://console.cloud.google.com/apis/credentials

Tras cambios en Console: guardar, esperar 1–5 min, Ctrl+F5.

---

## 7. API JavaScript `TandemAddressPlaces`

Config (inyectada por el parcial):

```javascript
window.TandemAddressPlacesConfig = {
  apiKey: "...",
  language: "es",
  region: "ES"
};
```

Métodos principales (`tandem-address-places.js`):

| Método | Uso |
|--------|-----|
| `init(options?)` | Inicializa todos los `.tandem-address-block` (o `options.selector`) |
| `initBlocks(selector)` | Igual, devuelve Promise si carga Maps |
| `loadMapsApi()` | Carga script Google una sola vez |
| `populateFromPlace`, `updateMap`, `setField`, `getField`, `clearMapState` | Uso avanzado / página específica |

Campos DOM: id `{Prefix}_{Suffix}` (ej. `Loc_Formatted_Address`, `Loc_Lat`).

Autocomplete: clase `js-tandem-places-autocomplete` en el input de búsqueda.

---

## 8. Avisos en UI (permitidos)

- `#tandemAddressApiWarning` — clave ausente o error de carga (parcial scripts).
- `.js-tandem-address-maps-error` — por bloque de dirección.
- Alerta en `_JobsideFormFields` si no hay `GoogleMaps:ApiKey` (solo Jobside).

Estos **no** sustituyen el texto de ayuda gris bajo cada campo (sigue prohibido en [UI-Formularios-y-Estilo.md](./UI-Formularios-y-Estilo.md)).

---

## 9. Checklist Google Places

1. [ ] Columnas en BD + entidad + Bind POST
2. [ ] Parcial `_GooglePlacesAddressBlock` con prefijo correcto
3. [ ] `_GooglePlacesAddressScripts` en `@section scripts` de Create/Edit
4. [ ] Entradas en `Design.csproj`
5. [ ] Web.config: `GoogleMaps:ApiKey`, Language, Region
6. [ ] Google Cloud: APIs + referentes `localhost:44384`
7. [ ] Si doble dirección: checkbox **fuera** de `#billAddressSection` + script toggle + `CopyLocToBill` en servidor
8. [ ] Probar selección Places, mapa, guardado y recarga en Edit

---

*Widget principal: `PlaceAutocompleteElement` (Places API New). Fallback legacy: `google.maps.places.Autocomplete` si el widget nuevo no está disponible.*
