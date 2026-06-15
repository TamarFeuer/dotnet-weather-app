import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// The shape of the data the API returns for a month (matches the backend's
// WeatherInfo record). Exported so the component can use the same type.
export interface WeatherInfo {
	minTemp: number;
	maxTemp: number;
	average: number;
	description: string;
}

// A service = a reusable class. This one's only job is to call the backend API.
//
// @Injectable({ providedIn: 'root' }) marks the class as a service Angular can
// hand out, and 'root' means one shared instance for the whole app (a
// singleton). This is Angular's dependency injection - the same idea as 
// C# AddScoped registrations.
@Injectable({ providedIn: 'root' })
export class WeatherService {
	private readonly apiUrl = 'http://localhost:5151/api/weather/temperature';

	// The other side of dependency injection: this service RECEIVES HttpClient
	// through its constructor (Angular supplies it automatically) - exactly like
	// constructor injection in the C# backend. We switched HttpClient on earlier
	// with provideHttpClient().
	constructor(private http: HttpClient) {}

	// Calls GET /api/weather/temperature?month=<month>.
	// Returns an Observable of the month's WeatherInfo (range + average +
	// description). The response arrives later; whoever cares "subscribes" to it.
	getWeather(month: string) {
		return this.http.get<WeatherInfo>(this.apiUrl, {
			params: { month },
		});
	}
}
