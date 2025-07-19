using Flexlib.Common;
using Flexlib.Interface.GUI;

namespace Flexlib.Interface.Input;


public static class Parsing
{
    public static ParsedInput Parse(string[] args)
    {
        if (args.Length == 0)
            return new GUIStartUp(new GUIConfig());

        string command = args[0];
        string[] options = args.Skip(1).ToArray();

        return command.ToLower() switch
        {
            "new-lib"           => new NewLibraryCommand(options),
            "new-item"      => new NewItemCommand(options),
            "list-items"    => new ListItemsCommand(options),
            "get-layout"    => new GetLibraryLayoutCommand(options),
            "set-layout"    => new SetLibraryLayoutCommand(options),
            "new-prop"      => new NewPropertyCommand(options),
            "list-props"    => new ListPropertiesCommand(options),
            "edit-prop"     => new EditPropertyCommand(options),
            "new-comment"  => new NewCommentCommand(options),
            "list-comments" => new ListCommentsCommand(options),
            "edit-comment"  => new EditCommentCommand(options),
            "fetch-files"       => new FetchFilesCommand(options), 
            "remove-lib"    => new RemoveLibraryCommand(options),
            "help"          => new UnknownCommand($"Unknown command: {command}"),
            _               => new UnknownCommand($"Unknown command: {command}")
        };
    }
}

public abstract class ParsedInput
{
    public abstract bool IsValid();
}

