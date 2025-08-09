using Flexlib.Infrastructure.Modelling;

namespace Flexlib.Interface.Input;

public class Option
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Syntax { get; set; } = "";
    public VariableDomain OptionDomain { get; set; } = new();
    public bool Mandatory { get; set; } = false;
    public string DefaultValue { get; set; } = "";
    public bool Reversible { get; set; } = false;

    public Option(string name, VariableDomain domain, bool mandatory = false)
    {
        Name = name;
        OptionDomain = domain;
        Mandatory = mandatory;
    }

    public Option(){}
}


