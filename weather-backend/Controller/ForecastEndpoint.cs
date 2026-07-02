// ============================================================================
// CONTROLLER - API (HTTP endpoints)
// ============================================================================
// The forecast endpoints. Separate from WeatherEndpoint (the month feature) so
// each controller has one job. Both live under /api/weather.
//
//   GET /api/weather/cities            -> ["Amsterdam", "Rotterdam", ...]
//   GET /api/weather/forecast?city=... -> [ { date, minTemp, maxTemp, condition }, ... ]
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Models;
using WeatherAPI.Service;

namespace WeatherAPI.Controller
{
	[ApiController]
	[Route("api/weather")]
	public class ForecastEndpoint : ControllerBase
	{
		private readonly IForecastService _service;

		public ForecastEndpoint(IForecastService service)
		{
			_service = service;
		}

		// The list of cities the dropdown can offer.
		[HttpGet("cities")]
		public IActionResult GetCities() => Ok(_service.GetCityNames());

		// The 5-day forecast for one city. async Task<> because the service call
		// reaches out to the live weather API - the controller awaits it.
		[HttpGet("forecast")]
		public async Task<IActionResult> GetForecast([FromQuery] string city)
		{
			if (string.IsNullOrWhiteSpace(city))
				return BadRequest(new { error = "Provide a city, e.g. ?city=Amsterdam" });

			IReadOnlyList<ForecastDay>? forecast = await _service.GetForecastAsync(city);
			if (forecast is null)
				return NotFound(new { error = $"Unknown city '{city}'." });

			return Ok(forecast);
		}
	}
}
