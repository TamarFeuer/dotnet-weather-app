import { forecastReducer } from './forecast.reducer';
import { initialForecastState, ForecastState } from './forecast.state';
import {
  citiesRequested,
  citiesLoaded,
  citySelected,
  forecastLoaded,
  forecastFailed,
} from './forecast.actions';
import { ForecastDay } from '../../forecast.service';

describe('forecastReducer', () => {
  const days: ForecastDay[] = [
    { date: '2026-07-03', minTemp: 13, maxTemp: 21, condition: 'Cloudy' },
    { date: '2026-07-04', minTemp: 14, maxTemp: 22, condition: 'Rain' },
  ];

  it('returns the initial state for an action it does not handle', () => {
    const state = forecastReducer(undefined, { type: 'some other action' } as never);

    expect(state).toEqual(initialForecastState);
  });

  describe('citiesRequested', () => {
    it('changes nothing at all', () => {
      // This looks like a pointless test, but it pins down a real and easily
      // misunderstood design decision: the reducer has NO handler for this
      // action. citiesRequested exists only to wake the effect that fetches the
      // city list. Every action reaches the reducer, but a reducer only reacts
      // to the ones it has an on() for.
      const state = forecastReducer(initialForecastState, citiesRequested());

      expect(state).toEqual(initialForecastState);
    });
  });

  describe('citiesLoaded', () => {
    it('stores the city list', () => {
      const state = forecastReducer(
        initialForecastState,
        citiesLoaded({ cities: ['Amsterdam', 'Rotterdam'] })
      );

      expect(state.cities).toEqual(['Amsterdam', 'Rotterdam']);
      // The city list fills the dropdown quietly in the background, so it must
      // not touch the loading flag that belongs to the forecast request.
      expect(state.loading).toBe(false);
    });
  });

  describe('citySelected', () => {
    it('remembers the city, turns loading on and clears any old error', () => {
      const failed: ForecastState = { ...initialForecastState, error: 'Could not load' };

      const state = forecastReducer(failed, citySelected({ city: 'Amsterdam' }));

      expect(state.selectedCity).toBe('Amsterdam');
      expect(state.loading).toBe(true);
      expect(state.error).toBeNull();
    });
  });

  describe('forecastLoaded', () => {
    it('stores the days and turns loading off', () => {
      const loading: ForecastState = { ...initialForecastState, loading: true };

      const state = forecastReducer(loading, forecastLoaded({ days }));

      expect(state.days).toEqual(days);
      expect(state.loading).toBe(false);
    });
  });

  describe('forecastFailed', () => {
    it('drops the days, turns loading off and keeps the message', () => {
      const loaded: ForecastState = { ...initialForecastState, days, loading: true };

      const state = forecastReducer(loaded, forecastFailed({ error: 'Could not load' }));

      expect(state.days).toEqual([]);
      expect(state.loading).toBe(false);
      expect(state.error).toBe('Could not load');
    });

    it('leaves the city list alone', () => {
      // A failed forecast must not empty the dropdown: the user still needs to
      // be able to pick another city and try again.
      const loaded: ForecastState = {
        ...initialForecastState,
        cities: ['Amsterdam', 'Rotterdam'],
        days,
      };

      const state = forecastReducer(loaded, forecastFailed({ error: 'boom' }));

      expect(state.cities).toEqual(['Amsterdam', 'Rotterdam']);
    });
  });
});
