using Microsoft.AspNetCore.Mvc;

namespace LivePhotoFrame.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleDataController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet("[action]")]
    public IEnumerable<WeatherForecast> WeatherForecasts(int startDateIndex = 0)
    {
        return Enumerable.Range(1, 5).Select(index =>
        {
            var rng = Random.Shared;
            var c = rng.Next(-20, 55);
            return new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index + startDateIndex).ToString("d"),
                TemperatureC = c,
                TemperatureF = 32 + (int)(c / 0.5556),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };
        });
    }

    public sealed class WeatherForecast
    {
        public string DateFormatted { get; init; } = string.Empty;
        public int TemperatureC { get; init; }
        public int TemperatureF { get; init; }
        public string Summary { get; init; } = string.Empty;
    }
}
