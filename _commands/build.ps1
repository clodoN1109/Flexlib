param (
    [switch]$WithRuntimeTests,
    [string]$Configuration = "Debug",
    [switch]$NoClearHost,
    [switch]$HistoryGraph,
    [string]$Version = "",
    [ValidateSet(
        "win-x64",
        "win-arm64",
        "linux-x64",
        "linux-arm64",
        "osx-x64",
        "osx-arm64"
    )]
    [string[]]$RuntimeIds = @([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
)


. "$PSScriptRoot\build\LogHandler.ps1"
. "$PSScriptRoot\build\BuildProcess.ps1"
. "$PSScriptRoot\build\BuildHistory.ps1"
. "$PSScriptRoot\build\Utils.ps1"

$BuildHistoryPath = "$PSScriptRoot\..\builds\builds.json"

if (-not $NoClearHost) {
    Clear-Host
}

if ($HistoryGraph) {
    $history = GetBuildHistory 

    Write-Fill "BUILD STATS" -ForegroundColor Cyan
    PlotHistoryGraph $history
    Write-Fill "END - BUILD STATS" -ForegroundColor Cyan

    return
}

Write-Fill "BUILD" -ForegroundColor Cyan
$env:DOTNET_CLI_UI_LANGUAGE = "en"

$buildID = DetermineBuildID $BuildHistoryPath
$allLogs = @()

foreach ($RuntimeId in $RuntimeIds) {

    $BuildArtifactsPath = if ($Configuration -eq 'Debug') {
        "$PSScriptRoot\..\builds\last\Debug\net8.0\$RuntimeId"
    } else {
        "$PSScriptRoot\..\builds\last\Release\net8.0\$RuntimeId"
    }

    $buildSize = 0
    if (Test-Path $BuildArtifactsPath) {
        $buildSize = Get-ChildItem -Path $BuildArtifactsPath -Recurse -File |
                     Measure-Object -Property Length -Sum |
                     Select-Object -ExpandProperty Sum
    } else {
        Write-Warning "Build artifacts path not found: $BuildArtifactsPath"
        $buildSize = 0
    }

    $LogStream = ExecuteBuildProcess -Configuration $Configuration -BuildId $buildID -Version $Version -RuntimeIds @($RuntimeId)
    $allLogs += $LogStream

    $errorCount, $warningCount = HandleLog $LogStream

    $newEntry = SaveBuildHistory $Configuration $buildID $ErrorCount $WarningCount $buildSize
    $buildID = $newEntry.id

    if ($errorCount -eq 0) {
        Write-Fill "BUILD Nº $buildID for $RuntimeId COMPLETED"
    } else {
        Write-Fill "BUILD Nº $buildID for $RuntimeId FAILED" -ForegroundColor Red
    }

    if (($Configuration -eq "Debug") -and $WithRuntimeTests -and ($errorCount -eq 0) -and ($warningCount -eq 0)) {
        Start-Sleep 1
        & "$PSScriptRoot/test.ps1" -Cmd run -NoClearHost
    }
}

$ResultRequestedByAnotherScript = ($MyInvocation.ScriptName -ne "")

if ($ResultRequestedByAnotherScript) {
    return $newEntry
}

Write-Fill "END - BUILD" -ForegroundColor Cyan
