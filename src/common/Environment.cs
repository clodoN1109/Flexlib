using System.Reflection;
using System.IO;
using Flexlib.Infrastructure.Meta;

namespace Flexlib.Common;

public static class Env{
    
    public static string? GetExecutingAssemblyLocation()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }    

    public static string Version => BuildInfo.SemanticVersion ?? "";

    public static string BuildId => BuildInfo.BuildId ?? "";

    public static string OS => Environment.OSVersion.ToString();
    
}


