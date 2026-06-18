import { WeatherInfo } from '../weather.service';

// The store's state is one big object split into named sections ("slices").
// This interface describes our "weather" slice - the weather state the app
// shares across components.
export interface WeatherState {
	selectedMonth: string | null; // which month the user picked
	info: WeatherInfo | null;     // the loaded weather (null until loaded)
	loading: boolean;             // is a request in flight?
	error: string | null;         // an error message, if the request failed
}

// The starting state, before anything has happened.
export const initialWeatherState: WeatherState = {
	selectedMonth: null,
	info: null,
	loading: false,
	error: null,
};
