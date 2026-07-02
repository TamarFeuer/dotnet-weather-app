// ============================================================================
// SERVICE - contract
// ============================================================================
// IForecastService is what the forecast controller calls. It validates the
// city and gets its forecast, and it lists the supported cities for the UI.
using WeatherAPI.Models;

namespace WeatherAPI.Service
{
	public interface IForecastService
	{
		// The 5-day forecast for the city, or null if we don't support that city.
		Task<IReadOnlyList<ForecastDay>?> GetForecastAsync(string cityName);

		// The names of the cities we support (used to fill the dropdown).
		IReadOnlyList<string> GetCityNames();
	}
}
