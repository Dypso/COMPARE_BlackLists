using BlacklistApi.Filters;

namespace BlacklistApi.Services;

public class BlacklistLoaderService : BackgroundService
{
    private readonly IBloomFilter _bloomFilter;
    private readonly ICuckooFilter _cuckooFilter;
    private readonly IUltraFastBitArrayFilter _ultraFastFilter;
    private readonly ITwoLevelCache _twoLevelCache;
    private readonly ILogger<BlacklistLoaderService> _logger;

    public BlacklistLoaderService(
        IBloomFilter bloomFilter,
        ICuckooFilter cuckooFilter,
        IUltraFastBitArrayFilter ultraFastFilter,
        ITwoLevelCache twoLevelCache,
        ILogger<BlacklistLoaderService> logger)
    {
        _bloomFilter = bloomFilter;
        _cuckooFilter = cuckooFilter;
        _ultraFastFilter = ultraFastFilter;
        _twoLevelCache = twoLevelCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Loading blacklist data into filters...");

            // Load data from file
            var blacklistData = await File.ReadAllLinesAsync("blacklist.txt", stoppingToken);

            // Load data into each filter in parallel
            await Task.WhenAll(
                LoadBloomFilterAsync(blacklistData),
                LoadCuckooFilterAsync(blacklistData),
                LoadUltraFastFilterAsync(blacklistData),
                LoadTwoLevelCacheAsync(blacklistData)
            );

            _logger.LogInformation("Blacklist data loaded successfully into all filters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blacklist data");
            throw;
        }
    }

    private Task LoadBloomFilterAsync(string[] data)
    {
        return Task.Run(() => {
            foreach (var item in data)
            {
                _bloomFilter.Add(item);
            }
        });
    }

    private Task LoadCuckooFilterAsync(string[] data)
    {
        return Task.Run(() => {
            foreach (var item in data)
            {
                _cuckooFilter.Add(item);
            }
        });
    }

    private Task LoadUltraFastFilterAsync(string[] data)
    {
        return Task.Run(() => {
            foreach (var item in data)
            {
                _ultraFastFilter.Add(item);
            }
        });
    }

    private async Task LoadTwoLevelCacheAsync(string[] data)
    {
        foreach (var item in data)
        {
            await _twoLevelCache.SetAsync(item, true);
        }
    }
}