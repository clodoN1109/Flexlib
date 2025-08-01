function Interface  {

    param (
        [string]$Cmd,
        [string[]]$TestsList,
        [switch]$NoClearHost,
        [switch]$UpdateReferences
    ) 

    $projectRoot = "C:\Users\clovi\OneDrive\Área de Trabalho\Clodo\Work\Projects\Incubator\FlexLib"
    $devCommands = "$projectRoot/Dev/_commands"
    $testCommandDir = "$devCommands/test"

    . "$testCommandDir/application.ps1"
    . "$testCommandDir/utils.ps1"

    $TestsFiles = Get-ChildItem -Path "$PSScriptRoot/tests" -Recurse -Filter "*.ps1"

    if (-not $NoClearHost) {
        Clear-Host
    }

    switch ($Cmd) {
        "help" {
            ShowUsage $TestsFiles
        }

        "run" {
            $selectedTests =
                if ($TestsList.Length -eq 0) {
                    $TestsFiles
                } else {
                    Get-TestFilesByNames -names $TestsList -testsFiles $TestsFiles
                }

                Run-Tests $selectedTests -UpdateReferences:$UpdateReferences
        }

        default {
            Write-Host "❌ Unknown command '$Cmd'. Use 'help' for usage." -ForegroundColor Red
            exit 1
        }
    }
}
