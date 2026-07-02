import { createReducer, on } from '@ngrx/store';
import { initialForecastState } from './forecast.state';
import {
	citiesLoaded,
	citySelected,
	forecastLoaded,
	forecastFailed,
} from './forecast.actions';

// The forecast reducer: the same PURE (state, action) => new state shape as the
// weather reducer, wiring each action to how it updates the forecast slice.
export const forecastReducer = createReducer(
	initialForecastState,

	// The city list arrived: store it (no loading flag - this fills the dropdown
	// quietly in the background).
	on(citiesLoaded, (state, { cities }) => ({
		...state,
		cities,
	})),

	// User picked a city: remember it, turn loading on, clear any old error.
	on(citySelected, (state, { city }) => ({
		...state,
		selectedCity: city,
		loading: true,
		error: null,
	})),

	// The forecast arrived: store the days and turn loading off.
	on(forecastLoaded, (state, { days }) => ({
		...state,
		days,
		loading: false,
	})),

	// A request failed: clear the days, turn loading off, keep the error message.
	on(forecastFailed, (state, { error }) => ({
		...state,
		days: [],
		loading: false,
		error,
	})),
);
