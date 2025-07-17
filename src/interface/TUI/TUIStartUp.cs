using Flexlib.Interface;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.TUI;
    
public class TUIStartUp : ParsedInput {

    public TUIStartUp(TUIConfig tuiConfig)
    {

        

    }

    public override bool IsValid()
    {
        return true;
    }

    public string UsageInstructions()
    {
        return "Usage: ";
    } 

}

public class TUIConfig{}
