# WeatherAPI

A small ASP.NET Core Web API built to learn **Clean Architecture**. It returns a
random temperature for a chosen month, and can read its data from either a JSON
file or a SQLite database - swappable with one line.

```
GET /api/weather/temperature?month=January  ->  { "temperature": 4 }
```

## Repository layout

This repo holds two apps side by side:

- `weather-backend/` - the ASP.NET Core API (this README describes it; all
  backend paths below are relative to this folder)
- `weather-frontend/` - the Angular app (work in progress)

## Backend structure

Inside `weather-backend/`, four folders organized by responsibility, with the
request flowing Controller → Service → Repository (Models is the shared data
type):

![WeatherAPI project structure - Controller/ (API, HTTP endpoint):
WeatherEndpoint.cs; Service/ (business logic): WeatherService.cs,
IWeatherService.cs; Repository/ (data access): WeatherRepository.cs,
IWeatherRepository.cs, IMonthDataSource.cs, WeatherDbContext.cs,
SqlMonthDataSource.cs, JsonMonthDataSource.cs, months.json; Models/
(data model): Temperature.cs](docs/clean_architecture.png)

## How a request flows

**Stage 1 - startup (the choice):**
`Program.cs` registers one driver for `IMonthDataSource` (currently
`SqlMonthDataSource` = SQLite). The choice is locked in once, before any
request arrives.

**Stage 2 - the request (`GET /api/weather/temperature?month=July`):**

1. Browser sends `GET /api/weather/temperature?month=July`.
2. Kestrel receives it; routing matches `WeatherEndpoint`.
3. DI builds the chain; for `IMonthDataSource` it builds the chosen driver
   (`SqlMonthDataSource` + `WeatherDbContext`).
4. Controller validates the month and calls `_service.GetTemperature("July")`.
5. `WeatherService` calls `_repository.GetByMonth("July")`.
6. `WeatherRepository` calls `_dataSource.GetAll()` (the chosen driver).
7. `SqlMonthDataSource` → EF Core → `SELECT * FROM Temperatures` →
   `weather.db` → `Temperature` rows.
8. `WeatherRepository` picks the July row (14-23); `WeatherService` rolls a
   random value inside it and returns the `int`.
9. Controller wraps it as `{ "temperature": N }`; Kestrel sends the JSON back.

Quick view of the call chain:

```
Browser → Kestrel → WeatherEndpoint → WeatherService →
WeatherRepository → SqlMonthDataSource → weather.db
```

```
Kestrel            = web server
WeatherEndpoint    = controller
WeatherService     = service (business logic)
WeatherRepository  = repository (data access)
SqlMonthDataSource = chosen driver
weather.db         = database
```

## Swapping storage (JSON ↔ SQLite)

Both drivers sit behind the same `IMonthDataSource` interface. Pick one in
`Program.cs` - comment one line, uncomment the other:

```csharp
// builder.Services.AddScoped<IMonthDataSource, JsonMonthDataSource>();  // file
builder.Services.AddScoped<IMonthDataSource, SqlMonthDataSource>();      // SQLite (default)
```

Nothing else in the app changes - the controller, service, and repository
never know which storage is behind the interface.

- **JSON driver** reads `Repository/months.json`.
- **SQLite driver** uses EF Core; the database file `weather.db` is created and
  seeded automatically on startup by `EnsureCreated()`.

## Running it

Backend, from `weather-backend/`:

```bash
cd weather-backend
dotnet run
```

Then test it in your browser:

- `http://localhost:5151/api/weather/temperature?month=July`

Frontend, from `weather-frontend/`:

```bash
cd weather-frontend
npm start            # Angular dev server on http://localhost:4200
```

Press `Ctrl+C` to stop either one.

## Inspecting the database

When the SQLite driver is active, you can look inside `weather.db` directly with
the `sqlite3` CLI (install it with `sudo apt install sqlite3`):

```bash
cd weather-backend
echo "=== SELECT * FROM Temperatures ==="; sqlite3 -header -column weather.db "SELECT * FROM Temperatures;"
```

## Notes

- `weather.db` is generated at runtime (and git-ignored) - it rebuilds itself
  from the seed data in `WeatherDbContext`.
- Targets .NET 10. Storage via `Microsoft.EntityFrameworkCore.Sqlite`.
