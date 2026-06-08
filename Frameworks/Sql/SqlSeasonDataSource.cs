// ============================================================================
// LAYER 4 — Frameworks & Drivers (the outermost layer)
// ============================================================================
// SqlSeasonDataSource is a DRIVER — a doer that KEEPS the ISeasonDataSource
// promise by querying the SQLite database (through EF Core's DbContext).
//
// It is the exact sibling of JsonSeasonDataSource: same promise, different
// technology. The gateway (WeatherRepository) can't tell them apart — that is
// the whole point of the port. Switching between them is one line in Program.cs.
using WeatherAPI.InterfaceAdapters;
using WeatherAPI.Models;

namespace WeatherAPI.Frameworks
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
