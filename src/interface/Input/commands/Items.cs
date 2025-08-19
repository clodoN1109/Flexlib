using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;

public class NewItemCommand : Command
{
    public string LibraryName { get; }
    public string ItemName { get; }
    public string ItemOrigin { get; }

    public NewItemCommand(string[] options)
    {
        LibraryName = options.Length  > 0 ? options[0] : "";
        ItemName    = options.Length  > 1 ? options[1] : "";
        ItemOrigin  = options.Length  > 2 ? options[2] : "";
        Options     = options;
    }

    public override string Type => "new-item";

    public override bool IsValid()
    {
        return Options.Length < 4;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-item",
            Description = "Creates a new item in the selected library.",
            Group = CommandGroups.Items,
            Syntax = "new-item <library name> <item name> [item origin]",
            Options = new List<Option>
            {
                new Option{
                    Name = "item origin",
                    Description = "The information necessary and sufficient to locate the item.",
                    OptionDomain = new VariableDomain(),
                },
                
                new Option{
                    Name = "item name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true

                },
            }
        };
    }
}

public class RenameItemCommand : Command
{
    public string LibraryName   { get; }
    public string NewName       { get; }
    public object ItemId        { get; }

    public RenameItemCommand(string[] options)
    {
        LibraryName =   options.Length > 0 ? options[0]   :  "";
        ItemId      =   options.Length > 1 ? options[1]   :  "";
        NewName     =   options.Length > 2 ? options[2]   :  "";
        Options     =   options;
    }

    public override string Type => "remove-item";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "rename-item",
            Description = "Renames the selected item.",
            Group = CommandGroups.Items,
            Syntax = "rename-item <library name> <item id> <new name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "new name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

            }
        };
    }
}

public class UpdateItemOriginCommand : Command
{
    public string LibraryName { get; }
    public object ItemId { get; }
    public string NewOrigin { get; }

    public UpdateItemOriginCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        ItemId      = options.Length > 1 ? options[1] : "";
        NewOrigin   = options.Length > 2 ? options[2] : "";
        Options     = options;
    }

    public override string Type => "update-origin";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "update-origin",
            Description = "Updates the origin of the selected item.",
            Group = CommandGroups.Items,
            Syntax = "update-origin <library name> <item id> <new origin>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "new origin",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

            }
        };
    }
}

public class GetItemOriginCommand : Command
{
    public string LibraryName   { get; }
    public object ItemId        { get; }

    public GetItemOriginCommand(string[] options)
    {
        LibraryName =   options.Length > 0 ? options[0]   :  "";
        ItemId      =   options.Length > 1 ? options[1]   :  "";
        Options     =   options;
    }

    public override string Type => "get-origin";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "get-origin",
            Description = "Get the current origin for the selected item.",
            Group = CommandGroups.Items,
            Syntax = "get-origin <library name> <item id>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
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
        LibraryName = options.Length > 0 ? options[0] : "";
        ItemId      = options.Length > 1 ? options[1] : "";
        Options     = options;
    }

    public override string Type => "remove-item";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "remove-item",
            Description = "Removes the selected item from the selected library.",
            Group = CommandGroups.Items,
            Syntax = "remove-item <library name> <item id>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

            }
        };
    }
}

public class ViewItemCommand : Command
{
    public string LibraryName { get; }
    public object ItemId { get; }
    public string Application{ get; }
    
    public ViewItemCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        ItemId      = options.Length > 1 ? options[1] : "";
        Application = options.Length > 2 ? options[1] : "Default App";
        Options     = options;
    }

    public override string Type => "view-item";

    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length < 4;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "view-item",
            Description = "Opens for visualization the selected item from the selected library.",
            Group = CommandGroups.Items,
            Syntax = "view-item <library name> <item id> [preferred application]",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "preferred application",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false,
                    DefaultValue = "Default Application"
                },

            }
        };
    }
}

public class ListItemsCommand : Command
{
    public string LibraryName { get; }
    public string ItemName { get; }
    public string FilterSequence { get; }
    public string SortSequence { get; }

    public ListItemsCommand(string[] options)
    {
        Options = options;
        LibraryName     = options.Length > 0 ? options[0] : "";
        FilterSequence  = options.Length > 1 ? options[1] : "";
        ItemName        = options.Length > 2 ? options[2] : "";
        SortSequence    = options.Length > 3 ? options[3] : "";
    }
    
    public override string Type => "list-items";

    public override bool IsValid()
    {

        return Options.Length > 0 && Options.Length < 5;
        
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list items",
            Description = "Presents a filtered and sorted list of items of the selected library.",
            Group = CommandGroups.Items,
            Syntax = "list-items <library name> [\"filter sequence\"] [\"item name\"] [\"sort sequence\"]",
            Examples = new List<string> {
                "list-items Literature \"physics,math/Newton, Gottfried Leibniz/1780-1856\" \"optics,principles\" year/publisher",
                "list-items Cinema Ernst/*/1990-2021 \"\" budget/year/rating",
                "list-items Music * \"Sonata, Concerto\" year/artist"
                },
            Options = new List<Option>
            {
                new Option{
                    Name = "library name",
                    Mandatory = true
                },

                new Option{
                    Name = "filter sequence",
                    Description = "A sequence of properties that sequencially filters a library based on its current layout.",
                    Syntax = "<property-value>[/property-value ...]",
                },
                
                new Option{
                    Name = "sort sequence",
                    Description = "A sequence of properties that sequencially sorts a library based on its current layout.",
                    Syntax = "<property>[/<property ...]",
                },
                
                new Option{
                    Name = "item name",
                    Description = "A substring of the target item's name.",
                }

            }
        };
    }
}

