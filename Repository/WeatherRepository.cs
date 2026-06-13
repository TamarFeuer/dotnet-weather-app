// ============================================================================
// REPOSITORY - data access
// ============================================================================
// WeatherRepository implements IWeatherRepository. Its job is to fetch the data
// the service asks for and shape it into a Temperature.
//
// It doesn't touch storage directly: it goes through the ISeasonDataSource
// contract, whose driver (SqlSeasonDataSource for SQLite, JsonSeasonDataSource
// for the JSON file) is chosen in Program.cs. Swapping drivers is one line
// there, and this class never changes because it only ever sees the interface.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class WeatherRepository : IWeatherRepository
	{
		// WeatherRepository depends on the ISeasonDataSource interface;
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