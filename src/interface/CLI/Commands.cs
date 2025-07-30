using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;

public abstract class Command : Flexlib.Interface.Input.Action
{
    public abstract UsageInfo GetUsageInfo();

    public string[] Options { get; protected set; } = Array.Empty<string>();

    public bool IsHelp()
    {
        if (Options.Length > 0 && Options[0].ToLowerInvariant() == "help")
        {
            return true;
        }

        return false;
    }
}

public class NewUserCommand : Command
{

    public NewUserCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "new-user";

    public override bool IsValid()
    {
        if (Options.Length == 0)
        {
            return true;
        }

        return false;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-user",
            Description = "Creates a new user.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib new-user",
            Options = new List<CommandOption>()
        };
    }
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

    public override string Type => "new-lib";

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-lib",
            Description = "Creates a new library with the selected name and located at the selected path.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib new-lib <library name> [library path]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "path",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false
                }
            }
        };
    }
}

public class RemoveLibraryCommand : Command
{
    public string Name { get; }

    public RemoveLibraryCommand(string[] options)
    {
        Name = options.Length > 0 ? options[0] : "";
    }

    public override string Type => "remove-lib";

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-lib",
            Description = "Removes the selected library and all its items.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib remove-lib <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
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

    public override string Type => "new-item";

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LibraryName) && !string.IsNullOrWhiteSpace(ItemOrigin);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-item",
            Description = "Creates a new item in the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib new-item <item origin> [item name] [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item origin",
                    Description = "The information necessary and sufficient to locate the item.",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "item name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false
                },
                
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false,
                    DefaultValue = "Default Library" 

                },
            }
        };
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

    public override string Type => "remove-item";

    public override bool IsValid()
    {
        return !TypeTests.IsNull(ItemId) && ItemId is string s && ( s != "" ) && !string.IsNullOrWhiteSpace(LibraryName);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-item",
            Description = "Removes the selected item from the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib remove-item <item id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false,
                    DefaultValue = "Default Library"
                },

            }
        };
    }
}

public class ListLibrariesCommand : Command
{

    public ListLibrariesCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "list-libs";

    public override bool IsValid()
    {

        if ( Options.Length == 0 )
        {
            return true;
        }

        return false;
        
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list-libs",
            Description = "Lists all accessible existing libraries.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib list-libs",
            Options = new List<CommandOption>()
        };
    }
}

public class ListItemsCommand : Command
{
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
    
    public override string Type => "list-items";

    public override bool IsValid()
    {

        if ((Options.Length > 0) && (Options.Length < 4))
        {
            return true;
        }

        return false;
        
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list items",
            Description = "Presents a filtered and sorted list of items of the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib list-items <library name> [\"filter sequence\"] [\"sort sequence\"]",
            Examples = new List<string> {
                "flexlib list-items Literature \"physics,math/Newton, Gottfried Leibniz/1780-1856\" year/publisher",
                "flexlib list-items Cinema Ernst/*/1990-2021 year",
                "flexlib list-items Music * name"
                },
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption{
                    Name = "filter sequence",
                    Description = "A sequence of properties that sequencially filters a library based on its current layout.",
                    Syntax = "<property-value>[/property-value ...]",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false
                },
                
                new CommandOption{
                    Name = "sort sequence",
                    Description = "A sequence of properties that sequencially sorts a library based on its current layout.",
                    Syntax = "<property>[/<property ...]",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false
                }

            }
        };
    }
}

public class GetLibraryLayoutCommand : Command
{
    public string LibraryName { get; }

    public GetLibraryLayoutCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
    }

    public override string Type => "get-layout";

    public override bool IsValid()
    {

        if ((Options.Length > 0) && (Options.Length < 2))
        {
            return true;
        }

        return false;
        
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "get-layout",
            Description = "Gets the current library layout.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib get-layout <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }

}

public class SetLibraryLayoutCommand : Command
{
    public string LibraryName { get; }
    public string LayoutString { get; }

    public SetLibraryLayoutCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
        LayoutString = options.Length > 1 ? options[1] : "";
    }

    public override string Type => "set-layout";

    public override bool IsValid()
    {

        if (Options.Length == 2)
        {
            return true;
        }

        return false;
        
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "set-layout",
            Description = "Redefines the selected library layout.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib set-layout <library name> <layout>",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "layout",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true,
                    Syntax = "<property>[/property ...]"
                }

            }
        };
    }
}

public class FetchFilesCommand : Command
{
    public string? LibraryName { get; } 

    public FetchFilesCommand(string[] options)
    {
        Options = options;
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
    }
    
    public override string Type => "fetch-files";

    public override bool IsValid()
    {
        return (Options.Length <= 1);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "fetch-files",
            Description = "Fetches the selected library's files from the defined origins of items and saves them to the local system.",
            Group = CommandGroups.Storage,
            Syntax = "flexlib fetch-files [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false,
                    DefaultValue = "Default Library"
                }
            }
        };
    }

}

public class NewPropertyCommand : Command
{
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
    
    public override string Type => "new-prop";

    public override bool IsValid()
    {
        return (Options.Length >= 1 && Options.Length <= 3);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-prop",
            Description = "Defines a new property for the selected library and all its items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib new-prop <property name> [library name] [property type]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                
                new CommandOption{
                    Name = "property type",
                    Description = "The property type/domain.",
                    OptionDomain = new VariableDomain("string", "integer", "decimal", "float", "bool", "list"),
                    DefaultValue = "string"
                },

            }
        };
    }

}

public class ListPropertiesCommand : Command
{
    public string LibName { get; } 
    public string ItemName { get; } 

    public ListPropertiesCommand(string[] options)
    {
        Options = options;
        LibName = options.Length > 0 ? options[0] : "Default Library";
        ItemName = options.Length > 1 ? options[1] : "";
    }

    public override string Type => "list-props";
    
    public override bool IsValid()
    {
        return (Options.Length > 0 && Options.Length < 3);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list properties",
            Description = "List all defined properties for the selected library.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib list-props [library name] [item name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                
                new CommandOption{
                    Name = "item name",
                    OptionDomain = new VariableDomain()
                },

            }
        };
    }
}

public class SetPropertyCommand : Command
{
    public string PropName { get; } 
    public string NewValue { get; } 
    public object ItemId { get; } 
    public string LibName { get; } 

    public SetPropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        NewValue = options.Length > 1 ? options[1] : "";
        ItemId = options.Length > 2 ? options[2] : "";
        LibName = options.Length > 3 ? options[3] : "Default Library";
    }
    
    public override string Type => "set-prop";

    public override bool IsValid()
    {
        return (Options.Length > 1 && Options.Length < 5);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "set-prop",
            Description = "Defines a new property for the selected library and all its items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib set-prop <property name> <new value> <item id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "new value",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }


            }
        };
    }

}

public class RemovePropertyCommand : Command
{
    public string PropName { get; } 
    public string LibName { get; } 

    public RemovePropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }
    
    public override string Type => "remove-prop";

    public override bool IsValid()
    {
        return (Options.Length > 1 && Options.Length < 3);
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-prop",
            Description = "Removes a property definition from a selected library and the corresponding values for all the library's items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib remove-prop <property name> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }



}

public abstract class CommentCommand : Command
{
    public object ItemId { get; } 

    protected CommentCommand(string[] options)
    {
        Options = options;
        ItemId = options.Length > 0 ? options[0] : "";
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
    
    public override string Type => "new-comment";
    
    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length <= 3;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-comment",
            Description = "Creates a new comment for the selected library item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib new-comment <item id> [library name] [comment]" ,
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                new CommandOption{
                    Name = "comment",
                    OptionDomain = new VariableDomain()
                }
            }
        };
    }


}

public class ListCommentsCommand : CommentCommand
{
    public string LibName { get; set; }
    
    public ListCommentsCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override string Type => "list-comments";

    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length < 3;
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list-comments",
            Description = "List all comments from a selected library item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib list-comments <item id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
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
    
    public override string Type => "edit-comment";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "edit-comment",
            Description = "Edit a selected commment.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib edit-comment <item id> <comment id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "comment id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
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
    
    public override string Type => "remove-comment";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-comment",
            Description = "Remove a comment from a selected item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib remove-comment <item id> <comment id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "comment id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }

}

public class HelpCommand : Command
{
    public HelpCommand(string[] options) { }
    public HelpCommand() { }

    public override bool IsValid() => true;
    public override string Type => "help";

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> { Env.Version, Env.BuildId },
            Title = "CLI HELP",
            Description = "Flexlib is a lightweight system for designing flexible and interconnected libraries with just a few keystrokes.",
            Group = CommandGroups.Help,
            Syntax = "flexlib <command>",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "command",
                    Description = "Specifies the action to be invoked in the Flexlib application.",
                    OptionDomain = new VariableDomain( CommandsList.Items ),
                    Mandatory = true
                }
            }
        };
    }
}   

