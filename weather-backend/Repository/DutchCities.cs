// ============================================================================
// REPOSITORY - reference data
// ============================================================================
// The cities the app supports, with their coordinates. This is fixed reference
// data (city coordinates don't change), so a static list is enough - no
// database needed. The service uses it to validate a city name and to hand the
// coordinates to the forecast source.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public static class DutchCities
	{
		public static readonly IReadOnlyList<City> All =
		[
			new City("Amsterdam",  52.37, 4.90),
			new City("Rotterdam",  51.92, 4.48),
			new City("The Hague",  52.08, 4.31),
			new City("Utrecht",    52.09, 5.12),
			new City("Eindhoven",  51.44, 5.48),
			new City("Groningen",  53.22, 6.57),
			new City("Maastricht", 50.85, 5.69),
			new City("Arnhem",     51.98, 5.91),
		];

		// Case-insensitive lookup; null if we don't know the city.
		public static City? Find(string name) =>
			All.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
	}
}
