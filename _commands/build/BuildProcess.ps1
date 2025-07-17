function ExecuteBuildProcess () {

# 2>&1 is a PowerShell redirection operator that merges the error stream with the output stream.
    $output = dotnet build "$PSScriptRoot\..\..\src\Flexlib.csproj" -v:q 2>&1

    return $output

}
