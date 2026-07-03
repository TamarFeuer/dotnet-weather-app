import { Component } from '@angular/core';
import { TypicalWeather } from './components/typical-weather/typical-weather';
import { CityPicker } from './components/city-picker/city-picker';
import { ForecastGrid } from './components/forecast-grid/forecast-grid';

@Component({
  selector: 'app-root',
  imports: [TypicalWeather, CityPicker, ForecastGrid],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  // App is just a shell: it hosts the components, and each one talks to the
  // store on its own. No state, no service call, no input/output wiring here.
}
