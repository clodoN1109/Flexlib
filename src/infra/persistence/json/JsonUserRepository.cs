using System.Text.Json;
using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Environment;
using System;


namespace Flexlib.Infrastructure.Persistence;

public class JsonUserRepository : IUserRepository
{
    private readonly string _usersFilePath;
    private readonly string _dataDirectory;
    private readonly List<IUser> _cache = new();
    private readonly string _sessionFilePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonUserRepository()
    {
        string? exeFolder = Env.GetExecutingAssemblyLocation();
        if (string.IsNullOrWhiteSpace(exeFolder) || !Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Executable directory not found: {exeFolder}");

        string? appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(appDataFolder) || !Directory.Exists(appDataFolder))
            throw new DirectoryNotFoundException($"AppData directory not found: {appDataFolder}");

        _dataDirectory =  EnsureDataDirectory(exeFolder, appDataFolder);
        _usersFilePath = Path.Combine(_dataDirectory, "users.json");

        string sessionDir = Path.Combine(appDataFolder, "Flexlib");
        Directory.CreateDirectory(sessionDir);
        _sessionFilePath = Path.Combine(sessionDir, ".session");

        EnsureFilesExist();
        LoadCache();
    }

    private string EnsureDataDirectory(string exeFolder, string appDataFolder)
    {
        string dataDirectory;

    #if DEBUG
        dataDirectory = Path.Combine(exeFolder, "data");
    #else
        string flexlibDir = Path.Combine(appDataFolder, "Flexlib");
        Directory.CreateDirectory(flexlibDir);
        dataDirectory = Path.Combine(flexlibDir, "data");
    #endif

        Directory.CreateDirectory(dataDirectory);
        return dataDirectory;
    }

    public Result SaveSession(string id)
    {
        try
        {
            var session = new Session
            {
                Id = Session.HashId(id),
                CreationDate = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(session, _jsonOptions);
            File.WriteAllText(_sessionFilePath, json);
            return Result.Success("Session file updated.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Could not save session: {ex.Message}");
        }
    }

    public Session GetSession()
    {
        try
        {
            if (!File.Exists(_sessionFilePath))
                return new Session { Id = "", CreationDate = DateTime.MinValue };

            var json = File.ReadAllText(_sessionFilePath);
            var session = JsonSerializer.Deserialize<Session>(json, _jsonOptions);

            return session ?? new Session { Id = "", CreationDate = DateTime.MinValue };
        }
        catch
        {
            return new Session { Id = "", CreationDate = DateTime.MinValue };
        }
    }

    public Result CloseSession()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
                File.Delete(_sessionFilePath);

            return Result.Success("Session closed.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Could not close session: {ex.Message}");
        }
    }


    public IUser? Get(string id)
    {
        return _cache.FirstOrDefault(u => u.Credentials.UserId.ToLowerInvariant() == id.ToLowerInvariant());
    }

    public IUser? GetByHashedId(string hashedId)
    {
        foreach (var user in _cache)
        {
            string userId = user.Credentials.UserId;
            string hashed = Session.HashId(userId);

            if (hashed == hashedId)
                return user;
        }

        return null;
    }

    public bool Exists(string id)
    {
        return _cache.Any(u => u.Credentials.UserId.ToLowerInvariant() == id.ToLowerInvariant());
    }

    public void Save(IUser user)
    {
        var existing = _cache.FirstOrDefault(u => u.Credentials.UserId.ToLowerInvariant() == user.Credentials.UserId.ToLowerInvariant());
        if (existing != null)
            _cache.Remove(existing);

        _cache.Add(user);
        SaveAllUsers(_cache.OfType<User>().ToList());
    }

    public void Delete(string id)
    {
        var existing = _cache.FirstOrDefault(u => u.Credentials.UserId.ToLowerInvariant() == id.ToLowerInvariant());
        if (existing != null)
        {
            _cache.Remove(existing);
            SaveAllUsers(_cache.OfType<User>().ToList());
        }
    }

    public IEnumerable<IUser> GetAll()
    {
        return _cache;
    }

    private void LoadCache()
    {
        _cache.Clear();
        _cache.AddRange(LoadAllUsers());
    }

    private List<User> LoadAllUsers()
    {
        try
        {
            if (!File.Exists(_usersFilePath))
                return new List<User>();

            var json = File.ReadAllText(_usersFilePath);
            var users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions);
            return users ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[JsonUserRepository] Error loading users.json: {ex.Message}");
            return new List<User>();
        }
    }

    private void SaveAllUsers(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, _jsonOptions);
        File.WriteAllText(_usersFilePath, json);
    }

    private void EnsureFilesExist()
    {
        if (!File.Exists(_usersFilePath))
            SaveAllUsers(new List<User>());
    }
}
