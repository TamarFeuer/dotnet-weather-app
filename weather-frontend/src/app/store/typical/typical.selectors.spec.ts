import { selectTypicalInfo } from './typical.selectors';
import { TypicalState } from './typical.state';
import { TypicalInfo } from '../../typical.service';

// A selector is a pure function too. .projector() runs only the last step of it,
// handing your slice straight in, so no store is needed to test one.
describe('typical selectors', () => {
  const info: TypicalInfo = {
    minTemp: 14,
    maxTemp: 23,
    average: 18,
    description: 'Warm',
  };

  const state: TypicalState = {
    selectedMonth: 'July',
    info,
    loading: false,
    error: null,
  };

  it('selectTypicalInfo picks the info out of the slice', () => {
    expect(selectTypicalInfo.projector(state)).toEqual(info);
  });

  it('selectTypicalInfo is null before anything has loaded', () => {
    expect(selectTypicalInfo.projector({ ...state, info: null })).toBeNull();
  });
});
