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
$item4             = "$dataPath/input/Item4.pdf"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

# Library
$libraryName  = "TestLibrary"

# Run simulations
# --------------------------------------------------------
# Create the library
& $flexlibPath new-lib $libraryName $resultsPath
# --------------------------------------------------------
# Create items
& $flexlibPath new-item $item1 "Item1" $libraryName
& $flexlibPath new-item $item2 "Item2" $libraryName
& $flexlibPath new-item $item3 "Item3" $libraryName
& $flexlibPath new-item $item4 "Item4" $libraryName
# --------------------------------------------------------
# Create properties
& $flexlibPath new-prop "author"    $libraryName string
& $flexlibPath new-prop "publisher" $libraryName string
& $flexlibPath new-prop "theme"     $libraryName list
& $flexlibPath new-prop "year"      $libraryName int
# --------------------------------------------------------
# Authors
& $flexlibPath set-prop "author" "newton"   $item1 $libraryName
& $flexlibPath set-prop "author" "pascal"   $item2 $libraryName
& $flexlibPath set-prop "author" "einstein" $item3 $libraryName
& $flexlibPath set-prop "author" "euler"    $item4 $libraryName
# Publishers
& $flexlibPath set-prop "publisher" "dover"          $item1 $libraryName
& $flexlibPath set-prop "publisher" "dover"          $item2 $libraryName
& $flexlibPath set-prop "publisher" "nova fronteira" $item3 $libraryName
& $flexlibPath set-prop "publisher" "alta books"     $item4 $libraryName
# Themes
& $flexlibPath set-prop "theme" "mathematics" $item1 $libraryName
& $flexlibPath set-prop "theme" "physics"     $item1 $libraryName
& $flexlibPath set-prop "theme" "mathematics" $item2 $libraryName
& $flexlibPath set-prop "theme" "philosophy"  $item2 $libraryName
& $flexlibPath set-prop "theme" "mathematics" $item3 $libraryName
& $flexlibPath set-prop "theme" "history"     $item3 $libraryName
& $flexlibPath set-prop "theme" "history"     $item4 $libraryName
& $flexlibPath set-prop "theme" "physics"     $item4 $libraryName
# Years
& $flexlibPath set-prop "year" 1920 $item1 $libraryName
& $flexlibPath set-prop "year" 1940 $item2 $libraryName
& $flexlibPath set-prop "year" 1945 $item3 $libraryName
& $flexlibPath set-prop "year" 2001 $item4 $libraryName
# --------------------------------------------------------
& $flexlibPath set-layout TestLibrary theme/publisher/author/year
# --------------------------------------------------------
$output = & $flexlibPath list-items TestLibrary "" 'mathematics/*/newton,pascal,euler/1935-2002'
$output | Set-Content "$resultsPath/output.txt"
# --------------------------------------------------------

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

# Skips the second line (build number), which changes every test preceded by a build and would break meaningful comparison
$actual   = Get-Content "$resultsPath/output.txt"     | Where-Object { $true } | ForEach-Object -Begin { $i = 0 } -Process { if ($i++ -ne 1) { $_ } }
$expected = Get-Content "$referencesPath/output.txt"  | Where-Object { $true } | ForEach-Object -Begin { $i = 0 } -Process { if ($i++ -ne 1) { $_ } }
$diff_3 = Compare-Object $actual $expected


# Clean up after test
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

# Return result
if ($diff_1 -or $diff_2 -or $diff_3) {
    return $false
} else {
    return $true
}
