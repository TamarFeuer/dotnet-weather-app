import { Component, signal } from '@angular/core';
import { WeatherService, WeatherInfo } from './weather.service';
import { MonthPicker } from './month-picker';

@Component({
  selector: 'app-root',
  imports: [MonthPicker],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('weather-frontend');

  // A signal = a reactive value. It holds the chosen month's info (null until a
  // month is picked). When we .set() it, the template re-renders by itself -
  // that's what gives the live update with no F5.
  protected readonly info = signal<WeatherInfo | null>(null);

  // Inject the WeatherService (dependency injection, same as the backend).
  constructor(private weather: WeatherService) {}

  // Runs when the MonthPicker emits a month. Calls the API; when the response
  // arrives, .set() stores it in the signal, which refreshes the screen.
  onMonthSelected(month: string) {
    this.weather.getWeather(month).subscribe(response => {
      this.info.set(response);
    });
  }
}
