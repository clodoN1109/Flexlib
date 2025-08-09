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
        ItemOrigin = options.Length > 0 ? options[0] : "";
        ItemName = options.Length > 1 ? options[1] : Infer.ItemNameFromOrigin(ItemOrigin);
        LibraryName = options.Length > 2 ? options[2] : "Default Library";
    }

    public override string Type => "new-item";

    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(LibraryName) && !string.IsNullOrWhiteSpace(ItemOrigin);
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-item",
            Description = "Creates a new item in the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib new-item <item origin> [item name] [library name]",
            Options = new List<Option>
            {
                new Option{
                    Name = "item origin",
                    Description = "The information necessary and sufficient to locate the item.",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "item name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = false,
                    DefaultValue = "Default Library" 

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
        ItemId      =   options.Length > 0 ? options[0]   :  "";
        NewName     =   options.Length > 1 ? options[1]   :  "";
        LibraryName =   options.Length > 2 ? options[2]   :  "";
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
            Syntax = "flexlib rename-item <item id> <new name> <library name>",
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
    public string LibraryName   { get; }
    public object ItemId        { get; }
    public string NewOrigin     { get; }

    public UpdateItemOriginCommand(string[] options)
    {
        ItemId      =   options.Length > 0 ? options[0]   :  "";
        NewOrigin   =   options.Length > 1 ? options[1]   :  "";
        LibraryName =   options.Length > 2 ? options[2]   :  "";
        Options     =   options;
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
            Meta = new List<string> {},
            Title = "update-origin",
            Description = "Updates the origin of the selected item.",
            Group = CommandGroups.Items,
            Syntax = "flexlib update-origin <item id> <new origin> <library name>",
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-item",
            Description = "Removes the selected item from the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib remove-item <item id> [library name]",
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
                    Mandatory = false,
                    DefaultValue = "Default Library"
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
        Options = options;
        ItemId =  options.Length > 0 ? options[0] : "";
        LibraryName = options.Length > 1 ? options[1] : "Default Library";
        Application = options.Length > 2 ? options[1] : "Default App";
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
            Syntax = "flexlib view-item <item id> [library name] [preferred application]",
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
                    Mandatory = false,
                    DefaultValue = "Default Library"
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
        LibraryName = options.Length > 0 ? options[0] : "Default Library";
        FilterSequence = options.Length > 1 ? options[1] : "";
        ItemName = options.Length > 2 ? options[2] : "";
        SortSequence = options.Length > 3 ? options[3] : "";
    }
    
    public override string Type => "list-items";

    public override bool IsValid()
    {

        if ((Options.Length > 0) && (Options.Length < 5))
        {
            return true;
        }

        return false;
        
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list items",
            Description = "Presents a filtered and sorted list of items of the selected library.",
            Group = CommandGroups.Items,
            Syntax = "flexlib list-items <library name> [\"filter sequence\"] [\"item name\"] [\"sort sequence\"]",
            Examples = new List<string> {
                "flexlib list-items Literature \"physics,math/Newton, Gottfried Leibniz/1780-1856\" \"optics,principles\" year/publisher",
                "flexlib list-items Cinema Ernst/*/1990-2021 \"\" budget/year/rating",
                "flexlib list-items Music * \"Sonata, Concerto\" year/artist"
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

