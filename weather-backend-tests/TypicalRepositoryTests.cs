using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Tests;

// A fake storage driver. TypicalRepository talks to the IMonthDataSource port,
// never to SQLite or a JSON file directly, so we can hand it a plain in-memory
// list. No database, no file, no EF Core.
public class FakeMonthDataSource(IReadOnlyList<Temperature> rows) : IMonthDataSource
{
    public IReadOnlyList<Temperature> GetAll() => rows;
}

public class TypicalRepositoryTests
{
    private static readonly IReadOnlyList<Temperature> Rows =
    [
        new Temperature { Id = 1, Month = "January", MinTemp = 1, MaxTemp = 6 },
        new Temperature { Id = 7, Month = "July", MinTemp = 14, MaxTemp = 23 },
    ];

    private static TypicalRepository Repository() =>
        new(new FakeMonthDataSource(Rows));

    [Theory]
    [InlineData("July")]
    [InlineData("july")]
    [InlineData("JULY")]
    [InlineData("jUlY")]
    public void GetByMonth_MatchesRegardlessOfCasing(string month)
    {
        // The month arrives straight from a query string, so whatever the caller
        // typed is what we get. Casing must not decide whether we find the row.
        Temperature? row = Repository().GetByMonth(month);

        Assert.NotNull(row);
        Assert.Equal("July", row!.Month);
        Assert.Equal(14, row.MinTemp);
        Assert.Equal(23, row.MaxTemp);
    }

    [Fact]
    public void GetByMonth_ReturnsNull_WhenTheMonthIsNotInTheData()
    {
        Assert.Null(Repository().GetByMonth("Smarch"));
    }

    [Fact]
    public void GetByMonth_ReturnsNull_WhenTheStorageIsEmpty()
    {
        var repository = new TypicalRepository(new FakeMonthDataSource([]));

        Assert.Null(repository.GetByMonth("July"));
    }
}
