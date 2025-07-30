using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flexlib.Infrastructure.Meta;

public static class BuildInfo
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    private static readonly FileVersionInfo _fileVersionInfo = FileVersionInfo.GetVersionInfo(_assembly.Location);

    public static string Version => _fileVersionInfo.ProductVersion ?? "unknown";
    public static string InformationalVersion => _fileVersionInfo.ProductVersion ?? "unknown";

    public static string SemanticVersion
    {
        get
        {
            // Match major.minor.patch
            var match = Regex.Match(Version, @"\d+\.\d+\.\d+");
            return match.Success ? match.Value : "0.0.0";
        }
    }

    public static string? BuildId
    {
        get
        {
            var info = _fileVersionInfo.ProductVersion;
            if (info != null && info.Contains("+build."))
            {
                var parts = info.Split("+build.");
                return parts.Length == 2 ? parts[1] : null;
            }
            return null;
        }
    }
}


