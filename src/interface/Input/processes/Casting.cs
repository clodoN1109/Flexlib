namespace Flexlib.Interface.Input;
public static partial class Input
{
    public static string[] Casting(object? input)
    {
        if (input is not string[] cast || cast.Length == 0)
            return [];

        return cast;
    }


}
