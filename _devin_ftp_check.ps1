$ErrorActionPreference = 'Continue'
$alive = [bool](Get-Process -Id 17768 -ErrorAction SilentlyContinue)
Write-Output ("PROC17768_ALIVE=" + $alive)
$pass = (Get-Content 'C:\00_Tandem2026\_ftp_develop_pass.txt' -Raw).Trim()
$cred = New-Object System.Net.NetworkCredential('ftpdevelop', $pass)
$base = 'ftp://win8236.site4now.net:21/'
function ListCount($path) {
    try {
        $req = [System.Net.FtpWebRequest]::Create($base + $path)
        $req.Credentials = $cred
        $req.Method = [System.Net.WebRequestMethods+Ftp]::ListDirectory
        $req.UsePassive = $true; $req.KeepAlive = $false; $req.Timeout = 30000
        $resp = $req.GetResponse()
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $n = 0; $names = @()
        while (($l = $sr.ReadLine()) -ne $null) { $n++; if ($names.Count -lt 12) { $names += $l } }
        $sr.Close(); $resp.Close()
        Write-Output ("LIST " + $path + " count=" + $n + " :: " + ($names -join ', '))
    } catch { Write-Output ("LIST " + $path + " FAIL " + $_.Exception.Message) }
}
function Exists($path) {
    try {
        $req = [System.Net.FtpWebRequest]::Create($base + $path)
        $req.Credentials = $cred
        $req.Method = [System.Net.WebRequestMethods+Ftp]::GetFileSize
        $req.UsePassive = $true; $req.KeepAlive = $false; $req.Timeout = 30000
        $resp = $req.GetResponse(); $resp.Close()
        Write-Output ("EXISTS " + $path)
    } catch { Write-Output ("MISSING " + $path) }
}
ListCount ''
ListCount 'bin'
Exists 'Web.config'
Exists 'bin/Desing.dll'
Exists 'Scripts/MasterArticles/master-article-details-stl-viewer.js'
