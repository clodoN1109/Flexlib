param (
    [switch]$WithRuntimeTests,
    [string]$Configuration = "Debug",
    [switch]$NoClearHost,
    [switch]$PlotHistoryGraph,
    [string]$Version = ""
)

. "$PSScriptRoot\build\LogHandler.ps1"
. "$PSScriptRoot\build\BuildProcess.ps1"
. "$PSScriptRoot\build\BuildHistory.ps1"
. "$PSScriptRoot\build\Utils.ps1"

$BuildHistoryPath = "$PSScriptRoot\..\builds\builds.json"

if (-not $NoClearHost) {
    Clear-Host
}

Write-Fill "BUILD" -ForegroundColor Cyan

$env:DOTNET_CLI_UI_LANGUAGE = "en"

$buildID = DetermineBuildID $BuildHistoryPath

$LogStream = ExecuteBuildProcess $Configuration $buildID $Version

$errorCount, $warningCount = HandleLog $LogStream
    
$newEntry = SaveBuildHistory $Configuration $buildID $ErrorCount $WarningCount

$buildID = $newEntry.id

if ( $errorCount -eq 0) {
    
    Write-Fill "BUILD Nº $buildID COMPLETED"

} else {

    Write-Fill "BUILD Nº $buildID FAILED" -ForegroundColor Red

}

if ($PlotHistoryGraph) 
{
    $history = GetBuildHistory 

    Write-Fill "BUILD HISTORY" -ForegroundColor Cyan
    PlotHistoryGraph $history
    Write-Fill "END" -ForegroundColor Cyan
}

if (($configuration -eq "Debug") -and ($WithRuntimeTests) -and ($errorCount -eq 0) -and ($warningCount -eq 0)) {
     
    Start-Sleep 1

    & "$PSScriptRoot/test.ps1" -Cmd run -NoClearHost

}

$ResultRequestedByAnotherScript = ( $MyInvocation.ScriptName -ne "" )

if ($ResultRequestedByAnotherScript) {
    return $newEntry;
}

