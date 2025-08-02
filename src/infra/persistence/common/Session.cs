namespace Flexlib.Infrastructure.Persistence;

public class Session
{
    public string?    Id              { get; set; }
    public DateTime?  CreationDate    { get; set; }

    public static string HashId(string id)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

}
