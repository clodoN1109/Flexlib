using Flexlib.Common;
using System.IO;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.CLI;


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
        string? assemblyLocation = Env.GetExecutingAssemblyLocation();

        Name = options.Length > 0 ? options[0] : "";
        Path = options.Length > 1 ? options[1] : "";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib new-lib <name> [path]";
    } 
}

public class RemoveLibraryCommand : Command
{
    public string Name { get; }

    public RemoveLibraryCommand(string[] options)
    {
        Name = options.Length > 0 ? options[0] : "";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib remove-lib <name>";
    } 
}

public class NewItemCommand : Command
{
    public string LibraryName { get; }
    public string ItemName { get; }
    public string ItemOrigin { get; }

    public NewItemCommand(string[] options)
    {
        ItemOrigin = options.Length > 0 ? options[0] : "";
        ItemName = options.Length > 1 ? options[1] : Infer.ItemNameFromOrigin(ItemOrigin);
        LibraryName = options.Length > 2 ? options[2] : "Default Library";
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LibraryName) && !string.IsNullOrWhiteSpace(ItemOrigin);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib new-item <item origin> [item name] [library name]";
    }
}

public class RemoveItemCommand : Command
{
    public string LibraryName { get; }
    public object ItemId { get; }

    public RemoveItemCommand(string[] options)
    {
        ItemId =  options.Length > 0 ? options[0] : "";
        LibraryName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override bool IsValid()
    {
        return !TypeTests.IsNull(ItemId) && ItemId is string s && ( s != "" ) && !string.IsNullOrWhiteSpace(LibraryName);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib remove-item <item id> <library name>";
    }
}

public class ListLibrariesCommand : Command
{
    string[] Options;

    public ListLibrariesCommand(string[] options)
    {
        Options = options;
    }

    public override bool IsValid()
    {

        if ( Options.Length == 0 )
        {
            return true;
        }

        return false;
        
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib list-libs\n\n";
    }
}

public class ListItemsCommand : Command
{
    string[] Options;
    public string LibraryName { get; }
    public string FilterSequence { get; }
    public string SortSequence { get; }

    public ListItemsCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
        FilterSequence = options.Length > 1 ? options[1] : "";
        SortSequence = options.Length > 2 ? options[2] : "";
    }

    public override bool IsValid()
    {

        if ((Options.Length > 0) && (Options.Length < 4))
        {
            return true;
        }

        return false;
        
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib list-items <library name> [filter sequence] [sort sequence]\n\n" +
            "[filter sequence]: <property-value>[/property-value ...] \n\n" +
            "[sort sequence]: <property>[/<property ...] "
            ;
    }
}

public class GetLibraryLayoutCommand : Command
{
    string[] Options;
    public string LibraryName { get; }

    public GetLibraryLayoutCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
    }

    public override bool IsValid()
    {

        if ((Options.Length > 0) && (Options.Length < 2))
        {
            return true;
        }

        return false;
        
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib get-layout <library name>\n\n"
            ;
    }
}

public class SetLibraryLayoutCommand : Command
{
    string[] Options;
    public string LibraryName { get; }
    public string LayoutString { get; }

    public SetLibraryLayoutCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
        LayoutString = options.Length > 1 ? options[1] : "";
    }

    public override bool IsValid()
    {

        if (Options.Length == 2)
        {
            return true;
        }

        return false;
        
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib set-layout <library name> <layout>\n\n" +
            "<layout>: <property>[/property ...] \n\n"
            ;
    }
}


public class FetchFilesCommand : Command
{
    string[] Options;
    public string? LibraryName { get; } 

    public FetchFilesCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
    }

    public override bool IsValid()
    {
        return (Options.Length <= 1);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib refetch [library name]";
    }
}

public class NewPropertyCommand : Command
{
    string[] Options;
    public string PropName { get; } 
    public string PropType { get; } 
    public string LibName { get; } 

    public NewPropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        LibName = options.Length > 1 ? options[1] : "Default Library";
        PropType = options.Length > 2 ? options[2] : "string";
    }

    public override bool IsValid()
    {
        return (Options.Length >= 1 && Options.Length <= 3);
    }
    
    public override string UsageInstructions()
    {
        return
            "Usage:  flexlib new-prop <property name> [library name] [property type] \n\n" +
            "   <property name>   The name of the new property.\n\n" +
            "   [property type]   (Optional) The type of the property.\n\n" +
            "       Supported types: string (default), integer, decimal, float, bool, list\n\n" +
            "   [library name]    (Optional) The name of the target library.\n\n";
    }

}

public class ListPropertiesCommand : Command
{
    string[] Options;
    public string LibName { get; } 
    public string ItemName { get; } 

    public ListPropertiesCommand(string[] options)
    {
        Options = options;
        LibName = options.Length > 0 ? options[0] : "Default Library";
        ItemName = options.Length > 1 ? options[1] : "";
    }

    public override bool IsValid()
    {
        return (Options.Length > 0 && Options.Length < 3);
    }
    
    public override string UsageInstructions()
    {
        return
            "Usage:  flexlib list-props [library name] [item name]";
    }

}

public class SetPropertyCommand : Command
{
    string[] Options;
    public string PropName { get; } 
    public string NewValue { get; } 
    public string ItemName { get; } 
    public string LibName { get; } 

    public SetPropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        NewValue = options.Length > 1 ? options[1] : "";
        ItemName = options.Length > 2 ? options[2] : "";
        LibName = options.Length > 3 ? options[3] : "Default Library";
    }

    public override bool IsValid()
    {
        return (Options.Length > 1 && Options.Length < 5);
    }
    
    public override string UsageInstructions()
    {
        return
            "Usage:  flexlib set-prop <property name> <new value> <item name> [library name]";
    }

}

public class RemovePropertyCommand : Command
{
    string[] Options;
    public string PropName { get; } 
    public string LibName { get; } 

    public RemovePropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override bool IsValid()
    {
        return (Options.Length > 1 && Options.Length < 3);
    }
    
    public override string UsageInstructions()
    {
        return
            "Usage:  flexlib remove-prop <property name> [library name]";
    }

}

public abstract class CommentCommand : Command
{
    public string[] Options { get; }
    public string ItemName { get; }

    protected CommentCommand(string[] options)
    {
        Options = options;
        ItemName = options.Length > 0 ? options[0] : "";
    }

}

public class NewCommentCommand : CommentCommand
{
    public string? Comment { get; set; }
    public string LibName { get; set; }

    public NewCommentCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
        Comment = options.Length > 2 ? options[2] : "";
    }
    
    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length <= 3;
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib new-comment <item name> [library name] [comment] \n";
    }
}

public class ListCommentsCommand : CommentCommand
{
    public string LibName { get; set; }
    
    public ListCommentsCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length < 3;
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib list-comments <item name> [library name]\n";
    }
}

public class EditCommentCommand : CommentCommand
{
    public string CommentId;
    public string LibName { get; set; }

    public EditCommentCommand(string[] options) : base(options) { 
        
        CommentId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib edit-comment <item name> <comment id> [library name]\n";
    }
}

public class RemoveCommentCommand : CommentCommand
{
    public string CommentId;
    public string LibName { get; set; }

    public RemoveCommentCommand(string[] options) : base(options) { 
        
        CommentId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib remove-comment <item name> <comment id> [library name]\n";
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
        var availableCommands = CommandsList.GetAvailableCommandsList();
        var commandsLine = string.Join(" ", availableCommands);

        int consoleWidth;
        try
        {
            consoleWidth = Console.WindowWidth;
        }
        catch
        {
            consoleWidth = 80; // fallback if not in console
        }

        string title = "░░░░ Flexlib CLI ";
        string titleBar = title + new string('░', Math.Max(0, consoleWidth - title.Length));
        string bottomBar = new string('░', consoleWidth);

        return string.Join("\n", new[]
        {
            titleBar,
            "",
            "   usage:      flexlib {command}",
            "",
            "commands:",
            "",
            "\t" + commandsLine,
            "",
            bottomBar,
            ""
        });
    }
}

public static class CommandsList{
    
    public static List<string> GetAvailableCommandsList()
    {
        return new List<string>{

            "❔     help",
            "\n\n\t🏛      new-lib",
            "list-libs",
            "remove-lib",
            "set-layout",
            "get-layout",
            "\n\n\t🕮      new-item", 
            "list-items",
            "remove-item",
            "view-item",
            "\n\n\t𝒜      new-comment",
            "list-comments",
            "edit-comment",
            "remove-comment",
            "\n\n\t📐     new-prop",
            "list-props",
            "set-prop",
            "remove-prop",
            "\n\n\t🢃      fetch-files",
            "\n\n\t🗔      gui",


        };

    }

}
