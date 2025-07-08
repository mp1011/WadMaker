namespace WadMaker.Extensions;
public static class IMapElementExtensions
{
    public static void Paint(this IMapElement element, StringBuilder sb)
    {
        var type = element.GetType();
        sb.AppendLine(type.Name);
        sb.AppendLine("{");

        var properties = type.GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(element);
            if(value != null)
                sb.AppendLine($"    {prop.Name} = {ValueToString(value)};");
        }

        sb.AppendLine("}");
    }

    public static void Paint<T>(this IEnumerable<T> elements, StringBuilder sb) where T:IMapElement
    {
        var index = 0;

        foreach(var item in elements)
        {
            sb.AppendLine($"//{index++}");
            item.Paint(sb);
        }
    }

    public static IEnumerable<T> ToDataArray<T>(this IEnumerable<IElementWrapper<T>> wrappers) where T : IMapElement
    {
        return wrappers.Select(wrapper => wrapper.Data);
    }

    private static string ValueToString(object value)
    {
        if (value is string)
            return $"\"{value}\"";
        else if (value == null)
            return "";
        else
            return value.ToString().ToLower();
    }
}

