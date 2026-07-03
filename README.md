# WeatherAPI

A small full-stack project built to learn layered backend architecture (ASP.NET
Core) and reactive frontend state management (Angular + NgRx). It has two
features:

- **Typical weather** for a month: a temperature range, a typical (average)
  value and a short description, read from a JSON file or a SQLite database
  (swappable with one line).
- **5-day forecast** for a Dutch city: live data fetched from the free
  [Open-Meteo](https://open-meteo.com/) API, behind the same swappable-driver
  pattern as the month feature.

```
GET /api/weather/temperature?month=January
->  { "minTemp": 1, "maxTemp": 6, "average": 3, "description": "Freezing" }

GET /api/weather/cities
->  ["Amsterdam", "Rotterdam", "The Hague", ...]

GET /api/weather/forecast?city=Amsterdam
->  [ { "date": "2026-07-03", "minTemp": 13, "maxTemp": 21, "condition": "Cloudy" }, ... ]
```

## The ideas behind it

Two patterns shape this project.

**Clean Architecture** - dependencies point inward: outer concerns (web,
database) depend on inner ones (business rules), never the reverse. The backend
uses a simplified, three-layer take on it (Controller → Service → Repository),
but the guiding idea is the classic model:

![Clean Architecture: concentric rings - Entities, Use Cases, Interface
Adapters, Frameworks & Drivers - with dependencies pointing
inward](docs/clean_architecture.png)

*Source: Robert C. Martin, "The Clean Architecture"
(blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)*

**One-directional data flow (NgRx)** - the frontend never mutates shared state
directly. A component dispatches an action, an effect does any async work, a
reducer produces the new state, and selectors feed it back to the components:
one loop, one direction.

![NgRx lifecycle: component dispatches an action, the reducer updates the store,
selectors feed the store back to the component; effects handle async work by
calling a service and dispatching a new action](docs/ngrx_lifecycle.png)

## Repository layout

This repo holds two apps side by side:

- `weather-backend/` - the ASP.NET Core API (this README describes it; all
  backend paths below are relative to this folder)
- `weather-frontend/` - the Angular app: this month's typical weather plus a
  city forecast grid, updating live with no page refresh

## Frontend

`weather-frontend/` is an Angular app. State lives in an **NgRx** store (the
Redux pattern: actions, reducers, selectors, effects) - the choice the project
made between the two options it considered, Signals and NgRx.

The store has two slices, one per feature:

- **`weather`** - this month's typical weather.
- **`forecast`** - the city list and the selected city's 5-day forecast.

The components each talk to the store on their own; the root `App` is just a
shell that hosts them:

- **`TypicalWeather`** - on load, dispatches the current month automatically (no
  dropdown), keeping the month feature in use. Shows the result via
  `TemperatureDisplay`.
- **`TemperatureDisplay`** - reads the month's info from the store and shows the
  range, average and description.
- **`CityPicker`** - a dropdown whose options come from the backend: on load it
  dispatches an action to fetch the city list, and on change it dispatches the
  chosen city.
- **`ForecastGrid`** - a container: reads the forecast slice and renders one
  **`DayCard`** per day.
- **`DayCard`** - presentational: holds no store logic; one day flows in through
  an input and it just renders it.

## Backend structure

Inside `weather-backend/`, four folders organized by responsibility, with each
request flowing Controller → Service → Repository (Models holds the shared data
types):

![WeatherAPI project structure - Controller/ (API, HTTP endpoints):
WeatherEndpoint.cs, ForecastEndpoint.cs; Service/ (business logic):
WeatherService.cs, IWeatherService.cs, WeatherInfo.cs, ForecastService.cs,
IForecastService.cs; Repository/ (data access): WeatherRepository.cs,
IWeatherRepository.cs, IMonthDataSource.cs, WeatherDbContext.cs,
SqlMonthDataSource.cs, JsonMonthDataSource.cs, months.json, IForecastSource.cs,
OpenMeteoForecastSource.cs, DutchCities.cs; Models/ (data models):
Temperature.cs, City.cs, ForecastDay.cs](docs/backend_structure.png)

Both features use the same shape: the controller calls a service, the service
calls something behind an interface (a "port"), and a concrete driver fills that
port. The month feature's port is `IMonthDataSource` (filled by SQLite or JSON);
the forecast feature's port is `IForecastSource` (filled by the Open-Meteo
driver). Swapping a driver is a one-line change in `Program.cs` and nothing else
in the app knows.

## How a forecast request flows

The full path of one request, from picking a city to the live API and back,
through the NgRx lifecycle (action → effect → reducer → selector):

```
1.  User picks a city in the dropdown
2.  <select>'s (change) fires → CityPicker dispatches citySelected("Amsterdam")
3.  The reducer sets loading: true on the forecast slice
4.  The loadForecast$ effect hears citySelected and calls the frontend
       ForecastService.getForecast("Amsterdam") → an Observable
    ----------------- crosses to the backend -----------------
5.  Kestrel receives GET /api/weather/forecast?city=Amsterdam
       → routing → ForecastEndpoint
6.  Controller calls _service.GetForecastAsync("Amsterdam")
7.  ForecastService validates the city via DutchCities.Find (404 if unknown),
       then calls _source.GetForecastAsync(city)
8.  IForecastSource → OpenMeteoForecastSource builds the Open-Meteo URL from the
       city's coordinates and awaits the live HTTP call
9.  The driver adapts Open-Meteo's JSON (parallel arrays + WMO weather codes)
       into a list of our own ForecastDay objects
10. Controller returns Ok(days) → JSON array of { date, minTemp, maxTemp, condition }
    ----------------- response crosses back -----------------
11. The effect's Observable emits the days; the effect dispatches forecastLoaded
12. The reducer writes the days into the forecast slice and sets loading: false
13. ForecastGrid's selectors (fed as signals) change → Angular re-renders
14. The grid renders one DayCard per day (no page refresh)
```

The month feature follows the same lifecycle: `TypicalWeather` dispatches
`monthSelected` on load, the effect calls the backend, and `TemperatureDisplay`
reads the result from the store.

## Swapping storage (JSON ↔ SQLite)

The month feature's two drivers sit behind the same `IMonthDataSource`
interface. Pick one in `Program.cs` - comment one line, uncomment the other:

```csharp
// builder.Services.AddScoped<IMonthDataSource, JsonMonthDataSource>();  // file
builder.Services.AddScoped<IMonthDataSource, SqlMonthDataSource>();      // SQLite (default)
```

Nothing else in the app changes - the controller, service, and repository never
know which storage is behind the interface.

- **JSON driver** reads `Repository/months.json`.
- **SQLite driver** uses EF Core; the database file `weather.db` is created and
  seeded automatically on startup by `EnsureCreated()`.

The forecast feature uses the same idea: `OpenMeteoForecastSource` sits behind
`IForecastSource`, wired in `Program.cs` with `AddHttpClient`. Swap it for a
seeded driver by changing that one line.

## Running it

Backend, from `weather-backend/`:

```bash
cd weather-backend
dotnet run
```

Then test it in your browser (the forecast needs an internet connection, since
it calls the live Open-Meteo API):

- `http://localhost:5151/api/weather/temperature?month=July`
- `http://localhost:5151/api/weather/cities`
- `http://localhost:5151/api/weather/forecast?city=Amsterdam`

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

The forecast feature has no local database (its data is live), but you can see
the raw data the frontend receives by opening the endpoint directly while the
backend is running:

```
http://localhost:5151/api/weather/forecast?city=Amsterdam
```

## Notes

- `weather.db` is generated at runtime (and git-ignored) - it rebuilds itself
  from the seed data in `WeatherDbContext`.
- The forecast feature calls Open-Meteo (free, no API key). The list of
  supported cities and their coordinates lives in `Repository/DutchCities.cs`.
- Backend targets .NET 10, storage via `Microsoft.EntityFrameworkCore.Sqlite`.
  Frontend uses Angular with `@ngrx/store`, `@ngrx/effects` and
  `@ngrx/store-devtools`.
