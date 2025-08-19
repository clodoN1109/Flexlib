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
# Run simulations

# Define entities
$libraryName = "TestLibrary"
# Create a new library
& $flexlibPath new-lib $libraryName $resultsPath
# Add new items
& $flexlibPath new-item $item1 "Item1" $libraryName
& $flexlibPath new-item $item2 "Item2" $libraryName
& $flexlibPath new-item $item3 "Item3" $libraryName
# Add properties
& $flexlibPath new-prop "Property1" $libraryName string
& $flexlibPath new-prop "Property2" $libraryName string
# Set property values
& $flexlibPath set-prop "Property1" "NewValue 1" $item1 $libraryName
& $flexlibPath set-prop "Property2" "NewValue 2" $item2 $libraryName

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
