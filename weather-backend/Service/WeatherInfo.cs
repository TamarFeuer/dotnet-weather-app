namespace WeatherAPI.Service
{
	// What the service produces for a month: the raw min/max range from storage,
	// plus two values the service computes itself - the typical (average)
	// temperature and a short description of it.
	public record WeatherInfo(int MinTemp, int MaxTemp, int Average, string Description);
}
