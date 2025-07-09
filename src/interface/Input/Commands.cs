using Flexlib.Common;
using System.IO;

namespace Flexlib.Interface;


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
        string defaultLocation = string.IsNullOrWhiteSpace(assemblyLocation)
            ? ""
            : System.IO.Path.Combine(assemblyLocation, "data");

        Name = options.Length > 0 ? options[0] : "";
        Path = options.Length > 1 ? options[1] : defaultLocation;
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Path);
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib new <name> [path]";
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

public class AddItemCommand : Command
{
    public string LibraryName { get; }
    public string ItemName { get; }
    public string ItemOrigin { get; }

    public AddItemCommand(string[] options)
    {
        ItemOrigin = options.Length > 0 ? options[0] : "";
        LibraryName = options.Length > 1 ? options[1] : "Default Library";
        ItemName = options.Length > 2 ? options[2] : Infer.ItemNameFromOrigin(ItemOrigin);
    }

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LibraryName) && !string.IsNullOrWhiteSpace(ItemOrigin);
    }
    
    public override string UsageInstructions()
    {
        return "Usage: flexlib add-item <item origin> [library name] [item name]";
    }
}

public class RefreshCommand : Command
{
    string[] Options;
    public string? LibraryName { get; } 

    public RefreshCommand(string[] options)
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
        return "Usage: flexlib refresh [library name]";
    }
}

public class AddPropertyCommand : Command
{
    string[] Options;
    public string PropName { get; } 
    public string PropType { get; } 
    public string LibName { get; } 

    public AddPropertyCommand(string[] options)
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
            "Usage:  flexlib add-prop <property name> [library name] [property type] \n\n" +
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
        return (Options.Length >= 1 && Options.Length <=2);
    }
    
    public override string UsageInstructions()
    {
        return
            "Usage:  flexlib list-props [library name] [item name]";
    }

}

public class EditPropertyCommand : Command
{
    string[] Options;
    public string PropName { get; } 
    public string NewValue { get; } 
    public string ItemName { get; } 
    public string LibName { get; } 

    public EditPropertyCommand(string[] options)
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
            "Usage:  flexlib edit-prop <property name> <new value> <item name> [library name]";
    }

}

public abstract class CommentCommand : Command
{
    public string[] Options { get; }
    public string ItemName { get; }
    public string LibName { get; set; }

    protected CommentCommand(string[] options)
    {
        Options = options;
        ItemName = options.Length > 0 ? options[0] : "";
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

}

public class MakeCommentCommand : CommentCommand
{
    public string? Comment { get; set; }

    public MakeCommentCommand(string[] options) : base(options) 
    {
        Comment = options.Length > 2 ? options[2] : "";
    }
    
    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length <= 3;
    }

    public override string UsageInstructions()
    {
        return "Usage: flexlib make-comment <item name> [library name] [comment] \n";
    }
}

public class ListCommentsCommand : CommentCommand
{
    public ListCommentsCommand(string[] options) : base(options) { }

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

        return
            "Usage: flexlib {command}\n\n" +
            "commands:\n" +
            "\n\t" + commandsLine;
    }
}

public static class CommandsList{
    
    public static List<string> GetAvailableCommandsList()
    {
        return new List<string>{

            "new",
            "\n\tadd-item",
            "\n\tmake-comment",
            "list-comments",
            "edit-comment",
            "\n\tadd-prop",
            "list-props",
            "edit-prop",
            "\n\trefresh",
            "\n\tremove-lib",
            "remove-item",
            "remove-prop",
            "remove-comment"


        };

    }

}

