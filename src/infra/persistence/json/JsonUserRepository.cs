using System.Text.Json;
using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Environment;
using System;


namespace Flexlib.Infrastructure.Persistence;

public class JsonUserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly string _dataDirectory;
    private readonly List<IUser> _cache = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonUserRepository()
    {
        string? exeFolder = Flexlib.Infrastructure.Environment.Env.GetExecutingAssemblyLocation();
        if (string.IsNullOrWhiteSpace(exeFolder) || !Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Executable directory not found: {exeFolder}");

        string? appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(appDataFolder) || !Directory.Exists(appDataFolder))
            throw new DirectoryNotFoundException($"AppData directory not found: {appDataFolder}");

        _dataDirectory = EnsureDataDirectory(exeFolder, appDataFolder);
        _filePath = Path.Combine(_dataDirectory, "users.json");

        EnsureFileExists();
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

    private void LoadCache()
    {
        _cache.Clear();
        _cache.AddRange(LoadAllUsers());
    }

    public IUser? Get(string id)
    {
        return _cache.FirstOrDefault(u => u.Credentials.UserId == id);
    }

    public bool Exists(string id)
    {
        return _cache.Any(u => u.Credentials.UserId == id);
    }

    public void Save(IUser user)
    {
        var existing = _cache.FirstOrDefault(u => u.Credentials.UserId == user.Credentials.UserId);
        if (existing != null)
            _cache.Remove(existing);

        _cache.Add(user);
        SaveAllUsers(_cache.OfType<User>().ToList());
    }

    public void Delete(string id)
    {
        var existing = _cache.FirstOrDefault(u => u.Credentials.UserId == id);
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

    private List<User> LoadAllUsers()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            var json = File.ReadAllText(_filePath);
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
        File.WriteAllText(_filePath, json);
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(_filePath))
            SaveAllUsers(new List<User>());
    }
}


