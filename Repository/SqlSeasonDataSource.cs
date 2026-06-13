// ============================================================================
// REPOSITORY - SQLite driver
// ============================================================================
// SqlSeasonDataSource implements ISeasonDataSource by querying the SQLite
// database through EF Core's DbContext.
//
// It is the sibling of JsonSeasonDataSource: same contract, different storage.
// WeatherRepository can't tell them apart - switching between them is one line
// in Program.cs.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class SqlSeasonDataSource : ISeasonDataSource
	{
		private readonly WeatherDbContext _db;

		// The DbContext is injected (registered in Program.cs).
		public SqlSeasonDataSource(WeatherDbContext db)
		{
			_db = db;
		}

		// ToList() runs the query: EF Core emits "SELECT * FROM Temperatures"
		// and turns each row back into a Temperature object.
		public IReadOnlyList<Temperature> GetAll() => _db.Temperatures.ToList();
	}
}
