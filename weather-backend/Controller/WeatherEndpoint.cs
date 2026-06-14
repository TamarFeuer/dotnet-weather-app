// ============================================================================
// CONTROLLER - API (HTTP endpoint)
// ============================================================================
// WeatherEndpoint receives the HTTP request and turns it into a service call.
// Its only job is translation; it holds no business logic. It calls the service
// through the IWeatherService interface, never the concrete class.
//
// Endpoint:  GET /api/weather/temperature?month=January
//   -> { "minTemp": 1, "maxTemp": 6, "average": 3, "description": "Freezing" }
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Service;

namespace WeatherAPI.Controller
{
	[ApiController]
	[Route("api/weather")]
	public class WeatherEndpoint : ControllerBase
	{
		// Injected via the IWeatherService interface - the endpoint never
		// references the concrete WeatherService class.
		private readonly IWeatherService _service;

		public WeatherEndpoint(IWeatherService service)
		{
			_service = service;
		}

		// Handles: GET /api/weather/temperature?month=January
		[HttpGet("temperature")]
		public IActionResult GetTemperature([FromQuery] string month)
		{
			if (string.IsNullOrWhiteSpace(month))
				return BadRequest(new { error = "Provide a month, e.g. ?month=January" });

			WeatherInfo? info = _service.GetWeather(month);
			if (info is null)
				return NotFound(new { error = $"Unknown month '{month}'." });

			// Returns { minTemp, maxTemp, average, description } as JSON.
			return Ok(info);
		}
	}
}
