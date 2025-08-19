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
    & $flexlibPath signup
}

# ========== LIBRARIES =========

& $flexlibPath new-lib GeneralLibrary  $resultsPath
& $flexlibPath new-lib ScienceLibrary  $resultsPath
& $flexlibPath new-lib CultureLibrary  $resultsPath

# ========== ITEMS ==========

& $flexlibPath new-item GeneralLibrary 1 $itemPaths[0]
& $flexlibPath new-item GeneralLibrary 2 $itemPaths[1]
& $flexlibPath new-item GeneralLibrary 3 $itemPaths[2]

& $flexlibPath new-item ScienceLibrary 1 $itemPaths[3]
& $flexlibPath new-item ScienceLibrary 2 $itemPaths[4]
& $flexlibPath new-item ScienceLibrary 3 $itemPaths[5]

& $flexlibPath new-item CultureLibrary 1 $itemPaths[6]
& $flexlibPath new-item CultureLibrary 2 $itemPaths[7]
& $flexlibPath new-item CultureLibrary 3 $itemPaths[8]

# ========== PROPERTIES ==========

$allLibs = @("GeneralLibrary", "ScienceLibrary", "CultureLibrary")

foreach ($lib in $allLibs) {
    & $flexlibPath new-prop $lib author     string
    & $flexlibPath new-prop $lib publisher  string
    & $flexlibPath new-prop $lib theme      list
    & $flexlibPath new-prop $lib year       int
    & $flexlibPath new-prop $lib language   string
    & $flexlibPath new-prop $lib difficulty string
}

# ========== PROPERTIES VALUES ==========

# GeneralLibrary Items
& $flexlibPath set-prop GeneralLibrary 1 author     Newton
& $flexlibPath set-prop GeneralLibrary 2 author     Pascal
& $flexlibPath set-prop GeneralLibrary 3 author     Leibniz

& $flexlibPath set-prop GeneralLibrary 1 publisher  dover
& $flexlibPath set-prop GeneralLibrary 2 publisher  springer
& $flexlibPath set-prop GeneralLibrary 3 publisher  'cambridge press'

& $flexlibPath set-prop GeneralLibrary 1 theme      mathematics
& $flexlibPath set-prop GeneralLibrary 1 theme      physics
& $flexlibPath set-prop GeneralLibrary 2 theme      logic
& $flexlibPath set-prop GeneralLibrary 3 theme      philosophy

& $flexlibPath set-prop GeneralLibrary 1 year       1687
& $flexlibPath set-prop GeneralLibrary 2 year       1654
& $flexlibPath set-prop GeneralLibrary 3 year       1710

& $flexlibPath set-prop GeneralLibrary 1 language   latin
& $flexlibPath set-prop GeneralLibrary 2 language   french
& $flexlibPath set-prop GeneralLibrary 3 language   german

& $flexlibPath set-prop GeneralLibrary 1 difficulty hard
& $flexlibPath set-prop GeneralLibrary 2 difficulty medium
& $flexlibPath set-prop GeneralLibrary 3 difficulty hard

# ScienceLibrary Items
& $flexlibPath set-prop ScienceLibrary 1 author     Curie
& $flexlibPath set-prop ScienceLibrary 2 author     Einstein
& $flexlibPath set-prop ScienceLibrary 3 author     Feynman

& $flexlibPath set-prop ScienceLibrary 1 publisher  "nobel house"
& $flexlibPath set-prop ScienceLibrary 2 publisher  "princeton"
& $flexlibPath set-prop ScienceLibrary 3 publisher  "mit press"

& $flexlibPath set-prop ScienceLibrary 1 theme      chemistry
& $flexlibPath set-prop ScienceLibrary 2 theme      physics
& $flexlibPath set-prop ScienceLibrary 3 theme      quantum

& $flexlibPath set-prop ScienceLibrary 1 year       1911
& $flexlibPath set-prop ScienceLibrary 2 year       1916
& $flexlibPath set-prop ScienceLibrary 3 year       1965

& $flexlibPath set-prop ScienceLibrary 1 language   french
& $flexlibPath set-prop ScienceLibrary 2 language   german
& $flexlibPath set-prop ScienceLibrary 3 language   english

& $flexlibPath set-prop ScienceLibrary 1 difficulty hard
& $flexlibPath set-prop ScienceLibrary 2 difficulty hard
& $flexlibPath set-prop ScienceLibrary 3 difficulty medium

# CultureLibrary Items
& $flexlibPath set-prop CultureLibrary 1 author     Homer
& $flexlibPath set-prop CultureLibrary 2 author     Shakespeare
& $flexlibPath set-prop CultureLibrary 3 author     Camus

& $flexlibPath set-prop CultureLibrary 1 publisher  penguin
& $flexlibPath set-prop CultureLibrary 2 publisher  'oxford press'
& $flexlibPath set-prop CultureLibrary 3 publisher  gallimard

& $flexlibPath set-prop CultureLibrary 1 theme      literature
& $flexlibPath set-prop CultureLibrary 2 theme      tragedy
& $flexlibPath set-prop CultureLibrary 3 theme      absurd

& $flexlibPath set-prop CultureLibrary 1 year       '-700'
& $flexlibPath set-prop CultureLibrary 2 year       1603
& $flexlibPath set-prop CultureLibrary 3 year       1942

& $flexlibPath set-prop CultureLibrary 1 language   greek
& $flexlibPath set-prop CultureLibrary 2 language   english
& $flexlibPath set-prop CultureLibrary 3 language   french

& $flexlibPath set-prop CultureLibrary 1 difficulty medium
& $flexlibPath set-prop CultureLibrary 2 difficulty hard
& $flexlibPath set-prop CultureLibrary 3 difficulty medium

# ========== NOTES ==========

& $flexlibPath new-note GeneralLibrary 1 "This is a single-line note with a reference to {ScienceLibrary/Item1}."
& $flexlibPath new-note GeneralLibrary 2 "This is a single-line note with a reference to {ScienceLibrary/Item2}."
& $flexlibPath new-note GeneralLibrary 3 "This is a single-line note with a reference to {ScienceLibrary/Item3}."
& $flexlibPath new-note GeneralLibrary 1 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note GeneralLibrary 2 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note GeneralLibrary 3 "This is a multi-line note.`nThis is a second line.`nThis is a third line."

& $flexlibPath new-note ScienceLibrary 1 "This is a single-line note with a reference to {GeneralLibrary/Item1}."
& $flexlibPath new-note ScienceLibrary 2 "This is a single-line note with a reference to {GeneralLibrary/Item2}."
& $flexlibPath new-note ScienceLibrary 3 "This is a single-line note with a reference to {GeneralLibrary/Item3}."
& $flexlibPath new-note ScienceLibrary 1 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note ScienceLibrary 2 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note ScienceLibrary 3 "This is a multi-line note.`nThis is a second line.`nThis is a third line."

& $flexlibPath new-note CultureLibrary 1 "This is a single-line note with a reference to {ScienceLibrary/Item1}."
& $flexlibPath new-note CultureLibrary 2 "This is a single-line note with a reference to {ScienceLibrary/Item2}."
& $flexlibPath new-note CultureLibrary 3 "This is a single-line note with a reference to {ScienceLibrary/Item3}."
& $flexlibPath new-note CultureLibrary 1 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note CultureLibrary 2 "This is a multi-line note.`nThis is a second line.`nThis is a third line."
& $flexlibPath new-note CultureLibrary 3 "This is a multi-line note.`nThis is a second line.`nThis is a third line."

# ========== LAYOUTS ==========

& $flexlibPath set-layout GeneralLibrary  theme/publisher/author/year
& $flexlibPath set-layout ScienceLibrary  theme/author/year
& $flexlibPath set-layout CultureLibrary  author/theme


