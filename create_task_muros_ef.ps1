$pat = '7iXv8E4C8xK90U3zPRV1GrpNyfTf0piLOt1I5xhxkoIWMtvZ0elmJQQJ99CDACAAAAAAAAAAAAASAZDO1BX0'
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(':' + $pat))
$headers = @{
    Authorization  = 'Basic ' + $base64Auth
    'Content-Type' = 'application/json-patch+json'
}

$usId = 688

$title = 'T7: Muros E y F - Detectar muros rectos sin esquinas L (extremos libres en ambos lados)'
$desc  = 'Detectar muros rectos formados por dos lineas paralelas independientes, sin ninguna esquina L conectada y con los 4 endpoints libres.

**Algoritmo:**
1. Construir HashSet de lineas ya usadas en paneles L (innerH/outerH/innerV/outerV de cada PanelInfoMuro)
2. Iterar pares de lineas (i, j) NO usadas en paneles L
3. Filtros para considerar par como muro E/F:
   a. Lineas paralelas (vectores direccion casi iguales o opuestos, tolerancia angular)
   b. Distancia perpendicular entre OFFSET_MINIMO_PANEL (50mm) y OFFSET_MAXIMO_PANEL (1500mm)
   c. Solapamiento longitudinal (proyeccion sobre eje del muro mayor que cero)
   d. Los 4 endpoints son extremos libres (no conectan con ninguna otra linea, TOL_CONEXION=1mm)
4. Si pasa filtros, calcular los 4 vertices del rectangulo emparejando endpoints por lado:
   - A0 (cerca de origen) <-> B0 (cerca de origen)
   - A1 (extremo lejano) <-> B1 (extremo lejano)
   - Vertices: [A0, A1, B1, B0] cerrado
5. Emitir 2 PolilineaDTO (patron US-679: ObjetoDB2d + ModelDesing 2700mm) + 4 cuadrados marcadores (T1)

**Archivos esperados:**
- Desing/Services/LCornerDetector.cs (metodo GenerarMurosLibresAislados + helpers)
- Sin cambios en plugin (DTOs ya cubiertos en T5/T1)

**Predecesoras:** US-688 T5 (#693), T6 (#694), T1 (#689)
**Referencia:** C:\temp\Muro_Recto3.png (E,F) y C:\temp\Muro_Recto 1 esquina.png'

$taskBody = @(
    @{ op = 'add'; path = '/fields/System.Title'; value = $title },
    @{ op = 'add'; path = '/fields/System.Description'; value = $desc },
    @{
        op    = 'add'
        path  = '/relations/-'
        value = @{
            rel = 'System.LinkTypes.Hierarchy-Reverse'
            url = "https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workItems/$usId"
        }
    }
)

$taskJson = ConvertTo-Json $taskBody -Depth 10
$taskResponse = Invoke-RestMethod -Uri 'https://dev.azure.com/VSCAD/tandem2026/_apis/wit/workitems/$Task?api-version=7.1' -Method Post -Headers $headers -Body $taskJson
Write-Output ("Task #{0} creada bajo US #{1}" -f $taskResponse.id, $usId)
Write-Output ("URL: https://dev.azure.com/VSCAD/tandem2026/_workitems/edit/{0}" -f $taskResponse.id)
