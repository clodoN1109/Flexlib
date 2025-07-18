function DetermineBuildID {

    param (
        
        [string]$BuildHistoryPath
    )
    
    $history = GetBuildHistory $BuildHistoryPath

    $nextID  = $history.count

    return $nextID

}

function SaveBuildHistory {

    param (
        
        [string]$BuildConfiguration,
        [int]$ErrorCount = 0,
        [int]$WarningCount = 0,
        [string]$BuildHistoryPath = "$PSScriptRoot\..\..\builds\builds.json"

    )

    $buildID = DetermineBuildID $BuildHistoryPath

    $history = GetBuildHistory $BuildHistoryPath

    $timestamp = [int][double]::Parse((Get-Date -UFormat %s))

    $newEntry = [PSCustomObject]@{
        id = $buildID
        configuration = $BuildConfiguration
        timestamp = $timestamp
        errors = $ErrorCount
        warnings = $WarningCount
    }
    
    $history.builds += $newEntry
    $history.count = $buildID + 1

    $json = $history | ConvertTo-Json -Depth 3
    Set-Content -Path $BuildHistoryPath -Value $json

    return $newEntry

}

function GetBuildHistory()
{
    param (
        
        [string]$BuildHistoryPath
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
