using Flexlib.Infrastructure.Modelling;
using Flexlib.Interface.Input;

public class SetProfileCommand : Command
{
    public string Name { get; }

    public SetProfileCommand(string[] options)
    {

        Name = options.Length > 0 ? options[0] : "";
        Options = options;
    }

    public override string Type => "set-profile";

    public override bool IsValid()
    {
        return Options.Length == 1;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "set-profile",
            Description = "Sets a profile that personalizes how the application interface is presented.",
            Group = CommandGroups.Config,
            Syntax = "set-profile <profile name>",
            Options = new List<Option>
            {
                new Option{
                    Name = "profile name",
                    OptionDomain = new VariableDomain("library", "project"),
                    DefaultValue = "library",
                    Mandatory = true
                },
                
            }
        };
    }
}
