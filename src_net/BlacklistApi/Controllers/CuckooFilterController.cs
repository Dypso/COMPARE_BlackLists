using Microsoft.AspNetCore.Mvc;
using BlacklistApi.Filters;
using BlacklistApi.Models;
using BlacklistApi.Monitoring;

namespace BlacklistApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuckooFilterController : ControllerBase
{
    private readonly ICuckooFilter _filter;
    private readonly IPerformanceMonitor _monitor;

    public CuckooFilterController(ICuckooFilter filter, IPerformanceMonitor monitor)
    {
        _filter = filter;
        _monitor = monitor;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidationRequest request)
    {
        var result = await _monitor.TrackOperationAsync("CuckooFilter_Validate", 
            () => Task.FromResult(_filter.Contains(request.EmvId)));
        
        return Ok(new { allowed = !result });
    }
}