# WeatherAPI

A small ASP.NET Core Web API built to learn **Clean Architecture**. For a chosen
month it returns the usual temperature range plus a typical (average) value and
a short description, and can read its data from either a JSON file or a SQLite
database - swappable with one line.

```
GET /api/weather/temperature?month=January
->  { "minTemp": 1, "maxTemp": 6, "average": 3, "description": "Freezing" }
```

## Repository layout

This repo holds two apps side by side:

- `weather-backend/` - the ASP.NET Core API (this README describes it; all
  backend paths below are relative to this folder)
- `weather-frontend/` - the Angular app: a month dropdown that shows the weather live

## Frontend

`weather-frontend/` is an Angular app: a month dropdown that calls the API and
shows the result live (no page refresh). It's split into two components -
`MonthPicker` (the dropdown; emits the chosen month) and `TemperatureDisplay`
(shows the range, average and description) - coordinated by the root `App`.

State is managed with **Angular Signals** - a deliberate choice between the two
options the project considered, Signals and NgRx.

## Backend structure

Inside `weather-backend/`, four folders organized by responsibility, with the
request flowing Controller → Service → Repository (Models is the shared data
type):

![WeatherAPI project structure - Controller/ (API, HTTP endpoint):
WeatherEndpoint.cs; Service/ (business logic): WeatherService.cs,
IWeatherService.cs, WeatherInfo.cs; Repository/ (data access): WeatherRepository.cs,
IWeatherRepository.cs, IMonthDataSource.cs, WeatherDbContext.cs,
SqlMonthDataSource.cs, JsonMonthDataSource.cs, months.json; Models/
(data model): Temperature.cs](docs/clean_architecture.png)

## How a request flows

The full path of one request, from the dropdown to the database and back. (The
storage driver - SQLite or the JSON file - is chosen once at startup in
`Program.cs`; see "Swapping storage" below.)

```
1.  User picks a month in the dropdown
2.  <select>'s (change) fires
3.  MonthPicker runs monthSelected.emit("July")
4.  App's binding (monthSelected)="onMonthSelected($event)"
       → Angular calls App.onMonthSelected("July")
5.  onMonthSelected calls this.weather.getWeather("July")   (frontend service)
6.  getWeather calls http.get<WeatherInfo>(apiUrl, {params:{month:"July"}})
       → returns an Observable
7.  .subscribe(...) runs → the HTTP request actually fires
    ----------------- crosses to the backend -----------------
8.  Kestrel receives GET /api/weather/temperature?month=July
       → routing → WeatherEndpoint
9.  Controller calls _service.GetWeather("July")            (backend C# service)
10. Service calls _repository.GetByMonth("July")
11. Repository → IMonthDataSource → SqlMonthDataSource → EF Core
       → SELECT FROM Temperatures → weather.db
12. Repository returns the July Temperature row
13. Service computes average + description → returns a WeatherInfo
14. Controller returns Ok(info) → JSON { minTemp, maxTemp, average, description }
    ----------------- response crosses back -----------------
15. The frontend Observable emits that response object
16. The subscribe callback runs: this.currentWeather.set(response)
       → App's currentWeather signal now holds the WeatherInfo
17. currentWeather changed → Angular re-renders → [info]="currentWeather()"
       passes the value into TemperatureDisplay's input
18. TemperatureDisplay's @if shows the range, average, description  (no F5)
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
