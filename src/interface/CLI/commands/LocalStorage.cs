using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;


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

