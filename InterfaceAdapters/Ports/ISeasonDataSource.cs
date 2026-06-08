// ============================================================================
// LAYER 3 — Interface Adapters
// ============================================================================
// ISeasonDataSource is the PORT the gateway uses to reach storage. It lives
// here (inner) and is IMPLEMENTED out in Frameworks (JsonSeasonDataSource).
//
// This is the same Dependency-Rule trick as IWeatherRepository, applied one
// level lower: the gateway owns the contract, the framework obeys it. That is
// what lets the gateway stay clean of any file/JSON/database details — those
// live entirely in the outer layer behind this interface.
using WeatherAPI.Models;

namespace WeatherAPI.InterfaceAdapters
{
	public interface ISeasonDataSource
	{
		// Returns every season's range as stored. The gateway picks the one
		// it needs; this contract knows nothing about "current season" logic.
		IReadOnlyList<Temperature> GetAll();
	}
}
