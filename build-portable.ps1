[CmdletBinding()]
param(
    [string]$Version = '1.0.7-beta.1',
    [ValidateSet('portable', 'light')]
    [string]$Flavor = 'portable'
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$suffix = if ($Flavor -eq 'portable') { 'portable-compressed' } else { 'light-requires-dotnet10' }
$stage = Join-Path $projectRoot "dist\DSH-Tray-Launcher-v$Version-win-x64-$suffix"
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
    '--self-contained', $(if ($Flavor -eq 'portable') { 'true' } else { 'false' })
    "-p:EnableCompressionInSingleFile=$($Flavor -eq 'portable')"
    '-p:PublishSingleFile=true'
    '-p:IncludeNativeLibrariesForSelfExtract=true'
    "-p:Version=$Version"
    "-p:AssemblyVersion=$($Version.Split('-')[0]).0"
    "-p:FileVersion=$($Version.Split('-')[0]).0"
    '-o', $publish
)

& $dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed: $LASTEXITCODE" }

Move-Item -LiteralPath (Join-Path $publish 'dsh-tray.exe') -Destination $stage
Remove-Item -LiteralPath $publish
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'BETA-VALIDATION.md') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'THIRD_PARTY_NOTICES.md') -Destination $stage
$description = if ($Flavor -eq 'portable') {
    '便携版（内置 .NET）：无需安装 .NET。仍需 Node.js/npm 和 DSH。'
} else {
    '轻量版：需要 .NET 10 Desktop Runtime x64（安装最新 10.0.x）。普通 .NET Runtime 不足以运行 WinForms。仍需 Node.js/npm 和 DSH。缺失运行时由 .NET 启动器提示；也可打开随包下载链接。'
}
$description | Set-Content -LiteralPath (Join-Path $stage '版本说明.txt') -Encoding utf8
if ($Flavor -eq 'light') {
    "[InternetShortcut]`r`nURL=https://dotnet.microsoft.com/en-us/download/dotnet/10.0" | Set-Content -LiteralPath (Join-Path $stage '安装.NET10桌面运行时.url') -Encoding ascii
}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal

$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zip).Hash.ToLowerInvariant()
"$hash *$(Split-Path -Leaf $zip)" | Set-Content -LiteralPath "$zip.sha256" -Encoding ascii
Get-FileHash -Algorithm SHA256 -LiteralPath $zip
