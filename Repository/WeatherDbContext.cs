// ============================================================================
// REPOSITORY - EF Core database context
// ============================================================================
// WeatherDbContext is EF Core's representation of the database.
//
// - DbSet<Temperature> Temperatures  =  the "Temperatures" TABLE.
// - Each Temperature object           =  one ROW.
// - The Id / Season / MinTemp / MaxTemp properties = the COLUMNS.
//
// OnModelCreating seeds the four seasons, so the database is never empty on
// first run. EF Core turns all of this into real SQL for us.
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class WeatherDbContext : DbContext
	{
		// The options (which database, which file) are injected from Program.cs
		// — the DbContext itself doesn't hardcode where the data lives.
		public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
			: base(options)
		{
		}

		// This property IS the table. Querying it generates "SELECT ... FROM Temperatures".
		public DbSet<Temperature> Temperatures => Set<Temperature>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Seed data: the same four ranges that used to live in seasons.json,
			// now rows in the table. HasData needs explicit primary-key values.
			modelBuilder.Entity<Temperature>().HasData(
				new Temperature { Id = 1, Season = "Summer", MinTemp = 17, MaxTemp = 25 },
				new Temperature { Id = 2, Season = "Autumn", MinTemp = 8,  MaxTemp = 14 },
				new Temperature { Id = 3, Season = "Spring", MinTemp = 8,  MaxTemp = 15 },
				new Temperature { Id = 4, Season = "Winter", MinTemp = 2,  MaxTemp = 8  }
			);
		}
	}
}
