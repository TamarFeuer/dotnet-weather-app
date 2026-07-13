import { createReducer, on } from '@ngrx/store';
import { initialTypicalState } from './typical.state';
import { monthSelected, typicalLoaded, typicalFailed } from './typical.actions';

// The reducer is a PURE function: (current state, action) => new state. It never
// changes the old state - it returns a NEW object (we spread ...state to copy
// the old fields, then override the ones that change).
export const typicalReducer = createReducer(
	initialTypicalState,

	// A month is showing: remember it, turn loading on, clear any old error.
	on(monthSelected, (state, { month }) => ({
		...state,
		selectedMonth: month,
		loading: true,
		error: null,
	})),

	// Data arrived: store it and turn loading off.
	on(typicalLoaded, (state, { info }) => ({
		...state,
		info,
		loading: false,
	})),

	// Request failed: clear the data, turn loading off, keep the error message.
	on(typicalFailed, (state, { error }) => ({
		...state,
		info: null,
		loading: false,
		error,
	})),
);
