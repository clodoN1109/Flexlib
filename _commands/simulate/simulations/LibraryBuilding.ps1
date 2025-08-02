# Input files
$itemPaths = @(
    "$dataPath/input/Item1.pdf",
    "$dataPath/input/Item2.pdf",
    "$dataPath/input/Item3.pdf",
    "$dataPath/input/Item4.pdf",
    "$dataPath/input/Item5.pdf",
    "$dataPath/input/Item6.pdf",
    "$dataPath/input/Item7.pdf",
    "$dataPath/input/Item8.pdf",
    "$dataPath/input/Item9.pdf"
)

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

Write-Host ""

if ($Mode -ne "DEBUG") {
    & $flexlibPath new-user
}
# ========== LIBRARIES =========

& $flexlibPath new-lib GeneralLibrary  $resultsPath
& $flexlibPath new-lib ScienceLibrary  $resultsPath
& $flexlibPath new-lib CultureLibrary  $resultsPath

# ========== ITEMS ==========

& $flexlibPath new-item $itemPaths[0] Item1  GeneralLibrary
& $flexlibPath new-item $itemPaths[1] Item2  GeneralLibrary
& $flexlibPath new-item $itemPaths[2] Item3  GeneralLibrary

& $flexlibPath new-item $itemPaths[3] Item4  ScienceLibrary
& $flexlibPath new-item $itemPaths[4] Item5  ScienceLibrary
& $flexlibPath new-item $itemPaths[5] Item6  ScienceLibrary

& $flexlibPath new-item $itemPaths[6] Item7  CultureLibrary
& $flexlibPath new-item $itemPaths[7] Item8  CultureLibrary
& $flexlibPath new-item $itemPaths[8] Item9  CultureLibrary

# ========== PROPERTIES ==========

$allLibs = @("GeneralLibrary", "ScienceLibrary", "CultureLibrary")

foreach ($lib in $allLibs) {
    & $flexlibPath new-prop author        $lib string
    & $flexlibPath new-prop publisher     $lib string
    & $flexlibPath new-prop theme         $lib list
    & $flexlibPath new-prop year          $lib int
    & $flexlibPath new-prop language      $lib string
    & $flexlibPath new-prop difficulty    $lib string
}

# ========== PROPERTIES VALUES ==========

# GeneralLibrary Items
& $flexlibPath set-prop author      Newton              1 GeneralLibrary
& $flexlibPath set-prop author      Pascal              2 GeneralLibrary
& $flexlibPath set-prop author      Leibniz             3 GeneralLibrary

& $flexlibPath set-prop publisher   dover               1 GeneralLibrary
& $flexlibPath set-prop publisher   springer            2 GeneralLibrary
& $flexlibPath set-prop publisher   'cambridge press'   3 GeneralLibrary

& $flexlibPath set-prop theme       mathematics         1 GeneralLibrary
& $flexlibPath set-prop theme       physics             1 GeneralLibrary
& $flexlibPath set-prop theme       logic               2 GeneralLibrary
& $flexlibPath set-prop theme       philosophy          3 GeneralLibrary

& $flexlibPath set-prop year        1687                1 GeneralLibrary
& $flexlibPath set-prop year        1654                2 GeneralLibrary
& $flexlibPath set-prop year        1710                3 GeneralLibrary

& $flexlibPath set-prop language    latin               1 GeneralLibrary
& $flexlibPath set-prop language    french              2 GeneralLibrary
& $flexlibPath set-prop language    german              3 GeneralLibrary

& $flexlibPath set-prop difficulty  hard                1 GeneralLibrary
& $flexlibPath set-prop difficulty  medium              2 GeneralLibrary
& $flexlibPath set-prop difficulty  hard                3 GeneralLibrary

# ScienceLibrary s
& $flexlibPath set-prop author      Curie               1 ScienceLibrary
& $flexlibPath set-prop author      Einstein            2 ScienceLibrary
& $flexlibPath set-prop author      Feynman             3 ScienceLibrary

& $flexlibPath set-prop publisher   "nobel house"       1 ScienceLibrary
& $flexlibPath set-prop publisher   "princeton"         2 ScienceLibrary
& $flexlibPath set-prop publisher   "mit press"         3 ScienceLibrary

& $flexlibPath set-prop theme       chemistry           1 ScienceLibrary
& $flexlibPath set-prop theme       physics             2 ScienceLibrary
& $flexlibPath set-prop theme       quantum             3 ScienceLibrary

& $flexlibPath set-prop year        1911                1 ScienceLibrary
& $flexlibPath set-prop year        1916                2 ScienceLibrary
& $flexlibPath set-prop year        1965                3 ScienceLibrary

& $flexlibPath set-prop language    french              1 ScienceLibrary
& $flexlibPath set-prop language    german              2 ScienceLibrary
& $flexlibPath set-prop language    english             3 ScienceLibrary

& $flexlibPath set-prop difficulty  hard                1 ScienceLibrary
& $flexlibPath set-prop difficulty  hard                2 ScienceLibrary
& $flexlibPath set-prop difficulty  medium              3 ScienceLibrary

# CultureLibrary s
& $flexlibPath set-prop author      Homer               1 CultureLibrary
& $flexlibPath set-prop author      Shakespeare         2 CultureLibrary
& $flexlibPath set-prop author      Camus               3 CultureLibrary

& $flexlibPath set-prop publisher   penguin             1 CultureLibrary
& $flexlibPath set-prop publisher   'oxford press'      2 CultureLibrary
& $flexlibPath set-prop publisher   gallimard           3 CultureLibrary

& $flexlibPath set-prop theme       literature          1 CultureLibrary
& $flexlibPath set-prop theme       tragedy             2 CultureLibrary
& $flexlibPath set-prop theme       absurd              3 CultureLibrary

& $flexlibPath set-prop year        '-700'              1 CultureLibrary
& $flexlibPath set-prop year        1603                2 CultureLibrary
& $flexlibPath set-prop year        1942                3 CultureLibrary

& $flexlibPath set-prop language    greek               1 CultureLibrary
& $flexlibPath set-prop language    english             2 CultureLibrary
& $flexlibPath set-prop language    french              3 CultureLibrary

& $flexlibPath set-prop difficulty  medium              1 CultureLibrary
& $flexlibPath set-prop difficulty  hard                2 CultureLibrary
& $flexlibPath set-prop difficulty  medium              3 CultureLibrary

# ========== COMMENTS ==========

& $flexlibPath new-comment 1 GeneralLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item1}." 
& $flexlibPath new-comment 2 GeneralLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item2}." 
& $flexlibPath new-comment 3 GeneralLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item3}." 
& $flexlibpath new-comment 1 GeneralLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 2 GeneralLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 3 GeneralLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 

& $flexlibpath new-comment 1 ScienceLibrary "This is a single-line comment with a reference to {GeneralLibrary/item1}." 
& $flexlibpath new-comment 2 ScienceLibrary "This is a single-line comment with a reference to {GeneralLibrary/item2}." 
& $flexlibpath new-comment 3 ScienceLibrary "This is a single-line comment with a reference to {GeneralLibrary/item3}." 
& $flexlibpath new-comment 1 ScienceLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 2 ScienceLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 3 ScienceLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 

& $flexlibPath new-comment 1 CultureLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item1}." 
& $flexlibPath new-comment 2 CultureLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item2}." 
& $flexlibPath new-comment 3 CultureLibrary "This is a single-line comment with a reference to {ScienceLibrary/Item3}." 
& $flexlibpath new-comment 1 CultureLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 2 CultureLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 
& $flexlibpath new-comment 3 CultureLibrary "This is a multi-line comment.`nThis is a second line.`nThis is a third line." 

# ========== LAYOUTS ==========

& $flexlibPath set-layout GeneralLibrary  theme/publisher/author/year
& $flexlibPath set-layout ScienceLibrary  theme/author/year
& $flexlibPath set-layout CultureLibrary  author/theme

