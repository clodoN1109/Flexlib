using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.Input.Commands;

public class NewDeskCommand : Command
{
    public string LibraryName   { get; }
    public string DeskName      { get; }

    public NewDeskCommand(string[] options)
    {
        LibraryName =   options.Length > 0 ? options[0]   :  "";
        DeskName    =   options.Length > 1 ? options[1]   :  "";
        Options     =   options;
    }

    public override string Type => "new-desk";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-desk",
            Description = "Creates a new desk to organize borrowed items and track their progress.",
            Group = CommandGroups.Desks,
            Syntax = "new-desk <library name> <desk name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "desk name",
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

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "list-desks",
            Description = "Lists all desks in the specified library.",
            Group = CommandGroups.Desks,
            Syntax = "list-desks <library name>",
            Options = new List<Option>
            {
                new Option
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
        LibraryName     = options.Length > 0 ? options[0] : "";
        DeskId          = options.Length > 1 ? options[1] : "";
        SortSequence    = options.Length > 2 ? options[2] : "";
        Options = options;
    }

    public override string Type => "view-desk";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "view-desk",
            Description = "Displays the items and progress for a specific desk.",
            Group = CommandGroups.Desks,
            Syntax = "view-desk <library name> <desk id> [sort sequence]",
            Options = new List<Option>
            {
                new Option
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option
                {
                    Name = "sort sequence",
                    OptionDomain = new VariableDomain("id", "name", "borrowedat", "appetite", "priority", "progress", "completion-value"),
                    Syntax = "selector1/selector2/..."
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
        LibraryName = options.Length > 0 ? options[0] : "";
        DeskId      = options.Length > 1 ? options[1] : "";
        ItemId      = options.Length > 2 ? options[2] : "";
        Options     = options;
    }

    public override string Type => "borrow-item";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "borrow-item",
            Description = "Borrows an item to a desk to track its usage or progress.",
            Group = CommandGroups.Desks,
            Syntax = "borrow-item <library name> <desk id> <item id>",
            Options = new List<Option>
            {
                new Option
                {
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }
}

public class ListLoansCommand : Command
{
    public string ItemId { get; }
    public string LibraryName { get; }

    public ListLoansCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        ItemId      = options.Length > 1 ? options[1] : "";
        Options     = options;
    }

    public override string Type => "list-loans";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "list-loans",
            Description = "Lists the loan history for the selected item.",
            Group = CommandGroups.Desks,
            Syntax = "list-loans <library name> <item id>",
            Options = new List<Option>
            {
                new Option
                {
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option
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
        LibraryName = options.Length > 0 ? options[0] : "";
        DeskId      = options.Length > 1 ? options[1] : "";
        ItemId      = options.Length > 2 ? options[2] : "";
        Options     = options;
    }

    public override string Type => "return-item";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { },
            Title = "return-item",
            Description = "Returns an item from a desk, marking it as no longer borrowed.",
            Group = CommandGroups.Desks,
            Syntax = "return-item <library name> <desk id> <item id>",
            Options = new List<Option>
            {
                new Option
                {
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option
                {
                    Name = "desk id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option
                {
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
            }
        };
    }
}

public class SetAppetiteCommand : Command
{
    public string ItemID { get; }
    public string DeskID { get; }
    public string LibraryName { get; }
    public string Date { get; }

    public SetAppetiteCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        DeskID      = options.Length > 1 ? options[1] : "";
        ItemID      = options.Length > 2 ? options[2] : "";
        Date        = options.Length > 3 ? options[3] : "";
        Options     = options;
    }

    public override string Type => "set-appetite";

    public override bool IsValid() => Options.Length == 4;

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string>(),
            Title = "set-appetite",
            Description = "Assigns an appetite time to an item.",
            Group = CommandGroups.Items,
            Syntax = "set-appetite <library name> <desk id> <item id> <date>",
            Options = new List<Option>
            {
                new("item id", new VariableDomain(), true),
                new("desk id", new VariableDomain(), true),
                new("library name", new VariableDomain(), true),
                new("date", new VariableDomain(), true)
                {
                    Description = "date and time in the format MM/dd/yy HH:mm (24-hour clock)"
                }
            },
            Examples = new List<string>
            {
                "set-appetite Literature 312 42 \"08/10/25 14:00\"",
                "set-appetite History B9cE 7 \"09/01/25 09:30\""
            }
        };
    }
}

public class SetProgressCommand : Command
{
    public string NewValue   { get; }
    public string ItemID     { get; }
    public string DeskID     { get; }
    public string LibraryName  { get; }

    public SetProgressCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        DeskID      = options.Length > 1 ? options[1] : "";
        ItemID      = options.Length > 2 ? options[2] : "";
        NewValue    = options.Length > 3 ? options[3] : "";
        Options     = options;
    }

    public override string Type => "set-progress";

    public override bool IsValid() => Options.Length == 4;

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string>(),
            Title = "set-progress",
            Description = "Updates the progress value of an item.",
            Group = CommandGroups.Items,
            Syntax = "set-progress <library name> <desk id> <item id> <new value>",
            Options = new List<Option>
            {
                new("new value",    new VariableDomain(), true),
                new("item id",      new VariableDomain(), true),
                new("desk id",      new VariableDomain(), true),
                new("library name", new VariableDomain(), true)
            }
        };
    }
}

public class DefineProgressCommand : Command
{
    public string Unit              { get; }
    public string CompletionValue   { get; }
    public string ItemID            { get; }
    public string DeskID            { get; }
    public string LibraryName         { get; }

    public DefineProgressCommand(string[] options)
    {
        LibraryName     = options.Length > 0 ? options[0] : "";
        DeskID          = options.Length > 1 ? options[1] : "";
        ItemID          = options.Length > 2 ? options[2] : "";
        Unit            = options.Length > 3 ? options[3] : "";
        CompletionValue = options.Length > 4 ? options[4] : "";
        Options         = options;
    }

    public override string Type => "define-progress";

    public override bool IsValid() => Options.Length == 5;

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string>(),
            Title = "define-progress",
            Description = "Defines or renames a progress state.",
            Group = CommandGroups.Items,
            Syntax = "define-progress <library name> <desk id> <item id> <unit> <completion value>",
            Options = new List<Option>
            {
                new("unit", new VariableDomain(), true),
                new("completion value", new VariableDomain(), true),
                new("item id", new VariableDomain(), true),
                new("desk id", new VariableDomain(), true),
                new("library name", new VariableDomain(), true)
            }
        };
    }
}

public class SetPriorityCommand : Command
{
    public string NewPriority { get; }
    public string ItemID      { get; }
    public string DeskID      { get; }
    public string LibraryName   { get; }

    public SetPriorityCommand(string[] options)
    {
        LibraryName   = options.Length > 0 ? options[0] : "";
        DeskID        = options.Length > 1 ? options[1] : "";
        ItemID        = options.Length > 2 ? options[2] : "";
        NewPriority   = options.Length > 3 ? options[3] : "";

        Options       = options;
    }

    public override string Type => "set-priority";

    public override bool IsValid() => Options.Length == 4;

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string>(),
            Title = "set-priority",
            Description = "Sets the priority of an item.",
            Group = CommandGroups.Items,
            Syntax = "set-priority <library name> <desk id> <item id> <priority>",
            Options = new List<Option>
            {
                new("priority", new VariableDomain(), true),
                new("item id",  new VariableDomain(), true),
                new("desk id",  new VariableDomain(), true),
                new("library name", new VariableDomain(), true)
            }
        };
    }
}

public class RenameDeskCommand : Command
{
    public string NewName    { get; }
    public string DeskID     { get; }
    public string LibraryName  { get; }

    public RenameDeskCommand(string[] options)
    {
        LibraryName = options.Length > 0 ? options[0] : "";
        DeskID      = options.Length > 1 ? options[1] : "";
        NewName     = options.Length > 2 ? options[2] : "";
        Options     = options;
    }

    public override string Type => "rename-desk";

    public override bool IsValid() => Options.Length == 3;

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Title = "rename-desk",
            Description = "Renames an existing desk.",
            Group = CommandGroups.Desks,
            Syntax = "rename-desk <library name> <desk id> <new name>",
            Options = new List<Option>
            {
                new("new name", new VariableDomain(), true),
                new("desk id", new VariableDomain(), true),
                new("library name", new VariableDomain(), true)
            }
        };
    }
}

