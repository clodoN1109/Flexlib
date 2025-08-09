using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;


public class HelpCommand : Command
{
    public HelpCommand(string[] options) { }
    public HelpCommand() { }

    public override bool IsValid() => true;
    public override string Type => "help";

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> { Env.Version, Env.BuildId },
            Title = "CLI HELP",
            Description = "Flexlib is a lightweight system for designing flexible and interconnected libraries with just a few keystrokes.",
            Group = CommandGroups.Help,
            Syntax = "flexlib <command>",
            Options = new List<Option>
            {
                new Option{
                    Name = "command",
                    Description = "Specifies the action to be invoked in the Flexlib application.",
                    OptionDomain = new VariableDomain( ActionsList.Items ),
                    Mandatory = true
                }
            }
        };
    }
}   


