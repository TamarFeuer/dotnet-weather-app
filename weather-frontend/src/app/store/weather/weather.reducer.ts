import { createReducer, on } from '@ngrx/store';
import { initialWeatherState } from './weather.state';
import { monthSelected, weatherLoaded, weatherFailed } from './weather.actions';

// The reducer is a PURE function: (current state, action) => new state. It never
// changes the old state - it returns a NEW object (we spread ...state to copy
// the old fields, then override the ones that change). createReducer wires each
// action to how it updates the state.
export const weatherReducer = createReducer(
	initialWeatherState,

	// User picked a month: remember it, turn loading on, clear any old error.
	on(monthSelected, (state, { month }) => ({
		...state,
		selectedMonth: month,
		loading: true,
		error: null,
	})),

	// Data arrived: store it and turn loading off.
	on(weatherLoaded, (state, { info }) => ({
		...state,
		info,
		loading: false,
	})),

	// Request failed: clear the data, turn loading off, keep the error message.
	on(weatherFailed, (state, { error }) => ({
		...state,
		info: null,
		loading: false,
		error,
	})),
);
