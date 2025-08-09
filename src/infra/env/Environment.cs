using Flexlib.Infrastructure.Meta;
using System.Reflection;
using System.IO;
using System;
using Flexlib.Infrastructure.Config;

namespace Flexlib.Infrastructure.Environment;

public static class Env
{

    public static string? GetExecutingAssemblyLocation()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    public static string Version => BuildInfo.SemanticVersion ?? "";

    public static string BuildId => BuildInfo.BuildId ?? "";

    public static string OS => System.Environment.OSVersion.ToString();

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif 
    }   
    public static int GetSafeWindowWidth()
    {
        try
        {
            int w = Console.WindowWidth;
            if (w > 0) return w;
        }
        catch (IOException) { }
        return GlobalConfig.ConsoleWidth ?? 80;
    }
      
    
}


