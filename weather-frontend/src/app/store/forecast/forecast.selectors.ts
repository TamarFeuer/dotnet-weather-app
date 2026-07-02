import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ForecastState } from './forecast.state';

// Grab the whole "forecast" slice. The string 'forecast' must match the key we
// register with provideState('forecast', forecastReducer) in app.config.ts.
const selectForecast = createFeatureSelector<ForecastState>('forecast');

// One derived selector per field the components need. Same memoized pattern as
// the weather selectors.
export const selectCities = createSelector(selectForecast, (state) => state.cities);
export const selectSelectedCity = createSelector(selectForecast, (state) => state.selectedCity);
export const selectDays = createSelector(selectForecast, (state) => state.days);
export const selectForecastLoading = createSelector(selectForecast, (state) => state.loading);
export const selectForecastError = createSelector(selectForecast, (state) => state.error);
