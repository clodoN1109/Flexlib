namespace Flexlib.Interface;

public static class Router {
    
    public static void Route(ParsedInput parsedInput) {

        switch (parsedInput) 
        {
            case Command cmd:
                if ( cmd.IsValid() )
                    CommandController.Handle(cmd);
                else 
                    Output.ExplainUsage(cmd.UsageInstructions());
                break;
            
            default:
                Output.ExplainUsage();
                break;
        }

    }

}
