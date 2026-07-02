import { Component, inject, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { citiesRequested, citySelected } from '../../store/forecast/forecast.actions';
import { selectCities } from '../../store/forecast/forecast.selectors';

@Component({
  selector: 'app-city-picker',
  templateUrl: './city-picker.html',
})
export class CityPicker implements OnInit {
  private store = inject(Store);

  // Unlike MonthPicker (whose 12 months were hardcoded here), the city list
  // lives in the store - it's loaded from the backend. We read it as a signal
  // so the dropdown fills in reactively once the list arrives.
  readonly cities = this.store.selectSignal(selectCities);

  // OnInit runs once, right after the component is created. We dispatch
  // citiesRequested here to kick off the fetch: the effect hears it, calls
  // GET /api/weather/cities, and the reducer writes the result into the store -
  // which flows straight back into cities() above.
  ngOnInit() {
    this.store.dispatch(citiesRequested());
  }

  // When the dropdown changes, dispatch citySelected - the effect then loads
  // that city's forecast. Same shape as MonthPicker's onMonthChange.
  onCityChange(city: string) {
    this.store.dispatch(citySelected({ city }));
  }
}
