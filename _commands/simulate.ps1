param (
    [string]$SimulationName = '',
    [switch]$ClearLastResults,
    [switch]$Help
)

. "$PSScriptRoot\simulate\Utils.ps1"
. "$PSScriptRoot\simulate\Help.ps1"

# Resolve paths
$flexlibPath            = Resolve-Path "$PSScriptRoot/flexlib.ps1"
$flexlibDataPath        = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"
$dataPath               = "$PSScriptRoot/simulate/data"  
$resultsPath            = "$dataPath/results"
$simulateCommandDir     = "$PSScriptRoot/simulate"
$simulationsPath        = "$simulateCommandDir/simulations"
$requestedSimulation    = "$simulationsPath/$SimulationName.ps1"

$ProgressPreference = 'SilentlyContinue'

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
Write-Host "     $SimulationName"
Write-Host ''

# Execute selected simulation
if (Test-Path $requestedSimulation) {

    & $requestedSimulation

} else {

    Write-Host ( "`n░░░░ SIMULATION $SimulationName NOT FOUND" + "░" * ( [System.Console]::WindowWidth - 26 - $SimulationName.Length)) -ForegroundColor Red

}

Write-Host ""
Write-Host ("░" * [System.Console]::WindowWidth )
