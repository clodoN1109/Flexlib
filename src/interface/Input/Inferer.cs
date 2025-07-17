using Flexlib.Common;

namespace Flexlib.Interface.Input;

public static class Infer{

    public static string ItemNameFromOrigin(string origin)
    {

        AddressType addrType = AddressAnalysis.GetAddressType(origin);
        string? baseName = null;

        switch (addrType)
        {
            case AddressType.Url:
                baseName = Path.GetFileNameWithoutExtension(new Uri(origin).AbsolutePath);
                break;

            case AddressType.Unc:
            case AddressType.LocalFile:
                baseName = Path.GetFileNameWithoutExtension(origin);
                break;

            case AddressType.IPAddress:
            case AddressType.Unknown:
            default:
                break;
        }

        return (baseName ?? "unnamed");
    }

}
