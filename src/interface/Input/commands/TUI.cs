namespace Flexlib.Interface.Input.Commands;

public class ClearCommand : Command
{

    public ClearCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "clear";

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
            Title = "Clear",
            Description = "Clears the output presented for the current context.",
            Group = CommandGroups.TUI,
            Syntax = "clear",
            Options = new List<Option>()
        };
    }


}

public class LightModeCommand : Command
{

    public LightModeCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "light";

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
            Title = "Light Mode",
            Description = "Selects light mode for the given context.",
            Group = CommandGroups.TUI,
            Syntax = "light",
            Options = new List<Option>()
        };
    }
}

public class DarkModeCommand : Command
{

    public DarkModeCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "dark";

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
            Title = "Dark Mode",
            Description = "Selects dark mode for the given context.",
            Group = CommandGroups.TUI,
            Syntax = "dark",
            Options = new List<Option>()
        };
    }
}