namespace Flexlib.Interface;

public static class Router {
    
    public static void Route(ParsedInput parsedInput) {

        switch (parsedInput) 
        {
            case Command cmd:
                CommandController.Handle(cmd);
                break;
            
            default:
                Output.ExplainUsage(parsedInput);
                break;
        }

    }

}
