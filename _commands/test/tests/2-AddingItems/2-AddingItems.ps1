# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../../../flexlib.ps1"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/temp/Debug/net8.0/data"
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$referencePath     = "$dataPath/references"
$item1             = "$dataPath/input/Item1.pdf"
$item2             = "$dataPath/input/Item2.pdf"
$item3             = "$dataPath/input/Item3.pdf"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Remove-Item "$resultsPath/*" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$flexlibDataPath/*" -Recurse -Force -ErrorAction SilentlyContinue

# Run test
& $flexlibPath new TestLibrary $resultsPath
& $flexlibPath add-item $item1 TestLibrary Item1
& $flexlibPath add-item $item2 TestLibrary Item2
& $flexlibPath add-item $item3 TestLibrary Item3

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

