param (
    [switch]$Clear
)

. "$PSScriptRoot\simulate\utils.ps1"

if ($Clear){

    & "$PSScriptRoot\simulate\ClearSimulation.ps1"

}
else{

    & "$PSScriptRoot\simulate\LibraryBuilding.ps1"

}

