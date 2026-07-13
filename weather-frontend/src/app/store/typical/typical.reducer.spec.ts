import { typicalReducer } from './typical.reducer';
import { initialTypicalState, TypicalState } from './typical.state';
import { monthSelected, typicalLoaded, typicalFailed } from './typical.actions';
import { TypicalInfo } from '../../typical.service';

// A reducer is a PURE function: (state, action) => new state. That makes it the
// easiest thing in the whole app to test. No Angular, no TestBed, no store, no
// mock, no HTTP. Just call it and look at what comes back.
describe('typicalReducer', () => {
  const info: TypicalInfo = {
    minTemp: 14,
    maxTemp: 23,
    average: 18,
    description: 'Warm',
  };

  it('returns the initial state for an action it does not handle', () => {
    const state = typicalReducer(undefined, { type: 'some other action' } as never);

    expect(state).toEqual(initialTypicalState);
  });

  describe('monthSelected', () => {
    it('remembers the month and turns loading on', () => {
      const state = typicalReducer(initialTypicalState, monthSelected({ month: 'July' }));

      expect(state.selectedMonth).toBe('July');
      expect(state.loading).toBe(true);
    });

    it('clears an error left over from a previous attempt', () => {
      const failed: TypicalState = { ...initialTypicalState, error: 'Could not load' };

      const state = typicalReducer(failed, monthSelected({ month: 'July' }));

      expect(state.error).toBeNull();
    });

    it('does not mutate the state it was handed', () => {
      // The purity check. NgRx relies on the old state object coming back
      // untouched: that is what lets it compare states cheaply and what makes
      // the DevTools time-travel slider work.
      const before = { ...initialTypicalState };

      typicalReducer(initialTypicalState, monthSelected({ month: 'July' }));

      expect(initialTypicalState).toEqual(before);
    });
  });

  describe('typicalLoaded', () => {
    it('stores the info and turns loading off', () => {
      const loading: TypicalState = { ...initialTypicalState, loading: true };

      const state = typicalReducer(loading, typicalLoaded({ info }));

      expect(state.info).toEqual(info);
      expect(state.loading).toBe(false);
    });
  });

  describe('typicalFailed', () => {
    it('drops the info, turns loading off and keeps the message', () => {
      const loaded: TypicalState = { ...initialTypicalState, info, loading: true };

      const state = typicalReducer(loaded, typicalFailed({ error: 'Could not load' }));

      // The old info is cleared on purpose: showing stale data next to an error
      // message would be worse than showing nothing.
      expect(state.info).toBeNull();
      expect(state.loading).toBe(false);
      expect(state.error).toBe('Could not load');
    });
  });
});
