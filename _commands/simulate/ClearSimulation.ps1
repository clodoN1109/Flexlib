try {
    # Clean up
    Safe-Cleanup $resultsPath
    Safe-Cleanup $flexlibDataPath

    # If we got here, everything was OK
    $width = [System.Console]::WindowWidth
    Write-Host ( "`n░░░░ SIMULATION RESULTS CLEARED " + "░" * ($width - 32)) -ForegroundColor Green
}
catch {
    Write-Host "`n✗ Failed attempt to clean up simulation results." -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor DarkRed
}

