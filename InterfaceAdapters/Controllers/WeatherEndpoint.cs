// ============================================================================
// LAYER 3 — Interface Adapters
// ============================================================================
// WeatherEndpoint is a CONTROLLER — the classic Interface Adapter on the INPUT
// side. Its only job is translation: receive the HTTP request and turn it into
// a use-case call (it contains NO business logic itself).
//
// It does reference ASP.NET ([ApiController], ControllerBase) — but in the
// strict layout the controller is still an Interface Adapter; the actual web
// FRAMEWORK (the Kestrel host, routing, DI wiring) is what lives in the
// Frameworks layer, i.e. Program.cs and the NuGet packages.
//
// The Dependency Rule in action:
//   WeatherEndpoint --> IWeatherService --> IWeatherRepository
// Every arrow points inward, toward the abstractions, never the reverse.
//
// Endpoint:  GET /api/weather/temperature
//   -> { "temperature": 22 }
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.UseCases;

namespace WeatherAPI.InterfaceAdapters
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