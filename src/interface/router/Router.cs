using Flexlib.Interface.Input; 
using Flexlib.Infrastructure.Interop; 
using Flexlib.Infrastructure.Config; 
using Flexlib.Interface.CLI; 
using Flexlib.Interface.TUI; 
using Flexlib.Interface.GUI; 
using Flexlib.Interface.Output;

namespace Flexlib.Interface.Router;


public static class Router
{
    private static AgnosticEmitter _emitter = new();

    public static void Route(ProcessedInput input)
    {
        Result? result = null;

        switch (input)
        {
            case Command cmd:
                ConsoleRouter.Route(cmd);
                break;

            case GUIStartUp gui:
                if (gui.IsValid())
                    _emitter.Emit("\nLaunching Flexlib GUI.\n");

                result = Result.Fail("GUI not implemented.");
                break;

            case TUIStartUp tui:
                if (!tui.IsValid())
                {
                    result = Result.Fail(tui.ToString() ?? "TUI startup failed.");
                    break;
                }

                if (tui.IsHelp()) 
                {
                    result = Result.Fail(tui.ToString() ?? "TUI startup failed.");
                    break; 
                }

                _emitter.Emit("\nLaunching Flexlib TUI.\n");

                var launcher = new TUILauncher();
                var config = new TUIConfig(tui.Theme, tui.Language);
                ITUIApp app = new TUIApp(config);

                result = launcher.Prepare(app);
                if (result.IsFailure)
                    break;

                result = launcher.Launch();
                break;

            default:
                result = Result.Fail("Unknown input.");
                break;
            }

        if (result != null && result.IsFailure)
            _emitter.Emit(result.Message);
    }
}
