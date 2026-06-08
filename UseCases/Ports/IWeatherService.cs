// ============================================================================
// LAYER 2 — Application Business Rules (Use Cases)
// ============================================================================
// IWeatherService is the USE CASE INPUT PORT — the contract the controller
// calls to trigger the use case. WeatherEndpoint depends on this port and
// calls GetTemperature(); it never touches the concrete WeatherService.
namespace WeatherAPI.UseCases
{
	public interface IWeatherService
	{
		int GetTemperature();
	}
}