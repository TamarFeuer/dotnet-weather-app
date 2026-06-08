// ============================================================================
// THE COMPOSITION ROOT
// ============================================================================
// Program.cs is the one special place in Clean Architecture where the
// Dependency Rule is deliberately relaxed. Everywhere else, classes depend
// only on the INTERFACES from the inner layers. But SOMEONE has to decide
// which concrete class implements each interface — and that decision is made
// here, at the outermost edge of the app, when it starts up.
//
// That is why this file is allowed to reference every layer at once.
using Microsoft.EntityFrameworkCore;  // UseSqlite, EnsureCreated
using WeatherAPI.UseCases;          // interfaces + WeatherService (Layer 2)
using WeatherAPI.InterfaceAdapters; // controller, gateway, presenter, DTO (Layer 3)
using WeatherAPI.Frameworks;        // data-source drivers + DbContext (Layer 4)


// This call is also what creates the Kestrel web server — the thing that
// actually listens for network requests (you see "server: Kestrel" in the
// response headers).
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register the MVC controllers (this is what makes WeatherEndpoint reachable).
builder.Services.AddControllers();

// Swagger? is an interactive web page (at /swagger) for testing the API from the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- The actual wiring of the architecture ---------------------------------
// "When something asks for IWeatherRepository, give it a WeatherRepository."
// "When something asks for IWeatherService, give it a WeatherService."
// ASP.NET's container then builds the whole chain for us automatically:
//   WeatherEndpoint <- IWeatherService <- IWeatherRepository
//
// AddScoped = create one instance per HTTP request (the standard choice for
// web APIs). The inner layers never see these lines — they only ever know
// about the interfaces.
// Register EF Core's DbContext, pointed at a local SQLite file (weather.db).
// This is the "database engine" wiring — pure Frameworks-layer concern.
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite("Data Source=weather.db"));

// --- THE ONE-LINE DRIVER SWAP ---
// The gateway depends only on the ISeasonDataSource PORT. We choose which
// DRIVER fills it here. Comment one line, uncomment the other — nothing else
// in the app changes.
// builder.Services.AddScoped<ISeasonDataSource, JsonSeasonDataSource>();  // file
builder.Services.AddScoped<ISeasonDataSource, SqlSeasonDataSource>();        // SQLite
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

WebApplication app = builder.Build();

// On startup, make sure the SQLite file, the Temperatures table, and the seed
// rows all exist. EnsureCreated builds the database from WeatherDbContext if it
// isn't there yet (a simple alternative to EF "migrations" for a small app).
using (IServiceScope scope = app.Services.CreateScope())
{
    WeatherDbContext db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
}

// Note we only turn the UI on during development. You usually don't expose
// that test page to the public in production — it's a developer tool.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();     // serves the swagger.json document
    app.UseSwaggerUI();   // serves the interactive web page built from it
}

app.MapControllers();   // hooks the controller routes into the request pipeline
app.Run();

