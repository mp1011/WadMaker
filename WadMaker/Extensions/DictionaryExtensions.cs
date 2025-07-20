namespace WadMaker.Extensions;

public static class DictionaryExtensions
{
    public static V? Try<K,V>(this Dictionary<K,V> dictionary, K key) where K:notnull
    {
        V? value;
        if (dictionary.TryGetValue(key, out value))
            return value;
        else
            return default(V);
    }
}
