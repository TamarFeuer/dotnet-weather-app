// ============================================================================
// MODELS - data model
// ============================================================================
// A city we can forecast for. Weather APIs work with coordinates, not names,
// so each city carries its latitude/longitude.
// (A record: a compact, immutable data holder - like WeatherInfo.)
namespace WeatherAPI.Models
{
	public record City(string Name, double Latitude, double Longitude);
}
