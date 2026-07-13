// ============================================================================
// CONTROLLER - API (HTTP endpoint)
// ============================================================================
// TypicalEndpoint receives the HTTP request and turns it into a service call.
// Its only job is translation; it holds no business logic. It calls the service
// through the ITypicalService interface, never the concrete class.
//
// Endpoint:  GET /api/weather/typical?month=January
//   -> { "minTemp": 1, "maxTemp": 6, "average": 3, "description": "Freezing" }
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Service;

namespace WeatherAPI.Controller
{
	[ApiController]
	[Route("api/weather")]
	public class TypicalEndpoint : ControllerBase
	{
		// Injected via the ITypicalService interface - the endpoint never
		// references the concrete TypicalService class.
		private readonly ITypicalService _service;

		public TypicalEndpoint(ITypicalService service)
		{
			_service = service;
		}

		// Handles: GET /api/weather/typical?month=January
		[HttpGet("typical")]
		public IActionResult GetTypical([FromQuery] string month)
		{
			if (string.IsNullOrWhiteSpace(month))
				return BadRequest(new { error = "Provide a month, e.g. ?month=January" });

			TypicalInfo? info = _service.GetTypical(month);
			if (info is null)
				return NotFound(new { error = $"Unknown month '{month}'." });

			// Returns { minTemp, maxTemp, average, description } as JSON.
			return Ok(info);
		}
	}
}
