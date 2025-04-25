var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache")
    .WithImage("redis:7.2");

var apiService1 = builder.AddProject<Projects.WebApplication>("apiservice1")
    .WithEndpoint("https", e => { e.Port = 7101; e.IsProxied = false; e.IsExternal = false; })
    .WithEndpoint("http", e => { e.Port = 5001; e.IsProxied = false; e.IsExternal = false; })
    .WithReference(redis);
var apiService2 = builder.AddProject<Projects.WebApplication>("apiservice2")
    .WithEndpoint("https", e => { e.Port = 7102; e.IsProxied = false; e.IsExternal = false; })
    .WithEndpoint("http", e => { e.Port = 5002; e.IsProxied = false; })
    .WithReference(redis);

var gateway = builder.AddProject<Projects.ApiGateway>("gateway")
    .WithReference(apiService1)
    .WithReference(apiService2)
    .WithEndpoint("https", e =>
    {
        e.Port = 7200;
        e.IsProxied = false;
    });

builder.Build().Run();
