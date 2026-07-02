// ============================================================================
// REPOSITORY - live API driver
// ============================================================================
// OpenMeteoForecastSource implements IForecastSource by calling the real
// Open-Meteo API (free, no key). It is the only class that knows Open-Meteo
// exists: it builds the request URL, reads their JSON shape, and ADAPTS it to
// our own ForecastDay model. Swap it for a seeded driver and nothing else in
// the app changes - same pattern as the Sql/Json month drivers.
//
// ASYNC, finally explained: a network call takes time (tens of milliseconds to
// seconds). A synchronous method would BLOCK the thread that whole time. An
// async method instead RETURNS a Task ("the result will arrive later") and
// frees the thread; `await` says "pause here until the result arrives, without
// blocking". It's the backend twin of the frontend's Observable + subscribe.
using System.Text.Json.Serialization;
using WeatherAPI.Models;

namespace WeatherAPI.Repository
{
	public class OpenMeteoForecastSource : IForecastSource
	{
		// The backend's HttpClient - the C# twin of Angular's HttpClient: an
		// object for making HTTP requests to another server. Injected via the
		// constructor (registered with AddHttpClient in Program.cs).
		private readonly HttpClient _http;

		public OpenMeteoForecastSource(HttpClient http)
		{
			_http = http;
		}

		public async Task<IReadOnlyList<ForecastDay>> GetForecastAsync(City city)
		{
			// Ask Open-Meteo for 5 days of min/max temperature + weather code.
			string url =
				$"https://api.open-meteo.com/v1/forecast" +
				$"?latitude={city.Latitude}&longitude={city.Longitude}" +
				$"&daily=temperature_2m_min,temperature_2m_max,weather_code" +
				$"&timezone=auto&forecast_days=5";

			// GET the URL and deserialize the JSON body into our DTOs, one call.
			// `await` = the request flies off; this method resumes when the
			// response has arrived.
			OpenMeteoResponse? response = await _http.GetFromJsonAsync<OpenMeteoResponse>(url);

			DailyBlock? daily = response?.Daily;
			if (daily is null || daily.Time.Count == 0)
				return [];

			// ADAPT: Open-Meteo returns parallel arrays (dates[], mins[], maxs[],
			// codes[]); we zip them into one ForecastDay per index.
			List<ForecastDay> days = [];
			for (int i = 0; i < daily.Time.Count; i++)
			{
				days.Add(new ForecastDay
				{
					Date = DateOnly.Parse(daily.Time[i]),
					MinTemp = (int)Math.Round(daily.MinTemps[i]),
					MaxTemp = (int)Math.Round(daily.MaxTemps[i]),
					Condition = DescribeWmoCode(daily.WeatherCodes[i]),
				});
			}
			return days;
		}

		// Open-Meteo reports the condition as a WMO code (an international
		// numeric weather standard). Translating code -> text is part of
		// adapting THEIR format to OURS, so it belongs here in the driver.
		private static string DescribeWmoCode(int code) => code switch
		{
			0           => "Clear",
			1 or 2      => "Partly cloudy",
			3           => "Cloudy",
			45 or 48    => "Fog",
			>= 51 and <= 57 => "Drizzle",
			>= 61 and <= 67 => "Rain",
			>= 71 and <= 77 => "Snow",
			>= 80 and <= 82 => "Showers",
			85 or 86    => "Snow showers",
			>= 95       => "Thunderstorm",
			_           => "Unknown",
		};

		// DTOs matching Open-Meteo's JSON shape. [JsonPropertyName] maps their
		// snake_case field names onto our C# property names. Private nested
		// classes: nothing outside this driver ever sees Open-Meteo's shape.
		private sealed class OpenMeteoResponse
		{
			[JsonPropertyName("daily")]
			public DailyBlock? Daily { get; set; }
		}

		private sealed class DailyBlock
		{
			[JsonPropertyName("time")]
			public List<string> Time { get; set; } = [];

			[JsonPropertyName("temperature_2m_min")]
			public List<double> MinTemps { get; set; } = [];

			[JsonPropertyName("temperature_2m_max")]
			public List<double> MaxTemps { get; set; } = [];

			[JsonPropertyName("weather_code")]
			public List<int> WeatherCodes { get; set; } = [];
		}
	}
}
