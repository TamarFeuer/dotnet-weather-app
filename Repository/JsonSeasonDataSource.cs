// ============================================================================
// REPOSITORY - JSON file driver
// ============================================================================
// JsonSeasonDataSource implements ISeasonDataSource by reading seasons.json.
// It is the only class that knows about files and JSON (System.IO and
// System.Text.Json appear here and nowhere else).
//
// It is the sibling of SqlSeasonDataSource: same contract, different storage.
// Swapping to it is one line in Program.cs, and nothing else changes.
using System.Text.Json;
using WeatherAPI.Models;

namespace WeatherAPI.Repository
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
			string path = Path.Combine(AppContext.BaseDirectory, "Repository", "seasons.json");
			// open the file → read its entire contents into one big string → close the file
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<List<Temperature>>(json, Options)
			       ?? new List<Temperature>();
		}
	}
}
