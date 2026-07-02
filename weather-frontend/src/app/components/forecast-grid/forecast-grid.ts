import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { DayCard } from '../day-card/day-card';
import {
  selectDays,
  selectForecastLoading,
  selectForecastError,
  selectSelectedCity,
} from '../../store/forecast/forecast.selectors';

@Component({
  selector: 'app-forecast-grid',
  // ForecastGrid RENDERS DayCards, so it imports DayCard. This is component
  // composition: a container component reading from the store, and a
  // presentational child that just shows what it's handed.
  imports: [DayCard],
  templateUrl: './forecast-grid.html',
  styleUrl: './forecast-grid.css',
})
export class ForecastGrid {
  private store = inject(Store);

  // Everything this component shows comes straight from the forecast slice.
  readonly days = this.store.selectSignal(selectDays);
  readonly loading = this.store.selectSignal(selectForecastLoading);
  readonly error = this.store.selectSignal(selectForecastError);
  readonly city = this.store.selectSignal(selectSelectedCity);
}
