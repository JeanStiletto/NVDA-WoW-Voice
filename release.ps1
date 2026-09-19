# Build NvdaWowVoiceSetup.exe and publish it as a GitHub release.
#
#   .\release.ps1 -Notes "what changed"          publish the version in the csproj
#   .\release.ps1 -Notes "..." -DryRun           build + show what would happen
#
# The release is marked Latest, so this link always serves the newest exe:
#   https://github.com/JeanStiletto/NVDA-WoW-Voice/releases/latest/download/NvdaWowVoiceSetup.exe
param(
    [Parameter(Mandatory = $true)][string]$Notes,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$Repo = 'JeanStiletto/NVDA-WoW-Voice'
$Csproj = Join-Path $PSScriptRoot 'NvdaWowVoice.csproj'
$Exe = Join-Path $PSScriptRoot 'bin\Release\net48\NvdaWowVoiceSetup.exe'

$version = ([xml](Get-Content $Csproj -Raw)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "No <Version> in $Csproj" }
$tag = "v$version"
Write-Host "Version $version (tag $tag)"

# The payload is a copy of the Sku installer's. When this folder sits in the Sku
# workspace, warn if the two have drifted apart.
$skuPayload = Join-Path $PSScriptRoot '..\SkuInstaller\payload\sapi2sr-payload.zip'
if (Test-Path $skuPayload) {
    $a = (Get-FileHash (Join-Path $PSScriptRoot 'payload\sapi2sr-payload.zip')).Hash
    $b = (Get-FileHash $skuPayload).Hash
    if ($a -ne $b) { Write-Warning 'payload\sapi2sr-payload.zip differs from the Sku installer payload. Copy it over if the Sku one is newer.' }
    else { Write-Host 'Payload matches the Sku installer payload.' }
}

# The readable signing script must be the one inside the payload.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $PSScriptRoot 'payload\sapi2sr-payload.zip'))
try {
    $reader = New-Object System.IO.StreamReader(($zip.Entries | Where-Object FullName -eq 'sku-nvda-voice-sign.ps1').Open())
    $inZip = $reader.ReadToEnd(); $reader.Dispose()
} finally { $zip.Dispose() }
$onDisk = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot 'signing\sku-nvda-voice-sign.ps1'))
if ($inZip -ne $onDisk) { throw 'signing\sku-nvda-voice-sign.ps1 differs from the copy inside the payload. Re-extract it.' }

& dotnet build $Csproj -c Release -nologo -clp:NoSummary
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

# "release not found" is an expected answer here. In PowerShell 5.1 a native
# command's stderr becomes a terminating error under 'Stop', so relax it.
$ErrorActionPreference = 'Continue'
& gh release view $tag --repo $Repo *> $null
$exists = ($LASTEXITCODE -eq 0)
$ErrorActionPreference = 'Stop'

if ($DryRun) {
    if ($exists) { Write-Host "DRY RUN: would upload $Exe to existing release $tag (--clobber)" }
    else { Write-Host "DRY RUN: would create release $tag with $Exe, marked Latest" }
    return
}

if ($exists) {
    & gh release upload $tag $Exe --repo $Repo --clobber
} else {
    & gh release create $tag $Exe --repo $Repo --title "NVDA Voice for WoW $version" --notes $Notes --latest
}
if ($LASTEXITCODE -ne 0) { throw 'gh release failed.' }
Write-Host "Done. https://github.com/$Repo/releases/latest/download/NvdaWowVoiceSetup.exe"
