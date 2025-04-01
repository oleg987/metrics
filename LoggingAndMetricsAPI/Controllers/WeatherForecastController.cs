using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace LoggingAndMetricsAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IMeterFactory _meterFactory;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMeterFactory meterFactory)
    {
        _logger = logger;
        _meterFactory = meterFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        // Let`s simulate data receiving time.
        var delay = Random.Shared.Next(1000);

        await Task.Delay(delay, ct);

        var meter = _meterFactory.Create("WeatherForecast");

        var successCounter = meter.CreateCounter<int>("ok_counter");
        var badCounter = meter.CreateCounter<int>("bad_counter");
        var notFoundCounter = meter.CreateCounter<int>("not_found_counter");
        var errorCounter = meter.CreateCounter<int>("error_counter");
        

        // Simulate some errors
        if (delay > 900)
        {
            if (delay - 900 < 33)
            {
                badCounter.Add(1);
                return BadRequest();
            }

            if (delay - 900 < 66)
            {
                notFoundCounter.Add(1);
                return NotFound();
            }

            errorCounter.Add(1);
            return StatusCode(500);
        }
        
        successCounter.Add(1);
        
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        return Ok(forecast);
    }
}