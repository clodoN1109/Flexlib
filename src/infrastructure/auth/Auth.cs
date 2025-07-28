using Flexlib.Application.Ports;
using Flexlib.Common;
using Flexlib.Domain;
using Flexlib.Interface.Input;

namespace Flexlib.Infrastructure.Authentication;

public class Authenticator
{
    private readonly IUserRepository _userRepo;
    private readonly IReader _reader;
    private readonly string _sessionFilePath;

    public Authenticator(IUserRepository userRepo, IReader reader)
    {
        _userRepo = userRepo;
        _reader = reader;

        string sessionDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flexlib");
        Directory.CreateDirectory(sessionDir);
        _sessionFilePath = Path.Combine(sessionDir, ".session");
    }

    public bool IsNotAuthenticated(IUser user)
    {
        var storedUser = _userRepo.Get(user.Id);
        if (storedUser is null || string.IsNullOrWhiteSpace(storedUser.Credentials.HashedPassword))
            return true;

        bool verified = Password.Verify(
            user.Credentials.PlainPassword ?? "",
            storedUser.Credentials.HashedPassword
        );

        if (verified)
        {
            user.State = UserState.LoggedIn;
            user.Name = storedUser.Name;

            SaveSession(user.Id); // ⬅️ Save session on success
        }

        return !verified;
    }

    public IUser? TryRestoreSession()
    {
        if (!File.Exists(_sessionFilePath))
            return null;

        var id = File.ReadAllText(_sessionFilePath).Trim();

        if (string.IsNullOrWhiteSpace(id))
            return null;

        var user = _userRepo.Get(id);
        if (user == null)
        {
            ClearSession();
            return null;
        }

        user.State = UserState.LoggedIn;
        return user;
    }

    public void ClearSession()
    {
        if (File.Exists(_sessionFilePath))
            File.Delete(_sessionFilePath);
    }

    private void SaveSession(string id)
    {
        File.WriteAllText(_sessionFilePath, id);
    }

    public Result RegisterUser()
    {
        Console.Write("Enter your name: ");
        string? name = Console.ReadLine()?.Trim();

        Console.Write("Choose a unique user ID: ");
        string id = Console.ReadLine()?.Trim() ?? "";

        string? password = _reader.ReadPassword("Create a password: ");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
            return Result.Fail("Name, ID and password are required.");

        if (_userRepo.Exists(id))
            return Result.Fail("A user with this ID already exists.");

        string hashedPassword = Password.Hash(password);

        var user = new User(new UntrustedAccessInfo(id, password, name));
        user.Credentials.HashedPassword = hashedPassword;

        _userRepo.Save(user);
        SaveSession(id); // ⬅️ Save session here too

        return Result.Success($"User '{name}' registered successfully.");
    }
}


