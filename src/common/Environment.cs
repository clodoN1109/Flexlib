namespace Flexlib.Common;

using System.Reflection;
using System.IO;

public static class Env{
    
    public static string? GetExecutingAssemblyLocation()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }    

    public static string Version => BuildInfo.SemanticVersion ?? "";

    public static string BuildId => BuildInfo.BuildId ?? "";

    public static string OS => Environment.OSVersion.ToString();
    
}


