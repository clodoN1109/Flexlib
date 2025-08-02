function ExplainUsage {

    Write-Host "`nUsage: " -NoNewLine
    Write-Host "simulate <simulation name> [-Help] [-ClearLastResults] [Mode]" -ForegroundColor Yellow
    Write-Host "`navailable simulations:`n"
    Get-ChildItem $simulationsPath| Foreach-Object { Write-Host "`t$($_.BaseName)" -ForegroundColor Green}

}
