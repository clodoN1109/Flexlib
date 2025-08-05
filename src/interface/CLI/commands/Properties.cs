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
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list properties",
            Description = "List all defined properties for the selected library or item.",
            Group = CommandGroups.Properties,
            Syntax = "flexlib list-props [library name] [item id]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                
                new CommandOption{
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

