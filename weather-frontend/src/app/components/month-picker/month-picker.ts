import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { monthSelected } from '../../store/weather/weather.actions';

@Component({
  selector: 'app-month-picker',
  templateUrl: './month-picker.html',
})
export class MonthPicker {
  // The 12 months shown in the dropdown (data this component owns).
  protected readonly months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
  ];

  // Inject the store so this component can dispatch actions to it directly.
  private store = inject(Store);

  // When the dropdown changes, dispatch the monthSelected action to the store
  // (instead of emitting an output up to a parent).
  onMonthChange(month: string) {
    this.store.dispatch(monthSelected({ month }));
  }
}
