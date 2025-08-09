using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;

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

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-lib",
            Description = "Creates a new library with the selected name and located at the selected path.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib new-lib <library name> [library path]",
            Options = new List<Option>
            {
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
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

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-lib",
            Description = "Removes the selected library and all its items.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib remove-lib <library name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list-libs",
            Description = "Lists all accessible existing libraries.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib list-libs",
            Options = new List<Option>()
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "get-layout",
            Description = "Gets the current library layout.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib get-layout <library name>",
            Options = new List<Option>
            {
                new Option{
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
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "set-layout",
            Description = "Redefines the selected library layout.",
            Group = CommandGroups.Libraries,
            Syntax = "flexlib set-layout <library name> <layout>",
            Options = new List<Option>
            {
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                
                new Option{
                    Name = "layout",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true,
                    Syntax = "<property>[/property ...]"
                }

            }
        };
    }
}

