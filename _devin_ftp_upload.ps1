$ErrorActionPreference = 'Continue'
$pass = (Get-Content 'C:\00_Tandem2026\_ftp_develop_pass.txt' -Raw).Trim()
$cred = New-Object System.Net.NetworkCredential('ftpdevelop', $pass)
$localBase = 'C:\00_Tandem2026\artifacts\design_site4now'
$remoteBase = 'ftp://win8236.site4now.net:21/'

foreach ($del in @('index.html', '.env')) {
    try {
        $req = [System.Net.FtpWebRequest]::Create($remoteBase + $del)
        $req.Credentials = $cred
        $req.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
        $req.UsePassive = $true; $req.KeepAlive = $false; $req.Timeout = 30000
        $resp = $req.GetResponse(); $resp.Close()
        Write-Output ("DELETED " + $del)
    } catch { Write-Output ("DELETE_SKIP " + $del) }
}

$files = Get-ChildItem $localBase -Recurse -File
$total = $files.Count
$i = 0; $ok = 0; $fail = 0
$failList = New-Object System.Collections.Generic.List[string]
$createdDirs = @{}
$sw = [System.Diagnostics.Stopwatch]::StartNew()
foreach ($f in $files) {
    $i++
    $rel = $f.FullName.Substring($localBase.Length).TrimStart('\', '/') -replace '\\', '/'
    $url = $remoteBase + $rel
    try {
        $dirRel = [System.IO.Path]::GetDirectoryName($rel)
        if ($dirRel) {
            $cur = ''
            foreach ($part in ($dirRel -split '/')) {
                $cur = if ($cur) { $cur + '/' + $part } else { $part }
                if (-not $createdDirs.ContainsKey($cur)) {
                    $dirUrl = $remoteBase + $cur + '/'
                    try {
                        $dreq = [System.Net.FtpWebRequest]::Create($dirUrl)
                        $dreq.Credentials = $cred
                        $dreq.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
                        $dreq.UsePassive = $true
                        $dreq.KeepAlive = $false
                        $dreq.Timeout = 20000
                        $dresp = $dreq.GetResponse()
                        $dresp.Close()
                    } catch { }
                    $createdDirs[$cur] = $true
                }
            }
        }
        $req = [System.Net.FtpWebRequest]::Create($url)
        $req.Credentials = $cred
        $req.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
        $req.UseBinary = $true
        $req.UsePassive = $true
        $req.KeepAlive = $false
        $req.Timeout = 180000
        $fs = [System.IO.File]::OpenRead($f.FullName)
        try {
            $rs = $req.GetRequestStream()
            $fs.CopyTo($rs)
            $rs.Close()
        } finally { $fs.Close() }
        $resp = $req.GetResponse()
        $resp.Close()
        $ok++
    } catch {
        $fail++
        $failList.Add($rel + ' :: ' + $_.Exception.Message)
    }
    if (($i % 100) -eq 0) {
        $mb = [math]::Round(($sw.Elapsed.TotalMinutes),1)
        Write-Output ("PROGRESS $i/$total ok=$ok fail=$fail min=$mb")
    }
}
$sw.Stop()
Write-Output ("UPLOAD_DONE total=$total ok=$ok fail=$fail minutes=" + [math]::Round($sw.Elapsed.TotalMinutes,1))
foreach ($fl in $failList) { Write-Output ("FAIL " + $fl) }
