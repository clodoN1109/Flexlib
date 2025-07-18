function ExecuteBuildProcess {
    param (
        [string]$Configuration = "Debug"  # Default to Debug if not provided
    )

    $projectPath = Join-Path $PSScriptRoot "..\..\src\Flexlib.csproj"
    $output = dotnet build $projectPath -c $Configuration -v:q 2>&1

    return $output
}

