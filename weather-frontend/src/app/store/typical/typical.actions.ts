import { createAction, props } from '@ngrx/store';
import { TypicalInfo } from '../../typical.service';

// An ACTION is a plain object describing something that happened. createAction
// takes a unique type string (convention: '[Source] Event') and props()
// declares the payload it carries.

// A month was chosen (dispatched by TypicalWeather for the current month).
export const monthSelected = createAction(
	'[Typical] Month Selected',
	props<{ month: string }>()
);

// The API returned the typical weather (dispatched by the effect, on success).
export const typicalLoaded = createAction(
	'[Typical] Loaded',
	props<{ info: TypicalInfo }>()
);

// The API call failed (dispatched by the effect, on error).
export const typicalFailed = createAction(
	'[Typical] Failed',
	props<{ error: string }>()
);
