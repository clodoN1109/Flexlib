param (
    [string]$Cmd = 'help',
    [string[]]$TestsList = @(),
    [switch]$NoClearHost,
    [switch]$UpdateReferences
)

$projectRoot = "C:\Users\clovi\OneDrive\Área de Trabalho\Clodo\Work\Projects\Incubator\FlexLib"
$devCommands = "$projectRoot/Dev/_commands"
$testCommandDir = "$devCommands/test"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/win-x64/data/"

. "$testCommandDir/interface.ps1" 

Interface $Cmd $TestsList -NoClearHost:$NoClearHost -UpdateReferences:$UpdateReferences

