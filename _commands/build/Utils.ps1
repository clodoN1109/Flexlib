function Write-NoNewLine {
    param (
        [string]$Text = "",
        [int]$Rest = $Host.UI.RawUI.WindowSize.Width,
        [string]$ForegroundColor = "White"
    )
    
    $Rest = $Rest - $Text.Length
    if ($Rest -lt 0) { $Rest = 0 }
    
    Write-Host $Text -NoNewLine -ForegroundColor $ForegroundColor

    return $Rest

}

function Write-Fill {
    param (
        [string]$Text = "",
        [char]$FillChar = '░'
    )

    $Prefix = [string]$FillChar * 4
    $width = $Host.UI.RawUI.WindowSize.Width
    $fillLength = $width - $Text.Length - $Prefix.Length
    if ($fillLength -lt 0) { $fillLength = 0 }

    Write-Host "`n$Prefix $Text $([string]$FillChar * ($fillLength - 2))`n"
}

