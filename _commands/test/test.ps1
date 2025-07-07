param (
    [string]$Cmd = 'help',
    [string[]]$TestsList = @()
)

. "$PSScriptRoot/interface.ps1"

Interface $Cmd $TestsList

