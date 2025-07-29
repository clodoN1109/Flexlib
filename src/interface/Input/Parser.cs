using Flexlib.Common;
using Flexlib.Interface.GUI;
using Flexlib.Interface.CLI;

namespace Flexlib.Interface.Input;


public static class Parsing
{
    public static ParsedInput Parse(string[] args)
    {
        if (args.Length == 0)
            return new HelpCommand();

        string firstArg = args[0];
        string[] options = args.Skip(1).ToArray();

        return firstArg.ToLower() switch
        {
            "new-user"           => new NewUserCommand(options),
            "new-lib"           => new NewLibraryCommand(options),
            "new-item"      => new NewItemCommand(options),
            "remove-item"      => new RemoveItemCommand(options),
            "list-libs"    => new ListLibrariesCommand(options),
            "list-items"    => new ListItemsCommand(options),
            "get-layout"    => new GetLibraryLayoutCommand(options),
            "set-layout"    => new SetLibraryLayoutCommand(options),
            "new-prop"      => new NewPropertyCommand(options),
            "list-props"    => new ListPropertiesCommand(options),
            "set-prop"     => new SetPropertyCommand(options),
            "remove-prop"     => new RemovePropertyCommand(options),
            "new-comment"  => new NewCommentCommand(options),
            "list-comments" => new ListCommentsCommand(options),
            "edit-comment"  => new EditCommentCommand(options),
            "remove-comment"  => new RemoveCommentCommand(options),
            "fetch-files"       => new FetchFilesCommand(options), 
            "remove-lib"    => new RemoveLibraryCommand(options),
            "gui"          => new GUIStartUp(new GUIConfig()),
            "help"          => new HelpCommand(options),
            _               => new UnknownInput($"Unknown input: {firstArg}")
        };
    }
}

public abstract class ParsedInput
{
    public abstract bool IsValid();
}

public class UnknownInput : ParsedInput
{
    public string Message { get; }

    public UnknownInput(string message)
    {
        Message = message;
    }

    public override bool IsValid() => false;
}

