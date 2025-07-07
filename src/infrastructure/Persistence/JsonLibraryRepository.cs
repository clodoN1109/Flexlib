using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Common;
using Flexlib.Infrastructure.Persistence.Common;
using System.IO;

namespace Flexlib.Infrastructure.Persistence;

public class JsonLibraryRepository : ILibraryRepository
{
    private readonly string _dataDirectory;
    private readonly string _metaFile;
    private List<Library> _cache;

    public JsonLibraryRepository()
    {
        _cache = new List<Library>();

        _dataDirectory = EnsureDataDirectory();

        _metaFile = EnsureMetaFile();

    }

    private string EnsureDataDirectory()
    {
#if DEBUG
        
        string? exeFolder = Env.GetExecutingAssemblyLocation();

        if (!Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Executable file directory not found: {exeFolder}");
        
        string dataDirectory = Path.Combine(exeFolder, "data/");
#else
        string dataDirectory = "~/AppData/Flexlib/data/";
#endif

        Directory.CreateDirectory(dataDirectory);
    
        return dataDirectory;
    }
    
    private string EnsureMetaFile()
    {
        string? exeFolder = Env.GetExecutingAssemblyLocation();
        
        if (!Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Directory not found: {exeFolder}");

#if DEBUG
        string metaFile = Path.Combine(exeFolder, "data/libraries.json");
#else 
        string metaFile = Path.Combine(exeFolder, "data/libraries.json");
#endif

        if (File.Exists(metaFile))
        {
            var json = File.ReadAllText(metaFile);
            _cache = JsonHelpers.ReadJson(json) ?? new List<Library>();
        }
        else
        {
            _cache = new List<Library>();
        }

        return metaFile;
    }

    public void Save(Library lib)
    {
        _cache.RemoveAll(l => l.Name == lib.Name && l.Path == lib.Path);
        
        _cache.Add(lib);

        JsonHelpers.WriteJson(_metaFile, _cache);
        UpdateLibFileStructure(lib);
        UpdateLibMetaFile(lib);
        UpdateItems(lib);
        UpdateLocalStorage(lib);
       
    }

    public void RemoveLibraryByName(string name)
    {
        _cache.RemoveAll(l => l.Name == name);
        JsonHelpers.WriteJson(_metaFile, _cache);
    }

    private void UpdateItems(Library lib)
    {
        string itemsDir = Path.Combine(lib.Path, "items/");

        foreach (LibraryItem item in lib.Items)
        {
            UpdateItemMetaFile(item, lib);
        }
    }
    
    private void UpdateLocalStorage(Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string itemsDir = Path.Combine(libDir, "items");
        string localDir = Path.Combine(itemsDir, "local");

        Directory.CreateDirectory(localDir);

        var localFiles = new HashSet<string?>(
            Directory.GetFiles(localDir).Select(Path.GetFileName),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (LibraryItem item in lib.Items)
        {
            string sourcePath = item.Origin!;
            
            string itemName = item.Name!;
            string fileExtension = Path.GetExtension( Path.GetFileName(sourcePath) );
            string fileName = $"{itemName}{fileExtension}";

            string targetPath = Path.Combine(localDir, fileName);

            if (!localFiles.Contains(fileName))
            {
                AddressType type = AddressAnalysis.GetAddressType(sourcePath);
                CopyHelpers.TryCopyToLocal(type, sourcePath, targetPath);
            }
        }
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

    private void UpdateLibMetaFile(Library lib)
    {

        string libDir = Path.Combine(lib.Path, $"{lib.Name}");

        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");
        JsonHelpers.WriteJson(libMetaFile, lib.Items);
    
    }

    private void UpdateItemMetaFile(LibraryItem item, Library lib)
    {
        string itemMetaFile = Path.Combine(lib.Path, lib.Name!, "items", $"{item.Name}.json");
        JsonHelpers.WriteJson(itemMetaFile, item);
    }

    private void DeleteLibMetaFile(Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");

        if (File.Exists(libMetaFile))
            File.Delete(libMetaFile);
    }

    private void DeleteItemMetaFile(LibraryItem item, Library lib)
    {
        string itemMetaFile = Path.Combine(lib.Path, lib.Name!, "items", $"{item.Name}.json");

        if (File.Exists(itemMetaFile))
            File.Delete(itemMetaFile);
    }


    private void DeleteAllLocalMetaFiles()
    {
        foreach (var lib in _cache)
        {
            if (lib == null) continue;

            DeleteLibMetaFile(lib);

            if (lib.Items == null) continue;

            foreach (var item in lib.Items)
            {
                DeleteItemMetaFile(item, lib);
            }
        }
    }

    public bool Exists(string name) => _cache.Any( l => l.Name == name);

    public Library? GetByName(string name) => _cache.FirstOrDefault(l => l.Name == name);

    public IEnumerable<Library> GetAll() => _cache;

}
