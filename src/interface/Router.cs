using Flexlib.Interface.Controllers;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;
using Flexlib.Interface.TUI;

namespace Flexlib.Interface.Router;


public static class Router {

    public static ConsolePresenter presenter = new ConsolePresenter();

    public static void Route(ParsedInput parsedInput) {

        switch (parsedInput) 
        {
            case Command cmd:
                if ( cmd.IsValid() )
                    ConsoleController.Handle(cmd);
                else 
                    presenter.ExplainUsage(cmd.UsageInstructions());
                break;

            case TUIStartUp tui:
                if ( tui.IsValid() ) 
                {  
                    presenter.Message("Launching terminal user interface.");
                    //TUIController.Handle(tui);
                }
                break;
            
            default:
                presenter.ExplainUsage();
                break;
        }

    }

}
