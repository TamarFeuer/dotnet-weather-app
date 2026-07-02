import { createFeatureSelector, createSelector } from '@ngrx/store';
import { WeatherState } from './weather.state';

// Grab the whole "weather" slice from the store. The string 'weather' must match
// the key we registered with provideState('weather', weatherReducer).
const selectWeather = createFeatureSelector<WeatherState>('weather');

// Derived selectors: each reads one field out of the slice. createSelector
// memoizes (caches) the result - if the slice hasn't changed, it returns the
// cached value instead of recomputing. Components read these from the store.
export const selectInfo = createSelector(selectWeather, (state) => state.info);
export const selectLoading = createSelector(selectWeather, (state) => state.loading);
export const selectError = createSelector(selectWeather, (state) => state.error);
export const selectSelectedMonth = createSelector(selectWeather, (state) => state.selectedMonth);
