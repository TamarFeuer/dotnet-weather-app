// ============================================================================
// REPOSITORY - relational driver (EF Core)
// ============================================================================
// SqlMonthDataSource implements IMonthDataSource by querying a relational
// database through EF Core's DbContext.
//
// Note the name: it is the SQL driver, not the SQLite driver. Nothing in this
// file names a database engine, because EF Core is itself an abstraction over
// them. The engine is chosen in Program.cs (UseNpgsql today, UseSqlite before,
// UseSqlServer if you liked), and this class never noticed the switch.
//
// It is the sibling of JsonMonthDataSource: same contract, different storage.
// TypicalRepository can't tell them apart - switching between them is one line
// in Program.cs.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class SqlMonthDataSource : IMonthDataSource
	{
		private readonly WeatherDbContext _db;

		// The DbContext is injected (registered in Program.cs).
		public SqlMonthDataSource(WeatherDbContext db)
		{
			_db = db;
		}

		// ToList() runs the query: EF Core emits "SELECT * FROM Temperatures"
		// and turns each row back into a Temperature object.
		public IReadOnlyList<Temperature> GetAll() => _db.Temperatures.ToList();
	}
}
