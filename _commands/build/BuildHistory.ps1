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

    $maxErrors   = ($builds | Measure-Object -Property errors -Maximum).Maximum
    $maxWarnings = ($builds | Measure-Object -Property warnings -Maximum).Maximum

    $consoleWidth = $Host.UI.RawUI.WindowSize.Width
    $leftMargin = 4  # 2 digits + " |"
    $cellWidth = 3
    $maxPlotWidth = [math]::Floor(($consoleWidth - $leftMargin) / $cellWidth)
    $errorScale = [math]::Ceiling(($maxErrors + 1) / $maxPlotWidth)

    # Grid indexed by scaled coordinates
    $grid = @{}

    foreach ($b in $builds) {
        $x = [math]::Floor($b.errors / $errorScale)
        $y = $b.warnings
        $key = "$x,$y"
        if (-not $grid.ContainsKey($key)) {
            $grid[$key] = "•"
        }
    }

    # Mark the latest build
    $latest = $builds[-1]

    if ($latest.errors -eq 0 -and $latest.warnings -eq 0) {
        # Green check for successful build
        $lastBuildMark = "`e[32m✓`e[0m"   # ✓ in green
    }
    elseif ($latest.warnings -ne 0) {
        # Yellow W for warnings
        $lastBuildMark = "`e[33mX`e[0m"   # W in yellow
    }
    else {
        # Red X for errors
        $lastBuildMark = "`e[31mX`e[0m"   # X in red
    }

    $lx = [math]::Floor($latest.errors / $errorScale)
    $ly = $latest.warnings
    $grid["$lx,$ly"] = $lastBuildMark
    
    # Headings
    Write-Host ""
    Write-Host "Build Quality Scatter - " -NoNewLine
    Write-Host "Errors" -NoNewLine -ForegroundColor Red
    Write-Host " X " -NoNewLine
    Write-Host "Warnings" -ForegroundColor Yellow
    Write-Host "Legend: • = build, X = latest build (ID $($latest.id) at $($latest.timestamp))" -ForegroundColor Green
    Write-Host "X uncertainty = +$($errorScale - 1)" -ForegroundColor DarkGray
    Write-Host ""

    # Plot
    for ($y = $maxWarnings; $y -ge 0; $y--) {
        Write-Host ("{0,2}" -f $y) -ForegroundColor Yellow -NoNewline
        Write-Host " |" -NoNewline

        for ($x = 0; $x -lt $maxPlotWidth; $x++) {
            $key = "$x,$y"
            if ($grid.ContainsKey($key)) {
                Write-Host ("  " + $grid[$key]) -NoNewline
            } else {
                Write-Host "   " -NoNewline
            }
        }

        Write-Host ""
    }

    # X-axis
    $xAxis = "    " + ('-' * ($maxPlotWidth * $cellWidth))
    Write-Host $xAxis

    # X-axis labels
    $switch = 1; # Used to alternate printed values and avoid overlaping over 99.
    $xLabels = "    "
    for ($x = 0; $x -lt $maxPlotWidth; $x++) {
        $label = $x * $errorScale
        if ($label -le 99 -or $switch -eq 1){
            $xLabels += ("{0,3}" -f $label)
            $switch = 0;
        } else {
            $xLabels += ("{0,3}" -f "")
            $switch = 1;
        }

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
