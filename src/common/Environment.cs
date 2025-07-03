namespace Flexlib.Common;

using System.Reflection;
using System.IO;

public static class Env{
    
    public static string? GetExecutingAssemblyLocation()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }    
    
}


