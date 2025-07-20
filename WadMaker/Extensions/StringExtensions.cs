namespace WadMaker.Extensions;

public static class StringExtensions
{
    public static T? ParseAs<T>(this string? value) where T: struct
    {
        T result = default(T);
        if (Enum.TryParse<T>(value, out result))
            return result;
        else
            return null;
    }
}
