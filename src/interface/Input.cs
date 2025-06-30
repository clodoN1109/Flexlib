namespace Flexlib.Interface;

public static class Input
{
    public static ParsedInput Parse(string[] args)
    {
        if (args.Length == 0)
            return new InvalidCommand("");

        string command = args[0];
        string[] options = args.Skip(1).ToArray();

        return command.ToLower() switch
        {
            "new" => new NewLibraryCommand(options),
            "update" => new UpdateLibraryCommand(options),
            _     => new InvalidCommand($"Unknown command: {command}")
        };
    }
}

public abstract class ParsedInput
{
    public abstract bool IsValid();
}

public abstract class Command : ParsedInput
{
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
}

public class UpdateLibraryCommand : Command
{
    public string Id { get; }
    public string NewName { get; }

    public UpdateLibraryCommand(string[] options)
    {
        Id = options.Length > 0 ? options[0] : "";
        NewName = options.Length > 1 ? options[1] : "";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(NewName);
    }
}

public class InvalidCommand : Command
{
    public string Message { get; }

    public InvalidCommand(string message)
    {
        Message = message;
    }

    public override bool IsValid() => false;
}

