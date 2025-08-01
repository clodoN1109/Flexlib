function Get-TestFilesByNames($names, $testsFiles) {
    return $testsFiles | Where-Object {
        $testName = ($_.BaseName -split "-", 2)[1]
        $names -contains $testName
    }
}

function Run-Test() {

    param (
        
        $test,
        [switch]$UpdateReferences,
        $arrowHeadColumn = 25
        
    )
 
    $displayName = ($test.BaseName -split "-", 2)[1]
    $reserveWidth = $displayName.Length + 5
    $arrowLength = $arrowHeadColumn - $reserveWidth
    Write-Host -NoNewline "❔  $displayName"
    Start-Sleep -Milliseconds 300

    try {
        if ($UpdateReferences) {
            ArrowMessage -Message "Updating references" `
                         -ColumnStart $reserveWidth `
                         -ArrowLength $arrowLength `
                         -ArrowColor Yellow `
                         -MessageColor Yellow `
                         -ReturnToLastPosition

            $result = & $test.FullName -UpdateReferences:$true    

            ArrowMessage -Message "Updated              " `
                         -ColumnStart $reserveWidth `
                         -ArrowLength $arrowLength `
                         -ArrowColor Green `
                         -MessageColor Green `
                         -ReturnToLastPosition

        } else {
            $result = & $test.FullName
        }
        Start-Sleep -Milliseconds 500
        if ($result -eq $true) {
            Write-Host "`r✅  $displayName" -ForegroundColor Green
        } else {
            Write-Host "`r❌  $displayName" -ForegroundColor Red
        }
    } catch {
        Write-Host "`r💥 Error in ${displayName}: $_" -ForegroundColor Red
    }
    Write-Host ('-' * [System.Console]::WindowWidth) -ForegroundColor DarkGray
}

function Run-Tests() {
    
    param (
        $selectedTests,
        [switch]$UpdateReferences
    )

    [System.Console]::CursorVisible = $false
    
    Write-Fill "USE CASE TESTING" -ForegroundColor Cyan
    Write-Host ('-' * [System.Console]::WindowWidth) -ForegroundColor DarkGray
    Start-Sleep 1
    
    $maxBaseNameLength = $selectedTests | 
    ForEach-Object { $_.BaseName.Length } | 
    Measure-Object -Maximum | 
    Select-Object -ExpandProperty Maximum

    foreach ($test in $selectedTests) {
        if ($UpdateReferences) {
            Run-Test $test -UpdateReferences:$true -ArrowHeadColumn ($maxBaseNameLength + 10)
        } else {
            Run-Test $test
        }
    }

    Write-Fill "END - USE CASE TESTING" -ForegroundColor Cyan
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
    Write-Host "`nUsage: .\test.ps1 <run|update-references|help> [<test name 1>, <test name 2>, ...]" -ForegroundColor Yellow
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

