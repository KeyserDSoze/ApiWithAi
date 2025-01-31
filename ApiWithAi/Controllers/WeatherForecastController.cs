using Microsoft.AspNetCore.Mvc;

namespace ApiWithAi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Retrieves a list of weather forecasts for a specified city.
        /// </summary>
        /// <remarks>
        /// This endpoint expects a <c>city</c> query parameter.
        /// </remarks>
        /// <param name="city">The city to get weather forecasts for.</param>
        /// <returns>An <see cref="IEnumerable{WeatherForecast}"/> containing the list of weather forecasts.</returns>
        /// <example>
        /// GET /WeatherForecast/Get?city=New%20York
        /// </example>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get([FromQuery] string city)
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
