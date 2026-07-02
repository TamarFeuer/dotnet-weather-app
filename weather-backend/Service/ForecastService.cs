// ============================================================================
// SERVICE - business logic
// ============================================================================
// ForecastService validates the city name against our supported list, then asks
// the forecast source (the Open-Meteo driver) for that city's days. It never
// knows the API exists - it only talks to the IForecastSource port.
using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Service
{
	public class ForecastService : IForecastService
	{
		private readonly IForecastSource _source;

		public ForecastService(IForecastSource source)
		{
			_source = source;
		}

		// async because the source makes a network call. We await it and return
		// the result; null means the city wasn't one we support.
		public async Task<IReadOnlyList<ForecastDay>?> GetForecastAsync(string cityName)
		{
			City? city = DutchCities.Find(cityName);
			if (city is null)
				return null;

			return await _source.GetForecastAsync(city);
		}

		public IReadOnlyList<string> GetCityNames() =>
			DutchCities.All.Select(c => c.Name).ToList();
	}
}
