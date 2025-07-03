using Flexlib.Interface;

class Program 
{
    static void Main(string[] args)
    {
    
        var parsedInput = Input.Parse(args);

        Router.Route(parsedInput); 
        
    }

}
