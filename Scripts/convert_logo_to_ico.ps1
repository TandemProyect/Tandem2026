param(
    [string]$PngPath = "C:\Users\jag\.cursor\projects\c-00-Tandem2026\assets\c__Users_jag_AppData_Roaming_Cursor_User_workspaceStorage_empty-window_images_Logo-87739fff-a047-45f2-a46a-23dd79b78620.png",
    [string]$IcoPath = "C:\Users\jag\.cursor\projects\c-00-Tandem2026\assets\logo.ico"
)

if (-not (Test-Path -LiteralPath $PngPath)) {
    throw "PNG no encontrado: $PngPath"
}

$null = Add-Type -AssemblyName System.Drawing -ErrorAction SilentlyContinue
$null = Add-Type -AssemblyName System.Drawing.Common -ErrorAction SilentlyContinue

$img = [System.Drawing.Image]::FromFile($PngPath)
$bmp = New-Object System.Drawing.Bitmap($img)
$bmp.Save($IcoPath, [System.Drawing.Imaging.ImageFormat]::Icon)

$img.Dispose()
$bmp.Dispose()

Write-Output "OK: $IcoPath"

