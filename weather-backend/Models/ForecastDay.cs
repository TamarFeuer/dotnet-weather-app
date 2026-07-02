// ============================================================================
// MODELS - data model
// ============================================================================
// One day of a city's forecast: the date, its expected temperature range, and
// a short weather condition ("Sunny", "Rain", ...). Plain data, no logic.
namespace WeatherAPI.Models
{
	public class ForecastDay
	{
		// DateOnly = a date without a time-of-day (serializes as "2026-07-03").
		public DateOnly Date { get; set; }
		public int MinTemp { get; set; }
		public int MaxTemp { get; set; }
		public required string Condition { get; set; }
	}
}
