using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WeatherAPI.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Temperatures",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Month = table.Column<string>(type: "text", nullable: false),
					MinTemp = table.Column<int>(type: "integer", nullable: false),
					MaxTemp = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Temperatures", x => x.Id);
				});

			migrationBuilder.InsertData(
				table: "Temperatures",
				columns: new[] { "Id", "MaxTemp", "MinTemp", "Month" },
				values: new object[,]
				{
					{ 1, 6, 1, "January" },
					{ 2, 7, 1, "February" },
					{ 3, 10, 3, "March" },
					{ 4, 14, 5, "April" },
					{ 5, 18, 9, "May" },
					{ 6, 21, 12, "June" },
					{ 7, 23, 14, "July" },
					{ 8, 23, 14, "August" },
					{ 9, 19, 11, "September" },
					{ 10, 15, 8, "October" },
					{ 11, 10, 4, "November" },
					{ 12, 7, 2, "December" }
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Temperatures");
		}
	}
}
