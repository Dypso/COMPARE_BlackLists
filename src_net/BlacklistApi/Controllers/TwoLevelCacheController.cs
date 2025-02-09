using Microsoft.AspNetCore.Mvc;
using BlacklistApi.Services;
using BlacklistApi.Models;
using BlacklistApi.Monitoring;

namespace BlacklistApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwoLevelCacheController : ControllerBase
{
    private readonly ITwoLevelCache _cache;
    private readonly IPerformanceMonitor _monitor;

    public TwoLevelCacheController(ITwoLevelCache cache, IPerformanceMonitor monitor)
    {
        _cache = cache;
        _monitor = monitor;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidationRequest request)
    {
        var result = await _monitor.TrackOperationAsync("TwoLevelCache_Validate", 
            () => _cache.GetAsync(request.EmvId));
        
        return Ok(new { allowed = !result });
    }
}