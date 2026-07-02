import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError, of } from 'rxjs';
import { ForecastService } from '../../forecast.service';
import {
	citiesRequested,
	citiesLoaded,
	citySelected,
	forecastLoaded,
	forecastFailed,
} from './forecast.actions';

// TWO effects this time, one per request - the same pattern as the weather
// effect, just twice. Each listens for one action, does the HTTP call, and
// dispatches a result action.
@Injectable()
export class ForecastEffects {
	private actions$ = inject(Actions);
	private forecast = inject(ForecastService);

	// When citiesRequested is dispatched, fetch the dropdown options.
	loadCities$ = createEffect(() =>
		this.actions$.pipe(
			ofType(citiesRequested),
			switchMap(() =>
				this.forecast.getCities().pipe(
					map((cities) => citiesLoaded({ cities })),
					catchError((err) =>
						of(forecastFailed({ error: err.message ?? 'Could not load the cities' }))
					)
				)
			)
		)
	);

	// When citySelected is dispatched, fetch that city's forecast.
	loadForecast$ = createEffect(() =>
		this.actions$.pipe(
			ofType(citySelected),
			switchMap(({ city }) =>
				this.forecast.getForecast(city).pipe(
					map((days) => forecastLoaded({ days })),
					catchError((err) =>
						of(forecastFailed({ error: err.message ?? 'Could not load the forecast' }))
					)
				)
			)
		)
	);
}
