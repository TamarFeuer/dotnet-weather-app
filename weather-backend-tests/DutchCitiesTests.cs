using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Tests;

// DutchCities is plain reference data plus one lookup method, so these tests
// need nothing at all: no fakes, no database, no network.
public class DutchCitiesTests
{
    [Fact]
    public void All_HoldsCitiesWithPlausibleDutchCoordinates()
    {
        Assert.NotEmpty(DutchCities.All);

        Assert.All(DutchCities.All, city =>
        {
            Assert.False(string.IsNullOrWhiteSpace(city.Name));

            // The Netherlands lies roughly between 50-54 degrees north and 3-8
            // degrees east. Coordinates outside that box would mean a typo, and
            // a typo here means the forecast silently comes back for the wrong
            // place on earth - a bug no compiler would ever catch.
            Assert.InRange(city.Latitude, 50.0, 54.0);
            Assert.InRange(city.Longitude, 3.0, 8.0);
        });
    }

    [Theory]
    [InlineData("Amsterdam")]
    [InlineData("amsterdam")]
    [InlineData("AMSTERDAM")]
    [InlineData("aMsTeRdAm")]
    public void Find_MatchesRegardlessOfCasing(string name)
    {
        City? city = DutchCities.Find(name);

        Assert.NotNull(city);
        Assert.Equal("Amsterdam", city!.Name);
    }

    [Fact]
    public void Find_ReturnsNull_ForACityWeDoNotSupport()
    {
        // Paris is a perfectly real city, just not one of ours. The service
        // relies on this null to answer with a 404 instead of calling out to
        // the weather API for a city the app does not offer.
        Assert.Null(DutchCities.Find("Paris"));
    }
}
