import { Component } from '@angular/core';
import { MonthPicker } from './components/month-picker/month-picker';
import { TemperatureDisplay } from './components/temperature-display/temperature-display';
import { CityPicker } from './components/city-picker/city-picker';
import { ForecastGrid } from './components/forecast-grid/forecast-grid';

@Component({
  selector: 'app-root',
  imports: [MonthPicker, TemperatureDisplay, CityPicker, ForecastGrid],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  // App is now just a shell: it hosts the two components, and each one talks to
  // the store on its own. No state, no service call, no input/output wiring here.
}
