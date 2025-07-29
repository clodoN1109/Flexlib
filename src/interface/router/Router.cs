using Flexlib.Interface.Input; 
using Flexlib.Interface.CLI; 
using Flexlib.Interface.GUI; 
using Flexlib.Interface.Output; 

namespace Flexlib.Interface.Router;


public static class Router
{

    private static AgnosticEmitter _emitter = new AgnosticEmitter();

    public static void Route(ParsedInput input)
    {
       switch(input)
       {
            case Command cmd:
                ConsoleRouter.Route(cmd);
                break;

            case GUIStartUp gui:
                if (gui.IsValid())
                {
                    _emitter.Emit("Launching Flexlib GUI.");
                    // GUIController.Handle(gui, user);
                }
                break;
            
            default:
                _emitter.Emit("Invalid input.");
                break;

       }
    } 

}
