using Flexlib.Application.Ports;
using Flexlib.Interface.Input;


namespace Flexlib.Interface.Input;


public abstract class Command : ParsedInput, IAction
{
    public abstract CommandUsageInfo GetUsageInfo();

    public abstract string Type { get; }

    public string[] Options { get; protected set; } = Array.Empty<string>();

    public bool IsSpecificHelp() => Options.Length > 0 && Options[0].ToLowerInvariant() == "help";

    public static bool IsKnownCommandName(string commandName) => ActionsList.Items.Contains(commandName);

}










