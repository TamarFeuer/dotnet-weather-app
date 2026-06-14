// ============================================================================
// SERVICE - business logic
// ============================================================================
// WeatherService takes the month's raw range from the repository and does the
// business logic: it computes the typical (average) temperature and turns that
// into a short description. It implements IWeatherService.
//
// Pure logic - no ASP.NET, no database. The repository arrives via the
// constructor (constructor injection), so it never knows where data comes from.
using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Service
{
	public class WeatherService : IWeatherService
	{
		private readonly IWeatherRepository _repository;

		// The repository is "injected" from outside. We depend on the
		// abstraction (IWeatherRepository), not a specific implementation.
		public WeatherService(IWeatherRepository repository)
		{
			_repository = repository;
		}

		// Looks up the month, then ENRICHES it: the typical temperature is the
		// midpoint of the range, and Describe() labels it. Returns null if the
		// month is unknown.
		public WeatherInfo? GetWeather(string month)
		{
			Temperature? row = _repository.GetByMonth(month);
			if (row is null)
				return null;

			int average = (row.MinTemp + row.MaxTemp) / 2;
			string description = Describe(average);
			return new WeatherInfo(row.MinTemp, row.MaxTemp, average, description);
		}

		// Business rule: turn the typical temperature into a simple label.
		private static string Describe(int average)
		{
			if (average <= 5) return "Freezing";
			if (average <= 11) return "Cold";
			if (average <= 16) return "Mild";
			return "Warm";
		}
	}
}
