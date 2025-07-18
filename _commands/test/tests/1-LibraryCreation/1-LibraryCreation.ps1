# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../../../flexlib.ps1"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"
$resultsPath       = "$PSScriptRoot/data/results"
$referencePath     = "$PSScriptRoot/data/references"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Remove-Item "$resultsPath/*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$flexlibDataPath/*" -Recurse -Force -ErrorAction SilentlyContinue

# Run test
& $flexlibPath new-lib TestLibrary $resultsPath

# Compare artifacts
$diff_1 = Compare-Folders -Expected "$referencePath/TestLibrary" -Actual "$resultsPath/TestLibrary"
$diff_2 = Compare-Object `
    (Get-Content "$flexlibDataPath/libraries.json") `
    (Get-Content "$referencePath/libraries.json")

# Clean up after test
Remove-Item "$resultsPath/*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$flexlibDataPath/*" -Recurse -Force -ErrorAction SilentlyContinue

# Return result
if ($diff_1 -or $diff_2) {
    return $false
} else {
    return $true
}

