using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpPost("sender")]
        public async Task<IActionResult> Sender(string msg)
        {
            int count = 60;
            while(count != 0){
                await _hubContext.Clients.Group(MetricType.TimeRate.ToString()).SendAsync("ReceiveMessage", new RateMessage(DateTime.UtcNow.Millisecond, GetRandomRate()));
                await Task.Delay(1000);
                count--;
            }
            return Ok("Message sent");
        }


        private static decimal GetRandomRate()
        {
            return Math.Round((decimal)(Random.Shared.NextDouble() * 100), 4); 
        }
    }
}
