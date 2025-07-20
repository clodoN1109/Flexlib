using Flexlib.Interface;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.GUI;
    
public class GUIStartUp : ParsedInput {

    public GUIStartUp(GUIConfig guiConfig)
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

public class GUIConfig{}
