import { TestBed } from '@angular/core/testing';
import { provideMockStore } from '@ngrx/store/testing';
import { App } from './app';

// App is only a shell, but the components it hosts (TypicalWeather, CityPicker,
// ForecastGrid) all inject the NgRx Store. Without a Store provider Angular
// throws NG0201, so the test has to supply one.
//
// provideMockStore gives a fake store: dispatch() does nothing and the selectors
// simply read the state below. That keeps this test fast, offline and
// deterministic - no effects run, so nothing reaches the backend or Open-Meteo.
const initialState = {
  typical: {
    selectedMonth: null,
    info: null,
    loading: false,
    error: null,
  },
  forecast: {
    cities: [],
    selectedCity: null,
    days: [],
    loading: false,
    error: null,
  },
};

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideMockStore({ initialState })],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the page title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Weather in the Netherlands'
    );
  });
});
