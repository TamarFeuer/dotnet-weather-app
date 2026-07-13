import { TestBed } from '@angular/core/testing';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { ForecastGrid } from './forecast-grid';
import {
  selectCities,
  selectSelectedCity,
  selectDays,
  selectForecastLoading,
  selectForecastError,
} from '../../store/forecast/forecast.selectors';
import { ForecastDay } from '../../forecast.service';

// ForecastGrid is a container: it reads four values from the store and decides
// what to show. With a MockStore we can put the store into any state we like and
// check what appears - including states that are hard to reach by hand, like a
// failed request.
describe('ForecastGrid', () => {
  let store: MockStore;

  const days: ForecastDay[] = [
    { date: '2026-07-03', minTemp: 13, maxTemp: 21, condition: 'Cloudy' },
    { date: '2026-07-04', minTemp: 14, maxTemp: 22, condition: 'Rain' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ForecastGrid],
      providers: [
        provideMockStore({
          selectors: [
            { selector: selectCities, value: [] },
            { selector: selectSelectedCity, value: null },
            { selector: selectDays, value: [] },
            { selector: selectForecastLoading, value: false },
            { selector: selectForecastError, value: null },
          ],
        }),
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);
  });

  function render(): HTMLElement {
    const fixture = TestBed.createComponent(ForecastGrid);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows nothing before a city has been picked', () => {
    const el = render();

    // No loading, no error, no days: the grid stays quiet rather than showing an
    // empty box.
    expect(el.textContent?.trim()).toBe('');
    expect(el.querySelectorAll('app-day-card').length).toBe(0);
  });

  it('shows a loading message while the forecast is on its way', () => {
    store.overrideSelector(selectForecastLoading, true);

    const el = render();

    expect(el.textContent).toContain('Loading the forecast');
  });

  it('shows the error message when the request failed', () => {
    store.overrideSelector(selectForecastError, 'Could not load the forecast');

    const el = render();

    expect(el.querySelector('.error')?.textContent).toContain('Could not load the forecast');
  });

  it('renders one day card per day, under a heading naming the city', () => {
    store.overrideSelector(selectDays, days);
    store.overrideSelector(selectSelectedCity, 'Amsterdam');

    const el = render();

    expect(el.querySelector('h2')?.textContent).toContain('Amsterdam');
    expect(el.querySelectorAll('app-day-card').length).toBe(2);
  });

  it('prefers the error over the days it still holds', () => {
    // Loading takes priority over error, and error over days. This pins that
    // order down, so a future change to the template cannot quietly show a stale
    // forecast next to an error message.
    store.overrideSelector(selectDays, days);
    store.overrideSelector(selectForecastError, 'boom');

    const el = render();

    expect(el.querySelector('.error')).not.toBeNull();
    expect(el.querySelectorAll('app-day-card').length).toBe(0);
  });
});
