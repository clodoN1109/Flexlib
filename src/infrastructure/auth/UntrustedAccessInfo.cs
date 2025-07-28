namespace Flexlib.Infrastructure.Authentication;

public class UntrustedAccessInfo
{
    public string? Name { get; set; }
    public string Id { get; set; }
    public string Password { get; set; }

    public UntrustedAccessInfo(string id, string password)
    {
        Id = id;
        Password = password;
    }
    
    public UntrustedAccessInfo(string id, string password, string name)
    {
        Name = name;
        Id = id;
        Password = password;
    }
}

