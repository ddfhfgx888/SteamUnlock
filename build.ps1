$ErrorActionPreference = 'Stop'
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$assets = Join-Path $PSScriptRoot 'vendor\OpenSteamTools'
$source = Join-Path $PSScriptRoot 'src\SteamHelper.cs'
$dist = Join-Path $PSScriptRoot 'dist'
$output = Join-Path $dist 'SteamHelper.exe'

$requiredFiles = @('OpenSteamTool.dll', 'dwmapi.dll', 'xinput1_4.dll')
if (-not (Test-Path $csc)) { throw "C# compiler not found: $csc" }
if (-not (Test-Path $source)) { throw "Source file not found: $source" }
foreach ($file in $requiredFiles) {
    $path = Join-Path $assets $file
    if (-not (Test-Path $path)) { throw "Required vendor file not found: $path" }
}

New-Item -ItemType Directory -Path $dist -Force | Out-Null

& $csc /nologo /target:exe /platform:anycpu /optimize+ /out:$output `
    /reference:System.Web.Extensions.dll `
    /reference:System.IO.Compression.dll `
    /reference:System.IO.Compression.FileSystem.dll `
    "/resource:$assets\OpenSteamTool.dll,Embedded.OpenSteamTool.dll" `
    "/resource:$assets\dwmapi.dll,Embedded.dwmapi.dll" `
    "/resource:$assets\xinput1_4.dll,Embedded.xinput1_4.dll" `
    $source

if ($LASTEXITCODE -ne 0) { throw "C# compiler exited with code $LASTEXITCODE" }
Write-Host "Built: $output"
