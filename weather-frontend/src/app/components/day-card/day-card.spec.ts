import { TestBed } from '@angular/core/testing';
import { DayCard } from './day-card';
import { ForecastDay } from '../../forecast.service';

// DayCard is presentational: it holds no store logic at all. One day flows in
// through its input and it renders it. That makes it the simplest component in
// the app to test - no store, no mock, no NgRx.
describe('DayCard', () => {
  const day: ForecastDay = {
    date: '2026-07-03',
    minTemp: 13,
    maxTemp: 21,
    condition: 'Cloudy',
  };

  async function render(input: ForecastDay): Promise<HTMLElement> {
    await TestBed.configureTestingModule({ imports: [DayCard] }).compileComponents();

    const fixture = TestBed.createComponent(DayCard);
    fixture.componentRef.setInput('day', input);
    fixture.detectChanges();

    return fixture.nativeElement as HTMLElement;
  }

  it('renders the temperatures and the condition', async () => {
    const el = await render(day);

    expect(el.textContent).toContain('13');
    expect(el.textContent).toContain('21');
    expect(el.textContent).toContain('Cloudy');
  });

  it('turns the ISO date into a readable label', async () => {
    const el = await render(day);

    // "2026-07-03" should become something like "Fri 3 Jul", not the raw string.
    const label = el.querySelector('.day')?.textContent ?? '';

    expect(label).toContain('Jul');
    expect(label).not.toContain('2026-07-03');
  });
});
