using Flexlib.Common;

namespace Flexlib.Interface;

public static class Input
{
    public static ParsedInput Parse(string[] args)
    {
        if (args.Length == 0)
            return new UnknownCommand("");

        string command = args[0];
        string[] options = args.Skip(1).ToArray();

        return command.ToLower() switch
        {
            "new" => new NewLibraryCommand(options),
            "add-item" => new AddItemCommand(options),
            "refresh" => new RefreshCommand(options), 
            _     => new UnknownCommand($"Unknown command: {command}")
        };
    }
}

public abstract class ParsedInput
{
    public abstract bool IsValid();
}

public abstract class Command : ParsedInput
{
    public abstract string UsageInstructions(); 
}

public class NewLibraryCommand : Command
{
    public string Name { get; }
    public string Path { get; }

    public NewLibraryCommand(string[] options)
    {
        Name = options.Length > 0 ? options[0] : "";
        Path = options.Length > 1 ? options[1] : "";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Path);
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib new {name} {path}";
    } 
}

public class AddItemCommand : Command
{
    public string LibraryName { get; }
    public string ItemName { get; }
    public string ItemOrigin { get; }

    public AddItemCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        ItemOrigin = options.Length > 1 ? options[1] : "";
        ItemName = options.Length > 2 ? options[2] : "";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LibraryName) && !string.IsNullOrWhiteSpace(ItemOrigin);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib add-item {library name} {item origin} <item name>";
    }
}

public class RefreshCommand : Command
{
    string[] Options;
    public string? LibraryName { get; } 

    public RefreshCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : null;
    }

    public override bool IsValid()
    {
        return (Options.Length <= 1);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib refresh [library name]";
    }
}

public class UnknownCommand : Command
{
    public string Message { get; }

    public UnknownCommand(string message)
    {
        Message = message;
    }

    public override bool IsValid() => false;

    public override string UsageInstructions()
    {
        var availableCommands = Commands.GetAvailableCommandsList();
        var commandsLine = string.Join(" ", availableCommands);

        return
            "Usage: flexlib {command}\n\n" +
            "commands:\n" +
            "\n\t" + commandsLine;
    }
}


public class Commands{
    
    public static List<string> GetAvailableCommandsList()
    {
        return new List<string>{

            "new",
            "add-item",
            "refresh"

        };

    }

}
