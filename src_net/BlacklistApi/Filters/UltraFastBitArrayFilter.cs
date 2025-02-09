using System.Collections;

namespace BlacklistApi.Filters;

public interface IUltraFastBitArrayFilter
{
    bool Contains(string value);
    void Add(string value);
}

public class UltraFastBitArrayFilter : IUltraFastBitArrayFilter
{
    private readonly BitArray _bits;
    private readonly int _size;
    private readonly object _lock = new();

    public UltraFastBitArrayFilter(int capacity)
    {
        _size = capacity;
        _bits = new BitArray(capacity);
    }

    public bool Contains(string value)
    {
        var hash = FnvHash(value) % _size;
        lock (_lock)
        {
            return _bits[hash];
        }
    }

    public void Add(string value)
    {
        var hash = FnvHash(value) % _size;
        lock (_lock)
        {
            _bits[hash] = true;
        }
    }

    private static ulong FnvHash(string value)
    {
        const ulong fnvPrime = 1099511628211;
        const ulong fnvOffsetBasis = 14695981039346656037;

        var hash = fnvOffsetBasis;
        foreach (var c in value)
        {
            hash = hash * fnvPrime;
            hash = hash ^ c;
        }
        return hash;
    }
}