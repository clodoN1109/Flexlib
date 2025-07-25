. "$PSScriptRoot\ASCIIGraphics.ps1"

function PrintABeautifulLog( $logsGroupedByProjectAndFile, [int]$errorCount, [int]$warningCount) {
    
    $rest = Write-NoNewLine "░░░░ BUILD LOG ░░░░░░░░░░░░"
    $rest = write-NoNewLine " $errorCount" -ForegroundColor Red -Rest $rest
    $rest = Write-NoNewLine " ERRORS " -Rest $rest
    $rest = Write-NoNewLine "░░" -Rest $rest
    $rest = Write-NoNewLine " $warningCount" -ForegroundColor Yellow -Rest $rest
    $rest = Write-NoNewLine " WARNINGS ░░░░░░░░░░░░" -Rest $rest
    Write-Host ("░" * $rest)
    Write-Host
        
    foreach ($projectName in $logsGroupedByProjectAndFile.Keys) {
        Write-Host "PROJECT: $projectName" -ForegroundColor Cyan
        
        $project = $logsGroupedByProjectAndFile[$projectName]
        foreach ($file in $project.Keys) {
            $fileName = Split-Path $file -Leaf
            $path = Split-Path $file -Parent
            Write-Host '┌―' -NoNewLine
            Write-Host ('―' * ("$path/$fileName".Length + 2)) -NoNewLine
            Write-Host "―┐"
            Write-Host "│  $path\" -ForegroundColor Gray -NoNewLine
            Write-Host "$fileName" -ForegroundColor Green -NoNewLine
            Write-Host "  │"
            Write-Host '└―' -NoNewLine
            Write-Host ('―' * ("$path/$fileName".Length + 2)) -NoNewLine
            Write-Host "―┘"

            foreach ($entry in $project[$file]) {
                $typeColor = if ($entry.Type -eq "error") { "Red" } else { "Yellow" }
                Write-Host "[$($entry.Type.ToUpper())]".PadLeft(9) -ForegroundColor $typeColor -NoNewLine
                Write-Host " at line" -ForegroundColor Gray -NoNewLine
                Write-Host " $($entry.Line):" -ForegroundColor Green -NoNewLine
                Write-Host " $($entry.Code) - $($entry.Message)" -ForegroundColor White
                Write-Host ('-' * [System.Console]::WindowWidth)
            }
        }
    }

    if ($errorCount     -ne 0)      { DrawSadFace;      return;     }
    if ($warningCount   -ne 0)      { DrawWorriedFace;  return;     }
    DrawHappyFace

}
