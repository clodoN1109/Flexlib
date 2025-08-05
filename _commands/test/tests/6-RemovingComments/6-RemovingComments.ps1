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
& $flexlibPath new-item $item2 'Compound Name' TestLibrary
& $flexlibPath new-item $item3 Item3 TestLibrary
& $flexlibPath new-comment 1 TestLibrary 'This is a comment.'
& $flexlibPath new-comment 1 TestLibrary 'Another comment.'
& $flexlibPath new-comment 2 TestLibrary 'This is a comment quoting {TestLibrary/Item1}.'
& $flexlibPath new-comment 3 TestLibrary 'This is a comment quoting {TestLibrary/Item1} and {TestLibrary/Compound Name}.'
& $flexlibPath new-comment 3 TestLibrary 'This is a yet another comment.'
& $flexlibPath remove-comment 3 1 TestLibrary 
& $flexlibPath remove-comment 2 1 TestLibrary 

if ($UpdateReferences) {
    Safe-Cleanup $referencesPath
    Safe-CopyItems $resultsPath     $referencesPath
    Safe-CopyItems $flexlibDataPath $referencesPath
}

# Compare artifacts
$diff_1 = Compare-Folders -Expected "$referencesPath/TestLibrary" -Actual "$resultsPath/TestLibrary"
$referenceMetaFile = Get-Content "$flexlibDataPath/libraries.json" | ConvertFrom-Json
$resultMetaFile    = Get-Content "$referencesPath/libraries.json"  | ConvertFrom-Json
Remove-TimeFields $referenceMetaFile
Remove-TimeFields $resultMetaFile
$diff_2 = Compare-Object $referenceMetaFile $resultMetaFile 

# Clean up after test
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath
# Return result
if ($diff_1 -or $diff_2) {
    return $false
} else {
    return $true
}
