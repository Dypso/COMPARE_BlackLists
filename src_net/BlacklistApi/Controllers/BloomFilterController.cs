using Microsoft.AspNetCore.Mvc;
using BlacklistApi.Filters;
using BlacklistApi.Models;
using BlacklistApi.Monitoring;

namespace BlacklistApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BloomFilterController : ControllerBase
{
    private readonly IBloomFilter _filter;
    private readonly IPerformanceMonitor _monitor;

    public BloomFilterController(IBloomFilter filter, IPerformanceMonitor monitor)
    {
        _filter = filter;
        _monitor = monitor;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidationRequest request)
    {
        var result = await _monitor.TrackOperationAsync("BloomFilter_Validate", 
            () => Task.FromResult(_filter.Contains(request.EmvId)));
        
        return Ok(new { allowed = !result });
    }
}