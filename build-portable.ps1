[CmdletBinding()]
param(
    [string]$Version = '1.0.2'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$stage = Join-Path $projectRoot "dist\DSH-Tray-Launcher-v$Version-win-x64-portable"
$zip = "$stage.zip"

if ((Test-Path -LiteralPath $stage) -or (Test-Path -LiteralPath $zip)) {
    throw "Output already exists. Remove it explicitly before rebuilding: $stage"
}

$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$publish = Join-Path $stage 'app'
$publishArgs = @(
    'publish'
    (Join-Path $projectRoot 'tray\dsh-tray.csproj')
    '-c', 'Release'
    '-r', 'win-x64'
    '--self-contained', 'true'
    '-p:PublishSingleFile=true'
    '-p:IncludeNativeLibrariesForSelfExtract=true'
    "-p:Version=$Version"
    "-p:AssemblyVersion=$Version.0"
    "-p:FileVersion=$Version.0"
    '-o', $publish
)

& $dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed: $LASTEXITCODE" }

Move-Item -LiteralPath (Join-Path $publish 'dsh-tray.exe') -Destination $stage
Remove-Item -LiteralPath $publish
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'THIRD_PARTY_NOTICES.md') -Destination $stage
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal

$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zip).Hash.ToLowerInvariant()
"$hash *$(Split-Path -Leaf $zip)" | Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
Get-FileHash -Algorithm SHA256 -LiteralPath $zip
