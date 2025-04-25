using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

Console.WriteLine("Connecting to SignalR");

var cookies = new CookieContainer();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7200/chathub?userId=User2", options =>
    {
        options.HttpMessageHandlerFactory = _ => new HttpClientHandler
        {
            CookieContainer = cookies,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<RateMessage>("ReceiveMessage", msg =>
{
    Console.WriteLine($"[{DateTimeOffset.FromUnixTimeMilliseconds(msg.Timestamp)}] Rate: {msg.Rate}");
});

await connection.StartAsync();
Console.WriteLine("Connected to hub");

await connection.InvokeAsync("Subscribe", "User2", 1);
Console.WriteLine("Sent subscription for metric 1 (TimeRate)");




await Task.Delay(Timeout.Infinite);