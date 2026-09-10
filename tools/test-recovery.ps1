param([switch]$Install)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $root 'tray/bin/Release/net10.0-windows/win-x64/dsh-tray.exe'
$run = Join-Path $root ('validation/' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
foreach ($mode in @('contract-tests', 'test-core', 'test-fallback')) {
    $homePath = Join-Path $run $mode
    New-Item -ItemType Directory -Path $homePath -Force | Out-Null
    if ($mode -eq 'test-fallback') {
        $profileDir = Join-Path $homePath 'profiles/web'
        New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
        '{"name":"broken-fixture","private":true,"dependencies":{},"dsh":{"profile":{"bundles":["fixture-missing-plugin"]}}}' |
            Set-Content -LiteralPath (Join-Path $profileDir 'package.json') -Encoding utf8
    }
    $psi = [Diagnostics.ProcessStartInfo]::new($exe)
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.ArgumentList.Add("--$mode")
    $psi.ArgumentList.Add($homePath)
    $proc = [Diagnostics.Process]::Start($psi)
    $proc.WaitForExit()
    Get-Content -LiteralPath (Join-Path $homePath 'tray-test/result.json')
    if ($proc.ExitCode -ne 0) { throw "$mode failed: $homePath" }
}
if ($Install) {
    $psi = [Diagnostics.ProcessStartInfo]::new($exe)
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    foreach ($item in @('--test-install', (Join-Path $run 'install'), '0.1.5-rc.1')) { $psi.ArgumentList.Add($item) }
    $proc = [Diagnostics.Process]::Start($psi)
    $proc.WaitForExit()
    Get-Content -LiteralPath (Join-Path $run 'install/tray-test/result.json')
    if ($proc.ExitCode -ne 0) { throw 'Install transaction failed' }
}
Write-Output "Evidence: $run"
