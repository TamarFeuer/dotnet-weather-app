// ============================================================================
// SERVICE - contract
// ============================================================================
// ITypicalService is the contract the controller calls. TypicalEndpoint depends
// on this interface and calls GetTypical(month); it never touches the concrete
// TypicalService.
namespace WeatherAPI.Service
{
	public interface ITypicalService
	{
		TypicalInfo? GetTypical(string month);
	}
}
