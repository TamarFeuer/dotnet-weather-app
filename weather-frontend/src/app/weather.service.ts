import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// A service = a reusable class. This one's only job is to call the backend API.
//
// @Injectable({ providedIn: 'root' }) marks the class as a service Angular can
// hand out, and 'root' means one shared instance for the whole app (a
// singleton). This is Angular's dependency injection - the same idea as your
// C# AddScoped registrations.
@Injectable({ providedIn: 'root' })
export class WeatherService {
	private readonly apiUrl = 'http://localhost:5151/api/weather/temperature';

	// The other side of dependency injection: this service RECEIVES HttpClient
	// through its constructor (Angular supplies it automatically) - exactly like
	// constructor injection in your C# backend. We switched HttpClient on earlier
	// with provideHttpClient().
	constructor(private http: HttpClient) {}

	// Calls GET /api/weather/temperature?month=<month>.
	// Returns an Observable: the response arrives later, and whoever cares
	// "subscribes" to be notified when it does (the observer pattern).
	getTemperature(month: string) {
		return this.http.get<{ temperature: number }>(this.apiUrl, {
			params: { month },
		});
	}
}
