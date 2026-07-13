import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideStore, provideState } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { typicalReducer } from './store/typical/typical.reducer';
import { TypicalEffects } from './store/typical/typical.effects';
import { forecastReducer } from './store/forecast/forecast.reducer';
import { ForecastEffects } from './store/forecast/forecast.effects';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideHttpClient(),
		// NgRx: set up the store and the effects runner.
		provideStore(),
		provideEffects(TypicalEffects, ForecastEffects),
		// Register the two slices: "typical" (this month) and "forecast" (cities).
		provideState('typical', typicalReducer),
		provideState('forecast', forecastReducer),
		// Redux DevTools: lets the browser extension show the store + actions.
		provideStoreDevtools({ maxAge: 25, connectInZone: true, name: 'WeatherApp' }),
	]
};
