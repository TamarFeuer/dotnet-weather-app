// ============================================================================
// SERVICE - contract
// ============================================================================
// IWeatherService is the contract the controller calls. WeatherEndpoint depends
// on this interface and calls GetTemperature(); it never touches the concrete
// WeatherService.
namespace WeatherAPI.Service
{
	public interface IWeatherService
	{
		int GetTemperature();
	}
}