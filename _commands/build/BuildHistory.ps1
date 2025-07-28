function PlotHistoryGraph {
    param (
        [Parameter(Mandatory)]
        $history
    )

    $builds = $history.builds
    if (-not $builds) {
        Write-Host "No builds to plot." -ForegroundColor Red
        return
    }

    $maxErrors = ($builds | Measure-Object -Property errors -Maximum).Maximum
    $maxWarnings = ($builds | Measure-Object -Property warnings -Maximum).Maximum

    $grid = @{}

    # Use errors as X and warnings as Y
    foreach ($b in $builds) {
        $key = "$($b.errors),$($b.warnings)"
        if (-not $grid.ContainsKey($key)) {
            $grid[$key] = "•"
        }
    }

    # Mark the latest build with "X"
    $latest = $builds[-1]
    $grid["$($latest.errors),$($latest.warnings)"] = "X"

    # Plot: top to bottom = high to low warnings
    Write-Host ""
    Write-Host "Build Quality Scatter - " -NoNewLine
    Write-Host "Errors" -NoNewLine -ForegroundColor Red
    Write-Host " X " -NoNewLine
    Write-Host "Warnings" -ForegroundColor Yellow
    Write-Host "Legend: • = build, X = latest build (ID $($latest.id) at $($latest.timestamp))" -ForegroundColor Green
    Write-Host ""

    for ($y = $maxWarnings; $y -ge 0; $y--) {
        # Print Y label
        Write-Host ("{0,2}" -f $y) -ForegroundColor Yellow -NoNewline
        Write-Host " |" -NoNewline

        for ($x = 0; $x -le $maxErrors; $x++) {
            $key = "$x,$y"
            if ($grid.ContainsKey($key)) {
                Write-Host ("  " + $grid[$key]) -NoNewline  # 3-char cell: 2 spaces + symbol
            } else {
                Write-Host "   " -NoNewline
            }
        }

        Write-Host ""
    }

    # X-axis line
    $xAxis = "    " + ('-' * (($maxErrors + 1) * 3))
    Write-Host $xAxis

    # X-axis labels
    $xLabels = "    "  # Align with Y axis + separator
    for ($x = 0; $x -le $maxErrors; $x++) {
        $xLabels += "{0,3}" -f $x
    }
    Write-Host $xLabels -ForegroundColor Red
    Write-Host ""
}


function DetermineBuildID {

    param (
        
        [string]$BuildHistoryPath
    )
    
    $history = GetBuildHistory $BuildHistoryPath

    $nextID  = $history.builds.Length

    return $nextID

}

function SaveBuildHistory {

    param (
        
        [string]$BuildConfiguration,
        [int]$BuildID,   
        [int]$ErrorCount = 0,
        [int]$WarningCount = 0,
        [string]$BuildHistoryPath = "$PSScriptRoot\..\..\builds\builds.json"

    )

    $history = GetBuildHistory $BuildHistoryPath

    $timestamp  = (Get-Date).ToString("s")  # ISO 8601 format

    $newEntry = [PSCustomObject]@{
        id = $BuildID
        configuration = $BuildConfiguration
        timestamp = $timestamp
        errors = $ErrorCount
        warnings = $WarningCount
    }
    
    $history.builds += $newEntry
    $history.count = $BuildID + 1

    $json = $history | ConvertTo-Json -Depth 3
    Set-Content -Path $BuildHistoryPath -Value $json

    return $newEntry

}

function GetBuildHistory()
{
    param (
        
        [string]$BuildHistoryPath = "$PSScriptRoot\..\..\builds\builds.json"
    )

    if (Test-Path $BuildHistoryPath) 
    {
        $history = Get-Content $BuildHistoryPath | ConvertFrom-Json
    } else {
        $history = [PSCustomObject]@{
                count = 0
                builds = @()
            }
    }

    return $history

}
