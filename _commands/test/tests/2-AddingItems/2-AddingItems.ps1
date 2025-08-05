param (
    [switch]$UpdateReferences
)

# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../../../flexlib.ps1"
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$referencesPath     = "$dataPath/references"
$item1             = "$dataPath/input/Item1.pdf"
$item2             = "$dataPath/input/Item2.pdf"
$item3             = "$dataPath/input/Item3.pdf"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

# Run test
& $flexlibPath new-lib TestLibrary $resultsPath
& $flexlibPath new-item $item1 Item1 TestLibrary
& $flexlibPath new-item $item2 Item2 TestLibrary
& $flexlibPath new-item $item3 Item3 TestLibrary

if ($UpdateReferences) {
    Safe-Cleanup $referencesPath
    Safe-CopyItems $resultsPath     $referencesPath
    Safe-CopyItems $flexlibDataPath $referencesPath
}

# Compare artifacts
$diff_1 = Compare-Folders -Expected "$referencesPath/TestLibrary" -Actual "$resultsPath/TestLibrary"
$diff_2 = Compare-Object `
    (Get-Content "$flexlibDataPath/libraries.json") `
    (Get-Content "$referencesPath/libraries.json")

# Clean up after test
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath
# Return result
if ($diff_1 -or $diff_2) {
    return $false
} else {
    return $true
}

