import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// The shape of one forecast day the API returns (matches the backend's
// ForecastDay model). `date` is a string here because JSON has no date type -
// ASP.NET serializes the C# DateOnly as an ISO string like "2026-07-03".
export interface ForecastDay {
	date: string;
	minTemp: number;
	maxTemp: number;
	condition: string;
}

// A second service, the frontend twin of the backend's ForecastService. Its
// only job is to call the two new backend endpoints. Kept separate from
// WeatherService (the month feature) so each service has one job - the same
// one-class-one-job split we made on the backend.
@Injectable({ providedIn: 'root' })
export class ForecastService {
	private readonly baseUrl = 'http://localhost:5151/api/weather';

	constructor(private http: HttpClient) {}

	// GET /api/weather/cities -> the list of city names for the dropdown.
	getCities() {
		return this.http.get<string[]>(`${this.baseUrl}/cities`);
	}

	// GET /api/weather/forecast?city=<city> -> that city's 5-day forecast.
	getForecast(city: string) {
		return this.http.get<ForecastDay[]>(`${this.baseUrl}/forecast`, {
			params: { city },
		});
	}
}
