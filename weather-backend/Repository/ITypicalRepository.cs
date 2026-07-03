// ============================================================================
// REPOSITORY - contract
// ============================================================================
// IWeatherRepository is the data-access contract. The service depends on it to
// fetch data; WeatherRepository implements it. Keeping it an interface lets the
// service stay independent of how the data is actually stored.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public interface IWeatherRepository
	{
		Temperature? GetByMonth(string month);
	}
}