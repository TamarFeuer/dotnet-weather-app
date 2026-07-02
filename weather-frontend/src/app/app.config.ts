import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideStore, provideState } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { weatherReducer } from './store/weather/weather.reducer';
import { WeatherEffects } from './store/weather/weather.effects';
import { forecastReducer } from './store/forecast/forecast.reducer';
import { ForecastEffects } from './store/forecast/forecast.effects';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideHttpClient(),
		// NgRx: set up the store and the effects runner.
		provideStore(),
		provideEffects(WeatherEffects, ForecastEffects),
		// Register the two slices: "weather" (months) and "forecast" (cities).
		provideState('weather', weatherReducer),
		provideState('forecast', forecastReducer),
		// Redux DevTools: lets the browser extension show the store + actions.
		provideStoreDevtools({ maxAge: 25, connectInZone: true, name: 'WeatherApp' }),
	]
};
