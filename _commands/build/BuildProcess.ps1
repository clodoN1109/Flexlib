function ExecuteBuildProcess {
    param (
        [string]$Configuration = "Debug",
        [int]$BuildId,
        [string]$Version
    )

    if (-not $Version) { $Version = "0.0.0" }

    $projectPath = Join-Path $PSScriptRoot "..\..\src\Flexlib.csproj"

    $buildArgs = @(
        $projectPath
        "-c", $Configuration
        "-p:Version=$Version"
        "-p:InformationalVersion=$Version+build.$BuildId"
        "-v:q"
    )

    $output = dotnet build @buildArgs 2>&1
    return $output
}

