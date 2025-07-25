using Flexlib.Interface.Input;
using Flexlib.Interface.Router;
using Flexlib.Interface.Output;

class Program 
{
    static void Main(string[] args)
    {

        PrettyException.HookGlobalHandler();

        string[] normalizedInput  = Normalizer.Normalize(args);
        
        ParsedInput parsedInput  = Parsing.Parse(normalizedInput);   

        Router.Route(parsedInput); 
        
    }

}
