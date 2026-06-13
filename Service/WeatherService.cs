// ============================================================================
// SERVICE - business logic
// ============================================================================
// WeatherService does the work: find the current season, get its range, and
// pick a random value inside it. It implements IWeatherService and uses
// IWeatherRepository to get data.
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
		private readonly Random _random;

		// The repository is "injected" from outside. We depend on the
		// abstraction (IWeatherRepository), not a specific implementation.
		public WeatherService(IWeatherRepository repository)
		{
			_repository = repository;
			_random = new Random();
		}

		public int GetTemperature()
		{
			string season = GetCurrentSeason();
			// Ask the contract for the range; we don't know or care that the
			// data is currently hardcoded behind WeatherRepository.
			Temperature temp = _repository.GetBySeason(season);
			// Max + 1 because Random.Next's upper bound is EXCLUSIVE.
			return _random.Next(temp.MinTemp, temp.MaxTemp + 1);
		}

		// Business rule: map the current calendar month to a season.
		private string GetCurrentSeason()
		{
			int month = DateTime.Now.Month;
			if (month >= 6 && month <= 8) return "Summer";
			if (month >= 9 && month <= 11) return "Autumn";
			if (month >= 3 && month <= 5) return "Spring";
			return "Winter";
		}
	}
}