namespace Flexlib.Interface.Input.Commands;
public class ExitCommand : Command
{

    public ExitCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "exit";

    public override bool IsValid()
    {
        if (Options.Length < 1)
        {
            return true;
        }

        return false;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "Exit",
            Description = "Exits from an opened Flexlib instance.",
            Group = CommandGroups.TUI,
            Syntax = "exit",
            Options = new List<Option>()
        };
    }
}