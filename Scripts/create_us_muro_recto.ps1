$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headers = @{
    Authorization = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

# Create User Story
$body = @(
    @{
        op = 'add'
        path = '/fields/System.Title'
        value = 'Detectar y extruir muros rectos entre esquinas L'
    },
    @{
        op = 'add'
        path = '/fields/System.Description'
        value = 'Dado un conjunto de esquinas L detectadas, identificar los tramos rectos de muro entre ellas y generar la geometría 3D.

**Alcance:**
- Detectar los 24 puntos numerados (ver Muro_Recto3.png en C:\temp)
- Dibujar cuadrados de color en cada punto (mitad del radio del círculo actual)
- Formar polilíneas rectangulares por cada tramo de muro (A-F) según la leyenda del documento
- Duplicar cada polilínea (capa ObjetoDB2d + ModelDesing)
- Extruir a 2.70m en capa ModelDesing

**Muros:**
- A: puntos 1-2-3-4 (entre esquinas)
- B: puntos 5-6-7-8 (entre esquinas)
- C: puntos 9-10-11-12 (entre esquinas)
- D: puntos 13-14-15-16 (entre esquinas)
- E: puntos 17-18-19-20 (sin esquina)
- F: puntos 21-22-23-24 (sin esquina)

**Referencia:** Muro_Recto3.png en C:\temp'
    },
    @{
        op = 'add'
        path = '/fields/System.Tags'
        value = 'muro-recto; extrusion; 3D; L-corner'
    }
)

$json = ConvertTo-Json $body -Depth 10
$response = Invoke-RestMethod -Uri 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$User Story?api-version=7.1' -Method Post -Headers $headers -Body $json
$usId = $response.id
Write-Output "US ID: $usId"

# Add tasks as children
$tasks = @(
    @{ title = 'T1: Dibujar cuadrados de color en los 24 puntos numerados'; desc = 'Sustituir/subir cuadrados de lado = radio/2 en cada punto con color único según tipo de punto (verde, amarillo, azul, cian, rojo, blanco, magenta, criss).' },
    @{ title = 'T2: Formar polilíneas rectangulares para muros A-D (entre esquinas)'; desc = 'Para cada par de esquinas adyacentes, identificar los 4 puntos que forman el tramo recto y crear polilínea cerrada. Secuencias: A=1-2-3-4, B=5-6-7-8, C=9-10-11-12, D=13-14-15-16.' },
    @{ title = 'T3: Formar polilíneas para muros E-F (sin esquina)'; desc = 'Muros que no arrancan de esquina L. Secuencias: E=17-18-19-20, F=21-22-23-24.' },
    @{ title = 'T4: Duplicar polilíneas y extruir a 2.70m'; desc = 'Cada polilínea se duplica: capa ObjetoDB2d (sin extrusión) + capa ModelDesing (Thickness=2700). Mismo patrón que las esquinas L existentes.' }
)

foreach ($task in $tasks) {
    $taskBody = @(
        @{ op = 'add'; path = '/fields/System.Title'; value = $task.title },
        @{ op = 'add'; path = '/fields/System.Description'; value = $task.desc },
        @{
            op = 'add'
            path = '/relations/-'
            value = @{
                rel = 'System.LinkTypes.Hierarchy-Reverse'
                url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workItems/$usId"
            }
        }
    )
    $taskJson = ConvertTo-Json $taskBody -Depth 10
    $taskResponse = Invoke-RestMethod -Uri 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$Task?api-version=7.1' -Method Post -Headers $headers -Body $taskJson
    Write-Output "  Task: $($taskResponse.id) - $($task.title)"
}

# Move US to In Progress
$moveBody = @(
    @{
        op = 'add'
        path = '/fields/System.State'
        value = 'In Progress'
    }
)
$moveJson = ConvertTo-Json $moveBody -Depth 10
Invoke-RestMethod -Uri "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$usId`?api-version=7.1" -Method Patch -Headers $headers -Body $moveJson
Write-Output "US $usId moved to In Progress"
