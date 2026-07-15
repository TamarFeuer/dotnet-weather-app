using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Controller;
using WeatherAPI.Models;
using WeatherAPI.Service;

namespace WeatherAPI.UnitTests;

// Fake services. A controller's only job is translating HTTP into a service call
// and the answer back into a status code, so that is exactly what we test: given
// what the service returns, does the controller pick the right response?
//
// No web server is started, no HTTP is sent. We call the method directly and look
// at the IActionResult that comes back.
public class FakeTypicalService(TypicalInfo? info) : ITypicalService
{
    public TypicalInfo? GetTypical(string month) => info;
}

public class FakeForecastService(IReadOnlyList<ForecastDay>? days, IReadOnlyList<string> cities)
    : IForecastService
{
    public Task<IReadOnlyList<ForecastDay>?> GetForecastAsync(string cityName) =>
        Task.FromResult(days);

    public IReadOnlyList<string> GetCityNames() => cities;
}

public class TypicalEndpointTests
{
    private static readonly TypicalInfo Info = new(14, 23, 18, "Warm");

    [Fact]
    public void GetTypical_Returns200WithTheInfo_WhenTheMonthIsKnown()
    {
        var endpoint = new TypicalEndpoint(new FakeTypicalService(Info));

        IActionResult result = endpoint.GetTypical("July");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(Info, ok.Value);
    }

    [Fact]
    public void GetTypical_Returns404_WhenTheServiceKnowsNoSuchMonth()
    {
        // The service answers null for an unknown month. Turning that null into a
        // 404 rather than a 200-with-nothing is the controller's decision.
        var endpoint = new TypicalEndpoint(new FakeTypicalService(null));

        IActionResult result = endpoint.GetTypical("Smarch");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetTypical_Returns400_WhenNoMonthIsGiven(string month)
    {
        // A missing month is the caller's mistake (400), not a missing resource
        // (404). Getting that distinction right is what this pins down.
        var endpoint = new TypicalEndpoint(new FakeTypicalService(Info));

        IActionResult result = endpoint.GetTypical(month);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}

public class ForecastEndpointTests
{
    private static readonly IReadOnlyList<ForecastDay> Days =
    [
        new ForecastDay { Date = new DateOnly(2026, 7, 3), MinTemp = 13, MaxTemp = 21, Condition = "Cloudy" },
    ];

    private static readonly IReadOnlyList<string> Cities = ["Amsterdam", "Rotterdam"];

    [Fact]
    public void GetCities_Returns200WithTheCityList()
    {
        var endpoint = new ForecastEndpoint(new FakeForecastService(Days, Cities));

        IActionResult result = endpoint.GetCities();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(Cities, ok.Value);
    }

    [Fact]
    public async Task GetForecast_Returns200WithTheDays_WhenTheCityIsKnown()
    {
        var endpoint = new ForecastEndpoint(new FakeForecastService(Days, Cities));

        IActionResult result = await endpoint.GetForecast("Amsterdam");

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(Days, ok.Value);
    }

    [Fact]
    public async Task GetForecast_Returns404_WhenTheServiceKnowsNoSuchCity()
    {
        var endpoint = new ForecastEndpoint(new FakeForecastService(null, Cities));

        IActionResult result = await endpoint.GetForecast("Paris");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetForecast_Returns400_WhenNoCityIsGiven(string city)
    {
        var endpoint = new ForecastEndpoint(new FakeForecastService(Days, Cities));

        IActionResult result = await endpoint.GetForecast(city);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
