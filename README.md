# WeatherAPI

A small ASP.NET Core Web API built to learn **Clean Architecture**. It returns a
random temperature for the current season, and can read its data from either a
JSON file or a SQLite database - swappable with one line.

```
GET /api/weather/temperature  ->  { "temperature": 21 }
```

## The four layers

Each folder is one ring of the Clean Architecture diagram (🟡 → 🔴 → 🟢 → 🔵),
with dependencies pointing **inward** only.

![WeatherAPI Clean Architecture layers - Models (Enterprise Business Rules):
Temperature.cs; UseCases (Application Business Rules): Ports/IWeatherService.cs,
Ports/IWeatherRepository.cs, Interactors/WeatherService.cs; InterfaceAdapters
(Interface Adapters): Controllers/WeatherEndpoint.cs, Gateways/WeatherRepository.cs,
Ports/ISeasonDataSource.cs; Frameworks (Frameworks & Drivers): Sql/WeatherDbContext.cs,
Sql/SqlSeasonDataSource.cs, Json/JsonSeasonDataSource.cs, Json/seasons.json](docs/architecture-layers.png)

Within each layer, **promises (interfaces) live in `Ports/`** and **doers
(classes) live in role folders** (`Interactors/`, `Controllers/`, `Gateways/`).

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
9. Gateway picks the Summer row (17-25); `WeatherService` rolls a random value
   inside it and returns the `int`.
10. Controller wraps it as `{ "temperature": N }`; Kestrel sends the JSON back.

Quick view of the call chain:

```
Browser → Kestrel → WeatherEndpoint → WeatherService →
WeatherRepository → SqlSeasonDataSource → weather.db
```

```
Kestrel             = web server
WeatherEndpoint     = controller
WeatherService      = interactor
WeatherRepository   = gateway
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

Nothing else in the app changes - the controller, use case, and gateway never
know which storage is behind the port.

- **JSON driver** reads `Frameworks/Json/seasons.json`.
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
