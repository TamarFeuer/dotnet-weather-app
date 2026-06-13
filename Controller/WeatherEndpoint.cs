// ============================================================================
// CONTROLLER - API (HTTP endpoint)
// ============================================================================
// WeatherEndpoint receives the HTTP request and turns it into a service call.
// Its only job is translation; it holds no business logic. It calls the service
// through the IWeatherService interface, never the concrete class.
//
// Endpoint:  GET /api/weather/temperature
//   -> { "temperature": 22 }
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Service;

namespace WeatherAPI.Controller
{
	[ApiController]
	[Route("api/weather")]
	public class WeatherEndpoint : ControllerBase
	{
		// Injected via the IWeatherService interface — the endpoint never
		// references the concrete WeatherService class.
		private readonly IWeatherService _service;

		public WeatherEndpoint(IWeatherService service)
		{
			_service = service;
		}

		// Handles: GET /api/weather/temperature
		[HttpGet("temperature")]
		public IActionResult GetTemperature()
		{
			int temperature = _service.GetTemperature();
			// Wrap the bare int so the JSON comes out as { "temperature": 22 }.
			return Ok(new { temperature });
		}
	}
}