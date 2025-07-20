param (
    [switch]$WithRuntimeTests,
    [string]$Configuration = "Debug"
)

. "$PSScriptRoot\build\LogHandler.ps1"
. "$PSScriptRoot\build\BuildProcess.ps1"
. "$PSScriptRoot\build\BuildHistory.ps1"
. "$PSScriptRoot\build\Utils.ps1"

$env:DOTNET_CLI_UI_LANGUAGE = "en"

$LogStream = ExecuteBuildProcess $Configuration

$errorCount, $warningCount = HandleLog $LogStream
    
$newEntry = SaveBuildHistory $Configuration $ErrorCount $WarningCount

$buildID = $newEntry.id

Write-Fill "BUILD Nº $buildID COMPLETED"

if (($configuration -eq "Debug") -and ($WithRuntimeTests) -and ($errorCount -eq 0) -and ($warningCount -eq 0)) {
     
    Start-Sleep 1

    & "$PSScriptRoot/test.ps1" run

}

$ResultRequestedByAnotherScript = ( $MyInvocation.ScriptName -ne "" )

if ($ResultRequestedByAnotherScript) {
    return $newEntry;
}

