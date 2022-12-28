using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Api.Security;

namespace StartingMultiTenant.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly TokenBuilder _tokenBuilder;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            TokenBuilder tokenBuilder) {
            _logger = logger;
            _tokenBuilder = tokenBuilder;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get() {
            string ss= _tokenBuilder.CreateJwtToken(new Model.Domain.ApiClientModel() { ClientId="sdfds"}) ;
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}