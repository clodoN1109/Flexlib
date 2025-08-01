param (
    [string]$Cmd = 'help',
    [string[]]$TestsList = @(),
    [switch]$NoClearHost,
    [switch]$UpdateReferences
)

$projectRoot = "C:\Users\clovi\OneDrive\Área de Trabalho\Clodo\Work\Projects\Incubator\FlexLib"
$devCommands = "$projectRoot/Dev/_commands"
$testCommandDir = "$devCommands/test"

. "$testCommandDir/interface.ps1" 

Interface $Cmd $TestsList -NoClearHost:$NoClearHost -UpdateReferences:$UpdateReferences

