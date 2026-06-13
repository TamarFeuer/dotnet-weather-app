// ============================================================================
// REPOSITORY - storage contract
// ============================================================================
// ISeasonDataSource is the contract WeatherRepository uses to reach storage.
// Its drivers (SqlSeasonDataSource, JsonSeasonDataSource) implement it. This is
// what keeps WeatherRepository free of any file/JSON/database details and lets
// the storage be swapped behind the interface.
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public interface ISeasonDataSource
	{
		// Returns every season's range as stored. WeatherRepository picks the one
		// it needs; this contract knows nothing about "current season" logic.
		IReadOnlyList<Temperature> GetAll();
	}
}
