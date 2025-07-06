using Flexlib.Interface;

class Program 
{
    static void Main(string[] args)
    {
    
        var parsedInput = Parsing.Parse(args);

        Router.Route(parsedInput); 
        
    }

}
