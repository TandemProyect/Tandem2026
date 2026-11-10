$ErrorActionPreference = 'Stop'
$pass = (Get-Content 'C:\00_Tandem2026\_ftp_develop_pass.txt' -Raw).Trim()
$cred = New-Object System.Net.NetworkCredential('ftpdevelop', $pass)
$base = 'ftp://win8236.site4now.net:21/'
foreach ($ssl in @($false, $true)) {
    try {
        $req = [System.Net.FtpWebRequest]::Create($base)
        $req.Credentials = $cred
        $req.Method = [System.Net.WebRequestMethods+Ftp]::ListDirectory
        $req.UsePassive = $true
        $req.KeepAlive = $false
        $req.EnableSsl = $ssl
        $req.Timeout = 30000
        $resp = $req.GetResponse()
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $i = 0
        Write-Output ("SSL=" + $ssl)
        while (($l = $sr.ReadLine()) -ne $null -and $i -lt 20) { Write-Output ("  " + $l); $i++ }
        $sr.Close(); $resp.Close()
        Write-Output 'FTP_LOGIN_OK'
        exit 0
    } catch {
        Write-Output ("SSL=" + $ssl + " FAIL: " + $_.Exception.Message)
    }
}
exit 1
