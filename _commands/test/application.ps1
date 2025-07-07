function Get-TestFilesByNames($names, $testsFiles) {
    return $testsFiles | Where-Object {
        $testName = ($_.BaseName -split "-", 2)[1]
        $names -contains $testName
    }
}

function Run-Test($test) {
    $displayName = ($test.BaseName -split "-", 2)[1]
    Write-Host -NoNewline "❔  $displayName"
    Start-Sleep -Milliseconds 300

    try {
        $result = & $test.FullName
        if ($result -eq $true) {
            Write-Host "`r✅  $displayName" -ForegroundColor Green
        } else {
            Write-Host "`r❌  $displayName" -ForegroundColor Red
        }
    } catch {
        Write-Host "`r💥 Error in ${displayName}: $_" -ForegroundColor Red
    }

    Write-Host ('-' * [System.Console]::WindowWidth)
}

function Run-Tests($selectedTests) {
    Clear-Host
    Write-Host ('=' * [System.Console]::WindowWidth)
    Write-Host "`n  TEST SUITE`n"
    Write-Host ('=' * [System.Console]::WindowWidth)

    foreach ($test in $selectedTests) {
        Run-Test $test
    }
}

function Compare-Folders {
    param (
        [string]$Expected,
        [string]$Actual
    )

    $expectedItems = Get-ChildItem $Expected -Recurse

    $hasDifferences = $false
    foreach ($item in $expectedItems) {

        $relativePath = $item.FullName.Substring($Expected.Length).TrimStart('\', '/')     
        $actualPath = Join-Path $Actual $relativePath

        if (!(Test-Path $actualPath)) {
            $hasDifferences = $true 
            continue
        }

    }
    if ($hasDifferences) {
        return $true
    }

    return $false
}

function ShowUsage($testsFiles) {
    Write-Host "`nUsage: .\test.ps1 run [<test name 1>, <test name 2>, ...]" -ForegroundColor Yellow
    Write-Host "       .\test.ps1 help`n"
    Write-Host "Available tests:" -ForegroundColor Green

    foreach ($test in $testsFiles) {
        $parts = $test.BaseName -split "-", 2
        if ($parts.Length -eq 2) {
            Write-Host "  $($parts[1])"
        } else {
            Write-Host "  $($test.BaseName)"
        }
    }

    exit 0
}

