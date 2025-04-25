using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

public class ChatHub : Hub
{
    private readonly IDatabase _redis;

    public ChatHub(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task Subscribe(string userId, MetricType metric)
    {
        await _redis.SetAddAsync($"subs:{userId}", metric.ToString());
        await Groups.AddToGroupAsync(Context.ConnectionId, metric.ToString());
    }

    public async Task Unsubscribe(string userId, MetricType metric)
    {
        await _redis.SetRemoveAsync($"subs:{userId}", metric.ToString());
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, metric.ToString());
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (!string.IsNullOrEmpty(userId))
        {
            var subscriptions = await _redis.SetMembersAsync($"subs:{userId}");
            foreach (var metric in subscriptions)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, metric!);
            }
        }

        await base.OnConnectedAsync();
    }
}


public enum MetricType
{
    TimeRate,
    Rate
}