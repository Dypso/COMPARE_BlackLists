namespace BlacklistApi.Filters;

public interface ICuckooFilter
{
    bool Contains(string value);
    void Add(string value);
}

public class CuckooFilter : ICuckooFilter
{
    private readonly int _bucketCount;
    private readonly int _entriesPerBucket;
    private readonly Entry?[][] _buckets;
    private readonly object[] _locks;
    private const int MaxKicks = 500;

    public CuckooFilter(int capacity, int entriesPerBucket = 4)
    {
        _bucketCount = capacity;
        _entriesPerBucket = entriesPerBucket;
        _buckets = new Entry?[_bucketCount][];
        _locks = new object[_bucketCount];

        for (var i = 0; i < _bucketCount; i++)
        {
            _buckets[i] = new Entry?[entriesPerBucket];
            _locks[i] = new object();
        }
    }

    public bool Contains(string value)
    {
        var (hash1, hash2) = GetHashes(value);
        var fingerprint = GetFingerprint(value);

        return CheckBucket(hash1, fingerprint) || CheckBucket(hash2, fingerprint);
    }

    public void Add(string value)
    {
        var (hash1, hash2) = GetHashes(value);
        var fingerprint = GetFingerprint(value);
        var entry = new Entry(fingerprint);

        if (TryInsertIntoBucket(hash1, entry) || TryInsertIntoBucket(hash2, entry))
            return;

        // If both buckets are full, start cuckoo kicking
        var currentHash = hash1;
        var currentEntry = entry;

        for (var kicks = 0; kicks < MaxKicks; kicks++)
        {
            var bucket = _buckets[currentHash];
            var lockObj = _locks[currentHash];

            lock (lockObj)
            {
                var randomIndex = Random.Shared.Next(_entriesPerBucket);
                var temp = bucket[randomIndex];
                bucket[randomIndex] = currentEntry;

                if (temp == null)
                    return;

                currentEntry = temp.Value;
                currentHash = GetAlternateHash(currentHash, currentEntry.Fingerprint);
            }
        }

        // If we reach here, we need to resize the filter
        throw new InvalidOperationException("Filter is too full");
    }

    private bool CheckBucket(int bucketIndex, ulong fingerprint)
    {
        lock (_locks[bucketIndex])
        {
            var bucket = _buckets[bucketIndex];
            return bucket.Any(entry => entry?.Fingerprint == fingerprint);
        }
    }

    private bool TryInsertIntoBucket(int bucketIndex, Entry entry)
    {
        lock (_locks[bucketIndex])
        {
            var bucket = _buckets[bucketIndex];
            for (var i = 0; i < _entriesPerBucket; i++)
            {
                if (bucket[i] == null)
                {
                    bucket[i] = entry;
                    return true;
                }
            }
        }
        return false;
    }

    private (int hash1, int hash2) GetHashes(string value)
    {
        var hash1 = Math.Abs(value.GetHashCode()) % _bucketCount;
        var fingerprint = GetFingerprint(value);
        var hash2 = GetAlternateHash(hash1, fingerprint);
        return (hash1, hash2);
    }

    private int GetAlternateHash(int hash, ulong fingerprint)
    {
        return (int)((hash ^ fingerprint) % _bucketCount);
    }

    private static ulong GetFingerprint(string value)
    {
        return (ulong)Math.Abs(value.GetHashCode());
    }

    private readonly struct Entry
    {
        public readonly ulong Fingerprint;
        public Entry(ulong fingerprint) => Fingerprint = fingerprint;
    }
}