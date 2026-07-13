# WeatherAPI

A small full-stack project built to learn layered backend architecture (ASP.NET
Core) and reactive frontend state management (Angular + NgRx). It has two
features:

- **Typical weather** for a month: a temperature range, a typical (average)
  value and a short description, read from a JSON file or a PostgreSQL database
  (swappable with one line).
- **5-day forecast** for a Dutch city: live data fetched from the free
  [Open-Meteo](https://open-meteo.com/) API, behind the same swappable-driver
  pattern as the month feature.

```
GET /api/weather/typical?month=January
->  { "minTemp": 1, "maxTemp": 6, "average": 3, "description": "Freezing" }

GET /api/weather/cities
->  ["Amsterdam", "Rotterdam", "The Hague", ...]

GET /api/weather/forecast?city=Amsterdam
->  [ { "date": "2026-07-03", "minTemp": 13, "maxTemp": 21, "condition": "Cloudy" }, ... ]
```

## Contents

- [The ideas behind it](#the-ideas-behind-it)
- [Repository layout](#repository-layout)
- [Frontend](#frontend)
- [Backend structure](#backend-structure)
- [How a forecast request flows](#how-a-forecast-request-flows)
- [Swapping storage (JSON or PostgreSQL)](#swapping-storage-json-or-postgresql)
- [Running it](#running-it)
- [Continuous integration](#continuous-integration)
- [Inspecting the database](#inspecting-the-database)
- [Notes](#notes)

## The ideas behind it

Two patterns shape this project.

**Clean Architecture** - dependencies point inward: outer concerns (web,
database) depend on inner ones (business rules), never the reverse. The backend
uses a simplified take on it: a request flows through three layers (Controller →
Service → Repository), which pass around plain data models (Temperature, City,
ForecastDay, TypicalInfo). Those models are not a fourth step but the data
itself - the innermost ring, the "Entities" at the center of the classic model
below:

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

- **`typical`** - this month's typical weather.
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
TypicalEndpoint.cs, ForecastEndpoint.cs; Service/ (business logic):
TypicalService.cs, ITypicalService.cs, TypicalInfo.cs, ForecastService.cs,
IForecastService.cs; Repository/ (data access): TypicalRepository.cs,
ITypicalRepository.cs, IMonthDataSource.cs, WeatherDbContext.cs,
SqlMonthDataSource.cs, JsonMonthDataSource.cs, months.json, IForecastSource.cs,
OpenMeteoForecastSource.cs, DutchCities.cs; Models/ (data models):
Temperature.cs, City.cs, ForecastDay.cs](docs/backend_structure.png)

Both features use the same shape: the controller calls a service, the service
calls something behind an interface (a "port"), and a concrete driver fills that
port. The month feature's port is `IMonthDataSource` (filled by PostgreSQL or JSON);
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

## Swapping storage (JSON or PostgreSQL)

The month feature's two drivers sit behind the same `IMonthDataSource`
interface. Pick one in `Program.cs` - comment one line, uncomment the other:

```csharp
// builder.Services.AddScoped<IMonthDataSource, JsonMonthDataSource>();  // file
builder.Services.AddScoped<IMonthDataSource, SqlMonthDataSource>();      // database (default)
```

Nothing else in the app changes - the controller, service, and repository never
know which storage is behind the interface.

- **JSON driver** reads `Repository/months.json`.
- **SQL driver** uses EF Core against PostgreSQL. The table is created and seeded
  on startup by `EnsureCreated()`.

Note that the abstraction is two layers deep. `IMonthDataSource` decides *SQL or
JSON*; EF Core then decides *which SQL engine*, in one line of `Program.cs`:

```csharp
options.UseNpgsql(connectionString)   // was UseSqlite("Data Source=weather.db")
```

The project ran on SQLite before this, and `SqlMonthDataSource`, the repository,
the service and the controllers did not change by a single character. That is
also why the class is called `SqlMonthDataSource` and not `SqliteMonthDataSource`.

The forecast feature uses the same idea: `OpenMeteoForecastSource` sits behind
`IForecastSource`, wired in `Program.cs` with `AddHttpClient`. Swap it for a
seeded driver by changing that one line.

## Running it

**1. Start the database.** PostgreSQL runs in a Docker container:

```bash
docker run --name weather-postgres \
  -e POSTGRES_USER=weather \
  -e POSTGRES_PASSWORD=weather \
  -e POSTGRES_DB=weatherapi \
  -p 5432:5432 \
  -d postgres:17
```

**2. Configure the connection string.** It is deliberately not in the repository:

```bash
cd weather-backend
cp appsettings.Development.example.json appsettings.Development.json
# then fill in the password you used above
```

`appsettings.Development.json` is git-ignored, so a real password never gets
committed. In CI and in the cloud the value comes from the environment variable
`ConnectionStrings__WeatherDb` instead, which overrides the file.

**3. Run the backend**, from `weather-backend/`:

```bash
dotnet run
```

Then test it in your browser (the forecast needs an internet connection, since
it calls the live Open-Meteo API):

- `http://localhost:5151/api/weather/typical?month=July`
- `http://localhost:5151/api/weather/cities`
- `http://localhost:5151/api/weather/forecast?city=Amsterdam`

Frontend, from `weather-frontend/`:

```bash
cd weather-frontend
npm start            # Angular dev server on http://localhost:4200
```

Press `Ctrl+C` to stop either one.

## Continuous integration

Every push to `main` triggers a pipeline in Azure DevOps, defined as code in
[`azure-pipelines.yml`](azure-pipelines.yml). It spins up a fresh
Microsoft-hosted Ubuntu agent, then runs two jobs, one per half of the project,
so a broken frontend cannot hide a broken backend:

- **Backend**: build, then 48 xUnit tests.
- **Frontend**: `npm ci`, build, then 47 tests.

Both publish their results and their coverage, so a run shows every test by name
and, on a failure, the exact assertion that broke.

The tests earn their keep: they caught a bug where the coordinates were formatted
using the machine's locale, so a Dutch machine would have sent `52,37` instead of
`52.37` and Open-Meteo would have rejected it. The app worked fine on the build
agent, which runs an English locale. No amount of running the app would have found
that.

This is a DevSecOps setup that is still being built out. Still to come: quality
gates (a pull request may only merge if the pipeline is green) and security
scanning that blocks rather than warns.

## Inspecting the database

When the SQL driver is active, you can look inside PostgreSQL with `psql`, which
ships inside the container, so nothing needs installing:

```bash
docker exec -it weather-postgres psql -U weather -d weatherapi -c 'SELECT * FROM "Temperatures";'
```

The double quotes around `"Temperatures"` are not optional: PostgreSQL folds
unquoted identifiers to lower case, and EF Core created the table with a capital
T.

The forecast feature has no local database (its data is live), but you can see
the raw data the frontend receives by opening the endpoint directly while the
backend is running:

```
http://localhost:5151/api/weather/forecast?city=Amsterdam
```

## Notes

- The database runs in Docker and is created and seeded on startup by
  `EnsureCreated()`, from the seed data in `WeatherDbContext`. Nothing about it
  is stored in the repository.
- No password lives in git. `appsettings.Development.json` (your local values) is
  git-ignored; `appsettings.Development.example.json` (a template) is committed.
  CI and the cloud supply the value through `ConnectionStrings__WeatherDb`.
- The forecast feature calls Open-Meteo (free, no API key). The list of
  supported cities and their coordinates lives in `Repository/DutchCities.cs`.
- Backend targets .NET 10, storage via `Npgsql.EntityFrameworkCore.PostgreSQL`.
  Frontend uses Angular with `@ngrx/store`, `@ngrx/effects` and
  `@ngrx/store-devtools`.
