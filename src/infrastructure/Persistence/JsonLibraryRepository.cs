using System.Text.Json;
using Flexlib.Application.Ports;
using Flexlib.Domain;
using System.Reflection;
using System.IO;

namespace Flexlib.Infrastructure.Persistence;

public class JsonLibraryRepository : ILibraryRepository
{
    private readonly string _dataDirectory;
    private readonly string _metaFile;
    private List<Library> _cache;

    public JsonLibraryRepository()
    {
        string? exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Directory not found: {exeFolder}");
#if DEBUG
        _dataDirectory = Path.Combine(exeFolder, "data/");
        _metaFile = Path.Combine(_dataDirectory, "libraries.json");
#else
        _dataDirectory = Path.Combine(exeFolder, "data/");
        _metaFile = Path.Combine(_dataDirectory, "libraries.json");
#endif

        Directory.CreateDirectory(_dataDirectory);

        if (File.Exists(_metaFile))
        {
            var json = File.ReadAllText(_metaFile);
            _cache = JsonSerializer.Deserialize<List<Library>>(json) ?? new List<Library>();
        }
        else
        {
            _cache = new List<Library>();
        }
    }

    public void Save(Library lib)
    {
        _cache.RemoveAll(l => l.Name == lib.Name && l.Path == lib.Path);
        
        _cache.Add(lib);
        
        File.WriteAllText(_metaFile, JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true }));

        UpdateLibrary(lib);
       
    }

    private void UpdateLibrary(Library lib)
    {
 
        UpdateLibFileStructure(lib);
        UpdateLibMetaFile(lib);
        UpdateItems(lib);
        UpdateLocalStorage(lib);
        
    }

    private void UpdateItems(Library lib)
    {
        string itemsDir = Path.Combine(lib.Path, "items/");

        foreach (LibraryItem item in lib.Items)
        {
            UpdateItemMetaFile(item);
        }
    }
    
    private void UpdateLocalStorage(Library lib)
    {

        string libDir = Path.Combine(lib.Path, $"{lib.Name}");
        string itemsDir = Path.Combine(libDir, "items/");
        string localDir = Path.Combine(itemsDir, "local/");

        string[] localFiles = Directory.GetFiles(localDir);

        DeleteUnlistedItems(lib);
    }

    private void DeleteUnlistedItems(Library lib)
    {
          
    }

    private void UpdateLibFileStructure(Library lib)
    {   
        string libDir = Path.Combine(lib.Path, $"{lib.Name}");
        string itemsDir = Path.Combine(libDir, "items/");
        string localDir = Path.Combine(itemsDir, "local/");

        Directory.CreateDirectory(libDir);
        Directory.CreateDirectory(itemsDir);
        Directory.CreateDirectory(localDir);
 
    }

    private void UpdateLibMetaFile(Library lib){

        string libDir = Path.Combine(lib.Path, $"{lib.Name}");

        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");
        File.WriteAllText(libMetaFile, JsonSerializer.Serialize(lib.Items, new JsonSerializerOptions {WriteIndented = true }));
    
    }

    private void UpdateItemMetaFile(LibraryItem item)
    {
        string itemMetaFile = Path.Combine(item.Path, $"{item.Name}.json");
        File.WriteAllText(itemMetaFile, JsonSerializer.Serialize(item, new JsonSerializerOptions {WriteIndented = true }));
    }

    public bool Exists(string name, string path) => _cache.Any( l => l.Name == name && l.Path == path);

    public Library? GetByName(string name) => _cache.FirstOrDefault(l => l.Name == name);

    public IEnumerable<Library> GetAll() => _cache;

}
