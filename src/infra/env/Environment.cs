using Flexlib.Infrastructure.Meta;
using System.Reflection;
using System.IO;
using System;
using Flexlib.Infrastructure.Config;
using System.Diagnostics;

namespace Flexlib.Infrastructure.Environment;

public static class Env
{

    // Folder that contains the actual running executable (works with single-file)
    public static string GetExecutingAssemblyLocation()
    {
        var exePath = System.Environment.ProcessPath
                      ?? Process.GetCurrentProcess().MainModule?.FileName
                      ?? throw new InvalidOperationException("Cannot determine executable path.");
        return Path.GetDirectoryName(exePath)!;
    }

    // Full path to the running executable (the single-file host)
    public static string GetExecutingAssemblyFullName()
    {
        return System.Environment.ProcessPath
               ?? Process.GetCurrentProcess().MainModule?.FileName
               ?? throw new InvalidOperationException("Cannot determine executable path.");
    }

    public static string? GetApplicationPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Flexlib.exe");
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
      
    public static int GetSafeWindowHeight()
    {
        try
        {
            int h = Console.WindowHeight;
            if (h > 0) return h;
        }
        catch (IOException) { }
        return 25; // or a config value, or something sensible
    }

}


