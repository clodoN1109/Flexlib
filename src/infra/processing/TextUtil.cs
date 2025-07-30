namespace Flexlib.Common;

public static class TextUtil
{
    public static string Truncate(string input, int maxLength = 40)
    {
        return input.Length <= maxLength ? input : input.Substring(0, maxLength);
    }

}
