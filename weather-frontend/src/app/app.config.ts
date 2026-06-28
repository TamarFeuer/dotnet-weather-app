import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideStore, provideState } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { weatherReducer } from './store/weather.reducer';
import { WeatherEffects } from './store/weather.effects';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideHttpClient(),
		// NgRx: set up the store and the effects runner.
		provideStore(),
		provideEffects(WeatherEffects),
		// Register the "weather" slice, managed by weatherReducer.
		provideState('weather', weatherReducer),
		// Redux DevTools: lets the browser extension show the store + actions.
		provideStoreDevtools({ maxAge: 25, connectInZone: true, name: 'WeatherApp' }),
	]
};
