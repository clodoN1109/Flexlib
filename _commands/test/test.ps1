param (
    [string]$Cmd = 'help',
    [string[]]$TestsList = @(),
    [switch]$NoClearHost
)

. "$PSScriptRoot/interface.ps1" 

Interface $Cmd $TestsList -NoClearHost:$NoClearHost

