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
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
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
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    Mandatory = true
                },

                new CommandOption{
                    Name = "filter sequence",
                    Description = "A sequence of properties that sequencially filters a library based on its current layout.",
                    Syntax = "<property-value>[/property-value ...]",
                },
                
                new CommandOption{
                    Name = "sort sequence",
                    Description = "A sequence of properties that sequencially sorts a library based on its current layout.",
                    Syntax = "<property>[/<property ...]",
                },
                
                new CommandOption{
                    Name = "item name",
                    Description = "A substring of the target item's name.",
                }

            }
        };
    }
}

