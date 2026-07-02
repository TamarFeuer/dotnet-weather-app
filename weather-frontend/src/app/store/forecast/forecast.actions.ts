import { createAction, props } from '@ngrx/store';
import { ForecastDay } from '../../forecast.service';

// The forecast feature has TWO things to load - the city list and a city's
// forecast - so it has two request/loaded pairs. Same past-tense "something
// happened" naming as the weather slice.

// The app wants the dropdown options (dispatched when the city picker loads).
export const citiesRequested = createAction('[Forecast] Cities Requested');

// The API returned the city list (dispatched by the effect, on success).
export const citiesLoaded = createAction(
	'[Forecast] Cities Loaded',
	props<{ cities: string[] }>()
);

// The user picked a city (dispatched by CityPicker).
export const citySelected = createAction(
	'[Forecast] City Selected',
	props<{ city: string }>()
);

// The API returned that city's forecast (dispatched by the effect, on success).
export const forecastLoaded = createAction(
	'[Forecast] Forecast Loaded',
	props<{ days: ForecastDay[] }>()
);

// A request failed (dispatched by either effect, on error).
export const forecastFailed = createAction(
	'[Forecast] Forecast Failed',
	props<{ error: string }>()
);
