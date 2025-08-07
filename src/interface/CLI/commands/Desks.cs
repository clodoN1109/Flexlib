using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;

public class NewDeskCommand : Command
{
    public string LibraryName   { get; }
    public string DeskName      { get; }

    public NewDeskCommand(string[] options)
    {
        DeskName    =   options.Length > 0 ? options[0]   :  "";
        LibraryName =   options.Length > 1 ? options[1]   :  "";
        Options     =   options;
    }

    public override string Type => "new-desk";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-desk",
            Description = "Creates a new desk to organize borrowed items and track their progress.",
            Group = CommandGroups.Desks,
            Syntax = "flexlib new-desk <desk name> <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "desk name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

            }
        };
    }
}

public class ListDesksCommand : Command
{
    public string LibraryName { get; }

    public ListDesksCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        Options = options;
    }

    public override string Type => "list-desks";

    public override bool IsValid()
    {
        return Options.Length == 1;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> { },
            Title = "list-desks",
            Description = "Lists all desks in the specified library.",
            Group = CommandGroups.Desks,
            Syntax = "flexlib list-desks <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }
}

public class ViewDeskCommand : Command
{
    public string DeskId        { get; }
    public string LibraryName   { get; }
    public string SortSequence  { get; }

    public ViewDeskCommand(string[] options)
    {
        DeskId          = options.Length > 0 ? options[0] : "";
        LibraryName     = options.Length > 1 ? options[1] : "";
        SortSequence    = options.Length > 2 ? options[2] : "";
        Options = options;
    }

    public override string Type => "view-desk";

    public override bool IsValid()
    {
        return (Options.Length > 1 && Options.Length < 3);
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> { },
            Title = "view-desk",
            Description = "Displays the items and progress for a specific desk.",
            Group = CommandGroups.Desks,
            Syntax = "flexlib view-desk <desk id> <library name> <sort sequence>",
            Options = new List<CommandOption>
            {
                new CommandOption
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new CommandOption
                {
                    Name = "sort sequence",
                    OptionDomain = new VariableDomain(),
                }
            }
        };
    }
}

public class BorrowItemCommand : Command
{
    public string ItemId { get; }
    public string DeskId { get; }
    public string LibraryName { get; }

    public BorrowItemCommand(string[] options)
    {
        ItemId = options.Length > 0 ? options[0] : "";
        DeskId = options.Length > 1 ? options[1] : "";
        LibraryName = options.Length > 2 ? options[2] : "";
        Options = options;
    }

    public override string Type => "borrow-item";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> { },
            Title = "borrow-item",
            Description = "Borrows an item to a desk to track its usage or progress.",
            Group = CommandGroups.Desks,
            Syntax = "flexlib borrow-item <item id> <desk id> <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption
                {
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }
}

public class ReturnItemCommand : Command
{
    public string ItemId { get; }
    public string DeskId { get; }
    public string LibraryName { get; }

    public ReturnItemCommand(string[] options)
    {
        ItemId = options.Length > 0 ? options[0] : "";
        DeskId = options.Length > 1 ? options[1] : "";
        LibraryName = options.Length > 2 ? options[2] : "";
        Options = options;
    }

    public override string Type => "return-item";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> { },
            Title = "return-item",
            Description = "Returns an item from a desk, marking it as no longer borrowed.",
            Group = CommandGroups.Desks,
            Syntax = "flexlib return-item <item id> <desk id> <library name>",
            Options = new List<CommandOption>
            {
                new CommandOption
                {
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }
}

