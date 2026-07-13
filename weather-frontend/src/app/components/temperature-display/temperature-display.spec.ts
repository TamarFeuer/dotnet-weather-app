import { TestBed } from '@angular/core/testing';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { TemperatureDisplay } from './temperature-display';
import { selectTypicalInfo } from '../../store/typical/typical.selectors';
import { TypicalInfo } from '../../typical.service';

// TemperatureDisplay reads one value from the store and renders it behind an
// @if. That @if has two branches, and a test that only ever sees "no data" would
// never run the interesting one - which is exactly what the coverage report
// caught: the template sat at 33%.
describe('TemperatureDisplay', () => {
  let store: MockStore;

  const info: TypicalInfo = {
    minTemp: 14,
    maxTemp: 23,
    average: 18,
    description: 'Warm',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TemperatureDisplay],
      providers: [
        provideMockStore({
          selectors: [{ selector: selectTypicalInfo, value: null }],
        }),
      ],
    }).compileComponents();

    store = TestBed.inject(MockStore);
  });

  function render(): HTMLElement {
    const fixture = TestBed.createComponent(TemperatureDisplay);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows nothing while there is no data yet', () => {
    const el = render();

    expect(el.textContent?.trim()).toBe('');
  });

  it('shows the range, the average and the description once loaded', () => {
    store.overrideSelector(selectTypicalInfo, info);

    const el = render();

    expect(el.querySelector('.result')?.textContent).toContain('14');
    expect(el.querySelector('.result')?.textContent).toContain('23');
    expect(el.textContent).toContain('18');
    expect(el.textContent).toContain('Warm');
  });
});
