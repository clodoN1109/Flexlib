using Flexlib.Interface.Input; 
using Flexlib.Interface.CLI; 
using Flexlib.Interface.GUI; 
using Flexlib.Interface.Output; 

namespace Flexlib.Interface.Router;


public static class Router
{
    private static AgnosticEmitter _emitter = new();

    public static void Route(ProcessedInput input)
    {
        switch (input)
        {
            case Command cmd:
                ConsoleRouter.Route(cmd);
                break;

            case GUIStartUp gui:
                _emitter.Emit("Launching Flexlib GUI.");
                break;

            default:
                _emitter.Emit("\nInvalid input.\n");
                break;
        }
    }
}

