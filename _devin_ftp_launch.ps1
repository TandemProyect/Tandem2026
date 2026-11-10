$ErrorActionPreference = 'Continue'
$running = Get-CimInstance Win32_Process -Filter "Name='powershell.exe'" | Where-Object { $_.CommandLine -match '_devin_ftp_upload' }
if ($running) {
    Write-Output ('RUNNING PID=' + ($running.ProcessId -join ','))
} else {
    $p = Start-Process -FilePath 'powershell' -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','C:\00_Tandem2026\_devin_ftp_upload.ps1' -RedirectStandardOutput 'C:\00_Tandem2026\_ftp_upload.log' -RedirectStandardError 'C:\00_Tandem2026\_ftp_upload.err' -WindowStyle Hidden -PassThru
    Write-Output ('STARTED PID=' + $p.Id)
}
if (Test-Path 'C:\00_Tandem2026\_ftp_upload.log') {
    Write-Output '--- LOG TAIL ---'
    Get-Content 'C:\00_Tandem2026\_ftp_upload.log' -Tail 8
}
