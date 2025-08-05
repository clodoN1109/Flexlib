using Flexlib.Interface.GUI;
using Flexlib.Application.Ports;
using Flexlib.Interface.CLI;

namespace Flexlib.Interface.Input;


public static partial class Input
{
    public static ParsedInput Parse(Normalized input)
    {
        var args = input.Args;
        if (args.Length == 0)
            return new HelpCommand();

        string firstArg = args[0];
        string[] options = args.Skip(1).ToArray();

        return firstArg.ToLower() switch
        {
            "signup"         => new NewUserCommand(options),
            "login"          => new LoginCommand(options),
            "logout"         => new LogoutCommand(options),
            "new-lib"        => new NewLibraryCommand(options),
            "new-item"       => new NewItemCommand(options),
            "remove-item"    => new RemoveItemCommand(options),
            "view-item"      => new ViewItemCommand(options),
            "list-libs"      => new ListLibrariesCommand(options),
            "list-items"     => new ListItemsCommand(options),
            "get-layout"     => new GetLibraryLayoutCommand(options),
            "set-layout"     => new SetLibraryLayoutCommand(options),
            "new-prop"       => new NewPropertyCommand(options),
            "list-props"     => new ListPropertiesCommand(options),
            "set-prop"       => new SetPropertyCommand(options),
            "remove-prop"    => new RemovePropertyCommand(options),
            "new-comment"    => new NewCommentCommand(options),
            "list-comments"  => new ListCommentsCommand(options),
            "edit-comment"   => new EditCommentCommand(options),
            "remove-comment" => new RemoveCommentCommand(options),
            "fetch-files"    => new FetchFilesCommand(options),
            "rebalance"      => new RebalanceLocalStorageCommand(options),
            "remove-lib"     => new RemoveLibraryCommand(options),
            "gui"            => new GUIStartUp(new GUIConfig()),
            "help"           => new HelpCommand(options),
            _                => new UnknownInput($"Unknown input: {firstArg}")
        };
    }
}

public abstract class ParsedInput : ProcessedInput {}

public class UnknownInput : ParsedInput
{
    public string Message { get; }

    public UnknownInput(string message)
    {
        Message = message;
    }

    public override bool IsValid() => false;
}

