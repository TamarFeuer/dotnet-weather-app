import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TypicalState } from './typical.state';

// Grab the whole "typical" slice from the store. The string 'typical' must match
// the key we registered with provideState('typical', typicalReducer).
const selectTypical = createFeatureSelector<TypicalState>('typical');

// Derived selector: reads one field out of the slice. createSelector memoizes
// (caches) the result - if the slice hasn't changed, it returns the cached
// value instead of recomputing. TemperatureDisplay reads this from the store.
export const selectTypicalInfo = createSelector(selectTypical, (state) => state.info);
