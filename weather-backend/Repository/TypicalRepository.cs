// ============================================================================
// REPOSITORY - data access
// ============================================================================
// TypicalRepository implements ITypicalRepository. Its job is to fetch the data
// the service asks for and shape it into a Temperature.
//
// It doesn't touch storage directly: it goes through the IMonthDataSource
// contract, whose driver (SqlMonthDataSource for SQLite, JsonMonthDataSource
// for the JSON file) is chosen in Program.cs. Swapping drivers is one line
// there, and this class never changes because it only ever sees the interface.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class TypicalRepository : ITypicalRepository
	{
		// TypicalRepository depends on the IMonthDataSource interface; the
		// concrete driver (JsonMonthDataSource / SqlMonthDataSource) is injected
		// via the constructor, wired up in Program.cs.
		private readonly IMonthDataSource _dataSource;

		public TypicalRepository(IMonthDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public Temperature? GetByMonth(string month)
		{
			// Load the rows from storage, then pick the row for the requested
			// month (case-insensitive). Returns null if the month is unknown.
			IReadOnlyList<Temperature> all = _dataSource.GetAll();
			return all.FirstOrDefault(t =>
				string.Equals(t.Month, month, StringComparison.OrdinalIgnoreCase));
		}
	}
}
