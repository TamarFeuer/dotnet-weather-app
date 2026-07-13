// ============================================================================
// STARTUP - wiring (the composition root)
// ============================================================================
// Program.cs is where the app is wired up at startup. Everywhere else, classes
// depend only on interfaces; here is where we decide which concrete class
// implements each one (which service, which repository, which storage driver).
using Microsoft.EntityFrameworkCore;  // UseNpgsql, EnsureCreated
using WeatherAPI.Service;             // ITypicalService, TypicalService
using WeatherAPI.Repository;          // repository, data-source port, drivers, DbContext


// This call is also what creates the Kestrel web server — the thing that
// actually listens for network requests (you see "server: Kestrel" in the
// response headers).
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register the MVC controllers (this is what makes the endpoints reachable).
builder.Services.AddControllers();

// CORS: let the Angular dev server (a different origin) call this API. Without
// this, the browser blocks requests coming from http://localhost:4200.
const string AngularDev = "AllowAngular";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDev, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// --- The actual wiring of the architecture ---------------------------------
// "When something asks for ITypicalRepository, give it a TypicalRepository."
// "When something asks for ITypicalService, give it a TypicalService."
// ASP.NET's container then builds the whole chain for us automatically:
//   TypicalEndpoint <- ITypicalService <- ITypicalRepository
//
// AddScoped = create one instance per HTTP request (the standard choice for
// web APIs). The other classes never see these lines; they only ever know
// about the interfaces.
// Register EF Core's DbContext, pointed at PostgreSQL.
//
// The connection string is NEVER written here. It comes from configuration, which
// .NET assembles from several sources, each overriding the one above it:
//
//   1. appsettings.json                  (in git - nothing sensitive belongs here)
//   2. appsettings.Development.json      (git-ignored - your local values)
//   3. environment variables             (how CI and the cloud supply it)
//
// So the same compiled code runs against a Docker container on a laptop, a
// throwaway database in the pipeline, or a managed database in the cloud. Only
// the configuration differs, and a real password never enters the repository.
string connectionString = builder.Configuration.GetConnectionString("WeatherDb")
    ?? throw new InvalidOperationException(
        "No connection string 'WeatherDb' was found. Copy "
        + "appsettings.Development.example.json to appsettings.Development.json and fill "
        + "it in, or set the environment variable ConnectionStrings__WeatherDb.");

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- THE ONE-LINE DRIVER SWAP ---
// TypicalRepository depends only on the IMonthDataSource interface. We choose
// which driver fills it here. Comment one line, uncomment the other - nothing
// else in the app changes.
//
// Note that SqlMonthDataSource did not change one letter when the database went
// from SQLite to PostgreSQL. It only ever talks to EF Core, and EF Core is itself
// an abstraction over the database engine. Two layers of abstraction stacked: our
// port picks SQL-or-JSON, and EF Core picks which SQL engine.
// builder.Services.AddScoped<IMonthDataSource, JsonMonthDataSource>();  // file
builder.Services.AddScoped<IMonthDataSource, SqlMonthDataSource>();        // PostgreSQL
builder.Services.AddScoped<ITypicalRepository, TypicalRepository>();
builder.Services.AddScoped<ITypicalService, TypicalService>();

// --- Forecast feature (live Open-Meteo API) ---
// AddHttpClient wires up an HttpClient for OpenMeteoForecastSource AND registers
// it as the IForecastSource in one line. Swap it for a seeded driver later by
// changing only this line.
builder.Services.AddHttpClient<IForecastSource, OpenMeteoForecastSource>();
builder.Services.AddScoped<IForecastService, ForecastService>();

WebApplication app = builder.Build();

// On startup, make sure the database, the Temperatures table, and the seed
// rows all exist. EnsureCreated builds the database from WeatherDbContext if it
// isn't there yet (a simple alternative to EF "migrations" for a small app).
using (IServiceScope scope = app.Services.CreateScope())
{
    WeatherDbContext db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors(AngularDev);   // apply the CORS policy (must come before MapControllers)
app.MapControllers();   // hooks the controller routes into the request pipeline
app.Run();

