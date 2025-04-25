using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace WebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IDatabase _redis;

    public MetricsController(IHubContext<ChatHub> hubContext, IConnectionMultiplexer redis)
    {
        _hubContext = hubContext;
        _redis = redis.GetDatabase();
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromQuery] string userId, [FromQuery] MetricType metric)
    {
        await _redis.SetAddAsync($"subs:{userId}", metric.ToString());
        return Ok($"Subscribed to {metric}");
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string userId, [FromQuery] MetricType metric)
    {
        await _redis.SetRemoveAsync($"subs:{userId}", metric.ToString());
        return Ok($"Unsubscribed from {metric}");
    }

    [HttpPost("sender")]
    public async Task<IActionResult> Sender([FromQuery] string userId, [FromQuery] MetricType metric)
    {
        var isSubscribed = await _redis.SetContainsAsync($"subs:{userId}", metric.ToString());
        if (!isSubscribed)
        {
            return BadRequest("User is not subscribed to this metric");
        }

        int count = 60;
        while (count-- > 0)
        {
            await _hubContext.Clients.Group(metric.ToString()).SendAsync("ReceiveMessage",
                new RateMessage(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), GetRandomRate()));
            await Task.Delay(1000);
        }

        return Ok("Metric messages sent");
    }

    private decimal GetRandomRate()
    {
        return Math.Round((decimal)(Random.Shared.NextDouble() * 100), 2);
    }
}
