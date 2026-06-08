// ============================================================================
// LAYER 3 — Interface Adapters
// ============================================================================
// WeatherRepository is a GATEWAY — the Interface Adapter on the DATA side.
// It is the concrete implementation of the IWeatherRepository port
// (defined inward in Use Cases), and its job is to ADAPT the use case's data
// needs to a real source.

// Today the active source is SQLite, reached through the ISeasonDataSource port
// whose driver (SqlSeasonDataSource) lives in the FRAMEWORKS layer. A JSON-file
// driver (JsonSeasonDataSource, reading Frameworks/Json/seasons.json) also sits
// behind the same port — swapping between them is one line in Program.cs, and
// the inner layers never change because they only ever see the interface.
using WeatherAPI.Models;
using WeatherAPI.UseCases;

namespace WeatherAPI.InterfaceAdapters
{
	public class WeatherRepository : IWeatherRepository
	{
		// The WeatherRepository gateway depends on ISeasonDataSource port;
		// the concrete driver (JsonSeasonDataSource / SqlSeasonDataSource) is injected via the constructor, wired up in Program.cs
		private readonly ISeasonDataSource _dataSource;

		public WeatherRepository(ISeasonDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public Temperature GetBySeason(string season)
		{
			// Load the rows from storage, then ADAPT: pick the row for the
			// requested season, falling back to Winter (the old default).
			IReadOnlyList<Temperature> all = _dataSource.GetAll();
			return all.FirstOrDefault(t => t.Season == season)
			       ?? all.First(t => t.Season == "Winter");
		}
	}
}