using Flexlib.Interface;

class Program 
{
    static void Main(string[] args)
    {
    
        ParsedInput input = Input.Parse(args);

        if ( input.IsValid() )
            Router.Route(input); 
        else 
            Output.ExplainUsage(input);
        
    }

}
