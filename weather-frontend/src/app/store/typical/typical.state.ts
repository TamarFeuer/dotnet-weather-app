import { TypicalInfo } from '../../typical.service';

// The store's state is one big object split into named sections ("slices").
// This interface describes our "typical" slice - this month's typical weather,
// shared across components.
export interface TypicalState {
	selectedMonth: string | null; // which month is showing
	info: TypicalInfo | null;     // the loaded typical weather (null until loaded)
	loading: boolean;             // is a request in flight?
	error: string | null;         // an error message, if the request failed
}

// The starting state, before anything has happened.
export const initialTypicalState: TypicalState = {
	selectedMonth: null,
	info: null,
	loading: false,
	error: null,
};
