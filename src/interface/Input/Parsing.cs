using Flexlib.Common;

namespace Flexlib.Interface;


public static class Parsing
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
            "add-prop" => new AddPropertyCommand(options),
            "list-props" => new ListPropertiesCommand(options),
            "edit-prop" => new EditPropertyCommand(options),
            "make-comment" => new MakeCommentCommand(options),
            "list-comments" => new ListCommentsCommand(options),
            "edit-comment" => new EditCommentCommand(options),
            "refresh" => new RefreshCommand(options), 
            "remove-lib" => new RemoveLibraryCommand(options),    
            _     => new UnknownCommand($"Unknown command: {command}")
        };
    }
}

public abstract class ParsedInput
{
    public abstract bool IsValid();
}

