import { createAction, props } from '@ngrx/store';
import { WeatherInfo } from '../weather.service';

// An ACTION is a plain object describing something that happened. createAction
// takes a unique type string (convention: '[Source] Event') and props()
// declares the payload it carries. Dispatching monthSelected({ month: 'July' })
// produces the object { type: '[Weather] Month Selected', month: 'July' }.

// The user picked a month (will be dispatched by MonthPicker).
export const monthSelected = createAction(
	'[Weather] Month Selected',
	props<{ month: string }>()
);

// The API returned the weather (will be dispatched by the effect, on success).
export const weatherLoaded = createAction(
	'[Weather] Weather Loaded',
	props<{ info: WeatherInfo }>()
);

// The API call failed (will be dispatched by the effect, on error).
export const weatherFailed = createAction(
	'[Weather] Weather Failed',
	props<{ error: string }>()
);
