using System;
using System.Linq;
using System.Text;
using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public static string LogoLine(int totalWidth)
    {
        string idInfo;

#if DEBUG
        idInfo = $"{Env.BuildId}";
#else
        idInfo = $"v{Env.Version}";
#endif

        string logo = Components.Logo();

        string spaceBetween = new string(' ', totalWidth - logo.Length - idInfo.Length);
        
        string logoLine = logo + spaceBetween + idInfo;

        return logoLine;
        
    }

}

