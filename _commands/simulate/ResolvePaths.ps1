switch ($Mode.ToLower()) {
    
    "debug" {
        $flexlibPath     = Resolve-Path "$PSScriptRoot/../flexlib.ps1"
        $flexlibDataPath = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/win-x64/data"
        break
    }

    "release" {

        $releasesPath = Resolve-Path "$HOME/Projects/Incubator/Flexlib/Ops/Delivery/releases/"
        $versionFolder = Join-Path $releasesPath $Version
        $zipFile = Join-Path $versionFolder "Flexlib-$Version.zip"

        if (-not (Test-Path $zipFile)) {
            Write-Host "`n❌ Zip file not found: $zipFile" -ForegroundColor Red
            return
        }

        $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("FlexlibExtract_" + [System.Guid]::NewGuid())
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

        try {
            Expand-Archive -Path $zipFile -DestinationPath $tempDir -Force

            $flexlibExePath = Join-Path $tempDir "Flexlib.exe"

            if (-not (Test-Path $flexlibExePath)) {
                throw "❌ Flexlib.exe not found in extracted release: $flexlibExePath"
            }

            $flexlibPath     = $flexlibExePath
            $flexlibDataPath = "$HOME/AppData/Local/Flexlib/data"

        } catch {
            Write-Host $_ -ForegroundColor Red
            return
        }

        break
    }

    "production" {
        $flexlibPath     = Resolve-Path "$HOME/Tools/Flexlib/Flexlib.exe"
        $flexlibDataPath = "$HOME/AppData/Local/Flexlib/data"
        break
    }

    default {
        Write-Host "`n❌ Invalid mode: $Mode" -ForegroundColor Red
        return
    }
}

# Shared paths
$dataPath            = "$PSScriptRoot/../simulate/data"
$resultsPath         = "$dataPath/results"
$simulateCommandDir  = "$PSScriptRoot/../simulate"
$simulationsPath     = "$simulateCommandDir/simulations"
$requestedSimulation = "$simulationsPath/$SimulationName.ps1"
$ProgressPreference = 'SilentlyContinue'

