param (
    [string]$SimulationName = '',
    [switch]$ClearLastResults,
    [switch]$Help,
    [ValidateSet("Debug", "Release", "Production")]
    [string]$Mode = "Debug",
    [string]$Version
)

if (($Mode -eq 'Release') -and (-not $Version)) {
    Write-Host "`n❌ You must specify a version in Release mode." -ForegroundColor Red
    return
}

. "$PSScriptRoot/simulate/Utils.ps1"
. "$PSScriptRoot/simulate/Help.ps1"
. "$PSScriptRoot/simulate/ResolvePaths.ps1"



if ($ClearLastResults) { 

    Write-Host ( "`n░░░░ CLEARING SIMULATION RESULTS " + "░" * ( [System.Console]::WindowWidth - 33))
    
    & "$simulateCommandDir\ClearSimulation.ps1"
    
    return
}

if ($Help -or ($SimulationName -eq '') ) {
    ExplainUsage
    return
}

Write-Host ( "`n░░░░ SIMULATION " + "░" * ( [System.Console]::WindowWidth - 16))
Write-Host ''
Write-Host "     $SimulationName `($($Mode.ToUpper()) mode`)"

# Execute selected simulation
if (Test-Path $requestedSimulation) {

    & $requestedSimulation

} else {

    Write-Host ( "`n░░░░ SIMULATION $SimulationName NOT FOUND " + "░" * ( [System.Console]::WindowWidth - 26 - $SimulationName.Length)) -ForegroundColor Red

}

Write-Host ""
Write-Host ("░" * [System.Console]::WindowWidth )

if ($Mode -eq 'Release') {

    try {
        Safe-Remove "$tempDir"
    } catch {
        Write-Host "⚠️ Failed to remove temporary directory: $tempDir" -ForegroundColor Yellow
    }

}
