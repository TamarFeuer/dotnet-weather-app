import {
  selectCities,
  selectSelectedCity,
  selectDays,
  selectForecastLoading,
  selectForecastError,
} from './forecast.selectors';
import { ForecastState } from './forecast.state';
import { ForecastDay } from '../../forecast.service';

describe('forecast selectors', () => {
  const days: ForecastDay[] = [
    { date: '2026-07-03', minTemp: 13, maxTemp: 21, condition: 'Cloudy' },
  ];

  const state: ForecastState = {
    cities: ['Amsterdam', 'Rotterdam'],
    selectedCity: 'Amsterdam',
    days,
    loading: false,
    error: null,
  };

  it('selectCities returns the dropdown options', () => {
    expect(selectCities.projector(state)).toEqual(['Amsterdam', 'Rotterdam']);
  });

  it('selectSelectedCity returns the chosen city', () => {
    expect(selectSelectedCity.projector(state)).toBe('Amsterdam');
  });

  it('selectDays returns the forecast days', () => {
    expect(selectDays.projector(state)).toEqual(days);
  });

  it('selectForecastLoading reports a request in flight', () => {
    expect(selectForecastLoading.projector({ ...state, loading: true })).toBe(true);
  });

  it('selectForecastError returns the error message', () => {
    expect(selectForecastError.projector({ ...state, error: 'boom' })).toBe('boom');
  });
});
