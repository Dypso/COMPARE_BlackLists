using BlacklistApi.Filters;
using BlacklistApi.Services;
using BlacklistApi.Monitoring;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Configure Application Insights with adaptive sampling
builder.Services.AddApplicationInsightsTelemetry(options => {
    options.EnableAdaptiveSampling = true;
    options.EnablePerformanceCounterCollectionModule = false;
});

// Configure custom performance monitoring
builder.Services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

// Configure Redis
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Blacklist_";
});

// Configure filters
builder.Services.AddSingleton<IBloomFilter>(sp => 
    new FilterBuilder(1000000, 0.001)
        .UseRedisBackplane(builder.Configuration.GetConnectionString("Redis"))
        .UseHashFunction(HashMethod.XXHash64)
        .Build()
);

builder.Services.AddSingleton<ICuckooFilter, CuckooFilter>();
builder.Services.AddSingleton<IUltraFastBitArrayFilter, UltraFastBitArrayFilter>();
builder.Services.AddSingleton<ITwoLevelCache, TwoLevelCache>();

builder.Services.AddHostedService<BlacklistLoaderService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();