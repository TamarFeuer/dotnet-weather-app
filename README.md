# WeatherAPI

A small ASP.NET Core Web API built to learn **Clean Architecture**. It returns a
random temperature for the current season, and can read its data from either a
JSON file or a SQLite database - swappable with one line.

```
GET /api/weather/temperature  ->  { "temperature": 21 }
```

## Project structure

Four folders, organized by responsibility, with the request flowing
Controller → Service → Repository (Models is the shared data type):

![WeatherAPI project structure - Controller/ (API, HTTP endpoint):
WeatherEndpoint.cs; Service/ (business logic): WeatherService.cs,
IWeatherService.cs; Repository/ (data access): WeatherRepository.cs,
IWeatherRepository.cs, ISeasonDataSource.cs, WeatherDbContext.cs,
SqlSeasonDataSource.cs, JsonSeasonDataSource.cs, seasons.json; Models/
(data model): Temperature.cs](docs/clean_architecture.png)

## How a request flows

**Stage 1 - startup (the choice):**
`Program.cs` registers one driver for `ISeasonDataSource` (currently
`SqlSeasonDataSource` = SQLite). The choice is locked in once, before any
request arrives.

**Stage 2 - the request (`GET /api/weather/temperature`):**

1. Browser sends `GET /api/weather/temperature`.
2. Kestrel receives it; routing matches `WeatherEndpoint`.
3. DI builds the chain; for `ISeasonDataSource` it builds the chosen driver
   (`SqlSeasonDataSource` + `WeatherDbContext`).
4. Controller calls `_service.GetTemperature()`.
5. `WeatherService` works out the season (June → Summer).
6. It calls `_repository.GetBySeason("Summer")`.
7. `WeatherRepository` calls `_dataSource.GetAll()` (the chosen driver).
8. `SqlSeasonDataSource` → EF Core → `SELECT * FROM Temperatures` →
   `weather.db` → `Temperature` rows.
9. `WeatherRepository` picks the Summer row (17-25); `WeatherService` rolls a
   random value inside it and returns the `int`.
10. Controller wraps it as `{ "temperature": N }`; Kestrel sends the JSON back.

Quick view of the call chain:

```
Browser → Kestrel → WeatherEndpoint → WeatherService →
WeatherRepository → SqlSeasonDataSource → weather.db
```

```
Kestrel             = web server
WeatherEndpoint     = controller
WeatherService      = service (business logic)
WeatherRepository   = repository (data access)
SqlSeasonDataSource = chosen driver
weather.db          = database
```

## Swapping storage (JSON ↔ SQLite)

Both drivers sit behind the same `ISeasonDataSource` port. Pick one in
`Program.cs` - comment one line, uncomment the other:

```csharp
// builder.Services.AddScoped<ISeasonDataSource, JsonSeasonDataSource>();  // file
builder.Services.AddScoped<ISeasonDataSource, SqlSeasonDataSource>();      // SQLite (default)
```

Nothing else in the app changes - the controller, service, and repository
never know which storage is behind the port.

- **JSON driver** reads `Repository/seasons.json`.
- **SQLite driver** uses EF Core; the database file `weather.db` is created and
  seeded automatically on startup by `EnsureCreated()`.

## Running it

```bash
dotnet run
```

Then open:

- `http://localhost:5151/swagger` - interactive test page
- `http://localhost:5151/api/weather/temperature` - the raw JSON

Press `Ctrl+C` to stop.

## Inspecting the database

When the SQLite driver is active, you can look inside `weather.db` directly with
the `sqlite3` CLI (install it with `sudo apt install sqlite3`):

```bash
echo "=== SELECT * FROM Temperatures ==="; sqlite3 -header -column weather.db "SELECT * FROM Temperatures;"
```

## Notes

- `weather.db` is generated at runtime (and git-ignored) - it rebuilds itself
  from the seed data in `WeatherDbContext`.
- Targets .NET 10. Storage via `Microsoft.EntityFrameworkCore.Sqlite`.
