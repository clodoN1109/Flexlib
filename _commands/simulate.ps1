param (
    [switch]$Clear
)

. "$PSScriptRoot\simulate\utils.ps1"

if ($Clear){

    Write-Host ( "`n░░░░ CLEARING SIMULATION " + "░" * ( [System.Console]::WindowWidth - 25))
    
    & "$PSScriptRoot\simulate\ClearSimulation.ps1"

}
else{

    Write-Host ( "`n░░░░ SIMULATION " + "░" * ( [System.Console]::WindowWidth - 16))
    Write-Host "`n  Library Building "
    
    & "$PSScriptRoot\simulate\LibraryBuilding.ps1"

}

Write-Host ""
Write-Host ("░" * [System.Console]::WindowWidth )
