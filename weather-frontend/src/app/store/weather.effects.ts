import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError, of } from 'rxjs';
import { WeatherService } from '../weather.service';
import { monthSelected, weatherLoaded, weatherFailed } from './weather.actions';

// An EFFECT listens for actions and does the side-effect work (here: the HTTP
// call), then dispatches a NEW action with the result. This keeps the impure
// work out of the reducer.
@Injectable()
export class WeatherEffects {
	// Actions is a stream of EVERY action dispatched to the store.
	private actions$ = inject(Actions);
	// Our existing service that calls the backend API.
	private weather = inject(WeatherService);

	// When monthSelected is dispatched, call the API and dispatch weatherLoaded
	// with the result (or weatherFailed if it errors).
	loadWeather$ = createEffect(() =>
		this.actions$.pipe(
			ofType(monthSelected),                          // only react to monthSelected
			switchMap(({ month }) =>                        // for each one, call the service
				this.weather.getWeather(month).pipe(
					map((info) => weatherLoaded({ info })),     // success -> weatherLoaded action
					// catchError sits on the INNER observable (per request), so a failure
					// ends only this request - the effect keeps listening for future months.
					// It must return an Observable, so of(...) wraps the failure action.
					catchError((err) =>
						of(weatherFailed({ error: err.message ?? 'Could not load the weather' }))
					)
				)
			)
		)
	);
}
