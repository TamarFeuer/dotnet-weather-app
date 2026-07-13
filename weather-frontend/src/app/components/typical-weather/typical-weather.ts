import { Component, inject, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { monthSelected } from '../../store/typical/typical.actions';
import { TemperatureDisplay } from '../temperature-display/temperature-display';

@Component({
  selector: 'app-typical-weather',
  // It reuses TemperatureDisplay to show the result - that component already
  // reads the month's info from the store, so nothing there changes.
  imports: [TemperatureDisplay],
  templateUrl: './typical-weather.html',
})
export class TypicalWeather implements OnInit {
  private store = inject(Store);

  // The current month's name, e.g. "July". Computed once when the component
  // is created.
  readonly month = new Date().toLocaleString('en-GB', { month: 'long' });

  // On load, dispatch monthSelected for the CURRENT month - no dropdown, no
  // user click. This keeps the whole month feature genuinely in use: the same
  // action -> effect -> SQLite/EF Core -> reducer -> selector lifecycle runs,
  // just triggered automatically instead of by MonthPicker.
  ngOnInit() {
    this.store.dispatch(monthSelected({ month: this.month }));
  }
}
