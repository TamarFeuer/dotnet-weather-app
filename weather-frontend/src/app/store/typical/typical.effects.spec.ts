import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Action } from '@ngrx/store';
import { Observable, of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';
import { TypicalEffects } from './typical.effects';
import { TypicalService, TypicalInfo } from '../../typical.service';
import { monthSelected, typicalLoaded, typicalFailed } from './typical.actions';

// An effect is a machine that turns one action into another, with a side effect
// in between. To test it we control both ends:
//   - provideMockActions lets us push any action into the effect's input stream,
//   - a stubbed TypicalService lets us decide what the "HTTP call" returns.
// So no store, no network, and we can test the failure path just as easily as
// the happy path - which is normally the hard one to reach by hand.
describe('TypicalEffects', () => {
  let effects: TypicalEffects;
  let actions$: Observable<Action>;

  const info: TypicalInfo = {
    minTemp: 14,
    maxTemp: 23,
    average: 18,
    description: 'Warm',
  };

  const typicalService = { getTypical: vi.fn() };

  beforeEach(() => {
    vi.resetAllMocks();

    TestBed.configureTestingModule({
      providers: [
        TypicalEffects,
        provideMockActions(() => actions$),
        { provide: TypicalService, useValue: typicalService },
      ],
    });

    effects = TestBed.inject(TypicalEffects);
  });

  it('calls the service and emits typicalLoaded on success', async () => {
    typicalService.getTypical.mockReturnValue(of(info));
    actions$ = of(monthSelected({ month: 'July' }));

    const result = await firstValueFrom(effects.loadTypical$);

    expect(typicalService.getTypical).toHaveBeenCalledWith('July');
    expect(result).toEqual(typicalLoaded({ info }));
  });

  it('emits typicalFailed when the request errors', async () => {
    typicalService.getTypical.mockReturnValue(throwError(() => new Error('Network down')));
    actions$ = of(monthSelected({ month: 'July' }));

    const result = await firstValueFrom(effects.loadTypical$);

    // catchError turns the error into an action instead of letting it kill the
    // effect. That is what keeps the app listening for the next month.
    expect(result).toEqual(typicalFailed({ error: 'Network down' }));
  });

  it('ignores actions it does not listen for', async () => {
    typicalService.getTypical.mockReturnValue(of(info));
    actions$ = of(typicalLoaded({ info })); // not monthSelected

    let emitted = false;
    effects.loadTypical$.subscribe(() => (emitted = true));

    expect(emitted).toBe(false);
    expect(typicalService.getTypical).not.toHaveBeenCalled();
  });
});
