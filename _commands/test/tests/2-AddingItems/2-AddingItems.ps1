# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../../../flexlib.ps1"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$referencePath     = "$dataPath/references"
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

# Compare artifacts
$diff_1 = Compare-Folders -Expected "$referencePath/TestLibrary" -Actual "$resultsPath/TestLibrary"
$diff_2 = Compare-Object `
    (Get-Content "$flexlibDataPath/libraries.json") `
    (Get-Content "$referencePath/libraries.json")

# Clean up after test
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath
# Return result
if ($diff_1 -or $diff_2) {
    return $false
} else {
    return $true
}

