using System.Globalization;
using System.Net;
using System.Text;
using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Tests;

// A stub HTTP layer. HttpClient does not send anything by itself: it hands the
// request to an HttpMessageHandler, and THAT is what talks to the network. Swap
// the handler and the same HttpClient answers from memory instead.
//
// So we can test the driver's real job - build the right URL, and translate
// Open-Meteo's JSON into our ForecastDay objects - without one byte of network
// traffic, without depending on Open-Meteo being up, and with weather data that
// does not change every day.
public class StubHttpMessageHandler(string json) : HttpMessageHandler
{
    // Remembers the URL that was asked for, so a test can check it.
    public Uri? RequestedUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestedUri = request.RequestUri;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });
    }
}

public class OpenMeteoForecastSourceTests
{
    private static readonly City Amsterdam = new("Amsterdam", 52.37, 4.90);

    // Open-Meteo answers with PARALLEL ARRAYS, not a list of day objects. Zipping
    // them back together by index is the driver's work, and getting that wrong
    // would silently pair the wrong temperature with the wrong date.
    private const string TwoDaysJson = """
    {
      "daily": {
        "time": ["2026-07-03", "2026-07-04"],
        "temperature_2m_min": [12.6, 13.2],
        "temperature_2m_max": [20.4, 21.8],
        "weather_code": [3, 61]
      }
    }
    """;

    private static (OpenMeteoForecastSource source, StubHttpMessageHandler handler) Build(string json)
    {
        var handler = new StubHttpMessageHandler(json);
        return (new OpenMeteoForecastSource(new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task GetForecastAsync_ZipsTheParallelArraysIntoDays()
    {
        (OpenMeteoForecastSource source, _) = Build(TwoDaysJson);

        IReadOnlyList<ForecastDay> days = await source.GetForecastAsync(Amsterdam);

        Assert.Equal(2, days.Count);

        Assert.Equal(new DateOnly(2026, 7, 3), days[0].Date);
        Assert.Equal(13, days[0].MinTemp); // 12.6 rounded
        Assert.Equal(20, days[0].MaxTemp); // 20.4 rounded
        Assert.Equal("Cloudy", days[0].Condition); // WMO code 3

        Assert.Equal(new DateOnly(2026, 7, 4), days[1].Date);
        Assert.Equal(13, days[1].MinTemp); // 13.2 rounded
        Assert.Equal(22, days[1].MaxTemp); // 21.8 rounded
        Assert.Equal("Rain", days[1].Condition); // WMO code 61
    }

    [Fact]
    public async Task GetForecastAsync_AsksForTheCityCoordinatesAndFiveDays()
    {
        (OpenMeteoForecastSource source, StubHttpMessageHandler handler) = Build(TwoDaysJson);

        await source.GetForecastAsync(Amsterdam);

        string url = handler.RequestedUri!.ToString();

        Assert.Contains("api.open-meteo.com", url);
        Assert.Contains("latitude=52.37", url);
        Assert.Contains("longitude=4.9", url);
        Assert.Contains("forecast_days=5", url);
    }

    [Fact]
    public async Task GetForecastAsync_BuildsTheUrlWithADotEvenUnderADutchLocale()
    {
        // The coordinates are interpolated into the URL as numbers. Under a Dutch
        // locale a decimal point becomes a COMMA (52,37), which Open-Meteo would
        // reject. The URL must not depend on the machine's regional settings.
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-NL");

            (OpenMeteoForecastSource source, StubHttpMessageHandler handler) = Build(TwoDaysJson);

            await source.GetForecastAsync(Amsterdam);

            string url = handler.RequestedUri!.ToString();

            Assert.Contains("latitude=52.37", url);
            Assert.DoesNotContain("52,37", url);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    // The WMO table is a long switch, and a mistyped number there would quietly
    // label rain as sunshine. Each row checks one branch of it.
    [Theory]
    [InlineData(0, "Clear")]
    [InlineData(1, "Partly cloudy")]
    [InlineData(2, "Partly cloudy")]
    [InlineData(3, "Cloudy")]
    [InlineData(45, "Fog")]
    [InlineData(51, "Drizzle")]
    [InlineData(61, "Rain")]
    [InlineData(71, "Snow")]
    [InlineData(80, "Showers")]
    [InlineData(85, "Snow showers")]
    [InlineData(95, "Thunderstorm")]
    [InlineData(999, "Unknown")]
    public async Task GetForecastAsync_TranslatesTheWmoCode(int code, string expected)
    {
        string json = $$"""
        {
          "daily": {
            "time": ["2026-07-03"],
            "temperature_2m_min": [10.0],
            "temperature_2m_max": [20.0],
            "weather_code": [{{code}}]
          }
        }
        """;

        (OpenMeteoForecastSource source, _) = Build(json);

        IReadOnlyList<ForecastDay> days = await source.GetForecastAsync(Amsterdam);

        Assert.Equal(expected, days[0].Condition);
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsNothing_WhenTheApiSendsNoDailyBlock()
    {
        // If the shape we depend on is missing, return an empty list rather than
        // throwing. The app degrades instead of crashing.
        (OpenMeteoForecastSource source, _) = Build("""{ "error": true }""");

        IReadOnlyList<ForecastDay> days = await source.GetForecastAsync(Amsterdam);

        Assert.Empty(days);
    }
}
