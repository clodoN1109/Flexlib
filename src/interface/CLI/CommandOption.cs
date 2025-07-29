using Flexlib.Common;

namespace Flexlib.Interface.CLI;

public class CommandOption
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Syntax { get; set; } = "";
    public Common.Domain OptionDomain { get; set; } = new();
    public bool Mandatory { get; set; } = false;
    public string DefaultValue { get; set; } = "";
    public bool Reversible { get; set; } = false;
}


