// ============================================================================
// REPOSITORY - forecast contract
// ============================================================================
// IForecastSource is the port for getting a city's forecast. Its driver
// (OpenMeteoForecastSource, calling the live Open-Meteo API) implements it;
// a seeded offline driver could implement it too. Same swap pattern as
// IMonthDataSource.
//
// Task<...> means the method is ASYNC: it starts work that takes time (a
// network call) and the result arrives later - the backend twin of the
// frontend's Observable. More on this when we write the driver.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public interface IForecastSource
	{
		// The days of the forecast for the given city (the city carries the
		// coordinates the weather API needs).
		Task<IReadOnlyList<ForecastDay>> GetForecastAsync(City city);
	}
}
