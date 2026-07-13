import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Action } from '@ngrx/store';
import { Observable, of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { ForecastEffects } from './forecast.effects';
import { ForecastService, ForecastDay } from '../../forecast.service';
import {
  citiesRequested,
  citiesLoaded,
  citySelected,
  forecastLoaded,
  forecastFailed,
} from './forecast.actions';

describe('ForecastEffects', () => {
  let effects: ForecastEffects;
  let actions$: Observable<Action>;

  const cities = ['Amsterdam', 'Rotterdam'];
  const days: ForecastDay[] = [
    { date: '2026-07-03', minTemp: 13, maxTemp: 21, condition: 'Cloudy' },
  ];

  const forecastService = {
    getCities: vi.fn(),
    getForecast: vi.fn(),
  };

  beforeEach(() => {
    vi.resetAllMocks();

    TestBed.configureTestingModule({
      providers: [
        ForecastEffects,
        provideMockActions(() => actions$),
        { provide: ForecastService, useValue: forecastService },
      ],
    });

    effects = TestBed.inject(ForecastEffects);
  });

  describe('loadCities$', () => {
    it('fetches the cities and emits citiesLoaded', async () => {
      forecastService.getCities.mockReturnValue(of(cities));
      actions$ = of(citiesRequested());

      const result = await firstValueFrom(effects.loadCities$);

      expect(forecastService.getCities).toHaveBeenCalled();
      expect(result).toEqual(citiesLoaded({ cities }));
    });

    it('emits forecastFailed when the city list cannot be fetched', async () => {
      forecastService.getCities.mockReturnValue(throwError(() => new Error('Offline')));
      actions$ = of(citiesRequested());

      const result = await firstValueFrom(effects.loadCities$);

      expect(result).toEqual(forecastFailed({ error: 'Offline' }));
    });
  });

  describe('loadForecast$', () => {
    it('fetches the forecast for the chosen city and emits forecastLoaded', async () => {
      forecastService.getForecast.mockReturnValue(of(days));
      actions$ = of(citySelected({ city: 'Amsterdam' }));

      const result = await firstValueFrom(effects.loadForecast$);

      expect(forecastService.getForecast).toHaveBeenCalledWith('Amsterdam');
      expect(result).toEqual(forecastLoaded({ days }));
    });

    it('emits forecastFailed when the forecast cannot be fetched', async () => {
      forecastService.getForecast.mockReturnValue(throwError(() => new Error('Offline')));
      actions$ = of(citySelected({ city: 'Amsterdam' }));

      const result = await firstValueFrom(effects.loadForecast$);

      expect(result).toEqual(forecastFailed({ error: 'Offline' }));
    });

    it('does not react to citiesRequested', async () => {
      // Each effect listens for exactly one action. Mixing them up is a classic
      // NgRx bug, so this pins the wiring down.
      forecastService.getForecast.mockReturnValue(of(days));
      actions$ = of(citiesRequested());

      let emitted = false;
      effects.loadForecast$.subscribe(() => (emitted = true));

      expect(emitted).toBe(false);
      expect(forecastService.getForecast).not.toHaveBeenCalled();
    });
  });
});
