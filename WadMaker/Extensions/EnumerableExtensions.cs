namespace WadMaker.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns each element along with the elements before, and after it, assuming a cycle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="groupSize"></param>
    /// <returns></returns>
    public static IEnumerable<(T,T,T)> WithNeighbors<T>(this T[] list)
    {
        for(int i = 0; i < list.Count(); i++)
        {
            yield return new(list[(i-1).NMod(list.Length)], list[i], list[(i+1) % list.Length]);
        }
    }

    public static void RemoveMany<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Remove(item);
        }
    }

    public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> search)
    {
        return search.All(s => list.Contains(s));
    }

    public static bool ContainsAny<T>(this IEnumerable<T> list, IEnumerable<T> search)
    {
        return search.Any(s => list.Contains(s));
    }

    public static (T Item, int Index)[] WithIndex<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var result = new List<(T, int)>();
        int idx = 0;
        foreach (var item in source)
        {
            result.Add((item, idx++));
        }
        return result.ToArray();
    }
}