import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError, of } from 'rxjs';
import { TypicalService } from '../../typical.service';
import { monthSelected, typicalLoaded, typicalFailed } from './typical.actions';

// An EFFECT listens for actions and does the side-effect work (here: the HTTP
// call), then dispatches a NEW action with the result. This keeps the impure
// work out of the reducer.
@Injectable()
export class TypicalEffects {
	// Actions is a stream of EVERY action dispatched to the store.
	private actions$ = inject(Actions);
	// Our service that calls the backend's typical-weather endpoint.
	private typical = inject(TypicalService);

	// When monthSelected is dispatched, call the API and dispatch typicalLoaded
	// with the result (or typicalFailed if it errors).
	loadTypical$ = createEffect(() =>
		this.actions$.pipe(
			ofType(monthSelected),                          // only react to monthSelected
			switchMap(({ month }) =>                        // for each one, call the service
				this.typical.getTypical(month).pipe(
					map((info) => typicalLoaded({ info })),     // success -> typicalLoaded action
					// catchError sits on the INNER observable (per request), so a failure
					// ends only this request - the effect keeps listening for future months.
					// It must return an Observable, so of(...) wraps the failure action.
					catchError((err) =>
						of(typicalFailed({ error: err.message ?? 'Could not load the typical weather' }))
					)
				)
			)
		)
	);
}
