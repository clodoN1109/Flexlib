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
    $consoleHeight = $Host.UI.RawUI.WindowSize.Height

    $leftMargin = 4  # 2 digits + " |"
    $cellWidth = 3
    $maxPlotWidth = [math]::Floor(($consoleWidth - $leftMargin) / $cellWidth)
    $maxPlotHeight = 20  # Limit the number of rows to avoid long graphs

    $errorScale = [math]::Ceiling(($maxErrors + 1) / $maxPlotWidth)
    $warningScale = [math]::Ceiling(($maxWarnings + 1) / $maxPlotHeight)

    # Grid indexed by scaled coordinates
    $grid = @{}

    foreach ($b in $builds) {
        $x = [math]::Floor($b.errors / $errorScale)
        $y = [math]::Floor($b.warnings / $warningScale)
        $key = "$x,$y"
        if (-not $grid.ContainsKey($key)) {
            $grid[$key] = "•"
        }
    }

    # Mark the latest build
    $latest = $builds[-1]

    if ($latest.errors -eq 0 -and $latest.warnings -eq 0) {
        $lastBuildMark = "`e[32m✓`e[0m"   # ✓ in green
    }
    elseif ($latest.warnings -ne 0) {
        $lastBuildMark = "`e[33mX`e[0m"   # X in yellow
    }
    else {
        $lastBuildMark = "`e[31mX`e[0m"   # X in red
    }

    $lx = [math]::Floor($latest.errors / $errorScale)
    $ly = [math]::Floor($latest.warnings / $warningScale)
    $grid["$lx,$ly"] = $lastBuildMark

    # Headings
    Write-Host "Build Quality Scatter - " -NoNewLine
    Write-Host "Errors" -NoNewLine -ForegroundColor Red
    Write-Host " X " -NoNewLine
    Write-Host "Warnings" -ForegroundColor Yellow
    Write-Host "Legend: • = build, X = latest build (ID $($latest.id) at $($latest.timestamp))" -ForegroundColor Green
    Write-Host "X uncertainty = +$($errorScale - 1), Y uncertainty = +$($warningScale - 1)" -ForegroundColor DarkGray
    Write-Host ""

    # Plot
    for ($y = $maxPlotHeight - 1; $y -ge 0; $y--) {
        $label = $y * $warningScale
        Write-Host ("{0,3}" -f $label) -ForegroundColor Yellow -NoNewline
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
    $switch = 1
    $xLabels = "     "
    for ($x = 0; $x -lt $maxPlotWidth; $x++) {
        $label = $x * $errorScale
        if ($label -le 99 -or $switch -eq 1){
            $xLabels += ("{0,3}" -f $label)
            $switch = 0
        } else {
            $xLabels += ("{0,3}" -f "")
            $switch = 1
        }
    }
    Write-Host $xLabels -ForegroundColor Red
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
