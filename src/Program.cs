using Flexlib.Interface.Input;
using Flexlib.Interface.Router;

class Program 
{
    static void Main(string[] args)
    {
    
        var parsedInput = Parsing.Parse(args);

        Router.Route(parsedInput); 
        
    }

}
