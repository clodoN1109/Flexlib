$env:DOTNET_CLI_UI_LANGUAGE = "en"

# 2>&1 is a PowerShell redirection operator that merges the error stream with the output stream.
$output = dotnet build "$PSScriptRoot\..\src\Flexlib.csproj" -v:q 2>&1

$grouped = @{}


# Parsing stardard error stream
foreach ($line in $output) {
    if ($line -match "^(?<file>.*\.cs)\((?<line>\d+),\d+\): (?<type>warning|error) (?<code>CS\d{4}): (?<message>.+?) \[(?<project>.+\.csproj)\]") {
        $entry = @{
            File    = $matches.file
            Line    = $matches.line
            Type    = $matches.type
            Code    = $matches.code
            Message = $matches.message.Trim()
        }

        $project = $matches.project

        if (-not $grouped.ContainsKey($project)) {
            $grouped[$project] = @()
        }

        # Generate a unique string key for deduplication
        $entryKey = "$($entry.File):$($entry.Line):$($entry.Type):$($entry.Code):$($entry.Message)"
        $existingKeys = $grouped[$project] | ForEach-Object {
            "$($_.File):$($_.Line):$($_.Type):$($_.Code):$($_.Message)"
        }

        if (-not ($existingKeys -contains $entryKey)) {
            $grouped[$project] += $entry
        }
    }
}

# Counting Errors and Warnings
$errorCount = 0
$warningCount = 0
foreach ($project in $grouped.Values) {
    foreach ($entry in $project) {
        if ($entry.Type -eq 'ERROR')   { $errorCount++ }
        elseif ($entry.Type -eq 'WARNING') { $warningCount++ }
    }
}

# Print organized output
Clear-Host
Write-Host "BUILD LOG - ($errorCount) ERRORS AND ($warningCount) WARNINGS"
Write-Host ('=' * [System.Console]::WindowWidth)
foreach ($project in $grouped.Keys) {
    Write-Host "PROJECT: $project" -ForegroundColor Cyan
    Write-Host ('=' * [System.Console]::WindowWidth)
    foreach ($entry in $grouped[$project]) {
        
        $fileName = Split-Path $entry.File -Leaf
        $path = Split-Path $entry.File -Parent

        $color = if ($entry.Type -eq "error") { "Red" } else { "Yellow" }
        Write-Host "[$($entry.Type.ToUpper())]" -ForegroundColor $color -NoNewLine
        Write-Host "`t$path\" -ForegroundColor Gray -NoNewLine
        Write-Host "$fileName`:$($entry.Line)`n" -ForegroundColor Green
        Write-Host " $($entry.Code) - $($entry.Message)" -ForegroundColor White
        Write-Host ('-' * [System.Console]::WindowWidth)
    }
}

Write-Host "`nBUILD PROCESS COMPLETED SUCCESSFULLY"
Write-Host "`nInitiating runtime test suite..."
Start-Sleep 2
if (($errorCount -eq 0) -and ($warningCount -eq 0)) {
    & "$PSScriptRoot/test.ps1" run
}
