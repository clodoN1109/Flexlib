function Interface ([string]$Cmd, [string[]]$TestsList) {
    . "$PSScriptRoot/application.ps1"
    . "$PSScriptRoot/utils.ps1"

    $TestsFiles = Get-ChildItem -Path "$PSScriptRoot/tests" -Recurse -Filter "*.ps1"

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

            Run-Tests $selectedTests
        }

        default {
            Write-Host "❌ Unknown command '$Cmd'. Use 'help' for usage." -ForegroundColor Red
            exit 1
        }
    }
}

