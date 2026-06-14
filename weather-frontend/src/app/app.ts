import { Component, signal } from '@angular/core';
import { WeatherService } from './weather.service';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('weather-frontend');

  // The component CLASS holds the data; the template (app.html) reads from here
  // to draw the screen. These are the 12 months the dropdown lists - the
  // template loops over this array to create one <option> per month.
  protected readonly months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
  ];

  // A signal = a reactive value. It holds the latest temperature (null until a
  // month is picked). When we .set() it, the template re-renders by itself -
  // that's what gives the live update with no F5.
  protected readonly temperature = signal<number | null>(null);

  // Inject the WeatherService (dependency injection, same as the backend).
  constructor(private weather: WeatherService) {}

  // Runs when the dropdown changes. Calls the API; when the response arrives,
  // .set() stores it in the signal, which refreshes the screen.
  onMonthSelected(month: string) {
    this.weather.getTemperature(month).subscribe(response => {
      this.temperature.set(response.temperature);
    });
  }
}
