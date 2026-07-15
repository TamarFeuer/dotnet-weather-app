using WeatherAPI.Models;
using WeatherAPI.Repository;
using WeatherAPI.Service;

namespace WeatherAPI.UnitTests;

// A fake forecast source.
//
// The real driver (OpenMeteoForecastSource) calls the live Open-Meteo API over
// the internet. A test must never do that:
//   - the data changes every day, so there is nothing stable to assert on,
//   - the network makes tests slow and flaky,
//   - and every build would lean on somebody else's free service for nothing.
//
// Because ForecastService depends on the IForecastSource PORT rather than on the
// concrete driver, we can hand it this stub instead. The service cannot tell the
// difference. That is the whole point of the port.
public class FakeForecastSource(IReadOnlyList<ForecastDay> days) : IForecastSource
{
    // Remembers what it was asked for, so a test can check the service passed
    // the right city through.
    public City? ReceivedCity { get; private set; }

    public Task<IReadOnlyList<ForecastDay>> GetForecastAsync(City city)
    {
        ReceivedCity = city;
        return Task.FromResult(days);
    }
}

public class ForecastServiceTests
{
    private static readonly IReadOnlyList<ForecastDay> TwoFakeDays =
    [
        new ForecastDay { Date = new DateOnly(2026, 1, 1), MinTemp = 1, MaxTemp = 5, Condition = "Cloudy" },
        new ForecastDay { Date = new DateOnly(2026, 1, 2), MinTemp = 2, MaxTemp = 6, Condition = "Rain" },
    ];

    [Fact]
    public async Task GetForecastAsync_ReturnsNull_ForAnUnsupportedCity()
    {
        var source = new FakeForecastSource(TwoFakeDays);
        var service = new ForecastService(source);

        IReadOnlyList<ForecastDay>? result = await service.GetForecastAsync("Paris");

        // The service rejects the city itself, and the controller turns this
        // null into a 404.
        Assert.Null(result);

        // It also never touched the source. That matters: an unknown city must
        // not cost a pointless call to the weather API.
        Assert.Null(source.ReceivedCity);
    }

    [Fact]
    public async Task GetForecastAsync_LooksUpTheCityAndPassesItToTheSource()
    {
        var source = new FakeForecastSource(TwoFakeDays);
        var service = new ForecastService(source);

        // Lower case on purpose: the service should still find the city.
        IReadOnlyList<ForecastDay>? result = await service.GetForecastAsync("amsterdam");

        Assert.Equal(TwoFakeDays, result);

        // The source was handed the real City object, coordinates and all. Those
        // coordinates are what the Open-Meteo driver puts in its URL.
        Assert.NotNull(source.ReceivedCity);
        Assert.Equal("Amsterdam", source.ReceivedCity!.Name);
        Assert.Equal(52.37, source.ReceivedCity.Latitude);
        Assert.Equal(4.90, source.ReceivedCity.Longitude);
    }

    [Fact]
    public void GetCityNames_ReturnsEverySupportedCity()
    {
        var service = new ForecastService(new FakeForecastSource(TwoFakeDays));

        IReadOnlyList<string> names = service.GetCityNames();

        Assert.Equal(DutchCities.All.Count, names.Count);
        Assert.Contains("Amsterdam", names);
        Assert.Contains("Maastricht", names);
    }
}
