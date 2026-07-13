using WeatherAPI.Models;
using WeatherAPI.Repository;
using WeatherAPI.Service;

namespace WeatherAPI.Tests;

// A fake repository. TypicalService only ever sees the ITypicalRepository
// interface, so we can hand it this stub instead of the real one. No database, no
// EF Core, no file on disk: the test stays fast, offline and deterministic.
//
// This is the payoff of the port/driver design. The service is testable BECAUSE
// its data source can be swapped out.
public class FakeTypicalRepository(Temperature? row) : ITypicalRepository
{
    public Temperature? GetByMonth(string month) => row;
}

public class TypicalServiceTests
{
    // Helper: run the service against one fake row with the given range.
    private static TypicalInfo? InfoFor(int minTemp, int maxTemp)
    {
        var repository = new FakeTypicalRepository(new Temperature
        {
            Id = 1,
            Month = "TestMonth",
            MinTemp = minTemp,
            MaxTemp = maxTemp,
        });

        return new TypicalService(repository).GetTypical("TestMonth");
    }

    [Fact]
    public void GetTypical_ReturnsNull_WhenTheMonthIsUnknown()
    {
        // The repository finds no row, so the service reports "unknown month".
        // The controller turns that null into a 404.
        var service = new TypicalService(new FakeTypicalRepository(null));

        Assert.Null(service.GetTypical("Smarch"));
    }

    [Fact]
    public void GetTypical_ComputesTheAverageOfTheRange()
    {
        TypicalInfo? info = InfoFor(minTemp: 14, maxTemp: 23);

        Assert.NotNull(info);
        Assert.Equal(14, info!.MinTemp);
        Assert.Equal(23, info.MaxTemp);
        Assert.Equal(18, info.Average); // (14 + 23) / 2 = 18, integer division
    }

    // The business rule is a set of thresholds (5, 11, 16), and thresholds are
    // exactly where bugs hide: is an average of 5 still "Freezing", or already
    // "Cold"? Each row below checks one side of a boundary.
    //
    // [Theory] with [InlineData] runs this single test once per row, instead of
    // six nearly identical [Fact] methods.
    [Theory]
    [InlineData(0, 10, "Freezing")]  // average 5  -> still Freezing (rule is <= 5)
    [InlineData(2, 10, "Cold")]      // average 6  -> Cold
    [InlineData(6, 16, "Cold")]      // average 11 -> still Cold (rule is <= 11)
    [InlineData(6, 18, "Mild")]      // average 12 -> Mild
    [InlineData(12, 20, "Mild")]     // average 16 -> still Mild (rule is <= 16)
    [InlineData(13, 21, "Warm")]     // average 17 -> Warm
    public void GetTypical_DescribesTheAverage(int minTemp, int maxTemp, string expected)
    {
        TypicalInfo? info = InfoFor(minTemp, maxTemp);

        Assert.NotNull(info);
        Assert.Equal(expected, info!.Description);
    }
}
