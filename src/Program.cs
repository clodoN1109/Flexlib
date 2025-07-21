using Flexlib.Interface.Input;
using Flexlib.Interface.Router;

class Program 
{
    static void Main(string[] args)
    {
        string[] normalizedInput  = Normalizer.Normalize(args);
        ParsedInput parsedInput  = Parsing.Parse(normalizedInput);   

        Router.Route(parsedInput); 
        
    }

}
