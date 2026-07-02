import { ForecastDay } from '../../forecast.service';

// The "forecast" slice - a SECOND section of the store, sitting alongside the
// existing "weather" slice. Same idea, different data: instead of one month's
// info, it holds the city dropdown options, the picked city, and its 5 days.
export interface ForecastState {
	cities: string[];             // the dropdown options (loaded from the API)
	selectedCity: string | null;  // which city the user picked
	days: ForecastDay[];          // the loaded 5-day forecast (empty until loaded)
	loading: boolean;             // is a forecast request in flight?
	error: string | null;         // an error message, if a request failed
}

// The starting state, before anything has happened.
export const initialForecastState: ForecastState = {
	cities: [],
	selectedCity: null,
	days: [],
	loading: false,
	error: null,
};
