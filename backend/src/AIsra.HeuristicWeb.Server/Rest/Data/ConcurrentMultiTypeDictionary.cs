using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace AIsra.HeuristicWeb.Server.Rest.Data;

public sealed class ConcurrentMultiTypeDictionary<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<TKey, object>> dict = new();

    public bool TryGet<T>(TKey key, [NotNullWhen(true)] out T? value) where T : notnull
    {
        value = default(T?);

        if (!dict.TryGetValue(typeof(TKey), out var innerDict))
        {
            return false;
        }

        if (!innerDict.TryGetValue(key, out var innerValue))
        {
            return false;
        }

        value = (T)innerValue;

        return true;
    }

    public void Set<T>(TKey key, T value) where T : notnull
        => dict.GetOrAdd(typeof(TKey), _ => new ConcurrentDictionary<TKey, object>())[key] = value;
}
