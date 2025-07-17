param (
    [switch]$WithRuntimeTests
)

. "$PSScriptRoot\build\LogHandler.ps1"
. "$PSScriptRoot\build\BuildProcess.ps1"

$env:DOTNET_CLI_UI_LANGUAGE = "en"

$LogStream = ExecuteBuildProcess 

$errorCount, $warningCount = HandleLog $LogStream

if (($WithRuntimeTests) -and ($errorCount -eq 0) -and ($warningCount -eq 0)) {
    Write-Host "`nBUILD PROCESS COMPLETED SUCCESSFULLY"
    Write-Host "`nBUILD Nº X"
    Start-Sleep 1
    Write-Host "`nInitiating runtime test suite...`n" -ForegroundColor DarkGreen
    Start-Sleep 2
    & "$PSScriptRoot/test.ps1" run
}
