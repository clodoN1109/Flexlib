using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;


public abstract class Command : ParsedInput, IAction
{
    public abstract CommandUsageInfo GetUsageInfo();

    public abstract string Type { get; }

    public string[] Options { get; protected set; } = Array.Empty<string>();

    public bool IsSpecificHelp() => Options.Length > 0 && Options[0].ToLowerInvariant() == "help";
}










