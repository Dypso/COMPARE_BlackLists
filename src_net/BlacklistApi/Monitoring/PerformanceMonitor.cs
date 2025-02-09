using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace BlacklistApi.Monitoring;

public interface IPerformanceMonitor
{
    Task TrackOperationAsync(string operation, Func<Task> action);
    Task<T> TrackOperationAsync<T>(string operation, Func<Task<T>> action);
}

public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly TelemetryClient _telemetryClient;
    private readonly double _samplingRate;
    private readonly Random _random;

    public PerformanceMonitor(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _samplingRate = 0.1; // 10% sampling in production
        _random = new Random();
    }

    public async Task TrackOperationAsync(string operation, Func<Task> action)
    {
        if (!ShouldSample()) 
        {
            await action();
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await action();
        }
        finally
        {
            sw.Stop();
            TrackMetrics(operation, sw.Elapsed);
        }
    }

    public async Task<T> TrackOperationAsync<T>(string operation, Func<Task<T>> action)
    {
        if (!ShouldSample())
        {
            return await action();
        }

        var sw = Stopwatch.StartNew();
        try
        {
            return await action();
        }
        finally
        {
            sw.Stop();
            TrackMetrics(operation, sw.Elapsed);
        }
    }

    private bool ShouldSample() => _random.NextDouble() < _samplingRate;

    private void TrackMetrics(string operation, TimeSpan elapsed)
    {
        _telemetryClient.TrackMetric($"{operation}_Duration", elapsed.TotalMilliseconds);
    }
}