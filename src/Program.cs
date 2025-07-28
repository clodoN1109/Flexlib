using Flexlib.Interface.Input;
using Flexlib.Interface.Router;
using Flexlib.Interface.Output;

class Program 
{
    static void Main(string[] args)
    {
#if DEBUG
        PrettyException.HookGlobalHandler();
#endif
        string[] normalizedInput  = Normalizer.Normalize(args);
        
        ParsedInput parsedInput  = Parsing.Parse(normalizedInput);   

        Router.Route(parsedInput); 
        
    }

}
