using Flexlib.Infrastructure.Meta;
using System.Reflection;
using System.IO;
using System;

namespace Flexlib.Infrastructure.Environment;

public static class Env{
    
    public static string? GetExecutingAssemblyLocation()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }    

    public static string Version => BuildInfo.SemanticVersion ?? "";

    public static string BuildId => BuildInfo.BuildId ?? "";

    public static string OS => System.Environment.OSVersion.ToString();
    
}


