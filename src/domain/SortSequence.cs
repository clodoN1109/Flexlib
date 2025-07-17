namespace Flexlib.Domain;

public class SortSequence
{

    public List<string> Elements;

    public SortSequence(string sortSequence)
    {
        Elements = sortSequence.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();       
    }

}

