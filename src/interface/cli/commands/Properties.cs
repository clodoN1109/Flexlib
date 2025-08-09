using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;


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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-prop",
            Description = "Defines a new property for the selected library and all its items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib new-prop <property name> [library name] [property type]",
            Options = new List<Option>
            {
                new Option{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                
                new Option{
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
    public string ItemId { get; } 

    public ListPropertiesCommand(string[] options)
    {
        Options = options;
        LibName = options.Length > 0 ? options[0] : "Default Library";
        ItemId = options.Length > 1 ? options[1] : "";
    }

    public override string Type => "list-props";
    
    public override bool IsValid()
    {
        return (Options.Length > 0 && Options.Length < 3);
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list properties",
            Description = "List all defined properties for the selected library or item.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib list-props [library name] [item id]",
            Options = new List<Option>
            {
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                
                new Option{
                    Name = "item id",
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "set-prop",
            Description = "Defines a new property for the selected library and all its items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib set-prop <property name> <new value> <item id> [library name]",
            Options = new List<Option>
            {
                new Option{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "new value",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }


            }
        };
    }

}

public class RenamePropertyCommand : Command
{
    public string PropName { get; } 
    public string NewName { get; } 
    public string LibName { get; } 

    public RenamePropertyCommand(string[] options)
    {
        Options     = options;
        PropName    = options.Length > 0 ? options[0] : "";
        NewName    = options.Length > 1 ? options[1] : "";
        LibName     = options.Length > 2 ? options[2]  : "";
    }
    
    public override string Type => "new-prop";

    public override bool IsValid()
    {
        return (Options.Length == 3);
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-prop",
            Description = "Rename a property for the selected library and all its items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib set-prop <property name> <new name> <library name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "new value",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }


            }
        };
    }

}

public class UnsetPropertyCommand : Command
{
    public string PropName { get; } 
    public string TargetValue { get; } 
    public object ItemId { get; } 
    public string LibName { get; } 

    public UnsetPropertyCommand(string[] options)
    {
        Options = options;
        PropName = options.Length > 0 ? options[0] : "";
        TargetValue = options.Length > 1 ? options[1] : "";
        ItemId = options.Length > 2 ? options[2] : "";
        LibName = options.Length > 3 ? options[3] : "";
    }
    
    public override string Type => "unset-prop";

    public override bool IsValid()
    {
        return (Options.Length == 4);
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "unset-prop",
            Description = "Sets a new value for the selected property of the selected item.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib set-prop <property name> <target value> <item id> <library name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "target value",
                    OptionDomain = new VariableDomain("* removes all entries"),
                    Mandatory = true
                },

                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-prop",
            Description = "Removes a property definition from a selected library and the corresponding values for all the library's items.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib remove-prop <property name> [library name]",
            Options = new List<Option>
            {
                new Option{
                    Name = "property name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }



}

