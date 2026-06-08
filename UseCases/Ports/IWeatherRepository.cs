// ============================================================================
// LAYER 2 — Application Business Rules (Use Cases)
// ============================================================================
// IWeatherRepository is an OUTGOING port (a data-access port). The use case USES it to reach out for data — it calls out, rather than
// being called in.
//
// The contract lives here in the inner layer; the WeatherRepository gateway
// (one ring out, in Interface Adapters) implements it. Outer obeys inner: the
// business logic dictates the shape it wants, storage obeys — which is what
// lets us swap file <-> SQLite without touching any business logic.
using WeatherAPI.Models;

namespace WeatherAPI.UseCases
{
	public interface IWeatherRepository
	{
		Temperature GetBySeason(string season);
	}
}