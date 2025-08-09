using Flexlib.Interface;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.GUI;
    
public class GUIStartUp : ParsedInput {

    public string Theme     { get; }
    public string Language  { get; }
    public string[] Config  { get; }

    public GUIStartUp(string[] guiConfig)
    {
        Theme     = guiConfig.Length > 0 ? guiConfig[0] : "dark";        
        Language  = guiConfig.Length > 1 ? guiConfig[1] : "en";        
        Config    = guiConfig;
    }

    public override bool IsValid()
    {
        return (Config.Length > 0 && Config.Length < 3);
    }

    public UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-item",
            Description = "Creates a new item in the selected library.",
            Syntax = "flexlib gui [theme] [language]",
            Options = new List<Option>
            {
                new Option{
                    Name = "theme",
                    Description = "Selects the color palette.",
                    OptionDomain = new VariableDomain("dark", "light"),
                    DefaultValue = "dark" 
                },
                
                new Option{
                    Name = "language",
                    Description = "Selects the language used to present UI text.",
                    OptionDomain = new VariableDomain("en", "pt"),
                    DefaultValue = "end" 
                }
            }
        };
    }
    
}


