// ============================================================================
// SERVICE - contract
// ============================================================================
// IWeatherService is the contract the controller calls. WeatherEndpoint depends
// on this interface and calls GetWeather(month); it never touches the concrete
// WeatherService.
namespace WeatherAPI.Service
{
	public interface IWeatherService
	{
		WeatherInfo? GetWeather(string month);
	}
}
