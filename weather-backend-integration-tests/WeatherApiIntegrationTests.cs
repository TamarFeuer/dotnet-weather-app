using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace WeatherAPI.IntegrationTests;

// IClassFixture shares ONE factory (so one container, booted once) across every
// test in this class. The tests then send real HTTP requests through the whole
// chain: Kestrel -> routing -> controller -> service -> repository -> EF Core ->
// the real PostgreSQL container. This is the seam none of the 95 unit tests
// touch, because they all use fakes.
public class WeatherApiIntegrationTests : IClassFixture<WeatherApiFactory>
{
    private readonly HttpClient _client;

    public WeatherApiIntegrationTests(WeatherApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // The JSON the API returns for a month, matching TypicalInfo.
    private record TypicalDto(int MinTemp, int MaxTemp, int Average, string Description);

    [Fact]
    public async Task Typical_ReturnsSeededData_ForAKnownMonth()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather/typical?month=July");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TypicalDto? info = await response.Content.ReadFromJsonAsync<TypicalDto>();

        // These exact numbers only appear if the migration seeded the twelve
        // months AND the whole chain read them back correctly. A unit test with a
        // fake repository can never prove the seed and the real query.
        Assert.NotNull(info);
        Assert.Equal(14, info!.MinTemp);
        Assert.Equal(23, info.MaxTemp);
        Assert.Equal(18, info.Average);
        Assert.Equal("Warm", info.Description);
    }

    [Fact]
    public async Task Typical_IsCaseInsensitive()
    {
        // "july" in lower case must still find "July". This is the exact spot that
        // could break silently if the month lookup ever moved into raw SQL, since
        // PostgreSQL is case-sensitive where SQLite was not. Proving it against the
        // real database is the point.
        HttpResponseMessage response = await _client.GetAsync("/api/weather/typical?month=july");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TypicalDto? info = await response.Content.ReadFromJsonAsync<TypicalDto>();
        Assert.Equal("Warm", info!.Description);
    }

    [Fact]
    public async Task Typical_Returns404_ForAnUnknownMonth()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather/typical?month=Smarch");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Typical_Returns400_WhenNoMonthIsGiven()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather/typical");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cities_ReturnsTheSupportedList()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/weather/cities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string[]? cities = await response.Content.ReadFromJsonAsync<string[]>();

        Assert.NotNull(cities);
        Assert.Equal(8, cities!.Length);
        Assert.Contains("Amsterdam", cities);
    }

    // Deliberately no test for /forecast here: it calls the live Open-Meteo API
    // over the internet, whose data changes daily. That belongs to the driver
    // unit tests (which stub the HTTP layer), not to an integration test, which
    // must stay offline and deterministic.
}
