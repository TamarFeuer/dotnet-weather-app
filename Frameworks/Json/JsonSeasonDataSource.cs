// ============================================================================
// LAYER 4 — Frameworks & Drivers (the outermost layer)
// ============================================================================
// JsonSeasonDataSource is the DB DRIVER. It is the ONLY class in the whole
// project that knows about files and JSON — System.IO and System.Text.Json
// appear here and nowhere else. That is exactly why it belongs in Frameworks:
// it is raw, technology-specific, and the most likely thing to be swapped
// (file today, SQL tomorrow).
//
// It implements ISeasonDataSource (the port defined inward in Interface
// Adapters), so the arrow still points INWARD: Frameworks -> InterfaceAdapters.
// Replacing this with a SqlSeasonDataSource later would not touch a single
// line of the gateway, the use case, or the entity.
using System.Text.Json;
using WeatherAPI.InterfaceAdapters;
using WeatherAPI.Models;

namespace WeatherAPI.Frameworks
{
	public class JsonSeasonDataSource : ISeasonDataSource
	{
		// JSON uses camelCase ("minTemp"); our properties are PascalCase
		// (MinTemp). Case-insensitive matching bridges the two.
		private static readonly JsonSerializerOptions Options =
			new() { PropertyNameCaseInsensitive = true };

		public IReadOnlyList<Temperature> GetAll()
		{
			// seasons.json is copied next to the built app (see the .csproj),
			// so we resolve it relative to the app's base directory.
			string path = Path.Combine(AppContext.BaseDirectory, "Frameworks", "Json", "seasons.json");
			// open the file → read its entire contents into one big string → close the file
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<List<Temperature>>(json, Options)
			       ?? new List<Temperature>();
		}
	}
}
