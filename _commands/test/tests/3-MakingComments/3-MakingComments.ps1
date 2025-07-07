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
& $flexlibPath add-item $item1 Item1 TestLibrary
& $flexlibPath add-item $item2 Item2 TestLibrary
& $flexlibPath add-item $item3 Item3 TestLibrary
& $flexlibPath make-comment Item1 TestLibrary 'This is a comment.'
& $flexlibPath make-comment Item2 TestLibrary 'This is a comment quoting {TestLibrary/Item1}.'
& $flexlibPath make-comment Item3 TestLibrary 'This is a comment quoting {TestLibrary/Item1} and {TestLibrary/Item2}.'


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
