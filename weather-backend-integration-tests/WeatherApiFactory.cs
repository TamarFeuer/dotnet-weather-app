using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace WeatherAPI.IntegrationTests;

// This is the "Test Container" from the whiteboard diagram, in code.
//
// It does two things, in order:
//   1. Starts a throwaway PostgreSQL container (Testcontainers). Nothing is
//      installed or left behind: the container is created for the test run and
//      destroyed after, just like a CI agent.
//   2. Boots the REAL app in-process (WebApplicationFactory<Program>), pointed at
//      that container. Program.cs then runs Migrate() against it, so the schema
//      and the twelve seeded months are created exactly as in production.
//
// The connection string is handed to the app through the environment variable
// ConnectionStrings__WeatherDb - the SAME way CI and the cloud supply it. So the
// test does not reach inside the app to rewire anything; it configures it from
// the outside, exactly like a real deployment would.
public class WeatherApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	// The image is passed to the constructor, not via .WithImage(): the
	// parameterless constructor is obsolete in current Testcontainers, which now
	// wants the image to be explicit. Pinned to the same major as the dev
	// container.
	private readonly PostgreSqlContainer _database =
		new PostgreSqlBuilder("postgres:17").Build();

	// xUnit calls this once, before any test in the class runs.
	public async Task InitializeAsync()
	{
		await _database.StartAsync();

		// The app reads this on startup, so it connects to the container instead
		// of any local database.
		Environment.SetEnvironmentVariable(
			"ConnectionStrings__WeatherDb", _database.GetConnectionString());
	}

	// xUnit calls this once, after all tests in the class have run. Explicit
	// interface implementation avoids clashing with WebApplicationFactory's own
	// DisposeAsync, which has a different return type.
	async Task IAsyncLifetime.DisposeAsync()
	{
		await _database.DisposeAsync();   // stop and delete the container
		await base.DisposeAsync();        // shut the app down
	}
}
