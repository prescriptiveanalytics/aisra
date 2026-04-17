using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace HEAL.HeuristicWeb.Rest.Server.Data;

public sealed class ConcurrentMultiTypeDictionary<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<TKey, object>> _dict = new();

    public bool TryGet<T>(TKey key, [NotNullWhen(true)] out T? value) where T : notnull
    {
        value = default;

        if (!_dict.TryGetValue(typeof(TKey), out var innerDict))
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
        => _dict.GetOrAdd(typeof(TKey), _ => new ConcurrentDictionary<TKey, object>())[key] = value;
}
