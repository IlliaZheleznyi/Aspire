using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yarp.ReverseProxy;
using System.Net.Http;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new RouteConfig
            {
                RouteId = "signalr",
                ClusterId = "signalr-cluster",
                Match = new RouteMatch
                {
                    Path = "/chathub/{**catch-all}"
                }
            },
            new RouteConfig
            {
                RouteId = "api-metrics",
                ClusterId = "signalr-cluster",
                Match = new RouteMatch
                {
                    Path = "/api/Metrics/{**catch-all}"
                }
            }
        },
        new[]
        {
            new ClusterConfig
            {
                ClusterId = "signalr-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "apiservice1", new DestinationConfig { Address = "https://localhost:7101/" } },
                    { "apiservice2", new DestinationConfig { Address = "https://localhost:7102/" } }
                },
                LoadBalancingPolicy = "RoundRobin",
                SessionAffinity = new SessionAffinityConfig
                {
                    Enabled = true,
                    Policy = "Cookie",
                    AffinityKeyName = "AffinityId"
                }
            }
        });

builder.Services.AddSingleton<IHttpMessageHandlerFactory, CustomHandlerFactory>();

var app = builder.Build();
app.MapReverseProxy();
app.Run();