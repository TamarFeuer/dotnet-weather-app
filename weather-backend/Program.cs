// ============================================================================
// STARTUP - wiring (the composition root)
// ============================================================================
// Program.cs is where the app is wired up at startup. Everywhere else, classes
// depend only on interfaces; here is where we decide which concrete class
// implements each one (which service, which repository, which storage driver).
using Microsoft.EntityFrameworkCore;  // UseSqlite, EnsureCreated
using WeatherAPI.Service;             // IWeatherService, WeatherService
using WeatherAPI.Repository;          // repository, data-source port, drivers, DbContext


// This call is also what creates the Kestrel web server — the thing that
// actually listens for network requests (you see "server: Kestrel" in the
// response headers).
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register the MVC controllers (this is what makes WeatherEndpoint reachable).
builder.Services.AddControllers();

// Swagger? is an interactive web page (at /swagger) for testing the API from the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
// "When something asks for IWeatherRepository, give it a WeatherRepository."
// "When something asks for IWeatherService, give it a WeatherService."
// ASP.NET's container then builds the whole chain for us automatically:
//   WeatherEndpoint <- IWeatherService <- IWeatherRepository
//
// AddScoped = create one instance per HTTP request (the standard choice for
// web APIs). The other classes never see these lines; they only ever know
// about the interfaces.
// Register EF Core's DbContext, pointed at a local SQLite file (weather.db).
// This is the database engine wiring.
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite("Data Source=weather.db"));

// --- THE ONE-LINE DRIVER SWAP ---
// WeatherRepository depends only on the IMonthDataSource interface. We choose
// which driver fills it here. Comment one line, uncomment the other - nothing
// else in the app changes.
// builder.Services.AddScoped<IMonthDataSource, JsonMonthDataSource>();  // file
builder.Services.AddScoped<IMonthDataSource, SqlMonthDataSource>();        // SQLite
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

app.UseCors(AngularDev);   // apply the CORS policy (must come before MapControllers)
app.MapControllers();   // hooks the controller routes into the request pipeline
app.Run();

