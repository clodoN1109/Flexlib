using System;
using System.IO;
using System.Net;

namespace Flexlib.Common;

public static class AddressAnalysis
{
    public static bool IsUrl(string address)
    {
        return Uri.TryCreate(address, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsFilePath(string address)
    {
        return !string.IsNullOrWhiteSpace(address)
               && !IsUrl(address)
               && Path.IsPathRooted(address);
    }

    public static bool IsUNCPath(string address)
    {
        return address.StartsWith(@"\\") || address.StartsWith("//");
    }

    public static bool IsIPAddress(string address)
    {
        return IPAddress.TryParse(address, out _);
    }

    public static AddressType GetAddressType(string address)
    {
        if (IsUrl(address)) return AddressType.Url;
        if (IsUNCPath(address)) return AddressType.Unc;
        if (IsIPAddress(address)) return AddressType.IPAddress;
        if (IsFilePath(address)) return AddressType.LocalFile;

        return AddressType.Unknown;
    }
}

public enum AddressType
{
    Url,
    Unc,
    IPAddress,
    LocalFile,
    Unknown
}


