import { TestBed } from '@angular/core/testing';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { vi } from 'vitest';
import { CityPicker } from './city-picker';
import { selectCities } from '../../store/forecast/forecast.selectors';
import { citiesRequested, citySelected } from '../../store/forecast/forecast.actions';

// CityPicker talks to the store in two directions: it DISPATCHES actions (on
// load and on change) and it READS the city list back. A MockStore lets us do
// both: we hand it a fixed city list, and we spy on dispatch to see what the
// component sent. Nothing is really dispatched, so no effect runs and no HTTP
// call is made.
describe('CityPicker', () => {
  let store: MockStore;
  let dispatch: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityPicker],
      providers: [
        provideMockStore({
          selectors: [{ selector: selectCities, value: ['Amsterdam', 'Rotterdam'] }],
        }),
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);
    dispatch = vi.spyOn(store, 'dispatch');
  });

  function render(): HTMLElement {
    const fixture = TestBed.createComponent(CityPicker);
    fixture.detectChanges(); // this is what runs ngOnInit
    return fixture.nativeElement as HTMLElement;
  }

  it('asks for the city list as soon as it appears', () => {
    render();

    // No click needed: ngOnInit dispatches this by itself, which is what kicks
    // off the effect that fetches the cities.
    expect(dispatch).toHaveBeenCalledWith(citiesRequested());
  });

  it('fills the dropdown with the cities from the store', () => {
    const el = render();

    const options = el.querySelectorAll('option');

    // One placeholder ("Choose a city") plus one option per city.
    expect(options.length).toBe(3);
    expect(options[1].textContent).toContain('Amsterdam');
    expect(options[2].textContent).toContain('Rotterdam');
  });

  it('dispatches the chosen city when the selection changes', () => {
    const el = render();
    dispatch.mockClear(); // forget the citiesRequested from ngOnInit

    const select = el.querySelector('select')!;
    select.value = 'Rotterdam';
    select.dispatchEvent(new Event('change'));

    expect(dispatch).toHaveBeenCalledWith(citySelected({ city: 'Rotterdam' }));
  });
});
